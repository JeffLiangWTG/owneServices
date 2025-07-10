namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class DocumentConvertExceptionTest : ExceptionTestCase<DocumentConvertException>
	{
		protected override DocumentConvertException GetNewExceptionToTest(string message)
		{
			return new DocumentConvertException(message);
		}
	}
}
