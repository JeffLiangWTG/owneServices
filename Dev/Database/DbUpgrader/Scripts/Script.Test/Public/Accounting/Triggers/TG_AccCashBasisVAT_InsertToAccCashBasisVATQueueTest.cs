using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Triggers;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Triggers.Testing
{
	[TestedType(typeof(TG_AccCashBasisVAT_InsertToAccCashBasisVATQueue))]
	class TG_AccCashBasisVAT_InsertToAccCashBasisVATQueueTest : DbCreateScriptTest
	{
		public void TestTrigger()
		{
			var cashVATPK1 = testHelper.InsertCashBasisVAT(linePK);
			var cashVATPK2 = testHelper.InsertCashBasisVAT(linePK);
			Action<int, Guid> assertAccCashBasisVATQueueCount = (totalNumber, cashVATPK) =>
				{
					AssertEquals("Total number of AccCashBasisVATQueue records", totalNumber,
						(int)testHelper.RunSQL(null, "SELECT COUNT(*) FROM dbo.AccCashBasisVATQueue", executionType: TestDbHelperBase.SQLExecutionTypes.ExecuteScalar));
					AssertEquals("AccCashBasisVATQueue record must exist.", 1,
						(int)testHelper.RunSQL(new { YCC_YC = cashVATPK }, "SELECT COUNT(*) FROM dbo.AccCashBasisVATQueue WHERE YCC_YC = @YCC_YC", executionType: TestDbHelperBase.SQLExecutionTypes.ExecuteScalar));
				};

			assertAccCashBasisVATQueueCount(2, cashVATPK1);
			assertAccCashBasisVATQueueCount(2, cashVATPK2);

			testHelper.RunSQL(new { YC_PK = cashVATPK1 }, "UPDATE dbo.AccCashBasisVAT SET YC_TaxAmount = 10, YC_SystemLastEditTimeUtc = GETUTCDATE(), YC_SystemLastEditUser = 'TST' WHERE YC_PK = @YC_PK");
			assertAccCashBasisVATQueueCount(2, cashVATPK1);
			assertAccCashBasisVATQueueCount(2, cashVATPK2);

			testHelper.RunSQL(new { YC_PK = cashVATPK2 }, "DELETE FROM dbo.AccCashBasisVAT WHERE YC_PK = @YC_PK");
			assertAccCashBasisVATQueueCount(1, cashVATPK1);
		}

		protected override void SetUp()
		{
			base.SetUp();

			testHelper = new TestDbHelper(TestConnection);
			var branchPK = testHelper.InsertBranch("ZZB", TestDbHelper.DefaultCompanyPK);
			var departmentPK = testHelper.InsertDepartment("ZZD");
			var jobPK = testHelper.InsertJob("JOB1", TestDbHelper.DefaultCompanyPK, branchPK, departmentPK, "JS", Guid.NewGuid(), "WRK", DateTime.Now);
			var chargeCodePK = testHelper.InsertChargeCode(TestDbHelper.DefaultCompanyPK, "CHG");
			var orgHeaderPK = testHelper.InsertOrgHeader("ORG", "Organisation");
			var invoicePK = testHelper.InsertTransactionHeader("AP", "INV", "100", 100, DateTime.Now, branchPK, departmentPK);
			linePK = testHelper.InsertTransactionLine(invoicePK, jobPK, chargeCodePK, null, branchPK, departmentPK, orgHeaderPK, 100, "CST", DateTime.Now, DateTime.Now);
		}

		Guid linePK;
		TestDbHelper testHelper;
	}
}

