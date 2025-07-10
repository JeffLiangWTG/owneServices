using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.Accounting.Business.CashBook.Testing;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.Business.Testing.CashBook.BankReconciliation
{
	class BankReconciliationSnapshotTest : TestCaseWithFactory
	{
		[SuspendCriticalValidation]
		public void TestConstructor()
		{
			var testObj = new BankReconciliationTest();
			testObj.ExternalSetup();
			var (_, _, bankRecon) = testObj.SetupTransactionsForExternalTests();

			var snapshot = new BankReconciliationSnapshot(bankRecon);
			CombineAssertions("Snapshot should load data from BankReconciliation", () =>
			{
				AssertEquals(1000m, snapshot.StatementBalance);
				AssertEquals(-2150m, snapshot.UnclearedCashbookAmount);
				AssertEquals(120m, snapshot.UnclearedStatementAmount);
				AssertEquals(-1030m, snapshot.AmendedBankStatementBalance);
				AssertEquals(0m, snapshot.CashBookBalance);
				AssertEquals(-1030m, snapshot.ReconError);
			});
		}
	}
}
