namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class DocumentEngineTooLongFormulaForThisFileFormatExceptionTest : ExcelLimitationForThisFileFormatExceptionTest
	{
		protected override ExcelLimitationBaseException GetNewExceptionToTest(string message)
		{
			return new DocumentEngineTooLongFormulaForThisFileFormatException();
		}
	}
}
