using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.GUI;
using Enterprise.Customs.DE.Registry;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Module.Testing
{
	[TestedType(typeof(JobDeclarationModule))]
	sealed class JobDeclarationModuleTest : EU.Module.Testing.JobDeclarationModuleTest
	{
		public void TestDerivesFromEu()
		{
			using (var module = new JobDeclarationModule())
			{
				AssertEquals("Derives from EU module", true, module.GetType().IsSubclassOf(typeof(EU.Module.JobDeclarationModule)));
			}
		}

		public void TestUsesDEFilterBusinessObject()
		{
			using (var module = new JobDeclarationModule())
			{
				AssertType<JobDeclarationFilterBusinessObject>(module.FilterBusinessObject);
			}
		}

		public void TestGetNewFilterControl()
		{
			using (var module = new JobDeclarationModule())
			using (var filterControl = module.GetNewFilterControlForGrid())
			{
				AssertType<JobDeclarationFilterStripControl>("FilterControl Type", filterControl);
			}
		}

		public void TestActionMenuItem_CreateDeclarationFromInventory_Visibility()
		{
			const string declarationFromInventoryName = "Create Declaration from Inventory";

			using (var module = new JobDeclarationModule())
			{
				var actionMenuItem = module.FormActionMenu.Single(x => x.Text.Contains("Action"));
				var createDeclarationFromInventoryMenuItem = actionMenuItem.MenuItems.Cast<MenuItem>().SingleOrDefault(mi => mi.Text == declarationFromInventoryName);
				AssertEquals("Precondition", false, DECustomsDataRegistry.Instance.BondedWarehouseCreateDeclarationFromInventory.Value);
				AssertNull("BondedWarehouseCreateDeclarationFromInventory = false", createDeclarationFromInventoryMenuItem);
			}

			DECustomsDataRegistry.Instance.BondedWarehouseCreateDeclarationFromInventory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (var module = new JobDeclarationModule())
			{
				var actionMenuItem = module.FormActionMenu.Single(x => x.Text.Contains("Action"));
				var createDeclarationFromInventoryMenuItem = actionMenuItem.MenuItems.Cast<MenuItem>().SingleOrDefault(mi => mi.Text == declarationFromInventoryName);
				AssertNotNull("BondedWarehouseCreateDeclarationFromInventory = true", createDeclarationFromInventoryMenuItem);
			}
		}

		public void TestActionMenuItem_CreateDeclarationFromInventory()
		{
			DECustomsDataRegistry.Instance.BondedWarehouseCreateDeclarationFromInventory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			CustomsDataRegistry.Instance.EnableInwardProcessing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (var module = new JobDeclarationModule())
			{
				var actionMenuItem = module.FormActionMenu.Single(x => x.Text.Contains("Action"));

				MenuItem createOnShipment = null;
				MenuItem createFromInventory = null;
				MenuItem createFromInventoryIPR = null;

				foreach (var menuItem in actionMenuItem.MenuItems.Cast<MenuItem>())
				{
					var text = menuItem.Text;

					if (text == "Create Declaration on Shipment")
					{
						createOnShipment = menuItem;
					}
					else if (text == "Create Declaration from Inventory")
					{
						createFromInventory = menuItem;
					}
					else if (text == "Create Declaration from Inventory - IPR")
					{
						createFromInventoryIPR = menuItem;
					}
				}

				CombineAssertions(() =>
				{
					AssertNotNull("Create Declaration on Shipment menu item should be present", createOnShipment);
					AssertNotNull("Create Declaration from Inventory menu Item should be present", createFromInventory);
					AssertNotNull("Create Declaration from Inventory - IPR menu Item should be present", createFromInventoryIPR);
					AssertEquals(
						"Create Declaration from Inventory menu item should be directly after Create Declaration on Shipment menu item",
						createOnShipment.Index + 1, createFromInventory.Index);
					AssertEquals(
						"Create Declaration from Inventory - IPR menu item should be directly after Create Declaration from Inventory menu item",
						createFromInventory.Index + 1, createFromInventoryIPR.Index);

					createFromInventory.PerformClick();
					AssertType<CreateDeclarationForm>("Create Declaration Form should be shown", ZFormModaliser.LastFormShownDialogForTest);

					createFromInventoryIPR.PerformClick();
					AssertType<CreateDeclarationForm>("Create Declaration Form should be shown", ZFormModaliser.LastFormShownDialogForTest);
				});
			}
		}

		public void TestActionMenuItem_CreateDeclarationFromInventoryIPR_Visibility()
		{
			const string declarationFromInventoryName = "Create Declaration from Inventory - IPR";

			using (var module = new JobDeclarationModule())
			{
				var actionMenuItem = module.FormActionMenu.Single(x => x.Text.Contains("Action"));
				var createDeclarationFromInventoryMenuItem = actionMenuItem.MenuItems.Cast<MenuItem>().SingleOrDefault(mi => mi.Text == declarationFromInventoryName);
				AssertEquals("Precondition", false, CustomsDataRegistry.Instance.EnableInwardProcessing.Value);
				AssertNull("EnableInwardProcessing = false", createDeclarationFromInventoryMenuItem);
			}

			CustomsDataRegistry.Instance.EnableInwardProcessing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (var module = new JobDeclarationModule())
			{
				var actionMenuItem = module.FormActionMenu.Single(x => x.Text.Contains("Action"));
				var createDeclarationFromInventoryMenuItem = actionMenuItem.MenuItems.Cast<MenuItem>().SingleOrDefault(mi => mi.Text == declarationFromInventoryName);
				AssertNotNull("EnableInwardProcessing = true", createDeclarationFromInventoryMenuItem);
			}
		}

		public void TestActionMenuItem_CreateDeclarationFromWarehouseOrder_Visibility()
		{
			const string createDeclarationFromWarehouseOrderName = "Create Declaration from Warehouse Order";

			using (var module = new JobDeclarationModule())
			{
				var actionMenuItem = module.FormActionMenu.Single(x => x.Text.Contains("Action"));
				var createDeclarationFromWarehouseOrderMenuItem = actionMenuItem.MenuItems.Cast<MenuItem>().SingleOrDefault(mi => mi.Text == createDeclarationFromWarehouseOrderName);
				AssertEquals("Precondition", false, DECustomsDataRegistry.Instance.BondedWarehouseCreateDeclarationFromWarehouseOrder.Value);
				AssertNull("BondedWarehouseCreateDeclarationFromWarehouseOrder = false", createDeclarationFromWarehouseOrderMenuItem);
			}

			DECustomsDataRegistry.Instance.BondedWarehouseCreateDeclarationFromWarehouseOrder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (var module = new JobDeclarationModule())
			{
				var actionMenuItem = module.FormActionMenu.Single(x => x.Text.Contains("Action"));
				var createDeclarationFromWarehouseOrderMenuItem = actionMenuItem.MenuItems.Cast<MenuItem>().SingleOrDefault(mi => mi.Text == createDeclarationFromWarehouseOrderName);
				AssertNotNull("BondedWarehouseCreateDeclarationFromWarehouseOrder = true", createDeclarationFromWarehouseOrderMenuItem);
			}
		}

		public void TestActionMenuItem_CreateDeclarationFromWarehouseOrder()
		{
			DECustomsDataRegistry.Instance.BondedWarehouseCreateDeclarationFromWarehouseOrder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			DECustomsDataRegistry.Instance.BondedWarehouseCreateDeclarationFromInventory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (var module = new JobDeclarationModule())
			{
				var actionMenuItem = module.FormActionMenu.Single(x => x.Text.Contains("Action"));

				MenuItem createFromInventory = null;
				MenuItem createFromWarehouseOrder = null;

				foreach (var menuItem in actionMenuItem.MenuItems.Cast<MenuItem>())
				{
					var text = menuItem.Text;

					if (text == "Create Declaration from Warehouse Order")
					{
						createFromWarehouseOrder = menuItem;
					}
					else if (text == "Create Declaration from Inventory")
					{
						createFromInventory = menuItem;
					}
				}

				CombineAssertions(() =>
				{
					AssertNotNull("Create Declaration from Warehouse Order menu item should be present", createFromWarehouseOrder);
					AssertEquals(
						"Create Declaration from Warehouse Order menu item should be directly after Create Declaration from Inventory menu item",
						createFromInventory.Index + 1, createFromWarehouseOrder.Index);

					createFromWarehouseOrder.PerformClick();
					AssertType<CreateDeclarationForm>("Create Declaration Form should be shown", ZFormModaliser.LastFormShownDialogForTest);
				});
			}
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Germany;

		protected override Type GetExpectedJobDeclarationType() => typeof(JobDeclaration);

		protected override Type GetExpectedInvoiceHeaderType() => typeof(JobComInvoiceHeader);

		protected override Type GetExpectedInvoiceLineType() => typeof(JobComInvoiceLine);

		protected override Customs.Business.BaseJobDeclaration CreateDeclarationForFetchHintTest(CargoWise.EntityFramework.BusinessObjectFactory factory, string messageType, int i)
		{
			var declaration = (JobDeclaration)base.CreateDeclarationForFetchHintTest(factory, messageType, i);
			declaration.ZG_PresentationEndDate = ZDateTime.Now.AddDays(i);
			var cei = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_LocationOfGoods = "LHR";
			cei.CEI_Style = "EL";
			return declaration;
		}
	}
}
