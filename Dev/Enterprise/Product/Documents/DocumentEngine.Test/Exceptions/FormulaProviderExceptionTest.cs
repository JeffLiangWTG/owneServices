namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class FormulaProviderExceptionTest : ExceptionTestCase<FormulaProviderException>
	{
		protected override FormulaProviderException GetNewExceptionToTest(string message)
		{
			return new FormulaProviderException(message);
		}
	}
}
