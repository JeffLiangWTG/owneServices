using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Enterprise.Integration;

namespace Enterprise.LogWalker.Test
{
	sealed class NewsBroadcasterWithMockCallForTesting : NewsBroadcaster
	{
		public bool ThrowErrorOnSecondProcess { get; set; }

		public void ProcessLogsAndReturnIsComplete_ForMockSubscriber(ILogger logger)
		{
			ProcessLogs(new SubscriberParameters() { Logger = logger }, new[] { new MockEventSubscriber() }, CancellationToken.None);
		}

		protected override LogSubscriberProcessResult CallQueuedLogsProcessor(LogSubscriber subscriber, LogSubscriber[] subscribers, SubscriberParameters subscriberParameters)
		{
			subscriberParameters.Logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "[{0}] subscriber started processing logs.", subscriber.FriendlyName));

			CalledSubscribers.Add(subscriber.Name);

			var result = base.CallQueuedLogsProcessor(subscriber, subscribers, subscriberParameters);
			if (ThrowErrorOnSecondProcess && CalledSubscribers.Count >= 2)
			{
				throw new InvalidOperationException("Throwing error on 2nd call");
			}
			return result;
		}

		public List<string> CalledSubscribers = new List<string>();
	}
}
