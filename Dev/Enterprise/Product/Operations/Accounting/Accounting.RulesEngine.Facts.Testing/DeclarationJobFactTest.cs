using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Moq;
using WTG.ProductionRules.Business.JobBillingDefaulting;

namespace Enterprise.Accounting.RulesEngine.Facts.Testing
{
	public class DeclarationJobFactTest : JobFactTest
	{
		public void TestPlugInNull_ThrowsException()
		{
			var environmentFactMock = new Mock<IEnvironmentFact>();
			AssertExceptionThrown<ArgumentNullException>(() => new DeclarationJobFact(null, environmentFactMock.Object));
		}

		public void TestPlugIn_InvoicingSupporterNull_ThrowsException()
		{
			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns((IJobInvoicingSupporter)null);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			AssertExceptionThrown<ArgumentNullException>(() => new DeclarationJobFact(plugInMock.Object, environmentFactMock.Object));
		}

		public void TestPlugIn_InvoicingSupporter_JobNull_ThrowsException()
		{
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns((JobHeader)null);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			AssertExceptionThrown<ArgumentNullException>(() => new DeclarationJobFact(plugInMock.Object, environmentFactMock.Object));
		}

		public void TestEnvironmentFactNull_ThrowsException()
		{
			var job = new BusinessObjectFactory().NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			AssertExceptionThrown<ArgumentNullException>(() => new DeclarationJobFact(plugInMock.Object, null));
		}

		public void TestShipmentType()
		{
			var serviceDirection = "ABC";
			var job = new BusinessObjectFactory().NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);
			invoicingSupporterMock.As<IServiceDirection>().Setup(x => x.ServiceDirection).Returns(serviceDirection);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var declarationFact = new DeclarationJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(serviceDirection, declarationFact.ShipmentType);
		}

		public void TestTransportMode()
		{
			var transportMode = "DEF";
			var job = new BusinessObjectFactory().NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);
			invoicingSupporterMock.Setup(x => x.TransportMode).Returns(transportMode);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var declarationFact = new DeclarationJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(transportMode, declarationFact.TransportMode);
		}

		public void TestContainerMode()
		{
			var containerMode = "FCL";
			var job = new BusinessObjectFactory().NewJobForTesting<JobHeader>();
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);
			invoicingSupporterMock.Setup(x => x.ContainerMode).Returns(containerMode);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var environmentFactMock = new Mock<IEnvironmentFact>();

			var declarationFact = new DeclarationJobFact(plugInMock.Object, environmentFactMock.Object);
			AssertEquals(containerMode, declarationFact.ContainerMode);
		}
	}
}
