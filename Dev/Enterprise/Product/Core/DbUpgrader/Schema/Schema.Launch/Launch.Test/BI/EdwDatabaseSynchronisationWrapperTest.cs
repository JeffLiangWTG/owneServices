using CargoWise.Bi.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared.Testing;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class EdwDatabaseSynchronisationWrapperTest : BusinessIntelligenceSynchronisationWrapperTest
	{
		protected override BusinessIntelligenceSynchronisationWrapper NewSynchronisationWrapper(AdminConnection testAdminCnx)
		{
			return new EdwDatabaseSynchronisationWrapperForTesting(TestDbName, testAdminCnx);
		}

		protected override void AssertHasSchemas(DbConnection testConnection, bool expected)
		{
			AssertSchemaExists(testConnection, BiConstants.BiAdminSchemaName, expected);
			AssertSchemaExists(testConnection, "Customs", expected);
			AssertSchemaExists(testConnection, "DomesticLogistics", expected);
			AssertSchemaExists(testConnection, "Finance", expected);
			AssertSchemaExists(testConnection, "Geography", expected);
			AssertSchemaExists(testConnection, "InternationalLogistics", expected);
			AssertSchemaExists(testConnection, "Organization", expected);
			AssertSchemaExists(testConnection, "Staging", expected);
			AssertSchemaExists(testConnection, "Transform", expected);
			AssertSchemaExists(testConnection, "Warehouse", expected);
		}

		protected override void AssertHasObjects(DbConnection testConnection, bool expected)
		{
			AssertTableExists(testConnection, BiConstants.BiAdminSchemaName, "MasterState", expected);
			AssertTableExists(testConnection, BiConstants.BiAdminSchemaName, "StagingTableState", expected);
			AssertTableExists(testConnection, BiConstants.BiAdminSchemaName, "TransformTableState", expected);
			AssertTableExists(testConnection, BiConstants.BiAdminSchemaName, "StagingTableConfiguration", expected);
			AssertTableExists(testConnection, BiConstants.BiAdminSchemaName, "TransformTableConfiguration", expected);
			AssertTableExists(testConnection, "Customs", "BAS__Declaration", expected);
			AssertTableExists(testConnection, "Finance", "BAS__JobHeader", expected);
			AssertTableExists(testConnection, "Organization", "BAS__Organization", expected);
			AssertTableExists(testConnection, "Staging", "JobShipment", expected);
			AssertTableExists(testConnection, "Staging", "RefCountry", expected);
		}

		protected override string TestDbName
		{
			get { return "Enterprise.DbUpgrader.Schema.EdwDatabaseSynchronisationWrapperTest.Db"; }
		}

		protected override string ActualDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		class EdwDatabaseSynchronisationWrapperForTesting : EdwDatabaseSynchronisationWrapper
		{
			public EdwDatabaseSynchronisationWrapperForTesting(string testDbToUpgrade, DbConnection upgConnection)
				: base(new DummyUpgradeManager(), testDbToUpgrade, upgConnection)
			{
			}
		}
	}
}
