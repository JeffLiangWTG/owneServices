using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCLPCOPushNotificationMessageProcessorTest : BRCResponseMessageProcessorAbstractTest<BRCLPCOPushNotificationMessageProcessor>
	{
		protected override IReadOnlyList<string> MessageFilterTypes => new[] { "PUS" };

		protected override IReadOnlyList<string> MessageFilterSubTypes => new[] { "LPC" };

		public void TestProcessResponseMessageSituation_LPCONotFound()
		{
			var cusLPCO = Factory.NewWithValidTestData<CusLPCOHeader>();
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(cusLPCO, MessageTypeList.Codes.PUS, EDIMessageSubTypeList.Codes.LPCO).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessageChangeSituation;

			var logger = ExecuteMessageProcessor(responseMessage);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
			AssertContains("Logger", "Error: \tUnable to find a LPCO with numeroLPCO 'E2100000000' for PUS message #1\r\n", logger.LogMessages.ToString());
		}

		public void TestProcessResponseMessageSituation_TestFindLinkedObjectAndProperties()
		{
			ReferenceTestDataHelper.CreateEntryStatusForLPCO(Factory);

			var cusLPCO = Factory.NewWithValidTestData<CusLPCOHeader>();
			cusLPCO.CPH_Number = "E2100000000";
			cusLPCO.CPH_Type = MessageTypeList.Codes.LPC;
			Factory.Save();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(cusLPCO, MessageTypeList.Codes.PUS, EDIMessageSubTypeList.Codes.LPCO).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessageChangeSituation;

			ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_GB", cusLPCO.Company.FirstActiveBranch.PK, responseMessage.EM_GB);
				AssertEquals("EM_LinkTable", "CusPermitHeader", responseMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", cusLPCO.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.PUS, responseMessage.EM_MessageType);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, responseMessage.EM_Status);

				AssertEquals("CPH_CustomsStatus", "P01", cusLPCO.CPH_CustomsStatus);
			});

			var log = cusLPCO.Logs.GetAllLogs().Cast<StmALog>().ToArray()[1];
			CombineAssertions(() =>
			{
				AssertEquals("SL_SE_NKEvent", Events.CustomsUpdate.Code, log.SL_SE_NKEvent);
				AssertEquals("SL_Reference", "|CRF=E2100000000|RES=Alterar a situação|STA=Para análise", log.SL_Reference);
				AssertEquals("SL_EventTime", new ZDateTime(2021, 12, 31, 12, 00, 00), log.SL_EventTime);
			});
		}

		public void TestProcessResponseMessageSituationInvalidLPCOStatus()
		{
			var cusLPCO = Factory.NewWithValidTestData<CusLPCOHeader>();
			cusLPCO.CPH_Number = "E2100000000";
			cusLPCO.CPH_Type = MessageTypeList.Codes.LPC;
			Factory.Save();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(cusLPCO, MessageTypeList.Codes.PUS, EDIMessageSubTypeList.Codes.LPCO).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessageChangeSituation;

			ExecuteMessageProcessor(responseMessage);
			AssertEquals("Unable to find LPCO Customs Status Code (starts with 'P') for novaSituacao 'PARA_ANALISE'.", ErrorReporter.LastMessageReported);
			ErrorReporter.Instance.Clear();
			ErrorReporter.Clear();
		}

		public void TestProcessResponseMessageInclusion_LPCONotFound()
		{
			var cusLPCO = Factory.NewWithValidTestData<CusLPCOHeader>();
			cusLPCO.CPH_Number = "E2100000002";
			cusLPCO.CPH_Type = MessageTypeList.Codes.LPC;
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(cusLPCO, MessageTypeList.Codes.PUS, EDIMessageSubTypeList.Codes.LPCO).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessageInclusion;

			var logger = ExecuteMessageProcessor(responseMessage);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
			AssertContains("Logger", "Error: \tUnable to find a LPCO with numeroLPCO 'E2100000000' for PUS message #1\r\n", logger.LogMessages.ToString());
		}

		public void TestProcessResponseMessageInclusion_TestFindLinkedObjectAndStmALog()
		{
			var cusLPCO = Factory.NewWithValidTestData<CusLPCOHeader>();
			cusLPCO.CPH_Number = "E2100000000";
			cusLPCO.CPH_Type = MessageTypeList.Codes.LPC;
			Factory.Save();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(cusLPCO, MessageTypeList.Codes.PUS, EDIMessageSubTypeList.Codes.LPCO).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessageInclusion;

			ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_GB", cusLPCO.Company.FirstActiveBranch.PK, responseMessage.EM_GB);
				AssertEquals("EM_LinkTable", "CusPermitHeader", responseMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", cusLPCO.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.PUS, responseMessage.EM_MessageType);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, responseMessage.EM_Status);
			});

			var log = cusLPCO.Logs.GetAllLogs().Cast<StmALog>().ToArray()[1];
			CombineAssertions(() =>
			{
				AssertEquals("SL_SE_NKEvent", Events.CustomsUpdate.Code, log.SL_SE_NKEvent);
				AssertEquals("SL_Reference", "|ACT=Informar no pedido o número do certificado.|CRF=E2100000000|STA=Para análise", log.SL_Reference);
				AssertEquals("SL_EventTime", new ZDateTime(2021, 12, 31, 12, 00, 00), log.SL_EventTime);
			});
		}

		public void TestProcessResponseMessageCancel_LPCONotFound()
		{
			var cusLPCO = Factory.NewWithValidTestData<CusLPCOHeader>();
			cusLPCO.CPH_Number = "E2100000002";
			cusLPCO.CPH_Type = MessageTypeList.Codes.LPC;
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(cusLPCO, MessageTypeList.Codes.PUS, EDIMessageSubTypeList.Codes.LPCO).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessageCancel;

			var logger = ExecuteMessageProcessor(responseMessage);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
			AssertContains("Logger", "Error: \tUnable to find a LPCO with numeroLPCO 'E2100000000' for PUS message #1\r\n", logger.LogMessages.ToString());
		}

		public void TestProcessResponseMessageCancel_TestFindLinkedObjectAndStmALog()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var cusLPCO = Factory.NewWithValidTestData<CusLPCOHeader>();
			cusLPCO.CPH_Number = "E2100000000";
			cusLPCO.CPH_Type = MessageTypeList.Codes.LPC;
			Factory.Save();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(cusLPCO, MessageTypeList.Codes.PUS, EDIMessageSubTypeList.Codes.LPCO).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessageCancel;

			ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_GB", cusLPCO.Company.FirstActiveBranch.PK, responseMessage.EM_GB);
				AssertEquals("EM_LinkTable", "CusPermitHeader", responseMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", cusLPCO.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.PUS, responseMessage.EM_MessageType);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, responseMessage.EM_Status);
			});

			var log = cusLPCO.Logs.GetAllLogs().Cast<StmALog>().ToArray()[1];
			CombineAssertions(() =>
			{
				AssertEquals("SL_SE_NKEvent", Events.CustomsUpdate.Code, log.SL_SE_NKEvent);
				AssertEquals("SL_Reference", "Exigência Cancelada|ACT=Informar no pedido o número do certificado.|CRF=E2100000000", log.SL_Reference);
				AssertEquals("SL_EventTime", new ZDateTime(2021, 12, 31, 14, 00, 00), log.SL_EventTime);
			});
		}

		internal const string JsonMessageChangeSituation = @"
		{
			""dataEvento"": ""31/12/2021 12:00:00.000000"",
			""numeroLPCO"": ""E2100000000"",
			""justificativa"": ""Alterar a situação"",
			""novaSituacao"": {
				""id"": ""PARA_ANALISE"",
				""descricao"": ""Para análise""
			}
		}";

		internal const string JsonMessageInclusion = @"
		{
			""numero"": ""1"",
			""numeroLPCO"": ""E2100000000"",
			""exigencia"": ""Informar no pedido o número do certificado."",
			""dataExigencia"": ""31/12/2021 12:00"",
			""situacao"": {
				""id"": ""PENDENTE"",
				""descricao"": ""Para análise""
			}
		}";

		internal const string JsonMessageCancel = @"
		{
			""numero"": ""1"",
			""numeroLPCO"": ""E2100000000"",
			""exigencia"": ""Informar no pedido o número do certificado."",
			""dataExigencia"": ""31/12/2021 12:00"",
			""dataCancelamento"": ""31/12/2021 14:00"",
			""justificativa"": ""Não é mais necessária a informação.""
		}";

		internal const string JsonMessageAnalysis = @"
		{
			""dataEvento"": ""31/12/2021 12:00"",
			""numeroLPCO"": ""E2100000000"",
			""justificativa"": ""Este pedido de LPCo não pode ser prorrogado."",
			""resultado"": ""INDEFERIDO""
		}";

		internal const string JsonMessageAuto = @"
		{
			""dataEvento"": ""01/12/2021 12:00"",
			""numeroLPCO"": ""E2100000000"",
			""mensagem"": ""Informações do pedido estão sendo conferidas."",
		}";
	}
}
