using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.AccountingMovements;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.AccountingMovements.Testing
{
	[TestedType(typeof(JCJNLJRJPostingMovements))]
	class JCJNLJRJPostingMovementsTest : DbCreateScriptTest
	{
		public void TestJCJNLTransactions()
		{
			SetupJCJNLTransactions();

			var sqlQuery = "SELECT * FROM JCJNLJRJPostingMovements(@HeaderPKs)";
			var command = Db.Connection.Command(sqlQuery);
			command.AddTableValuedParameter("@HeaderPKs", "dbo.TVP_uniqueidentifier", new Guid[] { headerPK });

			var result = DataUtils.GetDataTableFromCommand(command);

			MovementHelper.AssertJournalAmount(result, 0M);
			MovementHelper.AssertJournalAmount(result, 100M, MovementHelper.CFXAccountPK);
			MovementHelper.AssertJournalAmount(result, -100M, MovementHelper.ARSuspenseControlAccountPK);

			MovementHelper.AssertAggregationAmount(result);
		}

		void SetupJCJNLTransactions()
		{
			MovementHelper.Setup();

			var helper = new TestDbHelper(TestConnection);
			headerPK = helper.InsertTransactionHeader("JC", "JNL", "001", 100, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, postToGL: false);
			helper.InsertTransactionLine(headerPK, null, MovementHelper.ChargeCodePK, MovementHelper.GLAccountPK1, MovementHelper.BranchPK, MovementHelper.DepartmentPK, null, 100, "REV", MovementHelper.PostDate, null);
		}

		public void TestJCJRJTransactions()
		{
			SetupJCJRJTransactions();

			var sqlQuery = "SELECT * FROM JCJNLJRJPostingMovements(@HeaderPKs)";
			var command = Db.Connection.Command(sqlQuery);
			command.AddTableValuedParameter("@HeaderPKs", "dbo.TVP_uniqueidentifier", new Guid[] { headerPK });

			var result = DataUtils.GetDataTableFromCommand(command);

			MovementHelper.AssertJournalAmount(result, 0M);
			MovementHelper.AssertJournalAmount(result, 100M, MovementHelper.JobRevenueJournalControlAccountPK, branch: MovementHelper.BranchPK);
			MovementHelper.AssertJournalAmount(result, -100M, MovementHelper.ARSuspenseControlAccountPK, branch: MovementHelper.BranchPK);
			MovementHelper.AssertJournalAmount(result, -100M, MovementHelper.JobRevenueJournalControlAccountPK, branch: MovementHelper.BranchPK2);
			MovementHelper.AssertJournalAmount(result, 100M, MovementHelper.ARSuspenseControlAccountPK, branch: MovementHelper.BranchPK2);

			MovementHelper.AssertAggregationAmount(result);
		}

		void SetupJCJRJTransactions()
		{
			MovementHelper.Setup();

			var helper = new TestDbHelper(TestConnection);
			headerPK = helper.InsertTransactionHeader("JC", "JRJ", "001", 0, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, postToGL: false);
			helper.InsertTransactionLine(headerPK, null, null, MovementHelper.GLAccountPK1, MovementHelper.BranchPK, MovementHelper.DepartmentPK, null, 100, "REV", MovementHelper.PostDate, null);
			helper.InsertTransactionLine(headerPK, null, null, MovementHelper.GLAccountPK2, MovementHelper.BranchPK2, MovementHelper.DepartmentPK, null, -100, "REV", MovementHelper.PostDate, null);
		}

		Guid headerPK;

		AccountingMovementsTestHelper MovementHelper => movementHelper ?? (movementHelper = new AccountingMovementsTestHelper(TestConnection));
		AccountingMovementsTestHelper movementHelper;
	}
}

