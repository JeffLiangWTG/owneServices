using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade.Testing
{
	class UnitTestOnlineMainDatabaseSchemaSynchronisationWrapper : OnlineMainDatabaseSchemaSynchronisationWrapper
	{
		public UnitTestOnlineMainDatabaseSchemaSynchronisationWrapper(IUpgradeManager manager, string testDbToUpgrade)
			: base(manager, testDbToUpgrade, Db.Connection)
		{
		}

		protected override IAuxiliaryDbCreator GetTemplateDbCreator()
		{
			return new MainTemplateDbCreatorForTesting(TemplateDb);
		}
	}
}

