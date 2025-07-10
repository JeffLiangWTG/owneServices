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
	public class TransitDispatchTransportationUnitJobFactTest : JobFactTest
	{
		public void TestPlugInNull_ThrowsException()
		{
			var environmentFactMock = new Mock<IEnvironmentFact>();
			AssertExceptionThrown<ArgumentNullException>(() => new TransitDispatchTransportationUnitJobFact(null, environmentFactMock.Object));
		}

		public void TestPlugIn_InvoicingSupporterNull_ThrowsException()
		{
			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns((IJobInvoicingSupporter)null);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			AssertExceptionThrown<ArgumentNullException>(() => new TransitDispatchTransportationUnitJobFact(plugInMock.Object, environmentFactMock.Object));
		}

		public void TestPlugIn_InvoicingSupporter_JobNull_ThrowsException()
		{
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns((JobHeader)null);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			AssertExceptionThrown<ArgumentNullException>(() => new TransitDispatchTransportationUnitJobFact(plugInMock.Object, environmentFactMock.Object));
		}

		public void TestEnvironmentFactNull_ThrowsException()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			AssertExceptionThrown<ArgumentNullException>(() => new TransitDispatchTransportationUnitJobFact(plugInMock.Object, null));
		}

		public void TestDispatchTransportationUnitPK_ValidPK()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var plugInPK = Guid.NewGuid();
			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);
			plugInMock.Setup(x => x.PK).Returns(plugInPK);

			var plugInAsDispatchTransportationUnit = plugInMock.As<IWhsItemDispatchTransportationUnit>();
			plugInAsDispatchTransportationUnit.Setup(x => x.Warehouse.PK).Returns(Guid.NewGuid());

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var dispatchTransportationUnitJobFact = new TransitDispatchTransportationUnitJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(plugInPK, dispatchTransportationUnitJobFact.DispatchTransportationUnitPK);
		}

		public void TestDispatchTransportationUnitPK_InValidPK()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var invalidPlugInPK = ZGuid.Invalid;
			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);
			plugInMock.Setup(x => x.PK).Returns(invalidPlugInPK);

			var plugInAsDispatchTransportationUnit = plugInMock.As<IWhsItemDispatchTransportationUnit>();
			plugInAsDispatchTransportationUnit.Setup(x => x.Warehouse.PK).Returns(Guid.NewGuid());

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var dispatchTransportationUnitJobFact = new TransitDispatchTransportationUnitJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(Guid.Empty, dispatchTransportationUnitJobFact.DispatchTransportationUnitPK);
		}

		public void TestDispatchTransportationUnitPK_EmptyPK()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);
			plugInMock.Setup(x => x.PK).Returns(Guid.Empty);

			var plugInAsDispatchTransportationUnit = plugInMock.As<IWhsItemDispatchTransportationUnit>();
			plugInAsDispatchTransportationUnit.Setup(x => x.Warehouse.PK).Returns(Guid.NewGuid());

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var dispatchTransportationUnitJobFact = new TransitDispatchTransportationUnitJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(Guid.Empty, dispatchTransportationUnitJobFact.DispatchTransportationUnitPK);
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

			var plugInAsDispatchTransportationUnit = plugInMock.As<IWhsItemDispatchTransportationUnit>();
			plugInAsDispatchTransportationUnit.Setup(x => x.Warehouse).Returns(warehouse);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var dispatchTransportationUnitJobFact = new TransitDispatchTransportationUnitJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(warehouse.WW_WarehouseCode, dispatchTransportationUnitJobFact.Warehouse.Fact.Code);
		}

		public void TestTransportMode_HasLatestDispatchConsignment()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var plugInPK = Guid.NewGuid();
			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);
			plugInMock.Setup(x => x.PK).Returns(plugInPK);

			var plugInAsDispatchTransportationUnit = plugInMock.As<IWhsItemDispatchTransportationUnit>();
			var transportMode = "TSM";
			plugInAsDispatchTransportationUnit.Setup(x => x.Warehouse.PK).Returns(Guid.NewGuid());
			plugInAsDispatchTransportationUnit.Setup(x => x.LatestDispatchConsignment.TransportMode).Returns(transportMode);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var dispatchTransportationUnitJobFact = new TransitDispatchTransportationUnitJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(transportMode, dispatchTransportationUnitJobFact.TransportMode);
		}

		public void TestTransportMode_HasNoLatestDispatchConsignment()
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

				var plugInAsDispatchTransportationUnit = plugInMock.As<IWhsItemDispatchTransportationUnit>();
				plugInAsDispatchTransportationUnit.Setup(x => x.Warehouse.PK).Returns(Guid.NewGuid());
				plugInAsDispatchTransportationUnit.Setup(x => x.LatestDispatchConsignment).Returns((IConsignment)null);
				plugInAsDispatchTransportationUnit.Setup(x => x.UnitType).Returns(unitType);

				var environmentFactMock = new Mock<IEnvironmentFact>();

				var dispatchTransportationUnitJobFact = new TransitDispatchTransportationUnitJobFact(plugInMock.Object, environmentFactMock.Object);
				AssertEquals(expectedTransportMode, dispatchTransportationUnitJobFact.TransportMode);
			}
		}

		public void TestJobPlugInNotIWhsItemDispatchTransportationUnit()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var plugInPK = Guid.NewGuid();
			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);
			plugInMock.Setup(x => x.PK).Returns(plugInPK);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var dispatchTransportationUnitJobFact = new TransitDispatchTransportationUnitJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(Guid.Empty, dispatchTransportationUnitJobFact.DispatchTransportationUnitPK);
			AssertNull(dispatchTransportationUnitJobFact.Warehouse.Fact);
			AssertEquals(string.Empty, dispatchTransportationUnitJobFact.TransportMode);
		}

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;
	}
}
