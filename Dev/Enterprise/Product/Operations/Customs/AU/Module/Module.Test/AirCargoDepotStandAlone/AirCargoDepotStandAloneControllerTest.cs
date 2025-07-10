using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.AirCargo.GUI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.AirCargo.Testing
{
	[TestedType(typeof(AirCargoDepotStandAloneController))]
	sealed class AirCargoDepotStandAloneControllerTest : ZControllerBasherTest
	{
		public void TestCheckPoints()
		{
			AssertEquals(Env.Security.ACAMasterImportDelete, new AirCargoDepotStandAloneController().CheckPointForDeleteExposedForTest);
			AssertEquals(Env.Security.ACAMasterImportModify, new AirCargoDepotStandAloneController().CheckPointForEditExposedForTest);
			AssertEquals(Env.Security.ACAMasterImportNew, new AirCargoDepotStandAloneController().CheckPointForNewExposedForTest);
			AssertEquals(Env.Security.ACAMasterImportView, new AirCargoDepotStandAloneController().CheckPointForViewExposedForTest);
		}

		public void TestGetForm()
		{
			using (var form = ((ZControllerInternals)new AirCargoDepotStandAloneController()).GetForm(Factory.New<CusMAWB>()))
			{
				AssertType<AirCargoMasterForm>(form);
			}
		}

		public void TestAirDepotModuleID()
		{
			var controller = new AirCargoDepotStandAloneController();
			AssertEquals(ModuleIDs.Customs.AU.AirCargoDepot, controller.ModuleID);
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			var controller = new AirCargoDepotStandAloneController();
			AssertEquals("Should return a CusMAWB type", typeof(CusMAWB), controller.TypeOfTopLevelBusinessObject);
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var bizO = Factory.NewWithValidTestData<CusMAWB>();
			Factory.Save();
			return bizO;
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Australia;

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.AU.AirCargoDepot;

		protected override Type GetBusinessObjectType() => typeof(CusHAWB);

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
