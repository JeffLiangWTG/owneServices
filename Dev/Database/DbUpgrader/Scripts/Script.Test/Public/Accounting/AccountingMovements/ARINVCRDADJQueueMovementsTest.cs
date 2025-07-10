using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.AccountingMovements;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.AccountingMovements.Testing
{
	[TestedType(typeof(ARINVCRDADJQueueMovements))]
	class ARINVCRDADJQueueMovementsTest : DbCreateScriptTest
	{
		public void TestARINVCRDADJTransactionsWithCashBisisVAT()
		{
			SetupARINVCRDADJTransactionsWithCashBisisVAT();

			var sqlQuery = @"SELECT * FROM ARINVCRDADJPostingMovements(@HeaderPKs)
UNION ALL
SELECT * FROM ARINVCRDADJQueueMovements(@QueuePKs)
";
			var command = Db.Connection.Command(sqlQuery);
			command.AddTableValuedParameter("@HeaderPKs", "dbo.TVP_uniqueidentifier", new Guid[] { headerPK });
			command.AddTableValuedParameter("@QueuePKs", "dbo.TVP_uniqueidentifier", new Guid[] { cashVATPK });

			var result = DataUtils.GetDataTableFromCommand(command);

			MovementHelper.AssertJournalAmount(result, 0M);
			MovementHelper.AssertJournalAmount(result, 220M, MovementHelper.ARControlAccountPK, MovementHelper.PostDate);
			MovementHelper.AssertJournalAmount(result, -200M, MovementHelper.ARSuspenseControlAccountPK, MovementHelper.PostDate);
			MovementHelper.AssertJournalAmount(result, -20M, MovementHelper.PendingGSTOutputAccountPK, MovementHelper.PostDate);
			MovementHelper.AssertJournalAmount(result, -1M, MovementHelper.GSTOutputAccountPK, MovementHelper.CashDate);
			MovementHelper.AssertJournalAmount(result, 1M, MovementHelper.PendingGSTOutputAccountPK, MovementHelper.CashDate);

			MovementHelper.AssertAggregationAmount(result);
		}

		void SetupARINVCRDADJTransactionsWithCashBisisVAT()
		{
			MovementHelper.Setup();
			MovementHelper.EnableGSTCashBasis(TestDbHelper.DefaultCompanyPK);

			var helper = new TestDbHelper(TestConnection);

			headerPK = helper.InsertTransactionHeader("AR", "INV", "001", 220, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, postToGL: false);
			var linePK = helper.InsertTransactionLine(headerPK, null, null, MovementHelper.GLAccountPK2, MovementHelper.BranchPK, MovementHelper.DepartmentPK, null, 200, "REV", MovementHelper.PostDate, null, 20, 1, "C");
			cashVATPK = helper.InsertCashBasisVAT(linePK, MovementHelper.CashDate);
		}

		Guid headerPK;
		Guid cashVATPK;

		AccountingMovementsTestHelper MovementHelper => movementHelper ?? (movementHelper = new AccountingMovementsTestHelper(TestConnection));
		AccountingMovementsTestHelper movementHelper;
	}
}

