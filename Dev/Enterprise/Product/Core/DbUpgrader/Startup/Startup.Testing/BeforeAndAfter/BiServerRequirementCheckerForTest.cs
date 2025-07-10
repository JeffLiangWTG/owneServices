using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	sealed class BiServerRequirementCheckerForTest : BiServerRequirementChecker
	{
		public BiServerRequirementCheckerForTest()
			: base(new List<string>(new string[] { Db.AuditDatabaseName, Db.EdwDatabaseName }))
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

		internal override DbConnection.SqlServerEdition GetSqlServerEdition(DbConnection connection)
		{
			return (ServerEditionTestOverride == null) ?
				base.GetSqlServerEdition(connection) : ServerEditionTestOverride.Value;
		}
		public DbConnection.SqlServerEdition? ServerEditionTestOverride;
	}
}
