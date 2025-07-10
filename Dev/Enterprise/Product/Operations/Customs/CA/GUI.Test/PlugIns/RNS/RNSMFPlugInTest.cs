using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;

namespace Enterprise.Customs.CA.GUI.PlugIns.RNS.Testing
{
	sealed class RNSMFPlugInTest : ZPlugInGenericTest
	{
		public void TestOverrides()
		{
			var loadListConsol = Factory.New<CFSLoadListConsol>();
			using (var plugin = new RNSMFPlugIn(new RNSPlugInSupportLoadListWrapper(loadListConsol)))
			{
				AssertEquals("Name", "RNS/MF", plugin.Name);
				AssertEquals("TopLevelMenu", typeof(RNSMFMenu), plugin.TopLevelMenu.GetType());

				AssertEquals("Should be disabled for export", false, plugin.Enabled);
				loadListConsol.JK_RL_NKLoadPort = "USAAA";
				loadListConsol.JK_RL_NKDischargePort = "CABBB";
				AssertEquals("Should be enabled for import", true, plugin.Enabled);
			}
		}

		protected override ZPlugIn GetPlugInToTest()
		{
			var loadListConsol = Factory.New<CFSLoadListConsol>();
			return new RNSMFPlugIn(new RNSPlugInSupportLoadListWrapper(loadListConsol));
		}
	}
}
