using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Customs.AEManifest;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(CUSRESMessageProcessor))]
sealed class CUSRESMessageProcessorTest : BaseMessageProcessorAbstractTest<CUSRESMessageProcessor, ICUSRESDataProvider>
{
	public void TestProcessPreProcessOKMessage_ActionCodedDefault() => CombineAssertions(() =>
	{
		const string entryStatus = "ENT";
		inboundMessage.EM_Status = EDIMessageStatusList.Codes.PreProcessedOK;
		var linkedObject = Factory.New<LinkedObjectForTest>();
		inboundMessage.EM_LinkedObject = linkedObject;
		mockDataProvider.Setup(x => x.EntryStatus).Returns(entryStatus);
		mockDataProvider.Setup(x => x.InformationRequests).Returns(Array.Empty<IInformationRequest>());

		processor.ProcessMessage(inboundMessage, new LoggingInformation());

		AssertEquals("EntryStatus", entryStatus, linkedObject.EntryStatus);
		AssertEquals("MessageStatus", AEConstants.Messaging.MessageTypes.CUSRES, linkedObject.MessageStatus);
		AssertEquals("EM_Status", EDIMessageStatusList.Codes.ProcessedOK, inboundMessage.EM_Status);
		AssertCONTRLResponseMsgCreated();
	});

	public void TestProcessPreProcessOKMessage_CannotParseMessage() => CombineAssertions(() =>
	{
		inboundMessage.EM_Status = EDIMessageStatusList.Codes.PreProcessedOK;
		inboundMessage.EM_LinkedObject = null;
		mockDataProvider.Setup(x => x.IsParsed).Returns(false);
		mockDataProvider.Setup(x => x.InformationRequests).Returns(Array.Empty<IInformationRequest>());
		var logger = new LoggingInformation();

		processor.ProcessMessage(inboundMessage, logger);

		AssertEquals("EM_Status", EDIMessageStatusList.Codes.Discarded, inboundMessage.EM_Status);
		AssertEquals("StmNote", "Cannot parse CUSRES message.", inboundMessage.Notes.FindByDescription("Discard Reason")[0].ST_NoteText);
		AssertCollectionContains("log", "\tStatus set to Discarded due to the following reason: Cannot parse CUSRES message.", logger.UserLogStrings);
		AssertCONTRLResponseMsgCreated();
	});

	public void TestProcessPreProcessOKMessage_CannotFindLinkedObject() => CombineAssertions(() =>
	{
		inboundMessage.EM_Status = EDIMessageStatusList.Codes.PreProcessedOK;
		inboundMessage.EM_LinkedObject = null;
		mockDataProvider.Setup(x => x.IsParsed).Returns(true);
		mockDataProvider.Setup(x => x.InformationRequests).Returns(Array.Empty<IInformationRequest>());
		var logger = new LoggingInformation();

		processor.ProcessMessage(inboundMessage, logger);

		AssertEquals("EM_Status", EDIMessageStatusList.Codes.Discarded, inboundMessage.EM_Status);
		AssertEquals("StmNote", "Cannot find corresponding linked object.", inboundMessage.Notes.FindByDescription("Discard Reason")[0].ST_NoteText);
		AssertCollectionContains("log", "\tStatus set to Discarded due to the following reason: Cannot find corresponding linked object.", logger.UserLogStrings);
		AssertCONTRLResponseMsgCreated();
	});

	void AssertCONTRLResponseMsgCreated()
	{
		var cONTRLMessage = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit));
		AssertEquals(AEConstants.Messaging.MessageTypes.CONTRL, cONTRLMessage.EM_MessageType);
	}

	public void TestMessageInterpreter()
	{
		var interpreterProvider = processor.GetType().GetMethod("GetMessageInterpreter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		var interpreter = interpreterProvider.Invoke(processor, new object[] { inboundMessage });
		AssertType<CUSRESMessageInterpreter>(interpreter);
	}

	public void TestGetSerializationKeysResult()
	{
		var outboundMessage = Factory.New<AEEDIMessage>();
		outboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		var outboundMessageLinkedObject = Factory.New<LinkedObjectForTest>();
		outboundMessage.EM_LinkedObject = outboundMessageLinkedObject;
		outboundMessageLinkedObject.Messages.Add(outboundMessage);

		var mockMessageAttacheeProvider = new Mock<IMessageAttacheeProvider<ICUSRESDataProvider>>();
		mockMessageAttacheeProvider.Setup(x => x.GetAttachee(inboundMessage, It.IsAny<ICUSRESDataProvider>())).Returns(outboundMessageLinkedObject);

		var mockManifestController = new Mock<IManifestController>();
		mockManifestController.Setup(x => x.MessageAttacheeProvider).Returns(mockMessageAttacheeProvider.Object);

		var manifestController = mockManifestController.Object;

		using (ObjectFactory.Substitute(manifestController))
		{
			mockDataProvider.Setup(x => x.OutgoingAccessReference).Returns("DocumentIdentifier");
			var linkedBusinessObjectMetaData = processor.GetLinkedBusinessObjectMetaData(inboundMessage, new LoggingInformation());
			AssertEquals(ProcessingResult.New(new LinkedBusinessObjectMetaData(outboundMessageLinkedObject.TableName, outboundMessageLinkedObject.PK, outboundMessage.EM_GB, outboundMessageLinkedObject.JobNumber)),
				linkedBusinessObjectMetaData);
			var serializationKeysResult = processor.GetSerializationKeysResult(inboundMessage, new LoggingInformation(), linkedBusinessObjectMetaData.ReturnValue);
			AssertEquals(outboundMessageLinkedObject.JobNumber, serializationKeysResult.ReturnValue.Keys.Single());
		}
	}

	public void TestGetSerializationKeysResult_WhenJobNumberIsEmpty()
	{
		var metaData = new LinkedBusinessObjectMetaData(new ZString("TableName"), ZGuid.BrettsGuid, ZGuid.BrettsGuid, ZString.Empty);

		var result = processor.GetSerializationKeysResult(inboundMessage, new LoggingInformation(), metaData);

		AssertEquals(ProcessingResult.New(SerializationKeysResult.UnconstrainedParallelProcessing), result);
	}

	public void TestGetLinkedBusinessObjectMetaData_WhenOutgoingMessageNotExist()
	{
		var mockMessageAttacheeProvider = new Mock<IMessageAttacheeProvider<ICUSRESDataProvider>>();
		mockMessageAttacheeProvider.Setup(x => x.GetAttachee(inboundMessage, It.IsAny<ICUSRESDataProvider>())).Returns((IMessageAttachee)null);

		var mockManifestController = new Mock<IManifestController>();
		mockManifestController.Setup(x => x.MessageAttacheeProvider).Returns(mockMessageAttacheeProvider.Object);

		var manifestController = mockManifestController.Object;

		using (ObjectFactory.Substitute(manifestController))
		{
			mockDataProvider.Setup(x => x.OutgoingAccessReference).Returns("DocumentIdentifier");
			var result = processor.GetLinkedBusinessObjectMetaData(inboundMessage, new LoggingInformation());
			AssertEquals(ProcessingResult.New(LinkedBusinessObjectMetaData.Empty, (NoResString)string.Empty), result);
		}
	}
}
