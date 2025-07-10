using System;
using System.Data;
using System.DirectoryServices;
using System.Globalization;
using CargoWise.Common;
using NUnit.Framework;

namespace CargoWise.Data.SqlServer.Testing
{
	public class DbSecurityTest : TestCase
	{
		public void TestLockDownMsdbAccessWithEnterpriseDbUserChangedUserNameWhileOldDbUserAlreadyExists()
		{
			using (var connection = Db.NewAdminConnection())
			using (((ICurrentDbControl)connection).UseDatabase("msdb"))
			{
				const string testEnterpriseOldStaffUser = "EnterpriseDbUser_Odyssey_Test.StaffLogin";
				const string testEnterpriseNewStaffUser = testEnterpriseOldStaffUser + ".New";
				try
				{
					// Arrange
					connection.ExecuteNonQuery($"CREATE LOGIN [{testEnterpriseOldStaffUser}] WITH PASSWORD = N'NotImportantPassword', DEFAULT_DATABASE = master, CHECK_POLICY = OFF, DEFAULT_LANGUAGE = us_english;");
					connection.ExecuteNonQuery($"CREATE USER [{testEnterpriseOldStaffUser}] FOR LOGIN [{testEnterpriseOldStaffUser}];");
					connection.ExecuteNonQuery($"ALTER LOGIN [{testEnterpriseOldStaffUser}] WITH NAME = [{testEnterpriseNewStaffUser}]");

					// Act
					AssertNoExceptionThrown(() => DbSecurity.LockDownMsdbAccessOnServer(connection, Environment.MachineName, new[] { testEnterpriseNewStaffUser }));

					// Assert
					Assert($"Should Exist login {testEnterpriseNewStaffUser}.", SqlSecurityUtils.Login.Exists(connection, testEnterpriseNewStaffUser));
					Assert($"Should not Exist login {testEnterpriseOldStaffUser}.", !SqlSecurityUtils.Login.Exists(connection, testEnterpriseOldStaffUser));
					Assert($"Should Exist DbUser {testEnterpriseNewStaffUser}.", SqlSecurityUtils.DbUser.Exists(connection, testEnterpriseNewStaffUser));
					Assert($"Should not Exist DbUser {testEnterpriseOldStaffUser}.", !SqlSecurityUtils.DbUser.Exists(connection, testEnterpriseOldStaffUser));
				}
				finally
				{
					DropLoginAndUserIfExists(connection, new[] { testEnterpriseOldStaffUser, testEnterpriseNewStaffUser });
				}
			}
		}

		[TestRequiresAdministrativePrivileges("Need to create a test window user")]
		public void TestLockDownMsdbAccessWithAdUserChangedUserNameWhileOldDbUserAlreadyExists()
		{
			using (var connection = Db.NewAdminConnection())
			using (((ICurrentDbControl)connection).UseDatabase("msdb"))
			{
				const string testAdOldStaffUser = "AD.StaffLogin";
				const string testAdNewStaffUser = testAdOldStaffUser + ".New";
				var testOldLogin = $"{Environment.MachineName}\\{testAdOldStaffUser}";
				var testNewLogin = $"{Environment.MachineName}\\{testAdNewStaffUser}";

				using (var directoryEntry = new DirectoryEntry($"WinNT://{Environment.MachineName},computer"))
				using (var newUser = directoryEntry.Children.Add(testAdOldStaffUser, "user"))
				{
					try
					{
						// Arrange
						newUser.Invoke("SetPassword", new object[] { "2022@NotImportant" });
						newUser.Invoke("Put", new object[] { "Description", "Test User for DbSecurityTest" });
						newUser.CommitChanges();

						connection.ExecuteNonQuery($"CREATE LOGIN [{testOldLogin}] FROM WINDOWS;");
						connection.ExecuteNonQuery($"CREATE USER [{testOldLogin}] FOR LOGIN [{testOldLogin}];");

						newUser.Rename(testAdNewStaffUser);
						newUser.CommitChanges();
						connection.ExecuteNonQuery($"ALTER LOGIN [{testOldLogin}] WITH NAME = [{testNewLogin}]");

						// Act
						AssertNoExceptionThrown(() => DbSecurity.LockDownMsdbAccessOnServer(connection, Environment.MachineName, new[] { testNewLogin }));

						// Assert
						Assert($"Should Exist login {testNewLogin}.", SqlSecurityUtils.Login.Exists(connection, testNewLogin));
						Assert($"Should not Exist login {testOldLogin}.", !SqlSecurityUtils.Login.Exists(connection, testOldLogin));
						Assert($"Should Exist DbUser {testNewLogin}.", SqlSecurityUtils.DbUser.Exists(connection, testNewLogin));
						Assert($"Should not Exist DbUser {testOldLogin}.", !SqlSecurityUtils.DbUser.Exists(connection, testOldLogin));
					}
					finally
					{
						DropLoginAndUserIfExists(connection, new[] { testOldLogin, testNewLogin });
						directoryEntry.Children.Remove(newUser);
					}
				}
			}
		}

		void DropLoginAndUserIfExists(AdminConnection connection, string[] staffUsers)
		{
			foreach (string staffUser in staffUsers)
			{
				if (SqlSecurityUtils.Login.Exists(connection, staffUser))
				{
					SqlSecurityUtils.Login.Drop(connection, staffUser);
				}
				if (SqlSecurityUtils.DbUser.Exists(connection, staffUser))
				{
					SqlSecurityUtils.DbUser.Drop(connection, staffUser);
				}
			}

			SqlSecurityUtils.DbRole.Drop(connection, Db.SqlMsdb, DbRoleTypes.Constants.CwMsdbAccessDeniedRole);
		}

		public static void AssertDatabasePrincipal(AdminConnection connection, string dbName, string principalName, bool expectedExists, string message = "")
		{
			var sqlText = string.Format(@"
				SELECT count(*)
				FROM [{0}].sys.database_principals dl
				WHERE dl.name = '{1}'",
				dbName, principalName);
			var actualExists = ((int)connection.ExecuteScalar(sqlText) == 1);
			AssertEquals(string.Format("{0}: Principal [{1}] exists on database [{2}]?", message, principalName, dbName), expectedExists, actualExists);
		}

		public static void AssertSchema(AdminConnection connection, string dbName, string schemaName, bool expectedExists, string message = "")
		{
			var sqlText = string.Format(@"
				SELECT count(*)
				FROM [{0}].sys.schemas sch
				WHERE sch.name = '{1}'",
				dbName, schemaName);
			var actualExists = ((int)connection.ExecuteScalar(sqlText) == 1);
			AssertEquals(string.Format("{0}: Schema [{1}] exists on database [{2}]?", message, schemaName, dbName), expectedExists, actualExists);
		}

		public static void AssertDatabaseRoleMembership(AdminConnection connection, string dbName, string roleName, string userName, bool expectedIsMember)
		{
			var sqlText = string.Format(@"
				SELECT count(*)
				FROM [{0}].sys.database_role_members drm
				INNER JOIN [{0}].sys.database_principals dr ON dr.principal_id = drm.role_principal_id
				INNER JOIN [{0}].sys.database_principals dl ON dl.principal_id = drm.member_principal_id
				WHERE dr.name = '{1}'
				AND dl.name = '{2}'",
				dbName, roleName, userName);
			var actualIsMember = ((int)connection.ExecuteScalar(sqlText) == 1);
			AssertEquals(string.Format("Is user [{0}] member of role [{1}] on database [{2}]?", userName, roleName, dbName), expectedIsMember, actualIsMember);
		}

		public static void AssertDatabasePermission(AdminConnection connection, string dbName, string permissionName, string userName, string objectName, string columnName, string expectedPermissionState)
		{
			var sqlText = string.Format(@"
				SELECT dp.state_desc
				FROM [{0}].sys.database_permissions dp
				INNER JOIN [{0}].sys.database_principals dl ON dl.principal_id = dp.grantee_principal_id
				LEFT JOIN [{0}].sys.objects obj ON obj.object_id = dp.major_id
				LEFT JOIN [{0}].sys.columns col ON col.object_id = obj.object_id AND col.column_id = dp.minor_id
				WHERE dp.permission_name = '{1}'
				AND dl.name = '{2}'
				AND obj.name {3}
				AND col.name {4}",
				dbName, permissionName, userName,
				((objectName == null) ? "is null" : "= '" + objectName + "'"),
				((columnName == null) ? "is null" : "= '" + columnName + "'")
			);
			var actualPermissionStateObj = connection.ExecuteScalar(sqlText);
			var actualPermissionState = (actualPermissionStateObj == null) ? "REVOKE" : actualPermissionStateObj.ToString();
			AssertEquals(string.Format("User [{0}] permission [{1}] state on database [{2}]:", userName, permissionName, dbName), expectedPermissionState, actualPermissionState);
		}

		public static void AssertRoleSchemaPermission(AdminConnection connection, string dbName, string schemaName, string roleName, string permission, bool isExists = true)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				SELECT
					perm.permission_name
				FROM
					sys.database_permissions perm
				INNER JOIN
					sys.database_principals p
				ON
					perm.grantee_principal_id = p.principal_id
				WHERE
					perm.state = 'G'
					AND SCHEMA_NAME(perm.major_id) = '{0}'
					AND perm.permission_name = '{1}'
					AND p.type = 'R'
					AND p.name = '{2}'
				"
				, schemaName
				, permission
				, roleName
				);

			object result;
			using (((ICurrentDbControl)connection).UseDatabase(dbName))
			{
				result = connection.ExecuteScalar(sqlText);
			}

			if (isExists)
			{
				AssertNotNullOrEmpty(string.Format("Role [{0}] permission [{1}] state on [{2}].[{3}]", roleName, permission, dbName, schemaName), result as string);
			}
			else
			{
				AssertNullOrEmpty(string.Format("Role [{0}] should have no permission [{1}] on [{2}].[{3}]", roleName, permission, dbName, schemaName), result as string);
			}
		}

		public static void AssertTypePermission(AdminConnection connection, string dbName, string permissionName, string userName, string objectName, string expectedPermissionState)
		{
			var sqlText = string.Format(@"
				SELECT dp.state_desc
				FROM [{0}].sys.database_permissions dp
				INNER JOIN [{0}].sys.database_principals dl ON dl.principal_id = dp.grantee_principal_id
				LEFT JOIN [{0}].sys.types typ ON typ.user_type_id = dp.major_id
				WHERE dp.permission_name = '{1}'
				AND dl.name = '{2}'
				AND typ.name {3}",
				dbName, permissionName, userName,
				((objectName == null) ? "is null" : "= '" + objectName + "'")
			);
			var actualPermissionStateObj = connection.ExecuteScalar(sqlText);
			var actualPermissionState = (actualPermissionStateObj == null) ? "REVOKE" : actualPermissionStateObj.ToString();
			AssertEquals(string.Format("User [{0}] permission [{1}] state on database [{2}]:", userName, permissionName, dbName), expectedPermissionState, actualPermissionState);
		}

		public static void AssertPermissionOnSchema(AdminConnection connection, string dbName, string permissionName, string userName, string schemaName, string expectedPermissionState)
		{
			var sqlText = string.Format(@"
				SELECT dp.state_desc
				FROM [{0}].sys.database_permissions dp
				INNER JOIN [{0}].sys.database_principals dl ON dl.principal_id = dp.grantee_principal_id
				LEFT JOIN [{0}].sys.schemas sch ON sch.schema_id = dp.major_id
				WHERE dp.permission_name = '{1}'
				AND dl.name = '{2}'
				AND sch.name = '{3}'",
				dbName, permissionName, userName, schemaName
			);
			object actualPermissionStateObj = connection.ExecuteScalar(sqlText);
			var actualPermissionState = (actualPermissionStateObj == null) ? "REVOKE" : actualPermissionStateObj.ToString();
			AssertEquals(string.Format("User [{0}] permission [{1}] state on schema [{2}], database [{3}]:", userName, permissionName, schemaName, dbName), expectedPermissionState, actualPermissionState);
		}

		public static void AssertOutputLog(string actualLog, params string[] expectedLogEntries)
		{
			AssertEquals("Number of log entries:\r\n\r\n" + actualLog, expectedLogEntries.Length, actualLog.TrimEnd().Split('\n').Length);
			AssertLogEntries(actualLog, expectedLogEntries);
		}

		public static void AssertLogEntries(string actualLog, params string[] expectedLogEntries)
		{
			foreach (string expectedLog in expectedLogEntries)
			{
				AssertEquals(string.Format("Log entry [{0}] is in the output log [{1}]?", expectedLog, actualLog), true, actualLog.IndexOf(expectedLog, StringComparison.OrdinalIgnoreCase) > -1);
			}
		}

		public static void AssertRoleOwnerIsCorrect(AdminConnection connection, string dbName, string roleName, string expectedRoleOwner)
		{
			var sqlText = string.Format(@"
			SELECT u.name FROM [{0}].sys.database_principals dr
			INNER JOIN [{0}].sys.database_principals u ON u.principal_id = dr.owning_principal_id
			WHERE dr.type = 'R'
			AND dr.name = '{1}'",
			dbName, roleName);
			var actual = (string)connection.ExecuteScalar(sqlText);
			AssertEquals(string.Format("Is [{0}] the role owner for role [{1}] in database [{2}]?", expectedRoleOwner, roleName, dbName), expectedRoleOwner, actual);
		}

		public static void AssertObjectPrincipal(AdminConnection connection, string dbName, string schemaName, string objectName, string expectedPrincipalName)
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"
SELECT
	principal_name = own.name
FROM
	[{0}].sys.schemas                       AS sch
	JOIN [{0}].sys.objects                  AS obj ON obj.schema_id = sch.schema_id
	LEFT JOIN [{0}].sys.database_principals AS own ON own.principal_id = obj.principal_id
WHERE 1=1
	AND sch.name = '{1}'
	AND obj.name = '{2}'
"
				, dbName     // 0
				, schemaName // 1
				, objectName // 2
				);

			string actual = null;
			var principalName = connection.ExecuteScalar(sql);
			if (principalName != DBNull.Value)
			{
				actual = principalName.ToString();
			}

			AssertEquals(string.Format("Principal name for the object [{0}].[{1}].[{2}]", dbName, schemaName, objectName), expectedPrincipalName, actual);
		}

		public static void DropRoleOnDatabase(AdminConnection connection, string dbName, string roleName)
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"
DECLARE
	@SqlCmd nvarchar(max) = '';

-- Drop all members
SELECT
	@SqlCmd = @SqlCmd + 'ALTER ROLE [' + dr.name + '] DROP MEMBER [' + dm.name + '];'
FROM
	[{0}].sys.database_role_members    AS drm
	JOIN [{0}].sys.database_principals AS dr  ON dr.principal_id = drm.role_principal_id
	JOIN [{0}].sys.database_principals AS dm  ON dm.principal_id = drm.member_principal_id
WHERE
	dr.name = '{1}'
;

-- Alter authorization on all dependent objects (currently SP only)
SELECT
	@SqlCmd = @SqlCmd + 'ALTER AUTHORIZATION ON [' + sch.name + '].[' + obj.name + '] TO SCHEMA OWNER;'
FROM
	[{0}].sys.schemas                  AS sch
	JOIN [{0}].sys.objects             AS obj ON obj.schema_id = sch.schema_id
	JOIN [{0}].sys.database_principals AS own ON own.principal_id = obj.principal_id
WHERE 1=1
	AND obj.is_ms_shipped = 0
	AND obj.type = 'P'
	AND own.name = '{1}'
;

-- Drop Role IF EXISTS
SELECT
	@SqlCmd = @SqlCmd + 'DROP ROLE [' + name + '];'
FROM
	[{0}].sys.database_principals
WHERE
	name = '{1}' AND type = 'R'
;

EXEC [{0}].sys.sp_executesql @SqlCmd;
"
				, dbName   // 0
				, roleName // 1
				);
			connection.ExecuteNonQuery(sql);
		}

		public static void CreateSchema(AdminConnection connection, string dbName, string schemaName)
		{
			using (((ICurrentDbControl)connection).UseDatabase(dbName))
			{
				if (!connection.Exists("FROM sys.schemas WHERE name = @schemaName", cmd => cmd.AddParameter("@schemaName", SqlDbType.NVarChar, 128, schemaName)))
				{
					connection.ExecuteNonQuery($"CREATE SCHEMA {schemaName.QuoteName()} AUTHORIZATION [dbo]");
				}
			}
		}

		internal static void CreateRole(AdminConnection connection, string roleName)
		{
			connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, $"CREATE ROLE {roleName.QuoteName()}"));
		}

		internal static void GrantRoleSchemaPermissionOnDatabase(AdminConnection connection, string roleName, string schemaName, string permissionName)
		{
			connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, $"GRANT {permissionName} ON SCHEMA :: {schemaName.QuoteName()} TO {roleName.QuoteName()};"));
		}
	}
}
