using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.Testing
{
	class RegionOrgImpAddInfoLookupsTest : EU.Business.Testing.EUOrgImpAddInfoLookupsAbstractTest
	{
		public void TestMethodOfPaymentLookupValues()
		{
			var org = Factory.New<OrgHeader>();
			var addInfo = RegionOrgImpAddInfo.Get(org, Core.Constants.CountryCodes.Ireland);
			var list = addInfo.Lookups.DefermentMethodList;
			AssertEquals("Values", "A, E, J, M", list.CodesAsString);

			CombineAssertions(() =>
			{
				AssertEquals("Description A", "Payment in cash", list.GetDescriptionFromCode("A"));
				AssertEquals("Description E", "Deferred or postponed payment", list.GetDescriptionFromCode("E"));
				AssertEquals("Description J", "Payment through post office administration (postal consignments) or other public sector or government department", list.GetDescriptionFromCode("J"));
				AssertEquals("Description M", "Securities", list.GetDescriptionFromCode("M"));
				AssertSame("Cached", list, addInfo.Lookups.DefermentMethodList);
			});
		}
	}
}
