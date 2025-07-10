using System.Data;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class UpgradeTaskForCharCaseComparisonTest : EmbeddedUpgradeTask
	{
		public UpgradeTaskForCharCaseComparisonTest()
			: base(new DataFileForCharCaseComparisonTest())
		{
		}

		protected override string ComparingColumnsNames(DataTable upgradeTable)
		{
			return "SO_Name";
		}

		protected override void UpdateColumn(string columnName, DataRow targetRow, DataRow sourceRow)
		{
			if (columnName == "SO_DataContext" && sourceRow["SO_Name"].ToString() == "TestTemplate1")
			{
				targetRow[columnName] = NewDataContextValue;
			}
			else
			{
				base.UpdateColumn(columnName, targetRow, sourceRow);
			}
		}

		public const string NewDataContextValue = "#NewDataContext#";
	}
}
