using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Moq;
using WTG.ProductionRules.Business.JobBillingDefaulting;

namespace Enterprise.Accounting.RulesEngine.Facts.Testing
{
	public class TransitReceiveConsignmentJobFactTest : JobFactTest
	{
		public void TestPlugInNull_ThrowsException()
		{
			var environmentFactMock = new Mock<IEnvironmentFact>();
			AssertExceptionThrown<ArgumentNullException>(() => new TransitReceiveConsignmentJobFact(null, environmentFactMock.Object));
		}

		public void TestPlugIn_InvoicingSupporterNull_ThrowsException()
		{
			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns((IJobInvoicingSupporter)null);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			AssertExceptionThrown<ArgumentNullException>(() => new TransitReceiveConsignmentJobFact(plugInMock.Object, environmentFactMock.Object));
		}

		public void TestPlugIn_InvoicingSupporter_JobNull_ThrowsException()
		{
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns((JobHeader)null);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			AssertExceptionThrown<ArgumentNullException>(() => new TransitReceiveConsignmentJobFact(plugInMock.Object, environmentFactMock.Object));
		}

		public void TestEnvironmentFactNull_ThrowsException()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			AssertExceptionThrown<ArgumentNullException>(() => new TransitReceiveConsignmentJobFact(plugInMock.Object, null));
		}

		public void TestReceiveConsignmentPK_ValidPK()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var plugInPK = Guid.NewGuid();
			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);
			plugInMock.Setup(x => x.PK).Returns(plugInPK);

			var plugInAsConsignment = plugInMock.As<IConsignment>();
			plugInAsConsignment.Setup(x => x.Warehouse.PK).Returns(Guid.NewGuid());

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var receiveConsignmentJobFact = new TransitReceiveConsignmentJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(plugInPK, receiveConsignmentJobFact.ReceiveConsignmentPK);
		}

		public void TestReceiveConsignmentPK_InValidPK()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var invalidPlugInPK = ZGuid.Invalid;
			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);
			plugInMock.Setup(x => x.PK).Returns(invalidPlugInPK);

			var plugInAsConsignment = plugInMock.As<IConsignment>();
			plugInAsConsignment.Setup(x => x.Warehouse.PK).Returns(Guid.NewGuid());

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var receiveConsignmentJobFact = new TransitReceiveConsignmentJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(Guid.Empty, receiveConsignmentJobFact.ReceiveConsignmentPK);
		}

		public void TestReceiveConsignmentPK_EmptyPK()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);
			plugInMock.Setup(x => x.PK).Returns(Guid.Empty);

			var plugInAsConsignment = plugInMock.As<IConsignment>();
			plugInAsConsignment.Setup(x => x.Warehouse.PK).Returns(Guid.NewGuid());

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var receiveConsignmentJobFact = new TransitReceiveConsignmentJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(Guid.Empty, receiveConsignmentJobFact.ReceiveConsignmentPK);
		}

		public void TestWarehouseCode()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var plugInPK = Guid.NewGuid();
			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);
			plugInMock.Setup(x => x.PK).Returns(plugInPK);

			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "XXX";

			var plugInAsConsignment = plugInMock.As<IConsignment>();
			plugInAsConsignment.Setup(x => x.Warehouse).Returns(warehouse);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var receiveConsignmentJobFact = new TransitReceiveConsignmentJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(warehouse.WW_WarehouseCode, receiveConsignmentJobFact.Warehouse.Fact.Code);
		}

		public void TestDirection()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var plugInPK = Guid.NewGuid();
			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);
			plugInMock.Setup(x => x.PK).Returns(plugInPK);

			var plugInAsConsignment = plugInMock.As<IConsignment>();
			var direction = "DRT";
			plugInAsConsignment.Setup(x => x.Warehouse.PK).Returns(Guid.NewGuid());
			plugInAsConsignment.Setup(x => x.Direction).Returns(direction);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var receiveConsignmentJobFact = new TransitReceiveConsignmentJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(direction, receiveConsignmentJobFact.Direction);
		}

		public void TestTransportMode()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var plugInPK = Guid.NewGuid();
			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);
			plugInMock.Setup(x => x.PK).Returns(plugInPK);

			var plugInAsConsignment = plugInMock.As<IConsignment>();
			var transportMode = "TSM";
			plugInAsConsignment.Setup(x => x.Warehouse.PK).Returns(Guid.NewGuid());
			plugInAsConsignment.Setup(x => x.TransportMode).Returns(transportMode);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var receiveConsignmentJobFact = new TransitReceiveConsignmentJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(transportMode, receiveConsignmentJobFact.TransportMode);
		}

		public void TestJobPlugInNotIConsignment()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var plugInPK = Guid.NewGuid();
			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);
			plugInMock.Setup(x => x.PK).Returns(plugInPK);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var receiveConsignmentJobFact = new TransitReceiveConsignmentJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(Guid.Empty, receiveConsignmentJobFact.ReceiveConsignmentPK);
			AssertNull(receiveConsignmentJobFact.Warehouse.Fact);
			AssertEquals(string.Empty, receiveConsignmentJobFact.Direction);
			AssertEquals(string.Empty, receiveConsignmentJobFact.TransportMode);
		}

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;
	}
}
