namespace CargoWise.Bi.Maintenance
{
	using System;
	using System.Data;
	using System.Globalization;
	using System.Text.RegularExpressions;
	using CargoWise.Application;
	using CargoWise.Common;
	using CargoWise.Data;
	using CargoWise.DbUpgrader.Foundation;
	using Enterprise.Integration;
	using Enterprise.Integration.Licensing;
	using Enterprise.ZArchitecture.Environment;

	public class LinkedServerCreator
	{
		public LinkedServerCreator(AdminConnection biConnection, string linkedServerName, ILogger logger)
		{
			this.logger = logger;
			this.biConnection = biConnection;
			LinkedServerName = linkedServerName;
		}
		public LinkedServerCreator(AdminConnection biConnection, string linkedServerName, IUpgradeTaskWorkflowLogger logger)
		{
			upgradeTaskWorkflowLogger = logger;
			this.biConnection = biConnection;
			LinkedServerName = linkedServerName;
		}

		public ILogger logger;
		public IUpgradeTaskWorkflowLogger upgradeTaskWorkflowLogger;
		internal readonly DbConnection biConnection;

		public string LinkedServerName { get; private set; }
		public string LinkedServerNameWithoutPortNumber
		{
			get
			{
				return GetServerNameWithoutPortNumber(LinkedServerName);
			}
		}
		public const string DefaultPortNumber = "1433";
		string linkedServerPortNumber;
		public string LinkedServerPortNumber
		{
			get
			{
				if (linkedServerPortNumber.IsNullOrEmpty())
				{
					linkedServerPortNumber = GetServerPortNumber(LinkedServerName);
				}
				return linkedServerPortNumber;
			}
		}

		public const string Comma = ",";
		string linkedServerNameWithPortNumber;
		public string LinkedServerNameWithPortNumber
		{
			get
			{
				if (linkedServerNameWithPortNumber.IsNullOrEmpty())
				{
					linkedServerNameWithPortNumber = LinkedServerNameWithoutPortNumber + Comma + LinkedServerPortNumber;
				}
				return linkedServerNameWithPortNumber;
			}
		}

		public static string GetServerNameWithoutPortNumber(string serverName)
		{
			Argument.NotNull(serverName, nameof(serverName));

			return serverName.Contains(Comma) ? serverName.Split(',')[0] : serverName;
		}

		public static string GetServerPortNumber(string serverName)
		{
			Argument.NotNull(serverName, nameof(serverName));

			string result;
			if (TryParseServerPortNumber(serverName, out result))
			{
				return result;
			}
			else
			{
				result = GetMainDBServerPortNumber();
			}
			return result;
		}

		public static bool TryParseServerPortNumber(string serverName, out string port)
		{
			port = null;
			string[] splits;
			if (serverName.Contains(Comma)
				&& ((splits = serverName.Split(',')).Length >= 2)
				&& (!(port = Regex.Replace(splits[1], @"\s+", string.Empty)).IsNullOrEmpty()))
			{
				return true;
			}
			return false;
		}

		public void CreateLinkedServer()
		{
			var fallbackToNativeClient = true;

			if (IsWTGExternalSystem && !EnvProxy.IsHostedWithCargowise && IsMSOLEDBSQLProviderInstalled())
			{
				DropLinkedServer();
				CreateLinkedServerWithMSOLEDBSQL();
				fallbackToNativeClient = !IsLinkedServerConnectionAccessible();
			}

			if (fallbackToNativeClient)
			{
				DropLinkedServer();
				CreateLinkedServerWithNativeClient();
				IsLinkedServerConnectionAccessible(throwError: true);
			}
		}

		bool IsWTGExternalSystem
		{
			get
			{
				var registration = ObjectFactory.Get<IProductRegistration>();
				return (isWTGExternalSystem ?? (isWTGExternalSystem = (registration.IsWiseTechGlobalInternalEDISystem() || !registration.IsWiseTechGlobalInternalSystem()))).Value;
			}
		}

		bool? isWTGExternalSystem;

		public static string GetMainDBServerPortNumber()
		{
			string portNumber = DefaultPortNumber;
			using (var connection = Db.NewAdminConnection())
			using (((ICurrentDbControl)connection).UseDatabase(Db.SqlMasterDb))
			{
				portNumber = connection.ExecuteScalar(portNumberQuery).ToString();
			}
			return portNumber;
		}

		public static string GetLinkedServerDataSource(string serverName)
		{
			string dataSrc;
			using (var connection = Db.NewAdminConnection())
			using (((ICurrentDbControl)connection).UseDatabase(Db.SqlMasterDb))
			using (var cmd = connection.Command(serverInfoQuery))
			{
				cmd.AddParameter("@ServerName", SqlDbType.NVarChar, 128, serverName);
				dataSrc = cmd.ExecuteScalar() as string;
			}
			return dataSrc;
		}

		protected void CreateLinkedServerWithNativeClient()
		{
			var query = GetCreateLinkedServerWithNativeClientQuery();
			var providerString = GetProviderString(Provider.NativeClient);

			using (((ICurrentDbControl)biConnection).UseDatabase(Db.SqlMasterDb))
			using (var cmd = biConnection.Command(query))
			{
				cmd.AddParameter("@ProviderString", SqlDbType.NVarChar, 128, providerString);
				cmd.AddParameter("@ServerName", SqlDbType.NVarChar, 128, LinkedServerName);
				cmd.AddParameter("@ServerNameWithPortNumber", SqlDbType.NVarChar, 128, LinkedServerNameWithPortNumber);
				cmd.ExecuteNonQuery();
			}
		}

		protected void CreateLinkedServerWithMSOLEDBSQL()
		{
			var query = GetCreateLinkedServerWithMsOleDBQuery();
			var providerString = GetProviderString(Provider.MSOLEDBSQL);

			using (((ICurrentDbControl)biConnection).UseDatabase(Db.SqlMasterDb))
			using (var cmd = biConnection.Command(query))
			{
				cmd.AddParameter("@ProviderString", SqlDbType.NVarChar, 128, providerString);
				cmd.AddParameter("@ServerName", SqlDbType.NVarChar, 128, LinkedServerName);
				cmd.AddParameter("@ServerNameWithPortNumber", SqlDbType.NVarChar, 128, LinkedServerNameWithPortNumber);
				cmd.ExecuteNonQuery();
			}
		}

		public void DropLinkedServer()
		{
			using (((ICurrentDbControl)biConnection).UseDatabase(Db.SqlMasterDb))
			using (var cmd = biConnection.Command(dropLinkedServer))
			{
				cmd.AddParameter("@ServerName", SqlDbType.NVarChar, 128, LinkedServerName);
				cmd.ExecuteNonQuery();
			}
		}

		public bool IsLinkedServerConnectionAccessible(bool throwError = false)
		{
			var result = false;
			try
			{
				var sqlText = $"SELECT * FROM OPENQUERY([{LinkedServerName}], 'SELECT 1')";
				biConnection.ExecuteNonQuery(sqlText);
				result = true;
			}
			catch (SqlException ex)
			{
				if (throwError)
				{
					throw;
				}
				else
				{
					LogInfo(Res.GetString("6269f27b-cb3b-4037-ac11-185c73ce29ba", "Creating linked server failed for {0} with the below message:\r\n{1}", new string[] { LinkedServerNameWithoutPortNumber, ex.Message }));
				}
			}

			return result;
		}

		public void LogInfo(string message)
		{
			logger?.Log(LogType.Debug, message);
			upgradeTaskWorkflowLogger?.ShowInfoMessage(message);
		}

		public bool IsMSOLEDBSQLProviderInstalled()
		{
			return Convert.ToBoolean(biConnection.ExecuteScalar(doesMSOLEDBSQLProviderExist), CultureInfo.InvariantCulture);
		}

		#region SuppressResourceStringsCheckRegion

		string GetProviderString(Provider provider)
		{
			// The value for User Id will not have any effect but it must be provided to ensure that non-sysadmin users can connect. https://learn.microsoft.com/en-us/archive/blogs/mdegre/access-to-the-remote-server-is-denied-because-no-login-mapping-exists
			const string userID = "User ID=Self";

			switch (provider)
			{
				case Provider.MSOLEDBSQL:
					return $"{multiSubnetFailoverYes} {userID}"; // SQL Query Parameter
				case Provider.NativeClient:
					return $" {userID}"; // SQL Query Parameter
				default:
					return string.Empty;
			}
		}

		protected string GetCreateLinkedServerWithMsOleDBQuery()
		{
			var query = $@"
			IF NOT EXISTS (SELECT NULL FROM sys.servers WHERE name = @ServerName)
			BEGIN
							EXEC master.dbo.sp_MSset_oledb_prop N'MSOLEDBSQL', N'AllowInProcess', 1;
							EXEC dbo.sp_addlinkedserver @server = @ServerName, @srvproduct=N'', @provider=N'MSOLEDBSQL', {(IsWTGExternalSystem ? providerStrParam : string.Empty)}@datasrc = @ServerNameWithPortNumber;
			END";

			return string.Concat(query, System.Environment.NewLine, ensureServerOptionsQuery);
		}

		protected string GetCreateLinkedServerWithNativeClientQuery()
		{
			var query = $@"
			IF NOT EXISTS (SELECT NULL FROM sys.servers WHERE name = @ServerName)
			BEGIN
							EXEC dbo.sp_addlinkedserver @server = @ServerName, @srvproduct=N'', @provider=N'SQLNCLI', {(IsWTGExternalSystem ? providerStrParam : string.Empty)}@datasrc = @ServerNameWithPortNumber;
							EXEC dbo.sp_serveroption @server = @ServerName, @optname = N'connect timeout', @optvalue = N'60';
			END";

			return string.Concat(query, System.Environment.NewLine, ensureServerOptionsQuery);
		}

		public enum Provider
		{
			MSOLEDBSQL,
			NativeClient
		}

		const string multiSubnetFailoverYes = "MultiSubnetFailover=YES;";
		const string providerStrParam = "@provstr = @ProviderString, ";

		const string doesMSOLEDBSQLProviderExist = @"
                                DECLARE @providers TABLE ([name] NVARCHAR(100), [guid] NVARCHAR(100) NULL, [description] NVARCHAR(100) NULL);
                                INSERT INTO @providers EXEC master.sys.sp_enum_oledb_providers;
                                IF EXISTS(SELECT NULL FROM @providers WHERE NAME = 'MSOLEDBSQL') SELECT 1 ELSE SELECT 0;";

		const string dropLinkedServer = @"
                                                IF EXISTS (SELECT NULL FROM sys.servers WHERE name = @ServerName AND is_linked = 1)
                                                BEGIN
                                                                EXEC dbo.sp_dropserver @server=@ServerName, @droplogins='droplogins';
                                                END";

		const string ensureServerOptionsQuery = @"
                                                EXEC dbo.sp_addlinkedsrvlogin @rmtsrvname = @ServerName, @locallogin = NULL , @useself = N'True', @rmtuser = N'';
                                                EXEC dbo.sp_serveroption @server = @ServerName, @optname = N'data access', @optvalue = N'True';
                                                EXEC dbo.sp_serveroption @server = @ServerName, @optname = N'remote proc transaction promotion', @optvalue = N'False';
                                                EXEC dbo.sp_serveroption @server = @ServerName, @optname = N'rpc', @optvalue = N'True'
                                                EXEC dbo.sp_serveroption @server = @ServerName, @optname = N'rpc out', @optvalue = N'True'";

		const string portNumberQuery = "SELECT TOP 1 port FROM [sys].[dm_tcp_listener_states] WHERE type_desc = 'TSQL' ORDER BY listener_id ASC";

		const string serverInfoQuery = "SELECT data_source FROM [sys].[servers] WHERE is_linked = 1 AND name = @ServerName";
		#endregion
	}
}
