using System.Linq;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(CONTRLMessageProcessor))]
sealed class CONTRLMessageProcessorTest : BaseMessageProcessorAbstractTest<CONTRLMessageProcessor, ICONTRLDataProvider>
{
	public void TestProcessPreProcessOKMessage_ActionCoded4() => AssertProcessPreProcessOKMessage("4", "ERR", "CTL");

	public void TestProcessPreProcessOKMessage_ActionCoded7() => AssertProcessPreProcessOKMessage("7", "ERR", "CTL");

	public void TestProcessPreProcessOKMessage_ActionCoded8() => AssertProcessPreProcessOKMessage("8", "ACK", "CTL");

	public void TestProcessPreProcessOKMessage_ActionCodedDefault() => AssertProcessPreProcessOKMessage("", "UNK", "CTL");

	public void TestMessageInterpreter()
	{
		var interpreterProvider = processor.GetType().GetMethod("GetMessageInterpreter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		var interpreter = interpreterProvider.Invoke(processor, new object[] { inboundMessage });
		AssertType<CONTRLMessageInterpreter>(interpreter);
	}

	public void TestGetSerializationKeysResult()
	{
		var outboundInterchange = Factory.New<EDIInterchange>();
		outboundInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.UAECustoms;
		outboundInterchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
		outboundInterchange.EI_Status = EDIInterchange.Status.Sent;
		outboundInterchange.EI_InterchangeNum = "Interchange1";
		var outboundMessage = Factory.New<AEEDIMessage>();
		outboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		var outboundMessageLinkedObject = Factory.New<LinkedObjectForTest>();
		outboundMessage.EM_LinkedObject = outboundMessageLinkedObject;
		outboundInterchange.ContainedMessages.Add(outboundMessage);
		mockDataProvider.Setup(x => x.OutgoingAccessReference).Returns("Interchange1");
		var linkedBusinessObjectMetaData = processor.GetLinkedBusinessObjectMetaData(inboundMessage, new LoggingInformation());
		AssertEquals(ProcessingResult.New(new LinkedBusinessObjectMetaData(outboundMessageLinkedObject.TableName, outboundMessageLinkedObject.PK, outboundMessage.EM_GB, outboundMessageLinkedObject.JobNumber)),
			linkedBusinessObjectMetaData);
		var serializationKeysResult = processor.GetSerializationKeysResult(inboundMessage, new LoggingInformation(), linkedBusinessObjectMetaData.ReturnValue);
		AssertEquals(outboundMessageLinkedObject.JobNumber, serializationKeysResult.ReturnValue.Keys.Single());
	}

	void AssertProcessPreProcessOKMessage(string actionCode, string expectedEntryStatus, string expectedMessageStatus) => CombineAssertions(() =>
	{
		inboundMessage.EM_Status = EDIMessageStatusList.Codes.PreProcessedOK;
		var linkedObject = Factory.New<LinkedObjectForTest>();
		inboundMessage.EM_LinkedObject = linkedObject;
		var mockInterchangeResponse = new Mock<ICONTRLInterchangeResponseProvider>();
		mockInterchangeResponse.Setup(x => x.ActionCode).Returns(actionCode);
		mockDataProvider.Setup(x => x.InterchangeResponse).Returns(mockInterchangeResponse.Object);

		processor.ProcessMessage(inboundMessage, new LoggingInformation());

		AssertEquals("EntryStatus", expectedEntryStatus, linkedObject.EntryStatus);
		AssertEquals("MessageStatus", expectedMessageStatus, linkedObject.MessageStatus);
		AssertEquals("EM_Status", EDIMessageStatusList.Codes.ProcessedOK, inboundMessage.EM_Status);
	});
}
