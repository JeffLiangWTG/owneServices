namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class MetadataHasChangedExceptionTest : ExceptionTestCase<MetadataHasChangedException>
	{
		protected override MetadataHasChangedException GetNewExceptionToTest(string message)
		{
			return new MetadataHasChangedException();
		}
	}
}
