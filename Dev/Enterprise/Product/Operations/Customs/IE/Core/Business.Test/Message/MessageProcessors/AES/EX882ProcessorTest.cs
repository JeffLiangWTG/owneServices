using System;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX882;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	[TestedType(typeof(EX882Processor))]
	class EX882ProcessorTest : EntryHeaderMessageProcessorTest<EX882Processor, AESInboundEDIMessage, AESOutboundEDIMessage, EX882Provider>
	{
		protected override void AssertProcessResultCore(CusEntryHeader entry, AESInboundEDIMessage incomingMessage)
		{
			AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);

			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Document Upload Request Cancellation message has been received from Customs for Job B00001000 through the EX882 message stating that Revenue have now decided to cancel the uploading document request which was sent before through EX582 message." },
				new string[] { "staff1@where.com" });

			AssertEquals("Upload Request Cancellation", EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestCancelled, entry.EntryInstruction.RequestedDocuments[0].CSI_Status);

			AssertEquals("CSI_Status DocumentsConfirmedReceived Not changed", EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived, entry.EntryInstruction.RequestedDocuments[1].CSI_Status);
		}

		public void TestMessageInterpretationType()
		{
			AssertEquals("EX882MessageInterpreter", typeof(EX882MessageInterpreter), new EX882ProcessorForTest(logger, typeof(Ex882)).MessageInterpreterType);
		}

		protected override ZString MessageFriendlyName => "EX882: Documents Upload Request Cancellation";

		protected override EX882Processor Processor => new EX882Processor(logger, typeof(Ex882));

		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.EX882;

		protected override ZString MessageText => AESInterchangeProcessorTestHelper.GetStandardEX882Text("21IEDUB11A782454R2");

		protected override (JobDeclaration declaration, CusEntryHeader messageAttachee, EDIMessage outgoingMessage, AESInboundEDIMessage incomingMessage) CreateSetupData()
		{
			var result = base.CreateSetupData();
			var collection = result.messageAttachee.EntryInstruction.RequestedDocuments;
			var doc1 = collection.AddNew();
			doc1.CSI_Code = "ID01";
			doc1.RequestInformation = "Document of type ID01";
			doc1.CSI_DateOfIssue = new ZDateTime(2022, 8, 3);
			doc1.CSI_DateOfExpiry = new ZDateTime(2022, 8, 10);
			doc1.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened;

			var doc2 = collection.AddNew();
			doc2.CSI_Code = "ID03";
			doc2.RequestInformation = "Document of type ID03";
			doc2.CSI_DateOfIssue = new ZDateTime(2022, 8, 3);
			doc2.CSI_DateOfExpiry = new ZDateTime(2022, 8, 11);
			doc2.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;

			return result;
		}

		class EX882ProcessorForTest : EX882Processor
		{
			public EX882ProcessorForTest(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
			{
			}

			public new Type MessageInterpreterType => base.MessageInterpreterType;
		}
	}
}
