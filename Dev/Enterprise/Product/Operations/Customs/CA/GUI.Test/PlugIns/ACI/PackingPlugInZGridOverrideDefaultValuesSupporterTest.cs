using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class PackingPlugInZGridOverrideDefaultValuesSupporterTest : TestCaseWithFactory
	{
		public void TestOverrideDefaultValuesMenuItem()
		{
			var master = Factory.New<CusSCAOceanBill>();
			var house = master.HouseBills.AddNew();
			using (var form = new ZForm(master))
			using (var userControl = new ConsolACIUserControl())
			{
				userControl.SetDataBinding(master, "HouseBills");
				form.Show();
				var houseTabControl = userControl.FindSingle<ZTabControl>("BillOfLadingTabControl");
				var packingTabPage = userControl.FindSingle<ZTabPage>("PackingTabPage");
				houseTabControl.SelectTab(packingTabPage);
				var packingUserControl = userControl.FindSingle<PackingPlugInUserControl>("packingUserControl");
				var packingGrid = packingUserControl.FindSingle<ZGrid>("PackingGrid");
				packingGrid.SetDataBinding(house.PackLines, "");
				packingGrid.ContextMenu.OnPopup_ForTest();
				AssertNull("Override Default Menu should not be visible on stand alone", packingGrid.ContextMenu.MenuItems.FindByText("Override Default Values"));
			}

			var shipment = Factory.New<ForwardingShipment>();
			house.CA_JS = shipment.PK;

			using (var form = new ZForm(master))
			using (var userControl = new ConsolACIUserControl())
			{
				userControl.SetDataBinding(master, "HouseBills");
				form.Show();
				var houseTabControl = userControl.FindSingle<ZTabControl>("BillOfLadingTabControl");
				var packingTabPage = userControl.FindSingle<ZTabPage>("PackingTabPage");
				houseTabControl.SelectTab(packingTabPage);
				var packingUserControl = userControl.FindSingle<PackingPlugInUserControl>("packingUserControl");
				var packingGrid = packingUserControl.FindSingle<ZGrid>("PackingGrid");
				packingGrid.SetDataBinding(house.PackLines, "");
				packingGrid.ContextMenu.OnPopup_ForTest();
				AssertNotNull("Override Default Menu should contain Override Default Values", packingGrid.ContextMenu.MenuItems.FindByText("Override Default Values"));
			}
		}

		public void TestValueChanged()
		{
			var master = Factory.New<CusSCAOceanBill>();
			var house = master.HouseBills.AddNew();
			var shipment = Factory.New<ForwardingShipment>();
			house.CA_JS = shipment.PK;

			using (var form = new ZForm(master))
			using (var userControl = new ConsolACIUserControl())
			{
				userControl.SetDataBinding(master, "HouseBills");
				form.Show();
				var houseTabControl = userControl.FindSingle<ZTabControl>("BillOfLadingTabControl");
				var packingTabPage = userControl.FindSingle<ZTabPage>("PackingTabPage");
				houseTabControl.SelectTab(packingTabPage);
				var packingUserControl = userControl.FindSingle<PackingPlugInUserControl>("packingUserControl");
				var packingGrid = packingUserControl.FindSingle<ZGrid>("PackingGrid");
				packingGrid.SetDataBinding(house.PackLines, "");
				packingGrid.ContextMenu.OnPopup_ForTest();
				var overrideMenuItem = packingGrid.ContextMenu.MenuItems.FindByText("Override Default Values");
				AssertEquals("overrideMenuItem should be visible", true, overrideMenuItem.Visible);
				AssertEquals("overrideMenuItem should be unchecked", false, overrideMenuItem.Checked);

				overrideMenuItem.PerformClick();
				AssertEquals("overrideMenuItem should be checked", true, overrideMenuItem.Checked);
				AssertEquals("CA_OverrideFreightDefaults should be synced to true", true, house.CA_OverrideFreightDefaults);

				overrideMenuItem.PerformClick();
				AssertEquals("overrideMenuItem should be unchecked", false, overrideMenuItem.Checked);
				AssertEquals("CA_OverrideFreightDefaults should be synced to false", false, house.CA_OverrideFreightDefaults);

				house.CA_OverrideFreightDefaults = true;
				AssertEquals("overrideMenuItem should be synced to checked", true, overrideMenuItem.Checked);
			}
		}
	}
}
