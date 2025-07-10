using System;
using System.Drawing;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.Testing
{
	public class GridColourSchemeManagerForTest : GridColourSchemeManager
	{
		public GridColourSchemeManagerForTest(ZGrid grid) : base(grid)
		{
			Grid = grid;
		}

		readonly ZGrid Grid;
		GridColourScheme ColorScheme;

		public void SetLastUsedSchemeForCurrentUser(GridColourScheme colorScheme)
		{
			ColorScheme = colorScheme;
			colourFactory.SetLastUsedSchemeForCurrentUser(FilterBusinessObject, colorScheme);
		}

		public void OnCustomRowBackgroundColourDeciding(BusinessObject bizO)
		{
			var eventArgs = new ColourDecidingEventArgs(bizO);
			grid_CustomRowBackgroundColourDeciding(Grid, eventArgs);
		}

		public GridColourStripBusinessObject AddColorStrip(FilterStripBusinessObject filterBO, Type bOType, Color color)
		{
			var gridColorStripBO = new GridColourStripBusinessObject(filterBO, ColorScheme, bOType) { BGColor = color };
			ColorScheme.ColourStrips.Add(gridColorStripBO);
			return gridColorStripBO;
		}

		public static void RecreateGridColourSchemeManager(ZGrid grid, GridColourScheme scheme, bool allowPrecalculateRows)
		{
			grid.GridColourSchemeManagerForTest.colourFactory.SetLastUsedSchemeForCurrentUser(grid.GridColourSchemeManagerForTest.FilterBusinessObject, scheme);
			grid.GridColourSchemeManagerForTest.AllowPrecalculateColorsForAllRowsInGrid = allowPrecalculateRows;
		}

		public static Color GetCustomRowBackgroundColour(ZGrid grid, int index)
		{
			return grid.GetCustomRowBackgroundColour(index);
		}
	}
}
