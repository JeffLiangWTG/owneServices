using System.Linq;
using Enterprise.Accounting.Business.Base.AccStatement;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.Accounting.Business.CashBook.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocBankReconciliation))]
	sealed class DocBankReconciliationTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocBankReconciliation.New(BankRecon, Factory)
			};
		}

		BankReconciliation BankRecon;
		protected override void SetUp()
		{
			BankRecon = new BankReconciliation(Factory);
			base.SetUp();
		}

		[SuspendCriticalValidation]
		public void TestPropertiesMatchBankReconciliationSnapshot()
		{
			var bankRecTest = new BankReconciliationTest();
			bankRecTest.ExternalSetup();
			var (unclearedTransaction, clearedTransaction, bankRecon) = bankRecTest.SetupTransactionsForExternalTests();

			var docWrapper = DocBankReconciliation.New(bankRecon, Factory);
			CombineAssertions("DocWrapper should use properties from BankReconciliation", () =>
			{
				AssertEquals(bankRecon.BankAccount.AB_Code, docWrapper.BankAccount.Code);
				AssertEquals(bankRecon.BankAccount.AB_AccountNum, docWrapper.BankAccount.AccountNum);
				AssertEquals(bankRecon.ReconcileDate, docWrapper.ReconcileDate);
				AssertEquals(bankRecon.StatementDate, docWrapper.StatementDate);

				AssertEquals(bankRecon.OpeningSnapshot.StatementBalance, docWrapper.OpeningStatementBalance);
				AssertEquals(bankRecon.OpeningSnapshot.UnclearedCashbookAmount, docWrapper.OpeningUnclearedCashbookAmount);
				AssertEquals(bankRecon.OpeningSnapshot.UnclearedStatementAmount, docWrapper.OpeningUnclearedStatementAmount);
				AssertEquals(bankRecon.OpeningSnapshot.AmendedBankStatementBalance, docWrapper.OpeningAmendedBankStatementBalance);
				AssertEquals(bankRecon.OpeningSnapshot.CashBookBalance, docWrapper.OpeningCashBookBalance);
				AssertEquals(bankRecon.OpeningSnapshot.ReconError, docWrapper.OpeningReconError);

				AssertEquals(bankRecon.ClosingBalanceReadOnly, docWrapper.ClosingStatementBalance);
				AssertEquals(bankRecon.UnclearedCashbookAmount, docWrapper.ClosingUnclearedCashbookAmount);
				AssertEquals(bankRecon.UnclearedStatementAmount, docWrapper.ClosingUnclearedStatementAmount);
				AssertEquals(bankRecon.AmendedBankStatementBalance, docWrapper.ClosingAmendedBankStatementBalance);
				AssertEquals(bankRecon.CashBookBalance, docWrapper.ClosingCashBookBalance);
				AssertEquals(bankRecon.ReconError, docWrapper.ClosingReconError);
			});
		}

		[SuspendCriticalValidation]
		public void TestClearedPropertiesMatchBankReconciliationSnapshot()
		{
			var bankRecTest = new BankReconciliationTest();
			bankRecTest.ExternalSetup();
			var (unclearedTransaction, clearedTransaction, bankRecon) = bankRecTest.SetupTransactionsForExternalTests();

			var unclearedTran = bankRecon.MergedTransactions.FindByPK(unclearedTransaction.PK) as BankReconTransaction;
			unclearedTran.IsCleared = true;
			var clearedTran = bankRecon.MergedTransactions.FindByPK(clearedTransaction.PK) as BankReconTransaction;
			clearedTran.IsCleared = false;

			var docWrapper = DocBankReconciliation.New(bankRecon, Factory);
			CombineAssertions("DocWrapper for Cleared transaction should match originally uncleared transaction properties", () =>
			{
				AssertEquals(bankRecon.TransactionIdsClearedInCurrentSession.Count, docWrapper.TransactionsCleared.Count);

				var docClearedTrans = docWrapper.TransactionsCleared[0];
				AssertEquals(unclearedTran.TransactionDate, docClearedTrans.TransactionDate);
				AssertEquals(unclearedTran.InvoiceDate, docClearedTrans.InvoiceDate);
				AssertEquals(unclearedTran.Type, docClearedTrans.Type);
				AssertEquals(unclearedTran.Method, docClearedTrans.Method);
				AssertEquals(unclearedTran.ChequeRef, docClearedTrans.ChequeRef);
				AssertEquals(unclearedTran.BatchNo, docClearedTrans.BatchNo);
				AssertEquals(unclearedTran.Payee, docClearedTrans.Payee);
				AssertEquals(unclearedTran.Debit, docClearedTrans.Debit);
				AssertEquals(unclearedTran.Credit, docClearedTrans.Credit);
				AssertEquals(unclearedTran.IsCleared, docClearedTrans.IsCleared);
				AssertEquals(unclearedTran.LineType, docClearedTrans.LineType);
				AssertEquals(unclearedTran.ClearedDate, docClearedTrans.ClearedDate);
			});
			CombineAssertions("DocWrapper for Cleared transaction should match originally uncleared transaction properties", () =>
			{
				AssertEquals(bankRecon.TransactionIdsUnclearedInCurrentSession.Count, docWrapper.TransactionsUncleared.Count);

				var docUnclearedTrans = docWrapper.TransactionsUncleared[0];
				AssertEquals(clearedTran.TransactionDate, docUnclearedTrans.TransactionDate);
				AssertEquals(clearedTran.InvoiceDate, docUnclearedTrans.InvoiceDate);
				AssertEquals(clearedTran.Type, docUnclearedTrans.Type);
				AssertEquals(clearedTran.Method, docUnclearedTrans.Method);
				AssertEquals(clearedTran.ChequeRef, docUnclearedTrans.ChequeRef);
				AssertEquals(clearedTran.BatchNo, docUnclearedTrans.BatchNo);
				AssertEquals(clearedTran.Payee, docUnclearedTrans.Payee);
				AssertEquals(clearedTran.Debit, docUnclearedTrans.Debit);
				AssertEquals(clearedTran.Credit, docUnclearedTrans.Credit);
				AssertEquals(clearedTran.IsCleared, docUnclearedTrans.IsCleared);
				AssertEquals(clearedTran.LineType, docUnclearedTrans.LineType);
				AssertEquals(clearedTran.ClearedDate, docUnclearedTrans.ClearedDate);
			});
		}

		[SuspendCriticalValidation]
		public void TestChangePropertiesMatchBankReconciliationSnapshot()
		{
			var bankRecTest = new BankReconciliationTest();
			bankRecTest.ExternalSetup();
			var (unclearedTransaction, clearedTransaction, bankRecon) = bankRecTest.SetupTransactionsForExternalTests();

			var snapshotBefore = bankRecon.MergedTransactionsSnapshot();
			var changedTran = (BankReconTransaction)bankRecon.MergedTransactions.First(x => x.PK == unclearedTransaction.PK);
			changedTran.AH_InvoiceAmount += 10m;
			changedTran.AH_OutstandingAmount += 10m;
			changedTran.AH_OSTotal += 10m;
			changedTran.OnLoaded();
			var removedTran = bankRecon.MergedTransactions.Where(x => x.PK != unclearedTransaction.PK).First();
			bankRecon.MergedTransactions.Remove(removedTran);
			var addedTran = Factory.NewWithValidTestData<Statement>();
			bankRecon.MergedTransactions.Add(addedTran);
			var snapshotAfter = bankRecon.MergedTransactionsSnapshot();
			bankRecon.TrackTransactionChanges(snapshotBefore, snapshotAfter);

			var docWrapper = DocBankReconciliation.New(bankRecon, Factory);
			CombineAssertions("DocWrapper for Added transaction should match originally added transaction properties", () =>
			{
				var docAddedTrans = docWrapper.TransactionsAdded.Cast<DocBankReconciliationTransaction>().First(x => x.Identifier == addedTran.PK);
				AssertEquals(addedTran.TransactionDate, docAddedTrans.TransactionDate);
				AssertEquals(addedTran.InvoiceDate, docAddedTrans.InvoiceDate);
				AssertEquals(addedTran.Type, docAddedTrans.Type);
				AssertEquals(addedTran.Method, docAddedTrans.Method);
				AssertEquals(addedTran.ChequeRef, docAddedTrans.ChequeRef);
				AssertEquals(addedTran.BatchNo, docAddedTrans.BatchNo);
				AssertEquals(addedTran.Payee, docAddedTrans.Payee);
				AssertEquals(addedTran.Debit, docAddedTrans.Debit);
				AssertEquals(addedTran.Credit, docAddedTrans.Credit);
				AssertEquals(addedTran.IsCleared, docAddedTrans.IsCleared);
				AssertEquals(addedTran.LineType, docAddedTrans.LineType);
				AssertEquals(addedTran.ClearedDate, docAddedTrans.ClearedDate);
			});
			CombineAssertions("DocWrapper for Removed transaction should match originally removed transaction properties", () =>
			{
				var removedTranAsInterface = (IBankReconMergedTransaction)removedTran;
				var docRemovedTrans = docWrapper.TransactionsRemoved.Cast<DocBankReconciliationTransaction>().First(x => x.Identifier == removedTran.PK);
				AssertEquals(removedTranAsInterface.TransactionDate, docRemovedTrans.TransactionDate);
				AssertEquals(removedTranAsInterface.InvoiceDate, docRemovedTrans.InvoiceDate);
				AssertEquals(removedTranAsInterface.Type, docRemovedTrans.Type);
				AssertEquals(removedTranAsInterface.Method, docRemovedTrans.Method);
				AssertEquals(removedTranAsInterface.ChequeRef, docRemovedTrans.ChequeRef);
				AssertEquals(removedTranAsInterface.BatchNo, docRemovedTrans.BatchNo);
				AssertEquals(removedTranAsInterface.Payee, docRemovedTrans.Payee);
				AssertEquals(removedTranAsInterface.Debit, docRemovedTrans.Debit);
				AssertEquals(removedTranAsInterface.Credit, docRemovedTrans.Credit);
				AssertEquals(removedTranAsInterface.IsCleared, docRemovedTrans.IsCleared);
				AssertEquals(removedTranAsInterface.LineType, docRemovedTrans.LineType);
				AssertEquals(removedTranAsInterface.ClearedDate, docRemovedTrans.ClearedDate);
			});
			CombineAssertions("DocWrapper for Changed transaction should have different amounts", () =>
			{
				var docAddedTrans = docWrapper.TransactionsAdded.Cast<DocBankReconciliationTransaction>().First(x => x.Identifier == changedTran.PK);
				var docRemovedTrans = docWrapper.TransactionsRemoved.Cast<DocBankReconciliationTransaction>().First(x => x.Identifier == changedTran.PK);
				AssertNotEquals(docAddedTrans.Credit, docRemovedTrans.Credit);
				AssertEquals(10m, docAddedTrans.Credit - docRemovedTrans.Credit);
			});
		}
	}
}
