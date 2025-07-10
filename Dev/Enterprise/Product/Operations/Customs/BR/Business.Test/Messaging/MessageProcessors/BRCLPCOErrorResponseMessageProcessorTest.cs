using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	sealed class BRCLPCOErrorResponseMessageProcessorTest : BRCResponseMessageProcessorAbstractTest<BRCLPCOErrorResponseMessageProcessor>
	{
		protected override IReadOnlyList<string> MessageFilterTypes => new[] { "LPC" };

		protected override IReadOnlyList<string> MessageFilterSubTypes => new[] { "ERR" };

		public void TestProcessMessage_SingleOutgoingMessage()
		{
			var lpcoHeader = Factory.NewWithValidTestData<CusLPCOHeader>();
			var (requestMessages, responseMessage) = BRCResponseMessageProcessorTest.CreateResponseMessage(lpcoHeader, MessageTypeList.Codes.LPC, EDIMessageSubTypeList.Codes.Error);
			responseMessage.EM_MessageText = ResponseMessageText;
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			AssertMessageProcessed(lpcoHeader, ResponseMessageText);
		}

		public void TestProcessMessage_MultipleOutgoingMessages()
		{
			var lpcoHeader1 = Factory.NewWithValidTestData<CusLPCOHeader>();
			var lpcoHeader2 = Factory.NewWithValidTestData<CusLPCOHeader>();
			var lpcoHeader3 = Factory.NewWithValidTestData<CusLPCOHeader>();
			var (requestMessages, responseMessage) = BRCResponseMessageProcessorTest.CreateMultipleMessagesAndInterchange(new [] { lpcoHeader1, lpcoHeader2, lpcoHeader3 }, MessageTypeList.Codes.LPC, EDIMessageSubTypeList.Codes.Error);
			responseMessage.EM_MessageText = ResponseMessageText;
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);

			var responseMessages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive).AddToFilter(EDIMessageSchema.EM_LinkTable, CusLPCOHeader.Schema.TableName));
			AssertEquals(3, responseMessages.Length);
			AssertNotNull(responseMessages.Single(x => x.PK == responseMessage.PK));

			AssertMessageProcessed(lpcoHeader1, ResponseMessageText);
			AssertMessageProcessed(lpcoHeader2, ResponseMessageText);
			AssertMessageProcessed(lpcoHeader3, ResponseMessageText);
		}

		[ExpectNoExceptions]
		public void TestProcessMessage_NoLPCOHeaderFound()
		{
			var declaration = Factory.New<JobDeclaration>();
			var (requestMessages, responseMessage) = BRCResponseMessageProcessorTest.CreateMultipleMessagesAndInterchange(new[] { declaration }, MessageTypeList.Codes.LPC, EDIMessageSubTypeList.Codes.Error);
			responseMessage.EM_MessageText = ResponseMessageText;
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
		}

		void AssertMessageProcessed(CusLPCOHeader lpcoHeader, string expectedMessageText)
		{
			CombineAssertions(() =>
			{
				var message = lpcoHeader.Messages.LastIncomingMessage;
				AssertEquals("EM_LinkTable", "CusPermitHeader", message.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", lpcoHeader.PK, message.EM_LinkUniqueID);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.LPC, message.EM_MessageType);
				AssertEquals("EM_MessageSubType", EDIMessageSubTypeList.Codes.Error, message.EM_MessageSubType);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, message.EM_Status);
				AssertEquals("EM_MessageText", expectedMessageText, message.EM_MessageText);
				AssertEquals("EM_GB", lpcoHeader.Company.FirstActiveBranch.PK, message.EM_GB);

				AssertEquals("CPH_MessageStatus", BRMessageStatusList.Codes.Rejected, lpcoHeader.CPH_MessageStatus);
			});
		}

		string ResponseMessageText => "{\"message\": \"O usuário logado não é representante legal do CPF/CNPJ Raiz.\"}";
	}
}
