using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.Testing
{
	public class AccEPaymentQuoteLookupsTest : BusinessObjectLookupsTestCase
	{
		protected AccEPaymentQuoteLookups Lookups => lookups ?? (lookups = new AccEPaymentQuoteLookups(Factory.New<AccEPaymentQuote>()));
		AccEPaymentQuoteLookups lookups;

		public void TestStatusCodeList()
		{
			AssertEquals(8, Lookups.StatusCodeList.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { "QUE", "REQ", "RQF", "ERR", "RCV", "ACP", "DCD", "EXP" }, Lookups.StatusCodeList.GetAllCodes());
		}

		public void TestProviderCodeList()
		{
			AssertEquals(1, Lookups.ProviderCodeList.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { "OFX" }, Lookups.ProviderCodeList.GetAllCodes());
		}
	}
}
