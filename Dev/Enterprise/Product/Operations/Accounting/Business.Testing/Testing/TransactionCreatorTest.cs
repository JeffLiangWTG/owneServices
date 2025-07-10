using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.Business.CashBook.OpeningPayment;
using Enterprise.Accounting.Business.CashBook.OpeningReceipt;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	class TransactionCreatorTest : TestCaseWithFactory
	{
		[TestDate(2009, 5, 20)]
		public void TestCreateAPTransactions()
		{
			// Most methods in TestObjectCreator do NOT have load-before-create protection.
			// Create each transaction twice to make sure NO duplicate bizO were created by different TestObjectCreator.
			AssertTransactionCore(typeof(APInvoice), LedgerTypes.AccountsPayable, TransactionTypes.Invoice);
			AssertTransactionCore(typeof(APInvoice), LedgerTypes.AccountsPayable, TransactionTypes.Invoice);

			AssertTransactionCore(typeof(APAdjustmentNote), LedgerTypes.AccountsPayable, TransactionTypes.AdjustmentNote);
			AssertTransactionCore(typeof(APAdjustmentNote), LedgerTypes.AccountsPayable, TransactionTypes.AdjustmentNote);

			AssertTransactionCore(typeof(APCreditNote), LedgerTypes.AccountsPayable, TransactionTypes.CreditNote);
			AssertTransactionCore(typeof(APCreditNote), LedgerTypes.AccountsPayable, TransactionTypes.CreditNote);
		}

		[TestDate(2009, 5, 20)]
		public void TestCreateARTransactions()
		{
			AssertTransactionCore(typeof(ARInvoice), LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);
			AssertTransactionCore(typeof(ARInvoice), LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);

			AssertTransactionCore(typeof(ARAdjustmentNote), LedgerTypes.AccountsReceivable, TransactionTypes.AdjustmentNote);
			AssertTransactionCore(typeof(ARAdjustmentNote), LedgerTypes.AccountsReceivable, TransactionTypes.AdjustmentNote);

			AssertTransactionCore(typeof(ARCreditNote), LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote);
			AssertTransactionCore(typeof(ARCreditNote), LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote);
		}

		[TestDate(2009, 5, 20)]
		public void TestCreateGLTransactions()
		{
			AssertTransactionCore(typeof(GLJournal), LedgerTypes.General, TransactionTypes.GLAutoJournal);
			AssertTransactionCore(typeof(GLJournal), LedgerTypes.General, TransactionTypes.GLAutoJournal);

			AssertTransactionCore(typeof(GLJournal), LedgerTypes.General, TransactionTypes.GLReversingJournal);
			AssertTransactionCore(typeof(GLJournal), LedgerTypes.General, TransactionTypes.GLReversingJournal);

			AssertTransactionCore(typeof(GLJournal), LedgerTypes.General, TransactionTypes.GLStandardJournal);
			AssertTransactionCore(typeof(GLJournal), LedgerTypes.General, TransactionTypes.GLStandardJournal);

			AssertTransactionCore(typeof(GLJournal), LedgerTypes.General, TransactionTypes.GLNoteJournal);
			AssertTransactionCore(typeof(GLJournal), LedgerTypes.General, TransactionTypes.GLNoteJournal);
		}

		[TestDate(2009, 5, 20)]
		public void TestCreateCBTransactions()
		{
			AssertTransactionCore(typeof(DirectPayment), LedgerTypes.CashBook, TransactionTypes.DirectPayment);
			AssertTransactionCore(typeof(DirectPayment), LedgerTypes.CashBook, TransactionTypes.DirectPayment);

			AssertTransactionCore(typeof(DirectReceipt), LedgerTypes.CashBook, TransactionTypes.DirectReceipt);
			AssertTransactionCore(typeof(DirectReceipt), LedgerTypes.CashBook, TransactionTypes.DirectReceipt);

			AssertTransactionCore(typeof(OpeningPayment), LedgerTypes.CashBook, TransactionTypes.OpeningPayment);
			AssertTransactionCore(typeof(OpeningPayment), LedgerTypes.CashBook, TransactionTypes.OpeningPayment);

			AssertTransactionCore(typeof(OpeningReceipt), LedgerTypes.CashBook, TransactionTypes.OpeningReceipt);
			AssertTransactionCore(typeof(OpeningReceipt), LedgerTypes.CashBook, TransactionTypes.OpeningReceipt);
		}

		[TestDate(2009, 5, 20)]
		public void TestCreateJCTransactions()
		{
			AssertTransactionCore(typeof(JCJournalHeader), LedgerTypes.JobCosting, TransactionTypes.Journal);
			AssertTransactionCore(typeof(JCJournalHeader), LedgerTypes.JobCosting, TransactionTypes.Journal);

			AssertTransactionCore(typeof(JobRevenueJournal), LedgerTypes.JobCosting, TransactionTypes.JobRevenueJournal);
			AssertTransactionCore(typeof(JobRevenueJournal), LedgerTypes.JobCosting, TransactionTypes.JobRevenueJournal);
		}

		[TestDate(2009, 5, 20)]
		public void TestCreateUATransactions()
		{
			AssertTransactionCore(typeof(UAInvoice), LedgerTypes.UnapprovedPayableTransactions, TransactionTypes.UAInvoice);
			AssertTransactionCore(typeof(UAInvoice), LedgerTypes.UnapprovedPayableTransactions, TransactionTypes.UAInvoice);

			AssertTransactionCore(typeof(UACreditNote), LedgerTypes.UnapprovedPayableTransactions, TransactionTypes.UACreditNote);
			AssertTransactionCore(typeof(UACreditNote), LedgerTypes.UnapprovedPayableTransactions, TransactionTypes.UACreditNote);
		}

		protected override void SetUp()
		{
			base.SetUp();

			new TestObjectCreator(Factory).CreateTestPeriodsForEntireYear(2009);
			Factory.Save();

			TransactionCreator = new TransactionCreator();
		}

		#region Implementation

		void AssertTransactionCore(Type type, string ledger, string transactionType)
		{
			var transaction = TransactionCreator.CreateTransaction(Factory, ledger, transactionType);
			AssertEquals(type, transaction.GetType());
			transaction.RunPreSaveValidation();
			AssertNoErrors(transaction);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		TransactionCreator TransactionCreator;

		#endregion
	}
}
