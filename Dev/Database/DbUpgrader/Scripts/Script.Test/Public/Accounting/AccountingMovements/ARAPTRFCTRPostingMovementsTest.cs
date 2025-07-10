using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.AccountingMovements;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.AccountingMovements.Testing
{
	[TestedType(typeof(ARAPTRFCTRPostingMovements))]
	class ARAPTRFCTRPostingMovementsTest : DbCreateScriptTest
	{
		#region TRF

		public void TestARTRFTransactions()
		{
			SetupARTRFTransactions();

			var sqlQuery = "SELECT * FROM ARAPTRFCTRPostingMovements(@HeaderPKs)";
			var command = Db.Connection.Command(sqlQuery);
			command.AddTableValuedParameter("@HeaderPKs", "dbo.TVP_uniqueidentifier", new Guid[] { headerPK1, headerPK2 });

			var result = DataUtils.GetDataTableFromCommand(command);
			MovementHelper.AssertJournalAmount(result, 0M);
			MovementHelper.AssertContainJournal(result, 100M, MovementHelper.ARControlAccountPK);
			MovementHelper.AssertContainJournal(result, -100M, MovementHelper.ARControlAccountPK);
			MovementHelper.AssertAggregationAmount(result, isEmptyAggregation: true);
		}

		void SetupARTRFTransactions()
		{
			MovementHelper.Setup();

			var helper = new TestDbHelper(TestConnection);
			headerPK1 = helper.InsertTransactionHeader("AR", "TRF", "001", 100, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, count: 1, postToGL: false);
			headerPK2 = helper.InsertTransactionHeader("AR", "TRF", "001", -100, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, count: 2, postToGL: false);
		}

		public void TestAPTRFTransactions()
		{
			SetupAPTRFTransactions(100, 100, 1, "AUD");

			var sqlQuery = "SELECT * FROM ARAPTRFCTRPostingMovements(@HeaderPKs)";
			var command = Db.Connection.Command(sqlQuery);
			command.AddTableValuedParameter("@HeaderPKs", "dbo.TVP_uniqueidentifier", new Guid[] { headerPK1, headerPK2 });

			var result = DataUtils.GetDataTableFromCommand(command);
			MovementHelper.AssertJournalAmount(result, 0M);
			MovementHelper.AssertContainJournal(result, 100M, MovementHelper.APControlAccountPK);
			MovementHelper.AssertContainJournal(result, -100M, MovementHelper.APControlAccountPK);
			MovementHelper.AssertAggregationAmount(result, isEmptyAggregation: true);
		}

		void SetupAPTRFTransactions(decimal invoiceAmount, decimal osAmount, decimal exchangeRate, string currency)
		{
			MovementHelper.Setup();

			var helper = new TestDbHelper(TestConnection);
			headerPK1 = helper.InsertTransactionHeader("AP", "TRF", "001", invoiceAmount, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, count: 1, postToGL: false, osAmount: osAmount, exchangeRate: exchangeRate, currency: currency);
			headerPK2 = helper.InsertTransactionHeader("AP", "TRF", "001", -invoiceAmount, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, count: 2, postToGL: false, osAmount: -osAmount, exchangeRate: exchangeRate, currency: currency);
		}

		public void TestARAPTRFTransactionsWithHighPrecisionExchangeRate()
		{
			SetupAPTRFTransactions(4045.93M, 31757.72M, 0.1274M, "USD");
			MovementHelper.SetCompanyReciprocal(TestDbHelper.DefaultCompanyPK, isReciprocal: true);

			var sqlQuery = "SELECT * FROM ARAPTRFCTRPostingMovements(@HeaderPKs)";
			var command = Db.Connection.Command(sqlQuery);
			command.AddTableValuedParameter("@HeaderPKs", "dbo.TVP_uniqueidentifier", new Guid[] { headerPK1, headerPK2 });

			var result = DataUtils.GetDataTableFromCommand(command);
			MovementHelper.AssertJournalAmount(result, 0M);
			MovementHelper.AssertContainJournal(result, 4045.93M, MovementHelper.APControlAccountPK);
			MovementHelper.AssertContainJournal(result, -4045.93M, MovementHelper.APControlAccountPK);
			MovementHelper.AssertContainOSJournal(result, 31757.72M, MovementHelper.APControlAccountPK);
			MovementHelper.AssertContainOSJournal(result, -31757.72M, MovementHelper.APControlAccountPK);

			MovementHelper.AssertAggregationAmount(result, isEmptyAggregation: true);
		}

		#endregion

		#region CTR

		public void TestARAPCTRTransactions()
		{
			SetupARAPCTRTransactions(100, 100, 1, "AUD");

			var sqlQuery = "SELECT * FROM ARAPTRFCTRPostingMovements(@HeaderPKs)";
			var command = Db.Connection.Command(sqlQuery);
			command.AddTableValuedParameter("@HeaderPKs", "dbo.TVP_uniqueidentifier", new Guid[] { headerPK1, headerPK2 });

			var result = DataUtils.GetDataTableFromCommand(command);
			MovementHelper.AssertJournalAmount(result, 0M);
			MovementHelper.AssertJournalAmount(result, 100M, MovementHelper.APControlAccountPK);
			MovementHelper.AssertJournalAmount(result, -100M, MovementHelper.ARControlAccountPK);

			MovementHelper.AssertAggregationAmount(result);
		}

		public void TestARAPCTRTransactionsWithHighPrecisionExchangeRate()
		{
			SetupARAPCTRTransactions(4045.93M, 31757.72M, 0.1274M, "USD"); //31757.72 * 0.1274 = 4045.93
			MovementHelper.SetCompanyReciprocal(TestDbHelper.DefaultCompanyPK, isReciprocal: true);

			var sqlQuery = "SELECT * FROM ARAPTRFCTRPostingMovements(@HeaderPKs)";
			var command = Db.Connection.Command(sqlQuery);
			command.AddTableValuedParameter("@HeaderPKs", "dbo.TVP_uniqueidentifier", new Guid[] { headerPK1, headerPK2 });

			var result = DataUtils.GetDataTableFromCommand(command);
			MovementHelper.AssertJournalAmount(result, 0M);
			MovementHelper.AssertJournalAmount(result, 4045.93M, MovementHelper.APControlAccountPK);
			MovementHelper.AssertJournalAmount(result, -4045.93M, MovementHelper.ARControlAccountPK);
			MovementHelper.AssertOSJournalAmount(result, 31757.72M, MovementHelper.APControlAccountPK);
			MovementHelper.AssertOSJournalAmount(result, -31757.72M, MovementHelper.ARControlAccountPK);

			MovementHelper.AssertAggregationAmount(result);
		}

		void SetupARAPCTRTransactions(decimal invoiceAmount, decimal osAmount, decimal exchangeRate, string currency)
		{
			MovementHelper.Setup();

			var helper = new TestDbHelper(TestConnection);

			headerPK1 = helper.InsertTransactionHeader("AP", "CTR", "001", invoiceAmount, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, count: 1, postToGL: false, osAmount: osAmount, exchangeRate: exchangeRate, currency: currency);
			headerPK2 = helper.InsertTransactionHeader("AR", "CTR", "001", -invoiceAmount, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, count: 2, postToGL: false, osAmount: -osAmount, exchangeRate: exchangeRate, currency: currency);
		}

		#endregion

		Guid headerPK1, headerPK2;

		AccountingMovementsTestHelper MovementHelper => movementHelper ?? (movementHelper = new AccountingMovementsTestHelper(TestConnection));
		AccountingMovementsTestHelper movementHelper;
	}
}

