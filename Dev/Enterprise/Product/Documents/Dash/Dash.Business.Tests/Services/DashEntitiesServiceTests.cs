using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Dash.Business.Services;
using Enterprise.DocumentScanning.Integration;
using Moq;
using SharedConstants = WTG.Shared.Dash.Common.Constants;

namespace Enterprise.Dash.Business.Tests
{
	public class DashEntitiesServiceTests : TestCaseWithFactory
	{
		public void TestLoadAPInvoices()
		{
			// arrange
			var dashDocument1 = Factory.NewWithValidTestData<DashDocument>();
			dashDocument1.DDD_ParseType = SharedConstants.ParseType.Code.AccountPayableInvoice;
			dashDocument1.DDD_ParseStatus = SharedConstants.ParseStatus.Code.ReadyForThirdPartyProcessing;

			var dashDocument2 = Factory.NewWithValidTestData<DashDocument>();
			dashDocument2.DDD_ParseType = SharedConstants.ParseType.Code.AccountPayableInvoice;
			dashDocument2.DDD_ParseStatus = SharedConstants.ParseStatus.Code.ReadyForThirdPartyProcessing;

			var dashDocument3 = Factory.NewWithValidTestData<DashDocument>();
			dashDocument3.DDD_ParseType = SharedConstants.ParseType.Code.BillOfLading;
			dashDocument3.DDD_ParseStatus = SharedConstants.ParseStatus.Code.ReadyForThirdPartyProcessing;

			var dashDocument4 = Factory.NewWithValidTestData<DashDocument>();
			dashDocument4.DDD_ParseType = SharedConstants.ParseType.Code.AccountPayableInvoice;
			dashDocument4.DDD_ParseStatus = SharedConstants.ParseStatus.Code.Processing;

			var apInvoice1 = Factory.NewWithValidTestData<DashAPInvoice>();
			apInvoice1.DPI_DDD_DashDocID = dashDocument1.PK;
			var apInvoice2 = Factory.NewWithValidTestData<DashAPInvoice>();
			apInvoice2.DPI_DDD_DashDocID = dashDocument2.PK;

			var apInvoiceCluster1 = Factory.NewWithValidTestData<DashAPInvoiceCluster>();
			apInvoiceCluster1.DPC_DPI_HeaderID = apInvoice1.PK;
			var apInvoiceCluster2 = Factory.NewWithValidTestData<DashAPInvoiceCluster>();
			apInvoiceCluster2.DPC_DPI_HeaderID = apInvoice2.PK;

			var apInvoiceChargeLine1 = Factory.NewWithValidTestData<DashAPInvoiceChargeLine>();
			apInvoiceChargeLine1.DPL_DPC_ClusterID = apInvoiceCluster1.PK;
			var apInvoiceChargeLine2 = Factory.NewWithValidTestData<DashAPInvoiceChargeLine>();
			apInvoiceChargeLine2.DPL_DPC_ClusterID = apInvoiceCluster1.PK;
			var apInvoiceChargeLine3 = Factory.NewWithValidTestData<DashAPInvoiceChargeLine>();
			apInvoiceChargeLine3.DPL_DPC_ClusterID = apInvoiceCluster2.PK;

			var apInvoiceRef1 = Factory.NewWithValidTestData<DashAPInvoiceRef>();
			apInvoiceRef1.DPR_DPI_HeaderID = apInvoice1.PK;
			var apInvoiceRef2 = Factory.NewWithValidTestData<DashAPInvoiceRef>();
			apInvoiceRef2.DPR_DPI_HeaderID = apInvoice1.PK;
			var apInvoiceRef3 = Factory.NewWithValidTestData<DashAPInvoiceRef>();
			apInvoiceRef3.DPR_DPI_HeaderID = apInvoice2.PK;

			var apInvoiceChargeLineRef1 = Factory.NewWithValidTestData<DashAPInvoiceChargeLineRef>();
			apInvoiceChargeLineRef1.DLR_DPL_ChargeLineID = apInvoiceChargeLine1.PK;
			apInvoiceChargeLineRef1.DLR_DPR_RefID = apInvoiceRef1.PK;
			var apInvoiceChargeLineRef2 = Factory.NewWithValidTestData<DashAPInvoiceChargeLineRef>();
			apInvoiceChargeLineRef2.DLR_DPL_ChargeLineID = apInvoiceChargeLine2.PK;
			apInvoiceChargeLineRef2.DLR_DPR_RefID = apInvoiceRef2.PK;

			var apInvoiceClusterRef1 = Factory.NewWithValidTestData<DashAPInvoiceClusterRef>();
			apInvoiceClusterRef1.DRC_DPC_ClusterID = apInvoiceCluster2.PK;
			apInvoiceClusterRef1.DRC_DPR_RefID = apInvoiceRef3.PK;

			Factory.Save();

			var shipamaxServiceMock = new Mock<IShipamaxService>();
			var dashErrorReporterMock = new Mock<IDashErrorReporter>();
			var dashEntitiesService = new DashEntitiesService(shipamaxServiceMock.Object, dashErrorReporterMock.Object);

			// act
			var apInvoices = dashEntitiesService.LoadAPInvoices([dashDocument1.PK, dashDocument2.PK]);

			// assert

			// assert APInvoice1
			AssertEquals(2, apInvoices.Length);

			var loadedAPInvoice1 = apInvoices.FirstOrDefault(x => x.PK == apInvoice1.PK);
			var loadedAPInvoice2 = apInvoices.FirstOrDefault(x => x.PK == apInvoice2.PK);

			AssertNotNull(loadedAPInvoice1);
			AssertNotNull(loadedAPInvoice2);

			// assert APInvoice1->APInvoiceClusters
			AssertEquals(1, apInvoice1.DashAPInvoiceClusters.Count);
			var loadedAPInvoiceCluster1 = apInvoice1.DashAPInvoiceClusters[0];
			AssertEquals(apInvoiceCluster1.PK, loadedAPInvoiceCluster1.PK);

			// assert APInvoice1->APInvoiceClusters->APInvoiceChargeLiens
			var loadedCluster1ChargeLines = loadedAPInvoiceCluster1.DashAPInvoiceChargeLines;
			AssertEquals(2, loadedCluster1ChargeLines.Count);

			var loadedCluster1ChargeLine1 = loadedAPInvoiceCluster1.DashAPInvoiceChargeLines.FirstOrDefault(x => x.PK == apInvoiceChargeLine1.PK) as DashAPInvoiceChargeLine;
			var loadedCluster1ChargeLine2 = loadedAPInvoiceCluster1.DashAPInvoiceChargeLines.FirstOrDefault(x => x.PK == apInvoiceChargeLine2.PK) as DashAPInvoiceChargeLine;

			AssertNotNull(loadedCluster1ChargeLine1);
			AssertNotNull(loadedCluster1ChargeLine2);

			// assert APInvoice1->APInvoiceClusters->APInvoiceChargeLiens->APInvoiceChargeLineRefs
			var loadedAPInvoiceCluster1ChargeLine1Refs = loadedCluster1ChargeLine1.DashAPInvoiceChargeLineRefs;
			AssertEquals(1, loadedAPInvoiceCluster1ChargeLine1Refs.Count);

			var loadedAPInvoiceClusterChargeLineRef1 = loadedAPInvoiceCluster1ChargeLine1Refs[0];
			AssertEquals(apInvoiceChargeLineRef1.PK, loadedAPInvoiceClusterChargeLineRef1.PK);
			AssertEquals(apInvoiceRef1.PK, loadedAPInvoiceClusterChargeLineRef1.DashAPInvoiceRef.PK);

			var loadedAPInvoiceCluster1ChargeLine2Refs = loadedCluster1ChargeLine2.DashAPInvoiceChargeLineRefs;
			AssertEquals(1, loadedAPInvoiceCluster1ChargeLine2Refs.Count);

			var loadedAPInvoiceClusterChargeLineRef2 = loadedAPInvoiceCluster1ChargeLine2Refs[0];
			AssertEquals(apInvoiceChargeLineRef2.PK, loadedAPInvoiceClusterChargeLineRef2.PK);
			AssertEquals(apInvoiceRef2.PK, loadedAPInvoiceClusterChargeLineRef2.DashAPInvoiceRef.PK);

			// assert APInvoice2->APInvoiceClusters
			AssertEquals(1, apInvoice2.DashAPInvoiceClusters.Count);
			var loadedAPInvoiceCluster2 = apInvoice2.DashAPInvoiceClusters[0];
			AssertEquals(apInvoiceCluster2.PK, loadedAPInvoiceCluster2.PK);

			// assert APInvoice2->APInvoiceClusters->APInvoiceChargeLines
			var loadedCluster2ChargeLines = loadedAPInvoiceCluster2.DashAPInvoiceChargeLines;
			AssertEquals(2, loadedCluster1ChargeLines.Count);

			// assert APInvoice2->APInvoiceClusters->APInvoiceClusterRefs
			var loadedAPInvoiceCluster2Refs = apInvoiceCluster2.DashAPInvoiceClusterRefs;
			AssertEquals(1, loadedAPInvoiceCluster2Refs.Count);

			var loadedAPInvoiceClusterRef1 = loadedAPInvoiceCluster2Refs[0];
			AssertEquals(apInvoiceClusterRef1.PK, loadedAPInvoiceClusterRef1.PK);
			AssertEquals(apInvoiceRef3.PK, loadedAPInvoiceClusterRef1.DashAPInvoiceRef.PK);
		}

		public void TestUpdateStatusToComplete_Factory_Save_Is_Not_Called()
		{
			var docId = ZGuid.NewZGuid();
			var docToken = ZGuid.NewZGuid().ToString();
			var dashDocument = Factory.NewWithValidTestData<DashDocument>();
			dashDocument.DDD_ParseStatus = SharedConstants.ParseStatus.Code.ReadyForThirdPartyProcessing;
			dashDocument.DDD_DocID = ZGuid.NewZGuid();
			dashDocument.DDD_DocToken = docToken;

			Factory.Save();

			var shipamaxServiceMock = new Mock<IShipamaxService>();
			var dashErrorReporterMock = new Mock<IDashErrorReporter>();
			var dashEntitiesService = new DashEntitiesService(shipamaxServiceMock.Object, dashErrorReporterMock.Object);

			var result = dashEntitiesService.UpdateStatusToComplete(dashDocument, false);

			AssertEquals(true, result);
			AssertEquals(SharedConstants.ParseStatus.Code.Complete, dashDocument.DDD_ParseStatus);

			var newFactory = new BusinessObjectFactory();
			var loadedDashDocument = newFactory.Load<DashDocument>(dashDocument.PK);

			AssertEquals(SharedConstants.ParseStatus.Code.Complete, dashDocument.DDD_ParseStatus);
			AssertEquals(SharedConstants.ParseStatus.Code.ReadyForThirdPartyProcessing, loadedDashDocument.DDD_ParseStatus);

			shipamaxServiceMock.Verify(x => x.SaveParseResult(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<ShipamaxParseResult>()));
		}

		public void TestUpdateStatusToComplete_Factory_Save_Is_Called()
		{
			var docId = ZGuid.NewZGuid();
			var docToken = ZGuid.NewZGuid().ToString();
			var dashDocument = Factory.NewWithValidTestData<DashDocument>();
			dashDocument.DDD_ParseStatus = SharedConstants.ParseStatus.Code.ReadyForThirdPartyProcessing;
			dashDocument.DDD_DocID = ZGuid.NewZGuid();
			dashDocument.DDD_DocToken = docToken;

			Factory.Save();

			var shipamaxServiceMock = new Mock<IShipamaxService>();
			var dashErrorReporterMock = new Mock<IDashErrorReporter>();
			var dashEntitiesService = new DashEntitiesService(shipamaxServiceMock.Object, dashErrorReporterMock.Object);

			var result = dashEntitiesService.UpdateStatusToComplete(dashDocument, true);

			AssertEquals(true, result);
			AssertEquals(SharedConstants.ParseStatus.Code.Complete, dashDocument.DDD_ParseStatus);

			var newFactory = new BusinessObjectFactory();
			var loadedDashDocument = newFactory.Load<DashDocument>(dashDocument.PK);

			AssertEquals(SharedConstants.ParseStatus.Code.Complete, dashDocument.DDD_ParseStatus);
			AssertEquals(SharedConstants.ParseStatus.Code.Complete, loadedDashDocument.DDD_ParseStatus);

			shipamaxServiceMock.Verify(x => x.SaveParseResult(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<ShipamaxParseResult>()));
		}

		public void TestUpdateStatusToComplete_Returns_False_When_ShipamaxServiceException_Is_Thrown()
		{
			var docId = ZGuid.NewZGuid();
			var docToken = ZGuid.NewZGuid().ToString();
			var dashDocument = Factory.NewWithValidTestData<DashDocument>();
			dashDocument.DDD_ParseStatus = SharedConstants.ParseStatus.Code.ReadyForThirdPartyProcessing;
			dashDocument.DDD_DocID = ZGuid.NewZGuid();
			dashDocument.DDD_DocToken = docToken;

			Factory.Save();

			var shipamaxServiceMock = new Mock<IShipamaxService>();
			shipamaxServiceMock.Setup(x => x.SaveParseResult(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<ShipamaxParseResult>())).Throws<ShipamaxServiceException>(() => throw new ShipamaxServiceException(ShipamaxServiceErrorType.ValidationError, "DocToken is incorrect"));
			var dashErrorReporterMock = new Mock<IDashErrorReporter>();
			var disposableMock = new Mock<IDisposable>();
			dashErrorReporterMock
				.Setup(r => r.GatherAdditionalInformation(dashDocument))
				.Returns(disposableMock.Object)
				.Verifiable();
			var dashEntitiesService = new DashEntitiesService(shipamaxServiceMock.Object, dashErrorReporterMock.Object);

			var result = dashEntitiesService.UpdateStatusToComplete(dashDocument, true);

			AssertEquals(false, result);
			AssertEquals(SharedConstants.ParseStatus.Code.Complete, dashDocument.DDD_ParseStatus);

			var newFactory = new BusinessObjectFactory();
			var loadedDashDocument = newFactory.Load<DashDocument>(dashDocument.PK);

			AssertEquals(SharedConstants.ParseStatus.Code.Complete, dashDocument.DDD_ParseStatus);
			AssertEquals(SharedConstants.ParseStatus.Code.ReadyForThirdPartyProcessing, loadedDashDocument.DDD_ParseStatus);

			shipamaxServiceMock.Verify(x => x.SaveParseResult(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<ShipamaxParseResult>()));

			dashErrorReporterMock.Verify(r => r.GatherAdditionalInformation(It.Is<DashDocument>(d => d == dashDocument)), Times.Once);

			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}
	}
}
