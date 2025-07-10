using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;

namespace CargoWise.Data
{
	#region SuppressResourceStringsCheckRegion

	public abstract class DbRole
	{
		public abstract string Name { get; }

		public virtual string[] DbDatabasePermissions => Array.Empty<string>();

		public virtual string[] GetDbSchemas(DbConnection connection) => Array.Empty<string>();

		public virtual string[] DbSchemaPermissions => Array.Empty<string>();

		public virtual IList<(string Permission, string ObjectFullName)> DbObjectPermissions => new List<(string, string)>();

		public string CreateRoleSQL => $"CREATE ROLE {Name.QuoteName()};";

		public string GrantDatabasePermissionsSQL(string[] permissions) => permissions.Length > 0 ? $"GRANT {string.Join(", ", permissions)} TO {Name.QuoteName()};" : string.Empty;

		public string RevokeDatabasePermissionsSQL(string[] permissions) => permissions.Length > 0 ? $"REVOKE {string.Join(", ", permissions)} FROM {Name.QuoteName()};" : string.Empty;

		public string GrantSchemaPermissionSQL(string schemaName, string[] permissions) => $"GRANT {string.Join(", ", permissions)} ON SCHEMA::{schemaName.QuoteName()} TO {Name.QuoteName()};";

		public string RevokeSchemaPermissionSQL(string schemaName, string[] permissions) => $"REVOKE {string.Join(", ", permissions)} ON SCHEMA::{schemaName.QuoteName()} FROM {Name.QuoteName()};";

		public string GrantObjectPermissionSQL(string permission, string objectFullName) => $"GRANT {permission} ON {objectFullName} TO {Name.QuoteName()};";

		public string RevokeObjectPermissionSQL(string permission, string objectFullName) => $"REVOKE {permission} ON {objectFullName} FROM {Name.QuoteName()};";

		public string CreateDbRoleAndGrantPermissionsSQL(DbConnection connection)
		{
			var dbSchemas = GetDbSchemas(connection);

			var sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine(CreateRoleSQL);

			var grantDatabasePermissionsSQL = GrantDatabasePermissionsSQL(DbDatabasePermissions);
			if (!string.IsNullOrWhiteSpace(grantDatabasePermissionsSQL))
			{
				sqlStringBuilder.AppendLine(grantDatabasePermissionsSQL);
			}

			foreach (var dbSchema in dbSchemas)
			{
				var grantSchemaPermissionsSQL = GrantSchemaPermissionSQL(dbSchema, DbSchemaPermissions);
				if (!string.IsNullOrWhiteSpace(grantSchemaPermissionsSQL))
				{
					sqlStringBuilder.AppendLine(grantSchemaPermissionsSQL);
				}
			}

			foreach (var objectPermission in DbObjectPermissions)
			{
				sqlStringBuilder.AppendLine(GrantObjectPermissionSQL(objectPermission.Permission, objectPermission.ObjectFullName));
			}

			return sqlStringBuilder.ToString().Trim();
		}

		protected string[] GetAllSchemas(DbConnection connection)
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"
SELECT 
	name
FROM
	sys.schemas
WHERE
	name not in ('sys', 'guest', 'INFORMATION_SCHEMA') AND name not like 'db[_]%'
");

			var allSchemaNames = new List<string>();
			connection.ExecuteReader(sql, reader => allSchemaNames.Add(reader.GetString(0).Trim()));

			return allSchemaNames.ToArray();
		}

		public int? GetDbRoleId(DbConnection connection, string dbName)
		{
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot 
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			var sql = string.Format(CultureInfo.InvariantCulture,
				"SELECT principal_id FROM {0}.sys.database_principals WHERE name = @RoleName"   // SQL command
				, dbName.QuoteName()
				);

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@RoleName", SqlDbType.NVarChar, 128, Name);
				var dbReaderRoleObj = cmd.ExecuteScalar();
				return (dbReaderRoleObj == null) ? null : Convert.ToInt32(dbReaderRoleObj, CultureInfo.InvariantCulture);
			}
		}

		#region CreateRoleAndPermissionGrantCommandsIfRequired

		public void CreateRoleAndPermissionGrantCommandsIfRequired(DbConnection connection, string dbName, out StringBuilder cmdBuilder)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			cmdBuilder = new StringBuilder();

			int? dbRoleId = GetDbRoleId(connection, dbName);
			if (dbRoleId.HasValue)
			{
				AppendSchemaPermissions(connection, dbName, dbRoleId, cmdBuilder);
				AppendDatabasePermissions(connection, dbName, dbRoleId, cmdBuilder);
				AppendObjectPermissions(connection, dbName, dbRoleId, cmdBuilder);
			}
			else
			{
				cmdBuilder.AppendLine(CreateDbRoleAndGrantPermissionsSQL(connection));
			}
		}

		void AppendSchemaPermissions(DbConnection connection, string dbName, int? dbRoleId, StringBuilder cmdBuilder)
		{
			var dbRoleSchemas = GetDbSchemas(connection);
			var dbRoleSchemaPermissions = DbSchemaPermissions;
			var currentSchemaPermissions = GetSchemaPermissions(connection, dbName, dbRoleId.Value);

			var expectedSchemaPermissions = new List<(string SchemaName, string Permission)>();
			IEnumerableExtensions.ForEach(dbRoleSchemas, s => expectedSchemaPermissions.AddRange(dbRoleSchemaPermissions.Select(p => (s, p))));

			var permissionsToRevoke = currentSchemaPermissions.Where(csp => !expectedSchemaPermissions.Any(rsp => rsp.SchemaName == csp.SchemaName && rsp.Permission == csp.Permission)).ToList();
			var permissionsToGrant = expectedSchemaPermissions.Where(rsp => !currentSchemaPermissions.Any(csp => csp.SchemaName == rsp.SchemaName && csp.Permission == rsp.Permission)).ToList();

			IEnumerableExtensions.ForEach(permissionsToRevoke.GroupBy(p => p.SchemaName, p => p.Permission, (key, g) => (key, g.ToArray())), schemaPermission =>
			{
				cmdBuilder.AppendLine(RevokeSchemaPermissionSQL(schemaPermission.key, schemaPermission.Item2));
			});

			IEnumerableExtensions.ForEach(permissionsToGrant.GroupBy(p => p.SchemaName, p => p.Permission, (key, g) => (key, g.ToArray())), schemaPermission =>
			{
				cmdBuilder.AppendLine(GrantSchemaPermissionSQL(schemaPermission.key, schemaPermission.Item2));
			});
		}

		void AppendDatabasePermissions(DbConnection connection, string dbName, int? dbRoleId, StringBuilder cmdBuilder)
		{
			var dbRoleDatabasePermissions = DbDatabasePermissions;
			var currentDatabasePermissions = GetDatabasePermissions(connection, dbName, dbRoleId.Value);

			var permissionsToRevoke = currentDatabasePermissions.Where(cdp => !dbRoleDatabasePermissions.Any(dp => dp == cdp)).ToArray();
			var permissionsToGrant = dbRoleDatabasePermissions.Where(dp => !currentDatabasePermissions.Any(cdp => dp == cdp)).ToArray();

			cmdBuilder.AppendLine(RevokeDatabasePermissionsSQL(permissionsToRevoke));
			cmdBuilder.AppendLine(GrantDatabasePermissionsSQL(permissionsToGrant));
		}

		void AppendObjectPermissions(DbConnection connection, string dbName, int? dbRoleId, StringBuilder cmdBuilder)
		{
			var dbRoleObjectPermissions = DbObjectPermissions;
			var currentObjectPermissions = GetObjectPermissions(connection, dbName, dbRoleId.Value);

			var permissionsToRevoke = currentObjectPermissions.Where(cop => !dbRoleObjectPermissions.Any(op => op.Permission == cop.permission && op.ObjectFullName == cop.objectFullName)).ToArray();
			var permissionsToGrant = dbRoleObjectPermissions.Where(op => !currentObjectPermissions.Any(cop => cop.permission == op.Permission && cop.objectFullName == op.ObjectFullName)).ToArray();

			foreach (var permissionToRevoke in permissionsToRevoke)
			{
				cmdBuilder.AppendLine(RevokeObjectPermissionSQL(permissionToRevoke.permission, permissionToRevoke.objectFullName));
			}

			foreach (var permissionToGrant in permissionsToGrant)
			{
				cmdBuilder.AppendLine(GrantObjectPermissionSQL(permissionToGrant.Permission, permissionToGrant.ObjectFullName));
			}
		}

		public bool IsDatabasePermissionExpected(string permission) => DbDatabasePermissions.Contains(permission);

		public bool IsSchemaPermissionExpected(string permission, string schema, DbConnection connection) => DbSchemaPermissions.Contains(permission) && GetDbSchemas(connection).Contains(schema);

		IEnumerable<(string SchemaName, string Permission)> GetSchemaPermissions(DbConnection connection, string dbName, int dbRoleId)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			var sql = string.Format(CultureInfo.InvariantCulture, @"
SELECT
	SCHEMA_NAME(perm.major_id), perm.permission_name
FROM
	sys.database_permissions perm
INNER JOIN
	sys.database_principals p
ON
	perm.grantee_principal_id = p.principal_id
WHERE
	perm.state = 'G'
	AND p.type = 'R'
	AND perm.class_desc = 'SCHEMA'
	AND p.principal_id = @RoleId
"
	);
			var schemaPermissions = new List<(string SchemaName, string Permission)>();

			using (((ICurrentDbControl)connection).UseDatabase(dbName))
			{
				using (var cmd = connection.Command(sql))
				{
					cmd.AddParameter("@RoleId", SqlDbType.Int, dbRoleId);

					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							schemaPermissions.Add((reader.GetString(0), reader.GetString(1)));
						}
					}
				}
			}

			return schemaPermissions;
		}

		IEnumerable<string> GetDatabasePermissions(DbConnection connection, string dbName, int dbRoleId)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			var sql = string.Format(CultureInfo.InvariantCulture, @"
SELECT
	pe.permission_name
FROM
	sys.database_principals AS pr
INNER JOIN
	sys.database_permissions AS pe
ON
	pe.grantee_principal_id = pr.principal_id
WHERE
	pr.principal_id = @RoleId
	AND pr.type_desc = 'DATABASE_ROLE'
	AND pe.class_desc = 'DATABASE'
	AND pe.state = 'G'
"
	);
			var schemaPermissions = new List<string>();

			using (((ICurrentDbControl)connection).UseDatabase(dbName))
			{
				using (var cmd = connection.Command(sql))
				{
					cmd.AddParameter("@RoleId", SqlDbType.Int, dbRoleId);

					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							schemaPermissions.Add(reader.GetString(0));
						}
					}
				}
			}

			return schemaPermissions.ToArray();
		}

		IEnumerable<(string permission, string objectFullName)> GetObjectPermissions(DbConnection connection, string dbName, int dbRoleId)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			var sql = string.Format(CultureInfo.InvariantCulture, @"
SELECT
	pe.permission_name,
	OBJECT_SCHEMA_NAME(pe.major_id) + N'.' + OBJECT_NAME(pe.major_id)
FROM
	sys.database_principals AS pr
INNER JOIN
	sys.database_permissions AS pe
ON
	pe.grantee_principal_id = pr.principal_id
WHERE
	pr.principal_id = @RoleId
	AND pr.type_desc = 'DATABASE_ROLE'
	AND pe.class_desc = 'OBJECT_OR_COLUMN'
	AND pe.state = 'G' 
"
	);
			var objectPermissions = new List<(string permission, string objectFullName)>();
			using (((ICurrentDbControl)connection).UseDatabase(dbName))
			{
				using (var cmd = connection.Command(sql))
				{
					cmd.AddParameter("@RoleId", SqlDbType.Int, dbRoleId);

					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							objectPermissions.Add((reader.GetString(0), reader.GetString(1)));
						}
					}
				}
			}

			return objectPermissions;
		}

		#endregion

		public virtual void EnsureExists(AdminConnection connection, string dbName)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			using (((ICurrentDbControl)connection).UseDatabase(dbName))
			{
				if (!SqlSecurityUtils.DbRole.Exists(connection, Name))
				{
					SqlSecurityUtils.DbRole.Create(connection, Name);
				}
			}
		}
	}

	#endregion
}
