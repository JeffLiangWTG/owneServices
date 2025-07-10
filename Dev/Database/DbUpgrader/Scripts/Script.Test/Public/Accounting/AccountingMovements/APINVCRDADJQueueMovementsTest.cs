using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.AccountingMovements;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.AccountingMovements.Testing
{
	[TestedType(typeof(APINVCRDADJQueueMovements))]
	class APINVCRDADJQueueMovementsTest : DbCreateScriptTest
	{
		public void TestAPINVCRDADJTransactionsWithCashBisisVAT()
		{
			SetupAPINVCRDADJTransactionsWithCashBisisVAT();

			var sqlQuery = @"SELECT * FROM APINVCRDADJPostingMovements(@HeaderPKs)
UNION ALL
SELECT * FROM ARAPINVCRDADJLinesReversingMovements(@LinePKs)
UNION ALL
SELECT * FROM APINVCRDADJQueueMovements(@QueuePKs)
";
			var command = Db.Connection.Command(sqlQuery);
			command.AddTableValuedParameter("@HeaderPKs", "dbo.TVP_uniqueidentifier", new Guid[] { headerPK });
			command.AddTableValuedParameter("@LinePKs", "dbo.TVP_uniqueidentifier", new Guid[] { linePK });
			command.AddTableValuedParameter("@QueuePKs", "dbo.TVP_uniqueidentifier", new Guid[] { cashVATPK });

			var result = DataUtils.GetDataTableFromCommand(command);

			MovementHelper.AssertJournalAmount(result, 0M);
			MovementHelper.AssertJournalAmount(result, 110M, MovementHelper.APControlAccountPK, MovementHelper.PostDate);
			MovementHelper.AssertJournalAmount(result, -10M, MovementHelper.PendingGSTInputAccountPK, MovementHelper.PostDate);
			MovementHelper.AssertJournalAmount(result, -100M, MovementHelper.APSuspenseControlAccountPK, MovementHelper.PostDate);
			MovementHelper.AssertJournalAmount(result, 100M, MovementHelper.APSuspenseControlAccountPK, MovementHelper.ReverseDate);
			MovementHelper.AssertJournalAmount(result, -100M, MovementHelper.GLAccountPK2, MovementHelper.ReverseDate);
			MovementHelper.AssertJournalAmount(result, -1M, MovementHelper.GSTInputAccountPK, MovementHelper.CashDate);
			MovementHelper.AssertJournalAmount(result, 1M, MovementHelper.PendingGSTInputAccountPK, MovementHelper.CashDate);

			MovementHelper.AssertAggregationAmount(result);
		}

		void SetupAPINVCRDADJTransactionsWithCashBisisVAT()
		{
			MovementHelper.Setup();
			MovementHelper.EnableGSTCashBasis(TestDbHelper.DefaultCompanyPK);

			var helper = new TestDbHelper(TestConnection);

			headerPK = helper.InsertTransactionHeader("AP", "INV", "001", 110, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, postToGL: false);
			linePK = helper.InsertTransactionLine(headerPK, null, null, MovementHelper.GLAccountPK2, MovementHelper.BranchPK, MovementHelper.DepartmentPK, null, 100, "CST", MovementHelper.PostDate, MovementHelper.ReverseDate, 10, 1, "C");
			cashVATPK = helper.InsertCashBasisVAT(linePK, MovementHelper.CashDate);
		}

		public void TestAPINVCRDADJTransactionsWithRecoverableTax()
		{
			SetupAPINVCRDADJTransactionsWithRecoverableTax();

			var sqlQuery = @"SELECT * FROM APINVCRDADJPostingMovements(@HeaderPKs)
UNION ALL
SELECT * FROM ARAPINVCRDADJLinesReversingMovements(@LinePKs)
UNION ALL
SELECT * FROM APINVCRDADJQueueMovements(@QueuePKs)
";
			var command = Db.Connection.Command(sqlQuery);
			command.AddTableValuedParameter("@HeaderPKs", "dbo.TVP_uniqueidentifier", new Guid[] { headerPK });
			command.AddTableValuedParameter("@LinePKs", "dbo.TVP_uniqueidentifier", new Guid[] { linePK });
			command.AddTableValuedParameter("@QueuePKs", "dbo.TVP_uniqueidentifier", new Guid[] { cashVATPK });

			var result = DataUtils.GetDataTableFromCommand(command);

			MovementHelper.AssertJournalAmount(result, 0M);
			MovementHelper.AssertJournalAmount(result, 110M, MovementHelper.APControlAccountPK, MovementHelper.PostDate);
			MovementHelper.AssertJournalAmount(result, -10M, MovementHelper.PendingGSTInputAccountPK, MovementHelper.PostDate);
			MovementHelper.AssertJournalAmount(result, -100M, MovementHelper.APSuspenseControlAccountPK, MovementHelper.PostDate);
			MovementHelper.AssertJournalAmount(result, 100M, MovementHelper.APSuspenseControlAccountPK, MovementHelper.ReverseDate);
			MovementHelper.AssertJournalAmount(result, -100M, MovementHelper.GLAccountPK2, MovementHelper.ReverseDate);
			MovementHelper.AssertJournalAmount(result, 1M, MovementHelper.PendingGSTInputAccountPK, MovementHelper.CashDate);
			MovementHelper.AssertJournalAmount(result, -0.8M, MovementHelper.GSTInputAccountPK, MovementHelper.CashDate);
			MovementHelper.AssertJournalAmount(result, -0.2M, MovementHelper.GLAccountPK2, MovementHelper.CashDate);

			MovementHelper.AssertAggregationAmount(result);
		}

		void SetupAPINVCRDADJTransactionsWithRecoverableTax()
		{
			MovementHelper.Setup();
			MovementHelper.EnableGSTCashBasis(TestDbHelper.DefaultCompanyPK);

			var helper = new TestDbHelper(TestConnection);

			headerPK = helper.InsertTransactionHeader("AP", "INV", "001", 110, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, postToGL: false);
			linePK = helper.InsertTransactionLine(headerPK, null, null, MovementHelper.GLAccountPK2, MovementHelper.BranchPK, MovementHelper.DepartmentPK, null, 100, "CST", MovementHelper.PostDate, MovementHelper.ReverseDate, 10, 0.8M, "C");
			cashVATPK = helper.InsertCashBasisVAT(linePK, MovementHelper.CashDate);
		}

		Guid headerPK, linePK;
		Guid cashVATPK;

		AccountingMovementsTestHelper MovementHelper => movementHelper ?? (movementHelper = new AccountingMovementsTestHelper(TestConnection));
		AccountingMovementsTestHelper movementHelper;
	}
}

