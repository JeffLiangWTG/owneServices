using System.Collections.Generic;

namespace CargoWise.Data.Testing
{
	sealed class DbRoleForTest : DbRole
	{
		public override string Name => "testDbRole";

		public override string[] DbDatabasePermissions => new[] { "DP1", "DP2" };

		public override string[] GetDbSchemas(DbConnection connection) => new[] { "schema1", "schema2", "schema3" };

		public override string[] DbSchemaPermissions => new[] { "SP1", "SP2" };

		public override IList<(string Permission, string ObjectFullName)> DbObjectPermissions => new List<(string, string)> { ("DP1", "O1"), ("DP2", "O2") };
	}
}
