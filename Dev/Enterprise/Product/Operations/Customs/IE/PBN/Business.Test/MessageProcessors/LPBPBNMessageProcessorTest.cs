using System;
using System.Text.Json;
using CargoWise.Customs.IE.MessageDefinitions.PBN;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.PBN.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.Business.Testing;

[TestedType(typeof(LPBPBNMessageProcessor))]
sealed class LPBPBNMessageProcessorTest : MessageAttacheeMessageProcessorTest<LPBPBNMessageProcessor, PBNInboundEDIMessage, PBNOutboundEDIMessage, LPBPBNProvider, AsycudaManifestHeader, AsycudaManifestHeader>
{
	protected override ZString MessageFriendlyName => "PBN LPB Message Processor";

	protected override LPBPBNMessageProcessor Processor => new LPBPBNMessageProcessor(logger, typeof(LPBDefinition));

	protected override ZString MessageType => PBNMessageTypes.Codes.LookupPBN;

	protected override ZString MessageText => JsonSerializer.Serialize(GetLPBResponse());

	public void TestMessageInterpreterType()
	{
		var processor = new LPBPBNMessageProcessorForTest(logger, typeof(LPBDefinition));
		AssertEquals("Message Processor should have a Correct MessageInterpreterType.", typeof(LPBPBNMessageInterpreter), processor.MessageInterpreterType);
	}

	public void TestMessageProcessorType()
	{
		var incomingMessage = PBNMessageTestHelper.SetupMessages(Factory, string.Empty, MessageType).incomingMessage;
		var messageProcessor = new IE.Business.BranchMessageProcessor();
		AssertType("Type should be LPBPBNMessageProcessor", typeof(LPBPBNMessageProcessor), messageProcessor.GetApplicationTypeProcessorCore(incomingMessage));
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
			AssertEquals("Message ApplicationReference", "AB123FGH", incomingMessage.EM_ApplicationReference);
		});
	}

	public override void TestEnsureMessageTextIsValid()
	{
		CombineAssertions(() =>
		{
			AssertNotNullOrEmpty("Message text is not null or empty", MessageText);
			var dataObject = JsonSerializer.Deserialize(MessageText, typeof(LPBDefinition));
			AssertNotNull("Deserialized object is not null", dataObject);
		});
	}

	static LPBDefinition textForLPBResponse;

	static LPBDefinition GetLPBResponse()
	{
		if (textForLPBResponse == null)
		{
			textForLPBResponse = new LPBDefinition();
			textForLPBResponse.PbnID = "AB123FGH";
			textForLPBResponse.Status = "INCOMPLETE";
			textForLPBResponse.Issue = "Some fields are missing";
			textForLPBResponse.Direction = "IN_IRELAND";
			textForLPBResponse.EmptyVehicle = false;
			textForLPBResponse.Declarations =
			[
				new() { DeclarationId = "9511", DeclarationType = "New Declaration" },
				new() { DeclarationId = "9510", DeclarationType = "import" }
			];
			textForLPBResponse.ContactDetails = new PBNContactDetails { Email = "TestEmail@id.org", MobileNum1 = "+12345678910", MobileNum2 = "+910000000" };
		}
		return textForLPBResponse;
	}

	protected override (AsycudaManifestHeader declaration, AsycudaManifestHeader messageAttachee, IE.Business.EDIMessage outgoingMessage, PBNInboundEDIMessage incomingMessage) CreateSetupData(string incomingMessageText = null)
	{
		var testData = PBNMessageTestHelper.SetupMessages(Factory, MessageText, MessageType, true);
		return (testData.relatedJobAndAttachee, testData.relatedJobAndAttachee, testData.outgoingMessage, testData.incomingMessage);
	}

	sealed class LPBPBNMessageProcessorForTest : LPBPBNMessageProcessor
	{
		public LPBPBNMessageProcessorForTest(LoggingInformation logger, Type messageObjectType) : base(logger, messageObjectType) { }
		public new Type MessageInterpreterType => base.MessageInterpreterType;
	}
}
