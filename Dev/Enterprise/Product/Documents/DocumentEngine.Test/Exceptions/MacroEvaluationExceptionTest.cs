namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class MacroEvaluationExceptionTest : ExceptionTestCase<MacroEvaluationException>
	{
		protected override MacroEvaluationException GetNewExceptionToTest(string message)
		{
			return new MacroEvaluationException(message, new Enterprise.DocumentEngine.MacroValueProviders.CompanyCode());
		}
	}
}
