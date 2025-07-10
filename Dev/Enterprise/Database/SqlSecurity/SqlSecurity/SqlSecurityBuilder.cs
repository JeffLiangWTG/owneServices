using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.DataProtection;
using CargoWise.SqlSecurity.Common;
using CargoWise.SqlSecurity.Database;
using CargoWise.SqlSecurity.Server;
using Enterprise.MasterFiles.Business;
using static Enterprise.MasterFiles.Business.DbUserManager;

namespace Enterprise.SqlSecurity
{
	// ====================================================
	// NOTE
	// This module is currently being actively refactored, and
	// will soon be moved to CargoWise/Shared.
	// If making changes here, please keep Leonid Shchurov (LDS)
	// in the loop
	// ====================================================

	class SqlSecurityBuilder
	{
		internal SqlSecurityBuilder(string mainDatabaseName, bool isHostedInWiseCloud, bool isDedicatedServer, IStaffInfoProvider staffInfoProvider)
		{
			_ = string.IsNullOrWhiteSpace(mainDatabaseName) ? throw new ArgumentException(nameof(mainDatabaseName)) : mainDatabaseName;
			_ = staffInfoProvider ?? throw new ArgumentNullException(nameof(staffInfoProvider));

			this.mainDatabaseName = mainDatabaseName;
			this.isHostedInWiseCloud = isHostedInWiseCloud;
			this.isDedicatedServer = isDedicatedServer;
			this.staffInfoProvider = staffInfoProvider;
		}

		readonly bool isDedicatedServer;
		readonly bool isHostedInWiseCloud;
		readonly string mainDatabaseName;
		readonly IStaffInfoProvider staffInfoProvider;

		IEnumerable<StaffLoginInfo> staffLoginsInfo;
		IEnumerable<StaffLoginInfo> StaffLoginsInfo
		{
			get
			{
				if (staffLoginsInfo is null)
				{
					staffLoginsInfo = staffInfoProvider.GetStaffLoginsInfo();
				}
				return staffLoginsInfo;
			}
		}

		#region Server level

		public SqlServerProposedEntities GetServerLevelInfoWithNewBuilder(AdminConnection connection)
		{
			// ====================================================
			// NOTE
			// This module is currently being actively refactored, and
			// will soon be moved to CargoWise/Shared.
			// If making changes here, please keep Leonid Shchurov (LDS)
			// in the loop
			// ====================================================

			var builder = new ServerProposedBuilder();
			var credentialsProvider = ExistingCredentialProvider.Create(connection, mainDatabaseName, StaffLoginsInfo);

			var windowsLoginMarkerRoleName = $"{mainDatabaseName}_WindowsLogin";

			var isHostedInWiseCloudSharedServer = isHostedInWiseCloud && !isDedicatedServer;
			foreach (var loginInfo in StaffLoginsInfo)
			{
				if (loginInfo.DbAuthenticationMode == DbUserManager.DatabaseAuthenticationMode.Windows)
				{
					builder.AddWindowsLogin(
						loginInfo.LoginName,
						defaultDatabase: mainDatabaseName,
						markerRoleName: windowsLoginMarkerRoleName
					);
				}
				else
				{
					var sid = credentialsProvider.GetSidForLogin(loginInfo.LoginName);
					var password = credentialsProvider.GetHashForLogin(loginInfo.LoginName);

					builder.AddSqlLogin(
						loginInfo.LoginName,
						defaultDatabase: mainDatabaseName,
						passwordMode: ProposedPasswordMode.CreateOrAlter,
						passwordHash: password,
						sid: sid == null ? null : new SID(sid),
						isPolicyChecked: false,
						isExpirationChecked: false
					);
				}

				builder.GrantPermission(loginInfo.LoginName, "CONNECT SQL", SqlServerSecurableType.Server);
				if (isHostedInWiseCloudSharedServer)
				{
					builder.DenyPermission(loginInfo.LoginName, "VIEW ANY DATABASE", SqlServerSecurableType.Server);
				}

				if (!isHostedInWiseCloudSharedServer && loginInfo.IsDatabaseDeveloper)
				{
					builder.GrantPermission(loginInfo.LoginName, "ALTER TRACE", SqlServerSecurableType.Server);
					builder.GrantPermission(loginInfo.LoginName, "VIEW SERVER STATE", SqlServerSecurableType.Server);
					builder.GrantPermission(loginInfo.LoginName, "ALTER ANY EVENT SESSION", SqlServerSecurableType.Server);
					builder.GrantPermission(loginInfo.LoginName, "VIEW ANY DEFINITION", SqlServerSecurableType.Server);
					if (connection.ServerVersionNumber.IsEqualOrAboveSqlGeneration(SqlServerVersionNumber.SqlGeneration.Sql2022))
					{
						builder.GrantPermission(loginInfo.LoginName, "VIEW ANY ERROR LOG", SqlServerSecurableType.Server);
					}
				}
			}

			var applicationLoginNames = new[] {
				credentialsProvider.Logins.Reader.UserName,
				credentialsProvider.Logins.Writer.UserName,
				credentialsProvider.Logins.RestrictedReader.UserName,
				credentialsProvider.Logins.RestrictedWriter.UserName,
				credentialsProvider.Logins.UnrestrictedWriter.UserName
			};

			foreach (var loginName in applicationLoginNames)
			{
				builder.AddSqlLogin(
					loginName,
					defaultDatabase: "master",
					passwordMode: ProposedPasswordMode.CreateOrAlter,
					passwordHash: credentialsProvider.GetHashForLogin(loginName),
					sid: new SID(credentialsProvider.GetSidForLogin(loginName)),
					isPolicyChecked: false,
					isExpirationChecked: false
				);

				builder.GrantPermission(loginName, "CONNECT SQL", SqlServerSecurableType.Server);
			}

			var result = builder.Build();

			var resultWithoutMarkerRoles = RemoveMarkerRoleRelatedEntities(result, windowsLoginMarkerRoleName);
			return resultWithoutMarkerRoles;
		}

		public SqlSecurityBuilderResult GetServerLevelInfo(AdminConnection connection)
		{
			return MapToSqlSecurityBuilderResult(GetServerLevelInfoWithNewBuilder(connection));
		}

		SqlServerProposedEntities RemoveMarkerRoleRelatedEntities(SqlServerProposedEntities entities, string windowsLoginMarkerRoleName)
		{
			// Warning: Temporary ugly hack
			//
			// The new builder API requires marker roles for windows logins, as that's the only reliable way to identify them in the existing list
			// The old code doesn't accomodate for this yet, and old tests break if I keep the marker roles
			// Here, I am removing all mentions of the marker role from the builder result. This is to avoid changing the builder API which is in a different repo.
			//
			// Future steps:
			// 1. Remove old redundant tests once we're happy that spec testing covers the same functionality
			// 2. Remove this hack, adding marker roles back to the builder result
			// 3. Marker roles will be correctly used in the new synchronizer code once that's attached to this

			return new SqlServerProposedEntities()
			{
				Principals = entities.Principals = entities.Principals.Where(p => p.MemberName != windowsLoginMarkerRoleName).ToArray(),
				RoleMemberships = entities.RoleMemberships = entities.RoleMemberships.Where(r => r.RolePrincipalName != windowsLoginMarkerRoleName).ToArray(),
				Permissions = entities.Permissions
			};
		}

		SqlSecurityBuilderResult MapToSqlSecurityBuilderResult(SqlServerProposedEntities entities)
		{
			return new SqlSecurityBuilderResult()
			{
				ProposedPrincipalsAndMemberships = BackwardsCompatibility.PrincipalsAndMembershipsAsSql(entities),
				ProposedPermissions = BackwardsCompatibility.PermissionsAsSql(entities),
			};
		}

		#endregion Server level

		#region Database level

		public SqlDatabaseProposedEntities GetDatabaseLevelInfoWithNewBuilder(
			AdminConnection mainConnection,
			AdminConnection targetConnection,
			DatabaseType databaseType)
		{
			// ====================================================
			// NOTE
			// This module is currently being actively refactored, and
			// will soon be moved to CargoWise/Shared.
			// If making changes here, please keep Leonid Shchurov (LDS)
			// in the loop
			// ====================================================

			var logins = ExistingCredentialProvider.GetActiveApplicationLoginsFromMainDatabase(mainConnection, mainDatabaseName);

			var databaseName = targetConnection.CurrentDatabase;
			var builder = new DatabaseProposedBuilder();
			var windowsLoginMarkerRoleName = $"{mainDatabaseName}_WindowsLogin";

			// If SingleSharedRef => just grant guest permissions and return.
			if (databaseType == DatabaseType.SingleSharedRef)
			{
				GrantGuestPermissions(builder, targetConnection.CurrentDatabase);
				return builder.Build();
			}

			// Add core roles
			AddCommonRoles(builder);

			// Add the application’s main “reader,” “writer,” etc., users
			AddCoreDatabaseUsers(builder, databaseName, logins);

			// Check for existence of the HRM schema
			bool hasHrmSchema = CheckIfHrmSchemaExists(targetConnection);
			if (hasHrmSchema)
			{
				builder.GrantPermission(DbRoleTypes.CwHRMStaffRole, "SELECT", SqlDatabaseSecurableType.Schema, securable: DbSecurity.SqlHrmSchema);
			}

			// For each staff login, figure out the roles they should get, and grant relevant permissions
			foreach (var loginInfo in StaffLoginsInfo)
			{
				AddOrUpdateStaffLogin(builder, targetConnection, loginInfo, databaseType, databaseName, logins.UnrestrictedWriter);
			}

			// Grant role-level DB-wide permissions
			GrantRestrictedReaderPermissions(builder, databaseName);
			GrantRestrictedWriterPermissions(builder, databaseName, logins);
			GrantUnrestrictedWriterPermissions(builder, databaseName);
			GrantReaderRolePermissions(builder, databaseName);

			// Grant schema/object-level permissions
			var databaseObjectTypesAndSchemas = GetObjectsAndTypesAndSchemas(targetConnection);
			foreach (var item in databaseObjectTypesAndSchemas)
			{
				GrantPermissionsForDatabaseItem(builder, item, databaseType);
			}

			return RemoveMarkerRoleRelatedEntities(builder.Build(), windowsLoginMarkerRoleName);
		}

		void GrantGuestPermissions(DatabaseProposedBuilder builder, string database)
		{
			builder.GrantPermission("guest", "CONNECT", SqlDatabaseSecurableType.Database, securable: database);
			builder.GrantPermission("guest", "EXECUTE", SqlDatabaseSecurableType.Database, securable: database);
			builder.GrantPermission("guest", "SELECT", SqlDatabaseSecurableType.Database, securable: database);
		}

		void AddCommonRoles(DatabaseProposedBuilder builder)
		{
			builder.AddRole(DbRoleTypes.CwReaderRole)
				   .AddMembership(DbRoleTypes.CwReaderRole, "db_datareader");

			builder.AddRole(DbRoleTypes.CwRestrictedReaderRole);
			builder.AddRole(DbRoleTypes.CwRestrictedWriterRole);
			builder.AddRole(DbRoleTypes.CwUnrestrictedWriterRole);
			builder.AddRole(DbRoleTypes.CwHRMStaffRole);
		}

		void AddCoreDatabaseUsers(
			DatabaseProposedBuilder builder,
			string databaseName,
			CommonCredentials logins)
		{
			// Reader + membership
			builder.AddUser(logins.Reader.UserName, defaultSchema: null)
				   .AddMembership(logins.Reader.UserName, DbRoleTypes.CwReaderRole)
				   .AddMembership(logins.Reader.UserName, "db_datareader");

			// Writer + membership
			builder.AddUser(logins.Writer.UserName, defaultSchema: null)
				   .AddMembership(logins.Writer.UserName, DbRoleTypes.CwRestrictedWriterRole);

			// Restricted Reader
			builder.AddUser(logins.RestrictedReader.UserName, defaultSchema: null)
				   .AddMembership(logins.RestrictedReader.UserName, DbRoleTypes.CwRestrictedReaderRole);

			// Restricted Writer
			builder.AddUser(logins.RestrictedWriter.UserName, defaultSchema: null)
				   .AddMembership(logins.RestrictedWriter.UserName, DbRoleTypes.CwRestrictedWriterRole);

			// Unrestricted Writer
			builder.AddUser(logins.UnrestrictedWriter.UserName, defaultSchema: null)
				   .AddMembership(logins.UnrestrictedWriter.UserName, DbRoleTypes.CwUnrestrictedWriterRole);

			// CONNECT for all
			builder.GrantPermission(logins.Reader.UserName, "CONNECT", SqlDatabaseSecurableType.Database, securable: databaseName);
			builder.GrantPermission(logins.Writer.UserName, "CONNECT", SqlDatabaseSecurableType.Database, securable: databaseName);
			builder.GrantPermission(logins.RestrictedReader.UserName, "CONNECT", SqlDatabaseSecurableType.Database, securable: databaseName);
			builder.GrantPermission(logins.RestrictedWriter.UserName, "CONNECT", SqlDatabaseSecurableType.Database, securable: databaseName);
			builder.GrantPermission(logins.UnrestrictedWriter.UserName, "CONNECT", SqlDatabaseSecurableType.Database, securable: databaseName);

			// EXECUTE for all
			builder.GrantPermission(logins.Reader.UserName, "EXECUTE", SqlDatabaseSecurableType.Database, securable: databaseName);
			builder.GrantPermission(logins.Writer.UserName, "EXECUTE", SqlDatabaseSecurableType.Database, securable: databaseName);
			builder.GrantPermission(logins.RestrictedReader.UserName, "EXECUTE", SqlDatabaseSecurableType.Database, securable: databaseName);
			builder.GrantPermission(logins.RestrictedWriter.UserName, "EXECUTE", SqlDatabaseSecurableType.Database, securable: databaseName);
			builder.GrantPermission(logins.UnrestrictedWriter.UserName, "EXECUTE", SqlDatabaseSecurableType.Database, securable: databaseName);
		}

		bool CheckIfHrmSchemaExists(AdminConnection connection)
		{
			var databaseObjectTypesAndSchemas = GetObjectsAndTypesAndSchemas(connection);
			return databaseObjectTypesAndSchemas.Any(
				t => t.SecurableType.Equals("SCHEMA", StringComparison.OrdinalIgnoreCase)
					 && string.IsNullOrEmpty(t.SecurableSchema)
					 && t.SecurableName.Equals(DbSecurity.SqlHrmSchema, StringComparison.OrdinalIgnoreCase));
		}

		void AddOrUpdateStaffLogin(
			DatabaseProposedBuilder builder,
			AdminConnection connection,
			StaffLoginInfo loginInfo,
			DatabaseType databaseType,
			string databaseName,
			DBCredentials unrestrictedWriterLogin)
		{
			bool isWindowsLogin = loginInfo.DbAuthenticationMode == DbUserManager.DatabaseAuthenticationMode.Windows;

			// If Windows user => add them as a Windows user
			if (isWindowsLogin)
			{
				// windowsLoginMarkerRoleName is added as the default role
				var windowsLoginMarkerRoleName = $"{mainDatabaseName}_WindowsLogin";
				builder.AddWindowsUser(loginInfo.LoginName, windowsLoginMarkerRoleName, defaultSchema: null);
			}
			else
			{
				builder.AddUser(loginInfo.LoginName, defaultSchema: null);
			}

			// Filter out roles that shouldn’t be applied in certain conditions
			var filteredRoles = loginInfo.StaffDatabaseAccessGroupRoles
				.Where(r => !ShouldExcludeRole(r, databaseType, isHostedInWiseCloud, connection))
				.ToList();

			// Add membership for the roles that remain
			foreach (var role in filteredRoles)
			{
				builder.AddMembership(loginInfo.LoginName, role);
			}

			// Grant CONNECT to the entire DB
			builder.GrantPermission(loginInfo.LoginName, "CONNECT", SqlDatabaseSecurableType.Database, securable: databaseName);

			// If not a BI-type DB, allow the main “unrestricted writer” to impersonate
			if (!DatabaseType.BI.HasFlag(databaseType))
			{
				builder.GrantPermission(
					unrestrictedWriterLogin.UserName,
					"IMPERSONATE",
					SqlDatabaseSecurableType.User,
					securable: loginInfo.LoginName
				);
			}

			// If this is a UserRepository, and the user has “DbDataWriterRole” => grant creation rights
			bool hasDbDataWriterRole = loginInfo.StaffDatabaseAccessGroupRoles
				.Contains(DbRoleTypes.DbDataWriterRole, StringComparer.OrdinalIgnoreCase);

			if (databaseType == DatabaseType.UserRepository && hasDbDataWriterRole)
			{
				GrantUserRepositoryDataWriterPermissions(builder, loginInfo.LoginName, databaseName);
			}

			// If this is a UserRepository, and the user has “CwRestrictedReaderRole” => grant SELECT and EXECUTE
			bool hasRestrictedReaderRole = loginInfo.StaffDatabaseAccessGroupRoles
				.Contains(DbRoleTypes.CwRestrictedReaderRole, StringComparer.OrdinalIgnoreCase);

			if (databaseType == DatabaseType.UserRepository && hasRestrictedReaderRole)
			{
				builder.GrantPermission(loginInfo.LoginName, "EXECUTE", SqlDatabaseSecurableType.Database, securable: databaseName);
				builder.GrantPermission(loginInfo.LoginName, "SELECT", SqlDatabaseSecurableType.Database, securable: databaseName);
			}
		}

		bool ShouldExcludeRole(string role, DatabaseType dbType, bool hostedInWiseCloud, AdminConnection connection)
		{
			bool isDataWriterRole = role.Equals(DbRoleTypes.DbDataWriterRole, StringComparison.OrdinalIgnoreCase);
			bool isBackupOperator = role.Equals(DbRoleTypes.DbBackupOperatorRole, StringComparison.OrdinalIgnoreCase);
			bool isGlobalDbServer = DataUtils.IsWiseTechGlobalDatabaseServer(connection);

			if (isDataWriterRole && dbType != DatabaseType.UserRepository)
			{
				return true;
			}

			if (isBackupOperator && (hostedInWiseCloud || isGlobalDbServer))
			{
				return true;
			}

			return false;
		}

		void GrantRestrictedReaderPermissions(DatabaseProposedBuilder builder, string databaseName)
		{
			builder.GrantPermission(DbRoleTypes.CwRestrictedReaderRole, "SHOWPLAN", SqlDatabaseSecurableType.Database, securable: databaseName);
			builder.GrantPermission(DbRoleTypes.CwRestrictedReaderRole, "VIEW DEFINITION", SqlDatabaseSecurableType.Database, securable: databaseName);
			builder.GrantPermission(DbRoleTypes.CwRestrictedReaderRole, "SELECT", SqlDatabaseSecurableType.Object, securable: "sql_expression_dependencies", securableSchema: "sys");
		}

		void GrantRestrictedWriterPermissions(DatabaseProposedBuilder builder, string databaseName, CommonCredentials logins)
		{
			builder.GrantPermission(DbRoleTypes.CwRestrictedWriterRole, "IMPERSONATE", SqlDatabaseSecurableType.User, securable: logins.Reader.UserName);
			builder.GrantPermission(DbRoleTypes.CwRestrictedWriterRole, "IMPERSONATE", SqlDatabaseSecurableType.User, securable: logins.RestrictedReader.UserName);

			builder.GrantPermission(DbRoleTypes.CwRestrictedWriterRole, "ALTER", SqlDatabaseSecurableType.Database, securable: databaseName);
			builder.GrantPermission(DbRoleTypes.CwRestrictedWriterRole, "CREATE SCHEMA", SqlDatabaseSecurableType.Database, securable: databaseName);
			builder.GrantPermission(DbRoleTypes.CwRestrictedWriterRole, "REFERENCES", SqlDatabaseSecurableType.Database, securable: databaseName);
			builder.GrantPermission(DbRoleTypes.CwRestrictedWriterRole, "SHOWPLAN", SqlDatabaseSecurableType.Database, securable: databaseName);
			builder.GrantPermission(DbRoleTypes.CwRestrictedWriterRole, "VIEW DATABASE STATE", SqlDatabaseSecurableType.Database, securable: databaseName);
			builder.GrantPermission(DbRoleTypes.CwRestrictedWriterRole, "VIEW DEFINITION", SqlDatabaseSecurableType.Database, securable: databaseName);

			builder.GrantPermission(DbRoleTypes.CwRestrictedWriterRole, "SELECT", SqlDatabaseSecurableType.Object, securable: "sql_expression_dependencies", securableSchema: "sys");
		}

		void GrantUnrestrictedWriterPermissions(DatabaseProposedBuilder builder, string databaseName)
		{
			builder.GrantPermission(DbRoleTypes.CwUnrestrictedWriterRole, "DELETE", SqlDatabaseSecurableType.Database, securable: databaseName);
			builder.GrantPermission(DbRoleTypes.CwUnrestrictedWriterRole, "EXECUTE", SqlDatabaseSecurableType.Database, securable: databaseName);
			builder.GrantPermission(DbRoleTypes.CwUnrestrictedWriterRole, "INSERT", SqlDatabaseSecurableType.Database, securable: databaseName);
			builder.GrantPermission(DbRoleTypes.CwUnrestrictedWriterRole, "SELECT", SqlDatabaseSecurableType.Database, securable: databaseName);
			builder.GrantPermission(DbRoleTypes.CwUnrestrictedWriterRole, "UPDATE", SqlDatabaseSecurableType.Database, securable: databaseName);

			builder.GrantPermission(DbRoleTypes.CwUnrestrictedWriterRole, "VIEW DATABASE STATE", SqlDatabaseSecurableType.Database, securable: databaseName);
			builder.GrantPermission(DbRoleTypes.CwUnrestrictedWriterRole, "VIEW DEFINITION", SqlDatabaseSecurableType.Database, securable: databaseName);
		}

		void GrantReaderRolePermissions(DatabaseProposedBuilder builder, string databaseName)
		{
			builder.GrantPermission(DbRoleTypes.CwReaderRole, "SHOWPLAN", SqlDatabaseSecurableType.Database, securable: databaseName);
			builder.GrantPermission(DbRoleTypes.CwReaderRole, "VIEW DEFINITION", SqlDatabaseSecurableType.Database, securable: databaseName);
		}

		void GrantPermissionsForDatabaseItem(
			DatabaseProposedBuilder builder,
			(string SecurableType, string SecurableSchema, string SecurableName) item,
			DatabaseType databaseType)
		{
			// Handy local booleans
			bool isSchema = item.SecurableType.Equals("SCHEMA", StringComparison.OrdinalIgnoreCase);
			bool isObjectOrType = item.SecurableType.Equals("OBJECT", StringComparison.OrdinalIgnoreCase)
								  || item.SecurableType.Equals("TYPE", StringComparison.OrdinalIgnoreCase);
			bool isCdcSchema = item.SecurableName.Equals(DbSecurity.SqlCdcSchema, StringComparison.OrdinalIgnoreCase);
			bool isStagingSchema = item.SecurableName.Equals(DbSecurity.SqlStagingSchema, StringComparison.OrdinalIgnoreCase);
			bool isHrmSchema = item.SecurableName.Equals(DbSecurity.SqlHrmSchema, StringComparison.OrdinalIgnoreCase);

			// For "Main" database: grant EXECUTE to “ReaderRole” on all objects and types
			if (databaseType == DatabaseType.Main && isObjectOrType)
			{
				builder.GrantPermission(
					DbRoleTypes.CwReaderRole,
					"EXECUTE",
					item.SecurableType,
					securable: item.SecurableName,
					securableSchema: item.SecurableSchema
				);
			}

			// If this is a schema, skip if it’s cdc, staging, or hrm
			if (isSchema && !(isCdcSchema || isStagingSchema || isHrmSchema))
			{
				// Grant restricted READ perms
				builder.GrantPermission(DbRoleTypes.CwRestrictedReaderRole, "EXECUTE", SqlDatabaseSecurableType.Schema, securable: item.SecurableName);
				builder.GrantPermission(DbRoleTypes.CwRestrictedReaderRole, "SELECT", SqlDatabaseSecurableType.Schema, securable: item.SecurableName);

				// Grant restricted WRITE perms
				GrantRestrictedWriterSchemaPermissions(builder, item.SecurableName);
			}

			// Unrestricted writer => can see change tracking, create sequence, etc.
			if (isSchema)
			{
				builder.GrantPermission(DbRoleTypes.CwUnrestrictedWriterRole, "VIEW CHANGE TRACKING", SqlDatabaseSecurableType.Schema, securable: item.SecurableName);
				builder.GrantPermission(DbRoleTypes.CwUnrestrictedWriterRole, "CREATE SEQUENCE", SqlDatabaseSecurableType.Schema, securable: item.SecurableName);
			}
		}

		void GrantRestrictedWriterSchemaPermissions(DatabaseProposedBuilder builder, string schemaName)
		{
			builder.GrantPermission(DbRoleTypes.CwRestrictedWriterRole, "EXECUTE", SqlDatabaseSecurableType.Schema, securable: schemaName);
			builder.GrantPermission(DbRoleTypes.CwRestrictedWriterRole, "SELECT", SqlDatabaseSecurableType.Schema, securable: schemaName);
			builder.GrantPermission(DbRoleTypes.CwRestrictedWriterRole, "INSERT", SqlDatabaseSecurableType.Schema, securable: schemaName);
			builder.GrantPermission(DbRoleTypes.CwRestrictedWriterRole, "UPDATE", SqlDatabaseSecurableType.Schema, securable: schemaName);
			builder.GrantPermission(DbRoleTypes.CwRestrictedWriterRole, "DELETE", SqlDatabaseSecurableType.Schema, securable: schemaName);
			builder.GrantPermission(DbRoleTypes.CwRestrictedWriterRole, "ALTER", SqlDatabaseSecurableType.Schema, securable: schemaName);
			builder.GrantPermission(DbRoleTypes.CwRestrictedWriterRole, "CREATE SEQUENCE", SqlDatabaseSecurableType.Schema, securable: schemaName);
		}

		void GrantUserRepositoryDataWriterPermissions(DatabaseProposedBuilder builder, string loginName, string databaseName)
		{
			builder.GrantPermission(loginName, "CREATE TYPE", SqlDatabaseSecurableType.Database, securable: databaseName);
			builder.GrantPermission(loginName, "CREATE SCHEMA", SqlDatabaseSecurableType.Database, securable: databaseName);
			builder.GrantPermission(loginName, "CREATE VIEW", SqlDatabaseSecurableType.Database, securable: databaseName);
			builder.GrantPermission(loginName, "CREATE FUNCTION", SqlDatabaseSecurableType.Database, securable: databaseName);
			builder.GrantPermission(loginName, "CREATE PROCEDURE", SqlDatabaseSecurableType.Database, securable: databaseName);
			builder.GrantPermission(loginName, "CREATE TABLE", SqlDatabaseSecurableType.Database, securable: databaseName);
			builder.GrantPermission(loginName, "INSERT", SqlDatabaseSecurableType.Database, securable: databaseName);
			builder.GrantPermission(loginName, "UPDATE", SqlDatabaseSecurableType.Database, securable: databaseName);
			builder.GrantPermission(loginName, "DELETE", SqlDatabaseSecurableType.Database, securable: databaseName);
			builder.GrantPermission(loginName, "REFERENCES", SqlDatabaseSecurableType.Database, securable: databaseName);
			builder.GrantPermission(loginName, "ALTER", SqlDatabaseSecurableType.Schema, securable: "dbo");
		}

		IEnumerable<(string SecurableType, string SecurableSchema, string SecurableName)> GetObjectsAndTypesAndSchemas(AdminConnection connection)
		{
			var result = new List<(string SecurableType, string SecurableSchema, string SecurableName)>();
			using (var command = connection.Command($@"
SELECT
	SecurableType = N'OBJECT'
	, SecurableSchema = sch.name
	, SecurableName = obj.name
FROM sys.objects AS obj
	JOIN sys.schemas AS sch ON obj.schema_id = sch.schema_id
WHERE 1=1
	AND type IN ('FN', 'FS', 'AF')
	AND obj.is_ms_shipped = 0

UNION ALL

SELECT
	SecurableType = N'TYPE'
	, SecurableSchema = sch.name
	, SecurableName = tp.name
FROM sys.types AS tp
	JOIN sys.schemas AS sch ON tp.schema_id = sch.schema_id
WHERE tp.system_type_id = 243

UNION ALL

SELECT
	SecurableType = N'SCHEMA'
	, SecurableSchema = N''
	, SecurableName = sch.name
FROM sys.schemas sch
WHERE 1=1
	AND sch.name not in (N'sys', N'guest', N'INFORMATION_SCHEMA')
	AND name not like N'db[_]%'
"))
			{
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add(((string)reader["SecurableType"], (string)reader["SecurableSchema"], (string)reader["SecurableName"]));
					}
				}

				return result;
			}
		}

		public SqlSecurityBuilderResult GetDatabaseLevelInfo(
			AdminConnection mainConnection,
			AdminConnection targetConnection,
			DatabaseType databaseType)
		{
			return MapToSqlSecurityBuilderResult(GetDatabaseLevelInfoWithNewBuilder(mainConnection, targetConnection, databaseType));
		}

		SqlDatabaseProposedEntities RemoveMarkerRoleRelatedEntities(SqlDatabaseProposedEntities entities, string windowsLoginMarkerRoleName)
		{
			// Warning: Temporary ugly hack
			//
			// The new builder API requires marker roles for windows logins, as that's the only reliable way to identify them in the existing list
			// The old code doesn't accomodate for this yet, and old tests break if I keep the marker roles
			// Here, I am removing all mentions of the marker role from the builder result. This is to avoid changing the builder API which is in a different repo.
			//
			// Future steps:
			// 1. Remove old redundant tests once we're happy that spec testing covers the same functionality
			// 2. Remove this hack, adding marker roles back to the builder result
			// 3. Marker roles will be correctly used in the new synchronizer code once that's attached to this

			return new SqlDatabaseProposedEntities()
			{
				Principals = entities.Principals = entities.Principals.Where(p => p.MemberName != windowsLoginMarkerRoleName).ToArray(),
				RoleMemberships = entities.RoleMemberships = entities.RoleMemberships.Where(r => r.RolePrincipalName != windowsLoginMarkerRoleName).ToArray(),
				Permissions = entities.Permissions
			};
		}

		SqlSecurityBuilderResult MapToSqlSecurityBuilderResult(SqlDatabaseProposedEntities entities)
		{
			return new SqlSecurityBuilderResult()
			{
				ProposedPrincipalsAndMemberships = BackwardsCompatibility.DatabasePrincipalsAndMembershipsAsSql(entities),
				ProposedPermissions = BackwardsCompatibility.DatabasePermissionsAsSql(entities),
			};
		}

		#endregion Database level
	}
}
