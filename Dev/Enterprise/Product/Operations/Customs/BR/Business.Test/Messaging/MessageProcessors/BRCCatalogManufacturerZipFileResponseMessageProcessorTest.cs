using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCCatalogManufacturerZipFileResponseMessageProcessorTest : BRCResponseMessageProcessorAbstractTest<BRCCatalogManufacturerZipFileResponseMessageProcessor>
	{
		protected override IReadOnlyList<string> MessageFilterTypes => new[] { "CAT" };

		protected override IReadOnlyList<string> MessageFilterSubTypes => new[] { "MZI" };

		public void TestProcessResponseMessageInvalidJson()
		{
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(Factory.New<OrgHeader>(), MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.ManufacturerZipFile, requestMessageSubType: EDIMessageSubTypeList.Codes.ManufacturerZipFile).ResponseMessage;
			responseMessage.EM_MessageText = @"[]";
			AssertExceptionThrown<JsonException>(() => ExecuteMessageProcessor(responseMessage));
		}

		public void TestProcessResponseMessage_InvalidResponse()
		{
			var message = JsonMessageWhenKnowledgeFalse.Replace("\"codigoProduto\": 123,", "");
			AssertMessageWithoutTag(message);

			message = JsonMessageWhenKnowledgeFalse.Replace("\"cpfCnpjRaiz\": \"75400331\",", "");
			AssertMessageWithoutTag(message);

			message = JsonMessageWhenKnowledgeFalse.Replace(",\r\n    \"codigoPais\": \"PE\"", "");
			AssertMessageWithoutTag(message);

			void AssertMessageWithoutTag(string message)
			{
				var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(Factory.New<OrgHeader>(), MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.ManufacturerZipFile, requestMessageSubType: EDIMessageSubTypeList.Codes.ManufacturerZipFile).ResponseMessage;
				responseMessage.EM_MessageText = message;

				var logger = ExecuteMessageProcessor(responseMessage);
				CombineAssertions(() =>
				{
					AssertEquals("EM_LinkTable", ZString.Empty, responseMessage.EM_LinkTable);
					AssertEquals("EM_LinkUniqueID", ZGuid.Empty, responseMessage.EM_LinkUniqueID);
					AssertEquals("EM_MessageSubType", EDIMessageSubTypeList.Codes.ManufacturerZipFile, responseMessage.EM_MessageSubType);
					AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
					AssertContains("Logger", $"Message #1: Message deserialization was failed or tag 'codigoProduto', 'codigoPais'  or 'cpfCnpjRaiz' not found.", logger.LogMessages.ToString());
				});
			}
		}

		public void TestProcessResponseMessageSuccess_UpdateForeignOperators()
		{
			var owner = CreateOwner();
			var goodsCatalog = CreateGoodsCatalog(owner);
			var productionInfos = goodsCatalog.ForeignOperators.AddNew();
			productionInfos.CountryCode = "PE";
			productionInfos.AuthorityCode = "123";

			Factory.Save();

			AssertReceivedMessage(JsonMessageWhenKnowledgeTrue, ("PE", "123", "ACT"));
			AssertReceivedMessage(JsonMessageWhenKnowledgeTrue, ("PE", "123", "ACT"));
			AssertReceivedMessage(JsonMessageWhenKnowledgeFalse, ("PE", "123", "ACT"), ("PE", "", "ACC"));
			AssertReceivedMessage(JsonMessageWhenKnowledgeFalse, ("PE", "123", "ACT"), ("PE", "", "ACC"));
			AssertReceivedMessage(JsonMessageWhenKnowledgeTrue.Replace("PE", "US"), ("PE", "123", "ACT"), ("PE", "", "ACC"), ("US", "123", "ACC"));
			AssertReceivedMessage(JsonMessageWhenKnowledgeFalse.Replace("PE", "US"), ("PE", "123", "ACT"), ("PE", "", "ACC"), ("US", "123", "ACC"), ("US", "", "ACC"));

			void AssertReceivedMessage(string message, params (ZString, ZString, ZString)[] expectedProductions)
			{
				var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(owner, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.ManufacturerZipFile, requestMessageSubType: EDIMessageSubTypeList.Codes.ManufacturerZipFile).ResponseMessage;
				responseMessage.EM_MessageText = message;

				var logger = ExecuteMessageProcessor(responseMessage);
				AssertEquals("UpdateCustomStatusOnSaving Suspended", true, goodsCatalog.IsUpdateCustomStatusOnSavingSuspended);

				Factory.Save();
				CombineAssertions(() =>
				{
					AssertEquals("EM_GB", goodsCatalog.Company.FirstActiveBranch.PK, responseMessage.EM_GB);
					AssertEquals("EM_LinkTable", goodsCatalog.TableName, responseMessage.EM_LinkTable);
					AssertEquals("EM_LinkUniqueID", goodsCatalog.PK, responseMessage.EM_LinkUniqueID);
					AssertEquals("EM_MessageType", MessageTypeList.Codes.CAT, responseMessage.EM_MessageType);
					AssertEquals("EM_MessageSubType", EDIMessageSubTypeList.Codes.ManufacturerZipFile, responseMessage.EM_MessageSubType);
					AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, responseMessage.EM_Status);
					AssertEquals("UpdateCustomStatusOnSaving not Suspended", false, goodsCatalog.IsUpdateCustomStatusOnSavingSuspended);
					AssertContainsExactElementsInAnyOrder("ForeignOperators.CGI_Reference", expectedProductions, goodsCatalog.ForeignOperators.Select(x => (x.CountryCode, x.AuthorityCode, x.CGI_CustomsStatus)));
				});
			}
		}

		public void TestProcessResponseMessageSuccess_LinkProductionInfoToForeignOperator()
		{
			var owner = CreateOwner();
			var goodsCatalog = CreateGoodsCatalog(owner);
			var foreignOperator = CreateForeignOperator(owner.PK);

			Factory.Save();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(owner, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.ManufacturerZipFile, requestMessageSubType: EDIMessageSubTypeList.Codes.ManufacturerZipFile).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessageWhenKnowledgeTrue;

			var logger = ExecuteMessageProcessor(responseMessage);
			AssertEquals("UpdateCustomStatusOnSaving Suspended", true, goodsCatalog.IsUpdateCustomStatusOnSavingSuspended);

			Factory.Save();
			CombineAssertions(() =>
			{
				var foreignOperatorProductionInfo = goodsCatalog.ForeignOperators.FirstOrDefault();
				AssertEquals("EM_GB", goodsCatalog.Company.FirstActiveBranch.PK, responseMessage.EM_GB);
				AssertEquals("EM_LinkTable", goodsCatalog.TableName, responseMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", goodsCatalog.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.CAT, responseMessage.EM_MessageType);
				AssertEquals("EM_MessageSubType", EDIMessageSubTypeList.Codes.ManufacturerZipFile, responseMessage.EM_MessageSubType);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, responseMessage.EM_Status);
				AssertEquals("UpdateCustomStatusOnSaving not Suspended", false, goodsCatalog.IsUpdateCustomStatusOnSavingSuspended);
				AssertEquals("ForeignOperators", 1, goodsCatalog.ForeignOperators.Count);
				AssertEquals("ForeignOperators.CGI_BFR_ForeignOperator", foreignOperator.PK, foreignOperatorProductionInfo.CGI_BFR_ForeignOperator);
				using (Registry.BRCustomsDataRegistry.Instance.EnableForeignOperator.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("ForeignOperators.AuthorityCode", "123", foreignOperatorProductionInfo.AuthorityCode);
					AssertEquals("ForeignOperators.CountryCode", Core.Constants.CountryCodes.Peru, foreignOperatorProductionInfo.CountryCode);
				}
			});
		}

		[TestDate(2024, 05, 05, 10, 10, 0)]
		public void TestProcessResponseMessageSuccess_NoMatchCatalog()
		{
			var owner = CreateOwner();
			Factory.Save();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(owner, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.ManufacturerZipFile, requestMessageSubType: EDIMessageSubTypeList.Codes.ManufacturerZipFile).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessageWhenKnowledgeFalse;
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_LinkTable", ZString.Empty, responseMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", ZGuid.Empty, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.CAT, responseMessage.EM_MessageType);
				AssertEquals("EM_MessageSubType", EDIMessageSubTypeList.Codes.ManufacturerZipFile, responseMessage.EM_MessageSubType);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
				AssertContains("Logger", "Message #1: No matching Catalog was found with Owner Root CNPJ '75400331' and Lookup Code '123'", logger.LogMessages.ToString());
			});
		}

		public void TestProcessResponseMessageSuccess_CZPNotProcessed()
		{
			var owner = CreateOwner();
			var goodsCatalog = CreateGoodsCatalog(owner);
			Factory.Save();

			var responseCZP = BRCResponseMessageProcessorTest.CreateResponseMessage(goodsCatalog, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.CatalogZipFile, requestMessageSubType: EDIMessageSubTypeList.Codes.CatalogZipFile).ResponseMessage;
			var responseMZI = BRCResponseMessageProcessorTest.CreateResponseMessage(goodsCatalog, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.ManufacturerZipFile, requestMessageSubType: EDIMessageSubTypeList.Codes.ManufacturerZipFile).ResponseMessage;
			responseMZI.EM_MessageText = JsonMessageWhenKnowledgeTrue;
			Factory.Save();

			CombineAssertions("CZP not processed", () =>
			{
				AssertExceptionThrown<MessageProcessLockException>(() =>
				{
					var logger = ExecuteMessageProcessor(responseMZI);
					AssertContains("Logger", "Message #1 postponed, the response for outgoing Catalog Zip File or Operator Zip File wasn't processed yet.", logger.LogMessages.ToString());
				});
				AssertEquals("EM_LinkTable", ZString.Empty, responseMZI.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", ZGuid.Empty, responseMZI.EM_LinkUniqueID);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Queued, responseMZI.EM_Status);
			});

			responseCZP.EM_Status = EDIMessageStatusList.Codes.Received;
			Factory.Save();
			var logger = ExecuteMessageProcessor(responseMZI);
			CombineAssertions("CZP has been processed", () =>
			{
				AssertEquals("EM_LinkTable", goodsCatalog.TableName, responseMZI.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", goodsCatalog.PK, responseMZI.EM_LinkUniqueID);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, responseMZI.EM_Status);
				AssertEquals("ForeignOperators updated", 1, goodsCatalog.ForeignOperators.Count);
			});
		}

		[TestDate(2024, 05, 05, 10, 10, 0)]
		public void TestProcessResponseMessageSuccess_CZPAndOZINotProcessed()
		{
			var owner = CreateOwner();
			var goodsCatalog = CreateGoodsCatalog(owner);
			Factory.Save();

			var responseOZI = BRCResponseMessageProcessorTest.CreateResponseMessage(goodsCatalog, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.OperatorZipFile, requestMessageSubType: EDIMessageSubTypeList.Codes.OperatorZipFile).ResponseMessage;
			responseOZI.EM_HeldUntilDate = ZDateTime.Now.AddMinutes(5);
			var responseCZP = BRCResponseMessageProcessorTest.CreateResponseMessage(goodsCatalog, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.CatalogZipFile, requestMessageSubType: EDIMessageSubTypeList.Codes.CatalogZipFile).ResponseMessage;
			responseCZP.EM_HeldUntilDate = ZDateTime.Now.AddMinutes(5);
			var responseMZI = BRCResponseMessageProcessorTest.CreateResponseMessage(goodsCatalog, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.ManufacturerZipFile, requestMessageSubType: EDIMessageSubTypeList.Codes.ManufacturerZipFile).ResponseMessage;
			responseMZI.EM_MessageText = JsonMessageWhenKnowledgeTrue;
			Factory.Save();

			CombineAssertions("CZP & OZI not processed", () =>
			{
				var logger = ExecuteMessageProcessor();
				AssertEquals("EM_LinkTable", ZString.Empty, responseMZI.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", ZGuid.Empty, responseMZI.EM_LinkUniqueID);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Queued, responseMZI.EM_Status);
				AssertEquals("EM_HeldUntilDate should be updated to a future time", new ZDateTime(2024, 05, 05, 10, 12, 0), responseMZI.EM_HeldUntilDate);
				AssertContains("Logger", "Message #1 postponed, the response for outgoing Catalog Zip File or Operator Zip File wasn't processed yet.", logger.LogMessages.ToString());
			});

			responseCZP.EM_Status = EDIMessageStatusList.Codes.Received;
			Factory.Save();
			CombineAssertions("CZP processed but OZI not processed", () =>
			{
				TestDateAttribute.AddMinutes(3);
				var logger = ExecuteMessageProcessor();
				AssertEquals("EM_LinkTable", ZString.Empty, responseMZI.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", ZGuid.Empty, responseMZI.EM_LinkUniqueID);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Queued, responseMZI.EM_Status);
				AssertEquals("EM_HeldUntilDate should be updated to a future time", new ZDateTime(2024, 05, 05, 10, 15, 0), responseMZI.EM_HeldUntilDate);
				AssertContains("Logger", "Message #1 postponed, the response for outgoing Catalog Zip File or Operator Zip File wasn't processed yet.", logger.LogMessages.ToString());
			});

			responseOZI.EM_Status = EDIMessageStatusList.Codes.Received;
			Factory.Save();
			CombineAssertions("CZP & OZI all been processed", () =>
			{
				TestDateAttribute.AddMinutes(3);
				ExecuteMessageProcessor();
				AssertEquals("EM_LinkTable", goodsCatalog.TableName, responseMZI.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", goodsCatalog.PK, responseMZI.EM_LinkUniqueID);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, responseMZI.EM_Status);
				AssertEquals("ForeignOperators updated", 1, goodsCatalog.ForeignOperators.Count);
			});

			LoggingInformationForTesting ExecuteMessageProcessor()
			{
				var logger = new LoggingInformationForTesting();
				new BRCIncomingMessageProcessor(logger).ExecuteBatch();
				responseMZI.Reload();
				goodsCatalog.ForeignOperators.RefreshFromDb();
				return logger;
			}
		}

		CusGoodsCatalog CreateGoodsCatalog(OrgHeader owner)
		{
			var goodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			goodsCatalog.CGC_CatalogCode = "C123";
			goodsCatalog.CGC_AuthorityVersion = "9";
			goodsCatalog.CGC_AuthorityIdentifier = "123";
			goodsCatalog.CGC_OH_Owner = owner.PK;
			return goodsCatalog;
		}

		OrgHeader CreateOwner()
		{
			var brazil = Factory.Load<RefCountry>(Core.Constants.CountryGuids.Brazil);
			var owner = new MasterFilesTestHelper(Factory).CreateOrganisation("TEST ORG", "CATOR", "STREET 1", "CITY", "0987654321");
			owner.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "75.400.331/0001-15", Core.Constants.CountryCodes.Brazil);
			owner.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ, "75400331", Core.Constants.CountryCodes.Brazil);
			return owner;
		}

		CusBRForeignOperator CreateForeignOperator(ZGuid ownerPK)
		{
			var cusBRForeignOperator = Factory.NewWithValidTestData<CusBRForeignOperator>();
			cusBRForeignOperator.BFR_AuthorityIdentifier = "123";
			cusBRForeignOperator.BFR_AuthorityVersion = "9";
			cusBRForeignOperator.BFR_OH_Owner = ownerPK;
			cusBRForeignOperator.BFR_RN_NKCountryCode = Core.Constants.CountryCodes.Peru;
			return cusBRForeignOperator;
		}

		const string JsonMessageWhenKnowledgeTrue = @"{
    ""seq"": 10,
    ""cpfCnpjRaiz"": ""75400331"",
    ""codigoOperadorEstrangeiro"": ""123"",
    ""cpfCnpjFabricante"": ""75400331000115"",
    ""conhecido"": true,
    ""codigoProduto"": 123,
    ""vincular"": true,
    ""dataReferencia"": ""2020-07-20"",
    ""codigoPais"": ""PE""
  }";

		const string JsonMessageWhenKnowledgeFalse = @"{
    ""seq"": 10,
    ""cpfCnpjRaiz"": ""75400331"",
    ""cpfCnpjFabricante"": ""75400331000115"",
    ""conhecido"": false,
    ""codigoProduto"": 123,
    ""vincular"": true,
    ""dataReferencia"": ""2020-07-20"",
    ""codigoPais"": ""PE""
  }";
	}
}
