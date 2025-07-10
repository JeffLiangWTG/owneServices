using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

[TestedType(typeof(ClickableMenuItemComponent))]
sealed class ClickableMenuItemComponentTest : ClickableItemAbstractTest
{
	public void TestMenuItemClick()
	{
		var menuItemComponent = new ClickableMenuItemComponent(
			new ClickableContextForTest(nctsHeader),
			(m, i) => m.Click += (s, e) => i.ClickableContext.Execute(i)
		);

		menuItemComponent.MenuItem.PerformClick();
		AssertEquals("Clickable Context For Test", UnitTestUserNotification.Instance.LastMessage.Text);
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.NewDepartureNctsHeader();
		clickableItem = new ClickableMenuItemComponent(new ClickableContextForTest(nctsHeader));
	}

	NctsHeader nctsHeader;
	IClickableItem clickableItem;

	protected override IClickableItem GetClickableItem() => clickableItem;
	protected override ITopLevelBusinessObjectProvider GetTopLevelBusinessObjectProvider() => nctsHeader.MovementHeader;
}
