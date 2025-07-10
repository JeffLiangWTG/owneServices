using CargoWise.Data;

namespace Enterprise.DocumentScanning.Module.Testing
{
	class DocumentDbMergerModuleForTesting : DocumentDbMergerModule
	{
		protected override DbConnection.SqlServerEdition SqlServerEdition
		{
			get { return (sqlServerEditionOverride == null) ? base.SqlServerEdition : sqlServerEditionOverride.Value; }
		}

		public DbConnection.SqlServerEdition? sqlServerEditionOverride;

		public bool CheckIsAllowedToOpen_Exposed()
		{
			return base.CheckIsAllowedToShow();
		}
	}
}
