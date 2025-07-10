using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	public class DEOrgImpAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestVATClaimBackList()
		{
			var org = Factory.New<OrgHeader>();
			var addInfo = org.CountryData.ImpAddInfo as DEOrgImpAddInfo;
			var list = addInfo.Lookups.VATClaimBackList;
			AssertEquals("N, Y", list.CodesAsString);
			AssertSame(list, addInfo.Lookups.VATClaimBackList);
		}
	}
}
