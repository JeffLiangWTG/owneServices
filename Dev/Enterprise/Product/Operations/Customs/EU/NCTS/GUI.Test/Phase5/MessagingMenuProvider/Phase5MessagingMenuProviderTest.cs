using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class Phase5MessagingMenuProviderTest : TestCaseWithFactory
	{
		public void TestMessageInitiator()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;

			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				var sendMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Send to Customs");

				CombineAssertions(() =>
				{
					AssertEquals("Pre-condition", false, nctsHeader.HasMessageInitiator);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					sendMenuItem.PerformClick();
					AssertEquals("HasMessageInitiator", true, nctsHeader.HasMessageInitiator);
					AssertType<SendsMessagesToCustomsGUI>("MessageInitiator", nctsHeader.MessageInitiator);
				});
			}
		}

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new Phase5MessagingMenuProvider(null));
		}

		public void TestForViewFormAfterMakeArrivalNotificationClick()
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture;

			Factory.Save();

			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				var sendMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Make Arrival Notification for this Departure");
				sendMenuItem.PerformClick();

				CombineAssertions("Arrival Movement Form is shown", () =>
				{
					var arrivalMovementForm = ZFormModaliser.LastFormShownDialogForTest;
					AssertType<Phase5ArrivalMovementForm>(arrivalMovementForm);

					AssertEquals("Arrival Movement Form has correct ControllerID", ControllerIDs.Customs.EU.NctsMovementController, ((Phase5ArrivalMovementForm)arrivalMovementForm).ControllerID);
					ZFormUtilities.ReloadCurrentForm((Phase5ArrivalMovementForm)arrivalMovementForm);
					AssertNotContains("Arrival Movement Form can be reloaded without form type cannot be reloaded error", "This form type cannot be reloaded", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestCreateMenuItems()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			var provider = new Phase5MessagingMenuProvider(nctsHeader);
			var expectedMenuItems = new string[]
			{
				"Send to Customs",
				"Make Arrival Notification for this Departure",
				"-",
				"Inventory Management",
				TSRegisterManagementMenuText,
				"-",
				"Import Entry Lines",
				"Import Invoice Lines",
				"&Copy Previous Goods Item",
				"Lock Customs Declaration",
				"Unlock Customs Declaration"
			};
			AssertContainsExactElementsInExactOrder("Menu Items for Arrival Movement", expectedMenuItems, provider.CreateMenuItems().Select(x => x.Text));
		}

		public void TestRefreshMenu()
		{
			provider.RefreshMenu();
			AssertContainsExactElementsInExactOrder(new[] { "Send to Customs", "-", "Import Invoice Lines" }, menuItems.Where(x => x.Visible).Select(x => x.Text));
		}

		public void TestInventoryManagementMenu()
		{
			// Test visibility (Departure/ arrival/ registry on/off)
			var header = Factory.New<NctsHeader>();
			header.BH_HeaderType = NctsMovementType.Codes.Departure;
			header.Bills.AddNew();
			var provider = new Phase5MessagingMenuProvider(header);
			var menuItems = provider.CreateMenuItems();

			using (NctsConfigurationTestHelper.TemporarilySetConfiguration(Factory, "GetImportBondedWarehouseOrderAvailable", false))
			{
				provider.RefreshMenu();
				AssertContainsExactElementsInExactOrder(new[] { "Send to Customs", "-", "Import Entry Lines", "Import Invoice Lines", "&Copy Previous Goods Item" },
					menuItems.Where(x => x.Visible).Select(x => x.Text));
			}

			using (NctsConfigurationTestHelper.TemporarilySetConfiguration(Factory, "GetImportBondedWarehouseOrderAvailable", true))
			{
				provider.RefreshMenu();
				AssertContainsExactElementsInExactOrder(new[] { "Send to Customs", "-", "Inventory Management", "-", "Import Entry Lines", "Import Invoice Lines", "&Copy Previous Goods Item" }, menuItems.Where(x => x.Visible).Select(x => x.Text));

				header.BH_HeaderType = NctsMovementType.Codes.Arrival;
				provider.RefreshMenu();
				AssertContainsExactElementsInExactOrder(new[] { "Send to Customs", "-", "Import Invoice Lines" },
					menuItems.Where(x => x.Visible).Select(x => x.Text));

				header.BH_HeaderType = NctsMovementType.Codes.Departure;
				header.Bills.DeleteAll();
				provider.RefreshMenu();
				AssertContainsExactElementsInExactOrder(new[] { "Send to Customs", "-", "Import Entry Lines", "Import Invoice Lines", "&Copy Previous Goods Item" },
					menuItems.Where(x => x.Visible).Select(x => x.Text));
			}
		}

		public void TestInventoryManagementMenuItems() => CombineAssertions(() =>
		{
			using (NctsConfigurationTestHelper.TemporarilySetConfiguration(Factory, "GetImportBondedWarehouseOrderAvailable", true))
			using (NctsCustomsDataRegistry.Instance.EnableInventoryManagement.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var header = Factory.New<NctsHeader>();
				header.BH_HeaderType = NctsMovementType.Codes.Departure;
				var provider = new Phase5MessagingMenuProvider(header);
				var menuItems = provider.CreateMenuItems();
				var inventoryManagementMenuItem = menuItems.First(x => x.Text == "Inventory Management");
				AssertHouseConsingnmentMenuItems(provider, header, inventoryManagementMenuItem);
			}
		});

		public void TestTSRegisterManagementHouseConsignmentMenuItems() => CombineAssertions(() =>
		{
			var adtPremises = Factory.New<Integration.Customs.EU.ICusTempStorageRegPremises>();
			adtPremises.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
			adtPremises.SRP_CustomsLocation = "C001";

			using (EUCustomsDataRegistry.Instance.RegisterEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var header = Factory.New<NctsHeader>();
				header.BH_HeaderType = NctsMovementType.Codes.Departure;
				header.MovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "C001";
				var provider = new Phase5MessagingMenuProvider(header);
				var menuItems = provider.CreateMenuItems();
				var tsRegisterManagementMenuItem = menuItems.First(x => x.Text == TSRegisterManagementMenuText);

				provider.RefreshMenu();
				var billItems = tsRegisterManagementMenuItem.MenuItems.OfType<ZMenuItem>().Where(x => x.Visible).Select(x => x.Text);
				AssertContainsExactElementsInExactOrder("No bills", new[] { HouseConsignmentMenuText(1) }, billItems);

				AssertHouseConsingnmentMenuItems(provider, header, tsRegisterManagementMenuItem);
			}
		});

		void AssertHouseConsingnmentMenuItems(Phase5MessagingMenuProvider provider, NctsHeader header, ZMenuItem parentMenuItem)
		{
			header.Bills.AddNew();
			header.Bills.AddNew();
			var bill3 = header.Bills.AddNew();
			provider.RefreshMenu();
			var billItems = parentMenuItem.MenuItems.OfType<ZMenuItem>().Where(x => x.Visible).Select(x => x.Text);
			AssertContainsExactElementsInExactOrder(new[] { "House Consignment 1", "House Consignment 2", "House Consignment 3" }, billItems);

			header.Bills.AddNew();
			header.Bills.Delete(bill3);
			provider.RefreshMenu();
			billItems = parentMenuItem.MenuItems.OfType<ZMenuItem>().Where(x => x.Visible).Select(x => x.Text);
			AssertContainsExactElementsInExactOrder(new[] { "House Consignment 1", "House Consignment 2", "House Consignment 3" }, billItems);
		}

		public void TestInventoryManagementMenuItems_Arrival()
		{
			using (NctsConfigurationTestHelper.TemporarilySetConfiguration(Factory, "GetImportBondedWarehouseOrderAvailable", true))
			using (NctsCustomsDataRegistry.Instance.EnableInventoryManagement.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var header = Factory.New<NctsHeader>();
				header.BH_HeaderType = NctsMovementType.Codes.Arrival;
				var provider = new Phase5MessagingMenuProvider(header);
				var menuItems = provider.CreateMenuItems();
				var inventoryManagementMenuItem = menuItems.First(x => x.Text == "Inventory Management");
				header.Bills.AddNew();
				provider.RefreshMenu();

				AssertEquals("When Arrival no items for bills are added", 0, inventoryManagementMenuItem.MenuItems.OfType<ZMenuItem>().Count());
				AssertEquals("Should not be visible when arrival", false, inventoryManagementMenuItem.Visible);
			}
		}

		[RequiresSTA]
		public void TestImportWarehouseOrder()
		{
			using (NctsConfigurationTestHelper.TemporarilySetConfiguration(Factory,
						"GetImportBondedWarehouseOrderAvailable", true))
			{
				var header = Factory.New<NctsHeader>();
				header.BH_HeaderType = NctsMovementType.Codes.Departure;
				var bill = header.Bills.AddNew();
				bill.B0_ReferenceID = "ref1";
				var provider = new Phase5MessagingMenuProvider(header);
				Mock.Get(header.Configuration).CallBase = true;
				using (var form = new ZForm())
				{
					var formMenuItems = form.Menu.MenuItems;
					provider.RefreshMenu();
					formMenuItems.AddRange(provider.CreateMenuItems().ToArray());
					form.Show();
					var submenu = formMenuItems.OfType<ZMenuItem>().First(x => x.Text == "Inventory Management");
					var billItem = submenu.MenuItems.OfType<ZMenuItem>().Single(x => x.Visible);
					var importItem = billItem.MenuItems.OfType<ZMenuItem>().SingleOrDefault(x => x.Text == "Import Warehouse Order");
					AssertNotNull(importItem);

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
					importItem.PerformClick();
					var lastForm = ZFormModaliser.LastFormShownDialogForTest;
					AssertEquals(typeof(EmbeddedModulePopup), lastForm.GetType());
					AssertEquals(ModuleIDs.WhsOrder, ((EmbeddedModulePopup)lastForm).CurrentModule.ModuleID);

					var bill2 = header.Bills.AddNew();
					var goodsItem2 = bill2.GoodsItems.AddNew();
					goodsItem2.BY_BondedWhsQuantity = 10;
					provider.RefreshMenu();
					billItem = submenu.MenuItems.OfType<ZMenuItem>().Single(x => x.Text == HouseConsignmentMenuText(1));
					var billItem2 = submenu.MenuItems.OfType<ZMenuItem>().Single(x => x.Text == HouseConsignmentMenuText(2));
					AssertEquals("\"Import Warehouse Order\" is hidden when any goodsItem with warehouse info exists", false, billItem.MenuItems.OfType<ZMenuItem>().Any(x => x.Text == "Import Warehouse Order"));
					AssertEquals("\"Import Warehouse Order\" is hidden when any goodsItem with warehouse info exists", false, billItem2.MenuItems.OfType<ZMenuItem>().Any(x => x.Text == "Import Warehouse Order"));

					goodsItem2.BY_BondedWhsQuantity = 0;
					bill.IsOutwardOrderImported = true;
					provider.RefreshMenu();

					billItem = submenu.MenuItems.OfType<ZMenuItem>().Single(x => x.Text == HouseConsignmentMenuText(1));
					billItem2 = submenu.MenuItems.OfType<ZMenuItem>().Single(x => x.Text == HouseConsignmentMenuText(2));
					AssertEquals("\"Import Warehouse Order\" is hidden when IsOutwardOrderImported is true for that bill", false, billItem.MenuItems.OfType<ZMenuItem>().Any(x => x.Text == "Import Warehouse Order"));
					AssertEquals("\"Import Warehouse Order\" is available when another bill has imported an order but current bill has not", true, billItem2.MenuItems.OfType<ZMenuItem>().Any(x => x.Text == "Import Warehouse Order"));
				}
			}
		}

		[RequiresSTA]
		public void TestTSRegisterManagementMenuItem() => CombineAssertions(() =>
		{
			var adtPremises = Factory.New<Integration.Customs.EU.ICusTempStorageRegPremises>();
			adtPremises.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
			adtPremises.SRP_CustomsLocation = "C001";
			var lamPremises = Factory.New<Integration.Customs.EU.ICusTempStorageRegPremises>();
			lamPremises.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility;
			lamPremises.SRP_CustomsLocation = "C002";

			var header = Factory.New<NctsHeader>();
			header.BH_HeaderType = NctsMovementType.Codes.Departure;

			var provider = new Phase5MessagingMenuProvider(header);
			var menuItems = provider.CreateMenuItems();

			AssertMenuItem(true);
			AssertMenuItem(true, registerEnabled: false, registerEnabledDevelopmentOnly: true);
			AssertMenuItem(false, registerEnabled: false);
			AssertMenuItem(false, goodsLocation: "C002");
			AssertMenuItem(false, goodsLocation: "C999");

			void AssertMenuItem(bool expectedMenuItemVisible, bool registerEnabled = true, bool registerEnabledDevelopmentOnly = false, string goodsLocation = "C001", [CallerLineNumber] int line = 0)
			{
				var assertionMessage = $"[{line}] registerEnabled={registerEnabled} registerEnabledForDeveloper={registerEnabledDevelopmentOnly} goodsLocation={goodsLocation}";

				using (EUCustomsDataRegistry.Instance.RegisterEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registerEnabled))
				using (EUCustomsDataRegistry.Instance.RegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registerEnabledDevelopmentOnly))
				{
					header.MovementHeader.GoodsLocation.CGL_AdditionalIdentifier = goodsLocation;
					provider.RefreshMenu();
					var inventoryManagementMenuItem = menuItems.FindByText(TSRegisterManagementMenuText);
					AssertEquals(assertionMessage, expectedMenuItemVisible, inventoryManagementMenuItem.Visible);
				}
			}
		});

		public void TestTSRegisterManagementMenuItem_Arrival()
		{
			var adtPremises = Factory.New<Integration.Customs.EU.ICusTempStorageRegPremises>();
			adtPremises.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
			adtPremises.SRP_CustomsLocation = "C001";
			var lamPremises = Factory.New<Integration.Customs.EU.ICusTempStorageRegPremises>();
			lamPremises.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility;
			lamPremises.SRP_CustomsLocation = "C002";

			var header = Factory.New<NctsHeader>();
			header.BH_HeaderType = NctsMovementType.Codes.Arrival;

			var provider = new Phase5MessagingMenuProvider(header);
			var menuItems = provider.CreateMenuItems();

			using (EUCustomsDataRegistry.Instance.RegisterEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (EUCustomsDataRegistry.Instance.RegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				provider.RefreshMenu();
				var inventoryManagementMenuItem = menuItems.FindByText(TSRegisterManagementMenuText);
				AssertEquals("Not visible for arrival", false, inventoryManagementMenuItem.Visible);
			}
		}

		[RequiresSTA]
		public void TestSelectInventoryTS() => CombineAssertions(() =>
		{
			var adtPremises = Factory.New<Integration.Customs.EU.ICusTempStorageRegPremises>();
			adtPremises.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
			adtPremises.SRP_CustomsLocation = "C001";

			using (EUCustomsDataRegistry.Instance.RegisterEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var header = Factory.New<NctsHeader>();
				header.BH_HeaderType = NctsMovementType.Codes.Departure;
				header.MovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "C001";

				var provider = new Phase5MessagingMenuProvider(header);
				var menuItems = provider.CreateMenuItems();

				provider.RefreshMenu();
				var inventoryManagementMenuItem1 = menuItems.FindByText(TSRegisterManagementMenuText);
				var houseConsignmentMenuItem1 = inventoryManagementMenuItem1.MenuItems.FindByText(HouseConsignmentMenuText(1));
				var selectInventoryMenuItem1 = houseConsignmentMenuItem1.MenuItems.FindByText(SelectInventoryMenuText);
				AssertNotNull("No bills: MenuItem", selectInventoryMenuItem1 != null);
				AssertEquals("No bills: Tag", ZGuid.Empty, selectInventoryMenuItem1.Tag);

				var bill = header.Bills.AddNew();

				provider.RefreshMenu();
				var inventoryManagementMenuItem2 = menuItems.FindByText(TSRegisterManagementMenuText);
				var houseConsignmentMenuItem2 = inventoryManagementMenuItem2.MenuItems.FindByText(HouseConsignmentMenuText(1));
				var selectInventoryMenuItem2 = houseConsignmentMenuItem2.MenuItems.FindByText(SelectInventoryMenuText);
				AssertNotNull("With bills: MenuItem", selectInventoryMenuItem2);
				AssertEquals("With bills: Tag", bill.PK, selectInventoryMenuItem2.Tag);
			}
		});

		[RequiresSTA]
		public void TestSelectInventoryWHS()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_HeaderType = NctsMovementType.Codes.Departure;
			var bill = header.Bills.AddNew();
			bill.B0_ReferenceID = "ref1";

			var provider = new Phase5MessagingMenuProvider(header);
			using (var form = new ZForm())
			{
				var formMenuItems = form.Menu.MenuItems;
				formMenuItems.AddRange(provider.CreateMenuItems().ToArray());
				var submenu = formMenuItems.OfType<ZMenuItem>().First(x => x.Text == "Inventory Management");
				using (NctsCustomsDataRegistry.Instance.EnableInventoryManagement.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					var header1 = Factory.NewWithValidTestData<OrgHeader>();
					header1.OH_Code = "WH1";
					header1.OH_IsWarehouseClient = true;
					header1.OrganisationTypes = OrganisationTypes.WarehouseClient;
					var address = header1.Addresses.AddNew();
					address.OA_Address1 = "ADD1";
					address.AddAddressType(OrgAddressType.Office);
					bill.Header.MovementHeader.BM_OA_WarehouseAddress = address.PK;

					provider.RefreshMenu();
					var billItem = submenu.MenuItems.OfType<ZMenuItem>().Single(x => x.Visible);
					AssertEquals("Precondition", (ZBool)false, bill.IsOutwardOrderImported);
					var selectInventoryItem = billItem.MenuItems.OfType<ZMenuItem>().SingleOrDefault(x => x.Text == "Select Inventory");
					AssertNotNull(selectInventoryItem);
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
					selectInventoryItem.PerformClick();
					var lastForm = ZFormModaliser.LastFormShownDialogForTest;
					AssertEquals(typeof(InventorySelectionForm), lastForm.GetType());

					bill.IsOutwardOrderImported = true;
					provider.RefreshMenu();
					billItem = submenu.MenuItems.OfType<ZMenuItem>().Single(x => x.Visible);
					AssertEquals("\"Select Inventory\" is hidden when IsOutwardOrderImported = True", false, billItem.MenuItems.OfType<ZMenuItem>().Any(x => x.Text == "Select Inventory"));

					bill.IsOutwardOrderImported = false;
					header.MovementHeader.BM_OA_WarehouseAddress = ZGuid.Empty;
					provider.RefreshMenu();
					billItem = submenu.MenuItems.OfType<ZMenuItem>().Single(x => x.Visible);
					AssertEquals("\"Select Inventory\" is hidden when EnableInventoryManagement = True and FromWarehouseAddress is Empty", false, billItem.MenuItems.OfType<ZMenuItem>().Any(x => x.Text == "Select Inventory"));

					bill.Header.MovementHeader.BM_OA_WarehouseAddress = address.PK;
					var secondBill = header.Bills.AddNew();
					secondBill.IsOutwardOrderImported = true;

					provider.RefreshMenu();
					billItem = submenu.MenuItems.OfType<ZMenuItem>().Single(x => x.Text == HouseConsignmentMenuText(1));
					var billItem2 = submenu.MenuItems.OfType<ZMenuItem>().Single(x => x.Text == HouseConsignmentMenuText(2));

					AssertEquals("\"Select Inventory\" is hidden if Any bill has IsOutwardOrderImported = true", false, billItem.MenuItems.OfType<ZMenuItem>().Any(x => x.Text == "Select Inventory"));
					AssertEquals("\"Select Inventory\" is hidden if Any bill has IsOutwardOrderImported = true", false, billItem2.MenuItems.OfType<ZMenuItem>().Any(x => x.Text == "Select Inventory"));

					secondBill.Delete();
				}

				using (NctsCustomsDataRegistry.Instance.EnableInventoryManagement.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					bill.IsOutwardOrderImported = false;
					provider.RefreshMenu();
					var billItem = submenu.MenuItems.OfType<ZMenuItem>().Single(x => x.Visible);
					AssertEquals("\"Select Inventory\" is hidden when EnableInventoryManagement = False", false, billItem.MenuItems.OfType<ZMenuItem>().Any(x => x.Text == "Select Inventory"));
				}
			}
		}

		public void TestCancelInventory()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_HeaderType = NctsMovementType.Codes.Departure;
			var bill = header.Bills.AddNew();
			bill.B0_ReferenceID = "ref1";

			var warehouseIntegrationSupporter = (IWarehouseIntegrationSupporter)header;
			var provider = new Phase5MessagingMenuProvider(header);
			using (var form = new ZForm())
			{
				var formMenuItems = form.Menu.MenuItems;
				provider.RefreshMenu();
				formMenuItems.AddRange(provider.CreateMenuItems().ToArray());
				form.Show();
				var submenu = formMenuItems.OfType<ZMenuItem>().Single(x => x.Text == "Inventory Management");
				var houseConsignmentItem = submenu.MenuItems.OfType<ZMenuItem>().Single(x => x.Text == HouseConsignmentMenuText(1));
				var houseConsignmentMenuItems = houseConsignmentItem.MenuItems.OfType<ZMenuItem>();

				AssertNullOrEmpty("precondition", warehouseIntegrationSupporter.WarehouseTransactionStatus);
				AssertEquals(false, houseConsignmentMenuItems.Any(mi => mi.Text == "Cancel Inventory"));

				warehouseIntegrationSupporter.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
				provider.RefreshMenu();
				houseConsignmentItem = submenu.MenuItems.OfType<ZMenuItem>().Single(x => x.Text == HouseConsignmentMenuText(1));
				houseConsignmentMenuItems = houseConsignmentItem.MenuItems.OfType<ZMenuItem>();
				var cancelInventoryItem = houseConsignmentMenuItems.SingleOrDefault(x => x.Text == "Cancel Inventory");
				AssertNotNull(cancelInventoryItem);

				cancelInventoryItem.PerformClick();

				Assert(warehouseIntegrationSupporter.Logs.HasLogWith(l => l.Event.SE_Code == Events.CancelTheWarehouseJob.Code));
			}
		}

		public void TestImportEntryLinesMenuItemVisibility()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			var provider = new Phase5MessagingMenuProvider(nctsHeader);
			var menuItems = provider.CreateMenuItems();

			CombineAssertions(() =>
			{
				nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
				provider.RefreshMenu();
				AssertImportEntryLinesVisibility(expectedVisibility: false);

				nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
				provider.RefreshMenu();
				AssertImportEntryLinesVisibility(expectedVisibility: true);
			});

			void AssertImportEntryLinesVisibility(bool expectedVisibility)
			{
				var importEntryLinesMenu = menuItems.Single(x => x.Text == "Import Entry Lines");
				AssertEquals("'Import Entry Lines' Menu Item Visible", expectedVisibility, importEntryLinesMenu.Visible);
			}
		}

		public void TestImportInvoiceLinesMenuItems()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_HeaderType = NctsMovementType.Codes.Arrival;
			header.Bills.AddNew();
			var provider = new Phase5MessagingMenuProvider(header);
			var menuItems = provider.CreateMenuItems();
			provider.RefreshMenu();
			AssertContainsExactElementsInExactOrder(new[] { "Send to Customs", "-", "Import Invoice Lines" }, menuItems.Where(x => x.Visible).Select(x => x.Text));

			header.BH_HeaderType = NctsMovementType.Codes.Departure;
			provider.RefreshMenu();
			AssertContainsExactElementsInExactOrder(new[] { "Send to Customs", "-", "Inventory Management", "-", "Import Entry Lines", "Import Invoice Lines", "&Copy Previous Goods Item" }, menuItems.Where(x => x.Visible).Select(x => x.Text));

			header.Bills.AddNew();
			provider.RefreshMenu();
			AssertContainsExactElementsInExactOrder(new[] { "Send to Customs", "-", "Inventory Management", "-", "Import Entry Lines", "Import Invoice Lines", "&Copy Previous Goods Item" }, menuItems.Where(x => x.Visible).Select(x => x.Text));

			var submenu = menuItems.First(x => x.Text == "Import Invoice Lines");
			var submenuitems = submenu.MenuItems.OfType<ZMenuItem>().Where(x => x.Visible).Select(x => x.Text);
			var expected4 = Enumerable.Range(1, 4).Select(i => string.Format("Import to House Consignment {0}", i));
			AssertContainsExactElementsInExactOrder("Import Invoice Lines MenuItems", expected4.Take(2), submenuitems);

			header.Bills.AddNew();
			header.Bills.AddNew();
			provider.RefreshMenu();
			AssertContainsExactElementsInExactOrder("Import Invoice Lines MenuItems", expected4, submenuitems);

			header.Bills.DeleteAll();
			header.Bills.AddNew();
			header.Bills.AddNew();
			header.Bills.AddNew();
			provider.RefreshMenu();
			AssertContainsExactElementsInExactOrder("Import Invoice Lines MenuItems", expected4.Take(3), submenuitems);
		}

		[RequiresSTA]
		public void TestImportInvoiceLinesMenu()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_HeaderType = NctsMovementType.Codes.Departure;
			header.Bills.AddNew();
			var provider = new Phase5MessagingMenuProvider(header);
			using (var form = new ZForm())
			{
				var formMenuItems = form.Menu.MenuItems;
				provider.RefreshMenu();
				formMenuItems.AddRange(provider.CreateMenuItems().ToArray());
				form.Show();
				var menuItem = formMenuItems.FindByText("Import Invoice Lines");
				AssertNotNull(menuItem);
				menuItem.PerformClick();
				var lastFormShown = ZFormModaliser.LastFormShownForTest;
				AssertType<EmbeddedModulePopup>("LastFormShown type", lastFormShown);
				AssertEquals("Customs Entries", lastFormShown.Text);

				header.Bills.AddNew();
				provider.RefreshMenu();
				AssertEquals(2, menuItem.MenuItems.Count);
				menuItem.MenuItems[0].PerformClick();
				lastFormShown = ZFormModaliser.LastFormShownForTest;
				AssertType<EmbeddedModulePopup>("LastFormShown type", lastFormShown);
				AssertEquals("Customs Entries", lastFormShown.Text);
			}
		}

		[RequiresSTA]
		public void TestImportEntryLinesMenu()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.Bills.AddNew();
			var provider = new Phase5MessagingMenuProvider(nctsHeader);
			using (var form = new ZForm())
			{
				var formMenuItems = form.Menu.MenuItems;
				provider.RefreshMenu();
				formMenuItems.AddRange(provider.CreateMenuItems().ToArray());
				form.Show();

				var importEntryLinesMenuItem = formMenuItems.FindByText("Import Entry Lines");
				AssertNotNull("'Import Entry Lines' Menu Item", importEntryLinesMenuItem);
				importEntryLinesMenuItem.PerformClick();
				var lastFormShown = ZFormModaliser.LastFormShownForTest;
				AssertType<EmbeddedModulePopup>("LastFormShown type", lastFormShown);
				AssertEquals("LastFormShown Text", "Customs Entries", lastFormShown.Text);

				nctsHeader.Bills.AddNew();
				provider.RefreshMenu();
				AssertContainsExactElementsInExactOrder(
					"'Import Entry Lines' SubMenu Items",
					new[] { "Import to House Consignment 1", "Import to House Consignment 2" },
					importEntryLinesMenuItem.MenuItems.Cast<MenuItem>().Select(x => x.Text));
				importEntryLinesMenuItem.MenuItems[0].PerformClick();
				lastFormShown = ZFormModaliser.LastFormShownForTest;
				AssertType<EmbeddedModulePopup>("LastFormShown type", lastFormShown);
				AssertEquals("LastFormShown Text", "Customs Entries", lastFormShown.Text);
			}
		}

		public void TestGetProvider()
		{
			var header = Factory.New<NctsHeader>();
			var nctsMessagingMenuProviders = new KeyObjectHandleDictionaryObject
			{
				{ Core.Constants.CountryCodes.Latvia, new TestObjectHandle(new Phase5MessagingMenuProviderForTest(header)) }
			};

			using (ObjectFactory.Substitute("NCTSMessagingMenuProviders", nctsMessagingMenuProviders))
			{
				AssertType<Phase5MessagingMenuProviderForTest>(Phase5MessagingMenuProvider.GetProvider(header));
			}
		}

		public void TestGetProvider_Default()
		{
			AssertType<Phase5MessagingMenuProvider>(Phase5MessagingMenuProvider.GetProvider(Factory.New<NctsHeader>()));
		}

		public void TestPreSaveInvoked()
		{
			const string messageSaveConfirmation = "The Job has not yet been saved. Do you want to save and proceed?";

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;

			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				var sendMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Send to Customs");

				CombineAssertions(() =>
				{
					AssertEquals("Pre-condition not saved", true, nctsHeader.HasChanges);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					sendMenuItem.PerformClick();
					AssertEquals("Message for deny save", messageSaveConfirmation, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Save denied", true, nctsHeader.HasChanges);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					sendMenuItem.PerformClick();
					AssertEquals("Message for confirm save", messageSaveConfirmation, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Save confirmed", false, nctsHeader.HasChanges);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					sendMenuItem.PerformClick();
					AssertNotEquals("No message when no changes", messageSaveConfirmation, UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestSendToCustoms_SetLocalDepartureReferenceNumber_UseLocalReferenceNumberIgnoreInDatabaseCheckCore_Disabled()
		{
			AssertSendToCustoms_SetLocalReferenceNumber_UseLocalReferenceNumberIgnoreInDatabaseCheckCore(NctsMovementType.Codes.Departure, false);
		}

		public void TestSendToCustoms_SetLocalDepartureReferenceNumber_UseLocalReferenceNumberIgnoreInDatabaseCheckCore_Enabled()
		{
			AssertSendToCustoms_SetLocalReferenceNumber_UseLocalReferenceNumberIgnoreInDatabaseCheckCore(NctsMovementType.Codes.Departure, true);
		}

		public void TestSendToCustoms_SetLocalArrivalReferenceNumber_UseLocalReferenceNumberIgnoreInDatabaseCheckCore_Disabled()
		{
			AssertSendToCustoms_SetLocalReferenceNumber_UseLocalReferenceNumberIgnoreInDatabaseCheckCore(NctsMovementType.Codes.Arrival, false);
		}

		public void TestSendToCustoms_SetLocalArrivalReferenceNumber_UseLocalReferenceNumberIgnoreInDatabaseCheckCore_Enabled()
		{
			AssertSendToCustoms_SetLocalReferenceNumber_UseLocalReferenceNumberIgnoreInDatabaseCheckCore(NctsMovementType.Codes.Arrival, true);
		}

		void AssertSendToCustoms_SetLocalReferenceNumber_UseLocalReferenceNumberIgnoreInDatabaseCheckCore(ZString headerType, bool useLocalReferenceNumberIgnoreInDatabaseCheckCore)
		{
			nctsHeader.BH_HeaderType = headerType;
			var nctsMovementForm = nctsHeader.IsArrivalMovement ? ((ZTemplateForm)new Phase5ArrivalMovementForm(nctsHeader)) : new Phase5DepartureMovementForm(nctsHeader);

			using (Registry.EUCustomsDataRegistry.Instance.NctsIsManualDepartureCustomerReferenceEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (Registry.EUCustomsDataRegistry.Instance.NctsIsManualArrivalCustomerReferenceEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (NctsConfigurationTestHelper.TemporarilySetUseLocalReferenceNumberIgnoreInDatabaseCheckCore(Factory, useLocalReferenceNumberIgnoreInDatabaseCheckCore))
			using (nctsMovementForm)
			{
				var sendMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Send to Customs");

				CombineAssertions(() =>
				{
					var movementHeader = nctsHeader.IsArrivalMovement ? ((NctsCommonMovementHeader)nctsHeader.ArrivalMovementHeader) : nctsHeader.MovementHeader;
					AssertEquals("BM_PaperlessInbondNum empty", ZString.Empty, movementHeader.BM_PaperlessInbondNum);
					sendMenuItem.PerformClick();
					AssertEquals("BM_PaperlessInbondNum empty after PerformClick", ZString.Empty, movementHeader.BM_PaperlessInbondNum);
					movementHeader.HeaderBranch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "BE1230789654", "BE");
					sendMenuItem.PerformClick();
					if (useLocalReferenceNumberIgnoreInDatabaseCheckCore)
					{
						AssertNotEquals("Eori configured: BM_PaperlessInbondNum not empty PerformClick", ZString.Empty, movementHeader.BM_PaperlessInbondNum);
					}
					else
					{
						AssertEquals("Eori configured: BM_PaperlessInbondNum empty PerformClick", ZString.Empty, movementHeader.BM_PaperlessInbondNum);
					}
				});
			}
		}

		public void TestCanMakeArrivalNotification_Arrival()
		{
			var menuItem = menuItems.FirstOrDefault(x => x.Caption.ToString() == "Make Arrival Notification for this Departure");
			TestCanMakeArrivalNotification(false, menuItem, EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		}

		public void TestCanMakeArrivalNotification_Departure()
		{
			var menuItem = menuItems.FirstOrDefault(x => x.Caption.ToString() == "Make Arrival Notification for this Departure");
			CombineAssertions(() =>
			{
				TestCanMakeArrivalNotification(false, menuItem, EU.NCTS.Business.NctsMovementType.Codes.Departure);

				var transitStatusCodeList = new NctsTransitStatusList().GetAllCodes();
				foreach (var transitStatusCode in transitStatusCodeList)
				{
					TestCanMakeArrivalNotification(transitStatusCode == NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture || transitStatusCode == NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit, menuItem, EU.NCTS.Business.NctsMovementType.Codes.Departure, transitStatusCode);
				}
			});
		}

		public void TestMakeArrivalNotificationClick()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationNotSent;
			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "123";
			nctsHeader.DestinationCustomsOfficeCodeForDeparture = "ABC001";

			Factory.Save();

			var numberOfNctsHeadersBeforeClick = Factory.Load<NctsHeader>(new ZQuery(CusInBondHeaderSchema.BH_HeaderType, "A")).Length;

			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				var sendMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Make Arrival Notification for this Departure");
				sendMenuItem.PerformClick();

				CombineAssertions(() =>
				{
					var numberOfNctsHeadersAfterClick = Factory.Load<NctsHeader>(new ZQuery(CusInBondHeaderSchema.BH_HeaderType, "A")).Length;
					AssertEquals("A new arrival notification should have been created", numberOfNctsHeadersBeforeClick + 1, numberOfNctsHeadersAfterClick);

					sendMenuItem.PerformClick();
					AssertEquals("Message when the arrival notification cannot be created", "NCTSP5 arrival for MRN 123 already generated.", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestGetNctsHeaderGenerator()
		{
			var testProvider = new Phase5MessagingMenuProviderForTest(nctsHeader);
			AssertType<NctsHeaderGenerator>(testProvider.GetNctsHeaderGeneratorForTest());
		}

		void TestCanMakeArrivalNotification(bool expected, ZMenuItem menuItem, string headerType, string customsStatus = "")
		{
			nctsHeader.BH_HeaderType = headerType;
			var commonMovement = nctsHeader.IsDepartureMovement ? nctsHeader.MovementHeader : (NctsCommonMovementHeader)nctsHeader.ArrivalMovementHeader;
			commonMovement.BM_CustomsStatus = customsStatus;
			provider.RefreshMenu();
			AssertEquals($"headerType: {headerType}, customsStatus: {customsStatus}", expected, menuItem?.Visible);
		}

		public void LockUnlockCustomsDeclarationMenuItem_NotAllowedForUser() => CombineAssertions(() =>
		{
			var typeValue = "ARN";
			var tabValue = "ARN";
			CreateDeclarationLockConfig(typeValue, tabValue);

			var menuItemLock = menuItems.FindByText("Lock Customs Declaration");
			var menuItemUnlock = menuItems.FindByText("Unlock Customs Declaration");

			using (CustomsDataRegistry.Instance.DeclarationLockForEdit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, declarationLockConfigCollection))
			{
				nctsHeader.Logs.AddNew(Events.CustomsEntryStatus, "EVENTREF" + typeValue);

				Env.Security.EuNctsLockOrUnlockFileForEdit.IsAllowed = false;
				nctsHeader.LockFile("Lock file");
				AssertLockUnlockDeclarationMenuItem("Lock Customs Declaration visible, LCK event, not allowed for user", false, menuItemLock);
				AssertLockUnlockDeclarationMenuItem("Unlock Customs Declaration visible, LCK event, not allowed for user", false, menuItemUnlock);

				nctsHeader.UnlockFile("Unlock file");
				AssertLockUnlockDeclarationMenuItem("Lock Customs Declaration visible, UCK event, not allowed for user", false, menuItemLock);
				AssertLockUnlockDeclarationMenuItem("Unlock Customs Declaration visible, UCK event, not allowed for user", false, menuItemUnlock);

				Env.Security.EuNctsLockOrUnlockFileForEdit.IsAllowed = true;
				nctsHeader.LockFile("Lock file");
				AssertLockUnlockDeclarationMenuItem("Lock Customs Declaration visible, LCK event, allowed for user", false, menuItemLock);
				AssertLockUnlockDeclarationMenuItem("Unlock Customs Declaration visible, LCK event, allowed for user", true, menuItemUnlock);

				nctsHeader.UnlockFile("Unlock file");
				AssertLockUnlockDeclarationMenuItem("Lock Customs Declaration visible, UCK event, allowed for user", true, menuItemLock);
				AssertLockUnlockDeclarationMenuItem("Unlock Customs Declaration visible, UCK event, allowed for user", false, menuItemUnlock);
			}
		});

		public void TestLockCustomsDeclarationMenuItem()
		{
			var lockMenuItem = menuItems.FindByText("Lock Customs Declaration");
			AssertEquals("Tag should be LCK", Events.LockForEditCode, lockMenuItem.Tag);
		}

		public void TestUnLockCustomsDeclarationMenuItem()
		{
			var unlockMenuItem = menuItems.FindByText("Unlock Customs Declaration");
			AssertEquals("Tag should be UCK", Events.UnlockForEditCode, unlockMenuItem.Tag);
		}

		public void TestLockUnlockCustomsDeclarationMenuItem_Arrival_ARN()
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			LockUnlockCustomsDeclarationMenuItem("ARN", "ARN");
		}

		public void TestLockUnlockCustomsDeclarationMenuItem_Arrival_ULR()
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			LockUnlockCustomsDeclarationMenuItem("ULR", "ULR");
		}

		public void TestLockUnlockCustomsDeclarationMenuItem_Departure()
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			LockUnlockCustomsDeclarationMenuItem("DEP", "HDR");
		}

		public void LockUnlockCustomsDeclarationMenuItem(string typeValue, string tabValue) => CombineAssertions(typeValue + " - " + tabValue, () =>
		{
			Env.Security.EuNctsLockOrUnlockFileForEdit.IsAllowed = true;
			CreateDeclarationLockConfig(typeValue, tabValue);

			var menuItemLock = menuItems.FindByText("Lock Customs Declaration");
			var menuItemUnlock = menuItems.FindByText("Unlock Customs Declaration");

			AssertLockUnlockDeclarationMenuItem("Lock Customs Declaration visible", false, menuItemLock);
			AssertLockUnlockDeclarationMenuItem("Unlock Customs Declaration visible", false, menuItemUnlock);

			using (CustomsDataRegistry.Instance.DeclarationLockForEdit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, declarationLockConfigCollection))
			{
				AssertLockUnlockDeclarationMenuItem("Lock Customs Declaration visible, no CES event", false, menuItemLock);
				AssertLockUnlockDeclarationMenuItem("Unlock Customs Declaration visible, no CES event", false, menuItemUnlock);

				nctsHeader.Logs.AddNew(Events.CustomsEntryStatus, "EVENTREF" + typeValue);
				AssertLockUnlockDeclarationMenuItem("Lock Customs Declaration visible, no LCK event", true, menuItemLock);
				AssertLockUnlockDeclarationMenuItem("Unlock Customs Declaration visible, no LCK event", false, menuItemUnlock);

				nctsHeader.LockFile("Lock file");
				AssertLockUnlockDeclarationMenuItem("Lock Customs Declaration visible, LCK event", false, menuItemLock);
				AssertLockUnlockDeclarationMenuItem("Unlock Customs Declaration visible, LCK event", true, menuItemUnlock);

				nctsHeader.UnlockFile("Unlock file");
				AssertLockUnlockDeclarationMenuItem("Lock Customs Declaration visible, UCK event", true, menuItemLock);
				AssertLockUnlockDeclarationMenuItem("Unlock Customs Declaration visible, UCK event", false, menuItemUnlock);
			}

			declarationLockConfigCollection[0].EventInfos[0].EventReference = "BADEVENTREF";
			using (CustomsDataRegistry.Instance.DeclarationLockForEdit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, declarationLockConfigCollection))
			{
				AssertLockUnlockDeclarationMenuItem("Lock Customs Declaration visible, no matching EventReference", false, menuItemLock);
				AssertLockUnlockDeclarationMenuItem("Unlock Customs Declaration visible, no matching EventReference", false, menuItemUnlock);
			}
		});

		void AssertLockUnlockDeclarationMenuItem(string message, bool expected, MenuItem menuItem)
		{
			provider.RefreshMenu();
			AssertEquals(message, expected, menuItem.Visible);
		}

		public void TestLockOrUnlockCustomsFileMenuItems_Click()
		{
			var collection = new DeclarationLockConfigCollection(null, Factory);

			var config = collection.AddNew();
			config.DeclarationType = MessageTypeList.Codes.NctsArrivalNotification;

			var tabInfo = config.TabInfos.AddNew();
			tabInfo.TabPage = Core.Constants.Customs.DeclarationTabPages.Codes.NctsArrivalNotification;

			using (CustomsDataRegistry.Instance.DeclarationLockForEdit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var declaration = Factory.New<NctsHeader>();
				declaration.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				declaration.SetMovementType(NctsMovementType.Codes.Arrival);

				((ICustomsFileParent)declaration).DeclarationTypeInfo.SetValueFromString(NctsMovementType.Codes.Arrival);
				declaration.Logs.RemoveAndDeleteAll();
				declaration.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks;
				declaration.Factory.Save();

				using (var form = new Phase5ArrivalMovementForm(declaration))
				{
					form.Show();
					Application.DoEvents();

					var nctsMenu = form.Menu.MenuItems.FindByText("&NCTS");
					var lockCustomsFileMenuItem = nctsMenu.MenuItems.FindByText("Lock Customs Declaration");
					var unlockCustomsFileMenuItem = nctsMenu.MenuItems.FindByText("Unlock Customs Declaration");

					lockCustomsFileMenuItem.PerformClick();
					AssertType<CustomsWriteToLogForm>(ZFormModaliser.LastFormShownDialogForTest);

					declaration.Factory.Save();

					unlockCustomsFileMenuItem.PerformClick();
					AssertType<CustomsWriteToLogForm>(ZFormModaliser.LastFormShownDialogForTest);
				}
			}
		}

		public void TestWriteLockLog()
		{
			var messagingMenuProvider = new Phase5MessagingMenuProviderForTest(nctsHeader);
			messagingMenuProvider.WriteLockLog_Exposed(new BusinessObject[] { nctsHeader }, "TestReference");

			CombineAssertions(() =>
			{
				var log = nctsHeader.Logs.Find(c => c.SL_SE_NKEvent == AutoEvents.LockForEditCode).First();
				Assert("Should contain the active LCK event", !log.IsCancelled);
				AssertEquals("Event should contain the reference", "TestReference", log.SL_Reference);
			});
		}

		public void TestWriteUnlockLog()
		{
			var messagingMenuProvider = new Phase5MessagingMenuProviderForTest(nctsHeader);
			messagingMenuProvider.WriteUnlockLog_Exposed(new BusinessObject[] { nctsHeader }, "TestReference");

			CombineAssertions(() =>
			{
				var log = nctsHeader.Logs.Find(c => c.SL_SE_NKEvent == AutoEvents.UnlockForEditCode).First();
				Assert("Should contain the active UCK event", !log.IsCancelled);
				AssertEquals("Event should contain the reference", "TestReference", log.SL_Reference);
			});
		}

		public void TestCopyPreviousGoodsLine_DefaultTrue()
		{
			using (CustomsDataRegistry.Instance.AlwaysCopyFromPreviousLine.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				using (var form = new ZForm())
				{
					nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
					var bill = nctsHeader.Bills.AddNew();
					var goodsItems = bill.GoodsItems;

					var menu = new Phase5NctsMessagingMenuItem();
					menu.NctsHeader = nctsHeader;
					form.Menu.MenuItems.Add(menu);
					form.Show();
					var copyPreviousGoodsItemMenuItem = form.Menu.MenuItems.FindByText("&Copy Previous Goods Item", true);
					CombineAssertions("Default is set to TRUE in registry", () =>
					{
						AssertNotNull(copyPreviousGoodsItemMenuItem);
						AssertEquals("Menu item 'Copy Previous Goods Item' is Visible", true, copyPreviousGoodsItemMenuItem.Visible);
						AssertEquals("Menu item is default checked", true, copyPreviousGoodsItemMenuItem.Checked);
						AssertEquals("Flag in bill is default checked", true, bill.B0_CopyLastGoodsLineToNewLines);
						AssertEquals("Flag in goodsItems-collection is checked", true, goodsItems.CopyLastGoodsItemToNewLines);
						copyPreviousGoodsItemMenuItem.PerformClick();
						AssertEquals("Menu item is unchecked after click menu item", false, copyPreviousGoodsItemMenuItem.Checked);
						AssertEquals("Flag in bill is unchecked after click menu item", false, bill.B0_CopyLastGoodsLineToNewLines);
						AssertEquals("Flag in goodsItems-collection is unchecked after click menu item", false, goodsItems.CopyLastGoodsItemToNewLines);
						copyPreviousGoodsItemMenuItem.PerformClick();
						AssertEquals("Menu item is checked after click menu item again", true, copyPreviousGoodsItemMenuItem.Checked);
						AssertEquals("Flag in bill is checked after click menu item again", true, bill.B0_CopyLastGoodsLineToNewLines);
						AssertEquals("Flag in goodsItems-collection is checked after click menu item again", true, goodsItems.CopyLastGoodsItemToNewLines);
					});
				}
			}
		}

		public void TestCopyPreviousGoodsLine_DefaultFalse()
		{
			using (CustomsDataRegistry.Instance.AlwaysCopyFromPreviousLine.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				using (var form = new ZForm())
				{
					nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
					var bill = nctsHeader.Bills.AddNew();
					var goodsItems = bill.GoodsItems;

					var menu = new Phase5NctsMessagingMenuItem();
					menu.NctsHeader = nctsHeader;
					form.Menu.MenuItems.Add(menu);
					form.Show();
					var copyPreviousGoodsItemMenuItem = form.Menu.MenuItems.FindByText("&Copy Previous Goods Item", true);
					CombineAssertions("Default is set to FALSE in registry", () =>
					{
						AssertNotNull(copyPreviousGoodsItemMenuItem);
						AssertEquals("Menu item 'Copy Previous Goods Item' is Visible", true, copyPreviousGoodsItemMenuItem.Visible);
						AssertEquals("Menu item is default unchecked", false, copyPreviousGoodsItemMenuItem.Checked);
						AssertEquals("Flag in bill is default unchecked", false, bill.B0_CopyLastGoodsLineToNewLines);
						AssertEquals("Flag in goodsItems-collection is unchecked", false, goodsItems.CopyLastGoodsItemToNewLines);
						copyPreviousGoodsItemMenuItem.PerformClick();
						AssertEquals("Menu item is checked after click menu item", true, copyPreviousGoodsItemMenuItem.Checked);
						AssertEquals("Flag in bill is checked after click menu item", true, bill.B0_CopyLastGoodsLineToNewLines);
						AssertEquals("Flag in goodsItems-collection is checked after click menu item", true, goodsItems.CopyLastGoodsItemToNewLines);
						copyPreviousGoodsItemMenuItem.PerformClick();
						AssertEquals("Menu item is unchecked after click menu item again", false, copyPreviousGoodsItemMenuItem.Checked);
						AssertEquals("Flag in bill is unchecked after click menu item again", false, bill.B0_CopyLastGoodsLineToNewLines);
						AssertEquals("Flag in goodsItems-collection is unchecked after click menu item again", false, goodsItems.CopyLastGoodsItemToNewLines);
					});
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			provider = new Phase5MessagingMenuProvider(nctsHeader);
			menuItems = provider.CreateMenuItems();
		}

		NctsHeader nctsHeader;
		Phase5MessagingMenuProvider provider;
		IEnumerable<ZMenuItem> menuItems;
		DeclarationLockConfigCollection declarationLockConfigCollection;

		void CreateDeclarationLockConfig(string typeValue, string tabValue)
		{
			if (declarationLockConfigCollection == null)
			{
				declarationLockConfigCollection = new DeclarationLockConfigCollection(null, Factory);
			}
			var config = declarationLockConfigCollection.AddNew();
			config.DeclarationType = typeValue;
			var tabInfo = config.TabInfos.AddNew();
			tabInfo.TabPage = tabValue;
			var eventInfo = config.EventInfos.AddNew();
			eventInfo.EventType = Events.CustomsEntryStatusCode;
			eventInfo.EventReference = "EVENTREF" + typeValue;
			eventInfo.EventSource = Core.Constants.Customs.EventLockSourceTypes.Codes.NctsHeader;
		}

		sealed class Phase5MessagingMenuProviderForTest : Phase5MessagingMenuProvider
		{
			public Phase5MessagingMenuProviderForTest(NctsHeader header)
				: base(header)
			{
			}

			public NctsHeaderGenerator GetNctsHeaderGeneratorForTest() => GetNctsHeaderGenerator();

			public void WriteLockLog_Exposed(BusinessObject[] businessObjects, ZString reference) => WriteLockLog(businessObjects, reference, true);

			public void WriteUnlockLog_Exposed(BusinessObject[] businessObjects, ZString reference) => WriteLockLog(businessObjects, reference, false);
		}

		const string TSRegisterManagementMenuText = "TS Register Management";
		const string SelectInventoryMenuText = "Select Inventory";

		static string HouseConsignmentMenuText(ZShort sequenceNumber) => $"House Consignment {sequenceNumber}";
	}
}
