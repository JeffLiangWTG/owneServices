namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class FormulaProviderNotReadyExceptionTest : ExceptionTestCase<FormulaProviderNotReadyException>
	{
		protected override FormulaProviderNotReadyException GetNewExceptionToTest(string message)
		{
			return new FormulaProviderNotReadyException(message);
		}
	}
}
