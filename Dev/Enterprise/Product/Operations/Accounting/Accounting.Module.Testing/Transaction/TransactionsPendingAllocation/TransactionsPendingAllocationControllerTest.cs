using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.Module.Transaction.Base.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Transaction.Testing
{
	[TestedType(typeof(TransactionsPendingAllocationController))]
	public class TransactionsPendingAllocationControllerTest : TransactionControllerWithReadOnlyBehaviourControlledBySourceTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.TransactionsPendingAllocation;
		}

		public void TestShowEditFormWithStatuses_WhenImplemented_ITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProvider()
		{
			var (transaction, request) = CreateTransaction();
			var expectedMessage = "This transaction is approved or awaiting approval and cannot be modified.";
			var (eRequestProvider, _) = TestObjectCreator.MockCountryFactoryForTPAAeInvoicing(isTPAAeInvoicingProviderImplemented: true);

			eRequestProvider.Setup(e => e.IsTransactionEditable(It.IsAny<AccTransactionHeader>())).Returns(true);
			TestShowEditFormWithTransaction(transaction, null);

			eRequestProvider.Setup(e => e.IsTransactionEditable(It.IsAny<AccTransactionHeader>())).Returns(false);
			TestShowEditFormWithTransaction(transaction, expectedMessage);
		}

		public void TestShowEditFormWithStatuses_WhenNotImplemented_ITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProvider()
		{
			var (transaction, request) = CreateTransaction();
			TestObjectCreator.MockCountryFactoryForTPAAeInvoicing(isTPAAeInvoicingProviderImplemented: false);

			TestShowEditFormWithTransaction(transaction, null);
		}

		public void TestShowEditFormWithCancelledEntity()
		{
			TestShowEditFormWithTransaction(CreateTransactionCancelled("A0001"), "This transaction is canceled and cannot be modified.");
		}

		public void TestShowEditFormWithExportedEntity()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("A0001", TestObjectCreator.Creditor1, 100);
			var genExport = TestObjectCreator.CreateGenExportBatchSequenceHeader(1001, transaction.PK, 1);
			Factory.Save();

			TestShowEditFormWithTransaction(transaction, "This transaction has been exported and cannot be edited");
		}

		TransactionPendingAllocation CreateTransactionCancelled(string transactionNumber)
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation(transactionNumber, TestObjectCreator.Creditor1, 100);
			Factory.Save();

			transaction.IsCancelled = true;

			return transaction;
		}

		(TransactionPendingAllocation, TransactionPendingAllocationApprovalRequest) CreateTransaction(/*string xp_ApprovalStatus, string transactionNumber*/)
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("A0001", TestObjectCreator.Creditor1, 100);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(transaction);
			Factory.Save();

			return (transaction, request);
		}

		void TestShowEditFormWithTransaction(TransactionPendingAllocation transaction, string expectedMessage)
		{
			var controller = new TransactionsPendingAllocationController();

			using (var form = controller.ShowEditForm(transaction))
			{
				if (expectedMessage == null)
				{
					Assert("Allocating form opened.", controller.GetOpenedForm(transaction) != null);
				}
				else
				{
					AssertEquals("Error should be shown", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestGetFormWithTransactionFormOpened()
		{
			var controller = new TransactionsPendingAllocationController();

			var invoicePending = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			var creditNotePending = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			invoicePending.AH_TransactionType = TransactionTypes.InvoicePendingAllocation;
			creditNotePending.AH_TransactionType = TransactionTypes.CreditNotePendingAllocation;
			Factory.Save();

			var invoiceAllocated = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePending).Invoice;
			var creditNoteAllocated = TransactionAllocationConverter.ConvertUnallocatedToAP(creditNotePending).Invoice;
			invoiceAllocated.Factory.RefreshEnabled = false;
			creditNoteAllocated.Factory.RefreshEnabled = false;
			invoiceAllocated.Factory.Save();
			creditNoteAllocated.Factory.Save();

			var invoiceController = ZControllerFactory.Create(ControllerIDs.APInvoice);
			var creditNoteController = ZControllerFactory.Create(ControllerIDs.APCreditNote);

			using (var form = invoiceController.ShowEditForm(invoiceAllocated))
			{
				Assert("Allocating Form opened.", invoiceController.GetOpenedForm(invoicePending) != null);

				using (var paForm = controller.ShowEditForm(invoicePending))
				{
					AssertEquals("Open PA form should switch to Allocating form.", form, paForm);
				}
			}

			using (var form = creditNoteController.ShowEditForm(creditNoteAllocated))
			{
				Assert("Allocating Form opened.", creditNoteController.GetOpenedForm(creditNotePending) != null);

				using (var paForm = controller.ShowEditForm(creditNotePending))
				{
					AssertEquals("Open PA form should switch to Allocating form.", form, paForm);
				}
			}
		}

		public void TestGetFormWithoutTransactionFormOpened()
		{
			var controller = new TransactionsPendingAllocationController();

			var invoicePending = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			var creditNotePending = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			invoicePending.AH_TransactionType = TransactionTypes.InvoicePendingAllocation;
			creditNotePending.AH_TransactionType = TransactionTypes.CreditNotePendingAllocation;
			Factory.Save();

			var invoiceController = ZControllerFactory.Create(ControllerIDs.APInvoice);
			var creditNoteController = ZControllerFactory.Create(ControllerIDs.APCreditNote);

			Assert("Allocating Form is not open.", invoiceController.GetOpenedForm(invoicePending) == null);
			using (var paForm = controller.ShowEditForm(invoicePending))
			{
				Assert("Open PA form should not switch Allocating form.", paForm as TransactionPendingAllocationForm != null);
			}

			Assert("Allocating Form is not opened.", creditNoteController.GetOpenedForm(creditNotePending) == null);
			using (var paForm = controller.ShowEditForm(creditNotePending))
			{
				Assert("Open PA form should not switch to Allocating form.", paForm as TransactionPendingAllocationForm != null);
			}
		}

		public void TestFindExistingAllocationController()
		{
			var controller = new TransactionsPendingAllocationController();

			var invoicePendingForAPInv = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			var creditNotePending = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			var invoicePendingForARCrd = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			invoicePendingForAPInv.AH_TransactionType = TransactionTypes.InvoicePendingAllocation;
			creditNotePending.AH_TransactionType = TransactionTypes.CreditNotePendingAllocation;
			invoicePendingForARCrd.AH_TransactionType = TransactionTypes.InvoicePendingAllocation;
			Factory.Save();

			var apInvoiceAllocated = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingForAPInv).Invoice;
			var apCreditNoteAllocated = TransactionAllocationConverter.ConvertUnallocatedToAP(creditNotePending).Invoice;
			var arCreditNoteAllocated = TransactionAllocationConverter.ConvertUnallocatedToAR(invoicePendingForARCrd).Invoice;
			apInvoiceAllocated.Factory.RefreshEnabled = false;
			apCreditNoteAllocated.Factory.RefreshEnabled = false;
			arCreditNoteAllocated.Factory.RefreshEnabled = false;
			apInvoiceAllocated.Factory.Save();
			apCreditNoteAllocated.Factory.Save();
			arCreditNoteAllocated.Factory.Save();

			var apInvoiceController = ZControllerFactory.Create(ControllerIDs.APInvoice);
			var apCreditNoteController = ZControllerFactory.Create(ControllerIDs.APCreditNote);
			var arCreditNoteController = ZControllerFactory.Create(ControllerIDs.ARCreditNote);

			AssertEquals("null data source", null, GetSwitchResult(controller, null));
			AssertEquals("null data source", null, GetOperationID(controller, null));
			AssertEquals("AP Credit Note Pending Allocation bizO", null, GetSwitchResult(controller, creditNotePending));
			AssertEquals("AP Credit Note Pending Allocation bizO", ControllerIDs.APCreditNote, GetOperationID(controller, creditNotePending));
			AssertEquals("AP Invoice Pending Allocation bizO", null, GetSwitchResult(controller, invoicePendingForAPInv));
			AssertEquals("AP Invoice Pending Allocation bizO", ControllerIDs.APInvoice, GetOperationID(controller, invoicePendingForAPInv));
			AssertEquals("AR Credit Note Pending Allocation bizO", null, GetSwitchResult(controller, invoicePendingForARCrd, LedgerTypes.AccountsReceivable));
			AssertEquals("AR Credit Note Pending Allocation bizO", ControllerIDs.ARCreditNote, GetOperationID(controller, invoicePendingForARCrd, LedgerTypes.AccountsReceivable));

			AssertEquals("No form of Allocating AP Credit Note bizO is opened.", null, apInvoiceController.GetOpenedForm(creditNotePending));
			AssertEquals("No form of Allocating AP Invoice bizO is opened.", null, apInvoiceController.GetOpenedForm(invoicePendingForAPInv));
			AssertEquals("No form of Allocating AR Credit Note bizO is opened.", null, apInvoiceController.GetOpenedForm(invoicePendingForARCrd));

			using (var form = apInvoiceController.ShowEditForm(apInvoiceAllocated))
			{
				AssertEquals("Allocating AP Credit Note bizO", null, GetSwitchResult(controller, creditNotePending));
				AssertEquals("Allocating AP Credit Note bizO", ControllerIDs.APCreditNote, GetOperationID(controller, creditNotePending));
				AssertEquals("Allocating AP Invoice bizO", ControllerIDs.APInvoice, GetSwitchResult(controller, invoicePendingForAPInv));
				AssertEquals("Allocating AP Invoice bizO", ControllerIDs.APInvoice, GetOperationID(controller, invoicePendingForAPInv));
				AssertEquals("Allocating AR Credit Note bizO", null, GetSwitchResult(controller, invoicePendingForARCrd, LedgerTypes.AccountsReceivable));
				AssertEquals("Allocating AR Credit Note bizO", ControllerIDs.ARCreditNote, GetOperationID(controller, invoicePendingForARCrd, LedgerTypes.AccountsReceivable));
			}

			AssertEquals("No form of Allocating AP Credit Note bizO is opened.", null, apInvoiceController.GetOpenedForm(creditNotePending));
			AssertEquals("No form of Allocating AP Invoice bizO is opened.", null, apInvoiceController.GetOpenedForm(invoicePendingForAPInv));
			AssertEquals("No form of Allocating AR Credit Note bizO is opened.", null, apInvoiceController.GetOpenedForm(invoicePendingForARCrd));

			using (var form = apCreditNoteController.ShowEditForm(apCreditNoteAllocated))
			{
				AssertEquals("Allocating AP Credit Note bizO", ControllerIDs.APCreditNote, GetSwitchResult(controller, creditNotePending));
				AssertEquals("Allocating AP Credit Note bizO", ControllerIDs.APCreditNote, GetOperationID(controller, creditNotePending));
				AssertEquals("Allocating AP Invoice bizO", null, GetSwitchResult(controller, invoicePendingForAPInv));
				AssertEquals("Allocating AP Invoice bizO", ControllerIDs.APInvoice, GetOperationID(controller, invoicePendingForAPInv));
				AssertEquals("Allocating AR Credit Note bizO", null, GetSwitchResult(controller, invoicePendingForARCrd, LedgerTypes.AccountsReceivable));
				AssertEquals("Allocating AR Credit Note bizO", ControllerIDs.ARCreditNote, GetOperationID(controller, invoicePendingForARCrd, LedgerTypes.AccountsReceivable));
			}

			AssertEquals("No form of Allocating AP Credit Note bizO is opened.", null, apInvoiceController.GetOpenedForm(creditNotePending));
			AssertEquals("No form of Allocating AP Invoice bizO is opened.", null, apInvoiceController.GetOpenedForm(invoicePendingForAPInv));
			AssertEquals("No form of Allocating AR Credit Note bizO is opened.", null, apInvoiceController.GetOpenedForm(invoicePendingForARCrd));

			using (var form = arCreditNoteController.ShowEditForm(arCreditNoteAllocated))
			{
				AssertEquals("Allocating AP Credit Note bizO", null, GetSwitchResult(controller, creditNotePending));
				AssertEquals("Allocating AP Credit Note bizO", ControllerIDs.APCreditNote, GetOperationID(controller, creditNotePending));
				AssertEquals("Allocating AP Invoice bizO", null, GetSwitchResult(controller, invoicePendingForAPInv));
				AssertEquals("Allocating AP Invoice bizO", ControllerIDs.APInvoice, GetOperationID(controller, invoicePendingForAPInv));
				AssertEquals("Allocating AR Credit Note bizO", ControllerIDs.ARCreditNote, GetSwitchResult(controller, invoicePendingForARCrd, LedgerTypes.AccountsReceivable));
				AssertEquals("Allocating AR Credit Note bizO", ControllerIDs.ARCreditNote, GetOperationID(controller, invoicePendingForARCrd, LedgerTypes.AccountsReceivable));
			}
		}

		public void TestFindExistingAllocationControllerWithSignInvertedPendingTransaction()
		{
			var controller = new TransactionsPendingAllocationController();

			var transactionPending = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			transactionPending.AH_TransactionType = TransactionTypes.InvoicePendingAllocation;
			transactionPending.AH_OSExTaxAmount = 100;
			Factory.Save();

			var invoiceAllocated = TransactionAllocationConverter.ConvertUnallocatedToAP(transactionPending).Invoice;
			invoiceAllocated.Factory.RefreshEnabled = false;
			invoiceAllocated.Factory.Save();

			var invoiceController = ZControllerFactory.Create(ControllerIDs.APInvoice);

			AssertEquals("No form of Allocating Invoice bizO is opened.", null, invoiceController.GetOpenedForm(transactionPending));
			AssertEquals("Allocating Invoice bizO", null, GetSwitchResult(controller, transactionPending));
			AssertEquals("Allocating Invoice bizO", ControllerIDs.APInvoice, GetOperationID(controller, transactionPending));

			using (var form = invoiceController.ShowEditForm(invoiceAllocated))
			{
				AssertEquals("Form of Allocating Invoice bizO is opened.", form, invoiceController.GetOpenedForm(transactionPending));
				AssertEquals("Allocating Invoice bizO", ControllerIDs.APInvoice, GetSwitchResult(controller, transactionPending));
				AssertEquals("Allocating Invoice bizO", ControllerIDs.APInvoice, GetOperationID(controller, transactionPending));

				var newFactory = new BusinessObjectFactory();
				var invoiceInAnotherFactory = newFactory.Load<TransactionPendingAllocation>(transactionPending.PK);
				invoiceInAnotherFactory.AH_OSExTaxAmount = -100;
				invoiceInAnotherFactory.AH_Ledger = LedgerTypes.TransactionsPendingAllocation;
				invoiceInAnotherFactory.AH_TransactionType = TransactionTypes.CreditNotePendingAllocation;

				AssertEquals("Allocating CreditNote bizO", ControllerIDs.APInvoice, GetSwitchResult(controller, invoiceInAnotherFactory));
				AssertEquals("Allocating CreditNote bizO", ControllerIDs.APCreditNote, GetOperationID(controller, invoiceInAnotherFactory));
			}
		}

		ControllerID GetSwitchResult(TransactionsPendingAllocationController source, TransactionPendingAllocation dataSource, string ledger = LedgerTypes.AccountsPayable)
		{
			var form = source.TrySwitchToExistingAllocationForm(dataSource, ledger);
			return form == null ? null : form.ControllerID;
		}

		ControllerID GetOperationID(TransactionsPendingAllocationController source, TransactionPendingAllocation dataSource, string ledger = LedgerTypes.AccountsPayable)
		{
			var result = source.GetOperationController(dataSource, ledger);
			return result == null ? null : result.ID;
		}

		public void TestGetFormCoreThroughProcessTask_NoException()
		{
			try
			{
				var transactionPending = Factory.NewWithValidTestData<TransactionPendingAllocation>();
				transactionPending.AH_TransactionType = TransactionTypes.InvoicePendingAllocation;
				transactionPending.AH_OSExTaxAmount = 100;
				((IWorkflowProvider)transactionPending).WorkflowItems.Tasks.AddNew();
				Factory.Save();

				using (var processTasks = new ProcessTasksModule())
				{
					processTasks.ModuleDecisionProvider.HandleDefaultAction(((IWorkflowProvider)transactionPending).WorkflowItems.ToArray());
					var form = ZApplication.GetOpenForms().OfType<TransactionPendingAllocationForm>().FirstOrDefault();
					AssertNotNull(form);
				}
			}
			finally
			{
				foreach (Form form in ZApplication.GetOpenForms().OfType<TransactionPendingAllocationForm>())
				{
					form.Dispose();
				}
			}
		}

		public void TestGetFormCoreThroughProcessTask_NoEditSecurity()
		{
			try
			{
				Env.Security.TransactionsPendingAllocationEdit_DirectEntered.IsAllowed = false;

				var transactionPending = Factory.NewWithValidTestData<TransactionPendingAllocation>();
				transactionPending.AH_TransactionType = TransactionTypes.InvoicePendingAllocation;
				transactionPending.AH_OSExTaxAmount = 100;
				((IWorkflowProvider)transactionPending).WorkflowItems.Tasks.AddNew();
				Factory.Save();

				using (var processTasks = new ProcessTasksModule())
				{
					processTasks.ModuleDecisionProvider.HandleDefaultAction(((IWorkflowProvider)transactionPending).WorkflowItems.ToArray());
					var form = ZApplication.GetOpenForms().OfType<TransactionPendingAllocationForm>().FirstOrDefault();
					AssertNotNull(form);
					AssertEquals(ODisplayMode.ReadOnly, form.DisplayMode);
				}
			}
			finally
			{
				foreach (Form form in ZApplication.GetOpenForms().OfType<TransactionPendingAllocationForm>())
				{
					form.Dispose();
				}
			}
		}

		#region Implementation

		protected override BusinessObject ParentTransactionHeaderRow => transactionPendingAllocation;

		protected override InvoicingBase TransactionImportedFromUniversalXML
		{
			get
			{
				var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
				var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
				request.Initialize(transaction, "some xml", true);
				Factory.Save();
				Assert("Precondition: HasUniversalTransaction", transaction.IsImportedFromUniversalXML);
				return transaction;
			}
		}

		protected override void SetupTransactionHeaderRows()
		{
			transactionPendingAllocation = GetBusinessObjectThatIsInTheDatabase() as TransactionPendingAllocation;
		}

		protected override InvoicingBase ChangeStateOfParentTransactionHeaderRow()
		{
			var invoiceAllocated = TransactionAllocationConverter.ConvertUnallocatedToAP(transactionPendingAllocation).Invoice;
			invoiceAllocated.Factory.RefreshEnabled = false;
			invoiceAllocated.Factory.Save();

			return transactionPendingAllocation;
		}

		protected override string MessageToShowWhenControllerMismatchForBusinessEntity => "The transaction pending allocation cannot be displayed because another user has changed or deleted the record. Closing and re-opening this window will refresh your data.";

		TransactionPendingAllocation transactionPendingAllocation;

		protected override SecurityCheckpoint ExpectedEditCheckPointForDirectEnteredTransaction => Env.Security.TransactionsPendingAllocationEdit_DirectEntered;

		protected override SecurityCheckpoint ExpectedEditCheckPointForUniveralXMLImportedTransaction => Env.Security.TransactionsPendingAllocationEdit_ImportSourced;

		protected override SecurityCheckpoint ExpectedEditHeaderCheckPointForDirectEnteredTransaction => Env.Security.TransactionsPendingAllocationEdit_DirectEntered_EditInvoiceHeader;

		protected override SecurityCheckpoint ExpectedEditHeaderCheckPointForUniveralXMLImportedTransaction => Env.Security.TransactionsPendingAllocationEdit_ImportSourced_EditInvoiceHeader;

		#endregion

		#region INavigationControllerIDProvider

		public void TestGetValidControllerID()
		{
			var controller = Controller as INavigationControllerIDProvider;

			var nonRelatedBizO = Factory.NewWithValidTestData<ARInvoice>();
			var invoicePending = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			var creditNotePending = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			invoicePending.AH_TransactionType = TransactionTypes.InvoicePendingAllocation;
			creditNotePending.AH_TransactionType = TransactionTypes.CreditNotePendingAllocation;
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			var creditNote = Factory.NewWithValidTestData<APCreditNote>();
			var incompleteInvoice = Factory.NewWithValidTestData<APInvoice>();
			incompleteInvoice.SaveAsIncomplete();
			var incompleteCreditNote = Factory.NewWithValidTestData<APCreditNote>();
			incompleteCreditNote.SaveAsIncomplete();

			AssertEquals("ControllerID for non-related bizO should be TransactionsPendingAllocation.", ControllerIDs.TransactionsPendingAllocation, controller.GetValidControllerID(nonRelatedBizO));
			AssertEquals("ControllerID for AP Credit Note bizO should be APCreditNote.", ControllerIDs.APCreditNote, controller.GetValidControllerID(creditNote));
			AssertEquals("ControllerID for AP Invoice bizO should be APInvoice.", ControllerIDs.APInvoice, controller.GetValidControllerID(invoice));
			AssertEquals("ControllerID for Credit Note Pending Allocation bizO should be TransactionsPendingAllocation.", ControllerIDs.TransactionsPendingAllocation, controller.GetValidControllerID(creditNotePending));
			AssertEquals("ControllerID for Invoice Pending Allocation bizO should be TransactionsPendingAllocation.", ControllerIDs.TransactionsPendingAllocation, controller.GetValidControllerID(invoicePending));
			AssertEquals("ControllerID for AP Incomplete Invoice bizO should be APIncompleteInvoice.", ControllerIDs.APIncompleteInvoice, controller.GetValidControllerID(incompleteInvoice));
			AssertEquals("ControllerID for AP Incomplete CreditNote bizO should be APIncompleteCreditNote.", ControllerIDs.APIncompleteCreditNote, controller.GetValidControllerID(incompleteCreditNote));
		}

		public void TestShouldLoadBusinessObject()
		{
			var controller = Controller as TransactionsPendingAllocationController;
			AssertEquals(true, controller.ShouldLoadBusinessObject);
		}

		#endregion
	}
}
