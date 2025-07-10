using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class EUOrgImpAddInfoLookupsBaseOnlyTest : EUOrgImpAddInfoLookupsAbstractTest
	{
		public void TestDefermentMethodList()
		{
			var org = Factory.New<OrgHeader>();
			var addInfo = org.CountryData.RegionSpecificImpAddInfo as EUOrgImpAddInfo;
			var list = addInfo.Lookups.DefermentMethodList;
			CombineAssertions(() =>
			{
				AssertEquals("Values", "A, B, C, D", list.CodesAsString);
				AssertSame("Cached", list, addInfo.Lookups.DefermentMethodList);
			});
		}
	}
}
