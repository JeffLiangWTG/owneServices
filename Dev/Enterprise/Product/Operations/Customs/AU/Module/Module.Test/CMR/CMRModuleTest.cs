using System;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module.Testing
{
	abstract class CMRModuleTest : ZArchitecture.Modules.Testing.ZModuleBasherTest
	{
		public void TestGetNewEmbeddedControlLegacy()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			using (Control embeddedControl = module.EmbeddedControl)
			{
				AssertEquals("GetNewEmbeddedControl is a ZLabel in Legacy mode", typeof(ZLabel), embeddedControl.GetType());
			}
		}

		public void TestGetNewEmbeddedControlCMR()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			using (Control embeddedControl = module.EmbeddedControl)
			{
				AssertEquals("GetNewEmbeddedControl is not a ZLabel in CMR mode", GetTypeOfFilterControl(), embeddedControl.GetType());
			}
		}

		public void TestIsCMRLegacy()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			Assert("iscmr", !module.IsCMR);
		}

		public void TestIsCMRCMR()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			Assert("iscmr", module.IsCMR);
		}

		public void TestToolBarButtonsCMR()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertNotNull("toolbarbuttons", module.ToolBarButtons);
		}

		public void TestFormActionMenuLegacy()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			AssertNull("formactionmenu", module.FormActionMenu);
		}

		public void TestFormActionMenuCMR()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertNotNull("formactionmenu", module.FormActionMenu);
		}

		public override void TestModuleShowsAndCanSearch()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			base.TestModuleShowsAndCanSearch();
		}

		protected abstract Type GetTypeOfFilterControl();

		protected override string CountryCode => Core.Constants.CountryCodes.Australia;

		protected override void PrepareModuleForBashing(ZEmbeddedModule module)
		{
			base.PrepareModuleForBashing(module);
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
		}

		protected override void SetUp()
		{
			base.SetUp();
			module = (CMRModule)ZModuleFactory.Instance.Create(GetModuleID());
		}

		protected override void TearDown()
		{
			module.Dispose();
			base.TearDown();
		}

		CMRModule module;
	}
}
