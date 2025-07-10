using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.Business.CashBook.ExchangeDifference;
using Enterprise.Accounting.Business.CashBook.OpeningPayment;
using Enterprise.Accounting.Business.CashBook.OpeningReceipt;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.Testing
{
	public class ARAPControllerCreatorTestCase : TestCaseWithFactory
	{
		public void TestGetNewControllerForMiscTransactions()
		{
			APDiscount testAPDisc = Factory.NewWithValidTestData<APDiscount>();
			ZController controller = AccountingControllerCreator.GetNewController(testAPDisc);
			AssertEquals("Controller should be for APDiscount", ControllerIDs.APDiscount, controller.ID);

			AROverpayment testAROVP = Factory.NewWithValidTestData<AROverpayment>();
			controller = AccountingControllerCreator.GetNewController(testAROVP);
			AssertEquals("Controller should be for AROverpayment", ControllerIDs.AROverpayment, controller.ID);

			ARExchangeDifference testARExx = Factory.NewWithValidTestData<ARExchangeDifference>();
			controller = AccountingControllerCreator.GetNewController(testARExx);
			AssertEquals("Controller should be for ARExchangeDiff", ControllerIDs.ARExchangeDifference, controller.ID);

			ARJournal testARJNL = Factory.NewWithValidTestData<ARJournal>();
			controller = AccountingControllerCreator.GetNewController(testARJNL);
			AssertEquals("Controller should be for ARJournal", ControllerIDs.ARJournal, controller.ID);
			controller = AccountingControllerCreator.GetNewController(testARJNL, ModuleIDs.ZARMatching);
			AssertEquals("Controller should be for ARBankFeeJournal", ControllerIDs.ARBankFeeJournal, controller.ID);

			APJournal testAPJNL = Factory.NewWithValidTestData<APJournal>();
			controller = AccountingControllerCreator.GetNewController(testAPJNL);
			AssertEquals("Controller should be for APJournal", ControllerIDs.APJournal, controller.ID);
			controller = AccountingControllerCreator.GetNewController(testAPJNL, ModuleIDs.ZAPMatching);
			AssertEquals("Controller should be for APBankFeeJournal", ControllerIDs.APBankFeeJournal, controller.ID);
		}

		public void TestGetNewControllerForCashbookTransactions()
		{
			DirectPayment cBDpy = Factory.NewWithValidTestData<DirectPayment>();
			ZController controller = AccountingControllerCreator.GetNewController(cBDpy);
			AssertEquals("Should be DirectPayment", ControllerIDs.DirectPayment, controller.ID);

			DirectReceipt cBDrc = Factory.NewWithValidTestData<DirectReceipt>();
			controller = AccountingControllerCreator.GetNewController(cBDrc);
			AssertEquals("Should be DirectReceipt", ControllerIDs.DirectReceipt, controller.ID);

			OpeningPayment cBOpy = Factory.NewWithValidTestData<OpeningPayment>();
			controller = AccountingControllerCreator.GetNewController(cBOpy);
			AssertEquals("Should be OpeningPayment", ControllerIDs.OpeningPayment, controller.ID);

			OpeningReceipt cBOrc = Factory.NewWithValidTestData<OpeningReceipt>();
			controller = AccountingControllerCreator.GetNewController(cBOrc);
			AssertEquals("Should be OpeningReceipt", ControllerIDs.OpeningReceipt, controller.ID);

			BankTransferFromRow cBTrf = Factory.NewWithValidTestData<BankTransferFromRow>();
			controller = AccountingControllerCreator.GetNewController(cBTrf);
			AssertEquals("Should be BankTransfer", ControllerIDs.BankTransfer, controller.ID);

			CashbookExchangeDiff cBExx = Factory.NewWithValidTestData<CashbookExchangeDiff>();
			controller = AccountingControllerCreator.GetNewController(cBExx);
			AssertEquals("Should be BankCurrencyAdjustment", ControllerIDs.BankCurrencyAdjustment, controller.ID);
		}

		public void TestGetNewControllerForUnapprovedPayableTransactions()
		{
			UAInvoice uAI = Factory.NewWithValidTestData<UAInvoice>();
			ZController controller = AccountingControllerCreator.GetNewController(uAI);
			AssertEquals("Should be UAInvoice", ControllerIDs.UAInvoice, controller.ID);

			UACreditNote uAC = Factory.NewWithValidTestData<UACreditNote>();
			controller = AccountingControllerCreator.GetNewController(uAC);
			AssertEquals("Should be UACreditNote", ControllerIDs.UACreditNote, controller.ID);
		}

		public void TestGetNewControllerForTransactionPendingAllocation()
		{
			var transactionPendingAllocation = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			var controller = AccountingControllerCreator.GetNewController(transactionPendingAllocation);
			AssertEquals("Should be TransactionsPendingAllocation", ControllerIDs.TransactionsPendingAllocation, controller.ID);

			controller = AccountingControllerCreator.GetNewController(transactionPendingAllocation, ModuleIDs.TransactionsPendingAllocationApproval);
			AssertEquals("Should be TransactionsPendingAllocation", ControllerIDs.TransactionsPendingAllocation, controller.ID);
		}

		public void TestGetNewControllerForJobCostingTransactions()
		{
			var journal = (AccTransactionHeader)Factory.NewWithValidTestData<JCJournalHeader>();
			ZController controller = AccountingControllerCreator.GetNewController(journal);
			AssertEquals("Should be JCCostingJournal", ControllerIDs.JCCostingJournal, controller.ID);

			journal = Factory.NewWithValidTestData<JobRevenueJournal>();
			controller = AccountingControllerCreator.GetNewController(journal);
			AssertEquals("Should be JobRevenueJournal", ControllerIDs.JobRevenueJournal, controller.ID);
		}

		public void TestGetNewControllerForIncompleteTransaction()
		{
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.SaveAsIncomplete();
			ZController controller = AccountingControllerCreator.GetNewController(invoice);
			AssertEquals(ControllerIDs.APIncompleteInvoice, controller.ID);

			var creditNote = Factory.NewWithValidTestData<APCreditNote>();
			creditNote.SaveAsIncomplete();
			controller = AccountingControllerCreator.GetNewController(creditNote);
			AssertEquals(ControllerIDs.APIncompleteCreditNote, controller.ID);
		}

		public void TestGetNewControllerForARInvoice()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var controller = AccountingControllerCreator.GetNewController(invoice, ModuleIDs.UnapprovedIntercompanyTransaction);
			AssertEquals(ControllerIDs.ARInvoiceForInterCompanyTransaction, controller.ID);

			controller = AccountingControllerCreator.GetNewController(invoice, ModuleIDs.UnapprovedTransaction);
			AssertEquals(ControllerIDs.ARInvoice, controller.ID);
		}

		public void TestGetNewControllerForARCreditNote()
		{
			var creditNote = Factory.NewWithValidTestData<ARCreditNote>();
			var controller = AccountingControllerCreator.GetNewController(creditNote, ModuleIDs.UnapprovedIntercompanyTransaction);
			AssertEquals(ControllerIDs.ARCreditNoteForInterCompanyTransaction, controller.ID);

			controller = AccountingControllerCreator.GetNewController(creditNote, ModuleIDs.UnapprovedTransaction);
			AssertEquals(ControllerIDs.ARCreditNote, controller.ID);
		}

		public void TestGetNewControllerForApprovaRequestModule()
		{
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			ZController controller = AccountingControllerCreator.GetNewController(invoice, ModuleIDs.APInvoiceApproval);
			AssertEquals(ControllerIDs.APInvoiceNewForApproval, controller.ID);

			invoice.SaveAsIncomplete();
			controller = AccountingControllerCreator.GetNewController(invoice, ModuleIDs.APInvoiceApproval);
			AssertEquals(ControllerIDs.APInvoiceLinkedToApproval, controller.ID);

			var creditNote = Factory.NewWithValidTestData<APCreditNote>();
			controller = AccountingControllerCreator.GetNewController(creditNote, ModuleIDs.APInvoiceApproval);
			AssertEquals(ControllerIDs.APCreditNoteNewForApproval, controller.ID);

			creditNote.SaveAsIncomplete();
			controller = AccountingControllerCreator.GetNewController(creditNote, ModuleIDs.APInvoiceApproval);
			AssertEquals(ControllerIDs.APCreditNoteLinkedToApproval, controller.ID);
		}
	}
}
