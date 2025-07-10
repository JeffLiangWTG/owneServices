using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM482;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM482Processor))]
	class IM482ProcessorTest : EntryHeaderMessageProcessorTest<IM482Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IM482Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM482;

		protected override ZString MessageText => GetMessageText();

		protected virtual ZString GetMessageText() => AISInterchangeProcessorTestHelper.GetStandardIM482Text("21IEDUB11A782454R2", "LRN123456789");

		protected override ZString MessageFriendlyName => "IM482: Documents Request";

		protected override (JobDeclaration declaration, CusEntryHeader messageAttachee, EDIMessage outgoingMessage, AISInboundEDIMessage incomingMessage) CreateSetupData()
		{
			var result = base.CreateSetupData();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			result.messageAttachee.CH_CEI_Instruction = entryInstruction.PK;
			var docExisted = result.messageAttachee.EntryInstruction.RequestedDocuments.AddNew();

			docExisted.CSI_Code = "Z123";
			docExisted.CSI_Description = "Test";
			docExisted.CSI_AdditionalDescription = "1";
			docExisted.CSI_DateOfIssue = new ZDateTime(2023, 8, 11);
			docExisted.CSI_DateOfExpiry = new ZDateTime(2023, 8, 11);
			docExisted.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;

			var otherEntryInstruction = Factory.New<CusEntryInstruction>();
			var otherRequestedDocument = otherEntryInstruction.RequestedDocuments.AddNew();

			otherRequestedDocument.CSI_Code = "Z234";
			otherRequestedDocument.CSI_Description = "Test";
			otherRequestedDocument.CSI_AdditionalDescription = "2";
			otherRequestedDocument.CSI_DateOfIssue = new ZDateTime(2023, 8, 11);
			otherRequestedDocument.CSI_DateOfExpiry = new ZDateTime(2023, 8, 11);
			otherRequestedDocument.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;

			return result;
		}

		protected override void AssertProcessResultCore(CusEntryHeader entry, AISInboundEDIMessage incomingMessage)
		{
			var requestedDocuments = entry.EntryInstruction.RequestedDocuments;

			AssertEquals("RequestedDocuments.Count", 2, requestedDocuments.Count);

			var doc1 = requestedDocuments[0];
			AssertEquals("CSI_Code", "Z123", doc1.CSI_Code);
			AssertEquals("RequestInformation", "Test1", doc1.RequestInformation);
			AssertEquals("CSI_DateOfIssue", new ZDateTime(2023, 8, 11), doc1.CSI_DateOfIssue);
			AssertEquals("CSI_DateOfExpiry", new ZDateTime(2023, 8, 11), doc1.CSI_DateOfExpiry);
			AssertEquals("CSI_Status", EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived, doc1.CSI_Status);

			var doc2 = requestedDocuments[1];
			AssertEquals("CSI_Code", "Z234", doc2.CSI_Code);
			AssertEquals("RequestInformation", "Test2", doc2.RequestInformation);
			AssertEquals("CSI_DateOfIssue", new ZDateTime(2023, 8, 11), doc2.CSI_DateOfIssue);
			AssertEquals("CSI_DateOfExpiry", new ZDateTime(2023, 8, 11), doc2.CSI_DateOfExpiry);
			AssertEquals("CSI_Status", EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened, doc2.CSI_Status);

			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Documents Request (IM482) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}

		protected override IM482Processor Processor => new IM482Processor(logger, typeof(Im482));
	}
}
