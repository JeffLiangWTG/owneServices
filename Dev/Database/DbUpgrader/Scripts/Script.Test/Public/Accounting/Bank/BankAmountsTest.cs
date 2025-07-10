using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Bank;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Bank
{
	[TestedType(typeof(BankAmounts))]
	class BankAmountsTest : DbCreateScriptTest
	{
		[TestDate(2019, 07, 12)]
		public void TestView()
		{
			var helper = new TestDbHelper(TestConnection);
			var currentBranch = helper.InsertBranch("CBH", TestDbHelper.DefaultCompanyPK);
			var currentDep = helper.InsertDepartment("CDP");

			var bankAccountPK = helper.InsertBankAccount("BANKCDE", GetFirstGLAccount());
			helper.InsertTransactionHeader("AP", "PAY", "00005013", -100m, DateTime.Today, currentBranch, currentDep, bankAccountPK);

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection,
				$"select * from dbo.BankAmounts");
			AssertEquals("Result should have one row", 1, result.Rows.Count);

			AssertEquals("BankCode", bankAccountPK, result.Rows[0]["BankPK"]);
			AssertEquals("PostDate", DateTime.Today, result.Rows[0]["PostDate"]);
			AssertEquals("OSAmount", 100m, result.Rows[0]["OSAmount"]);
			AssertEquals("LocalAmount", 100m, result.Rows[0]["LocalAmount"]);
			AssertEquals("Branch", currentBranch, result.Rows[0]["BranchPK"]);
		}

		Guid GetFirstGLAccount()
		{
			string sql = "SELECT TOP 1 AG_PK FROM dbo.AccGLHeader";
			return (Guid)TestConnection.ExecuteScalar(sql);
		}
	}
}

