using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	class CusSupplyChainActorReferenceLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList()
		{
			CombineAssertions(() =>
			{
				var codeList = lookups.CodeList;
				AssertEquals("CodesAsString", "CS, FW, MF, WH", codeList.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<SupplyChainActorRoleList>(), codeList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var cusSupplyChainActorReference = Factory.New<CusSupplyChainActorReference>();
			lookups = new CusSupplyChainActorReferenceLookups(cusSupplyChainActorReference);
		}
		CusSupplyChainActorReferenceLookups lookups;
	}
}
