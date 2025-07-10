using NUnit.Framework;

namespace Enterprise.ErrorReporting.ServiceTasks.Test
{
	[TestedType(typeof(ReportErrorsServiceTask))]
	sealed class ReportErrorsServiceTaskTest_BatchesLargeDataSet : ReportErrorsServiceTaskTestBase
	{
		public void TestBatchesLargeSends()
		{
			var logger = InitialiseAndRunTaskSchedule(ServiceTask);

			AssertEquals(6, logger.Count);
			int loggerIndex = 0;
			AssertEquals("Information|Sent 10 error reports", logger[loggerIndex++]);
			AssertEquals("Debug|Marked 10 error reports as 'SNT'", logger[loggerIndex++]);
			AssertEquals("Information|Sent 10 error reports", logger[loggerIndex++]);
			AssertEquals("Debug|Marked 10 error reports as 'SNT'", logger[loggerIndex++]);
			AssertEquals("Information|Sent 3 error reports", logger[loggerIndex++]);
			AssertEquals("Debug|Marked 3 error reports as 'SNT'", logger[loggerIndex++]);
		}
		protected override string GetTestDataSqlFileName()
		{
			return "Enterprise.ErrorReporting.ServiceTasks.Test.CreateTestDataLarge.sql";
		}

		protected override int GetNumTestErrorReports()
		{
			return 23;
		}
	}
}
