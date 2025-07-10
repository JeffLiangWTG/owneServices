using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.BufferManagement
{
	public class PurgeBMNCNShapeFromStmALog : PurgeGhostRecordsFromStmALog
	{
		public override string UserDescription => "Purge all records related to BMNCNShape from StmALog";

		protected override string ParentTableName => "BMNCNShape";

		protected override string ParentTablePKColumnName => "BNS_PK";
	}
}
