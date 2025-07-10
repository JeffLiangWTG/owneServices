using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.Test
{
	public class GBOrgImpAddInfoLookups : BusinessObjectLookupsTestCase
	{
		public void TestDefermentMethodList()
		{
			var org = Factory.New<OrgHeader>();
			var addInfo = org.CountryData.RegionSpecificImpAddInfo as EUOrgImpAddInfo;
			var list = addInfo.Lookups.DefermentMethodList;
			AssertEquals("A, B, C, D", list.CodesAsString);
			AssertSame(list, addInfo.Lookups.DefermentMethodList);
		}
	}
}
