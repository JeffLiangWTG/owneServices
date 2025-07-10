using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Business.MessageProcessor.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(INCUniversalCustomsMessageProcessor))]
sealed class INCUniversalCustomsMessageProcessorTest : UniversalCustomsMessageProcessorTest<INCUniversalCustomsMessageProcessor>
{
	public void TestHasMessageNeedingASeparateFactory()
	{
		AssertEquals(true, new INCUniversalCustomsMessageProcessor().HasMessageNeedingASeparateFactory);
	}

	public void TestShouldMessageBeProcessedInASeparateFactory()
	{
		AssertEquals(false, new INCUniversalCustomsMessageProcessor().ShouldMessageBeProcessedInASeparateFactory(Factory.New<EDIMessage>()));
	}

	public void TestGetLinkedBusinessObjectMetaData()
	{
		var incomingMessage = Factory.New<EDIMessage>();
		incomingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.INCustoms;
		incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		incomingMessage.EM_MessageData = new EmbeddedResourceRetriever().GetBytes("Enterprise.Customs.IN.Business.Testing.Message.MessageProcessors.TestFiles.EmailWithoutAttachment.eml");

		var expectedLog = "\tFailed to load the outgoing message by email subject info: Message Number: 0000001, Message Sub Type: ACM, Filing Date: 20240819, Receiver ID: INBLR4.";
		var expectedResult = ProcessingResult.New(LinkedBusinessObjectMetaData.Empty, (NoResString)$"Unable to get the job related to this message: {expectedLog}");
		AssertGetLinkedBusinessObjectMetaData(incomingMessage, expectedResult,
			expectedLog: expectedLog,
			expectedDebugLog: " *** \tGet LinkedObject by FileProcessingErrorMessageProcessor");
	}

	public void TestGetLinkedBusinessObjectMetaData_CheckLinkObject()
	{
		var linkObject = Factory.NewWithValidTestData<OrgHeader>();

		var outgoingMessage = Factory.New<EDIMessage>();
		outgoingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.INCustoms;
		outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		outgoingMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.AirCgm;
		outgoingMessage.EM_MessageType = EDIMessageTypeList.Codes.ConsolGeneralManifest;
		outgoingMessage.EM_MessageOwner = "INBLR4";
		outgoingMessage.EM_SystemCreateTimeUtc = new DateTime(2024, 8, 19);
		outgoingMessage.EM_LinkedObject = linkObject;
		outgoingMessage.EM_MessageNum = "0000001";

		var incomingMessage = Factory.New<EDIMessage>();
		incomingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.INCustoms;
		incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		incomingMessage.EM_MessageData = new EmbeddedResourceRetriever().GetBytes("Enterprise.Customs.IN.Business.Testing.Message.MessageProcessors.TestFiles.EmailWithoutAttachment.eml");
		Factory.Save();

		var logger = new LoggingInformation();
		new INCUniversalCustomsMessageProcessor().GetLinkedBusinessObjectMetaData(incomingMessage, logger);

		CombineAssertions(() =>
		{
			AssertEquals("LinkObject is not IMessageAttachee, type is Enterprise.MasterFiles.Business.OrgHeader", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.ActiveEntryHeaders.AddNew();
			outgoingMessage.EM_LinkedObject = header;
			declaration.JE_DeclarationReference = string.Empty;

			new INCUniversalCustomsMessageProcessor().GetLinkedBusinessObjectMetaData(incomingMessage, logger);
			AssertEquals("LinkObject is not IJobNumber or it has empty JobNumber, type is Enterprise.Customs.IN.Business.CusEntryHeader", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		});
	}

	public void TestProcessMessage()
	{
		var incomingMessage = Factory.New<EDIMessage>();
		incomingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.INCustoms;
		incomingMessage.EM_ReceiveTransmit = Messaging.Business.EDIMessage.Direction.Receive;
		incomingMessage.EM_MessageData = new EmbeddedResourceRetriever().GetBytes("Enterprise.Customs.IN.Business.Testing.Message.MessageProcessors.TestFiles.EmailWithoutAttachment.eml");

		AssertMessageProcessed(incomingMessage, Messaging.Business.EDIMessage.Status.Failed,
			expectedLog: "Failed to load the outgoing message by email subject info: Message Number: 0000001, Message Sub Type: ACM, Filing Date: 20240819, Receiver ID: INBLR4.",
			expectedDebugLog: " *** \tMessage processing by FileProcessingErrorMessageProcessor");
	}

	protected override string ApplicationCode => ApplicationCodeList.Codes.INCustoms;

	void AssertMessageProcessed(EDIMessage incomingMessage, string expectdStatus, string expectedLog = null, string expectedDebugLog = null)
	{
		CombineAssertions(() =>
		{
			var logger = new LoggingInformation();
			new INCUniversalCustomsMessageProcessor().ProcessMessage(incomingMessage, logger, null);
			AssertEquals("EM_Status", expectdStatus, incomingMessage.EM_Status);
			AssertContains("Processing Log", expectedLog, incomingMessage.Notes.FindByDescription("Processing Log").SingleOrDefault()?.ST_NoteText);
			AssertEquals("Debug Log", expectedDebugLog, logger.DebugLogStrings[0]);
		});
	}

	void AssertGetLinkedBusinessObjectMetaData(EDIMessage incomingMessage, ProcessingResult<LinkedBusinessObjectMetaData> expectedResult, string expectedLog, string expectedDebugLog)
	{
		CombineAssertions(() =>
		{
			var logger = new LoggingInformation();
			var result = new INCUniversalCustomsMessageProcessor().GetLinkedBusinessObjectMetaData(incomingMessage, logger);
			AssertEquals("Result", expectedResult, result);
			AssertMultilineASCIIEquals("Logs", expectedLog, string.Join("\r\n", logger.UserLogStrings.Cast<string>()));
			AssertEquals("Debug Log", expectedDebugLog, logger.DebugLogStrings[0]);
		});
	}
}
