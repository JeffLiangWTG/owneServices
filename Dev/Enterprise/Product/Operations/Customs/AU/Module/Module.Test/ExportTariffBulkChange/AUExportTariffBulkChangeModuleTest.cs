using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(AUExportTariffBulkChangeModule))]
	sealed class AUExportTariffBulkChangeModuleTest : ZArchitecture.Modules.Testing.ZPopupModuleBasherTest
	{
		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.ExportTariffBulkChange, module.ID);
		}

		public void TestLicenseCheckpoint()
		{
			AssertEquals(Env.Licence.Broker, module.LicenceCheckPoint);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.ExportTariffBulkChange, module.SecurityCheckpoint);
		}

		public void TestGetNewController()
		{
			using (var moduleForm = module.ShowNew())
			{
				AssertType<AUExportTariffBulkChangeController>(((IPopupModuleInternalsForTesting)module).LastController);
			}
		}

		public void TestShow()
		{
			using (IZForm form = new AUExportTariffBulkChangeController().ShowNewForm())
			{
				AssertNotNull(form);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.ExportTariffBulkChange;

		protected override void SetUp()
		{
			base.SetUp();
			module = new AUExportTariffBulkChangeModule();
		}

		protected override void TearDown()
		{
			if (module != null)
			{
				module.Dispose();
			}

			base.TearDown();
		}

		AUExportTariffBulkChangeModule module;
	}
}
