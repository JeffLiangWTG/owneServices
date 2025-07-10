using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.AccountingMovements;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.AccountingMovements.Testing
{
	[TestedType(typeof(ARAPPAYRECPostingMovements))]
	class ARAPPAYRECPostingMovementsTest : DbCreateScriptTest
	{
		public void TestARAPPAYTransactions()
		{
			SetupARAPPAYTransactions();

			var sqlQuery = "SELECT * FROM ARAPPAYRECPostingMovements(@HeaderPKs)";
			var command = Db.Connection.Command(sqlQuery);
			command.AddTableValuedParameter("@HeaderPKs", "dbo.TVP_uniqueidentifier", new Guid[] { headerPK1, headerPK2 });

			var result = DataUtils.GetDataTableFromCommand(command);
			MovementHelper.AssertJournalAmount(result, 0M);
			MovementHelper.AssertJournalAmount(result, -100M, MovementHelper.GLAccountPK1);
			MovementHelper.AssertJournalAmount(result, -200M, MovementHelper.GLAccountPK2);
			MovementHelper.AssertJournalAmount(result, 100M, MovementHelper.APControlAccountPK);
			MovementHelper.AssertJournalAmount(result, 200M, MovementHelper.ARControlAccountPK);

			MovementHelper.AssertAggregationAmount(result);
		}

		void SetupARAPPAYTransactions()
		{
			MovementHelper.Setup();

			var helper = new TestDbHelper(TestConnection);
			var bankAccountPK1 = helper.InsertBankAccount("ZBA", MovementHelper.GLAccountPK1);
			var bankAccountPK2 = helper.InsertBankAccount("ZBB", MovementHelper.GLAccountPK2);

			headerPK1 = helper.InsertTransactionHeader("AP", "PAY", "001", 100, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, bankAccountPK1, postToGL: false);
			headerPK2 = helper.InsertTransactionHeader("AR", "PAY", "002", 200, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, bankAccountPK2, postToGL: false);
		}

		public void TestARAPRECTransactions()
		{
			SetupARAPRECTransactions();

			var sqlQuery = "SELECT * FROM ARAPPAYRECPostingMovements(@HeaderPKs)";
			var command = Db.Connection.Command(sqlQuery);
			command.AddTableValuedParameter("@HeaderPKs", "dbo.TVP_uniqueidentifier", new Guid[] { headerPK1, headerPK2 });

			var result = DataUtils.GetDataTableFromCommand(command);
			MovementHelper.AssertJournalAmount(result, 0M);
			MovementHelper.AssertJournalAmount(result, 100M, MovementHelper.GLAccountPK1);
			MovementHelper.AssertJournalAmount(result, 200M, MovementHelper.GLAccountPK2);
			MovementHelper.AssertJournalAmount(result, -100M, MovementHelper.APControlAccountPK);
			MovementHelper.AssertJournalAmount(result, -200M, MovementHelper.ARControlAccountPK);

			MovementHelper.AssertAggregationAmount(result);
		}

		void SetupARAPRECTransactions()
		{
			MovementHelper.Setup();

			var helper = new TestDbHelper(TestConnection);
			var bankAccountPK1 = helper.InsertBankAccount("ZBA", MovementHelper.GLAccountPK1);
			var bankAccountPK2 = helper.InsertBankAccount("ZBB", MovementHelper.GLAccountPK2);

			headerPK1 = helper.InsertTransactionHeader("AP", "REC", "001", -100, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, bankAccountPK1, postToGL: false);
			headerPK2 = helper.InsertTransactionHeader("AR", "REC", "002", -200, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, bankAccountPK2, postToGL: false);
		}

		Guid headerPK1, headerPK2;

		AccountingMovementsTestHelper MovementHelper => movementHelper ?? (movementHelper = new AccountingMovementsTestHelper(TestConnection));
		AccountingMovementsTestHelper movementHelper;
	}
}

