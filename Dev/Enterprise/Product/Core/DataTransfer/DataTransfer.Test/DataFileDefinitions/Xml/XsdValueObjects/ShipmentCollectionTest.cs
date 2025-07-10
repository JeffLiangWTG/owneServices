using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.Testing
{
	[TestedType(typeof(Xsd.ShipmentCollection))]
	sealed class ShipmentCollectionTest : ValueObjectCollectionTestCase
	{
		public void TestGetTotalShipmentWeight()
		{
			Xsd.ShipmentCollection collection = new Xsd.ShipmentCollection();
			AssertEquals("TotalShipmentWeight should be 0", "0", collection.GetTotalShipmentWeight());

			Xsd.Shipment ship1 = collection.AddNew();
			Xsd.Shipment ship2 = collection.AddNew();
			AssertEquals("TotalShipmentWeight should be 0", "0", collection.GetTotalShipmentWeight());

			ship1.ShipmentDetails.Weight.Value = 10.321m;
			ship2.ShipmentDetails.Weight.Value = 20.012m;
			AssertEquals("TotalShipmentWeight should be 30.333", "30.333", collection.GetTotalShipmentWeight());
		}

		public void TestGetTotalShipmentVolume()
		{
			Xsd.ShipmentCollection collection = new Xsd.ShipmentCollection();
			AssertEquals("TotalShipmentVolume should be 0", "0", collection.GetTotalShipmentVolume());

			Xsd.Shipment ship1 = collection.AddNew();
			Xsd.Shipment ship2 = collection.AddNew();
			AssertEquals("TotalShipmentVolume should be 0", "0", collection.GetTotalShipmentVolume());

			ship1.ShipmentDetails.Volume.Value = 10.123m;
			ship2.ShipmentDetails.Volume.Value = 20.210m;
			AssertEquals("TotalShipmentVolume should be 30.333", "30.333", collection.GetTotalShipmentVolume());
		}
	}
}
