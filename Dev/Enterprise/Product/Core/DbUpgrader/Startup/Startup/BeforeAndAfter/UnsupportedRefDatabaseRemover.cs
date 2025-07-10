using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Database.Shared;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.ReferenceDatabases;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Startup
{
	class UnsupportedRefDatabaseRemover : RefDatabaseRemover
	{
		protected override string CleaningStartLog(int refDbCount)
		{
			return FormattableString.Invariant($"Removing unsupported reference databases ({refDbCount})");
		}

		protected override List<string> GetDatabasesToDrop(IUpgradeContext upgradeContext, AdminConnection mainConnection, IUpgradeTaskWorkflowLogger logger)
		{
			var dropList = new List<string>();

			var inUseDatabaseSuffixes = new ReferenceDbUpgradeDirector(upgradeContext, mainConnection, logger).ExclusiveRefDbSuffixList.ToList();

			var sql = FormattableString.Invariant($@"
SELECT
	IndividualRefDBName = d.name
FROM
	sys.databases            AS d
	LEFT JOIN @InUseDbSuffix AS s ON CONCAT(DB_NAME(), s.Value) = d.name COLLATE database_default
WHERE 1=1
	AND d.name LIKE CONCAT(DB_NAME(), N'[_]{RefDbTableNameResolver.RefDbAffix}[_]___[_]__')
	AND s.Value is NULL
ORDER BY
	IndividualRefDBName
"
				);

			mainConnection.ExecuteReader(sql
				, (cmd) =>
				{
					cmd.AddTableValuedParameter<string>("@InUseDbSuffix", TVPHelper.TVP_nvarchar, inUseDatabaseSuffixes);
				}
				, (reader) =>
				{
					dropList.Add((string)reader["IndividualRefDBName"]);
				});

			return dropList;
		}
	}
}
