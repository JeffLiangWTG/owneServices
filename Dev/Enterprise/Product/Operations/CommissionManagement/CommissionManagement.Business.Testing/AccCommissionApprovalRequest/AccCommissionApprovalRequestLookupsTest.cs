using CargoWise.EntityFramework.Testing;

namespace Enterprise.CommissionManagement.Business.Testing
{
	internal class AccCommissionApprovalRequestLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestApprovingStaff1s()
		{
			var approvalRequest = Factory.New<AccCommissionApprovalRequest>();
			var lookups = approvalRequest.Lookups;
			AssertType(typeof(CommissionAuthorizationStaffCollection), lookups.ApprovingStaff1s);
		}

		public void TestApprovingStaff2s()
		{
			var approvalRequest = Factory.New<AccCommissionApprovalRequest>();
			var lookups = approvalRequest.Lookups;
			AssertType(typeof(CommissionAuthorizationStaffCollection), lookups.ApprovingStaff2s);
		}
	}
}
