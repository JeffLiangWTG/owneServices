using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.AccountingMovements;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.AccountingMovements.Testing
{
	[TestedType(typeof(ARAPEXXOVPDSCJNLPostingMovements))]
	class ARAPEXXOVPDSCJNLPostingMovementsTest : DbCreateScriptTest
	{
		#region ARAP EXX

		public void TestARAPEXXTransactions()
		{
			SetupARAPEXXTransactions();

			var sqlQuery = "SELECT * FROM ARAPEXXOVPDSCJNLPostingMovements(@HeaderPKs)";
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

		void SetupARAPEXXTransactions()
		{
			MovementHelper.Setup();

			var helper = new TestDbHelper(TestConnection);

			headerPK1 = helper.InsertTransactionHeader("AP", "EXX", "001", 100, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, glAccountPK: MovementHelper.GLAccountPK1, postToGL: false);
			headerPK2 = helper.InsertTransactionHeader("AR", "EXX", "002", 200, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, glAccountPK: MovementHelper.GLAccountPK2, postToGL: false);
		}

		#endregion

		#region ARAP OVP

		public void TestARAPOVPTransactions()
		{
			SetupARAPOVPTransactions();

			var sqlQuery = "SELECT * FROM ARAPEXXOVPDSCJNLPostingMovements(@HeaderPKs)";
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

		void SetupARAPOVPTransactions()
		{
			MovementHelper.Setup();

			var helper = new TestDbHelper(TestConnection);

			headerPK1 = helper.InsertTransactionHeader("AP", "OVP", "001", 100, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, glAccountPK: MovementHelper.GLAccountPK1, postToGL: false);
			headerPK2 = helper.InsertTransactionHeader("AR", "OVP", "002", 200, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, glAccountPK: MovementHelper.GLAccountPK2, postToGL: false);
		}

		#endregion

		#region ARAP DSC

		public void TestARAPDSCTransactions()
		{
			SetupARAPDSCTransactions();

			var sqlQuery = "SELECT * FROM ARAPEXXOVPDSCJNLPostingMovements(@HeaderPKs)";
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

		void SetupARAPDSCTransactions()
		{
			MovementHelper.Setup();

			var helper = new TestDbHelper(TestConnection);

			headerPK1 = helper.InsertTransactionHeader("AP", "DSC", "001", 100, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, glAccountPK: MovementHelper.GLAccountPK1, postToGL: false);
			headerPK2 = helper.InsertTransactionHeader("AR", "DSC", "002", 200, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, glAccountPK: MovementHelper.GLAccountPK2, postToGL: false);
		}

		#endregion

		#region ARAP JNL

		public void TestARAPJNLTransactions()
		{
			SetupARAPJNLTransactions(100, 100, 200, 200, exchangeRate: 1m, currency: "AUD");

			var sqlQuery = "SELECT * FROM ARAPEXXOVPDSCJNLPostingMovements(@HeaderPKs)";
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

		public void TestARAPJNTransactionsWithHighPrecisionExchangeRate()
		{
			SetupARAPJNLTransactions(4045.93M, 31757.72M, 4045.93M, 31757.72M, exchangeRate: 0.1274M, currency: "USD"); //31757.72 * 0.1274 = 4045.93
			MovementHelper.SetCompanyReciprocal(TestDbHelper.DefaultCompanyPK, isReciprocal: true);

			var sqlQuery = "SELECT * FROM ARAPEXXOVPDSCJNLPostingMovements(@HeaderPKs)";
			var command = Db.Connection.Command(sqlQuery);
			command.AddTableValuedParameter("@HeaderPKs", "dbo.TVP_uniqueidentifier", new Guid[] { headerPK1, headerPK2 });

			var result = DataUtils.GetDataTableFromCommand(command);
			MovementHelper.AssertJournalAmount(result, 0M);
			MovementHelper.AssertJournalAmount(result, -4045.93M, MovementHelper.GLAccountPK1);
			MovementHelper.AssertJournalAmount(result, -4045.93M, MovementHelper.GLAccountPK2);
			MovementHelper.AssertJournalAmount(result, 4045.93M, MovementHelper.APControlAccountPK);
			MovementHelper.AssertJournalAmount(result, 4045.93M, MovementHelper.ARControlAccountPK);

			MovementHelper.AssertOSJournalAmount(result, -31757.72M, MovementHelper.GLAccountPK1);
			MovementHelper.AssertOSJournalAmount(result, -31757.72M, MovementHelper.GLAccountPK2);
			MovementHelper.AssertOSJournalAmount(result, 31757.72M, MovementHelper.APControlAccountPK);
			MovementHelper.AssertOSJournalAmount(result, 31757.72M, MovementHelper.ARControlAccountPK);

			MovementHelper.AssertAggregationAmount(result);
		}

		void SetupARAPJNLTransactions(decimal invoiceAmount1, decimal osAmount1, decimal invoiceAmount2, decimal osAmount2, decimal exchangeRate, string currency)
		{
			MovementHelper.Setup();

			var helper = new TestDbHelper(TestConnection);

			headerPK1 = helper.InsertTransactionHeader("AP", "JNL", "001", invoiceAmount1, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, glAccountPK: MovementHelper.GLAccountPK1, postToGL: false, osAmount: osAmount1, exchangeRate: exchangeRate, currency: currency);
			headerPK2 = helper.InsertTransactionHeader("AR", "JNL", "002", invoiceAmount2, MovementHelper.PostDate, MovementHelper.BranchPK, MovementHelper.DepartmentPK, glAccountPK: MovementHelper.GLAccountPK2, postToGL: false, osAmount: osAmount2, exchangeRate: exchangeRate, currency: currency);
		}

		#endregion

		Guid headerPK1, headerPK2;

		AccountingMovementsTestHelper MovementHelper => movementHelper ?? (movementHelper = new AccountingMovementsTestHelper(TestConnection));
		AccountingMovementsTestHelper movementHelper;
	}
}

