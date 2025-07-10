using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.SqlSecurity.Test
{
	public static class AssertionsHelper
	{
		public static void AssertHashMatchesPassword(AdminConnection adminConnection, string message, string password, byte[] passwordHash)
		{
			using (var command = adminConnection.Command("SELECT PWDCOMPARE(@password, @passwordHash)"))
			{
				command.AddParameter("@password", SqlDbType.NVarChar, 128, password);
				command.AddParameter("@passwordHash", SqlDbType.VarBinary, 128, passwordHash);
				Assert.That(command.ExecuteScalar(), Is.EqualTo(1), message);
			}
		}

		public static void AssertHashMatchesLoginPasswordHash(AdminConnection adminConnection, string message, string loginName, byte[] hash)
		{
			using (var command = adminConnection.Command("SELECT password_hash FROM sys.sql_logins WHERE name = @loginName"))
			{
				command.AddParameter("@loginName", SqlDbType.NVarChar, 128, loginName);
				Assert.That(hash, Is.EqualTo(command.ExecuteScalar()), message);
			}
		}

		public static void AssumeLoginHashMatchesPassword(AdminConnection adminConnection, string message, string loginName, string password)
		{
			using (var command = adminConnection.Command("SELECT PWDCOMPARE(@password, password_hash) FROM sys.sql_logins WHERE name = @loginName"))
			{
				command.AddParameter("@loginName", SqlDbType.NVarChar, 128, loginName);
				command.AddParameter("@password", SqlDbType.NVarChar, 128, password);

				Assume.That(command.ExecuteScalar(), Is.EqualTo(1), message);
			}
		}

		public static void AssertLoginHashMatchesPassword(AdminConnection adminConnection, string message, string loginName, string password)
		{
			using (var command = adminConnection.Command("SELECT PWDCOMPARE(@password, password_hash) FROM sys.sql_logins WHERE name = @loginName"))
			{
				command.AddParameter("@loginName", SqlDbType.NVarChar, 128, loginName);
				command.AddParameter("@password", SqlDbType.NVarChar, 128, password);

				Assert.That(command.ExecuteScalar(), Is.EqualTo(1), message);
			}
		}

		public static void AssertServerPrincipalHasServerPermissionsOnly(AdminConnection connection, string principalName, Dictionary<string, string[]> expectedPermissionsOnServer)
		{
			using (var command = connection.Command(@"
SELECT
	pm.state,
	permissionAndSecurableIdKey = CONCAT(pm.permission_name, IIF(pm.major_id <> 0 , CONVERT(nvarchar(30), pm.major_id), N''))
FROM sys.server_principals AS p
	JOIN sys.server_permissions AS pm ON pm.grantee_principal_id = p.principal_id
 WHERE 1=1
	AND p.name = @principalName
"))
			{
				command.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName);

				var permissionsActual = new Dictionary<string, List<string>>();
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var state = (string)reader["state"];
						if (!permissionsActual.TryGetValue(state, out List<string> permissions))
						{
							permissionsActual[state] = new List<string>();
						}

						permissionsActual[state].Add((string)reader["permissionAndSecurableIdKey"]);
					}
				}

				Assert.That(permissionsActual.Keys, Is.EquivalentTo(expectedPermissionsOnServer.Keys).IgnoreCase, "Expected permissions states must match actual permisison states.");

				foreach (var permissions in permissionsActual)
				{
					Assert.That(permissions.Value, Is.EquivalentTo(expectedPermissionsOnServer[permissions.Key]).IgnoreCase, $"Principal '{principalName}' should have permissions {string.Join(", ", expectedPermissionsOnServer[permissions.Key].Select(p => $"'{p}'"))} for state '{permissions.Key}' on server and no other.");
				}
			}
		}

		public static void AssertServerPrincipalHasNoRoles(AdminConnection connection, string principalName)
		{
			using (var command = connection.Command(@"
SELECT
	r.name
FROM sys.server_principals AS p
	JOIN sys.server_role_members AS m ON m.member_principal_id = p.principal_id
	JOIN sys.server_principals AS r ON m.role_principal_id = r.principal_id
 WHERE p.name = @principalName
"))
			{
				command.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName);

				var rolesActual = new List<string>();
				using (var reader = command.ExecuteReader())
				{
					Assert.That(reader.Read(), Is.False, $"Login '{principalName}' should have no roles");
				}
			}
		}

		public static void AssertPrincipalMissingCaseSensitive(AdminConnection connection, string principalName, string type)
		{
			using (var command = connection.Command($"SELECT type FROM sys.server_principals WHERE name = @principalName COLLATE {Db.DatabaseCaseSensitiveCollation}"))
			{
				command.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName);

				Assert.That(command.ExecuteScalar(), Is.Null, $"Server principal '{principalName}' of type '{type}' should not be present (case sensitive).");
			}
		}

		public static void AssertPrincipalExistsCaseSensitive(AdminConnection connection, string principalName, string type)
		{
			using (var command = connection.Command($"SELECT type FROM sys.server_principals WHERE name = @principalName COLLATE {Db.DatabaseCaseSensitiveCollation}"))
			{
				command.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName);

				Assert.That((string)command.ExecuteScalar(), Is.EqualTo(type).IgnoreCase, $"Server principal '{principalName}' of type '{type}' should be present (case sensitive).");
			}
		}

		public static void AssertPrincipalExistsAndIsNotDisabled(AdminConnection connection, string principalName, string type)
		{
			using (var command = connection.Command("SELECT type FROM sys.server_principals WHERE name = @principalName AND is_disabled = 0"))
			{
				command.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName);

				Assert.That((string)command.ExecuteScalar(), Is.EqualTo(type).IgnoreCase, $"Principal '{principalName}' of type '{type}' should have been created.");
			}
		}

		public static void AssumePrincipalExistsAndIsNotDisabled(AdminConnection connection, string principalName, string type)
		{
			using (var command = connection.Command("SELECT type FROM sys.server_principals WHERE name = @principalName AND is_disabled = 0"))
			{
				command.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName);

				Assert.That((string)command.ExecuteScalar(), Is.EqualTo(type).IgnoreCase, $"Principal '{principalName}' of type '{type}' should have been created.");
			}
		}

		public static void AssertSqlLoginExpirationPolicyIsDisabled(AdminConnection connection, string loginName)
		{
			using (var command = connection.Command("SELECT is_expiration_checked FROM sys.sql_logins WHERE name = @loginName"))
			{
				command.AddParameter("@loginName", SqlDbType.NVarChar, 128, loginName);

				Assert.That(command.ExecuteScalar(), Is.EqualTo(false), $"Sql login '{loginName}' should have expiration policy disabled.");
			}
		}

		public static void AssertSqlLoginPasswordPolicyIsDisabled(AdminConnection connection, string loginName)
		{
			using (var command = connection.Command("SELECT is_policy_checked FROM sys.sql_logins WHERE name = @loginName"))
			{
				command.AddParameter("@loginName", SqlDbType.NVarChar, 128, loginName);

				Assert.That(command.ExecuteScalar(), Is.EqualTo(false), $"Sql login '{loginName}' should have password policy disabled.");
			}
		}

		public static void AssertSqlLoginDefaultDatabaseEquals(AdminConnection connection, string loginName, string expectedDefaultDatabase)
		{
			using (var command = connection.Command("SELECT default_database_name FROM sys.server_principals WHERE name = @loginName"))
			{
				command.AddParameter("@loginName", SqlDbType.NVarChar, 128, loginName);

				Assert.That(command.ExecuteScalar(), Is.EqualTo(expectedDefaultDatabase).IgnoreCase, $"Sql login '{loginName}' should have default database set to '{expectedDefaultDatabase}'.");
			}
		}

		public static void AssertSqlLoginLanguageEquals(AdminConnection connection, string loginName, string expectedLanguage)
		{
			using (var command = connection.Command("SELECT default_language_name FROM sys.server_principals WHERE name = @loginName"))
			{
				command.AddParameter("@loginName", SqlDbType.NVarChar, 128, loginName);

				Assert.That(command.ExecuteScalar(), Is.EqualTo(expectedLanguage).IgnoreCase, $"Sql login '{loginName}' should have language set to '{expectedLanguage}'.");
			}
		}

		public static void AssertSqlLoginSidEquals(AdminConnection connection, string loginName, string expectedSid)
		{
			using (var command = connection.Command("SELECT sid FROM sys.sql_logins WHERE name = @loginName"))
			{
				command.AddParameter("@loginName", SqlDbType.NVarChar, 128, loginName);

				Assert.That(DataUtils.BytesToHexString((byte[])command.ExecuteScalar()), Is.EqualTo(expectedSid).IgnoreCase, $"Sql login '{loginName}' should have sid set to '{expectedSid}'.");
			}
		}

		public static void AssumePrincipalMissing(AdminConnection connection, string principalName)
		{
			using (var command = connection.Command("SELECT type FROM sys.server_principals WHERE name = @principalName"))
			{
				command.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName);

				Assume.That(command.ExecuteScalar(), Is.Null, $"There should be no Server principal with name '{principalName}'.");
			}
		}

		public static void AssertPrincipalMissing(AdminConnection connection, string principalName)
		{
			using (var command = connection.Command("SELECT type FROM sys.server_principals WHERE name = @principalName"))
			{
				command.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName);

				Assert.That(command.ExecuteScalar(), Is.Null, $"There should be no server principal with name '{principalName}'.");
			}
		}

		public static void AssertCollectionsEquivalent<T>(string message, IEnumerable<T> actualCollection, IEnumerable<T> expectedCollection)
		{
			Assert.That(actualCollection, Is.EquivalentTo(expectedCollection), message);
		}

		public static void AssumeStaffExists(AdminConnection connection, string staffLogin)
		{
			using (var command = connection.Command($@"
SELECT GS_LoginName FROM dbo.GlbStaff AS s WHERE GS_LoginName = @staffLogin
"))
			{
				command.AddParameterBasedOnDbColumn("@staffLogin", staffLogin, GlbStaffSchema.GS_LoginName);
				Assume.That((string)command.ExecuteScalar(), Is.EqualTo(staffLogin).IgnoreCase, $"Staff member '{staffLogin} should exist.");
			}
		}

		public static void AssumeStaffMissing(AdminConnection connection, string staffLogin)
		{
			using (var command = connection.Command($@"
SELECT GS_LoginName FROM dbo.GlbStaff AS s WHERE GS_LoginName = @staffLogin
"))
			{
				command.AddParameterBasedOnDbColumn("@staffLogin", staffLogin, GlbStaffSchema.GS_LoginName);
				Assume.That(command.ExecuteScalar(), Is.Null, $"Staff member '{staffLogin} should not exist.");
			}
		}
	}
}
