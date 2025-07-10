using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(AirCargoCusUnderbondPluginController))]
	sealed class AirCargoCusUnderbondPluginControllerTest : ZControllerBasherTest
	{
		public void TestGetPlugin()
		{
			var controller = new AirCargoCusUnderbondPluginController();
			var mAWB = Factory.New<CusMAWB>();
			mAWB.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			using (ZArchitecture.PlugIn.ZPlugIn plugin = controller.GetPlugInInternal(mAWB))
			{
				AssertNotNull("Plugin is not null", plugin);
				AssertEquals("Plugin is of type CusUnderbondPlugin", typeof(GUI.CMRCusUnderbondPlugin), plugin.GetType());
			}
		}

		public void TestPluginName()
		{
			var controller = new AirCargoCusUnderbondPluginController();
			var mAWB = Factory.New<CusMAWB>();
			mAWB.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			using (ZArchitecture.PlugIn.ZPlugIn plugin = controller.GetPlugInInternal(mAWB))
			{
				AssertEquals("Plugin.Name", "Master Underbond Movement", plugin.Name);
			}
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Australia;

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.AU.AirCargoCusUnderbondPluginController;

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
	}
}
