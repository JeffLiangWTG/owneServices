[assembly: WTG.StaticAnalysis.Annotation.UsesConstants(typeof(CargoWise.Bi.Common.BiConstants))]

namespace Enterprise.DbUpgrader.Schema
{
	using CargoWise.Bi.Common;
	using CargoWise.Data;
	using Enterprise.DbUpgrader.Shared;
	using Resource.Version;

	abstract class BusinessIntelligenceSynchronisationWrapper : SchemaSynchronisationWrapper
	{
		public BusinessIntelligenceSynchronisationWrapper(IUpgradeManager manager, string dbToUpgrade, DbConnection upgConnection)
			: base(manager, dbToUpgrade, upgConnection)
		{
		}

		protected override void RunSynchronisationActions()
		{
			base.RunSynchronisationActions();
			CreateAndValidateCheckConstraints();
			AddMainDbSchemaVersionExtPty();
		}

		void AddMainDbSchemaVersionExtPty()
		{
			Manager.ShowInfoMessage("Adding Main DB Schema version to BI database");
			var ptyValue = SchemaVersion.Application.ToString();
			DataUtils.SaveDbExtendedProperty(UpgConnection, BiConstants.MainDbSchemaVersionExtPtyName, ptyValue, DbBeingUpgraded);
		}
	}
}
