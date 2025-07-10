using System;
using CargoWise.EntityFramework;

namespace Enterprise.LogWalker.Testing
{
	[Serializable]
	sealed class NewsTransmitterForServiceLevelTest : NewsTransmitter
	{
		public NewsTransmitterForServiceLevelTest(LogSubscriber subscriber, LogSubscriber[] subscribers, SubscriberParameters subscriberParameters) : base(subscriber, subscribers, subscriberParameters) { }

		public Func<BusinessObjectFactory> GetFactoryMethod { set { getFactoryMethod = value; } }
		Func<BusinessObjectFactory> getFactoryMethod;

		protected override BusinessObjectFactory GetNewBusinessObjectFactory()
		{
			if (getFactoryMethod != null)
			{
				return getFactoryMethod();
			}

			return base.GetNewBusinessObjectFactory();
		}
	}
}
