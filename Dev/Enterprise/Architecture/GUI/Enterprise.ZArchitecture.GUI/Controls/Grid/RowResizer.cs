using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.GUI
{
	public class RowResizer
	{
		public RowResizer(ZGrid grid, params string[] columnsToResize)
		{
			this.grid = grid;
			this.columnsToResize = columnsToResize;
			grid.AfterBind += new EventHandler(grid_AfterBind);
			grid.Layout += grid_Layout;
		}

		void grid_AfterBind(object sender, EventArgs e)
		{
			foreach (var columnToResize in columnsToResize)
			{
				grid.Columns[columnToResize].ColumnStyle.WidthChanged += new EventHandler(ColumnStyle_WidthChanged);
				((ZTextBoxColumnStyle)grid.Columns[columnToResize].ColumnStyle).TextBox.Multiline = true;
			}

			ResizeRows();
		}

		void ColumnStyle_WidthChanged(object sender, EventArgs e)
		{
			ResizeRows();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "internal code")]
		void grid_Layout(object sender, LayoutEventArgs e)
		{
			if (e.AffectedProperty != "Parent")
			{
				ResizeRows();
			}
		}

		void ResizeRows()
		{
			if (dataGridRowsProperty == null)
			{
				dataGridRowsProperty = typeof(DataGrid).GetProperty("DataGridRows", BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic);
			}
			var dataGridRows = (Array)dataGridRowsProperty.GetValue(grid, null);
			for (var row = 0; row < dataGridRows.Length; row++)
			{
				var maxHeight = columnsToResize.Max(c => GetRowHeight(row, c));
				maxHeight = maxHeight <= 0 ? 20 : maxHeight;

				var dataGridRow = dataGridRows.GetValue(row);
				dataGridRowHeightProperty = dataGridRow.GetType().GetProperty("Height");
				try
				{
					dataGridRowHeightProperty.SetValue(dataGridRow, maxHeight + 6, null);
				}
				catch (TargetInvocationException e)
				{
					if (e.InnerException is IndexOutOfRangeException)
					{
						ErrorReporter.ReportOnce("IndexOutOfRangeExceptionInDataGridRowHeight",
							string.Format("Exception occured while calling DataGridRow.Height in RowResizer. row = {0}, dataGridRow.RowNumber = {1}, dataGridRows.Length = {2}, if we get DataGridRows again and get Length = {3}, firstVisibleRow = {4}",
							row,
							dataGridRow.GetType().GetProperty("RowNumber").GetValue(dataGridRow, null),
							dataGridRows.Length,
							((Array)dataGridRowsProperty.GetValue(grid, null)).Length,
							typeof(DataGrid).GetField("firstVisibleRow", BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic).GetValue(grid)
							));
					}
					else
					{
						throw;
					}
				}
			}
		}
		PropertyInfo dataGridRowsProperty;
		PropertyInfo dataGridRowHeightProperty;

		int GetRowHeight(int row, string column)
		{
			var columnStyle = (ZTextBoxColumnStyle)grid.Columns[column].ColumnStyle;
			var columnIndex = columnStyle.DataGridTableStyle.GridColumnStyles.IndexOf(columnStyle);
			if (columnIndex > -1)
			{
				using (var graphics = columnStyle.TextBox.CreateGraphics())
				{
					return graphics.MeasureString(grid[row, columnIndex].ToString(), columnStyle.TextBox.Font, columnStyle.Width).ToSize().Height;
				}
			}
			else
			{
				return 20;
			}
		}

		readonly ZGrid grid;
		readonly string[] columnsToResize;
	}
}
