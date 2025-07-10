using System.Data;
using Enterprise.DbUpgrader.Data.BaseData.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data.BaseData.StmData
{
	public class StmDataUpgradeTask : SystemInstallDataUpgradeTask
	{
		public StmDataUpgradeTask()
			: base(new StmDataDataFile())
		{
		}

		/// <summary>
		/// Existing records are not updated
		/// </summary>
		protected override void UpdateColumn(string columnName, DataRow targetRow, DataRow sourceRow)
		{
			// Skips update
		}

		/// <summary>
		/// Relies on:
		/// AccGLHeader
		/// </summary>
		protected override void DoInsert(DataRow sourceRow, DataTable targetTable, ref int targetIndex)
		{
			if (!SameNkExists(StmDataSchema.SD_Name, sourceRow[StmDataSchema.SD_Name.Name].ToString()) &&
				FkIsNullOrReferencedPkExists(AccGLHeaderSchema.PK, sourceRow[StmDataSchema.SD_GuidValue.Name]))
			{
				base.DoInsert(sourceRow, targetTable, ref targetIndex);
			}
		}
	}
}
