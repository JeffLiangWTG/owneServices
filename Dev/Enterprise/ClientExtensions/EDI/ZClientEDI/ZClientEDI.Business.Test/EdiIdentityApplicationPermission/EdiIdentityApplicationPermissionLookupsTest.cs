using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IdentityApplication.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IdentityApplicationPermission.Business.Testing
{
	[TestedType(typeof(EdiIdentityApplicationPermissionLookups))]
	public class EdiIdentityApplicationPermissionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPermissionListForCW1CustomerApplicationAndNonCW1CustomerApplication()
		{
			var customerApplication = Factory.New<EdiIdentityApplication>();
			customerApplication.IsCustomerApplication = true;
			var permission = customerApplication.Permissions.AddNew();
			var permissionList = permission.Lookups.ApplicationPermissions;

			AssertContainsExactElementsInAnyOrder("Non-CW1 customer application should not have Audit permission.", new[] { CustomerApplicationPermissionsList.Codes.CargoVisibilityAPI }, permissionList.GetAllCodes());

			var cw1Application = Factory.New<EdiIdentityApplication>();
			customerApplication.IDA_IDA_ParentApplication = cw1Application.PK;
			permissionList = permission.Lookups.ApplicationPermissions;

			AssertContainsExactElementsInAnyOrder(new[] { CustomerApplicationPermissionsList.Codes.CargoVisibilityAPI, CustomerApplicationPermissionsList.Codes.AuditAPI }, permissionList.GetAllCodes());
		}

		public void TestPermissionListForNonCustomerApplication()
		{
			var nonCustomerApplication = Factory.NewWithValidTestData<EdiIdentityApplication>();
			nonCustomerApplication.IsCustomerApplication = false;
			var permission = nonCustomerApplication.Permissions.AddNew();
			var permissionList = permission.Lookups.ApplicationPermissions;
			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Non customer application should only have 1 option '*' as permission is not implemented with Azure B2C", new[] { "*" }, permissionList.GetAllCodes());
		}
	}
}
