using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR884C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(TR884CProcessor))]
	class TR884CProcessorTest : NCTSDepartureMessageProcessorAbstractTest<TR884CProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, TR884CProvider>
	{
		protected override void AssertProcessResultCore(NctsDepartureMovementHeader movementHeader, NCTSInboundEDIMessage incomingMessage)
		{
			AssertEquals("BM_MessageStatus", LogicalStatusList.Codes.Accepted, movementHeader.BM_MessageStatus);

			var requestedDocuments = movementHeader.Header.RequestedDocuments;
			AssertEquals("RequestedDocuments.Count", 2, requestedDocuments.Count);

			var doc1 = requestedDocuments[0];
			AssertEquals("CSI_Code", "Y029", doc1.CSI_Code);
			AssertEquals("RequestInformation", "Original description", doc1.RequestInformation);
			AssertEquals("CSI_DateOfIssue", new ZDateTime(2020, 12, 22), doc1.CSI_DateOfIssue);
			AssertEquals("CSI_DateOfExpiry", new ZDateTime(2022, 7, 29), doc1.CSI_DateOfExpiry);
			AssertEquals("CSI_Status", EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived, doc1.CSI_Status);

			var doc2 = requestedDocuments[1];
			AssertEquals("CSI_Code", "Y022", doc2.CSI_Code);
			AssertEquals("RequestInformation", "Info 1", doc2.RequestInformation);
			AssertEquals("CSI_DateOfIssue", new ZDateTime(2020, 12, 22), doc2.CSI_DateOfIssue);
			AssertEquals("CSI_DateOfExpiry", new ZDateTime(2022, 7, 29), doc2.CSI_DateOfExpiry);
			AssertEquals("CSI_Status", EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestCancelled, doc2.CSI_Status);

			AssertMessageInterpretation(incomingMessage, @"
				A Document Presentation Request Cancellation (TR884) message has been received for Job B00001000.<br />
				<br />
				<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
					<tr><td>MRN</td><td>19MRNCC060C0123456</td></tr>
					<tr><td>Case Id</td><td>Test ID</td></tr>
					<tr><td>Document Presentation Request Cancellation Reason</td><td>Test Presentation Request Cancellation Reason</td></tr>
				</table>");
		}

		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.TR884C;

		protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardTR884CText();

		protected override ZString MessageFriendlyName => "TR884C: DOCUMENT PRESENTATION REQUEST CANCELLATION";

		protected override TR884CProcessor Processor => new TR884CProcessor(logger, typeof(Tr884C));

		protected override (NctsHeader declaration, NctsDepartureMovementHeader messageAttachee, EDIMessage outgoingMessage, NCTSInboundEDIMessage incomingMessage) CreateSetupData()
		{
			var result = base.CreateSetupData();

			var existingDoc1 = result.declaration.RequestedDocuments.AddNew();
			existingDoc1.CSI_Code = "Y029";
			existingDoc1.CSI_Description = "Original description";
			existingDoc1.CSI_DateOfIssue = new ZDateTime(2020, 12, 22);
			existingDoc1.CSI_DateOfExpiry = new ZDateTime(2022, 7, 29);
			existingDoc1.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;

			var existingDoc2 = result.declaration.RequestedDocuments.AddNew();
			existingDoc2.CSI_Code = "Y022";
			existingDoc2.CSI_Description = "Info 1";
			existingDoc2.CSI_DateOfIssue = new ZDateTime(2020, 12, 22);
			existingDoc2.CSI_DateOfExpiry = new ZDateTime(2022, 7, 29);
			existingDoc2.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.PhysicallyPresentDocument;

			return result;
		}
	}
}
