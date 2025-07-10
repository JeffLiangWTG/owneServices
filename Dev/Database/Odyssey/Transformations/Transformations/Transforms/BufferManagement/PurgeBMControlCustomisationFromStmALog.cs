using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.BufferManagement
{
	public class PurgeBMControlCustomisationFromStmALog : PurgeGhostRecordsFromStmALog
	{
		public override string UserDescription => "Purge all records related to BMControlCustomisation from StmALog";

		protected override string ParentTableName => "BMControlCustomisation";

		protected override string ParentTablePKColumnName => "FM_PK";
	}
}
