using System.Data;

namespace Enterprise.DbUpgrader.Data
{
	public class AccAssetDepreciationBookUpgradeTask : EmbeddedUpgradeTask
	{
		public AccAssetDepreciationBookUpgradeTask() : base(new AccAssetDepreciationBookDataFile())
		{
		}

		protected override void UpdateColumn(string columnName, DataRow targetRow, DataRow sourceRow)
		{
			if (columnName == "ADB_IsActive" || columnName == "ADB_Description")
			{
				sourceRow[columnName] = targetRow[columnName];
			}
			base.UpdateColumn(columnName, targetRow, sourceRow);
		}
	}
}
