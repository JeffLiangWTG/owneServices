using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCForeignOperatorInboundMessageCreatorTest : BRCInboundMessageCreatorAbstractTest
	{
		protected override ZString InterchangeType => MessageTypeList.Codes.OPE;

		public void TestGenerateMessageFromInterchange()
		{
			AssertCreateMessageFromInterchange(CreateUniversalInterchangeXml(responseMessage: $"[ {ResponseMessage} ]"), ResponseMessage);
		}

		public void TestGenerateMessageFromInterchange_BER()
		{
			var responseMessage = "{\"message\": \"O usuário logado não é representante legal do CPF/CNPJ Raiz.\"}";
			var messageText = UniversalEventTestDataHelper.CreateUniversalInterchangeXml(messageType: MessageConstants.MessageType.BER, responseMessage: responseMessage);
			var outgoingInterchange = ProcessEDIInterchange(messageText);
			outgoingInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			BRCResponseMessageProcessorTest.CreateMessage(Factory, outgoingInterchange.PK, null, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.OPE, EDIMessageSubTypeList.Codes.Original);
			Factory.Save();

			AssertCreateMessageFromInterchange(messageText, responseMessage, expectedMessageSubType: EDIMessageSubTypeList.Codes.Error, outgoingInterchangeSessionGuid: outgoingInterchange.EI_SessionGUID);
		}

		public void TestGenerateMessageFromInterchange_RES()
		{
			AssertCreateMessageFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(messageType: MessageConstants.MessageType.RES, responseMessage: $"[ {ResponseMessage} ]"), ResponseMessage, expectedMessageSubType: EDIMessageSubTypeList.Codes.Success);
		}

		public void TestGenerateMultipleMessagesFromInterchange_RES()
		{
			var responseMessage1 = ResponseMessage;
			var responseMessage2 = ResponseMessage.Replace("\"seq\": 1", "\"seq\": 2");
			var responseMessage3 = ResponseMessage.Replace("\"seq\": 1", "\"seq\": 3");

			AssertGenerateMultipleMessagesFromInterchange($"[ {responseMessage1}, {responseMessage2}, {responseMessage3} ]");
			AssertGenerateMultipleMessagesFromInterchange(EncodeAndZipMessages($"[{responseMessage1}, {responseMessage2}]", $"[{responseMessage3}]"));

			void AssertGenerateMultipleMessagesFromInterchange(string responseMessage)
			{
				var interchange = ProcessEDIInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(messageType: MessageConstants.MessageType.RES, responseMessage: responseMessage));
				CombineAssertions(() =>
				{
					AssertEquals("Interchange status should be set to Received", EDIInterchangeStatusList.Codes.Received, interchange.EI_Status);
					AssertEquals("Should have been 3 message extracted from interchange", 3, interchange.ContainedMessages.Count);
				});

				AssertEDIMessageCreated(interchange.ContainedMessages[0], responseMessage1, MessageTypeList.Codes.OPE, EDIMessageSubTypeList.Codes.Success);
				AssertEDIMessageCreated(interchange.ContainedMessages[1], responseMessage2, MessageTypeList.Codes.OPE, EDIMessageSubTypeList.Codes.Success);
				AssertEDIMessageCreated(interchange.ContainedMessages[2], responseMessage3, MessageTypeList.Codes.OPE, EDIMessageSubTypeList.Codes.Success);
			}
		}

		const string ResponseMessage = @"{
  ""seq"": 1,
  ""codigo"": ""1"",
  ""erros"": [
    """"
  ],
  ""sucesso"": true,
  ""versao"": ""4""
}";
	}
}
