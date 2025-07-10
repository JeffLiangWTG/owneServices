using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR082C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(TR082CProcessor))]
	class TR082CProcessorTest : NCTSDepartureMessageProcessorAbstractTest<TR082CProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, TR082CProvider>
	{
		protected override void AssertProcessResultCore(NctsDepartureMovementHeader movementHeader, NCTSInboundEDIMessage incomingMessage)
		{
			AssertEquals("BM_MessageStatus", LogicalStatusList.Codes.Accepted, movementHeader.BM_MessageStatus);

			var requestedDocuments = movementHeader.Header.RequestedDocuments;
			AssertEquals("RequestedDocuments.Count", 3, requestedDocuments.Count);

			var doc = requestedDocuments[0];
			AssertEquals("CSI_Code", "Y029", doc.CSI_Code);
			AssertEquals("RequestInformation", "Original description", doc.RequestInformation);
			AssertEquals("CSI_DateOfIssue", new ZDateTime(1971, 9, 18), doc.CSI_DateOfIssue);
			AssertEquals("CSI_DateOfExpiry", new ZDateTime(1971, 10, 18), doc.CSI_DateOfExpiry);
			AssertEquals("CSI_Status", EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened, doc.CSI_Status);

			// duplicated Requested Document
			var doc1 = requestedDocuments[1];
			AssertEquals("CSI_Code", "Y029", doc1.CSI_Code);
			AssertEquals("RequestInformation", "Original description 2", doc1.RequestInformation);
			AssertEquals("CSI_DateOfIssue", new ZDateTime(1971, 9, 18), doc1.CSI_DateOfIssue);
			AssertEquals("CSI_DateOfExpiry", new ZDateTime(1971, 10, 18), doc1.CSI_DateOfExpiry);
			AssertEquals("CSI_Status", EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened, doc1.CSI_Status);

			var doc2 = requestedDocuments[2];
			AssertEquals("CSI_Code", "Y022", doc2.CSI_Code);
			AssertEquals("RequestInformation", "Info 1", doc2.RequestInformation);
			AssertEquals("CSI_DateOfIssue", new ZDateTime(1971, 9, 18), doc2.CSI_DateOfIssue);
			AssertEquals("CSI_DateOfExpiry", new ZDateTime(1971, 10, 18), doc2.CSI_DateOfExpiry);
			AssertEquals("CSI_Status", EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened, doc2.CSI_Status);

			AssertMessageInterpretation(incomingMessage, @"
				A Documents Request (TR082) message has been received for Job B00001000.<br />
				<br />
				<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
					<tr><td>MRN</td><td>19MRNCC060C0123456</td></tr>
					<tr><td>LRN</td><td>LRNCC060C0123456789012</td></tr>
					<tr><td>Request Date</td><td>18-Sep-71 00:00</td></tr>
					<tr><td>Date Limit</td><td>18-Oct-71 00:00</td></tr>
				</table><br />
				<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
					<tr><td>Additional Information Document Type</td><td>Y022</td></tr>
					<tr><td>Additional Information Document Complementary Information</td><td>Info 1</td></tr>
				</table><br />
				<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
					<tr><td>Additional Information Document Type</td><td>Y029</td></tr>
					<tr><td>Additional Information Document Complementary Information</td><td>Info 2</td></tr>
				</table>");
			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Documents Request (TR082) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.TR082C;

		protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardTR082CText("19MRNCC060C0123456", "LRNCC060C0123456789012");

		protected override ZString MessageFriendlyName => "TR082C: DOCUMENTS REQUEST";

		protected override TR082CProcessor Processor => new TR082CProcessor(logger, typeof(Tr082C));

		protected override (NctsHeader declaration, NctsDepartureMovementHeader messageAttachee, EDIMessage outgoingMessage, NCTSInboundEDIMessage incomingMessage) CreateSetupData()
		{
			var result = base.CreateSetupData();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var existingDoc1 = result.declaration.RequestedDocuments.AddNew();

			existingDoc1.CSI_Code = "Y029";
			existingDoc1.CSI_Description = "Original description";
			existingDoc1.CSI_DateOfIssue = new ZDateTime(2020, 12, 22);
			existingDoc1.CSI_DateOfExpiry = new ZDateTime(2022, 7, 29);
			existingDoc1.CSI_Status = "ABC";

			var existingDoc2 = result.declaration.RequestedDocuments.AddNew();
			existingDoc2.CSI_Code = "Y029";
			existingDoc2.CSI_Description = "Original description 2";
			existingDoc2.CSI_DateOfIssue = new ZDateTime(2020, 11, 12);
			existingDoc2.CSI_DateOfExpiry = new ZDateTime(2023, 9, 01);
			existingDoc2.CSI_Status = "XYZ";

			return result;
		}
	}
}
