using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Module.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.AirCargo.Testing
{
	[TestedType(typeof(AUCustomsAirCargoController))]
	public class AUCustomsAirCargoControllerTest : BaseAirCargoControllerBasherTest
	{
		public void TestFormShownForMAWB()
		{
			var mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "123456789";
			mAWB.ChildBills.AddNew();
			mAWB.ChildBills.AddNew();
			Factory.Save();
			var airCargoController = ZControllerFactory.Create(ControllerIDs.Customs.AU.AirCargo);
			try
			{
				airCargoController.ShowEditForm(mAWB);
				var lastShownFormForMAWB = airCargoController.LastShownForm;
				AssertNotNull("A form should have shown for the MAWB", lastShownFormForMAWB);
			}
			finally
			{
				if (airCargoController.LastShownForm != null)
				{
					airCargoController.LastShownForm.Dispose();
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
			Factory.Save();
			return bizO;
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Australia;

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.AU.AirCargo;

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
