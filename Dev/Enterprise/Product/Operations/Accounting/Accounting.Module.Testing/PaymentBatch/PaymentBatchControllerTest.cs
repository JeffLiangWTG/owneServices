using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(PaymentBatchController))]
	public class PaymentBatchControllerTest : ZControllerBasherTest
	{
		public override void TestNewForm()
		{
			Assert("Currently not support", true);
		}

		public override void TestViewForm()
		{
			var batch = Factory.NewWithValidTestData<AccPaymentBatch>();
			Factory.Save();

			using (var form = Controller.ShowViewForm(batch))
			{
				AssertNotNull(form);
				AssertType<PaymentBatchForm>(form);
				AssertType<APPaymentBatchPoster>(form.BusinessEntityForPersistingForm);
				AssertEquals(ODisplayMode.ReadOnly, form.DisplayMode);
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
			}

			Env.Security.ViewPaymentBatch.IsAllowed = false;
			AssertEquals(Env.Security.ViewPaymentBatch, Controller.GetCheckPointForView(batch));
			using (var form = Controller.ShowViewForm(batch))
			{
				AssertNull(form);
				AssertEquals(Env.Security.ViewPaymentBatch.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public override void TestEditForm()
		{
			var batch = Factory.NewWithValidTestData<AccPaymentBatch>();
			AssertEquals("Perconditon", Core.Constants.AccPaymentBatchStatus.Working, batch.APB_Status);
			Factory.Save();

			using (var form = Controller.ShowEditForm(batch))
			{
				AssertNotNull(form);
				AssertType<PaymentBatchForm>(form);
				AssertType<APPaymentBatchPoster>(form.BusinessEntityForPersistingForm);
				AssertEquals(ODisplayMode.Browse, form.DisplayMode);
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
			}

			Env.Security.EditPaymentBatch.IsAllowed = false;
			AssertEquals(Env.Security.EditPaymentBatch, Controller.GetCheckPointForEdit(batch));
			using (var form = Controller.ShowEditForm(batch))
			{
				AssertNotNull(form);
				AssertEquals(ODisplayMode.ReadOnly, form.DisplayMode);
			}
		}

		public void TestShowEditFormWhenStatusNotEqualToWorking()
		{
			var batch = Factory.NewWithValidTestData<AccPaymentBatch>();
			batch.APB_Status = Core.Constants.AccPaymentBatchStatus.Completed;
			Factory.Save();
			AssertNotEquals("Perconditon", Core.Constants.AccPaymentBatchStatus.Working, batch.APB_Status);
			using (var form = Controller.ShowEditForm(batch))
			{
				AssertNotNull(form);
				AssertEquals(ODisplayMode.ReadOnly, form.DisplayMode);
			}

			Env.Security.ViewPaymentBatch.IsAllowed = false;
			AssertEquals(Env.Security.ViewPaymentBatch, Controller.GetCheckPointForView(batch));

			using (var form = Controller.ShowEditForm(batch))
			{
				AssertNull(form);
				AssertEquals(Env.Security.ViewPaymentBatch.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public override void TestDeleteForm()
		{
			Assert("Currently not support", true);
		}

		public override void TestTemplateCopyForm()
		{
			Assert("Currently not support", true);
		}

		public void TestPaymentBatchController()
		{
			var controller = new PaymentBatchController();

			AssertEquals(ModuleIDs.PaymentBatch, controller.ModuleID);
			Assert(controller.MakeUrlsOnlyOpenableForCurrentCompany);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.PaymentBatch;
		}
	}
}
