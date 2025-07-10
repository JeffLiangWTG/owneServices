using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Testing
{
	sealed class StaffRolesNotificationHelperTest : TestCaseWithFactory
	{
		public void TestGetRoles()
		{
			StaffRolesNotificationTestHelper.AssertDefaultRoles(StaffRolesNotificationHelper.GetRoles());
		}

		public void TestGetRolesWithCustomRoles()
		{
			var staffRoles = new CodeDescriptionPairList(DataRegistry.Instance.OrgStaffMemberAssignmentRoles);
			staffRoles.AddPairIfNotExist("BRO", "Brogrammer");
			DataRegistry.Instance.OrgStaffMemberAssignmentRoles = staffRoles;

			StaffRolesNotificationTestHelper.AssertRoles(StaffRolesNotificationHelper.GetRoles(), new CodeDescriptionPair("BRO", "Brogrammer"));
		}

		public void TestSetAllBoolsTo()
		{
			CodeDescriptionBoolDisallowNewCollection testRoles = StaffRolesNotificationHelper.GetRoles();
			StaffRolesNotificationHelper.SetAllBoolsTo(testRoles, true);

			StaffRolesNotificationTestHelper.AssertBoolValue(testRoles, true, false, System.Array.Empty<string>());
		}

		public void TestSetBoolTo()
		{
			CodeDescriptionBoolDisallowNewCollection testRoles = StaffRolesNotificationHelper.GetRoles();
			StaffRolesNotificationHelper.SetBoolTo(testRoles, true, "ACT");

			StaffRolesNotificationTestHelper.AssertBoolValue(testRoles, true, true, "ACT");
		}
	}
}
