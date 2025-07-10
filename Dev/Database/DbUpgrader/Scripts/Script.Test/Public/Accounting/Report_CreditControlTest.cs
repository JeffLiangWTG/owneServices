using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(Report_CreditControl))]
	class Report_CreditControlTest : DbCreateScriptTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1122:DoNotUseDateTimeParse", Justification = "Testing")]
		public void TestResult()
		{
			Guid companyPK = Guid.Parse("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC");
			Guid companyDataPK = Guid.Parse("491C5F9D-D08A-4FEE-89D1-43230D76E396");

			string reportSql = string.Format(@"select * from Report_CreditControl('{0}', 'ALL', '', GETDATE(), '', '', '', '', '', '', 'All', 'All')", companyPK);
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);

			AssertEquals("Result should have no rows", 0, result.Rows.Count);

			string testDataSql = string.Format(@"update dbo.OrgCompanyData set OB_ARCreditApproved = 1, OB_ARCreditLimit = 1000, OB_ARAccountAndCreditReviewDue = '01-Jul-2012'
insert into dbo.StmALog (SL_PK, SL_Table, SL_Parent, SL_PostedTimeUtc, SL_EventTime, SL_GS_NKUser, SL_SE_NKEvent) values (NEWID(), 'OrgCompanyData', '{0}', '8-Jun-2012', '8-Jun-2012', 'E', 'CDA')
insert into dbo.StmALog (SL_PK, SL_Table, SL_Parent, SL_PostedTimeUtc, SL_EventTime, SL_GS_NKUser, SL_SE_NKEvent) values (NEWID(), 'OrgCompanyData', '{0}', GETDATE(), GETDATE(), 'E', 'CDE')",
				companyDataPK);
			TestConnection.ExecuteNonQuery(testDataSql);

			result = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);

			AssertEquals("Result should have 1 row", 1, result.Rows.Count);
			AssertEquals("Result should have 25 columns", 25, result.Columns.Count);
			AssertEquals("WESCOM", result.Rows[0]["OrgCode"].ToString().Trim()); // Hardcoded column name in function result
			AssertEquals("WESTPOINT COMPANY", result.Rows[0]["OrgName"].ToString().Trim()); // Hardcoded column name in function result
			AssertEquals(1000M, decimal.Parse(result.Rows[0]["CreditLimit"].ToString())); // Hardcoded column name in function result
			AssertEquals(true, result.Rows[0]["CreditApproved"]); // Hardcoded column name in function result
			AssertEquals(false, result.Rows[0]["CreditOnHold"]); // Hardcoded column name in function result
			AssertEquals(new DateTime(2012, 7, 1), DateTime.Parse(result.Rows[0]["CreditReviewDue"].ToString())); // Hardcoded column name in function result
		}
	}
}

