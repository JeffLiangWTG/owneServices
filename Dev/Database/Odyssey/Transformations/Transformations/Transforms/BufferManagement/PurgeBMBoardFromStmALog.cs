using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.BufferManagement
{
	public class PurgeBMBoardFromStmALog : PurgeGhostRecordsFromStmALog
	{
		public override string UserDescription => "Purge all records related to BMBoard from StmALog";

		protected override string ParentTableName => "BMBoard";

		protected override string ParentTablePKColumnName => "MB_PK";
	}
}
