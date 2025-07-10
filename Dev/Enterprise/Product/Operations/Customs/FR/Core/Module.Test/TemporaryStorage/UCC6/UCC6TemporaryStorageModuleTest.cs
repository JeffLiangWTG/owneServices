using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.GUI.UCC6TemporaryStorage;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Module.TempStorage.Testing
{
	[TestedType(typeof(UCC6TemporaryStorageModule))]
	public class UCC6TemporaryStorageModuleTest : ZModuleBasherTest
	{
		public void TestActionMenuItemsContainsPntsResponseInterchangeImporter()
		{
			using (var module = new UCC6TemporaryStorageModuleForTest())
			{
				_ = module.FormActionMenu;
				var actionMenuItems = module.ActionsMenuItem.MenuItems;
				var addPntsResponseInterchangeActionMenuItem = actionMenuItems.FindByText(PntsInboundInterchangeImporter.PntsResponseInterchangeAddActionText);
				AssertNotNull("The menu item of PNTS response interchange importer should exist in the actions menu.", addPntsResponseInterchangeActionMenuItem);
			}
		}

		public void TestModuleButtonsAndNewForm()
		{
			using (var module = new UCC6TemporaryStorageModuleForTest())
			{
				module.SetupAndGetGrid();
				var istMenuItem = module.NewMenuItem.MenuItems.FindByText("New IST");
				var ladtMenuItem = module.NewMenuItem.MenuItems.FindByText("New LADT");
				AssertNotNull(istMenuItem);
				AssertNotNull(ladtMenuItem);

				istMenuItem.PerformClick();
				var controller = module.ControllerForTest as UCC6TemporaryStorageController;
				AssertNotNull(controller);
				var lastForm = module.ControllerForTest.LastShownForm as UCC6TemporaryStorageForm;
				AssertEquals(FRConstants.TemporaryStorage.AppCodeIST, ((TemporaryStorageHeader)lastForm.BusinessEntity).AMA_ManifestType);
				lastForm.Dispose();

				ladtMenuItem.PerformClick();
				controller = module.ControllerForTest as UCC6TemporaryStorageController;
				AssertNotNull(controller);
				lastForm = module.ControllerForTest.LastShownForm as UCC6TemporaryStorageForm;
				AssertEquals(FRConstants.TemporaryStorage.AppCodeLAD, ((TemporaryStorageHeader)lastForm.BusinessEntity).AMA_ManifestType);
				lastForm.Dispose();
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.UCC6TemporaryStorage;

		protected override string CountryCode => Core.Constants.CountryCodes.France;

		protected override bool HasController() => true;
	}

	class UCC6TemporaryStorageModuleForTest : UCC6TemporaryStorageModule
	{
		protected override ZController GetNewController(string appCode)
		{
			ControllerForTest = base.GetNewController(appCode);
			return ControllerForTest;
		}

		internal ZController ControllerForTest;
	}
}
