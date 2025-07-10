using System.Drawing;

using CargoWise.Common;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI.Grid
{
	internal class ZGridColumnLabelRenderer : ControlExtension, IAutomaticLabelExtension
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		public void Refresh()
		{
			var grid = (ZGrid)Owner;

			foreach (var column in grid.Columns)
			{
				var style = (ZTextBoxColumnStyle)column.ColumnStyle;
				style.RefreshHeader();
			}

			// hack: can't get column.Invalidate work
			var layoutData = ReflectionUtil.GetFieldValue(grid, "layout");
			var headerRect = (Rectangle)ReflectionUtil.GetFieldValue(layoutData, "ColumnHeaders");
			grid.Invalidate(headerRect);
		}
	}
}