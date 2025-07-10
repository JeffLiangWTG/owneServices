using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(AirCargoDeclarationCusUnderbondController))]
	sealed class AirCargoDeclarationCusUnderbondControllerTest : ZControllerBasherTest
	{
		public void TestGetPlugInForwardingShipment()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			CusMAWB shipMAWB = Factory.New<CusMAWB>();
			CusHAWB shipHAWB = CusHAWB.CreateNew(shipMAWB, shipment);
			shipMAWB.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			using (ZPlugIn plugin = controller.GetPlugInInternal(shipment))
			{
				AssertNotNull("Plugin is not null", plugin);
				AssertEquals("Plugin is of type CusUnderbondPlugin", typeof(GUI.CMRCusUnderbondPlugin), plugin.GetType());
			}
		}

		public void TestGetPlugInHAWB()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			mAWB.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			using (ZPlugIn plugin = controller.GetPlugInInternal(hAWB))
			{
				AssertNotNull("Plugin is not null", plugin);
				AssertEquals("Plugin is of type CusUnderbondPlugin", typeof(GUI.CMRCusUnderbondPlugin), plugin.GetType());
			}
		}

		public void TestGetPlugInMAWB()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			using (ZPlugIn plugin = controller.GetPlugInInternal(mAWB))
			{
				AssertNotNull("Plugin is not null", plugin);
				AssertEquals("Plugin is of type CusUnderbondPlugin", typeof(GUI.CMRCusUnderbondPlugin), plugin.GetType());
			}
		}

		public void TestPluginName()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			mAWB.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			using (ZPlugIn plugin = controller.GetPlugInInternal(hAWB))
			{
				AssertEquals("Plugin.Name", "House Underbond Movement", plugin.Name);
			}
		}

		public void TestGetPlugInDummy()
		{
			using (ZPlugIn plugin = controller.GetPlugInInternal(Factory.New(typeof(DummyBusinessObject))))
			{
				AssertNull("Plugin is null", plugin);
			}
		}

		public void TestGetPlugInShipmentWithNoHAWB()
		{
			ForwardingShipment shipmentWithNoHAWB = Factory.New<ForwardingShipment>();

			using (ZPlugIn plugin = controller.GetPlugInInternal(shipmentWithNoHAWB))
			{
				AssertNull("Plugin is null", plugin);
			}
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Australia;

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.AU.AirCargoDeclarationCusUnderbondController;

		protected override BusinessObject GetBusinessObjectWithoutValidationErrors()
		{
			var bizO = base.GetBusinessObjectWithoutValidationErrors() as CusMAWB;
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.BreakBulk;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_MasterBillNum = "12345678905";
			consol.Transports[0].JW_TransportMode = Core.Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportType = "FL1";
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";
			bizO.CM_JK = consol.PK;
			bizO.CM_MAWB = "bill1";
			Factory.Save();
			return bizO;
		}

		protected override void SetUp()
		{
			base.SetUp();
			controller = new AirCargoDeclarationCusUnderbondController();
		}

		AirCargoDeclarationCusUnderbondController controller;
	}
}
