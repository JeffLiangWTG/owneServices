using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.SqlSecurity.Test
{
	public static class DatabaseAssertionsHelper
	{
		public static void AssertPrincipalHasGrantPermissionsOnDatabaseOnly(AdminConnection connection, string principalName, params string[] expectedPermissionsOnDatabase)
		{
			using (var command = connection.Command(
				@"
SELECT
	pm.state_desc,
	pm.permission_name,
	pm.class_desc
FROM sys.database_principals AS p
	JOIN sys.database_permissions AS pm ON pm.grantee_principal_id = p.principal_id
 WHERE p.name = @principalName
"))
			{
				command.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName);
				var actualPermissionsOnDatabbase = new List<string>();
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var actualPermission = (string)reader["permission_name"];

						Assert.That((string)reader["class_desc"], Is.EqualTo("DATABASE").IgnoreCase, $"Database principal {principalName} should not have any permissions on class '{reader["class_desc"]}'.");
						Assert.That((string)reader["state_desc"], Is.EqualTo("GRANT").IgnoreCase, $"Database principal {principalName} should not have any '{reader["state_desc"]}' permission on database.");
						Assert.That(expectedPermissionsOnDatabase, Does.Contain(actualPermission).IgnoreCase, $"Database principal {principalName} should not have permission '{actualPermission}' on database.");

						actualPermissionsOnDatabbase.Add(actualPermission);
					}
				}

				Assert.That(actualPermissionsOnDatabbase, Is.EquivalentTo(expectedPermissionsOnDatabase).IgnoreCase, "Actual permissions should match expected permissions.");
			}
		}

		public static void AssertPrincipalHasGrantPermissionsOnDatabase(AdminConnection connection, string principalName, params string[] expectedPermissionsOnDatabase)
		{
			using (var command = connection.Command(
				@"
SELECT
	pm.state_desc,
	pm.permission_name
FROM sys.database_principals AS p
	JOIN sys.database_permissions AS pm ON 1=1
		AND pm.grantee_principal_id = p.principal_id
		AND pm.class = 0
 WHERE 1=1
	AND p.name = @principalName
"))
			{
				command.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName);
				var actualPermissionsOnDatabase = new List<string>();

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var actualPermission = (string)reader["permission_name"];

						Assert.That((string)reader["state_desc"], Is.EqualTo("GRANT").IgnoreCase, $"Database principal {principalName} should not have any '{reader["state_desc"]}' permission on database.");
						Assert.That(expectedPermissionsOnDatabase, Does.Contain(actualPermission).IgnoreCase, $"Database principal {principalName} should not have permission '{actualPermission}' on database.");

						actualPermissionsOnDatabase.Add(actualPermission);
					}
				}

				Assert.That(actualPermissionsOnDatabase, Is.EquivalentTo(expectedPermissionsOnDatabase).IgnoreCase, "Actual permissions should match expected permissions.");
			}
		}

		public static void AssertPrincipalIsAMemeberOfRoles(AdminConnection connection, string principalName, params string[] databaseRoles)
		{
			using (var command = connection.Command(
				@"
SELECT
	roleName = roles.name
FROM sys.database_principals AS p
	JOIN sys.database_role_members AS rm ON 1=1
		AND rm.member_principal_id = p.principal_id
	JOIN sys.database_principals AS roles ON 1=1
		AND roles.principal_id = rm.role_principal_id
WHERE 1=1
	AND p.name = @principalName
"))
			{
				command.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName);
				var permissionsActual = new List<string>();
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						permissionsActual.Add((string)reader["roleName"]);
					}
				}

				Assert.That(permissionsActual, Is.EquivalentTo(databaseRoles).IgnoreCase, $"Database principal '{principalName}' should be a member of the following database roles {string.Join(", ", databaseRoles.Select(role => $"'{role}'"))} and no other roles.");
			}
		}

		public static void AssertPrincipalHasGrantImpersonatePermissionsOnUsersAndNoOtherPermissionsOnPrincipals(AdminConnection connection, string principalName, params string[] expectedImpersonatedUsers)
		{
			using (var command = connection.Command(
				@"
SELECT
	state_desc = pm.state_desc
	, permission_name = pm.permission_name
	, grantor = grantor.name
FROM sys.database_principals AS grantee
	JOIN sys.database_permissions AS pm ON 1=1
		AND pm.grantee_principal_id = grantee.principal_id
		AND pm.class = 4
	JOIN sys.database_principals AS grantor ON pm.grantor_principal_id = grantor.principal_id
WHERE grantee.name = @principalName
"))
			{
				command.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName);
				var actualImpersonatedUsers = new List<string>();
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var impersonatedUser = (string)reader["grantor"];
						Assert.That((string)reader["state_desc"], Is.EqualTo("GRANT").IgnoreCase, $"Database principal '{principalName}' should not have any '{reader["state_desc"]}' permissions on principals.");
						Assert.That((string)reader["permission_name"], Is.EqualTo("IMPERSONATE").IgnoreCase, $"Database principal '{principalName}' is only expected to have IMPERSONATE permission on principals.");
						Assert.That(expectedImpersonatedUsers, Does.Contain(impersonatedUser).IgnoreCase, $"Database principal '{principalName}' should have IMPERSONATE permission on user '{impersonatedUser}'.");

						actualImpersonatedUsers.Add(impersonatedUser);
					}
				}

				Assert.That(actualImpersonatedUsers, Is.EquivalentTo(expectedImpersonatedUsers).IgnoreCase, "Actual impersonated users should match expected impersonated users.");
			}
		}

		public static void AssertPrincipalHasNoPermissionsOnOtherPrincipals(AdminConnection connection, string principalName)
		{
			var permissionOnPrincipalExists = connection.Exists(
				@"
FROM sys.database_principals AS grantee
	JOIN sys.database_permissions AS pm ON 1=1
		AND pm.grantee_principal_id = grantee.principal_id
		AND pm.class = 4
	JOIN sys.database_principals AS grantor ON pm.grantor_principal_id = grantor.principal_id
WHERE grantee.name = @principalName
",
				cmd => cmd.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName));
			Assert.That(permissionOnPrincipalExists, Is.False, $"There should be no permissions on othe principals granted to '{principalName}'.");
		}

		public static void AssertPrincipalHasGrantPermissionsOnSchema(AdminConnection connection, string principalName, string schemaName, params string[] expectedPermissionsOnSchema)
		{
			using (var command = connection.Command(
				@"
SELECT
	pm.state_desc,
	pm.permission_name
FROM sys.database_principals AS grantee
	JOIN sys.database_permissions AS pm ON 1=1
		AND pm.grantee_principal_id = grantee.principal_id
		AND pm.class = 3
WHERE 1=1
	AND grantee.name = @principalName
	AND pm.major_id = SCHEMA_ID(@schemaName)
"))
			{
				command.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName);
				command.AddParameter("@schemaName", SqlDbType.NVarChar, 128, schemaName);

				var actualPermissionsOnSchema = new List<string>();

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var actualPermission = (string)reader["permission_name"];

						Assert.That((string)reader["state_desc"], Is.EqualTo("GRANT").IgnoreCase, $"Database principal {principalName} should not have any '{reader["state_desc"]}' permission on schema '{schemaName}'.");
						Assert.That(expectedPermissionsOnSchema, Does.Contain(actualPermission).IgnoreCase, $"Database principal {principalName} should not have permission '{actualPermission}' on '{schemaName}'.");

						actualPermissionsOnSchema.Add(actualPermission);
					}
				}

				Assert.That(actualPermissionsOnSchema, Is.EquivalentTo(expectedPermissionsOnSchema).IgnoreCase, $"Actual permissions should match expected permissions for schema '{schemaName}'.");
			}
		}

		public static void AssertPrincipalHasNoPermissionsOnObjectsOtherThanSpecified(AdminConnection connection, string principalName, params string[] objectNames)
		{
			using (var command = connection.Command(
	$@"
SELECT
	pm.state_desc,
	pm.permission_name,
	objectName = OBJECT_NAME(pm.major_id)
FROM sys.database_principals AS grantee
	JOIN sys.database_permissions AS pm ON 1=1
		AND pm.grantee_principal_id = grantee.principal_id
		AND pm.class = 1
 WHERE 1=1
	AND grantee.name = @principalName
	{string.Join(System.Environment.NewLine, objectNames.Select((objectName, index) => $"AND pm.major_id <> OBJECT_ID(@objectName{index})"))}
"))
			{
				command.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName);
				for (var i = 0; i < objectNames.Length; i++)
				{
					command.AddParameter($"@objectName{i}", SqlDbType.NVarChar, 261, objectNames[i]);
				}

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						Assert.Fail($"Database principal '{principalName}' should have no '{reader["state_desc"]} {reader["permission_name"]}' permission on OBJECT '{reader["objectName"]}'.");
					}
				}
			}
		}

		public static void AssertPrincipalHasGrantPermissionsOnObjectOfTypeAndNoOtherPermissionsOnThisType(AdminConnection connection, string principalName, string objectType, string objectName, params string[] expectedPermissions)
		{
			using (var command = connection.Command(
				@"
SELECT
	pm.state_desc,
	pm.permission_name
FROM sys.database_principals AS grantee
	JOIN sys.database_permissions AS pm ON 1=1
		AND pm.grantee_principal_id = grantee.principal_id
		AND pm.class = 1
	JOIN sys.all_objects AS obj ON obj.object_id = pm.major_id
 WHERE 1=1
	AND grantee.name = @principalName
	AND obj.type = @objectType
"))
			{
				command.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName);
				command.AddParameter("@objectType", SqlDbType.NVarChar, 128, objectType);
				command.AddParameter("@objectName", SqlDbType.NVarChar, 261, objectName);

				var actualPermissionsOnObject = new List<string>();

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var actualPermission = (string)reader["permission_name"];

						Assert.That((string)reader["state_desc"], Is.EqualTo("GRANT").IgnoreCase, $"Database principal {principalName} should not have any '{reader["state_desc"]}' permission on object '{objectName}'.");
						Assert.That(expectedPermissions, Does.Contain(actualPermission).IgnoreCase, $"Database principal {principalName} should not have permission '{actualPermission}' on object '{objectName}'.");

						actualPermissionsOnObject.Add(actualPermission);
					}
				}

				Assert.That(actualPermissionsOnObject, Is.EquivalentTo(expectedPermissions).IgnoreCase, $"Actual permissions should match expected permissions for object '{objectName}'.");
			}
		}

		public static void AssertPrincipalHasGrantPermissionsOnAllCustomTableValueTypesAndNoOther(AdminConnection connection, string principalName, params string[] expectedPermissionsOnType)
		{
			using (var command = connection.Command(
				@"
SELECT name, user_type_id FROM sys.types WHERE system_type_id = 243
"))
			{
				var typesInfo = new Dictionary<string, int>();
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						typesInfo.Add((string)reader["name"], (int)reader["user_type_id"]);
					}
				}

				command.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName);
				command.AddParameter("@typeId", SqlDbType.Int, 0);

				foreach (var typeInfo in typesInfo)
				{
					command.CommandText = $@"
SELECT
	state_desc,
	permission_name
FROM sys.database_permissions AS pm
	JOIN sys.database_principals AS grantee ON grantee.principal_id = pm.grantee_principal_id
WHERE 1=1
	AND major_id = @typeId
	AND grantee.name = @principalName
	AND pm.class = 6
";
					command.SetParameterValue("@typeId", typeInfo.Value);
					var actualPermissionsOnType = new List<string>();
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							var actualPermission = (string)reader["permission_name"];
							Assert.That((string)reader["state_desc"], Is.EqualTo("GRANT").IgnoreCase, $"Database principal '{principalName}' should not have any '{reader["state_desc"]}' permissions on any custom table value type.");
							Assert.That(expectedPermissionsOnType, Does.Contain(actualPermission).IgnoreCase, $"Database principal {principalName} should not have permission '{actualPermission}' on custom table value type '{typeInfo.Value}'.");

							actualPermissionsOnType.Add(actualPermission);
						}
					}

					Assert.That(actualPermissionsOnType, Is.EquivalentTo(expectedPermissionsOnType).IgnoreCase, $"Actual permissions on custom table value type '{typeInfo.Value}' should match expected.");
				}
			}
		}

		public static void AssertPrincipalHasNoPermissionsOnTypesThatAreNotCustomTableValueTypes(AdminConnection connection, string principalName)
		{
			using (var command = connection.Command(
				@"
SELECT
	pm.permission_name
FROM sys.database_principals AS grantee
	JOIN sys.database_permissions AS pm ON 1=1
		AND pm.grantee_principal_id = grantee.principal_id
	JOIN sys.types AS tp ON 1=1
		AND pm.major_id = tp.user_type_id
		AND pm.class = 6
		AND tp.system_type_id <> 243
 WHERE grantee.name = @principalName
"))
			{
				command.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName);

				var permissionsActual = new List<string>();
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						permissionsActual.Add((string)reader["permission_name"]);
					}
				}

				Assert.That(permissionsActual, Is.Empty, $"Database principal '{principalName}' should have no permissions on types that are not custom table value types.");
			}
		}

		public static void AssertPrincipalHasGrantPermissionsOnAllObjectsOfTypeAndNoOther(AdminConnection connection, string principalName, string objectType, params string[] expectedPermissionsOnObject)
		{
			using (var command = connection.Command(
				@"
SELECT object_id FROM sys.all_objects WHERE type = @objectType AND is_ms_shipped = 0
"))
			{
				command.AddParameter("@objectType", SqlDbType.NVarChar, 128, objectType);
				var objectIds = new List<int>();
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						objectIds.Add((int)reader["object_id"]);
					}
				}

				command.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName);
				command.AddParameter("@objectId", SqlDbType.Int, 0);

				foreach (var objectId in objectIds)
				{
					command.CommandText = $@"
SELECT
	state_desc,
	permission_name
FROM sys.database_permissions AS pm
	JOIN sys.database_principals AS grantee ON 1=1
		AND grantee.principal_id = pm.grantee_principal_id
WHERE 1=1
	AND major_id = @objectId
	AND grantee.name = @principalName
";
					command.SetParameterValue("@objectId", objectId);

					var actualPermissionsOnObject = new List<string>();

					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							var actualPermission = (string)reader["permission_name"];
							Assert.That((string)reader["state_desc"], Is.EqualTo("GRANT").IgnoreCase, $"Database principal '{principalName}' should not have any '{reader["state_desc"]}' permissions on any object of type '{objectType}'.");
							Assert.That(expectedPermissionsOnObject, Does.Contain(actualPermission).IgnoreCase, $"Database principal {principalName} should not have permission '{actualPermission}' on object of type '{objectType}'.");

							actualPermissionsOnObject.Add(actualPermission);
						}
					}

					Assert.That(actualPermissionsOnObject, Is.EquivalentTo(expectedPermissionsOnObject).IgnoreCase, $"Actual permissions on object of type '{objectType}' should match expected.");
				}
			}
		}

		public static void AssertPrincipalHasNoPermissionsOnMicrosoftShippedObjects(AdminConnection connection, string principalName)
		{
			using (var command = connection.Command(
				@"
SELECT
	pm.permission_name
FROM sys.database_principals AS grantee
	JOIN sys.database_permissions AS pm ON 1=1
		AND pm.grantee_principal_id = grantee.principal_id
	JOIN sys.all_objects AS obj ON 1=1
		AND pm.major_id = obj.object_id
		AND pm.class = 1
 WHERE grantee.name = @principalName AND obj.is_ms_shipped = 1
"))
			{
				command.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName);

				var permissionsActual = new List<string>();
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						permissionsActual.Add((string)reader["permission_name"]);
					}
				}

				Assert.That(permissionsActual, Is.Empty, $"Database principal '{principalName}' should have no permissions on microsoft shipped objects.");
			}
		}

		public static void AssertPrincipalHasNoPermissionsOnClassesOtherThanExcluded(AdminConnection connection, string principalName, params int[] excludedClasses)
		{
			var classParameters = new string[excludedClasses.Length];
			for (var i = 0; i < excludedClasses.Length; i++)
			{
				classParameters[i] = $"@class{i}";
			}

			using (var command = connection.Command(
				$@"
SELECT
	pm.permission_name
FROM sys.database_principals AS grantee
	JOIN sys.database_permissions AS pm ON pm.grantee_principal_id = grantee.principal_id
 WHERE grantee.name = @principalName AND pm.class NOT IN ({string.Join(", ", classParameters)})
"))
			{
				command.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName);
				for (var i = 0; i < excludedClasses.Length; i++)
				{
					command.AddParameter($"@class{i}", SqlDbType.Int, excludedClasses[i]);
				}

				var permissionsActual = new List<string>();
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						permissionsActual.Add((string)reader["permission_name"]);
					}
				}

				Assert.That(permissionsActual, Is.Empty, $"Database principal '{principalName}' should have no permissions on classes other than {string.Join(", ", excludedClasses.Select(t => $"{t}"))}.");
			}
		}

		public static void AssertPrincipalHasNoPermissionsOnObjectsOfTypesOtherThanExcluded(AdminConnection connection, string principalName, params string[] excludedObjectTypes)
		{
			var objectTypeParameters = new string[excludedObjectTypes.Length];
			for (var i = 0; i < excludedObjectTypes.Length; i++)
			{
				objectTypeParameters[i] = $"@objectType{i}";
			}

			using (var command = connection.Command(
				$@"
SELECT
	state_desc = pm.state_desc
	, permission_name = pm.permission_name
	, objectName = obj.name
FROM sys.database_principals AS grantee
	JOIN sys.database_permissions AS pm ON 1=1
		AND pm.grantee_principal_id = grantee.principal_id
		AND pm.class = 1
	JOIN sys.all_objects AS obj ON pm.major_id = obj.object_id
 WHERE 1=1
	AND grantee.name = @principalName
	AND obj.type NOT IN ({string.Join(", ", objectTypeParameters)})
"))
			{
				command.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName);
				for (var i = 0; i < excludedObjectTypes.Length; i++)
				{
					command.AddParameter($"@objectType{i}", SqlDbType.NVarChar, 128, excludedObjectTypes[i]);
				}

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						Assert.Fail($"Database principal '{principalName}' should have no '{reader["state_desc"]} {reader["permission_name"]}' permission on object '{reader["objectName"]}'.");
					}
				}
			}
		}

		public static void AssertPrincipalHasNoPermissionsOnSchema(AdminConnection connection, string principalName, string schemaName)
		{
			using (var command = connection.Command(
				@"
SELECT
	pm.permission_name
FROM sys.database_principals AS grantee
	JOIN sys.database_permissions AS pm ON 1=1
		AND pm.grantee_principal_id = grantee.principal_id
	JOIN sys.schemas AS sch ON 1=1
		AND pm.major_id = sch.schema_id
		AND pm.class = 3
 WHERE grantee.name = @principalName AND sch.name = @schemaName
"))
			{
				command.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName);
				command.AddParameter("@schemaName", SqlDbType.NVarChar, 128, schemaName);

				var permissionsActual = new List<string>();
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						permissionsActual.Add((string)reader["permission_name"]);
					}
				}

				Assert.That(permissionsActual, Is.Empty, $"Database principal '{principalName}' should have no permissions on schema '{schemaName}'.");
			}
		}

		public static void AssertPrincipalHasNoRoles(AdminConnection connection, string principalName)
		{
			using (var command = connection.Command(
	@"
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
					Assert.That(reader.Read(), Is.False, $"Database principal '{principalName}' should have no roles.");
				}
			}
		}

		public static void AssertPrincipalHasRoles(AdminConnection connection, string principalName, params string[] roles)
		{
			using (var command = connection.Command(
				@"
SELECT
	roleName = r.name
FROM sys.database_principals AS p
	JOIN sys.database_role_members m ON m.member_principal_id = p.principal_id
	JOIN sys.database_principals AS r ON m.role_principal_id = r.principal_id
 WHERE p.name = @principalName
"))
			{
				command.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName);
				var rolesActual = new List<string>();
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						rolesActual.Add((string)reader["roleName"]);
					}
				}

				Assert.That(rolesActual, Is.EquivalentTo(roles).IgnoreCase, $"Database principal '{principalName}' should have roles {string.Join(", ", roles.Select(r => $"'{r}'"))} and no other.");
			}
		}

		public static void AssertPrincipalsMissing(AdminConnection connection, params string[] principalNames)
		{
			Assert.Multiple(() =>
			{
				foreach (var principalName in principalNames)
				{
					AssertPrincipalMissing(connection, principalName);
				}
			});
		}

		public static void AssumePrincipalsMissing(AdminConnection connection, params string[] principalNames)
		{
			foreach (var principalName in principalNames)
			{
				AssumePrincipalMissing(connection, principalName);
			}
		}

		public static void AssertSqlUsersExist(AdminConnection connection, params string[] sqlUsersNames)
		{
			Assert.Multiple(() =>
			{
				foreach (var userName in sqlUsersNames)
				{
					AssertPrincipalExists(connection, userName, "S");
				}
			});
		}

		public static void AssertRolesExist(AdminConnection connection, params string[] databaseRoleNames)
		{
			Assert.Multiple(() =>
			{
				foreach (var roleName in databaseRoleNames)
				{
					AssertPrincipalExists(connection, roleName, "R");
				}
			});
		}

		public static void AssertPrincipalMissingCaseSensitive(AdminConnection connection, string principalName, string type)
		{
			using (var command = connection.Command($"SELECT type FROM sys.database_principals WHERE name = @principalName COLLATE {Db.DatabaseCaseSensitiveCollation}"))
			{
				command.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName);

				Assert.That(command.ExecuteScalar(), Is.Null, $"Database principal '{principalName}' of type '{type}' should not be present (case sensitive).");
			}
		}

		public static void AssertPrincipalExistsCaseSensitive(AdminConnection connection, string principalName, string type)
		{
			using (var command = connection.Command($"SELECT type FROM sys.database_principals WHERE name = @principalName COLLATE {Db.DatabaseCaseSensitiveCollation}"))
			{
				command.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName);

				Assert.That((string)command.ExecuteScalar(), Is.EqualTo(type).IgnoreCase, $"Database principal '{principalName}' of type '{type}' should be present (case sensitive).");
			}
		}

		public static void AssertPrincipalExists(AdminConnection connection, string principalName, string type)
		{
			using (var command = connection.Command("SELECT type FROM sys.database_principals WHERE name = @principalName"))
			{
				command.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName);
				Assert.That((string)command.ExecuteScalar(), Is.EqualTo(type), $"Database principal '{principalName}' of type '{type}' should exist");
			}
		}

		public static void AssumePrincipalExists(AdminConnection connection, string principalName, string type)
		{
			using (var command = connection.Command("SELECT type FROM sys.database_principals WHERE name = @principalName"))
			{
				command.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName);
				Assume.That((string)command.ExecuteScalar(), Is.EqualTo(type), $"Database principal '{principalName}' of type '{type}' should exist.");
			}
		}

		public static void AssumePrincipalMissing(AdminConnection connection, string principalName)
		{
			using (var command = connection.Command("SELECT type FROM sys.database_principals WHERE name = @principalName"))
			{
				command.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName);
				Assume.That(command.ExecuteScalar(), Is.Null, $"There should be no database principal with name '{principalName}'.");
			}
		}

		public static void AssertPrincipalMissing(AdminConnection connection, string principalName)
		{
			using (var command = connection.Command("SELECT type FROM sys.database_principals WHERE name = @principalName"))
			{
				command.AddParameter("@principalName", SqlDbType.NVarChar, 128, principalName);
				Assert.That(command.ExecuteScalar(), Is.Null, $"There should be no database principal with name '{principalName}'.");
			}
		}

		public static void AssumeSchemaExists(AdminConnection connection, string schemaName)
		{
			Assume.That(Helper.SchemaExists(connection, schemaName), Is.True, $"There should be a schema '{schemaName}' in the database.");
		}

		public static void AssumeTypeExists(AdminConnection connection, string schemaName, string typeName)
		{
			Assume.That(Helper.TypeExists(connection, schemaName, typeName), Is.True, $"There should be a type '{typeName}' inside schema '{schemaName}' in the database.");
		}

		public static void AssumeFunctionExists(AdminConnection connection, string schemaName, string functionType, string functionName)
		{
			Assume.That(Helper.FunctionExists(connection, schemaName, functionType, functionName), Is.True, $"There should be a function '{functionName}' inside schema '{schemaName}' in the database.");
		}
	}
}
