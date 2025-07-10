using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class SharedDatabasePreparationStrategyWithMockMainDbForTesting : SharedDatabasePreparationStrategy
	{
		public SharedDatabasePreparationStrategyWithMockMainDbForTesting(IUpgradeContext context, DbConnection upgradeConnection, int versionToUpgradTo)
			: base(context, TestMockDb, RefDbTypeEnum.Customs, "US", upgradeConnection, versionToUpgradTo)
		{
		}

		public const string TestMockDb = "MockDbSharedDatabasePreparationStrategyTest";
	}
}
