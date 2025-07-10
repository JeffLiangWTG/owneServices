using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;

namespace Enterprise.SqlSecurity.Test
{
	static class StaffUser
	{
		internal static class Permissions
		{
			internal static IEnumerable<string> SpecificToRoleByDatabaseType(DatabaseType dbType, string roleName, string principalName)
			{
				switch (dbType)
				{
					case DatabaseType.UserRepository when roleName == DbRoleTypes.CwRestrictedReaderRole:
						return new[]
						{
							$"('G', N'EXECUTE', N'DATABASE', N'', N'{Helper.DatabaseNameFromDatabaseType(dbType)}', N'', N'{principalName}', N'')",
							$"('G', N'SELECT', N'DATABASE', N'', N'{Helper.DatabaseNameFromDatabaseType(dbType)}', N'', N'{principalName}', N'')",
						};
					case DatabaseType.UserRepository when roleName == DbRoleTypes.DbDataWriterRole:
						return new[]
						{
							$"('G', N'CREATE TYPE', N'DATABASE', N'', N'{Helper.DatabaseNameFromDatabaseType(dbType)}', N'', N'{principalName}', N'')",
							$"('G', N'CREATE SCHEMA', N'DATABASE', N'', N'{Helper.DatabaseNameFromDatabaseType(dbType)}', N'', N'{principalName}', N'')",
							$"('G', N'CREATE VIEW', N'DATABASE', N'', N'{Helper.DatabaseNameFromDatabaseType(dbType)}', N'', N'{principalName}', N'')",
							$"('G', N'CREATE FUNCTION', N'DATABASE', N'', N'{Helper.DatabaseNameFromDatabaseType(dbType)}', N'', N'{principalName}', N'')",
							$"('G', N'CREATE PROCEDURE', N'DATABASE', N'', N'{Helper.DatabaseNameFromDatabaseType(dbType)}', N'', N'{principalName}', N'')",
							$"('G', N'CREATE TABLE', N'DATABASE', N'', N'{Helper.DatabaseNameFromDatabaseType(dbType)}', N'', N'{principalName}', N'')",
							$"('G', N'INSERT', N'DATABASE', N'', N'{Helper.DatabaseNameFromDatabaseType(dbType)}', N'', N'{principalName}', N'')",
							$"('G', N'UPDATE', N'DATABASE', N'', N'{Helper.DatabaseNameFromDatabaseType(dbType)}', N'', N'{principalName}', N'')",
							$"('G', N'DELETE', N'DATABASE', N'', N'{Helper.DatabaseNameFromDatabaseType(dbType)}', N'', N'{principalName}', N'')",
							$"('G', N'REFERENCES', N'DATABASE', N'', N'{Helper.DatabaseNameFromDatabaseType(dbType)}', N'', N'{principalName}', N'')",
							$"('G', N'ALTER', N'SCHEMA', N'', N'dbo', N'', N'{principalName}', N'')",
						};
				}

				return Array.Empty<string>();
			}

			internal static IEnumerable<string> ByRolesAndDatabaseTypeOnDatabase(DatabaseType dbType, string[] roles)
			{
				switch (dbType)
				{
					case DatabaseType.UserRepository when
					roles.Contains(DbRoleTypes.CwRestrictedReaderRole, StringComparer.OrdinalIgnoreCase)
					&& roles.Contains(DbRoleTypes.DbDataWriterRole, StringComparer.OrdinalIgnoreCase):
						return new[]
						{
							"CONNECT",

							"CREATE TYPE",
							"CREATE SCHEMA",
							"CREATE VIEW",
							"CREATE FUNCTION",
							"CREATE PROCEDURE",
							"CREATE TABLE",
							"INSERT",
							"UPDATE",
							"DELETE",
							"REFERENCES",

							"SELECT",
							"EXECUTE",
						};
					case DatabaseType.UserRepository when roles.Contains(DbRoleTypes.CwRestrictedReaderRole, StringComparer.OrdinalIgnoreCase):
						return new[]
						{
							"CONNECT",

							"SELECT",
							"EXECUTE",
						};
					case DatabaseType.UserRepository when roles.Contains(DbRoleTypes.DbDataWriterRole, StringComparer.OrdinalIgnoreCase):
						return new[]
						{
							"CONNECT",

							"CREATE TYPE",
							"CREATE SCHEMA",
							"CREATE VIEW",
							"CREATE FUNCTION",
							"CREATE PROCEDURE",
							"CREATE TABLE",
							"INSERT",
							"UPDATE",
							"DELETE",
							"REFERENCES",
						};
					default:
						return new[]
						{
							"CONNECT",
						};
				}
			}

			internal static IEnumerable<string> ByRolesAndDatabaseTypeOnDboSchema(DatabaseType dbType, string[] roles)
			{
				var permissions = new List<string>();
				switch (dbType)
				{
					case DatabaseType.UserRepository when roles.Contains(DbRoleTypes.DbDataWriterRole):
						permissions.AddRange(new[]
						{
							"ALTER",
						});
						break;
				}

				return permissions;
			}
		}
	}
}
