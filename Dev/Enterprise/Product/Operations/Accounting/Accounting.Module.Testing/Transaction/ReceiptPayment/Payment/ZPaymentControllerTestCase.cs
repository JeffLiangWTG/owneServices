using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module.Testing
{
	public abstract class ZPaymentControllerTestCase : AccountingTransactionControllerTest
	{
		public void TestShowNewForm()
		{
			CoreTestForShowNewForm();
		}

		protected abstract void CoreTestForShowNewForm();

		protected override void TearDown()
		{
			if (ZFormModaliser.ActiveForm != null)
			{
				ZForm activeForm = ZFormModaliser.ActiveForm as ZForm;

				if (activeForm != null)
				{
					activeForm.Dispose();
				}
			}

			base.TearDown();
		}

		public void TestGetForm()
		{
			ZPaymentController paymentController = GetPaymentControllerForTheTest();
			PaymentApprovalBase paymentApproval = paymentController.GetNewPaymentApproval_ForTestOnly();
			using (IZForm newForm = paymentController.GetForm_ForTestOnly(paymentApproval))
			{
				AssertNotNull("New form shouldn't be null", newForm);
				Assert("New form shouldbe PaymentApprovalForm", newForm is PaymentApprovalForm);
			}
			ARPayment aRPayment = Factory.NewWithValidTestData<ARPayment>();
			using (IZForm newForm = paymentController.GetForm_ForTestOnly(aRPayment))
			{
				AssertNotNull("New form shouldn't be null", newForm);
				Assert("New form shouldbe PaymentForm", newForm is PaymentForm);
			}
			APPayment aPPayment = Factory.NewWithValidTestData<APPayment>();
			using (IZForm newForm = paymentController.GetForm_ForTestOnly(aPPayment))
			{
				AssertNotNull("New form shouldn't be null", newForm);
				Assert("New form shouldbe PaymentForm", newForm is APPaymentForm);
			}
		}

		public abstract void TestModuleID();

		protected abstract ZPaymentController GetPaymentControllerForTheTest();
	}
}
