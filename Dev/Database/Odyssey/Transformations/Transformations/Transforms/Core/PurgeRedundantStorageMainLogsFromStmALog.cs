using System.Collections.Generic;
using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core
{
	public class PurgeRedundantStorageMainLogsFromStmALog : PurgeGhostRecordsFromStmALog
	{
		public override string UserDescription => "Purge all records related to StorageMain from StmALog";
		protected override IList<string> EventCodes => new List<string> { "ADD", "EDT", "DEL" };
		protected override string ParentTableName => "StorageMain";

		protected override string ParentTablePKColumnName => "SM_PK";
	}
}
