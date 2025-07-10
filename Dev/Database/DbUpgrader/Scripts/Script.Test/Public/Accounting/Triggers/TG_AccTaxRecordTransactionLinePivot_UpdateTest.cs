using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Triggers;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Triggers.Testing
{
	[TestedType(typeof(TG_AccTaxRecordTransactionLinePivot_Update))]
	class TG_AccTaxRecordTransactionLinePivot_UpdateTest : DBCreateTriggerScriptTest
	{
		public void TestShouldNotAllowToUpdateValues()
		{
			var testHelper = new TestDbHelper(TestConnection);
			var branchPK = testHelper.InsertBranch("ZZB", TestDbHelper.DefaultCompanyPK);
			var departmentPK = testHelper.InsertDepartment("ZZD");
			var jobPK = testHelper.InsertJob("JOB1", TestDbHelper.DefaultCompanyPK, branchPK, departmentPK, "JS", Guid.NewGuid(), "WRK", DateTime.Now);
			var chargeCodePK = testHelper.InsertChargeCode(TestDbHelper.DefaultCompanyPK, "CHG");
			var orgHeaderPK = testHelper.InsertOrgHeader("ORG", "Organisation");
			var taxIdPK = testHelper.InsertTaxRate("TAX1");
			var invoicePK = testHelper.InsertTransactionHeader("AP", "INV", "100", 100, DateTime.Now, branchPK, departmentPK);
			var linePK = testHelper.InsertTransactionLine(invoicePK, jobPK, chargeCodePK, null, branchPK, departmentPK, orgHeaderPK, 100, "CST", DateTime.Now, DateTime.Now);
			var taxConfigurationPK = testHelper.InsertTaxConfiguration("ADT", TestDbHelper.DefaultCompanyPK);
			var taxTransactionPK = testHelper.InsertTaxTransaction(invoicePK, TestDbHelper.DefaultCompanyPK, branchPK, departmentPK, taxConfigurationPK, taxIdPK);
			testHelper.InsertTaxTransactionPivot(taxTransactionPK, linePK);

			var expectedMessage = "Cannot update any value of AccTaxRecordTransactionLinePivot";
			var updateSql = "UPDATE dbo.AccTaxRecordTransactionLinePivot SET ATP_LocalTaxAmount = @ATP_LocalTaxAmount";
			AssertExceptionThrown<SqlException>("Should be: " + expectedMessage, expectedMessage + "\r\nThe transaction ended in the trigger. The batch has been aborted.", delegate { testHelper.RunSQL(new { ATP_LocalTaxAmount = 155 }, updateSql); });
		}
	}
}
