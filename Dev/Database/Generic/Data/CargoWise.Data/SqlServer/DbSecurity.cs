using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.DataProtection;

namespace CargoWise.Data
{
	#region SuppressResourceStringsCheckRegion
	public class DbSecurity
	{
		public const string SqlHrmSchema = "hrm"; // Schema name constant, should not use Res.GetString
		public const string SqlCdcSchema = "cdc"; // Schema name constant, should not use Res.GetString
		public const string SqlStagingSchema = "Staging"; // Schema name constant, should not use Res.GetString

		#region SERVER LEVEL

		#region EFFECTIVE DB SECURITY MODE

		/// <summary>
		/// Checks if client is effectively in OPEN database security mode,
		/// regardless of what it says in their local licence key info and on our records.
		/// </summary>
		public bool? IsDatabaseSecurityOpen()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				var disallowedRoleMembers = GetDisallowedServerRoleMembers(adminConnection, true);
				if (disallowedRoleMembers.Count > 0)
				{
					return true;
				}
			}

			try
			{
				using (var saConnection = Db.NewExtraConnection(Db.ServerName, Db.SqlMasterDb, Db.SysAdminUserLogin, GetSaPwd()))
				{
					saConnection.EnsureIsOpen();
					return false;
				}
			}
			catch (SqlException ex)
			{
				if (new DbErrorMatch(ex).ExceptionType == DbErrorType.LoginFailedForUser)
				{
					// If login fails => SA password is client owned
					return true;
				}
				else
				{
					return null;
				}
			}
		}

		public struct ServerRoleMembership
		{
			public ServerRoleMembership(string roleName, string loginName, string loginType)
			{
				this.roleName = roleName;
				this.loginName = loginName;
				this.loginType = loginType;
			}

			public string RoleName { get { return roleName; } }
			readonly string roleName;
			public string LoginName { get { return loginName; } }
			readonly string loginName;
			public string LoginType { get { return loginType; } }
			readonly string loginType;
		}

		/// <summary>
		/// MADE VIRTUAL FOR TESTING PURPOSES ONLY
		/// </summary>
		protected virtual string GetSaPwd()
		{
			return Db.SaValue;
		}

		#endregion // EFFECTIVE DB SECURITY MODE

		/// <summary>
		/// Retrives logins other than our ones which have admin access to the server.
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="allowDbCreator"></param>
		/// <returns>DataTable (RoleName, LoginName, LoginType)</returns>
		protected IList<ServerRoleMembership> GetDisallowedServerRoleMembers(AdminConnection connection, bool allowDbCreator = false)
		{
			Argument.NotNull(connection, nameof(connection));

			var sql = string.Format(CultureInfo.InvariantCulture, @"
SELECT
	RoleName  = sr.name,
	LoginName = sl.name,
	LoginType = sl.type_desc
FROM
	sys.server_role_members    AS srm
	JOIN sys.server_principals AS sr  ON sr.principal_id = srm.role_principal_id
	JOIN sys.server_principals AS sl  ON sl.principal_id = srm.member_principal_id
WHERE
	sl.name NOT in ('{0}', '{1}', '{2}')
	AND sr.name NOT in ('serveradmin', 'processadmin', 'diskadmin')
	{3}
"
				, SysAdminCredentials.SysAdminUserName
				, OdysseyAdminCredentials.AdminUserName
				, GetSqlAgentServiceAccountName(connection)
				, (allowDbCreator) ? "AND sr.name NOT in ('dbcreator', 'setupadmin')" : ""
				);

			var disallowedRoleMemberTable = DataUtils.GetDataTableFromQuery(connection, sql);

			var disallowedRoleMembers = (
				from roleMember in disallowedRoleMemberTable.AsEnumerable()
				select new ServerRoleMembership(roleMember["RoleName"].ToString(), roleMember["LoginName"].ToString(), roleMember["LoginType"].ToString()));

			return disallowedRoleMembers?.ToList() ?? new List<ServerRoleMembership>();
		}

		protected string GetSqlAgentServiceAccountName(DbConnection connection)
		{
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot

			var instanceName = connection.ServerInstanceName;
			return
				"NT Service\\SQL" +
				(string.IsNullOrWhiteSpace(instanceName)
					? "SERVERAGENT"
					: ("AGENT$" + instanceName));
		}

		#endregion // SERVER LEVEL

		#region DATABASE LEVEL

		public static ReadOnlyCollection<string> CommonPermissionList => new ReadOnlyCollection<string>(new string[] { "CONNECT" });

		public static ReadOnlyCollection<string> ReadOnlyDBUserPermissionList => new ReadOnlyCollection<string>(new string[] { "EXECUTE", "SELECT" });

		public static ReadOnlyCollection<string> DatabaseDeveloperPermissionList => new ReadOnlyCollection<string>(new string[] { "CREATE TYPE", "CREATE SCHEMA", "CREATE VIEW", "CREATE FUNCTION",
			"CREATE PROCEDURE", "CREATE TABLE", "INSERT", "UPDATE", "DELETE", "REFERENCES" });

		#region Refresh Database Reader Role

		internal protected string LoadAuditServer(AdminConnection connection)
		{
			if (!isAuditServerLoaded)
			{
				lock (DbRegistry.BiAuditServer)
				{
					try
					{
						if (auditServerNameCache == null)
						{
							auditServerNameCache = DbRegistry.BiAuditServer.LoadValue(connection);
#if DEBUG
							if (auditServerNameCache == null)
							{
								auditServerNameCache = connection.ServerName;
							}
#endif
						}
					}
					catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.InvalidObjectName)
					{ }
				}
				isAuditServerLoaded = true;
			}

			return auditServerNameCache;
		}
		bool isAuditServerLoaded;
		string auditServerNameCache;

		internal protected string LoadDataWarehouseServer(AdminConnection connection)
		{
			if (!isDwServerLoaded)
			{
				lock (DbRegistry.BiDataWarehouseServer)
				{
					try
					{
						if (dwServerNameCache == null)
						{
							dwServerNameCache = DbRegistry.BiDataWarehouseServer.LoadValue(connection);
#if DEBUG
							if (dwServerNameCache == null)
							{
								dwServerNameCache = connection.ServerName;
							}
#endif
						}
					}
					catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.InvalidObjectName)
					{ }
				}
				isDwServerLoaded = true;
			}

			return dwServerNameCache;
		}
		bool isDwServerLoaded;
		string dwServerNameCache;

		/// <summary>
		/// [cwReaderRole]
		/// - On all databases
		///   + Creates it if does not exist
		///   + Add it as a member of db_datareader
		///   + Grants it VIEW DEFINITION rights
		/// - On the main database only
		///   + Grants it EXECUTE rights on:
		///     + All Aggregate function CLR (AF)
		///     + All SQL scalar function (FN)
		///     + All Assembly CLR scalar-function (FS)
		///     + Selected safe/read-only Stored Procedures
		/// </summary>
		public void RefreshDbReaderRolePermissions(AdminConnection connection, Action<string> logMessage)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNull(logMessage, nameof(logMessage));

			if (connection.IsInTransactionOtherThanTransactionedTestCase)
			{
				throw new InvalidOperationException("Do not attempt to apply login security changes in a transaction as this can result in cross-system locking.");
			}

			var mainDbName = ((ICurrentDbControl)connection).InitialDatabase.Trim();

			var dbNames = connection.GetDatabases(DatabaseType.AllWritable, writable: true);
			foreach (var dbName in dbNames)
			{
				RefreshDbReaderRoleOnDatabase(connection, mainDbName, dbName, logMessage);
			}

			var auditServer = LoadAuditServer(connection);
			RefreshDbReaderRoleOnBiDatabase(mainDbName, auditServer, Db.AuditDatabaseName, logMessage);

			var dwServer = LoadDataWarehouseServer(connection);
			RefreshDbReaderRoleOnBiDatabase(mainDbName, dwServer, Db.EdwDatabaseName, logMessage);
		}

		void RefreshDbReaderRoleOnDatabase(AdminConnection connection, string mainDbName, string dbName, Action<string> logMessage)
		{
			if (connection.IsDbWriteableAndOnline(dbName))
			{
				var shouldGrantExecuteRights = string.Equals(dbName, mainDbName, StringComparison.OrdinalIgnoreCase);

				if (RefDbTableNameResolver.IsSharedDatabase(dbName))
				{
					using (((ICurrentDbControl)connection).UseDatabase(dbName))
					{
						RefreshDbReaderRoleOnDatabase(connection, mainDbName, dbName, shouldGrantExecuteRights, logMessage);
					}
				}
				else
				{
					RefreshDbReaderRoleOnDatabase(connection, mainDbName, dbName, shouldGrantExecuteRights, logMessage);
				}
			}
		}

		void RefreshDbReaderRoleOnBiDatabase(string mainDbName, string biServer, string biDbName, Action<string> logMessage)
		{
			if (!string.IsNullOrEmpty(biServer))
			{
				try
				{
					using (var biConnection = Db.NewAdminConnection(biServer, biDbName))
					{
						RefreshDbReaderRoleOnDatabase(biConnection, mainDbName, biDbName, logMessage);
					}
				}
				catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.DatabaseDoesNotExist)
				{
				}
			}
		}

		public void RefreshDbReaderRoleOnDatabase(DbConnection connection, string mainDbName, string dbName, bool shouldGrantExecuteRights, Action<string> logMessage)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(dbName, nameof(dbName));
			Argument.NotNull(logMessage, nameof(logMessage));

			new CwReaderRole().GetRefreshRoleSQL(connection, mainDbName, dbName, shouldGrantExecuteRights, out StringBuilder cmdBuilder);

			var stmt = cmdBuilder.ToString();
			if (stmt.Length > 0)
			{
				if (stmt.SplitByLine().Count() > 1) //ignoring the one guaranteed line unrelated to the role
				{
					logMessage(string.Format(CultureInfo.InvariantCulture, "Role [{0}] permissions updated on database [{1}].", CwReaderRole, dbName));
				}
				var sqlBatch = string.Format(CultureInfo.InvariantCulture, "EXEC {0}.sys.sp_executesql N'{1}';", dbName.QuoteName(), stmt.QuoteEscapedName('\''));

				ExecuteInSqlTryCatch(connection, sqlBatch);
			}
		}

		public const string CwReaderRole = DbRoleTypes.CwReaderRole;

		#endregion // Refresh Database Reader Role

		#region Refresh Schema Database Roles

		public void RefreshSchemaDbRoles(AdminConnection connection, Action<string> logMessage)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNull(logMessage, nameof(logMessage));

			if (connection.IsInTransactionOtherThanTransactionedTestCase)
			{
				throw new InvalidOperationException("Do not attempt to apply login security changes in a transaction as this can result in cross-system locking.");
			}

			var dbNames = connection.GetDatabases(DatabaseType.AllWritable, writable: true);
			var dbCwRoles = DbRoleTypes.AllDbRoles.Where(role => !(role is CwReaderRole)).ToArray();

			foreach (var dbName in dbNames)
			{
				RefreshSchemaDbRoleOnDatabase(connection, dbName, dbCwRoles, logMessage);
			}

			var auditServer = LoadAuditServer(connection);
			RefreshSchemaDbRoleOnBiDatabase(auditServer, Db.AuditDatabaseName, dbCwRoles, logMessage);

			var dwServer = LoadDataWarehouseServer(connection);
			RefreshSchemaDbRoleOnBiDatabase(dwServer, Db.EdwDatabaseName, dbCwRoles, logMessage);
		}

		void RefreshSchemaDbRoleOnBiDatabase(string biServer, string biDbName, DbRole[] dbCwRoles, Action<string> logMessage)
		{
			if (!string.IsNullOrEmpty(biServer))
			{
				try
				{
					using (var biConnection = Db.NewAdminConnection(biServer, biDbName))
					{
						RefreshSchemaDbRoleOnDatabase(biConnection, biDbName, dbCwRoles, logMessage);
					}
				}
				catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.DatabaseDoesNotExist)
				{
				}
			}
		}

		void RefreshSchemaDbRoleOnDatabase(AdminConnection connection, string dbName, DbRole[] dbCwRoles, Action<string> logMessage)
		{
			if (connection.IsDbWriteableAndOnline(dbName))
			{
				using (((ICurrentDbControl)connection).UseDatabase(dbName))
				{
					foreach (var dbRole in dbCwRoles)
					{
						RefreshSchemaDbRoleOnDatabase(connection, dbName, dbRole, logMessage);
					}
				}
			}
		}

		void RefreshSchemaDbRoleOnDatabase(DbConnection connection, string dbName, DbRole dbRole, Action<string> logMessage)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(dbName, nameof(dbName));
			Argument.NotNull(dbRole, nameof(dbRole));
			Argument.NotNull(logMessage, nameof(logMessage));

			dbRole.CreateRoleAndPermissionGrantCommandsIfRequired(connection, dbName, out var cmdBuilder);

			var stmt = cmdBuilder.ToString().Trim();
			if (stmt.Length > 0)
			{
				ExecuteInSqlTryCatch(connection, stmt.QuoteEscapedName('\''));
				logMessage(string.Format(CultureInfo.InvariantCulture, "Role [{0}] permissions updated on database [{1}].", dbRole.Name, dbName));
			}
		}

		#endregion

		#endregion // DATABASE LEVEL

		#region MSDB 

		public static bool LockDownMsdbAccessOnServer(AdminConnection connection, string serverName, IEnumerable<string> loginsToCheck)
		{
			var dbRole = DbRoleTypes.CwMsdbAccessDeniedRole;

			var result = false;
			using (((ICurrentDbControl)connection).UseDatabase(Db.SqlMsdb))
			{
				// create msdb role with permissions
				dbRole.EnsureExists(connection, Db.SqlMsdb);

				// add all staff and application logins from the main db to the msdb role
				foreach (var login in loginsToCheck)
				{
					if (!SqlSecurityUtils.LoginSidMatchesDbUserSid(connection, login)) // check if dbUser is valid
					{
						if (!SqlSecurityUtils.Login.Exists(connection, login)) // if login does not exist - skip it
						{
							continue;
						}

						result = true;

						if (SqlSecurityUtils.Login.TryToGetAssociatedDbUser(connection, Db.SqlMsdb, login, out var associatedDbUser)
							&& !login.Equals(associatedDbUser, StringComparison.OrdinalIgnoreCase))
						{
							SqlSecurityUtils.DbUser.Drop(connection, dbUserName: associatedDbUser);
						}

						if (SqlSecurityUtils.DbUser.Exists(connection, login)) // if invalid dbUser exits - drop it
						{
							SqlSecurityUtils.DbUser.Drop(connection, dbUserName: login);
						}

						SqlSecurityUtils.DbUser.Create(connection, dbUserName: login);

						SqlSecurityUtils.DbRole.AddMember(connection, dbRoleName: dbRole.Name, dbPrincipalName: login);
					}
					else if (!SqlSecurityUtils.DbRole.Contains(connection, dbRole.Name, login))
					{
						result = true;

						SqlSecurityUtils.DbRole.AddMember(connection, dbRoleName: dbRole.Name, dbPrincipalName: login);
					}
				}

				CleanUpDbUsersWithoutLogin(connection);
			}

			return result;
		}

		static void CleanUpDbUsersWithoutLogin(AdminConnection connection)
		{
			var loginsToDrop = new List<string>();
			var sql = @"
SELECT
	dp.name
FROM
	sys.database_principals         AS dp
	LEFT JOIN sys.server_principals AS sp ON sp.name = dp.name COLLATE database_default
		AND sp.sid = dp.sid
WHERE 1=1
	AND dp.type in ('U', 'S')
	AND dp.name NOT in (N'dbo', N'guest', N'INFORMATION_SCHEMA', N'sys', N'MS_DataCollectorInternalUser')
	AND sp.name is NULL
ORDER BY
	dp.name

";
			connection.ExecuteReader(sql, reader => loginsToDrop.Add((string)reader["name"]));
			foreach (var login in loginsToDrop)
			{
				SqlSecurityUtils.DbUser.Drop(connection, dbUserName: login);
			}
		}

		#endregion // MSDB

		#region Implementation

		protected virtual bool IsSharedDatabase(string dbName)
		{
			return RefDbTableNameResolver.IsSharedDatabase(dbName);
		}

		void ExecuteInSqlTryCatch(DbConnection connection, string sqlCommand)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(sqlCommand, nameof(sqlCommand));

			var tryCacheWrappedCmd = dbTryCatchWrapper.ExecuteInSqlTryCatch(sqlCommand);
			connection.ExecuteNonQuery(tryCacheWrappedCmd);
		}

		readonly DbTryCatchWrapper dbTryCatchWrapper = new DbTryCatchWrapper();

		#endregion // Implementation
	}

	#endregion // SuppressResourceStringsCheckRegion
}
