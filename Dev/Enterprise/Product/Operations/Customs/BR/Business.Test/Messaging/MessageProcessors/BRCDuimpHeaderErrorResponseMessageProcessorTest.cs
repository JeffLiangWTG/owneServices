using System.Collections.Generic;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCDuimpHeaderErrorResponseMessageProcessorTest : BRCResponseMessageProcessorAbstractTest<BRCDuimpHeaderErrorResponseMessageProcessor>
	{
		protected override IReadOnlyList<string> MessageFilterTypes => new[] { "CIH" };

		protected override IReadOnlyList<string> MessageFilterSubTypes => new[] { "ERR" };

		public void TestMessageFriendlyName()
		{
			var processor = new BRCDuimpHeaderErrorResponseMessageProcessor(new LoggingInformation());
			AssertEquals("MessageFriendlyName", "DUIMP Error Response Message", processor.MessageFriendlyName);
		}

		public void TestProcessErrorResponseMessage_ORI()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.CDI;

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CIH, EDIMessageSubTypeList.Codes.Error, requestMessageSubType: EDIMessageSubTypeList.Codes.Original).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessageResponse;
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("EM_LinkUniqueID", entry.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", entry.TableName, responseMessage.EM_LinkTable);
				AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessage.EM_Status);
				AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
				AssertEquals("CH_Status", BRMessageStatusList.Codes.Rejected, entry.CH_Status);
				AssertEquals("MessageRejected log added", AutoEvents.MessageRejected.Code, entry.Logs.MostRecentLog.SL_SE_NKEvent);
			});
		}

		public void TestProcessErrorResponseMessage_UPD()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.CDI;
			entry.MovementReferenceNumberSetter("38BR15856778945");

			var (requestMessage, responseMessage) = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CIH, EDIMessageSubTypeList.Codes.Error, requestMessageSubType: EDIMessageSubTypeList.Codes.Update);
			responseMessage.EM_MessageText = JsonMessageResponse;
			CreateLineMessageAndSetAsProcessed(entry, EDIMessageSubTypeList.Codes.Addition);
			CreateLineMessageAndSetAsProcessed(entry, EDIMessageSubTypeList.Codes.Update);
			var lineResponseMessage = CreateLineMessage(entry, EDIMessageSubTypeList.Codes.Deletion);
			Factory.Save();

			CombineAssertions(() =>
			{
				var logger = new LoggingInformationForTesting();
				var exception = AssertExceptionThrown<MessageProcessLockException>("MessageProcessLockException thrown to postpone",
					"Message #1 postponed: Entry Header B00001000-1, has CIL message waiting response.", () =>
					{
						var logger = ExecuteMessageProcessor(responseMessage);
						AssertEquals("Logger", "Warning: \tMessage #1 postponed: Entry Header B00001000-1, has CIL message waiting response.\r\n", logger.LogMessages.ToString());
					});

				AssertEquals("EM_LinkUniqueID", entry.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", entry.TableName, responseMessage.EM_LinkTable);
				AssertEquals("EM_Status", EDIMessage.Status.PreProcessedOK, responseMessage.EM_Status);
				AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);

				AssertEquals("CH_Status", BRMessageStatusList.Codes.NotSent, entry.CH_Status);
				AssertNull("Log AutoEvents.MessageRejected should not be created", entry.Logs.MostRecentLog);
			});

			lineResponseMessage.EM_Status = EDIMessage.Status.ProcessedOK;
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("EM_LinkUniqueID", entry.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", entry.TableName, responseMessage.EM_LinkTable);
				AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessage.EM_Status);
				AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);

				AssertEquals("CH_Status", BRMessageStatusList.Codes.Rejected, entry.CH_Status);
				AssertEquals("MessageRejected log added", AutoEvents.MessageRejected.Code, entry.Logs.MostRecentLog.SL_SE_NKEvent);
			});
		}

		EDIMessage CreateLineMessage(CusEntryHeader entry, string lineMessageSubType)
		{
			return BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CIL, EDIMessageSubTypeList.Codes.Success,
				requestMessageSubType: lineMessageSubType).ResponseMessage;
		}

		void CreateLineMessageAndSetAsProcessed(CusEntryHeader entry, string lineMessageSubType)
		{
			CreateLineMessage(entry, lineMessageSubType).EM_Status = EDIMessage.Status.ProcessedOK;
		}

		const string JsonMessageResponse = @"{
    ""message"": ""O item número 888 que você está informando não existe na Duimp 24BR00000009256/0, verifique se está correto."",
    ""errors"": [
    ]
}";
	}
}
