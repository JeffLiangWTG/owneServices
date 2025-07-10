using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCForeignOperatorSuccessResponseMessageProcessorTest : BRCResponseMessageProcessorAbstractTest<BRCForeignOperatorSuccessResponseMessageProcessor>
	{
		protected override IReadOnlyList<string> MessageFilterTypes => new[] { "OPE" };

		protected override IReadOnlyList<string> MessageFilterSubTypes => new[] { "SUC" };

		public void TestProcessResponseMessageWithoutSeq()
		{
			var foreignOperator = CreateForeignOperator(Factory);
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(foreignOperator, MessageTypeList.Codes.OPE, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessageResponseWithoutSeq;
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
				AssertContains("Logger", $"Message #1: Message deserialization was failed or tag 'seq' not found or is empty.", logger.LogMessages.ToString());

				AssertEquals("BFR_AuthorityIdentifier not changed", "1", foreignOperator.BFR_AuthorityIdentifier);
				AssertEquals("BFR_AuthorityVersion not changed", "2", foreignOperator.BFR_AuthorityVersion);
				AssertEquals("BFR_MessageStatus", ZString.Empty, foreignOperator.BFR_MessageStatus);
				AssertEquals("UpdateMessageStatusOnSaving NOT Suspended", false, foreignOperator.IsUpdateMessageStatusOnSavingSuspended);
			});
		}

		public void TestProcessResponseMessageNullSeq()
		{
			var foreignOperator = CreateForeignOperator(Factory);
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(foreignOperator, MessageTypeList.Codes.OPE, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessageResponseNullSeq;
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
				AssertContains("Logger", "Message #1: Message deserialization was failed or tag 'seq' not found or is empty.", logger.LogMessages.ToString());

				AssertEquals("BFR_AuthorityIdentifier not changed", "1", foreignOperator.BFR_AuthorityIdentifier);
				AssertEquals("BFR_AuthorityVersion not changed", "2", foreignOperator.BFR_AuthorityVersion);
				AssertEquals("BFR_MessageStatus", ZString.Empty, foreignOperator.BFR_MessageStatus);
				AssertEquals("UpdateMessageStatusOnSaving NOT Suspended", false, foreignOperator.IsUpdateMessageStatusOnSavingSuspended);
			});
		}

		public void TestProcessResponseMessageInvalidJson()
		{
			var foreignOperator = CreateForeignOperator(Factory);
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(foreignOperator, MessageTypeList.Codes.OPE, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = @"[]";

			CombineAssertions(() =>
			{
				AssertExceptionThrown<JsonException>(() => ExecuteMessageProcessor(responseMessage));
				AssertEquals("BFR_AuthorityIdentifier not changed", "1", foreignOperator.BFR_AuthorityIdentifier);
				AssertEquals("BFR_AuthorityVersion not changed", "2", foreignOperator.BFR_AuthorityVersion);
				AssertEquals("BFR_MessageStatus", ZString.Empty, foreignOperator.BFR_MessageStatus);
				AssertEquals("UpdateMessageStatusOnSaving NOT Suspended", false, foreignOperator.IsUpdateMessageStatusOnSavingSuspended);
			});
		}

		public void TestProcessResponseMessageWithoutSequenceMatch()
		{
			var foreignOperator = CreateForeignOperator(Factory);
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(foreignOperator, MessageTypeList.Codes.OPE, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessageResponse.Replace("\"seq\": 1", "\"seq\": 5");
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
				AssertContains("Logger", "Unable to locate the related Business Object for OPE message #1", logger.LogMessages.ToString());

				AssertEquals("BFR_AuthorityIdentifier not changed", "1", foreignOperator.BFR_AuthorityIdentifier);
				AssertEquals("BFR_AuthorityVersion not changed", "2", foreignOperator.BFR_AuthorityVersion);
				AssertEquals("BFR_MessageStatus", ZString.Empty, foreignOperator.BFR_MessageStatus);
				AssertEquals("UpdateMessageStatusOnSaving NOT Suspended", false, foreignOperator.IsUpdateMessageStatusOnSavingSuspended);
			});
		}

		public void TestProcessMessage_WithoutErrors()
		{
			var foreignOperator = CreateForeignOperator(Factory);
			foreignOperator.Logs.AddNew(Events.MessageSent, ActionList.Codes.Activate);
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(foreignOperator, MessageTypeList.Codes.OPE, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessageResponse;
			Factory.Save();

			AssertMessageProcessed(foreignOperator, responseMessage, hasError: false, ForeignOperatorCustomsStatusTypeList.Codes.Active);
		}

		public void TestProcessMessage_WithErrors()
		{
			var foreignOperator = CreateForeignOperator(Factory);
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(foreignOperator, MessageTypeList.Codes.OPE, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessageResponseWithError;
			Factory.Save();

			AssertMessageProcessed(foreignOperator, responseMessage, hasError: true);
		}

		[TestDate(2025, 02, 28, 18, 00, 00)]
		public void TestProcessMessage_CustomsStatusSet()
		{
			var foreignOperator = CreateForeignOperator(Factory);
			foreignOperator.Logs.AddNew(Events.MessageSent, ActionList.Codes.Activate, ZDateTimeOffset.Now.AddDays(-2));
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(foreignOperator, MessageTypeList.Codes.OPE, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessageResponse;
			
			Factory.Save();
			AssertMessageProcessed(foreignOperator, responseMessage, hasError: false, ForeignOperatorCustomsStatusTypeList.Codes.Active);

			foreignOperator.Logs.AddNew(Events.MessageSent, ActionList.Codes.CreateNewVersion, ZDateTimeOffset.Now.AddDays(-1));
			Factory.Save();
			AssertMessageProcessed(foreignOperator, responseMessage,hasError: false, ForeignOperatorCustomsStatusTypeList.Codes.Active);

			foreignOperator.Logs.AddNew(Events.MessageSent, ActionList.Codes.Deactivate, ZDateTimeOffset.Now);
			Factory.Save();
			AssertMessageProcessed(foreignOperator, responseMessage, hasError: false, ForeignOperatorCustomsStatusTypeList.Codes.Inactive);
		}

		void AssertMessageProcessed(CusBRForeignOperator foreignOperator, BREDIMessage responseMessage, bool hasError, string customStatus = null)
		{
			ExecuteMessageProcessor(responseMessage);
			AssertEquals("UpdateMessageStatusOnSaving Suspended", true, foreignOperator.IsUpdateMessageStatusOnSavingSuspended);
			Factory.Save();
			CombineAssertions(() =>
			{
				var message = foreignOperator.Messages.LastIncomingMessage;
				AssertEquals("EM_LinkTable", "CusBRForeignOperator", message.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", foreignOperator.PK, message.EM_LinkUniqueID);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.OPE, message.EM_MessageType);
				AssertEquals("EM_MessageSubType", EDIMessageSubTypeList.Codes.Success, message.EM_MessageSubType);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, message.EM_Status);
				AssertEquals("UpdateMessageStatusOnSaving NOT Suspended", false, foreignOperator.IsUpdateMessageStatusOnSavingSuspended);

				if (hasError)
				{
					Assert("Log AutoEvents.MessageRejected Created", foreignOperator.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == AutoEvents.MessageRejected.Code));
					AssertEquals("BFR_MessageStatus should be Rejected", BRMessageStatusList.Codes.Rejected, foreignOperator.BFR_MessageStatus);
				}
				else
				{
					AssertEquals("BFR_MessageStatus should be Accepted", BRMessageStatusList.Codes.Accepted, foreignOperator.BFR_MessageStatus);
					AssertEquals("BFR_AuthorityIdentifier should be Accepted", "123456789", foreignOperator.BFR_AuthorityIdentifier);
					AssertEquals("BFR_AuthorityVersion should be Accepted", "1", foreignOperator.BFR_AuthorityVersion);
					AssertEquals("BFR_CustomsStatus should be Accepted", customStatus, foreignOperator.BFR_CustomsStatus);
				}
			});
		}

		public static CusBRForeignOperator CreateForeignOperator(BusinessObjectFactory factory)
		{
			var brazil = factory.Load<RefCountry>(Core.Constants.CountryGuids.Brazil);
			var owner = new MasterFilesTestHelper(factory).CreateOrganisation("TEST ORG", "CATOR", "STREET 1", "CITY", "0987654321");
			owner.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "22.955.307/0001-89", brazil);
			owner.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ, "22955307", brazil);
			var foreign = new MasterFilesTestHelper(factory).CreateOrganisation("TEST ORG 2", "CATOR", "STREET 2", "CITY", "0987654321");
			foreign.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "33.775.353/0001-12", brazil);
			foreign.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ, "33775353", brazil);

			var foreignOperator = factory.NewWithValidTestData<CusBRForeignOperator>();
			foreignOperator.BFR_OH_Owner = owner.PK;
			foreignOperator.BFR_AuthorityIdentifier = "1";
			foreignOperator.BFR_AuthorityVersion = "2";
			foreignOperator.BFR_OH_ForeignOperator = foreign.PK;

			return foreignOperator;
		}

		const string JsonMessageResponse = @"{ 
    ""seq"": 1, 
    ""codigo"": ""123456789"", 
    ""erros"": [ 
    ], 
    ""sucesso"": true, 
    ""versao"": ""1""  
  }";

		const string JsonMessageResponseWithoutSeq = @"{
    ""codigo"": ""123456789"", 
    ""erros"": [ 
    ], 
    ""sucesso"": true, 
    ""versao"": null 
  }";

		const string JsonMessageResponseNullSeq = @"{ 
    ""seq"": null, 
    ""codigo"": ""123456789"", 
    ""erros"": [ 
    ], 
    ""sucesso"": true, 
    ""versao"": null 
  }";

		internal const string JsonMessageResponseWithError = @"{ 
    ""seq"": 1, 
    ""codigo"": ""123456789"", 
    ""erros"": [ ""string"" ],
    ""sucesso"": false, 
    ""versao"": null 
  }";
	}
}
