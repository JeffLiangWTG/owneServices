namespace Enterprise.DbUpgrader.Schema
{
	using CargoWise.Data;
	using Enterprise.DbUpgrader.Schema.Template;
	using Enterprise.DbUpgrader.Shared;

	class EdwDatabaseSynchronisationWrapper : BusinessIntelligenceSynchronisationWrapper
	{
		public static EdwDatabaseSynchronisationWrapper New(IUpgradeManager manager, DbConnection upgConnection)
		{
			return new EdwDatabaseSynchronisationWrapper(manager, Db.EdwDatabaseName, upgConnection);
		}

		protected EdwDatabaseSynchronisationWrapper(IUpgradeManager manager, string dbToUpgrade, DbConnection upgConnection)
			: base(manager, dbToUpgrade, upgConnection)
		{
		}

		protected override IAuxiliaryDbCreator GetTemplateDbCreator()
		{
			return new BiEdwDbTemplate(Manager, TemplateDb, UpgConnection.ServerName);
		}

		protected override void RunSynchronisationActions()
		{
			base.RunSynchronisationActions();
			Manager.ShowInfoMessage("Populating EDW control tables");
			new EdwDatabasePopulator(UpgConnection).Run();
		}
	}
}
