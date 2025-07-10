using System;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX582;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	[TestedType(typeof(EX582Processor))]
	class EX582ProcessorTest : EntryHeaderMessageProcessorTest<EX582Processor, AESInboundEDIMessage, AESOutboundEDIMessage, EX582Provider>
	{
		protected override (JobDeclaration declaration, CusEntryHeader messageAttachee, EDIMessage outgoingMessage, AESInboundEDIMessage incomingMessage) CreateSetupData()
		{
			var result = base.CreateSetupData();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			result.messageAttachee.CH_CEI_Instruction = entryInstruction.PK;
			var docExisted = result.messageAttachee.EntryInstruction.RequestedDocuments.AddNew();

			docExisted.CSI_Code = "Z740";
			docExisted.CSI_Description = "Info";
			docExisted.CSI_AdditionalDescription = "123";
			docExisted.CSI_DateOfIssue = new ZDateTime(2020, 12, 22);
			docExisted.CSI_DateOfExpiry = new ZDateTime(2022, 7, 29);
			docExisted.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;

			var otherEntryInstruction = Factory.New<CusEntryInstruction>();
			var otherRequestedDocument = otherEntryInstruction.RequestedDocuments.AddNew();

			otherRequestedDocument.CSI_Code = "Z750";
			otherRequestedDocument.CSI_Description = "Info";
			otherRequestedDocument.CSI_AdditionalDescription = "456";
			otherRequestedDocument.CSI_DateOfIssue = new ZDateTime(2020, 12, 22);
			otherRequestedDocument.CSI_DateOfExpiry = new ZDateTime(2022, 7, 29);
			otherRequestedDocument.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;

			return result;
		}

		protected override void AssertProcessResultCore(CusEntryHeader entry, AESInboundEDIMessage incomingMessage)
		{
			AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);

			var requestedDocuments = entry.EntryInstruction.RequestedDocuments;

			AssertEquals("RequestedDocuments.Count", 2, requestedDocuments.Count);

			var doc1 = requestedDocuments[0];
			AssertEquals("CSI_Code", "Z740", doc1.CSI_Code);
			AssertEquals("RequestInformation", "Info123", doc1.RequestInformation);
			AssertEquals("CSI_DateOfIssue", new ZDateTime(2020, 12, 22), doc1.CSI_DateOfIssue);
			AssertEquals("RequestInformation", new ZDateTime(2022, 7, 29), doc1.CSI_DateOfExpiry);
			AssertEquals("CSI_Status", EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived, doc1.CSI_Status);

			var doc2 = requestedDocuments[1];
			AssertEquals("CSI_Code", "Z750", doc2.CSI_Code);
			AssertEquals("RequestInformation", "Info456", doc2.RequestInformation);
			AssertEquals("CSI_DateOfIssue", new ZDateTime(2020, 12, 22), doc2.CSI_DateOfIssue);
			AssertEquals("RequestInformation", new ZDateTime(2022, 7, 29), doc2.CSI_DateOfExpiry);
			AssertEquals("CSI_Status", EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened, doc2.CSI_Status);

			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Document Request message has been received from Customs for Job B00001000 through the EX582 message." },
				new string[] { "staff1@where.com" });
		}

		public void TestMessageInterpretationType()
		{
			AssertEquals("EX582MessageInterpreter", typeof(EX582MessageInterpreter), new EX582ProcessorForTest(logger, typeof(Ex582)).GetMessageInterpretationType());
		}

		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.EX582;

		protected override ZString MessageText => AESInterchangeProcessorTestHelper.GetStandardEX582Text("LRN123456789", "21IEDUB11A782454R2");

		protected override ZString MessageFriendlyName => "EX582: DOCUMENTS REQUEST";

		protected override EX582Processor Processor => new EX582Processor(logger, typeof(Ex582));

		class EX582ProcessorForTest : EX582Processor
		{
			public EX582ProcessorForTest(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
			{
			}

			public Type GetMessageInterpretationType() => MessageInterpreterType;
		}
	}
}
