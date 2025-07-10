using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	[TestedType(typeof(ShipmentAndBillingDetailsCollection))]
	internal class ShipmentAndBillingDetailsCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ShipmentAndBillingDetailsCollection>
	{
		public void TestRowsAreAddedAndRemovedCorrectly()
		{
			var consol = ObjectCreator.CreateGatewayConsolsAndShipments().gC0002;
			ObjectCreator.CreateJob(consol);
			Factory.Save();
			AssertNotEquals("Pre-requisite: consol has at least one shipment", 0, consol.Shipments.Count);
			var existingShipment = consol.Shipments.First() as ForwardingShipment;
			consol.Shipments.Remove(existingShipment);

			var detailsCollection = new ShipmentAndBillingDetailsCollection(consol);
			AssertEquals("A row is created for every shipment", consol.Shipments.Count, detailsCollection.Count);

			Assert("Pre-requisite: A row does not exist for shipment", detailsCollection.Cast<ShipmentAndBillingDetailsRow>().All(x => x.RelatedJobNum != existingShipment.JobNumber));
			var rowCount = detailsCollection.Count;

			detailsCollection.AddRowForShipment(existingShipment);
			AssertEquals("One additional row when shipment added", rowCount + 1, detailsCollection.Count);
			Assert("Row matching shipment now in details", detailsCollection.Cast<ShipmentAndBillingDetailsRow>().Any(x => x.RelatedJobNum == existingShipment.JobNumber));
			rowCount = detailsCollection.Count;

			detailsCollection.RemoveRowForShipment(existingShipment);
			AssertEquals("One less row when shipment removed", rowCount - 1, detailsCollection.Count);
			Assert("Row matching shipment no longer in details", detailsCollection.Cast<ShipmentAndBillingDetailsRow>().All(x => x.RelatedJobNum != existingShipment.JobNumber));
			rowCount = detailsCollection.Count;

			var newShipment = ObjectCreator.CreateShipment("S102030", consol);
			detailsCollection.AddRowForShipment(newShipment);
			ObjectCreator.CreateJob(newShipment);
			Assert("Pre-requisite: new shipment is not saved", !newShipment.IsInDatabase);
			AssertEquals("No Change in row count", rowCount, detailsCollection.Count);
			Assert("Row matching shipment does not exist", detailsCollection.Cast<ShipmentAndBillingDetailsRow>().All(x => x.RelatedJobNum != newShipment.JobNumber));

			Factory.Save();
			AssertEquals("One additional row when new shipment saved", rowCount + 1, detailsCollection.Count);
			Assert("Row matching shipment now in details", detailsCollection.Cast<ShipmentAndBillingDetailsRow>().Any(x => x.RelatedJobNum == newShipment.JobNumber));
			rowCount = detailsCollection.Count;

			newShipment = ObjectCreator.CreateShipment("S102031", consol);
			detailsCollection.AddRowForShipment(newShipment);
			ObjectCreator.CreateJob(newShipment);
			Assert("Pre-requisite: new shipment is not saved", !newShipment.IsInDatabase);
			AssertEquals("No Change in row count", rowCount, detailsCollection.Count);
			Assert("Row matching shipment does not exist", detailsCollection.Cast<ShipmentAndBillingDetailsRow>().All(x => x.RelatedJobNum != newShipment.JobNumber));

			detailsCollection.RemoveRowForShipment(newShipment);
			AssertEquals("No Change in row count", rowCount, detailsCollection.Count);
			Assert("Row matching shipment does not exist", detailsCollection.Cast<ShipmentAndBillingDetailsRow>().All(x => x.RelatedJobNum != newShipment.JobNumber));

			Factory.Save();
			AssertEquals("No Change in row count", rowCount, detailsCollection.Count);
			Assert("Row matching shipment does not exist", detailsCollection.Cast<ShipmentAndBillingDetailsRow>().All(x => x.RelatedJobNum != newShipment.JobNumber));

			newShipment = ObjectCreator.CreateShipment("S102032", consol);
			detailsCollection.AddRowForShipment(newShipment);
			ObjectCreator.CreateJob(newShipment);

			AssertNoExceptionThrown("When Shipment is deleted, no exceptions are thrown", () =>
			{
				newShipment.Job.Delete();
				newShipment.Delete();
				Factory.Save();
				AssertEquals("No Change in row count", rowCount, detailsCollection.Count);
			});
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ShipmentAndBillingDetailsRow();
		}

		protected override ShipmentAndBillingDetailsCollection GetCollectionToTest()
		{
			return new ShipmentAndBillingDetailsCollection(Factory.New<ForwardingConsol>());
		}

		TestObjectCreator ObjectCreator => objectCreator ?? (objectCreator = new TestObjectCreator(Factory));
		TestObjectCreator objectCreator;
	}
}
