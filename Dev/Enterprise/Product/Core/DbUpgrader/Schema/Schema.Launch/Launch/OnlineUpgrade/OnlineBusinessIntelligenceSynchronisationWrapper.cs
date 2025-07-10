namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade
{
	using CargoWise.Data;
	using Enterprise.DbUpgrader.Schema.Template;
	using Enterprise.DbUpgrader.Shared;
	using Enterprise.DbUpgrader.Transformations;

	class OnlineAuditDatabaseSynchronisationWrapper : OnlineSchemaSynchronisationWrapper
	{
		public OnlineAuditDatabaseSynchronisationWrapper(IUpgradeManager manager, string dbToUpgrade, DbConnection upgConnection)
			: base(manager, dbToUpgrade, upgConnection)
		{
		}

		protected override void RunOnlineTableChangesAndTransformations()
		{
			RunOneOffOnlineTransformations();
		}

		protected override IAuxiliaryDbCreator GetTemplateDbCreator()
		{
			return new AuditDbTemplate(Manager, TemplateDb, UpgConnection.ServerName);
		}

		void RunOneOffOnlineTransformations()
		{
			Manager.StartTask("Running one-off transformations in Audit Database");

			new TransformationDirector(Manager, auditConnection: UpgConnection, edwConnection: null).OnlineBiPreUpgradeRun();
		}
	}

	class OnlineEdwDatabaseSynchronisationWrapper : OnlineSchemaSynchronisationWrapper
	{
		public OnlineEdwDatabaseSynchronisationWrapper(IUpgradeManager manager, string dbToUpgrade, DbConnection upgConnection)
			: base(manager, dbToUpgrade, upgConnection)
		{
		}

		protected override void RunOnlineTableChangesAndTransformations()
		{
			RunOneOffOnlineTransformations();
		}

		protected override IAuxiliaryDbCreator GetTemplateDbCreator()
		{
			return new BiEdwDbTemplate(Manager, TemplateDb, UpgConnection.ServerName);
		}

		void RunOneOffOnlineTransformations()
		{
			Manager.StartTask("Running one-off transformations in Edw Database");

			new TransformationDirector(Manager, auditConnection: null, edwConnection: UpgConnection).OnlineBiPreUpgradeRun();
		}
	}
}
