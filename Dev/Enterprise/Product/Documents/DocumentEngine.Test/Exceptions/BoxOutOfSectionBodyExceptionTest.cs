namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class BoxOutOfSectionBodyExceptionTest : ExceptionTestCase<BoxOutOfSectionBodyException>
	{
		protected override BoxOutOfSectionBodyException GetNewExceptionToTest(string message)
		{
			return new BoxOutOfSectionBodyException(message);
		}
	}
}
