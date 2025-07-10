using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.AccountingMovements;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.AccountingMovements.Testing
{
	[TestedType(typeof(CBDRCPostingMovements))]
	class CBDRCPostingMovementsTest : DbCreateScriptTest
	{
		public void TestCBDRCTransactions()
		{
			SetupCBDRCTransactions();

			var sqlQuery = "SELECT * FROM CBDRCPostingMovements(@HeaderPKs)";
			var command = Db.Connection.Command(sqlQuery);
			command.AddTableValuedParameter("@HeaderPKs", "dbo.TVP_uniqueidentifier", new Guid[] { headerPK });

			var result = DataUtils.GetDataTableFromCommand(command);
			MovementHelper.AssertJournalAmount(result, 0M);
			MovementHelper.AssertJournalAmount(result, 110M, MovementHelper.GLAccountPK1);
			MovementHelper.AssertJournalAmount(result, -100M, MovementHelper.GLAccountPK2);
			MovementHelper.AssertJournalAmount(result, -10M, MovementHelper.GSTOutputAccountPK);

			MovementHelper.AssertAggregationAmount(result);
		}

		public void SetupCBDRCTransactions()
		{
			MovementHelper.Setup();

			var helper = new TestDbHelper(TestConnection);
			var bankAccountPK = helper.InsertBankAccount("ZBA", MovementHelper.GLAccountPK1);

			headerPK = helper.InsertTransactionHeader("CB", "DRC", "001", 110, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, bankAccountPK, postToGL: false);
			helper.InsertTransactionLine(headerPK, null, null, MovementHelper.GLAccountPK2, MovementHelper.BranchPK, MovementHelper.DepartmentPK, null, 100, "DRC", MovementHelper.PostDate, null, 10);
		}

		Guid headerPK;

		AccountingMovementsTestHelper MovementHelper => movementHelper ?? (movementHelper = new AccountingMovementsTestHelper(TestConnection));
		AccountingMovementsTestHelper movementHelper;
	}
}

