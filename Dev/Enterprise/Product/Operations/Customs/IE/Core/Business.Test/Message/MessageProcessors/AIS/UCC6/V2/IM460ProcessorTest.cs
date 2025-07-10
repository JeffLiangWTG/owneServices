using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM460;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM460Processor))]
	class IM460ProcessorTest : EntryHeaderMessageProcessorTest<IM460Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IIM460Provider>
	{
		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_EntryStatus", "CON", messageAttachee.CH_EntryStatus);

			AssertMessageInterpretation(incomingMessage, IM460MessageInterpreterTest.Interpretation("B00001000"));

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Control Notice (IM460) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });

			AssertRequestedDocumentsAdded(messageAttachee);
		}

		void AssertRequestedDocumentsAdded(CusEntryHeader entry)
		{
			var requestedDocuments = entry.EntryInstruction.RequestedDocuments;
			AssertEquals("RequestedDocuments.Count", 3, requestedDocuments.Count);

			var doc0 = requestedDocuments[0];
			AssertEquals("CSI_Code", "Y057", doc0.CSI_Code);
			AssertEquals("RequestInformation", "Please provide", doc0.RequestInformation);
			AssertEquals("CSI_DateOfIssue", new ZDateTime(2023, 9, 14), doc0.CSI_DateOfIssue);
			AssertEquals("CSI_DateOfExpiry", new ZDateTime(2023, 9, 21), doc0.CSI_DateOfExpiry);
			AssertEquals("CSI_Status", EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened, doc0.CSI_Status);
			AssertEquals("CSI_RN_NKCountryCode", "AB", doc0.CSI_RN_NKCountryCode);

			var doc1 = requestedDocuments[1];
			AssertEquals("CSI_Code", "Z750", doc1.CSI_Code);
			AssertEquals("RequestInformation", "Info456", doc1.RequestInformation);
			AssertEquals("CSI_DateOfIssue", new ZDateTime(2023, 8, 29), doc1.CSI_DateOfIssue);
			AssertEquals("CSI_DateOfExpiry", new ZDateTime(2023, 9, 29), doc1.CSI_DateOfExpiry);
			AssertEquals("CSI_Status", EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived, doc1.CSI_Status);
			AssertEquals("CSI_RN_NKCountryCode", "IE", doc1.CSI_RN_NKCountryCode);

			var doc2 = requestedDocuments[2];
			AssertEquals("CSI_Code", "Y022", doc2.CSI_Code);
			AssertEquals("RequestInformation", "Description", doc2.RequestInformation);
			AssertEquals("CSI_DateOfIssue", new ZDateTime(2023, 9, 14), doc2.CSI_DateOfIssue);
			AssertEquals("CSI_DateOfExpiry", new ZDateTime(2023, 9, 21), doc2.CSI_DateOfExpiry);
			AssertEquals("CSI_Status", EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened, doc2.CSI_Status);
			AssertEquals("CSI_RN_NKCountryCode", "CD", doc2.CSI_RN_NKCountryCode);
		}

		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM460;

		protected override ZString MessageText => GetMessageText();

		protected virtual ZString GetMessageText() => AISInterchangeProcessorTestHelper.GetStandardIM460Text("LRN123456789", "21IEDUB11A782454R2", "23IECUSREG000072U1");

		protected override ZString MessageFriendlyName => "IM460 – Control Notice";

		protected override IM460Processor Processor => new IM460Processor(logger, typeof(Im460));

		protected override (JobDeclaration declaration, CusEntryHeader messageAttachee, EDIMessage outgoingMessage, AISInboundEDIMessage incomingMessage) CreateSetupData()
		{
			MessageTestHelper.SetupCL716Types(Factory);
			MessageTestHelper.SetupCL215Types(Factory);

			var result = base.CreateSetupData();

			var entryInstruction = Factory.New<CusEntryInstruction>();
			result.messageAttachee.CH_CEI_Instruction = entryInstruction.PK;
			var docExisted = result.messageAttachee.EntryInstruction.RequestedDocuments.AddNew();
			docExisted.CSI_Code = "Y057";
			docExisted.RequestInformation = "Please provide";
			docExisted.CSI_DateOfIssue = new ZDateTime(2023, 9, 14);
			docExisted.CSI_DateOfExpiry = new ZDateTime(2023, 9, 21);
			docExisted.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened;
			docExisted.CSI_RN_NKCountryCode = "AB";

			var otherRequestedDocument = entryInstruction.RequestedDocuments.AddNew();
			otherRequestedDocument.CSI_Code = "Z750";
			otherRequestedDocument.RequestInformation = "Info456";
			otherRequestedDocument.CSI_DateOfIssue = new ZDateTime(2023, 8, 29);
			otherRequestedDocument.CSI_DateOfExpiry = new ZDateTime(2023, 9, 29);
			otherRequestedDocument.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;
			otherRequestedDocument.CSI_RN_NKCountryCode = "IE";

			return result;
		}
	}
}
