namespace Enterprise.DbUpgrader.Schema
{
	using System;
	using CargoWise.Common;
	using CargoWise.Data;
	using Enterprise.DbUpgrader.Shared;

	public class DocManagerSchemaSynchronisationWrapper : SchemaSynchronisationWrapper
	{
		public DocManagerSchemaSynchronisationWrapper(IUpgradeManager manager, string docManagerDbToUpgrade, DbConnection upgConnection)
			: base(manager, docManagerDbToUpgrade, upgConnection)
		{
		}

		protected override void RunSynchronisationActions()
		{
			var isWritable = DocManagerUtils.IsDbWriteableForDocManager(DbBeingUpgraded);
			using (new DisposableAction(() => UpgConnection.AlterDbWriteableStateForDocManager(DbBeingUpgraded, isWritable)))
			{
				base.RunSynchronisationActions();
				CreateAndValidateCheckConstraints();
			}
		}

		protected override IAuxiliaryDbCreator GetTemplateDbCreator()
		{
			return new DocManagerTemplate(Manager, TemplateDb);
		}

		protected override string GetTemplateDbName()
		{
			var dbSuffix = "_SD";
			var dbSuffixIndex = DbBeingUpgraded.IndexOf(dbSuffix, StringComparison.OrdinalIgnoreCase);
			var newTemplateDbName = (dbSuffixIndex > -1)
				? DbBeingUpgraded.Substring(0, dbSuffixIndex + dbSuffix.Length)
				: DbBeingUpgraded + dbSuffix
				;

			return UpgUtils.GetTemplateDbName(newTemplateDbName);
		}
	}
}
