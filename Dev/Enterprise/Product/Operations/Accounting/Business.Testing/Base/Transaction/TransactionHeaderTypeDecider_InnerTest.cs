using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.Business.CashBook.ExchangeDifference;
using Enterprise.Accounting.Business.CashBook.OpeningPayment;
using Enterprise.Accounting.Business.CashBook.OpeningReceipt;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using TransactionTypes = Enterprise.ZArchitecture.Core.TransactionTypes;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	public class TransactionHeaderTypeDecider_InnerTest : TestCaseWithFactory
	{
		public void TestCashbookType()
		{
			TransactionHeaderCollection transCollection = new TransactionHeaderCollection(Factory);
			transCollection.Load();

			int countBefore = transCollection.Count;

			DepositBatch trans1 = Factory.New<DepositBatch>();
			DirectPayment trans2 = Factory.New<DirectPayment>();
			DirectReceipt trans3 = Factory.New<DirectReceipt>();
			OpeningReceipt trans4 = Factory.New<OpeningReceipt>();
			OpeningPayment trans5 = Factory.New<OpeningPayment>();
			BankTransferFromRow trans6 = Factory.New<BankTransferFromRow>();
			BankTransferFromRow trans6_reversed = Factory.New<BankTransferFromRow>();
			trans6_reversed.AH_TransactionCount = 4;
			CashbookExchangeDiff trans7 = Factory.New<CashbookExchangeDiff>();
			DirectDebitBatchHeader trans8 = Factory.New<DirectDebitBatchHeader>();
			BankTransferCharge trans9 = Factory.New<BankTransferCharge>();
			BankTransferCharge trans9_reversed = Factory.New<BankTransferCharge>();
			trans9_reversed.AH_TransactionCount = 6;
			BankTransferToRow trans10 = Factory.New<BankTransferToRow>();
			BankTransferToRow trans10_reversed = Factory.New<BankTransferToRow>();
			trans10_reversed.AH_TransactionCount = 5;

			transCollection.Load();
			AssertEquals(13, transCollection.Count - countBefore);
			AssertType(typeof(BankTransferCharge), transCollection.FindByPK(trans9_reversed.PK));
		}

		public void TestCashbookTypeDecisionIntegration()
		{
			var transfer = new BankTransfer(Factory, null);
			transfer.BankTransferFromPK = TestObjectCreator.AUDBankAccount.PK;
			transfer.BankTransferToPK = TestObjectCreator.AUDBankAccount2.PK;
			Factory.Save();

			var decider = new TransactionHeaderTypeDecider();
			AssertEquals(typeof(BankTransferFromRow), decider.GetTypeForLoad(((INeedRow)transfer.TransferRowFrom).Row, Factory));
			AssertEquals(typeof(BankTransferToRow), decider.GetTypeForLoad(((INeedRow)transfer.TransferRowTo).Row, Factory));
			((IReversing)transfer).GenerateReverseTransaction(false);
			var reverseTransfer = (BankTransfer)((IReversing)transfer).ReverseTransaction;
			AssertEquals(typeof(BankTransferFromRow), decider.GetTypeForLoad(((INeedRow)reverseTransfer.TransferRowFrom).Row, Factory));
			AssertEquals(typeof(BankTransferToRow), decider.GetTypeForLoad(((INeedRow)reverseTransfer.TransferRowTo).Row, Factory));
		}

		public void TestAccountReceivableType()
		{
			TransactionHeaderCollection transCollection = new TransactionHeaderCollection(Factory);
			transCollection.Load();

			int countBefore = transCollection.Count;

			ARInvoice trans1 = Factory.New(typeof(ARInvoice)) as ARInvoice;
			ARCreditNote trans2 = Factory.New(typeof(ARCreditNote)) as ARCreditNote;
			ARAdjustmentNote trans3 = Factory.New(typeof(ARAdjustmentNote)) as ARAdjustmentNote;
			ARPayment trans4 = Factory.New(typeof(ARPayment)) as ARPayment;
			ARContraRow trans5 = Factory.New(typeof(ARContraRow)) as ARContraRow;
			ARJournal trans6 = Factory.New(typeof(ARJournal)) as ARJournal;
			ARDiscount trans7 = Factory.New(typeof(ARDiscount)) as ARDiscount;
			ARExchangeDifference trans8 = Factory.New(typeof(ARExchangeDifference)) as ARExchangeDifference;
			AROverpayment trans9 = Factory.New(typeof(AROverpayment)) as AROverpayment;

			transCollection.Load();
			AssertEquals(9, transCollection.Count - countBefore);
		}

		public void TestAccountPayableType()
		{
			TransactionHeaderCollection transCollection = new TransactionHeaderCollection(Factory);
			transCollection.Load();

			int countBefore = transCollection.Count;

			APInvoice trans1 = Factory.New(typeof(APInvoice)) as APInvoice;
			APCreditNote trans2 = Factory.New(typeof(APCreditNote)) as APCreditNote;
			APAdjustmentNote trans3 = Factory.New(typeof(APAdjustmentNote)) as APAdjustmentNote;
			APPayment trans4 = Factory.New(typeof(APPayment)) as APPayment;
			APContraRow trans5 = Factory.New(typeof(APContraRow)) as APContraRow;
			APJournal trans6 = Factory.New(typeof(APJournal)) as APJournal;
			APDiscount trans7 = Factory.New(typeof(APDiscount)) as APDiscount;
			APExchangeDifference trans8 = Factory.New(typeof(APExchangeDifference)) as APExchangeDifference;
			APOverpayment trans9 = Factory.New(typeof(APOverpayment)) as APOverpayment;

			transCollection.Load();
			AssertEquals(9, transCollection.Count - countBefore);
		}

		public void TestUnapprovedPayableTransactionsType()
		{
			TransactionHeaderCollection transCollection = new TransactionHeaderCollection(Factory);
			transCollection.Load();

			int countBefore = transCollection.Count;

			UAInvoice trans1 = Factory.New<UAInvoice>();
			UACreditNote trans2 = Factory.New<UACreditNote>();

			transCollection.Load();
			AssertEquals(2, transCollection.Count - countBefore);
		}

		public void TestJobCostingType()
		{
			TransactionHeaderCollection transCollection = new TransactionHeaderCollection(Factory);
			transCollection.Load();

			int countBefore = transCollection.Count;

			JobRevenueJournal jrJournal = Factory.New<JobRevenueJournal>();
			JCJournalHeader jcJournal = Factory.New<JCJournalHeader>();

			transCollection.Load();
			AssertEquals(2, transCollection.Count - countBefore);
		}

		public void TestUnsupportedTransactionTypes()
		{
			AssertExceptionForUnsupportedTransactionTypes("XX", TransactionTypes.Invoice, 0, false, true, false);
			AssertExceptionForUnsupportedTransactionTypes(LedgerTypes.AccountsPayable, "XXX", 0, false, true);
			AssertExceptionForUnsupportedTransactionTypes(LedgerTypes.IncompleteTransactions, "XXX", 0, false, true);
			AssertExceptionForUnsupportedTransactionTypes(LedgerTypes.AccountsPayable, TransactionTypes.Transfer, 10, true, false);
			AssertExceptionForUnsupportedTransactionTypes(LedgerTypes.IncompleteTransactions, TransactionTypes.Transfer, 10, true, false);
			AssertExceptionForUnsupportedTransactionTypes(LedgerTypes.AccountsReceivable, "XXX", 0, false, true);
			AssertExceptionForUnsupportedTransactionTypes(LedgerTypes.AccountsReceivable, TransactionTypes.Transfer, 10, true, false);
			AssertExceptionForUnsupportedTransactionTypes(LedgerTypes.CashBook, "XXX", 0, false, true);
			AssertExceptionForUnsupportedTransactionTypes(LedgerTypes.CashBook, TransactionTypes.Transfer, 10, true, false);
			AssertExceptionForUnsupportedTransactionTypes(LedgerTypes.JobCosting, "XXX", 0, false, true);
			AssertExceptionForUnsupportedTransactionTypes(LedgerTypes.UnapprovedPayableTransactions, "XXX", 0, false, true);
		}

		void AssertExceptionForUnsupportedTransactionTypes(string transactionLedger, string transactionType, byte transactionCount, bool isTypeValid, bool isCountValid, bool isLedgerValid = true)
		{
			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			var testTransaction = testFactory.New<AccTransactionHeader>();
			testTransaction.AH_Ledger = transactionLedger;
			testTransaction.AH_TransactionType = transactionType;
			testTransaction.AH_TransactionCount = transactionCount;
			try
			{
				testFactory.Load<TransactionHeader>(testTransaction.PK);
			}
			catch (ArgumentException ex)
			{
				string expectedMessage = string.Empty;
				if (!isLedgerValid)
				{
					expectedMessage = "Ledger '" + testTransaction.AH_Ledger + "' is not valid. Ledger must be AR, AP, CB, JC or PA";
				}
				else if (!isTypeValid)
				{
					expectedMessage = "Transaction Type '" + testTransaction.AH_TransactionType + "' is not a valid " + testTransaction.AH_Ledger + " Transaction Type";
				}
				else if (!isCountValid)
				{
					expectedMessage = "Transaction Count " + testTransaction.AH_TransactionCount + " is not a valid " + testTransaction.AH_Ledger + " " + testTransaction.AH_TransactionType + " Type";
				}
				AssertEquals(expectedMessage, ex.Message);
				AssertEquals(ex, ErrorReporter.LastExceptionReported);
				AssertEquals(string.Format("Problem Transaction PK '{0}'.", testTransaction.PK), ErrorReporter.LastMessageReported);
				ExceptionReporterTestListener.Instance.Clear();
				ErrorReporter.Clear();
			}
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}
	}
}