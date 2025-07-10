using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.NZ.Testing
{
	[TestedType(typeof(DocShipmentCollection))]
	sealed class DocShipmentCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocShipmentCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var forwardingShipment = Factory.New<ForwardingShipment>();
			return DocShipment.New(forwardingShipment, Factory);
		}

		protected override DocShipmentCollection GetCollectionToTest()
		{
			return new DocShipmentCollection(Factory);
		}

		public void TestCount()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();

			DocShipmentCollection testCollection = new DocShipmentCollection(Factory);
			testCollection.Contruct(consol.Shipments);
			AssertEquals("Three items", 3, testCollection.Count);
		}

		public void TestSort()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S0001000";
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S0001001";
			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_UniqueConsignRef = "S0001002";

			DocShipmentCollection testCollection = new DocShipmentCollection(Factory);
			testCollection.Contruct(consol.Shipments);
			AssertEquals("First Item", shipment1.JS_UniqueConsignRef, testCollection[0].ShipmentNumber);
			AssertEquals("Second Item", shipment2.JS_UniqueConsignRef, testCollection[1].ShipmentNumber);
			AssertEquals("Third Item", shipment3.JS_UniqueConsignRef, testCollection[2].ShipmentNumber);
		}

		public void TestSequence()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S0001000";
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S0001001";
			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_UniqueConsignRef = "S0001002";

			DocShipmentCollection testCollection = new DocShipmentCollection(Factory);
			testCollection.Contruct(consol.Shipments);
			AssertEquals("Shipment 1", 1, testCollection[0].SequenceNumber);
			AssertEquals("Shipment 2", 2, testCollection[1].SequenceNumber);
			AssertEquals("Shipment 3", 3, testCollection[2].SequenceNumber);
		}
	}
}
