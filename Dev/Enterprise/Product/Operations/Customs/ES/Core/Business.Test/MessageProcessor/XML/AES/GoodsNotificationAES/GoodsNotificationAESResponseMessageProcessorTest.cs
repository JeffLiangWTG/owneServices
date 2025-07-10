using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CC511C_v514.CC511CV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Testing;

public class GoodsNotificationAESResponseMessageProcessorTest : AESCommonResponseMessageProcessorTest<GoodsNotificationAESResponseMessageProcessor, Cc511Cv1Sal>
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
			AssertAcceptedExportNotification(message, entryStatusCode: EntryStatusCodes.Cleared, acceptanceDate: preparationDateTime, entryReleaseDate: preparationDateTime, csvT2L: CsvT2L, expectedMessageInterpretation: expectedMessageInterpretationText);

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
		AssertAcceptedExportNotification(message, entryStatusCode: EntryStatusCodes.Cleared, circuit: CircuitCodeList.Codes.RED, acceptanceDate: preparationDateTime, entryReleaseDate: preparationDateTime, expectedMessageInterpretation: expectedMessageInterpretationText);
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
		AssertAcceptedExportNotification(message, entryStatusCode: EntryStatusCodes.Cleared, circuitCan: CircuitCodeList.Codes.RED, acceptanceDate: preparationDateTime, entryReleaseDate: preparationDateTime, expectedMessageInterpretation: expectedMessageInterpretationText);
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
			AssertAcceptedExportNotification(message, entryStatusCode: EntryStatusCodes.CustomsDeclarationAccepted, circuit: CircuitCodeList.Codes.ORANGE, acceptanceDate: preparationDateTime, csvClearance: ZString.Empty, indirectExport: false, expectedMessageInterpretation: expectedMessageInterpretationText);

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
		AssertAcceptedExportNotification(message, entryStatusCode: EntryStatusCodes.CustomsDeclarationAccepted, circuitCan: CircuitCodeList.Codes.GREEN, acceptanceDate: preparationDateTime, entryReleaseDate: entryReleaseDate, indirectExport: false, expectedMessageInterpretation: expectedMessageInterpretationText);
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
		AssertAcceptedExportNotification(message, entryStatusCode: EntryStatusCodes.CustomsDeclarationAccepted, circuitCan: CircuitCodeList.Codes.ORANGE, acceptanceDate: preparationDateTime, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);
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
			AssertAcceptedExportNotification(message, entryStatusCode: EntryStatusCodes.PendingForEuOffice, circuit: ZString.Empty, acceptanceDate: preparationDateTime, csvClearance: "AAAAAAAAAAAAAAAA", indirectExport: false, expectedMessageInterpretation: expectedMessageInterpretationText);

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
			AssertAcceptedExportNotification(message, entryStatusCode: EntryStatusCodes.PendingForEuOffice, circuit: ZString.Empty, csvClearance: ZString.Empty, acceptanceDate: preparationDateTime, indirectExport: false, expectedMessageInterpretation: expectedMessageInterpretationText);

			AssertNewInboxMessages(entryHeader.Messages, [DeclarationMessageTypeList.Codes.ExportClearanceCommunication, DeclarationMessageTypeList.Codes.ExportNonConformityCommunication, DeclarationMessageTypeList.Codes.ExportCceControlCommunication], DeclarantId, DeclarantName, MRNCode);
		}
	}

	public void TestProcessAcceptedMessage_CLP_EntryInstructionB()
	{
		instruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.B;

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
		AssertAcceptedExportNotification(message, entryStatusCode: EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, acceptanceDate: preparationDateTime, entryReleaseDate: preparationDateTime, csvT2L: CsvT2L, expectedMessageInterpretation: expectedMessageInterpretationText);
	}

	public void TestProcessAcceptedMessage_CLP_EntryInstructionC()
	{
		instruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.C;

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
		AssertAcceptedExportNotification(message, entryStatusCode: EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, acceptanceDate: preparationDateTime, entryReleaseDate: preparationDateTime, csvT2L: CsvT2L, expectedMessageInterpretation: expectedMessageInterpretationText);
	}

	public void TestCreateDocumentCaptureRequestEDIMessageWhenCSVClearance_Export_AllDocs()
	{
		newDeclaration.ZG_CTStatusID = "T2LF";
		entryHeader.ZG_CSVExitCertificate = CsvExitCertificate;
		entryHeader.ZG_CSVT2L = "CSVT2L";
		entryHeader.IndirectExport = true;

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitResponseCodeLMessageAEAT(), InterchangeID);

		ProcessMessageForTest(message);

		CombineAssertions(() =>
		{
			entryHeader.Messages.Reload(true);
			var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
			AssertEquals("Doc Messages sent number is", 4, docMessages.Length);

			AssertDocumentRequestEDIMessages(docMessages, new List<(ZString fileName, ZString urlParameter)>() { (MRNCode + "_E_AEAT_CLR.pdf", CsvClearance),
																												(MRNCode + "_E_AEAT_CLR_EXT.pdf", CsvExitCertificate),
																												(MRNCode + "_E_AEAT_t2lf.pdf", CsvT2L),
																												(MRNCode + "_E_AEAT_ead.pdf", MRNCode) });
		});
	}

	public void TestCreateDocumentCaptureRequestEDIMessageWhenCSVClearance_Export_NoDocs()
	{
		newDeclaration.ZG_CTStatusID = "T2LF";
		newDeclaration.ZG_IsTrainingDeclaration = false;
		entryHeader.ZG_CSVExitCertificate = CsvExitCertificate;
		entryHeader.ZG_CSVT2L = "CSVT2L";
		entryHeader.IndirectExport = true;

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

	void AssertAcceptedExportNotification(TestEdiMessage message, string entryStatusCode, string circuit = CircuitCodeList.Codes.GREEN, string circuitCan = "", ZDateTime? acceptanceDate = null, string csvClearance = CsvClearance, ZDateTime? entryReleaseDate = null, string csvT2L = "", bool indirectExport = true, string expectedMessageInterpretation = "")
	{
		AssertExportNotification(message, entryStatusCode: entryStatusCode, circuit: circuit, circuitCan: circuitCan, acceptanceDate: acceptanceDate, mrn: MRNCode, csvClearance: csvClearance, entryReleaseDate: entryReleaseDate, csvT2L: csvT2L, indirectExport: indirectExport, expectedMessageInterpretation: expectedMessageInterpretation, messageSubType: "ACC");
	}

	void AssertExportNotification(TestEdiMessage message, string entryStatusCode, string messageSubType, string circuit = "", string circuitCan = "", ZDateTime? acceptanceDate = null, string mrn = "", string csvClearance = "", ZDateTime? entryReleaseDate = null, string csvT2L = "", bool indirectExport = false, string expectedMessageInterpretation = "")
	{
		AssertEquals("IndirectExport", indirectExport, entryHeader.IndirectExport);

		GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageSubType: messageSubType, entryStatusCode: entryStatusCode, circuit: circuit, circuitCan: circuitCan, acceptanceDate: acceptanceDate, movementReferenceNumber: mrn, csvClearance: csvClearance, entryReleaseDate: entryReleaseDate, csvT2L: csvT2L, messageNum: MessageNum);
	}

	string GetAcceptanceTestFileGreenCircuitResponseCodeLMessageAEAT() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.GoodsNotificationAESTestFilePath, "AcceptedGreenCircuitLMessageAEAT.txt");
	string GetAcceptanceTestFileRedCircuitResponseCodeLMessageAEAT() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.GoodsNotificationAESTestFilePath, "AcceptedRedCircuitLMessageAEAT.txt");
	string GetAcceptanceTestFileRedCircuitResponseCodeLMessageATC() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.GoodsNotificationAESTestFilePath, "AcceptedRedCircuitLMessageATC.txt");
	string GetAcceptanceTestFileOrangeCircuitResponseCodeBMessageAEAT() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.GoodsNotificationAESTestFilePath, "AcceptedOrangeCircuitBMessageAEAT.txt");
	string GetAcceptanceTestFileGreenCircuitResponseCodeBMessageATC() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.GoodsNotificationAESTestFilePath, "AcceptedGreenCircuitBMessageATC.txt");
	string GetAcceptanceTestFileOrangeCircuitResponseCodeBMessageATC() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.GoodsNotificationAESTestFilePath, "AcceptedOrangeCircuitBMessageATC.txt");
	string GetAcceptanceTestFileResponseCodeCMessage() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.GoodsNotificationAESTestFilePath, "AcceptedCMessage.txt");
	protected override string GetRejectedTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.GoodsNotificationAESTestFilePath, "RejectedMessage.txt");
	protected override string GetErrorTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.GoodsNotificationAESTestFilePath, "ErrorMessage.txt");
	protected override string GetAcceptanceTestFileWithLongSegmentId() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.GoodsNotificationAESTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

	readonly ZDateTime preparationDateTime = new DateTime(2022, 6, 10, 12, 34, 56);

	readonly ZDateTime entryReleaseDate = new ZDateTime(2022, 5, 30);
	const string CsvExitCertificate = "A1234567890";
	const string CsvT2L = "ABCDEFGHIJKLMNOP";

	protected override ZString GetExpectedProcessorFriendlyName() => "Export Notification Declaration Message Processor";

	protected override ZString RejectedMRN => ZString.Empty;

	protected override ZDateTime RejectedAcceptanceDate => ZDateTime.Empty;

	protected override ZString RejectedMessageStatus => EDIMessage.Status.Rejected;

	protected override ZString ErrorMRN => ZString.Empty;

	protected override ZDateTime ErrorAcceptanceDate => ZDateTime.Empty;

	protected override ZString ErrorMessageInterpretation => "<H3>Rejected Declaration</H3>" +
			"<H4>List of Errors:</H4>" +
			"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
			"<tr><td><strong>Line / Column</strong></td><td><strong>Location</strong></td><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
			"<tr><td>14 / 34</td><td>782</td><td>18</td><td>Se esperaba nodo {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CC511CXV1Ent.xsd}security y ha venido {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CC511CXV1Ent.xsd}totalAmountInvoiced</td><td>Wrong value</td></tr>" +
			"</table>";

	protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => [DeclarationMessageTypeList.Codes.ExportNotification];

	protected override GoodsNotificationAESResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new GoodsNotificationAESResponseMessageProcessor(logger, new BranchCustomsMessageProcessorForTest());
}
