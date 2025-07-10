using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Accounting.Module.Testing
{
	public abstract class PaymentProcessingControllerTest : ZControllerBasherTest
	{
		public void TestShowEditFormForPostedApproval()
		{
			PaymentApprovalBase approval = (PaymentApprovalBase)GetBusinessObjectThatIsInTheDatabase();
			PaymentProcessingController controller = GetNewController();

			approval.AV_Status = ZArchitecture.Core.PaymentApprovalStatus.FullyApproved;

			using (IZForm form = controller.ShowEditForm(approval))
			{
				AssertNotNull("Form", form);
				AssertEquals("Form Display Mode", ODisplayMode.Browse, form.DisplayMode);
			}

			approval.AV_Status = ZArchitecture.Core.PaymentApprovalStatus.Posted;

			using (IZForm form = controller.ShowEditForm(approval))
			{
				AssertNotNull("Form", form);
				AssertEquals("Form Display Mode", ODisplayMode.ReadOnly, form.DisplayMode);
			}
		}

		public void TestShowDeleteFormForPostedApproval()
		{
			PaymentApprovalBase approval = (PaymentApprovalBase)GetBusinessObjectThatIsInTheDatabase();
			PaymentProcessingController controller = GetNewController();

			approval.AV_Status = ZArchitecture.Core.PaymentApprovalStatus.FullyApproved;
			Factory.Save();
			using (IZForm form = controller.ShowDeleteForm(approval))
			{
				AssertNotNull("Form", form);
				AssertEquals("Form Display Mode", ODisplayMode.Delete, form.DisplayMode);
			}

			approval.AV_Status = ZArchitecture.Core.PaymentApprovalStatus.Posted;
			Factory.Save();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			bool exceptionThrown = false;

			try
			{
				controller.ShowDeleteForm(approval);
			}
			catch (ControllerShowDeleteFormNotSupportedException e)
			{
				exceptionThrown = true;

				AssertEquals("Message should be displayed",
					PaymentProcessingController.CannotDeletePostedTransactionMessage, e.Message);
			}

			AssertEquals("Exception should have been thrown", true, exceptionThrown);
		}

		protected abstract PaymentProcessingController GetNewController();
	}
}
