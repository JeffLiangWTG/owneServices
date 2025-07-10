using System;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(Report_ClientDeclarationJobAnalysis))]
	class Report_ClientDeclarationJobAnalysisTest : DbCreateScriptTest
	{
		[TestDate(2016, 10, 20)]
		public void TestReportOnlyContainsActiveDeclarations()
		{
			var referenceDate = new DateTime(2016, 10, 20);
			var testHelper = new TestDbHelper(TestConnection);

			var companyPK = TestDataCreator.CreateCompany("TC1", "AU", "AUD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "AUCHI", referenceDate);
			var departmentPK = TestDataCreator.CreateDepartment("TST");

			var declaration1PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1);
			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2);

			var job1PK = testHelper.InsertJob("JOB001", companyPK, branchPK, departmentPK, "JE", declaration1PK, "WRK", referenceDate);
			var job2PK = testHelper.InsertJob("JOB002", companyPK, branchPK, departmentPK, "JE", declaration2PK, "WRK", referenceDate);

			TestConnection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, @"
UPDATE dbo.JobDeclaration
SET
	JE_IsCancelled = 1,
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '~BP'
WHERE
	JE_PK = '{0}'", declaration1PK));

			var dtPreCondition = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT JE_PK, JE_IsCancelled FROM dbo.JobDeclaration");
			AssertEquals(2, dtPreCondition.Rows.Count);
			AssertEquals(true, dtPreCondition.Select(string.Format(CultureInfo.InvariantCulture, "JE_PK = '{0}'", declaration1PK))[0]["JE_IsCancelled"]);
			AssertEquals(false, dtPreCondition.Select(string.Format(CultureInfo.InvariantCulture, "JE_PK = '{0}'", declaration2PK))[0]["JE_IsCancelled"]);

			var dtResult = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(CultureInfo.InvariantCulture, "SELECT JE_DeclarationReference FROM Report_ClientDeclarationJobAnalysis ('{0}', '', '', '', '', '')", companyPK));
			AssertEquals(1, dtResult.Rows.Count);
			AssertEquals("B002", dtResult.Rows[0]["JE_DeclarationReference"]);
		}

		[TestDate(2016, 10, 20)]
		public void TestJobInactive()
		{
			var testHelper = new TestDbHelper(TestConnection);

			var referenceDate = new DateTime(2016, 10, 20);

			var companyPK = TestDataCreator.CreateCompany("TC1", "AU", "AUD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "AUCHI", referenceDate);
			var departmentPK = TestDataCreator.CreateDepartment("TST");

			var declaration1PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1);

			var job1PK = testHelper.InsertJob("JOB001", companyPK, branchPK, departmentPK, "JE", declaration1PK, "WRK", DateTime.Now);

			testHelper.RunSQL(new { JH_PK = job1PK }, @"
UPDATE dbo.JobHeader
SET
	JH_IsActive = 0,
	JH_SystemLastEditTimeUtc = GETUTCDATE(),
	JH_SystemLastEditUser = '~BP'
WHERE
	JH_PK = @JH_PK");

			var dtResult = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(CultureInfo.InvariantCulture, "SELECT JH_SystemCreateTimeUtc, JE_DeclarationReference FROM Report_ClientDeclarationJobAnalysis ('{0}', '', '', '', '', '')", companyPK));
			AssertEquals("B001", dtResult.Rows[0]["JE_DeclarationReference"]);
			AssertEquals(DBNull.Value, dtResult.Rows[0]["JH_SystemCreateTimeUtc"]);
		}
	}
}

