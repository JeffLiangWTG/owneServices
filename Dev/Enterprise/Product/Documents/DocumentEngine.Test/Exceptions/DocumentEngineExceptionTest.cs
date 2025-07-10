namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class DocumentEngineExceptionTest : ExceptionTestCase<DocumentEngineException>
	{
		protected override DocumentEngineException GetNewExceptionToTest(string message)
		{
			return new DocumentEngineException(message);
		}
	}
}
