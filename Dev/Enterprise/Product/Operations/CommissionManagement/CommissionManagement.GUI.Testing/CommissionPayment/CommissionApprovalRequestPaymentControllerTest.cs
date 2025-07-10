using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.CommissionManagement.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.CommissionManagement.GUI.Testing
{
	public class CommissionApprovalRequestPaymentControllerTest : TestCaseWithFactory
	{
		public void TestPromptUserForProcessPayment_BeforeApproved()
		{
			var approvalRequest = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
			approvalRequest.CRQ_GS_NKApprovingStaff1 = "ADL";
			approvalRequest.CRQ_Staff1HasApproved = false;
			var item = approvalRequest.Items.AddNew();
			item.FillWithValidTestData();

			Factory.Save();

			using (var form = new ZForm(approvalRequest))
			{
				form.Show();

				var paymentController = new CommissionApprovalRequestPaymentController(form, approvalRequest);
				paymentController.PromptUserForProcessPayment();
				CombineAssertions("Should not have processed payment", () =>
				{
					AssertEquals("LastMessage.Caption", "Unable to Process Payment", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("LastMessage.Text", "Cannot process payment until all authorization staff have approved this request.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("line.CL0_PaidDateTimeUtc", ZDateTime.Empty, item.CommissionLine.CL0_PaidDateTimeUtc);
				});
			}
		}
	}
}
