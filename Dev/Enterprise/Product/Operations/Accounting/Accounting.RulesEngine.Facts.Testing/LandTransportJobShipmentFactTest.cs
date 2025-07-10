using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Core;
using static Enterprise.Core.Constants.FreightShipmentDirection.Code;

namespace Enterprise.Accounting.RulesEngine.Facts.Testing
{
	public class LandTransportJobShipmentFactTest : TestCase
	{
		public void TestPlugIn_InvoicingSupporterNull_ThrowsException()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new LandTransportJobShipmentFact(null));
		}

		public void TestPK()
		{
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();

			var shipmentFact = new LandTransportJobShipmentFact(invoicingSupporterMock.Object);
			AssertNotNull(shipmentFact.PK);
		}

		public void TestTransportMode()
		{
			var transportMode = "YYY";
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.TransportMode).Returns(transportMode);

			var shipmentFact = new LandTransportJobShipmentFact(invoicingSupporterMock.Object);
			AssertEquals(transportMode, shipmentFact.TransportMode);
		}

		public void TestJobDirection_Export()
		{
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.IsExport).Returns(true);

			var shipmentFact = new LandTransportJobShipmentFact(invoicingSupporterMock.Object);
			AssertEquals(Export, shipmentFact.JobDirection);
		}

		public void TestJobDirection_Import()
		{
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.IsImport).Returns(true);

			var shipmentFact = new LandTransportJobShipmentFact(invoicingSupporterMock.Object);
			AssertEquals(Import, shipmentFact.JobDirection);
		}

		public void TestJobDirection_CrossTrade()
		{
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.IsCrossTrade).Returns(true);

			var shipmentFact = new LandTransportJobShipmentFact(invoicingSupporterMock.Object);
			AssertEquals(CrossTrade, shipmentFact.JobDirection);
		}

		public void TestJobDirection_Domestic()
		{
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.IsDomestic).Returns(true);

			var shipmentFact = new LandTransportJobShipmentFact(invoicingSupporterMock.Object);
			AssertEquals(Domestic, shipmentFact.JobDirection);
		}

		public void TestJobDirection_Empty()
		{
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.IsExport).Returns(false);
			invoicingSupporterMock.Setup(x => x.IsImport).Returns(false);
			invoicingSupporterMock.Setup(x => x.IsCrossTrade).Returns(false);
			invoicingSupporterMock.Setup(x => x.IsDomestic).Returns(false);

			var shipmentFact = new LandTransportJobShipmentFact(invoicingSupporterMock.Object);
			AssertEquals(string.Empty, shipmentFact.JobDirection);
		}

		public void TestOrigin()
		{
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Origin).Returns((RefUNLOCO)null);

			var originFactMock = new Mock<IUNLOCOFact>();

			var originPort = Factory.New<RefUNLOCO>();
			originPort.RL_RN_NKCountryCode = "AU";
			invoicingSupporterMock.Setup(x => x.Origin).Returns(originPort);

			var shipmentFact = new LandTransportJobShipmentFact(invoicingSupporterMock.Object, originFact: originFactMock.Object);
			AssertNotNull(shipmentFact.Origin);
			AssertType<FactLeftJoin<IUNLOCOFact>>(shipmentFact.Origin);
			AssertNotNull(shipmentFact.Origin.Fact);

			shipmentFact = new LandTransportJobShipmentFact(invoicingSupporterMock.Object, originFact: null);
			AssertNotNull(shipmentFact.Origin);
			AssertType<FactLeftJoin<IUNLOCOFact>>(shipmentFact.Origin);
			AssertNull(shipmentFact.Origin.Fact);
		}

		public void TestDestination()
		{
			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Destination).Returns((RefUNLOCO)null);

			var destinationFactMock = new Mock<IUNLOCOFact>();

			var destinationPort = Factory.New<RefUNLOCO>();
			destinationPort.RL_RN_NKCountryCode = "AU";
			invoicingSupporterMock.Setup(x => x.Destination).Returns(destinationPort);

			var shipmentFact = new LandTransportJobShipmentFact(invoicingSupporterMock.Object, destinationFact: destinationFactMock.Object);
			AssertNotNull(shipmentFact.Destination);
			AssertType<FactLeftJoin<IUNLOCOFact>>(shipmentFact.Destination);
			AssertNotNull(shipmentFact.Destination.Fact);

			shipmentFact = new LandTransportJobShipmentFact(invoicingSupporterMock.Object, destinationFact: null);
			AssertNotNull(shipmentFact.Destination);
			AssertType<FactLeftJoin<IUNLOCOFact>>(shipmentFact.Destination);
			AssertNull(shipmentFact.Destination.Fact);
		}

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;
	}
}
