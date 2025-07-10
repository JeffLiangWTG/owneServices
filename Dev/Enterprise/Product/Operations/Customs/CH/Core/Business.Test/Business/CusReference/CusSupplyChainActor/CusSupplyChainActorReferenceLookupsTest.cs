using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business.Testing;

class CusSupplyChainActorReferenceLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCodeList()
	{
		CombineAssertions(() =>
		{
			var cusSupplyChainActorReference = Factory.New<CusSupplyChainActorReference>();
			var lookups = new CusSupplyChainActorReferenceLookups(cusSupplyChainActorReference);
			var codeList = lookups.CodeList;
			AssertEquals("CodesAsString", "CS, FW, MF, WH", codeList.CodesAsString);
			AssertSame("Cached", Factory.GetCachedValue<SupplyChainActorRoleList>(), codeList);
		});
	}
}
