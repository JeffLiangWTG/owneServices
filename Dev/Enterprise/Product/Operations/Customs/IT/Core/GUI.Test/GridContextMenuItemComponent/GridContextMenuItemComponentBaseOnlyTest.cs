using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(MockContextMenuItemComponent))]
sealed class GridContextMenuItemComponentBaseOnlyTest : GridContextMenuItemComponentAbstractTest<DummyBusinessObject>
{
	public void TestMenuItemVisibility()
	{
		using (var messagesGrid = new MessagesGridForTest(Factory.New<DummyBusinessObject>()))
		{
			var contextMenuItemComponent = GetNewContextMenuItemComponent(messagesGrid);
			contextMenuItemComponent.Initialize();
			var mockMenuItem = messagesGrid.ContextMenu.MenuItems.FindByText(MenuItemText);

			CombineAssertions("[Case 1]: no messages selected", () =>
			{
				messagesGrid.Messages.RemoveAndDeleteAll();
				AssertEquals("No selected elements", 0, messagesGrid.SelectedElements.Length);
				messagesGrid.ContextMenu.DoPopup();
				Assert("Menu item not visible", !mockMenuItem.Visible);
			});

			CombineAssertions("[Case 2]: 1 message selected", () =>
			{
				messagesGrid.Messages.AddNew();
				AssertEquals("1 element selected", 1, messagesGrid.SelectedElements.Length);
				messagesGrid.ContextMenu.DoPopup();
				Assert("Menu item is visible", mockMenuItem.Visible);
			});

			CombineAssertions("[Case 3]: more than 1 message selected", () =>
			{
				messagesGrid.Messages.AddNew();
				AssertEquals("2 elements selected", 2, messagesGrid.SelectedElements.Length);
				messagesGrid.ContextMenu.DoPopup();
				Assert("Menu item is not visible", !mockMenuItem.Visible);
			});
		}
	}

	public void TestThrowExceptionWhenMenuItemIsNull()
	{
		using (var grid = new ZGrid())
		{
			var contextMenuItemComponent = new NullMenuItemMockComponent(grid);
			AssertExceptionThrown<InvalidOperationException>("MenuItem must be not null", () => contextMenuItemComponent.Initialize());
		}
	}

	protected override string MenuItemText => "Mock";

	protected override GridContextMenuItemComponent<DummyBusinessObject> GetNewContextMenuItemComponent(ZGrid grid) => new MockContextMenuItemComponent(grid);

	class NullMenuItemMockComponent : GridContextMenuItemComponent<DummyBusinessObject>
	{
		public NullMenuItemMockComponent(ZGrid grid) : base(grid)
		{
		}

		protected override void Execute(ZMenuItem menuItem)
		{
		}

		protected override ZMenuItem GetMenuItem() => null;
	}

	class MockContextMenuItemComponent : GridContextMenuItemComponent<DummyBusinessObject>
	{
		public MockContextMenuItemComponent(ZGrid grid) : base(grid)
		{
		}

		protected override void Execute(ZMenuItem menuItem)
		{
		}

		protected override ZMenuItem GetMenuItem() => new ZMenuItem("Mock");
	}
}
