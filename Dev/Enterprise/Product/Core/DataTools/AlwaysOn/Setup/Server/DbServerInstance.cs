using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Common;
using CargoWise.DataProtection;
using CargoWise.DataProtection.Administration.SqlServer;
using static System.FormattableString;

namespace Enterprise.AlwaysOn.Setup
{
	public abstract class DbServerInstance : IDbServerInstance
	{
		protected DbServerInstance(SqlServerInfo serverInfo)
		{
			this.serverInfo = serverInfo;
		}

		public void Load()
		{
			lastErrorMessage = null;
			try
			{
				var sqlContext = GetSqlExecutionContextForServer();

				odysseyAdminLogin = DbSecurity.GetDbLoginInfo(sqlContext, OdysseyAdminCredentials.AdminUserName, true);

				LoadUnsafe(sqlContext);
				OnSuccessfulLoad(sqlContext);

				isLoaded = true;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				string errorMessage = "";
				if (serverInfo is not null)
				{
					errorMessage = Invariant($"Server={serverInfo.ServerAlias} - ");
				}
				errorMessage += ex is AlwaysOnException ? ex.Message : ex.ToString();

				lastErrorMessage = errorMessage;
				isLoaded = false;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		protected virtual string GetWindowsServerVersion(ISqlExecutionContext sqlContext)
		{
			return (string)sqlContext.ExecuteScalar("SELECT windows_release FROM sys.dm_os_windows_info");
		}

		bool IsWindowsServerForAlwaysOn(ISqlExecutionContext sqlContext)
		{
			return Convert.ToDouble(GetWindowsServerVersion(sqlContext), CultureInfo.InvariantCulture) >= 6.3; // 6.3 is the version of Windows Server 2012 R2
		}

		void OnSuccessfulLoad(ISqlExecutionContext sqlContext)
		{
			ChangeInstanceLevelAuthorizationsToSysAdmin(sqlContext);
			InvokeAdditionalTasksAfterLoad(sqlContext);
		}

		protected virtual void ChangeInstanceLevelAuthorizationsToSysAdmin(ISqlExecutionContext sqlContext)
		{
			try
			{
				DbSecurity.ChangeInstanceLevelAuthorizationsToSysAdmin(sqlContext, odysseyAdminLogin.LoginName);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				// no non-critical exceptions is expected here, any exceptions should be logged when a logger is implemented
			}
		}

		protected virtual void InvokeAdditionalTasksAfterLoad(ISqlExecutionContext sqlContext)
		{
		}

		protected virtual void LoadUnsafe(ISqlExecutionContext sqlContext)
		{
			serverInfo.LoadDetailsFromServer(sqlContext);

			if (!IsWindowsServerForAlwaysOn(sqlContext))
			{
				throw new AlwaysOnException("AlwaysOn requires Windows Server 2012 R2 or later version.");
			}

			if (!IsAlwaysOnHighAvailabilityEnabled(sqlContext))
			{
				throw new AlwaysOnException("This database server instance is not enabled for AlwaysOn High Availability. Please refer to release notes for instructions on how to enable it.");
			}

			cluster = GetFailoverCluster(sqlContext);
			if (cluster == null)
			{
				throw new AlwaysOnException("This server is not part of a failover cluster configuration.");
			}

			alwaysOnEndpointPort = GetEndpointPort(sqlContext);
			isFailoverClusterInstance = IsClusteredSqlServerInstance(sqlContext);
		}

		#region IDbServerInstance Members

		SqlServerInfo IDbServerInstance.ServerInfo
		{
			get { return serverInfo; }
		}
		protected readonly SqlServerInfo serverInfo;

		IFailoverCluster IDbServerInstance.FailoverCluster
		{
			get { return cluster; }
		}
		IFailoverCluster cluster;

		int IDbServerInstance.AlwaysOnEndpointPort
		{
			get { return alwaysOnEndpointPort; }
		}
		protected int alwaysOnEndpointPort;

		bool IDbServerInstance.IsFailoverClusterInstance
		{
			get { return isFailoverClusterInstance; }
		}
		protected bool isFailoverClusterInstance;

		string IValidationStatus.LastErrorMessage
		{
			get { return lastErrorMessage; }
		}
		protected string lastErrorMessage;

		bool IValidationStatus.IsLoaded
		{
			get { return isLoaded; }
		}
		bool isLoaded;

		bool IValidationStatus.HasErrors
		{
			get { return !string.IsNullOrWhiteSpace(lastErrorMessage); }
		}

		DbLoginInfo IDbServerInstance.OdysseyAdminLogin
		{
			get { return odysseyAdminLogin; }
		}
		protected DbLoginInfo odysseyAdminLogin;

		#endregion // IDbServerInstance Members

		#region Load Data

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		bool IsClusteredSqlServerInstance(ISqlExecutionContext sqlContext)
		{
			return Convert.ToBoolean(sqlContext.ExecuteScalar("SELECT SERVERPROPERTY ('IsClustered')"));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		bool IsAlwaysOnHighAvailabilityEnabled(ISqlExecutionContext sqlContext)
		{
			var result = sqlContext.ExecuteScalar("SELECT SERVERPROPERTY ('IsHadrEnabled')");
			return Convert.ToBoolean(result);
		}

		IFailoverCluster GetFailoverCluster(ISqlExecutionContext sqlContext)
		{
			var clusterNameObj = sqlContext.ExecuteScalar("SELECT TOP (1) cluster_name FROM sys.dm_hadr_cluster");

			if (clusterNameObj == null || clusterNameObj == DBNull.Value)
			{
				return null;
			}

			var sql = "SELECT member_name FROM sys.dm_hadr_cluster_members WHERE member_type = 0";
			var clusterNodes = new List<string>();

			using (var reader = sqlContext.ExecuteReader(sql))
			{
				while (reader.Read())
				{
					var readerObj = reader[0] ?? throw new InvalidOperationException("member_name column returned a null value");

					clusterNodes.Add(readerObj.ToString());
				}
			}

			return FailoverClusterFactory.New(clusterNameObj.ToString(), clusterNodes);
		}

		int GetEndpointPort(ISqlExecutionContext sqlContext)
		{
			const string getDbMirroringEndpointSql = "SELECT ep.port FROM sys.tcp_endpoints ep WHERE ep.[type] = 4";

			var objResult = sqlContext.ExecuteScalar(getDbMirroringEndpointSql);
			return (objResult == null) ? 0 : Convert.ToInt32(objResult);
		}

		#endregion

		#region Security (Endpoints and Logins)

		/// <summary>
		/// Specify the Endpoint URL When Adding or Modifying an Availability Replica
		/// http://msdn.microsoft.com/en-us/library/ff878441.aspx
		/// Create a Database Mirroring Endpoint for Windows Authentication
		/// http://msdn.microsoft.com/en-us/library/ms190456.aspx
		/// Use Certificates for a Database Mirroring Endpoint 
		/// http://msdn.microsoft.com/en-us/library/ms191477.aspx
		/// Allow Network Access to a Database Mirroring Endpoint Using Windows Authentication
		/// http://msdn.microsoft.com/en-us/library/ms178029.aspx
		/// </summary>
		protected void CreateMirroringEndpoint(ISqlExecutionContext sqlContext, int endpointPort)
		{
			var createDbMirroringEndpointSql = Invariant($@"
CREATE ENDPOINT [Hadr_endpoint]
	STATE=STARTED
	AS TCP (LISTENER_PORT = {endpointPort})
	FOR DATABASE_MIRRORING (ENCRYPTION = REQUIRED ALGORITHM AES, ROLE = ALL);
");

			sqlContext.ExecuteNonQuery(createDbMirroringEndpointSql);
		}

		/// <summary>
		/// Starts endpoint (Hadr_endpoint), if not already.
		/// Grants endpoint access to windows login running SQL service.
		/// Starts event session (AlwaysOn_health), if not already.
		/// </summary>
		protected void EnsureEndpointCommunication(ISqlExecutionContext sqlContext, string sqlServiceAccount)
		{
			StartEndpoint(sqlContext);
			GrantEndpointPermission(sqlContext, sqlServiceAccount);
			StartAlwaysOnHealthEventSession(sqlContext);
		}

		void StartEndpoint(ISqlExecutionContext sqlContext)
		{
			var sqlText = Invariant($@"
IF (SELECT state FROM sys.endpoints WHERE name = N'{"Hadr_endpoint"}') <> 0
	ALTER ENDPOINT [{"Hadr_endpoint"}] STATE = STARTED;
");

			sqlContext.ExecuteNonQuery(sqlText);
		}

		void GrantEndpointPermission(ISqlExecutionContext sqlContext, string sqlServiceAccount)
		{
			var sqlText = Invariant($@"
IF not exists(SELECT null FROM sys.server_principals WHERE name = '{sqlServiceAccount}')
	CREATE LOGIN [{sqlServiceAccount}] FROM WINDOWS;
GRANT CONNECT ON ENDPOINT::[Hadr_endpoint] TO [{sqlServiceAccount}];
");

			sqlContext.ExecuteNonQuery(sqlText);
		}

		void StartAlwaysOnHealthEventSession(ISqlExecutionContext sqlContext)
		{
			var sqlText = Invariant($@"
IF EXISTS(SELECT null FROM sys.server_event_sessions WHERE name = '{"AlwaysOn_health"}')
	ALTER EVENT SESSION [{"AlwaysOn_health"}] ON SERVER WITH (STARTUP_STATE = ON);
IF NOT EXISTS(SELECT null FROM sys.dm_xe_sessions WHERE name='{"AlwaysOn_health"}')
	ALTER EVENT SESSION [{"AlwaysOn_health"}] ON SERVER STATE=START;
");

			sqlContext.ExecuteNonQuery(sqlText);
		}
		#endregion // Database Security (Endpoints and Logins)

		protected virtual ISqlExecutionContext GetSqlExecutionContextForServer() => Program.SqlContextManager.GetSqlExecutionContext(serverInfo);
	}
}
