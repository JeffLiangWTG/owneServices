using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.AccountingMovements;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.AccountingMovements.Testing
{
	[TestedType(typeof(JCJNLJRJLinesReversingMovements))]
	class JCJNLJRJLinesReversingMovementsTest : DbCreateScriptTest
	{
		public void TestJCJNLTransactionsWithReversingDate()
		{
			SetupJCJNLTransactionsWithReversingDate();

			var sqlQuery = @"SELECT * FROM JCJNLJRJPostingMovements(@HeaderPKs)
UNION ALL
SELECT * FROM JCJNLJRJLinesReversingMovements(@LinePKs)
";
			var command = Db.Connection.Command(sqlQuery);
			command.AddTableValuedParameter("@HeaderPKs", "dbo.TVP_uniqueidentifier", new Guid[] { headerPK });
			command.AddTableValuedParameter("@LinePKs", "dbo.TVP_uniqueidentifier", new Guid[] { linePK1 });

			var result = DataUtils.GetDataTableFromCommand(command);

			MovementHelper.AssertJournalAmount(result, 0M);
			MovementHelper.AssertJournalAmount(result, 100M, MovementHelper.CFXAccountPK);
			MovementHelper.AssertJournalAmount(result, -100M, MovementHelper.GLAccountPK1);

			MovementHelper.AssertAggregationAmount(result);
		}

		void SetupJCJNLTransactionsWithReversingDate()
		{
			MovementHelper.Setup();

			var helper = new TestDbHelper(TestConnection);
			headerPK = helper.InsertTransactionHeader("JC", "JNL", "001", 100, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, postToGL: false);
			linePK1 = helper.InsertTransactionLine(headerPK, null, MovementHelper.ChargeCodePK, MovementHelper.GLAccountPK1, MovementHelper.BranchPK, MovementHelper.DepartmentPK, null, 100, "REV", MovementHelper.PostDate, MovementHelper.ReverseDate);
		}

		public void TestJCJRJTransactionsWithReversingDate()
		{
			SetupJCJRJTransactionsWithReversingDate();

			var sqlQuery = @"SELECT * FROM JCJNLJRJPostingMovements(@HeaderPKs)
UNION ALL
SELECT * FROM JCJNLJRJLinesReversingMovements(@LinePKs)
";
			var command = Db.Connection.Command(sqlQuery);
			command.AddTableValuedParameter("@HeaderPKs", "dbo.TVP_uniqueidentifier", new Guid[] { headerPK });
			command.AddTableValuedParameter("@LinePKs", "dbo.TVP_uniqueidentifier", new Guid[] { linePK1, linePK2 });

			var result = DataUtils.GetDataTableFromCommand(command);

			MovementHelper.AssertJournalAmount(result, 0M);
			MovementHelper.AssertJournalAmount(result, -100M, MovementHelper.GLAccountPK1, branch: MovementHelper.BranchPK);
			MovementHelper.AssertJournalAmount(result, 100M, MovementHelper.GLAccountPK2, branch: MovementHelper.BranchPK2);
			MovementHelper.AssertJournalAmount(result, 100M, MovementHelper.JobRevenueJournalControlAccountPK, branch: MovementHelper.BranchPK);
			MovementHelper.AssertJournalAmount(result, -100M, MovementHelper.JobRevenueJournalControlAccountPK, branch: MovementHelper.BranchPK2);

			MovementHelper.AssertAggregationAmount(result);
		}

		void SetupJCJRJTransactionsWithReversingDate()
		{
			MovementHelper.Setup();

			var helper = new TestDbHelper(TestConnection);
			headerPK = helper.InsertTransactionHeader("JC", "JRJ", "001", 0, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, postToGL: false);
			linePK1 = helper.InsertTransactionLine(headerPK, null, null, MovementHelper.GLAccountPK1, MovementHelper.BranchPK, MovementHelper.DepartmentPK, null, 100, "REV", MovementHelper.PostDate, MovementHelper.ReverseDate);
			linePK2 = helper.InsertTransactionLine(headerPK, null, null, MovementHelper.GLAccountPK2, MovementHelper.BranchPK2, MovementHelper.DepartmentPK, null, -100, "REV", MovementHelper.PostDate, MovementHelper.ReverseDate);
		}

		Guid headerPK, linePK1, linePK2;

		AccountingMovementsTestHelper MovementHelper => movementHelper ?? (movementHelper = new AccountingMovementsTestHelper(TestConnection));
		AccountingMovementsTestHelper movementHelper;
	}
}

