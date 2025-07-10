using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	class StatusRequestLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestModuleList()
		{
			CombineAssertions(() =>
			{
				AssertSame("Cached", Factory.GetCachedValue<ExportStatusRequestModuleCodeList>(), lookups.ModuleList);
				AssertEquals("Values", "AES, NCTS", lookups.ModuleList.CodesAsString);
			});
		}

		public void TestRoleList_NCTS()
		{
			CombineAssertions(() =>
			{
				statusRequest.Module = ExportStatusRequestModuleCodeList.Codes.NCTS;
				var roleList = lookups.RoleList;
				AssertSame("Cached", Factory.GetCachedValue<ExportStatusRequestNCTSRoleList>(), roleList);
				AssertEquals("Values", "1, 2, 3, 4, 5", roleList.CodesAsString);
			});
		}

		public void TestRoleList_AES()
		{
			CombineAssertions(() =>
			{
				statusRequest.Module = ExportStatusRequestModuleCodeList.Codes.AES;
				var roleList = lookups.RoleList;
				AssertSame("Cached", Factory.GetCachedValue<ExportStatusRequestAESRoleList>(), roleList);
				AssertEquals("Values", "1, 2, 3, 4", roleList.CodesAsString);
			});
		}

		public void TestIdentifications()
		{
			AssertType<OrgHeaderCollection>(lookups.Identifications);
		}

		protected override void SetUp()
		{
			base.SetUp();
			statusRequest = Factory.New<StatusRequest>();
			lookups = new StatusRequestLookups(statusRequest);
		}
		StatusRequest statusRequest;
		StatusRequestLookups lookups;
	}
}
