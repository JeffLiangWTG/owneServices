using System;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	public sealed class GridColumnReference<TColumnInfo> : IGridColumnReference
		where TColumnInfo : ZGridColumnInfo, new()
	{
		public GridColumnReference(string columnName, int width, Action<TColumnInfo> configure)
			: this(columnName, width)
		{
			this.configure = configure;
		}

		public GridColumnReference(string columnName, int width)
		{
			this.columnName = Argument.NotNullOrEmpty(columnName, nameof(columnName));
			this.width = width;
		}

		readonly string columnName;
		readonly int width;
		readonly Action<TColumnInfo> configure;

		#region IGridColumnReference

		ZGridColumnInfo IGridColumnReference.CreateGridColumnInfo()
		{
			var columnInfo = new TColumnInfo { ColumnName = columnName, Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(width) };
			configure?.Invoke(columnInfo);
			return columnInfo;
		}

		string IGridColumnReference.ColumnName => columnName;

		#endregion
	}
}
