using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class CusCAeMHItemZGridOverrideDefaultValuesSupporterTest : TestCaseWithFactory
	{
		public void TestOverrideDefaultValuesMenuItem()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();
			using (var form = new ZForm(master))
			using (var userControl = new CusCAeMHHouseUserControl())
			{
				userControl.SetDataBinding(master, "HouseBills");
				form.Show();
				var houseTabControl = userControl.FindSingle<ZTabControl>("HouseTabControl");
				var packLineItemsTabPage = userControl.FindSingle<ZTabPage>("ItemsTabPage");
				houseTabControl.SelectTab(packLineItemsTabPage);
				var itemUserControl = userControl.FindSingle<CusCAeMHItemUserControl>("cusCAeMHItemUserControl");
				var itemGrid = itemUserControl.FindSingle<ZGrid>("eMHItemsGrid");
				itemGrid.SetDataBinding(house.Items, "");
				itemGrid.ContextMenu.OnPopup_ForTest();
				AssertNull("Override Default Menu should not be visible on stand alone", itemGrid.ContextMenu.MenuItems.FindByText("Override Default Values"));
			}

			var shipment = Factory.New<ForwardingShipment>();
			house.BW_ParentID = shipment.PK;

			using (var form = new ZForm(master))
			using (var userControl = new CusCAeMHHouseUserControl())
			{
				userControl.SetDataBinding(master, "HouseBills");
				form.Show();
				var houseTabControl = userControl.FindSingle<ZTabControl>("HouseTabControl");
				var packLineItemsTabPage = userControl.FindSingle<ZTabPage>("ItemsTabPage");
				houseTabControl.SelectTab(packLineItemsTabPage);
				var itemUserControl = userControl.FindSingle<CusCAeMHItemUserControl>("cusCAeMHItemUserControl");
				var itemGrid = itemUserControl.FindSingle<ZGrid>("eMHItemsGrid");
				itemGrid.SetDataBinding(house.Items, "");
				itemGrid.ContextMenu.OnPopup_ForTest();
				AssertNotNull("Override Default Menu should contain Override Default Values", itemGrid.ContextMenu.MenuItems.FindByText("Override Default Values"));
			}
		}

		public void TestValueChanged()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();
			var shipment = Factory.New<ForwardingShipment>();
			house.BW_ParentID = shipment.PK;

			using (var form = new ZForm(master))
			using (var userControl = new CusCAeMHHouseUserControl())
			{
				userControl.SetDataBinding(master, "HouseBills");
				form.Show();
				var houseTabControl = userControl.FindSingle<ZTabControl>("HouseTabControl");
				var packLineItemsTabPage = userControl.FindSingle<ZTabPage>("ItemsTabPage");
				houseTabControl.SelectTab(packLineItemsTabPage);
				var itemUserControl = userControl.FindSingle<CusCAeMHItemUserControl>("cusCAeMHItemUserControl");
				var itemGrid = itemUserControl.FindSingle<ZGrid>("eMHItemsGrid");
				itemGrid.SetDataBinding(house.Items, "");
				itemGrid.ContextMenu.OnPopup_ForTest();
				var overrideMenuItem = itemGrid.ContextMenu.MenuItems.FindByText("Override Default Values");
				AssertEquals("overrideMenuItem should be visible", true, overrideMenuItem.Visible);
				AssertEquals("overrideMenuItem should be unchecked", false, overrideMenuItem.Checked);

				overrideMenuItem.PerformClick();
				AssertEquals("overrideMenuItem should be checked", true, overrideMenuItem.Checked);
				AssertEquals("BW_OverrideFreightDefaults should be synced to true", true, house.BW_OverrideFreightDefaults);

				overrideMenuItem.PerformClick();
				AssertEquals("overrideMenuItem should be unchecked", false, overrideMenuItem.Checked);
				AssertEquals("BW_OverrideFreightDefaults should be synced to false", false, house.BW_OverrideFreightDefaults);

				house.BW_OverrideFreightDefaults = true;
				AssertEquals("overrideMenuItem should be synced to checked", true, overrideMenuItem.Checked);
			}
		}
	}
}
