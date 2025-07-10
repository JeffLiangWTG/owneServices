using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.GB.GUI.Ccsuk.Testing
{
	class CcsukShipmentMultiHawbPluginForDirectConsolsTests : CcsukShipmentMultiHawbPluginTests
	{
		public void TestShownButWithErrorMessageWhenNoMawbLinkedToConsol()
		{
			consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_MasterBillNum = "12512345678";
			consol.JK_RL_NKDischargePort = "GBLHR";
			consol.JK_AgentType = "DRT";
			shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKDestination = "GBLHR";
			var basic = Factory.New<CusMAWB>();
			var basic2 = Factory.New<CusMAWB>();
			Factory.Save();
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				using (var plugin = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse))
				{
					AssertEquals("Plugin not yet enabled, no consol", false, plugin.Enabled);
					shipment.Consols.Add(consol);
				}
			}
			shipment.Consols.Add(consol);
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				using (var plugin = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse))
				{
					AssertEquals("Plugin is enabled now that we have a consol", true, plugin.Enabled);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					plugin.SelectTabPage();
					AssertContains("Consol does not have a CCSUK basic linked to it - see warning message", "Instead open the consol", plugin.TabPage.Controls[0].Text);
				}
			}
			basic.CM_MAWB = consol.JK_MasterBillNum;
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				using (var plugin = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					plugin.SelectTabPage();
					AssertType("Multiple mawbs on consol", typeof(ConsolToManyMawbsPluginHelper), plugin.BusinessEntity);
					AssertEquals(true, ((BusinessObject)plugin.BusinessEntity).ReadOnly);
				}
			}
			basic2.CM_MAWB = basic.CM_MAWB;
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				using (var plugin = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					plugin.SelectTabPage();
					AssertType("Multiple mawbs on consol", typeof(ConsolToManyMawbsPluginHelper), plugin.BusinessEntity);
					AssertEquals(true, ((BusinessObject)plugin.BusinessEntity).ReadOnly);
				}
			}
		}

		protected override ZPlugIn GetPlugInToTest()
		{
			MakeGoodShipmentOnGoodConsol();
			consol.JK_AgentType = "DRT";
			shipment.JS_HouseBill = "Worker";
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = consol.JK_MasterBillNum;
			return new CcsukShipmentMultiHawbPluginForDirectConsols(shipment);
		}
	}
}

