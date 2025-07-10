using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCImportLicenseAcceptMessageProcessorTest : BRCResponseMessageProcessorAbstractTest<BRCImportLicenseAcceptMessageProcessor>
	{
		protected override IReadOnlyList<string> MessageFilterTypes => new[] { "LIC" };

		protected override IReadOnlyList<string> MessageFilterSubTypes => Array.Empty<string>();

		public void TestMessageTypesToInclude()
		{
			var processor = new BRCImportLicenseAcceptMessageProcessor(new LoggingInformation());
			var messageTypesToInclude = processor.MessageTypesToInclude;
			AssertEquals(1, messageTypesToInclude.Count);
			AssertEquals(MessageTypeList.Codes.LIC, messageTypesToInclude[0]);
		}

		public void TestProcessResponseMessage_EntryNotFound()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			entry.ImportLicenseIdentifier = "LI00000001";
			entry.CH_Status = BRMessageStatusList.Codes.Rejected;
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.LIC, EDIMessageSubTypeList.Codes.Error).ResponseMessage;
			responseMessage.EM_MessageText = GetMessageText(importLicenseIdentifier: "LI00000002");

			var logger = ExecuteMessageProcessor(responseMessage);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
			AssertContains("Logger", "Error: \tUnable to find an Entry with Import License Identifier 'LI00000002' for LIC message #1\r\n", logger.LogMessages.ToString());
		}

		public void TestProcessResponseMessage_LogErrorWithDeserializationFailedAndException()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			entry.ImportLicenseIdentifier = "LI00000001";
			entry.CH_Status = BRMessageStatusList.Codes.Rejected;
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.LIC, EDIMessageSubTypeList.Codes.Error).ResponseMessage;
			responseMessage.EM_MessageText = "aaaaaaaa";
			AssertExceptionThrown<InvalidOperationException>(() => { ExecuteMessageProcessor(responseMessage); });
		}

		public void TestProcessResponseMessage_StatusRejected()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			entry.ImportLicenseIdentifier = "LI00000001";
			entry.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;
			Factory.Save();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.LIC, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = GetMessageText(entryNumber: "", reason2: "ERRO COM O DRAWBACK2");

			ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("CH_Status", BRMessageStatusList.Codes.Rejected, entry.CH_Status);
				AssertEquals("Log AutoEvents.MessageRejected", AutoEvents.MessageRejected.Code, entry.Logs.MostRecentLog.SL_SE_NKEvent);
				AssertEquals("Log Count should be 2", 2, entry.Logs.GetAllLogs().Count);
				AssertEquals("Log AutoEvents.SL_Reference", "|CRF=000002;LI00000001|RES=ERRO COM O DRAWBACK", entry.Logs.GetAllLogs()[0].SL_Reference);
				AssertEquals("Log AutoEvents.SL_EventTime", new ZDateTime(2022, 4, 7), entry.Logs.GetAllLogs()[0].SL_EventTime);
				AssertEquals("Log AutoEvents.SL_Reference", "|CRF=000002;LI00000001|RES=ERRO COM O DRAWBACK2", entry.Logs.GetAllLogs()[1].SL_Reference);
				AssertEquals("Log AutoEvents.SL_EventTime", new ZDateTime(2022, 4, 7), entry.Logs.GetAllLogs()[1].SL_EventTime);
			});
		}

		public void TestProcessResponseMessage_StatusRejectedWithNoReason()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			entry.ImportLicenseIdentifier = "LI00000001";
			entry.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;
			Factory.Save();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.LIC, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = GetMessageText(entryNumber: "", reason: "");

			ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("CH_Status", BRMessageStatusList.Codes.Rejected, entry.CH_Status);
				AssertEquals("Log Count should be 1", 1, entry.Logs.GetAllLogs().Count);
				AssertEquals("Log AutoEvents.MessageRejected", AutoEvents.MessageRejected.Code, entry.Logs.MostRecentLog.SL_SE_NKEvent);
				AssertEquals("Log AutoEvents.SL_Reference", "|CRF=000002;LI00000001", entry.Logs.MostRecentLog.SL_Reference);
				AssertEquals("Log AutoEvents.SL_EventTime", new ZDateTime(2022, 4, 7), entry.Logs.MostRecentLog.SL_EventTime);
			});
			}

		public void TestProcessResponseMessage_MNRUpdated()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			entry.ImportLicenseIdentifier = "LI00000001";
			entry.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;
			Factory.Save();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.LIC, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = GetMessageText();

			ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, responseMessage.EM_Status);
				AssertEquals("EM_LinkTable", "CusEntryHeader", responseMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", entry.PK, responseMessage.EM_LinkUniqueID);

				AssertEquals("CH_Status", BRMessageStatusList.Codes.Accepted, entry.CH_Status);
				AssertEquals("MovementReferenceNumber updated", "22000005", entry.MovementReferenceNumber);
				AssertEquals("MovementReferenceNumberIssueDate updated", new ZDateTime("2022-04-07"), entry.MovementReferenceNumberIssueDate);
			});
				}

		public void TestProcessResponseMessage_ImportLicenseNumberUpdated()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			var invLineLIC1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var invLineLIC2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			declaration.DoMerge();
			var entry = declaration.ActiveEntryHeaders[0];
			entry.ImportLicenseIdentifier = "LI00000001";
			entry.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;

			var declarationISW = Factory.NewWithValidTestData<JobDeclaration>();
			declarationISW.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invLineISW1 = declarationISW.Invoices.AddNew().InvoiceLines.AddNew();
			invLineISW1.JI_ParentID = invLineLIC1.PK;
			invLineISW1.JI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			var invLineISW2 = declarationISW.Invoices.AddNew().InvoiceLines.AddNew();
			invLineISW2.JI_ParentID = invLineLIC2.PK;
			invLineISW2.JI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;

			Factory.Save();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.LIC, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = GetMessageText();

			ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, responseMessage.EM_Status);

				AssertEquals("CH_Status", BRMessageStatusList.Codes.Accepted, entry.CH_Status);
				AssertEquals("MovementReferenceNumber updated", "22000005", entry.MovementReferenceNumber);
				AssertEquals("MovementReferenceNumberIssueDate updated", new ZDateTime("2022-04-07"), entry.MovementReferenceNumberIssueDate);
				AssertEquals("Import License Number ", "22000005", invLineISW1.ImportLicenseNumber);
				AssertEquals("Import License Number ", "22000005", invLineISW2.ImportLicenseNumber);
			});
		}

		[TestDate(2022, 5, 6)]
		public void TestProcessResponseMessage_InvalidIssueDate()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			entry.ImportLicenseIdentifier = "LI00000001";
			entry.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;
			Factory.Save();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.LIC, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = GetMessageText(issueDate: "32/04/2022");

			ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, responseMessage.EM_Status);
				AssertEquals("CH_Status", BRMessageStatusList.Codes.Accepted, entry.CH_Status);
				AssertEquals("MovementReferenceNumber updated", "22000005", entry.MovementReferenceNumber);
				AssertEquals("MovementReferenceNumberIssueDate should be", ZDateTime.Today, entry.MovementReferenceNumberIssueDate);
			});
		}

		string GetMessageText(string importLicenseIdentifier = null, string entryNumber = null, string issueDate = null, string reason = null, string reason2 = null)
		{
			return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<lote-li>
  <cpfUsuario>000.000.000-01</cpfUsuario>
  <dataHoraEnvioFormatada>20/04/2022 15:25:00</dataHoraEnvioFormatada>
  <idLote>000002</idLote>
  <listaLIVORetorno>
    <li>
      <dtRegistro>{issueDate ?? "07/04/2022"}</dtRegistro>
      <idSolicitacao>{importLicenseIdentifier ?? "LI00000001"}</idSolicitacao>
      <importador>
        <numero>58500398000105</numero>
        <tipoImportador>J</tipoImportador>
      </importador>
      <mensagemDiagnostico>
        <mensagemDiagnostico>{reason ?? "ERRO COM O DRAWBACK"}</mensagemDiagnostico>
				{(reason2 == null ? "" : $"<mensagemDiagnostico>{reason2}</mensagemDiagnostico>")}
      </mensagemDiagnostico>
      <numeroLI>{entryNumber ?? "22000005"}</numeroLI>
    </li>
  </listaLIVORetorno>
</lote-li>";
		}
	}
}
