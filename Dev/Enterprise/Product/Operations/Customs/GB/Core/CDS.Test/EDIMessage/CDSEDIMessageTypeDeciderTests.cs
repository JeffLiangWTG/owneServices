using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.CDS.Testing
{
	class CDSEDIMessageTypeDeciderTests : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var typeDecider = new CDSEDIMessageTypeDecider();
			var message = Factory.New<CDSEDIMessage>();
			var row = ((INeedRow)message).Row;

			CombineAssertions(() =>
			{
				message.EM_MessageType = CDSEDIMessageTypeList.Codes.SynchronousResponse;
				AssertEquals(typeof(CDSSynchronousResponseEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = CDSEDIMessageTypeList.Codes.EHubErrorResponse;
				AssertEquals(typeof(CDSErrorResponseEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = CDSEDIMessageTypeList.Codes.Response;
				AssertEquals(typeof(CDSResponseEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = CDSEDIMessageTypeList.Codes.InventoryLinkingControlResponse;
				AssertEquals(typeof(CDSInventoryLinkingControlResponseEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = CDSEDIMessageTypeList.Codes.InventoryLinkingMovementResponse;
				AssertEquals(typeof(CDSInventoryLinkingMovementResponseEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = CDSEDIMessageTypeList.Codes.InventoryLinkingMovementTotalsResponse;
				AssertEquals(typeof(CDSInventoryLinkingMovementTotalsResponseEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = CDSEDIMessageTypeList.Codes.InventoryLinkingQueryResponse;
				AssertEquals(typeof(CDSInventoryLinkingQueryResponseEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = CDSEDIMessageTypeList.Codes.InventoryLinkingConsolidationRequest;
				AssertEquals(typeof(CDSInventoryLinkingConsolidationRequestEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = CDSEDIMessageTypeList.Codes.InventoryLinkingMovementRequest;
				AssertEquals(typeof(CDSInventoryLinkingMovementRequestEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = CDSEDIMessageTypeList.Codes.InventoryLinkingQueryRequest;
				AssertEquals(typeof(CDSInventoryLinkingQueryRequestEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = CDSEDIMessageTypeList.Codes.NewDeclaration;
				AssertEquals(typeof(CDSNewDeclarationEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = CDSEDIMessageTypeList.Codes.AmendDeclaration;
				AssertEquals(typeof(CDSAmendDeclarationEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = CDSEDIMessageTypeList.Codes.CancelDeclaration;
				AssertEquals(typeof(CDSCancelDeclarationEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = CDSEDIMessageTypeList.Codes.NewAmendment;
				AssertEquals(typeof(CDSAmendmentComparisonEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = CDSEDIMessageTypeList.Codes.QueryResponse;
				AssertEquals(typeof(CDSDeclarationInfoResponseEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = CDSEDIMessageTypeList.Codes.ArrivalNotification;
				AssertEquals(typeof(CDSArrivalAmendmentDeclarationEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = CDSEDIMessageTypeList.Codes.FecChallenge;
				AssertEquals(typeof(CDSFECAmendmentDeclarationEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = CDSEDIMessageTypeList.Codes.NilAmendment;
				AssertEquals(typeof(CDSNilAmendmentDeclarationEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = CDSEDIMessageTypeList.Codes.CDSPentantAcaMessage;
				AssertEquals(typeof(CDSPentantAcaMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = CDSEDIMessageTypeList.Codes.DocumentUploadConfirmation;
				AssertEquals(typeof(CDSDocumentUploadConfirmationResponse), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = "XX";
				AssertEquals(typeof(CDSEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
			});

			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCDSViaCCSUK;
			CombineAssertions(() =>
			{
				message.EM_MessageType = CDSEDIMessageTypeList.Codes.Response;
				AssertEquals(typeof(CDSResponseEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = CDSEDIMessageTypeList.Codes.InventoryLinkingControlResponse;
				AssertEquals(typeof(CDSInventoryLinkingControlResponseEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = CDSEDIMessageTypeList.Codes.InventoryLinkingMovementResponse;
				AssertEquals(typeof(CDSInventoryLinkingMovementResponseEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = CDSEDIMessageTypeList.Codes.InventoryLinkingMovementTotalsResponse;
				AssertEquals(typeof(CDSInventoryLinkingMovementTotalsResponseEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = CDSEDIMessageTypeList.Codes.InventoryLinkingQueryResponse;
				AssertEquals(typeof(CDSInventoryLinkingQueryResponseEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = CDSEDIMessageTypeList.Codes.InventoryLinkingConsolidationRequest;
				AssertEquals(typeof(CDSInventoryLinkingConsolidationRequestEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = CDSEDIMessageTypeList.Codes.InventoryLinkingMovementRequest;
				AssertEquals(typeof(CDSInventoryLinkingMovementRequestEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = CDSEDIMessageTypeList.Codes.InventoryLinkingQueryRequest;
				AssertEquals(typeof(CDSInventoryLinkingQueryRequestEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = CDSEDIMessageTypeList.Codes.NewDeclaration;
				AssertEquals(typeof(CDSNewDeclarationEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = CDSEDIMessageTypeList.Codes.AmendDeclaration;
				AssertEquals(typeof(CDSAmendDeclarationEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = CDSEDIMessageTypeList.Codes.CancelDeclaration;
				AssertEquals(typeof(CDSCancelDeclarationEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = CDSEDIMessageTypeList.Codes.NewAmendment;
				AssertEquals(typeof(CDSAmendmentComparisonEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = CDSEDIMessageTypeList.Codes.QueryResponse;
				AssertEquals(typeof(CDSDeclarationInfoResponseEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = "XX";
				AssertEquals(typeof(GbEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
			});

			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCDSDISQuery;
			AssertEquals(typeof(CDSDISQueryMessage), typeDecider.GetTypeForLoad(row, Factory));
		}
	}
}
