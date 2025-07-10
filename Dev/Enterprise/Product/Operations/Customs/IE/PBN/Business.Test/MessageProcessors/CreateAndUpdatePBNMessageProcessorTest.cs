using System;
using System.Text.Json;
using CargoWise.Customs.IE.MessageDefinitions.PBN;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.PBN.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.Business.Testing;

[TestedType(typeof(CreateAndUpdatePBNMessageProcessor))]
sealed class CreateAndUpdatePBNMessageProcessorTest : MessageAttacheeMessageProcessorTest<CreateAndUpdatePBNMessageProcessor, PBNInboundEDIMessage, PBNOutboundEDIMessage, CreateAndUpdatePBNProvider, AsycudaManifestHeader, AsycudaManifestHeader>
{
	protected override ZString MessageFriendlyName => "PBN Message Processor for CPB, UPB and UPD";

	protected override CreateAndUpdatePBNMessageProcessor Processor => new CreateAndUpdatePBNMessageProcessor(logger, typeof(CreateAndUpdatePBNMessageDefinition));

	protected override ZString MessageType => PBNMessageTypes.Codes.CreatePBN;

	protected override ZString MessageText => JsonSerializer.Serialize(PBNMessageTestHelper.GetCreateOrUpdatePBNResponse());

	protected override void AssertProcessResultCore(AsycudaManifestHeader header, PBNInboundEDIMessage incomingMessage)
	{
		AssertEquals("Should update AMA_MessageStatus", LogicalStatusList.Codes.Accepted, header.AMA_MessageStatus);
		AssertEquals("Should update RegistrationStatus", PBNCustomsStatusList.Codes.Proceed, header.RegistrationStatus);

		AssertEquals("Should have 1 customs declaration left, 1 has been deleted", 1, header.CustomsReferenceCollection.Count);
		foreach (var declaration in header.CustomsReferenceCollection)
		{
			AssertEquals("Should update customs declaration item CSI_Status", PBNDeclarationReferenceStatusList.Codes.HBA, declaration.CSI_Status);
		}
		AssertEquals("Should have 1 transit declaration left, 1 has been deleted", 1, header.CustomsReferenceCollection.Count);
		foreach (var declaration in header.TransitDeclarationCollection)
		{
			AssertEquals("Should update transit declaration item CSI_Status", PBNDeclarationReferenceStatusList.Codes.HBA, declaration.CSI_Status);
		}

		MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for MAN0000001",
				["A CPB - Create PBN message which contains 'Manifest Job Number' has been received for Request Report AA11GH99."],
				["staff1@where.com"]);
	}

	public void TestMessageInterpreterType()
	{
		var processor = new CPBMessageProcessorForTest(logger, typeof(CreateAndUpdatePBNMessageDefinition));
		AssertEquals("Message Processor should have a Correct MessageInterpreterType.", typeof(CreateAndUpdatePBNMessageInterpreter), processor.MessageInterpreterType);
	}

	public void TestMessageProcessorType()
	{
		CombineAssertions("Message processor type for each message type", () =>
		{
			CreateMessageAndCheck(PBNMessageTypes.Codes.CreatePBN);
			CreateMessageAndCheck(PBNMessageTypes.Codes.UpdatePBNDeclarations);
			CreateMessageAndCheck(PBNMessageTypes.Codes.UpdatePBN);
		});

		void CreateMessageAndCheck(string messageType)
		{
			var incomingMessage = PBNMessageTestHelper.SetupMessages(Factory, string.Empty, messageType).incomingMessage;

			var messageProcessor = new IE.Business.BranchMessageProcessor();
			AssertType($"Processor Type should be CreateAndUpdatePBNMessageProcessor for message: {messageType}", typeof(CreateAndUpdatePBNMessageProcessor), messageProcessor.GetApplicationTypeProcessorCore(incomingMessage));
		}
	}

	public void TestMessageProcessor_EmptyMessage()
	{
		CreateAndProcessMessage(string.Empty, PBNMessageTypes.Codes.CreatePBN, "FAL", string.Empty);
		CreateAndProcessMessage(string.Empty, PBNMessageTypes.Codes.UpdatePBNDeclarations, "FAL", string.Empty);
		CreateAndProcessMessage(string.Empty, PBNMessageTypes.Codes.UpdatePBN, "FAL", string.Empty);
	}

	public void TestMessageProcessor_InvalidMessage()
	{
		CreateAndProcessMessage("1234", PBNMessageTypes.Codes.CreatePBN, "FAL", string.Empty);
		CreateAndProcessMessage("1234", PBNMessageTypes.Codes.UpdatePBNDeclarations, "FAL", string.Empty);
		CreateAndProcessMessage("1234", PBNMessageTypes.Codes.UpdatePBN, "FAL", string.Empty);
	}

	public void TestMessageProcessor_ValidMessage()
	{
		CreateAndProcessMessage(MessageText, PBNMessageTypes.Codes.CreatePBN, "PRS", "AA11GH99");
		CreateAndProcessMessage(MessageText, PBNMessageTypes.Codes.UpdatePBNDeclarations, "PRS", "AA11GH99");
		CreateAndProcessMessage(MessageText, PBNMessageTypes.Codes.UpdatePBN, "PRS", "AA11GH99");
	}

	public override void TestEnsureMessageTextIsValid()
	{
		CombineAssertions(() =>
		{
			AssertNotNullOrEmpty("Message text is not null or empty", MessageText);
			var dataObject = JsonSerializer.Deserialize(MessageText, typeof(CreateAndUpdatePBNMessageDefinition));
			AssertNotNull("Deserialized object is not null", dataObject);
		});
	}

	void CreateAndProcessMessage(string messageText, string messageType, string expectedMessageStatus, string expectedMessageApplicationReference)
	{
		var incomingMessage = PBNMessageTestHelper.SetupMessages(Factory, messageText, messageType, true).incomingMessage;

		ProcessMessage(incomingMessage);
		CombineAssertions($"Message type is: {messageType}", () =>
		{
			AssertEquals("Message Status", expectedMessageStatus, incomingMessage.EM_Status);
			AssertEquals("Message ApplicationReference", expectedMessageApplicationReference, incomingMessage.EM_ApplicationReference);
		});
	}

	protected override (AsycudaManifestHeader declaration, AsycudaManifestHeader messageAttachee, IE.Business.EDIMessage outgoingMessage, PBNInboundEDIMessage incomingMessage) CreateSetupData(string incomingMessageText = null)
	{
		var (manifestHeader, outgoingMessage, incomingMessage) = PBNMessageTestHelper.SetupMessages(Factory, MessageText, MessageType, true);

		var customsDeclaration1 = manifestHeader.CustomsReferenceCollection.AddNew();
		customsDeclaration1.CSI_ReferenceNumber = "CUSDEC1";
		var transitDeclaration1 = manifestHeader.TransitDeclarationCollection.AddNew();
		transitDeclaration1.CSI_ReferenceNumber = "TRANSITDEC1";

		var customsDeclarationToDelete = manifestHeader.CustomsReferenceCollection.AddNew();
		customsDeclarationToDelete.CSI_ReferenceNumber = "CUSDEC2DEL";
		customsDeclarationToDelete.CSI_Status = PBNDeclarationReferenceStatusList.Codes.TBD;
		var transitDeclarationToDelete = manifestHeader.TransitDeclarationCollection.AddNew();
		transitDeclarationToDelete.CSI_ReferenceNumber = "TRANSITDEC2DEL";
		transitDeclarationToDelete.CSI_Status = PBNDeclarationReferenceStatusList.Codes.TBD;

		return (manifestHeader, manifestHeader, outgoingMessage, incomingMessage);
	}

	sealed class CPBMessageProcessorForTest : CreateAndUpdatePBNMessageProcessor
	{
		public CPBMessageProcessorForTest(LoggingInformation logger, Type messageDefinitionType) : base(logger, messageDefinitionType) { }
		public new Type MessageInterpreterType => base.MessageInterpreterType;
	}
}
