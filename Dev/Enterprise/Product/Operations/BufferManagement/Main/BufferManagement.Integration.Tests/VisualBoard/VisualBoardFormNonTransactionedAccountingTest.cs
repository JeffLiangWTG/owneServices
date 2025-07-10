using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.Accounting.GUI.ARAP.Contra;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.GUI.ARAP.Invoicing.IntercompanyCostsApportionment;
using Enterprise.Accounting.GUI.ARAP.Journal;
using Enterprise.Accounting.GUI.CashBook;
using Enterprise.Accounting.GUI.CashBook.Transfer;
using Enterprise.BufferManagement.GUI.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.VisualBoards.GUI;
using Enterprise.VisualBoards.GUI.Test;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Integration.Tests
{
	public class VisualBoardFormNonTransactionedAccountingTest : VisualBoardFormBaseNonTransactionedTest
	{
		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBoard_ShouldOpenPayInvoicesFormInMainThread()
		{
			var mainThreadId = Factory.ThreadSentry.OwnerThread.ThreadID;
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			var bizo = Factory.NewWithValidTestData<APInvoice>();
			bizo.AH_OH = testOrg.PK;
			new TestObjectCreator(Factory).CreateInvoiceLine(bizo, bizo.TransactionCurrency, bizo.AH_ExchangeRate, 94m, 0m, 0m, 94m, 0m, 0m);

			var board = SetupBoardWithGridAndFilter(ModuleIDs.APTransaction);

			Factory.Save();

			AssertShowFormFromBoard_ShouldOpenInMainThread<PaymentBatchForm>(board, "Pay Invoices");
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBoard_WithTransactionsPendingAllocationGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingEdit()
		{
			Board_WithAccountingGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingContextMenuItem<TransactionPendingAllocationForm>(
				ModuleIDs.TransactionsPendingAllocation, "&Edit");
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBoard_WithTransactionsPendingAllocationGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingDelete()
		{
			Board_WithAccountingGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingContextMenuItem<TransactionPendingAllocationForm>(
				ModuleIDs.TransactionsPendingAllocation, "&Delete");
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBoard_WithAPIncompleteInvoicesGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingEdit()
		{
			Board_WithAccountingGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingContextMenuItem<InvoiceForm>(
				ModuleIDs.APIncompleteInvoices, "&Edit", "111", LedgerTypes.IncompleteTransactions, TransactionTypes.IncompleteInvoice, paymentApprovalAction: (transaction) =>
				{
					((APInvoice)transaction).Lines.AddNew();
					((APInvoice)transaction).SaveAsIncomplete();
				}, instance: Factory.NewWithValidTestData<APInvoice>());
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBoard_WithUnapprovedTransactionGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingEdit()
		{
			Board_WithAccountingGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingContextMenuItem<UAInvoiceForm>(
				ModuleIDs.UnapprovedTransaction, "&Edit", "111", LedgerTypes.UnapprovedPayableTransactions, TransactionTypes.UAInvoice);
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBoard_WithTransactionsPendingAllocationGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingAllocateTransactions()
		{
			Board_WithAccountingGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingContextMenuItem<InvoiceForm>(
				ModuleIDs.TransactionsPendingAllocation, "Allocate Transactions");
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBoard_WithAPTransactionGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingNewInvoice()
		{
			Board_WithAccountingGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingContextMenuItem<InvoiceForm>(
				ModuleIDs.APTransaction, "New Invoice");
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBoard_WithAPTransactionGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingNewCreditNote()
		{
			Board_WithAccountingGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingContextMenuItem<CreditNoteForm>(
				ModuleIDs.APTransaction, "New Credit Note");
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBoard_WithAPTransactionGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingNewAdjustmentNote()
		{
			Board_WithAccountingGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingContextMenuItem<AdjustmentNoteForm>(
				ModuleIDs.APTransaction, "New Adjustment Note");
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBoard_WithAPTransactionGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingNewJournal()
		{
			Board_WithAccountingGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingContextMenuItem<JournalBaseForm>(
				ModuleIDs.APTransaction, "New Journal");
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBoard_WithAPTransactionGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingNewTransfer()
		{
			Board_WithAccountingGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingContextMenuItem<TransferForm>(
				ModuleIDs.APTransaction, "New Transfer");
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBoard_WithAPTransactionGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingNewContra()
		{
			Board_WithAccountingGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingContextMenuItem<ContraForm>(
				ModuleIDs.APTransaction, "New Contra");
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBoard_WithAPTransactionGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingNewReceipt()
		{
			Board_WithAccountingGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingContextMenuItem<ReceiptForm>(
				ModuleIDs.APTransaction, "New Receipt");
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBoard_WithAPTransactionGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingNewPayment()
		{
			Board_WithAccountingGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingContextMenuItem<PaymentApprovalForm>(
				ModuleIDs.APTransaction, "New Payment");
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBoard_WithAPTransactionGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingNewOverheadCostsApportionment()
		{
			Board_WithAccountingGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingContextMenuItem<IntercompanyCostsApportionmentForm>(
				ModuleIDs.APTransaction, "New Overhead Costs Apportionment");
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBoard_WithARTransactionGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingNewPeriodicInvoice()
		{
			Board_WithAccountingGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingContextMenuItem<PeriodicInvoicingForm>(
				ModuleIDs.ARTransaction, "New Periodic Invoice");
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBoard_WithARTransactionGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingNewBulkPeriodicInvoices()
		{
			Board_WithAccountingGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingContextMenuItem<PeriodicInvoicingBulkForm>(
				ModuleIDs.ARTransaction, "New Bulk Periodic Invoices");
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBoard_WithCashbookTransactionGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingNewOpeningReceipt()
		{
			Board_WithAccountingGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingContextMenuItem<OpeningReceiptForm>(
				ModuleIDs.CashbookTransaction, "New Opening Receipt");
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBoard_WithCashbookTransactionGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingNewOpeningPayment()
		{
			Board_WithAccountingGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingContextMenuItem<OpeningPaymentForm>(
				ModuleIDs.CashbookTransaction, "New Opening Payment");
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBoard_WithCashbookTransactionGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingNewBankCurrencyAdjustment()
		{
			Board_WithAccountingGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingContextMenuItem<NewCashBookExchangeDiffForm>(
				ModuleIDs.CashbookTransaction, "New Bank Currency Adjustment");
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBoard_WithCashbookTransactionGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingNewBankTransfer()
		{
			Board_WithAccountingGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingContextMenuItem<BankTransferForm>(
				ModuleIDs.CashbookTransaction, "New Bank Transfer");
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBoard_WithCashbookTransactionGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingNewDirectReceipt()
		{
			Board_WithAccountingGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingContextMenuItem<DirectReceiptForm>(
				ModuleIDs.CashbookTransaction, "New Direct Receipt");
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBoard_WithCashbookTransactionGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingNewDirectPayment()
		{
			Board_WithAccountingGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingContextMenuItem<DirectPaymentForm>(
				ModuleIDs.CashbookTransaction, "New Direct Payment");
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBoard_WithUnapprovedTransactionGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingNewInvoice()
		{
			Board_WithAccountingGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingContextMenuItem<UAInvoiceForm>(
				ModuleIDs.UnapprovedTransaction, "New Invoice");
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBoard_WithUnapprovedTransactionGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingNewCreditNote()
		{
			Board_WithAccountingGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingContextMenuItem<UACreditNoteForm>(
				ModuleIDs.UnapprovedTransaction, "New Credit Note");
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBoard_WithAPEnquiryGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingNewInvoice()
		{
			Board_WithAccountingGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingContextMenuItem<InvoiceForm>(
				ModuleIDs.APEnquiry, "New Invoice");
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBoard_WithAREnquiryGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingNewInvoice()
		{
			Board_WithAccountingGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingContextMenuItem<InvoiceForm>(
				ModuleIDs.AREnquiry, "New Invoice");
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBoard_APPaymentProcessing_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingDeleteInvoice()
		{
			Board_WithAccountingGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingContextMenuItem<PaymentApprovalWithAuthorisationForm>(
				ModuleIDs.APPaymentProcessing, "&Delete", paymentApprovalAction: (transaction) =>
				{
					var approval = Factory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
					approval.AV_AH = transaction.PK;
				});
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBoard_ARPaymentProcessing_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingDeleteInvoice()
		{
			Board_WithAccountingGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingContextMenuItem<PaymentApprovalWithAuthorisationForm>(
				ModuleIDs.ARPaymentProcessing, "&Delete", paymentApprovalAction: (transaction) =>
				{
					var approval = Factory.NewWithValidTestData<ARPaymentApprovalWithAuthorisation>();
					approval.AV_AH = transaction.PK;
				});
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBoard_WithAPTransactionGrid_WithDepartmentFilter_ShouldNotCauseThreadSentryException_AfterEditingDepartment()
		{
			var parentDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			parentDepartment.GE_SystemCode = true;

			var department = Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_SystemCode = false;
			department.GE_Desc = "Test Department";

			var board = SetupBoardWithGridAndFilter(
				ModuleIDs.APTransaction,
				moduleFilter => FilterStripsTestHelper.AddFilterStrip<ModuleGuidFilter>(moduleFilter, "Department", filter => filter.Property = department.PK));

			Factory.Save();

			using (var form = VisualBoardFormDisplayer.ShowBoard(board))
			{
				form.AwaitAll();
				var departmentGuidFindBox = form.FindSingle<ZGuidFindBox>(guidFindBox => guidFindBox.Name == "PropertyFindBox");
				KeySender.SendKeyDownToProcessCmdKey(departmentGuidFindBox.CodeBox, Keys.F3);
				form.AwaitAll();

				var departmentForm = ZApplication.GetOpenForms().OfType<GlbDepartmentForm>().Single();
				var parentDepartmentGuidFindBox = departmentForm.FindSingle<ZGuidFindBox>(guidFindBox => guidFindBox.Name == "GE_GEBoundGuidFindBox");
				parentDepartmentGuidFindBox.CodeBox.Text = parentDepartment.GE_Code;
				parentDepartmentGuidFindBox.CommitBoundValue();

				var continueWithSave = departmentForm.FireSaveButton();
				AssertEquals("The department should be saved successfully without validation errors.", ContinueWithSave.Yes, continueWithSave);
				departmentForm.Close();
			}
		}

		void Board_WithAccountingGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingContextMenuItem<T>(
			ModuleIdentifier module, string menuItem, string num = null, string ledger = null, string type = null, Action<AccTransactionHeader> paymentApprovalAction = null, AccTransactionHeader instance = null) where T : Form
		{
			var bizo = new TransactionsPendingAllocation(Factory);
			var transaction1 = instance ?? bizo.Transactions.AddNew();

			if (num != null)
			{
				transaction1.AH_TransactionNum = num;
				transaction1.AH_Ledger = ledger;
				transaction1.AH_TransactionType = type;
			}

			paymentApprovalAction?.Invoke(transaction1);

			var board = SetupBoardWithGridAndFilter(module);

			Factory.Save();

			try
			{
				using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
				using (var form = VisualBoardFormDisplayer.ShowBoard(board))
				{
					form.AwaitAll();
					VisualBoardFormDisplayer.ShowBoard(board);
					var visualBoardForm = ZApplication.GetOpenForms().OfType<VisualBoardForm>().SingleOrDefault();

					var grid = visualBoardForm.FindAll<ZGrid>().Single();
					grid.SelectAllElements();

					var contextMenuItem = grid.ContextMenu.MenuItems.FindByText(menuItem, true);
					contextMenuItem.PerformClick();
				}
			}
			finally
			{
				var form = Application.OpenForms.OfType<T>().Single();
				AssertNotNull(form);
				form.Close();
			}
		}
	}
}
