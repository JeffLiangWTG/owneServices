using System;

namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class ExcelInterfaceExceptionTest : ExceptionTestCase<ExcelInterfaceException>
	{
		public void TestTypeIsSet()
		{
			var exception = new ExcelInterfaceException(ExcelInterfaceExceptionType.CouldNotOpenStream, "Fred");
			AssertEquals("exception.Type", exception.Type, ExcelInterfaceExceptionType.CouldNotOpenStream);

			exception = new ExcelInterfaceException(ExcelInterfaceExceptionType.ErrorInsertingImage, "Fred", new Exception("Other Stuff"));
			AssertEquals("exception.Type", exception.Type, ExcelInterfaceExceptionType.ErrorInsertingImage);
		}

		public void TestTypeIsSerialized()
		{
			var exception = new ExcelInterfaceException(ExcelInterfaceExceptionType.ErrorFindingCellRange, "Fillet");

			var deSerialisedException = SerializeAndDeserializeExceptionJSON(exception);
			AssertEquals("deSerialisedException.Message", "Fillet", deSerialisedException.Message);
			AssertEquals("deSerialisedException.Type", ExcelInterfaceExceptionType.ErrorFindingCellRange, deSerialisedException.Type);
		}

		protected override ExcelInterfaceException GetNewExceptionToTest(string message)
		{
			return new ExcelInterfaceException(ExcelInterfaceExceptionType.ErrorSettingCellValue, message);
		}
	}
}
