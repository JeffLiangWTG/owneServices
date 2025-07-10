using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.AccountingMovements;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.AccountingMovements.Testing
{
	[TestedType(typeof(CBDPYPostingMovements))]
	class CBDPYPostingMovementsTest : DbCreateScriptTest
	{
		public void TestCBDPYTransactions()
		{
			SetupCBDPYTransactions();

			var sqlQuery = "SELECT * FROM CBDPYPostingMovements(@HeaderPKs)";
			var command = Db.Connection.Command(sqlQuery);
			command.AddTableValuedParameter("@HeaderPKs", "dbo.TVP_uniqueidentifier", new Guid[] { headerPK });

			var result = DataUtils.GetDataTableFromCommand(command);
			MovementHelper.AssertJournalAmount(result, 0M);
			MovementHelper.AssertJournalAmount(result, 110M, MovementHelper.GLAccountPK1);
			MovementHelper.AssertJournalAmount(result, -100M, MovementHelper.GLAccountPK2);
			MovementHelper.AssertJournalAmount(result, -10M, MovementHelper.GSTInputAccountPK);

			MovementHelper.AssertAggregationAmount(result);
		}

		public void SetupCBDPYTransactions()
		{
			MovementHelper.Setup();

			var helper = new TestDbHelper(TestConnection);
			var bankAccountPK = helper.InsertBankAccount("ZBA", MovementHelper.GLAccountPK1);

			headerPK = helper.InsertTransactionHeader("CB", "DPY", "001", 110, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, bankAccountPK, postToGL: false);
			helper.InsertTransactionLine(headerPK, null, null, MovementHelper.GLAccountPK2, MovementHelper.BranchPK, MovementHelper.DepartmentPK, null, 100, "DPY", MovementHelper.PostDate, null, 10);
		}

		public void TestCBDPYTransactionsWithRecoverableTax()
		{
			SetupCBDPYTransactionsWithRecoverableTax();

			var sqlQuery = "SELECT * FROM CBDPYPostingMovements(@HeaderPKs)";
			var command = Db.Connection.Command(sqlQuery);
			command.AddTableValuedParameter("@HeaderPKs", "dbo.TVP_uniqueidentifier", new Guid[] { headerPK });

			var result = DataUtils.GetDataTableFromCommand(command);
			MovementHelper.AssertJournalAmount(result, 0M);
			MovementHelper.AssertJournalAmount(result, 110M, MovementHelper.GLAccountPK1);
			MovementHelper.AssertJournalAmount(result, -104M, MovementHelper.GLAccountPK2);
			MovementHelper.AssertJournalAmount(result, -6M, MovementHelper.GSTInputAccountPK);

			MovementHelper.AssertAggregationAmount(result);
		}

		public void SetupCBDPYTransactionsWithRecoverableTax()
		{
			MovementHelper.Setup();

			var helper = new TestDbHelper(TestConnection);
			var bankAccountPK = helper.InsertBankAccount("ZBA", MovementHelper.GLAccountPK1);

			headerPK = helper.InsertTransactionHeader("CB", "DPY", "001", 110, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, bankAccountPK, postToGL: false);
			helper.InsertTransactionLine(headerPK, null, null, MovementHelper.GLAccountPK2, MovementHelper.BranchPK, MovementHelper.DepartmentPK, null, 100, "DPY", MovementHelper.PostDate, null, 10, 0.6M);
		}

		Guid headerPK;

		AccountingMovementsTestHelper MovementHelper => movementHelper ?? (movementHelper = new AccountingMovementsTestHelper(TestConnection));
		AccountingMovementsTestHelper movementHelper;
	}
}

