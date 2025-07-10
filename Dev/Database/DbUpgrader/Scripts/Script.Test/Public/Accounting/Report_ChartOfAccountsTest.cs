using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(Report_ChartOfAccounts))]
	class Report_ChartOfAccountsTest : DbCreateScriptTest
	{
		public void TestReportAccounts()
		{
			CreateAccountGLHeaders("3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1", "6610.10.10", "TEST ACCOUNT ACTIVE", "HDR", "CR", "0", "AS", "1", "1");
			CreateAccountGLHeaders("363958D7-A610-4015-97FD-A6C15E701BD8", "6611.10.10", "TEST ACCOUNT NON-ACTIVE", "HDR", "CR", "0", "AS", "0", "1");

			TestReportOnlyActiveFlagUnset();
			TestReportOnlyActiveFlagSet();
		}

		public void TestContainsUnits()
		{
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccGLHeader (AG_PK, AG_AccountNum, AG_Description, AG_AccountType, AG_StatisticalUnits, AG_DebitCredit, AG_ControlAccount, AG_AG_HeaderDependsOnTotal, AG_TotalLevel, AG_Column) VALUES ('01E62CE1-24AE-414E-AE47-5F3FEE9D4F26', '2330.20.30', 'NTE Account 1', 'NTE', 'KWH', 'CR', 1, NULL, 0, 'TS')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccGLHeader (AG_PK, AG_AccountNum, AG_Description, AG_AccountType, AG_StatisticalUnits, AG_DebitCredit, AG_ControlAccount, AG_AG_HeaderDependsOnTotal, AG_TotalLevel, AG_Column) VALUES ('196C91D7-D9CC-480F-AFBA-47B33E75F935', '3330.20.30', 'NTE Account 2', 'NTE', 'TON', 'CR', 1, NULL, 0, 'TN')");

			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM  Report_ChartOfAccounts ('','{TestDbHelper.DefaultCompanyPK}','0','0')");
			var rowselected = result.Select("AccountNumber = '2330.20.30' AND Type = 'NTE'");
			AssertEquals("Should only find 1 account that matches criteria", 1, rowselected.Length);
			AssertEquals("KWH", rowselected[0]["Units"].ToString());

			rowselected = result.Select("AccountNumber = '3330.20.30' AND Type = 'NTE'");
			AssertEquals("Should only find 1 account that matches criteria", 1, rowselected.Length);
			AssertEquals("TON", rowselected[0]["Units"].ToString());

			var rows = from DataRow row in result.Rows where row["Type"].ToString() != "NTE" select row;
			foreach (var item in rows)
			{
				AssertEquals("", item["Units"].ToString());
			}
		}

		public void TestContainMultiSubAccounts()
		{
			var testDBHelper = new TestDbHelper(Db.Connection);
			var accountPK1 = testDBHelper.InsertGLAccount("2221.00.00", "Test account 1");
			var accountPK2 = testDBHelper.InsertGLAccount("2222.00.00", "Test account 2");
			var accountPK3 = testDBHelper.InsertGLAccount("2223.00.00", "Test account 3");
			testDBHelper.InsertAccGLHeaderSubAccount(accountPK2, true, OrgHeaderSchema.Constants.Prefix);
			testDBHelper.InsertAccGLHeaderSubAccount(accountPK2, false, GlbGroupSchema.Constants.Prefix);
			testDBHelper.InsertAccGLHeaderSubAccount(accountPK3, true, OrgHeaderSchema.Constants.Prefix);
			testDBHelper.InsertAccGLHeaderSubAccount(accountPK3, false, GlbGroupSchema.Constants.Prefix);
			testDBHelper.InsertAccGLHeaderSubAccount(accountPK3, false, GlbStaffSchema.Constants.Prefix);
			testDBHelper.InsertAccGLHeaderSubAccount(accountPK3, false, AccGroupsSchema.Constants.Prefix);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_ChartOfAccounts ('','{TestDbHelper.DefaultCompanyPK}','0','0')");
			var rowselected = result.Select("AccountNumber = '2221.00.00'");
			AssertEquals("Should only find 1 account that matches criteria", 1, rowselected.Length);
			AssertEquals("", rowselected[0]["MultiSubAccountTypeCode"].ToString());

			rowselected = result.Select("AccountNumber = '2222.00.00'");
			AssertEquals("Should only find 1 account that matches criteria", 1, rowselected.Length);
			AssertEquals("ORG: Y, SGP: N", rowselected[0]["MultiSubAccountTypeCode"].ToString());

			rowselected = result.Select("AccountNumber = '2223.00.00'");
			AssertEquals("Should only find 1 account that matches criteria", 1, rowselected.Length);
			AssertEquals("ORG: Y, SEG: N, STR: N, SGP: N", rowselected[0]["MultiSubAccountTypeCode"].ToString());
		}

		void CreateAccountGLHeaders(string accountPK, string accountNum, string accountDesc, string accountType, string accountDebitCreditType, string accountTotalLevel, string accountColumn, string accountIsActive, string accountControlAccount)
		{
			TestConnection.ExecuteNonQuery(
			  "INSERT INTO dbo.AccGLHeader (AG_PK, AG_AccountNum, AG_Description, AG_AccountType, AG_DebitCredit, AG_ControlAccount, AG_TotalLevel, AG_Column, AG_IsActive, AG_SystemCreateTimeUtc, AG_SystemCreateUser, AG_SystemLastEditTimeUtc, AG_SystemLastEditUser) VALUES (" +
				"'" + accountPK + "', " +
				"'" + accountNum + "', " +
				"'" + accountDesc + "', " +
				"'" + accountType + "', " +
				"'" + accountDebitCreditType + "', " +
				"'" + accountControlAccount + "', " +
				"'" + accountTotalLevel + "', " +
				"'" + accountColumn + "', " +
				"'" + accountIsActive + "', GetUtcDate(), '~BP', GetUtcDate(), '~BP')"
			  );
		}

		void TestReportOnlyActiveFlagUnset()
		{
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_ChartOfAccounts('DAU', '{TestDbHelper.DefaultCompanyPK}', 0, 0)");
			DataRow[] rows;

			rows = result.Select("AccountNumber = '6610.10.10'");
			AssertEquals("Report should give back Active Accounts", rows[0]["AccountNumber"], "6610.10.10");

			rows = result.Select("AccountNumber = '6611.10.10'");
			AssertEquals("Report should give back Non-Active Accounts", rows[0]["AccountNumber"], "6611.10.10");
		}

		void TestReportOnlyActiveFlagSet()
		{
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_ChartOfAccounts('DAU', '{TestDbHelper.DefaultCompanyPK}', 0, 1)");
			DataRow[] rows;

			rows = result.Select("AccountNumber = '6610.10.10'");
			AssertEquals("Report should give back Active Accounts", rows[0]["AccountNumber"], "6610.10.10");

			rows = result.Select("AccountNumber = '6611.10.10'");

			AssertEquals("Report should NOT give back Non-Active Accounts", rows.Length, 0);
		}
	}
}

