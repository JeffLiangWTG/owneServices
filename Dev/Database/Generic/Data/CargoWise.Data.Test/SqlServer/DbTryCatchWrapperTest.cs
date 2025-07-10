using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class DbTryCatchWrapperTest : TransactionedTestCase
	{
		public void TestWrapCommandToHandleSpecificError()
		{
			var tryCatchWrapper = new DbTryCatchWrapper();
			string sqlText = tryCatchWrapper.WrapCommandToHandleSpecificError("RAISERROR (15175, 16, 1)", 0, "SELECT 'This will not be executed'");
			AssertErrorNumber(TestConnection, sqlText, 15175);

			sqlText = tryCatchWrapper.WrapCommandToHandleSpecificError("RAISERROR (15175, 16, 1)", 15175, "SELECT '=FIRST_ERROR_HANDLED_SUCCESSFULLY='");
			string handledResult = TestConnection.ExecuteScalar(sqlText).ToString();
			AssertEquals("Comand result after handling error", "=FIRST_ERROR_HANDLED_SUCCESSFULLY=", handledResult);

			sqlText = tryCatchWrapper.WrapCommandToHandleSpecificError("RAISERROR ('SomeUserError', 16, 1)", 0, "SELECT 'This will not be executed'");
			AssertErrorNumber(TestConnection, sqlText, 50000);

			sqlText = tryCatchWrapper.WrapCommandToHandleSpecificError("RAISERROR ('SomeUserError', 16, 1)", 50000, "SELECT '=SECOND_ERROR_HANDLED_SUCCESSFULLY='");
			handledResult = TestConnection.ExecuteScalar(sqlText).ToString();
			AssertEquals("Comand result after handling error", "=SECOND_ERROR_HANDLED_SUCCESSFULLY=", handledResult);
		}

		public static void AssertErrorNumber(DbConnection connection, string sqlCmd, int expectedErrorNumber)
		{
			try
			{
				connection.ExecuteNonQuery(sqlCmd);
				Fail("Should throw exception");
			}
			catch (SqlException ex)
			{
				AssertEquals("Wrong Exception caught - " + ex.Message, expectedErrorNumber, ex.Number);
			}
		}
	}
}
