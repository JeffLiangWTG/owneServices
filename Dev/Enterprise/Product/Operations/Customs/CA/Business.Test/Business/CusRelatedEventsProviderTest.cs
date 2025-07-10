using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusRelatedEventsProviderTest : TestCase
	{
		public void TestAvailableApplicationCodes()
		{
			var provider = new CusRelatedEventsProvider();
			AssertEquals("CAS,CAA,CAH,CAR", string.Join(",", provider.AvailableApplicationCodes));
		}
	}
}
