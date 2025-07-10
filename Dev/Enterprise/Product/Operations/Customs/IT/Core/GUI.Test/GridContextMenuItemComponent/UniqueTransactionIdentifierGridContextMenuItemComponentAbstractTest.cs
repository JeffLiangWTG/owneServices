using CargoWise.Windows.UI;
using Enterprise.Customs.IT.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IT.GUI.Testing;

public abstract class UniqueTransactionIdentifierGridContextMenuItemComponentAbstractTest : GridContextMenuItemComponentAbstractTest<ITEDIMessage>
{
	public void TestMenuItemCaption()
	{
		using (var messagesGrid = new ZGrid())
		using (var component = GetNewContextMenuItemComponent(messagesGrid))
		{
			component.Initialize();

			var menuItem = messagesGrid.ContextMenu.MenuItems.FindByName("RequestResponsesMenuItem");
			AssertEquals("MenuItem Text", "Request Responses", menuItem.Text);
		}
	}

	protected override string MenuItemText => "Request Responses";
}
