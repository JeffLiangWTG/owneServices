using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class TestSqlExceptionBuilder : TestCase
	{
		public void TestCreateSqlException()
		{
			SqlError sqlError = SqlExceptionBuilder.CreateSqlError(1337, byte.MaxValue, byte.MinValue, "dbserver", "lolinternet", "@@lols", 111);
			SqlErrorCollection errors = SqlExceptionBuilder.CreateSqlErrorCollection(sqlError);

			SqlException exception = SqlExceptionBuilder.CreateSqlException(errors);
			AssertEquals(1337, exception.Number);
		}

		public void TestCreateSqlErrorCollection()
		{
			SqlError sqlError = SqlExceptionBuilder.CreateSqlError(1337, byte.MaxValue, byte.MinValue, "dbserver", "lolinternet", "@@lols", 111);
			SqlErrorCollection errors = SqlExceptionBuilder.CreateSqlErrorCollection(sqlError);
			AssertEquals("Should have count of ", 1, errors.Count);
		}

		public void TestCreateSqlError()
		{
			SqlError sqlError = SqlExceptionBuilder.CreateSqlError(1337, byte.MaxValue, byte.MinValue, "dbserver", "lolinternet", "@@lols", 111);
			AssertEquals("Incorrect value", 1337, sqlError.Number);
			AssertEquals("Incorrect value", byte.MaxValue, sqlError.State);
			AssertEquals("Incorrect value", byte.MinValue, sqlError.Class);
			AssertEquals("Incorrect value", "dbserver", sqlError.Server);
			AssertEquals("Incorrect value", "lolinternet", sqlError.Message);
			AssertEquals("Incorrect value", "@@lols", sqlError.Procedure);
			AssertEquals("Incorrect value", 111, sqlError.LineNumber);
		}
	}
}
