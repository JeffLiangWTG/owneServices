using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCInboundMessageCreatorBaseOnlyTest : BRCInboundMessageCreatorAbstractTest
	{
		protected override ZString InterchangeType => MessageTypeList.Codes.SUB;

		public void TestGenerateMessageFromInterchange_XUE()
		{
			AssertCreateMessageFromInterchange(CreateUniversalInterchangeXml(responseMessage: ResponseMessage), ResponseMessage);
		}

		public void TestGenerateMessageFromInterchange_RES()
		{
			AssertCreateMessageFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(messageType: "RES", responseMessage: ResponseMessage), ResponseMessage, expectedMessageSubType: EDIMessageSubTypeList.Codes.Success);
		}

		public void TestGenerateMessageFromInterchange_BER()
		{
			AssertCreateMessageFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(messageType: "BER", responseMessage: ErrorResponseMessage), ErrorResponseMessage, expectedMessageSubType: EDIMessageSubTypeList.Codes.Error);
		}

		public void TestGenerateMessageFromInterchange_SetBranch()
		{
			var caCompany = Factory.NewWithValidTestData<GlbCompany>();
			caCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			var caBranch = Factory.NewWithValidTestData<GlbBranch>();
			caCompany.Branches.Add(caBranch);
			Factory.Save();

			var interchange = ProcessEDIInterchange("Test", caBranch.PK);
			CombineAssertions(() =>
			{
				AssertEquals("Interchange status should be set to Received", EDIInterchangeStatusList.Codes.Received, interchange.EI_Status);
				AssertEquals("Should have been 1 message extracted from interchange", 1, interchange.ContainedMessages.Count);
				AssertEquals("EM_GB set to a BR branch", Core.Constants.CountryCodes.Brazil, interchange.ContainedMessages[0].Company.GC_RN_NKCountryCode);
			});
		}

		const string ResponseMessage = @"
		{
			""id"": 1,
			""evento"": ""duex-historico"",
			""endpoint"": ""https://endpoint:443""
		}";

		const string ErrorResponseMessage = @"
		{
			""message"":""Já existe uma assinatura para esse evento. Você deve usar o serviço de edição."",
			""code"":""PLAT-ER9100"",
			""field"":""evento"",
			""path"":null,
			""tag"":""[PLAT-WEZBAJCSPR]"",
			""date"":""2023-10-27T14:02:31"",
			""detail"":[],
			""severity"":""ERROR"",
			""info"": {
				""mnemonico"":""PLAT"",
				""sistema"":""Plataforma [Módulo Notificação - Serviços de Notificação de Usuários e Webhooks]"",
				""ambiente"":""TRE"",
				""visao"":""PRIV"",
				""usuario"":""26357553883"",
				""url"":""/plat-notificacao/api/ext/webhook"",
				""fluxo"":null,
				""trackerId"":""jn368nmyGs""
			}
		}";
	}
}
