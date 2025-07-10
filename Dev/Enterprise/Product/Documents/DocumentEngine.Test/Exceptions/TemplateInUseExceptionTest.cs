namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class TemplateInUseExceptionTest : ExceptionTestCase<TemplateInUseException>
	{
		protected override TemplateInUseException GetNewExceptionToTest(string message)
		{
			return new TemplateInUseException(message);
		}
	}
}
