using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.Module.Transaction;
using Enterprise.Accounting.Module.Transaction.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module.TransactionApproval.Testing
{
	public abstract class APTransactionsLinkedToApprovalControllerTest<TBusinessObject, TForm> : APIncompleteTransactionsControllerTest<TBusinessObject, TForm>
		where TBusinessObject : InvoicingBase
		where TForm : BaseInvoicingForm
	{
		public override void TestShowCancelForm()
		{
			Assert("This module is not using this method.", true);
		}

		public override void TestGetForm_ReturnsNullForm_WhenRestoreSavedDataReturnsFailed()
		{
			Assert("This test is not applicable for transactions with approval request", true);
		}

		protected override void AssertGetFormBisoFactory(InvoicingBase bizo)
		{
			AssertEquals("A form should open bizo in the same factory as this is incomplete invoice with approval request restored in special way.", Factory._Instance, bizo.Factory._Instance);
		}

		protected override void AssertJobErrorInTestRestoreSavedDataHasShowErrorHandler(string jobNumber)
		{
			AssertNull("Invoice shouldn't be restored in the controller as it's restored in a linked approval request", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		protected override void AssertSwitchingToCurrentAPIncompleteTransactionFormDoesNotChangeDisplayMode(Func<APIncompleteTransactionsController, InvoicingBase, IZForm> showForm1, Func<APIncompleteTransactionsController, InvoicingBase, IZForm> showForm2)
		{
			Assert("This test is not applicable for transactions with approval request", true);
		}

		protected override void AssertSwitchingToOpenNewAPInvoiceFormSavedAsIncomplete(Func<APIncompleteTransactionsController, InvoicingBase, IZForm> showForm2)
		{
			Assert("This test is not applicable for transactions with approval request", true);
		}

		protected override void SaveAsIncomplete(TBusinessObject invoice)
		{
			TestObjectCreator.CreateAndSaveApprovalRequestForInvoiceInNewFactoryAsInProduction(invoice);
		}

		public override void TestGetValidControllerID()
		{
			var controllerId = GetControllerID();
			var relativeId = GetRelativeControllerID();
			var provider = ZControllerFactory.Create(controllerId) as INavigationControllerIDProvider;

			var invoice = GetBusinessObject();
			var relativeInvoice = GetBusinessObjectThatIsInTheDatabase();
			var invoicingBase = relativeInvoice as InvoicingBase;
			var relativeLedger = invoicingBase != null ? invoicingBase.AH_Ledger.ToString() : relativeInvoice.GetType().Name;
			var nonRelatedBizO = Factory.NewWithValidTestData<ARInvoice>();

			AssertEquals(string.Format("ControllerID for non-related bizO should be {0}.", controllerId.Name), controllerId, provider.GetValidControllerID(nonRelatedBizO));
			AssertEquals(string.Format("ControllerID for {0} transaction should be {1}.", invoice.AH_Ledger, relativeId.Name), relativeId, provider.GetValidControllerID(invoice));
			AssertEquals(string.Format("ControllerID for {0} transaction should be {1}.", relativeLedger, relativeId.Name), relativeId, provider.GetValidControllerID(relativeInvoice));

			AssertOtherGetValidControllerIDCase(provider);
		}

		public override void TestReportInvalidDataSource()
		{
			var dataSource = Factory.NewWithValidTestData<TBusinessObject>();
			Factory.Save();

			var controller = Controller as APTransactionsLinkedToApprovalController;
			using (var form = controller.GetForm_ForTest(dataSource))
			{
				var expectedErrorMessage =
					string.Format(@"Message: ZController {5} is handling an invalid bizO {0}. The correct controller ID should be {1}
AH_TransactionType: {2}
AH_Ledger: {3}
Details:
Header: PK = {4}", dataSource.GetType().ToString(), ReportInvalidDataSource_CorrectControllerID, dataSource.AH_TransactionType, dataSource.AH_Ledger, dataSource.PK.ToString(), GetControllerID());
				AssertEquals("Should have a developer notification exception.", 1, ExceptionReporterTestListener.Instance.Count);
				Assert("The developer notification exception should be about using invalid controller.", ExceptionReporterTestListener.Instance[0].InnerException.Message.Contains(expectedErrorMessage));
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		protected abstract ControllerID ReportInvalidDataSource_CorrectControllerID { get; }

		protected override SecurityCheckpoint ExpectedEditCheckPointForDirectEnteredTransaction => Env.Security.APInvoiceApproval_Edit_DirectEntered;

		protected override SecurityCheckpoint ExpectedEditCheckPointForUniveralXMLImportedTransaction => Env.Security.APInvoiceApproval_Edit_ImportSourced;

		protected override SecurityCheckpoint ExpectedEditHeaderCheckPointForDirectEnteredTransaction => Env.Security.APInvoiceApproval_Edit_DirectEntered_EditInvoiceHeader;

		protected override SecurityCheckpoint ExpectedEditHeaderCheckPointForUniveralXMLImportedTransaction => Env.Security.APInvoiceApproval_Edit_ImportSourced_EditInvoiceHeader;

		public override void TestGetFormCoreThroughProcessTask_NoEditSecurity()
		{
			Env.Security.APInvoiceApproval_Edit_DirectEntered.IsAllowed = false;

			try
			{
				var invoice = (InvoicingBase)GetBusinessObjectThatIsInTheDatabase();
				((IWorkflowProvider)invoice).WorkflowItems.Tasks.AddNew();
				invoice.SaveAsIncomplete();

				using (var processTasks = new ProcessTasksModule())
				{
					processTasks.ModuleDecisionProvider.HandleDefaultAction(((IWorkflowProvider)invoice).WorkflowItems.ToArray());
					var form = ZApplication.GetOpenForms().OfType<TForm>().FirstOrDefault();
					AssertNotNull(form);
					AssertEquals(ODisplayMode.ReadOnly, form.DisplayMode);
				}
			}
			finally
			{
				foreach (Form form in ZApplication.GetOpenForms().OfType<TForm>())
				{
					form.Dispose();
				}
			}
		}
	}
}
