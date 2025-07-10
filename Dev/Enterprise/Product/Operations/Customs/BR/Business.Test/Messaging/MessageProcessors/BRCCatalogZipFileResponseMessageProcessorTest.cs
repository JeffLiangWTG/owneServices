using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCCatalogZipFileResponseMessageProcessorTest : BRCResponseMessageProcessorAbstractTest<BRCCatalogZipFileResponseMessageProcessor>
	{
		protected override IReadOnlyList<string> MessageFilterTypes => new[] { "CAT" };

		protected override IReadOnlyList<string> MessageFilterSubTypes => new[] { "CZP" };

		public void TestProcessResponseMessage_InvalidJson()
		{
			AssertExceptionThrown<JsonException>(() => AddCatalogZipFileResponseAndProcess("[]"));
		}

		ZQuery GetCatalogByAuthorityAndOwner(string authority, ZGuid ownerPK)
		{
			var query = new ZQuery(CusGoodsCatalogSchema.CGC_AuthorityIdentifier, "1");
			query.AddToFilter(CusGoodsCatalogSchema.CGC_OH_Owner, ownerPK);
			return query;
		}

		public void TestProcessResponseMessage_NewAndUpdate()
		{
			var importer = CreateImporter();
 
			var catalogWithDiferentOwner = Factory.New<CusGoodsCatalog>();
			catalogWithDiferentOwner.CGC_AuthorityIdentifier = "1";
			catalogWithDiferentOwner.CGC_OH_Owner = importer3.PK;
			catalogWithDiferentOwner.CGC_Type = GoodsCatalogTypeList.Codes.Import;
			catalogWithDiferentOwner.CGC_Description = "Test Goods";

			var jsonMessage = JsonMessage;
			var catalogUpdatedLog = "Catalog with Authority Identifier '1' already exists and was updated.";
			var catalogNotUpdatedLog = $"Catalog with Authority Identifier '1' already exists and was not updated.";

			(var logger, var message) = AddCatalogZipFileResponseAndProcess(jsonMessage);
			var catalog = Factory.LoadTop1<CusGoodsCatalog>(GetCatalogByAuthorityAndOwner("1",importer.PK));
			AssertMessageAndNewCusGoodsCatalog(message, "2", "Teste (Reativação)", "Teste Descricao", "", Events.CatalogAddedByDownload.Code);
			ExecuteMessageProcessor();
			AssertMessageAndNewCusGoodsCatalog(message, "2", "Teste (Reativação)", "Teste Descricao", catalogNotUpdatedLog, AutoEvents.CatalogNotUpdatedByDownloadCode);

			jsonMessage = jsonMessage.Replace("\"versao\" : \"2\"", "\"versao\" : \"3\"");
			ExecuteMessageProcessor();
			AssertMessageAndNewCusGoodsCatalog(message, "3", "Teste (Reativação)", "Teste Descricao", catalogUpdatedLog, AutoEvents.CatalogUpdatedByDownloadCode);

			jsonMessage = jsonMessage.Replace("\"denominacao\" : \"Teste (Reativação)\"", "\"denominacao\" : \"Teste (Reativação) XXX\"");
			ExecuteMessageProcessor();
			AssertMessageAndNewCusGoodsCatalog(message, "3", "Teste (Reativação) XXX", "Teste Descricao", catalogUpdatedLog, AutoEvents.CatalogUpdatedByDownloadCode);

			jsonMessage = jsonMessage.Replace("\"descricao\" : \"Teste Descricao\"", "\"descricao\" : \"Teste Descricao XXX\"");
			ExecuteMessageProcessor();
			AssertMessageAndNewCusGoodsCatalog(message, "3", "Teste (Reativação) XXX", "Teste Descricao XXX", catalogUpdatedLog, AutoEvents.CatalogUpdatedByDownloadCode);

			jsonMessage = JsonMessage;
			catalog.CGC_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			(logger, message) = AddCatalogZipFileResponseAndProcess(jsonMessage);
			AssertEquals("Catalog not updated due to CGC_MessageStatus is not ACC", false, catalog.HasChanges);
			AssertEquals("CGC_MessageStatus not updated", BRMessageStatusList.Codes.AwaitingResponse, catalog.CGC_MessageStatus);
			AssertEquals("Logger", "Warning: \tCatalog with Authority Identifier '1' can only be updated when Message Status = 'ACC'\r\n", logger.LogMessages.ToString());

			void ExecuteMessageProcessor()
			{
				(logger, message) = AddCatalogZipFileResponseAndProcess(jsonMessage);
				AssertEquals("ResetStatuses Suspended", true, catalog.IsResetStatusesSuspended);
				AssertEquals("UpdateCustomStatusOnSaving Suspended", true, catalog.IsUpdateCustomStatusOnSavingSuspended);
			}

			void AssertMessageAndNewCusGoodsCatalog(EDIMessage message, string version, string description, string compDesc, string log, string eventCode)
			{
				CombineAssertions(() =>
				{
					AssertEquals("EM_GB", catalog.Company.FirstActiveBranch.PK, message.EM_GB);
					AssertEquals("EM_MessageType", MessageTypeList.Codes.CAT, message.EM_MessageType);
					AssertEquals("EM_MessageSubType", EDIMessageSubTypeList.Codes.CatalogZipFile, message.EM_MessageSubType);
					AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, message.EM_Status);
					AssertEquals("EM_LinkedObject", catalog, message.EM_LinkedObject);

					AssertEquals("CGC_AuthorityIdentifier", "1", catalog.CGC_AuthorityIdentifier);
					AssertEquals("CGC_Description", description, catalog.CGC_Description);
					AssertEquals("CGC_AuthorityStatus", "0", catalog.CGC_AuthorityStatus);
					AssertEquals("CGC_Type", "IMP", catalog.CGC_Type);
					AssertEquals("CGC_Tariff", "01012100", catalog.CGC_Tariff);
					AssertEquals("CGC_AuthorityVersion", version, catalog.CGC_AuthorityVersion);
					AssertEquals("CGC_OH_Owner", importer.PK, catalog.CGC_OH_Owner);
					AssertEquals("ComplementaryDescription", compDesc, catalog.ComplementaryDescription);
					AssertEquals($"{eventCode} Event added", eventCode, catalog.Logs.LogsNotInDB.LastOrDefault()?.SL_SE_NKEvent);
					AssertContains("Logger", log, logger.LogMessages.ToString());
					logger.ClearLogs();

					catalog.Validation.ValidateAll();
					AssertNoErrors(catalog);

					AssertNoExceptionThrown(Factory.Save);
					AssertNotEquals("CGC_CatalogCode", ZString.Empty, catalog.CGC_CatalogCode);
					AssertEquals("IsResetStatusesSuspended", false, catalog.IsResetStatusesSuspended);
					AssertEquals("CGC_MessageStatus", BRMessageStatusList.Codes.Accepted, catalog.CGC_MessageStatus);
					AssertEquals("CGC_CustomsStatus", CustomsPostedStatusList.Codes.Accepted, catalog.CGC_CustomsStatus);
					AssertEquals("UpdateCustomStatusOnSaving not Suspended", false, catalog.IsUpdateCustomStatusOnSavingSuspended);
				});
			}
		}

		public void TestProcessResponseMessage_UpdateLocalPartNumbers()
		{
			var importer = CreateImporter();

			var jsonMessage = JsonMessage;
			var catalogUpdatedLog = "Catalog with Authority Identifier '1' already exists and was updated.";
			var catalogNotUpdatedLog = $"Catalog with Authority Identifier '1' already exists and was not updated.";

			string GetEventReference(string partNumber) => $"|DES=Local Part Number {partNumber} was removed during the download because it was not contained in the latest downloaded JSON file.";

			(var logger, var message) = AddCatalogZipFileResponseAndProcess(jsonMessage);
			var catalog = Factory.LoadTop1<CusGoodsCatalog>(new ZQuery(CusGoodsCatalogSchema.CGC_AuthorityIdentifier, "1"));
			AssertLocalPartNumbersUpdated(new[] { "LR1", "LR2", "LR3" }, "", (Events.CatalogAddedByDownloadCode, ""));

			(logger, message) = AddCatalogZipFileResponseAndProcess(jsonMessage);
			AssertLocalPartNumbersUpdated(new[] { "LR1", "LR2", "LR3" }, catalogNotUpdatedLog, (Events.CatalogNotUpdatedByDownloadCode, ""));

			var product1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product1.OP_PartNum = "LR1";
			var product2 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product2.OP_PartNum = "LR2";
			var product3 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product3.OP_PartNum = "LR3";
			var pivot1 = product1.PivotsForBinding.AddNew();
			pivot1.CI_CGC_Catalog = catalog.PK;
			var pivot2 = product2.PivotsForBinding.AddNew();
			pivot2.CI_CGC_Catalog = catalog.PK;
			var pivot3 = product3.PivotsForBinding.AddNew();
			pivot3.CI_CGC_Catalog = catalog.PK;

			Factory.Save();

			(logger, message) = AddCatalogZipFileResponseAndProcess(jsonMessage.Replace("\"codigosInterno\" : [ \"LR1\", \"LR2\", \"LR3\" ]", "\"codigosInterno\" : [ \"LR1\", \"LR4\" ]"));
			AssertLocalPartNumbersUpdated(new[] { "LR1", "LR4" }, catalogUpdatedLog,
				(Events.CatalogUpdatedByDownloadCode, ""),
				(Events.ItemRemovedCode, GetEventReference("LR2")),
				(Events.ItemRemovedCode, GetEventReference("LR3")));

			CombineAssertions(() =>
			{
				AssertEquals("LR1 not removed", catalog.PK, pivot1.CI_CGC_Catalog);
				AssertEquals("LR2 removed", ZGuid.Empty, pivot2.CI_CGC_Catalog);
				AssertEquals("LR3 removed", ZGuid.Empty, pivot3.CI_CGC_Catalog);
			});

			void AssertLocalPartNumbersUpdated(string[] localPartNumbers, string log, params (string code, string reference)[] events)
			{
				CombineAssertions(() =>
				{
					AssertContainsExactElementsInAnyOrder("LocalPartNumbers", localPartNumbers, catalog.LocalPartNumbers.Select(x => x.CGI_Reference));
					AssertContainsExactElementsInAnyOrder(events.Select(x => $"{x.code}: {x.reference}"), catalog.Logs.LogsNotInDB.Select(x => $"{x.SL_SE_NKEvent}: {x.SL_Reference}"));
					AssertContains("Logger", log, logger.LogMessages.ToString());
					logger.ClearLogs();

					AssertNoExceptionThrown(Factory.Save);
				});
			}
		}

		public void TestProcessResponseMessage_UpdateAttributes()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var profileType = helper.CreateRefCusProfileType(Constants.Profile.Types.NCMTE, "HSN", Core.Constants.CountryCodes.Brazil);
			var stringQuestion1 = helper.CreateRefCusProfileQuestion(profileType, "Test", "ATT_5898", "Text", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue,
				Universal.Constants.ProfileQuestion.AnswerDataTypes.String, allowMultipleAnswers: true);
			var stringQuestion2 = helper.CreateRefCusProfileQuestion(profileType, "Test", "ATT_5946", "Text", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue,
				Universal.Constants.ProfileQuestion.AnswerDataTypes.String, allowMultipleAnswers: true);
			var stringQuestion3 = helper.CreateRefCusProfileQuestion(profileType, "Test", "ATT_5396", "Text", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue,
				Universal.Constants.ProfileQuestion.AnswerDataTypes.String, allowMultipleAnswers: true);

			var importer = CreateImporter();

			var jsonMessageWithAttributes = JsonMessage.Replace("\"atributos\" : null", atributos).Replace("\"atributosCompostos\" : null", atributosCompostos);
			(var logger, var message) = AddCatalogZipFileResponseAndProcess(jsonMessageWithAttributes);
			var catalog = Factory.LoadTop1<CusGoodsCatalog>(new ZQuery(CusGoodsCatalogSchema.CGC_AuthorityIdentifier, "1"));

			var catalogUpdatedLog = "Catalog with Authority Identifier '1' already exists and was updated.";
			var catalogNotUpdatedLog = $"Catalog with Authority Identifier '1' already exists and was not updated.";

			AssertMessageAndCusGoodsCatalog(message, "", Events.CatalogAddedByDownload.Code);
			(logger, message) = AddCatalogZipFileResponseAndProcess(jsonMessageWithAttributes);
			AssertMessageAndCusGoodsCatalog(message, catalogNotUpdatedLog, Events.CatalogNotUpdatedByDownload.Code);

			catalog.Attributes.AddNew("ATT_5111", "1");
			catalog.Attributes.GetFirstElementHaving("ATT_5898").Delete();
			catalog.Attributes.GetFirstElementHaving("ATT_5396").CY_Data = "6";

			catalog.Attributes.AddNew("ATT_5112", "1");
			catalog.Attributes.GetFirstElementHaving("ATT_7551").Delete();
			catalog.Attributes.GetFirstElementHaving("ATT_7552").CY_Data = "01";

			(logger, message) = AddCatalogZipFileResponseAndProcess(jsonMessageWithAttributes);
			AssertMessageAndCusGoodsCatalog(message, catalogUpdatedLog, Events.CatalogUpdatedByDownload.Code);

			(logger, message) = AddCatalogZipFileResponseAndProcess(JsonMessage.Replace("\"atributosCompostos\" : null", "\"atributosCompostos\": [ { \"atributo\": \"ATT_7553\", \"valores\": null, } ]"));
			AssertEquals("atributos is NOT null", 1, catalog.Attributes.Count);
			AssertAttributesContains(catalog.Attributes, "ATT_7553", ZString.Empty);

			(logger, message) = AddCatalogZipFileResponseAndProcess(JsonMessage);
			AssertEquals("atributos is null & atributosCompostos is null", 0, catalog.Attributes.Count);

			var attribute1 = catalog.Attributes.AddNew("ATT_5898", "1");
			attribute1.TariffProfileQuestion = TariffProfileQuestion.New(stringQuestion1);
			var attribute2 = catalog.Attributes.AddNew("ATT_5946", "1");
			attribute2.TariffProfileQuestion = TariffProfileQuestion.New(stringQuestion1);
			var attribute3 = catalog.Attributes.AddNew("ATT_5396", "1");
			attribute3.TariffProfileQuestion = TariffProfileQuestion.New(stringQuestion1);

			var jsonMessageWithMultivaluedAttribues = JsonMessage.Replace("\"atributos\" : null", atributos).Replace("\"atributosMultivalorados\" : []", atributosMultivalorados);
			AddCatalogZipFileResponseAndProcess(jsonMessageWithMultivaluedAttribues);
			AssertEquals("atributos is NOT null", 3, catalog.Attributes.Count);
			AssertMultivaluedAttributesContains(catalog.Attributes, "ATT_5898", new ZString[] { "1", "2", "3" });
			AssertMultivaluedAttributesContains(catalog.Attributes, "ATT_5946", new ZString[] { "4", "5", "6" });
			AssertMultivaluedAttributesContains(catalog.Attributes, "ATT_5396", new ZString[] { "7", "8", "9" });

			void AssertMessageAndCusGoodsCatalog(EDIMessage message, string log, string eventCode)
			{
				CombineAssertions(() =>
				{
					AssertEquals("EM_GB", catalog.Company.FirstActiveBranch.PK, message.EM_GB);
					AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, message.EM_Status);
					AssertEquals("EM_LinkedObject", catalog, message.EM_LinkedObject);

					AssertEquals("CGC_MessageStatus", BRMessageStatusList.Codes.Accepted, catalog.CGC_MessageStatus);
					AssertEquals("CGC_CustomsStatus", CustomsPostedStatusList.Codes.Accepted, catalog.CGC_CustomsStatus);

					AssertEquals($"{eventCode} Event added", eventCode, catalog.Logs.LogsNotInDB.LastOrDefault()?.SL_SE_NKEvent);
					AssertEquals("Attributes.Count", 7, catalog.Attributes.Count);
					AssertAttributesContains(catalog.Attributes, "ATT_5898", "1");
					AssertAttributesContains(catalog.Attributes, "ATT_5946", "5");
					AssertAttributesContains(catalog.Attributes, "ATT_5396", "Teste");
					AssertAttributesContains(catalog.Attributes, "ATT_7553", ZString.Empty);
					AssertAttributesContains(catalog.Attributes, "ATT_7550", "01");
					AssertAttributesContains(catalog.Attributes, "ATT_7551", "12.2222");
					AssertAttributesContains(catalog.Attributes, "ATT_7552", "03");

					AssertContains("Logger", log, logger.LogMessages.ToString());
					logger.ClearLogs();

					Factory.Save();
				});
			}

			void AssertAttributesContains(AttributeCusCodeDataCollection attributes, ZString code, ZString data)
			{
				var attribute = attributes.GetElementsHaving(code).SingleOrDefault();
				AssertNotNull($"Contains Attribute[{code}]", attribute);
				AssertEquals($"Attribute[{code}] = {data}", data, attribute.CY_Data);
			}

			void AssertMultivaluedAttributesContains(AttributeCusCodeDataCollection attributes, ZString code, ZString[] data)
			{
				var attribute = attributes.GetElementsHaving(code).SingleOrDefault();
				AssertNotNull($"Contains Attribute[{code}]", attribute);
				AssertEquals($"Attribute[{code}]", data.FirstOrDefault(), attribute.CY_Data);
				AssertContainsExactElementsInExactOrder($"Attribute[{code}] = {data}", data.Skip(1), attribute.MultivaluedAttributesLinked.Select(x => x.CY_Data));
			}
		}

		public void TestProcessResponseMessage_AllDownloadCatalogResponsesProcessedLog()
		{
			var importer = CreateImporter();
			var logger = new LoggingInformationForTesting();

			var (requestMessage, responseMessage1) = BRCResponseMessageProcessorTest.CreateResponseMessage(importer, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.CatalogZipFile);
			responseMessage1.EM_MessageText = JsonMessage;

			var responseMessage2 = BRCResponseMessageProcessorTest.CreateMessage(Factory, responseMessage1.Interchange.PK, null, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.CatalogZipFile);
			responseMessage2.EM_MessageText = JsonMessage.Replace("\"codigo\" : 1", "\"codigo\" : 2");

			var responseMessage3 = BRCResponseMessageProcessorTest.CreateMessage(Factory, responseMessage1.Interchange.PK, null, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.PreProcessedOK, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.CatalogZipFile);
			responseMessage3.EM_MessageText = JsonMessage.Replace("\"codigo\" : 1", "\"codigo\" : 3");

			Factory.Save();

			var expectedEvent = AutoEvents.AllDownloadCatalogResponsesProcessed;
			new BRCCatalogZipFileResponseMessageProcessor(logger).ProcessMessage(responseMessage1);
			AssertNull("Log AllDownloadCatalogResponsesProcessed not added", importer.Logs.MostRecentLogByEventTime(expectedEvent));

			new BRCCatalogZipFileResponseMessageProcessor(logger).ProcessMessage(responseMessage2);
			AssertNull("Log AllDownloadCatalogResponsesProcessed not added", importer.Logs.MostRecentLogByEventTime(expectedEvent));

			new BRCCatalogZipFileResponseMessageProcessor(logger).ProcessMessage(responseMessage3);
			AssertEquals("Log AllDownloadCatalogResponsesProcessed added", expectedEvent.Code, importer.Logs.MostRecentLog.SL_SE_NKEvent);
			AssertEquals("SL_Reference", requestMessage.PK.ToString(), importer.Logs.MostRecentLog.SL_Reference);
		}

		(LoggingInformationForTesting, EDIMessage) AddCatalogZipFileResponseAndProcess(ZString messageText)
		{
			var responseMessage = BRCResponseMessageProcessorTest.CreateMessage(Factory, ZGuid.Empty, null, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.CatalogZipFile);
			responseMessage.EM_MessageText = messageText;
			Factory.Save();

			return (ExecuteMessageProcessor(responseMessage), responseMessage);
		}

		public void TestProcessResponseMessage_IncorrectData()
		{
			var importer = CreateImporter();
			AssertErrorMessage(JsonMessage.Replace("\"denominacao\" : \"Teste (Reativação)\"", "\"denominacao\" : \"\""), "'denominacao' tag is empty");
			AssertErrorMessage(JsonMessage.Replace("\"modalidade\" : \"IMPORTACAO\"", "\"modalidade\" : \"ERROR\""), "'modalidade' tag is empty or invalid");
			var multipleEmptyValuesJson = JsonMessage.Replace("\"denominacao\" : \"Teste (Reativação)\"", "\"denominacao\" : \"\"").Replace("\"modalidade\" : \"IMPORTACAO\"", "\"modalidade\" : \"ERROR\"");
			var expectedError = "'modalidade' tag is empty or invalid, 'denominacao' tag is empty";
			AssertErrorMessage(multipleEmptyValuesJson, expectedError);

			AssertErrorMessage(JsonMessage.Replace("\"cpfCnpjRaiz\" : \"22955307\"", "\"cpfCnpjRaiz\" : \"\""), "Message deserialization was failed, tag 'codigo' or 'cpfCnpjRaiz' not found");
			multipleEmptyValuesJson = JsonMessage.Replace("\"cpfCnpjRaiz\" : \"22955307\"", "\"cpfCnpjRaiz\" : \"\"");
			expectedError = "Message deserialization was failed, tag 'codigo' or 'cpfCnpjRaiz' not found";
			AssertErrorMessage(multipleEmptyValuesJson, expectedError);
		}

		public void TestProcessResponseMessage_OrganisationDontExist()
		{
			var importer = CreateImporter();
			AssertErrorMessage(JsonMessage.Replace("\"cpfCnpjRaiz\" : \"22955307\"", "\"cpfCnpjRaiz\" : \"00123456\""), "Root CNPJ '00123456' does not have a matching organization");
			importer.Delete();
			AssertErrorMessage(JsonMessage, "Root CNPJ '22955307' does not have a matching organization");
		}

		void AssertErrorMessage(string jsonMessage, string expectedMessage)
		{
			(var logger, var responseMessage) = AddCatalogZipFileResponseAndProcess(jsonMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
				AssertContains("Logger", $"Message #1: {expectedMessage}. The message status was updated to 'FAL'.", logger.LogMessages.ToString());
			});
			logger.ClearLogs();
		}

		OrgHeader CreateImporter()
		{
			var importer1 = Factory.NewWithValidTestData<OrgHeader>();
			importer1.OH_Code = "BRIMPORTER1";
			importer1.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "22.955.307/0001-88", Core.Constants.CountryCodes.Brazil);
			importer1.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ, "22955307", Core.Constants.CountryCodes.Brazil);

			var importer2 = Factory.NewWithValidTestData<OrgHeader>();
			importer2.OH_Code = "BRIMPORTER2";
			importer2.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "22955307", Core.Constants.CountryCodes.Brazil);

			importer3 = Factory.NewWithValidTestData<OrgHeader>();
			importer3.OH_Code = "BRIMPORTER3";
			importer3.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "22.955.308/0001-88", Core.Constants.CountryCodes.Brazil);
			importer3.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ, "22955308", Core.Constants.CountryCodes.Brazil);
			Factory.Save();

			return importer1;
		}

		OrgHeader importer3;

		public const string JsonMessage = @"{
  ""seq"" : 1,
  ""codigo"" : 1,
  ""descricao"" : ""Teste Descricao"",
  ""denominacao"" : ""Teste (Reativação)"",
  ""cpfCnpjRaiz"" : ""22955307"",
  ""situacao"" : ""Ativado"",
  ""modalidade"" : ""IMPORTACAO"",
  ""ncm"" : ""01012100"",
  ""versao"" : ""2"",
  ""atributos"" : null,
  ""atributosMultivalorados"" : [],
  ""atributosCompostosMultivalorados"" : [],
  ""atributosCompostos"" : null,
  ""codigosInterno"" : [ ""LR1"", ""LR2"", ""LR3"" ]
}";

		const string atributos = @"
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
      ""atributo"": null,
      ""valor"": null
    }
  ]";

		const string atributosCompostos = @"
  ""atributosCompostos"": [
    {
      ""atributo"": ""ATT_7553"",
      ""valores"": [
        {
          ""atributo"": ""ATT_7550"",
          ""valor"": ""01""
        },
        {
          ""atributo"": ""ATT_7551"",
          ""valor"": ""12.2222""
        },
        {
          ""atributo"": ""ATT_7552"",
          ""valor"": ""03""
        },
				{
					""atributo"": null,
					""valor"": null
				}
      ],
    }
  ]";

		const string atributosMultivalorados = @"
    ""atributosMultivalorados"": [
    {
      ""atributo"": ""ATT_5898"",
      ""valores"": [
        ""1"",
        ""2"",
        ""3""
      ]
    },
    {
      ""atributo"": ""ATT_5946"",
      ""valores"": [
        ""4"",
        ""5"",
        ""6""
      ]
    },
    {
      ""atributo"": ""ATT_5396"",
      ""valores"": [
        ""7"",
        ""8"",
        ""9""
      ]
    }
  ]";
	}
}
