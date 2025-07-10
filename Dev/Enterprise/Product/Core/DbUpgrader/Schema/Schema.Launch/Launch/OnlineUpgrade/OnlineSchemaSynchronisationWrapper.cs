namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade
{
	using CargoWise.Data;
	using Enterprise.DbUpgrader.Shared;

	abstract class OnlineSchemaSynchronisationWrapper : SchemaSynchronisationWrapper
	{
		public OnlineSchemaSynchronisationWrapper(IUpgradeManager manager, string dbToUpgrade, DbConnection upgConnection)
			: base(manager, dbToUpgrade, upgConnection)
		{
		}

		protected override sealed void RunSynchronisationActions()
		{
			RunOnlineTableChangesAndTransformations();
			CreateNewIndexesOnline();
		}

		protected abstract void RunOnlineTableChangesAndTransformations();

		/// <summary>
		/// Must be the last part of online upgrade to match newly created columns
		/// </summary>
		void CreateNewIndexesOnline()
		{
			var indexSynchroniser = new IndexScriptRunner(UpgConnection, DbBeingUpgraded, TemplateDb, Manager);
			indexSynchroniser.CreateNewOnLineIndexes();
			indexSynchroniser.CreateUniqueIndexesOnline();
		}
	}
}
