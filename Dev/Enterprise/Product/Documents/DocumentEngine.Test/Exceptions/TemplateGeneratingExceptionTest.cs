using FlexCel.XlsAdapter;

namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class TemplateGeneratingExceptionTest : ExceptionTestCase<TemplateGeneratingException>
	{
		protected override TemplateGeneratingException GetNewExceptionToTest(string message)
		{
			return new TemplateGeneratingException(message, new FlexCelXlsAdapterException());
		}
	}
}
