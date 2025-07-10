using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.TakeUpSubLedgers.Testing
{
	[TestedType(typeof(BatchAggregator_Validation_ControlAccounts))]
	class BatchAggregator_Validation_ControlAccountsTest : BatchAggregatorValidationTest
	{
		public override void TestAggregationValidation()
		{
			PrepareForAggregation(setupControlAccounts: false);

			var exceptionMessage = RunAndAssertAggregationWithError();
			var expected = @"Please set up the following Control Accounts in the registry (Accounting > General Ledger Defaults > Control Account): AR Control Account, AP Control Account, Accrued Revenue Control Account, Accrued Cost Control Account, Revenue Suspense Control Account, Cost Suspense Control Account, Reportable Tax Input Control Account, Reportable Tax Output Control Account";
			AssertEquals("Exception Message", expected, exceptionMessage);

			DbHelper.RunSQL(new { GC_PK = TestDbHelper.DefaultCompanyPK }, "UPDATE dbo.GlbCompany SET GC_IsGSTCashBasis = 1, GC_SystemLastEditUser = 'E', GC_SystemLastEditTimeUtc = GetDate() WHERE GC_PK = @GC_PK");
			exceptionMessage = RunAndAssertAggregationWithError();
			expected += @", Pending Tax Input Control Account, Pending Tax Output Control Account";
			AssertEquals("Exception Message", expected, exceptionMessage);
		}
	}
}

