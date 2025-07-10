using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.BufferManagement
{
	public class PurgeBMComponentFromStmALog : PurgeGhostRecordsFromStmALog
	{
		public override string UserDescription => "Purge all records related to BMComponent from StmALog";

		protected override string ParentTableName => "BMComponent";

		protected override string ParentTablePKColumnName => "FC_PK";
	}
}
