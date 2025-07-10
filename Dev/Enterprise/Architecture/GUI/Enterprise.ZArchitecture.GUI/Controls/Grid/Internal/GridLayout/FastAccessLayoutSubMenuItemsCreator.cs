using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	/// <summary>
	/// Adds context-sensitive menus for a grid for existing layouts for easy access
	/// </summary>
	internal class FastAccessLayoutSubMenuItemsCreator
	{
		public FastAccessLayoutSubMenuItemsCreator(ZGrid grid, MenuItem availableLayoutsMenu)
		{
			this.grid = grid;
			this.availableLayoutsMenu = availableLayoutsMenu;
		}

		readonly ZGrid grid;
		readonly MenuItem availableLayoutsMenu;

		public void AddSubMenus(IEnumerable<IGridLayoutStorage> layouts)
		{
			availableLayoutsMenu.MenuItems.Clear();

			foreach (var layout in layouts)
			{
				availableLayoutsMenu.MenuItems.Add(new ZMenuItem(StmModuleFilter.GetDisplayName(layout.ColumnLayoutDisplayName, isUserDefinedFilter: false), new EventHandler(OnLayoutMenuSelected)) { Tag = layout });
			}

			if (availableLayoutsMenu.MenuItems.Count > 0)
			{
				availableLayoutsMenu.MenuItems.Add(new ZMenuItem("-"));
			}

			availableLayoutsMenu.MenuItems.Add(new ZMenuItem(ResetToSystemDefaultLayoutMenuText, new EventHandler(OnLayoutMenuSelected)));
		}

		internal static string ResetToSystemDefaultLayoutMenuText
		{
			get { return Res.GetString("97233de4-7800-46ab-a758-6a2440568ce4", "System Default Layout"); }
		}

		void OnLayoutMenuSelected(object sender, EventArgs e)
		{
			var menu = (KMenuItem)sender;

			if (menu.Text == ResetToSystemDefaultLayoutMenuText)
			{
				grid.CurrentColumnLayout = null;
				grid.Columns.HasLayoutChanged = true;
				grid.ResetColumns();
			}
			else
			{
				grid.CurrentColumnLayout = (IGridLayoutStorage)menu.Tag;
				grid.LoadUserLayoutSettings();
			}
			grid.Columns.HasLayoutChanged = true; // when disposed, this layout is remembered as the last one.
			grid.SaveLastSelectedLayout = true; // when disposed, this layout is remembered as the last one.
			grid.LastFocusedColumn.HideEditControl();
		}
	}
}
