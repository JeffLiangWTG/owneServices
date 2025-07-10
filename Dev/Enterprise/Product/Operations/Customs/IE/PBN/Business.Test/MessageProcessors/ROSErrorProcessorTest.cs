using System.Text.Json;
using CargoWise.Customs.IE.MessageDefinitions.PBN;
using CargoWise.Types;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.PBN.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.Business.Testing;

[TestedType(typeof(ROSErrorProcessor))]
sealed class ROSErrorProcessorTest : MessageAttacheeMessageProcessorTest<ROSErrorProcessor, PBNInboundEDIMessage, PBNOutboundEDIMessage, ROSErrorProvider, AsycudaManifestHeader, AsycudaManifestHeader>
{
	public override void TestEnsureMessageTextIsValid()
	{
		CombineAssertions(() =>
		{
			AssertNotNullOrEmpty("Message text is not null or empty", MessageText);
			var dataObject = JsonSerializer.Deserialize(MessageText, typeof(ROSErrorDefinition));
			AssertNotNull("Deserialized object is not null", dataObject);
		});
	}

	public void TestUniversalInterchangeXMLEmbeddedMessageText()
	{
		var xmlMessageText = PBNMessageTestHelper.universalInterchangeEventText;
		var incomingMessage = PBNMessageTestHelper.SetupMessages(Factory, xmlMessageText, MessageType, true).incomingMessage;
		CombineAssertions(() =>
		{
			ProcessMessage(incomingMessage);
			AssertEquals("Message Status", "PRS", incomingMessage.EM_Status);
		});
	}

	protected override ZString MessageType => PBNMessageTypes.Codes.CreatePBN;

	protected override ZString MessageText => JsonSerializer.Serialize(ROSErrorInterpreterTest.NewDataObjectToTest());

	protected override ZString MessageFriendlyName => "ROS Error Processor";

	protected override ROSErrorProcessor Processor => new ROSErrorProcessor(logger, typeof(ROSErrorDefinition));

	protected override (AsycudaManifestHeader declaration, AsycudaManifestHeader messageAttachee, EDIMessage outgoingMessage, PBNInboundEDIMessage incomingMessage) CreateSetupData(string incomingMessageText = null)
	{
		var testData = PBNMessageTestHelper.SetupMessages(Factory, MessageText, MessageType, true);
		return (testData.relatedJobAndAttachee, testData.relatedJobAndAttachee, testData.outgoingMessage, testData.incomingMessage);
	}
}
