using System;
using Enterprise.Integration;

namespace Enterprise.LogWalker.Testing
{
	[Serializable]
	class NewsTransmitterForTesting : NewsTransmitter
	{
		public NewsTransmitterForTesting(MockSubscriber subscriber, LogSubscriber[] subscribers = null, ILogger logger = null)
			: base(subscriber, subscribers ?? new[] { subscriber }, new SubscriberParameters() { Logger = logger ?? subscriber.TestLogger })
		{
		}

		public void ProcessEntireQueue()
		{
			ProcessLogQueueBatch(0);
		}
	}
}
