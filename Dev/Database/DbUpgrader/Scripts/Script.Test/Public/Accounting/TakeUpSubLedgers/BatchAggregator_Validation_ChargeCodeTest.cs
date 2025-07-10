using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.TakeUpSubLedgers.Testing
{
	[TestedType(typeof(BatchAggregator_Validation_ChargeCode))]
	class BatchAggregator_Validation_ChargeCodeTest : BatchAggregatorValidationTest
	{
		public override void TestAggregationValidation()
		{
			PrepareForAggregation(setAccountsOnChargeCodes: false);

			var exceptionMessage = RunAndAssertAggregationWithError();
			var expected = "Charge codes incorrectly setup. Please check the GL Account Setup for these charges: BOND, CLAIM";
			AssertEquals("Exception Message", expected, exceptionMessage);
		}
	}
}

