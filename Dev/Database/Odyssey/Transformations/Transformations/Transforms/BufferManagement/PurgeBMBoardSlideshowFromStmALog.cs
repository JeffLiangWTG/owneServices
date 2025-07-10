using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.BufferManagement
{
	public class PurgeBMBoardSlideshowFromStmALog : PurgeGhostRecordsFromStmALog
	{
		public override string UserDescription => "Purge all records related to BMBoardSlideshow from StmALog";

		protected override string ParentTableName => "BMBoardSlideshow";

		protected override string ParentTablePKColumnName => "MD_PK";
	}
}
