using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusRelatedEventsProviderTest : TestCase
	{
		public void TestAvailableApplicationCodes()
		{
			var provider = new CusRelatedEventsProvider();
			AssertEquals("CMR", string.Join(",", provider.AvailableApplicationCodes));
		}
	}
}
