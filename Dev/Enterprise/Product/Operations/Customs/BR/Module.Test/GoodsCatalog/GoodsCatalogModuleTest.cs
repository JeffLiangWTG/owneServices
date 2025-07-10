using System.Windows.Forms;
using Enterprise.Customs.BR.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(GoodsCatalogModule))]
	class GoodsCatalogModuleTest : ZModuleBasherTest
	{
		public void TestOperationalActionsPlugInAdded()
		{
			using (var module = (GoodsCatalogModule)GetModule())
			{
				AssertNotNull(module.Plugins.GetPlugin(ControllerIDs.OperationalActions));
			}
		}

		public void TestCheckpoints()
		{
			using (var module = (ZFilterGridModule)GetModule())
			{
				AssertEquals(Env.Security.GoodsCatalog, module.SecurityCheckpoint);
				AssertEquals(Env.Licence.Broker, module.LicenceCheckPoint);
			}
		}

		public void TestDownloadGoodsCatalogMenuItem()
		{
			using (var module = new GoodsCatalogModuleForTesting())
			{
				var downloadGoodsCatalogMenuItem = module.GetNewActionMenuItems().FindByText("Download Catalog");
				AssertNotNull("DownloadGoodsCatalog Menu Item is visible", downloadGoodsCatalogMenuItem);

				downloadGoodsCatalogMenuItem.PerformClick();
				AssertType<DownloadGoodsCatalogForm>("Goods Catalog Download form is been shown", ZFormModaliser.LastFormShownDialogForTest);
			}
		}
		public void TestSupportsWorkflow()
		{
			using (var module = new GoodsCatalogModule())
			{
				Assert("SupportsWorkflow should be True", module.SupportsWorkflow);
			}
		}

		protected sealed override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.GoodsCatalog;

		sealed class GoodsCatalogModuleForTesting : GoodsCatalogModule
		{
			internal new MenuItem[] GetNewActionMenuItems() => base.GetNewActionMenuItems();
		}
	}
}
