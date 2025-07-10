using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MasterFiles.Testing
{
	public class RegionOrgImpAddInfoLookupsTest : EU.Business.Testing.EUOrgImpAddInfoLookupsAbstractTest
	{
		public void TestDefermentMethodList()
		{
			var org = Factory.New<OrgHeader>();
			var addInfo = RegionOrgImpAddInfo.Get(org, Core.Constants.CountryCodes.France);
			var list = addInfo.Lookups.DefermentMethodList;
			CombineAssertions(() =>
			{
				AssertEquals("Values", "A, M, R", list.CodesAsString);
				AssertSame("Cached", list, addInfo.Lookups.DefermentMethodList);
			});
		}
	}
}
