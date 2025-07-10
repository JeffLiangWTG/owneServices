using System;
using System.Diagnostics;
using System.Threading;

namespace Enterprise.LogWalker.Testing
{
	[Serializable]
	sealed class NewsTransmitterWithSingleLoop : NewsTransmitter
	{
		public NewsTransmitterWithSingleLoop(MockSubscriber subscriber) : this(subscriber, new[] { subscriber }, new SubscriberParameters() { Logger = subscriber.TestLogger }) { }
		public NewsTransmitterWithSingleLoop(LogSubscriber subscriber, LogSubscriber[] subscribers, SubscriberParameters subscriberParameters) : base(subscriber, subscribers, subscriberParameters) { }
		internal override bool ContinueTransmitting(int batchSize, BatchResult foundThisLoop, Stopwatch sw, CancellationToken token) => false;
	}
}
