using System.Collections.Generic;

namespace CargoWise.Data.Testing
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Used in DbSecurityLockDown tests")]
	sealed class DbSecurityForTest : DbSecurity
	{
		public IList<ServerRoleMembership> GetDisallowedServerRoleMembers_Exposed(AdminConnection connection, bool allowDbCreator = false)
		{
			return base.GetDisallowedServerRoleMembers(connection, allowDbCreator);
		}

		public string GetSqlAgentServiceAccountName_Exposed(DbConnection connection)
		{
			return base.GetSqlAgentServiceAccountName(connection);
		}

		protected override bool IsSharedDatabase(string dbName) => IsSharedDatabase_Override ?? base.IsSharedDatabase(dbName);

		public bool? IsSharedDatabase_Override { get; set; }

		public string AuditServer_Exposed(AdminConnection connection)
		{
			return base.LoadAuditServer(connection);
		}

		public string DataWarehouseServer_Exposed(AdminConnection connection)
		{
			return base.LoadDataWarehouseServer(connection);
		}
	}
}
