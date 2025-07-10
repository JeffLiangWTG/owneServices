using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IdentityApplicationPermission.Business.Testing
{
	[TestedType(typeof(EdiIdentityApplicationPermission))]
	public class EdiIdentityApplicationPermissionTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParentApplication()
		{
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			var permission = application.Permissions.AddNew();
			Factory.Save();

			AssertEquals(application, permission.Application);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			var permission = application.Permissions.AddNew();
			return permission;
		}

		public void TestPermission()
		{
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			var permission = application.Permissions.AddNew();
			permission.IAP_Scope = "*";
			AssertEquals("*", permission.Permission);

			permission.IAP_Scope = "f30a1049-9790-4653-bbac-79c7a040278c";
			AssertEquals(CustomerApplicationPermissionsList.Codes.CargoVisibilityAPI, permission.Permission);

			var cwApplication = Factory.NewWithValidTestData<EdiIdentityApplication>();
			cwApplication.IDA_ClientID = "testAppId";
			application.IDA_IDA_ParentApplication = cwApplication.PK;
			permission.IAP_Scope = "testAppId/Audit";
			AssertEquals(CustomerApplicationPermissionsList.Codes.AuditAPI, permission.Permission);
		}

		public void TestScope()
		{
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			var permission = application.Permissions.AddNew();
			permission.Permission = "*";
			AssertEquals("*", permission.IAP_Scope);

			permission.Permission = CustomerApplicationPermissionsList.Codes.CargoVisibilityAPI;
			AssertEquals("f30a1049-9790-4653-bbac-79c7a040278c", permission.IAP_Scope);

			var cwApplication = Factory.NewWithValidTestData<EdiIdentityApplication>();
			cwApplication.IDA_ClientID = "testAppId";
			application.IDA_IDA_ParentApplication = cwApplication.PK;
			permission.Permission = CustomerApplicationPermissionsList.Codes.AuditAPI;
			AssertEquals("testAppId/Audit", permission.IAP_Scope);
		}
	}
}
