using System;
using CargoWise.Data.Testing;

namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class SQLExecutionExceptionTest : ExceptionTestCase<SQLExecutionException>
	{
		public void TestGetMessageForSQLExceptionWorkingProperly()
		{
			SqlException sqlException = GetValidSqlException();
			SQLExecutionException exception = new SQLExecutionException("Fergie", "Thunder", sqlException);
			AssertEquals("exception.Message", "Error loading table [Fergie]. Error: [Errors Are Fun] occurred running SQL: [Thunder]. SqlException: Msg 121, Level 4, State 3, Procedure No Procedure, Line 5, Errors Are Fun", exception.Message);
		}

		public void TestGetMessageForNonSQLExceptionWorkingProperly()
		{
			InvalidOperationException originalException = new InvalidOperationException("This is a test message");
			SQLExecutionException sqlExecutionException = new SQLExecutionException("Dummy Table Name", "Dummy command text", originalException);
			AssertEquals("sqlExecutionException.Message", "Error loading table [Dummy Table Name]. System.InvalidOperationException: [This is a test message] occurred running SQL: [Dummy command text]. ", sqlExecutionException.Message);
		}

		public void TestGetMessageSqlException()
		{
			var sql = "select 1/0 as a;";

			try
			{
				CargoWise.Data.Db.Connection.ExecuteNonQuery(sql);
			}
			catch (Exception exception)
			{
				var ex = new SQLExecutionException("SqlExceptionTest", sql, exception);
				var msg = ex.Message;

				AssertEquals(true, msg.IndexOf("Msg 8134, Level 16, State 1, Line 1, Divide by zero error encountered.") >= 0);
			}

			var sqlProc = @"
				if OBJECT_ID('ProcTestGetMessageSqlException_G981D') is not null drop procedure ProcTestGetMessageSqlException_G981D;
				EXECUTE ( 'create procedure dbo.ProcTestGetMessageSqlException_G981D as begin select 1/0 as a; end') ;
				exec ProcTestGetMessageSqlException_G981D;";
			var sqlCleanup = @"if OBJECT_ID('ProcTestGetMessageSqlException_G981D') is not null drop procedure ProcTestGetMessageSqlException_G981D;";

			try
			{
				CargoWise.Data.Db.Connection.ExecuteNonQuery(sqlProc);
			}
			catch (Exception exception)
			{
				var ex = new SQLExecutionException("SqlExceptionTest", sqlProc, exception);
				var msg = ex.Message;

				AssertEquals(true, msg.IndexOf("Msg 8134, Level 16, State 1, Procedure ProcTestGetMessageSqlException_G981D, Line 1, Divide by zero error encountered.") >= 0);
			}
			finally
			{
				CargoWise.Data.Db.Connection.ExecuteNonQuery(sqlCleanup);
			}
		}

		protected override SQLExecutionException GetNewExceptionToTest(string message)
		{
			SqlException sqlException = GetValidSqlException();
			return new SQLExecutionException("Fergie", "Thunder", sqlException);
		}

		static SqlException GetValidSqlException()
		{
			var error = SqlExceptionBuilder.CreateSqlError(121, 3, 4, "Server Me", "Errors Are Fun", "No Procedure", 5);
			var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			return SqlExceptionBuilder.CreateSqlException(errors);
		}
	}
}
