namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class DocumentAreaUnbreakableExceptionTest : ExceptionTestCase<DocumentAreaUnbreakableException>
	{
		protected override DocumentAreaUnbreakableException GetNewExceptionToTest(string message)
		{
			return new DocumentAreaUnbreakableException(message);
		}
	}
}
