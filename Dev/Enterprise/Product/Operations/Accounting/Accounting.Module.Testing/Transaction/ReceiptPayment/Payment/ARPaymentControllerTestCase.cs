using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ZARPaymentController))]
	class ARPaymentControllerTestCase : ZPaymentControllerTestCase
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ZARPayment;
		}

		protected override SecurityCheckpoint ExpectedCheckPointForNew
		{
			get { return Env.Security.NewReceivablesPayment; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForDelete
		{
			get { return Env.Security.ReverseReceivablesPayment; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForView
		{
			get { return Env.Security.ReceivablesTransactions; }
		}

		protected override void CoreTestForShowNewForm()
		{
			ZARPaymentController controller = new ZARPaymentController();

			using (IZForm newForm = controller.ShowNewForm())
			{
				AssertNotNull("New Form", newForm);
				AssertEquals("New Form Type", newForm.GetType(), typeof(PaymentApprovalForm));

				PaymentApprovalForm newFormAsPaymentApprovalForm = (PaymentApprovalForm)newForm;
				Type typeOfBusinessEntity = newFormAsPaymentApprovalForm.BusinessEntity.GetType();

				AssertEquals("Type of New BusinessEntity", typeof(ARPaymentApprovalWithoutAuthorisation), typeOfBusinessEntity);
				AssertEquals("BusinessEntity in database", false, ((BusinessObject)newFormAsPaymentApprovalForm.BusinessEntity).IsInDatabase);

				var tabControl = (newForm as ZForm).FindSingle<ZTemplateTabControl>();
				AssertNotEquals("WorkflowTabPage Is Visible", -1, tabControl.TabPages.IndexOfKey("WorkflowTabPage"));
			}
		}

		protected override ZPaymentController GetPaymentControllerForTheTest()
		{
			return new ZARPaymentController();
		}

		protected override BusinessObject ParentTransactionHeaderRow
		{
			get { return TestTransaction; }
		}

		protected override void SetupTransactionHeaderRows()
		{
			TestTransaction = Factory.NewWithValidTestData<ARPayment>();
			Factory.Save();
		}

		public override void TestModuleID()
		{
			AssertEquals(ModuleIDs.ARTransaction, Controller.ModuleID);
		}

		ARPayment TestTransaction;
	}
}
