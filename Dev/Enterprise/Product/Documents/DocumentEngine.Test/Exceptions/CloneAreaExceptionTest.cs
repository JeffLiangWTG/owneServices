namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class CloneAreaExceptionTest : ExceptionTestCase<CloneAreaException>
	{
		protected override CloneAreaException GetNewExceptionToTest(string message)
		{
			return new CloneAreaException(message);
		}
	}
}
