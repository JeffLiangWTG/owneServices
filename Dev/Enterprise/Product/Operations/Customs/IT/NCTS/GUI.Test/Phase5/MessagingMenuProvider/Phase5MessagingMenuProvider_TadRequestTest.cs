using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

sealed class Phase5MessagingMenuProvider_TadRequestTest : Phase5MessagingMenuProvider_MenuItemAbstractTest
{
	public void TestMenuItemAvailability()
	{
		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		var provider = new Phase5MessagingMenuProvider(nctsHeader);
		var menuItems = provider.CreateMenuItems();
		provider.RefreshMenu();

		CombineAssertions(() =>
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Arrival;
			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = ZString.Empty;
			provider.RefreshMenu();
			AssertNull(GetMenuItem());

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
			provider.RefreshMenu();
			AssertNull(GetMenuItem());

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			provider.RefreshMenu();
			AssertMenuItem(GetMenuItem(), visible: true, enabled: false);

			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "24ITQ0B8TEK17951J4";
			provider.RefreshMenu();
			AssertMenuItem(GetMenuItem(), visible: true, enabled: true);
		});

		return;

		ZMenuItem GetMenuItem() => menuItems.SingleOrDefault(x => x.Text == MenuItemText);
	}

	public void TestMenuItemClick()
	{
		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "24ITQ0B8TEK17951J4";

		var provider = new Phase5MessagingMenuProvider(nctsHeader);
		var menuItems = provider.CreateMenuItems();
		provider.RefreshMenu();

		CombineAssertions(() =>
		{
			var menuItem = menuItems.SingleOrDefault(x => x.Text == MenuItemText);
			AssertMenuItem(menuItem, visible: true, enabled: true);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			menuItem.PerformClick();
			AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
		});
	}

	protected override string MenuItemText => "TAD Request";
}
