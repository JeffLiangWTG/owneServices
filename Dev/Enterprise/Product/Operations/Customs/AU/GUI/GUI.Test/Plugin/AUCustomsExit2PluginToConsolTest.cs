using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.PlugIn.Testing;

namespace Enterprise.Customs.AU.Declaration.GUI.PlugIn.Testing
{
	sealed class AUCustomsExit2PluginToConsolTest : ZPlugInGenericTest
	{
		public void TestCMRContingencyAirMenuItem()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			using (AUCustomsExit2PluginToConsol testPlugIn = new AUCustomsExit2PluginToConsol(consol))
			{
				AssertNotNull(testPlugIn.UserControl);
				Menu brokerageMenu = testPlugIn.TopLevelMenu;
				AssertContainsMenuItem(brokerageMenu, "Air Freight &CRN Contingency");
				AssertContainsMenuItem(brokerageMenu, "Air Freight &EDN Contingency");
			}
		}

		public void TestCMRContingencySeaMenuItem()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			using (AUCustomsExit2PluginToConsol testPlugIn = new AUCustomsExit2PluginToConsol(consol))
			{
				AssertNotNull(testPlugIn.UserControl);
				Menu brokerageMenu = testPlugIn.TopLevelMenu;
				AssertContainsMenuItem(brokerageMenu, "Sea Freight &CRN Contingency");
				AssertContainsMenuItem(brokerageMenu, "Sea Freight &EDN Contingency");
			}
		}
		public void TestLoadZPlugIn()
		{
			using (AUCustomsExit2PluginToConsol testPlugIn = new AUCustomsExit2PluginToConsol(Factory.New<ForwardingConsol>()))
			{
				AssertNotNull(testPlugIn.UserControl);
				Menu brokerageMenu = testPlugIn.TopLevelMenu;
				AssertNotNull(brokerageMenu);
			}
		}

		public void TestVisibilityOfConsolPlugIn()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			using (AUCustomsExit2PluginToConsol testPlugIn = new AUCustomsExit2PluginToConsol(consol))
			{
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "NZAKL";
				AssertEquals("PlugIn visible", true, testPlugIn.Enabled);

				consol.JK_RL_NKLoadPort = "NZAKL";
				consol.JK_RL_NKDischargePort = "AUSYD";
				AssertEquals("PlugIn invisible", false, testPlugIn.Enabled);

				Transport transport1 = consol.Transports[0];
				transport1.JW_RL_NKLoadPort = "AUSYD";
				transport1.JW_RL_NKDiscPort = "AUBNE";

				Transport transport2 = consol.Transports.AddNew();
				transport2.JW_RL_NKLoadPort = "AUBNE";
				transport2.JW_RL_NKDiscPort = "NZAKL";

				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "NZAKL";
				AssertEquals("PlugIn visible - dont be confused by the initial domestic leg", true, testPlugIn.Enabled);
			}
		}

		public void TestGetCustomsManifestStatus()
		{
			var consol = Factory.New<ForwardingConsol>();
			using (var testPlugIn = new AUCustomsExit2PluginToConsolForTest(consol))
			{
				var entryNum = new Business.FreightConsolWrapper(consol).CreateCusEntryNumber();
				entryNum.CE_EntryNum = "12345678901234";
				AssertEquals("CustomsManifestStatusType", typeof(Exit2ManifestStatus), testPlugIn.GetCustomsManifestStatusInternal(consol).GetType());
				entryNum.CE_EntryNum = "123456789";
				AssertEquals("CustomsManifestStatusType", typeof(ESMManifestStatus), testPlugIn.GetCustomsManifestStatusInternal(consol).GetType());
			}
		}

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugInToTest() => new AUCustomsExit2PluginToConsol(Factory.New<ForwardingConsol>());

		void AssertContainsMenuItem(Menu menu, string text)
		{
			bool found = false;

			foreach (MenuItem item in menu.MenuItems)
			{
				if (item.Text == text)
				{
					found = true;
					break;
				}
			}
			Assert("Menu Item " + text, found);
		}

		sealed class AUCustomsExit2PluginToConsolForTest : AUCustomsExit2PluginToConsol
		{
			public AUCustomsExit2PluginToConsolForTest(ForwardingConsol consol) : base(consol)
			{
			}

			internal CustomsManifestStatus GetCustomsManifestStatusInternal(IManifestProvider manifestProvider) => GetCustomsManifestStatus(manifestProvider);
		}
	}
}
