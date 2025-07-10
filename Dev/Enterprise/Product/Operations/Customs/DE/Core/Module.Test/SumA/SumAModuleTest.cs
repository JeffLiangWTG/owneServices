using Enterprise.Customs.DE.GUI;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Module.Testing
{
	[TestedType(typeof(SumAModule))]
	class SumAModuleTest : EU.TemporaryStorage.Module.Testing.TemporaryStorageModuleAbstractTest
	{
		public void TestWorkflowType()
		{
			using (var module = new SumAModule())
			{
				AssertEquals("module.SupportsWorkflow", new CusTempStorageJobHeaderWorkflowDescriptor().Code,
					module.WorkflowType);
			}
		}

		public void TestWorkflowSupported()
		{
			using (var module = new SumAModule())
			{
				AssertEquals("module.SupportsWorkflow", true, module.SupportsWorkflow);
			}
		}

		public void TestLicenceAndSecurityCheckPoint()
		{
			using (var module = new SumAModule())
			{
				AssertEquals("Licence Checkpoint", Env.Licence.ImportBroker, module.LicenceCheckPoint);
				AssertEquals("SecurityCheckpoint", Env.Security.CustomsTemporaryStorage, module.SecurityCheckpoint);
			}
		}

		public void TestStatementModuleAllows()
		{
			using (var module = new SumAModule())
			{
				AssertEquals("module.AllowNew", true, module.AllowNew);
				AssertEquals("module.AllowEdit", true, module.AllowEdit);
				AssertEquals("module.AllowDelete", false, module.AllowDelete);
				AssertEquals("module.AllowView", false, module.AllowView);
				AssertEquals("module.AllowUniversalCopy", false, module.AllowUniversalCopy);
			}
		}

		public override void TestModuleShowsAndCanSearch()
		{
			var dec = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			dec.SJH_AppCode = Business.CusTempStorage.TemporaryStorageApplicationCodeList.Codes.SumA;
			dec.SJH_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();
			base.TestModuleShowsAndCanSearch();
		}

		public void TestModuleButtonsAndNewForm()
		{
			using (var module = new SumAModuleForTest())
			{
				module.SetupAndGetGrid();
				var sumAMenuItem = module.NewMenuItem.MenuItems.FindByText("New SumA Job");
				var rexMenuItem = module.NewMenuItem.MenuItems.FindByText("New Re-Export Job");
				AssertNotNull(sumAMenuItem);
				AssertNotNull(rexMenuItem);

				sumAMenuItem.PerformClick();
				var controller = module.ControllerForTest as SumAController;
				AssertNotNull(controller);
				var lastForm = module.ControllerForTest.LastShownForm as TemporaryStorageForm;
				AssertEquals(Business.CusTempStorage.TemporaryStorageApplicationCodeList.Codes.SumA, ((CusTempStorageJobHeader)lastForm.BusinessEntity).SJH_AppCode);
				lastForm.Dispose();

				rexMenuItem.PerformClick();
				controller = module.ControllerForTest as SumAController;
				AssertNotNull(controller);
				lastForm = module.ControllerForTest.LastShownForm as TemporaryStorageForm;
				AssertEquals(Business.CusTempStorage.TemporaryStorageApplicationCodeList.Codes.REX, ((CusTempStorageJobHeader)lastForm.BusinessEntity).SJH_AppCode);
				lastForm.Dispose();
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.TemporaryStorage;

		protected override string CountryCode => Core.Constants.CountryCodes.Germany;

		protected override bool HasController() => true;

		class SumAModuleForTest : SumAModule
		{
			protected override ZController GetNewController(string appCode)
			{
				ControllerForTest = base.GetNewController(appCode);
				return ControllerForTest;
			}
			internal ZController ControllerForTest;
		}
	}
}
