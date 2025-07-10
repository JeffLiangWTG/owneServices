using System.Collections.Generic;
using System.Windows.Forms;

using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI.Support;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public sealed class ZGridContextMenuHelper
	{
		ZGridContextMenuHelper()
		{
			dynamicMenuProviders = new List<IDynamicMenuProvider<DynamicMenuItem>>();
		}

		public static ZGridContextMenuHelper Instance
		{
			get
			{
				if (fInstance == null)
				{
					fInstance = new ZGridContextMenuHelper();
				}
				return fInstance;
			}
		}
		[SuppressThreadStaticFieldMessage]
		static ZGridContextMenuHelper fInstance;

		public void AddProvider(IDynamicMenuProvider<DynamicMenuItem> provider)
		{
			dynamicMenuProviders.Add(provider);
		}

		public void Synchronise(ZGrid grid)
		{
			RemoveDyanamicMenuItems(grid);
			AddDynamicMenuItems(grid);
			UpdateDeleteMenuItemIfNeeded(grid);
		}

		void UpdateDeleteMenuItemIfNeeded(ZGrid grid)
		{
			var menuItem = grid.DeleteMenuItemModule;

			if (menuItem != null && grid.SelectedElements.Length > 0)
			{
				var businessObjects = grid.SelectedElements;
				BusinessObject selectedBusinessObject = null;

				selectedBusinessObject = grid.SelectedElements[0];

				if (businessObjects.Length == 0 && grid.CurrentRowIndex == 0)
				{
					// it means first row is in focus, but not really selected - the case when we just opened module
					// and pressed right click on the first Row - it not get selected, but has got focus
					// Delete menu is present and if we click it - it assumes that first row IS selected
					// so we need to hack it a bit. By this if statement. Feel free to refactor if you find a better way.
					// Alex K.

					selectedBusinessObject = (BusinessObject)grid.List[grid.CurrentRowIndex];
				}

				var cancellable = selectedBusinessObject as ICancellable;

				if (cancellable != null && PreventDeleteAttribute.IsTrue(selectedBusinessObject.GetType()))
				{
					menuItem.Text = GetDeleteMenuItemTextForCancellableBizo(selectedBusinessObject);
				}
			}
		}

#if DEBUG
		internal
#endif
		string GetDeleteMenuItemTextForCancellableBizo(BusinessObject bizo)
		{
			return CancellableHelper.GetICancellable(bizo).IsCancelled
				? ZFilterGridModule.DeleteButtonCaptions.Activate
				: ZFilterGridModule.DeleteButtonCaptions.Deactivate;
		}

		void AddDynamicMenuItems(ZGrid grid)
		{
			var businessObjects = grid.SelectedElements;
			if (businessObjects.Length == 1)
			{
				var selectedbusinessObject = grid.SelectedElements[0];
				foreach (var provider in dynamicMenuProviders)
				{
					var menuItem = provider.GetMenuItem(grid.FindForm(), selectedbusinessObject);
					if (menuItem != null)
					{
						grid.ContextMenu.MenuItems.Add(menuItem);
					}
				}
			}
		}

		void RemoveDyanamicMenuItems(ZGrid grid)
		{
			foreach (MenuItem menuItem in grid.ContextMenu.MenuItems)
			{
				if (menuItem is DynamicMenuItem)
				{
					grid.ContextMenu.MenuItems.Remove(menuItem);
					menuItem.Dispose();
				}
			}
		}

		readonly List<IDynamicMenuProvider<DynamicMenuItem>> dynamicMenuProviders;
	}
}
