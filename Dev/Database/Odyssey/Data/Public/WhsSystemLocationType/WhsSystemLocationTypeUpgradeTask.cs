using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data
{
	public class WhsSystemLocationTypeUpgradeTask : EmbeddedUpgradeTask
	{
		public WhsSystemLocationTypeUpgradeTask() : base(new WhsSystemLocationTypeDataFile())
		{
		}

		#region DoInsert

		protected override void DoInsert(DataRow sourceRow, DataTable targetTable, ref int targetIndex)
		{
			var newCode = GetUniqueCode(sourceRow, targetTable);
			if (!string.IsNullOrEmpty(newCode))
			{
				if (sourceRow.Table.Columns.Contains(WhsLocationTypeSchema.Constants.WLT_Code))
				{
					sourceRow[WhsLocationTypeSchema.Constants.WLT_Code] = newCode;
				}
			}

			base.DoInsert(sourceRow, targetTable, ref targetIndex);
		}

		string GetUniqueCode(DataRow sourceRow, DataTable targetTable)
		{
			var newCode = (string)sourceRow[WhsLocationTypeSchema.Constants.WLT_Code];

			if (targetTable.Columns.Contains(WhsLocationTypeSchema.Constants.WLT_Code))
			{
				var existingCodes = new HashSet<string>();
				foreach (DataRow row in targetTable.Rows)
				{
					existingCodes.Add((string)row[WhsLocationTypeSchema.Constants.WLT_Code]);
				}

				for (int i = 0; i < 1000 && existingCodes.Contains(newCode); i++)
				{
					newCode = newCode.Substring(0, WhsLocationTypeSchema.WLT_Code.MaxLength - i.ToString(CultureInfo.InvariantCulture).Length);
					newCode += i.ToString(CultureInfo.InvariantCulture);
				}
			}

			return newCode;
		}

		#endregion

		#region DoDelete

		protected override void DoDelete(DataRow targetRow, ref int targetIndex)
		{
			if ((bool)targetRow[WhsLocationTypeSchema.Constants.WLT_IsSystem])
			{
				targetRow[WhsLocationTypeSchema.Constants.WLT_IsSystem] = 0;
			}
			targetIndex++;

			// do not call base as we do not want to delete any location types
		}

		#endregion

		#region UpdateColumn

		protected override void UpdateColumn(string columnName, DataRow targetRow, DataRow sourceRow)
		{
			// do nothing as we do not want to override user modified location types
		}

		#endregion
	}
}
