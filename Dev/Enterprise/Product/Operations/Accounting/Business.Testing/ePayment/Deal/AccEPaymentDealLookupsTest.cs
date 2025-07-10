using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.Testing
{
	sealed class AccEPaymentDealLookupsTest : BusinessObjectLookupsTestCase
	{
		AccEPaymentDealLookups Lookups => lookups ?? (lookups = new AccEPaymentDealLookups(Factory.New<AccEPaymentDeal>()));
		AccEPaymentDealLookups lookups;

		public void TestStatusCodeList()
		{
			AssertEquals(11, Lookups.StatusCodeList.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { "QUE", "PEN", "RDY", "REQ", "ACP", "INP", "PAI", "SMF", "CAN", "DEC", "FAL" }, Lookups.StatusCodeList.GetAllCodes());
		}

		public void TestProviderCodeList()
		{
			AssertEquals(1, Lookups.ProviderCodeList.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { "OFX" }, Lookups.ProviderCodeList.GetAllCodes());
		}
	}
}
