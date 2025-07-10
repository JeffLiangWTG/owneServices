using System;
using System.Collections.Generic;
using Enterprise.Core.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	sealed class GridColumnLayout : IGridColumnLayout
	{
		#region IGridColumnLayout

		IReadOnlyCollection<ZGridColumnInfo> IGridColumnLayout.Columns => gridColumns.Values;

		bool IGridColumnLayout.HasColumn(string columnName) => HasColumn(columnName);

		#endregion

		internal void AddColumn(ZGridColumnInfo columnInfo)
		{
			var columnName = columnInfo.ColumnName;
			if (gridColumns.ContainsKey(columnName))
			{
				throw new InvalidOperationException(FormattableString.Invariant($"Column with the same name already exists: {columnName}"));
			}

			columnInfo.ColumnName = columnName;
			gridColumns.Add(columnName, columnInfo);
		}

		bool HasColumn(string columnName) => gridColumns.ContainsKey(columnName);

		readonly Dictionary<string, ZGridColumnInfo> gridColumns = new Dictionary<string, ZGridColumnInfo>();
	}
}
