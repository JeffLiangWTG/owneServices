using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(VoyageManifestModule))]
	sealed class VoyageManifestModuleTest : ZArchitecture.Modules.Testing.ZModuleBasherTest
	{
		public void TestGetNewEmbeddedControlLegacy()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			Control embeddedControl = module.EmbeddedControl;
			AssertEquals("GetNewEmbeddedControl is a ZLabel in Legacy mode", typeof(ZLabel), embeddedControl.GetType());
			embeddedControl.Dispose();
		}

		public void TestGetNewEmbeddedControlCMR()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			Control embeddedControl = module.EmbeddedControl;
			AssertEquals("GetNewEmbeddedControl is a VoyageManifestFilterControl in CMR mode", typeof(VoyageManifestFilterControl), embeddedControl.GetType());
			embeddedControl.Dispose();
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.AU.VoyageManifest;

		protected override void SetUp()
		{
			base.SetUp();
			module = new VoyageManifestModule();
		}

		VoyageManifestModule module;
		protected override void TearDown()
		{
			module.Dispose();
			base.TearDown();
		}
	}
}
