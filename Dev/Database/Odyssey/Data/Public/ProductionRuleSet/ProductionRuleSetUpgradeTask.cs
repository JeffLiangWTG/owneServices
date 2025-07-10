using System.Data;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data
{
	public class ProductionRuleSetUpgradeTask : EmbeddedUpgradeTask
	{
		public ProductionRuleSetUpgradeTask() : base(new ProductionRuleSetDataFile())
		{
		}

		#region DoInsert

		protected override void DoInsert(DataRow sourceRow, DataTable targetTable, ref int targetIndex)
		{
			if (sourceRow.Table.Columns.Contains(ProductionRuleSetSchema.Constants.PRS_IsLive) && sourceRow.Table.Columns.Contains(ProductionRuleSetSchema.Constants.PRS_Context))
			{
				var sql = string.Format("SELECT COUNT(*) FROM dbo.ProductionRuleSet WHERE PRS_WW_Warehouse IS NULL AND PRS_IsLive = 1 AND PRS_Context = '{0}' AND PRS_IsSystem = 0", sourceRow[ProductionRuleSetSchema.Constants.PRS_Context].ToString());
				var dBContainsNullWhsIsLiveRule = (int)Db.Connection.ExecuteScalar(sql) > 0;
				sourceRow[ProductionRuleSetSchema.Constants.PRS_IsLive] = !dBContainsNullWhsIsLiveRule;
			}
			base.DoInsert(sourceRow, targetTable, ref targetIndex);
		}

		#endregion

		#region UpdateColumn

		protected override void UpdateColumn(string columnName, DataRow targetRow, DataRow sourceRow)
		{
			if (columnName == ProductionRuleSetSchema.Constants.PRS_IsLive)
			{
				sourceRow[ProductionRuleSetSchema.Constants.PRS_IsLive] = targetRow[ProductionRuleSetSchema.Constants.PRS_IsLive];
			}
			base.UpdateColumn(columnName, targetRow, sourceRow);
		}

		#endregion
	}
}
