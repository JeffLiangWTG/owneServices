using System;
using System.Text.Json;
using CargoWise.Customs.IE.MessageDefinitions.PBN;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.PBN.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.Business.Testing;

[TestedType(typeof(LPCPBNMessageProcessor))]
sealed class LPCPBNMessageProcessorTest : MessageAttacheeMessageProcessorTest<LPCPBNMessageProcessor, PBNInboundEDIMessage, PBNOutboundEDIMessage, LPCPBNProvider, AsycudaManifestHeader, AsycudaManifestHeader>
{
	protected override ZString MessageFriendlyName => "PBN LPC Message Processor";

	protected override LPCPBNMessageProcessor Processor => new LPCPBNMessageProcessor(logger, typeof(LPCDefinition));

	protected override ZString MessageType => PBNMessageTypes.Codes.LookupPBNChannel;

	protected override ZString MessageText => JsonSerializer.Serialize(GetLPCResponse());

	public void TestMessageInterpreterType()
	{
		var processor = new LPCPBNMessageProcessorForTest(logger, typeof(LPCDefinition));
		AssertEquals("Message Processor should have a Correct MessageInterpreterType.", typeof(LPCPBNMessageInterpreter), processor.MessageInterpreterType);
	}

	public void TestMessageProcessorType()
	{
		var incomingMessage = PBNMessageTestHelper.SetupMessages(Factory, string.Empty, MessageType).incomingMessage;
		var messageProcessor = new IE.Business.BranchMessageProcessor();
		AssertType("Type should be LPCPBNMessageProcessor", typeof(LPCPBNMessageProcessor), messageProcessor.GetApplicationTypeProcessorCore(incomingMessage));
	}

	public void TestMessageProcessor_EmptyMessage()
	{
		var incomingMessage = PBNMessageTestHelper.SetupMessages(Factory, string.Empty, MessageType, true).incomingMessage;
		ProcessMessage(incomingMessage);
		CombineAssertions(() =>
		{
			AssertEquals("Message Status", "FAL", incomingMessage.EM_Status);
			AssertEquals("Message ApplicationReference", ZString.Empty, incomingMessage.EM_ApplicationReference);
		});
	}

	public void TestMessageProcessor_InvalidMessage()
	{
		var incomingMessage = PBNMessageTestHelper.SetupMessages(Factory, "1234", MessageType, true).incomingMessage;
		ProcessMessage(incomingMessage);
		CombineAssertions(() =>
		{
			AssertEquals("Message Status", "FAL", incomingMessage.EM_Status);
			AssertEquals("Message ApplicationReference", ZString.Empty, incomingMessage.EM_ApplicationReference);
		});
	}

	public void TestMessageProcessor_ValidMessage()
	{
		var incomingMessage = PBNMessageTestHelper.SetupMessages(Factory, MessageText, MessageType, true).incomingMessage;
		ProcessMessage(incomingMessage);
		CombineAssertions(() =>
		{
			AssertEquals("Message Status", "PRS", incomingMessage.EM_Status);
			AssertEquals("Message ApplicationReference", "AB1234GH", incomingMessage.EM_ApplicationReference);
		});
	}

	public override void TestEnsureMessageTextIsValid()
	{
		CombineAssertions(() =>
		{
			AssertNotNullOrEmpty("Message text is not null or empty", MessageText);
			var dataObject = JsonSerializer.Deserialize(MessageText, typeof(LPCDefinition));
			AssertNotNull("Deserialized object is not null", dataObject);
		});
	}

	static LPCDefinition textForLPCResponse;

	static LPCDefinition GetLPCResponse()
	{
		if (textForLPCResponse == null)
		{
			textForLPCResponse = new LPCDefinition();
			textForLPCResponse.PbnID = "AB1234GH";
			textForLPCResponse.Channel = "History channel";
			textForLPCResponse.Action = "Some fields are missing";
			textForLPCResponse.PairedTransport = new PBNPairedTransport
			{ CustomsOffice = "IE Customs", ShipId = "AA1234", RegistrationNumber = "ASD3358", ScheduledTimeofArrival = "202501021324" };
		}
		return textForLPCResponse;
	}

	protected override (AsycudaManifestHeader declaration, AsycudaManifestHeader messageAttachee, IE.Business.EDIMessage outgoingMessage, PBNInboundEDIMessage incomingMessage) CreateSetupData(string incomingMessageText = null)
	{
		var testData = PBNMessageTestHelper.SetupMessages(Factory, MessageText, MessageType, true);
		return (testData.relatedJobAndAttachee, testData.relatedJobAndAttachee, testData.outgoingMessage, testData.incomingMessage);
	}

	sealed class LPCPBNMessageProcessorForTest : LPCPBNMessageProcessor
	{
		public LPCPBNMessageProcessorForTest(LoggingInformation logger, Type messageObjectType) : base(logger, messageObjectType) { }
		public new Type MessageInterpreterType => base.MessageInterpreterType;
	}
}
