namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class ReportSQLTimeoutExceptionTest : ExceptionTestCase<ReportSQLTimeoutException>
	{
		protected override ReportSQLTimeoutException GetNewExceptionToTest(string message)
		{
			return new ReportSQLTimeoutException();
		}
	}
}
