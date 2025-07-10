using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Internal;

namespace Enterprise.Customs.AU.Sailing.GUI.Testing
{
	sealed class ManifestPluginToSailingTest : ZArchitecture.PlugIn.Testing.ZPlugInGenericTest
	{
		public void TestName()
		{
			AssertEquals("Name", "Customs Manifest", plugin.Name);
		}

		public void TestTopLevelMenu()
		{
			AssertNotNull(plugin.TopLevelMenu);
			AssertEquals("is cached", plugin.TopLevelMenu, plugin.TopLevelMenu);
			AssertEquals("type", typeof(ManifestPluginMenu), plugin.TopLevelMenu.GetType());
		}

		public void TestHasUserControl()
		{
			AssertNull("User Control", plugin.UserControl);
		}

		public void TestPluginAlwaysAllowed()
		{
			AssertEquals("CheckPoint", Env.Licence.AlwaysAllow, ((IPlugInInternals)plugin).LicenceCheckPoint);
		}

		public void TestHostAndBusinessEntityDifferent()
		{
			AssertEquals("host", typeof(JobVoyage), ((IPlugInInternals)plugin).HostBusinessEntity.GetType());
			AssertEquals("business", typeof(CustomsJobVoyageWrapper), plugin.BusinessEntity.GetType());
		}

		protected override ZPlugIn GetPlugInToTest() => new ManifestPluginToSailing(Factory.New<JobVoyage>());

		ManifestPluginToSailing plugin;
		protected override void SetUp()
		{
			base.SetUp();
			plugin = new ManifestPluginToSailing(Factory.New<JobVoyage>());
		}

		protected override void TearDown()
		{
			base.TearDown();
			plugin.Dispose();
		}
	}
}
