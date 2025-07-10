using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.Business.CashBook.ExchangeDifference;
using Enterprise.Accounting.Business.CashBook.OpeningPayment;
using Enterprise.Accounting.Business.CashBook.OpeningReceipt;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	[TestedType(typeof(CashbookTransactionCollection))]
	public class Test : TransactionHeaderCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CashbookTransactionCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<DirectPayment>();
		}

		public void TestCashbookTransactionFiltering()
		{
			DirectPayment cBDpy = Factory.NewWithValidTestData<DirectPayment>();
			DirectReceipt cBDrc = Factory.NewWithValidTestData<DirectReceipt>();
			OpeningPayment cBOpy = Factory.NewWithValidTestData<OpeningPayment>();
			OpeningReceipt cBOrc = Factory.NewWithValidTestData<OpeningReceipt>();
			BankTransferFromRow cBTrf = Factory.NewWithValidTestData<BankTransferFromRow>();
			CashbookExchangeDiff cBExx = Factory.NewWithValidTestData<CashbookExchangeDiff>();

			// Direct Debit Header
			AccTransactionHeader dDRBatchHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			dDRBatchHeader.AH_TransactionType = ZArchitecture.Core.TransactionTypes.DDRBatch;
			dDRBatchHeader.AH_Ledger = LedgerTypes.CashBook;
			dDRBatchHeader.AH_GB = GlbBranch.CurrentBranch.PK;

			// Deposit Batch Header
			DepositBatch receiptBatch = Factory.NewWithValidTestData<DepositBatch>();

			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			APPayment aPPay = Factory.NewWithValidTestData<APPayment>();
			ARReceipt aRRec = Factory.NewWithValidTestData<ARReceipt>();
			APJournal aPJnl = Factory.NewWithValidTestData<APJournal>();
			APCreditNote aPCrd = Factory.NewWithValidTestData<APCreditNote>();
			ARExchangeDifference aRExx = Factory.NewWithValidTestData<ARExchangeDifference>();

			CashbookTransactionCollection cashbookTransactions = new CashbookTransactionCollection(Factory);
			cashbookTransactions.Load();

			AssertEquals("There should be 8 elements in the collection", 8, cashbookTransactions.Count);
			Assert("Should contain DirectPayment", cashbookTransactions.Contains(cBDpy));
			Assert("Should contain DirectReceipt", cashbookTransactions.Contains(cBDrc));
			Assert("Should contain OpeningPayment", cashbookTransactions.Contains(cBOpy));
			Assert("Should contain OpeningReceipt", cashbookTransactions.Contains(cBOrc));
			Assert("Should contain BankCurrencyAdjustment", cashbookTransactions.Contains(cBExx));
			Assert("Should contain BankTransfer", cashbookTransactions.Contains(cBTrf));
			Assert("Should contain APPayment", cashbookTransactions.Contains(aPPay));
			Assert("Should contain ARReceipt", cashbookTransactions.Contains(aRRec));
		}
	}
}
