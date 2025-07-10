using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

abstract class Phase5MessagingMenuProvider_MenuItemAbstractTest : TestCaseWithFactory
{
	public void TestMenuItem()
	{
		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		var provider = new Phase5MessagingMenuProvider(nctsHeader);
		var menuItems = provider.CreateMenuItems();
		provider.RefreshMenu();

		var menuItem = menuItems.SingleOrDefault(x => x.Text == MenuItemText);
		AssertNotNull(menuItem);
	}

	protected void AssertMenuItem(ZMenuItem menuItem, bool visible, bool enabled)
	{
		AssertNotNull(menuItem);
		AssertEquals($"Menu item '{MenuItemText}' visible '{visible}'", expected: visible, menuItem.Visible);
		AssertEquals($"Menu item '{MenuItemText}' enabled '{enabled}'", expected: enabled, menuItem.Enabled);
	}

	protected void AssertRequestCreation(NctsHeader header, string requestType, string expectedMessage)
	{
		AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

		var messages = header?.MovementHeader?.Messages;
		AssertNotNull(messages);

		messages.Reload(reLoadExistingRows: false);
		var lastMessage = messages.GetLastMessageByType(requestType);
		AssertNotNull($"Message '{requestType}' should have been created", lastMessage);
	}

	protected abstract string MenuItemText { get; }
}
