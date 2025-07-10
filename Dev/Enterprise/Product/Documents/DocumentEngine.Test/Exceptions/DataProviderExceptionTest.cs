namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class DataProviderExceptionTest : ExceptionTestCase<DataProviderException>
	{
		protected override DataProviderException GetNewExceptionToTest(string message)
		{
			return new DataProviderException(message);
		}
	}
}
