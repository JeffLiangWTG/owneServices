using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(ProfitAndLossWithDepartmentDep))]
	class ProfitAndLossWithDepartmentDepTest : DbCreateScriptTest
	{
		public void TestTransactionCategoryGroup()
		{
			var helper = new TestDbHelper(TestConnection);

			var categotyGroups = TestHelper.CreateGLAggregate(helper, "A", 26, 100, 202001, helper.GLAccountPK1, helper.DefaultDepartmentPK, helper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);

			var sqlText = $"SELECT * FROM ProfitAndLossWithDepartmentDep(202001,'{TestDbHelper.DefaultCompanyPK}','','','{"{0}"}')";

			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, "A00"));
			AssertEquals("Result should have 2 row", 2, result.Rows.Count);

			var row = FirstOrDefault(result, "Department", DBNull.Value);
			AssertEquals(0m, row["CurrentPeriodRevenue"]);
			AssertEquals(0m, row["CurrentPeriodCost"]);

			row = FirstOrDefault(result, "Department", helper.DefaultDepartmentPK);
			AssertEquals(0m, row["CurrentPeriodRevenue"]);
			AssertEquals(-100m, row["CurrentPeriodCost"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, categotyGroups));
			AssertEquals("Result should have 2 row", 2, result.Rows.Count);

			row = FirstOrDefault(result, "Department", DBNull.Value);
			AssertEquals(0m, row["CurrentPeriodRevenue"]);
			AssertEquals(0m, row["CurrentPeriodCost"]);

			row = FirstOrDefault(result, "Department", helper.DefaultDepartmentPK);
			AssertEquals(0m, row["CurrentPeriodRevenue"]);
			AssertEquals(-2500m, row["CurrentPeriodCost"]);
		}
	}
}

