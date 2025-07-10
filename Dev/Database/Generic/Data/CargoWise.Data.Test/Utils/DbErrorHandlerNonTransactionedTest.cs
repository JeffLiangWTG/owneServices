using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	public class DbErrorHandlerNonTransactionedTest : TestCase
	{
		public void TestGetExtraDebugInformationElevation()
		{
			var exception = SqlExceptionBuilder.CreateSqlException(
				SqlExceptionBuilder.CreateSqlErrorCollection(
					SqlExceptionBuilder.CreateSqlError(-2, 0, 11, Db.ServerName, "Timeout expired.  The timeout period elapsed prior to completion of the operation or the server is not responding.", "", 0)));
			string extraDebugInformation = new DbErrorHandler(exception, Db.Connection).GetExtraDebugInformation();
			Assert("Extra Debug Information should not contain \"Cannot find\" errors\r\n\r\nActual Result:\r\n" + extraDebugInformation, !extraDebugInformation.Contains("Cannot find"));
			Assert("Extra Debug Information should contain \"-- LOGGED IN USERS --\"\r\n\r\nActual Result:\r\n" + extraDebugInformation, extraDebugInformation.Contains("-- LOGGED IN USERS --"));
		}

		public void TestCanRecover()
		{
			var exception = SqlExceptionBuilder.CreateSqlException(
				SqlExceptionBuilder.CreateSqlErrorCollection(
					SqlExceptionBuilder.CreateSqlError(
						2627, 0, 1, Db.ServerName, "Violation of PRIMARY KEY constraint 'PK_ID'. Cannot insert duplicate key in object 'dbo.StmALog'. The duplicate key value is (b189b073-5be2-4c83-b6b6-bd15817ed36c).", "", 0
					)));
			Assert("Cannot recover from duplicate PK error.", !new DbErrorHandler(exception, Db.Connection).CanRecover);

			exception = SqlExceptionBuilder.CreateSqlException(
				SqlExceptionBuilder.CreateSqlErrorCollection(
					SqlExceptionBuilder.CreateSqlError(
						2627, 0, 1, Db.ServerName, "Violation of PRIMARY KEY constraint 'FK_UX_SomeOtherKey'. Cannot insert duplicate key in object 'dbo.StmALog'. The duplicate key value is (b189b073-5be2-4c83-b6b6-bd15817ed36c).", "", 0
					)));
			Assert("Can recover from non-PK duplicate error.", new DbErrorHandler(exception, Db.Connection).CanRecover);
		}

		public void TestDeadlockErrorMessageContent()
		{
			int spID = Db.Connection.SPID;

			Db.Connection.ExecuteNonQuery("DECLARE @TXT VARCHAR(50) SELECT @TXT = 'DEADLOCK UNIT TEST'");

			var exception = SqlExceptionBuilder.CreateSqlException(
				SqlExceptionBuilder.CreateSqlErrorCollection(
					SqlExceptionBuilder.CreateSqlError(1205, 51, 13, Db.ServerName, $"Transaction (Process ID {spID}) was deadlocked on resources with another process and has been chosen as the deadlock victim. Rerun the transaction.", string.Empty, 0)));

			string extraDebugInformation = new DbErrorHandler(exception, Db.Connection).GetExtraDebugInformation();

			Assert("No stack trace available", !string.IsNullOrWhiteSpace(exception.StackTrace));
			Assert("Invalid SQL Statement\n" + extraDebugInformation, extraDebugInformation.Contains("DEADLOCK UNIT TEST"));
			Assert("Extra Debug Information does not contain full stack trace:\n" + extraDebugInformation, extraDebugInformation.Contains(nameof(TestDeadlockErrorMessageContent)));
		}
	}
}
