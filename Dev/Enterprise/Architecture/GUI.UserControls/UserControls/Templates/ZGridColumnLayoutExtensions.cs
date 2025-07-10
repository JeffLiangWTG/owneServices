using System.Linq;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.GUI
{
	public static class ZGridColumnLayoutExtensions
	{
		public static void ApplyGridColumnLayout(this ZGrid grid, IGridColumnLayoutProvider gridColumnLayoutProvider)
		{
			Argument.NotNull(grid, nameof(grid));
			Argument.NotNull(gridColumnLayoutProvider, nameof(gridColumnLayoutProvider));
			var gridColumnLayout = Argument.NotNull(gridColumnLayoutProvider.Layout, nameof(gridColumnLayoutProvider.Layout));

			using (grid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				var gridLayoutColumns = gridColumnLayout.Columns.ToArray();

				var columnsToRemove = grid.Columns
					.Select(c => c.ColumnName)
					.Except(gridLayoutColumns.Select(c => c.ColumnName))
					.ToArray();

				if (columnsToRemove.Length > 0)
				{
					grid.RemoveFromAvailableColumns(columnsToRemove);
				}

				grid.ColumnStyles.Clear();
				grid.ColumnStyles.AddRange(gridLayoutColumns);
			}
		}
	}
}
