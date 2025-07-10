using NUnit.Framework;

namespace Enterprise.PAVE.MENT.Business.Test
{
	class MENTStandAloneFactoryProviderTest : TestCase
	{
		public void TestShouldNotCacheSecondaryProvider()
		{
			var factoryProvider = new MENTStandAloneFactoryProvider();
			var secondaryServerConnectionProvider = factoryProvider.GetSecondaryServerConnectionProvider();
			AssertNotEquals(secondaryServerConnectionProvider, factoryProvider.GetSecondaryServerConnectionProvider());
		}
	}
}
