using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocShipmentCollection))]
	sealed class DocShipmentCollectionTestCase : DocShipmentCollectionTestCase<DocShipmentCollection>
	{
		protected override DocShipmentCollection GetCollectionToTest()
		{
			return new DocShipmentCollection(Factory);
		}
	}

	public abstract class DocShipmentCollectionTestCase<T> : NonPersistentBusinessObjectCollectionTestCase<T> where T : DocShipmentCollection
	{
		public void TestSortOnInterimReceipt()
		{
			DocShipmentCollection collection = new DocShipmentCollection(Factory);

			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "S1";
			shipment1.JS_InterimReceipt = "R1";

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "S3";
			shipment2.JS_InterimReceipt = "R2";

			var shipment3 = Factory.New<ForwardingShipment>();
			shipment3.JS_UniqueConsignRef = "S4";
			shipment3.JS_InterimReceipt = "R3";

			DocShipment shipment1Wrapper = DocShipment.New(shipment1, Factory);
			DocShipment shipment2Wrapper = DocShipment.New(shipment2, Factory);
			DocShipment shipment3Wrapper = DocShipment.New(shipment3, Factory);

			collection.Add(shipment3Wrapper);
			collection.Add(shipment1Wrapper);
			collection.Add(shipment2Wrapper);
			collection.SortOnInterimReceipt();

			AssertEquals(3, collection.Count);
			AssertEquals("S1", collection[0].ShipmentNumber);
			AssertEquals("S3", collection[1].ShipmentNumber);
			AssertEquals("S4", collection[2].ShipmentNumber);

			var shipment4 = Factory.New<ForwardingShipment>();
			shipment4.JS_UniqueConsignRef = "S2";
			shipment4.JS_InterimReceipt = "R2";
			DocShipment shipment4Wrapper = DocShipment.New(shipment4, Factory);

			collection.Add(shipment4Wrapper);
			collection.SortOnInterimReceipt();

			AssertEquals(4, collection.Count);
			AssertEquals("S1", collection[0].ShipmentNumber);
			AssertEquals("S2", collection[1].ShipmentNumber);
			AssertEquals("R2", collection[1].InterimReceipt);
			AssertEquals("S3", collection[2].ShipmentNumber);
			AssertEquals("R2", collection[2].InterimReceipt);
			AssertEquals("S4", collection[3].ShipmentNumber);

			AssertEquals(1, collection[0].SequenceNumber);
			AssertEquals(2, collection[1].SequenceNumber);
			AssertEquals(3, collection[2].SequenceNumber);
			AssertEquals(4, collection[3].SequenceNumber);
		}

		public void TestSortOnHBL()
		{
			var ship1 = Factory.New<ForwardingShipment>();
			var ship2 = Factory.New<ForwardingShipment>();
			ship1.JS_HouseBill = "HSE123";
			ship2.JS_HouseBill = "HSE321";

			DocShipment ship1Wrapper = DocShipment.New(ship1, Factory);
			DocShipment ship2Wrapper = DocShipment.New(ship2, Factory);

			DocShipmentCollection shipmentColl = new DocShipmentCollection(Factory);
			shipmentColl.Add(ship1Wrapper);
			shipmentColl.Add(ship2Wrapper);

			AssertEquals("Sequence", 1, shipmentColl[0].SequenceNumber);
			AssertEquals("HBL", "HSE123", shipmentColl[0].HouseBill);
			AssertEquals("Sequence", 2, shipmentColl[1].SequenceNumber);
			AssertEquals("HBL", "HSE321", shipmentColl[1].HouseBill);

			ship1.JS_HouseBill = "HSE555";
			ship2.JS_HouseBill = "HSE444";
			shipmentColl.SortOnHBL();
			AssertEquals("Sequence", 1, shipmentColl[0].SequenceNumber);
			AssertEquals("HBL", "HSE444", shipmentColl[0].HouseBill);
			AssertEquals("Sequence", 2, shipmentColl[1].SequenceNumber);
			AssertEquals("HBL", "HSE555", shipmentColl[1].HouseBill);
		}

		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var commonShipment = Factory.New<CommonShipment>();
			return DocShipment.New(commonShipment, Factory);
		}

		#endregion
	}
}
