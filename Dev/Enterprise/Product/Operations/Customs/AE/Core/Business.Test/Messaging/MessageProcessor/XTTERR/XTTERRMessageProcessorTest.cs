using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(XTTERRMessageProcessor))]
sealed class XTTERRMessageProcessorTest : BaseMessageProcessorAbstractTest<XTTERRMessageProcessor, IXTTERRDataProvider>
{
	public void TestProcessPreProcessOKMessage() => CombineAssertions(() =>
	{
		var sessionId = ZGuid.NewZGuid();
		var linkedObject = Factory.New<LinkedObjectForTest>();

		var outboundInterchange = Factory.New<EDIInterchange>();
		outboundInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.UAECustoms;
		outboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		outboundInterchange.EI_Status = EDIInterchangeStatusList.Codes.Sent;
		outboundInterchange.EI_InterchangeNum = "Interchange1";
		outboundInterchange.EI_SessionGUID = sessionId;

		var outboundMessage = Factory.New<AEEDIMessage>();
		outboundMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		outboundMessage.EM_LinkedObject = linkedObject;
		outboundInterchange.ContainedMessages.Add(outboundMessage);

		var inboundInterchange = Factory.New<EDIInterchange>();
		inboundInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.UAECustoms;
		inboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		inboundInterchange.EI_InterchangeNum = "Interchange2";
		inboundInterchange.EI_SessionGUID = sessionId;

		inboundMessage.EM_Status = EDIMessageStatusList.Codes.PreProcessedOK;
		inboundMessage.EM_LinkedObject = linkedObject;
		inboundInterchange.ContainedMessages.Add(inboundMessage);
		mockDataProvider.Setup(x => x.OutgoingAccessReference).Returns("Interchange1");

		processor.ProcessMessage(inboundMessage, new LoggingInformation());

		AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, outboundMessage.EM_Status);
		AssertEquals("EntryStatus", EDIMessageStatusList.Codes.Error, linkedObject.EntryStatus);
		AssertEquals("MessageStatus", EDIMessageStatusList.Codes.Discarded, linkedObject.MessageStatus);
		AssertEquals("Note", "Message failed due to xT sending error.", outboundMessage.Notes.FindByDescription(InterchangeProviderBase.ProcessingLogDescription).Single().ST_NoteDataAsText);
	});

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
}
