using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Client.MFI.CaroTrans.Export;
using Enterprise.DataTransfer.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.MFI.GUI.Testing
{
	public class MFIMenuTest : TestCaseWithFactory
	{
		public void TestMenuWithConsol()
		{
			MFIMenu.Initialise();
			using (ZForm form = new ZForm(Factory.New(typeof(CommonConsol))))
			using (ActionDataMenuItem menu = ActionDataMenuItem.New(form))
			{
				menu.OnPopup(EventArgs.Empty);
				AssertEquals("Initialised Menu Type", typeof(MFIMenu), menu.GetType());
				AssertEquals("Customised MenuItem Count should be same as the number added in AddCustomMenuItems() method", 1, menu.MenuItems.Count);
				AssertEquals("Export to CaroTrans", menu.MenuItems[0].Text);
			}
		}

		public void TestDefaultFileName()
		{
			MFIMenu.Initialise();
			CommonConsol consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_UniqueConsignRef = "C00001001";
			using (ZForm form = new ZForm(consol))
			using (MFIMenu menu = (MFIMenu)MFIMenu.New(form))
			{
				AssertEquals("SYD1001", menu.GetDefaultFileNameForExport(consol));
			}
		}

		public void TestMenuWithShipment()
		{
			MFIMenu.Initialise();
			using (ZForm form = new ZForm(CommonShipment.New(Factory)))
			using (ActionDataMenuItem menu = ActionDataMenuItem.New(form))
			{
				AssertEquals("Initialised Menu Type", typeof(MFIMenu), menu.GetType());
				AssertEquals("Customised MenuItem Count should be 0 - only have custom Consol menu items added in AddCustomMenuItems() method", 0, menu.MenuItems.Count);
			}
		}

		public void TestMenuOnConsolForm()
		{
			using (ConsolFormForActionMenuTesting form = new ConsolFormForActionMenuTesting(Factory.New<ForwardingConsol>()))
			{
				form.ActionMenuItemForTesting.OnPopup(EventArgs.Empty);
				MenuItem dataMenu = GetMenuItem(form.ActionMenuItemForTesting.MenuItems, "&Data");
				AssertNotNull("Should have the data menu item", dataMenu);
				AssertEquals("Data menu's item count should be same as the number added in AddCustomMenuItems() method", 1, dataMenu.MenuItems.Count);
			}
		}

		public void TestMenuOnShipmentForm()
		{
			// using special subclassed form with AllowActionDataMenuItem = true
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			using (ShipmentFormForActionMenuTesting form = new ShipmentFormForActionMenuTesting(Factory.New<ForwardingShipment>()))
			{
				form.ActionMenuItemForTesting.OnPopup(EventArgs.Empty);
				MenuItem dataMenu = GetMenuItem(form.ActionMenuItemForTesting.MenuItems, "&Data");
				AssertNull("Should NOT have the data menu item, even though we've allowed it in the form, because there are no shipment specific menu items to show.", dataMenu);
			}
		}

		public void TestExportWithoutFileExtension()
		{
			using (MFIDataExportFormForTest exportForm = new MFIDataExportFormForTest(new CaroTransFlatFileDataExporter(new BusinessObjectFactory()), new CollectionWrapperBusinessObjectReader(new MainFormConsolCollection(new BusinessObjectFactory()))))
			{
				AssertEquals("", exportForm.GetOutputTextBox.Text);
				exportForm.Export();
				AssertEquals("File extenion has not been set. Export terminated. \r\nPlease set up registry at MFI Client Extensions -> CaroTrans Tracking Export -> Export File Extension.", exportForm.GetOutputTextBox.Text);
			}
		}

		internal class MFIDataExportFormForTest : Enterprise.Client.MFI.GUI.MFIMenu.MFIDataExportForm
		{
			public MFIDataExportFormForTest(FlatFileDataExporter exporter, BusinessObjectReader readerObject) : base(exporter, readerObject)
			{
			}

			public Enterprise.ZArchitecture.ZTextBox GetOutputTextBox
			{
				get
				{
					return OutputTextbox;
				}
			}

			public new void Export()
			{
				base.Export();
			}
		}

		MenuItem GetMenuItem(MenuItem.MenuItemCollection menuItems, string menuItemText)
		{
			MenuItem returnValue = null;
			foreach (MenuItem item in menuItems)
			{
				if (item.Text == menuItemText)
				{
					returnValue = item;
					break;
				}
			}

			return returnValue;
		}

		class ShipmentFormForActionMenuTesting : ShipmentForm
		{
			public ShipmentFormForActionMenuTesting(ForwardingShipment shipment) : base(shipment)
			{
			}

			public MenuItem ActionMenuItemForTesting
			{
				get
				{
					return ActionsMenuItem;
				}
			}

			protected override bool AllowActionDataMenuItem
			{
				get
				{
					return true;
				}
			}
		}

		class ConsolFormForActionMenuTesting : ConsolForm
		{
			public ConsolFormForActionMenuTesting(ForwardingConsol consol) : base(consol)
			{
			}

			public MenuItem ActionMenuItemForTesting
			{
				get
				{
					return ActionsMenuItem;
				}
			}
		}
	}
}
