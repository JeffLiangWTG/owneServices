using CargoWise.Data;

namespace Enterprise.DbHealth.Check
{
	class DbConsistencySecondaryRegistryWorker : DbConsistencyRegistryWorker
	{
		public DbConsistencySecondaryRegistryWorker(DbConnection connection, string databaseName)
			: base(connection, databaseName)
		{
		}

		protected override int LastCheckStep
		{
			get => 0;
			set { }
		}

		protected override string LastCheckStepDatabase
		{
			get => string.Empty;
			set { }
		}

		protected override string LastCheckStepTableOrView
		{
			get => string.Empty;
			set { }
		}

		protected override string LastCheckStepElapsedTime
		{
			get => string.Empty;
			set { }
		}
	}
}
