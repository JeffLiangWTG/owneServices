using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(AUImportTariffBulkChangeModule))]
	sealed class AUImportTariffBulkChangeModuleTest : ZArchitecture.Modules.Testing.ZPopupModuleBasherTest
	{
		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.ImportTariffBulkChange, module.ID);
		}

		public void TestLicenseCheckpoint()
		{
			AssertEquals(Env.Licence.Broker, module.LicenceCheckPoint);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.ImportTariffBulkChange, module.SecurityCheckpoint);
		}

		public void TestGetNewController()
		{
			using (var moduleForm = module.ShowNew())
			{
				AssertType<AUImportTariffBulkChangeController>(((IPopupModuleInternalsForTesting)module).LastController);
			}
		}

		public void TestShow()
		{
			using (IZForm form = new AUImportTariffBulkChangeController().ShowNewForm())
			{
				AssertNotNull(form);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			module = new AUImportTariffBulkChangeModule();
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ImportTariffBulkChange;
		}

		protected override void TearDown()
		{
			if (module != null)
			{
				module.Dispose();
			}

			base.TearDown();
		}

		AUImportTariffBulkChangeModule module;
	}
}
