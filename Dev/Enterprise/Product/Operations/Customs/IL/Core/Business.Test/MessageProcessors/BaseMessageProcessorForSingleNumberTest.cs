using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.IL.Business.MessageProcessors;
using Enterprise.Customs.IL.Business.Testing.MessageProcessors;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IL.Business.Testing
{
	public abstract class BaseMessageProcessorForSingleNumberTest<TProccessor, TResponse, TMessage>
		: BaseILBranchCustomsApplicationTypeMessageProcessorTest<TProccessor, TMessage>
		where TProccessor : BaseMessageProcessorForSingleNumber<TResponse>
		where TResponse : class
		where TMessage : ILEDIResponseMessage
	{
		public void TestProcessMessage_Discarded_WhenNumberNotFound()
		{
			var message = GetMessage(false);
			cusEntryNumber.CE_EntryNum = "123";

			var loggingInformation = new LoggingInformation();
			var processor = CreateProcessor(loggingInformation);
			var result = processor.GetLinkedBusinessObjectMetaData(message, loggingInformation);

			CombineAssertions("When No Number Found", () =>
			{
				Assert("Discard message provided", !result.DiscardReason.IsEmpty);
				AssertEquals("The message has note with text", CouldNotLocateMessage, result.DiscardReason);
			});
		}

		public void TestProcessMessage_Discarded_WhenShipmentFoundMoreThanOneRecordFromNumber()
		{
			var factory = Factory;
			var message = GetMessage(false);
			cusEntryNumber.CE_EntryNum = EntryNum;

			var forwardingShipment2 = factory.NewWithValidTestData<ForwardingShipment>();
			var cusEntryNumberDON2 = CusEntryNumber.LoadOrCreate<CusEntryNumber>(forwardingShipment2, NumberType, "IL");
			cusEntryNumberDON2.CE_EntryNum = EntryNum;
			factory.Save();

			var loggingInformation = new LoggingInformation();
			var processor = CreateProcessor(loggingInformation);
			var result = processor.GetLinkedBusinessObjectMetaData(message, loggingInformation);

			CombineAssertions("When More Than One Number Found", () =>
			{
				Assert("Discard message provided", !result.DiscardReason.IsEmpty);
				AssertEquals("The message has note with text", MoreThanOneMessage, result.DiscardReason);
			});
		}

		public void TestProcessMessage_ProcessedOKAndCES_WhenShipmentFoundOnceFromNumber()
		{
			var message = GetMessage(false);
			message.EM_LinkedObject = shipment;
			Factory.Save();

			var processor = CreateProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			Factory.Save();

			message.Reload();
			AssertLogsMessage(shipment, message);
		}

		public void TestProcessMessage_ProcessedOKAndLogMessageReceivedEvent()
		{
			var message = GetMessage(false);
			message.EM_LinkedObject = shipment;
			Factory.Save();

			AssertNull("Prerequisite: no message received event should be logged", shipment.Logs.MostRecentLogByEventTime(Events.MessageReceived, new ZQuery(StmALogSchema.SL_Reference, message.EM_MessageType)));

			var processor = CreateProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			Factory.Save();

			message.Reload();
			CombineAssertions("When One Number Found", () =>
			{
				AssertEquals("The message status is Processed", "PRS", message.EM_Status);

				var log = shipment.Logs.MostRecentLogByEventTime(Events.MessageReceived, new ZQuery(StmALogSchema.SL_Reference, message.EM_MessageType));
				AssertNotNull("A message received event should be logged", log);
				AssertEquals(true, log.IsInDatabase);
			});
		}

		public void TestProcessMessage_MessageReferenceCleared_WhenWithdrawCancelAccepted()
		{
			shipment.Logs.AddNew(Events.MessageSent, reference: $"MST={DocumentName}", dateTime: ZDateTimeOffset.Now.AddHours(-2));
			shipment.Logs.AddNew(Events.MessageWithdrawCancelRequest, reference: $"MST={DocumentName}", dateTime: ZDateTimeOffset.Now.AddHours(-1));
			var message = GetMessage(true);
			message.EM_LinkedObject = shipment;
			Factory.Save();

			var processor = CreateProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			Factory.Save();

			message.Reload();
			CombineAssertions("When Withdraw Cancel Accepted", () =>
			{
				AssertEquals("The message status is Processed", "PRS", message.EM_Status);

				var log = shipment.Logs.MostRecentLogByEventTime(Events.MessageWithdrawCancelAccepted);
				AssertNotNull("New log have been created", log);
				AssertEquals("the log Is In Database", true, log.IsInDatabase);
				AssertEquals("message is connected to Shipment", shipment, message.EM_LinkedObject);

				var cusEntryNumber = CusEntryNumber.LoadOrCreate<CusEntryNumber>(shipment, NumberType, CountryCodes.Israel);
				AssertNullOrEmpty("Message Reference Cleared", cusEntryNumber.CE_EntryNum);
			});
		}

		public void TestProcessMessageWithoutReference_ProcessedOKAndLogMessageEvent_WithLinkedObject()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var sentMessage = GetRequestMessage(shipment);
			sentMessage.EM_LinkedObject = shipment;
			var logger = new LoggingInformation();
			var packer = new ILMessagePacker();
			var sentInterchange = Factory.New<ILEDIInterchange>();
			_ = packer.Pack(sentMessage, sentInterchange, logger);

			var receivedInterchange = CreateTestInterchangeWithoutReference();

			receivedInterchange.EI_SessionGUID = sentInterchange.EI_SessionGUID;

			Factory.Save();

			sentMessage.Reload();
			var unpacker = new FeedbackMessageUnpacker<TMessage>();
			var loggerMock = new Mock<ILoggingInformation>();
			var messageBody = ILBusinessTestHelper.GetMessageBody(receivedInterchange.EI_BodyText);
			var messageResponseHeader = ILBusinessTestHelper.GetMessageResponseHeader(receivedInterchange.EI_BodyText);
			var unpackResult = unpacker.Unpack(receivedInterchange, messageBody, messageResponseHeader, ZString.Empty, null, null, loggerMock.Object);
			var message = unpackResult.EdiMessages.Single();
			message.EM_LinkedObject = sentMessage.EM_LinkedObject;

			var processor = CreateProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			Factory.Save();

			message.Reload();

			CombineAssertions("When no Reference Number in the message", () =>
			{
				AssertEquals("The message status is Processed", "PRS", message.EM_Status);
				AssertNotNull("message linked object should not be null", message.EM_LinkedObject);
				var forwardingShipment = (ForwardingShipment)message.EM_LinkedObject;
				var receivedEventlog = forwardingShipment.Logs.MostRecentLogByEventTime(Events.MessageReceived, new ZQuery(StmALogSchema.SL_Reference, message.EM_MessageType));
				AssertNotNull("A message received event should be logged", receivedEventlog);
				AssertEquals("the log Is In Database", true, receivedEventlog.IsInDatabase);
				var rejectedEventlog = forwardingShipment.Logs.MostRecentLogByEventTime(Events.MessageRejected);
				AssertNotNull("A message rejected event should be logged", rejectedEventlog);
				AssertEquals("the log Is In Database", true, rejectedEventlog.IsInDatabase);
				AssertContains($"|DEP=Customs|MST={DocumentName}|RFN=10000000", rejectedEventlog.SL_Reference);
				AssertEquals("message is connected to Shipment", forwardingShipment, message.EM_LinkedObject);
			});
		}

		public void TestProcessMessageWithoutReference_DiscardedWhenSentMessageNotFound_WithoutLinkedObject()
		{
			var receivedInterchange = CreateTestInterchangeWithoutReference();

			var unpacker = new FeedbackMessageUnpacker<TMessage>();
			var loggerMock = new Mock<ILoggingInformation>();
			var messageBody = ILBusinessTestHelper.GetMessageBody(receivedInterchange.EI_BodyText);
			var messageResponseHeader = ILBusinessTestHelper.GetMessageResponseHeader(receivedInterchange.EI_BodyText);
			var unpackResult = unpacker.Unpack(receivedInterchange, messageBody, messageResponseHeader, ZString.Empty, null, null, loggerMock.Object);
			var message = unpackResult.EdiMessages.Single();

			var loggingInformation = new LoggingInformation();
			var processor = CreateProcessor(loggingInformation);
			var result = processor.GetLinkedBusinessObjectMetaData(message, loggingInformation);
			Factory.Save();

			message.Reload();
			CombineAssertions("When No Reference Number in the message and Could not locate Shipment by original Sent message", () =>
			{
				Assert("Discard message provided", !result.DiscardReason.IsEmpty);
				AssertEquals("The message has note with text", CouldNotLocateByOriginalSentMessageMessage, result.DiscardReason);
			});
		}

		protected TMessage GetMessage(bool isWithdrawCancelMessage)
		{
			var message = Factory.New<TMessage>();
			message.EM_Status = "QUE";
			message.EM_ReceiveTransmit = "RCV";

			message.EM_MessageText =
				isWithdrawCancelMessage
				? WithdrawCancelMessageText
				: BasicSuccessfulMessageText;
			return message;
		}

		protected override void SetUp()
		{
			base.SetUp();

			(shipment, cusEntryNumber) = CreateForwardingShipmentAndEntryNumber();
			cusEntryNumber.CE_EntryNum = EntryNum;
		}

		protected void AssertLogsMessage(EnterpriseBusinessObject businessObject, TMessage message)
		{
			CombineAssertions("When One Number Found", () =>
			{
				AssertEquals("The message status is Processed", "PRS", message.EM_Status);

				var shipmentLogs = businessObject.Logs;
				var customsEntryStatusLog = shipmentLogs.MostRecentLogByEventTime(Events.CustomsEntryStatus);
				AssertNotNull("New CustomsEntryStatus log have been created", customsEntryStatusLog);
				AssertEquals("CustomsEntryStatuslog.ReferenceFreeText", GetExpectedCustomsEntryStatusLogReferenceFreeText, customsEntryStatusLog.ReferenceFreeText);

				var messageAcceptedLog = shipmentLogs.MostRecentLogByEventTime(Events.MessageAccepted);
				AssertNotNull("New log have been created", messageAcceptedLog);
				AssertEquals("the log Is In Database", true, messageAcceptedLog.IsInDatabase);

				var cusEntryNumber = CusEntryNumber.LoadOrCreate<CusEntryNumber>(businessObject, NumberType, CountryCodes.Israel);
				AssertEquals("Message Reference not Cleared", EntryNum, cusEntryNumber.CE_EntryNum);
			});
		}

		protected override ZGuid ExpectedBranchPk => GlbBranch.CurrentBranch.PK;

		protected override BusinessObject ExpectedLinkedObject => shipment;

		protected ZString CouldNotLocateByOriginalSentMessageMessage => "Could not locate Shipment by original Sent message.";
		protected abstract string WithdrawCancelMessageText { get; }
		protected abstract string NoReferenceMessageText { get; }
		protected abstract string NumberType { get; }
		protected abstract string CouldNotLocateMessage { get; }
		protected abstract string MoreThanOneMessage { get; }
		protected abstract string EntryNum { get; }
		protected abstract bool ExpectedSupportsConsol { get; }
		protected abstract string DocumentName { get; }
		protected abstract string GetExpectedCustomsEntryStatusLogReferenceFreeText { get; }

		protected abstract EDIMessage GetRequestMessage(ForwardingShipment forwardingShipment);

		protected (ForwardingShipment, CusEntryNumber) CreateForwardingShipmentAndEntryNumber()
		{
			var factory = Factory;
			var forwardingShipment = factory.NewWithValidTestData<ForwardingShipment>();
			var cusEntryNumber = CusEntryNumber.LoadOrCreate<CusEntryNumber>(forwardingShipment, NumberType, CountryCodes.Israel);
			return (forwardingShipment, cusEntryNumber);
		}

		protected ILEDIInterchange CreateTestInterchangeWithoutReference()
		{
			var interchange = Factory.NewWithValidTestData<ILEDIInterchange>();
			interchange.EI_From = "EASYLOG2TEST_EAD";
			interchange.EI_To = "HYEDFRCMT";
			interchange.EI_ApplicationCode = ILEDIInterchange.ApplicationCodes.ILCustoms;
			interchange.EI_InterchangeType = ExpectedMessageTypesToInclude;
			interchange.EI_InterchangeNum = "237";
			interchange.EI_ReceiveTransmit = Messaging.Integration.ReceiveTransmitList.Codes.Receive;
			interchange.EI_Status = ILEDIInterchange.Status.Queued;
			interchange.EI_BodyText = NoReferenceMessageText;
			return interchange;
		}

		protected ForwardingShipment shipment;
		protected CusEntryNumber cusEntryNumber;
	}
}
