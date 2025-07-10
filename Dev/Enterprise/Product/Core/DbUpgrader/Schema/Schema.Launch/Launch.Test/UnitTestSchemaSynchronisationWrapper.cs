namespace Enterprise.DbUpgrader.Schema.Testing
{
	using CargoWise.Data;
	using Enterprise.DbUpgrader.Shared;

	class UnitTestSchemaSynchronisationWrapper : SchemaSynchronisationWrapper
	{
		public UnitTestSchemaSynchronisationWrapper(IUpgradeManager manager, string testDbToUpgrade, DbConnection upgConnection)
			: base(manager, testDbToUpgrade, upgConnection)
		{
		}

		protected override void RunSynchronisationActions()
		{
			base.RunSynchronisationActions();
			CreateAndValidateCheckConstraints();
		}

		protected override IAuxiliaryDbCreator GetTemplateDbCreator()
		{
			return new MainTemplateDbCreatorForTesting(TemplateDb);
		}
	}
}
