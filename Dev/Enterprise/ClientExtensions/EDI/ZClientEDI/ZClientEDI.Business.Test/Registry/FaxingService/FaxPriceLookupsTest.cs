using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	internal sealed class FaxPriceLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCurrencies()
		{
			FaxPrice bizo = new FaxPrice();
			AssertNotNull(bizo.Lookups.Currencies);
		}
	}
}