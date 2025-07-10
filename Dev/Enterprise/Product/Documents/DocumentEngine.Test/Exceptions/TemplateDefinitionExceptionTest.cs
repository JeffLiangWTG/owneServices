using Enterprise.DocumentEngine.FlexCelInterface;

namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class TemplateDefinitionExceptionTest : ExceptionTestCase<TemplateDefinitionException>
	{
		protected override TemplateDefinitionException GetNewExceptionToTest(string message)
		{
			return new TemplateDefinitionException(message, new CellReference("Sheet Name", "A34"));
		}
	}
}
