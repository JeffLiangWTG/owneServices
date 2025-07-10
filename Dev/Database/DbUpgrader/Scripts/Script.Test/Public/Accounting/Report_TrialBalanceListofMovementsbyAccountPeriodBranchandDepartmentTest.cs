using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(Report_TrialBalanceListofMovementsbyAccountPeriodBranchandDepartment))]
	class Report_TrialBalanceListofMovementsbyAccountPeriodBranchandDepartmentTest : DbCreateScriptTest
	{
		public void TestTransactionCategoryGroup()
		{
			var helper = new TestDbHelper(TestConnection);
			int categoryCount = 26;
			var categotyGroups = TestHelper.CreateGLAggregate(helper, "A", categoryCount, 100, 202001, helper.GLAccountPK1, helper.DefaultDepartmentPK, helper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);

			var sqlText = $"SELECT * FROM Report_TrialBalanceListofMovementsbyAccountPeriodBranchandDepartment('{TestDbHelper.DefaultCompanyPK}',202001,202001,'{helper.DefaultBranchPK}','{helper.DefaultDepartmentPK}','{"{0}"}')";

			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, "A00"));
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertDateRow(result.Select("TransactionCategory = 'A00'")[0]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, categotyGroups));
			AssertEquals("Result should have one row", 25, result.Rows.Count);
			for (int i = 0; i < categoryCount - 1; i++)
			{
				var category = $"A{i.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')}";
				AssertDateRow(result.Select($"TransactionCategory = '{category}'")[0]);
			}

			void AssertDateRow(DataRow row)
			{
				AssertEquals("EDI", row["Company"]);
				AssertEquals("P&L", row["Type"]);
				AssertEquals("111.222.01", row["Account"]);
				AssertEquals("TestGLAccount 1", row["AccDescription"]);
				AssertEquals("ZZB", row["Br"]);
				AssertEquals(helper.DefaultBranchPK, row["BrPK"]);
				AssertEquals("ZZD", row["Dept"]);
				AssertEquals(helper.DefaultDepartmentPK, row["DeptPK"]);
				AssertEquals("Miscellaneous", row["Activity"]);
				AssertEquals("Other", row["Direction"]);
				AssertEquals("Other", row["Mode"]);
				AssertEquals("2020", row["PeriodYear"]);
				AssertEquals(202001, row["Period"]);
				AssertEquals(100m, row["PeriodAmount"]);
				AssertEquals("AUD", row["Currency"]);
			}
		}

		public void TestContainsUnits()
		{
			var testDbHelper = new TestDbHelper(TestConnection);
			var companyPK = testDbHelper.InsertCompany("TST", "CHA", "CNY", "CA", false, false);
			var branchPK = testDbHelper.InsertBranch("CCB", companyPK);
			var departmentPK = testDbHelper.InsertDepartment("CCD");
			var plAccountPK = testDbHelper.InsertGLAccount("PLAcc", "Test PLAccount");
			var noteAccountPK = testDbHelper.InsertGLAccount("NoteAcc", "Test NoteAccount", "NTE", "KWH");

			testDbHelper.InsertGLAggregate(110, "", 202004, plAccountPK, branchPK, departmentPK, companyPK);
			testDbHelper.InsertGLAggregate(120, "", 202004, plAccountPK, branchPK, departmentPK, companyPK);
			testDbHelper.InsertGLAggregate(130, "", 202004, noteAccountPK, branchPK, departmentPK, companyPK);
			var sql = $@"
						Select * from Report_TrialBalanceListofMovementsbyAccountPeriodBranchandDepartment(
						'{companyPK}',
						202004,
						202004,
						'',
						'',
						''
						)";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals(2, result.Rows.Count);
			var rowselected = (from DataRow row in result.Rows where row["Account"].ToString().StartsWith("PLAcc") select row).FirstOrDefault();
			AssertEquals("", rowselected["Units"].ToString());
			AssertEquals(230M, (decimal)rowselected["PeriodAmount"]);

			rowselected = (from DataRow row in result.Rows where row["Account"].ToString().StartsWith("NoteAcc") select row).FirstOrDefault();
			AssertEquals("KWH", rowselected["Units"].ToString());
			AssertEquals(130M, (decimal)rowselected["PeriodAmount"]);
		}
	}
}

