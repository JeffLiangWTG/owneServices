using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;

namespace Enterprise.Dat.Implementation.Preconditions
{
	[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "This assembly has no reference to enterprise libraries")]
	class SqlServerNameChecker : IPrecondition
	{
		public SqlServerNameChecker(SqlServerServiceRestarter restarter)
		{
			this.restarter = restarter;
		}

		public string ErrorMessage
		{
			get { return "ServerName on this machine does not match matchine name. This has been fixed, but requires a restart of the sql server service (or machine)"; }
		}

		public bool CheckPreconditionMet()
		{
			var actualName = GetActualServerInstanceName();
			var registeredName = GetRegisteredServerInstanceName();
			if (registeredName != actualName)
			{
				SetSQLServerName(registeredName, actualName);
				restarter.RequireRestart(ErrorMessage);
			}
			return true;
		}

		static void SetSQLServerName(string oldSqlServerName, string newSqlServerName)
		{
			string sql = @"
              EXEC ('sp_dropserver ''' +  @OldServerName + '''');
              EXEC ('sp_addserver ''' + @NewServerName+''', ''local''');
			";
			using (var connection = LocalDBConnection.GetConnection())
			{
				try
				{
					connection.Open();

					using (var cmd = connection.CreateCommand())
					{
						cmd.CommandText = sql;
						cmd.Parameters.AddWithValue("@OldServerName", oldSqlServerName);
						cmd.Parameters.AddWithValue("@NewServerName", newSqlServerName);

						cmd.ExecuteNonQuery();
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
			}
		}

		static string GetActualServerInstanceName()
		{
			string result = string.Empty;

			string sql = "SELECT SERVERPROPERTY('ServerName')";
			using (var connection = LocalDBConnection.GetConnection())
			{
				try
				{
					connection.Open();

					using (var cmd = connection.CreateCommand())
					{
						cmd.CommandText = sql;
						using (var reader = cmd.ExecuteReader())
						{
							if (reader.Read())
							{
								result = (string)reader[0];
							}
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
			}

			return result;
		}

		static string GetRegisteredServerInstanceName()
		{
			string result = string.Empty;
			string sql = "SELECT [name] FROM sys.servers WHERE server_id = 0;";

			using (var connection = LocalDBConnection.GetConnection())
			{
				try
				{
					connection.Open();

					using (var cmd = connection.CreateCommand())
					{
						cmd.CommandText = sql;
						using (var reader = cmd.ExecuteReader())
						{
							if (reader.Read())
							{
								result = (string)reader[0];
							}
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
			}

			return result;
		}

		readonly SqlServerServiceRestarter restarter;
	}
}
