using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.AccStatement;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing.CashBook.BankReconciliation
{
	class BankReconTransactionSnapshotTest : TestCaseWithFactory
	{
		public void TestConstructor_WithPrimitives()
		{
			var guid = ZGuid.NewZGuid();
			var now = ZDateTime.Now;
			var snapshot = new BankReconTransactionSnapshot(guid, now, now.AddSeconds(1), "Type", "Method", "Ref#", "Batch#", "Payee", 100m, 0m, false, "lineType", ZDateTime.Empty);
			AssertEquals(guid, snapshot.PK);
			AssertEquals(now, snapshot.TransactionDate);
			AssertEquals(now.AddSeconds(1), snapshot.InvoiceDate);
			AssertEquals("Type", snapshot.Type);
			AssertEquals("Method", snapshot.Method);
			AssertEquals("Ref#", snapshot.ChequeRef);
			AssertEquals("Batch#", snapshot.BatchNo);
			AssertEquals("Payee", snapshot.Payee);
			AssertEquals(100m, snapshot.Debit);
			AssertEquals(0m, snapshot.Credit);
			AssertEquals(false, snapshot.IsCleared);
			AssertEquals("lineType", snapshot.LineType);
			AssertEquals(ZDateTime.Empty, snapshot.ClearedDate);

			var snapshot2 = new BankReconTransactionSnapshot(guid, now, now.AddSeconds(1), "Type", "Method", "Ref#", "Batch#", "Payee", 0m, 80m, true, "lineType", now.AddSeconds(2));
			AssertEquals(guid, snapshot2.PK);
			AssertEquals(now, snapshot2.TransactionDate);
			AssertEquals(now.AddSeconds(1), snapshot2.InvoiceDate);
			AssertEquals("Type", snapshot2.Type);
			AssertEquals("Method", snapshot2.Method);
			AssertEquals("Ref#", snapshot2.ChequeRef);
			AssertEquals("Batch#", snapshot2.BatchNo);
			AssertEquals("Payee", snapshot2.Payee);
			AssertEquals(0m, snapshot2.Debit);
			AssertEquals(80m, snapshot2.Credit);
			AssertEquals(true, snapshot2.IsCleared);
			AssertEquals("lineType", snapshot2.LineType);
			AssertEquals(now.AddSeconds(2), snapshot2.ClearedDate);
		}

		[SuspendCriticalValidation]
		public void TestConstructor_WithStatementBizo()
		{
			var statement = Factory.NewWithValidTestData<Statement>();
			statement.AS_Amount = 100m;
			statement.AS_DebitCredit = Statement.DEBIT;
			var snapshot = new BankReconTransactionSnapshot(statement);
			AssertEquals(statement.PK, statement.PK);
			AssertEquals(100m, snapshot.Credit);          // Yes, these are meant to be backwards; Statement swaps debit & credit
			AssertEquals(0m, snapshot.Debit);

			var statement2 = Factory.NewWithValidTestData<Statement>();
			statement2.AS_Amount = 80m;
			statement2.AS_DebitCredit = Statement.CREDIT;
			var snapshot2 = new BankReconTransactionSnapshot(statement2);
			AssertEquals(statement2.PK, statement2.PK);
			AssertEquals(0m, snapshot2.Credit);          // Yes, these are meant to be backwards;  Statement swaps debit & credit
			AssertEquals(80m, snapshot2.Debit);
		}

		[SuspendCriticalValidation]
		public void TestConstructor_WithBankReconTransactionBizo()
		{
			var transaction = Factory.New<BankReconTransaction>();
			transaction.AH_Ledger = LedgerTypes.CashBook;
			transaction.AH_TransactionType = TransactionTypes.DirectPayment;
			transaction.AH_InvoiceAmount = 120m;
			transaction.AH_OutstandingAmount = 120m;
			transaction.AH_OSTotal = 120m;
			transaction.OnLoaded();

			var snapshot = new BankReconTransactionSnapshot(transaction);
			AssertEquals(snapshot.PK, transaction.PK);
			AssertEquals(120m, snapshot.Debit);
			AssertEquals(0m, snapshot.Credit);

			var transaction2 = Factory.New<BankReconTransaction>();
			transaction2.AH_Ledger = LedgerTypes.CashBook;
			transaction2.AH_TransactionType = TransactionTypes.DirectPayment;
			transaction2.AH_InvoiceAmount = -180m;
			transaction2.AH_OutstandingAmount = -180m;
			transaction2.AH_OSTotal = -180m;
			transaction2.OnLoaded();

			var snapshot2 = new BankReconTransactionSnapshot(transaction2);
			AssertEquals(snapshot2.PK, transaction2.PK);
			AssertEquals(0m, snapshot2.Debit);
			AssertEquals(180m, snapshot2.Credit);
		}

		public void TestEqualsWithAmount()
		{
			var guid1 = ZGuid.NewZGuid();
			var guid2 = ZGuid.NewZGuid();

			Assert(ObjWith(guid1, 10m, 0m).EqualsWithAmount(ObjWith(guid1, 10m, 0m)));
			Assert(ObjWith(guid2, 0m, 20m).EqualsWithAmount(ObjWith(guid2, 0m, 20m)));

			Assert(!ObjWith(guid1, 0m, 20m).EqualsWithAmount(ObjWith(guid1, 0m, 21m)));
			Assert(!ObjWith(guid1, 10m, 0m).EqualsWithAmount(ObjWith(guid1, 11m, 0m)));
			Assert(!ObjWith(guid1, 10m, 0m).EqualsWithAmount(ObjWith(guid1, 0m, 20m)));
			Assert(!ObjWith(guid1, 10m, 20m).EqualsWithAmount(ObjWith(guid2, 10m, 20m)));
		}

		BankReconTransactionSnapshot ObjWith(ZGuid pk, ZDecimal debit, ZDecimal credit)
			=> new BankReconTransactionSnapshot(pk, ZDateTime.Now, ZDateTime.Now, "Type", "Method", "Ref#", "Batch#", "Payee", debit, credit, false, "lineType", ZDateTime.Empty);
	}
}
