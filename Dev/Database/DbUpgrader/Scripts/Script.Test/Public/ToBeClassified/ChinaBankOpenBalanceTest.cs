using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.ToBeClassified;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.ToBeClassified
{
	[TestedType(typeof(ChinaBankOpenBalance))]
	class ChinaBankOpenBalanceTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			var fromDate = new DateTime(2015, 05, 31).ToString("yyyyMMdd");

			var helper = new TestDbHelper(TestConnection);
			var currentCompany = TestDbHelper.DefaultCompanyPK;
			var currentBranch = helper.InsertBranch("CBH", TestDbHelper.DefaultCompanyPK);
			var currentDep = helper.InsertDepartment("CDP");

			var glAccount1 = helper.InsertGLAccount("TEST1", "TEST1");
			var glAccount2 = helper.InsertGLAccount("TEST2", "TEST2");
			var nonCurrentBranch = helper.InsertBranch("OTH", currentCompany);

			var bankAccount1PK = helper.InsertBankAccount("BANK1", glAccount1, branchPK: currentBranch);
			var bankAccount2PK = helper.InsertBankAccount("BANK2", glAccount2, branchPK: nonCurrentBranch);

			helper.InsertTransactionHeader("AP", "PAY", "00005013", -100m, new DateTime(2015, 05, 25), currentBranch, currentDep, bankAccount1PK);
			helper.InsertTransactionHeader("AP", "PAY", "000050132", -500m, new DateTime(2015, 05, 25), currentBranch, currentDep, bankAccount2PK);
			helper.InsertTransactionHeader("AP", "PAY", "00005014", -100m, new DateTime(2015, 05, 25), nonCurrentBranch, currentDep, bankAccount1PK);
			helper.InsertTransactionHeader("AP", "PAY", "000050142", -500m, new DateTime(2015, 05, 25), nonCurrentBranch, currentDep, bankAccount2PK);

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, $"select * from ChinaBankOpenBalance('{bankAccount1PK}', '{fromDate}', '{currentBranch}') ORDER BY AB_Code");
			AssertEquals("Result should have one row", 2, result.Rows.Count);
			var resultForBank1 = result.Select("AB_Code = 'BANK1'")[0];
			var resultForBank2 = result.Select("AB_Code = 'BANK2'")[0];

			AssertEquals("BankCode", "BANK1", resultForBank1["AB_Code"]);
			AssertEquals("AccumulatedBankOSAmount", 100m, resultForBank1["AccumulatedBankOSAmount"]);
			AssertEquals("BankOpenBalance", 0m, resultForBank1["BankOpenBalance"]);
			AssertEquals("LocalBankOpenBalance", 0m, resultForBank1["LocalBankOpenBalance"]);
			AssertEquals("AB_GB", currentBranch, new Guid(resultForBank1["AB_GB"].ToString()));

			AssertEquals("BankCode", "BANK2", resultForBank2["AB_Code"]);
			AssertEquals("AccumulatedBankOSAmount", DBNull.Value, resultForBank2["AccumulatedBankOSAmount"]);
			AssertEquals("BankOpenBalance", 0m, resultForBank2["BankOpenBalance"]);
			AssertEquals("LocalBankOpenBalance", 0m, resultForBank2["LocalBankOpenBalance"]);
			AssertEquals("AB_GB", nonCurrentBranch, new Guid(resultForBank2["AB_GB"].ToString()));

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"select * from ChinaBankOpenBalance('{bankAccount2PK}', '{fromDate}', '{currentBranch}') ORDER BY AB_Code");
			AssertEquals("Result should have one row", 2, result.Rows.Count);
			resultForBank1 = result.Select("AB_Code = 'BANK1'")[0];
			resultForBank2 = result.Select("AB_Code = 'BANK2'")[0];

			AssertEquals("BankCode", "BANK1", resultForBank1["AB_Code"]);
			AssertEquals("AccumulatedBankOSAmount", DBNull.Value, resultForBank1["AccumulatedBankOSAmount"]);
			AssertEquals("BankOpenBalance", 0m, resultForBank1["BankOpenBalance"]);
			AssertEquals("LocalBankOpenBalance", 0m, resultForBank1["LocalBankOpenBalance"]);
			AssertEquals("AB_GB", currentBranch, new Guid(resultForBank1["AB_GB"].ToString()));

			AssertEquals("BankCode", "BANK2", resultForBank2["AB_Code"]);
			AssertEquals("AccumulatedBankOSAmount", 500m, resultForBank2["AccumulatedBankOSAmount"]);
			AssertEquals("BankOpenBalance", 0m, resultForBank2["BankOpenBalance"]);
			AssertEquals("LocalBankOpenBalance", 0m, resultForBank2["LocalBankOpenBalance"]);
			AssertEquals("AB_GB", nonCurrentBranch, new Guid(resultForBank2["AB_GB"].ToString()));

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"select * from ChinaBankOpenBalance('{bankAccount1PK}', '{fromDate}', null) ORDER BY AB_Code");
			AssertEquals("Result should have one row", 2, result.Rows.Count);
			resultForBank1 = result.Select("AB_Code = 'BANK1'")[0];
			resultForBank2 = result.Select("AB_Code = 'BANK2'")[0];

			AssertEquals("BankCode", "BANK1", resultForBank1["AB_Code"]);
			AssertEquals("AccumulatedBankOSAmount", 200m, resultForBank1["AccumulatedBankOSAmount"]);
			AssertEquals("BankOpenBalance", 0m, resultForBank1["BankOpenBalance"]);
			AssertEquals("LocalBankOpenBalance", 0m, resultForBank1["LocalBankOpenBalance"]);
			AssertEquals("AB_GB", currentBranch, new Guid(resultForBank1["AB_GB"].ToString()));

			AssertEquals("BankCode", "BANK2", resultForBank2["AB_Code"]);
			AssertEquals("AccumulatedBankOSAmount", DBNull.Value, resultForBank2["AccumulatedBankOSAmount"]);
			AssertEquals("BankOpenBalance", 0m, resultForBank2["BankOpenBalance"]);
			AssertEquals("LocalBankOpenBalance", 0m, resultForBank2["LocalBankOpenBalance"]);
			AssertEquals("AB_GB", nonCurrentBranch, new Guid(resultForBank2["AB_GB"].ToString()));

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"select * from ChinaBankOpenBalance('{bankAccount2PK}', '{fromDate}', null) ORDER BY AB_Code");
			AssertEquals("Result should have one row", 2, result.Rows.Count);
			resultForBank1 = result.Select("AB_Code = 'BANK1'")[0];
			resultForBank2 = result.Select("AB_Code = 'BANK2'")[0];

			AssertEquals("BankCode", "BANK1", resultForBank1["AB_Code"]);
			AssertEquals("AccumulatedBankOSAmount", DBNull.Value, resultForBank1["AccumulatedBankOSAmount"]);
			AssertEquals("BankOpenBalance", 0m, resultForBank1["BankOpenBalance"]);
			AssertEquals("LocalBankOpenBalance", 0m, resultForBank1["LocalBankOpenBalance"]);
			AssertEquals("AB_GB", currentBranch, new Guid(resultForBank1["AB_GB"].ToString()));

			AssertEquals("BankCode", "BANK2", resultForBank2["AB_Code"]);
			AssertEquals("AccumulatedBankOSAmount", 1000m, resultForBank2["AccumulatedBankOSAmount"]);
			AssertEquals("BankOpenBalance", 0m, resultForBank2["BankOpenBalance"]);
			AssertEquals("LocalBankOpenBalance", 0m, resultForBank2["LocalBankOpenBalance"]);
			AssertEquals("AB_GB", nonCurrentBranch, new Guid(resultForBank2["AB_GB"].ToString()));
		}
	}
}

