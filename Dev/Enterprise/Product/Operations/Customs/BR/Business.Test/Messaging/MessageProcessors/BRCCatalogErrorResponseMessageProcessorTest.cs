using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business.Testing
{
	sealed class BRCCatalogErrorResponseMessageProcessorTest : BRCResponseMessageProcessorAbstractTest<BRCCatalogErrorResponseMessageProcessor>
	{
		protected override IReadOnlyList<string> MessageFilterTypes => new[] { "CAT" };

		protected override IReadOnlyList<string> MessageFilterSubTypes => new[] { "ERR" };

		public void TestProcessMessage_SingleOutgoingMessage()
		{
			var goodsCatalog = BRCCatalogSuccessResponseMessageProcessorTest.CreateGoodsCatalog(Factory);
			var (requestMessages, responseMessage) = BRCResponseMessageProcessorTest.CreateResponseMessage(goodsCatalog, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Error);
			responseMessage.EM_MessageText = ResponseMessageText;
			Factory.Save();
			ExecuteMessageProcessor(responseMessage);
			AssertEquals("UpdateCustomStatusOnSaving Suspended", true, goodsCatalog.IsUpdateCustomStatusOnSavingSuspended);

			Factory.Save();
			AssertMessageProcessed(goodsCatalog, ResponseMessageText);
		}

		public void TestProcessMessage_MultipleOutgoingMessages()
		{
			var goodsCatalog1 = BRCCatalogSuccessResponseMessageProcessorTest.CreateGoodsCatalog(Factory);
			var goodsCatalog2 = BRCCatalogSuccessResponseMessageProcessorTest.CreateGoodsCatalog(Factory);
			var goodsCatalog3 = BRCCatalogSuccessResponseMessageProcessorTest.CreateGoodsCatalog(Factory);
			var (requestMessages, responseMessage) = BRCResponseMessageProcessorTest.CreateMultipleMessagesAndInterchange(new [] { goodsCatalog1, goodsCatalog2, goodsCatalog3 }, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Error);
			responseMessage.EM_MessageText = ResponseMessageText;
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			Factory.Save();
			var responseMessages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive).AddToFilter(EDIMessageSchema.EM_LinkTable, CusGoodsCatalog.Schema.TableName));
			AssertEquals(3, responseMessages.Length);
			AssertNotNull(responseMessages.Single(x => x.PK == responseMessage.PK));

			AssertMessageProcessed(goodsCatalog1, ResponseMessageText);
			AssertMessageProcessed(goodsCatalog2, ResponseMessageText);
			AssertMessageProcessed(goodsCatalog3, ResponseMessageText);
		}

		public void TestProcessMessage_NoCatalogFound()
		{
			var responseMessage = BRCResponseMessageProcessorTest.CreateMessage(Factory, Guid.Empty, null, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Error);
			responseMessage.EM_MessageText = ResponseMessageText;
			var logger = ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
				AssertContains("Log Message", "Unable to locate the related Business Object for CAT message #1", logger.LogMessages.ToString());
			});
		}

		void AssertMessageProcessed(CusGoodsCatalog catalog,string expectedMessageText)
		{
			CombineAssertions(() =>
			{
				var message = catalog.Messages.LastIncomingMessage;
				AssertEquals("EM_LinkTable", "CusGoodsCatalog", message.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", catalog.PK, message.EM_LinkUniqueID);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.CAT, message.EM_MessageType);
				AssertEquals("EM_MessageSubType", EDIMessageSubTypeList.Codes.Error, message.EM_MessageSubType);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, message.EM_Status);
				AssertEquals("EM_MessageText", expectedMessageText, message.EM_MessageText);
				AssertEquals("EM_GB", catalog.Company.FirstActiveBranch.PK, message.EM_GB);

				Assert("Log AutoEvents.MessageRejected Created", catalog.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == AutoEvents.MessageRejected.Code));
				AssertEquals("CGC_MessageStatus", EDIMessageStatusList.Codes.Rejected, catalog.CGC_MessageStatus);
				AssertEquals("UpdateCustomStatusOnSaving not Suspended", false, catalog.IsUpdateCustomStatusOnSavingSuspended);
			});
		}

		string ResponseMessageText => "{\"message\": \"O usuário logado não é representante legal do CPF/CNPJ Raiz.\"}";
	}
}
