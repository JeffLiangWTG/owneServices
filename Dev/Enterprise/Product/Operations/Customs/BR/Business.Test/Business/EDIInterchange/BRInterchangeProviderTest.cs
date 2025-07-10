using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class BRInterchangeProviderTest : InterchangeProviderTestCase
	{
		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection)
		{
			return new BRInterchangeProvider(collection);
		}

		public override void TestMessagesPopulateNewInterchange()
		{
			var message1 = CreateTransmitMessage(Factory, MessageTypeList.Codes.CDE, ExportEntryActionCodeList.Codes.RET, "21BR0000022649");
			var message2 = CreateTransmitMessage(Factory, MessageTypeList.Codes.CDE, ExportEntryActionCodeList.Codes.RET, "21BR0000022650");
			var messages = new NonDependentEDIMessageCollection(Factory);
			messages.AddRange(new BREDIMessage[] { message1, message2 });
			var provider = new BRInterchangeProvider(messages);
			provider.PackCollatedMessagesIntoInterchanges();

			AssertEquals("Number Of Interchanges", 2, provider.Interchanges.Length);
			AssertNotEquals("Confirmed different interchanges for each message no collation", message1.EM_EI, message2.EM_EI);

			AssertEDIMessage(message1, MessageTypeList.Codes.CDE, @"{""custom.MessageSubType"":""RET"",""custom.ReferenceNumber"":""21BR0000022649""}");
			AssertEDIMessage(message2, MessageTypeList.Codes.CDE, @"{""custom.MessageSubType"":""RET"",""custom.ReferenceNumber"":""21BR0000022650""}");
		}

		public void TestMessagesPopulateNewInterchange_LPC()
		{
			var message1 = CreateTransmitMessage(Factory, MessageTypeList.Codes.LPC, LPCOEntryActionCodeList.Codes.REQ, "21BR0000022649|444");
			var message2 = CreateTransmitMessage(Factory, MessageTypeList.Codes.LPC, LPCOEntryActionCodeList.Codes.REQ, "21BR0000022649");
			var messages = new NonDependentEDIMessageCollection(Factory);
			messages.AddRange(new BREDIMessage[] { message1, message2 });
			var provider = new BRInterchangeProvider(messages);
			provider.PackCollatedMessagesIntoInterchanges();

			AssertEquals("Number Of Interchanges", 2, provider.Interchanges.Length);
			AssertNotEquals("Confirmed different interchanges for each message no collation", message1.EM_EI, message2.EM_EI);
			AssertEDIMessage(message1, MessageTypeList.Codes.LPC, @"{""custom.MessageSubType"":""REQ"",""custom.ReferenceNumber"":""21BR0000022649"",""custom.MessageRequirement"":""444""}");
			AssertEDIMessage(message2, MessageTypeList.Codes.LPC, @"{""custom.MessageSubType"":""REQ"",""custom.ReferenceNumber"":""21BR0000022649"",""custom.MessageRequirement"":""""}");
		}

		public void TestMessagesPopulateNewInterchange_SUB()
		{
			var message1 = CreateTransmitMessage(Factory, MessageTypeList.Codes.SUB, SubscriptionMessageTypesList.Codes.CAN, "eventID01");
			var message2 = CreateTransmitMessage(Factory, MessageTypeList.Codes.SUB, SubscriptionMessageTypesList.Codes.CAN, "eventID02");
			var message3 = CreateTransmitMessage(Factory, MessageTypeList.Codes.SUB, SubscriptionMessageTypesList.Codes.ORI, "eventID03");
			var messages = new NonDependentEDIMessageCollection(Factory);
			messages.AddRange(new BREDIMessage[] { message1, message2, message3 });
			var provider = new BRInterchangeProvider(messages);
			provider.PackCollatedMessagesIntoInterchanges();

			AssertEquals("Number Of Interchanges", 3, provider.Interchanges.Length);
			AssertNotEquals("Confirmed different interchanges for each message no collation", message1.EM_EI, message2.EM_EI);
			AssertEDIMessage(message1, MessageTypeList.Codes.SUB, @"{""custom.MessageSubType"":""CAN"",""custom.SubscriptionId"":""eventID01""}");
			AssertEDIMessage(message2, MessageTypeList.Codes.SUB, @"{""custom.MessageSubType"":""CAN"",""custom.SubscriptionId"":""eventID02""}");
			AssertEDIMessage(message3, MessageTypeList.Codes.SUB, @"{""custom.MessageSubType"":""ORI""}");
		}

		public void TestMessagesPopulateNewInterchange_CDD()
		{
			AssertInterchangeHeaderText(ImportEntryActionCodeList.Codes.RET);
			AssertInterchangeHeaderText(ImportEntryActionCodeList.Codes.CVH);
			AssertInterchangeHeaderText(ImportEntryActionCodeList.Codes.DIA);

			void AssertInterchangeHeaderText(string subType)
			{
				var message1 = CreateTransmitMessage(Factory, MessageTypeList.Codes.CDD, subType, "21BR0000022649|1");
				var message2 = CreateTransmitMessage(Factory, MessageTypeList.Codes.CDD, subType, "21BR0000022650");
				var messages = new NonDependentEDIMessageCollection(Factory);
				messages.AddRange(new BREDIMessage[] { message1, message2 });
				var provider = new BRInterchangeProvider(messages);
				provider.PackCollatedMessagesIntoInterchanges();

				AssertEquals("Number Of Interchanges", 2, provider.Interchanges.Length);
				AssertEDIMessage(message1, MessageTypeList.Codes.CDD, $@"{{""custom.MessageSubType"":""{subType}"",""custom.ReferenceNumber"":""21BR0000022649"",""custom.BR.VersionNumber"":""1""}}");
				AssertEDIMessage(message2, MessageTypeList.Codes.CDD, $@"{{""custom.MessageSubType"":""{subType}"",""custom.ReferenceNumber"":""21BR0000022650"",""custom.BR.VersionNumber"":""""}}");
			}
		}

		public void TestMessagesPopulateNewInterchange_CAT_ORI()
		{
			var productJson = GetProductJsonMessage("25043511");

			var message = CreateTransmitMessage(Factory, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Original, "25043511", productJson);
			var messages = new NonDependentEDIMessageCollection(Factory);
			messages.AddRange(new BREDIMessage[] { message });

			var provider = new BRInterchangeProvider(messages);
			var interchanges = provider.Interchanges;
			AssertEquals("Number Of Interchanges", 1, interchanges.Length);

			var interchange = interchanges[0];
			CombineAssertions(() =>
			{
				AssertEquals("To", BREDIInterchange.BRCustoms, interchange.EI_To);
				AssertEquals("From", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
				AssertEquals("Interchange Type ", MessageTypeList.Codes.CAT, interchange.EI_InterchangeType);
				AssertEquals("Application Code ", BREDIInterchange.ApplicationCodes.BRCustoms, interchange.EI_ApplicationCode);
				AssertEquals("Header Text", "{\"custom.MessageSubType\":\"ORI\"}", interchange.EI_HeaderText);
				AssertEquals("Body Text", $"[{productJson}]", interchange.EI_BodyText.ToString());
			});
		}

		public void TestMessagesPopulateNewInterchange_OPE()
		{
			var foreignOperatorJson = GetForeignOperatorJsonMessage("25043511");

			var message = CreateTransmitMessage(Factory, MessageTypeList.Codes.OPE, EDIMessageSubTypeList.Codes.Original, "25043511", foreignOperatorJson);
			var messages = new NonDependentEDIMessageCollection(Factory);
			messages.AddRange(new BREDIMessage[] { message });

			var provider = new BRInterchangeProvider(messages);
			var interchanges = provider.Interchanges;
			AssertEquals("Number Of Interchanges", 1, interchanges.Length);

			var interchange = interchanges[0];
			CombineAssertions(() =>
			{
				AssertEquals("To", BREDIInterchange.BRCustoms, interchange.EI_To);
				AssertEquals("From", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
				AssertEquals("Interchange Type ", MessageTypeList.Codes.OPE, interchange.EI_InterchangeType);
				AssertEquals("Application Code ", BREDIInterchange.ApplicationCodes.BRCustoms, interchange.EI_ApplicationCode);
				AssertEquals("Header Text", "{\"custom.MessageSubType\":\"ORI\",\"custom.ReferenceNumber\":\"25043511\"}", interchange.EI_HeaderText);
				AssertEquals("Body Text", $"[{foreignOperatorJson}]", interchange.EI_BodyText.ToString());
			});
		}

		public void TestMessagesPopulateNewInterchange_CIH()
		{
			var message1 = CreateTransmitMessage(Factory, MessageTypeList.Codes.CIH, EDIMessageSubTypeList.Codes.Original, "24BR00000052101");
			var message2 = CreateTransmitMessage(Factory, MessageTypeList.Codes.CIH, EDIMessageSubTypeList.Codes.Update, "24BR00000052102|2");
			var message3 = CreateTransmitMessage(Factory, MessageTypeList.Codes.CIH, EDIMessageSubTypeList.Codes.CompleteConsult, "24BR00000052103|3");

			var message4 = CreateTransmitMessage(Factory, MessageTypeList.Codes.CIH, EDIMessageSubTypeList.Codes.Rectification, "24BR00000052104|4");
			var message5 = CreateTransmitMessage(Factory, MessageTypeList.Codes.CIH, EDIMessageSubTypeList.Codes.Deletion, "24BR00000052105|5");
			var message6 = CreateTransmitMessage(Factory, MessageTypeList.Codes.CIH, ImportEntryActionCodeList.Codes.DIA, "24BR00000052106|6");
			var message7 = CreateTransmitMessage(Factory, MessageTypeList.Codes.CIH, ImportEntryActionCodeList.Codes.REG, "24BR00000052107|7");

			var messages = new NonDependentEDIMessageCollection(Factory);
			messages.AddRange(new BREDIMessage[] { message1, message2, message3, message4, message5, message6, message7 });
			var provider = new BRInterchangeProvider(messages);
			provider.PackCollatedMessagesIntoInterchanges();

			AssertEquals("Number Of Interchanges", 7, provider.Interchanges.Length);
			AssertNotEquals("Confirmed different interchanges for each message no collation", message1.EM_EI, message2.EM_EI);
			AssertEDIMessage(message1, MessageTypeList.Codes.CIH, @"{""custom.MessageSubType"":""ORI"",""custom.ReferenceNumber"":""24BR00000052101""}");
			AssertEDIMessage(message2, MessageTypeList.Codes.CIH, @"{""custom.MessageSubType"":""UPD"",""custom.ReferenceNumber"":""24BR00000052102"",""custom.BR.VersionNumber"":""2""}");
			AssertEDIMessage(message3, MessageTypeList.Codes.CIH, @"{""custom.MessageSubType"":""COM"",""custom.ReferenceNumber"":""24BR00000052103"",""custom.BR.VersionNumber"":""3""}");

			AssertEDIMessage(message4, MessageTypeList.Codes.CIH, @"{""custom.MessageSubType"":""RET"",""custom.ReferenceNumber"":""24BR00000052104"",""custom.BR.VersionNumber"":""4""}");
			AssertEDIMessage(message5, MessageTypeList.Codes.CIH, @"{""custom.MessageSubType"":""DEL"",""custom.ReferenceNumber"":""24BR00000052105"",""custom.BR.VersionNumber"":""5""}");
			AssertEDIMessage(message6, MessageTypeList.Codes.CIH, @"{""custom.MessageSubType"":""DIA"",""custom.ReferenceNumber"":""24BR00000052106"",""custom.BR.VersionNumber"":""6""}");
			AssertEDIMessage(message7, MessageTypeList.Codes.CIH, @"{""custom.MessageSubType"":""REG"",""custom.ReferenceNumber"":""24BR00000052107"",""custom.BR.VersionNumber"":""7""}");
		}

		public void TestMessagesPopulateNewInterchange_CIL()
		{
			var message1 = CreateTransmitMessage(Factory, MessageTypeList.Codes.CIL, EDIMessageSubTypeList.Codes.Addition, "24BR00000052101|1");
			var message2 = CreateTransmitMessage(Factory, MessageTypeList.Codes.CIL, EDIMessageSubTypeList.Codes.Update, "24BR00000052102|2");
			var message3 = CreateTransmitMessage(Factory, MessageTypeList.Codes.CIL, EDIMessageSubTypeList.Codes.Deletion, "24BR00000052103|3|6");

			var message4 = CreateTransmitMessage(Factory, MessageTypeList.Codes.CIL, ImportEntryActionCodeList.Codes.CVH, "24BR00000052104|4");

			var messages = new NonDependentEDIMessageCollection(Factory);
			messages.AddRange(new BREDIMessage[] { message1, message2, message3, message4  });
			var provider = new BRInterchangeProvider(messages);
			provider.PackCollatedMessagesIntoInterchanges();

			AssertEquals("Number Of Interchanges", 4, provider.Interchanges.Length);
			AssertNotEquals("Confirmed different interchanges for each message no collation", message1.EM_EI, message2.EM_EI);
			AssertEDIMessage(message1, MessageTypeList.Codes.CIL, @"{""custom.MessageSubType"":""ADD"",""custom.ReferenceNumber"":""24BR00000052101"",""custom.BR.VersionNumber"":""1""}");
			AssertEDIMessage(message2, MessageTypeList.Codes.CIL, @"{""custom.MessageSubType"":""UPD"",""custom.ReferenceNumber"":""24BR00000052102"",""custom.BR.VersionNumber"":""2""}");
			AssertEDIMessage(message3, MessageTypeList.Codes.CIL, @"{""custom.MessageSubType"":""DEL"",""custom.ReferenceNumber"":""24BR00000052103"",""custom.BR.VersionNumber"":""3"",""custom.ItemNumber"":""6""}");

			AssertEDIMessage(message4, MessageTypeList.Codes.CIL, @"{""custom.MessageSubType"":""CVH"",""custom.ReferenceNumber"":""24BR00000052104"",""custom.BR.VersionNumber"":""4""}");
		}

		public void TestMessagesPopulateNewInterchange_CAT_LIN()
		{
			var productLinkJson = GetProductLinkJsonMessage("25043511");

			var message1 = CreateTransmitMessage(Factory, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Link, messageBody: $"[{productLinkJson}, {productLinkJson}]");
			var message2 = CreateTransmitMessage(Factory, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Link, messageBody: $"[{productLinkJson}, {productLinkJson}]");
			var messages = new NonDependentEDIMessageCollection(Factory);
			messages.AddRange(new BREDIMessage[] { message1, message2 });

			var provider = new BRInterchangeProvider(messages);
			var interchanges = provider.Interchanges;
			AssertEquals("Number Of Interchanges", 2, interchanges.Length);

			foreach (var interchange in interchanges)
			{
				CombineAssertions(() =>
				{
					AssertEquals("To", BREDIInterchange.BRCustoms, interchange.EI_To);
					AssertEquals("From", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
					AssertEquals("Interchange Type ", MessageTypeList.Codes.CAT, interchange.EI_InterchangeType);
					AssertEquals("Application Code ", BREDIInterchange.ApplicationCodes.BRCustoms, interchange.EI_ApplicationCode);
					AssertEquals("Header Text", "{\"custom.MessageSubType\":\"LIN\"}", interchange.EI_HeaderText);
					AssertContains("Body Text", $"{productLinkJson},{productLinkJson.Replace("\"seq\": 1,", "\"seq\": 2,")}", interchange.EI_BodyText.ToString());
				});
			}
		}

		public void TestMessagesPopulateNewInterchange_CAT()
		{
			var message1 = CreateTransmitMessage(Factory, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.CatalogZipFile, "75400331|true");
			var message2 = CreateTransmitMessage(Factory, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.CatalogZipFile, "75400331|false");
			var message3 = CreateTransmitMessage(Factory, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.ManufacturerZipFile, "75400331");
			var message4 = CreateTransmitMessage(Factory, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.OperatorZipFile, "75400331|true");
			var message5 = CreateTransmitMessage(Factory, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.OperatorZipFile, "75400331|false");
			var messages = new NonDependentEDIMessageCollection(Factory);
			messages.AddRange(new BREDIMessage[] { message1, message2, message3, message4, message5 });
			var provider = new BRInterchangeProvider(messages);
			provider.PackCollatedMessagesIntoInterchanges();

			AssertEquals("Number Of Interchanges", 5, provider.Interchanges.Length);
			AssertNotEquals("Confirmed different interchanges for each message no collation", message1.EM_EI, message2.EM_EI);
			AssertEDIMessage(message1, MessageTypeList.Codes.CAT, @"{""custom.MessageSubType"":""CZP"",""custom.BR.CNPJ"":""75400331"",""custom.BR.DisplayDisabled"":""true""}");
			AssertEDIMessage(message2, MessageTypeList.Codes.CAT, @"{""custom.MessageSubType"":""CZP"",""custom.BR.CNPJ"":""75400331"",""custom.BR.DisplayDisabled"":""false""}");
			AssertEDIMessage(message3, MessageTypeList.Codes.CAT, @"{""custom.MessageSubType"":""MZI"",""custom.BR.CNPJ"":""75400331""}");
			AssertEDIMessage(message4, MessageTypeList.Codes.CAT, @"{""custom.MessageSubType"":""OZI"",""custom.BR.CNPJ"":""75400331"",""custom.BR.DisplayDisabled"":""true""}");
			AssertEDIMessage(message5, MessageTypeList.Codes.CAT, @"{""custom.MessageSubType"":""OZI"",""custom.BR.CNPJ"":""75400331"",""custom.BR.DisplayDisabled"":""false""}");
		}

		public void TestMessagesPopulateNewInterchange_RTT()
		{
			var jsonMessage = @"{
""ncm"": ""84149039"",
""codigoPais"": 158,
""dataFatoGerador"": ""2023-04-17"",
""tipoOperacao"": ""I""
}";
			var message1 = CreateTransmitMessage(Factory, MessageTypeList.Codes.RTT, EDIMessageSubTypeList.Codes.MandatoryTributaryTreatment, string.Empty, messageBody: jsonMessage);
			var message2 = CreateTransmitMessage(Factory, MessageTypeList.Codes.RTT, EDIMessageSubTypeList.Codes.MandatoryTributaryTreatment, string.Empty, messageBody: jsonMessage);
			var message3 = CreateTransmitMessage(Factory, MessageTypeList.Codes.RTT, EDIMessageSubTypeList.Codes.MandatoryTributaryTreatment, string.Empty, messageBody: jsonMessage);
			var message4 = CreateTransmitMessage(Factory, MessageTypeList.Codes.RTT, EDIMessageSubTypeList.Codes.MandatoryTributaryTreatment, string.Empty, messageBody: jsonMessage);
			var messages = new NonDependentEDIMessageCollection(Factory);
			messages.AddRange(new BREDIMessage[] { message1, message2, message3, message4 });
			var provider = new BRInterchangeProvider(messages);
			provider.PackCollatedMessagesIntoInterchanges();

			AssertEquals("Number Of Interchanges", 4, provider.Interchanges.Length);
			AssertNotEquals("Confirmed different interchanges for each message no collation", message1.EM_EI, message2.EM_EI);
			AssertEDIMessage(message1, MessageTypeList.Codes.RTT, @"{""custom.MessageSubType"":""MTT"",""custom.ReferenceNumber"":""""}", bodyText: jsonMessage);
			AssertEDIMessage(message2, MessageTypeList.Codes.RTT, @"{""custom.MessageSubType"":""MTT"",""custom.ReferenceNumber"":""""}", bodyText: jsonMessage);
			AssertEDIMessage(message3, MessageTypeList.Codes.RTT, @"{""custom.MessageSubType"":""MTT"",""custom.ReferenceNumber"":""""}", bodyText: jsonMessage);
			AssertEDIMessage(message4, MessageTypeList.Codes.RTT, @"{""custom.MessageSubType"":""MTT"",""custom.ReferenceNumber"":""""}", bodyText: jsonMessage);
		}

		public void AssertEDIMessage(EDIMessage message, string interchangeType, string expectedHeaderText, string bodyText = "Message Text")
		{
			CombineAssertions(() =>
			{
				var interchange = message.Interchange;
				AssertEquals("Message is Sent", EDIMessage.Status.Sent, message.EM_Status);
				AssertEquals("To", BREDIInterchange.BRCustoms, interchange.EI_To);
				AssertEquals("From", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
				AssertEquals("Interchange Status", EDIInterchange.Status.Queued, interchange.EI_Status);
				AssertEquals("Interchange Type ", interchangeType, interchange.EI_InterchangeType);
				AssertEquals("Application Code ", BREDIInterchange.ApplicationCodes.BRCustoms, interchange.EI_ApplicationCode);
				AssertEquals("Header Text", expectedHeaderText, interchange.EI_HeaderText);
				AssertEquals("Body Text", bodyText, interchange.EI_BodyText);
				AssertEquals("Foot Text", ZString.Empty, interchange.EI_FooterText);
				AssertEquals("Transport Type", EDIInterchange.TransportType.xT, interchange.EI_TransportType);
			});
		}

		public static BREDIMessage CreateTransmitMessage(BusinessObjectFactory factory, string messageType, string messageSubType, string applicationReference = "", string messageBody = "Message Text", ZDateTime? date = null, string user = "E")
		{
			var message = factory.New<BREDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.BRCustoms;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_MessageText = messageBody;
			message.EM_MessageType = messageType;
			message.EM_MessageSubType = messageSubType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ApplicationReference = applicationReference;
			if (date != null)
			{
				message.EM_SystemCreateTimeUtc = date.GetValueOrDefault();
				message.EM_SystemCreateUser = user;
			}
			return message;
		}

		public static string GetProductJsonMessage(string rootCnpj) => @$"
  {{
    ""seq"": 1,
    ""codigo"": 1,
    ""descricao"": ""Produto Teste"",
    ""denominacao"": ""Denominacao"",
    ""cpfCnpjRaiz"": ""{rootCnpj}"",
    ""situacao"": ""ATIVADO"",
    ""modalidade"": ""IMPORTACAO"",
    ""ncm"": ""02011000"",
    ""versao"": ""1"",
    ""atributos"": [
      {{
        ""atributo"": ""ATT_1"",
        ""valor"": ""01""
      }}
    ],
    ""codigosInterno"": [
      ""string""
    ],
    ""dataReferencia"": ""2020-07-20""
  }}
";

		string GetForeignOperatorJsonMessage(string rootCnpj) => @$"
  {{
    ""seq"": 1,
    ""cpfCnpjRaiz"": ""{rootCnpj}"",
    ""codigo"": ""01"",
    ""versao"": ""1"",
    ""tin"": ""50178"",
    ""nome"": ""Foreign Operator Name"",
    ""situacao"": ""Active"",
    ""logradouro"": ""Address"",
    ""nomeCidade"": ""SAO PAULO"",
    ""codigoSubdivisaoPais"": ""SP"",
    ""codigoPais"": ""BR"",
    ""cep"": ""04547006"",
    ""codigoInterno"": ""CW"",
    ""email"": ""support@wisetechglobal.com"",
    ""dataReferencia"": ""21-06-2016"",
    ""identificacoesAdicionais"": [
      {{
        ""numero"": ""125782"",
        ""codigo"": ""01""
      }}
    ]
  }}
";
		public static string GetProductLinkJsonMessage(string rootCnpj) => $@"
  {{
    ""seq"": 1,
    ""cpfCnpjRaiz"": ""{rootCnpj}"",
    ""codigoOperadorEstrangeiro"": ""123"",
    ""cpfCnpjFabricante"": ""25043512"",
    ""conhecido"": true,
    ""codigoProduto"": 111,
    ""vincular"": true,
    ""dataReferencia"": ""21-06-2016"",
    ""codigoPais"": ""DE""
  }}";
	}
}
