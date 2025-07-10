using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.AccountingMovements;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.AccountingMovements.Testing
{
	[TestedType(typeof(CBEXXPostingMovements))]
	class CBEXXPostingMovementsTest : DbCreateScriptTest
	{
		public void TestCBEXXTransactions()
		{
			SetupCBEXXTransactions();

			var sqlQuery = "SELECT * FROM CBEXXPostingMovements(@HeaderPKs)";
			var command = Db.Connection.Command(sqlQuery);
			command.AddTableValuedParameter("@HeaderPKs", "dbo.TVP_uniqueidentifier", new Guid[] { headerPK });

			var result = DataUtils.GetDataTableFromCommand(command);
			MovementHelper.AssertJournalAmount(result, 0M);
			MovementHelper.AssertJournalAmount(result, 100M, MovementHelper.GLAccountPK1);
			MovementHelper.AssertJournalAmount(result, -100M, MovementHelper.GLAccountPK2);

			MovementHelper.AssertOSJournalAmount(result, 0M);
			MovementHelper.AssertOSJournalAmount(result, 0M, MovementHelper.GLAccountPK1);
			MovementHelper.AssertOSJournalAmount(result, 0M, MovementHelper.GLAccountPK2);

			MovementHelper.AssertAggregationAmount(result);
		}

		public void SetupCBEXXTransactions()
		{
			MovementHelper.Setup();

			var helper = new TestDbHelper(TestConnection);
			var bankAccountPK = helper.InsertBankAccount("ZBA", MovementHelper.GLAccountPK1);

			headerPK = helper.InsertTransactionHeader("CB", "EXX", "001", 100, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, bankAccountPK, glAccountPK: MovementHelper.GLAccountPK2, postToGL: false);
		}

		Guid headerPK;

		AccountingMovementsTestHelper MovementHelper => movementHelper ?? (movementHelper = new AccountingMovementsTestHelper(TestConnection));
		AccountingMovementsTestHelper movementHelper;
	}
}

