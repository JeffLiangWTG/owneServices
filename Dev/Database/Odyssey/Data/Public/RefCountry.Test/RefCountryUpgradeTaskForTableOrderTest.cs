using System.Data;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class RefCountryUpgradeTaskForTableOrderTest : RefCountryUpgradeTask
	{
		public RefCountryUpgradeTaskForTableOrderTest() : base()
		{
			TablesInUpgradingOrder = new string[ResourceFile.TableNames.Length];
		}

		public string[] TablesInUpgradingOrder;
		int TableIndex;

		protected override string ComparingColumnsNames(DataTable upgradeTable)
		{
			TablesInUpgradingOrder[TableIndex] = upgradeTable.TableName;
			TableIndex++;

			return base.ComparingColumnsNames(upgradeTable);
		}

		protected override void DoDelete(DataRow targetRow, ref int targetIndex)
		{
			targetIndex++;
		}

		protected override void DoInsert(DataRow sourceRow, DataTable targetTable, ref int targetIndex)
		{
		}

		protected override void UpdateColumn(string columnName, DataRow targetRow, DataRow sourceRow)
		{
		}
	}
}
