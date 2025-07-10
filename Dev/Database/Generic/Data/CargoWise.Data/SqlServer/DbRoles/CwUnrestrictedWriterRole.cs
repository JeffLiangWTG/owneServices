namespace CargoWise.Data
{
	#region SuppressResourceStringsCheckRegion

	public class CwUnrestrictedWriterRole : DbRole
	{
		public override string Name => DbRoleTypes.CwUnrestrictedWriterRole;

		public override string[] GetDbSchemas(DbConnection connection) => GetAllSchemas(connection);

		public override string[] DbDatabasePermissions => new[] { "SELECT", "INSERT", "UPDATE", "DELETE", "EXECUTE", "VIEW DATABASE STATE", "VIEW DEFINITION" };

		public override string[] DbSchemaPermissions => new[] { "VIEW CHANGE TRACKING", "CREATE SEQUENCE" };
	}

	#endregion
}
