using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;

namespace CargoWise.Data
{
	public static partial class SqlSecurityUtils
	{
		#region Login

		public static class Login
		{
			public static void Create(AdminConnection connection, string defaultDatabase, string loginName, string password)
			{
				try
				{
					connection.ExecuteNonQuery($@"
CREATE LOGIN {loginName.QuoteName()} WITH PASSWORD = N'{password.QuoteEscapedName('\'')}', DEFAULT_DATABASE = {defaultDatabase.QuoteName()}, CHECK_POLICY = OFF, DEFAULT_LANGUAGE = us_english;
");
				}
				catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.ServerPrincipalAlreadyExists)
				{
					// skip due to the login already exists
				}
			}

			public static bool Exists(AdminConnection connection, string loginName)
			{
				using (((ICurrentDbControl)connection).UseDatabase(Db.SqlMasterDb))
				{
					return connection.Exists($@"
FROM
	sys.server_principals
WHERE 1=1
	AND name = @loginName
"
						, cmd =>
						{
							cmd.AddParameter("@loginName", SqlDbType.NVarChar, 128, loginName);
						});
				}
			}

			public static List<string> GetAllStaffDbLogins(AdminConnection connection, string mainDb)
			{
				using (((ICurrentDbControl)connection).UseDatabase(Db.SqlMasterDb))
				{
					var staffLogins = new List<string>();
					var staffLoginLikePattern = $"{DataUtils.ReplaceSqlLikeWildcard(DbUserRepository.GetStaffDbLoginFullPrefix(mainDb))}%";

					connection.ExecuteReader($@"
SELECT name
FROM sys.server_principals 
WHERE name LIKE @staffLoginLikePattern
UNION
SELECT name
FROM sys.server_principals 
WHERE default_database_name = @defaultDatabase;
					"
						, cmd =>
						{
							cmd.AddParameter("@staffLoginLikePattern", SqlDbType.NVarChar, 128, staffLoginLikePattern);
							cmd.AddParameter("@defaultDatabase", SqlDbType.NVarChar, 128, mainDb);
						}
						, reader =>
						{
							staffLogins.Add((string)reader["name"]);
						});

					return staffLogins;
				}
			}

			public static void Drop(AdminConnection connection, string loginName)
			{
				using (((ICurrentDbControl)connection).UseDatabase(Db.SqlMasterDb))
				{
					try
					{
						connection.ExecuteNonQuery($"DROP LOGIN {loginName.QuoteName()};");
					}
					catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.ServerPrincipalDoesNotExist)
					{
						// skip due to the login does not exist yet
					}
				}
			}

			public static bool TryToGetAssociatedDbUser(AdminConnection connection, string dbName, string loginName, out string associatedDbUser)
			{
				var result = new List<string>();
				using (((ICurrentDbControl)connection).UseDatabase(dbName))
				{
					connection.ExecuteReader(@"
SELECT
	u.name as dbUserName
FROM
	sys.server_principals        AS l
	JOIN sys.database_principals AS u ON u.sid = l.sid
WHERE 1=1
	AND l.name = @loginName
"
					, cmd => cmd.AddParameter("@loginName", SqlDbType.NVarChar, 128, loginName)
					, dataReader => result.Add((string)dataReader["dbUserName"]));
				}

				associatedDbUser = result.FirstOrDefault();
				return result.Any();
			}
		}

		#endregion // Login

		#region DbRole

		public static class DbRole
		{
			public static bool Exists(AdminConnection connection, string dbName, string dbRoleName)
			{
				using (((ICurrentDbControl)connection).UseDatabase(dbName))
				{
					return SqlSecurityUtils.DbRole.Exists(connection, dbRoleName);
				}
			}

			public static bool Exists(AdminConnection connection, string dbRoleName)
			{
				return connection.Exists($@"
FROM
	sys.database_principals
WHERE 1=1
	AND type = 'R'
	AND name = @dbRoleName
"
					, cmd =>
					{
						cmd.AddParameter("@dbRoleName", SqlDbType.NVarChar, 128, dbRoleName);
					});
			}

			public static void Create(AdminConnection connection, string dbName, string dbRoleName)
			{
				using (((ICurrentDbControl)connection).UseDatabase(dbName))
				{
					SqlSecurityUtils.DbRole.Create(connection, dbRoleName);
				}
			}

			public static void Create(AdminConnection connection, string dbRoleName)
			{
				try
				{
					connection.ExecuteNonQuery($"CREATE ROLE {dbRoleName.QuoteName()};");
				}
				catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.DatabasePrincipalAlreadyExists)
				{
					// skip due to the role already exists
				}
			}

			public static void Drop(AdminConnection connection, string dbName, string dbRoleName)
			{
				using (((ICurrentDbControl)connection).UseDatabase(dbName))
				{
					SqlSecurityUtils.DbRole.Drop(connection, dbRoleName);
				}
			}

			public static void Drop(AdminConnection connection, string dbRoleName)
			{
				try
				{
					foreach (var dbUserName in DbRole.GetMembers(connection, dbRoleName))
					{
						DbRole.DropMember(connection, dbRoleName, dbUserName);
					}

					connection.ExecuteNonQuery($"DROP ROLE {dbRoleName.QuoteName()};");
				}
				catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.DatabasePrincipalDoesNotExist)
				{
					// skip due to the role does not exist yet
				}
			}

			public static List<string> GetMembers(AdminConnection connection, string dbName, string dbRoleName)
			{
				using (((ICurrentDbControl)connection).UseDatabase(dbName))
				{
					return SqlSecurityUtils.DbRole.GetMembers(connection, dbRoleName);
				}
			}

			public static List<string> GetMembers(AdminConnection connection, string dbRoleName)
			{
				var members = new List<string>();
				connection.ExecuteReader($@"
SELECT
	m.name
FROM
	sys.database_role_members    AS rm
	JOIN sys.database_principals AS r ON r.principal_id = rm.role_principal_id
	JOIN sys.database_principals AS m ON m.principal_id = rm.member_principal_id
WHERE 1=1
	AND r.name = @dbRoleName

"
					, cmd =>
					{
						cmd.AddParameter("@dbRoleName", SqlDbType.NVarChar, 128, dbRoleName);
					}
					, reader =>
					{
						members.Add((string)reader["name"]);
					});

				return members;
			}

			public static bool Contains(AdminConnection connection, string dbName, string dbRoleName, string dbPrincipalName)
			{
				using (((ICurrentDbControl)connection).UseDatabase(dbName))
				{
					return SqlSecurityUtils.DbRole.Contains(connection, dbRoleName, dbPrincipalName);
				}
			}

			public static bool Contains(AdminConnection connection, string dbRoleName, string dbPrincipalName)
			{
				return connection.Exists($@"
FROM
	sys.database_role_members    AS rm
	JOIN sys.database_principals AS r ON r.principal_id = rm.role_principal_id
	JOIN sys.database_principals AS m ON m.principal_id = rm.member_principal_id
WHERE 1=1
	AND r.name = @dbRoleName
	AND m.name = @dbUserName
"
					, cmd =>
					{
						cmd.AddParameter("@dbRoleName", SqlDbType.NVarChar, 128, dbRoleName);
						cmd.AddParameter("@dbUserName", SqlDbType.NVarChar, 128, dbPrincipalName);
					});
			}

			public static void AddMember(AdminConnection connection, string dbName, string dbRoleName, string dbPrincipalName)
			{
				using (((ICurrentDbControl)connection).UseDatabase(dbName))
				{
					SqlSecurityUtils.DbRole.AddMember(connection, dbRoleName, dbPrincipalName);
				}
			}

			public static void AddMember(AdminConnection connection, string dbRoleName, string dbPrincipalName)
			{
				connection.ExecuteNonQuery($"ALTER ROLE {dbRoleName.QuoteName()} ADD MEMBER {dbPrincipalName.QuoteName()};");
			}

			public static void DropMember(AdminConnection connection, string dbName, string dbRoleName, string dbPrincipalName)
			{
				using (((ICurrentDbControl)connection).UseDatabase(dbName))
				{
					SqlSecurityUtils.DbRole.DropMember(connection, dbRoleName, dbPrincipalName);
				}
			}

			public static void DropMember(AdminConnection connection, string dbRoleName, string dbPrincipalName)
			{
				connection.ExecuteNonQuery($"ALTER ROLE {dbRoleName.QuoteName()} DROP MEMBER {dbPrincipalName.QuoteName()};");
			}
		}

		#endregion // DbRole

		#region DbUser

		public static class DbUser
		{
			public static bool Exists(AdminConnection connection, string dbName, string dbUserName)
			{
				using (((ICurrentDbControl)connection).UseDatabase(dbName))
				{
					return SqlSecurityUtils.DbUser.Exists(connection, dbUserName);
				}
			}

			public static bool Exists(AdminConnection connection, string dbUserName)
			{
				return connection.Exists($@"
FROM
	sys.database_principals
WHERE 1=1
	AND name = @dbUserName
"
					, cmd =>
					{
						cmd.AddParameter("@dbUserName", SqlDbType.NVarChar, 128, dbUserName);
					});
			}

			public static void Create(AdminConnection connection, string dbName, string dbUserName)
			{
				using (((ICurrentDbControl)connection).UseDatabase(dbName))
				{
					SqlSecurityUtils.DbUser.Create(connection, dbUserName);
				}
			}

			public static void Create(AdminConnection connection, string dbUserName)
			{
				try
				{
					connection.ExecuteNonQuery($"CREATE USER {dbUserName.QuoteName()} FOR LOGIN {dbUserName.QuoteName()};");
				}
				catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.DatabasePrincipalAlreadyExists)
				{
					// skip due to the database user already exists
				}
			}

			public static void Drop(AdminConnection connection, string dbName, string dbUserName)
			{
				using (((ICurrentDbControl)connection).UseDatabase(dbName))
				{
					SqlSecurityUtils.DbUser.Drop(connection, dbUserName);
				}
			}

			public static void Drop(AdminConnection connection, string dbUserName)
			{
				try
				{
					connection.ExecuteNonQuery($"DROP USER {dbUserName.QuoteName()};");
				}
				catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.DatabasePrincipalDoesNotExist)
				{
					// skip due to the database user does not exist yet
				}
			}
		}

		#endregion // DbUser

		#region DbPermission

		public static class DbPermission
		{
			public static bool Exists(AdminConnection connection, string dbName, string dbPrincipalName, Data.DbPermission permission, string permissionClassValue)
			{
				using (((ICurrentDbControl)connection).UseDatabase(dbName))
				{
					return SqlSecurityUtils.DbPermission.Exists(connection, dbPrincipalName, permission, permissionClassValue);
				}
			}

			public static bool Exists(AdminConnection connection, string dbPrincipalName, Data.DbPermission permission, string permissionClassValue)
			{
				switch (permission.PermissionClass.Code)
				{
					case DbPermissionClass.Constants.Database.Code:
					case DbPermissionClass.Constants.User.Code:
						return DatabasePermissionCheck();
					default:
						throw new NotImplementedException();
				}

				bool DatabasePermissionCheck()
				{
					return connection.Exists(@"
FROM
	sys.database_permissions     AS per
	JOIN sys.database_principals AS grantee ON grantee.principal_id = per.grantee_principal_id
WHERE 1=1
	AND per.class = @class
	AND per.type = @type
	AND per.state = @state
	AND grantee.name = @dbPrincipalName

"
					, cmd =>
					{
						cmd.AddParameter("@class", SqlDbType.TinyInt, permission.PermissionClass.Code);
						cmd.AddParameter("@type", SqlDbType.Char, 4, permission.PermissionType.Code);
						cmd.AddParameter("@state", SqlDbType.Char, 1, permission.PermissionState.Code);
						cmd.AddParameter("@dbPrincipalName", SqlDbType.NVarChar, 128, dbPrincipalName);
					});
				}
			}

			public static void Create(AdminConnection connection, string dbName, string dbPrincipalName, Data.DbPermission permission, string permissionClassValue)
			{
				using (((ICurrentDbControl)connection).UseDatabase(dbName))
				{
					SqlSecurityUtils.DbPermission.Create(connection, dbPrincipalName, permission, permissionClassValue);
				}
			}

			public static void Create(AdminConnection connection, string dbPrincipalName, Data.DbPermission permission, string permissionClassValue)
			{
				connection.ExecuteNonQuery($"{permission}::{permissionClassValue.QuoteName()} TO {dbPrincipalName.QuoteName()};");
			}

			public static void Revoke(AdminConnection connection, string dbName, string dbPrincipalName, Data.DbPermission permission, string permissionClassValue)
			{
				using (((ICurrentDbControl)connection).UseDatabase(dbName))
				{
					connection.ExecuteNonQuery($"{permission.ToRevoke()}::{permissionClassValue.QuoteName()} TO {dbPrincipalName.QuoteName()};");
				}
			}

			public static List<string> GetImpersonatePermissionGranteePrincipalNames(AdminConnection connection, string dbName, string grantorPrincipalName)
			{
				var sqlGetImpersonatePermissionsGranteePrincipalNames = @"
SELECT 
	grantee.name
FROM
	sys.database_permissions     AS per
	JOIN sys.database_principals AS grantee ON grantee.principal_id = per.grantee_principal_id
	JOIN sys.database_principals AS grantor ON grantor.principal_id = per.grantor_principal_id
WHERE 1=1
	AND per.class = 4
    AND per.type = 'IM'
	AND grantor.name = @grantorPrincipalName";

				using (((ICurrentDbControl)connection).UseDatabase(dbName))
				using (var cmd = connection.Command(sqlGetImpersonatePermissionsGranteePrincipalNames))
				{
					cmd.AddParameter("@grantorPrincipalName", SqlDbType.NVarChar, 128, grantorPrincipalName);

					var result = new List<string>();
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							var granteePrincipalName = reader["name"].ToString();

							result.Add(granteePrincipalName);
						}
					}

					return result;
				}
			}

			public static void RevokeImpersonatePermissions(AdminConnection connection, string dbName, string grantorPrincipalName)
			{
				var granteePrincipalNames = GetImpersonatePermissionGranteePrincipalNames(connection, dbName, grantorPrincipalName);
				var permission = new Data.DbPermission(DbPermissionState.Grant, DbPermissionType.Impersonate, DbPermissionClass.User);
				foreach (var granteePrincipalName in granteePrincipalNames)
				{
					Revoke(connection, dbName, granteePrincipalName, permission, grantorPrincipalName);
				}
			}
		}

		#endregion // DbPermission

		#region ServerPermission

		public static class ServerPermission
		{
			public static bool ExistsByGrantor(AdminConnection connection, string grantorPrincipalName, Data.DbPermission permission)
			{
				using (((ICurrentDbControl)connection).UseDatabase(Db.SqlMasterDb))
				{
					switch (permission.PermissionClass.Code)
					{
						case DbPermissionClass.Constants.Login.Code:
							return LoginPermissionCheck();
						default:
							throw new NotImplementedException();
					}

					bool LoginPermissionCheck()
					{
						return connection.Exists(@"
FROM
	sys.server_permissions     AS per
	JOIN sys.server_principals AS grantor ON grantor.principal_id = per.grantor_principal_id
WHERE 1=1
	AND per.class = @class
	AND per.type = @type
	AND per.state = @state
	AND grantor.name = @grantorPrincipalName
"
						, cmd =>
						{
							cmd.AddParameter("@class", SqlDbType.TinyInt, permission.PermissionClass.Code);
							cmd.AddParameter("@type", SqlDbType.Char, 4, permission.PermissionType.Code);
							cmd.AddParameter("@state", SqlDbType.Char, 1, permission.PermissionState.Code);
							cmd.AddParameter("@grantorPrincipalName", SqlDbType.NVarChar, 128, grantorPrincipalName);
						});
					}
				}
			}

			public static void Create(AdminConnection connection, Data.DbPermission permission, string permissionClassValue, string granteePrincipalName)
			{
				using (((ICurrentDbControl)connection).UseDatabase(Db.SqlMasterDb))
				{
					connection.ExecuteNonQuery($"{permission}::{permissionClassValue.QuoteName()} TO {granteePrincipalName.QuoteName()};");
				}
			}

			public static void Revoke(AdminConnection connection, Data.DbPermission permission, string permissionClassValue, string granteePrincipalName)
			{
				using (((ICurrentDbControl)connection).UseDatabase(Db.SqlMasterDb))
				{
					connection.ExecuteNonQuery($"{permission.ToRevoke()}::{permissionClassValue.QuoteName()} TO {granteePrincipalName.QuoteName()};");
				}
			}

			public static List<string> GetImpersonatePermissionGranteePrincipalNames(AdminConnection connection, string grantorPrincipalName)
			{
				using (((ICurrentDbControl)connection).UseDatabase(Db.SqlMasterDb))
				{
					var sqlGetImpersonatePermissionGranteePrincipalNames = @"
SELECT 
	grantee.name
FROM
	sys.server_permissions     AS per
	JOIN sys.server_principals AS grantee ON grantee.principal_id = per.grantee_principal_id
	JOIN sys.server_principals AS grantor ON grantor.principal_id = per.grantor_principal_id
WHERE 1=1
	AND per.class = 101
    AND per.type = 'IM'
	AND grantor.name = @grantorPrincipalName";

					using (var cmd = connection.Command(sqlGetImpersonatePermissionGranteePrincipalNames))
					{
						cmd.AddParameter("@grantorPrincipalName", SqlDbType.NVarChar, 128, grantorPrincipalName);

						var result = new List<string>();
						using (var reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								var granteeName = reader["name"].ToString().Trim();

								result.Add(granteeName);
							}
						}

						return result;
					}
				}
			}

			public static void RevokeImpersonatePermissions(AdminConnection connection, string grantorPrincipalName)
			{
				var granteePrincipalNames = GetImpersonatePermissionGranteePrincipalNames(connection, grantorPrincipalName);
				var permission = new Data.DbPermission(DbPermissionState.Grant, DbPermissionType.Impersonate, DbPermissionClass.Login);
				foreach (var granteePrincipalName in granteePrincipalNames)
				{
					ServerPermission.Revoke(connection, permission, grantorPrincipalName, granteePrincipalName);
				}
			}
		}

		#endregion // ServerPermission

		public static bool LoginSidMatchesDbUserSid(AdminConnection connection, string dbName, string loginName)
		{
			using (((ICurrentDbControl)connection).UseDatabase(dbName))
			{
				return SqlSecurityUtils.LoginSidMatchesDbUserSid(connection, loginName);
			}
		}

		public static bool LoginSidMatchesDbUserSid(AdminConnection connection, string loginName)
		{
			return connection.Exists(@"
FROM
	sys.server_principals        AS l
	JOIN sys.database_principals AS u ON u.sid = l.sid
WHERE 1=1
	AND l.name = @loginName
	AND u.name = @loginName
"
				, cmd =>
				{
					cmd.AddParameter("@loginName", SqlDbType.NVarChar, 128, loginName);
				});
		}

		public static List<string> GetCurrentMainDbApplicationLogins(AdminConnection connection, string mainDb)
		{
			var result = new List<string>();

			var applicationLoginLikePattern = DataUtils.ReplaceSqlLikeWildcard(mainDb + "_") + "%";

			using (((ICurrentDbControl)connection).UseDatabase(Db.SqlMasterDb))
			{
				connection.ExecuteReader($@"-- GetCurrentMainDbApplicationLogins
SELECT
	name
FROM
	sys.server_principals
WHERE
	name LIKE @applicationLoginLikePattern

"
					, cmd =>
					{
						cmd.AddParameter("@applicationLoginLikePattern", SqlDbType.NVarChar, 128, applicationLoginLikePattern);
					}
					, reader => result.Add((string)reader["name"]));
			}

			return result;
		}
	}
}

#region Test
#if DEBUG

#region Partial class

namespace CargoWise.Data
{
	using NUnit.Framework;

	public static partial class SqlSecurityUtils
	{
		public static void CreateDbUserWithoutLogin_ForTest(AdminConnection connection, string dbName, string dbUserName)
		{
			using (((ICurrentDbControl)connection).UseDatabase(dbName))
			{
				try
				{
					connection.ExecuteNonQuery($"CREATE USER {dbUserName.QuoteName()} WITHOUT LOGIN");
				}
				catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.DatabasePrincipalAlreadyExists)
				{
					// skip due to the role already exists
				}
			}
		}

		public static void AssertLoginExists(AdminConnection connection, string loginName, bool expected)
		{
			var actual = SqlSecurityUtils.Login.Exists(connection, loginName);

			Assertion.AssertEquals($"Does login {loginName.QuoteName()} exist?", expected, actual);
		}

		public static void AssertDbRoleExists(AdminConnection connection, string dbRoleName, bool expected)
		{
			AssertDbRoleExists(connection, connection.CurrentDatabase, dbRoleName, expected);
		}

		public static void AssertDbRoleExists(AdminConnection connection, string dbName, string dbRoleName, bool expected)
		{
			var actual = SqlSecurityUtils.DbRole.Exists(connection, dbName, dbRoleName);

			Assertion.AssertEquals($"Does database role {dbRoleName.QuoteName()} exist on database {dbName.QuoteName()}?", expected, actual);
		}

		public static void AssertDbUserExists(AdminConnection connection, string dbName, string dbUserName, bool expected)
		{
			var actual = SqlSecurityUtils.DbUser.Exists(connection, dbName, dbUserName);

			Assertion.AssertEquals($"Does database user {dbUserName.QuoteName()} exist on database {dbName.QuoteName()}?", expected, actual);
		}

		public static void AssertDbPermissionExists(AdminConnection connection, string dbPrincipalName, Data.DbPermission permission, string permissionClassValue, bool expected)
		{
			AssertDbPermissionExists(connection, connection.CurrentDatabase, dbPrincipalName, permission, permissionClassValue, expected);
		}

		public static void AssertDbPermissionExists(AdminConnection connection, string dbName, string dbPrincipalName, Data.DbPermission permission, string permissionClassValue, bool expected)
		{
			var actual = SqlSecurityUtils.DbPermission.Exists(connection, dbName, dbPrincipalName, permission, permissionClassValue);

			Assertion.AssertEquals($"Does permission {permission.ToString().QuoteName()} of database principal {dbPrincipalName.QuoteName()} exist on database {dbName.QuoteName()}?", expected, actual);
		}

		public static void AssertServerPermissionExistsByGrantor(AdminConnection connection, string grantorPrincipalName, Data.DbPermission permission, bool expected)
		{
			var actual = SqlSecurityUtils.ServerPermission.ExistsByGrantor(connection, grantorPrincipalName, permission);

			Assertion.AssertEquals($"Does permission {permission.ToString().QuoteName()} of server principal {grantorPrincipalName.QuoteName()} exist on server?", expected, actual);
		}

		public static void AssertDbRoleContainsDbUser(AdminConnection connection, string dbName, string dbRoleName, string dbUserName, bool expected)
		{
			var actual = SqlSecurityUtils.DbRole.Contains(connection, dbName, dbRoleName, dbUserName);

			Assertion.AssertEquals($"Does database role {dbRoleName.QuoteName()} on database {dbName.QuoteName()} contain database user {dbUserName.QuoteName()}?", expected, actual);
		}

		public static void AssertLoginSidMatchesDbUserSid(AdminConnection connection, string dbName, string loginName, bool expected)
		{
			var actual = SqlSecurityUtils.LoginSidMatchesDbUserSid(connection, dbName, loginName);

			Assertion.AssertEquals($"Does SID of the login {loginName.QuoteName()} match SID of its database user {loginName.QuoteName()} on database {dbName.QuoteName()}?", expected, actual);
		}
	}
}

#endregion // Partial class

#endif
#endregion
