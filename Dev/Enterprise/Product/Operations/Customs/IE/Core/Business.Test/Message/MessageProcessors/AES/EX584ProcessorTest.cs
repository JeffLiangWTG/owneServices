using System;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX584;
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
	[TestedType(typeof(EX584Processor))]
	class EX584ProcessorTest : EntryHeaderMessageProcessorTest<EX584Processor, AESInboundEDIMessage, AESOutboundEDIMessage, EX584Provider>
	{
		public void TestMessageInterpreter()
		{
			AssertEquals("MessageInterpreter", typeof(EX584MessageInterpreter), new EX584ProcessorForTest(logger, typeof(Ex584)).MessageInterpreterType);
		}

		protected override void AssertProcessResultCore(CusEntryHeader entry, AESInboundEDIMessage incomingMessage)
		{
			AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);

			MessageProcessorNotificationTestHelper.AssertEmail(
			incomingMessage.MessageTypeWithDescription + " Response for B00001000",
			new[] { "A Document Request Presentation message has been received from Customs for Job B00001000 through the EX584 message stating that a list of following documents will need to be presented physically. " },
			new string[] { "staff1@where.com" });
		}

		protected override ZString MessageFriendlyName => "EX584: Request Document Presentation";

		protected override EX584Processor Processor => new EX584Processor(logger, typeof(Ex584));

		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.EX584;

		protected override ZString MessageText => AESInterchangeProcessorTestHelper.GetStandardEX584Text("LRN123456789", "21IEDUB11A782454R2");

		class EX584ProcessorForTest : EX584Processor
		{
			public EX584ProcessorForTest(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
			{
			}

			public new Type MessageInterpreterType => base.MessageInterpreterType;
		}
	}

	[TestedType(typeof(EX584Processor))]
	class EX584ProcessorNoDuplicationTest : EX584ProcessorTest
	{
		protected override void AssertProcessResultCore(CusEntryHeader entry, AESInboundEDIMessage incomingMessage)
		{
			var processResult = entry.EntryInstruction.RequestedDocuments.Cast<EU.Business.RequestedDocument>().OrderBy(item => item.CSI_Code).ThenBy(item => item.CSI_DateOfExpiry).ToArray();
			AssertEquals("4 items(2 newly created)", 4, processResult.Length);

			var requestDate = new ZDateTime(2022, 8, 3);
			var limitDate = new ZDateTime(2022, 8, 10);

			var item1 = processResult[0];
			AssertEquals("New item 1 from message, CSI_Code", "D01", item1.CSI_Code);
			AssertEquals("New item 1 from message, RequestInformation", "Document of type 01", item1.RequestInformation);
			AssertEquals("New item 1 from message, CSI_DateOfIssue", requestDate, item1.CSI_DateOfIssue);
			AssertEquals("New item 1 from message, CSI_DateOfExpiry", limitDate, item1.CSI_DateOfExpiry);
			AssertEquals("New item 1 from message, CSI_Status", EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.PhysicallyPresentDocument, item1.CSI_Status);

			var item2 = processResult[1];
			AssertEquals("Existing Item 2 (matched item 2 from message), CSI_Code", "D02", item2.CSI_Code);
			AssertEquals("Existing Item 2 (matched item 2 from message), RequestInformation", "Document of type 02", item2.RequestInformation);
			AssertEquals("Existing Item 2 (matched item 2 from message), CSI_DateOfIssue", requestDate, item2.CSI_DateOfIssue);
			AssertEquals("Existing Item 2 (matched item 2 from message), CSI_DateOfExpiry", limitDate, item2.CSI_DateOfExpiry);
			AssertEquals("Existing Item 2 (matched item 2 from message), CSI_Status(Unchanged)", string.Empty, item2.CSI_Status);

			var item3 = processResult[2];
			AssertEquals("New item 3 from message, CSI_Code", "D03", item3.CSI_Code);
			AssertEquals("New item 3 from message, RequestInformation", "Document of type 03", item3.RequestInformation);
			AssertEquals("New item 3 from message, CSI_DateOfIssue", requestDate, item3.CSI_DateOfIssue);
			AssertEquals("New item 3 from message, CSI_DateOfExpiry", limitDate, item3.CSI_DateOfExpiry);
			AssertEquals("New item 3 from message, CSI_Status", EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.PhysicallyPresentDocument, item3.CSI_Status);

			var item4 = processResult[3];
			AssertEquals("Existing Item 4 (unmatched), CSI_Code", "D03", item4.CSI_Code);
			AssertEquals("Existing Item 4 (unmatched), RequestInformation", "Document of type 03", item4.RequestInformation);
			AssertEquals("Existing Item 4 (unmatched), CSI_DateOfIssue", requestDate, item4.CSI_DateOfIssue);
			AssertEquals("Existing Item 4 (unmatched), CSI_DateOfExpiry", new ZDateTime(2022, 8, 11), item4.CSI_DateOfExpiry);
			AssertEquals("Existing Item 4 (unmatched), CSI_Status", string.Empty, item4.CSI_Status);
		}

		protected override (JobDeclaration declaration, CusEntryHeader messageAttachee, EDIMessage outgoingMessage, AESInboundEDIMessage incomingMessage) CreateSetupData()
		{
			var result = base.CreateSetupData();
			var collection = result.messageAttachee.EntryInstruction.RequestedDocuments;
			var doc1 = collection.AddNew();
			doc1.CSI_Code = "D02";
			doc1.RequestInformation = "Document of type 02";
			doc1.CSI_DateOfIssue = new ZDateTime(2022, 8, 3);
			doc1.CSI_DateOfExpiry = new ZDateTime(2022, 8, 10);

			var doc2 = collection.AddNew();
			doc2.CSI_Code = "D03";
			doc2.RequestInformation = "Document of type 03";
			doc2.CSI_DateOfIssue = new ZDateTime(2022, 8, 3);
			doc2.CSI_DateOfExpiry = new ZDateTime(2022, 8, 11);

			return result;
		}

		protected override ZString MessageText => AESInterchangeProcessorTestHelper.GetStandardEX584Text("LRN123456789", "21IEDUB11A782454R2", 3);
	}
}
