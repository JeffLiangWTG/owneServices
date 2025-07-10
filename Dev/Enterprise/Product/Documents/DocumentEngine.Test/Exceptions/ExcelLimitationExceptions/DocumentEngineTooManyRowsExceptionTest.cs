namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class DocumentEngineTooManyRowsExceptionTest : ExcelLimitationExceptionTest
	{
		protected override ExcelLimitationBaseException GetNewExceptionToTest(string message)
		{
			return new DocumentEngineTooManyRowsException();
		}
	}
}
