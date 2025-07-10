namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class MissingWorksheetExceptionTest : ExceptionTestCase<MissingWorksheetException>
	{
		protected override MissingWorksheetException GetNewExceptionToTest(string message)
		{
			return new MissingWorksheetException("Could not find worksheet named [Sheet Name] in template [Template Name].", "Sheet Name", "Template Name");
		}
	}
}
