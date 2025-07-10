namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class ReportProcessingExceptionTest : ExceptionTestCase<ReportProcessingException>
	{
		public void TestMessage()
		{
			ReportProcessingException reportProcessingException = new ReportProcessingException("Error Title", "Error Message");

			AssertEquals("reportProcessingException.Message", @"
Error Title

Error Message".Trim(), reportProcessingException.Message);
		}

		protected override ReportProcessingException GetNewExceptionToTest(string message)
		{
			return new ReportProcessingException("Hello Sailor", null);
		}
	}
}
