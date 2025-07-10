using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR084C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(TR084CProcessor))]
	class TR084CProcessorTest : NCTSDepartureMessageProcessorAbstractTest<TR084CProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, TR084CProvider>
	{
		protected override void AssertProcessResultCore(NctsDepartureMovementHeader messageAttachee, NCTSInboundEDIMessage incomingMessage)
		{
			AssertEquals("BM_MessageStatus", LogicalStatusList.Codes.Accepted, messageAttachee.BM_MessageStatus);

			var requestedDocuments = messageAttachee.Header.RequestedDocuments;
			var doc = requestedDocuments[0];
			AssertEquals("CSI_Code", "Y024", doc.CSI_Code);
			AssertEquals("RequestInformation", "Original description", doc.RequestInformation);
			AssertEquals("CSI_DateOfIssue", new ZDateTime(2023, 02, 24, 15, 33, 23), doc.CSI_DateOfIssue);
			AssertEquals("CSI_DateOfExpiry", new ZDateTime(2023, 03, 26, 16, 0, 0), doc.CSI_DateOfExpiry);
			AssertEquals("CSI_Status", EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.PhysicallyPresentDocument, doc.CSI_Status);

			// duplicated Requested Document
			var doc1 = requestedDocuments[1];
			AssertEquals("CSI_Code", "Y024", doc1.CSI_Code);
			AssertEquals("RequestInformation", "Original description 1", doc1.RequestInformation);
			AssertEquals("CSI_DateOfIssue", new ZDateTime(2023, 02, 24, 15, 33, 23), doc1.CSI_DateOfIssue);
			AssertEquals("CSI_DateOfExpiry", new ZDateTime(2023, 03, 26, 16, 0, 0), doc1.CSI_DateOfExpiry);
			AssertEquals("CSI_Status", EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.PhysicallyPresentDocument, doc1.CSI_Status);

			var doc2 = requestedDocuments[2];
			AssertEquals("CSI_Code", "Y022", doc2.CSI_Code);
			AssertEquals("RequestInformation", "TR084 add info completementary information", doc2.RequestInformation);
			AssertEquals("CSI_DateOfIssue", new ZDateTime(2023, 02, 24, 15, 33, 23), doc2.CSI_DateOfIssue);
			AssertEquals("CSI_DateOfExpiry", new ZDateTime(2023, 03, 26, 16, 0, 0), doc2.CSI_DateOfExpiry);
			AssertEquals("CSI_Status", EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.PhysicallyPresentDocument, doc2.CSI_Status);

			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Request Document Presentation (TR084) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
			AssertMessageInterpretation(incomingMessage, $@"A Request Document Presentation (TR084) message has been received for Job B00001000.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDU4EX144268149</td></tr><tr><td>LRN</td><td>LRNTR084123456789</td></tr><tr><td>Request Date</td><td>24-Feb-23 15:33</td></tr><tr><td>Date Limit</td><td>26-Mar-23 16:00</td></tr></table><br /><br />Additional Information<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Document Type</td><td>Y022</td></tr><tr><td>Document Complementary Information</td><td>TR084 add info completementary information</td></tr></table><br /><br />Additional Information<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Document Type</td><td>Y024</td></tr><tr><td>Document Complementary Information</td><td>TR084 add info completementary information 2</td></tr></table>");
		}

		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.TR084C;

		protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardTR084CText();

		protected override ZString MessageFriendlyName => "TR084C: REQUEST DOCUMENT PRESENTATION";

		protected override TR084CProcessor Processor => new TR084CProcessor(logger, typeof(Tr084C));

		protected override (NctsHeader declaration, NctsDepartureMovementHeader messageAttachee, EDIMessage outgoingMessage, NCTSInboundEDIMessage incomingMessage) CreateSetupData()
		{
			var result = base.CreateSetupData();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var existingDoc = result.declaration.RequestedDocuments.AddNew();

			existingDoc.CSI_Code = "Y024";
			existingDoc.CSI_Description = "Original description";
			existingDoc.CSI_DateOfIssue = new ZDateTime(2020, 12, 22);
			existingDoc.CSI_DateOfExpiry = new ZDateTime(2022, 7, 29);
			existingDoc.CSI_Status = "ABC";

			var existingDoc1 = result.declaration.RequestedDocuments.AddNew();

			existingDoc1.CSI_Code = "Y024";
			existingDoc1.CSI_Description = "Original description 1";
			existingDoc1.CSI_DateOfIssue = new ZDateTime(2020, 11, 12);
			existingDoc1.CSI_DateOfExpiry = new ZDateTime(2023, 9, 01);
			existingDoc1.CSI_Status = "XYZ";

			return result;
		}
	}
}
