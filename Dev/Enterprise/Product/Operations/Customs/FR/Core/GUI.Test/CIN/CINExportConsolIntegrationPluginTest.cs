using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.GUI.CIN.Testing
{
	class CINExportConsolIntegrationPluginTest : ZPlugInGenericTest
	{
		protected override ZPlugIn GetPlugInToTest()
		{
			return new CINExportConsolIntegrationPlugIn(consol);
		}

		public void TestVisibility()
		{
			#region Plugin Italy for air export consol is not visible

			consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "ITXXX";
			consol.JK_RL_NKDischargePort = "POXXX";
			consol.JK_MasterBillNum = "MB0001";
			consol.JK_TransportMode = "AIR";
			AssertCINExportMenuAndPlugin(consol, false);

			#endregion

			#region Plugin France for air export consol is visible

			consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "FRXXX";
			consol.JK_RL_NKDischargePort = "AUXXX";
			consol.JK_MasterBillNum = "MB0001";
			consol.JK_TransportMode = "AIR";
			AssertCINExportMenuAndPlugin(consol, true);

			#endregion

			#region Plugin France for sea  export consol is not visible

			consol.JK_TransportMode = "SEA";
			AssertCINExportMenuAndPlugin(consol, false);

			#endregion

			#region Plugin France for air not export consol is not visible

			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "AEXXX";
			AssertCINExportMenuAndPlugin(consol, false);

			#endregion
		}

		public void TestMenuAndPlugin()
		{
			consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "FRXXX";
			consol.JK_RL_NKDischargePort = "AUXXX";
			consol.JK_MasterBillNum = "MB0001";
			consol.JK_TransportMode = "AIR";
			using (var plugin = new CINExportConsolIntegrationPlugIn(consol))
			{
				AssertEquals(true, plugin.Enabled);

				var rootMenuForRefresh = (EDIMenu)plugin.TopLevelMenu;

				AssertNotNull(rootMenuForRefresh);
				AssertEquals(1, rootMenuForRefresh.MenuItems.Count);
				AssertEquals(true, rootMenuForRefresh.MenuItems[0].Visible);

				rootMenuForRefresh.Dispose();
			}
		}

		#region SendCINExport745
		public void TestSendCINExport745()
		{
			consol = SetupForwardingConsol();
			var query = new ZQuery(EDIMessageSchema.EM_MessageType, "CIN");
			query.AddToFilter(EDIMessageSchema.EM_Status, "QUE");
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, "FRC");
			query.AddToFilter(EDIMessageSchema.EM_MessageSubType, "745");

			using (var plugin = GetPlugInToTest())
			{
				var rootMenuForRefresh = (EDIMenu)plugin.TopLevelMenu;
				var menu745 = rootMenuForRefresh.MenuItems[0];
				menu745.PerformClick();
				AssertContains("There is no declaration to be sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIMessage), query));

				SetupConsolWithDeclarations(consol);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				menu745.PerformClick();
				AssertEquals(CINExportMessageSender.MessageSendSuccessful, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(2, Factory.GetDatabaseCount(typeof(EDIMessage), query));
			}
		}

		ForwardingConsol SetupForwardingConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "FRXXX";
			consol.JK_RL_NKDischargePort = "AUXXX";
			consol.JK_MasterBillNum = "MB0001";
			consol.JK_TransportMode = "AIR";
			return consol;
		}

		void SetupConsolWithDeclarations(ForwardingConsol consol)
		{
			var shipment1 = consol.Shipments.AddNew();
			var declaration1 = Factory.New<Business.Declaration.JobDeclaration>();
			declaration1.ActiveEntryHeaders.AddNew();
			declaration1.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration1.JE_JS = shipment1.PK;

			var shipment2 = consol.Shipments.AddNew();
			var declaration2 = Factory.New<Business.Declaration.JobDeclaration>();
			declaration2.JE_JS = shipment2.PK;
			declaration2.ActiveEntryHeaders.AddNew();
			declaration2.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
		}
		#endregion

		#region Methods
		void AssertCINExportMenuAndPlugin(ForwardingConsol consol, bool expected)
		{
			using (var pluginFR = new CINExportConsolIntegrationPlugIn(consol))
			{
				AssertEquals(expected, pluginFR.Enabled);
			}
		}
		#endregion

		ForwardingConsol consol;
	}
}
