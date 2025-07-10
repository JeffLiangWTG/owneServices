namespace Enterprise.StlAnalysis.Load
{
	#region SuppressResourceStringsCheckRegion
	class SqlServerInfo
	{
		#region Factory Methods

		public static SqlServerInfo NewClientSpecificSqlServerInfo(HostedClient client)
		{
			string serverName = !string.IsNullOrEmpty(client.OfflineServerName) ? client.OfflineServerName : client.SystemCode + ".db.wisegrid.net";
			string databaseName = !string.IsNullOrEmpty(client.OfflineDatabaseName) ? client.OfflineDatabaseName : "Odyssey" + client.SystemCode; // Stand-alone tool calculating DB name based on WiseCloud convention

			return NewClientSpecificSqlServerInfo(serverName, databaseName);
		}

		public static SqlServerInfo NewClientSpecificSqlServerInfo(string serverName, string databaseName)
		{
			return new SqlServerInfo(serverName, databaseName, loginName: null, loginPwd: null) { UseRestrictedReaderLoginCredentials = true };
		}

		/// <summary>
		/// Uses IntegratedSecurity = TRUE (Windows Authentication) by passing a null loginName.
		/// </summary>
		public static SqlServerInfo NewEdiProdSqlServerInfo()
		{
			string serverName = EdiProdServer;
			string databaseName = EdiProdDbName;
			return new SqlServerInfo(serverName, databaseName, loginName: null, loginPwd: null);
		}

		/// <summary>
		/// Uses IntegratedSecurity = TRUE (Windows Authentication) by passing a null loginName.
		/// </summary>
		public static SqlServerInfo NewEdiFaxDbSqlServerInfo()
		{
			string serverName = EdiProdServer;
			string databaseName = "EDIFaxDB";
			return new SqlServerInfo(serverName, databaseName, loginName: null, loginPwd: null);
		}

		public static SqlServerInfo NewDeniedPartyScreeningSqlServerInfo()
		{
			return new SqlServerInfo("syddps.db.wtg.zone", "DpsAuditTransactions", "eHubUser", "eHubUser");
		}

		const string EdiProdServer = "ediProdReadOnly.db.wtg.zone";
		const string EdiProdDbName = "ediProd";

		#endregion

		SqlServerInfo(string serverName, string databaseName, string loginName, string loginPwd)
		{
			this.serverName = serverName;
			this.databaseName = databaseName;
			this.loginName = loginName;
			this.loginPwd = loginPwd;
		}

		public override string ToString()
		{
			return ServerName;
		}

		public string ServerName { get { return serverName; } }
		readonly string serverName;

		public string DatabaseName { get { return databaseName; } }
		readonly string databaseName;

		public string LoginName { get { return loginName; } }
		readonly string loginName;

		public string LoginPwd { get { return loginPwd; } }
		readonly string loginPwd;
		public bool UseRestrictedReaderLoginCredentials { get; private set; }
	}
	#endregion
}
