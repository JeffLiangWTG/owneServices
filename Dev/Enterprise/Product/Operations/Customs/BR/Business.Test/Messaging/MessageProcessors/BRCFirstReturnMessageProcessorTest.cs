using System;
using System.Collections.Generic;
using System.Text.Json;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCFirstReturnMessageProcessorTest : BRCResponseMessageProcessorAbstractTest<BRCFirstReturnMessageProcessor>
	{
		protected override IReadOnlyList<string> MessageFilterTypes => new[] { "CDC" };

		protected override IReadOnlyList<string> MessageFilterSubTypes => Array.Empty<string>();

		public void TestMessageTypesToInclude()
		{
			var processor = new BRCFirstReturnMessageProcessor(new LoggingInformation());
			var messageTypesToInclude = processor.MessageTypesToInclude;
			AssertEquals(1, messageTypesToInclude.Count);
			AssertEquals(MessageTypeList.Codes.CDC, messageTypesToInclude[0]);
		}

		public void TestProcessResponseMessage_StatusRejected()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CDC, EDIMessageSubTypeList.Codes.Error).ResponseMessage;
			responseMessage.EM_MessageText = @"{
   ""message"":""O cabeçalho 'Role-Type' não está na requisição."",
   ""code"":""PUCX-ER0101"",
   ""tag"":""[081454RXF]"",
   ""status"":422,
   ""severity"":""ERROR""
   }";
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			Factory.Save();

			AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
			AssertEquals("CH_Status should be REJ", BRMessageStatusList.Codes.Rejected, entry.CH_Status);
			AssertEquals("Log AutoEvents.MessageRejected Created", AutoEvents.MessageRejected.Code, entry.Logs.MostRecentLog.SL_SE_NKEvent);
		}

		public void TestProcessResponseMessage_LogRejected()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = BRMessageStatusList.Codes.Rejected;

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CDC, EDIMessageSubTypeList.Codes.Error).ResponseMessage;
			responseMessage.EM_MessageText = @"{
   ""message"":""O cabeçalho 'Role-Type' não está na requisição."",
   ""code"":""PUCX-ER0101"",
   ""tag"":""[081454RXF]"",
   ""status"":422,
   ""severity"":""ERROR""
   }";
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);
			AssertEquals("Logger", "Information: \tMessage #1: First return error message received, message status not updated, because current status is 'REJ'\r\n", logger.LogMessages.ToString());
		}

		public void TestProcessResponseMessage_StatusIsAwaiting()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CDC, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = @"{
   ""message"":""Operação realizada com sucesso"",
   ""code"":""PUCX-ER0101"",
   ""tag"":""[081454RXF]"",
   ""status"":200,
   ""severity"":""""
}";
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);
			AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
			AssertEquals("CH_Status should be AWA", BRMessageStatusList.Codes.AwaitingResponse, entry.CH_Status);
			AssertEquals("Logger", "Information: \tMessage #1: First return success message received, keep current status 'AWA'\r\n", logger.LogMessages.ToString());
		}

		public void TestProcessResponseMessage_LogError()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = BRMessageStatusList.Codes.Rejected;

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CDC, EDIMessageSubTypeList.Codes.Error).ResponseMessage;
			responseMessage.EM_MessageText = @"{
   ""message"":""O cabeçalho 'Role-Type' não está na requisição."",
   ""code"":""PUCX-ER0101"",
   ""tag"":""[081454RXF]"",
   ""status"":422,
   ""severity"":""FAILUREDNOTMAP""
   }";
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);
			AssertEquals("Logger", "Error: \tMessage #1: First return message with unknown severity 'FAILUREDNOTMAP' received.\r\n", logger.LogMessages.ToString());
		}

		public void TestProcessResponseMessage_LogErrorWithDeserializationFailed()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = BRMessageStatusList.Codes.Rejected;
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CDC, EDIMessageSubTypeList.Codes.Error).ResponseMessage;
			responseMessage.EM_MessageText = @"";
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);
			AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
			AssertEquals("Logger", "Error: \tMessage #1: Message deserialization was failed.\r\n", logger.LogMessages.ToString());
		}

		public void TestProcessResponseMessage_LogErrorWithDeserializationFailedAndException()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = BRMessageStatusList.Codes.Rejected;
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CDC, EDIMessageSubTypeList.Codes.Error).ResponseMessage;
			responseMessage.EM_MessageText = "aaaaaaaa";
			Factory.Save();
			AssertExceptionThrown<JsonException>(() => { ExecuteMessageProcessor(responseMessage); });
		}
	}
}
