using System;
using System.Linq;
using System.Reflection;
using CargoWise.Common;

namespace Enterprise.Registry.GUI
{
	public class VerticalPlacementHelper
	{
		public VerticalPlacementHelper(IVerticalPlacementClient client)
		{
			Argument.NotNull(client, "client");
			this.client = client;
		}

		readonly IVerticalPlacementClient client;

		public void AdjustVerticalPlacement()
		{
			client.SetGridHeight(Math.Min(GetGridsHeight(), client.GridMaxHeight));
			int totalHeightInGrids = client.Grids.Sum(grid => grid.Height);
			client.SetAdditionalControlsPosition(totalHeightInGrids + additionalControlTopPadding);
		}

		int GetGridsHeight()
		{
			int height = 0;

			if (dataGridRowsPropertyInfos == null)
			{
				dataGridRowsPropertyInfos = new PropertyInfo[client.Grids.Length];
				for (int i = 0; i < client.Grids.Length; i++)
				{
					dataGridRowsPropertyInfos[i] = client.Grids[i].GetType().GetProperty("DataGridRows", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
				}
			}

			for (int i = 0; i < client.Grids.Length; i++)
			{
				object[] rows = (object[])dataGridRowsPropertyInfos[i].GetValue(client.Grids[i], null);
				int gridHeight = rows.Sum(row => (int)row.GetType().GetProperty("Height").GetValue(row, null));
				height += rows.Length > 0 ? gridHeight + ExtraRowsInHeightCalculation * (gridHeight / rows.Length) : emptyGridHeight;
			}

			return height;
		}

		PropertyInfo[] dataGridRowsPropertyInfos;

		public const int ExtraRowsInHeightCalculation = 3;  //One for header caption and two for bottom padding
		const int additionalControlTopPadding = 12;
		const int emptyGridHeight = 80;
	}
}
