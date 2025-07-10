namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class DocumentMenuExceptionTest : ExceptionTestCase<DocumentMenuException>
	{
		protected override DocumentMenuException GetNewExceptionToTest(string message)
		{
			return new DocumentMenuException(message);
		}
	}
}
