using System.Linq;
using CargoWise.Common;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IT.GUI;

public static class ZGridExtension
{
	/// <summary>
	/// Add column next to a specified column. Add as first if the specified column has not been found
	/// </summary>
	/// <param name="grid"></param>
	/// <param name="adjoiningColumnName">Existing column to add new the column next to it</param>
	/// <param name="newColumn">New column to add to grid</param>
	public static void AddAdjoiningColumn(this ZGrid grid, string adjoiningColumnName, ZGridColumnInfo newColumn)
	{
		Argument.NotNull(grid, nameof(grid));
		Argument.NotNullOrEmpty(adjoiningColumnName, nameof(adjoiningColumnName));
		Argument.NotNull(newColumn, nameof(newColumn));

		var index = grid.ColumnStyles
			.Cast<ZGridColumnInfo>()
			.IndexOf(x => x.ColumnName == adjoiningColumnName);

		grid.ColumnStyles.Insert(++index, newColumn);
	}
}
