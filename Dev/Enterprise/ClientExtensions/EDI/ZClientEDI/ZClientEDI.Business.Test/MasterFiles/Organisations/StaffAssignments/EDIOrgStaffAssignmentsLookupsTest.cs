using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	internal class EDIOrgStaffAssignmentsLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestStaffRoles()
		{
			var staffAssignment = Factory.NewWithValidTestData<EDIOrgStaffAssignments>();
			Assert(staffAssignment.Lookups.StaffRoles.ContainsCode("RM1"));
			Assert(staffAssignment.Lookups.StaffRoles.ContainsCode("RM2"));
		}
	}
}
