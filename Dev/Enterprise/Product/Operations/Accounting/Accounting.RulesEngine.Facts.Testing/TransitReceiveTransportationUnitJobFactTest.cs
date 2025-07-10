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
	public class TransitReceiveTransportationUnitJobFactTest : JobFactTest
	{
		public void TestPlugInNull_ThrowsException()
		{
			var environmentFactMock = new Mock<IEnvironmentFact>();
			AssertExceptionThrown<ArgumentNullException>(() => new TransitReceiveTransportationUnitJobFact(null, environmentFactMock.Object));
		}

		public void TestPlugIn_InvoicingSupporterNull_ThrowsException()
		{
			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns((IJobInvoicingSupporter)null);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			AssertExceptionThrown<ArgumentNullException>(() => new TransitReceiveTransportationUnitJobFact(plugInMock.Object, environmentFactMock.Object));
		}

		public void TestPlugIn_InvoicingSupporter_JobNull_ThrowsException()
		{
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns((JobHeader)null);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			AssertExceptionThrown<ArgumentNullException>(() => new TransitReceiveTransportationUnitJobFact(plugInMock.Object, environmentFactMock.Object));
		}

		public void TestEnvironmentFactNull_ThrowsException()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			AssertExceptionThrown<ArgumentNullException>(() => new TransitReceiveTransportationUnitJobFact(plugInMock.Object, null));
		}

		public void TestReceiveTransportationUnitPK_ValidPK()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var plugInPK = Guid.NewGuid();
			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);
			plugInMock.Setup(x => x.PK).Returns(plugInPK);

			var plugInAsReceiveTransportationUnit = plugInMock.As<IWhsItemReceiveTransportationUnit>();
			plugInAsReceiveTransportationUnit.Setup(x => x.Warehouse.PK).Returns(Guid.NewGuid());

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var receiveTransportationUnitJobFact = new TransitReceiveTransportationUnitJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(plugInPK, receiveTransportationUnitJobFact.ReceiveTransportationUnitPK);
		}

		public void TestReceiveTransportationUnitPK_InValidPK()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var invalidPlugInPK = ZGuid.Invalid;
			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);
			plugInMock.Setup(x => x.PK).Returns(invalidPlugInPK);

			var plugInAsReceiveTransportationUnit = plugInMock.As<IWhsItemReceiveTransportationUnit>();
			plugInAsReceiveTransportationUnit.Setup(x => x.Warehouse.PK).Returns(Guid.NewGuid());

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var receiveTransportationUnitJobFact = new TransitReceiveTransportationUnitJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(Guid.Empty, receiveTransportationUnitJobFact.ReceiveTransportationUnitPK);
		}

		public void TestReceiveTransportationUnitPK_EmptyPK()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);
			plugInMock.Setup(x => x.PK).Returns(Guid.Empty);

			var plugInAsReceiveTransportationUnit = plugInMock.As<IWhsItemReceiveTransportationUnit>();
			plugInAsReceiveTransportationUnit.Setup(x => x.Warehouse.PK).Returns(Guid.NewGuid());

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var receiveTransportationUnitJobFact = new TransitReceiveTransportationUnitJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(Guid.Empty, receiveTransportationUnitJobFact.ReceiveTransportationUnitPK);
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

			var plugInAsReceiveTransportationUnit = plugInMock.As<IWhsItemReceiveTransportationUnit>();
			plugInAsReceiveTransportationUnit.Setup(x => x.Warehouse).Returns(warehouse);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var receiveTransportationUnitJobFact = new TransitReceiveTransportationUnitJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(warehouse.WW_WarehouseCode, receiveTransportationUnitJobFact.Warehouse.Fact.Code);
		}

		public void TestTransportMode_HasLatestReceiveConsignment()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var plugInPK = Guid.NewGuid();
			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);
			plugInMock.Setup(x => x.PK).Returns(plugInPK);

			var plugInAsReceiveTransportationUnit = plugInMock.As<IWhsItemReceiveTransportationUnit>();
			var transportMode = "TSM";
			plugInAsReceiveTransportationUnit.Setup(x => x.Warehouse.PK).Returns(Guid.NewGuid());
			plugInAsReceiveTransportationUnit.Setup(x => x.LatestReceiveConsignment.TransportMode).Returns(transportMode);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var receiveTransportationUnitJobFact = new TransitReceiveTransportationUnitJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(transportMode, receiveTransportationUnitJobFact.TransportMode);
		}

		public void TestTransportMode_HasNoLatestReceiveConsignment()
		{
			TestCase("CNT", "SEA");
			TestCase("ULD", "AIR");
			TestCase("VEH", "ROA");

			void TestCase(string unitType, string expectedTransportMode)
			{
				var job = Factory.NewJobForTesting<JobHeader>();
				var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
				invoicingSupporterMock.Setup(x => x.Job).Returns(job);

				var plugInPK = Guid.NewGuid();
				var plugInMock = new Mock<IJobInvoicingPlugIn>();
				plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);
				plugInMock.Setup(x => x.PK).Returns(plugInPK);

				var plugInAsReceiveTransportationUnit = plugInMock.As<IWhsItemReceiveTransportationUnit>();
				plugInAsReceiveTransportationUnit.Setup(x => x.Warehouse.PK).Returns(Guid.NewGuid());
				plugInAsReceiveTransportationUnit.Setup(x => x.LatestReceiveConsignment).Returns((IConsignment)null);
				plugInAsReceiveTransportationUnit.Setup(x => x.UnitType).Returns(unitType);

				var environmentFactMock = new Mock<IEnvironmentFact>();

				var receiveTransportationUnitJobFact = new TransitReceiveTransportationUnitJobFact(plugInMock.Object, environmentFactMock.Object);
				AssertEquals(expectedTransportMode, receiveTransportationUnitJobFact.TransportMode);
			}
		}

		public void TestJobPlugInNotIWhsItemReceiveTransportationUnit()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var plugInPK = Guid.NewGuid();
			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);
			plugInMock.Setup(x => x.PK).Returns(plugInPK);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var receiveTransportationUnitJobFact = new TransitReceiveTransportationUnitJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(Guid.Empty, receiveTransportationUnitJobFact.ReceiveTransportationUnitPK);
			AssertNull(receiveTransportationUnitJobFact.Warehouse.Fact);
			AssertEquals(string.Empty, receiveTransportationUnitJobFact.TransportMode);
		}

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;
	}
}
