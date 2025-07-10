using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformations;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade
{
	class OnlineMainDatabaseSchemaSynchronisationWrapper : OnlineSchemaSynchronisationWrapper
	{
		public OnlineMainDatabaseSchemaSynchronisationWrapper(IUpgradeManager manager, string dbToUpgrade, DbConnection upgConnection)
			: base(manager, dbToUpgrade, upgConnection)
		{
		}

		protected override void RunOnlineTableChangesAndTransformations()
		{
			RunPriorTransformations();
			RunTablePreSynchronisation();

			RunOneOffOnlineTransformations();

			RunOnLineTransformations();
		}

		void RunPriorTransformations()
		{
			Manager.StartTask("Running prior transformations");

			new AddUniqueIdentifierTVP(Manager).Run();
		}

		void RunTablePreSynchronisation()
		{
			Manager.StartTask("Running table pre-synchronisation");

			var synchroniser = new TablePreSynchroniser(Manager, DbBeingUpgraded, TemplateDb);
			synchroniser.AddAndPopulateAuditAndNaturalKeyColumns();
			synchroniser.ConvertCharFlagsToBit();
			synchroniser.ConvertCharToChar();
			synchroniser.ConvertDecimalToDecimal();
			synchroniser.ConvertDateTimesToDate();
		}

		void RunOneOffOnlineTransformations()
		{
			Manager.StartTask("Running one-off transformations");

			new TransformationDirector(Manager).OnlinePreUpgradeRun();
		}

		void RunOnLineTransformations()
		{
			Manager.StartTask("Running auto transformations");

			new NewColumnUpgrader(Manager, DbBeingUpgraded, TemplateDb).Run();
		}
	}
}
