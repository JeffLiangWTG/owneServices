using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCImportPushNotificationMessageProcessorTest : BRCResponseMessageProcessorAbstractTest<BRCImportPushNotificationMessageProcessor>
	{
		protected override IReadOnlyList<string> MessageFilterTypes => new[] { "PUS" };

		protected override IReadOnlyList<string> MessageFilterSubTypes => new[] { "IMP" };

		public void TestProcessResponseMessageDiagnostic_EntryNotFound()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.CDI;
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.PUS, EDIMessageSubTypeList.Codes.Import).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessageDiagnostic;

			var logger = ExecuteMessageProcessor(responseMessage);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
			AssertContains("Logger", "Error: \tUnable to find an Entry with Entry Number '20BR00000000198' for PUS message #1\r\n", logger.LogMessages.ToString());
		}

		public void TestProcessResponseMessageDiagnostic()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.MovementReferenceNumberSetter("20BR00000000198");
			entry.CH_MessageType = MessageTypeList.Codes.CDI;
			entry.CH_RiskChannel = RiskChannelList.Codes.Green;
			Factory.Save();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.PUS, EDIMessageSubTypeList.Codes.Import).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessageDiagnostic;

			ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
				AssertEquals("EM_LinkTable", "CusEntryHeader", responseMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", entry.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.PUS, responseMessage.EM_MessageType);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, responseMessage.EM_Status);
				AssertEquals("CH_RiskChannel", RiskChannelList.Codes.Green, entry.CH_RiskChannel);
			});

			var log = entry.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault();
			CombineAssertions(() =>
			{
				AssertEquals("SL_SE_NKEvent", Events.CustomsUpdate.Code, log.SL_SE_NKEvent);
				AssertEquals("SL_Reference", "|CDN=20BR00000000198|STU=01 - xxxxx", log.SL_Reference);
				AssertEquals("SL_EventTime", new ZDateTime(2020, 04, 28, 14, 50, 29), log.SL_EventTime);
			});
		}

		public void TestProcessResponseMessageRegister_EntryNotFound()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.CDI;
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.PUS, EDIMessageSubTypeList.Codes.Import).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessageRegister;

			var logger = ExecuteMessageProcessor(responseMessage);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
			AssertContains("Logger", "Error: \tUnable to find an Entry with Entry Number '20BR00000000198' for PUS message #1\r\n", logger.LogMessages.ToString());
		}

		public void TestProcessResponseMessageRegister()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.MovementReferenceNumberSetter("20BR00000000198");
			entry.CH_MessageType = MessageTypeList.Codes.CDI;
			entry.CH_RiskChannel = RiskChannelList.Codes.Green;
			Factory.Save();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.PUS, EDIMessageSubTypeList.Codes.Import).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessageRegister;

			ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
				AssertEquals("EM_LinkTable", "CusEntryHeader", responseMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", entry.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.PUS, responseMessage.EM_MessageType);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, responseMessage.EM_Status);
				AssertEquals("CH_RiskChannel", RiskChannelList.Codes.Green, entry.CH_RiskChannel);
			});

			var log = entry.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault();
			CombineAssertions(() =>
			{
				AssertEquals("SL_SE_NKEvent", Events.CustomsUpdate.Code, log.SL_SE_NKEvent);
				AssertEquals("SL_Reference", "|CDN=20BR00000000198|STU=A solicitação de registro de sua Duimp sem alertas e erros foi concluída.", log.SL_Reference);
				AssertEquals("SL_EventTime", new ZDateTime(2020, 04, 28, 14, 50, 29), log.SL_EventTime);
			});
		}

		public void TestProcessResponseMessageSituation_EntryNotFound()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.CDI;
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.PUS, EDIMessageSubTypeList.Codes.Import).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessageSituation;

			var logger = ExecuteMessageProcessor(responseMessage);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
			AssertContains("Logger", "Error: \tUnable to find an Entry with Entry Number '20BR00000000198' for PUS message #1\r\n", logger.LogMessages.ToString());
		}

		public void TestProcessResponseMessageSituation()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.MovementReferenceNumberSetter("20BR00000000198");
			entry.CH_MessageType = MessageTypeList.Codes.CDI;
			entry.CH_RiskChannel = RiskChannelList.Codes.Green;
			Factory.Save();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.PUS, EDIMessageSubTypeList.Codes.Import).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessageSituation;

			ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
				AssertEquals("EM_LinkTable", "CusEntryHeader", responseMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", entry.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.PUS, responseMessage.EM_MessageType);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, responseMessage.EM_Status);
				AssertEquals("CH_RiskChannel", RiskChannelList.Codes.Red, entry.CH_RiskChannel);
			});

			var log = entry.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault();
			CombineAssertions(() =>
			{
				AssertEquals("SL_SE_NKEvent", Events.CustomsUpdate.Code, log.SL_SE_NKEvent);
				AssertEquals("SL_Reference", "|CDN=20BR00000000198|STU=A sua Duimp se encontra em conferência.", log.SL_Reference);
				AssertEquals("SL_EventTime", new ZDateTime(2020, 04, 28, 14, 50, 29), log.SL_EventTime);
			});
		}

		public void TestProcessResponseMessageSituation_InvalidRiskChannel()
		{
			var jsonMessageUpdated = JsonMessageSituation.Replace("\"canal\": \"VERMELHO\"", " \"canal\": \"TEST\"");
			AssertInvalidResponse(jsonMessageUpdated, "Unable to find Risk Channel Code for canal 'TEST'.");

			jsonMessageUpdated = JsonMessageSituation.Replace("\"canal\": VERMELHO", "\"canal\": ");
			AssertInvalidResponse(jsonMessageUpdated, string.Empty);
		}

		public void TestProcessResponseMessageValidEntryStatus()
		{
			ReferenceTestDataHelper.CreateEntryStatusForImport(Factory);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.MovementReferenceNumberSetter("20BR00000000198");
			entry.CH_MessageType = MessageTypeList.Codes.CDI;
			entry.CH_RiskChannel = RiskChannelList.Codes.Green;
			Factory.Save();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.PUS, EDIMessageSubTypeList.Codes.Import).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessageEntryStatus;

			ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
				AssertEquals("EM_LinkTable", "CusEntryHeader", responseMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", entry.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.PUS, responseMessage.EM_MessageType);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, responseMessage.EM_Status);

				AssertEquals("CH_EntryStatus", "I31", entry.CH_EntryStatus);
				AssertEquals("CH_AuthorityVersion", "1", entry.CH_AuthorityVersion);
				AssertEquals("MovementReferenceNumber", "20BR00000000198", entry.MovementReferenceNumber);

				var log = entry.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus);
				AssertEquals("CES Log Reference", "I31", log.SL_Reference);
				AssertEquals("CES Log Event Time", new ZDateTime(2020, 04, 28, 14, 50, 29), log.SL_EventTime);
			});

			responseMessage.EM_MessageText = JsonMessageEntryStatusVersionEmpty;
			ExecuteMessageProcessor(responseMessage);
			AssertEquals("CH_AuthorityVersion", "1", entry.CH_AuthorityVersion);
		}

		public void TestProcessResponseMessageInValidEntryStatus()
		{
			AssertInvalidResponse(JsonMessageEntryStatus, "Unable to find Import Customs Status Code (starts with 'I') for situacaoDuimp 'EM_CONFERENCIA_SELECIONADA'.");
			AssertInvalidResponse(JsonMessageSituation, string.Empty);
		}

		void AssertInvalidResponse(string jsonMessage, string expectedErrorReportedMessage)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.MovementReferenceNumberSetter("20BR00000000198");
			entry.CH_MessageType = MessageTypeList.Codes.CDI;
			Factory.Save();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.PUS, EDIMessageSubTypeList.Codes.Import).ResponseMessage;
			responseMessage.EM_MessageText = jsonMessage;

			ExecuteMessageProcessor(responseMessage);
			AssertEquals(expectedErrorReportedMessage, ErrorReporter.LastMessageReported);
			ErrorReporter.Instance.Clear();
			ErrorReporter.Clear();
		}

		internal const string JsonMessageDiagnostic = @"
		{
			""code"": ""DIMP-INXXXX"",
			""message"": ""A solicitação de Diagnóstico da sua Duimp foi concluída."",
			""identificacao"": {
				""numero"": ""20BR00000000198"",
				""versao"": ""0""
			},
			""niImportador"": ""00000000000191"",
			""situacaoDuimp"": """",
			""evento"": [
				""Solicitação de Diganóstico""
			],
			""dataEvento"": ""2020-04-28T14:50:29-0300"",
			""diagnostico"": {
				""situacao"": ""SituacaoDiagnostico-Enum"",
				""dataGeracao"": ""2020-04-28T14:50:29-0300"",
				""resultado"": [
					{
						""tipo"": ""SeveridadeDiagnostico-Enum"",
						""item"": ""0"",
						""mensagem"": {
							""codigo"": ""01"",
							""texto"": ""xxxxx""
						},
						""sistemaOrigem"": ""yyyyyyy""
					}
				]
			}
		}";

		internal const string JsonMessageRegister = @"
		{
			""registroIniciado"": true,
			""code"": ""DIMP-INXXXX"",
			""message"": ""A solicitação de registro de sua Duimp sem alertas e erros foi concluída."",
			""identificacao"": {
				""numero"": ""20BR00000000198"",
				""versao"": ""0""
			},
			""niImportador"": ""00000000000191"",
			""situacaoDuimp"": """",
			""evento"": [
				""Solicitação de Registro""
			],
			""dataEvento"": ""2020-04-28T14:50:29-0300"",
			""diagnostico"": {
				""situacao"": ""SituacaoDiagnostico-Enum"",
				""dataGeracao"": ""2020-04-28T14:50:29-0300""
			}
		}";

		internal const string JsonMessageSituation = @"
		{
			""code"": ""DIMP-INXXXX"",
			""message"": ""A sua Duimp se encontra em conferência."",
			""identificacao"": {
				""numero"": ""20BR00000000198"",
				""versao"": ""1""
			},
			""niImportador"": ""00000000000191"",
			""situacaoDuimp"": """",
			""evento"": [
				""Solicitação de Registro""
			],
			""dataEvento"": ""2020-04-28T14:50:29-0300"",
			""canal"": ""VERMELHO""
		}";

		internal const string JsonMessageEntryStatus = @"
		{
			""code"": ""DIMP-INXXXX"",
			""message"": ""A solicitação de registro de sua Duimp sem alertas e erros foi concluída."",
			""identificacao"": {
				""numero"": ""20BR00000000198"",
				""versao"": ""1""
			},
			""niImportador"": ""00000000000191"",
			""situacaoDuimp"": ""EM_CONFERENCIA_SELECIONADA"",
			""evento"": [
				""Solicitação de Registro""
			],
			""dataEvento"": ""2020-04-28T14:50:29-0300"",
			""canal"": ""VERDE""
		}";

		internal const string JsonMessageEntryStatusVersionEmpty = @"
		{
			""code"": ""DIMP-INXXXX"",
			""message"": ""A solicitação de registro de sua Duimp sem alertas e erros foi concluída."",
			""identificacao"": {
				""numero"": ""20BR00000000198"",
				""versao"": """"
			},
			""niImportador"": ""00000000000191"",
			""situacaoDuimp"": ""EM_CONFERENCIA_SELECIONADA"",
			""evento"": [
				""Solicitação de Registro""
			],
			""dataEvento"": ""2020-04-28T14:50:29-0300"",
			""canal"": ""VERDE""
		}";
	}
}
