using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(BalanceSheetAndProfitAndLossBase))]
	class BalanceSheetAndProfitAndLossBaseTest : DbCreateScriptTest
	{
		public void TestTransactionCategoryGroup()
		{
			var helper = new TestDbHelper(TestConnection);

			var categotyGroups = TestHelper.CreateGLAggregate(helper, "A", 26, 100, 202001, helper.GLAccountPK1, helper.DefaultDepartmentPK, helper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);

			var sqlText = $"SELECT * FROM BalanceSheetAndProfitAndLossBase(202001,'{TestDbHelper.DefaultCompanyPK}','','','Y','','','PNL','','DAU','','','0','','','{"{0}"}','')";

			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, "A00"));
			var rows = result.Select($"AccountPK = '{helper.GLAccountPK1}'");
			AssertEquals("Result should have one row", 1, rows.Length);
			AssertEquals("111.222.01", rows[0]["AccountNumber"]);
			AssertEquals("TestGLAccount 1", rows[0]["AccountName"]);
			AssertEquals("P&L", rows[0]["AccountType"]);
			AssertEquals(-100m, rows[0]["CurrentPeriod"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, categotyGroups));
			rows = result.Select($"AccountPK = '{helper.GLAccountPK1}'");
			AssertEquals("Result should have one row", 1, rows.Length);
			AssertEquals("111.222.01", rows[0]["AccountNumber"]);
			AssertEquals("TestGLAccount 1", rows[0]["AccountName"]);
			AssertEquals("P&L", rows[0]["AccountType"]);
			AssertEquals(-2500m, rows[0]["CurrentPeriod"]);
		}
	}
}

