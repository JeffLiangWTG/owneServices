using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class BaseMessageProcessorBaseOnlyTest : BaseMessageProcessorAbstractTest<MessageProcessorForTest, IInboundMessageDataProvider>
{
	public void TestProcessPreProcessOKMessage() => CombineAssertions(() =>
	{
		const string messageRawText = "MessageRawText";
		inboundMessage.EM_MessageText = messageRawText;
		inboundMessage.EM_Status = EDIMessageStatusList.Codes.PreProcessedOK;
		processor.ProcessMessage(inboundMessage, new LoggingInformation());
		AssertEquals("Message processed", 1, processor.ProcessPreProcessOKMessageCalls);
		AssertEquals("Message Interpreter not set", messageRawText, inboundMessage.EM_MessageInterpretation);

		processor.MessageInterpreter = new MessageInterpreterForTest();
		processor.ProcessMessage(inboundMessage, new LoggingInformation());
		AssertEquals("Message Interpreted", "MessageInterpretationForTest", inboundMessage.EM_MessageInterpretation);
	});

	public void TestProcessPreProcessMessage_UnsupportedMessageStatus()
	{
		const string messageRawText = "MessageRawText";
		inboundMessage.EM_MessageText = messageRawText;
		inboundMessage.EM_Status = EDIMessageStatusList.Codes.Queued;

		AssertExceptionThrown<InvalidOperationException>($"Unsupported message status: {inboundMessage.EM_Status}", () => processor.ProcessMessage(inboundMessage, new LoggingInformation()));
	}

	public void TestGetLinkedBusinessObjectMetaData_WhenOutgoingMessageExists()
	{
		var linkedObject = Factory.New<LinkedObjectForTest>();
		processor.OutgoingMessage = Factory.New<EDIMessage>();
		processor.OutgoingMessage.EM_LinkedObject = linkedObject;

		var result = processor.GetLinkedBusinessObjectMetaData(inboundMessage, new LoggingInformation());

		AssertEquals(ProcessingResult.New(new LinkedBusinessObjectMetaData(linkedObject.TableName, linkedObject.PK, processor.OutgoingMessage.EM_GB, linkedObject.JobNumber)), result);
	}

	public void TestGetLinkedBusinessObjectMetaData_WhenOutgoingMessageNotExist()
	{
		processor.OutgoingMessage = null;

		var result = processor.GetLinkedBusinessObjectMetaData(inboundMessage, new LoggingInformation());

		AssertEquals(ProcessingResult.New(LinkedBusinessObjectMetaData.Empty, (NoResString)"Message is discarded. Cannot find corresponding sent message."), result);
	}

	public void TestGetSerializationKeysResult_WhenJobNumberIsNotEmpty()
	{
		var metaData = new LinkedBusinessObjectMetaData(new ZString("TableName"), ZGuid.BrettsGuid, ZGuid.BrettsGuid, new ZString("JobNumber"));

		var result = processor.GetSerializationKeysResult(inboundMessage, new LoggingInformation(), metaData);

		AssertEquals(metaData.JobNumber, result.ReturnValue.Keys.Single());
	}

	public void TestGetSerializationKeysResult_WhenJobNumberIsEmpty()
	{
		var metaData = new LinkedBusinessObjectMetaData(new ZString("TableName"), ZGuid.BrettsGuid, ZGuid.BrettsGuid, ZString.Empty);

		var result = processor.GetSerializationKeysResult(inboundMessage, new LoggingInformation(), metaData);

		AssertEquals(ProcessingResult.New(SerializationKeysResult.SerialProcessingInReceivedOrder), result);
	}
}

sealed class MessageProcessorForTest : BaseMessageProcessor<IInboundMessageDataProvider>
{
	public int ProcessPreProcessOKMessageCalls;

	public EDIMessage OutgoingMessage;

	protected override EDIMessage GetOutgoingMessage(EDIMessage message, IInboundMessageDataProvider dataProvider)
	{
		return OutgoingMessage;
	}

	protected override void ProcessPreProcessOKMessageCore(EDIMessage message, IInboundMessageDataProvider dataProvider, LoggingInformation logger) => ProcessPreProcessOKMessageCalls++;

	public IMessageInterpreter<IInboundMessageDataProvider> MessageInterpreter { get; set; }

	protected override IMessageInterpreter<IInboundMessageDataProvider> GetMessageInterpreter(EDIMessage message)
		=> MessageInterpreter;
}

sealed class MessageInterpreterForTest : IMessageInterpreter<IInboundMessageDataProvider>
{
	public ZString GetMessageInterpretation(IInboundMessageDataProvider dataProvider) => "MessageInterpretationForTest";
}
