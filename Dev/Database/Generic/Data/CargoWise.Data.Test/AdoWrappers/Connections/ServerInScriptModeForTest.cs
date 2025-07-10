namespace CargoWise.Data.Testing
{
	sealed class ServerInScriptModeForTest
	{
		public void Connect()
		{
			var sqlError = SqlExceptionBuilder.CreateSqlError(18401, 1, 1, "SqlServerInScriptMode", "Sql Server is being upgraded", "", 1);
			var errCollection = SqlExceptionBuilder.CreateSqlErrorCollection(sqlError);
			var sqlEx = SqlExceptionBuilder.CreateSqlException(errCollection);
			throw sqlEx;
		}
	}
}
