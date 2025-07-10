using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Testing;

class RegionOrgImpAddInfoLookupsTest : EU.Business.Testing.EUOrgImpAddInfoLookupsAbstractTest
{
	public void TestMethodOfPaymentLookupValues()
	{
		var org = Factory.New<OrgHeader>();
		var addInfo = RegionOrgImpAddInfo.Get(org, Core.Constants.CountryCodes.Netherlands);
		var list = addInfo.Lookups.DefermentMethodList;
		CombineAssertions(() =>
		{
			AssertEquals("Values", "B, A", list.CodesAsString);
			AssertSame("Cached", list, addInfo.Lookups.DefermentMethodList);
		});
	}
}
