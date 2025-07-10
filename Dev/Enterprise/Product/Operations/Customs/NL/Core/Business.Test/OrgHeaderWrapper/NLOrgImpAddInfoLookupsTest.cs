using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Testing;

public class NLOrgImpAddInfoLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestVATDefermentList()
	{
		var org = Factory.New<OrgHeader>();
		var addInfo = org.CountryData.ImpAddInfo as NLOrgImpAddInfo;
		var list = addInfo.Lookups.VATDefermentList;
		AssertEquals("N, Y", list.CodesAsString);
		AssertSame(list, addInfo.Lookups.VATDefermentList);
	}
}
