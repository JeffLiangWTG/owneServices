using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.TakeUpSubLedgers.Testing
{
	[TestedType(typeof(BatchAggregator_TakeUpALLSubLedgers))]
	class BatchAggregator_Validation_TransactionLineTest : BatchAggregatorValidationTest
	{
		public override void TestAggregationValidation()
		{
			Assert("There is a test for this in Enterprise.Accounting.Business.Aggregator.Testing.BatchAggregatorTest.TestValidationLineType", true);
		}
	}
}

