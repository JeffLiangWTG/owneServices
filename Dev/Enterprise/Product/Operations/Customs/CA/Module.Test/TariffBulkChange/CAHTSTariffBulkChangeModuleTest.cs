using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(CAHTSTariffBulkChangeModule))]
	sealed class CAHTSTariffBulkChangeModuleTest : ZArchitecture.Modules.Testing.ZPopupModuleBasherTest
	{
		public void TestGetNewController()
		{
			module.Show();
			var lastController = ((IPopupModuleInternalsForTesting)module).LastController;
			AssertType<CAHTSTariffBulkChangeController>(lastController);
			lastController.LastShownForm?.Dispose();
		}

		public void TestShow()
		{
			using (IZForm form = new CAHTSTariffBulkChangeController().ShowNewForm())
			{
				AssertNotNull(form);
			}
		}

		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.Customs.CA.HTSTariffBulkChange, module.ID);
		}

		public void TestLicenseCheckpoint()
		{
			AssertEquals(Env.Licence.Broker, module.LicenceCheckPoint);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.HTSTariffBulkChange, module.SecurityCheckpoint);
		}

		protected override void SetUp()
		{
			base.SetUp();
			module = new CAHTSTariffBulkChangeModule();
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.CA.HTSTariffBulkChange;

		protected override void TearDown()
		{
			module.Dispose();
			base.TearDown();
		}

		CAHTSTariffBulkChangeModule module;
	}
}
