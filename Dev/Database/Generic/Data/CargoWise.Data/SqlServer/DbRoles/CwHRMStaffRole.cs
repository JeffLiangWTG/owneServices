using System.Linq;

namespace CargoWise.Data
{
	#region SuppressResourceStringsCheckRegion

	public class CwHRMStaffRole : DbRole
	{
		public override string Name => DbRoleTypes.CwHRMStaffRole;

		public override string[] GetDbSchemas(DbConnection connection) => GetAllSchemas(connection).Any(schema => schema == DbSecurity.SqlHrmSchema) ? new[] { DbSecurity.SqlHrmSchema } : System.Array.Empty<string>();

		public override string[] DbSchemaPermissions => new[] { "SELECT" };
	}

	#endregion
}
