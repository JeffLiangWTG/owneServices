namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class DocumentEngineTooManyColumnsForThisFileFormatExceptionTest : ExcelLimitationForThisFileFormatExceptionTest
	{
		protected override ExcelLimitationBaseException GetNewExceptionToTest(string message)
		{
			return new DocumentEngineTooManyColumnsForThisFileFormatException();
		}
	}
}
