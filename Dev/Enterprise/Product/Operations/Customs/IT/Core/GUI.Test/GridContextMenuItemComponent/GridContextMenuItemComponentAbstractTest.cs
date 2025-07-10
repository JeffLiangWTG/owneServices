using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.IT.GUI.Testing;

public abstract class GridContextMenuItemComponentAbstractTest<T> : TestCaseWithFactory
	where T : BusinessObject
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("grid parameter is required", () => GetNewContextMenuItemComponent(null));
	}

	public void TestDisposeUnHookEvents()
	{
		using (var messagesGrid = new MessagesGridForTest(Factory.New<DummyBusinessObject>()))
		{
			var contextMenuItemComponent = GetNewContextMenuItemComponent(messagesGrid);
			contextMenuItemComponent.Initialize();

			messagesGrid.Messages.AddNew();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			var contextMenu = messagesGrid.ContextMenu;
			var parentMenuItem = contextMenuItemComponent.ParentMenuItem;

			var menuItem = (parentMenuItem?.MenuItems ?? contextMenu.MenuItems).FindByText(MenuItemText);

			((IDisposable)contextMenuItemComponent).Dispose();
			menuItem.PerformClick();
			AssertNull("UserNotification.LastMessage.Text", UnitTestUserNotification.Instance.LastMessage.Text);

			messagesGrid.Messages.AddNew();

			if (parentMenuItem is null)
			{
				contextMenu.DoPopup();
			}
			else
			{
				parentMenuItem.ShowPopupMenu();
			}

			Assert("Events are unhooked, even if 2 messages are selected, menu item is visible ", menuItem.Visible);
		}
	}

	protected abstract GridContextMenuItemComponent<T> GetNewContextMenuItemComponent(ZGrid grid);

	protected abstract string MenuItemText { get; }
}

#region MessagesGridForTest

public class MessagesGridForTest : ZGrid
{
	public MessagesGridForTest(BusinessObject businessObject)
	{
		Master = businessObject;
	}

	public BusinessObject Master { get; }

	public ITEDIMessageCollection Messages => messages ?? (messages = new ITEDIMessageCollection(Master));
	ITEDIMessageCollection messages;

	public override T[] GetSelectedElements<T>() => Messages.ToArray<T>();
}

#endregion

#region EntriesGridForTest

public class EntriesGridForTest : ZGrid
{
	public EntriesGridForTest(JobDeclaration declaration)
	{
		Declaration = declaration;
	}

	public JobDeclaration Declaration { get; }

	public override T[] GetSelectedElements<T>() => Declaration.CustomsEntryHeaders.ToArray<T>();
}

#endregion
