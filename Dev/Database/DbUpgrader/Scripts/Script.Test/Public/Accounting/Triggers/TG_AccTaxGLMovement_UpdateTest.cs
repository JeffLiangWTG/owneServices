using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Triggers;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Triggers.Testing
{
	[TestedType(typeof(TG_AccTaxGLMovement_Update))]
	class TG_AccTaxGLMovement_UpdateTest : DBCreateTriggerScriptTest
	{
		public void TestShouldNotAllowToUpdateValues()
		{
			var dbHelper = new TestDbHelper(TestConnection);
			var companyPK = TestDbHelper.DefaultCompanyPK;
			var branchPK = dbHelper.DefaultBranchPK;
			var departmentPK = dbHelper.DefaultDepartmentPK;
			var taxIdPK = dbHelper.InsertTaxRate("TAX1");
			var debitGLAccountPK = dbHelper.InsertGLAccount("1001.10.10", "Debit Account");
			var creditGLAccountPK = dbHelper.InsertGLAccount("1001.10.20", "Credit Account");
			var transactionHeaderPK = dbHelper.InsertTransactionHeader("AR", "INV", "001001", 110, DateTime.Today, branchPK, departmentPK);
			var taxConfigurationPK = dbHelper.InsertTaxConfiguration("ADT", companyPK);
			var taxTransactionPK = dbHelper.InsertTaxTransaction(transactionHeaderPK, companyPK, branchPK, departmentPK, taxConfigurationPK, taxIdPK);
			dbHelper.InsertTaxGLMovement(taxTransactionPK, debitGLAccountPK, creditGLAccountPK, 10);

			var expectedMessage = "Cannot update any value of AccTaxGLMovement";
			var updateSql = "UPDATE dbo.AccTaxGLMovement SET ATM_Amount = @ATM_Amount, ATM_SystemLastEditTimeUtc = GETUTCDATE(), ATM_SystemLastEditUser = 'TST'";
			AssertExceptionThrown<SqlException>("Should be: " + expectedMessage, expectedMessage + "\r\nThe transaction ended in the trigger. The batch has been aborted.", delegate { dbHelper.RunSQL(new { ATM_Amount = 15 }, updateSql); });
		}
	}
}
