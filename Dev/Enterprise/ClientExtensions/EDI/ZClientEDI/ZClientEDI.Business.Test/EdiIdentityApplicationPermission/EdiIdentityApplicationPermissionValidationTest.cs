using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.IdentityApplication.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IdentityApplicationPermission.Business.Testing
{
	[TestedType(typeof(EdiIdentityApplicationPermissionValidation))]
	public class EdiIdentityApplicationPermissionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestUniqueCheck()
		{
			var application = Factory.New<EdiIdentityApplication>();
			var permission1 = application.Permissions.AddNew();
			permission1.IAP_Scope = CustomerApplicationPermissionsList.Codes.AuditAPI;

			var permission2 = application.Permissions.AddNew();
			permission2.IAP_Scope = CustomerApplicationPermissionsList.Codes.CargoVisibilityAPI;
			permission2.Validation.ValidateAll();
			Assert(!permission2.IAP_ScopeInfo.HasErrors());

			var permission3 = application.Permissions.AddNew();
			permission3.IAP_Scope = CustomerApplicationPermissionsList.Codes.AuditAPI;
			permission3.Validation.ValidateAll();
			Assert(permission3.IAP_ScopeInfo.HasErrors());
		}

		public void TestScopeCheckForNonCW1Application()
		{
			var cw1Application = Factory.New<EdiIdentityApplication>();
			cw1Application.IDA_ClientID = "078F69D4-E5B2-413D-9DA6-000B0F5F291D";

			var application = Factory.New<EdiIdentityApplication>();
			application.IsCustomerApplication = true;
			application.IDA_IDA_ParentApplication = cw1Application.PK;

			var permission = application.Permissions.AddNew();
			permission.Validation.ValidateAll();
			Assert(permission.IAP_ScopeInfo.HasError("Please enter a Permission."));

			permission.Permission = CustomerApplicationPermissionsList.Codes.AuditAPI;
			permission.Validation.ValidateAll();
			Assert(!permission.IAP_ScopeInfo.HasErrors());

			application.IDA_IDA_ParentApplication = ZGuid.Empty;
			permission.Validation.ValidateAll();
			Assert(permission.IAP_ScopeInfo.HasError("Non-CW1 customer application should not have Audit Permission."));
		}
	}
}
