using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ZAPPaymentController))]
	public class APPaymentControllerTestCase : ZPaymentControllerTestCase
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ZAPPayment;
		}

		protected override SecurityCheckpoint ExpectedCheckPointForNew
		{
			get { return Env.Security.NewPayablesPayment; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForDelete
		{
			get { return Env.Security.ReversePayablesPayment; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForView
		{
			get { return Env.Security.PayablesTransactions; }
		}

		protected override void CoreTestForShowNewForm()
		{
			ZAPPaymentController controller = new ZAPPaymentController();
			bool originalSecurity = controller.CheckPointForNew_ForTestOnly.IsAllowed;
			try
			{
				controller.CheckPointForNew_ForTestOnly.IsAllowed = true;

				using (IZForm newForm = controller.ShowNewForm())
				{
					AssertNotNull("New Form", newForm);
					AssertEquals("New Form Type", newForm.GetType(), typeof(PaymentApprovalForm));

					PaymentApprovalForm newFormAsPaymentApprovalForm = (PaymentApprovalForm)newForm;
					Type typeOfBusinessEntity = newFormAsPaymentApprovalForm.BusinessEntity.GetType();

					AssertEquals("Type of New BusinessEntity", typeof(APPaymentApprovalWithoutAuthorisation), typeOfBusinessEntity);
					AssertEquals("BusinessEntity in database", false, ((BusinessObject)newFormAsPaymentApprovalForm.BusinessEntity).IsInDatabase);

					var tabControl = (newForm as ZForm).FindSingle<ZTemplateTabControl>();
					AssertNotEquals("WorkflowTabPage Is Visible", -1, tabControl.TabPages.IndexOfKey("WorkflowTabPage"));
				}

				controller.CheckPointForNew_ForTestOnly.IsAllowed = false;

				AssertExceptionThrown<SecurityAccessDeniedException>("Inusfficient security privileges.", () =>
					{
						using (IZForm newForm = controller.ShowNewForm())
						{
							AssertNull("New Form", newForm);
							ZString expected = controller.CheckPointForNew_ForTestOnly.ErrorMessageForNotAllowed;
							AssertEquals("Security Message should be shown", expected, UnitTestUserNotification.Instance.LastMessage.Text);
						}
					});
			}
			finally
			{
				controller.CheckPointForNew_ForTestOnly.IsAllowed = originalSecurity;
			}
		}

		protected override ZPaymentController GetPaymentControllerForTheTest()
		{
			return new ZAPPaymentController();
		}

		protected override BusinessObject ParentTransactionHeaderRow
		{
			get { return TestTransaction; }
		}

		protected override void SetupTransactionHeaderRows()
		{
			TestTransaction = Factory.NewWithValidTestData<APPayment>();
			Factory.Save();
		}

		public override void TestModuleID()
		{
			AssertEquals(ModuleIDs.APTransaction, Controller.ModuleID);
		}

		APPayment TestTransaction;
	}
}
