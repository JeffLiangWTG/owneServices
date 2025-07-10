using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	[TestedType(typeof(ShipmentAndBillingDetails))]
	internal class ShipmentAndBillingDetailsTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAlteringConsolShipmentsUpdatesDetails()
		{
			var consol = ObjectCreator.CreateGatewayConsolsAndShipments().gC0002;
			ObjectCreator.CreateJob(consol);
			Factory.Save();
			AssertNotEquals("Pre-requisite: consol has at least one shipment", 0, consol.Shipments.Count);
			var existingShipment = consol.Shipments.First() as ForwardingShipment;
			consol.Shipments.Remove(existingShipment);

			var details = new ShipmentAndBillingDetails(consol);
			AssertEquals("A row is created for every shipment", consol.Shipments.Count, details.DetailsRows.Count);

			AssertEquals("Pre-requisite: A row does not exist for shipment", ZGuid.Empty, details.GetRowPKByRelatedJob(existingShipment.JobNumber));
			var rowCount = details.DetailsRows.Count;

			consol.Shipments.Add(existingShipment);
			AssertEquals("One additional row when shipment added", rowCount + 1, details.DetailsRows.Count);
			AssertNotEquals("Row matching shipment now in details", ZGuid.Empty, details.GetRowPKByRelatedJob(existingShipment.JobNumber));
			rowCount = details.DetailsRows.Count;

			consol.Shipments.Remove(existingShipment);
			AssertEquals("One less row when shipment removed", rowCount - 1, details.DetailsRows.Count);
			AssertEquals("Row matching shipment no longer in details", ZGuid.Empty, details.GetRowPKByRelatedJob(existingShipment.JobNumber));
			rowCount = details.DetailsRows.Count;
		}

		public void TestGetRowPKByRelatedJob()
		{
			var consol = ObjectCreator.CreateGatewayConsolsAndShipments().gC0002;
			ObjectCreator.CreateJob(consol);
			Factory.Save();
			AssertNotEquals("Pre-requisite: consol has at least one shipment", 0, consol.Shipments.Count);
			var details = new ShipmentAndBillingDetails(consol);
			AssertEquals("Returns empty if no matching line", ZGuid.Empty, details.GetRowPKByRelatedJob("Something that isn't a job num"));
			foreach (ForwardingShipment shipment in consol.Shipments)
			{
				var shipmentRow = details.DetailsRows.Cast<ShipmentAndBillingDetailsRow>().First(x => x.RelatedJobNum == shipment.JobNumber);
				AssertEquals("returns PK of row matching job num", shipmentRow.PK, details.GetRowPKByRelatedJob(shipment.JobNumber));
			}
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ShipmentAndBillingDetails(Factory.New<ForwardingConsol>());
		}

		TestObjectCreator ObjectCreator => objectCreator ?? (objectCreator = new TestObjectCreator(Factory));
		TestObjectCreator objectCreator;

		#endregion
	}
}
