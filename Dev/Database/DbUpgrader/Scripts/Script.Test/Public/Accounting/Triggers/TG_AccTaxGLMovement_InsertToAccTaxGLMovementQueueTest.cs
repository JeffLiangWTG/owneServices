using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Triggers;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Triggers.Testing
{
	[TestedType(typeof(TG_AccTaxGLMovement_InsertToAccTaxGLMovementQueue))]
	class TG_AccTaxGLMovement_InsertToAccTaxGLMovementQueueTest : DbCreateScriptTest
	{
		public void TestTG_AccTaxGLMovement_InsertToAccTaxGLMovementQueue()
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

			var taxGlMovement1PK = dbHelper.InsertTaxGLMovement(taxTransactionPK, debitGLAccountPK, creditGLAccountPK, type: "PND");
			assertAccTaxGLMovementQueueCount(1, taxGlMovement1PK);

			var taxGlMovement2PK = dbHelper.InsertTaxGLMovement(taxTransactionPK, creditGLAccountPK, debitGLAccountPK, type: "RLS");
			assertAccTaxGLMovementQueueCount(2, taxGlMovement2PK);

			void assertAccTaxGLMovementQueueCount(int totalNumber, Guid taxGLMovementPK)
			{
				AssertEquals("Total number of AccTaxGLMovementQueue records", totalNumber,
					(int)dbHelper.RunSQL(null, "SELECT COUNT(*) FROM dbo.AccTaxGLMovementQueue", executionType: TestDbHelperBase.SQLExecutionTypes.ExecuteScalar));
				AssertEquals("AccTaxGLMovementQueue record must exist.", 1,
					(int)dbHelper.RunSQL(new { ATQ_ATM = taxGLMovementPK }, "SELECT COUNT(*) FROM dbo.AccTaxGLMovementQueue WHERE ATQ_ATM = @ATQ_ATM", executionType: TestDbHelperBase.SQLExecutionTypes.ExecuteScalar));
			}
		}
	}
}

