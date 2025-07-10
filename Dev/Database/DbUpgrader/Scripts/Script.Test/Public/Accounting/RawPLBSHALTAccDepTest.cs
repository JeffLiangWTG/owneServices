using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(RawPLBSHALTAccDep))]
	class RawPLBSHALTAccDepTest : DbCreateScriptTest
	{
		public void TestTransactionCategoryGroup()
		{
			var helper = new TestDbHelper(TestConnection);

			var categotyGroups = TestHelper.CreateGLAggregate(helper, "A", 26, 100, 202001, helper.GLAccountPK1, helper.DefaultDepartmentPK, helper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);

			var sqlText = $"SELECT * FROM RawPLBSHALTAccDep(202001,202001,202001,'{TestDbHelper.DefaultCompanyPK}','','{"{0}"}')";

			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, "A00"));
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			var row = FirstOrDefault(result, "GE_PK1", helper.DefaultDepartmentPK);
			AssertEquals(helper.GLAccountPK1, row["AG_PK"]);
			AssertEquals("111.222.01", row["AG_AccountNum"]);
			AssertEquals(100m, row["Amount"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, categotyGroups));
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			row = FirstOrDefault(result, "GE_PK1", helper.DefaultDepartmentPK);
			AssertEquals(helper.GLAccountPK1, row["AG_PK"]);
			AssertEquals("111.222.01", row["AG_AccountNum"]);
			AssertEquals(2500m, row["Amount"]);
		}
	}
}

