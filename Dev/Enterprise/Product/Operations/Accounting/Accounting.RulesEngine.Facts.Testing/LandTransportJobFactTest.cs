using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Moq;
using WTG.ProductionRules.Business.JobBillingDefaulting;
using WTG.ProductionRules.Business.JobBillingDefaulting.Facts;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.RulesEngine.Facts.Testing
{
	public class LandTransportJobFactTest : JobFactTest
	{
		public void TestPlugInNull_ThrowsException()
		{
			var environmentFactMock = new Mock<IEnvironmentFact>();
			var warehouseFact = new Mock<ILandTransportJobWarehouseFact>();
			var shipmentFact = new Mock<ILandTransportJobShipmentFact>();
			AssertExceptionThrown<ArgumentNullException>(() => new LandTransportJobFact(null, environmentFactMock.Object, shipmentFact.Object, warehouseFact.Object));
		}

		public void TestPlugIn_InvoicingSupporterNull_ThrowsException()
		{
			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns((IJobInvoicingSupporter)null);

			var environmentFactMock = new Mock<IEnvironmentFact>();
			var warehouseFact = new Mock<ILandTransportJobWarehouseFact>();
			var shipmentFact = new Mock<ILandTransportJobShipmentFact>();

			AssertExceptionThrown<ArgumentNullException>(() => new LandTransportJobFact(plugInMock.Object, environmentFactMock.Object, shipmentFact.Object, warehouseFact.Object));
		}

		public void TestPlugIn_InvoicingSupporter_JobNull_ThrowsException()
		{
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns((JobHeader)null);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();
			var warehouseFact = new Mock<ILandTransportJobWarehouseFact>();
			var shipmentFact = new Mock<ILandTransportJobShipmentFact>();

			AssertExceptionThrown<ArgumentNullException>(() => new LandTransportJobFact(plugInMock.Object, environmentFactMock.Object, shipmentFact.Object, warehouseFact.Object));
		}

		public void TestParentWarehouseOrder()
		{
			var landTransportJobFact = CreateTestLandTransportJobFact();
			AssertNotNull(landTransportJobFact.ParentWarehouseOrder);
			AssertType<FactLeftJoin<ILandTransportJobWarehouseFact>>(landTransportJobFact.ParentWarehouseOrder);
		}

		public void TestParentForwardingShipment()
		{
			var landTransportJobFact = CreateTestLandTransportJobFact();
			AssertNotNull(landTransportJobFact.ParentForwardingShipment);
			AssertType<FactLeftJoin<ILandTransportJobShipmentFact>>(landTransportJobFact.ParentForwardingShipment);
		}

		LandTransportJobFact CreateTestLandTransportJobFact()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();
			var warehouseFact = new Mock<ILandTransportJobWarehouseFact>();
			var shipmentFact = new Mock<ILandTransportJobShipmentFact>();

			return new LandTransportJobFact(
				plugInMock.Object,
				environmentFactMock.Object,
				shipmentFact: shipmentFact.Object,
				warehouseFact: warehouseFact.Object);
		}

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;
	}
}
