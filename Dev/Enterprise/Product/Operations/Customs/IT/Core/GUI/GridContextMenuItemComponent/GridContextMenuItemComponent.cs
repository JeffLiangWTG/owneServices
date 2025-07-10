using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.Customs.IT.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public abstract class GridContextMenuItemComponent<T> : IDisposable, IGridContextMenuItemComponent
	where T : BusinessObject
{
	protected GridContextMenuItemComponent(ZGrid grid, MenuItem parentMenuItem = null)
	{
		Grid = Argument.NotNull(grid, nameof(grid));
		ParentMenuItem = parentMenuItem;
	}

	protected ZGrid Grid { get; }

	internal MenuItem ParentMenuItem { get; }

	public void Initialize()
	{
		if (MenuItem == null)
		{
			throw new InvalidOperationException("MenuItem must be not null");
		}

		MenuItem.Click += MenuItem_Click;

		if (ParentMenuItem is not null)
		{
			ParentMenuItem.MenuItems.Add(menuItem);
			ParentMenuItem.Popup += ContextMenu_Popup;
		}
		else
		{
			ContextMenu.MenuItems.Add(menuItem);
			ContextMenu.Popup += ContextMenu_Popup;
		}
	}

	public void Dispose()
	{
		if (isDisposed)
		{
			return;
		}

		UnHookEvents();
		isDisposed = true;
	}

	protected T DataContext => GetDataContext();

	protected virtual T GetDataContext() => SelectedElements.OfType<T>().FirstOrDefault();

	protected abstract ZMenuItem GetMenuItem();

	protected abstract void Execute(ZMenuItem menuItem);

	#region Implementation

	ZMenuItem MenuItem => menuItem ?? (menuItem = GetMenuItem());
	ZMenuItem menuItem;

	void ContextMenu_Popup(object sender, EventArgs e)
	{
		menuItem.Visible = IsMenuItemVisible(menuItem);
		menuItem.Enabled = IsMenuItemEnabled(menuItem);
	}

	protected virtual bool IsMenuItemVisible(ZMenuItem menuItem) => SelectedElements.Length == 1;

	protected virtual bool IsMenuItemEnabled(ZMenuItem menuItem) => true;

	protected bool IsFormPreSaved(ITopLevelBusinessObjectProvider topLevelBizObjProvider, ZMenuItem menuItem)
	{
		var topLevelBizObj = Argument.NotNull(topLevelBizObjProvider.TopLevelBusinessObject, nameof(topLevelBizObjProvider.TopLevelBusinessObject));
		var parentForm = menuItem.ParentControl?.FindForm() as ZForm;
		return CustomsPlugIn.FormPreSaved(topLevelBizObj, parentForm);
	}

	void UnHookEvents()
	{
		if (MenuItem != null)
		{
			MenuItem.Click -= MenuItem_Click;
		}

		if (ParentMenuItem != null)
		{
			ParentMenuItem.Popup -= ContextMenu_Popup;
		}
		else
		{
			ContextMenu.Popup -= ContextMenu_Popup;
		}
	}

	void MenuItem_Click(object sender, EventArgs e)
	{
		if (sender is ZMenuItem menuItem)
		{
			Execute(menuItem);
		}
	}

	BusinessObject[] SelectedElements => Grid.SelectedElements;

	ContextMenu ContextMenu => Grid.ContextMenu;
	bool isDisposed;

	#endregion
}
