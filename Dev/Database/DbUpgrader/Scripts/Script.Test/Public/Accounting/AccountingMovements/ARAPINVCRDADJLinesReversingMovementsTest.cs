using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.AccountingMovements;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.AccountingMovements.Testing
{
	[TestedType(typeof(ARAPINVCRDADJLinesReversingMovements))]
	class ARAPINVCRDADJLinesReversingMovementsTest : DbCreateScriptTest
	{
		public void TestARINVCRDADJTransactionsWithReversingDate()
		{
			SetupARINVCRDADJTransactionsWithReversingDate();

			var sqlQuery = @"SELECT * FROM ARINVCRDADJPostingMovements(@HeaderPKs)
UNION ALL

SELECT * FROM ARAPINVCRDADJLinesReversingMovements(@LinePKs)";
			var command = Db.Connection.Command(sqlQuery);
			command.AddTableValuedParameter("@HeaderPKs", "dbo.TVP_uniqueidentifier", new Guid[] { headerPK });
			command.AddTableValuedParameter("@LinePKs", "dbo.TVP_uniqueidentifier", new Guid[] { linePK1, linePK2 });

			var result = DataUtils.GetDataTableFromCommand(command);

			MovementHelper.AssertJournalAmount(result, 0M);
			MovementHelper.AssertJournalAmount(result, 330M, MovementHelper.ARControlAccountPK, MovementHelper.PostDate);
			MovementHelper.AssertJournalAmount(result, -300M, MovementHelper.ARSuspenseControlAccountPK, MovementHelper.PostDate);
			MovementHelper.AssertJournalAmount(result, -30M, MovementHelper.GSTOutputAccountPK, MovementHelper.PostDate);
			MovementHelper.AssertJournalAmount(result, 300M, MovementHelper.ARSuspenseControlAccountPK, MovementHelper.ReverseDate);
			MovementHelper.AssertJournalAmount(result, -100M, MovementHelper.GLAccountPK1, MovementHelper.ReverseDate);
			MovementHelper.AssertJournalAmount(result, -200M, MovementHelper.GLAccountPK2, MovementHelper.ReverseDate);

			MovementHelper.AssertAggregationAmount(result);
		}

		void SetupARINVCRDADJTransactionsWithReversingDate()
		{
			MovementHelper.Setup();

			var helper = new TestDbHelper(TestConnection);

			headerPK = helper.InsertTransactionHeader("AR", "INV", "001", 330, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, postToGL: false);
			linePK1 = helper.InsertTransactionLine(headerPK, null, null, MovementHelper.GLAccountPK1, MovementHelper.BranchPK, MovementHelper.DepartmentPK, null, 100, "REV", MovementHelper.PostDate, MovementHelper.ReverseDate, 10);
			linePK2 = helper.InsertTransactionLine(headerPK, null, null, MovementHelper.GLAccountPK2, MovementHelper.BranchPK, MovementHelper.DepartmentPK, null, 200, "REV", MovementHelper.PostDate, MovementHelper.ReverseDate, 20);
		}

		public void TestAPINVCRDADJTransactionsWithReversingDate()
		{
			SetupAPINVCRDADJTransactionsWithReversingDate();

			var sqlQuery = @"SELECT * FROM APINVCRDADJPostingMovements(@HeaderPKs)
UNION ALL

SELECT * FROM ARAPINVCRDADJLinesReversingMovements(@LinePKs)";
			var command = Db.Connection.Command(sqlQuery);
			command.AddTableValuedParameter("@HeaderPKs", "dbo.TVP_uniqueidentifier", new Guid[] { headerPK });
			command.AddTableValuedParameter("@LinePKs", "dbo.TVP_uniqueidentifier", new Guid[] { linePK1, linePK2 });

			var result = DataUtils.GetDataTableFromCommand(command);

			MovementHelper.AssertJournalAmount(result, 0M);
			MovementHelper.AssertJournalAmount(result, 330M, MovementHelper.APControlAccountPK, MovementHelper.PostDate);
			MovementHelper.AssertJournalAmount(result, -300M, MovementHelper.APSuspenseControlAccountPK, MovementHelper.PostDate);
			MovementHelper.AssertJournalAmount(result, -30M, MovementHelper.GSTInputAccountPK, MovementHelper.PostDate);
			MovementHelper.AssertJournalAmount(result, 300M, MovementHelper.APSuspenseControlAccountPK, MovementHelper.ReverseDate);
			MovementHelper.AssertJournalAmount(result, -100M, MovementHelper.GLAccountPK1, MovementHelper.ReverseDate);
			MovementHelper.AssertJournalAmount(result, -200M, MovementHelper.GLAccountPK2, MovementHelper.ReverseDate);

			MovementHelper.AssertAggregationAmount(result);
		}

		void SetupAPINVCRDADJTransactionsWithReversingDate()
		{
			MovementHelper.Setup();

			var helper = new TestDbHelper(TestConnection);

			headerPK = helper.InsertTransactionHeader("AP", "INV", "001", 330, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, postToGL: false);
			linePK1 = helper.InsertTransactionLine(headerPK, null, null, MovementHelper.GLAccountPK1, MovementHelper.BranchPK, MovementHelper.DepartmentPK, null, 100, "CST", MovementHelper.PostDate, MovementHelper.ReverseDate, 10);
			linePK2 = helper.InsertTransactionLine(headerPK, null, null, MovementHelper.GLAccountPK2, MovementHelper.BranchPK, MovementHelper.DepartmentPK, null, 200, "CST", MovementHelper.PostDate, MovementHelper.ReverseDate, 20);
		}

		Guid headerPK, linePK1, linePK2;

		AccountingMovementsTestHelper MovementHelper => movementHelper ?? (movementHelper = new AccountingMovementsTestHelper(TestConnection));
		AccountingMovementsTestHelper movementHelper;
	}
}

