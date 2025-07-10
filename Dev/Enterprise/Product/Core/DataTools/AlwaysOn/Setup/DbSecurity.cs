using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.DataProtection.Administration.SqlServer;
using static System.FormattableString;

namespace Enterprise.AlwaysOn.Setup
{
	internal static class DbSecurity
	{
		static string CreateOneTimeUsePassword()
		{
			const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890.,;/)(*&^%$#@!`~<>?";
			var res = new StringBuilder();
			var rnd = new Random();
			for(int i = 0; i < 20; i++)
			{
				res.Append(valid[rnd.Next(valid.Length)]);
			}
			return res.ToString();
		}

		internal static void PropagateOdysseyAdminLogin(SqlServerInfo sqlServerInfo, DbLoginInfo primaryAdminLoginInfo)
		{
			var sqlContext = Program.SqlContextManager.GetSqlExecutionContext(sqlServerInfo);

			var odysseyAdminLogin = primaryAdminLoginInfo.LoginName;
			var existLoginSid = QueryLoginSid(sqlContext, odysseyAdminLogin);
			if (string.Equals(existLoginSid, primaryAdminLoginInfo.LoginSid, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}

			var helperLoginName = primaryAdminLoginInfo.LoginName + "_HelperLogin";
			var helperLoginPassword = CreateOneTimeUsePassword();

			var standByLogin = primaryAdminLoginInfo.LoginName + "_StandbyLogin";

			AlterAuthorizationsAndDropLoginIfExists(sqlContext, helperLoginName, SysAdminUserLogin);
			AlterAuthorizationsAndDropLoginIfExists(sqlContext, standByLogin, SysAdminUserLogin);

			// create a helper login to assist with the propagation process
			CreateAdminLogin(sqlContext, helperLoginName, helperLoginPassword, hashed: false);

			try
			{
				sqlContext = null;
				// close our own session
				Program.SqlContextManager.CurrentExecutionScope.DisposeAllConnections();

				// use the newly created helper to transfer the ownership of databases, drop exist OdysseyAdmin, and rename the standby to OdysseyAdmin
				var csBuilder = new SqlConnectionStringBuilder();
				AlwaysOnSqlExecutionContextManager.ConfigureConnectionString(csBuilder, sqlServerInfo.AliasDataSource);
				csBuilder.UserID = helperLoginName;
				csBuilder.Password = helperLoginPassword;
#pragma warning disable CW1116 // The existing CargoWise.Data.Db.Connection should be used rather than creating a new one - think twice if you really need a new connection.
				var helperConnection = new SqlConnection(csBuilder.ConnectionString);
#pragma warning restore CW1116
				helperConnection.Open();
				var helperSqlContext = new SqlExecutionContext(helperConnection, null);

				// create a standby login so that it can be renamed to the OdysseyAdmin login
				var loginHavingSameSid = (string)helperSqlContext.ExecuteScalar(Invariant($"SELECT name FROM sys.sql_logins WHERE sid = {primaryAdminLoginInfo.LoginSid}"));
				if (!string.IsNullOrEmpty(loginHavingSameSid))
				{
					RenameLoginIfExists(helperSqlContext, loginHavingSameSid, standByLogin);
				}
				else
				{
					// create a standby login
					CreateAdminLogin(helperSqlContext, standByLogin, primaryAdminLoginInfo.PwdHash, hashed: true, primaryAdminLoginInfo.LoginSid);
				}

				// change authorization on endpoints to sa
				ChangeEndPointsAuthorizationToLogin(helperSqlContext, odysseyAdminLogin, SysAdminUserLogin);

				// change authorization on availability groups to sa
				ChangeAvailabilityGroupsAuthorizationToLogin(helperSqlContext, odysseyAdminLogin, SysAdminUserLogin);

				AlterLoginEnable(helperSqlContext, odysseyAdminLogin, false);
				RenameLoginIfExists(helperSqlContext, odysseyAdminLogin);
				RenameLoginIfExists(helperSqlContext, standByLogin, odysseyAdminLogin);
				helperSqlContext = null;
			}
			finally
			{
				Program.SqlContextManager.CurrentExecutionScope.DisposeAllConnections();
				sqlContext = Program.SqlContextManager.GetSqlExecutionContext(sqlServerInfo);

				AlterAuthorizationsAndDropLoginIfExists(sqlContext, helperLoginName, odysseyAdminLogin);
				AlterAuthorizationsAndDropLoginIfExists(sqlContext, standByLogin, odysseyAdminLogin);
				AlterLoginEnable(sqlContext, odysseyAdminLogin);
			}
		}

		internal static void AlterAuthorizationsAndDropAlwaysOnOldLoginIfExists(ISqlExecutionContext sqlContext, string newOwnerLogin)
		{
			AlterAuthorizationsAndDropLoginIfExists(sqlContext, GetAlwaysOnOldLoginName(newOwnerLogin), newOwnerLogin);
		}

		internal static void AlterAuthorizationsAndDropLoginIfExists(ISqlExecutionContext sqlContext, string loginName, string newOwnerLogin)
		{
			if (!Exists(sqlContext, Invariant($"FROM sys.server_principals WHERE name = '{loginName.QuoteEscapedName('\'')}'")))
			{
				return;
			}

			// kill connections from the login
			KillLoginSessions(sqlContext, loginName);

			// endpoints
			ChangeEndPointsAuthorizationToLogin(sqlContext, loginName, newOwnerLogin);

			// availability groups
			ChangeAvailabilityGroupsAuthorizationToLogin(sqlContext, loginName, newOwnerLogin);

			// databases
			var allDatabases = QueryOwnerDatabases(sqlContext, loginName).ToArray();
			var snapshotDatabases = allDatabases.Where(x => x.IsSnapshotDb).ToArray();
			var ownerDatabases = allDatabases.Where(x => !x.IsSnapshotDb).ToArray();

			// drop snapshot databases
			snapshotDatabases.ForEach(x => sqlContext.ExecuteNonQuery($"DROP DATABASE {x.Name.QuoteName()};"));

			// alter db owners
			ownerDatabases.ForEach(x => ChangeDatabaseOwnerToLogin(sqlContext, x.Name, newOwnerLogin));

			// revoke db permissions
			ownerDatabases.ForEach(x => RevokeDatabaseLevelPermissionsFromLogin(sqlContext, x.Name, loginName));

			// kill all connections to the login and drop the login
			try
			{
				KillLoginSessionsAndDropLogin(sqlContext, loginName);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				// login may own one or more objects or delegates then cannot be dropped
			}
		}

		internal static DbLoginInfo GetDbLoginInfo(ISqlExecutionContext sqlContext, string loginName, bool isOdysseyAdminLogin = false)
		{
			DbLoginInfo dbLoginInfo = default;

			if (!string.IsNullOrEmpty(loginName))
			{
				var sqlText = Invariant($@"
SELECT top 1
	type,
	default_database_name,
	sys.fn_varbintohexsubstring(1, sid, 1, 0) AS SidStr,
	sys.fn_varbintohexsubstring(1, convert(varbinary(256), password_hash), 1, 0) AS HashedPassword
FROM
	sys.sql_logins
WHERE
	name = N'{loginName.QuoteEscapedName('\'')}'
");

				using (var reader = sqlContext.ExecuteReader(sqlText))
				{
					if (reader.Read())
					{
						var loginType = reader.Read(0);
						var defaultDatabase = reader.Read(1);
						var loginSid = reader.Read(2);
						var pwdHash = reader.Read(3);

						dbLoginInfo = new DbLoginInfo(loginName, loginType, defaultDatabase, loginSid, pwdHash, isOdysseyAdminLogin);
					}
				}
			}

			return dbLoginInfo;
		}

		internal static string QueryLoginSid(ISqlExecutionContext sqlContext, string loginName)
		{
			var sqlText = Invariant($@"
SELECT
	sys.fn_varbintohexsubstring(1, sid, 1, 0) AS SidStr
FROM
	sys.sql_logins
WHERE
	name = N'{loginName.QuoteEscapedName('\'')}'
");

			return (string)sqlContext.ExecuteScalar(sqlText);
		}

		internal static void CreateAdminLogin(ISqlExecutionContext sqlContext, string adminLogin, string adminPwd, bool hashed, string adminSid = null)
		{
			string passPhrase = hashed ? $"{adminPwd} HASHED" : $"'{adminPwd}'";
			var createAdminLoginSql = Invariant($@" 
IF (IS_SRVROLEMEMBER('sysadmin') = 1)
BEGIN
	CREATE LOGIN {adminLogin.QuoteName()} WITH PASSWORD = {passPhrase}, DEFAULT_DATABASE = master, {(string.IsNullOrEmpty(adminSid) ? string.Empty : Invariant($"SID = {adminSid},"))} CHECK_POLICY = OFF;
	ALTER SERVER ROLE [sysadmin] ADD MEMBER {adminLogin.QuoteName()};
END
ELSE
BEGIN
	RAISERROR('Current Windows account is not system administrator.', 16, 1);
END
"); // This is a SQL Script

			sqlContext.ExecuteNonQuery(createAdminLoginSql);
		}

		internal static void RenameLoginIfExists(ISqlExecutionContext sqlContext, string fromLoginName, string toLoginName = null)
		{
			if (string.IsNullOrEmpty(toLoginName))
			{
				toLoginName = Invariant($"{fromLoginName}{AlwaysOnOldLoginNameSuffix}");
				RenameLoginIfExists(sqlContext, toLoginName, Invariant($"{fromLoginName}_{Guid.NewGuid():N}"));
			}

			sqlContext.ExecuteNonQuery(Invariant($@"
IF EXISTS (SELECT NULL FROM sys.server_principals WHERE NAME = N'{fromLoginName.QuoteEscapedName('\'')}')
BEGIN
	ALTER LOGIN {fromLoginName.QuoteName()} WITH NAME = {toLoginName}
END;
"));
			AlterLoginEnable(sqlContext, toLoginName);
		}

		internal static void AlterLoginEnable(ISqlExecutionContext sqlContext, string loginName, bool enable = true)
		{
			if (!string.IsNullOrEmpty(loginName))
			{
				sqlContext.ExecuteNonQuery(Invariant($@"
IF EXISTS (SELECT NULL FROM sys.server_principals WHERE NAME = N'{loginName.QuoteEscapedName('\'')}')
BEGIN
	ALTER LOGIN {loginName.QuoteName()} {(enable ? "ENABLE" : "DISABLE")}
END;
"));
			}
		}

		internal static void KillLoginSessions(ISqlExecutionContext sqlContext, string loginName)
		{
			const string errorsToIgnore = "6106, 6107, 6120, 6109";
			var sql = Invariant($@"
DECLARE
	@rn  nchar(2) = CHAR(13) + CHAR(10),
	@cmd nvarchar(max) = ''

SELECT 
	@cmd += CONCAT(@rn, N'BEGIN TRY KILL ', s.session_id, N'; END TRY BEGIN CATCH if (ERROR_NUMBER() NOT in ({errorsToIgnore})) THROW; END CATCH;')
FROM
	sys.dm_exec_sessions AS s
WHERE 1=1
	AND s.is_user_process = 1
	AND s.login_name = N'{loginName.QuoteEscapedName('\'')}'
;
");
			sqlContext.ExecuteNonQuery(sql);
		}

		internal static void KillLoginSessionsAndDropLogin(ISqlExecutionContext sqlContext, string loginName)
		{
			const string errorsToIgnore = "6106, 6107, 6120, 6109";
			var sql = Invariant($@"
DECLARE
	@rn  nchar(2) = CHAR(13) + CHAR(10),
	@cmd nvarchar(max) = ''

SELECT 
	@cmd += CONCAT(@rn, N'BEGIN TRY KILL ', s.session_id, N'; END TRY BEGIN CATCH if (ERROR_NUMBER() NOT in ({errorsToIgnore})) THROW; END CATCH;')
FROM
	sys.dm_exec_sessions AS s
WHERE 1=1
	AND s.is_user_process = 1
	AND s.login_name = N'{loginName.QuoteEscapedName('\'')}'
;

EXEC (@cmd);

IF EXISTS (SELECT NULL FROM sys.server_principals WHERE NAME = N'{loginName.QuoteEscapedName('\'')}')
BEGIN
	DROP LOGIN [{loginName}];
END;
");
			sqlContext.ExecuteNonQuery(sql);
		}

		internal static void ChangeInstanceLevelAuthorizationsToSysAdmin(ISqlExecutionContext sqlContext, string odysseyAdminLogin)
		{
			// change authorization on endpoints to sa and the granted permissions
			ChangeEndPointsAuthorizationToLogin(sqlContext, odysseyAdminLogin, SysAdminUserLogin);

			// change authorization on availability groups to sa
			ChangeAvailabilityGroupsAuthorizationToLogin(sqlContext, odysseyAdminLogin, SysAdminUserLogin);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		internal static void ChangeEndPointsAuthorizationToLogin(ISqlExecutionContext sqlContext, string loginName, string toLoginName)
		{
			var sql = Invariant($@"
SELECT
	EndPointName = ep.name
	, Permission = IIF(sp.grantee_principal_id is NULL
		, N''
		, CONCAT(sp.state_desc, N' ', sp.permission_name, N' ON ENDPOINT::', QUOTENAME(ep.name), N' TO ', QUOTENAME(SUSER_NAME(sp.grantee_principal_id)), N';') COLLATE database_default
		)
FROM
	sys.endpoints                    AS ep
	LEFT JOIN sys.server_permissions AS sp ON sp.major_id = ep.endpoint_id
		AND sp.class = 105
WHERE 1=1
	AND ep.principal_id = SUSER_ID(@owner)

ORDER BY
	EndPointName, Permission
");
			var endPoints = new List<(string endPointName, string permission)>();
			using (var reader = sqlContext.ExecuteReader(sql, CommandType.Text, cmd =>
			{
				cmd.AddParameter("@owner", DbType.String, loginName, 128);
			}))
			{
				while (reader.Read())
				{
					endPoints.Add(((string)reader["EndPointName"], (string)reader["Permission"]));
				}
			}
			if (endPoints.Any())
			{
				var stmt = string.Join(
					Environment.NewLine,
					endPoints
						.Select(x => Invariant($@"
ALTER AUTHORIZATION ON ENDPOINT::{x.endPointName.QuoteName()} TO {toLoginName.QuoteName()};
"))
						.Distinct());
				sqlContext.ExecuteNonQuery(stmt);

				var restorePermissionSql = $@"
EXECUTE AS LOGIN = N'{toLoginName.QuoteEscapedName('\'')}';
{string.Join(
	Environment.NewLine,
	endPoints
		.Where(x => !string.IsNullOrEmpty(x.permission))
		.Select(x => x.permission))}
REVERT;
";
				sqlContext.ExecuteNonQuery(restorePermissionSql);
			}
		}

		internal static void ChangeAvailabilityGroupsAuthorizationToLogin(ISqlExecutionContext sqlContext, string loginName, string toLoginName)
		{
			var script = Invariant($@"
SELECT
	ag.name as GroupName
FROM
	sys.availability_groups as ag
	inner join sys.availability_replicas as ar
		on ag.group_id = ar.group_id
	inner join sys.server_principals as sp
		on ar.owner_sid = sp.sid
WHERE
	ar.owner_sid = SUSER_SID(N'{loginName.QuoteEscapedName('\'')}')
;
");
			var availabilityGroups = new List<string>();
			using (var reader = sqlContext.ExecuteReader(script))
			{
				while (reader.Read())
				{
					availabilityGroups.Add(reader.Read(0));
				}
			}

			if (availabilityGroups.Any())
			{
				var stmt = string.Join(System.Environment.NewLine, availabilityGroups.Select(x => $"ALTER AUTHORIZATION ON AVAILABILITY GROUP::{x.QuoteName()} to {toLoginName.QuoteName()}"));
				sqlContext.ExecuteNonQuery(stmt);
			}
		}

		internal static void ChangeDatabaseOwnerToLogin(ISqlExecutionContext sqlContext, string dbName, string toLoginName, int timeout = 300)
		{
			var sql = Invariant($@"
IF 1=1
	AND DATABASEPROPERTYEX(N'{dbName.QuoteEscapedName('\'')}', 'Updateability') = N'READ_WRITE'
	AND EXISTS
	(
		SELECT
			null
		FROM sys.databases dbs
			JOIN sys.server_principals sp ON dbs.owner_sid = sp.sid
		WHERE 1=1
			AND dbs.name = N'{dbName.QuoteEscapedName('\'')}'
			AND sp.name <> N'{toLoginName.QuoteEscapedName('\'')}'
			AND dbs.is_read_only = 0
	)
BEGIN
	ALTER AUTHORIZATION ON DATABASE::{dbName.QuoteName()} TO {toLoginName.QuoteName()};
END
");

			sqlContext.ExecuteNonQuery(sql, CommandType.Text, cmd => cmd.CommandTimeout = timeout);
		}

		internal static void RevokeDatabaseLevelPermissionsFromLogin(ISqlExecutionContext sqlContext, string dbName, string loginName)
		{
			var sql = Invariant($@"
SELECT
	dbPermission    = dp.permission_name,
	permissionState = dp.state,
	userName        = dl.name,
	userType        = dl.type_desc,
	securableClass  = dp.class,
	schemaName      = ISNULL(CASE dp.class WHEN 3 THEN sch.name ELSE objsch.name END, N''),
	objectName      = ISNULL(obj.name, N''),
	columnName      = ISNULL(col.name, N'')
FROM
	{dbName.QuoteName()}.sys.database_permissions     AS dp
	JOIN {dbName.QuoteName()}.sys.database_principals AS dl ON dl.principal_id = dp.grantee_principal_id
	LEFT JOIN
	(
		{dbName.QuoteName()}.sys.objects      AS obj
		JOIN {dbName.QuoteName()}.sys.schemas AS objsch ON objsch.schema_id = obj.schema_id
	) ON obj.object_id = dp.major_id AND dp.class = 1

	LEFT JOIN {dbName.QuoteName()}.sys.columns AS col ON col.object_id = obj.object_id AND col.column_id = dp.minor_id
	LEFT JOIN {dbName.QuoteName()}.sys.schemas AS sch ON sch.schema_id = dp.major_id AND dp.class = 3
WHERE
	dl.type NOT in ('R')
	AND
	(
		dp.class = 0
		OR dp.class = 1 AND obj.object_id is NOT NULL
		OR dp.class = 3 AND sch.schema_id is NOT NULL
	)
	AND dp.state != 'D'
	AND dp.type NOT in ('CO', 'VW', 'SPLN')
	AND NOT (dp.type = 'EX' AND dp.class = 0 AND dl.name = N'{loginName.QuoteEscapedName('\'')}')
	AND dl.name NOT in ('{SqlDbOwnerSchema}', '{SqlReservedSchemas}')
;
");

			var cmdBuilder = new StringBuilder();

			using (var reader = sqlContext.ExecuteReader(sql))
			{
				while (reader.Read())
				{
					var dbPermission = reader.Read("dbPermission");
					var permissionState = reader.Read("permissionState").ToUpper(CultureInfo.InvariantCulture);
					var userName = reader.Read("userName");
					var userType = reader.Read("userType");
					var securableClass = reader.Read<int>("securableClass");
					var schemaName = reader.Read("schemaName");
					var objectName = reader.Read("objectName");
					var columnName = reader.Read("columnName");

					if (!string.IsNullOrEmpty(dbPermission)
						&& !string.IsNullOrEmpty(permissionState)
						&& !string.IsNullOrEmpty(userName)
						&& !string.IsNullOrEmpty(userType)
						&& !string.IsNullOrEmpty(schemaName)
						&& !string.IsNullOrEmpty(objectName)
						&& !string.IsNullOrEmpty(columnName))
					{
						var line = Invariant($@"
REVOKE {dbPermission} {GetDatabaseSecurableClause(securableClass, schemaName, objectName, columnName)}
FROM {userName.QuoteName()} {(permissionState == "W" ? "CASCADE" : "")};
");

						cmdBuilder.AppendLine(line);
					}
				}
			}

			var stmt = cmdBuilder.ToString();
			if (stmt.Length > 0)
			{
				sql = Invariant($@"
EXEC {dbName.QuoteName()}.sys.sp_executesql N'{stmt.QuoteEscapedName('\'')}'
");
				sqlContext.ExecuteNonQuery(sql);
			}
		}

		internal static bool Exists(ISqlExecutionContext sqlContext, string sqlFromAndWhereClause, Action<IDbCommand> parameters = null)
		{
			var sql = Invariant($@"
SELECT CONVERT(bit, CASE WHEN EXISTS(SELECT NULL {sqlFromAndWhereClause}) THEN 1 ELSE 0 END);
");

			return Convert.ToBoolean(sqlContext.ExecuteScalar(sql, CommandType.Text, cmd => parameters?.Invoke(cmd)), CultureInfo.InvariantCulture);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const strings")]
		internal static string GetDatabaseSecurableClause(int securableClass, string schemaName, string majorName, string minorName)
		{
			switch (securableClass)
			{
				case 0: // Database
					return "";
				case 1: // Object or Column
					return "ON " + schemaName.QuoteName() + "." + majorName.QuoteName() + (string.IsNullOrEmpty(minorName) ? "" : minorName.QuoteName().QuoteName('('));
				case 3:  // Schema
					return "ON SCHEMA::" + schemaName.QuoteName();
				case 4:
					return "ON USER :: [" + majorName + "]";
				case 5:
					return "ON ASSEMBLY :: [" + majorName + "]";
				case 6:
					return "ON TYPE :: [" + schemaName + "].[" + majorName + "]";
				case 10:
					return "ON XML SCHEMA COLLECTION :: [" + schemaName + "].[" + majorName + "]";
				case 15:
					return "ON MESSAGE TYPE :: [" + majorName + "]";
				case 16:
					return "ON CONTRACT :: [" + majorName + "]";
				case 17:
					return "ON SERVICE :: [" + majorName + "]";
				case 18:
					return "ON REMOTE SERVICE BINDING :: [" + majorName + "]";
				case 19:
					return "ON ROUTE :: [" + majorName + "]";
				case 23:
					return "ON FULLTEXT CATALOG :: [" + majorName + "]"; // and ?? FULLTEXT STOPLIST ??
				case 24:
					return "ON SYMMETRIC KEY :: [" + majorName + "]";
				case 25:
					return "ON CERTIFICATE :: [" + majorName + "]";
				case 26:
					return "ON ASYMMETRIC KEY :: [" + majorName + "]";
				default:
					throw new ArgumentOutOfRangeException(nameof(securableClass), Invariant($"Securable class value of {securableClass} is out of range."));
			}
		}

		internal static IEnumerable<(string Name, bool IsReadOnly, bool IsSnapshotDb)> QueryOwnerDatabases(ISqlExecutionContext sqlContext, string ownerLoginName)
		{
			var script = Invariant($@"
SELECT
	name, is_read_only, CAST (CASE WHEN source_database_id is not null THEN 1 ELSE 0 END AS BIT) AS is_snapshot_db
FROM
	sys.databases
WHERE
	1 = 1
	AND owner_sid = SUSER_SID(N'{ownerLoginName}')
;
");
			var ownerDatabases = new List<(string Name, bool IsReadOnly, bool IsSnapshotDb)>();
			using (var reader = sqlContext.ExecuteReader(script))
			{
				while (reader.Read())
				{
					ownerDatabases.Add((reader.Read(0), reader.Read<bool>(1), reader.Read<bool>(2))); // Using SqlDataReaderExtensions to check returned values
				}
			}

			return ownerDatabases;
		}

		internal static IEnumerable<(string DatabaseName, string OwnerLogin)> QueryDatabaseOwnerLogin(ISqlExecutionContext sqlContext, IEnumerable<string> databases)
		{
			var script = Invariant($@"
SELECT
	name as databaseName, suser_sname(owner_sid) as loginName
FROM
	sys.databases
WHERE database_id > 4 and name in ({string.Join(",", databases.Select(x => $"N'{x.QuoteEscapedName('\'')}'"))})
;
");
			var databaseOwners = new List<(string DatabaseName, string OwnerLogin)>();
			using (var reader = sqlContext.ExecuteReader(script))
			{
				while (reader.Read())
				{
					databaseOwners.Add((reader.Read(0), reader.Read(1))); // Using SqlDataReaderExtensions to check returned values
				}
			}

			return databaseOwners;
		}

		internal static string GetAlwaysOnOldLoginName(string loginName)
		{
			return loginName + AlwaysOnOldLoginNameSuffix;
		}

		public const string AlwaysOnOldLoginNameSuffix = "AlwaysOnOldLogin";
		public const string SqlDbOwnerSchema = "dbo"; // Development constant, should not use Res.GetString
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public const string SysAdminUserLogin = "sa"; // Development constant, should not
		public const string SqlReservedSchemas = "sys,cdc"; // Development constant, should not use

#if DEBUG
		public const string OdysseyAdminLoginSid = "0xCC856E992D5C724C864E6832EBAD0B5D";
		public const string OdysseyAdminPwdHash = "0x0200E0C79E043BDDB7D51EA2A125874E9CEB45F831F3878698C8D9B8EFA4667B3D5F05A6B5A1157B590964EFBE9C20D2D4E969478449BC5EC34705C2390FB2CEB1DCC0000ADF";
#endif
	}
}
