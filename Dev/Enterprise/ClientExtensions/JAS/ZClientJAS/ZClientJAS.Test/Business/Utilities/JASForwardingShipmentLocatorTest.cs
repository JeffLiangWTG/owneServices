using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.Utilities.Testing
{
	internal class JASForwardingShipmentLocatorTest : TestCaseWithFactory
	{
		public void TestFind()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			AssertNull(ShipmentLocator.Find(consol, "HB101", "", "", ""));
			JASForwardingShipment shipment1 = (JASForwardingShipment)consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HB101";
			AssertEquals(shipment1.PK, ShipmentLocator.Find(consol, "HB101", "", "", "").PK);
			JASForwardingShipment shipment2 = CreateShipmentForTest("HB102", Core.Constants.TransportModes.Air, "GBLON", "ITMIL", new ZDateTime(2005, 1, 12));
			JASForwardingShipment shipment3 = CreateShipmentForTest("HB102", Core.Constants.TransportModes.Sea, "GBLON", "ITMIL", new ZDateTime(2005, 1, 12));
			JASForwardingShipment shipment4 = CreateShipmentForTest("HB102", Core.Constants.TransportModes.Air, "GBLON", "HKHKG", new ZDateTime(2005, 1, 12));
			JASForwardingShipment shipment5 = CreateShipmentForTest("HB102", Core.Constants.TransportModes.Sea, "GBLON", "HKHKG", new ZDateTime(2005, 1, 13));
			AssertNull("Not enough info", ShipmentLocator.Find(consol, "HB102", "", "", ""));
			AssertNull("Not enough info", ShipmentLocator.Find(consol, "HB102", Core.Constants.TransportModes.Air, "", ""));
			AssertNull("Not enough info", ShipmentLocator.Find(consol, "HB102", Core.Constants.TransportModes.Air, "GBLON", ""));
			AssertEquals(shipment2.PK, ShipmentLocator.Find(consol, "HB102", Core.Constants.TransportModes.Air, "GBDXY", "ITVAL").PK);
			AssertEquals(shipment3.PK, ShipmentLocator.Find(consol, "HB102", Core.Constants.TransportModes.Sea, "GBNPO", "ITMIL").PK);
		}

		public void TestFind_NullParam()
		{
			JASForwardingConsol consol = null;
			AssertNull(ShipmentLocator.Find(consol, "", "", "", ""));
			BusinessObjectFactory factory = null;
			AssertNull(ShipmentLocator.Find(factory, "", "", "", ""));
		}

		public void TestFind_ShouldNotTryToFindIfHouseBillNumberIsEmpty()
		{
			JASForwardingShipment shipment = ShipmentLocator.Find(Factory, "", Core.Constants.TransportModes.Air, "AUSYD", "GBLON");
			AssertNull("Should not attempt to find shipment if HouseBillNumber is not specified", shipment);
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			shipment = ShipmentLocator.Find(consol, "", Core.Constants.TransportModes.Air, "AUSYD", "GBLON");
			AssertNull("Should not attempt to find shipment if HouseBillNumber is not specified", shipment);
		}

		public void TestFind_FromFactory()
		{
			JASForwardingShipment shipment = CreateShipmentForTest("HB102", Core.Constants.TransportModes.Air, "GB", "IT", new ZDateTime(2005, 1, 12));
			AssertEquals(shipment.PK, ShipmentLocator.Find(Factory, "HB102", Core.Constants.TransportModes.Air, "GBLON", "ITMIL").PK);
		}

		JASForwardingShipment CreateShipmentForTest(ZString houseBillNumber, ZString transportMode, ZString origin, ZString destination, ZDateTime eTD)
		{
			JASForwardingShipment result = Factory.New<JASForwardingShipment>();
			result.JS_HouseBill = houseBillNumber;
			result.JS_TransportMode = transportMode;
			result.JS_RL_NKOrigin = origin;
			result.JS_RL_NKDestination = destination;
			result.JS_E_DEP = eTD;
			return result;
		}

		JASForwardingShipmentLocator ShipmentLocator
		{
			get
			{
				if (fShipmentLocator == null)
				{
					fShipmentLocator = new JASForwardingShipmentLocator();
				}

				return fShipmentLocator;
			}
		}

		JASForwardingShipmentLocator fShipmentLocator;
	}
}
