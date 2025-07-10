using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.TakeUpSubLedgers.Testing
{
	[TestedType(typeof(BatchAggregator_Validation_AppropriationAccount))]
	class BatchAggregator_Validation_AppropriationAccountTest : BatchAggregatorValidationTest
	{
		public override void TestAggregationValidation()
		{
			PrepareForAggregation();
			TestConnection.ExecuteNonQuery("Delete from  dbo.stmdata where SD_Name = 'GL_PL_APPROPRIATION_ACCOUNT'");

			var exceptionMessage = RunAndAssertAggregationWithError();
			var expected = @"Please set up the Appropriation Account in the registry (Accounting > Framework > PL Appropriation Account)";
			AssertEquals("Exception Message", expected, exceptionMessage);
		}
	}
}

