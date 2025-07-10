using CargoWise.Data;

namespace Enterprise.DbUpgrader.ReferenceDatabases
{
	sealed class RefDbPreparationStrategyForTesting : StandardRefDbPreparationStrategy
	{
		public RefDbPreparationStrategyForTesting(string mainDbName, RefDbTypeEnum refDbType, string refDbCountry, DbConnection upgradeConnection)
			: base(mainDbName, refDbType, refDbCountry, upgradeConnection)
		{
			testDbName = CalculatedPrivateRefDbName + "-TestMock";
		}

		protected override string GetLatestVersionDatabaseName()
		{
			return testDbName;
		}

		public void DoCreateDatabase_Exposed(AdminConnection adminConnection)
		{
			DoCreateDatabase(adminConnection);
		}

		readonly string testDbName;
	}
}
