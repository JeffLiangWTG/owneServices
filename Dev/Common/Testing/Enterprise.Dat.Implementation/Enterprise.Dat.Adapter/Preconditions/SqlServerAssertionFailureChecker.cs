using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Dat.Implementation.Preconditions
{
	[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "This assembly has no reference to enterprise libraries")]
	public class SqlServerAssertionFailureChecker : IPrecondition
	{
		public SqlServerAssertionFailureChecker(SqlServerServiceRestarter restarter)
		{
			this.restarter = restarter;
		}

		public string ErrorMessage => "A SQL Server Assertion Failed, the server may be in an unstable state and should be restarted";

		public bool CheckPreconditionMet()
		{
			using (var connection = LocalDBConnection.GetConnection())
			{
				connection.Open();

				var sql = $@"declare @startTime datetime
							 select @startTime = create_date from sys.databases where name = '{DatConfiguration.OdysseyTestDatabase}'
							 if @startTime is null
								 select @startTime = sqlserver_start_time FROM sys.dm_os_sys_info
							 EXEC master.dbo.xp_readerrorlog 0, 1, ""Assertion"", null, @startTime";

				using (var cmd = new SqlCommand(sql, connection))
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						restarter.RequireRestart(ErrorMessage + System.Environment.NewLine + (string)reader["Text"]);
					}
				}
			}

			return true;
		}

		readonly SqlServerServiceRestarter restarter;
	}
}
