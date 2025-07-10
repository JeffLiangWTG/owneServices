namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class InvalidGroupByColumnExceptionTest : ExceptionTestCase<InvalidGroupByColumnException>
	{
		protected override InvalidGroupByColumnException GetNewExceptionToTest(string message)
		{
			return new InvalidGroupByColumnException(message, "Oh no, not again!");
		}
	}
}
