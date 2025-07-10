using System;
using CargoWise.EntityFramework;

namespace Enterprise.LogWalker.Testing
{
	sealed class OperationsManagerForServiceLevelTest : OperationsManagerForTesting
	{
		public OperationsManagerForServiceLevelTest(LogSubscriber[] subscribers) : base(subscribers) { }

		public Func<BusinessObjectFactory> GetFactoryMethod { set { getFactoryMethod = value; } }
		Func<BusinessObjectFactory> getFactoryMethod;

		protected override NewsBroadcaster GetNewBroadcaster()
		{
			return new NewsBroadCasterForServiceLevelTest()
			{
				GetFactoryMethod = getFactoryMethod
			};
		}
	}
}
