using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(AccGLAggregateWithBranchDept))]
	class AccGLAggregateWithBranchDeptTest : DbCreateScriptTest
	{
		public void TestTransactionCategoryGroup()
		{
			var helper = new TestDbHelper(TestConnection);

			var categotyGroups = TestHelper.CreateGLAggregate(helper, "A", 26, 100, 202001, helper.GLAccountPK1, helper.DefaultDepartmentPK, helper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);

			var sqlText = $"SELECT * FROM AccGLAggregateWithBranchDept(202001,'{TestDbHelper.DefaultCompanyPK}','','','{"{0}"}')";

			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, "A00"));
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals(helper.GLAccountPK1, result.Rows[0]["AA_AG"]);
			AssertEquals(helper.DefaultBranchPK, result.Rows[0]["AA_GB"]);
			AssertEquals(helper.DefaultDepartmentPK, result.Rows[0]["AA_GE"]);
			AssertEquals(100m, result.Rows[0]["AA_Amount"]);
			AssertEquals(202001, result.Rows[0]["AA_Period"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, categotyGroups));
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals(helper.GLAccountPK1, result.Rows[0]["AA_AG"]);
			AssertEquals(helper.DefaultBranchPK, result.Rows[0]["AA_GB"]);
			AssertEquals(helper.DefaultDepartmentPK, result.Rows[0]["AA_GE"]);
			AssertEquals(2500m, result.Rows[0]["AA_Amount"]);
			AssertEquals(202001, result.Rows[0]["AA_Period"]);
		}
	}
}

