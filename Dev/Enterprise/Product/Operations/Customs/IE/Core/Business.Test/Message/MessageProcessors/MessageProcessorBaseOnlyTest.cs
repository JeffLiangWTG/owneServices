using System;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM917;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;

namespace Enterprise.Customs.IE.Business.Testing
{
	class MessageProcessorBaseOnlyTest : TestCaseWithFactory
	{
		public void TestPreProcessMessage_NullLinkedObject()
		{
			var message = CreateIncomingMessage("ID1");
			var logger = new LoggingInformation();
			new MessageProcessorForTest(logger).PreProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("LogError", true, logger.UserLogStrings.Contains("\tUnable to find a linked business object for message (Number:123, Type:917); message status set to ERROR."));
				AssertEquals("EM_Status", EDIMessage.Status.Error, message.EM_Status);
			});
		}

		public void TestPreProcessMessage_LinkedObject_BranchPKNotFound()
		{
			var message = CreateIncomingMessage("ID1");
			var entryHeader = Factory.New<CusEntryHeader>();
			new MessageProcessorForTest(new LoggingInformation()) { LinkedObject = entryHeader }.PreProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_LinkedObject", entryHeader, message.EM_LinkedObject);
				AssertEquals("EM_Status", EDIMessage.Status.PreProcessedOK, message.EM_Status);
			});
		}

		public void TestPreProcessMessage_LinkedObject_BranchPKFound()
		{
			var message = CreateIncomingMessage("ID1");
			var entryHeader = Factory.New<CusEntryHeader>();
			new MessageProcessorForTest(new LoggingInformation()) { LinkedObject = entryHeader, BranchPK = ZGuid.BrettsGuid }.PreProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_LinkedObject", entryHeader, message.EM_LinkedObject);
				AssertEquals("EM_GB", ZGuid.BrettsGuid, message.EM_GB);
				AssertEquals("EM_Status", EDIMessage.Status.PreProcessedOK, message.EM_Status);
			});
		}

		public void TestProcessMessage_CalledOnlyIfPreProcessedOK()
		{
			var message = CreateIncomingMessage("ID1");
			message.EM_Status = EDIMessage.Status.PreProcessedOK;
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			message.EM_LinkedObject = entry;
			var processor = new MessageProcessorForTest(new LoggingInformation());
			processor.ProcessMessage(message);
			AssertEquals(true, processor.ProcessMessageCoreCalled);
		}

		public void TestProcessMessage_NotPreProcessedOK()
		{
			var message = CreateIncomingMessage("ID1");
			message.EM_Status = EDIMessage.Status.Error;
			var processor = new MessageProcessorForTest(new LoggingInformation());
			processor.ProcessMessage(message);
			AssertEquals(false, processor.ProcessMessageCoreCalled);
		}

		public void TestMessageInterpretationIsSet()
		{
			var originalData = MessageAcknowledgementProcessorTest.CreateOriginalData(Factory);
			var outgoingMessage = originalData.outgoingMessage;
			var message = CreateIncomingMessage(outgoingMessage.EM_ApplicationReference);
			var processor = new MessageProcessorForTest(new LoggingInformation());
			processor.LinkedObject = originalData.entryHeader;
			processor.PreProcessMessage(message);
			processor.ProcessMessage(message);
			AssertXMLEquals(
				"EM_MessageInterpretation",
				@"Summary Text<br />
<br />AdvancedSummaries 1<br />AdvancedSummaries 2<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td colspan=""2"">Message Details Summary Text</td></tr><tr><td>MessageDetail1</td><td>MessageDetail1 Text</td></tr><tr><td>MessageDetail2</td><td>MessageDetail2 Text</td></tr><tr><td>SequencedMessageDetail1</td><td>SequencedMessageDetail1 Text</td></tr><tr><td>SequencedMessageDetail2</td><td>SequencedMessageDetail2 Text</td></tr></table><br />
<br />Summary:<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>AdditionalMessageDetail1</td><td>AdditionalMessageDetail1 Text</td></tr><tr><td>AdditionalMessageDetail2</td><td>AdditionalMessageDetail2 Text</td></tr></table><br />
<br />Summary Free Number of Columns:<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>FreeNumberOfColumns1 Title</td><td>FreeNumberOfColumns2 Title</td><td>FreeNumberOfColumns3 Title</td></tr><tr><td>FreeNumberOfColumns1 Text</td><td>FreeNumberOfColumns2 Text</td><td>FreeNumberOfColumns3 Text</td></tr></table><br />Description<br />
",
				message.EM_MessageInterpretation);
		}

		AISInboundEDIMessage CreateIncomingMessage(ZString applicationReference)
		{
			var message = Factory.New<AISInboundEDIMessage>();
			message.EM_MessageNum = "123";
			message.EM_MessageType = AISInterchangeTypeList.Codes.IM917;
			message.EM_ApplicationReference = applicationReference;
			var messageText = IEXmlObjectSerializer.Serialize(
			new Im917()
			{
				XmlNegativeAcknowledgement = new System.Collections.ObjectModel.Collection<XmlNegativeAcknowledgement>(new[]
				{
					new XmlNegativeAcknowledgement()
					{
						ErrorLineNumber = "1",
						ErrorColumnNumber = "2",
						ErrorReason = "BAD"
					}
				})
			});
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", messageText, includeResponseWrap: false);
			return message;
		}
	}

	class MessageProcessorForTest : MessageProcessor<AISInboundEDIMessage, IM917Provider>
	{
		public MessageProcessorForTest(LoggingInformation logger) : base(logger, typeof(Im917))
		{
		}

		public BusinessObject LinkedObject { get; set; }

		public ZGuid BranchPK { get; set; }

		public bool ProcessMessageCoreCalled => count > 0;

		protected override string ApplicationCodeCore => string.Empty;

		protected override string MessageFriendlyNameCore => string.Empty;

		protected override BusinessObject GetLinkedObject(BusinessObjectFactory factory, AISInboundEDIMessage message) => LinkedObject;

		protected override ZGuid GetBranchPk(BusinessObject linkedObject) => BranchPK;

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AISInboundEDIMessage message, IM917Provider provider) => count++;

		int count;

		protected override Type MessageInterpreterType => typeof(InboundMessageInterpreterForBaseTesting);
	}
}
