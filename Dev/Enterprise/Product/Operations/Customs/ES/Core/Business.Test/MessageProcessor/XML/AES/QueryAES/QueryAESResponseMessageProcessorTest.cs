using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CCAESC_v514.CCAESCV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Testing;

public class QueryAESResponseMessageProcessorTest : AESCommonResponseMessageProcessorTest<QueryAESResponseMessageProcessor, Ccaescv1Sal>
{
	public void TestProcessAcceptedMessage_StatusPA_GreenCircuit_AndTriggerInboxRequest_EHub()
	{
		using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(false))
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusPAGreenCircuitMessageAEAT, InterchangeID);

			ProcessMessageForTest(message);
			var expectedStatusDescription = "PA - Pending Presentation of Goods";
			var expectedMessageInterpretationText = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus(expectedStatusDescription);
			AssertAcceptedExportQueryDeclaration(message, entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);

			AssertNewInboxMessages(entryHeader.Messages, new ZString[] { DeclarationMessageTypeList.Codes.ExportInvalidationCommunication }, DeclarantId, DeclarantName, MRNCode);
		}
	}

	public void TestProcessAcceptedMessage_StatusPA_RedCircuit()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusPARedCircuitMessageAEAT, InterchangeID);

		ProcessMessageForTest(message);
		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<H4>Management Data</H4>" +
			"<br><table border=\"0\"><tr><td>Version:</td><td>&nbsp;&nbsp;</td><td>AES (XML)</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PA - Pending Presentation of Goods</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>11-10-2022</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES00999930006184</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>TEST444444444444</td></tr>" +
			"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>12-10-2022</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Exit Type:</td><td>&nbsp;&nbsp;</td><td>[I] Indirect</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Exit Result:</td><td>&nbsp;&nbsp;</td><td>A2 - Satisfied</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Effective Departure Clearance:</td><td>&nbsp;&nbsp;</td><td>64LQAGQ4Z4FYK8L7</td></tr>" +
			"<tr><td>Effective Departure Date:</td><td>&nbsp;&nbsp;</td><td>13-10-2022</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Exit Goods Date:</td><td>&nbsp;&nbsp;</td><td>10-10-2022</td></tr></table>" +
			"<H4>Exit Control</H4>" +
			"<br><table border=\"0\"><tr><td>Arrival Date:</td><td>&nbsp;&nbsp;</td><td>14-10-2022</td></tr>" +
			"<tr><td>Arrival Location:</td><td>&nbsp;&nbsp;</td><td>place</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>AAAAAAAAAAAAAAAA</td></tr>" +
			"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>15-10-2022</td></tr></table>";
		AssertAcceptedExportQueryDeclaration(message, entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, circuit: CircuitCodeList.Codes.RED, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);
	}

	public void TestProcessAcceptedMessage_StatusPA_GreenCircuit_ATC()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusPAGreenCircuitMessageATC, InterchangeID);

		ProcessMessageForTest(message);
		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<H4>Management Data</H4>" +
			"<br><table border=\"0\"><tr><td>Version:</td><td>&nbsp;&nbsp;</td><td>AES (XML)</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PA - Pending Presentation of Goods</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>11-10-2022</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES00999930006184</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr>" +
			"<tr><td>ATC Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>TEST444444444444</td></tr>" +
			"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>12-10-2022</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Exit Type:</td><td>&nbsp;&nbsp;</td><td>[I] Indirect</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Exit Result:</td><td>&nbsp;&nbsp;</td><td>A2 - Satisfied</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Effective Departure Clearance:</td><td>&nbsp;&nbsp;</td><td>64LQAGQ4Z4FYK8L7</td></tr>" +
			"<tr><td>Effective Departure Date:</td><td>&nbsp;&nbsp;</td><td>13-10-2022</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Exit Goods Date:</td><td>&nbsp;&nbsp;</td><td>10-10-2022</td></tr></table>" +
			"<H4>Exit Control</H4>" +
			"<br><table border=\"0\"><tr><td>Arrival Date:</td><td>&nbsp;&nbsp;</td><td>14-10-2022</td></tr>" +
			"<tr><td>Arrival Location:</td><td>&nbsp;&nbsp;</td><td>place</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>AAAAAAAAAAAAAAAA</td></tr>" +
			"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>15-10-2022</td></tr></table>";
		AssertAcceptedExportQueryDeclaration(message, entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, circuitCan: CircuitCodeList.Codes.GREEN, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);
	}

	public void TestProcessAcceptedMessage_StatusPA_OrangeCircuit_ATC()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusPAOrangeCircuitMessageATC, InterchangeID);

		ProcessMessageForTest(message);
		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<H4>Management Data</H4>" +
			"<br><table border=\"0\"><tr><td>Version:</td><td>&nbsp;&nbsp;</td><td>AES (XML)</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PA - Pending Presentation of Goods</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>11-10-2022</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES00999930006184</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr>" +
			"<tr><td>ATC Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>TEST444444444444</td></tr>" +
			"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>12-10-2022</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Exit Type:</td><td>&nbsp;&nbsp;</td><td>[I] Indirect</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Exit Result:</td><td>&nbsp;&nbsp;</td><td>A2 - Satisfied</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Effective Departure Clearance:</td><td>&nbsp;&nbsp;</td><td>64LQAGQ4Z4FYK8L7</td></tr>" +
			"<tr><td>Effective Departure Date:</td><td>&nbsp;&nbsp;</td><td>13-10-2022</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Exit Goods Date:</td><td>&nbsp;&nbsp;</td><td>10-10-2022</td></tr></table>" +
			"<H4>Exit Control</H4>" +
			"<br><table border=\"0\"><tr><td>Arrival Date:</td><td>&nbsp;&nbsp;</td><td>14-10-2022</td></tr>" +
			"<tr><td>Arrival Location:</td><td>&nbsp;&nbsp;</td><td>place</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>AAAAAAAAAAAAAAAA</td></tr>" +
			"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>15-10-2022</td></tr></table>";
		AssertAcceptedExportQueryDeclaration(message, entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, circuitCan: CircuitCodeList.Codes.ORANGE, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);
	}

	public void TestProcessAcceptedMessage_StatusPI()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusPIMessage, InterchangeID);

		ProcessMessageForTest(message);
		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<H4>Management Data</H4>" +
			"<br><table border=\"0\"><tr><td>Version:</td><td>&nbsp;&nbsp;</td><td>AES (XML)</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PI - Pre-Declaration Invalidated</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES00999930006184</td></tr></table>";
		AssertAcceptedExportQueryDeclaration(message, entryStatusCode: EntryStatusCodes.Invalidated, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);
	}

	public void TestProcessAcceptedMessage_StatusNL()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusNLMessage, InterchangeID);

		ProcessMessageForTest(message);
		var expectedStatusDescription = "NL - Not cleared";
		var expectedMessageInterpretationText = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus(expectedStatusDescription);
		AssertAcceptedExportQueryDeclaration(message, entryStatusCode: EntryStatusCodes.Invalidated, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);
	}

	public void TestProcessAcceptedMessage_StatusPL()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusPLMessage, InterchangeID);

		ProcessMessageForTest(message);
		var expectedStatusDescription = "PL - Pending Clearance";
		var expectedMessageInterpretationText = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus(expectedStatusDescription);
		AssertAcceptedExportQueryDeclaration(message, entryStatusCode: EntryStatusCodes.CustomsDeclarationAccepted, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);
	}

	public void TestProcessAcceptedMessage_StatusCA()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusCAMessage, InterchangeID);

		ProcessMessageForTest(message);
		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<H4>Management Data</H4>" +
			"<br><table border=\"0\"><tr><td>Version:</td><td>&nbsp;&nbsp;</td><td>AES (XML)</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>CA - Declaration Canceled</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES00999930006184</td></tr></table>";
		AssertAcceptedExportQueryDeclaration(message, entryStatusCode: EntryStatusCodes.Cancelled, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);
	}

	public void TestProcessAcceptedMessage_StatusSA()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusSAMessage, InterchangeID);

		ProcessMessageForTest(message);
		var expectedStatusDescription = "SA - Effective Exit";
		var expectedMessageInterpretationText = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus(expectedStatusDescription);
		AssertAcceptedExportQueryDeclaration(message, entryStatusCode: EntryStatusCodes.EffectiveDeparture, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);
	}

	public void TestProcessAcceptedMessage_StatusST()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusSTMessage, InterchangeID);

		ProcessMessageForTest(message);
		var expectedStatusDescription = "ST - Stop at Exit";
		var expectedMessageInterpretationText = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus(expectedStatusDescription);
		AssertAcceptedExportQueryDeclaration(message, entryStatusCode: EntryStatusCodes.GoodsStoppedAtDeparture, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);
	}

	public void TestProcessAcceptedMessage_StatusAW_WithCSVClearance_AndTriggerInboxRequest_EHub()
	{
		using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(false))
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusAWWithCSVClearanceMessage, InterchangeID);

			ProcessMessageForTest(message);
			var expectedStatusDescription = "AW - Waiting PCO Decision";
			var expectedMessageInterpretationText = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus(expectedStatusDescription);
			AssertAcceptedExportQueryDeclaration(message, entryStatusCode: EntryStatusCodes.PendingForEuOffice, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);

			AssertNewInboxMessages(entryHeader.Messages, new ZString[] { DeclarationMessageTypeList.Codes.ExportInvalidationCommunication }, DeclarantId, DeclarantName, MRNCode);
		}
	}

	public void TestProcessAcceptedMessage_StatusAW_WithoutCSVClearance_AndTriggerInboxRequest_EHub()
	{
		using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(false))
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusAWNoCSVClearanceMessage, InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<H4>Management Data</H4>" +
				"<br><table border=\"0\"><tr><td>Version:</td><td>&nbsp;&nbsp;</td><td>AES (XML)</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>AW - Waiting PCO Decision</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>11-10-2022</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES00999930006184</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Exit Type:</td><td>&nbsp;&nbsp;</td><td>[I] Indirect</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Exit Result:</td><td>&nbsp;&nbsp;</td><td>A2 - Satisfied</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Effective Departure Clearance:</td><td>&nbsp;&nbsp;</td><td>64LQAGQ4Z4FYK8L7</td></tr>" +
				"<tr><td>Effective Departure Date:</td><td>&nbsp;&nbsp;</td><td>13-10-2022</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Exit Goods Date:</td><td>&nbsp;&nbsp;</td><td>10-10-2022</td></tr></table>" +
				"<H4>Exit Control</H4>" +
				"<br><table border=\"0\"><tr><td>Arrival Date:</td><td>&nbsp;&nbsp;</td><td>14-10-2022</td></tr>" +
				"<tr><td>Arrival Location:</td><td>&nbsp;&nbsp;</td><td>place</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>AAAAAAAAAAAAAAAA</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>15-10-2022</td></tr></table>";
			AssertAcceptedExportQueryDeclaration(message, entryStatusCode: EntryStatusCodes.PendingForEuOffice, csvClearance: ZString.Empty, expectedMessageInterpretation: expectedMessageInterpretationText);

			AssertNewInboxMessages(entryHeader.Messages, new ZString[] { DeclarationMessageTypeList.Codes.ExportClearanceCommunication, DeclarationMessageTypeList.Codes.ExportNonConformityCommunication, DeclarationMessageTypeList.Codes.ExportCceControlCommunication }, DeclarantId, DeclarantName, MRNCode);
		}
	}

	public void TestProcessAcceptedMessage_StatusIN_INV()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusIVINVMessage, InterchangeID);

		ProcessMessageForTest(message);
		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<H4>Management Data</H4>" +
			"<br><table border=\"0\"><tr><td>Version:</td><td>&nbsp;&nbsp;</td><td>AES (XML)</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>IV - Invalidated</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Invalidation Date:</td><td>&nbsp;&nbsp;</td><td>14-10-2022</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES00999930006184</td></tr></table>";
		AssertAcceptedExportQueryDeclaration(message, entryStatusCode: EntryStatusCodes.Invalidated, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);
	}

	public void TestProcessAcceptedMessage_StatusIN_CAN()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusIVCANMessage, InterchangeID);

		ProcessMessageForTest(message);
		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<H4>Management Data</H4>" +
			"<br><table border=\"0\"><tr><td>Version:</td><td>&nbsp;&nbsp;</td><td>AES (XML)</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>IV - Invalidated</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES00999930006184</td></tr></table>";
		AssertAcceptedExportQueryDeclaration(message, entryStatusCode: EntryStatusCodes.Cancelled, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);
	}

	public void TestProcessAcceptedMessage_NoCSVClearance_CDA_AndTriggerInboxRequest_xT()
	{
		using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(true))
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusOtherNoCSVClearanceMessage, InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<H4>Management Data</H4>" +
				"<br><table border=\"0\"><tr><td>Version:</td><td>&nbsp;&nbsp;</td><td>AES (XML)</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DE - Indirect Dispatch</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>11-10-2022</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES00999930006184</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Exit Type:</td><td>&nbsp;&nbsp;</td><td>[I] Indirect</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Exit Result:</td><td>&nbsp;&nbsp;</td><td>A2 - Satisfied</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Effective Departure Clearance:</td><td>&nbsp;&nbsp;</td><td>64LQAGQ4Z4FYK8L7</td></tr>" +
				"<tr><td>Effective Departure Date:</td><td>&nbsp;&nbsp;</td><td>13-10-2022</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Exit Goods Date:</td><td>&nbsp;&nbsp;</td><td>10-10-2022</td></tr></table>";
			AssertAcceptedExportQueryDeclaration(message, entryStatusCode: EntryStatusCodes.CustomsDeclarationAccepted, csvClearance: ZString.Empty, csvT2L: ZString.Empty, expectedMessageInterpretation: expectedMessageInterpretationText);

			AssertNewCusPollingTransaction(entryHeader.PK, entryHeader.TablePrefix, new ZString[] { DeclarationMessageTypeList.Codes.ExportClearanceCommunication, DeclarationMessageTypeList.Codes.ExportNonConformityCommunication }, MRNCode);
		}
	}

	public void TestProcessAcceptedMessage_WithCSVClearance_CLP()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusOtherWithCSVClearanceMessage, InterchangeID);

		ProcessMessageForTest(message);
		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<H4>Management Data</H4>" +
			"<br><table border=\"0\"><tr><td>Version:</td><td>&nbsp;&nbsp;</td><td>AES (XML)</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DS - Direct Dispatch</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>11-10-2022</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES00999930006184</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>TEST444444444444</td></tr>" +
			"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>12-10-2022</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Exit Type:</td><td>&nbsp;&nbsp;</td><td>[I] Indirect</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Exit Result:</td><td>&nbsp;&nbsp;</td><td>A2 - Satisfied</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Effective Departure Clearance:</td><td>&nbsp;&nbsp;</td><td>64LQAGQ4Z4FYK8L7</td></tr>" +
			"<tr><td>Effective Departure Date:</td><td>&nbsp;&nbsp;</td><td>13-10-2022</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Exit Goods Date:</td><td>&nbsp;&nbsp;</td><td>10-10-2022</td></tr></table>";
		AssertAcceptedExportQueryDeclaration(message, entryStatusCode: EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);
	}

	public void TestProcessAcceptedMessage_WithCSVClearance_EFD()
	{
		instruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.Z;
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusOtherWithCSVClearanceMessage, InterchangeID);

		ProcessMessageForTest(message);
		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<H4>Management Data</H4>" +
			"<br><table border=\"0\"><tr><td>Version:</td><td>&nbsp;&nbsp;</td><td>AES (XML)</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DS - Direct Dispatch</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>11-10-2022</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES00999930006184</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>TEST444444444444</td></tr>" +
			"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>12-10-2022</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Exit Type:</td><td>&nbsp;&nbsp;</td><td>[I] Indirect</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Exit Result:</td><td>&nbsp;&nbsp;</td><td>A2 - Satisfied</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Effective Departure Clearance:</td><td>&nbsp;&nbsp;</td><td>64LQAGQ4Z4FYK8L7</td></tr>" +
			"<tr><td>Effective Departure Date:</td><td>&nbsp;&nbsp;</td><td>13-10-2022</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Exit Goods Date:</td><td>&nbsp;&nbsp;</td><td>10-10-2022</td></tr></table>";
		AssertAcceptedExportQueryDeclaration(message, entryStatusCode: EntryStatusCodes.EffectiveDeparture, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);
	}

	public void TestProcessAcceptedMessage_WithCSVClearance_CLR_AndTriggerInboxRequest_EHub()
	{
		using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(false))
		{
			instruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.A;
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusOtherWithCSVClearanceMessage, InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<H4>Management Data</H4>" +
				"<br><table border=\"0\"><tr><td>Version:</td><td>&nbsp;&nbsp;</td><td>AES (XML)</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DS - Direct Dispatch</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>11-10-2022</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES00999930006184</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>TEST444444444444</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>12-10-2022</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Exit Type:</td><td>&nbsp;&nbsp;</td><td>[I] Indirect</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Exit Result:</td><td>&nbsp;&nbsp;</td><td>A2 - Satisfied</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Effective Departure Clearance:</td><td>&nbsp;&nbsp;</td><td>64LQAGQ4Z4FYK8L7</td></tr>" +
				"<tr><td>Effective Departure Date:</td><td>&nbsp;&nbsp;</td><td>13-10-2022</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Exit Goods Date:</td><td>&nbsp;&nbsp;</td><td>10-10-2022</td></tr></table>";
			AssertAcceptedExportQueryDeclaration(message, entryStatusCode: EntryStatusCodes.Cleared, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);

			AssertNewInboxMessages(entryHeader.Messages, new ZString[] { DeclarationMessageTypeList.Codes.ExportExitResultCommunication, DeclarationMessageTypeList.Codes.ExportInvalidationCommunication }, DeclarantId, DeclarantName, MRNCode);
		}
	}

	public void TestProcessAcceptedMessage_GoodsShipment_AllDocuments()
	{
		AddSupportingDocuments_EntryHeader();
		AddAdditionalInformation_EntryHeader();
		AddAdditionalReference_EntryHeader();
		AddTransportDocument_EntryHeader();

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileWithGoodsShipmentAllDocumentsMessage, InterchangeID);

		ProcessMessageForTest(message);
		var expectedStatusDescription = "PA - Pending Presentation of Goods";
		var expectedMessageInterpretationText = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus(expectedStatusDescription);
		AssertAcceptedExportQueryDeclaration(message, entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);

		CombineAssertions(() =>
		{
			var chEntryHeaderSupDocs = GetCHSupportingDocuments(entryHeader);
			AssertContainsExactElementsInAnyOrder("The 3 CH SupportingDocuments after processing have the correct CSI_Codes for entryHeader.", new ZString[] { "Sup1", "Sup2", "Sup3" }, chEntryHeaderSupDocs.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CH SupportingDocuments after processing have the correct CSI_ReferenceNumber for entryHeader.", new ZString[] { "Ref1", "Ref2", "Ref3" }, chEntryHeaderSupDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CH SupportingDocuments after processing have the correct CSI_ItemNumber for entryHeader.", new ZInt[] { 123, 133, 1233 }, chEntryHeaderSupDocs.Select(x => x.CSI_ItemNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CH SupportingDocuments after processing have the correct CSI_AdditionalDescription for entryHeader.", new ZString[] { "name1", "name2", "name3" }, chEntryHeaderSupDocs.Select(x => x.CSI_AdditionalDescription).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CH SupportingDocuments after processing have the correct CSI_DateOfIssue for entryHeader.", new ZDateTime[] { new ZDateTime(2022, 09, 12), new ZDateTime(2022, 09, 10), new ZDateTime(2022, 09, 01) }, chEntryHeaderSupDocs.Select(x => x.CSI_DateOfIssue).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CH SupportingDocuments after processing have the correct CSI_Status for entryHeader.", new ZString[] { "ACC", "ACC", "ACC" }, chEntryHeaderSupDocs.Select(x => x.CSI_Status).ToArray());

			var chEntryHeaderAddRefs = GetCHAdditionalInfo(entryHeader).Where(doc => doc.CSI_SubType == "REF");
			AssertContainsExactElementsInAnyOrder("The 3 CH AdditionalReference after processing have the correct CSI_Codes for entryHeader.", new ZString[] { "Cod1", "Cod2", "Cod3" }, chEntryHeaderAddRefs.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CH AdditionalReference after processing have the correct CSI_ReferenceNumber for entryHeader.", new ZString[] { "Ref1", "Ref2", "Ref3" }, chEntryHeaderAddRefs.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CH AdditionalReference after processing have the correct CSI_Status for entryHeader.", new ZString[] { "ACC", "ACC", "ACC" }, chEntryHeaderAddRefs.Select(x => x.CSI_Status).ToArray());

			var chEntryHeaderAddInfo = GetCHAdditionalInfo(entryHeader).Where(doc => doc.CSI_SubType == "INF");
			AssertContainsExactElementsInAnyOrder("The 3 CH AdditionalInformation after processing have the correct CSI_Codes for entryHeader.", new ZString[] { "Inf1", "Inf2", "Inf3" }, chEntryHeaderAddInfo.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CH AdditionalInformation after processing have the correct CSI_ReferenceNumber for entryHeader.", new ZString[] { "Ref1", "Ref2", "Ref3" }, chEntryHeaderAddInfo.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CH AdditionalInformation after processing have the correct CSI_Status for entryHeader.", new ZString[] { "ACC", "ACC", "ACC" }, chEntryHeaderAddInfo.Select(x => x.CSI_Status).ToArray());

			var chEntryHeaderTransDocs = GetCHAdditionalInfo(entryHeader).Where(doc => doc.CSI_SubType == "TRA");
			AssertContainsExactElementsInAnyOrder("The 3 CH TransportDocument after processing have the correct CSI_Codes for entryHeader.", new ZString[] { "Tra1", "Tra2", "Tra3" }, chEntryHeaderTransDocs.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CH TransportDocument after processing have the correct CSI_ReferenceNumber for entryHeader.", new ZString[] { "ref1", "ref2", "ref3" }, chEntryHeaderTransDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CH TransportDocument after processing have the correct CSI_Status for entryHeader.", new ZString[] { "ACC", "ACC", "ACC" }, chEntryHeaderTransDocs.Select(x => x.CSI_Status).ToArray());
		});
	}
	
	public void TestProcessAcceptedMessage_GoodsShipment_SupportingDocuments()
	{
		AddSupportingDocuments_EntryHeader();
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileWithGoodsShipmentSupportingDocumentsMessage, InterchangeID);

		ProcessMessageForTest(message);
		var expectedStatusDescription = "PA - Pending Presentation of Goods";
		var expectedMessageInterpretationText = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus(expectedStatusDescription);
		AssertAcceptedExportQueryDeclaration(message, entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);

		CombineAssertions(() =>
		{
			var chEntryHeaderSupDocs = GetCHSupportingDocuments(entryHeader);
			AssertContainsExactElementsInAnyOrder("The 3 CH SupportingDocuments after processing have the correct CSI_Codes for entryHeader.", new ZString[] { "Sup1", "Sup2", "Sup3" }, chEntryHeaderSupDocs.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CH SupportingDocuments after processing have the correct CSI_ReferenceNumber for entryHeader.", new ZString[] { "Ref1", "Ref2", "Ref3" }, chEntryHeaderSupDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CH SupportingDocuments after processing have the correct CSI_ItemNumber for entryHeader.", new ZInt[] { 123, 133, 1233 }, chEntryHeaderSupDocs.Select(x => x.CSI_ItemNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CH SupportingDocuments after processing have the correct CSI_AdditionalDescription for entryHeader.", new ZString[] { "name1", "name2", "name3" }, chEntryHeaderSupDocs.Select(x => x.CSI_AdditionalDescription).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CH SupportingDocuments after processing have the correct CSI_DateOfIssue for entryHeader.", new ZDateTime[] { new ZDateTime(2022, 09, 12), new ZDateTime(2022, 09, 10), new ZDateTime(2022, 09, 01) }, chEntryHeaderSupDocs.Select(x => x.CSI_DateOfIssue).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CH SupportingDocuments after processing have the correct CSI_Status for entryHeader.", new ZString[] { "ACC", "ACC", "ACC" }, chEntryHeaderSupDocs.Select(x => x.CSI_Status).ToArray());
		});
	}

	public void TestProcessAcceptedMessage_GoodsShipment_AdditionalReference()
	{
		AddAdditionalReference_EntryHeader();
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileWithGoodsShipmentAdditionalReferenceMessage, InterchangeID);

		ProcessMessageForTest(message);
		var expectedStatusDescription = "PA - Pending Presentation of Goods";
		var expectedMessageInterpretationText = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus(expectedStatusDescription);
		AssertAcceptedExportQueryDeclaration(message, entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);

		CombineAssertions(() =>
		{
			var chEntryHeader = GetCHAdditionalInfo(entryHeader).Where(doc => doc.CSI_SubType == "REF");
			AssertContainsExactElementsInAnyOrder("The 3 CH AdditionalReference after processing have the correct CSI_Codes for entryHeader.", new ZString[] { "Cod1", "Cod2", "Cod3" }, chEntryHeader.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CH AdditionalReference after processing have the correct CSI_ReferenceNumber for entryHeader.", new ZString[] { "Ref1", "Ref2", "Ref3" }, chEntryHeader.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CH AdditionalReference after processing have the correct CSI_Status for entryHeader.", new ZString[] { "ACC", "ACC", "ACC" }, chEntryHeader.Select(x => x.CSI_Status).ToArray());
		});
	}

	public void TestProcessAcceptedMessage_GoodsShipment_AdditionalInformation()
	{
		AddAdditionalInformation_EntryHeader();
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileWithGoodsShipmentAdditionalInformationMessage, InterchangeID);

		ProcessMessageForTest(message);
		var expectedStatusDescription = "PA - Pending Presentation of Goods";
		var expectedMessageInterpretationText = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus(expectedStatusDescription);
		AssertAcceptedExportQueryDeclaration(message, entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);

		CombineAssertions(() =>
		{
			var chEntryHeader = GetCHAdditionalInfo(entryHeader).Where(doc => doc.CSI_SubType == "INF");
			AssertContainsExactElementsInAnyOrder("The 3 CH AdditionalInformation after processing have the correct CSI_Codes for entryHeader.", new ZString[] { "Inf1", "Inf2", "Inf3" }, chEntryHeader.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CH AdditionalInformation after processing have the correct CSI_ReferenceNumber for entryHeader.", new ZString[] { "Ref1", "Ref2", "Ref3" }, chEntryHeader.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CH AdditionalInformation after processing have the correct CSI_Status for entryHeader.", new ZString[] { "ACC", "ACC", "ACC" }, chEntryHeader.Select(x => x.CSI_Status).ToArray());
		});
	}

	public void TestProcessAcceptedMessage_GoodsShipment_Consignment_TransportDocument()
	{
		AddTransportDocument_EntryHeader();
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileWithGoodsShipmentConsignmentTransportDocumentMessage, InterchangeID);

		ProcessMessageForTest(message);
		var expectedStatusDescription = "PA - Pending Presentation of Goods";
		var expectedMessageInterpretationText = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus(expectedStatusDescription);
		AssertAcceptedExportQueryDeclaration(message, entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);

		CombineAssertions(() =>
		{
			var chEntryHeader = GetCHAdditionalInfo(entryHeader).Where(doc => doc.CSI_SubType == "TRA");
			AssertContainsExactElementsInAnyOrder("The 3 CH TransportDocument after processing have the correct CSI_Codes for entryHeader.", new ZString[] { "Tra1", "Tra2", "Tra3" }, chEntryHeader.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CH TransportDocument after processing have the correct CSI_ReferenceNumber for entryHeader.", new ZString[] { "ref1", "ref2", "ref3" }, chEntryHeader.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CH TransportDocument after processing have the correct CSI_Status for entryHeader.", new ZString[] { "ACC", "ACC", "ACC" }, chEntryHeader.Select(x => x.CSI_Status).ToArray());
		});
	}

	public void TestProcessAcceptedMessage_GoodsItem_AdditionalReference()
	{
		AddAdditionalReference_EntryLine();
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileWithGoodsItemAdditionalReferencesMessage, InterchangeID);

		ProcessMessageForTest(message);
		var expectedStatusDescription = "PA - Pending Presentation of Goods";
		var expectedMessageInterpretationText = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus(expectedStatusDescription);
		AssertAcceptedExportQueryDeclaration(message, entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);

		CombineAssertions(() =>
		{
			var entryLine1RefDocuments = GetCLAdditionalInfos(entryLine1).Where(doc => doc.CSI_SubType == "REF");
			AssertEquals("There are 3 CL AdditionReference Documents after processing for entryLine " + entryLine1.CL_LineNumber, 3, entryLine1RefDocuments.ToArray().Length);
			AssertContainsExactElementsInAnyOrder("The 3 CL AdditionalReference after processing have the correct CSI_Codes for entryLine1.", new ZString[] { "Y924", "Y925", "Y926" }, entryLine1RefDocuments.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL AdditionalReference after processing have the correct CSI_ReferenceNumber for entryLine1.", new ZString[] { "DE89564", "DE89565", "DE89566" }, entryLine1RefDocuments.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL AdditionalReference after processing have the correct CSI_Status for entryLine1.", new ZString[] { "ACC", "ACC", "ACC" }, entryLine1RefDocuments.Select(x => x.CSI_Status).ToArray());

			var entryLine2RefDocuments = GetCLAdditionalInfos(entryLine2).Where(doc => doc.CSI_SubType == "REF");
			AssertEquals("There are 2 CL AdditionReference Documents after processing for entryLine " + entryLine2.CL_LineNumber, 2, entryLine2RefDocuments.ToArray().Length);
			AssertContainsExactElementsInAnyOrder("The 2 CL AdditionalReference after processing have the correct CSI_Codes for entryLine2.", new ZString[] { "AAAA", "BBBB" }, entryLine2RefDocuments.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 2 CL AdditionalReference after processing have the correct CSI_ReferenceNumber for entryLine2.", new ZString[] { "REFERENCE1", "REFERENCE2" }, entryLine2RefDocuments.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 2 CL AdditionalReference after processing have the correct CSI_Status for entryLine2.", new ZString[] { "ACC", "ACC" }, entryLine2RefDocuments.Select(x => x.CSI_Status).ToArray());

			var entryLine3RefDocuments = GetCLAdditionalInfos(entryLine3).Where(doc => doc.CSI_SubType == "REF");
			AssertEquals("There are no AdditionReference Documents after processing for entryLine " + entryLine3.CL_LineNumber, 0, entryLine3RefDocuments.ToArray().Length);

			var entryLine4RefDocuments = GetCLAdditionalInfos(entryLine4).Where(doc => doc.CSI_SubType == "REF");
			AssertEquals("There are 2 CL AdditionReference Documents after processing for entryLine " + entryLine4.CL_LineNumber, 2, entryLine4RefDocuments.ToArray().Length);
			AssertContainsExactElementsInAnyOrder("The 2 CL AdditionalReference after processing have the correct CSI_Codes for entryLine4.", new ZString[] { "X047", "X049" }, entryLine4RefDocuments.Select(x => x.CSI_Code).ToArray());
		});
	}

	public void TestProcessAcceptedMessage_GoodsItem_TransportDocument()
	{
		AddTransportDocument_EntryLine();
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileWithGoodsItemTransportDocumentMessage, InterchangeID);

		ProcessMessageForTest(message);
		var expectedStatusDescription = "PA - Pending Presentation of Goods";
		var expectedMessageInterpretationText = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus(expectedStatusDescription);
		AssertAcceptedExportQueryDeclaration(message, entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);

		CombineAssertions(() =>
		{
			var entryLine1TransDocs = GetCLAdditionalInfos(entryLine1).Where(doc => doc.CSI_SubType == "TRA").ToArray();
			AssertEquals("There are 3 CL TransportDocument after processing for entryLine " + entryLine1.CL_LineNumber, 3, entryLine1TransDocs.Length);
			AssertContainsExactElementsInAnyOrder("The 3 CL TransportDocument after processing have the correct CSI_Codes for entryLine.", new ZString[] { "LineTra1", "LineTra2", "LineTra3" }, entryLine1TransDocs.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL TransportDocument after processing have the correct CSI_ReferenceNumber for entryLine.", new ZString[] { "LineRef1", "LineRef2", "LineRef3" }, entryLine1TransDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL TransportDocument after processing have the correct CSI_Status for entryLine.", new ZString[] { "ACC", "ACC", "ACC" }, entryLine1TransDocs.Select(x => x.CSI_Status).ToArray());

			var entryLine2TransDocs = GetCLAdditionalInfos(entryLine2).Where(doc => doc.CSI_SubType == "TRA");
			AssertEquals("There are 2 CL TransportDocument after processing for entryLine " + entryLine2.CL_LineNumber, 2, entryLine2TransDocs.ToArray().Length);
			AssertContainsExactElementsInAnyOrder("The 2 CL TransportDocument after processing have the correct CSI_Codes for entryLine2.", new ZString[] { "Line2Tra1", "Line2Tra2" }, entryLine2TransDocs.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 2 CL TransportDocument after processing have the correct CSI_ReferenceNumber for entryLine2.", new ZString[] { "REF1", "REF2" }, entryLine2TransDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 2 CL TransportDocument after processing have the correct CSI_Status for entryLine2.", new ZString[] { "ACC", "ACC" }, entryLine2TransDocs.Select(x => x.CSI_Status).ToArray());

			var entryLine3TransDocs = GetCLAdditionalInfos(entryLine3).Where(doc => doc.CSI_SubType == "TRA");
			AssertEquals("There are no TransportDocument Documents after processing for entryLine " + entryLine3.CL_LineNumber, 0, entryLine3TransDocs.ToArray().Length);

			var entryLine4TransDocs = GetCLAdditionalInfos(entryLine4).Where(doc => doc.CSI_SubType == "TRA");
			AssertEquals("There are 2 CL TransportDocument Documents after processing for entryLine " + entryLine4.CL_LineNumber, 2, entryLine4TransDocs.ToArray().Length);
			AssertContainsExactElementsInAnyOrder("The 2 CL TransportDocument after processing have the correct CSI_Codes for entryLine4.", new ZString[] { "X248", "X249" }, entryLine4TransDocs.Select(x => x.CSI_Code).ToArray());
		});
	}

	public void TestProcessAcceptedMessage_GoodsItem_AdditionalInformation()
	{
		AddAdditionalInformation_EntryLine();
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileWithGoodsItemAdditionalInformationMessage, InterchangeID);

		ProcessMessageForTest(message);
		var expectedStatusDescription = "PA - Pending Presentation of Goods";
		var expectedMessageInterpretationText = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus(expectedStatusDescription);
		AssertAcceptedExportQueryDeclaration(message, entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);

		CombineAssertions(() =>
		{
			var entryLine1AddInfoDocs = GetCLAdditionalInfos(entryLine1).Where(doc => doc.CSI_SubType == "INF").ToArray();
			AssertEquals("There are 3 CL AdditionalInformation after processing for entryLine " + entryLine1.CL_LineNumber, 3, entryLine1AddInfoDocs.Length);
			AssertContainsExactElementsInAnyOrder("The 3 CL AdditionalInformation after processing have the correct CSI_Codes for entryLine.", new ZString[] { "LineInf1", "LineInf2", "LineInf3" }, entryLine1AddInfoDocs.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL AdditionalInformation after processing have the correct CSI_ReferenceNumber for entryLine.", new ZString[] { "LineRef1", "LineRef2", "LineRef3" }, entryLine1AddInfoDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL AdditionalInformation after processing have the correct CSI_Status for entryLine.", new ZString[] { "ACC", "ACC", "ACC" }, entryLine1AddInfoDocs.Select(x => x.CSI_Status).ToArray());

			var entryLine2AddInfoDocs = GetCLAdditionalInfos(entryLine2).Where(doc => doc.CSI_SubType == "INF");
			AssertEquals("There are 2 CL AdditionalInformation after processing for entryLine " + entryLine2.CL_LineNumber, 2, entryLine2AddInfoDocs.ToArray().Length);
			AssertContainsExactElementsInAnyOrder("The 2 CL AdditionalInformation after processing have the correct CSI_Codes for entryLine2.", new ZString[] { "Line2Inf1", "Line2Inf2" }, entryLine2AddInfoDocs.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 2 CL AdditionalInformation after processing have the correct CSI_ReferenceNumber for entryLine2.", new ZString[] { "REF1", "REF2" }, entryLine2AddInfoDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 2 CL AdditionalInformation after processing have the correct CSI_Status for entryLine2.", new ZString[] { "ACC", "ACC" }, entryLine2AddInfoDocs.Select(x => x.CSI_Status).ToArray());

			var entryLine3AddInfoDocs = GetCLAdditionalInfos(entryLine3).Where(doc => doc.CSI_SubType == "INF");
			AssertEquals("There are no AdditionalInformation Documents after processing for entryLine " + entryLine3.CL_LineNumber, 0, entryLine3AddInfoDocs.ToArray().Length);

			var entryLine4AddinfoDocs = GetCLAdditionalInfos(entryLine4).Where(doc => doc.CSI_SubType == "INF");
			AssertEquals("There are 2 CL AdditionalInformation Documents after processing for entryLine " + entryLine4.CL_LineNumber, 2, entryLine4AddinfoDocs.ToArray().Length);
			AssertContainsExactElementsInAnyOrder("The 2 CL AdditionalInformation after processing have the correct CSI_Codes for entryLine4.", new ZString[] { "X148", "X149" }, entryLine4AddinfoDocs.Select(x => x.CSI_Code).ToArray());
		});
	}

	public void TestProcessAcceptedMessage_GoodsItem_SupportingDocuments()
	{
		AddSupportingDocuments_EntryLine();
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileWithSupportingDocumentsMessage, InterchangeID);

		ProcessMessageForTest(message);
		var expectedStatusDescription = "PA - Pending Presentation of Goods";
		var expectedMessageInterpretationText = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus(expectedStatusDescription);
		AssertAcceptedExportQueryDeclaration(message, entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);

		CombineAssertions(() =>
		{
			var clSupDocsEntryLine1 = GetCLSupportingDocuments(entryLine1);
			AssertEquals("There are 3 CL SupportingDocuments after processing for entryLine 1 " + entryLine1.CL_LineNumber, 3, clSupDocsEntryLine1.Length);
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_Codes for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "N380", "N705", "C055" }, clSupDocsEntryLine1.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_ReferenceNumber for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "OHAA32", "OHAA33", "OHAA34" }, clSupDocsEntryLine1.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_ItemNumber for entryLine 1" + entryLine1.CL_LineNumber, new ZInt[] { 2, 5, ZInt.Zero }, clSupDocsEntryLine1.Select(x => x.CSI_ItemNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_AdditionalDescription for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "Authority1", "Authority2", ZString.Empty }, clSupDocsEntryLine1.Select(x => x.CSI_AdditionalDescription).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_DateOfIssue for entryLine 1" + entryLine1.CL_LineNumber, new ZDateTime[] { new ZDateTime(2022, 10, 11, 00, 00, 00), ZDateTime.Empty, ZDateTime.Empty }, clSupDocsEntryLine1.Select(x => x.CSI_DateOfIssue).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_DateOfExpiry for entryLine 1" + entryLine1.CL_LineNumber, new ZDateTime[] { new ZDateTime(2022, 11, 01, 00, 00, 00), ZDateTime.Empty, ZDateTime.Empty }, clSupDocsEntryLine1.Select(x => x.CSI_DateOfExpiry).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_UnitOfQuantity for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "KGM", "HG", ZString.Empty }, clSupDocsEntryLine1.Select(x => x.CSI_UnitOfQuantity).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_Quantity for entryLine 1" + entryLine1.CL_LineNumber, new ZDecimal[] { 20.123456, 3.6, ZDecimal.Zero }, clSupDocsEntryLine1.Select(x => x.CSI_Quantity).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_RX_NKCurrency for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "EUR", "DOL", ZString.Empty }, clSupDocsEntryLine1.Select(x => x.CSI_RX_NKCurrency).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_Value for entryLine 1" + entryLine1.CL_LineNumber, new ZDecimal[] { 100.86, 80.73, ZDecimal.Zero }, clSupDocsEntryLine1.Select(x => x.CSI_Value).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_Status for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "ACC", "ACC", "ACC" }, clSupDocsEntryLine1.Select(x => x.CSI_Status).ToArray());

			var clSupDocsEntryLine2 = GetCLSupportingDocuments(entryLine2);
			AssertEquals("There are 2 CL SupportingDocuments after processing for entryLine 2" + entryLine2.CL_LineNumber, 2, clSupDocsEntryLine2.Length);
			AssertContainsExactElementsInAnyOrder("The 2 CL SupportingDocuments after processing have the correct CSI_Codes for entryLine 2" + entryLine2.CL_LineNumber, new ZString[] { "AAAA", "BBBB" }, clSupDocsEntryLine2.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 2 CL SupportingDocuments after processing have the correct CSI_ReferenceNumber for entryLine 2" + entryLine2.CL_LineNumber, new ZString[] { "REFERENCE1", "REFERENCE2" }, clSupDocsEntryLine2.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 2 CL SupportingDocuments after processing have the correct CSI_Status for entryLine 2" + entryLine2.CL_LineNumber, new ZString[] { "ACC", "ACC" }, clSupDocsEntryLine2.Select(x => x.CSI_Status).ToArray());

			var clSupDocsEntryLine3 = GetCLSupportingDocuments(entryLine3);
			AssertEquals("There are 0 CL SupportingDocuments after processing for entryLine 3" + entryLine3.CL_LineNumber, 0, clSupDocsEntryLine3.Length);

			var clSupDocsEntryLine4 = GetCLSupportingDocuments(entryLine4);
			AssertEquals("There are 2 CL SupportingDocuments (the original ones) after processing for entryLine 4" + entryLine3.CL_LineNumber, 2, clSupDocsEntryLine4.Length);
			AssertContainsExactElementsInAnyOrder("The 2 CL SupportingDocuments after processing have the correct CSI_Codes for entryLine 4" + entryLine4.CL_LineNumber, new ZString[] { "X007", "X008" }, clSupDocsEntryLine4.Select(x => x.CSI_Code).ToArray());
		});
	}

	public void TestProcessAcceptedMessageWithPreviousDocumentsInGoodsItem()
	{
		AddPreviousDocuments_EntryLine();
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileWithPreviousDocumentsMessage, InterchangeID);

		ProcessMessageForTest(message);
		var expectedStatusDescription = "PA - Pending Presentation of Goods";
		var expectedMessageInterpretationText = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus(expectedStatusDescription);
		AssertAcceptedExportQueryDeclaration(message, entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);

		CombineAssertions(() =>
		{
			var clPrevDocsEntryLine1 = GetCLPreviousDocuments(entryLine1);
			AssertEquals("There are 3 CL PreviousDocuments after processing for entryLine 1 " + entryLine1.CL_LineNumber, 3, clPrevDocsEntryLine1.Length);
			AssertContainsExactElementsInAnyOrder("The 3 CL PreviousDocuments after processing have the correct CSI_Codes for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "NCLE", "NCLA", "NCLH" }, clPrevDocsEntryLine1.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL PreviousDocuments after processing have the correct CSI_ReferenceNumber for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "12012021", "12012022", "12012023" }, clPrevDocsEntryLine1.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL PreviousDocuments after processing have the correct CSI_LineNo for entryLine 1" + entryLine1.CL_LineNumber, new ZInt[] { 2, 5, ZInt.Zero }, clPrevDocsEntryLine1.Select(x => x.CSI_LineNo).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL PreviousDocuments after processing have the correct CSI_UnitOfQuantity for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "KGM", "HG", ZString.Empty }, clPrevDocsEntryLine1.Select(x => x.CSI_UnitOfQuantity).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL PreviousDocuments after processing have the correct CSI_Quantity for entryLine 1" + entryLine1.CL_LineNumber, new ZDecimal[] { 20.12346, 3.6, ZDecimal.Zero }, clPrevDocsEntryLine1.Select(x => x.CSI_Quantity).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL PreviousDocuments after processing have the correct CSI_Status for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "ACC", "ACC", "ACC" }, clPrevDocsEntryLine1.Select(x => x.CSI_Status).ToArray());

			var clPrevDocsEntryLine2 = GetCLPreviousDocuments(entryLine2);
			AssertEquals("There are 2 CL PreviousDocuments after processing for entryLine 2" + entryLine2.CL_LineNumber, 2, clPrevDocsEntryLine2.Length);
			AssertContainsExactElementsInAnyOrder("The 2 CL PreviousDocuments after processing have the correct CSI_Codes for entryLine 2" + entryLine2.CL_LineNumber, new ZString[] { "AAAA", "BBBB" }, clPrevDocsEntryLine2.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 2 CL PreviousDocuments after processing have the correct CSI_ReferenceNumber for entryLine 2" + entryLine2.CL_LineNumber, new ZString[] { "REFERENCE1", "REFERENCE2" }, clPrevDocsEntryLine2.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 2 CL PreviousDocuments after processing have the correct CSI_Status for entryLine 2" + entryLine2.CL_LineNumber, new ZString[] { "ACC", "ACC" }, clPrevDocsEntryLine2.Select(x => x.CSI_Status).ToArray());

			var clPrevDocsEntryLine3 = GetCLPreviousDocuments(entryLine3);
			AssertEquals("There are 0 CL PreviousDocuments after processing for entryLine 3" + entryLine3.CL_LineNumber, 0, clPrevDocsEntryLine3.Length);

			var clPrevDocsEntryLine4 = GetCLPreviousDocuments(entryLine4);
			AssertEquals("There are 2 CL PreviousDocuments (the original ones) after processing for entryLine 4" + entryLine3.CL_LineNumber, 2, clPrevDocsEntryLine4.Length);
			AssertContainsExactElementsInAnyOrder("The 2 CL PreviousDocuments after processing have the correct CSI_Codes for entryLine 4" + entryLine4.CL_LineNumber, new ZString[] { "P007", "P008" }, clPrevDocsEntryLine4.Select(x => x.CSI_Code).ToArray());
		});
	}

	public void TestProcessAcceptedMessageWithAllDocumentsInGoodsItem()
	{
		AddSupportingDocuments_EntryLine();
		AddPreviousDocuments_EntryLine();
		AddAdditionalInformation_EntryLine();
		AddAdditionalReference_EntryLine();
		AddTransportDocument_EntryLine();

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileWithAllDocsInGoodsItemMessage, InterchangeID);

		ProcessMessageForTest(message);
		var expectedStatusDescription = "PA - Pending Presentation of Goods";
		var expectedMessageInterpretationText = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus(expectedStatusDescription);
		AssertAcceptedExportQueryDeclaration(message, entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);

		CombineAssertions(() =>
		{
			var clPrevDocsEntryLine1 = GetCLPreviousDocuments(entryLine1);
			AssertEquals("There are 3 CL PreviousDocuments after processing for entryLine 1 " + entryLine1.CL_LineNumber, 3, clPrevDocsEntryLine1.Length);
			AssertContainsExactElementsInAnyOrder("The 3 CL PreviousDocuments after processing have the correct CSI_Codes for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "NCLE", "NCLA", "NCLH" }, clPrevDocsEntryLine1.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL PreviousDocuments after processing have the correct CSI_ReferenceNumber for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "12012021", "12012022", "12012023" }, clPrevDocsEntryLine1.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL PreviousDocuments after processing have the correct CSI_Status for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "ACC", "ACC", "ACC" }, clPrevDocsEntryLine1.Select(x => x.CSI_Status).ToArray());

			var clSupDocsEntryLine1 = GetCLSupportingDocuments(entryLine1);
			AssertEquals("There are 3 CL SupportingDocuments after processing for entryLine 1 " + entryLine1.CL_LineNumber, 3, clSupDocsEntryLine1.Length);
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_Codes for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "N380", "N705", "C055" }, clSupDocsEntryLine1.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_ReferenceNumber for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "OHAA32", "OHAA33", "OHAA34" }, clSupDocsEntryLine1.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_Status for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "ACC", "ACC", "ACC" }, clSupDocsEntryLine1.Select(x => x.CSI_Status).ToArray());

			var clAddRefDocsEntryLine1 = GetCLAdditionalInfos(entryLine1).Where(doc => doc.CSI_SubType == "REF").ToArray();
			AssertEquals("There are 3 CL AdditionalReference after processing for entryLine 1 " + entryLine1.CL_LineNumber, 3, clAddRefDocsEntryLine1.Length);
			AssertContainsExactElementsInAnyOrder("The 3 CL AdditionalReference after processing have the correct CSI_Codes for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "Y924", "Y925", "Y926" }, clAddRefDocsEntryLine1.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL AdditionalReference after processing have the correct CSI_ReferenceNumber for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "DE89564", "DE89565", "DE89566" }, clAddRefDocsEntryLine1.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL AdditionalReference after processing have the correct CSI_Status for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "ACC", "ACC", "ACC" }, clAddRefDocsEntryLine1.Select(x => x.CSI_Status).ToArray());

			var clAddInfoDocsEntryLine1 = GetCLAdditionalInfos(entryLine1).Where(doc => doc.CSI_SubType == "INF").ToArray();
			AssertEquals("There are 3 CL AdditionalInformation after processing for entryLine 1 " + entryLine1.CL_LineNumber, 3, clAddInfoDocsEntryLine1.Length);
			AssertContainsExactElementsInAnyOrder("The 3 CL AdditionalInformation after processing have the correct CSI_Codes for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "LineInf1", "LineInf2", "LineInf3" }, clAddInfoDocsEntryLine1.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL AdditionalInformation after processing have the correct CSI_ReferenceNumber for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "LineRef1", "LineRef2", "LineRef3" }, clAddInfoDocsEntryLine1.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL AdditionalInformation after processing have the correct CSI_Status for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "ACC", "ACC", "ACC" }, clAddInfoDocsEntryLine1.Select(x => x.CSI_Status).ToArray());

			var clTransportDocsEntryLine1 = GetCLAdditionalInfos(entryLine1).Where(doc => doc.CSI_SubType == "TRA").ToArray();
			AssertEquals("There are 3 CL TransportDocument after processing for entryLine 1 " + entryLine1.CL_LineNumber, 3, clTransportDocsEntryLine1.Length);
			AssertContainsExactElementsInAnyOrder("The 3 CL TransportDocument after processing have the correct CSI_Codes for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "LineTra1", "LineTra2", "LineTra3" }, clTransportDocsEntryLine1.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL TransportDocument after processing have the correct CSI_ReferenceNumber for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "LineRef1", "LineRef2", "LineRef3" }, clTransportDocsEntryLine1.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL TransportDocument after processing have the correct CSI_Status for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "ACC", "ACC", "ACC" }, clTransportDocsEntryLine1.Select(x => x.CSI_Status).ToArray());

			var clPrevDocsEntryLine2 = GetCLPreviousDocuments(entryLine2);
			AssertEquals("There is 1 CL PreviousDocument after processing for entryLine 2" + entryLine2.CL_LineNumber, 1, clPrevDocsEntryLine2.Length);
			var clPrevDocEntryLine2 = clPrevDocsEntryLine2[0];
			AssertEquals("The CL PreviousDocument after processing has the correct CSI_Codes for entryLine 2" + entryLine2.CL_LineNumber, "AAAA", clPrevDocEntryLine2.CSI_Code);
			AssertEquals("The CL PreviousDocument after processing has the correct CSI_ReferenceNumber for entryLine 2" + entryLine2.CL_LineNumber, "REFERENCE1", clPrevDocEntryLine2.CSI_ReferenceNumber);
			AssertEquals("The CL PreviousDocument after processing has the correct CSI_Status for entryLine 2" + entryLine2.CL_LineNumber, "ACC", clPrevDocEntryLine2.CSI_Status);

			var clSupDocsEntryLine2 = GetCLSupportingDocuments(entryLine2);
			AssertEquals("There are 1 CL SupportingDocuments after processing for entryLine 2" + entryLine2.CL_LineNumber, 1, clSupDocsEntryLine2.Length);
			AssertContainsExactElementsInAnyOrder("The 1 CL SupportingDocuments after processing have the correct CSI_Codes for entryLine 2" + entryLine2.CL_LineNumber, new ZString[] { "BBBB" }, clSupDocsEntryLine2.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CL SupportingDocuments after processing have the correct CSI_ReferenceNumber for entryLine 2" + entryLine2.CL_LineNumber, new ZString[] { "REFERENCE2" }, clSupDocsEntryLine2.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CL SupportingDocuments after processing have the correct CSI_Status for entryLine 2" + entryLine2.CL_LineNumber, new ZString[] { "ACC" }, clSupDocsEntryLine2.Select(x => x.CSI_Status).ToArray());

			var clAddRefDocsEntryLine2 = GetCLAdditionalInfos(entryLine2).Where(doc => doc.CSI_SubType == "REF").ToArray();
			AssertEquals("There are 1 CL AdditionalReferences after processing for entryLine 2" + entryLine2.CL_LineNumber, 1, clAddRefDocsEntryLine2.Length);
			AssertContainsExactElementsInAnyOrder("The 1 CL AdditionalReferences after processing have the correct CSI_Codes for entryLine 2" + entryLine2.CL_LineNumber, new ZString[] { "CCCC" }, clAddRefDocsEntryLine2.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CL AdditionalReferences after processing have the correct CSI_ReferenceNumber for entryLine 2" + entryLine2.CL_LineNumber, new ZString[] { "REFERENCE3" }, clAddRefDocsEntryLine2.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CL AdditionalReferences after processing have the correct CSI_Status for entryLine 2" + entryLine2.CL_LineNumber, new ZString[] { "ACC" }, clAddRefDocsEntryLine2.Select(x => x.CSI_Status).ToArray());

			var clAddInfoDocsEntryLine2 = GetCLAdditionalInfos(entryLine2).Where(doc => doc.CSI_SubType == "INF").ToArray();
			AssertEquals("There are 1 CL AdditionalInformation after processing for entryLine 2" + entryLine2.CL_LineNumber, 1, clAddInfoDocsEntryLine2.Length);
			AssertContainsExactElementsInAnyOrder("The 1 CL AdditionalInformation after processing have the correct CSI_Codes for entryLine 2" + entryLine2.CL_LineNumber, new ZString[] { "LineInf1" }, clAddInfoDocsEntryLine2.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CL AdditionalInformation after processing have the correct CSI_ReferenceNumber for entryLine 2" + entryLine2.CL_LineNumber, new ZString[] { "LineRef1" }, clAddInfoDocsEntryLine2.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CL AdditionalInformation after processing have the correct CSI_Status for entryLine 2" + entryLine2.CL_LineNumber, new ZString[] { "ACC" }, clAddInfoDocsEntryLine2.Select(x => x.CSI_Status).ToArray());

			var clTransportDocsEntryLine2 = GetCLAdditionalInfos(entryLine2).Where(doc => doc.CSI_SubType == "TRA").ToArray();
			AssertEquals("There are 1 CL TransportDocument after processing for entryLine 2" + entryLine2.CL_LineNumber, 1, clTransportDocsEntryLine2.Length);
			AssertContainsExactElementsInAnyOrder("The 1 CL TransportDocument after processing have the correct CSI_Codes for entryLine 2" + entryLine2.CL_LineNumber, new ZString[] { "LineTra1" }, clTransportDocsEntryLine2.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CL TransportDocument after processing have the correct CSI_ReferenceNumber for entryLine 2" + entryLine2.CL_LineNumber, new ZString[] { "LineRef1" }, clTransportDocsEntryLine2.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CL TransportDocument after processing have the correct CSI_Status for entryLine 2" + entryLine2.CL_LineNumber, new ZString[] { "ACC" }, clTransportDocsEntryLine2.Select(x => x.CSI_Status).ToArray());

			var clPrevDocsEntryLine3 = GetCLPreviousDocuments(entryLine3);
			AssertEquals("There are 0 CL PreviousDocuments after processing for entryLine 3" + entryLine3.CL_LineNumber, 0, clPrevDocsEntryLine3.Length);

			var clSupDocsEntryLine3 = GetCLSupportingDocuments(entryLine3);
			AssertEquals("There are 0 CL SupportingDocuments after processing for entryLine 3" + entryLine3.CL_LineNumber, 0, clSupDocsEntryLine3.Length);

			var clAddRefEntryLine3 = GetCLAdditionalInfos(entryLine3).Where(doc => doc.CSI_SubType == "REF").ToArray();
			AssertEquals("There are 0 CL AdditionalReferences after processing for entryLine 3" + entryLine3.CL_LineNumber, 0, clAddRefEntryLine3.Length);

			var clAddInfoEntryLine3 = GetCLAdditionalInfos(entryLine3).Where(doc => doc.CSI_SubType == "INF").ToArray();
			AssertEquals("There are 0 CL AdditionalInformation after processing for entryLine 3" + entryLine3.CL_LineNumber, 0, clAddInfoEntryLine3.Length);

			var clTransDocsEntryLine3 = GetCLAdditionalInfos(entryLine3).Where(doc => doc.CSI_SubType == "TRA").ToArray();
			AssertEquals("There are 0 CL TransportDocument after processing for entryLine 3" + entryLine3.CL_LineNumber, 0, clTransDocsEntryLine3.Length);

			var clPrevDocsEntryLine4 = GetCLPreviousDocuments(entryLine4);
			AssertEquals("There are 2 CL PreviousDocuments (the original ones) after processing for entryLine 4" + entryLine4.CL_LineNumber, 2, clPrevDocsEntryLine4.Length);
			AssertContainsExactElementsInAnyOrder("The 2 CL PreviousDocuments after processing have the correct CSI_Codes for entryLine 4" + entryLine4.CL_LineNumber, new ZString[] { "P007", "P008" }, clPrevDocsEntryLine4.Select(x => x.CSI_Code).ToArray());

			var clSupDocsEntryLine4 = GetCLSupportingDocuments(entryLine4);
			AssertEquals("There are 2 CL SupportingDocuments (the original ones) after processing for entryLine 4" + entryLine4.CL_LineNumber, 2, clSupDocsEntryLine4.Length);
			AssertContainsExactElementsInAnyOrder("The 2 CL SupportingDocuments after processing have the correct CSI_Codes for entryLine 4" + entryLine4.CL_LineNumber, new ZString[] { "X007", "X008" }, clSupDocsEntryLine4.Select(x => x.CSI_Code).ToArray());

			var clAddRefEntryLine4 = GetCLAdditionalInfos(entryLine4).Where(doc => doc.CSI_SubType == "REF").ToArray();
			AssertEquals("There are 2 CL AdditionalReferences (the original ones) after processing for entryLine 4" + entryLine4.CL_LineNumber, 2, clAddRefEntryLine4.Length);
			AssertContainsExactElementsInAnyOrder("The 2 CL AdditionalReferences after processing have the correct CSI_Codes for entryLine 4" + entryLine4.CL_LineNumber, new ZString[] { "X047", "X049" }, clAddRefEntryLine4.Select(x => x.CSI_Code).ToArray());

			var clAddInfoEntryLine4 = GetCLAdditionalInfos(entryLine4).Where(doc => doc.CSI_SubType == "INF").ToArray();
			AssertEquals("There are 2 CL AdditionalInformation (the original ones) after processing for entryLine 4" + entryLine4.CL_LineNumber, 2, clAddInfoEntryLine4.Length);
			AssertContainsExactElementsInAnyOrder("The 2 CL AdditionalInformation after processing have the correct CSI_Codes for entryLine 4" + entryLine4.CL_LineNumber, new ZString[] { "X148", "X149" }, clAddInfoEntryLine4.Select(x => x.CSI_Code).ToArray());

			var clTransDocsEntryLine4 = GetCLAdditionalInfos(entryLine4).Where(doc => doc.CSI_SubType == "TRA").ToArray();
			AssertEquals("There are 2 CL TransportDocument (the original ones) after processing for entryLine 4" + entryLine4.CL_LineNumber, 2, clTransDocsEntryLine4.Length);
			AssertContainsExactElementsInAnyOrder("The 2 CL TransportDocument after processing have the correct CSI_Codes for entryLine 4" + entryLine4.CL_LineNumber, new ZString[] { "X248", "X249" }, clTransDocsEntryLine4.Select(x => x.CSI_Code).ToArray());
		});
	}

	public void TestProcessAcceptedMessage_AllDocInGoodShipmentAndGoodsItem()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileWithGoodsShipmentAndGoodsItemMessage, InterchangeID);

		ProcessMessageForTest(message);
		var expectedStatusDescription = "PA - Pending Presentation of Goods";
		var expectedMessageInterpretationText = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus(expectedStatusDescription);
		AssertAcceptedExportQueryDeclaration(message, entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);

		CombineAssertions(() =>
		{
			var chEntryHeaderSupDocs = GetCHSupportingDocuments(entryHeader);
			AssertContainsExactElementsInAnyOrder("The 1 CH SupportingDocuments after processing have the correct CSI_Codes for entryHeader.", new ZString[] { "Sup1" }, chEntryHeaderSupDocs.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CH SupportingDocuments after processing have the correct CSI_ReferenceNumber for entryHeader.", new ZString[] { "Ref1" }, chEntryHeaderSupDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CH SupportingDocuments after processing have the correct CSI_ItemNumber for entryHeader.", new ZInt[] { 123 }, chEntryHeaderSupDocs.Select(x => x.CSI_ItemNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CH SupportingDocuments after processing have the correct CSI_AdditionalDescription for entryHeader.", new ZString[] { "name1" }, chEntryHeaderSupDocs.Select(x => x.CSI_AdditionalDescription).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CH SupportingDocuments after processing have the correct CSI_DateOfIssue for entryHeader.", new ZDateTime[] { new ZDateTime(2022, 09, 10) }, chEntryHeaderSupDocs.Select(x => x.CSI_DateOfIssue).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CH SupportingDocuments after processing have the correct CSI_Status for entryHeader.", new ZString[] { "ACC" }, chEntryHeaderSupDocs.Select(x => x.CSI_Status).ToArray());

			var chEntryHeaderAddRefs = GetCHAdditionalInfo(entryHeader).Where(doc => doc.CSI_SubType == "REF");
			AssertContainsExactElementsInAnyOrder("The 1 CH AdditionalReference after processing have the correct CSI_Codes for entryHeader.", new ZString[] { "Cod1" }, chEntryHeaderAddRefs.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CH AdditionalReference after processing have the correct CSI_ReferenceNumber for entryHeader.", new ZString[] { "Ref1" }, chEntryHeaderAddRefs.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CH AdditionalReference after processing have the correct CSI_Status for entryHeader.", new ZString[] { "ACC" }, chEntryHeaderAddRefs.Select(x => x.CSI_Status).ToArray());

			var chEntryHeaderAddInfo = GetCHAdditionalInfo(entryHeader).Where(doc => doc.CSI_SubType == "INF");
			AssertContainsExactElementsInAnyOrder("The 1 CH AdditionalInformation after processing have the correct CSI_Codes for entryHeader.", new ZString[] { "Inf1" }, chEntryHeaderAddInfo.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CH AdditionalInformation after processing have the correct CSI_ReferenceNumber for entryHeader.", new ZString[] { "Ref1" }, chEntryHeaderAddInfo.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CH AdditionalInformation after processing have the correct CSI_Status for entryHeader.", new ZString[] { "ACC" }, chEntryHeaderAddInfo.Select(x => x.CSI_Status).ToArray());

			var chEntryHeaderTransDocs = GetCHAdditionalInfo(entryHeader).Where(doc => doc.CSI_SubType == "TRA");
			AssertContainsExactElementsInAnyOrder("The 1 CH TransportDocument after processing have the correct CSI_Codes for entryHeader.", new ZString[] { "Tra1" }, chEntryHeaderTransDocs.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CH TransportDocument after processing have the correct CSI_ReferenceNumber for entryHeader.", new ZString[] { "ref1" }, chEntryHeaderTransDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CH TransportDocument after processing have the correct CSI_Status for entryHeader.", new ZString[] { "ACC" }, chEntryHeaderTransDocs.Select(x => x.CSI_Status).ToArray());

			var clPrevDocsEntryLine1 = GetCLPreviousDocuments(entryLine1);
			AssertEquals("There are 1 CL PreviousDocuments after processing for entryLine 1 " + entryLine1.CL_LineNumber, 1, clPrevDocsEntryLine1.Length);
			AssertContainsExactElementsInAnyOrder("The 1 CL PreviousDocuments after processing have the correct CSI_Codes for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "NCLH" }, clPrevDocsEntryLine1.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CL PreviousDocuments after processing have the correct CSI_ReferenceNumber for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "12012023" }, clPrevDocsEntryLine1.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CL PreviousDocuments after processing have the correct CSI_Status for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "ACC" }, clPrevDocsEntryLine1.Select(x => x.CSI_Status).ToArray());

			var clSupDocsEntryLine1 = GetCLSupportingDocuments(entryLine1);
			AssertEquals("There are 1 CL SupportingDocuments after processing for entryLine 1 " + entryLine1.CL_LineNumber, 1, clSupDocsEntryLine1.Length);
			AssertContainsExactElementsInAnyOrder("The 1 CL SupportingDocuments after processing have the correct CSI_Codes for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "N380" }, clSupDocsEntryLine1.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CL SupportingDocuments after processing have the correct CSI_ReferenceNumber for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "OHAA32" }, clSupDocsEntryLine1.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CL SupportingDocuments after processing have the correct CSI_Status for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "ACC" }, clSupDocsEntryLine1.Select(x => x.CSI_Status).ToArray());

			var clAddRefDocsEntryLine1 = GetCLAdditionalInfos(entryLine1).Where(doc => doc.CSI_SubType == "REF").ToArray();
			AssertEquals("There are 1 CL AdditionalReference after processing for entryLine 1 " + entryLine1.CL_LineNumber, 1, clAddRefDocsEntryLine1.Length);
			AssertContainsExactElementsInAnyOrder("The 1 CL AdditionalReference after processing have the correct CSI_Codes for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "Y924" }, clAddRefDocsEntryLine1.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CL AdditionalReference after processing have the correct CSI_ReferenceNumber for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "DE89564" }, clAddRefDocsEntryLine1.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CL AdditionalReference after processing have the correct CSI_Status for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "ACC" }, clAddRefDocsEntryLine1.Select(x => x.CSI_Status).ToArray());

			var clAddInfoDocsEntryLine1 = GetCLAdditionalInfos(entryLine1).Where(doc => doc.CSI_SubType == "INF").ToArray();
			AssertEquals("There are 1 CL AdditionalInformation after processing for entryLine 1 " + entryLine1.CL_LineNumber, 1, clAddInfoDocsEntryLine1.Length);
			AssertContainsExactElementsInAnyOrder("The 1 CL AdditionalInformation after processing have the correct CSI_Codes for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "LineInf1" }, clAddInfoDocsEntryLine1.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CL AdditionalInformation after processing have the correct CSI_ReferenceNumber for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "LineRef1" }, clAddInfoDocsEntryLine1.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CL AdditionalInformation after processing have the correct CSI_Status for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "ACC" }, clAddInfoDocsEntryLine1.Select(x => x.CSI_Status).ToArray());

			var clTransportDocsEntryLine1 = GetCLAdditionalInfos(entryLine1).Where(doc => doc.CSI_SubType == "TRA").ToArray();
			AssertEquals("There are 1 CL TransportDocument after processing for entryLine 1 " + entryLine1.CL_LineNumber, 1, clTransportDocsEntryLine1.Length);
			AssertContainsExactElementsInAnyOrder("The 1 CL TransportDocument after processing have the correct CSI_Codes for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "LineTra1" }, clTransportDocsEntryLine1.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CL TransportDocument after processing have the correct CSI_ReferenceNumber for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "LineRef1" }, clTransportDocsEntryLine1.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CL TransportDocument after processing have the correct CSI_Status for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "ACC" }, clTransportDocsEntryLine1.Select(x => x.CSI_Status).ToArray());
		});
	}

	public void TestProcessAcceptedMessage_EntryStatusEFD_TriggerRequest()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusSAMessage, InterchangeID);

		ProcessMessageForTest(message);
		var expectedStatusDescription = "SA - Effective Exit";
		var expectedMessageInterpretationText = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus(expectedStatusDescription);
		AssertAcceptedExportQueryDeclaration(message, entryStatusCode: EntryStatusCodes.EffectiveDeparture, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);

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

	public void TestProcessAcceptedMessage_EntryStatusEFD_NoTriggerRequest()
	{
		entryHeader.CH_EntryStatus = EntryStatusCodes.EffectiveDeparture;
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusSAMessage, InterchangeID);

		ProcessMessageForTest(message);
		var expectedStatusDescription = "SA - Effective Exit";
		var expectedMessageInterpretationText = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus(expectedStatusDescription);
		AssertAcceptedExportQueryDeclaration(message, entryStatusCode: EntryStatusCodes.EffectiveDeparture, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);

		CombineAssertions(() =>
		{
			var lastMessage = entryHeader.Messages.LastMessage;

			AssertNotEquals("Last message is not RequestExportExitCertificate", "EDR", lastMessage.EM_MessageType);
			AssertNotEquals("LastMessage is not TRX", EDIMessage.Direction.Transmit, lastMessage.EM_ReceiveTransmit);
		});
	}

	public void TestProcessMessageEntryInstructionSubStyle()
	{
		CombineAssertions("When EQU message is received, if <AdditionalDeclarationType> value is X or Y, update the entry instruction sub style value to the received value", () =>
		{
			AssertEntryInstruction(acceptanceTestFileAdditionalDeclarationXMessage, "X", "C", "X");
			AssertEntryInstruction(acceptanceTestFileAdditionalDeclarationYMessage, "Y", "C", "Y");
			AssertEntryInstruction(acceptanceTestFileStatusAWNoCSVClearanceMessage, "Z", "A", "A");
		});

		void AssertEntryInstruction(string messageText, ZString additionalDeclarationType, ZString initialSubStyle, ZString expectedSubStyle)
		{
			instruction.CEI_SubStyle = initialSubStyle;
			instruction.Factory.Save();

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, messageText, InterchangeID);

			ProcessMessageForTest(message);

			AssertEquals($"Entry Instruction SubStyle '{initialSubStyle}'; AdditionalDeclarationType:{additionalDeclarationType}", expectedSubStyle, instruction.CEI_SubStyle);
		}
	}

	public void TestCreateDocumentCaptureRequestEDIMessageWhenCSVClearance_Export_AllDocs()
	{
		newDeclaration.ZG_CTStatusID = "T2LF";
		entryHeader.ZG_CSVExitCertificate = CsvExitCertificate;
		entryHeader.ZG_CSVT2L = "CSVT2L";
		entryHeader.IndirectExport = true;

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusPAGreenCircuitMessageAEAT, InterchangeID);

		ProcessMessageForTest(message);
		var expectedStatusDescription = "PA - Pending Presentation of Goods";
		var expectedMessageInterpretationText = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus(expectedStatusDescription);
		AssertAcceptedExportQueryDeclaration(message, entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);

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

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusPAGreenCircuitMessageAEAT, InterchangeID);

		ProcessMessageForTest(message);
		var expectedStatusDescription = "PA - Pending Presentation of Goods";
		var expectedMessageInterpretationText = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus(expectedStatusDescription);
		AssertAcceptedExportQueryDeclaration(message, entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);

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

	public void TestSetEntryIssueDate()
	{
		var messageAcceptanceDate = new ZDate(2022, 10, 11);
		entryHeader.MovementReferenceNumberIssueDate = MovementReferenceNumberIssueDate;
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusPIMessage, InterchangeID);
		ProcessMessageForTest(message);
		AssertEquals("MovementReferenceNumberIssueDate", messageAcceptanceDate, entryHeader.MovementReferenceNumberIssueDate);

		entryHeader.MovementReferenceNumberIssueDate = messageAcceptanceDate.AddHours(2);
		ProcessMessageForTest(message);
		AssertEquals("MovementReferenceNumberIssueDate", messageAcceptanceDate.AddHours(2), entryHeader.MovementReferenceNumberIssueDate);
	}

	void AssertAcceptedExportQueryDeclaration(TestEdiMessage message, string entryStatusCode, string circuit = CircuitCodeList.Codes.GREEN, string circuitCan = "", string csvClearance = CsvClearance, ZDateTime? entryReleaseDate = null, string csvT2L = CsvT2L, string expectedMessageInterpretation = "")
	{
		AssertExportQueryDeclaration(message, entryStatusCode: entryStatusCode, circuit: circuit, circuitCan: circuitCan, acceptanceDate: acceptanceDate, csvClearance: csvClearance, entryReleaseDate: entryReleaseDate, csvT2L: csvT2L, expectedMessageInterpretation: expectedMessageInterpretation, messageSubType: "ACC");
	}

	void AssertExportQueryDeclaration(TestEdiMessage message, string entryStatusCode, string messageSubType, string circuit = "", string circuitCan = "", ZDateTime? acceptanceDate = null, string csvClearance = "", ZDateTime? entryReleaseDate = null, string csvT2L = "", string expectedMessageInterpretation = "")
	{
		GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageSubType: messageSubType, entryStatusCode: entryStatusCode, circuit: circuit, circuitCan: circuitCan, acceptanceDate: acceptanceDate, movementReferenceNumber: MRNCode, csvClearance: csvClearance, entryReleaseDate: entryReleaseDate, csvT2L: csvT2L, messageNum: MessageNum);
	}

	ZString GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus(ZString statusDescription) => ZString.Format("<H3>Accepted Declaration</H3>" +
			"<H4>Management Data</H4>" +
			"<br><table border=\"0\"><tr><td>Version:</td><td>&nbsp;&nbsp;</td><td>AES (XML)</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>{0}</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>11-10-2022</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES00999930006184</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>TEST444444444444</td></tr>" +
			"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>12-10-2022</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Exit Type:</td><td>&nbsp;&nbsp;</td><td>[I] Indirect</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Exit Result:</td><td>&nbsp;&nbsp;</td><td>A2 - Satisfied</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Effective Departure Clearance:</td><td>&nbsp;&nbsp;</td><td>64LQAGQ4Z4FYK8L7</td></tr>" +
			"<tr><td>Effective Departure Date:</td><td>&nbsp;&nbsp;</td><td>13-10-2022</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Exit Goods Date:</td><td>&nbsp;&nbsp;</td><td>10-10-2022</td></tr></table>" +
			"<H4>Exit Control</H4>" +
			"<br><table border=\"0\"><tr><td>Arrival Date:</td><td>&nbsp;&nbsp;</td><td>14-10-2022</td></tr>" +
			"<tr><td>Arrival Location:</td><td>&nbsp;&nbsp;</td><td>place</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>AAAAAAAAAAAAAAAA</td></tr>" +
			"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>15-10-2022</td></tr></table>", statusDescription);

	public void TestProcessCancelationResponse_WithCONTransactionAndTSNotEnabled()
	{
		using (SetTemporaryStorageEnabled(false))
		{
			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, entryHeader.CH_BGMReference);

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusCAMessage, InterchangeID);
			ProcessMessageForTest(message);

			var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			var guarantee = regHeader.Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			TransactionsTestHelper.AssertProcessCancelationResponse_PreRequisites(numTransactions, regHeader.SRH_Status, regLine.SRL_CustomsStatus, numGuaranteeTransactions);

			message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusPIMessage, InterchangeID);
			ProcessMessageForTest(message);

			TransactionsTestHelper.AssertProcessCancelationResponse_PreRequisites(numTransactions, regHeader.SRH_Status, regLine.SRL_CustomsStatus, numGuaranteeTransactions);
		}
	}

	public void TestProcessCancelationResponse_WithCONTransactionAndLocationNotManagedInPremises()
	{
		using (SetTemporaryStorageEnabled(true))
		{
			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, entryHeader.CH_BGMReference);
			entryHeader.Declaration.CustomsEntryInstructions[0].GoodsLocation.Address.AuthorisationNumber = "9999000000";

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusCAMessage, InterchangeID);
			ProcessMessageForTest(message);

			var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			var guarantee = regHeader.Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			TransactionsTestHelper.AssertProcessCancelationResponse_PreRequisites(numTransactions, regHeader.SRH_Status, regLine.SRL_CustomsStatus, numGuaranteeTransactions);

			message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusPIMessage, InterchangeID);
			ProcessMessageForTest(message);

			TransactionsTestHelper.AssertProcessCancelationResponse_PreRequisites(numTransactions, regHeader.SRH_Status, regLine.SRL_CustomsStatus, numGuaranteeTransactions);
		}
	}

	public void TestProcessCancelationResponse_WithCONTransactionAndPremiseTypeADTNotManagedInPremises()
	{
		using (SetTemporaryStorageEnabled(true))
		{
			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, entryHeader.CH_BGMReference);
			regHeader.Premises.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusCAMessage, InterchangeID);
			ProcessMessageForTest(message);

			var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			var guarantee = regHeader.Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			TransactionsTestHelper.AssertProcessCancelationResponse_PreRequisites(numTransactions, regHeader.SRH_Status, regLine.SRL_CustomsStatus, numGuaranteeTransactions);

			message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusPIMessage, InterchangeID);
			ProcessMessageForTest(message);

			TransactionsTestHelper.AssertProcessCancelationResponse_PreRequisites(numTransactions, regHeader.SRH_Status, regLine.SRL_CustomsStatus, numGuaranteeTransactions);
		}
	}

	public void TestProcessCancelationResponse_WithCONTransactionAndMessageNotCANOrINV()
	{
		using (SetTemporaryStorageEnabled(true))
		{
			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, entryHeader.CH_BGMReference);

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusCAMessage, InterchangeID);
			ProcessMessageForTest(message);

			var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			var guarantee = regHeader.Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			TransactionsTestHelper.AssertProcessCancelationResponse_PreRequisites(numTransactions, regHeader.SRH_Status, regLine.SRL_CustomsStatus, numGuaranteeTransactions);

			message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusPIMessage, InterchangeID);
			ProcessMessageForTest(message);

			TransactionsTestHelper.AssertProcessCancelationResponse_PreRequisites(numTransactions, regHeader.SRH_Status, regLine.SRL_CustomsStatus, numGuaranteeTransactions);
		}
	}

	public void TestProcessCancelationResponse_WithCONTransactionAndTransactionNotDUE()
	{
		using (SetTemporaryStorageEnabled(true))
		{
			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, entryHeader.CH_BGMReference, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements);

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusCAMessage, InterchangeID);
			ProcessMessageForTest(message);

			var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			var guarantee = regHeader.Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			TransactionsTestHelper.AssertProcessCancelationResponse_PreRequisites(numTransactions, regHeader.SRH_Status, regLine.SRL_CustomsStatus, numGuaranteeTransactions);

			message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusPIMessage, InterchangeID);
			ProcessMessageForTest(message);

			TransactionsTestHelper.AssertProcessCancelationResponse_PreRequisites(numTransactions, regHeader.SRH_Status, regLine.SRL_CustomsStatus, numGuaranteeTransactions);
		}
	}

	public void TestProcessCancelationResponse_WithCONTransactionAndTransactionReferenceNotEntryHeader()
	{
		using (SetTemporaryStorageEnabled(true))
		{
			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, "TestReference");

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusCAMessage, InterchangeID);
			ProcessMessageForTest(message);

			var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			var guarantee = regHeader.Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			TransactionsTestHelper.AssertProcessCancelationResponse_PreRequisites(numTransactions, regHeader.SRH_Status, regLine.SRL_CustomsStatus, numGuaranteeTransactions);

			message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusPIMessage, InterchangeID);
			ProcessMessageForTest(message);

			TransactionsTestHelper.AssertProcessCancelationResponse_PreRequisites(numTransactions, regHeader.SRH_Status, regLine.SRL_CustomsStatus, numGuaranteeTransactions);
		}
	}

	public void TestProcessCancelationResponse_WithCONAndPNDTransaction()
	{
		using (SetTemporaryStorageEnabled(true))
		{
			var (regLineTransaction1, regLineTransaction2, regLineTransaction3, regLineTransaction4, regLineTransaction5, regLine2, guarantee) = TransactionsTestHelper.SetupTransactions(Factory, regLine, regHeader, entryHeader, cancelPreparationDate, MRNCode);

			TransactionsTestHelper.AssertProcessCancelationResponse_WithCONAndPNDTransaction_PreRequisites(regLine, regLine2, regHeader, guarantee, regLineTransaction1, regLineTransaction2, regLineTransaction3, regLineTransaction4, regLineTransaction5);

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusCAMessage, InterchangeID);
			ProcessMessageForTest(message);

			TransactionsTestHelper.AssertProcessCancelationResponse_WithCONAndPNDTransaction(regLine, regLine2, regHeader, entryHeader, guarantee, regLineTransaction1, regLineTransaction2, regLineTransaction3, regLineTransaction4, regLineTransaction5, cancelPreparationDate, MRNCode, TransactionsAESCommentPrefix);

			message = CreateNewEDIMessage(entryHeader.CH_BGMReference, acceptanceTestFileStatusCAMessage, InterchangeID);
			ProcessMessageForTest(message);

			TransactionsTestHelper.AssertProcessCancelationResponse_WithCONAndPNDTransaction_AfterProcess(regLine, regLine2, guarantee);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();

		instruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.B;

		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = instruction.PK;
		invoiceLine1.JI_Tariff = "2203001011";
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = instruction.PK;
		invoiceLine2.JI_Tariff = "2203001012";
		var invoiceLine3 = invoice.InvoiceLines.AddNew();
		invoiceLine3.JI_CEI = instruction.PK;
		invoiceLine3.JI_Tariff = "2203001013";
		var invoiceLine4 = invoice.InvoiceLines.AddNew();
		invoiceLine4.JI_CEI = instruction.PK;
		invoiceLine4.JI_Tariff = "2203001014";

		entryHeader.MovementReferenceNumber = MRNCode;

		entryLine1 = entryHeader.AllEntryLines.AddNew();
		entryLine1.CL_LineNumber = 1;
		invoiceLine1.JI_CL = entryLine1.PK;

		entryLine2 = entryHeader.AllEntryLines.AddNew();
		entryLine2.CL_LineNumber = 2;
		invoiceLine2.JI_CL = entryLine2.PK;

		entryLine3 = entryHeader.AllEntryLines.AddNew();
		entryLine3.CL_LineNumber = 3;
		invoiceLine3.JI_CL = entryLine3.PK;

		entryLine4 = entryHeader.AllEntryLines.AddNew();
		entryLine4.CL_LineNumber = 4;
		invoiceLine4.JI_CL = entryLine4.PK;

		(var orgHeader, var orgAddress) = SetUpOrganization();

		regHeader = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegHeader>();
		regHeader.SRH_AppCode = "AAA";
		regHeader.SRH_Reference = "reference";
		regHeader.SRH_Status = "CLS";

		SetUpPremises(entryHeader, orgAddress, regHeader);

		SetUpGuaranteeForRegHeader(orgHeader, regHeader);

		regLine = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLine>();
		regLine.SRL_LineNumber = 1;
		regLine.SRL_SRH = regHeader.PK;
		regLine.SRL_CustomsStatus = "CLS";
	}
	EU.TemporaryStorage.Business.CusTempStorageRegHeader regHeader;
	EU.TemporaryStorage.Business.CusTempStorageRegLine regLine;
	CusEntryLine entryLine1;
	CusEntryLine entryLine2;
	CusEntryLine entryLine3;
	CusEntryLine entryLine4;

	void AddSupportingDocuments_EntryHeader()
	{
		entryHeader.AddEntryHeaderDocument<SupportingDocument>("X001", "ES3600000001");
		entryHeader.AddEntryHeaderDocument<SupportingDocument>("X002", "ES3600000002");
		entryHeader.AddEntryHeaderDocument<SupportingDocument>("X003", "ES3600000003");
	}

	void AddAdditionalReference_EntryHeader()
	{
		entryHeader.AddEntryHeaderDocument<AdditionalInfo>("X001", "ES3600000001", subType: "REF");
		entryHeader.AddEntryHeaderDocument<AdditionalInfo>("X002", "ES3600000002", subType: "REF");
		entryHeader.AddEntryHeaderDocument<AdditionalInfo>("X003", "ES3600000003", subType: "REF");
	}

	void AddAdditionalInformation_EntryHeader()
	{
		entryHeader.AddEntryHeaderDocument<AdditionalInfo>("X001", "ES3600000001", "INF");
		entryHeader.AddEntryHeaderDocument<AdditionalInfo>("X002", "ES3600000002", "INF");
		entryHeader.AddEntryHeaderDocument<AdditionalInfo>("X003", "ES3600000003", "INF");
	}

	void AddTransportDocument_EntryHeader()
	{
		entryHeader.AddEntryHeaderDocument<AdditionalInfo>("X001", "ES3600000001", "TRA");
		entryHeader.AddEntryHeaderDocument<AdditionalInfo>("X002", "ES3600000002", "TRA");
		entryHeader.AddEntryHeaderDocument<AdditionalInfo>("X003", "ES3600000003", "TRA");
	}

	void AddAdditionalReference_EntryLine()
	{
		entryLine1.AddEntryLineDocument<AdditionalInfo>("X001", "ES3600000001", "REF");
		entryLine1.AddEntryLineDocument<AdditionalInfo>("X002", "ES3600000002", "REF");
		entryLine2.AddEntryLineDocument<AdditionalInfo>("X004", "ES3600000001", "REF");
		entryLine2.AddEntryLineDocument<AdditionalInfo>("X006", "ES3600000003", "REF");
		entryLine3.AddEntryLineDocument<AdditionalInfo>("X008", "ES3600000002", "REF");
		entryLine3.AddEntryLineDocument<AdditionalInfo>("X009", "ES3600000003", "REF");
		entryLine4.AddEntryLineDocument<AdditionalInfo>("X047", "ES3600000001", "REF");
		entryLine4.AddEntryLineDocument<AdditionalInfo>("X049", "ES3600000003", "REF");
	}

	void AddAdditionalInformation_EntryLine()
	{
		entryLine1.AddEntryLineDocument<AdditionalInfo>("X101", "ES3600000001", "INF");
		entryLine1.AddEntryLineDocument<AdditionalInfo>("X102", "ES3600000002", "INF");
		entryLine2.AddEntryLineDocument<AdditionalInfo>("X104", "ES3600000001", "INF");
		entryLine2.AddEntryLineDocument<AdditionalInfo>("X106", "ES3600000003", "INF");
		entryLine3.AddEntryLineDocument<AdditionalInfo>("X107", "ES3600000001", "INF");
		entryLine3.AddEntryLineDocument<AdditionalInfo>("X109", "ES3600000003", "INF");
		entryLine4.AddEntryLineDocument<AdditionalInfo>("X148", "ES3600000002", "INF");
		entryLine4.AddEntryLineDocument<AdditionalInfo>("X149", "ES3600000003", "INF");
	}

	void AddTransportDocument_EntryLine()
	{
		entryLine1.AddEntryLineDocument<AdditionalInfo>("X201", "ES3600000001", "TRA");
		entryLine1.AddEntryLineDocument<AdditionalInfo>("X202", "ES3600000002", "TRA");
		entryLine2.AddEntryLineDocument<AdditionalInfo>("X204", "ES3600000001", "TRA");
		entryLine2.AddEntryLineDocument<AdditionalInfo>("X206", "ES3600000003", "TRA");
		entryLine3.AddEntryLineDocument<AdditionalInfo>("X207", "ES3600000001", "TRA");
		entryLine3.AddEntryLineDocument<AdditionalInfo>("X209", "ES3600000003", "TRA");
		entryLine4.AddEntryLineDocument<AdditionalInfo>("X248", "ES3600000002", "TRA");
		entryLine4.AddEntryLineDocument<AdditionalInfo>("X249", "ES3600000003", "TRA");
	}

	void AddPreviousDocuments_EntryLine()
	{
		entryLine1.AddEntryLineDocument<PreviousDocument>("P001", "ES4000000001");
		entryLine1.AddEntryLineDocument<PreviousDocument>("P002", "ES4000000002");
		entryLine2.AddEntryLineDocument<PreviousDocument>("P003", "ES4000000003");
		entryLine2.AddEntryLineDocument<PreviousDocument>("P004", "ES4000000004");
		entryLine3.AddEntryLineDocument<PreviousDocument>("P005", "ES4000000005");
		entryLine3.AddEntryLineDocument<PreviousDocument>("P006", "ES4000000006");
		entryLine4.AddEntryLineDocument<PreviousDocument>("P007", "ES4000000007");
		entryLine4.AddEntryLineDocument<PreviousDocument>("P008", "ES4000000008");
	}

	void AddSupportingDocuments_EntryLine()
	{
		entryLine1.AddEntryLineDocument<SupportingDocument>("X001", "ES3600000001");
		entryLine1.AddEntryLineDocument<SupportingDocument>("X002", "ES3600000002");
		entryLine2.AddEntryLineDocument<SupportingDocument>("X003", "ES3600000003");
		entryLine2.AddEntryLineDocument<SupportingDocument>("X004", "ES3600000004");
		entryLine3.AddEntryLineDocument<SupportingDocument>("X005", "ES3600000005");
		entryLine3.AddEntryLineDocument<SupportingDocument>("X006", "ES3600000006");
		entryLine4.AddEntryLineDocument<SupportingDocument>("X007", "ES3600000007");
		entryLine4.AddEntryLineDocument<SupportingDocument>("X008", "ES3600000008");
	}

	string acceptanceTestFileStatusPAGreenCircuitMessageAEAT => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryAESTestFilePath, "AcceptedStatusPAGreenCircuitMessageAEAT.txt");
	string acceptanceTestFileStatusPARedCircuitMessageAEAT => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryAESTestFilePath, "AcceptedStatusPARedCircuitMessageAEAT.txt");
	string acceptanceTestFileStatusPAGreenCircuitMessageATC => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryAESTestFilePath, "AcceptedStatusPAGreenCircuitMessageATC.txt");
	string acceptanceTestFileStatusPAOrangeCircuitMessageATC => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryAESTestFilePath, "AcceptedStatusPAOrangeCircuitMessageATC.txt");
	string acceptanceTestFileStatusPIMessage => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryAESTestFilePath, "AcceptedStatusPIMessage.txt");
	string acceptanceTestFileStatusNLMessage => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryAESTestFilePath, "AcceptedStatusNLMessage.txt");
	string acceptanceTestFileStatusPLMessage => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryAESTestFilePath, "AcceptedStatusPLMessage.txt");
	string acceptanceTestFileStatusCAMessage => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryAESTestFilePath, "AcceptedStatusCAMessage.txt");
	string acceptanceTestFileStatusSAMessage => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryAESTestFilePath, "AcceptedStatusSAMessage.txt");
	string acceptanceTestFileStatusSTMessage => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryAESTestFilePath, "AcceptedStatusSTMessage.txt");
	string acceptanceTestFileStatusAWNoCSVClearanceMessage => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryAESTestFilePath, "AcceptedStatusAWNoCSVClearanceMessage.txt");
	string acceptanceTestFileStatusAWWithCSVClearanceMessage => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryAESTestFilePath, "AcceptedStatusAWWithCSVClearanceMessage.txt");
	string acceptanceTestFileStatusIVINVMessage => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryAESTestFilePath, "AcceptedStatusIVINVMessage.txt");
	string acceptanceTestFileStatusIVCANMessage => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryAESTestFilePath, "AcceptedStatusIVCANMessage.txt");
	string acceptanceTestFileStatusOtherNoCSVClearanceMessage => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryAESTestFilePath, "AcceptedStatusOtherNoCSVClearanceMessage.txt");
	string acceptanceTestFileStatusOtherWithCSVClearanceMessage => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryAESTestFilePath, "AcceptedStatusOtherWithCSVClearanceMessage.txt");
	string acceptanceTestFileWithSupportingDocumentsMessage => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryAESTestFilePath, "AcceptedWithSupportingDocumentsMessage.txt");
	string acceptanceTestFileWithGoodsShipmentAllDocumentsMessage => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryAESTestFilePath, "AcceptedWithGoodsShipmentAllDocumentsMessage.txt");
	string acceptanceTestFileWithGoodsShipmentSupportingDocumentsMessage => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryAESTestFilePath, "AcceptedWithGoodsShipmentSupportingDocumentsMessage.txt");
	string acceptanceTestFileWithGoodsShipmentAdditionalReferenceMessage => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryAESTestFilePath, "AcceptedWithGoodsShipmentAdditionalReferenceMessage.txt");
	string acceptanceTestFileWithGoodsShipmentAdditionalInformationMessage => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryAESTestFilePath, "AcceptedWithGoodsShipmentAdditionalInformationMessage.txt");
	string acceptanceTestFileWithGoodsShipmentConsignmentTransportDocumentMessage => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryAESTestFilePath, "AcceptedWithGoodsShipmentConsignmentTransportDocumentMessage.txt");
	string acceptanceTestFileWithGoodsItemAdditionalReferencesMessage => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryAESTestFilePath, "AcceptedWithGoodsItemAdditionalReferencesMessage.txt");
	string acceptanceTestFileWithGoodsItemTransportDocumentMessage => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryAESTestFilePath, "AcceptedWithGoodsItemTransportDocumentMessage.txt");
	string acceptanceTestFileWithGoodsItemAdditionalInformationMessage => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryAESTestFilePath, "AcceptedWithGoodsItemAdditionalInformationMessage.txt");
	string acceptanceTestFileWithPreviousDocumentsMessage => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryAESTestFilePath, "AcceptedWithPreviousDocumentsMessage.txt");
	string acceptanceTestFileWithAllDocsInGoodsItemMessage => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryAESTestFilePath, "AcceptedWithAllDocsMessage.txt");
	string acceptanceTestFileWithGoodsShipmentAndGoodsItemMessage => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryAESTestFilePath, "AcceptedWithGoodsShipmentAndGoodsItemMessage.txt");
	string acceptanceTestFileAdditionalDeclarationXMessage => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryAESTestFilePath, "AcceptedMessageAdditionalDeclarationX.txt");
	string acceptanceTestFileAdditionalDeclarationYMessage => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryAESTestFilePath, "AcceptedMessageAdditionalDeclarationY.txt");

	protected override string GetRejectedTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryAESTestFilePath, "RejectedMessage.txt");
	protected override string GetErrorTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryAESTestFilePath, "ErrorMessage.txt");
	protected override string GetAcceptanceTestFileWithLongSegmentId() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryAESTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

	readonly ZDateTime acceptanceDate = new ZDateTime(2022, 10, 11);
	readonly ZDateTime entryReleaseDate = new ZDateTime(2022, 10, 12);
	const string CsvExitCertificate = "A1234567890";
	const string CsvT2L = "UMAMEA2RAHWEE23R";

	protected override ZString GetExpectedProcessorFriendlyName() => "Export Query Declaration Message Processor";

	protected override ZString RejectedMRN => MRNCode;

	protected override ZDateTime RejectedAcceptanceDate => ZDateTime.Empty;

	protected override ZString RejectedMessageStatus => EDIMessage.Status.Rejected;

	protected override ZString ErrorMRN => MRNCode;

	protected override ZDateTime ErrorAcceptanceDate => ZDateTime.Empty;

	protected override ZString ErrorMessageInterpretation => "<H3>Rejected Declaration</H3>" +
			"<H4>List of Errors:</H4>" +
			"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
			"<tr><td><strong>Line / Column</strong></td><td><strong>Location</strong></td><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
			"<tr><td>14 / 34</td><td>782</td><td>18</td><td>Se esperaba nodo {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CCAESCV1Ent.xsd}security y ha venido {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CCAESCV1Ent.xsd}totalAmountInvoiced</td><td>Wrong value</td></tr>" +
			"</table>";

	protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.ExportQuery };

	protected override QueryAESResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new QueryAESResponseMessageProcessor(logger, new BranchCustomsMessageProcessorForTest());

	CusTempStorageRegLineTransaction SetUpTransaction(CusTempStorageRegLine regLine, ZString status, ZString referenceNum, string referenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration)
	{
		var regLineTransaction1 = regLine.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction1.SRT_TransactionStatus = status;
		regLineTransaction1.SRT_InternalReferenceNumber = referenceNum;
		regLineTransaction1.SRT_InternalReferenceType = referenceType;
		regLineTransaction1.SRT_PhysicalInOutDate = cancelPreparationDate.ToDateTimeOffset(null);
		regLineTransaction1.SRT_Reference = MRNCode;

		return regLineTransaction1;
	}

	readonly ZDateTime cancelPreparationDate = new ZDateTime(2022, 10, 11, 16, 25, 40);
}
