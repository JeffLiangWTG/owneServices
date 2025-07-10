using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Interfaces.Testing;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;

namespace Enterprise.Accounting.Business.Base.Reversing.Testing
{
	class ReversingFactory_InnerTest : TestCaseWithFactory
	{
		public void TestNewReversing()
		{
			ReversingBase returnedReversing = ReversingFactory.NewReversing(PayablesAndReceivablesTransaction);
			AssertEquals("Payables And Receivables Transaction should return payables and receivables reversing object",
				typeof(PayablesAndReceivablesReversing), returnedReversing.GetType());

			returnedReversing = ReversingFactory.NewReversing(CashBookTransaction);
			AssertEquals("Cash Book Transaction should return payables and receivables reversing object",
				typeof(CashBookReversing), returnedReversing.GetType());

			returnedReversing = ReversingFactory.NewReversing(JobCostingTransaction);
			AssertEquals("Job Costing Transaction should return payables and receivables reversing object",
				typeof(JobCostingReversing), returnedReversing.GetType());

			returnedReversing = ReversingFactory.NewReversing(PaymentTransaction);
			AssertEquals("Payment Transaction should return payment reversing object",
				typeof(PaymentReversing), returnedReversing.GetType());

			returnedReversing = ReversingFactory.NewReversing(ReceiptTransaction);
			AssertEquals("Receipt Transaction should return receipt reversing object",
				typeof(ReceiptReversing), returnedReversing.GetType());

			returnedReversing = ReversingFactory.NewReversing((InvoicingBase)Factory.New(typeof(ARAdjustmentNote)));
			AssertEquals("Invoicing Base transaction should return InvoicingBase reversing object",
				typeof(InvoicingBaseReversing), returnedReversing.GetType());

			returnedReversing = ReversingFactory.NewReversing(Factory.New<APInvoice>());
			AssertEquals("APInvoice transaction should return APInvoiceReversing object",
				typeof(APInvoiceReversing), returnedReversing.GetType());

			returnedReversing = ReversingFactory.NewReversing(Contra.New(Factory));
			AssertEquals("Contra transaction should return ContraReversing object",
				typeof(ContraReversing), returnedReversing.GetType());

			returnedReversing = ReversingFactory.NewReversing(Factory.New<ARInvoice>());
			AssertEquals("ARInvoice transaction should return ARInvoiceReversing object", typeof(ARInvoiceReversing), returnedReversing.GetType());

			returnedReversing = ReversingFactory.NewReversing((IGeneralLedger)Factory.New(typeof(GLJournal)));
			AssertEquals("GLJournal transaction should return GLJournalReversing object",
				typeof(GLJournalReversing), returnedReversing.GetType());

			returnedReversing = ReversingFactory.NewReversing(Factory.New<InvoiceBatchHeader>());
			AssertEquals("Invoice Batch Header transaction should return InvoiceBatchReversing object",
				typeof(InvoiceBatchReversing), returnedReversing.GetType());

			returnedReversing = ReversingFactory.NewReversing(Factory.New<CashBook.DirectReceipt.DirectReceipt>());
			AssertEquals("Invoice Batch Header transaction should return DirectReceiptReversing object",
					typeof(DirectReceiptReversing), returnedReversing.GetType());

			DepositBatchParent testBatch = new DepositBatchParent(Factory);
			returnedReversing = ReversingFactory.NewReversing(testBatch);
			AssertEquals("Deposit Batch should return DepositBatchReversing object",
				typeof(DepositBatchReversing), returnedReversing.GetType());

			returnedReversing = ReversingFactory.NewReversing(Factory.New<APDiscount>());
			AssertEquals("APDiscount should return MatchingOnlyTransactionReversing object",
				typeof(MiscellaneousTransactionReversing), returnedReversing.GetType());

			returnedReversing = ReversingFactory.NewReversing(Factory.New<ARDiscount>());
			AssertEquals("ARDiscount should return MatchingOnlyTransactionReversing object",
				typeof(MiscellaneousTransactionReversing), returnedReversing.GetType());

			returnedReversing = ReversingFactory.NewReversing(Factory.New<APOverpayment>());
			AssertEquals("APOverpayment should return MatchingOnlyTransactionReversing object",
				typeof(MiscellaneousTransactionReversing), returnedReversing.GetType());

			returnedReversing = ReversingFactory.NewReversing(Factory.New<AROverpayment>());
			AssertEquals("AROverpayment should return MatchingOnlyTransactionReversing object",
				typeof(MiscellaneousTransactionReversing), returnedReversing.GetType());

			returnedReversing = ReversingFactory.NewReversing(Factory.New<APExchangeDifference>());
			AssertEquals("APExchangeDifference should return MatchingOnlyTransactionReversing object",
				typeof(MiscellaneousTransactionReversing), returnedReversing.GetType());

			returnedReversing = ReversingFactory.NewReversing(Factory.New<ARExchangeDifference>());
			AssertEquals("ARExchangeDifference should return MatchingOnlyTransactionReversing object",
				typeof(MiscellaneousTransactionReversing), returnedReversing.GetType());

			returnedReversing = ReversingFactory.NewReversing(Factory.New<ARJournal>());
			AssertEquals("ARJournal should still return PayablesAndReceivablesReversing object",
				typeof(PayablesAndReceivablesReversing), returnedReversing.GetType());

			returnedReversing = ReversingFactory.NewReversing(Factory.New<APJournal>());
			AssertEquals("APJournal should still return PayablesAndReceivablesReversing object",
				typeof(PayablesAndReceivablesReversing), returnedReversing.GetType());

			BankTransfer testBankTransfer = new BankTransfer(Factory, null);
			testBankTransfer.BankTransferFromPK = TestObjectCreator.AUDBankAccount.PK;
			testBankTransfer.BankTransferToPK = TestObjectCreator.USDBankAccount.PK;
			returnedReversing = ReversingFactory.NewReversing(testBankTransfer);
			AssertEquals("APJournal should still return PayablesAndReceivablesReversing object",
				typeof(CashBookReversing), returnedReversing.GetType());

			returnedReversing = ReversingFactory.NewReversing(Factory.New<DirectDebitBatchHeader>());
			AssertEquals("APJournal should still return PayablesAndReceivablesReversing object",
				typeof(CashBookReversing), returnedReversing.GetType());
		}

		public void TestNullGivesNullResultOnNew()
		{
			ReversingBase returnedReversing = ReversingFactory.NewReversing(null);
			AssertNull("Passing null to new should return null", returnedReversing);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			PaymentTransaction = new TestIPayment();
			PayablesAndReceivablesTransaction = new TestIPayablesAndReceivables();
			CashBookTransaction = new TestICashBookTransaction();
			JobCostingTransaction = new TestIJobCostingTransaction();
			ReceiptTransaction = Factory.NewWithValidTestData<ARReceipt>();
			ReversingFactory = new ReversingFactory();
		}

		Receipt ReceiptTransaction;
		TestIPayment PaymentTransaction;
		TestIPayablesAndReceivables PayablesAndReceivablesTransaction;
		TestICashBookTransaction CashBookTransaction;
		TestIJobCostingTransaction JobCostingTransaction;
		ReversingFactory ReversingFactory;

		TestObjectCreator TestObjectCreator
		{
			get { return TestObjectCreator_innerValue ?? (TestObjectCreator_innerValue = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator TestObjectCreator_innerValue;

		#endregion
	}
}