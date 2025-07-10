using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAHouseForwardingShipmentCustomsStatusProviderTestCase : TestCaseWithFactory
	{
		public void TestStatuses()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			var shipment = consol.Shipments.AddNew();
			Assert("IsSea needs to return true for this to work", shipment.IsSea);
			var obl = Factory.New<CusSCAOceanBill>();
			obl.CB_ParentId = consol.PK;
			obl.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			var house = obl.HouseBills.AddNew();
			house.CA_JS = shipment.PK;
			Factory.Save();
			house.CA_MessageStatus = "WTO";
			house.CA_ShipmentStatus = "HLD";

			var customsStatusProvider = Enterprise.Customs.Forwarding.Business.ForwardingShipmentCustomsStatusProvider.New(shipment);

			AssertEquals("WTO", customsStatusProvider.CustomsMessageStatus());
			AssertEquals("HLD", customsStatusProvider.CustomsCargoStatus());
		}

		public void TestStatusesWhenNotSea()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Mail;
			var shipment = consol.Shipments.AddNew();
			Assert("IsSea needs to return false for this to work", !shipment.IsSea);
			var obl = Factory.New<CusSCAOceanBill>();
			obl.CB_ParentId = consol.PK;
			obl.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			var house = obl.HouseBills.AddNew();
			house.CA_JS = shipment.PK;
			Factory.Save();
			house.CA_MessageStatus = "WTO";
			house.CA_ShipmentStatus = "HLD";

			var customsStatusProvider = Enterprise.Customs.Forwarding.Business.ForwardingShipmentCustomsStatusProvider.New(shipment);

			Assert("Should be empty", customsStatusProvider.CustomsMessageStatus().IsEmpty);
			Assert("Should be empty", customsStatusProvider.CustomsCargoStatus().IsEmpty);
		}
	}
}
