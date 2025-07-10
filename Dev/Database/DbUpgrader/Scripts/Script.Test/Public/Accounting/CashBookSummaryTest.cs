using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(CashBookSummary))]
	class CashBookSummaryTest : DbCreateScriptTest
	{
		public void TestDoNotOverflowWithBigSummedAmount()
		{
			var helper = new TestDbHelper(TestConnection);
			var currentBranch = helper.InsertBranch("CBH", TestDbHelper.DefaultCompanyPK);
			var currentDep = helper.InsertDepartment("CDP");
			helper.InsertAccPeriod(2018, 06, TestDbHelper.DefaultCompanyPK);

			var postDateTime = new DateTime(2018, 06, 25);

			var glAccount1 = helper.InsertGLAccount("TEST1", "TEST1");
			var bankAccountPK1 = helper.InsertBankAccount("ZBA", glAccount1);
			var headerPK1 = helper.InsertTransactionHeader("AR", "PAY", "001", 560000000000000M, postDateTime, currentBranch, currentDep, bankAccountPK1, postToGL: false);
			var headerPK2 = helper.InsertTransactionHeader("AR", "PAY", "002", 700000000000000M, postDateTime, currentBranch, currentDep, bankAccountPK1, postToGL: false);

			var sqlQuery = "SELECT * FROM CashBookSummary(@EndDate, @Company)";

			using (var cmd = Db.Connection.Command(sqlQuery))
			{
				cmd.AddParameter("@EndDate", System.Data.SqlDbType.DateTime, postDateTime);
				cmd.AddParameter("@Company", System.Data.SqlDbType.UniqueIdentifier, TestDbHelper.DefaultCompanyPK);

				DataTable result = null;

				AssertNoExceptionThrown(() => result = DataUtils.GetDataTableFromCommand(cmd));
				AssertNotNull(result);
				AssertEquals("Result should have one row", 1, result.Rows.Count);
				AssertEquals("OSBalance", -1260000000000000M, result.Rows[0]["OSBalance"]);
				AssertEquals("LocalBalance", -1260000000000000M, result.Rows[0]["LocalBalance"]);
			}
		}
	}
}

