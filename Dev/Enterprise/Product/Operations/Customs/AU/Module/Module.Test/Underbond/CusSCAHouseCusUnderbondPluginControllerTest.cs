using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(CusSCAHouseCusUnderbondPluginController))]
	sealed class CusSCAHouseCusUnderbondPluginControllerTest : ZControllerBasherTest
	{
		public void TestGetPluginForCMRCusSCAHouse()
		{
			var pluginController = new CusSCAHouseCusUnderbondPluginController();
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var house = oceanBill.HouseBills.AddNew();
			using (var plugin = pluginController.GetPlugInInternal(house))
			{
				AssertNotNull("Plugin", plugin);
			}
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Australia;

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.AU.CusSCAHouseCusUnderbondPluginController;

		protected override BusinessObject GetBusinessObjectWithoutValidationErrors()
		{
			var consol = base.GetBusinessObjectWithoutValidationErrors() as ForwardingConsol;
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
			Factory.Save();
			return consol;
		}
	}
}
