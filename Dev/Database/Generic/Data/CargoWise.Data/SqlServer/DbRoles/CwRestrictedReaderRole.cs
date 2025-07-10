using System.Collections.Generic;
using System.Linq;

namespace CargoWise.Data
{
	#region SuppressResourceStringsCheckRegion

	public class CwRestrictedReaderRole : DbRole
	{
		public override string Name => DbRoleTypes.CwRestrictedReaderRole;

		public override string[] DbDatabasePermissions => new[] { "SHOWPLAN", "VIEW DEFINITION" };

		public override string[] GetDbSchemas(DbConnection connection) => GetAllSchemas(connection).Where(s => !lockedSchemas.Contains(s)).ToArray();

		readonly string[] lockedSchemas = { DbSecurity.SqlHrmSchema, DbSecurity.SqlCdcSchema, DbSecurity.SqlStagingSchema };

		public override string[] DbSchemaPermissions => new[] { "SELECT", "EXECUTE" };

		public override IList<(string Permission, string ObjectFullName)> DbObjectPermissions => new List<(string, string)> { ("SELECT", "sys.sql_expression_dependencies") };
	}

	#endregion
}
