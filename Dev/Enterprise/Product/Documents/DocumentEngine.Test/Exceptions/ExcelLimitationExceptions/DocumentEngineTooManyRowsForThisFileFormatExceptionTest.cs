namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class DocumentEngineTooManyRowsForThisFileFormatExceptionTest : ExcelLimitationForThisFileFormatExceptionTest
	{
		protected override ExcelLimitationBaseException GetNewExceptionToTest(string message)
		{
			return new DocumentEngineTooManyRowsForThisFileFormatException();
		}
	}
}
