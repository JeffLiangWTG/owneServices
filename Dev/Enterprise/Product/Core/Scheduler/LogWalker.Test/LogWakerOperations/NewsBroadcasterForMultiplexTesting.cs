using Enterprise.LogWalker.Testing;

namespace Enterprise.LogWalker.Test
{
	sealed class NewsBroadcasterForMultiplexTesting : NewsBroadcaster
	{
		protected override NewsTransmitter GetNewNewsTransmitter(LogSubscriber subscriber, LogSubscriber[] subscribers, SubscriberParameters subscriberParameters)
		{
			return new NewsTransmitterWithSingleLoop(subscriber, subscribers, subscriberParameters);
		}
	}
}
