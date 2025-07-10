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
	public class BRCForeignOperatorErrorResponseMessageProcessorTest : BRCResponseMessageProcessorAbstractTest<BRCForeignOperatorErrorResponseMessageProcessor>
	{
		protected override IReadOnlyList<string> MessageFilterTypes => new[] { "OPE" };

		protected override IReadOnlyList<string> MessageFilterSubTypes => new[] { "ERR" };

		public void TestProcessMessage_SingleOutgoingMessage()
		{
			var foreignOperator = BRCForeignOperatorSuccessResponseMessageProcessorTest.CreateForeignOperator(Factory);
			var (requestMessages, responseMessage) = BRCResponseMessageProcessorTest.CreateResponseMessage(foreignOperator, MessageTypeList.Codes.OPE, EDIMessageSubTypeList.Codes.Error);
			responseMessage.EM_MessageText = ResponseMessageText;
			Factory.Save();
			ExecuteMessageProcessor(responseMessage);
			Assert("UpdateMessageStatusOnSaving Suspended", foreignOperator.IsUpdateMessageStatusOnSavingSuspended);
			Factory.Save();
			AssertMessageProcessed(foreignOperator, ResponseMessageText);
		}

		public void TestProcessMessage_MultipleOutgoingMessages()
		{
			var foreignOperator1 = BRCForeignOperatorSuccessResponseMessageProcessorTest.CreateForeignOperator(Factory);
			var foreignOperator2 = BRCForeignOperatorSuccessResponseMessageProcessorTest.CreateForeignOperator(Factory);
			var foreignOperator3 = BRCForeignOperatorSuccessResponseMessageProcessorTest.CreateForeignOperator(Factory);
			var (requestMessages, responseMessage) = BRCResponseMessageProcessorTest.CreateMultipleMessagesAndInterchange(new[] { foreignOperator1, foreignOperator2, foreignOperator3 }, MessageTypeList.Codes.OPE, EDIMessageSubTypeList.Codes.Error);
			responseMessage.EM_MessageText = ResponseMessageText;
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				Assert("ForeignOperator 1 UpdateMessageStatusOnSaving Suspended", foreignOperator1.IsUpdateMessageStatusOnSavingSuspended);
				Assert("ForeignOperator 2 UpdateMessageStatusOnSaving Suspended", foreignOperator2.IsUpdateMessageStatusOnSavingSuspended);
				Assert("ForeignOperator 3 UpdateMessageStatusOnSaving Suspended", foreignOperator3.IsUpdateMessageStatusOnSavingSuspended);
			});
			Factory.Save();
			var responseMessages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive).AddToFilter(EDIMessageSchema.EM_LinkTable, CusBRForeignOperator.Schema.TableName));
			AssertEquals(3, responseMessages.Length);
			AssertNotNull(responseMessages.Single(x => x.PK == responseMessage.PK));

			AssertMessageProcessed(foreignOperator1, ResponseMessageText);
			AssertMessageProcessed(foreignOperator2, ResponseMessageText);
			AssertMessageProcessed(foreignOperator3, ResponseMessageText);
		}

		public void TestProcessMessage_NoForeignOperatorFound()
		{
			var responseMessage = BRCResponseMessageProcessorTest.CreateMessage(Factory, Guid.Empty, null, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.OPE, EDIMessageSubTypeList.Codes.Error);
			responseMessage.EM_MessageText = ResponseMessageText;
			var logger = ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
				AssertContains("Log Message", "Unable to locate the related Business Object for OPE message #1", logger.LogMessages.ToString());
			});
		}

		void AssertMessageProcessed(CusBRForeignOperator foreignOperator, string expectedMessageText)
		{
			CombineAssertions(() =>
			{
				var message = foreignOperator.Messages.LastIncomingMessage;
				AssertEquals("EM_LinkTable", "CusBRForeignOperator", message.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", foreignOperator.PK, message.EM_LinkUniqueID);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.OPE, message.EM_MessageType);
				AssertEquals("EM_MessageSubType", EDIMessageSubTypeList.Codes.Error, message.EM_MessageSubType);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, message.EM_Status);
				AssertEquals("EM_MessageText", expectedMessageText, message.EM_MessageText);
				Assert("UpdateMessageStatusOnSaving NOT Suspended", !foreignOperator.IsUpdateMessageStatusOnSavingSuspended);

				Assert("Log AutoEvents.MessageRejected Created", foreignOperator.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == AutoEvents.MessageRejected.Code));
				AssertEquals("BFR_MessageStatus", EDIMessageStatusList.Codes.Rejected, foreignOperator.BFR_MessageStatus);
			});
		}

		string ResponseMessageText => "{\"message\": \"O usuário logado não é representante legal do CPF/CNPJ Raiz.\"}";
	}
}
