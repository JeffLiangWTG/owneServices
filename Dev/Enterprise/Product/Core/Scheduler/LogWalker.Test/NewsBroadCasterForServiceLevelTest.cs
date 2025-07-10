using System;
using CargoWise.EntityFramework;

namespace Enterprise.LogWalker.Testing
{
	sealed class NewsBroadCasterForServiceLevelTest : NewsBroadcaster
	{
		public Func<BusinessObjectFactory> GetFactoryMethod { set { getFactoryMethod = value; } }
		Func<BusinessObjectFactory> getFactoryMethod;

		protected override NewsTransmitter GetNewNewsTransmitter(LogSubscriber subscriber, LogSubscriber[] subscribers, SubscriberParameters subscriberParameters)
		{
			return new NewsTransmitterForServiceLevelTest(subscriber, subscribers, subscriberParameters) { GetFactoryMethod = getFactoryMethod };
		}
	}
}
