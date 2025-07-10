using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CC515C_v514.CC515CV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Testing;

public class DeclarationAESResponseMessageProcessorTest : AESCommonResponseMessageProcessorTest<DeclarationAESResponseMessageProcessor, Cc515Cv1Sal>
{
	public void TestProcessAcceptedMessage_CLR_GreenCircuit_AndTriggerInboxRequest_EHub()
	{
		using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(false))
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitResponseCodeLMessageAEAT(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>10-06-2022</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES00999930006184</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>TEST444444444444</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>30-05-2022</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>PNS6NA3WMAUC4J8W</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Exit Type:</td><td>&nbsp;&nbsp;</td><td>[I] Indirect</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DE - Indirect Dispatch</td></tr></table>";
			AssertAcceptedExportDeclaration(message, entryStatusCode: EntryStatusCodes.Cleared, acceptanceDate: preparationDateTime, entryReleaseDate: preparationDateTime, csvT2L: CsvT2L, expectedMessageInterpretation: expectedMessageInterpretationText);

			AssertNewInboxMessages(entryHeader.Messages, [DeclarationMessageTypeList.Codes.ExportExitResultCommunication, DeclarationMessageTypeList.Codes.ExportInvalidationCommunication], DeclarantId, DeclarantName, MRNCode);
		}
	}

	public void TestProcessAcceptedMessage_CLR_RedCircuit()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileRedCircuitResponseCodeLMessageAEAT(), InterchangeID);

		ProcessMessageForTest(message);
		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>10-06-2022</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES00999930006184</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>TEST444444444444</td></tr>" +
			"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>30-05-2022</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>PNS6NA3WMAUC4J8W</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Exit Type:</td><td>&nbsp;&nbsp;</td><td>[I] Indirect</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DE - Indirect Dispatch</td></tr></table>";
		AssertAcceptedExportDeclaration(message, entryStatusCode: EntryStatusCodes.Cleared, circuit: CircuitCodeList.Codes.RED, acceptanceDate: preparationDateTime, entryReleaseDate: preparationDateTime, expectedMessageInterpretation: expectedMessageInterpretationText);
	}

	public void TestProcessAcceptedMessage_CLR_RedCircuit_ATC()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileRedCircuitResponseCodeLMessageATC(), InterchangeID);

		ProcessMessageForTest(message);
		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>10-06-2022</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES00999930006184</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr>" +
			"<tr><td>ATC Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>TEST444444444444</td></tr>" +
			"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>30-05-2022</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>PNS6NA3WMAUC4J8W</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Exit Type:</td><td>&nbsp;&nbsp;</td><td>[I] Indirect</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DE - Indirect Dispatch</td></tr></table>";
		AssertAcceptedExportDeclaration(message, entryStatusCode: EntryStatusCodes.Cleared, circuitCan: CircuitCodeList.Codes.RED, acceptanceDate: preparationDateTime, entryReleaseDate: preparationDateTime, expectedMessageInterpretation: expectedMessageInterpretationText);
	}

	public void TestProcessAcceptedMessage_CDA_OrangeCircuit_AndTriggerInboxRequest_xT()
	{
		using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(true))
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileOrangeCircuitResponseCodeBMessageAEAT(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>10-06-2022</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES00999930006184</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>PNS6NA3WMAUC4J8W</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Exit Type:</td><td>&nbsp;&nbsp;</td><td>[D] Direct</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DS - Direct Dispatch</td></tr></table>";
			AssertAcceptedExportDeclaration(message, entryStatusCode: EntryStatusCodes.CustomsDeclarationAccepted, circuit: CircuitCodeList.Codes.ORANGE, acceptanceDate: preparationDateTime, csvClearance: ZString.Empty, indirectExport: false, expectedMessageInterpretation: expectedMessageInterpretationText);

			AssertNewCusPollingTransaction(entryHeader.PK, entryHeader.TablePrefix, [DeclarationMessageTypeList.Codes.ExportClearanceCommunication, DeclarationMessageTypeList.Codes.ExportNonConformityCommunication], MRNCode);
		}
	}

	public void TestProcessAcceptedMessage_CDA_GreenCircuit_ATC()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitResponseCodeBMessageATC(), InterchangeID);

		ProcessMessageForTest(message);
		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>10-06-2022</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES00999930006184</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr>" +
			"<tr><td>ATC Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>TEST444444444444</td></tr>" +
			"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>30-05-2022</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>PNS6NA3WMAUC4J8W</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Exit Type:</td><td>&nbsp;&nbsp;</td><td>[D] Direct</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DS - Direct Dispatch</td></tr></table>";
		AssertAcceptedExportDeclaration(message, entryStatusCode: EntryStatusCodes.CustomsDeclarationAccepted, circuitCan: CircuitCodeList.Codes.GREEN, acceptanceDate: preparationDateTime, entryReleaseDate: entryReleaseDate, indirectExport: false, expectedMessageInterpretation: expectedMessageInterpretationText);
	}

	public void TestProcessAcceptedMessage_CDA_OrangeCircuit_ATC()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileOrangeCircuitResponseCodeBMessageATC(), InterchangeID);

		ProcessMessageForTest(message);
		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>10-06-2022</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES00999930006184</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr>" +
			"<tr><td>ATC Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>TEST444444444444</td></tr>" +
			"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>30-05-2022</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>PNS6NA3WMAUC4J8W</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Exit Type:</td><td>&nbsp;&nbsp;</td><td>[I] Indirect</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DE - Indirect Dispatch</td></tr></table>";
		AssertAcceptedExportDeclaration(message, entryStatusCode: EntryStatusCodes.CustomsDeclarationAccepted, circuitCan: CircuitCodeList.Codes.ORANGE, acceptanceDate: preparationDateTime, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);
	}

	public void TestProcessAcceptedMessage_PCO_WithCSVClearance_AndTriggerInboxRequest_EHub()
	{
		using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(false))
		{
			entryHeader.SetCSVClearanceNum("AAAAAAAAAAAAAAAA");

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileResponseCodeCMessage(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>10-06-2022</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES00999930006184</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>PNS6NA3WMAUC4J8W</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>AW - Waiting PCO Decision</td></tr></table>";
			AssertAcceptedExportDeclaration(message, entryStatusCode: EntryStatusCodes.PendingForEuOffice, circuit: ZString.Empty, acceptanceDate: preparationDateTime, csvClearance: "AAAAAAAAAAAAAAAA", indirectExport: false, expectedMessageInterpretation: expectedMessageInterpretationText);

			AssertNewInboxMessages(entryHeader.Messages, [DeclarationMessageTypeList.Codes.ExportInvalidationCommunication], DeclarantId, DeclarantName, MRNCode);
		}
	}

	public void TestProcessAcceptedMessage_PCO_WithoutCSVClearance_AndTriggerInboxRequest_EHub()
	{
		using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(false))
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileResponseCodeCMessage(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>10-06-2022</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES00999930006184</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>PNS6NA3WMAUC4J8W</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>AW - Waiting PCO Decision</td></tr></table>";
			AssertAcceptedExportDeclaration(message, entryStatusCode: EntryStatusCodes.PendingForEuOffice, circuit: ZString.Empty, acceptanceDate: preparationDateTime, csvClearance: ZString.Empty, indirectExport: false, expectedMessageInterpretation: expectedMessageInterpretationText);

			AssertNewInboxMessages(entryHeader.Messages, new ZString[] { DeclarationMessageTypeList.Codes.ExportClearanceCommunication, DeclarationMessageTypeList.Codes.ExportNonConformityCommunication, DeclarationMessageTypeList.Codes.ExportCceControlCommunication }, DeclarantId, DeclarantName, MRNCode);
		}
	}

	public void TestProcessAcceptedMessage_PDA_AndTriggerInboxRequest_EHub()
	{
		using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(false))
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileResponseCodePMessage(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>10-06-2022</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES00999930006184</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>PNS6NA3WMAUC4J8W</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PA - Pending Presentation of Goods</td></tr></table>";
			AssertAcceptedExportDeclaration(message, entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, circuit: ZString.Empty, acceptanceDate: preparationDateTime, csvClearance: ZString.Empty, indirectExport: false, expectedMessageInterpretation: expectedMessageInterpretationText);

			AssertNewInboxMessages(entryHeader.Messages, [DeclarationMessageTypeList.Codes.ExportInvalidationCommunication], DeclarantId, DeclarantName, MRNCode);
		}
	}

	public void TestProcessAcceptedMessage_EFD()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitResponseCodeLMessageAEAT(), InterchangeID);

		instruction.CEI_SubStyle = EntrySubStyleList.Codes.Z;

		ProcessMessageForTest(message);
		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>10-06-2022</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES00999930006184</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>TEST444444444444</td></tr>" +
			"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>30-05-2022</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>PNS6NA3WMAUC4J8W</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Exit Type:</td><td>&nbsp;&nbsp;</td><td>[I] Indirect</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DE - Indirect Dispatch</td></tr></table>";
		AssertAcceptedExportDeclaration(message, entryStatusCode: EntryStatusCodes.EffectiveDeparture, acceptanceDate: preparationDateTime, entryReleaseDate: preparationDateTime, csvT2L: CsvT2L, expectedMessageInterpretation: expectedMessageInterpretationText);

		CombineAssertions(() =>
		{
			var newRequestMessage = entryHeader.Messages.LastMessage;

			AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.ESCustomsMessage, newRequestMessage.EM_ApplicationCode);
			AssertEquals("message.EM_MessageType", DeclarationMessageTypeList.Codes.RequestExportExitCertificate, newRequestMessage.EM_MessageType);
			AssertEquals("message.EM_MessageSubType", DeclarationMessageSubTypeList.Codes.OriginalDeclaration, newRequestMessage.EM_MessageSubType);
			AssertEquals("message.EM_IsTestMessage", true, newRequestMessage.EM_IsTestMessage);
			AssertEquals("message.EM_ReceiveTransmit", EDIMessage.Direction.Transmit, newRequestMessage.EM_ReceiveTransmit);
			AssertEquals("message.EM_Status", EDIMessage.Status.Queued, newRequestMessage.EM_Status);
			AssertEquals("message.EM_ApplicationReference", "CertName", newRequestMessage.EM_ApplicationReference);
			AssertNull("message doesn't have interchange", newRequestMessage.Interchange);
		});
	}

	public void TestProcessAcceptedMessage_CLP_EntryInstructionB()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitResponseCodeLMessageAEAT(), InterchangeID);

		instruction.CEI_SubStyle = EntrySubStyleList.Codes.B;

		ProcessMessageForTest(message);
		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>10-06-2022</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES00999930006184</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>TEST444444444444</td></tr>" +
			"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>30-05-2022</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>PNS6NA3WMAUC4J8W</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Exit Type:</td><td>&nbsp;&nbsp;</td><td>[I] Indirect</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DE - Indirect Dispatch</td></tr></table>";
		AssertAcceptedExportDeclaration(message, entryStatusCode: EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, acceptanceDate: preparationDateTime, entryReleaseDate: preparationDateTime, csvT2L: CsvT2L, expectedMessageInterpretation: expectedMessageInterpretationText);
	}

	public void TestProcessAcceptedMessage_CLP_EntryInstructionC()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitResponseCodeLMessageAEAT(), InterchangeID);

		instruction.CEI_SubStyle = EntrySubStyleList.Codes.C;

		ProcessMessageForTest(message);
		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>10-06-2022</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES00999930006184</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>TEST444444444444</td></tr>" +
			"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>30-05-2022</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>PNS6NA3WMAUC4J8W</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Exit Type:</td><td>&nbsp;&nbsp;</td><td>[I] Indirect</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DE - Indirect Dispatch</td></tr></table>";
		AssertAcceptedExportDeclaration(message, entryStatusCode: EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, acceptanceDate: preparationDateTime, entryReleaseDate: preparationDateTime, csvT2L: CsvT2L, expectedMessageInterpretation: expectedMessageInterpretationText);
	}

	public void TestProcessAcceptedMessageWithSentCMP_CLR_EntryInstructionC()
	{
		SentMessage.EM_MessageSubType = "CMP";
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitResponseCodeLMessageAEAT(), InterchangeID);

		instruction.CEI_SubStyle = EntrySubStyleList.Codes.C;

		ProcessMessageForTest(message);
		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>10-06-2022</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES00999930006184</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>TEST444444444444</td></tr>" +
			"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>30-05-2022</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>PNS6NA3WMAUC4J8W</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Exit Type:</td><td>&nbsp;&nbsp;</td><td>[I] Indirect</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DE - Indirect Dispatch</td></tr></table>";
		AssertAcceptedExportDeclaration(message, entryStatusCode: EntryStatusCodes.Cleared, acceptanceDate: preparationDateTime, entryReleaseDate: preparationDateTime, csvT2L: CsvT2L, expectedMessageInterpretation: expectedMessageInterpretationText);
	}

	public void TestCreateDocumentCaptureRequestEDIMessageWhenCSVClearance_Export_AllDocs()
	{
		newDeclaration.ZG_CTStatusID = "T2LF";
		entryHeader.ZG_CSVExitCertificate = CsvExitCertificate;

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitResponseCodeLMessageAEAT(), InterchangeID);

		ProcessMessageForTest(message);

		CombineAssertions(() =>
		{
			entryHeader.Messages.Reload(true);
			var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
			AssertEquals("Doc Messages sent number is", 4, docMessages.Length);

			AssertDocumentRequestEDIMessages(docMessages,
				[(MRNCode + "_E_AEAT_CLR.pdf", CsvClearance),
				(MRNCode + "_E_AEAT_CLR_EXT.pdf", CsvExitCertificate),
				(MRNCode + "_E_AEAT_t2lf.pdf", CsvT2L),
				(MRNCode + "_E_AEAT_ead.pdf", MRNCode)]);
		});
	}

	public void TestCreateDocumentCaptureRequestEDIMessageWhenCSVClearance_Export_AllDocsExceptEADAndT2LF()
	{
		newDeclaration.ZG_CTStatusID = "T2LF";
		entryHeader.ZG_CSVExitCertificate = CsvExitCertificate;

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitResponseCodeBMessageATC(), InterchangeID);

		ProcessMessageForTest(message);

		CombineAssertions(() =>
		{
			entryHeader.Messages.Reload(true);
			var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
			AssertEquals("Doc Messages sent number is", 2, docMessages.Length);

			AssertDocumentRequestEDIMessages(docMessages,
				[(MRNCode + "_E_AEAT_CLR.pdf", CsvClearance),
				(MRNCode + "_E_AEAT_CLR_EXT.pdf", CsvExitCertificate)]);
		});
	}

	public void TestCreateDocumentCaptureRequestEDIMessageWhenCSVClearance_Export_NoDocs()
	{
		newDeclaration.ZG_CTStatusID = "T2LF";
		newDeclaration.ZG_IsTrainingDeclaration = false;
		entryHeader.ZG_CSVExitCertificate = CsvExitCertificate;
		entryHeader.ZG_CSVT2L = "CSVT2L";

		var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
		var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], MRNCode + "_E_AEAT_CLR.pdf", "CLR");
		var eDoc2 = docManagerInfo.AddFileOrDocument(new byte[1], MRNCode + "_E_AEAT_CLR_EXT.pdf", "CLR");
		var eDoc3 = docManagerInfo.AddFileOrDocument(new byte[1], MRNCode + "_E_AEAT_t2lf.pdf", "CAU");
		var eDoc4 = docManagerInfo.AddFileOrDocument(new byte[1], MRNCode + "_E_AEAT_ead.pdf", "EAD");
		docManagerInfo.Save();

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitResponseCodeLMessageAEAT(), InterchangeID);

		ProcessMessageForTest(message);

		CombineAssertions(() =>
		{
			docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
			var eDocs = docManagerInfo.GetRelatedEDocs();
			AssertEquals("Number of eDocs is correct", 4, eDocs.Count());

			var eDocNames = new List<ZString>()
								{ MRNCode + "_E_AEAT_CLR.pdf",
									MRNCode + "_E_AEAT_CLR_EXT.pdf",
									MRNCode + "_E_AEAT_t2lf.pdf",
									MRNCode + "_E_AEAT_ead.pdf" };
			AssertContainsExactElementsInAnyOrder("eDocs contains all documents with original names", eDocNames, eDocs.Cast<IeDoc>().Select(x => x.FileName).ToList());

			entryHeader.Messages.Reload(true);
			var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
			AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
		});
	}

	public void TestProcessAcceptedMessageCreatesSupportingDocumentsDuplicates()
	{
		var suppDoc1 = newDeclaration.SupportingDocuments.AddNew();
		suppDoc1.CSI_Code = "X001";
		suppDoc1.CSI_ReferenceNumber = "ES3600000001";

		var suppDoc2 = newDeclaration.SupportingDocuments.AddNew();
		suppDoc2.CSI_Code = "X002";
		suppDoc2.CSI_ReferenceNumber = "ES3600000002";

		var suppDoc21 = newDeclaration.SupportingDocuments.AddNew();
		suppDoc21.CSI_Code = "X002";
		suppDoc21.CSI_ReferenceNumber = "es3600000002";

		var entryInstruction = newDeclaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.A;
		var suppDoc3 = entryInstruction.SupportingDocuments.AddNew();
		suppDoc3.CSI_Code = "X001";
		suppDoc3.CSI_ReferenceNumber = "ES3600000001";

		var suppDoc4 = newDeclaration.SupportingDocuments.AddNew();
		suppDoc4.CSI_Code = "X003";
		suppDoc4.CSI_ReferenceNumber = "ES3600000003";

		Factory.Save();

		CombineAssertions(() =>
		{
			var chSupDocs = GetCHSupportingDocuments();
			AssertEquals("Before - There are 0 SupportingDocuments before calling ProcessImportEntryLineSupportingDocuments", 0, chSupDocs.Length);

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitResponseCodeLMessageAEAT(), InterchangeID);
			ProcessMessageForTest(message);

			var entryInstruction1 = newDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.A;
			var suppDoc5 = entryInstruction1.SupportingDocuments.AddNew();
			suppDoc5.CSI_Code = "X002";
			suppDoc5.CSI_ReferenceNumber = "ES3600000002";

			var suppDoc51 = entryInstruction1.SupportingDocuments.AddNew();
			suppDoc51.CSI_Code = "X002";
			suppDoc51.CSI_ReferenceNumber = "es3600000002";

			message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitResponseCodeLMessageAEAT(), InterchangeID);
			ProcessMessageForTest(message);

			var entryLine = entryHeader.MergedLines[0];
			var clSupDocs = GetCLSupportingDocuments(entryLine);
			AssertEquals("EntryLine 1, there are 3 entryLine SupportingDocuments after calling SaveSupportingDocumentsForCL for entryLine with CL_LineNumber " + entryLine.CL_LineNumber, 3, clSupDocs.Length);
			AssertContainsExactElementsInAnyOrder("EntryLine 1, the 3 entryLine SupportingDocuments after calling SaveSupportingDocumentsForCL have the correct CSI_Codes for entryLine with CL_LineNumber " + entryLine.CL_LineNumber, new ZString[] { "X001", "X002", "X003" }, clSupDocs.Select(x => x.CSI_Code).ToArray());

			entryLine = entryHeader.MergedLines[0];
			clSupDocs = GetCLSupportingDocuments(entryLine);
			AssertEquals("EntryLine 2, there are 3 entryLine SupportingDocuments after calling SaveSupportingDocumentsForCL for entryLine with CL_LineNumber " + entryLine.CL_LineNumber, 3, clSupDocs.Length);
			AssertContainsExactElementsInAnyOrder("EntryLine 2, the 3 entryLine SupportingDocuments after calling SaveSupportingDocumentsForCL have the correct CSI_Codes for entryLine with CL_LineNumber " + entryLine.CL_LineNumber, new ZString[] { "X001", "X002", "X003" }, clSupDocs.Select(x => x.CSI_Code).ToArray());
		});
	}

	public void TestProcessAcceptedMessageCreatesEntryLineSupportingDocuments()
	{
		var suppDoc1 = newDeclaration.SupportingDocuments.AddNew();
		suppDoc1.CSI_Code = "X001";
		suppDoc1.CSI_ReferenceNumber = "ES3600000001";

		var suppDoc2 = instruction.SupportingDocuments.AddNew();
		suppDoc2.CSI_Code = "X002";
		suppDoc2.CSI_ReferenceNumber = "ES3600000002";

		var suppDoc3 = invoiceLine.SupportingDocuments.AddNew();
		suppDoc3.CSI_Code = "X003";
		suppDoc3.CSI_ReferenceNumber = "ES3600000003";

		var suppDoc4 = invoiceLine.SupportingDocuments.AddNew();
		suppDoc4.CSI_Code = "N380";
		suppDoc4.CSI_ReferenceNumber = "ES36000N3801";
		suppDoc4.CSI_Status = ZString.Empty;

		entryLine.AddEntryLineDocument<SupportingDocument>("X004", "ES3600000004");
		entryLine.AddEntryLineDocument<SupportingDocument>("X005", "ES3600000005");
		entryLine.AddEntryLineDocument<SupportingDocument>("X006", "ES3600000006", subType: "LIQ", status: ZString.Empty);
		entryLine.Header.AddEntryHeaderDocument<SupportingDocument>("AAA", "ES3600000025");
		entryLine.Header.AddEntryHeaderDocument<SupportingDocument>("5018", "ES3600000022", subType: "LIQ", status: ZString.Empty);
		entryLine.Header.AddEntryHeaderDocument<SupportingDocument>("BBB", "ES3600000023", subType: "LIQ", status: "REJ");

		Factory.Save();

		CombineAssertions("Before", () =>
		{
			var clSupDocs = GetCLSupportingDocuments();
			AssertEquals("There are 3 CL SupportingDocuments before calling ProcessExportEntryLineSupportingDocuments", 3, clSupDocs.Length);
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments before calling ProcessExportEntryLineSupportingDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000004", "ES3600000005", "ES3600000006" }, clSupDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertEquals("The 3 EntryLine SupportingDocuments before calling ProcessExportEntryLineSupportingDocuments have the correct CSI_Status", false, clSupDocs.Any(x => x.CSI_Status == "ACC"));

			var chSupDocs = GetCHSupportingDocuments();
			AssertEquals("There are 3 CH SupportingDocuments before calling ProcessExportEntryLineSupportingDocuments", 3, chSupDocs.Length);
			AssertContainsExactElementsInAnyOrder("The 3 CH SupportingDocuments before calling ProcessExportEntryLineSupportingDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000025", "ES3600000022", "ES3600000023" }, chSupDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertEquals("The 3 EntryHeader SupportingDocuments before calling ProcessExportEntryLineSupportingDocuments have the correct CSI_Status", false, chSupDocs.Any(x => x.CSI_Status == "ACC"));
		});

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitResponseCodeLMessageAEAT(), InterchangeID);
		ProcessMessageForTest(message);

		CombineAssertions("For CL", () =>
		{
			var clSupDocs = GetCLSupportingDocuments();
			AssertEquals("There are 5 CL SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments", 5, clSupDocs.Length);
			AssertContainsExactElementsInAnyOrder("The 5 CL SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000001", "ES3600000002", "ES3600000003", "ES36000N3801", "ES3600000006" }, clSupDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertEquals("The 5 EntryLine SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments have the correct CSI_Status", false, clSupDocs.Any(x => x.CSI_Status != "ACC"));
		});

		CombineAssertions("For CH", () =>
		{
			var chSupDocs = GetCHSupportingDocuments();
			AssertEquals("There are 2 CH SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments", 2, chSupDocs.Length);
			AssertContainsExactElementsInAnyOrder("The 2 CH SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000022", "ES3600000023" }, chSupDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertEquals("The 2 EntryHeader SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments have the correct CSI_Status", false, chSupDocs.Any(x => x.CSI_Status != "ACC"));
		});
	}

	public void TestProcessAcceptedMessageCreatesEntryLinePreviousDocuments()
	{
		var prevDoc1 = invoiceLine.PreviousDocuments.AddNew();
		prevDoc1.CSI_Code = "X001";
		prevDoc1.CSI_ReferenceNumber = "ES3600000001";

		var prevDoc2 = invoiceLine.PreviousDocuments.AddNew();
		prevDoc2.CSI_Code = "X002";
		prevDoc2.CSI_ReferenceNumber = "ES3600000002";

		var prevDoc3 = invoiceLine.PreviousDocuments.AddNew();
		prevDoc3.CSI_Code = "X003";
		prevDoc3.CSI_ReferenceNumber = "ES3600000003";

		var prevDoc4 = invoiceLine.PreviousDocuments.AddNew();
		prevDoc4.CSI_Code = "N380";
		prevDoc4.CSI_ReferenceNumber = "ES36000N3801";
		prevDoc4.CSI_Status = ZString.Empty;

		entryLine.AddEntryLineDocument<PreviousDocument>("X004", "ES3600000004");
		entryLine.AddEntryLineDocument<PreviousDocument>("X005", "ES3600000005");

		Factory.Save();

		CombineAssertions("Before", () =>
		{
			var clPrevDocs = GetCLPreviousDocuments();
			AssertEquals("There are 2 CL PreviousDocuments before calling ProcessExportEntryLinePreviousDocuments", 2, clPrevDocs.Length);
			AssertContainsExactElementsInAnyOrder("The 2 CL PreviousDocuments before calling ProcessExportEntryLinePreviousDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000004", "ES3600000005" }, clPrevDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertEquals("The 2 EntryLine PreviousDocuments before calling ProcessExportEntryLinePreviousDocuments have the correct CSI_Status", false, clPrevDocs.Any(x => x.CSI_Status == "ACC"));
		});

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitResponseCodeLMessageAEAT(), InterchangeID);

		ProcessMessageForTest(message);

		CombineAssertions("After", () =>
		{
			var clPrevDocs = GetCLPreviousDocuments();
			AssertEquals("There are 4 CL PreviousDocuments after calling ProcessExportEntryLinePreviousDocuments", 4, clPrevDocs.Length);
			AssertContainsExactElementsInAnyOrder("The 4 CL PreviousDocuments after calling ProcessExportEntryLinePreviousDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000001", "ES3600000002", "ES3600000003", "ES36000N3801" }, clPrevDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertEquals("The 4 EntryLine PreviousDocuments after calling ProcessExportEntryLinePreviousDocuments have the correct CSI_Status", false, clPrevDocs.Any(x => x.CSI_Status != "ACC"));
		});
	}

	public void TestProcessAcceptedMessageCreatesEntryLineAdditionalInfos()
	{
		var prevDoc1 = newDeclaration.AdditionalInfos.AddNew();
		prevDoc1.CSI_Code = "X001";
		prevDoc1.CSI_ReferenceNumber = "ES3600000001";

		var prevDoc2 = instruction.AdditionalInfos.AddNew();
		prevDoc2.CSI_Code = "X002";
		prevDoc2.CSI_ReferenceNumber = "ES3600000002";

		var prevDoc3 = invoiceLine.AdditionalInfos.AddNew();
		prevDoc3.CSI_Code = "X003";
		prevDoc3.CSI_ReferenceNumber = "ES3600000003";

		var prevDoc4 = invoiceLine.AdditionalInfos.AddNew();
		prevDoc4.CSI_Code = "N380";
		prevDoc4.CSI_ReferenceNumber = "ES36000N3801";
		prevDoc4.CSI_Status = ZString.Empty;

		entryLine.AddEntryLineDocument<AdditionalInfo>("X004", "ES3600000004");
		entryLine.AddEntryLineDocument<AdditionalInfo>("X005", "ES3600000005");

		Factory.Save();

		CombineAssertions("Before", () =>
		{
			var clAddInfos = GetCLAdditionalInfos();
			AssertEquals("There are 2 CL AdditionalInfos before calling ProcessExportEntryLineAdditionalInfos", 2, clAddInfos.Length);
			AssertContainsExactElementsInAnyOrder("The 2 CL AdditionalInfos before calling ProcessExportEntryLineAdditionalInfos have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000004", "ES3600000005" }, clAddInfos.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertEquals("The 2 EntryLine AdditionalInfos before calling ProcessExportEntryLineAdditionalInfos have the correct CSI_Status", false, clAddInfos.Any(x => x.CSI_Status == "ACC"));
		});

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitResponseCodeLMessageAEAT(), InterchangeID);

		ProcessMessageForTest(message);

		CombineAssertions("After", () =>
		{
			var clAddInfos = GetCLAdditionalInfos();
			AssertEquals("There are 4 CL AdditionalInfos after calling ProcessExportEntryLineAdditionalInfos", 4, clAddInfos.Length);
			AssertContainsExactElementsInAnyOrder("The 4 CL AdditionalInfos after calling ProcessExportEntryLineAdditionalInfos have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000001", "ES3600000002", "ES3600000003", "ES36000N3801" }, clAddInfos.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertEquals("The 4 EntryLine AdditionalInfos after calling ProcessExportEntryLineAdditionalInfos have the correct CSI_Status", false, clAddInfos.Any(x => x.CSI_Status != "ACC"));
		});
	}

	public void TestProcessAcceptedMessageWithSentCMP_EntryInstructionSubStyle()
	{
		CombineAssertions("When EDP message type and CMP message sub type is accepted, update the sub style value in the related entry instruction from C to Y", () =>
		{
			AssertEntryInstruction("A", "A");
			AssertEntryInstruction("C", "C");

			SentMessage.EM_MessageSubType = "CMP";
			AssertEntryInstruction("A", "A");
			AssertEntryInstruction("C", "Y");
		});

		void AssertEntryInstruction(ZString initialSubStyle, ZString expectedSubStyle)
		{
			instruction.CEI_SubStyle = initialSubStyle;
			instruction.Factory.Save();

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileOrangeCircuitResponseCodeBMessageAEAT(), InterchangeID);

			ProcessMessageForTest(message);

			AssertEquals($"Entry Instruction SubStyle '{initialSubStyle}'", expectedSubStyle, instruction.CEI_SubStyle);
		}
	}

	public void TestProcessRejectedMessage_EntryStatusEmpty_TemporaryStorageRegisterNotEnabled()
	{
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		{
			var (regLineTransaction1, regLineTransaction2, regLineTransaction3) = SetUpTransactionsForRejectedTestForTemporaryStorageGoodsConsumption(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration);

			entryHeader.CH_EntryStatus = ZString.Empty;

			AddMessageProcessAndAssertResult_RejectedMessage(true);

			CombineAssertions(() =>
			{
				AssertEquals("regLineTransaction1's SRT_TransactionType was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2's SRT_TransactionType was not changed (not PND)", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction2.SRT_TransactionStatus);
				AssertEquals("regLineTransaction3's SRT_TransactionType was not changed (wrong internalRefType)", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction3.SRT_TransactionStatus);
			});
		}
	}

	public void TestProcessRejectedMessage_EntryStatusEmpty_TemporaryStorageEnabled()
	{
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (regLineTransaction1, regLineTransaction2, regLineTransaction3) = SetUpTransactionsForRejectedTestForTemporaryStorageGoodsConsumption(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration);

			entryHeader.CH_EntryStatus = ZString.Empty;

			AddMessageProcessAndAssertResult_RejectedMessage(true);

			CombineAssertions(() =>
			{
				AssertEquals("regLineTransaction1's SRT_TransactionType was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2's SRT_TransactionType was not changed (not PND)", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction2.SRT_TransactionStatus);
				AssertEquals("regLineTransaction3's SRT_TransactionType was not changed (wrong internalRefType)", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction3.SRT_TransactionStatus);
			});
		}
	}

	public void TestProcessRejectedMessage_EntryStatusNotEmpty_TemporaryStorageEnabled()
	{
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (regLineTransaction1, regLineTransaction2, regLineTransaction3) = SetUpTransactionsForRejectedTestForTemporaryStorageGoodsConsumption(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration);

			AddMessageProcessAndAssertResult_RejectedMessage(false);

			CombineAssertions(() =>
			{
				AssertEquals("regLineTransaction1's SRT_TransactionType was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2's SRT_TransactionType was not changed (not PND)", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction2.SRT_TransactionStatus);
				AssertEquals("regLineTransaction3's SRT_TransactionType was not changed (wrong internalRefType)", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction3.SRT_TransactionStatus);
			});
		}
	}

	public void TestLoggerNoWriteOffTransactionError()
	{
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			SetUpDataForConfirmTemporaryStorageGoodsConsumptionLoggerWriteOffTransactionError(entryHeader, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration);

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitResponseCodeLMessageAEAT(), InterchangeID);

			ProcessMessageForTest(message);
		}

		var expectedError = "Reference reference has a positive balance of 2.00 EUR. Please check the existing transactions for this reference and create a manual adjustment if needed.";
		AssertNotContains("Write off exception is not present in the logger exception", expectedError, GetAllConcatenatedUserLogStrings());
	}

	void AssertAcceptedExportDeclaration(TestEdiMessage message, string entryStatusCode, string circuit = CircuitCodeList.Codes.GREEN, string circuitCan = "", ZDateTime? acceptanceDate = null, string csvClearance = CsvClearance, ZDateTime? entryReleaseDate = null, string csvT2L = "", bool indirectExport = true, string expectedMessageInterpretation = "")
	{
		AssertExportDeclaration(message, entryStatusCode: entryStatusCode, circuit: circuit, circuitCan: circuitCan, acceptanceDate: acceptanceDate, mrn: MRNCode, csvClearance: csvClearance, entryReleaseDate: entryReleaseDate, csvT2L: csvT2L, indirectExport: indirectExport, expectedMessageInterpretation: expectedMessageInterpretation, messageSubType: "ACC");
	}

	void AssertExportDeclaration(TestEdiMessage message, string entryStatusCode, string messageSubType, string circuit = "", string circuitCan = "", ZDateTime? acceptanceDate = null, string mrn = "", string csvClearance = "", ZDateTime? entryReleaseDate = null, string csvT2L = "", bool indirectExport = false, string expectedMessageInterpretation = "")
	{
		AssertEquals("IndirectExport", indirectExport, entryHeader.IndirectExport);

		GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageSubType: messageSubType, entryStatusCode: entryStatusCode, circuit: circuit, circuitCan: circuitCan, acceptanceDate: acceptanceDate, movementReferenceNumber: mrn, csvClearance: csvClearance, entryReleaseDate: entryReleaseDate, csvT2L: csvT2L, messageNum: MessageNum);
	}

	string GetAcceptanceTestFileGreenCircuitResponseCodeLMessageAEAT() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DeclarationAESTestFilePath, "AcceptedGreenCircuitLMessageAEAT.txt");
	string GetAcceptanceTestFileRedCircuitResponseCodeLMessageAEAT() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DeclarationAESTestFilePath, "AcceptedRedCircuitLMessageAEAT.txt");
	string GetAcceptanceTestFileRedCircuitResponseCodeLMessageATC() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DeclarationAESTestFilePath, "AcceptedRedCircuitLMessageATC.txt");
	string GetAcceptanceTestFileOrangeCircuitResponseCodeBMessageAEAT() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DeclarationAESTestFilePath, "AcceptedOrangeCircuitBMessageAEAT.txt");
	string GetAcceptanceTestFileGreenCircuitResponseCodeBMessageATC() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DeclarationAESTestFilePath, "AcceptedGreenCircuitBMessageATC.txt");
	string GetAcceptanceTestFileOrangeCircuitResponseCodeBMessageATC() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DeclarationAESTestFilePath, "AcceptedOrangeCircuitBMessageATC.txt");
	string GetAcceptanceTestFileResponseCodeCMessage() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DeclarationAESTestFilePath, "AcceptedCMessage.txt");
	string GetAcceptanceTestFileResponseCodePMessage() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DeclarationAESTestFilePath, "AcceptedPMessage.txt");
	protected override string GetRejectedTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DeclarationAESTestFilePath, "RejectedMessage.txt");
	protected override string GetErrorTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DeclarationAESTestFilePath, "ErrorMessage.txt");
	protected override string GetAcceptanceTestFileWithLongSegmentId() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DeclarationAESTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

	readonly ZDateTime preparationDateTime = new DateTime(2022, 6, 10, 12, 34, 56);
	readonly ZDateTime entryReleaseDate = new ZDateTime(2022, 5, 30);
	const string CsvExitCertificate = "A1234567890";
	const string CsvT2L = "ABCDEFGHIJKLMNOP";

	protected override ZString GetExpectedProcessorFriendlyName() => "Export Declaration Message Processor";

	protected override ZString RejectedMRN => ZString.Empty;

	protected override ZDateTime RejectedAcceptanceDate => ZDateTime.Empty;

	protected override ZString RejectedMessageStatus => EDIMessage.Status.Rejected;

	protected override ZString ErrorMRN => ZString.Empty;

	protected override ZDateTime ErrorAcceptanceDate => ZDateTime.Empty;

	protected override ZString ErrorMessageInterpretation => "<H3>Rejected Declaration</H3>" +
			"<H4>List of Errors:</H4>" +
			"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
			"<tr><td><strong>Line / Column</strong></td><td><strong>Location</strong></td><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
			"<tr><td>14 / 34</td><td>782</td><td>18</td><td>Se esperaba nodo {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CC515CV1Ent.xsd}security y ha venido {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CC515CV1Ent.xsd}totalAmountInvoiced</td><td>Wrong value</td></tr>" +
			"</table>";

	protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => [DeclarationMessageTypeList.Codes.ExportUcc6, DeclarationMessageTypeList.Codes.ExportPreDeclaration];

	protected override DeclarationAESResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new DeclarationAESResponseMessageProcessor(logger, new BranchCustomsMessageProcessorForTest());
}
