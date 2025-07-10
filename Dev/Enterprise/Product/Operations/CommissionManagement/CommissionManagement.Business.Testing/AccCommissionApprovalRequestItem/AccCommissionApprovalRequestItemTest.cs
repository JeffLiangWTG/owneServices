using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(AccCommissionApprovalRequestItem))]
	internal class AccCommissionApprovalRequestItemTest : EnterpriseBusinessObjectTestCase
	{
		#region Properties

		public void TestCRI_IsSelected_ReadOnly()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ADL";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "SCW";

			var approvalRequest = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
			approvalRequest.CRQ_GS_NKApprovingStaff1 = "ADL";
			approvalRequest.CRQ_GS_NKApprovingStaff2 = "SCW";

			var item = approvalRequest.Items.AddNew();
			item.CRI_CL0 = Factory.NewWithValidTestData<AccCommissionLine>().PK;
			AssertEquals("Should be allowed to while still unapproved", false, item.CRI_IsSelectedInfo.ReadOnly);

			approvalRequest.CRQ_Staff1HasApproved = true;
			AssertEquals("Should no longer be able to edit once at least one staff has approved", true, item.CRI_IsSelectedInfo.ReadOnly);

			approvalRequest.CRQ_Staff1HasApproved = false;
			AssertEquals("Should be allowed to while still unapproved", false, item.CRI_IsSelectedInfo.ReadOnly);

			approvalRequest.CRQ_Staff2HasApproved = true;
			AssertEquals("Should no longer be able to edit once at least one staff has approved", true, item.CRI_IsSelectedInfo.ReadOnly);
		}

		#endregion
	}
}
