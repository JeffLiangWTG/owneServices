namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class MaxConcurrentReportConnectionsExceededTest : ExceptionTestCase<MaxConcurrentReportConnectionsExceeded>
	{
		protected override MaxConcurrentReportConnectionsExceeded GetNewExceptionToTest(string message)
		{
			return new MaxConcurrentReportConnectionsExceeded(message,"ReportInfo");
		}
	}
}
