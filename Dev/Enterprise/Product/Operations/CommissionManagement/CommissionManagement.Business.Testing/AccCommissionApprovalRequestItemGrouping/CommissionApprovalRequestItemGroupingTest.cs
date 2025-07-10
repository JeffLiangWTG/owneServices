using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(CommissionApprovalRequestItemGrouping))]
	internal class CommissionApprovalRequestItemGroupingTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIsSelected_ReadOnly()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ADL";

			var approvalRequest = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
			approvalRequest.CRQ_GS_NKApprovingStaff1 = "ADL";
			var approvalRequestItem = approvalRequest.Items.AddNew();
			approvalRequestItem.FillWithValidTestData();

			Factory.Save();

			var grouping = new CommissionApprovalRequestItemGrouping(Factory);
			grouping.Init(new[] { approvalRequestItem });

			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				approvalRequest.CRQ_Staff1HasApproved = false;
				AssertEquals(false, grouping.IsSelectedInfo.ReadOnly);

				approvalRequest.CRQ_Staff1HasApproved = true;
				AssertEquals(true, grouping.IsSelectedInfo.ReadOnly);
			}
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var line = Factory.NewWithValidTestData<AccCommissionLine>();
			Factory.Save();

			var requestItem = Factory.New<AccCommissionApprovalRequestItem>();
			requestItem.CRI_CL0 = line.PK;
			var grouping = new CommissionApprovalRequestItemGrouping(Factory);
			grouping.Init(new[] { requestItem });
			return grouping;
		}

		#endregion
	}
}
