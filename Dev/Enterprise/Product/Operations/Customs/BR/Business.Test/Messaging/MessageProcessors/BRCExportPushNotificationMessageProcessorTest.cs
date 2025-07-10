using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCExportPushNotificationMessageProcessorTest : BRCResponseMessageProcessorAbstractTest<BRCExportPushNotificationMessageProcessor>
	{
		protected override IReadOnlyList<string> MessageFilterTypes => new[] { "PUS" };

		protected override IReadOnlyList<string> MessageFilterSubTypes => new[] { "EXP" };

		public void TestProcessResponseMessage_EntryNotFound()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.CDE;
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.PUS, EDIMessageSubTypeList.Codes.Export).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessage;

			var logger = new LoggingInformationForTesting();
			CombineAssertions(() =>
			{
				var exception = AssertExceptionThrown<MessageProcessLockException>(() =>
				{
					var logger = ExecuteMessageProcessor(responseMessage);
					AssertEquals("Logger", "Warning: \tUnable to find an Entry with Entry Number '30BR0000006651' for PUS message #1\r\n", logger.LogMessages.ToString());
				});
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Queued, responseMessage.EM_Status);
				AssertEquals("Unable to find an Entry with Entry Number '30BR0000006651' for PUS message #1", exception.Message);
			});
		}

		[TestDate(2024, 05, 05, 10, 10, 0)]
		public void TestProcessResponseMessage_EntryNotFoundHeldUntilDate_RetryCountLessThan3()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.CDE;
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.PUS, EDIMessageSubTypeList.Codes.Export).ResponseMessage;
			responseMessage.EM_RetryCount = 1;
			responseMessage.EM_MessageText = JsonMessage;
			Factory.Save();

			var logger = new LoggingInformationForTesting();
			new BRCIncomingMessageProcessor(logger).ExecuteBatch();

			responseMessage.Reload();
			CombineAssertions(() =>
			{
				AssertEquals("Logger", @"Information: 	Pre-Process Message #1
Warning: 	Unable to find an Entry with Entry Number '30BR0000006651' for PUS message #1
Information: 	Unable to find an Entry with Entry Number '30BR0000006651' for PUS message #1
Information: 	Saving...
Information: 	1 message pre-processed
", logger.LogMessages.ToString());
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Queued, responseMessage.EM_Status);
				AssertEquals("EM_HeldUntilDate", new ZDateTime(2024, 05, 05, 10, 15, 0), responseMessage.EM_HeldUntilDate);
				AssertEquals("EM_RetryCount", (ZByte)2, responseMessage.EM_RetryCount);
			});
		}

		[TestDate(2024, 05, 05, 10, 10, 0)]
		public void TestProcessResponseMessage_EntryNotFoundHeldUntilDate_RetryCountMoreOrEqual3()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.CDE;
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.PUS, EDIMessageSubTypeList.Codes.Export).ResponseMessage;
			responseMessage.EM_MessageNum = "1";
			responseMessage.EM_RetryCount = 3;
			responseMessage.EM_MessageText = JsonMessage;
			Factory.Save();

			var logger = new LoggingInformationForTesting();
			new BRCIncomingMessageProcessor(logger).ExecuteBatch();

			responseMessage.Reload();
			CombineAssertions(() =>
			{
				AssertContains("Logger", "Message #1: Unable to find an Entry with Entry Number '30BR0000006651', retried 3 times.", logger.LogMessages.ToString());
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
				AssertEquals("EM_RetryCount", (ZByte)3, responseMessage.EM_RetryCount);
			});
		}

		public void TestProcessResponseMessage_EntryFound()
		{
			ReferenceTestDataHelper.CreateEntryStatusForExport(Factory);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.MovementReferenceNumberSetter("30BR0000006651");
			entry.CH_MessageType = MessageTypeList.Codes.CDE;
			Factory.Save();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.PUS, EDIMessageSubTypeList.Codes.Export).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessage;

			ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals(3, entry.Messages.Count);

				AssertEquals("EM_LinkTable", "CusEntryHeader", responseMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", entry.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.PUS, responseMessage.EM_MessageType);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, responseMessage.EM_Status);

				var log = entry.Logs.MostRecentLogByEventTime(Events.CustomsUpdate);
				AssertEquals("SL_Reference", "|CDN=30BR0000006651|DES=Registro", log.SL_Reference);
				AssertEquals("SL_EventTime", new ZDateTime(2020, 08, 26, 11, 40, 32, 613), log.SL_EventTime);

				var comMessage = entry.Messages.Cast<BREDIMessage>().First(x => x.EM_MessageSubType == EDIMessageSubTypeList.Codes.CompleteConsult);
				AssertEquals("COM Message EM_Status", EDIMessage.Status.Queued, comMessage.EM_Status);
				AssertEquals("COM Message EM_ReceiveTransmit", EDIMessage.Direction.Transmit, comMessage.EM_ReceiveTransmit);
				AssertEquals("COM Message EM_MessageText", ZString.Empty, comMessage.EM_MessageText);
				AssertEquals("COM Message EM_ApplicationReference", "30BR0000006651", comMessage.EM_ApplicationReference);
				AssertEquals("COM Message EM_LinkedObject", entry, comMessage.EM_LinkedObject);
				AssertEquals("COM Message EM_GB", entry.Branch.PK, comMessage.EM_GB);
			});
		}

		public void TestProcessResponseMessage_ValidEntryStatus()
		{
			ReferenceTestDataHelper.CreateEntryStatusForExport(Factory);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.MovementReferenceNumberSetter("30BR0000006651");
			entry.CH_MessageType = MessageTypeList.Codes.CDE;
			Factory.Save();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.PUS, EDIMessageSubTypeList.Codes.Export).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessageCancelled;

			ExecuteMessageProcessor(responseMessage);

			CombineAssertions(() =>
			{
				AssertEquals("EM_LinkTable", "CusEntryHeader", responseMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", entry.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.PUS, responseMessage.EM_MessageType);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, responseMessage.EM_Status);

				AssertEquals("CH_EntryStatus", "E70", entry.CH_EntryStatus);

				var log = entry.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus);
				AssertEquals("CES Log Reference", "E70", log.SL_Reference);
				AssertEquals("CES Log Event Time", new ZDateTime(2020, 08, 26, 11, 40, 32, 613), log.SL_EventTime);
			});
		}

		public void TestProcessResponseMessage_Registered()
		{
			ReferenceTestDataHelper.CreateEntryStatusForExport(Factory);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.MovementReferenceNumberSetter("30BR0000006651");
			entry.CH_MessageType = MessageTypeList.Codes.CDE;
			Factory.Save();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.PUS, EDIMessageSubTypeList.Codes.Export).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessage;

			ExecuteMessageProcessor(responseMessage);

			CombineAssertions(() =>
			{
				AssertEquals("EM_LinkTable", "CusEntryHeader", responseMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", entry.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.PUS, responseMessage.EM_MessageType);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, responseMessage.EM_Status);

				AssertEquals("CH_EntryStatus", "E10", entry.CH_EntryStatus);
				AssertNull(entry.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus));
			});
		}

		public void TestProcessResponseMessage_InvalidEntryStatus()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.MovementReferenceNumberSetter("30BR0000006651");
			entry.CH_MessageType = MessageTypeList.Codes.CDE;
			entry.CH_EntryStatus = ZString.Empty;
			Factory.Save();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.PUS, EDIMessageSubTypeList.Codes.Export).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessage;

			ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_LinkTable", "CusEntryHeader", responseMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", entry.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.PUS, responseMessage.EM_MessageType);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, responseMessage.EM_Status);

				AssertEquals("CH_EntryStatus NOT updated", ZString.Empty, entry.CH_EntryStatus);
			});
		}

		internal const string JsonMessage = @"
		{
			""tipo"":""REGISTRADA"",
			""descricao"":""Registro"",
			""data"":""26/08/2020 11:40:32:613"",
			""due"":{
				""numero"":""30BR0000006651"",
				""ruc"":""0BR00000000200000000000000000001632""
			}
		}";

		internal const string JsonMessageCancelled = @"
		{
			""tipo"":""AVERBADA_SEM_DIVERGENCIA"",
			""descricao"":""Registro"",
			""data"":""26/08/2020 11:40:32:613"",
			""due"":{
				""numero"":""30BR0000006651"",
				""ruc"":""0BR00000000200000000000000000001632""
			}
		}";
	}
}
