using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MasterFiles.Testing
{
	public class FROrgImpAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDeltaG1SubProcedureList()
		{
			var org = Factory.New<OrgHeader>();
			var frOrgImpAddInfo = FROrgImpAddInfo.Get(org);
			AssertEquals("C, D", frOrgImpAddInfo.Lookups.DeltaG1SubProcedureList.CodesAsString);
		}

		public void TestDeltaG1SubProcedureIsCached()
		{
			var org = Factory.New<OrgHeader>();
			var frOrgImpAddInfo = FROrgImpAddInfo.Get(org);
			var deltaG1SubProcedureList1 = frOrgImpAddInfo.Lookups.DeltaG1SubProcedureList;
			var deltaG1SubProcedureList2 = frOrgImpAddInfo.Lookups.DeltaG1SubProcedureList;
			Assert("The list should be cached.", ReferenceEquals(deltaG1SubProcedureList1, deltaG1SubProcedureList2));
		}

		public void TestVATProcedureList()
		{
			var org = Factory.New<OrgHeader>();
			var frOrgImpAddInfo = FROrgImpAddInfo.Get(org);
			AssertEquals("2, L, S", frOrgImpAddInfo.Lookups.VATProcedureList.CodesAsString);
		}

		public void TestVATProcedureListIsCached()
		{
			var org = Factory.New<OrgHeader>();
			var frOrgImpAddInfo = FROrgImpAddInfo.Get(org);
			var vatProcedureList1 = frOrgImpAddInfo.Lookups.VATProcedureList;
			var vatProcedureList2 = frOrgImpAddInfo.Lookups.VATProcedureList;
			Assert("The list should be cached.", ReferenceEquals(vatProcedureList1, vatProcedureList2));
		}
	}
}
