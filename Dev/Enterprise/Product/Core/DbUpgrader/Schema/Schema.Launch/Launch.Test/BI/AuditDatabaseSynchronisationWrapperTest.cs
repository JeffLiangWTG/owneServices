using CargoWise.Bi.Common;
using CargoWise.Data;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class AuditDatabaseSynchronisationWrapperTest : BusinessIntelligenceSynchronisationWrapperTest
	{
		protected override BusinessIntelligenceSynchronisationWrapper NewSynchronisationWrapper(AdminConnection testAdminCnx)
		{
			return new AuditDatabaseSynchronisationWrapperForTesting(TestDbName, testAdminCnx);
		}

		protected override void AssertHasSchemas(DbConnection testConnection, bool expected)
		{
			AssertSchemaExists(testConnection, BiConstants.BiAdminSchemaName, expected);
		}

		protected override void AssertHasObjects(DbConnection testConnection, bool expected)
		{
			AssertTableExists(testConnection, BiConstants.BiAdminSchemaName, "MasterState", expected);
			AssertTableExists(testConnection, BiConstants.BiAdminSchemaName, "TableState", expected);
			AssertTableExists(testConnection, Db.SqlDbOwnerSchema, "CusContainer", expected);
			AssertTableExists(testConnection, Db.SqlDbOwnerSchema, "AccChargeCode", expected);
		}

		protected override string TestDbName
		{
			get { return "Enterprise.DbUpgrader.Schema.AuditDatabaseSynchronisationWrapperTest.Db"; }
		}

		protected override string ActualDbName
		{
			get { return Db.AuditDatabaseName; }
		}
	}
}
