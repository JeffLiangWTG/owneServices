using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.AccountingMovements;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.AccountingMovements.Testing
{
	[TestedType(typeof(CBTRFPostingMovements))]
	class CBTRFPostingMovementsTest : DbCreateScriptTest
	{
		public void TestCBTRFTransactions()
		{
			SetupCBTRFTransactions(invoiceAmount: 100, osAmount: 100, exchangeRate: 1, currency: "AUD");

			var sqlQuery = "SELECT * FROM CBTRFPostingMovements(@HeaderPKs)";
			var command = Db.Connection.Command(sqlQuery);
			command.AddTableValuedParameter("@HeaderPKs", "dbo.TVP_uniqueidentifier", new Guid[] { headerPK1, headerPK2 });

			var result = DataUtils.GetDataTableFromCommand(command);
			MovementHelper.AssertJournalAmount(result, 0M);
			MovementHelper.AssertJournalAmount(result, 100M, MovementHelper.GLAccountPK1);
			MovementHelper.AssertJournalAmount(result, -100M, MovementHelper.GLAccountPK2);

			MovementHelper.AssertAggregationAmount(result);
		}

		public void TestCBTRFTransactionsWithHighPrecisionExchangeRate()
		{
			SetupCBTRFTransactions(invoiceAmount: 4045.93M, osAmount: 31757.72M, exchangeRate: 0.1274M, currency: "USD");
			MovementHelper.SetCompanyReciprocal(TestDbHelper.DefaultCompanyPK, isReciprocal: true);

			var sqlQuery = "SELECT * FROM CBTRFPostingMovements(@HeaderPKs)";
			var command = Db.Connection.Command(sqlQuery);
			command.AddTableValuedParameter("@HeaderPKs", "dbo.TVP_uniqueidentifier", new Guid[] { headerPK1, headerPK2 });

			var result = DataUtils.GetDataTableFromCommand(command);
			MovementHelper.AssertJournalAmount(result, 0M);
			MovementHelper.AssertJournalAmount(result, 4045.93M, MovementHelper.GLAccountPK1);
			MovementHelper.AssertJournalAmount(result, -4045.93M, MovementHelper.GLAccountPK2);
			MovementHelper.AssertOSJournalAmount(result, 31757.72M, MovementHelper.GLAccountPK1);
			MovementHelper.AssertOSJournalAmount(result, -31757.72M, MovementHelper.GLAccountPK2);

			MovementHelper.AssertAggregationAmount(result);
		}

		void SetupCBTRFTransactions(decimal invoiceAmount, decimal osAmount, decimal exchangeRate, string currency)
		{
			MovementHelper.Setup();

			var helper = new TestDbHelper(TestConnection);
			var bankAccountPK1 = helper.InsertBankAccount("ZBA", MovementHelper.GLAccountPK1);
			var bankAccountPK2 = helper.InsertBankAccount("ZBB", MovementHelper.GLAccountPK2);

			headerPK1 = helper.InsertTransactionHeader("CB", "TRF", "001", invoiceAmount, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, bankAccountPK1, count: 1, postToGL: false, osAmount: osAmount, exchangeRate: exchangeRate, currency: currency);
			headerPK2 = helper.InsertTransactionHeader("CB", "TRF", "001", -invoiceAmount, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, bankAccountPK2, count: 2, postToGL: false, osAmount: -osAmount, exchangeRate: exchangeRate, currency: currency);
		}

		Guid headerPK1, headerPK2;

		AccountingMovementsTestHelper MovementHelper => movementHelper ?? (movementHelper = new AccountingMovementsTestHelper(TestConnection));
		AccountingMovementsTestHelper movementHelper;
	}
}

