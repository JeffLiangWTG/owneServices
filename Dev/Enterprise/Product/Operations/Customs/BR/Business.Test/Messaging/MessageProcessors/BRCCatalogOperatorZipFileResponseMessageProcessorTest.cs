using System.Collections.Generic;
using System.Text.Json;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCCatalogOperatorZipFileResponseMessageProcessorTest : BRCResponseMessageProcessorAbstractTest<BRCCatalogOperatorZipFileResponseMessageProcessor>
	{
		protected override IReadOnlyList<string> MessageFilterTypes => new[] { "CAT" };

		protected override IReadOnlyList<string> MessageFilterSubTypes => new[] { "OZI" };

		public void TestProcessResponseMessage_InvalidJson()
		{
			var logger = new LoggingInformationForTesting();
			AssertExceptionThrown<JsonException>(() => AddForeignOperatorZipFileResponseAndProcess("[]", null, expectedIsUpdateMessageStatusOnSavingSuspended: true, ref logger));
		}

		public void TestProcessResponseMessage_NewAndUpdate()
		{
			var importer = CreateImporter();

			var jsonMessage = JsonMessage;
			var logger = new LoggingInformationForTesting();
			var foreignOperatorUpdatedLog = "Foreign Operator with Authority Identifier 'OPE_11' already exists and was updated.";
			var foreignOperatorNotUpdatedLog = "Foreign Operator with Authority Identifier 'OPE_11' already exists and was not updated.";

			CusBRForeignOperator foreignOperator = null;
			var message = ProcessForeignOperatorZipFileResponse();
			var owner = OrgHeader.FindByOrgCusCode(message.Factory, BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ, "22955307", Core.Constants.CountryCodes.Brazil);
			foreignOperator = new CusBRForeignOperator.Loader(Factory).LoadByOwnerAndAuthorityIdentifier(owner, "OPE_11");
			AssertMessageAndForeignOperator(message, "2", ZString.Empty, ForeignOperatorCustomsStatusTypeList.Codes.Active);
			AssertMessageAndForeignOperator(ProcessForeignOperatorZipFileResponse(), "2", foreignOperatorNotUpdatedLog, ForeignOperatorCustomsStatusTypeList.Codes.Active);

			foreignOperator.BFR_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			jsonMessage = JsonMessage.Replace("\"versao\": \"2\"", "\"versao\" : \"3\"");
			AssertMessageAndForeignOperator(ProcessForeignOperatorZipFileResponse(), "3", foreignOperatorUpdatedLog, ForeignOperatorCustomsStatusTypeList.Codes.Active);

			var foreignOperatorOrg = OrgHeader.FindByOrgCusCode(message.Factory, BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ, "11468247", Core.Constants.CountryCodes.Brazil);
			foreignOperator.BFR_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			foreignOperator.BFR_OH_ForeignOperator = foreignOperatorOrg.PK;
			jsonMessage = JsonMessage.Replace("\"versao\": \"2\"", "\"versao\" : \"3\"");
			AssertMessageAndForeignOperator(ProcessForeignOperatorZipFileResponse(), "3", foreignOperatorNotUpdatedLog, ForeignOperatorCustomsStatusTypeList.Codes.Active);

			foreignOperator.BFR_OH_ForeignOperator = ZGuid.Empty;
			foreignOperator.BFR_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			jsonMessage = JsonMessage.Replace("\"situacao\": \"Ativado\"", "\"situacao\" : \"Desativado\"").Replace("\"versao\": \"2\"", "\"versao\" : \"3\"");
			AssertMessageAndForeignOperator(ProcessForeignOperatorZipFileResponse(), "3", foreignOperatorUpdatedLog, ForeignOperatorCustomsStatusTypeList.Codes.Inactive);

			jsonMessage = JsonMessage.Replace("\"codigo\": \"OPE_11\"", "\"codigo\" : \"OPE_12\"");
			ProcessForeignOperatorZipFileResponse(false);
			var foreignOperatorNew = new CusBRForeignOperator.Loader(Factory).LoadByOwnerAndAuthorityIdentifier(owner, "OPE_12");
			AssertNotEquals(foreignOperator.PK, foreignOperatorNew.PK);

			EDIMessage ProcessForeignOperatorZipFileResponse(bool expectedIsUpdateMessageStatusOnSavingSuspended = true) => AddForeignOperatorZipFileResponseAndProcess(jsonMessage, foreignOperator, expectedIsUpdateMessageStatusOnSavingSuspended, ref logger);

			void AssertMessageAndForeignOperator(EDIMessage message, string version, string log, string customsStatus)
			{
				CombineAssertions(() =>
				{
					AssertEquals("EM_MessageType", MessageTypeList.Codes.CAT, message.EM_MessageType);
					AssertEquals("EM_MessageSubType", EDIMessageSubTypeList.Codes.OperatorZipFile, message.EM_MessageSubType);
					AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, message.EM_Status);
					AssertEquals("EM_LinkedObject", foreignOperator.PK, message.EM_LinkedObject.PK);
					AssertEquals("BFR_AuthorityIdentifier", "OPE_11", foreignOperator.BFR_AuthorityIdentifier);
					AssertEquals("BFR_AuthorityVersion", version, foreignOperator.BFR_AuthorityVersion);
					AssertEquals("BFR_OH_Owner", owner.PK, foreignOperator.BFR_OH_Owner);
					AssertEquals("BFR_Name", foreignOperator.ForeignOperator == null ? "MONKEY D. LUFFY COMPANY" : ZString.Empty, foreignOperator.BFR_Name);
					AssertEquals("BFR_RN_NKCountryCode", foreignOperator.ForeignOperator == null ? "CN" : ZString.Empty, foreignOperator.BFR_RN_NKCountryCode);
					AssertEquals("BFR_CustomsStatus", customsStatus, foreignOperator.BFR_CustomsStatus);
					AssertEquals("BFR_MessageStatus", BRMessageStatusList.Codes.Accepted, foreignOperator.BFR_MessageStatus);
					AssertContains("Logger", log, logger.LogMessages.ToString());
					logger.ClearLogs();
				});
			}
		}

		EDIMessage AddForeignOperatorZipFileResponseAndProcess(ZString messageText, CusBRForeignOperator foreignOperator, bool expectedIsUpdateMessageStatusOnSavingSuspended, ref LoggingInformationForTesting logger)
		{
			var responseMessage = BRCResponseMessageProcessorTest.CreateMessage(Factory, ZGuid.Empty, null, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.OperatorZipFile);
			responseMessage.EM_MessageText = messageText;
			Factory.Save();

			logger = ExecuteMessageProcessor(responseMessage);
			AssertEquals("UpdateMessageStatusOnSaving Suspended", expectedIsUpdateMessageStatusOnSavingSuspended, foreignOperator?.IsUpdateMessageStatusOnSavingSuspended ?? true);
			Factory.Save();
			AssertEquals("UpdateMessageStatusOnSaving NOT Suspended", false, foreignOperator?.IsUpdateMessageStatusOnSavingSuspended ?? false);
			return responseMessage;
		}

		public void TestProcessResponseMessage_OrganisationDontExist()
		{
			AssertProcessResponseMessage(JsonMessage, "Organization with RTC '22955307' not found");

			void AssertProcessResponseMessage(string jsonMessage, string expectedMessage)
			{
				var logger = new LoggingInformationForTesting();
				var responseMessage = AddForeignOperatorZipFileResponseAndProcess(jsonMessage, null, expectedIsUpdateMessageStatusOnSavingSuspended: true, ref logger);
				CombineAssertions(() =>
				{
					AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
					AssertEquals("EM_LinkUniqueID", ZGuid.Empty, responseMessage.EM_LinkUniqueID);
					AssertContains("Logger", $"Message #1: {expectedMessage}.", logger.LogMessages.ToString());
				});
				logger.ClearLogs();
			}
		}

		OrgHeader CreateImporter()
		{
			var importer1 = Factory.NewWithValidTestData<OrgHeader>();
			importer1.OH_Code = "BRIMPORTER1";
			importer1.OH_FullName = "BR IMPORTER 1";
			importer1.OH_RL_NKClosestPort = "BR";
			importer1.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "22.955.307/0001-88", Core.Constants.CountryCodes.Brazil);
			importer1.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ, "22955307", Core.Constants.CountryCodes.Brazil);

			var importer2 = Factory.NewWithValidTestData<OrgHeader>();
			importer2.OH_Code = "BRIMPORTER2";
			importer2.OH_FullName = "BR IMPORTER 2";
			importer2.OH_RL_NKClosestPort = "BR";
			importer2.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "22955307", Core.Constants.CountryCodes.Brazil);

			var forOperator = Factory.NewWithValidTestData<OrgHeader>();
			forOperator.OH_Code = "BRFO";
			forOperator.OH_FullName = "BR FOREIGN OPERATOR";
			forOperator.OH_RL_NKClosestPort = "BR";
			forOperator.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "11.468.247/0001-53", Core.Constants.CountryCodes.Brazil);
			forOperator.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ, "11468247", Core.Constants.CountryCodes.Brazil);
			Factory.Save();

			return importer1;
		}

		public const string JsonMessage = @"{
	""seq"": 4,
	""cpfCnpjRaiz"": ""22955307"",
	""codigo"": ""OPE_11"",
	""versao"": ""2"",
	""tin"": ""21333333333333333333333333333333333"",
	""nome"": ""MONKEY D. LUFFY COMPANY"",
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
	}
}
