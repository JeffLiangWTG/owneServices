using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.DataProtection;
using CargoWise.DataProtection.Administration.SqlServer;

namespace CargoWise.Data
{
	#region SuppressResourceStringsCheckRegion
	public abstract partial class DatabaseLogin
	{
		#region Construction and Validation

		protected DatabaseLogin(AdminConnection connection)
		{
			Argument.NotNull(connection, nameof(connection));

			this.baseDbName = GetBaseDatabaseName(connection);
			this.connection = connection;
		}

		static string GetAlphaNumericDbNameOrPrefix(string dbName)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			string result = null;

			var baseDbMatch = baseDbRegex.Match(dbName);

			if (baseDbMatch.Success)
			{
				var baseDbGroup = baseDbMatch.Groups["BASEDB"];

				if (baseDbGroup != null && !String.IsNullOrWhiteSpace(baseDbGroup.Value))
				{
					result = baseDbGroup.Value;
				}
			}
			else
			{
				result = DataUtils.IsDbNameAlphaNumeric(dbName) ? dbName : null;
			}

			return result;
		}

		static string GetBaseDatabaseName(DbConnection connectionForRetrievingBaseDb)
		{
			Argument.NotNull(connectionForRetrievingBaseDb, nameof(connectionForRetrievingBaseDb));

			return GetFirstNonSystemDatabaseFallingBackToMainAppDb(
				((ICurrentDbControl)connectionForRetrievingBaseDb).InitialDatabase,
				connectionForRetrievingBaseDb.CurrentDatabase
			);
		}

		static string GetFirstNonSystemDatabaseFallingBackToMainAppDb(params string[] candidateDatabases)
		{
			Argument.NotNull(candidateDatabases, nameof(candidateDatabases));

			foreach (var candidateDb in candidateDatabases)
			{
				if (!String.IsNullOrWhiteSpace(candidateDb))
				{
					var alphaNumericDbName = GetAlphaNumericDbNameOrPrefix(candidateDb);

					if (!String.IsNullOrWhiteSpace(alphaNumericDbName) && !Db.IsSystemDatabase(alphaNumericDbName))
					{
						return alphaNumericDbName;
					}
				}
			}

			return Db.DatabaseName;
		}

		static readonly Regex baseDbRegex = new Regex(@"^(?<BASEDB>[A-Z][A-Z0-9]*)_", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		#endregion

		#region Login (server level)

		public void EnsureLogin()
		{
			EnableLogin(msg => { });
			EnsureLoginCorrectlyMappedToAllDatabases(msg => { });
		}

		public abstract void EnableLogin(Action<string> logMessage);

		public void CheckAndFixSidIfRequired()
		{
			var wasDropped = DropLoginWithWrongSid();

			if (wasDropped)
			{
				EnableLogin(msg => { });
				EnsureLoginCorrectlyMappedToAllDatabases(msg => { });
			}
		}

		public void DisableLogin() => connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, DisableLoginMask, LoginName));

		public bool LoginExists()
		{
			var sqlText = string.Format(
				CultureInfo.InvariantCulture,
				"IF exists(SELECT null FROM sys.server_principals WHERE name = '{0}') SELECT 1 ELSE SELECT 0",
				LoginName);

			return Convert.ToBoolean(connection.ExecuteScalar(sqlText));
		}

		bool DropLoginWithWrongSid()
		{
			var dropWrongSidLoginSql = string.Format(CultureInfo.InvariantCulture, DropWrongSidLoginMask, LoginName, SqlServerLoginUtilities.ComputeSqlLoginSid(LoginName));
			var wasDropped = Convert.ToBoolean(connection.ExecuteScalar(dropWrongSidLoginSql), CultureInfo.InvariantCulture);
			return wasDropped;
		}

		public static string CalculateSid(string baseString)
		{
			Argument.NotNullOrEmpty(baseString, nameof(baseString));

			const int maxWidth = 16;

			var bytes =
				Encoding.ASCII.GetBytes(baseString.Trim().ToUpperInvariant())
				.Select((b, offset) => new { Position = offset % maxWidth, Value = b })
				.GroupBy(x => x.Position)
				.Select(x => (byte)(x.Sum(y => y.Value) % 256))
				.ToArray();

			var totalBytes =
				Enumerable.Range(0, maxWidth)
				.Select(i => bytes.ElementAtOrDefault(i))
				.ToArray();
			var sid = new Guid(totalBytes);

			var result = string.Format(CultureInfo.InvariantCulture, "0x{0}", sid.ToString("N"));
			return result;
		}

		public static bool IsSchemaPermissionExpected(AdminConnection connection, string server, string database, string loginName, string permission, string schema)
		{
			var login = connection.Logins.FirstOrDefault(x => x.LoginName == loginName);
			if (login != null)
			{
				return login.IsSchemaPermissionExpected(permission, schema);
			}

			return false;
		}

		bool IsSchemaPermissionExpected(string permission, string schema) => DbLevelRoles.Any(role => role.IsSchemaPermissionExpected(permission, schema, connection));

		public static bool IsApplicationLogin(string loginName) => Db.GetAllLoginNames(Db.DatabaseName).Any(applicationLoginName => applicationLoginName == loginName);

		#region Login admin scripts

		const string DisableLoginMask = @"
			IF EXISTS (SELECT null FROM sys.server_principals WHERE name = '{0}' AND is_disabled = 0)
			BEGIN
				ALTER LOGIN [{0}] DISABLE;
			END";

		const string DropWrongSidLoginMask = @"
			IF EXISTS (SELECT null FROM sys.server_principals WHERE name = '{0}' AND sid != {1})
			BEGIN
				DROP LOGIN [{0}];
				SELECT 1;
			END;
			SELECT 0;";

		#endregion

		public abstract string LoginSuffix { get; }
		public abstract string LoginName { get; }

		#endregion

		#region Mapped Database Users

		/// <summary>
		/// If a database is migrated to another SQL Server,
		/// the SID of the server login and the respective user on each database won't match
		/// and the login won't have access to these databases.
		/// Cater for SD, RefDb, UserRepository and other dependent databases.
		/// </summary>
		public void EnsureLoginCorrectlyMappedToAllDatabases(Action<string> logMessage)
		{
			if (LoginExists())
			{
				EnsureLoginCorrectlyMappedToAllDatabasesCore(false, logMessage);
				EnsureLoginCorrectlyMappedToAllDatabasesCore(true, logMessage);
			}
		}

		void EnsureLoginCorrectlyMappedToAllDatabasesCore(bool onlyCheckPermissionsAndRoles, Action<string> logMessage)
		{
			var allDatabases = connection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef).ToList();
			var dbsWithIncorrectlyMappedUser = GetDatabasesWithIncorrectlyMappedUser(allDatabases, onlyCheckPermissionsAndRoles);

			foreach (var dbName in dbsWithIncorrectlyMappedUser)
			{
				if (!string.IsNullOrWhiteSpace(dbName))
				{
					FixLoginMappingToGivenDatabase(dbName, onlyCheckPermissionsAndRoles, logMessage);
				}
			}
		}

		internal List<string> GetDatabasesWithIncorrectlyMappedUser(IEnumerable<string> allDatabases, bool onlyCheckPermissionsAndRoles)
		{
			var allDbNames = allDatabases.ToList();
			var incorrectlyMappedUserDbNames = new List<string>();

			if (!onlyCheckPermissionsAndRoles)
			{
				var sid = GetUserSid();
				if (sid != null && sid != DBNull.Value)
				{
					var notMappedDbNames = allDbNames.Where(dbName => !IsLoginMappedToDb(dbName, sid));

					incorrectlyMappedUserDbNames.AddRange(notMappedDbNames);
				}
			}

			var dbPermissions = DbLevelPermissions.ToList();
			var dbRoles = DbLevelRoles.Select(role => role.Name).ToList();
			var impersonatePermissionRoles = ImpersonatePermissionRoles.ToList();
			if (dbPermissions.Count > 0 || dbRoles.Count > 0 || impersonatePermissionRoles.Count > 0)
			{
				var incorrectlyPermissionsAndRolesMappedDbNames = allDbNames.Where(dbName => !ArePermissionsAndRolesMappedToDb(dbName, dbPermissions, dbRoles, impersonatePermissionRoles));

				incorrectlyMappedUserDbNames.AddRange(incorrectlyPermissionsAndRolesMappedDbNames);
			}

			return incorrectlyMappedUserDbNames.Distinct().ToList();
		}

		bool IsLoginMappedToDb(string dbName, object sid)
		{
			const string sql = @"
FROM
	sys.database_principals
WHERE
	name = @login
    AND sid = @sid
";

			using (((ICurrentDbControl)connection).UseDatabase(dbName))
			{
				return connection.Exists(sql, command =>
				{
					command.AddParameter("@login", SqlDbType.NVarChar, 128, LoginName);
					command.AddParameter("@sid", SqlDbType.VarBinary, 85, sid);
				});
			}
		}

		bool ArePermissionsAndRolesMappedToDb(string dbName, IList<string> dbPermissions, IList<string> dbRoles, IList<string> impersonatePermissionRoles)
		{
			// There are 2 concrete subclasses of DatabaseLogin with fixed lists of dbPermissions and dbRoles.
			// These lists are not overridden anywhere else.
			// So there are 2 equal sets of permissions+roles per login type.
			// They can be all included into query, either as parameters or literals,
			// and as long as login is a parameter - it will generate 2 different plans for all logins of these 2 types (per database).

			var sqlBuilder = new StringBuilder();
			sqlBuilder.AppendLine("FROM sys.database_principals");
			sqlBuilder.AppendLine("WHERE name = @login");

			var securityParameters = new List<(IDbDataParameter parameter, string value)>();

			foreach (var permissionName in dbPermissions)
			{
				var parameterName = "@permission_" + securityParameters.Count.ToString(CultureInfo.InvariantCulture);
				var parameter = connection.DataProviderFactory.NewDbParameter(parameterName, SqlDbType.NVarChar, 256);
				securityParameters.Add((parameter, permissionName));
				sqlBuilder
					.AppendFormat(CultureInfo.InvariantCulture,
						"AND EXISTS (SELECT NULL FROM sys.database_permissions WHERE grantee_principal_id = principal_id AND permission_name = {0})",
						parameterName)
					.AppendLine();
			}

			foreach (var roleName in dbRoles)
			{
				var parameterName = "@role_" + securityParameters.Count.ToString(CultureInfo.InvariantCulture);
				var parameter = connection.DataProviderFactory.NewDbParameter(parameterName, SqlDbType.NVarChar, 256);
				securityParameters.Add((parameter, roleName));
				sqlBuilder
					.AppendFormat(CultureInfo.InvariantCulture,
						"AND EXISTS (SELECT NULL FROM sys.database_role_members WHERE member_principal_id = principal_id AND role_principal_id in (SELECT principal_id FROM sys.database_principals WHERE name = {0}))",
						parameterName)
					.AppendLine();
			}

			foreach (var roleName in impersonatePermissionRoles)
			{
				var parameterName = "@impersonate_" + securityParameters.Count.ToString(CultureInfo.InvariantCulture);
				var parameter = connection.DataProviderFactory.NewDbParameter(parameterName, SqlDbType.NVarChar, 256);
				securityParameters.Add((parameter, roleName));
				sqlBuilder
					.AppendFormat(CultureInfo.InvariantCulture,
						"AND EXISTS (SELECT NULL FROM sys.database_permissions WHERE grantor_principal_id = principal_id AND permission_name = 'IMPERSONATE' AND grantee_principal_id in (SELECT principal_id FROM sys.database_principals WHERE name = {0}))",
						parameterName)
					.AppendLine();
			}

			using (((ICurrentDbControl)connection).UseDatabase(dbName))
			{
				return connection.Exists(sqlBuilder.ToString(), command =>
				{
					command.AddParameter("@login", SqlDbType.NVarChar, 128, LoginName);
					foreach (var securityParameter in securityParameters)
					{
						command.AddParameter(securityParameter.parameter, 0, 0, securityParameter.value);
					}
				});
			}
		}

		object GetUserSid()
		{
			const string sql = @"
SELECT
	sid
FROM
	sys.server_principals
WHERE
	name = @login
";

			using (((ICurrentDbControl)connection).UseDatabase(Db.SqlMasterDb))
			{
				return connection.ExecuteScalar(sql, command => { command.AddParameter("@login", SqlDbType.NVarChar, 128, LoginName); });
			}
		}

		public void EnsureLoginHasRightsToCurrentDatabase()
		{
			if (LoginExists() && connection.IsDbWriteable(connection.CurrentDatabase))
			{
				CheckSharedDatabaseAccess(connection.CurrentDatabase);

				//attempt to create db role in case it doesn't exist
				var createDbRolesAndGrantPermissionsScript = CreateDbRolesAndGrantPermissionsScript;
				if (!string.IsNullOrWhiteSpace(createDbRolesAndGrantPermissionsScript))
				{
					connection.ExecuteNonQuery(createDbRolesAndGrantPermissionsScript);
				}

				connection.ExecuteNonQuery(RecreateDbUserScript);

				connection.ExecuteNonQuery(UpdateDbUserScript);

				if (NeedToGrantImpersonateOfStaffLogins)
				{
					GrantImpersonateOfStaffLogins();
				}
			}
		}

		bool NeedToGrantImpersonateOfStaffLogins => IsImpersonateEnterpriseDbUser &&
			string.Equals(((ICurrentDbControl)connection).InitialDatabase, Db.DatabaseName, StringComparison.OrdinalIgnoreCase);

		void GrantImpersonateOfStaffLogins()
		{
			var sqlGetEnterpriseDbUsers = string.Format(CultureInfo.InvariantCulture,
				"SELECT name FROM sys.database_principals WHERE name LIKE N'{0}%';"
				, DataUtils.ReplaceSqlLikeWildcard(DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName))
				);

			var enterpriseDbUserNames = new List<string>();
			connection.ExecuteReader(sqlGetEnterpriseDbUsers, (reader) =>
			{
				enterpriseDbUserNames.Add(reader["name"].ToString());
			});

			if (enterpriseDbUserNames.Count > 0)
			{
				var impersonateEnterpriseDbUserScript = string.Join(";\n", enterpriseDbUserNames.Select(name => $"GRANT IMPERSONATE ON USER::{name.QuoteName()} TO {LoginName.QuoteName()}"));
				connection.ExecuteNonQuery(impersonateEnterpriseDbUserScript);
			}
		}

		public bool EnsureLoginHasRightsToRelevantDatabase(string dbName)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			if (LoginExists() && (
					connection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW & ~DatabaseType.SingleSharedRef).Contains(dbName, StringComparer.OrdinalIgnoreCase) ||
					CheckIfBiDatabaseExist(dbName)
				)
			)
			{
				FixLoginMappingToGivenDatabase(dbName, false, msg => { });
				FixLoginMappingToGivenDatabase(dbName, true, msg => { });
				return true;
			}

			return false;
		}

		bool CheckIfBiDatabaseExist(string dbName)
		{
			var output = false;

			if (String.Equals(baseDbName + Db.AuditDatabaseSuffix, dbName, StringComparison.OrdinalIgnoreCase) ||
					String.Equals(baseDbName + Db.EdwDatabaseSuffix, dbName, StringComparison.OrdinalIgnoreCase))
			{
				output = connection.DatabaseExists(dbName);
			}

			return output;
		}

		void FixLoginMappingToGivenDatabase(string dbName, bool onlyCheckPermissionsAndRoles, Action<string> logMessage)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			if (connection.IsDbWriteable(dbName))
			{
				FixLoginMappingToGivenDatabaseUnsafe(dbName, onlyCheckPermissionsAndRoles, logMessage);
			}
			else
			{
				try
				{
					connection.AlterDbWriteableState(dbName, true);
					FixLoginMappingToGivenDatabaseUnsafe(dbName, onlyCheckPermissionsAndRoles, logMessage);
				}
				finally
				{
					connection.AlterDbWriteableState(dbName, false);
				}
			}
		}

		/// <summary>
		/// This method goes on attempting to make user changes WITHOUT checking if:
		///   - the login exists
		///   - the database is writeable
		/// Do not use it unless it is safe to assume these requirements.
		/// </summary>
		void FixLoginMappingToGivenDatabaseUnsafe(string dbName, bool onlyCheckPermissionsAndRoles, Action<string> logMessage)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			using (((ICurrentDbControl)connection).UseDatabase(dbName))
			{
				CheckSharedDatabaseAccess(connection.CurrentDatabase);

				//attempt to create db role in case it doesn't exist
				var createDbRolsAndGrantPermissionsScript = CreateDbRolesAndGrantPermissionsScript;
				if (!string.IsNullOrWhiteSpace(createDbRolsAndGrantPermissionsScript))
				{
					connection.ExecuteNonQuery(createDbRolsAndGrantPermissionsScript);
					logMessage(string.Format(CultureInfo.InvariantCulture, $"Db Role(s) [{string.Join(", ", DbLevelRoles.Select(r => r.Name))}] created and permissions granted on database [{dbName}]."));
				}

				if (onlyCheckPermissionsAndRoles)
				{
					//at this point we assume the role and user exist and are correctly mapped

					connection.ExecuteNonQuery(UpdateDbUserScript);
					logMessage(string.Format(CultureInfo.InvariantCulture, $"Db User [{LoginName}] updated on database [{dbName}]."));
				}
				else
				{
					connection.ExecuteNonQuery(RecreateDbUserScript);
					logMessage(string.Format(CultureInfo.InvariantCulture, $"Db User [{LoginName}] recreated on database [{dbName}]."));
				}

				if (NeedToGrantImpersonateOfStaffLogins &&
					connection.IsDbWriteable(connection.CurrentDatabase))
				{
					GrantImpersonateOfStaffLogins();
					logMessage(string.Format(CultureInfo.InvariantCulture, $"Enterprise Db User Impersonate permissions created for [{LoginName}]."));
				}
			}
		}

		public void DropUserFromDatabase(string dbName)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			CheckSharedDatabaseAccess(dbName);

			var revokeImpersonateRoleScript = RevokeImpersonateRoleScript;
			if (!string.IsNullOrWhiteSpace(revokeImpersonateRoleScript))
			{
				var revokeImpersonateSql = string.Format(
					CultureInfo.InvariantCulture,
					"EXEC [{0}]..sp_executesql N'{1}'",
					dbName,
					DataUtils.EscapeSingleQuotes(revokeImpersonateRoleScript));

				connection.ExecuteNonQuery(revokeImpersonateSql);
			}

			var dropUserSql = string.Format(
				CultureInfo.InvariantCulture,
				"EXEC [{0}]..sp_executesql N'{1}'",
				dbName,
				DataUtils.EscapeSingleQuotes(DropDbUserScript));

			connection.ExecuteNonQuery(dropUserSql);
		}

		void CheckSharedDatabaseAccess(string dbName)
		{
			if (RefDbTableNameResolver.IsSharedDatabase(dbName) && connection.IsInTransaction)
			{
				throw new InvalidOperationException("Do not attempt to apply user security changes for a shared database in a transaction as this can result in cross-system locking.");
			}
		}

		string CreateDbRolesAndGrantPermissionsScript
		{
			get
			{
				var result = new StringBuilder();

				foreach (var role in DbLevelRoles)
				{
					var sql = role.CreateDbRoleAndGrantPermissionsSQL(connection);

					if (!string.IsNullOrWhiteSpace(sql))
					{
						result.AppendFormat(CultureInfo.InvariantCulture, "if not exists (select 1 from sys.database_principals where name = '{0}') ", role.Name);
						result.AppendLine(sql);
						result.AppendLine();
					}
				}

				return result.ToString();
			}
		}

		string RecreateDbUserScript
		{
			get
			{
				var createDbUserScript = new StringBuilder();
				createDbUserScript.AppendFormat(CultureInfo.InvariantCulture, "CREATE USER [{0}] FOR LOGIN [{0}];", LoginName);
				createDbUserScript.AppendLine();

				createDbUserScript.Append(UpdateDbUserScript);

				var result = RevokeImpersonateRoleScript + DropDbUserScript + createDbUserScript;

				return result;
			}
		}

		string UpdateDbUserScript
		{
			get
			{
				var result = new StringBuilder();

				foreach (var role in DbLevelRoles)
				{
					result.AppendFormat(CultureInfo.InvariantCulture, "ALTER ROLE [{1}] ADD MEMBER [{0}];", LoginName, role.Name);
					result.AppendLine();
				}

				foreach (var permission in DbLevelPermissions)
				{
					result.AppendFormat(CultureInfo.InvariantCulture, "GRANT {1} TO [{0}];", LoginName, permission);
					result.AppendLine();
				}

				foreach (var role in ImpersonatePermissionRoles)
				{
					result.AppendFormat(CultureInfo.InvariantCulture, @"IF DATABASE_PRINCIPAL_ID('{1}') IS NOT NULL
	BEGIN
	   GRANT IMPERSONATE ON USER::[{0}] to [{1}];
	END;", LoginName, role);

					result.AppendLine();
				}

				return result.ToString();
			}
		}

		string RevokeImpersonateRoleScript
		{
			get
			{
				if (ImpersonatePermissionRoles.Any())
				{
					var revokeImpersonateScript = new StringBuilder();
					foreach (var role in ImpersonatePermissionRoles)
					{
						revokeImpersonateScript.AppendFormat(CultureInfo.InvariantCulture, @"IF DATABASE_PRINCIPAL_ID('{1}') IS NOT NULL  
BEGIN
   REVOKE IMPERSONATE ON USER::[{0}] FROM [{1}];
END;",
						LoginName, role);

						revokeImpersonateScript.AppendLine();
					}

					const string impersonateSql = @"IF EXISTS (SELECT [name] FROM [sys].[database_principals]
WHERE [type] = N'S' AND [name] = N'{0}')
BEGIN
	{1}
END";

					return string.Format(CultureInfo.InvariantCulture,
						impersonateSql,
						LoginName,
						revokeImpersonateScript);
				}

				return string.Empty;
			}
		}

		string DropDbUserScript
		{
			get
			{
				const string dropUserSql = @"
						-- DROP OWNED SCHEMAS
						DECLARE @DropSchemaCmd|~*~| nvarchar(max) = '';
						SELECT
							@DropSchemaCmd|~*~| = @DropSchemaCmd|~*~| + 
								CASE
									WHEN exists(SELECT null FROM sys.objects obj WHERE obj.schema_id = sch.schema_id)
										THEN 'ALTER AUTHORIZATION ON SCHEMA::[' + sch.name + '] TO [dbo];'
									ELSE 'DROP SCHEMA [' + sch.name + '];'
								END
							FROM
								sys.schemas sch
								INNER JOIN sys.database_principals usr ON sch.principal_id = usr.principal_id
							WHERE usr.name = @UserName|~*~|
						IF (@DropSchemaCmd|~*~| != '') EXEC (@DropSchemaCmd|~*~|);

						-- CHANGE OWNERSHIP OF OWNED OBJECTS
						DECLARE @ChangeOwnershipCmd|~*~| nvarchar(max) = '';
						SELECT @ChangeOwnershipCmd|~*~| = @ChangeOwnershipCmd|~*~| + changeOwnershipCmd
							FROM (
								SELECT 'ALTER AUTHORIZATION ON OBJECT::[' + obj.name + '] TO SCHEMA OWNER;' COLLATE database_default AS changeOwnershipCmd
									FROM sys.database_principals usr
									INNER JOIN sys.objects obj ON obj.principal_id = usr.principal_id
									WHERE usr.name = @UserName|~*~|
								UNION ALL
								SELECT 'ALTER AUTHORIZATION ON CONTRACT::[' + ctr.name + '] TO [dbo];'
									FROM sys.database_principals usr
									INNER JOIN sys.service_contracts ctr ON ctr.principal_id = usr.principal_id
									WHERE usr.name = @UserName|~*~|
								UNION ALL
								SELECT 'ALTER AUTHORIZATION ON SERVICE::[' + svc.name + '] TO [dbo];'
									FROM sys.database_principals usr
									INNER JOIN sys.services svc ON svc.principal_id = usr.principal_id
									WHERE usr.name = @UserName|~*~|
							  UNION ALL
								SELECT 'ALTER AUTHORIZATION ON ROLE::[' + usr.name + '] TO [dbo];'
									FROM sys.database_principals usr
								  INNER JOIN sys.database_principals u ON u.principal_id = usr.owning_principal_id
									WHERE usr.type = 'R'
									AND u.name = @UserName|~*~|
							) OwnedObjs
						IF (@ChangeOwnershipCmd|~*~| != '') EXEC (@ChangeOwnershipCmd|~*~|);

						-- At last, drop the user
						EXEC (N'DROP USER [' + @UserName|~*~| + ']');
				";

				const string dropAllConflictingUsersRaw = @"
					DECLARE @LoginName sysname = '{0}'

					IF (
						db_name() not in ('master', 'model', 'tempdb')
						AND exists(
							SELECT null
								FROM sys.database_principals usr
								INNER JOIN sys.server_principals lgn on usr.sid = lgn.sid
								WHERE usr.name = 'dbo'
								AND lgn.name = @LoginName
						)
					)
					BEGIN
						DECLARE @AlterAuthorizationCmd nvarchar(max) = 'ALTER AUTHORIZATION ON DATABASE::[' + db_name() + '] TO [{1}]';
						EXEC (@AlterAuthorizationCmd);
					END

					IF exists(SELECT null FROM sys.database_principals WHERE name = @LoginName)
					BEGIN
						DECLARE @UserName_1 sysname = @LoginName;
						{2}
					END;

					DECLARE @UserName_2 sysname = (
						SELECT
							usr.name
						FROM
							sys.server_principals lgn
							INNER JOIN sys.database_principals usr ON usr.sid = lgn.sid
						WHERE
							lgn.name = @LoginName
					);

					IF (@UserName_2 is not null)
					BEGIN
						{3}
					END;
				";

				var result = String.Format(CultureInfo.InvariantCulture,
					dropAllConflictingUsersRaw,
					LoginName,
					OdysseyAdminCredentials.AdminUserName,
					dropUserSql.Replace("|~*~|", "_1"),
					dropUserSql.Replace("|~*~|", "_2"));

				return result;
			}
		}

		public abstract IEnumerable<DbRole> DbLevelRoles { get; }
		protected abstract IEnumerable<string> DbLevelPermissions { get; }
		protected virtual IEnumerable<string> ImpersonatePermissionRoles => Array.Empty<string>();
		public virtual bool IsImpersonateEnterpriseDbUser => false;

		#endregion

		protected readonly AdminConnection connection;
		protected readonly string baseDbName;
	}

	#endregion
}
