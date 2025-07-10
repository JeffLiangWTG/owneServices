using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCCatalogInboundMessageCreatorTest : BRCInboundMessageCreatorAbstractTest
	{
		protected override ZString InterchangeType => MessageTypeList.Codes.CAT;

		public void TestGenerateMessageFromInterchange()
		{
			AssertCreateMessageFromInterchange($"[ {ResponseSingleMessage} ]", ResponseSingleMessage);
		}

		public void TestGenerateMessageFromInterchange_RES()
		{
			AssertCreateMessageFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(messageType: MessageConstants.MessageType.RES, responseMessage: $"[ {ResponseSingleMessage} ]"), ResponseSingleMessage, expectedMessageSubType: EDIMessageSubTypeList.Codes.Success);
		}

		public void TestGenerateMessageFromInterchange_BER()
		{
			var responseMessage = "{\"message\": \"O usuário logado não é representante legal do CPF/CNPJ Raiz.\"}";
			var messageText = UniversalEventTestDataHelper.CreateUniversalInterchangeXml(messageType: MessageConstants.MessageType.BER, responseMessage: responseMessage);
			var outgoingInterchange = ProcessEDIInterchange(messageText);
			outgoingInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			BRCResponseMessageProcessorTest.CreateMessage(Factory, outgoingInterchange.PK, null, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Original);
			Factory.Save();

			AssertCreateMessageFromInterchange(messageText, responseMessage, expectedMessageSubType: EDIMessageSubTypeList.Codes.Error, outgoingInterchangeSessionGuid: outgoingInterchange.EI_SessionGUID);
		}

		public void TestGenerateMultipleMessagesFromInterchange_RES()
		{
			var responseMessage1 = ResponseSingleMessage;
			var responseMessage2 = ResponseSingleMessage.Replace("\"seq\": 1", "\"seq\": 2");
			var responseMessage3 = ResponseSingleMessage.Replace("\"seq\": 1", "\"seq\": 3");

			var interchange = ProcessEDIInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(messageType: MessageConstants.MessageType.RES, responseMessage: $"[ {responseMessage1}, {responseMessage2}, {responseMessage3} ]"));
			CombineAssertions(() =>
			{
				AssertEquals("Interchange status should be set to Received", EDIInterchangeStatusList.Codes.Received, interchange.EI_Status);
				AssertEquals("Should have been 3 message extracted from interchange", 3, interchange.ContainedMessages.Count);
			});

			AssertEDIMessageCreated(interchange.ContainedMessages[0], responseMessage1, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Success);
			AssertEDIMessageCreated(interchange.ContainedMessages[1], responseMessage2, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Success);
			AssertEDIMessageCreated(interchange.ContainedMessages[2], responseMessage3, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Success);
		}

		public void TestGenerateMessageFromInterchange_CAT_MZI()
		{
			AssertGenerateMessageFromInterchange(EDIMessageSubTypeList.Codes.ManufacturerZipFile, ResponseMZISingleMessage);
		}

		public void TestGenerateMessageFromInterchange_CAT_CZP()
		{
			AssertGenerateMessageFromInterchange(EDIMessageSubTypeList.Codes.CatalogZipFile, ResponseCZPSingleMessage);
		}

		public void TestGenerateMessageFromInterchange_CAT_OZI()
		{
			AssertGenerateMessageFromInterchange(EDIMessageSubTypeList.Codes.OperatorZipFile, ResponseOZISingleMessage);
		}

		public void TestGenerateMessageFromInterchange_CAT_LIN()
		{
			AssertGenerateMessageFromInterchange(EDIMessageSubTypeList.Codes.Link, ResponseSingleMessage);
		}

		void AssertGenerateMessageFromInterchange(string messageSubType, string singleResponseMessage)
		{
			var responseMessage1 = singleResponseMessage;
			var responseMessage2 = singleResponseMessage.Replace("\"seq\": 1", "\"seq\": 2");
			var responseMessage3 = singleResponseMessage.Replace("\"seq\": 1", "\"seq\": 3");

			AssertGenerateMessageFromInterchange(messageSubType, $"[ {responseMessage1}, {responseMessage2}, {responseMessage3} ]");
			AssertGenerateMessageFromInterchange(messageSubType, EncodeAndZipMessages($"[{responseMessage1}, {responseMessage2}]", $"[{responseMessage3}]"));

			void AssertGenerateMessageFromInterchange(string messageSubType, string responseMessage)
			{
				var messageText = UniversalEventTestDataHelper.CreateUniversalInterchangeXml(messageType: MessageConstants.MessageType.RES, responseMessage: responseMessage);
				var outgoingInterchange = ProcessEDIInterchange(messageText);
				outgoingInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
				BRCResponseMessageProcessorTest.CreateMessage(Factory, outgoingInterchange.PK, null, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CAT, messageSubType);
				Factory.Save();

				var interchange = ProcessEDIInterchange(messageText, outgoingInterchange.EI_SessionGUID);
				CombineAssertions(() =>
				{
					AssertEquals("Interchange status should be set to Received", EDIInterchangeStatusList.Codes.Received, interchange.EI_Status);
					AssertEquals("Should have been 3 message extracted from interchange", 3, interchange.ContainedMessages.Count);
				});

				AssertEDIMessageCreated(interchange.ContainedMessages[0], responseMessage1, MessageTypeList.Codes.CAT, messageSubType);
				AssertEDIMessageCreated(interchange.ContainedMessages[1], responseMessage2, MessageTypeList.Codes.CAT, messageSubType);
				AssertEDIMessageCreated(interchange.ContainedMessages[2], responseMessage3, MessageTypeList.Codes.CAT, messageSubType);
			}
		}

		const string ResponseSingleMessage = @"{
  ""seq"": 1,
  ""codigo"": ""1"",
  ""erros"": [
    """"
  ],
  ""sucesso"": true,
  ""versao"": ""4""
}";

		const string ResponseCZPSingleMessage = @"{
  ""seq"": 1,
  ""codigo"": 1,
  ""denominacao"": ""Teste (Reativação)"",
  ""cpfCnpjRaiz"": ""00638881"",
  ""situacao"": ""Ativado"",
  ""modalidade"": ""IMPORTACAO"",
  ""ncm"": ""01012100"",
  ""versao"": ""2"",
  ""atributos"": [
    {
      ""atributo"": ""ATT_5898"",
      ""valor"": ""1""
    },
    {
      ""atributo"": ""ATT_5946"",
      ""valor"": ""5""
    },
    {
      ""atributo"": ""ATT_5396"",
      ""valor"": ""Teste""
    },
    {
      ""atributo"": ""ATT_2263"",
      ""valor"": ""01""
    }
  ],
  ""atributosMultivalorados"": [],
  ""atributosCompostos"": [],
  ""atributosCompostosMultivalorados"": [],
  ""codigosInterno"": []
}";

		const string ResponseOZISingleMessage = @"{
  ""seq"": 1,
  ""cpfCnpjRaiz"": ""00638881"",
  ""codigo"": ""1"",
  ""versao"": ""2"",
  ""tin"": ""21333333333333333333333333333333333"",
  ""nome"": ""NAMENAMENAMENAMENAMENAMENAMENAMENAMENAMENAMENAMENAMENAMENAMENAMENAMENA"",
  ""situacao"": ""Ativado"",
  ""logradouro"": ""ADDRESSADDRESSADDRESSADDRESSADDRESSADDRESSADDRESSADDRESSADDRESSADDRESS"",
  ""nomeCidade"": ""CITYADDRESSCITYADDRESSCITYADDRESSCI"",
  ""codigoSubdivisaoPais"": ""CN-MO"",
  ""codigoPais"": ""CN"",
  ""cep"": ""789999999"",
  ""codigoInterno"": ""45678989898989898989898989898989899"",
  ""email"": ""emailemailemailemailemailemailemailemailemailemailemailem@email.com.br"",
  ""dataReferencia"": null,
  ""identificacoesAdicionais"": [
    {
      ""numero"": ""32111111111111111111111111111111111"",
      ""codigo"": ""10""
    },
    {
      ""numero"": ""32222222222222222222222222222222222"",
      ""codigo"": ""102""
    }
  ]
}";

		const string ResponseMZISingleMessage = @"{
  ""seq"": 1,
  ""cpfCnpjRaiz"": ""75400331"",
  ""codigoOperadorEstrangeiro"": ""123"",
  ""cpfCnpjFabricante"": ""75400331000115"",
  ""conhecido"": true,
  ""codigoProduto"": 123,
  ""vincular"": true,
  ""dataReferencia"": ""2020-07-20"",
  ""codigoPais"": ""PE""
}";
	}
}
