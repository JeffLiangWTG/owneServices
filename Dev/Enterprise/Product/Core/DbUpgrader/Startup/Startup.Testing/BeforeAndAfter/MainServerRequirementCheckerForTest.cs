using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	sealed class MainServerRequirementCheckerForTest : MainServerRequirementChecker
	{
		public MainServerRequirementCheckerForTest()
			: base(new List<string>(new string[] { Db.DatabaseName }), new string[] { Db.DatabaseName })
		{
		}

		public void CheckSqlServerGeneration_Exposed(DbConnection connection)
		{
			CheckSqlServerGeneration(connection);
		}

		public void CheckSqlServerEdition_Exposed(DbConnection connection)
		{
			CheckSqlServerEdition(connection);
		}

		public void CheckServicePackVersions_Exposed(DbConnection connection, IUpgradeTaskWorkflowLogger logger)
		{
			CheckServicePackVersions(connection, logger);
		}

		public void FixDbCollations_Exposed(DbConnection connection)
		{
			FixDbCollations(connection);
		}

		public void ValidateDbName_Exposed(string dbName)
		{
			ValidateDbName(dbName);
		}

		protected override bool IsDiskSpaceLow(int freeSpaceMb)
		{
			return (freeSpaceMb < int.MaxValue);
		}

		internal override DbConnection.SqlServerEdition GetSqlServerEdition(DbConnection connection)
		{
			return (ServerEditionTestOverride == null) ?
				base.GetSqlServerEdition(connection) : ServerEditionTestOverride.Value;
		}
		public DbConnection.SqlServerEdition? ServerEditionTestOverride;
	}
}
