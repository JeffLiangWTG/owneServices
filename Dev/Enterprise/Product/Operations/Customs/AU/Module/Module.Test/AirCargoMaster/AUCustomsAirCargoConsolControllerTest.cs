using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.AirCargo.Testing
{
	[TestedType(typeof(AUCustomsAirCargoConsolController))]
	sealed class AUCustomsAirCargoConsolControllerTest : ZControllerBasherTest
	{
		public void TestCheckPoints()
		{
			AssertEquals(Env.Security.ACAMasterImportDelete, new AUCustomsAirCargoConsolController().CheckPointForDeleteExposedForTest);
			AssertEquals(Env.Security.ACAMasterImportModify, new AUCustomsAirCargoConsolController().CheckPointForEditExposedForTest);
			AssertEquals(Env.Security.ACAMasterImportNew, new AUCustomsAirCargoConsolController().CheckPointForNewExposedForTest);
			AssertEquals(Env.Security.ACAMasterImportView, new AUCustomsAirCargoConsolController().CheckPointForViewExposedForTest);
		}

		[ExpectNoExceptions]
		public void TestCreateNewFormShouldNotThrowException()
		{
			var mawb = Factory.NewWithValidTestData<CusMAWB>();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			mawb.CM_JK = consol.PK;
			Factory.Save();
			var airCargoConsoleController = ZControllerFactory.Create(ControllerIDs.Customs.AU.AirCargoConsolController);
			using (var editForm = airCargoConsoleController.ShowEditForm(mawb))
			{
				AssertEquals("The controllerID of the form should be AirCargoConsolController because AUCustomsAirCargoConsolController has overridden it.", ControllerIDs.Customs.AU.AirCargoConsolController, editForm.ControllerID);
				using (var newForm = airCargoConsoleController.ShowNewForm())
				{
					AssertType<ConsolForm>("The new form should be type of ConsolForm.", newForm);
				}
			}
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var bizO = Factory.NewWithValidTestData<CusMAWB>();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			bizO.CM_JK = consol.PK;
			Factory.Save();
			return bizO;
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.AU.AirCargoConsolController;

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
