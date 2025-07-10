using System;
using System.Linq;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	public class ColumnResizer
	{
		public ColumnResizer(ZGrid grid, params string[] columnsToExpand)
		{
			this.grid = grid;
			this.columnsToExpand = columnsToExpand;

			grid.SizeChanged += new EventHandler(grid_SizeChanged);
			grid.AfterBind += new EventHandler(grid_AfterBind);
		}

		void grid_AfterBind(object sender, EventArgs e)
		{
			ResizeColumns();
		}

		void grid_SizeChanged(object sender, EventArgs e)
		{
			ResizeColumns();
		}

		void ResizeColumns()
		{
			if (grid.TableStyles.Count > 0)
			{
				var availableSize = grid.ClientSize.Width - grid.TableStyles[0].RowHeaderWidth;
				if (grid.IsVerticalScrollBarVisible)
				{
					availableSize -= ControlDpiScalingHelper.ScaleToCurrentDpiX(16);
				}

				var usedSize = grid.Columns.Where(c => c.IsVisible).Sum(c => c.ColumnStyle.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(3));
				var delta = availableSize - usedSize;
				var num = columnsToExpand.Where(c => grid.Columns[c].IsVisible).Count();

				foreach (var columnToExpand in columnsToExpand)
				{
					if (grid.Columns[columnToExpand].IsVisible)
					{
						ControlDpiScalingHelper.SetWidth(grid.Columns[columnToExpand].ColumnStyle, grid.Columns[columnToExpand].ColumnStyle.Width + delta / num, false);
					}
				}
			}
		}

		readonly ZGrid grid;
		readonly string[] columnsToExpand;
	}
}
