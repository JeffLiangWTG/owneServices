using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCCatalogManufacturerLinkResponseMessageProcessorTest : BRCResponseMessageProcessorAbstractTest<BRCCatalogManufacturerLinkResponseMessageProcessor>
	{
		protected override IReadOnlyList<string> MessageFilterTypes => new[] { "CAT" };

		protected override IReadOnlyList<string> MessageFilterSubTypes => new[] { "LIN" };

		public void TestMessageFriendlyName()
		{
			var processor = new BRCCatalogManufacturerLinkResponseMessageProcessor(new LoggingInformation());
			AssertEquals("MessageFriendlyName", "Goods Catalog Manufacturer Link Response", processor.MessageFriendlyName);
		}

		public void TestProcessResponseMessage()
		{
			CreateImporters();
			var catalog1 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog1.CGC_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			catalog1.CGC_AuthorityIdentifier = "12";
			catalog1.CGC_OH_Owner = importer1.PK;

			var catalog3 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog3.CGC_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			catalog3.CGC_AuthorityIdentifier = "12";
			catalog3.CGC_OH_Owner = importer2.PK;

			var catalog2 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog2.CGC_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			catalog2.CGC_AuthorityIdentifier = "13";
			catalog2.CGC_OH_Owner = importer2.PK;
			var foreignOperator = catalog2.ForeignOperators.AddNew();
			foreignOperator.CGI_Reference = "DE";
			foreignOperator.AuthorityCode = "123";

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(catalog1, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Link, requestMessageSubType: EDIMessageSubTypeList.Codes.Link, interchangeBodyText: GetJsonRequestMessageText()).ResponseMessage;
			responseMessage.EM_MessageText = GetJsonResponseMessageText();

			ExecuteMessageProcessor(responseMessage);
			AssertEquals("UpdateCustomStatusOnSaving Suspended", true, catalog2.IsUpdateCustomStatusOnSavingSuspended);

			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("EM_LinkUniqueID", catalog2.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", catalog2.TableName, responseMessage.EM_LinkTable);
				AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessage.EM_Status);
				AssertEquals("UpdateCustomStatusOnSaving not Suspended", false, catalog2.IsUpdateCustomStatusOnSavingSuspended);
				AssertEquals("foreignOperator CGI_CustomsStatus", CustomsPostedStatusList.Codes.Accepted, foreignOperator.CGI_CustomsStatus);
				Assert("NO Log AutoEvents.MessageRejected Created", !catalog2.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == AutoEvents.MessageRejected.Code));
				AssertEquals("catalog2 CGC_MessageStatus", BRMessageStatusList.Codes.Accepted, catalog2.CGC_MessageStatus);
			});
		}

		[TestDate(2024, 07, 17, 18, 00, 00)]
		public void TestProcessResponseMessageWithCAT_LINSentAtSameTimeNotProcessed()
		{
			CreateImporters();
			var catalog2 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog2.CGC_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			catalog2.CGC_AuthorityIdentifier = "13";
			catalog2.CGC_OH_Owner = importer2.PK;
			var foreignOperator1 = catalog2.ForeignOperators.AddNew();
			foreignOperator1.CGI_Reference = "DE";
			foreignOperator1.AuthorityCode = "123";
			var foreignOperator2 = catalog2.ForeignOperators.AddNew();
			foreignOperator2.CGI_Reference = "US";
			foreignOperator2.AuthorityCode = "456";

			var responseMessageLIN1 = BRCResponseMessageProcessorTest.CreateResponseMessage(catalog2, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Link, requestMessageSubType: EDIMessageSubTypeList.Codes.Link, interchangeBodyText: GetJsonRequestMessageText()).ResponseMessage;
			responseMessageLIN1.EM_MessageText = GetJsonResponseMessageText();
			var responseMessageLIN2 = responseMessageLIN1.Clone() as EDIMessage;
			Factory.Save();

			ExecuteMessageProcessor(responseMessageLIN1);
			CombineAssertions(() =>
			{
				AssertEquals("EM_LinkUniqueID", catalog2.PK, responseMessageLIN1.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", catalog2.TableName, responseMessageLIN1.EM_LinkTable);
				AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessageLIN1.EM_Status);
				AssertEquals("foreignOperator1 CGI_CustomsStatus", CustomsPostedStatusList.Codes.Accepted, foreignOperator1.CGI_CustomsStatus);
				AssertEquals("foreignOperator2 CGI_CustomsStatus", CustomsPostedStatusList.Codes.Active, foreignOperator2.CGI_CustomsStatus);
				Assert("NO Log AutoEvents.MessageRejected Created", !catalog2.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == AutoEvents.MessageRejected.Code));
				AssertEquals("catalog2 CGC_MessageStatus", BRMessageStatusList.Codes.AwaitingResponse, catalog2.CGC_MessageStatus);
			});
		}

		[TestDate(2024, 07, 17, 18, 00, 00)]
		public void TestProcessResponseMessageWithCAT_LINSentAtSameTimeProcessed()
		{
			CreateImporters();
			var catalog2 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog2.CGC_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			catalog2.CGC_AuthorityIdentifier = "13";
			catalog2.CGC_OH_Owner = importer2.PK;
			var foreignOperator1 = catalog2.ForeignOperators.AddNew();
			foreignOperator1.CGI_Reference = "DE";
			foreignOperator1.AuthorityCode = "123";
			var foreignOperator2 = catalog2.ForeignOperators.AddNew();
			foreignOperator2.CGI_Reference = "US";
			foreignOperator2.AuthorityCode = "456";

			var responseMessageLIN1 = BRCResponseMessageProcessorTest.CreateResponseMessage(catalog2, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Link, requestMessageSubType: EDIMessageSubTypeList.Codes.Link, interchangeBodyText: GetJsonRequestMessageText()).ResponseMessage;
			responseMessageLIN1.EM_MessageText = GetJsonResponseMessageText();
			var responseMessageLIN2 = BRCResponseMessageProcessorTest.CreateResponseMessage(catalog2, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Link, requestMessageSubType: EDIMessageSubTypeList.Codes.Link, interchangeBodyText: GetJsonRequestMessageText()).ResponseMessage;
			responseMessageLIN2.EM_MessageText = GetJsonResponseMessageText();
			responseMessageLIN2.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
			Factory.Save();

			ExecuteMessageProcessor(responseMessageLIN1);
			CombineAssertions(() =>
			{
				AssertEquals("EM_LinkUniqueID", catalog2.PK, responseMessageLIN1.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", catalog2.TableName, responseMessageLIN1.EM_LinkTable);
				AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessageLIN1.EM_Status);
				AssertEquals("foreignOperator1 CGI_CustomsStatus", CustomsPostedStatusList.Codes.Accepted, foreignOperator1.CGI_CustomsStatus);
				AssertEquals("foreignOperator2 CGI_CustomsStatus", CustomsPostedStatusList.Codes.Active, foreignOperator2.CGI_CustomsStatus);
				Assert("Log AutoEvents.MessageRejected Created", catalog2.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == AutoEvents.MessageRejected.Code));
				AssertEquals("catalog2 CGC_MessageStatus", BRMessageStatusList.Codes.Rejected, catalog2.CGC_MessageStatus);
			});
		}

		[TestDate(2024, 07, 17, 18, 00, 00)]
		public void TestProcessResponseMessageWithCAT_ORISentAtSameTime()
		{
			CreateImporters();
			var catalog2 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog2.CGC_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			catalog2.CGC_AuthorityIdentifier = "13";
			catalog2.CGC_OH_Owner = importer2.PK;
			var foreignOperator = catalog2.ForeignOperators.AddNew();
			foreignOperator.CGI_Reference = "DE";
			foreignOperator.AuthorityCode = "123";

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(catalog2, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Link, requestMessageSubType: EDIMessageSubTypeList.Codes.Link, interchangeBodyText: GetJsonRequestMessageText()).ResponseMessage;
			responseMessage.EM_MessageText = GetJsonResponseMessageText();
			BRCResponseMessageProcessorTest.CreateResponseMessage(catalog2, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Success).ResponseMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_LinkUniqueID", catalog2.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", catalog2.TableName, responseMessage.EM_LinkTable);
				AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessage.EM_Status);
				AssertEquals("foreignOperator CGI_CustomsStatus", CustomsPostedStatusList.Codes.Accepted, foreignOperator.CGI_CustomsStatus);
				Assert("NO Log AutoEvents.MessageRejected Created", !catalog2.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == AutoEvents.MessageRejected.Code));
				AssertEquals("catalog2 CGC_MessageStatus", BRMessageStatusList.Codes.AwaitingResponse, catalog2.CGC_MessageStatus);
			});
		}

		public void TestProcessResponseMessage_NotSuccess()
		{
			CreateImporters();
			var catalog1 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog1.CGC_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			catalog1.CGC_AuthorityIdentifier = "12";
			catalog1.CGC_OH_Owner = importer1.PK;

			var catalog2 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog2.CGC_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			catalog2.CGC_AuthorityIdentifier = "13";
			catalog2.CGC_OH_Owner = importer2.PK;
			var foreignOperator = catalog2.ForeignOperators.AddNew();
			foreignOperator.CGI_Reference = "DE";
			foreignOperator.AuthorityCode = "123";

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(catalog1, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Link, requestMessageSubType: EDIMessageSubTypeList.Codes.Link, interchangeBodyText: GetJsonRequestMessageText()).ResponseMessage;
			responseMessage.EM_MessageText = GetJsonResponseMessageText(false);
			ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_LinkUniqueID", catalog2.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", catalog2.TableName, responseMessage.EM_LinkTable);
				AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessage.EM_Status);
				AssertEquals("foreignOperator CGI_CustomsStatus", CustomsPostedStatusList.Codes.Active, foreignOperator.CGI_CustomsStatus);
				Assert("Log AutoEvents.MessageRejected Created", catalog2.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == AutoEvents.MessageRejected.Code));
				AssertEquals("catalog2 CGC_MessageStatus", BRMessageStatusList.Codes.Rejected, catalog2.CGC_MessageStatus);
			});
		}

		public void TestProcessResponseMessage_NullIncomingInterchange()
		{
			var responseMessage = BRCResponseMessageProcessorTest.CreateMessage(Factory, ZGuid.Empty, null, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Link);
			var logger = ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_LinkUniqueID", ZGuid.Empty, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", ZString.Empty, responseMessage.EM_LinkTable);
				AssertEquals("EM_Status", EDIMessage.Status.Failed, responseMessage.EM_Status);
				AssertContains("Logger", "Unable to find the incoming Interchange", logger.LogMessages.ToString());
			});
		}

		public void TestProcessResponseMessage_NullOutgoingInterchange()
		{
			var responseInterchange = BRCResponseMessageProcessorTest.CreateInterchange(Factory, ZGuid.NewZGuid(), EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Sent, MessageTypeList.Codes.CAT);
			var responseMessage = BRCResponseMessageProcessorTest.CreateMessage(Factory, responseInterchange.PK, null, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Link);
			var logger = ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_LinkUniqueID", ZGuid.Empty, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", ZString.Empty, responseMessage.EM_LinkTable);
				AssertEquals("EM_Status", EDIMessage.Status.Failed, responseMessage.EM_Status);
				AssertContains("Logger", "Unable to find the outgoing Interchange", logger.LogMessages.ToString());
			});
		}

		public void TestProcessResponseMessage_DeserializationFailed()
		{
			CreateImporters();
			var catalog1 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog1.CGC_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			catalog1.CGC_AuthorityIdentifier = "12";
			catalog1.CGC_OH_Owner = importer1.PK;
			catalog1.ForeignOperators.AddNew();
			catalog1.ForeignOperators.AddNew();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(catalog1, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Link, requestMessageSubType: EDIMessageSubTypeList.Codes.Link).ResponseMessage;
			responseMessage.EM_MessageText = "";
			var logger = ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_LinkUniqueID", ZGuid.Empty, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", ZString.Empty, responseMessage.EM_LinkTable);
				AssertEquals("EM_Status", EDIMessage.Status.Failed, responseMessage.EM_Status);
				AssertContains("Logger", "Message #1: Message deserialization was failed.", logger.LogMessages.ToString());
			});
		}

		public void TestProcessResponseMessage_CouldNotFindGoodsCatalog()
		{
			CreateImporters();
			var catalog1 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog1.CGC_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			catalog1.CGC_AuthorityIdentifier = "15";
			catalog1.CGC_OH_Owner = importer2.PK;
			catalog1.ForeignOperators.AddNew();
			catalog1.ForeignOperators.AddNew();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(catalog1, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Link, requestMessageSubType: EDIMessageSubTypeList.Codes.Link, interchangeBodyText: GetJsonRequestMessageText()).ResponseMessage;
			responseMessage.EM_MessageText = GetJsonResponseMessageText();
			var logger = ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_LinkUniqueID", ZGuid.Empty, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", ZString.Empty, responseMessage.EM_LinkTable);
				AssertEquals("EM_Status", EDIMessage.Status.Failed, responseMessage.EM_Status);
				AssertContains("Logger", "Unable to find a Goods Catalog with Authority Identifier '13'", logger.LogMessages.ToString());
			});
		}

		public void TestProcessResponseMessage_CouldNotFindForeignOperator()
		{
			CreateImporters();
			var catalog1 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog1.CGC_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			catalog1.CGC_AuthorityIdentifier = "13";
			catalog1.CGC_OH_Owner = importer2.PK;
			catalog1.ForeignOperators.AddNew();
			catalog1.ForeignOperators.AddNew();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(catalog1, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Link, requestMessageSubType: EDIMessageSubTypeList.Codes.Link, interchangeBodyText: GetJsonRequestMessageText()).ResponseMessage;
			responseMessage.EM_MessageText = GetJsonResponseMessageText();
			var logger = ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessage.Status.Failed, responseMessage.EM_Status);
				AssertContains("Logger", "Unable to find a Production Info with Reference 'DE'", logger.LogMessages.ToString());
			});
		}

		public void TestProcessResponseMessage_CouldNotFindRelatedObjectBySeq()
		{
			CreateImporters();
			var catalog1 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog1.CGC_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			catalog1.CGC_AuthorityIdentifier = "15";
			catalog1.CGC_OH_Owner = importer2.PK;
			catalog1.ForeignOperators.AddNew();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(catalog1, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Link, requestMessageSubType: EDIMessageSubTypeList.Codes.Link, interchangeBodyText: GetJsonRequestMessageText()).ResponseMessage;
			var jsonResponse = GetJsonResponseMessageText().Replace("\"seq\": 2", "\"seq\": 10");
			responseMessage.EM_MessageText = jsonResponse;
			var logger = ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_LinkUniqueID", ZGuid.Empty, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", ZString.Empty, responseMessage.EM_LinkTable);
				AssertEquals("EM_Status", EDIMessage.Status.Failed, responseMessage.EM_Status);
				AssertContains("Logger", "Unable to find the related object with 'seq' tag equal to 10", logger.LogMessages.ToString());
			});
		}

		public void TestProcessResponseMessage_ChangedCGI_CustomsStatus()
		{
			CreateImporters();
			var catalog1 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog1.CGC_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			catalog1.CGC_AuthorityIdentifier = "12";
			catalog1.CGC_OH_Owner = importer1.PK;

			var foreignOperator1 = catalog1.ForeignOperators.AddNew();
			foreignOperator1.CountryCode = "US";
			foreignOperator1.AuthorityCode = "123";
			var foreignOperator2 = catalog1.ForeignOperators.AddNew();
			foreignOperator2.CountryCode = "US";

			var catalog2 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog2.CGC_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			catalog2.CGC_AuthorityIdentifier = "13";
			catalog2.CGC_OH_Owner = importer2.PK;
			var foreignOperator3 = catalog2.ForeignOperators.AddNew();
			foreignOperator3.CountryCode = "DE";
			foreignOperator3.AuthorityCode = "123";
			var foreignOperator4 = catalog2.ForeignOperators.AddNew();
			foreignOperator4.CountryCode = "DE";

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(catalog1, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Link, requestMessageSubType: EDIMessageSubTypeList.Codes.Link, interchangeBodyText: GetJsonRequestMessageText()).ResponseMessage;
			responseMessage.EM_MessageText = GetJsonResponseMessageText();
			ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("foreignOperator1.CGI_CustomsStatus", CustomsPostedStatusList.Codes.Active, foreignOperator1.CGI_CustomsStatus);
				AssertEquals("foreignOperator2.CGI_CustomsStatus", CustomsPostedStatusList.Codes.Active, foreignOperator2.CGI_CustomsStatus);
				AssertEquals("foreignOperator3.CGI_CustomsStatus", CustomsPostedStatusList.Codes.Accepted, foreignOperator3.CGI_CustomsStatus);
				AssertEquals("foreignOperator4.CGI_CustomsStatus", CustomsPostedStatusList.Codes.Active, foreignOperator4.CGI_CustomsStatus);
				Assert("Log AutoEvents.MessageRejected Created", catalog2.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == AutoEvents.MessageRejected.Code));
				AssertEquals("catalog2 CGC_MessageStatus", BRMessageStatusList.Codes.Rejected, catalog2.CGC_MessageStatus);
			});
		}

		public void TestProcessResponseMessage_RemoveForeignOperator()
		{
			CreateImporters();
			var catalog1 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog1.CGC_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			catalog1.CGC_AuthorityIdentifier = "12";
			catalog1.CGC_OH_Owner = importer1.PK;

			var foreignOperator1 = catalog1.ForeignOperators.AddNew();
			foreignOperator1.CountryCode = "US";
			foreignOperator1.AuthorityCode = "123";
			var foreignOperator2 = catalog1.ForeignOperators.AddNew();
			foreignOperator2.CountryCode = "US";

			var catalog2 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog2.CGC_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			catalog2.CGC_AuthorityIdentifier = "13";
			catalog2.CGC_OH_Owner = importer2.PK;
			var foreignOperator3 = catalog2.ForeignOperators.AddNew();
			foreignOperator3.CountryCode = "DE";
			foreignOperator3.AuthorityCode = "123";
			var foreignOperator4 = catalog2.ForeignOperators.AddNew();
			foreignOperator4.CountryCode = "DE";

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(catalog1, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Link, requestMessageSubType: EDIMessageSubTypeList.Codes.Link, interchangeBodyText: GetJsonRequestMessageText(false)).ResponseMessage;
			responseMessage.EM_MessageText = GetJsonResponseMessageText();
			var logger = ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("foreignOperator1 deleted", false, foreignOperator1.IsDeleted);
				AssertEquals("foreignOperator2 deleted", false, foreignOperator2.IsDeleted);
				AssertEquals("foreignOperator3 deleted", true, foreignOperator3.IsDeleted);
				AssertEquals("foreignOperator4 deleted", false, foreignOperator4.IsDeleted);

				AssertEquals("ForeignOperators", 2, catalog1.ForeignOperators.Count);
				AssertEquals("ForeignOperators", 1, catalog2.ForeignOperators.Count);
				Assert("Log AutoEvents.MessageRejected Created", catalog2.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == AutoEvents.MessageRejected.Code));
				AssertEquals("catalog2 CGC_MessageStatus", BRMessageStatusList.Codes.Rejected, catalog2.CGC_MessageStatus);
			});
		}

		void CreateImporters()
		{
			var brazil = Factory.Load<RefCountry>(Core.Constants.CountryGuids.Brazil);
			importer1 = new MasterFilesTestHelper(Factory).CreateOrganisation("TEST ORG", "CATOR", "STREET 1", "CITY", "0987654321");
			importer1.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "22.955.307/0001-89", brazil);
			importer1.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ, "22955307", brazil);

			importer2 = new MasterFilesTestHelper(Factory).CreateOrganisation("TEST ORG 2", "CATOR", "STREET 2", "CITY", "0987654321");
			importer2.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "33.775.353/0001-12", brazil);
			importer2.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ, "33775353", brazil);
			Factory.Save();
		}

		OrgHeader importer1;
		OrgHeader importer2;

		string GetJsonResponseMessageText(bool success = true) => @"{ 
    ""seq"": 2, 
    ""codigo"": ""123456789"", 
    ""erros"": [ 
    ], 
    ""sucesso"": " + (success ? "true" : "false") + @", 
    ""versao"": null 
  }";

		string GetJsonRequestMessageText(bool isLinked = true) => @"[
  {
    ""seq"": 1,
    ""cpfCnpjRaiz"": ""22955307"",
    ""codigoOperadorEstrangeiro"": ""456"",
    ""cpfCnpjFabricante"": ""25043512"",
    ""conhecido"": false,
    ""codigoProduto"": 12,
    ""vincular"": " + (isLinked ? "true" : "false") + @",
    ""dataReferencia"": ""21-06-2016"",
    ""codigoPais"": ""US""
  },
  {
    ""seq"": 2,
    ""cpfCnpjRaiz"": ""33775353"",
    ""codigoOperadorEstrangeiro"": ""123"",
    ""cpfCnpjFabricante"": ""25043512"",
    ""conhecido"": true,
    ""codigoProduto"": 13,
    ""vincular"": " + (isLinked ? "true" : "false") + @",
    ""dataReferencia"": ""21-06-2016"",
    ""codigoPais"": ""DE""
  }
]";
	}
}
