using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.BufferManagement
{
	public class PurgeBMSystemFromStmALog : PurgeGhostRecordsFromStmALog
	{
		public override string UserDescription => "Purge all records related to BMSystem from StmALog";

		protected override string ParentTableName => "BMSystem";

		protected override string ParentTablePKColumnName => "FS_PK";
	}
}
