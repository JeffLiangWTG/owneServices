using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.AccountingMovements;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.AccountingMovements.Testing
{
	[TestedType(typeof(WIPPostingMovements))]
	class WIPPostingMovementsTest : DbCreateScriptTest
	{
		public void TestWIPWithoutReversingDate()
		{
			SetupWIPWithoutReversingDate();

			var sqlQuery = "SELECT * FROM WIPPostingMovements(@LinePKs)";
			var command = Db.Connection.Command(sqlQuery);
			command.AddTableValuedParameter("@LinePKs", "dbo.TVP_uniqueidentifier", new Guid[] { linePK });

			var result = DataUtils.GetDataTableFromCommand(command);
			MovementHelper.AssertJournalAmount(result, 0M);
			MovementHelper.AssertJournalAmount(result, -100M, MovementHelper.GLAccountPK1, MovementHelper.PostDate);
			MovementHelper.AssertJournalAmount(result, 100M, MovementHelper.WIPControlAccountPK, MovementHelper.PostDate);

			MovementHelper.AssertAggregationAmount(result);
		}

		void SetupWIPWithoutReversingDate()
		{
			MovementHelper.Setup();

			var helper = new TestDbHelper(TestConnection);

			var jobPK = helper.InsertJob("JOB01", TestDbHelper.DefaultCompanyPK, MovementHelper.BranchPK, MovementHelper.DepartmentPK, "JS", Guid.NewGuid(), "WRK", MovementHelper.PostDate);
			linePK = helper.InsertTransactionLine(null, jobPK, MovementHelper.ChargeCodePK, MovementHelper.GLAccountPK1, MovementHelper.BranchPK, MovementHelper.DepartmentPK, null, -100, "WIP", MovementHelper.PostDate, null);
		}

		Guid linePK;

		AccountingMovementsTestHelper MovementHelper => movementHelper ?? (movementHelper = new AccountingMovementsTestHelper(TestConnection));
		AccountingMovementsTestHelper movementHelper;
	}
}

