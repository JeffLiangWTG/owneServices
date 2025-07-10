using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.DataProtection;

namespace Enterprise.SqlSecurity.Test
{
	static class ApplicationUsersAndRoles
	{
		internal static IEnumerable<string> PrincipalsAndMemberships(string mainDatabaseName)
		{
			return new[]
			{
				$"(N'{DbRoleTypes.CwReaderRole}', 'R', N'db_datareader', NULL)",
				$"(N'{DbRoleTypes.CwRestrictedReaderRole}', 'R', N'', NULL)",
				$"(N'{DbRoleTypes.CwRestrictedWriterRole}', 'R', N'', NULL)",
				$"(N'{DbRoleTypes.CwUnrestrictedWriterRole}', 'R', N'', NULL)",
				$"(N'{DbRoleTypes.CwHRMStaffRole}', 'R', N'', NULL)",
				$"(N'{CargoWiseReaderLoginCredentials.UserNameFor(mainDatabaseName)}', 'S', N'{DbRoleTypes.CwReaderRole}', NULL)",
				$"(N'{CargoWiseReaderLoginCredentials.UserNameFor(mainDatabaseName)}', 'S', N'db_datareader', NULL)", // unnecessary
				$"(N'{CargoWiseWriterLoginCredentials.UserNameFor(mainDatabaseName)}', 'S', N'{DbRoleTypes.CwRestrictedWriterRole}', NULL)",
				$"(N'{RestrictedReaderLoginCredentials.UserNameFor(mainDatabaseName)}', 'S', N'{DbRoleTypes.CwRestrictedReaderRole}', NULL)",
				$"(N'{RestrictedWriterLoginCredentials.UserNameFor(mainDatabaseName)}', 'S', N'{DbRoleTypes.CwRestrictedWriterRole}', NULL)",
				$"(N'{UnrestrictedWriterLoginCredentials.UserNameFor(mainDatabaseName)}', 'S', N'{DbRoleTypes.CwUnrestrictedWriterRole}', NULL)",
			};
		}

		internal static class Permissions
		{
			internal static IEnumerable<string> ByDatabaseType(DatabaseType dbType, string mainDatabaseName)
			{
				var databaseName = Helper.DatabaseNameFromDatabaseType(dbType);

				var allDatabasesPermissions = new[]
				{
					$"('G', N'CONNECT', N'DATABASE', N'', N'{databaseName}', N'', N'{CargoWiseReaderLoginCredentials.UserNameFor(mainDatabaseName)}', N'')",
					$"('G', N'CONNECT', N'DATABASE', N'', N'{databaseName}', N'', N'{CargoWiseWriterLoginCredentials.UserNameFor(mainDatabaseName)}', N'')",
					$"('G', N'CONNECT', N'DATABASE', N'', N'{databaseName}', N'', N'{RestrictedReaderLoginCredentials.UserNameFor(mainDatabaseName)}', N'')",
					$"('G', N'CONNECT', N'DATABASE', N'', N'{databaseName}', N'', N'{RestrictedWriterLoginCredentials.UserNameFor(mainDatabaseName)}', N'')",
					$"('G', N'CONNECT', N'DATABASE', N'', N'{databaseName}', N'', N'{UnrestrictedWriterLoginCredentials.UserNameFor(mainDatabaseName)}', N'')",

					$"('G', N'EXECUTE', N'DATABASE', N'', N'{databaseName}', N'', N'{CargoWiseReaderLoginCredentials.UserNameFor(mainDatabaseName)}', N'')",
					$"('G', N'EXECUTE', N'DATABASE', N'', N'{databaseName}', N'', N'{CargoWiseWriterLoginCredentials.UserNameFor(mainDatabaseName)}', N'')",
					$"('G', N'EXECUTE', N'DATABASE', N'', N'{databaseName}', N'', N'{RestrictedReaderLoginCredentials.UserNameFor(mainDatabaseName)}', N'')",
					$"('G', N'EXECUTE', N'DATABASE', N'', N'{databaseName}', N'', N'{RestrictedWriterLoginCredentials.UserNameFor(mainDatabaseName)}', N'')",
					$"('G', N'EXECUTE', N'DATABASE', N'', N'{databaseName}', N'', N'{UnrestrictedWriterLoginCredentials.UserNameFor(mainDatabaseName)}', N'')",

					$"('G', N'SHOWPLAN', N'DATABASE', N'', N'{databaseName}', N'', N'{DbRoleTypes.CwRestrictedReaderRole}', N'')",
					$"('G', N'VIEW DEFINITION', N'DATABASE', N'', N'{databaseName}', N'', N'{DbRoleTypes.CwRestrictedReaderRole}', N'')",
					$"('G', N'SELECT', N'OBJECT', N'sys', N'sql_expression_dependencies', N'', N'{DbRoleTypes.CwRestrictedReaderRole}', N'')",

					$"('G', N'SHOWPLAN', N'DATABASE', N'', N'{databaseName}', N'', N'{DbRoleTypes.CwReaderRole}', N'')",
					$"('G', N'VIEW DEFINITION', N'DATABASE', N'', N'{databaseName}', N'', N'{DbRoleTypes.CwReaderRole}', N'')",

					$"('G', N'IMPERSONATE', N'USER', N'', N'{RestrictedReaderLoginCredentials.UserNameFor(mainDatabaseName)}', N'', N'{DbRoleTypes.CwRestrictedWriterRole}', N'')",
					$"('G', N'IMPERSONATE', N'USER', N'', N'{CargoWiseReaderLoginCredentials.UserNameFor(mainDatabaseName)}', N'', N'{DbRoleTypes.CwRestrictedWriterRole}', N'')",

					$"('G', N'ALTER', N'DATABASE', N'', N'{databaseName}', N'', N'{DbRoleTypes.CwRestrictedWriterRole}', N'')",
					$"('G', N'CREATE SCHEMA', N'DATABASE', N'', N'{databaseName}', N'', N'{DbRoleTypes.CwRestrictedWriterRole}', N'')",
					$"('G', N'REFERENCES', N'DATABASE', N'', N'{databaseName}', N'', N'{DbRoleTypes.CwRestrictedWriterRole}', N'')",
					$"('G', N'SHOWPLAN', N'DATABASE', N'', N'{databaseName}', N'', N'{DbRoleTypes.CwRestrictedWriterRole}', N'')",
					$"('G', N'VIEW DATABASE STATE', N'DATABASE', N'', N'{databaseName}', N'', N'{DbRoleTypes.CwRestrictedWriterRole}', N'')",
					$"('G', N'VIEW DEFINITION', N'DATABASE', N'', N'{databaseName}', N'', N'{DbRoleTypes.CwRestrictedWriterRole}', N'')",

					$"('G', N'SELECT', N'OBJECT', N'sys', N'sql_expression_dependencies', N'', N'{DbRoleTypes.CwRestrictedWriterRole}', N'')",

					$"('G', N'DELETE', N'DATABASE', N'', N'{databaseName}', N'', N'{DbRoleTypes.CwUnrestrictedWriterRole}', N'')",
					$"('G', N'EXECUTE', N'DATABASE', N'', N'{databaseName}', N'', N'{DbRoleTypes.CwUnrestrictedWriterRole}', N'')",
					$"('G', N'INSERT', N'DATABASE', N'', N'{databaseName}', N'', N'{DbRoleTypes.CwUnrestrictedWriterRole}', N'')",
					$"('G', N'SELECT', N'DATABASE', N'', N'{databaseName}', N'', N'{DbRoleTypes.CwUnrestrictedWriterRole}', N'')",
					$"('G', N'UPDATE', N'DATABASE', N'', N'{databaseName}', N'', N'{DbRoleTypes.CwUnrestrictedWriterRole}', N'')",

					$"('G', N'VIEW DATABASE STATE', N'DATABASE', N'', N'{databaseName}', N'', N'{DbRoleTypes.CwUnrestrictedWriterRole}', N'')",
					$"('G', N'VIEW DEFINITION', N'DATABASE', N'', N'{databaseName}', N'', N'{DbRoleTypes.CwUnrestrictedWriterRole}', N'')",

					$"('G', N'EXECUTE', N'SCHEMA', N'', N'dbo', N'', N'{DbRoleTypes.CwRestrictedReaderRole}', N'')",
					$"('G', N'SELECT', N'SCHEMA', N'','dbo', N'', N'{DbRoleTypes.CwRestrictedReaderRole}', N'')",

					$"('G', N'EXECUTE', N'SCHEMA', N'', N'OrderTracking', N'', N'{DbRoleTypes.CwRestrictedReaderRole}', N'')",
					$"('G', N'SELECT', N'SCHEMA', N'','OrderTracking', N'', N'{DbRoleTypes.CwRestrictedReaderRole}', N'')",

					$"('G', N'EXECUTE', N'SCHEMA', N'', N'dbo', N'', N'{DbRoleTypes.CwRestrictedWriterRole}', N'')",
					$"('G', N'SELECT', N'SCHEMA', N'', N'dbo', N'', N'{DbRoleTypes.CwRestrictedWriterRole}', N'')",
					$"('G', N'INSERT', N'SCHEMA', N'', N'dbo', N'', N'{DbRoleTypes.CwRestrictedWriterRole}', N'')",
					$"('G', N'DELETE', N'SCHEMA', N'', N'dbo', N'', N'{DbRoleTypes.CwRestrictedWriterRole}', N'')",
					$"('G', N'UPDATE', N'SCHEMA', N'', N'dbo', N'', N'{DbRoleTypes.CwRestrictedWriterRole}', N'')",
					$"('G', N'ALTER', N'SCHEMA', N'', N'dbo', N'', N'{DbRoleTypes.CwRestrictedWriterRole}', N'')",
					$"('G', N'CREATE SEQUENCE', N'SCHEMA', N'', N'dbo', N'', N'{DbRoleTypes.CwRestrictedWriterRole}', N'')",

					$"('G', N'EXECUTE', N'SCHEMA', N'', N'OrderTracking', N'', N'{DbRoleTypes.CwRestrictedWriterRole}', N'')",
					$"('G', N'SELECT', N'SCHEMA', N'', N'OrderTracking', N'', N'{DbRoleTypes.CwRestrictedWriterRole}', N'')",
					$"('G', N'INSERT', N'SCHEMA', N'', N'OrderTracking', N'', N'{DbRoleTypes.CwRestrictedWriterRole}', N'')",
					$"('G', N'DELETE', N'SCHEMA', N'', N'OrderTracking', N'', N'{DbRoleTypes.CwRestrictedWriterRole}', N'')",
					$"('G', N'UPDATE', N'SCHEMA', N'', N'OrderTracking', N'', N'{DbRoleTypes.CwRestrictedWriterRole}', N'')",
					$"('G', N'ALTER', N'SCHEMA', N'', N'OrderTracking', N'', N'{DbRoleTypes.CwRestrictedWriterRole}', N'')",
					$"('G', N'CREATE SEQUENCE', N'SCHEMA', N'', N'OrderTracking', N'', N'{DbRoleTypes.CwRestrictedWriterRole}', N'')",

					$"('G', N'VIEW CHANGE TRACKING', N'SCHEMA', N'', N'dbo', N'', N'{DbRoleTypes.CwUnrestrictedWriterRole}', N'')",
					$"('G', N'VIEW CHANGE TRACKING', N'SCHEMA', N'', N'{DbSecurity.SqlHrmSchema}', N'', N'{DbRoleTypes.CwUnrestrictedWriterRole}', N'')",
					$"('G', N'VIEW CHANGE TRACKING', N'SCHEMA', N'', N'OrderTracking', N'', N'{DbRoleTypes.CwUnrestrictedWriterRole}', N'')",
					$"('G', N'VIEW CHANGE TRACKING', N'SCHEMA', N'', N'{DbSecurity.SqlCdcSchema}', N'', N'{DbRoleTypes.CwUnrestrictedWriterRole}', N'')",
					$"('G', N'VIEW CHANGE TRACKING', N'SCHEMA', N'', N'{DbSecurity.SqlStagingSchema}', N'', N'{DbRoleTypes.CwUnrestrictedWriterRole}', N'')",
					$"('G', N'CREATE SEQUENCE', N'SCHEMA', N'', N'dbo', N'', N'{DbRoleTypes.CwUnrestrictedWriterRole}', N'')",
					$"('G', N'CREATE SEQUENCE', N'SCHEMA', N'', N'{DbSecurity.SqlHrmSchema}', N'', N'{DbRoleTypes.CwUnrestrictedWriterRole}', N'')",
					$"('G', N'CREATE SEQUENCE', N'SCHEMA', N'', N'OrderTracking', N'', N'{DbRoleTypes.CwUnrestrictedWriterRole}', N'')",
					$"('G', N'CREATE SEQUENCE', N'SCHEMA', N'', N'{DbSecurity.SqlCdcSchema}', N'', N'{DbRoleTypes.CwUnrestrictedWriterRole}', N'')",
					$"('G', N'CREATE SEQUENCE', N'SCHEMA', N'', N'{DbSecurity.SqlStagingSchema}', N'', N'{DbRoleTypes.CwUnrestrictedWriterRole}', N'')",

					$"('G', N'SELECT', N'SCHEMA', N'', N'{DbSecurity.SqlHrmSchema}', N'', N'{DbRoleTypes.CwHRMStaffRole}', N'')",
				};

				return allDatabasesPermissions;
			}
		}
	}
}
