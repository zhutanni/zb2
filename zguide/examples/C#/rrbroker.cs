﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using ZeroMQ;

namespace ZeroMQ.Test
{
	static partial class Program
	{
		public static void RRBroker(IDictionary<string, string> dict, string[] args)
		{
			//
			// Simple request-reply broker
			//
			// Author: metadings
			//

			// Prepare our context and sockets
			using (var context = new ZContext())
			using (var frontend = new ZSocket(context, ZSocketType.ROUTER))
			using (var backend = new ZSocket(context, ZSocketType.DEALER))
			{
				frontend.Bind("tcp://*:5559");
				backend.Bind("tcp://*:5560");

				// Initialize poll set
				var poll = ZPollItem.CreateReceiver();

				// Switch messages between sockets
				ZError error;
				ZMessage message;
				while (true)
				{
					if (frontend.PollIn(poll, out message, out error, TimeSpan.FromMilliseconds(64)))
					{
						using (message)
						{
							// Process all parts of the message
							Console_WriteZMessage(2, message, "frontend");
							backend.Send(message);
						}
					}
					else
					{
						if (error == ZError.ETERM)
							return;	// Interrupted
						if (error != ZError.EAGAIN)
							throw new ZException(error);
					}

					if (backend.PollIn(poll, out message, out error, TimeSpan.FromMilliseconds(64)))
					{
						// Process all parts of the message
						Console_WriteZMessage(2, message, " backend");
						frontend.Send(message);
					}
					else
					{
						if (error == ZError.ETERM)
							return;	// Interrupted
						if (error != ZError.EAGAIN)
							throw new ZException(error);
					}
				}
			}
		}
	}
}