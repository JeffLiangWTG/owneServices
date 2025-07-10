using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(SeaCargoHouseShipmentController))]
	sealed class SeaCargoHouseShipmentControllerTest : ZControllerBasherTest
	{
		public void TestTypeOfTopLevelBusinessObject()
		{
			var testController = new SeaCargoHouseShipmentController();
			AssertEquals(typeof(CusSCAHouse), testController.TypeOfTopLevelBusinessObject);
		}

		public override void TestViewForm()
		{
			base.TestViewForm();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			var transport = consol.Transports[0];
			transport.JW_RL_NKDiscPort = "AUSYD";
			var shipment = consol.Shipments.AddNew();
			var houseBill = Factory.NewWithValidTestData<CusSCAHouse>();
			houseBill.CA_JS = shipment.PK;
			Factory.Save();
			var controller = ZControllerFactory.Create(ControllerIDs.Customs.AU.SeaCargoHouseShipmentController);
			using (var jobForm = controller.ShowViewForm(houseBill))
			{
				AssertType(typeof(ShipmentForm), jobForm);
			}
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.AU.SeaCargoHouseShipmentController;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			var transport = consol.Transports[0];
			transport.JW_RL_NKDiscPort = "AUSYD";
			var shipment = consol.Shipments.AddNew();
			var houseBill = Factory.NewWithValidTestData<CusSCAHouse>();
			houseBill.CA_JS = shipment.PK;
			Factory.Save();
			return houseBill;
		}
	}
}
