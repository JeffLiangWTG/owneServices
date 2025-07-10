using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	public class TransactionLineTypeDecider_InnerTest : TestCaseWithFactory
	{
		public void TestAccountReceivableType()
		{
			TransactionLinesCollection transCollection = new TransactionLinesCollection(Factory);
			transCollection.Load();

			int countBefore = transCollection.Count;

			ARInvoiceLine trans1 = Factory.New<ARInvoiceLine>();
			trans1.AL_AH = Factory.New<ARInvoice>().PK;
			ARCreditNoteLine trans2 = Factory.New<ARCreditNoteLine>();
			trans2.AL_AH = Factory.New<ARCreditNote>().PK;
			ARAdjustmentNoteLine trans3 = Factory.New<ARAdjustmentNoteLine>();
			trans3.AL_AH = Factory.New<ARAdjustmentNote>().PK;

			transCollection.Load();
			AssertEquals(3, transCollection.Count - countBefore);
		}

		public void TestAccountPayableType()
		{
			TransactionLinesCollection transCollection = new TransactionLinesCollection(Factory);
			transCollection.Load();

			int countBefore = transCollection.Count;

			APInvoiceLine trans1 = Factory.New<APInvoiceLine>();
			trans1.AL_AH = Factory.New<APInvoice>().PK;
			APCreditNoteLine trans2 = Factory.New<APCreditNoteLine>();
			trans2.AL_AH = Factory.New<APCreditNote>().PK;
			APAdjustmentNoteLine trans3 = Factory.New<APAdjustmentNoteLine>();
			trans3.AL_AH = Factory.New<APAdjustmentNote>().PK;

			transCollection.Load();
			AssertEquals(3, transCollection.Count - countBefore);
		}

		public void TestUnapprovedPayableTransactionsType()
		{
			TransactionLinesCollection transCollection = new TransactionLinesCollection(Factory);
			transCollection.Load();

			int countBefore = transCollection.Count;

			UAInvoiceLine trans1 = Factory.New<UAInvoiceLine>();
			trans1.AL_AH = Factory.New<UAInvoice>().PK;
			UACreditNoteLine trans2 = Factory.New<UACreditNoteLine>();
			trans2.AL_AH = Factory.New<UACreditNote>().PK;

			transCollection.Load();
			AssertEquals(2, transCollection.Count - countBefore);
		}

		public void TestIncompleteInvoicesAndCreditNotes()
		{
			TransactionLinesCollection transCollection = new TransactionLinesCollection(Factory);
			transCollection.Load();

			int countBefore = transCollection.Count;

			var trans1 = Factory.New<APInvoiceLine>();
			var header1 = Factory.New<APInvoice>();
			header1.AH_Ledger = LedgerTypes.IncompleteTransactions;
			header1.AH_TransactionType = TransactionTypes.IncompleteInvoice;
			trans1.AL_AH = header1.PK;

			var trans2 = Factory.New<APCreditNoteLine>();
			var header2 = Factory.New<APCreditNote>();
			header2.AH_Ledger = LedgerTypes.IncompleteTransactions;
			header2.AH_TransactionType = TransactionTypes.IncompleteCreditNote;
			trans2.AL_AH = header2.PK;

			var trans3 = Factory.New<APAdjustmentNoteLine>();
			var header3 = Factory.New<APAdjustmentNote>();
			header3.AH_Ledger = LedgerTypes.IncompleteTransactions;
			header3.AH_TransactionType = TransactionTypes.IncompleteAdjustmentNote;
			trans3.AL_AH = header3.PK;

			transCollection.Load();
			AssertEquals(3, transCollection.Count - countBefore);
		}

		public void TestJCJNLREVWorks()
		{
			TransactionLinesCollection transCollection = new TransactionLinesCollection(Factory);
			JCJournalHeader cFXJournal = Factory.New<JCJournalHeader>();
			JCJournalLine line = Factory.New<JCJournalLine>();
			line.AL_LineType = TransactionLineTypes.Revenue;
			line.AL_AH = cFXJournal.PK;
			line.AL_AG = new TestObjectCreator(Factory).GLHeader1.PK;
			Factory.Save();
			transCollection.Load();
			AssertEquals("Transaction Type decider should not raise and exception", transCollection.Count, 1);
		}

		public void TestJobRevenueJournalLines_REV()
		{
			TransactionLinesCollection transCollection = new TransactionLinesCollection(Factory);
			JobRevenueJournal journal = Factory.New<JobRevenueJournal>();
			JobRevenueJournalLine line = Factory.New<JobRevenueJournalLine>();
			line.AL_LineType = TransactionLineTypes.Revenue;
			line.AL_AH = journal.PK;
			line.AL_AG = new TestObjectCreator(Factory).GLHeader1.PK;
			Factory.Save();
			transCollection.Load();
			AssertEquals("Transaction Type decider should not raise and exception", transCollection.Count, 1);
		}

		public void TestJobRevenueJournalLines_CST()
		{
			TransactionLinesCollection transCollection = new TransactionLinesCollection(Factory);
			JobRevenueJournal journal = Factory.New<JobRevenueJournal>();
			JobRevenueJournalLine line = Factory.New<JobRevenueJournalLine>();
			line.AL_LineType = TransactionLineTypes.Cost;
			line.AL_AH = journal.PK;
			line.AL_AG = new TestObjectCreator(Factory).GLHeader1.PK;
			Factory.Save();
			transCollection.Load();
			AssertEquals("Transaction Type decider should not raise and exception", transCollection.Count, 1);
		}

		public void TestWIPAccrual()
		{
			TransactionLinesCollection transCollection = new TransactionLinesCollection(Factory);
			transCollection.Load();

			int countBefore = transCollection.Count;

			WIP trans1 = Factory.New<WIP>();
			Accrual trans2 = Factory.New<Accrual>();

			transCollection.Load();
			AssertEquals(2, transCollection.Count - countBefore);
		}

		#region TestNotSupportedTransactionLines

		public void TestNotSupportedTransactionLines()
		{
			AssertExceptionsForNotSupportedTransactionLines(typeof(DirectPaymentLine), null, false, false, false, false);
			AssertExceptionsForNotSupportedTransactionLines(typeof(DirectReceiptLine), null, false, false, false, false);

			AssertExceptionsForNotSupportedTransactionLines(typeof(APInvoiceLine), null, true, false, false, false);
			AssertExceptionsForNotSupportedTransactionLines(typeof(APInvoiceLine), typeof(ARInvoice), true, true, false, false);
			AssertExceptionsForNotSupportedTransactionLines(typeof(APInvoiceLine), typeof(APJournal), true, true, true, false);
			AssertExceptionsForNotSupportedTransactionLines(typeof(APInvoiceLine), typeof(APInvoice), true, true, true, true);

			AssertExceptionsForNotSupportedTransactionLines(typeof(ARInvoiceLine), null, true, false, false, false);
			AssertExceptionsForNotSupportedTransactionLines(typeof(ARInvoiceLine), typeof(APInvoice), true, true, false, false);
			AssertExceptionsForNotSupportedTransactionLines(typeof(ARInvoiceLine), typeof(ARPayment), true, true, true, false);
			AssertExceptionsForNotSupportedTransactionLines(typeof(ARInvoiceLine), typeof(ARInvoice), true, true, true, true);

			AssertExceptionsForNotSupportedTransactionLines(typeof(JCJournalLine), typeof(JCJournalHeader), true, true, true, true);

			AssertExceptionsForNotSupportedTransactionLines(typeof(UAInvoiceLine), null, true, false, false, false);
			AssertExceptionsForNotSupportedTransactionLines(typeof(UAInvoiceLine), typeof(APInvoice), true, true, false, false);
			AssertExceptionsForNotSupportedTransactionLines(typeof(UAInvoiceLine), typeof(UAInvoice), true, true, true, true);
		}

		public void TestLineWithDeletedTransaction()
		{
			AssertExceptionsForTransactionLinesWithDeletedTransaction(typeof(APInvoiceLine), typeof(APInvoice), false);
			AssertExceptionsForTransactionLinesWithDeletedTransaction(typeof(APInvoiceLine), typeof(APInvoice), true);

			AssertExceptionsForTransactionLinesWithDeletedTransaction(typeof(ARInvoiceLine), typeof(ARInvoice), false);
			AssertExceptionsForTransactionLinesWithDeletedTransaction(typeof(ARInvoiceLine), typeof(ARInvoice), true);

			AssertExceptionsForTransactionLinesWithDeletedTransaction(typeof(JCJournalLine), typeof(JCJournalHeader), false);
			AssertExceptionsForTransactionLinesWithDeletedTransaction(typeof(JCJournalLine), typeof(JCJournalHeader), true);

			AssertExceptionsForTransactionLinesWithDeletedTransaction(typeof(UAInvoiceLine), typeof(UAInvoice), false);
			AssertExceptionsForTransactionLinesWithDeletedTransaction(typeof(UAInvoiceLine), typeof(UAInvoice), true);
		}

		void AssertExceptionsForNotSupportedTransactionLines(Type lineObjectType, Type transactionObjectType,
			bool isLineTypeValid, bool isTransactionPKValid, bool isTransactionLedgerValid, bool isTransactionTypeValid)
		{
			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			TransactionLinesCollection linesCollection = new TransactionLinesCollection(testFactory);
			TransactionLine testLine = (TransactionLine)testFactory.New(lineObjectType);
			TransactionHeader testTransaction = null;
			if (transactionObjectType != null)
			{
				testTransaction = (TransactionHeader)testFactory.New(transactionObjectType);
				testLine.AL_AH = testTransaction.PK;
			}
			try
			{
				linesCollection.Load();
			}
			catch (ArgumentException ex)
			{
				string expectedMessage = string.Empty;
				if (!isLineTypeValid)
				{
					expectedMessage = string.Format("Transaction Line Type '{0}' is not a valid.", testLine.AL_LineType);
				}
				else if (!isTransactionPKValid)
				{
					expectedMessage = string.Format("Transaction Header with PK '{0}' can't be loaded, Line Type '{1}'.",
						testLine.AL_AH, testLine.AL_LineType);
				}
				else if (!isTransactionLedgerValid)
				{
					expectedMessage = string.Format("Transaction Header Ledger '{0}' is not valid for Line Type '{1}'.",
						testTransaction.AH_Ledger, testLine.AL_LineType);
				}
				else if (!isTransactionTypeValid)
				{
					expectedMessage = string.Format("Transaction Type '{0}' is not valid for Transaction Header Ledger '{1}' and Line Type '{2}'.",
						testTransaction.AH_TransactionType, testTransaction.AH_Ledger, testLine.AL_LineType);
				}
				AssertEquals(expectedMessage, ex.Message);
				AssertEquals(ex, ErrorReporter.LastExceptionReported);
				AssertEquals(string.Format("Problem Line PK '{0}'.", testLine.PK), ErrorReporter.LastMessageReported);
				ExceptionReporterTestListener.Instance.Clear();
				ErrorReporter.Clear();
			}
			// No exception was thrown during Load
			if (isLineTypeValid && isTransactionPKValid && isTransactionLedgerValid && isTransactionTypeValid)
			{
				Assert("Line must be loaded.", linesCollection.Contains(testLine));
			}
			else
			{
				Assert("Line must not be loaded.", !linesCollection.Contains(testLine));
			}
		}

		void AssertExceptionsForTransactionLinesWithDeletedTransaction(Type lineObjectType, Type transactionObjectType,
			bool isTransactionDeleted)
		{
			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			TransactionLinesCollection linesCollection = new TransactionLinesCollection(testFactory);
			TransactionLine testLine = (TransactionLine)testFactory.New(lineObjectType);
			TransactionHeader testTransaction = null;
			if (transactionObjectType != null)
			{
				testTransaction = (TransactionHeader)testFactory.New(transactionObjectType);
				testLine.AL_AH = testTransaction.PK;
				if (isTransactionDeleted)
				{
					testTransaction.Delete();
				}
			}
			try
			{
				linesCollection.Load();
			}
			catch (ArgumentException ex)
			{
				string expectedMessage = string.Empty;
				if (isTransactionDeleted)
				{
					expectedMessage = string.Format("Transaction Header with PK '{0}' can't be loaded because it was deleted, Line Type '{1}'.",
						testLine.AL_AH, testLine.AL_LineType);
				}
				AssertEquals(expectedMessage, ex.Message);
				AssertEquals(ex, ErrorReporter.LastExceptionReported);
				AssertEquals(string.Format("Problem Line PK '{0}'.", testLine.PK), ErrorReporter.LastMessageReported);
				ExceptionReporterTestListener.Instance.Clear();
				ErrorReporter.Clear();
			}
			// No exception was thrown during Load
			if (!isTransactionDeleted)
			{
				Assert("Line must be loaded.", linesCollection.Contains(testLine));
			}
			else
			{
				Assert("Line must not be loaded.", !linesCollection.Contains(testLine));
			}
		}

		#endregion
	}
}