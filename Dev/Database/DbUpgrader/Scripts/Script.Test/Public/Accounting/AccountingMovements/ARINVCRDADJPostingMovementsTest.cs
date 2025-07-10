using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.AccountingMovements;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.AccountingMovements.Testing
{
	[TestedType(typeof(ARINVCRDADJPostingMovements))]
	class ARINVCRDADJPostingMovementsTest : DbCreateScriptTest
	{
		public void TestARINVCRDADJTransactionsWithAccrualBasisTax()
		{
			SetupARINVCRDADJTransactionsWithAccrualBasisTax();

			var sqlQuery = "SELECT * FROM ARINVCRDADJPostingMovements(@HeaderPKs)";
			var command = Db.Connection.Command(sqlQuery);
			command.AddTableValuedParameter("@HeaderPKs", "dbo.TVP_uniqueidentifier", new Guid[] { headerPK });

			var result = DataUtils.GetDataTableFromCommand(command);

			MovementHelper.AssertJournalAmount(result, 0M);
			MovementHelper.AssertJournalAmount(result, 110M, MovementHelper.ARControlAccountPK);
			MovementHelper.AssertJournalAmount(result, -100M, MovementHelper.ARSuspenseControlAccountPK);
			MovementHelper.AssertJournalAmount(result, -10M, MovementHelper.GSTOutputAccountPK);

			MovementHelper.AssertAggregationAmount(result);
		}

		void SetupARINVCRDADJTransactionsWithAccrualBasisTax()
		{
			MovementHelper.Setup();

			var helper = new TestDbHelper(TestConnection);

			headerPK = helper.InsertTransactionHeader("AR", "INV", "001", 330, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, postToGL: false);
			helper.InsertTransactionLine(headerPK, null, null, MovementHelper.GLAccountPK1, MovementHelper.BranchPK, MovementHelper.DepartmentPK, null, 100, "REV", MovementHelper.PostDate, null, 10, 1, "A");
		}

		public void TestARINVCRDADJTransactionsWithCashBisisTax()
		{
			SetupARINVCRDADJTransactionsWithCashBisisTax();

			var sqlQuery = "SELECT * FROM ARINVCRDADJPostingMovements(@HeaderPKs)";
			var command = Db.Connection.Command(sqlQuery);
			command.AddTableValuedParameter("@HeaderPKs", "dbo.TVP_uniqueidentifier", new Guid[] { headerPK });

			var result = DataUtils.GetDataTableFromCommand(command);

			MovementHelper.AssertJournalAmount(result, 0M);
			MovementHelper.AssertJournalAmount(result, 220M, MovementHelper.ARControlAccountPK);
			MovementHelper.AssertJournalAmount(result, -200M, MovementHelper.ARSuspenseControlAccountPK);
			MovementHelper.AssertJournalAmount(result, -20M, MovementHelper.PendingGSTOutputAccountPK);

			MovementHelper.AssertAggregationAmount(result);
		}

		void SetupARINVCRDADJTransactionsWithCashBisisTax()
		{
			MovementHelper.Setup();
			MovementHelper.EnableGSTCashBasis(TestDbHelper.DefaultCompanyPK);

			var helper = new TestDbHelper(TestConnection);

			headerPK = helper.InsertTransactionHeader("AR", "INV", "001", 220, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, postToGL: false);
			helper.InsertTransactionLine(headerPK, null, null, MovementHelper.GLAccountPK2, MovementHelper.BranchPK, MovementHelper.DepartmentPK, null, 200, "REV", MovementHelper.PostDate, null, 20, 1, "C");
		}

		Guid headerPK;

		AccountingMovementsTestHelper MovementHelper => movementHelper ?? (movementHelper = new AccountingMovementsTestHelper(TestConnection));
		AccountingMovementsTestHelper movementHelper;
	}
}

