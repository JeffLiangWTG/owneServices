using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.GUI.CusTempStorage;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Module.TempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageModule))]
	class TemporaryStorageModuleTest : EU.TemporaryStorage.Module.Testing.TemporaryStorageModuleAbstractTest
	{
		public override void TestModuleShowsAndCanSearch()
		{
			var dec = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			dec.SJH_AppCode = FRConstants.TemporaryStorage.AppCodeFRC;
			dec.SJH_GB = GlbBranch.CurrentBranch.PK;

			FRCCusTempStorageDec.New(dec);
			Factory.Save();

			base.TestModuleShowsAndCanSearch();
		}

		public void TestModuleButtonsAndNewForm()
		{
			using (var module = new TemporaryStorageModuleForTest())
			{
				module.SetupAndGetGrid();
				var istMenuItem = module.NewMenuItem.MenuItems.FindByText("New IST");
				var frcMenuItem = module.NewMenuItem.MenuItems.FindByText("New CIN/IST");
				var ladtMenuItem = module.NewMenuItem.MenuItems.FindByText("New LADT");
				var nfoMenuItem = module.NewMenuItem.MenuItems.FindByText("New IST From Other Job");
				AssertNotNull(istMenuItem);
				AssertNotNull(frcMenuItem);
				AssertNotNull(ladtMenuItem);
				AssertNotNull(nfoMenuItem);

				istMenuItem.PerformClick();
				var controller = module.ControllerForTest as TemporaryStorageController;
				AssertNotNull(controller);
				var lastForm = module.ControllerForTest.LastShownForm as CusTempStorageForm;
				AssertEquals(FRConstants.TemporaryStorage.AppCodeIST, ((CusTempStorageJobHeader)lastForm.BusinessEntity).SJH_AppCode);
				lastForm.Dispose();

				frcMenuItem.PerformClick();
				controller = module.ControllerForTest as TemporaryStorageController;
				AssertNotNull(controller);
				lastForm = module.ControllerForTest.LastShownForm as CusTempStorageForm;
				AssertEquals(FRConstants.TemporaryStorage.AppCodeFRC, ((CusTempStorageJobHeader)lastForm.BusinessEntity).SJH_AppCode);
				lastForm.Dispose();

				ladtMenuItem.PerformClick();
				controller = module.ControllerForTest as TemporaryStorageController;
				AssertNotNull(controller);
				lastForm = module.ControllerForTest.LastShownForm as CusTempStorageForm;
				AssertEquals(FRConstants.TemporaryStorage.AppCodeLAD, ((CusTempStorageJobHeader)lastForm.BusinessEntity).SJH_AppCode);
				lastForm.Dispose();

				nfoMenuItem.PerformClick();
				var nfoController = module.ControllerForTest as NewFromOthersTemporaryStorageController;
				AssertNotNull(nfoController);
				var lastForm2 = nfoController.LastShownForm as NewFromOtherForm;
				AssertType(typeof(TemporaryStorageWrapperFromParentHelper), lastForm2.BusinessEntity);
				lastForm2.Dispose();
			}
		}

		public void TestModuleConsolController()
		{
			var temporaryStorage = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			using (var module = new TemporaryStorageModuleForTest())
			{
				module.SetupAndGetGrid();
				var controller = module.GetNewController(temporaryStorage);
				Assert(controller is TemporaryStorageController);
			}

			temporaryStorage.SetRelatedBusinessObject(consol, Core.Constants.GenPivotTypes.CusStorageHeaderConsol);

			using (var module = new TemporaryStorageModuleForTest())
			{
				module.SetupAndGetGrid();
				var controller = module.GetNewController(temporaryStorage);
				Assert(controller is CINTemporaryStorageConsolController);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.TemporaryStorage;

		protected override string CountryCode => Core.Constants.CountryCodes.France;

		protected override bool HasController() => true;
	}

	class TemporaryStorageModuleForTest : TemporaryStorageModule
	{
		protected override ZController GetNewController(string appCode)
		{
			ControllerForTest = base.GetNewController(appCode);
			return ControllerForTest;
		}

		protected override ZController GetNewFromOthersTemporaryStorageController()
		{
			ControllerForTest = base.GetNewFromOthersTemporaryStorageController();
			return ControllerForTest;
		}

		internal ZController ControllerForTest;

		public new ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			ControllerForTestConsolController = base.GetNewController(selectedBusinessObject);
			return ControllerForTestConsolController;
		}
		internal ZController ControllerForTestConsolController;
	}
}
