using CargoWise.Types;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocBankReconciliationTransaction))]
	sealed class DocBankReconciliationTransactionTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocBankReconciliationTransaction.New(BankReconTransaction, Factory)
			};
		}

		BankReconTransaction BankReconTransaction;
		protected override void SetUp()
		{
			BankReconTransaction = Factory.NewWithValidTestData<BankReconTransaction>();
			base.SetUp();
		}

		public void TestPropertyWrapperOverridesForDebit()
		{
			var transaction = Factory.NewWithValidTestData<BankReconTransaction>();
			transaction.AH_Ledger = LedgerTypes.CashBook;
			transaction.AH_TransactionType = TransactionTypes.DirectPayment;
			transaction.AH_InvoiceAmount = 120m;
			transaction.AH_OutstandingAmount = 120m;
			transaction.AH_OSTotal = 120m;
			transaction.OnLoaded();

			var docTrans = DocBankReconciliationTransaction.New(transaction, Factory);
			AssertEquals(120m, docTrans.Debit);
			AssertEquals(0m, docTrans.Credit);

			var snapshot = SnapshotFor(transaction, debit: 121m, credit: 0m);
			var snapshotDocTrans = DocBankReconciliationTransaction.New(snapshot, Factory);
			AssertEquals(121m, snapshotDocTrans.Debit);
			AssertEquals(0m, snapshotDocTrans.Credit);
		}

		public void TestPropertyWrapperOverridesForCredit()
		{
			var transaction = Factory.NewWithValidTestData<BankReconTransaction>();
			transaction.AH_Ledger = LedgerTypes.CashBook;
			transaction.AH_TransactionType = TransactionTypes.DirectPayment;
			transaction.AH_InvoiceAmount = -180m;
			transaction.AH_OutstandingAmount = -180m;
			transaction.AH_OSTotal = -180m;
			transaction.OnLoaded();

			var docTrans = DocBankReconciliationTransaction.New(transaction, Factory);
			AssertEquals(0m, docTrans.Debit);
			AssertEquals(180m, docTrans.Credit);

			var snapshot = SnapshotFor(transaction, debit: 0m, credit: 181m);
			var snapshotDocTrans = DocBankReconciliationTransaction.New(snapshot, Factory);
			AssertEquals(0m, snapshotDocTrans.Debit);
			AssertEquals(181m, snapshotDocTrans.Credit);
		}

		BankReconTransactionSnapshot SnapshotFor(IBankReconMergedTransaction transaction, ZDecimal debit, ZDecimal credit)
			=> new BankReconTransactionSnapshot(transaction.Identifier,
				  transaction.TransactionDate,
				  transaction.InvoiceDate,
				  transaction.Type,
				  transaction.Method,
				  transaction.ChequeRef,
				  transaction.BatchNo,
				  transaction.Payee,
				  debit,
				  credit,
				  transaction.IsCleared,
				  transaction.LineType,
				  transaction.ClearedDate);
	}
}
