using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(AccMultilingualTrialBalance))]
	class AccMultilingualTrialBalanceTest : DbCreateScriptTest
	{
		public void TestContainUnits()
		{
			var testDBHelper = new TestDbHelper(TestConnection);
			var accountPK = TestHelper.InsertGLHeader("2201.02.01", "Test Account 1", "P&L", "");
			var accountPK1 = TestHelper.InsertGLHeader("2202.02.01", "Test Account 2", "P&L", "");
			var accountPK2 = TestHelper.InsertGLHeader("2203.02.01", "Test Account 3", "P&L", "");
			var accountPK3 = TestHelper.InsertGLHeader("2204.02.01", "Test Account 4", "P&L", "KWH");

			TestHelper.CreateGLAggregate(testDBHelper, "", 5, 100, 201201, accountPK, testDBHelper.DefaultDepartmentPK, testDBHelper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);
			TestHelper.CreateGLAggregate(testDBHelper, "", 6, 100, 201201, accountPK1, testDBHelper.DefaultDepartmentPK, testDBHelper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);

			TestHelper.InsertGLMappingAccount("", "2201.02.01", accountPK2, "Test", "COA", "");
			TestHelper.InsertGLMappingAccount("", "2202.02.01", accountPK3, "Test", "COA", "");

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM AccMultilingualTrialBalance(201201, '{TestDbHelper.DefaultCompanyPK}', '', '')");
			AssertEquals("Result should have 2 rows", 2, result.Rows.Count);

			DataRow rowSelected = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("2203") select row).FirstOrDefault();
			AssertEquals("Units field", "", rowSelected["Units"].ToString());
			rowSelected = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("2204") select row).FirstOrDefault();
			AssertEquals("Units field", "KWH", rowSelected["Units"].ToString());
		}

		public void TestContainNewColumnsWhichUseForCalculateTotalAmount()
		{
			var testDBHelper = new TestDbHelper(Db.Connection);
			var accountPK = TestHelper.InsertGLHeader("2201.02.01", "Test Account 1", "NTE", "");
			var accountPK1 = TestHelper.InsertGLHeader("2202.02.01", "Test Account 2", "P&L", "");
			var accountPK2 = TestHelper.InsertGLHeader("2203.02.01", "Test Account 3", "NTE", "");
			var accountPK3 = TestHelper.InsertGLHeader("2204.02.01", "Test Account 4", "P&L", "KWH");

			TestHelper.CreateGLAggregate(testDBHelper, "", 5, 100, 201208, accountPK, testDBHelper.DefaultDepartmentPK, testDBHelper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);
			TestHelper.CreateGLAggregate(testDBHelper, "", 6, 100, 201208, accountPK1, testDBHelper.DefaultDepartmentPK, testDBHelper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);

			TestHelper.InsertGLMappingAccount("", "2201.02.01", accountPK2, "Test", "COA", "NTE", "DR");
			TestHelper.InsertGLMappingAccount("", "2202.02.01", accountPK3, "Test", "COA", "", "DR");

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM AccMultilingualTrialBalance(201208, '{TestDbHelper.DefaultCompanyPK}', '', '')");
			AssertEquals("Result should have 2 rows", 2, result.Rows.Count);

			DataRow rowSelected = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("2203") select row).FirstOrDefault();
			AssertEquals("AccountType field", "NTE", rowSelected["AccountType"].ToString());
			AssertEquals("CurrentDebitForCalculateTotal field", 0M, rowSelected["CurrentDebitForCalculateTotal"]);
			AssertEquals("CurrentCreditForCalculateTotal field", 0M, rowSelected["CurrentCreditForCalculateTotal"]);
			AssertEquals("ClosingDebitForCalculateTotal field", 0M, rowSelected["ClosingDebitForCalculateTotal"]);
			AssertEquals("ClosingCreditForCalculateTotal field", 0M, rowSelected["ClosingCreditForCalculateTotal"]);

			rowSelected = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("2204") select row).FirstOrDefault();
			AssertEquals("AccountType field", "P&L", rowSelected["AccountType"].ToString());
			AssertEquals("CurrentDebitForCalculateTotal field", DBNull.Value, rowSelected["CurrentDebitForCalculateTotal"]);
			AssertEquals("CurrentCreditForCalculateTotal field", DBNull.Value, rowSelected["CurrentCreditForCalculateTotal"]);
			AssertEquals("ClosingDebitForCalculateTotal field", DBNull.Value, rowSelected["ClosingDebitForCalculateTotal"]);
			AssertEquals("ClosingCreditForCalculateTotal field", DBNull.Value, rowSelected["ClosingCreditForCalculateTotal"]);
		}
	}
}

