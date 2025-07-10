using CargoWise.Data;
using Enterprise.DbUpgrader.Shared.Testing;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class AuditDatabaseSynchronisationWrapperForTesting : AuditDatabaseSynchronisationWrapper
	{
		public AuditDatabaseSynchronisationWrapperForTesting(string testDbToUpgrade, DbConnection upgConnection)
			: base(new DummyUpgradeManager(), testDbToUpgrade, upgConnection)
		{
		}
	}
}
