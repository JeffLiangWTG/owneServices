using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCRequestTTCEReferenceFileInboundMessageCreatorTest : BRCInboundMessageCreatorAbstractTest
	{
		protected override ZString InterchangeType => MessageTypeList.Codes.RTT;

		public void TestGenerateMessageFromInterchange()
		{
			var responseMessageOTA = BRMessageTestHelper.GetEmbeddedResource(BRCOptionalTreatmentAttributesResponseMessageProcessorTest.ResponseMessageOTA);
			AssertCreateMessageFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(messageType: MessageTypeList.Codes.RTT, responseMessage: responseMessageOTA), responseMessageOTA);
		}

		public void TestGenerateMessageFromInterchange_RTT_OTA()
		{
			var responseMessageOTA = BRMessageTestHelper.GetEmbeddedResource(BRCOptionalTreatmentAttributesResponseMessageProcessorTest.ResponseMessageOTA);
			AssertGenerateMessageFromInterchange(EDIMessageSubTypeList.Codes.OptionalTreatmentAttributes, responseMessageOTA);
		}

		void AssertGenerateMessageFromInterchange(string messageSubType, string responseMessage)
		{
			var messageText = UniversalEventTestDataHelper.CreateUniversalInterchangeXml(messageType: MessageConstants.MessageType.RES, responseMessage: responseMessage);
			var outgoingInterchange = ProcessEDIInterchange(messageText);
			outgoingInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			BRCResponseMessageProcessorTest.CreateMessage(Factory, outgoingInterchange.PK, null, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.RTT, messageSubType);
			Factory.Save();

			var interchange = ProcessEDIInterchange(messageText, outgoingInterchange.EI_SessionGUID);
			CombineAssertions(() =>
			{
				AssertEquals("Interchange status should be set to Received", EDIInterchangeStatusList.Codes.Received, interchange.EI_Status);
				AssertEquals("Should have been 1 message extracted from interchange", 1, interchange.ContainedMessages.Count);
			});

			AssertEDIMessageCreated(interchange.ContainedMessages[0], responseMessage, MessageTypeList.Codes.RTT, messageSubType);
		}
	}
}
