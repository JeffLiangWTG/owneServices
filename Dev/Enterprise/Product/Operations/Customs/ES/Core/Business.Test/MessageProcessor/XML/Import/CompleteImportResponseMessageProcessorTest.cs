using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.ImportacionCompletaV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Testing;
using NUnit.Framework;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class CompleteImportResponseMessageProcessorTest : ImportGenericResponseMessageProcessorTest<CompleteImportResponseMessageProcessor, ImportacionCompletaV1Sal>
	{
		public void TestProcessPDCAcceptedGreeCircuitMessageOnlyMainland()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileWithClearanceOnlyMainland(), InterchangeID);

			ProcessMessageForTest(message);
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: ZString.Empty, messageSubType: "ACC", messageNum: MessageNum, entryStatusCode: EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, movementReferenceNumber: MRNCode, circuit: CircuitCodeList.Codes.GREEN, acceptanceDate: AcceptanceDate, entryReleaseDate: EntryReleaseDate, limitPaymentDate: LimitPaymentDate, csvClearance: CsvClearance, csvImportCertificate: CsvImportCertificate, paymentProofNumber: PaymentProofNumber, circuitCan: ZString.Empty, atcLimitPaymentDate: ZDateTime.Empty, atcPaymentProofNumber: ZString.Empty, parallel: true);
		}

		string GetAcceptanceTestFileWithClearanceOnlyMainland() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CompleteImportTestFilePath, "AcceptedGreenCircuitMessageOnlyMainland.txt");

		public void TestProcessPDCAcceptedGreenCircuitMessageEntryInstructionC()
		{
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.C;

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearance, InterchangeID);

			ProcessMessageForTest(message);
			AssertAcceptedMessage(message, entryHeader, entryStatusCode: EntryStatusCodes.Cleared, circuitCan: CircuitCodeList.Codes.ORANGE, parallel: true);
		}
		public void TestProcessPDCAcceptedGreenCircuitMessageEntryInstructionZ()
		{
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.Z;

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearance, InterchangeID);

			ProcessMessageForTest(message);
			AssertAcceptedMessage(message, entryHeader, circuitCan: CircuitCodeList.Codes.ORANGE, parallel: true);
		}

		[TestDate(2020, 10, 05, 09, 36, 0)]
		public void TestProcessPDCAcceptedGreenCircuitMessageEntryInstructionB()
		{
			entryHeader.TotalAmount = 380;

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearance, InterchangeID);

			ProcessMessageForTest(message);

			var expectedMessageInterpretation =
				"<H3>Accepted Declaration</H3>" +
				"<table border=\"0\"><tr><td>Description:</td><td>&nbsp;&nbsp;</td><td>(0)Admisión de DUA</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>04-11-2020, 00:00:00</td></tr>" +
				"<tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES00999930006184</td></tr>" +
				"<tr><td>Export Doc. (MRN):</td><td>&nbsp;&nbsp;</td><td>20EXP0999930006184</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr>" +
				"<tr><td>ATC Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>TEST444444444444</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>04-11-2020, 00:00:00</td></tr>" +
				"<tr><td>Import cert. (CSV):</td><td>&nbsp;&nbsp;</td><td>CRTF444444444444</td></tr></table>" +
				"<br><H2>Taxes and fees data</H2><br>" +
				"<table border=\"0\"><tr><td>Total:</td><td>&nbsp;&nbsp;</td><td>380</td></tr>" +
				"<tr><td>Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>310</td></tr>" +
				"<tr><td>ATC Total:</td><td>&nbsp;&nbsp;</td><td>100</td></tr>" +
				"<tr><td>ATC Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>75</td></tr>" +
				"<tr><td>Total Deferred VAT:</td><td>&nbsp;&nbsp;</td><td>35</td></tr>" +
				"<tr><td>Clearance Guarantee VAT Exemption:</td><td>&nbsp;&nbsp;</td><td>80</td></tr>" +
				"<tr><td>Real Clearance Guarantee:</td><td>&nbsp;&nbsp;</td><td>300</td></tr>" +
				"<tr><td>Pendency Guarantee VAT Exemption:</td><td>&nbsp;&nbsp;</td><td>10</td></tr>" +
				"<tr><td>Real Pendency Guarantee:</td><td>&nbsp;&nbsp;</td><td>10</td></tr></table><br>" +
				"<H2>Payment information</H2><br>" +
				"<table border=\"0\"><tr><td>Payment Proof Number:</td><td>&nbsp;&nbsp;</td><td>1234</td></tr>" +
				"<tr><td>Payment date limit:</td><td>&nbsp;&nbsp;</td><td>06-06-2079</td></tr>" +
				"<tr><td>ATC Proof of Payment Number:</td><td>&nbsp;&nbsp;</td><td>JUSTPAGOATC</td></tr>" +
				"<tr><td>ATC payment date limit:</td><td>&nbsp;&nbsp;</td><td>07-06-2021</td></tr></table><br>" +
				"<br><H2>Guarantees</H2><br>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
				"<thead><tr class=\"tableheadings\"><th>Customs</th><th>GRN</th><th>Real Debt</th><th>Potential Debt</th><th>Undetermined Real Debt</th></tr></thead>" +
				"<tr><td>AEAT</td><td>16ESAGL9990000096</td><td>387.51</td><td>5</td><td>30</td></tr>" +
				"<tr><td>ATC</td><td>16ESAGL9990000096</td><td>350</td><td>5</td><td>25</td></tr>" +
				"</table><br><br><H2>Taxes and fees response (Spanish Customs)</H2><br>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Item</th><th>Type</th><th>MAX/MIN Rate</th><th>Base Amount</th><th>Tax Rate</th><th>Total Amount</th><th>Total Guaranteed Amount</th></tr></thead>" +
				"<tr><td>1</td><td>A00</td><td>&nbsp;</td><td>100.650</td><td>2.700000 %</td><td>2.72</td><td>2.72</td></tr>" +
				"<tr><td>1</td><td>B00</td><td>MA</td><td>104.950</td><td>21.000000 %</td><td>22.04</td><td>22.04</td></tr>" +
				"<tr><td>2</td><td>A00</td><td>&nbsp;</td><td>1219.350</td><td>17.600000 %</td><td>214.61</td><td>214.61</td></tr>" +
				"<tr><td>2</td><td>B00</td><td>&nbsp;</td><td>1481.380</td><td>10.000000 €/KN</td><td>148.14</td><td>148.14</td></tr>" +
				"<tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>Total: </td><td>387.51</td><td>387.51</td></tr>" +
				"</table><br><br><H2>Required Certificates</H2><br><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead>" +
				"<tr class=\"tableheadings\"><th>Item</th><th>Measure</th><th>Agency</th><th>Documents</th></tr></thead>" +
				"<tr><td>2</td><td>SNM</td><td>SIF05 -  Sanidad Exterior - M&#186; Sanidad</td><td>N851, C085</td></tr>" +
				"<tr><td>2</td><td>SNM</td><td>SIF06 -  Sanidad Inetrior - M&#186; Sanidad</td><td>C657, C678</td></tr>" +
				"</table>";
			AssertAcceptedMessage(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, circuitCan: CircuitCodeList.Codes.ORANGE, parallel: false);

			CombineAssertions("Test EntryLines fees", () =>
			{
				AssertEquals("EntryLine1.Fees count", 2, entryLine1.Fees.Count);

				AssertEquals("EntryLine1.Fee[0].CF_ChargeType", "A00", entryLine1.Fees[0].CF_ChargeType);
				AssertEquals("EntryLine1.Fee[0].CF_BaseValue", 100.650M, entryLine1.Fees[0].CF_BaseValue);
				AssertEquals("EntryLine1.Fee[0].CF_Rate", 2.700000M, entryLine1.Fees[0].CF_Rate);
				AssertEquals("EntryLine1.Fee[0].MaxMin", ZString.Empty, entryLine1.Fees[0].MaxMin);
				AssertEquals("EntryLine1.Fee[0].G4_RateDuty", "%", entryLine1.Fees[0].G4_RateDuty);
				AssertEquals("EntryLine1.Fee[0].CF_ChargeAmount", 2.72M, entryLine1.Fees[0].CF_ChargeAmount);
				AssertEquals("EntryLine1.Fee[0].CF_RateOverrideReasonCode", "OVR", entryLine1.Fees[0].CF_RateOverrideReasonCode);

				AssertEquals("EntryLine1.Fee[1].CF_ChargeType", "B00", entryLine1.Fees[1].CF_ChargeType);
				AssertEquals("EntryLine1.Fee[1].CF_BaseValue", 104.950M, entryLine1.Fees[1].CF_BaseValue);
				AssertEquals("EntryLine1.Fee[1].CF_Rate", 21.000000M, entryLine1.Fees[1].CF_Rate);
				AssertEquals("EntryLine1.Fee[1].MaxMin", "MA", entryLine1.Fees[1].MaxMin);
				AssertEquals("EntryLine1.Fee[1].G4_RateDuty", "%", entryLine1.Fees[1].G4_RateDuty);
				AssertEquals("EntryLine1.Fee[1].CF_ChargeAmount", 22.04M, entryLine1.Fees[1].CF_ChargeAmount);
				AssertEquals("EntryLine1.Fee[1].CF_RateOverrideReasonCode", "OVR", entryLine1.Fees[1].CF_RateOverrideReasonCode);

				AssertEquals("EntryLine2.Fees count", 2, entryLine2.Fees.Count);

				AssertEquals("EntryLine2.Fee[0].CF_ChargeType", "A00", entryLine2.Fees[0].CF_ChargeType);
				AssertEquals("EntryLine2.Fee[0].CF_BaseValue", 1219.350M, entryLine2.Fees[0].CF_BaseValue);
				AssertEquals("EntryLine2.Fee[0].CF_Rate", 17.600000M, entryLine2.Fees[0].CF_Rate);
				AssertEquals("EntryLine2.Fee[0].MaxMin", ZString.Empty, entryLine2.Fees[0].MaxMin);
				AssertEquals("EntryLine2.Fee[0].G4_RateDuty", "%", entryLine2.Fees[0].G4_RateDuty);
				AssertEquals("EntryLine2.Fee[0].CF_ChargeAmount", 214.61M, entryLine2.Fees[0].CF_ChargeAmount);
				AssertEquals("EntryLine2.Fee[0].CF_RateOverrideReasonCode", "OVR", entryLine2.Fees[0].CF_RateOverrideReasonCode);

				AssertEquals("EntryLine2.Fee[1].CF_ChargeType", "B00", entryLine2.Fees[1].CF_ChargeType);
				AssertEquals("EntryLine2.Fee[1].CF_BaseValue", 1481.380M, entryLine2.Fees[1].CF_BaseValue);
				AssertEquals("EntryLine2.Fee[1].CF_Rate", 10.000000M, entryLine2.Fees[1].CF_Rate);
				AssertEquals("EntryLine2.Fee[1].MaxMin", ZString.Empty, entryLine2.Fees[1].MaxMin);
				AssertEquals("EntryLine2.Fee[1].G4_RateDuty", "KGM", entryLine2.Fees[1].G4_RateDuty);
				AssertEquals("EntryLine2.Fee[1].CF_RateOverrideReasonCode", "OVR", entryLine2.Fees[1].CF_RateOverrideReasonCode);

				AssertEquals("EntryLine3.Fees count (not deleted)", 2, entryLine3.Fees.Count);

				AssertEquals("EntryLine3.FeeVAT.CF_MethodOfPayment is DEF", UniversalReferenceConstants.FeeMethodOfPayment.Deferred, feeVAT.CF_MethodOfPayment);
				AssertEquals("EntryLine3.FeeNotVAT.CF_MethodOfPayment empty", ZString.Empty, feeNotVAT.CF_MethodOfPayment);
			});
		}

		public void TestProcessPDCAcceptedGreenCircuitMessageEntryInstructionBWithExistingMRN()
		{
			entryHeader.MovementReferenceNumberSetter(MRNCode, MovementReferenceNumberIssueDate);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearance, InterchangeID);

			ProcessMessageForTest(message);
			AssertAcceptedMessage(message, entryHeader, circuitCan: CircuitCodeList.Codes.ORANGE, parallel: true);
		}

		public void TestProcessPDCAcceptedGreenCircuitEntryInstructionBDifferedMessage()
		{
			entryHeader.TotalAmount = ZDecimal.Zero;

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearance, InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretation = "<H3>Warning: Taxes and fees data received differ from sent data</H3>";
			AssertAcceptedMessage(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, circuitCan: CircuitCodeList.Codes.ORANGE, parallel: true);
		}

		public void TestProcessPDCAcceptedGreenCircuitEntryInstructionNotBorZ()
		{
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.A;

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearance, InterchangeID);
			ProcessMessageForTest(message);
			AssertAcceptedMessage(message, entryHeader, entryStatusCode: EntryStatusCodes.Cleared, circuitCan: CircuitCodeList.Codes.ORANGE, parallel: true);
		}

		public void TestProcessPDCAcceptedYellowCircuitMessage()
		{
			entryHeader.TotalAmount = 380;
			AddFeesToEntryLine();
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileYellowCircuit(), InterchangeID);

			ProcessMessageForTest(message);
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: ZString.Empty, messageSubType: "ACC", messageNum: MessageNum, entryStatusCode: EntryStatusCodes.ClearedWithPendingDocuments, movementReferenceNumber: MRNCode, circuit: CircuitCodeList.Codes.YELLOW, acceptanceDate: AcceptanceDate, entryReleaseDate: ZDateTime.Empty, limitPaymentDate: ZDateTime.Empty, csvClearance: ZString.Empty, csvImportCertificate: ZString.Empty, paymentProofNumber: ZString.Empty, circuitCan: CircuitCodeList.Codes.YELLOW, atcLimitPaymentDate: ZDateTime.Empty, atcPaymentProofNumber: ZString.Empty, parallel: true);

			CombineAssertions("Test EntryLines fees", () =>
			{
				AssertEquals("EntryLine1.Fees count", 2, entryLine1.Fees.Count);

				AssertEquals("EntryLine1.Fee[0].CF_ChargeType", "A00", entryLine1.Fees[0].CF_ChargeType);
				AssertEquals("EntryLine1.Fee[0].CF_BaseValue", 100.650M, entryLine1.Fees[0].CF_BaseValue);
				AssertEquals("EntryLine1.Fee[0].CF_Rate", 2.700000M, entryLine1.Fees[0].CF_Rate);
				AssertEquals("EntryLine1.Fee[0].MaxMin", ZString.Empty, entryLine1.Fees[0].MaxMin);
				AssertEquals("EntryLine1.Fee[0].G4_RateDuty", "%", entryLine1.Fees[0].G4_RateDuty);
				AssertEquals("EntryLine1.Fee[0].CF_ChargeAmount", 2.72M, entryLine1.Fees[0].CF_ChargeAmount);
				AssertEquals("EntryLine1.Fee[0].CF_RateOverrideReasonCode", "OVR", entryLine1.Fees[0].CF_RateOverrideReasonCode);

				AssertEquals("EntryLine1.Fee[1].CF_ChargeType", "B00", entryLine1.Fees[1].CF_ChargeType);
				AssertEquals("EntryLine1.Fee[1].CF_BaseValue", 104.950M, entryLine1.Fees[1].CF_BaseValue);
				AssertEquals("EntryLine1.Fee[1].CF_Rate", 21.000000M, entryLine1.Fees[1].CF_Rate);
				AssertEquals("EntryLine1.Fee[1].MaxMin", "MA", entryLine1.Fees[1].MaxMin);
				AssertEquals("EntryLine1.Fee[1].G4_RateDuty", "%", entryLine1.Fees[1].G4_RateDuty);
				AssertEquals("EntryLine1.Fee[1].CF_ChargeAmount", 22.04M, entryLine1.Fees[1].CF_ChargeAmount);
				AssertEquals("EntryLine1.Fee[1].CF_RateOverrideReasonCode", "OVR", entryLine1.Fees[1].CF_RateOverrideReasonCode);

				AssertEquals("EntryLine2.Fees count", 2, entryLine2.Fees.Count);

				AssertEquals("EntryLine2.Fee[0].CF_ChargeType", "A00", entryLine2.Fees[0].CF_ChargeType);
				AssertEquals("EntryLine2.Fee[0].CF_BaseValue", 1219.350M, entryLine2.Fees[0].CF_BaseValue);
				AssertEquals("EntryLine2.Fee[0].CF_Rate", 17.600000M, entryLine2.Fees[0].CF_Rate);
				AssertEquals("EntryLine2.Fee[0].MaxMin", ZString.Empty, entryLine2.Fees[0].MaxMin);
				AssertEquals("EntryLine2.Fee[0].G4_RateDuty", "%", entryLine2.Fees[0].G4_RateDuty);
				AssertEquals("EntryLine2.Fee[0].CF_ChargeAmount", 214.61M, entryLine2.Fees[0].CF_ChargeAmount);
				AssertEquals("EntryLine2.Fee[0].CF_RateOverrideReasonCode", "OVR", entryLine2.Fees[0].CF_RateOverrideReasonCode);

				AssertEquals("EntryLine2.Fee[1].CF_ChargeType", "B00", entryLine2.Fees[1].CF_ChargeType);
				AssertEquals("EntryLine2.Fee[1].CF_BaseValue", 1481.380M, entryLine2.Fees[1].CF_BaseValue);
				AssertEquals("EntryLine2.Fee[1].CF_Rate", 10.000000M, entryLine2.Fees[1].CF_Rate);
				AssertEquals("EntryLine2.Fee[1].MaxMin", ZString.Empty, entryLine2.Fees[1].MaxMin);
				AssertEquals("EntryLine2.Fee[1].G4_RateDuty", "KGM", entryLine2.Fees[1].G4_RateDuty);
				AssertEquals("EntryLine2.Fee[1].CF_ChargeAmount", 148.14M, entryLine2.Fees[1].CF_ChargeAmount);
				AssertEquals("EntryLine2.Fee[1].CF_RateOverrideReasonCode", "OVR", entryLine2.Fees[1].CF_RateOverrideReasonCode);

				AssertEquals("EntryLine3.Fees count (not deleted)", 2, entryLine3.Fees.Count);

				AssertEquals("EntryLine3.FeeVAT.CF_MethodOfPayment is DEF", UniversalReferenceConstants.FeeMethodOfPayment.Deferred, feeVAT.CF_MethodOfPayment);
				AssertEquals("EntryLine3.FeeNotVAT.CF_MethodOfPayment empty", ZString.Empty, feeNotVAT.CF_MethodOfPayment);
			});
		}

		public void TestProcessPDCAcceptedYellowCircuitMessageWithStatusPDA()
		{
			entryHeader.TotalAmount = 380;
			AddFeesToEntryLine();
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileYellowCircuitPDAStatus(), InterchangeID);

			ProcessMessageForTest(message);
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: ZString.Empty, messageSubType: "ACC", messageNum: MessageNum, entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, movementReferenceNumber: MRNCode, circuit: ZString.Empty, acceptanceDate: AcceptanceDate, entryReleaseDate: ZDateTime.Empty, limitPaymentDate: ZDateTime.Empty, csvClearance: ZString.Empty, csvImportCertificate: ZString.Empty, paymentProofNumber: ZString.Empty, circuitCan: CircuitCodeList.Codes.YELLOW, atcLimitPaymentDate: ZDateTime.Empty, atcPaymentProofNumber: ZString.Empty, parallel: true);

			CombineAssertions("Test EntryLines fees", () =>
			{
				AssertEquals("EntryLine1.Fees count (not deleted and nothing added)", 2, entryLine1.Fees.Count);

				AssertEquals("EntryLine1.Fee[0].CF_ChargeType", "A00", entryLine1.Fees[0].CF_ChargeType);
				AssertEquals("EntryLine1.Fee[0].CF_BaseValue", 200.650M, entryLine1.Fees[0].CF_BaseValue);
				AssertEquals("EntryLine1.Fee[0].CF_Rate", "MA", entryLine1.Fees[0].MaxMin);
				AssertEquals("EntryLine1.Fee[0].G4_RateDuty", ZString.Empty, entryLine1.Fees[0].G4_RateDuty);
				AssertEquals("EntryLine1.Fee[0].CF_ChargeAmount", 3.72M, entryLine1.Fees[0].CF_ChargeAmount);
				AssertEquals("EntryLine1.Fee[0].CF_RateOverrideReasonCode", "ADD", entryLine1.Fees[0].CF_RateOverrideReasonCode);

				AssertEquals("EntryLine1.Fee[1].CF_ChargeType", "300", entryLine1.Fees[1].CF_ChargeType);
				AssertEquals("EntryLine1.Fee[1].CF_BaseValue", 200.650M, entryLine1.Fees[1].CF_BaseValue);
				AssertEquals("EntryLine1.Fee[1].CF_Rate", 3.700000M, entryLine1.Fees[1].CF_Rate);
				AssertEquals("EntryLine1.Fee[1].MaxMin", "MA", entryLine1.Fees[1].MaxMin);
				AssertEquals("EntryLine1.Fee[1].G4_RateDuty", ZString.Empty, entryLine1.Fees[1].G4_RateDuty);
				AssertEquals("EntryLine1.Fee[1].CF_ChargeAmount", 3.72M, entryLine1.Fees[1].CF_ChargeAmount);
				AssertEquals("EntryLine1.Fee[1].CF_RateOverrideReasonCode", "ADD", entryLine1.Fees[1].CF_RateOverrideReasonCode);

				AssertEquals("EntryLine2.Fees count (not added)", 0, entryLine2.Fees.Count);

				AssertEquals("EntryLine3.Fees count (not deleted)", 2, entryLine3.Fees.Count);

				AssertEquals("EntryLine3.FeeVAT.CF_MethodOfPayment is DEF", UniversalReferenceConstants.FeeMethodOfPayment.Deferred, feeVAT.CF_MethodOfPayment);
				AssertEquals("EntryLine3.FeeNotVAT.CF_MethodOfPayment empty", ZString.Empty, feeNotVAT.CF_MethodOfPayment);
			});
		}

		public void TestProcessPDCWithSentCMPAcceptedEntryInstructionSubStyle()
		{
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			CombineAssertions("When PDC message type and CMP message sub type is accepted, update the sub style value in the related entry instruction from C to Y", () =>
			{
				AssertEntryInstruction("A", "A");
				AssertEntryInstruction("C", "C");

				SentMessage.EM_MessageSubType = "CMP";
				AssertEntryInstruction("A", "A");
				AssertEntryInstruction("C", "Y");
			});

			void AssertEntryInstruction(ZString initialSubStyle, ZString expectedSubStyle)
			{
				entryInstruction.CEI_SubStyle = initialSubStyle;
				entryInstruction.Factory.Save();

				var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearance, InterchangeID);

				ProcessMessageForTest(message);

				AssertEquals($"Entry Instruction SubStyle '{initialSubStyle}'", expectedSubStyle, entryInstruction.CEI_SubStyle);
			}
		}

		public void TestProcessPDSAcceptedMessageAndTriggerInboxRequestWhenPreviousDocumentInDeclaration_EHub()
		{
			declaration.PreviousDocuments.AddNew();

			AssertProcessPDSAcceptedMessageAndTriggerInboxRequestWithPreviousDocument_EHub();
		}

		public void TestProcessPDSAcceptedMessageAndTriggerInboxRequestWhenPreviousDocumentInDeclaration_xT()
		{
			declaration.PreviousDocuments.AddNew();

			AssertProcessPDSAcceptedMessageAndTriggerInboxRequestWithPreviousDocument_xT();
		}

		public void TestProcessPDSAcceptedMessageAndTriggerInboxRequestWhenPreviousDocumentInInvoiceHeader_EHub()
		{
			invoice.PreviousDocuments.AddNew();

			AssertProcessPDSAcceptedMessageAndTriggerInboxRequestWithPreviousDocument_EHub();
		}

		public void TestProcessPDSAcceptedMessageAndTriggerInboxRequestWhenPreviousDocumentInInvoiceHeader_xT()
		{
			invoice.PreviousDocuments.AddNew();

			AssertProcessPDSAcceptedMessageAndTriggerInboxRequestWithPreviousDocument_xT();
		}

		public void TestProcessPDSAcceptedMessageAndTriggerInboxRequestWhenPreviousDocumentInInvoiceLines_EHub()
		{
			invoiceLine1.PreviousDocuments.AddNew();
			invoiceLine2.PreviousDocuments.AddNew();
			invoiceLine3.PreviousDocuments.AddNew();

			AssertProcessPDSAcceptedMessageAndTriggerInboxRequestWithPreviousDocument_EHub();
		}

		public void TestProcessPDSAcceptedMessageAndTriggerInboxRequestWhenPreviousDocumentInInvoiceLines_xT()
		{
			invoiceLine1.PreviousDocuments.AddNew();
			invoiceLine2.PreviousDocuments.AddNew();
			invoiceLine3.PreviousDocuments.AddNew();

			AssertProcessPDSAcceptedMessageAndTriggerInboxRequestWithPreviousDocument_xT();
		}

		public void TestProcessPDSAcceptedMessageAndNotTriggerInboxRequestWhenNoPreviousDocuments()
		{
			AssertProcessPDSAcceptedMessageAndNotTriggerInboxRequestWhenNoPreviousDocument();
		}

		public void TestProcessPDSAcceptedMessageAndNotTriggerInboxRequestWhenNoPreviousDocumentInOneInvoiceLine()
		{
			invoiceLine1.PreviousDocuments.AddNew();
			invoiceLine2.PreviousDocuments.AddNew();

			AssertProcessPDSAcceptedMessageAndNotTriggerInboxRequestWhenNoPreviousDocument();
		}

		public void TestProcessAcceptedMessageCreatesEntryLinePreviousDocuments()
		{
			entryHeader.TotalAmount = 380;

			var prevDoc1 = declaration.PreviousDocuments.AddNew();
			prevDoc1.CSI_Code = "X001";
			prevDoc1.CSI_ReferenceNumber = "ES3600000001";

			var prevDoc2 = declaration.PreviousDocuments.AddNew();
			prevDoc2.CSI_Code = "X002";
			prevDoc2.CSI_ReferenceNumber = "ES3600000002";

			var prevDoc3 = declaration.PreviousDocuments.AddNew();
			prevDoc3.CSI_Code = "X003";
			prevDoc3.CSI_ReferenceNumber = "ES3600000003";

			foreach (CusEntryLine entryLine in entryHeader.MergedLines)
			{
				entryLine.AddEntryLineDocument<PreviousDocument>("AAA", "10,00", subType: "LIQ", status: "ACC");
				entryLine.AddEntryLineDocument<PreviousDocument>("BBB", "10,00", subType: "LIQ", status: ZString.Empty);
				entryLine.AddEntryLineDocument<PreviousDocument>("CCC", "10,00", subType: "LIQ", status: "REJ");
				entryLine.AddEntryLineDocument<PreviousDocument>("DDD", "10,00", subType: ZString.Empty, status: ZString.Empty);
			}

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearance, InterchangeID);

			ProcessMessageForTest(message);
			AssertAcceptedMessage(message, entryHeader, entryStatusCode: EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, circuitCan: CircuitCodeList.Codes.ORANGE);

			CombineAssertions(() =>
			{
				foreach (CusEntryLine entryLine in entryHeader.MergedLines)
				{
					var clPrevDocs = GetCLPreviousDocuments(entryLine);
					AssertEquals("There is 1 entryLine PreviousDocuments after calling SavePreviousDocumentsForCL for entryLine with CL_LineNumber " + entryLine.CL_LineNumber, 1, clPrevDocs.Length);
					AssertEquals("The 1 EntryLine PreviousDocuments after calling ProcessImportEntryLinePreviousDocuments have the correct CSI_Codes", true, new ZString[] { "X001", "X002", "X003" }.Contains(clPrevDocs[0].CSI_Code));
					AssertEquals("The 1 entryLine PreviousDocuments after calling SavePreviousDocumentsForCL have the correct CSI_Statuc (ACC) for entryLine with CL_LineNumber " + entryLine.CL_LineNumber, true, clPrevDocs.All(x => x.CSI_Status == "ACC"));
				}
			});
		}

		public void TestProcessAcceptedMessageCreatesSupportingDocumentsDuplicates()
		{
			var suppDoc1 = declaration.SupportingDocuments.AddNew();
			suppDoc1.CSI_Code = "X001";
			suppDoc1.CSI_ReferenceNumber = "ES3600000001";

			var suppDoc2 = declaration.SupportingDocuments.AddNew();
			suppDoc2.CSI_Code = "X002";
			suppDoc2.CSI_ReferenceNumber = "ES3600000002";

			var suppDoc21 = declaration.SupportingDocuments.AddNew();
			suppDoc21.CSI_Code = "X002";
			suppDoc21.CSI_ReferenceNumber = "es3600000002";

			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.T2L;
			var suppDoc3 = entryInstruction.SupportingDocuments.AddNew();
			suppDoc3.CSI_Code = "X001";
			suppDoc3.CSI_ReferenceNumber = "ES3600000001";

			var suppDoc4 = declaration.SupportingDocuments.AddNew();
			suppDoc4.CSI_Code = "X003";
			suppDoc4.CSI_ReferenceNumber = "ES3600000003";

			Factory.Save();

			CombineAssertions(() =>
			{
				var chSupDocs = GetCHSupportingDocuments();
				AssertEquals("Before - There are 0 SupportingDocuments before calling ProcessImportEntryLineSupportingDocuments", 0, chSupDocs.Length);

				var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearance, InterchangeID);
				ProcessMessageForTest(message);

				var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction1.CEI_Style = Declaration.EntrySubStyleList.Codes.T2L;
				var suppDoc5 = entryInstruction1.SupportingDocuments.AddNew();
				suppDoc5.CSI_Code = "X002";
				suppDoc5.CSI_ReferenceNumber = "ES3600000002";

				var suppDoc51 = entryInstruction1.SupportingDocuments.AddNew();
				suppDoc51.CSI_Code = "X002";
				suppDoc51.CSI_ReferenceNumber = "es3600000002";

				var message1 = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearance, InterchangeID);
				ProcessMessageForTest(message1);

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
			entryHeader.TotalAmount = 380;

			var suppDoc1 = declaration.SupportingDocuments.AddNew();
			suppDoc1.CSI_Code = "X001";
			suppDoc1.CSI_ReferenceNumber = "ES3600000001";

			var suppDoc2 = declaration.SupportingDocuments.AddNew();
			suppDoc2.CSI_Code = "X002";
			suppDoc2.CSI_ReferenceNumber = "ES3600000002";

			var suppDoc3 = declaration.SupportingDocuments.AddNew();
			suppDoc3.CSI_Code = "X003";
			suppDoc3.CSI_ReferenceNumber = "ES3600000003";

			foreach (CusEntryLine entryLine in entryHeader.MergedLines)
			{
				entryLine.AddEntryLineDocument<SupportingDocument>("AAA", "10,00", subType: "LIQ", status: "ACC");
				entryLine.AddEntryLineDocument<SupportingDocument>("BBB", "10,00", subType: "LIQ", status: ZString.Empty);
				entryLine.AddEntryLineDocument<SupportingDocument>("CCC", "10,00", subType: "LIQ", status: "REJ");
				entryLine.AddEntryLineDocument<SupportingDocument>("DDD", "10,00", subType: ZString.Empty, status: ZString.Empty);
			}

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearance, InterchangeID);

			ProcessMessageForTest(message);
			AssertAcceptedMessage(message, entryHeader, entryStatusCode: EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, circuitCan: CircuitCodeList.Codes.ORANGE);

			CombineAssertions(() =>
			{
				foreach (CusEntryLine entryLine in entryHeader.MergedLines)
				{
					var clSupDocs = GetCLSupportingDocuments(entryLine);
					AssertEquals("There are 5 entryLine SupportingDocuments after calling SaveSupportingDocumentsForCL for entryLine with CL_LineNumber " + entryLine.CL_LineNumber, 5, clSupDocs.Length);
					AssertContainsExactElementsInAnyOrder("The 5 entryLine SupportingDocuments after calling SaveSupportingDocumentsForCL have the correct CSI_Codes for entryLine with CL_LineNumber " + entryLine.CL_LineNumber, new ZString[] { "X001", "X002", "X003", "BBB", "CCC" }, clSupDocs.Select(x => x.CSI_Code).ToArray());
					AssertEquals("The 5 entryLine SupportingDocuments after calling SaveSupportingDocumentsForCL have the correct CSI_Statuc (ACC) for entryLine with CL_LineNumber " + entryLine.CL_LineNumber, true, clSupDocs.All(x => x.CSI_Status == "ACC"));
				}
			});
		}

		public void TestProcessRejectedMessage_EntryStatusEmpty_TemporaryStorageRegisterNotEnabled()
		{
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var (regLineTransaction1, regLineTransaction2, regLineTransaction3) = SetUpTransactionsForRejectedTestForTemporaryStorageGoodsConsumption(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);

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
				var (regLineTransaction1, regLineTransaction2, regLineTransaction3) = SetUpTransactionsForRejectedTestForTemporaryStorageGoodsConsumption(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);

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
				var (regLineTransaction1, regLineTransaction2, regLineTransaction3) = SetUpTransactionsForRejectedTestForTemporaryStorageGoodsConsumption(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);

				AddMessageProcessAndAssertResult_RejectedMessage(false);

				CombineAssertions(() =>
				{
					AssertEquals("regLineTransaction1's SRT_TransactionType was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
					AssertEquals("regLineTransaction2's SRT_TransactionType was not changed (not PND)", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction2.SRT_TransactionStatus);
					AssertEquals("regLineTransaction3's SRT_TransactionType was not changed (wrong internalRefType)", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction3.SRT_TransactionStatus);
				});
			}
		}

		public void TestLoggerWriteOffTransactionError()
		{
			AssertLoggerWriteOffTransactionError();
		}

		public void TestProcessAcceptedMessageWithNoClearanceTriggers031DocumentCapture()
		{
			AssertProcessAcceptedMessageWithNoClearanceTriggers031DocumentCapture(
				messageBody: AcceptanceTestFileGreenCircuitWithoutClearance,
				expectedCircuitCan: "",
				expectedExportMRN: "");
		}

		void AssertAcceptedMessage(TestEdiMessage message, CusEntryHeader entryHeader, string expectedMessageInterpretation = "", string entryStatusCode = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, string circuitCan = "", bool parallel = false)
		{
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageNum: MessageNum, messageSubType: "ACC", entryStatusCode: entryStatusCode, circuit: ExpectedCircuit, acceptanceDate: AcceptanceDate, entryReleaseDate: EntryReleaseDate, limitPaymentDate: ExpectedLimitPaymentDate, csvClearance: CsvClearance, csvImportCertificate: CsvImportCertificate, circuitCan: circuitCan, atcLimitPaymentDate: AtcLimitPaymentDate, paymentProofNumber: ExpectedPaymentProofNumber, atcPaymentProofNumber: ExpectedAtcPaymentProofNumber, parallel: parallel, movementReferenceNumber: MRNCode, exportMRN: ExpectedExportMRN);
		}

		protected override string ExpectedEntryStatusForInstructionC => EntryStatusCodes.Cleared;
		protected override List<(ZString, ZString)> ExpectedDocumentsAEATInstructionC => new List<(ZString, ZString)> {
																													(MRNCode + "_I_AEAT_CLR_C.pdf", "TEST444444444444"),
																													(MRNCode + "_I_AEAT_CER_C.pdf", "CRTF444444444444"),
																													(MRNCode + "_I_AEAT_M031.pdf", MRNCode),
																													(MRNCode + "_I_AEAT_J031.pdf", MRNCode) };
		protected override int ExpcetedNumberDocMessagesAmountAEATInstructionC => 4;
		protected override List<(ZString, ZString)> ExpectedDocumentsATCInstructionC => new List<(ZString, ZString)> {
																													(MRNCode + "_I_AEAT_CLR_C.pdf", "TEST444444444444"),
																													(MRNCode + "_I_AEAT_CER_C.pdf", "CRTF444444444444"),
																													(MRNCode + "_I_AEAT_M031.pdf", MRNCode),
																													(MRNCode + "_I_AEAT_J031.pdf", MRNCode),
																													(MRNCode + "_I_AEAT_M032.pdf", MRNCode),
																													(MRNCode + "_I_AEAT_J032.pdf", MRNCode) };
		protected override int ExpcetedNumberDocMessagesAmountATCInstructionC => 6;
		string GetAcceptanceTestFileYellowCircuit() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CompleteImportTestFilePath, "AcceptedYellowCircuitMessage.txt");
		string GetAcceptanceTestFileYellowCircuitPDAStatus() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CompleteImportTestFilePath, "AcceptedYellowCircuitMessagePDAStatus.txt");
		protected override string AcceptanceTestFilePDSResponse => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CompleteImportTestFilePath, "AcceptedPDSMessage.txt");
		protected override string AcceptanceTestFileWithClearance => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CompleteImportTestFilePath, "AcceptedGreenCircuitMessage.txt");
		protected override string RejectedTestFile => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CompleteImportTestFilePath, "RejectedMessage.txt");
		protected override string AcceptanceTestFileWithoutGuarantees => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CompleteImportTestFilePath, "AcceptedGreenCircuitMessageNoGuarantees.txt");
		protected override string AcceptanceTestFileWithGuaranteesAEAT => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CompleteImportTestFilePath, "AcceptedGreenCircuitMessageGuaranteesAEAT.txt");
		protected override string AcceptanceTestFileWithGuaranteesATC => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CompleteImportTestFilePath, "AcceptedGreenCircuitMessageGuaranteesATC.txt");
		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CompleteImportTestFilePath, "AcceptedMessageWithLongSegmentId.txt");
		string AcceptanceTestFileGreenCircuitWithoutClearance => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CompleteImportTestFilePath, "AcceptedGreenCircuitMessageNoClearance.txt");

		protected override CompleteImportResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new CompleteImportResponseMessageProcessor(logger);

		protected override ZString GetExpectedProcessorFriendlyName() => "Import Complete Pre-Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration };

		protected override ZString RejectedMessageNum => "20200703102751000000";
		protected override ZString RejectedMessageInterpretation => "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td><strong>Error</strong></td><td><strong>Location/Description</strong></td></tr>" +
					"<tr><td>2814</td><td>1.2<br>Partida(1).CONCEPTOS TRIBUTARIOS (CAS 47)(2).BASE IMPONIBLE TRIBUTO(CAS 47). Cas.47.  Base Imponible del I.V.A. incorrecta. Número de orden del tributo: 2  Base correcta 91.29.C47TributoBaseImponible.KK</td></tr></table>";
		protected override bool IsImportQuery => false;
		protected override string ExpectedCircuit => CircuitCodeList.Codes.GREEN;
		protected override ZDateTime ExpectedLimitPaymentDate => ZDateTime.Empty;
		protected override ZDateTime ExpectedATCLimitPaymentDate => AtcLimitPaymentDate;
		protected override string ExpectedAtcPaymentProofNumber => AtcPaymentProofNumber;
		protected override string ExpectedPaymentProofNumber => PaymentProofNumber;
		protected override string ExpectedExportMRN => ExportMRN;
		protected override string ExpectedCircuitCan => CircuitCodeList.Codes.ORANGE;
		protected override string ExpectedMovementReferenceNumberReject => string.Empty;
		protected override ZDateTime ExpectedMovementReferenceNumberIssueDateReject => ZDateTime.Empty;
		protected override string ExpectedentryStatusCode => EntryStatusCodes.Cleared;
	}
}
