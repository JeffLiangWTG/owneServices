using System.Collections.Generic;
using System.Linq;

namespace CargoWise.Data
{
	#region SuppressResourceStringsCheckRegion

	public class CwRestrictedWriterRole : DbRole
	{
		public override string Name => DbRoleTypes.CwRestrictedWriterRole;

		public override string[] DbDatabasePermissions => new[] { "ALTER", "CREATE SCHEMA", "VIEW DATABASE STATE", "VIEW DEFINITION", "SHOWPLAN", "REFERENCES" };

		public override string[] GetDbSchemas(DbConnection connection) => GetAllSchemas(connection).Where(s => !lockedSchemas.Contains(s)).ToArray();

		readonly string[] lockedSchemas = { DbSecurity.SqlHrmSchema, DbSecurity.SqlCdcSchema, DbSecurity.SqlStagingSchema };

		public override string[] DbSchemaPermissions => new[] { "SELECT", "INSERT", "UPDATE", "DELETE", "EXECUTE", "ALTER", "CREATE SEQUENCE" };

		public override IList<(string Permission, string ObjectFullName)> DbObjectPermissions => new List<(string, string)> { ("SELECT", "sys.sql_expression_dependencies") };
	}

	#endregion
}
