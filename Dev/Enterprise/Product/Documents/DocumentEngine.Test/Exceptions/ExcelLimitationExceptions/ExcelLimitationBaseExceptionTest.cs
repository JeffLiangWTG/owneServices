namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	abstract class ExcelLimitationBaseExceptionTest : ExceptionTestCase<ExcelLimitationBaseException>
	{
		protected abstract override ExcelLimitationBaseException GetNewExceptionToTest(string message);
	}
}
