namespace CargoWise.Data.Testing
{
	sealed class DatabaseTestRole : DbRole
	{
		public override string Name => "DatabaseTestRole";

		public override string[] DbSchemaPermissions => new[] { "SELECT" };

		public override string[] GetDbSchemas(DbConnection connection) => new[] { "dbo" };
	}
}
