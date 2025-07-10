using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.AccountingMovements;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.AccountingMovements.Testing
{
	[TestedType(typeof(APINVCRDADJPostingMovements))]
	class APINVCRDADJPostingMovementsTest : DbCreateScriptTest
	{
		public void TestAPINVCRDADJTransactionsWithAccrualBasisTax()
		{
			SetupAPINVCRDADJTransactionsWithAccrualBasisTax();

			var sqlQuery = "SELECT * FROM APINVCRDADJPostingMovements(@HeaderPKs)";
			var command = Db.Connection.Command(sqlQuery);
			command.AddTableValuedParameter("@HeaderPKs", "dbo.TVP_uniqueidentifier", new Guid[] { headerPK });

			var result = DataUtils.GetDataTableFromCommand(command);

			MovementHelper.AssertJournalAmount(result, 0M);
			MovementHelper.AssertJournalAmount(result, 110M, MovementHelper.APControlAccountPK);
			MovementHelper.AssertJournalAmount(result, -100M, MovementHelper.APSuspenseControlAccountPK);
			MovementHelper.AssertJournalAmount(result, -10M, MovementHelper.GSTInputAccountPK);

			MovementHelper.AssertAggregationAmount(result);
		}

		void SetupAPINVCRDADJTransactionsWithAccrualBasisTax()
		{
			MovementHelper.Setup();

			var helper = new TestDbHelper(TestConnection);
			headerPK = helper.InsertTransactionHeader("AP", "INV", "001", 110, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, postToGL: false);
			helper.InsertTransactionLine(headerPK, null, null, MovementHelper.GLAccountPK1, MovementHelper.BranchPK, MovementHelper.DepartmentPK, null, 100, "CST", MovementHelper.PostDate, null, 10, 1, "A");
		}

		public void TestAPINVCRDADJTransactionsWithCashBisisTax()
		{
			SetupAPINVCRDADJTransactionsWithCashBisisTax();

			var sqlQuery = "SELECT * FROM APINVCRDADJPostingMovements(@HeaderPKs)";
			var command = Db.Connection.Command(sqlQuery);
			command.AddTableValuedParameter("@HeaderPKs", "dbo.TVP_uniqueidentifier", new Guid[] { headerPK });

			var result = DataUtils.GetDataTableFromCommand(command);

			MovementHelper.AssertJournalAmount(result, 0M);
			MovementHelper.AssertJournalAmount(result, 220M, MovementHelper.APControlAccountPK);
			MovementHelper.AssertJournalAmount(result, -200M, MovementHelper.APSuspenseControlAccountPK);
			MovementHelper.AssertJournalAmount(result, -20M, MovementHelper.PendingGSTInputAccountPK);

			MovementHelper.AssertAggregationAmount(result);
		}

		void SetupAPINVCRDADJTransactionsWithCashBisisTax()
		{
			MovementHelper.Setup();
			MovementHelper.EnableGSTCashBasis(TestDbHelper.DefaultCompanyPK);

			var helper = new TestDbHelper(TestConnection);

			headerPK = helper.InsertTransactionHeader("AP", "INV", "001", 220, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, postToGL: false);
			helper.InsertTransactionLine(headerPK, null, null, MovementHelper.GLAccountPK2, MovementHelper.BranchPK, MovementHelper.DepartmentPK, null, 200, "CST", MovementHelper.PostDate, null, 20, 1, "C");
		}

		#region Recoverable Tax

		public void TestAPINVCRDADJTransactionsWithAccrualBasisAndRecoverableTax()
		{
			SetupAPINVCRDADJTransactionsWithAccrualBasisAndRecoverableTax();

			var sqlQuery = "SELECT * FROM APINVCRDADJPostingMovements(@HeaderPKs)";
			var command = Db.Connection.Command(sqlQuery);
			command.AddTableValuedParameter("@HeaderPKs", "dbo.TVP_uniqueidentifier", new Guid[] { headerPK });

			var result = DataUtils.GetDataTableFromCommand(command);

			MovementHelper.AssertJournalAmount(result, 0M);
			MovementHelper.AssertJournalAmount(result, 110M, MovementHelper.APControlAccountPK);
			MovementHelper.AssertJournalAmount(result, -100M, MovementHelper.APSuspenseControlAccountPK);
			MovementHelper.AssertJournalAmount(result, -8M, MovementHelper.GSTInputAccountPK);
			MovementHelper.AssertJournalAmount(result, -2M, MovementHelper.GLAccountPK1);

			MovementHelper.AssertAggregationAmount(result);
		}

		void SetupAPINVCRDADJTransactionsWithAccrualBasisAndRecoverableTax()
		{
			MovementHelper.Setup();

			var helper = new TestDbHelper(TestConnection);
			headerPK = helper.InsertTransactionHeader("AP", "INV", "001", 110, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, postToGL: false);
			helper.InsertTransactionLine(headerPK, null, null, MovementHelper.GLAccountPK1, MovementHelper.BranchPK, MovementHelper.DepartmentPK, null, 100, "CST", MovementHelper.PostDate, null, 10, 0.8M, "A");
		}

		public void TestAPINVCRDADJTransactionsWithCashBasisAndRecoverableTax()
		{
			SetupAPINVCRDADJTransactionsWithCashBasisAndRecoverableTax();

			var sqlQuery = "SELECT * FROM APINVCRDADJPostingMovements(@HeaderPKs)";
			var command = Db.Connection.Command(sqlQuery);
			command.AddTableValuedParameter("@HeaderPKs", "dbo.TVP_uniqueidentifier", new Guid[] { headerPK });

			var result = DataUtils.GetDataTableFromCommand(command);

			MovementHelper.AssertJournalAmount(result, 0M);
			MovementHelper.AssertJournalAmount(result, 220M, MovementHelper.APControlAccountPK);
			MovementHelper.AssertJournalAmount(result, -200M, MovementHelper.APSuspenseControlAccountPK);
			MovementHelper.AssertJournalAmount(result, -20M, MovementHelper.PendingGSTInputAccountPK);

			MovementHelper.AssertAggregationAmount(result);
		}

		void SetupAPINVCRDADJTransactionsWithCashBasisAndRecoverableTax()
		{
			MovementHelper.Setup();
			MovementHelper.EnableGSTCashBasis(TestDbHelper.DefaultCompanyPK);

			var helper = new TestDbHelper(TestConnection);
			headerPK = helper.InsertTransactionHeader("AP", "INV", "001", 220, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, postToGL: false);
			helper.InsertTransactionLine(headerPK, null, null, MovementHelper.GLAccountPK2, MovementHelper.BranchPK, MovementHelper.DepartmentPK, null, 200, "CST", MovementHelper.PostDate, null, 20, 0.8M, "C");
		}

		#endregion

		Guid headerPK;

		AccountingMovementsTestHelper MovementHelper => movementHelper ?? (movementHelper = new AccountingMovementsTestHelper(TestConnection));
		AccountingMovementsTestHelper movementHelper;
	}
}

