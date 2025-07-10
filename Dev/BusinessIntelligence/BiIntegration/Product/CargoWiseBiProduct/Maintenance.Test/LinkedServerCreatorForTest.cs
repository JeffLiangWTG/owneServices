using System;
using System.Globalization;
using CargoWise.Data;
using Enterprise.Integration;

namespace CargoWise.Bi.Maintenance
{
	public class LinkedServerCreatorForTest : LinkedServerCreator
	{
		public LinkedServerCreatorForTest(AdminConnection biConnection, string linkedServerName, ILogger logger)
			: base(biConnection, linkedServerName, logger)
		{
		}

		public bool LinkedServerExists(DbConnection connection, string linkedServer)
		{
			return RegisteredServerExists(connection, linkedServer, isLinked: true);
		}

		public bool LinkedServerExistsAndIsAccessible(DbConnection connection, string linkedServer)
		{
			var exists = false;
			if (RegisteredServerExists(connection, linkedServer, isLinked: true))
			{
				exists = CanAccessLinkedServer(connection, linkedServer);
			}
			return exists;
		}

		public new string GetCreateLinkedServerWithMsOleDBQuery()
		{
			return base.GetCreateLinkedServerWithMsOleDBQuery();
		}

		public new string GetCreateLinkedServerWithNativeClientQuery()
		{
			return base.GetCreateLinkedServerWithNativeClientQuery();
		}

		bool RegisteredServerExists(DbConnection connection, string serverName, bool isLinked)
		{
			var sqlText = string.Format(
								 CultureInfo.InvariantCulture,
								 "IF EXISTS (SELECT NULL FROM sys.servers WHERE name = '{0}' AND is_linked = {1}) SELECT 1 ELSE SELECT 0",
								 serverName,
								 (isLinked ? "1" : "0"));
			var serverExists = Convert.ToBoolean(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture);
			return serverExists;
		}

		public bool LoginMappingExists(AdminConnection connection, string serverName, string loginName)
		{
			var sqlText = $@"
							IF EXISTS(SELECT *
							FROM sys.linked_logins ll
							JOIN sys.servers s on ll.server_id = s.server_id
							JOIN sys.sql_logins l on ll.local_principal_id = l.principal_id
							WHERE s.[name] = '{serverName}' AND l.name = '{loginName}')
							SELECT 1 ELSE SELECT 0
						";
			return Convert.ToBoolean(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture);
		}

		bool CanAccessLinkedServer(DbConnection connection, string serverName)
		{
			var sqlText = $"SELECT * FROM OPENQUERY([{serverName}], 'SELECT 1')";
			var canConnectToLinkedServer = Convert.ToBoolean(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture);
			return canConnectToLinkedServer;
		}

		public void CreateLinkedServerWithNativeClientForTest()
		{
			CreateLinkedServerWithNativeClient();
		}

		public void CreateLinkedServerWithMSOLEDBSQLForTest()
		{
			CreateLinkedServerWithMSOLEDBSQL();
		}

		public void EnsureLinkedServerNotExists()
		{
			if (LinkedServerExists(biConnection, LinkedServerName))
			{
				DropLinkedServer();
			}
		}
	}
}
