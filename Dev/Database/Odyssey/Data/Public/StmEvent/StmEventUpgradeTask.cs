using System.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data
{
	public class StmEventUpgradeTask : EmbeddedUpgradeTask
	{
		public StmEventUpgradeTask()
			: base(new StmEventDataFile())
		{
		}

		public StmEventUpgradeTask(EmbeddedDataFile resourceDataFile) : base(resourceDataFile)
		{
		}

		protected override void UpdateColumn(string columnName, DataRow targetRow, DataRow sourceRow)
		{
			if (columnName != StmEventSchema.Constants.SE_PropagateToParent && !(columnName == StmEventSchema.Constants.SE_Desc && (bool)sourceRow[StmEventSchema.Constants.SE_IsCustomizable]))
			{
				base.UpdateColumn(columnName, targetRow, sourceRow);
			}
		}
	}
}
