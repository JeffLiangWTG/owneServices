using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.ProfitLossByBranch;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.ProfitLossByBranch
{
	[TestedType(typeof(RawPLBSHALTAccBranchRetainedEarnings))]
	class RawPLBSHALTAccBranchRetainedEarningsTest : DbCreateScriptTest
	{
		public void TestTransactionCategoryGroup()
		{
			var helper = new TestDbHelper(TestConnection);

			var categotyGroups = TestHelper.CreateGLAggregate(helper, "A", 26, 100, 202001, helper.GLAccountPK1, helper.DefaultDepartmentPK, helper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);

			var sqlText = $"SELECT * FROM RawPLBSHALTAccBranchRetainedEarnings(202001,202003,202003,'{TestDbHelper.DefaultCompanyPK}','','{"{0}"}')";

			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, "A00"));
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals("4900.00.00", result.Rows[0]["AccNum"]);
			AssertEquals(100m, result.Rows[0]["Amount"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, categotyGroups));
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals("4900.00.00", result.Rows[0]["AccNum"]);
			AssertEquals(2500m, result.Rows[0]["Amount"]);
		}
	}
}

