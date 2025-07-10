using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.ModificacionPdcCas40V1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Testing;
using NUnit.Framework;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class Box40AmendmentImportResponseMessageProcessorTest : ImportGenericResponseMessageProcessorTest<Box40AmendmentImportResponseMessageProcessor, ModificacionPdcCas40V1Sal>
	{
		public void TestProcessPDCAcceptedGreenCircuitMessageEntryInstructionC()
		{
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.C;

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearance, InterchangeID);

			ProcessMessageForTest(message);
			AssertAcceptedEntryInstruction(message, entryHeader, entryStatusCode: EntryStatusCodes.Cleared, parallel: true);
		}

		public void TestProcessPDCAcceptedGreenCircuitMessageEntryInstructionZ()
		{
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.Z;

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearance, InterchangeID);

			ProcessMessageForTest(message);
			AssertAcceptedEntryInstruction(message, entryHeader, parallel: true);
		}

		[TestDate(2020, 10, 05, 09, 36, 0)]
		public void TestProcessPDCAcceptedGreenCircuitMessageEntryInstructionB()
		{
			entryHeader.TotalAmount = 380;

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearance, InterchangeID);

			ProcessMessageForTest(message);

			var expectedMessageInterpretation =
				"<H3>Accepted Declaration</H3>" +
				"<table border=\"0\"><tr><td>Description:</td><td>&nbsp;&nbsp;</td><td>(3)Admisión de DUA</td></tr></table>" +
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
			AssertProcessPDCAcceptedGreenCircuitEntryData(message, entryHeader, expectedMessageInterpretation, false);

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

		public void TestProcessPDCAcceptedGreenCircuitMessageEntryInstructionBWithExistingMRN()
		{
			entryHeader.MovementReferenceNumberSetter(MRNCode, MovementReferenceNumberIssueDate);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearance, InterchangeID);

			ProcessMessageForTest(message);
			AssertAcceptedEntryInstruction(message, entryHeader, parallel: true);
		}

		public void TestProcessPDCAcceptedGreenCircuitEntryInstructionBDifferedMessage()
		{
			entryHeader.TotalAmount = ZDecimal.Zero;

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearance, InterchangeID);

			ProcessMessageForTest(message);
			AssertProcessPDCAcceptedGreenCircuitEntryData(message, entryHeader, ZString.Empty, true);
			AssertContains("EM_MessageInterpretation", "<H3>Warning: Taxes and fees data received differ from sent data</H3>", message.EM_MessageInterpretation);
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
			AssertAcceptedEntryInstruction(message, entryHeader, entryStatusCode: EntryStatusCodes.ClearedWithPendingComplementaryDeclarations);

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

		void AssertProcessPDCAcceptedGreenCircuitEntryData(TestEdiMessage message, CusEntryHeader entryHeader, ZString expectedMessageInterpretation, bool parallel) => AssertAcceptedEntryInstruction(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, parallel: parallel);

		public void TestProcessPDCAcceptedGreenCircuitEntryInstructionNotBorZ()
		{
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.A;

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearance, InterchangeID);
			ProcessMessageForTest(message);
			AssertAcceptedEntryInstruction(message, entryHeader, entryStatusCode: EntryStatusCodes.Cleared, parallel: true);
		}

		public void TestProcessPDCAcceptedYellowCircuitMessage()
		{
			entryHeader.TotalAmount = 380;
			AddFeesToEntryLine();
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileYellowCircuit(), InterchangeID);

			ProcessMessageForTest(message);
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: ZString.Empty, messageSubType: "ACC", messageNum: MessageNum, movementReferenceNumber: MRNCode, entryStatusCode: EntryStatusCodes.ClearedWithPendingDocuments, circuit: CircuitCodeList.Codes.YELLOW, acceptanceDate: AcceptanceDate, circuitCan: CircuitCodeList.Codes.YELLOW, parallel: true);

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
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: ZString.Empty, messageSubType: "ACC", messageNum: MessageNum, movementReferenceNumber: MRNCode, entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, circuit: ZString.Empty, acceptanceDate: AcceptanceDate, circuitCan: CircuitCodeList.Codes.YELLOW, parallel: true);

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

		public void TestLoggerWriteOffTransactionError()
		{
			AssertLoggerWriteOffTransactionError();
		}

		void AssertAcceptedEntryInstruction(TestEdiMessage message, CusEntryHeader entryHeader, string expectedMessageInterpretation = "", string entryStatusCode = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, bool parallel = false)
		{
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageNum: MessageNum, messageSubType: "ACC", entryStatusCode: entryStatusCode, circuit: ExpectedCircuit, acceptanceDate: AcceptanceDate, entryReleaseDate: EntryReleaseDate, limitPaymentDate: ExpectedLimitPaymentDate, csvClearance: CsvClearance, csvImportCertificate: CsvImportCertificate, circuitCan: ExpectedCircuitCan, atcLimitPaymentDate: AtcLimitPaymentDate, parallel: parallel, movementReferenceNumber: MRNCode, atcPaymentProofNumber: ExpectedAtcPaymentProofNumber, paymentProofNumber: ExpectedPaymentProofNumber, exportMRN: ExpectedExportMRN);
		}

		protected override string ExpectedEntryStatusForInstructionC => EntryStatusCodes.Cleared;
		protected override string ExpectedCircuit => CircuitCodeList.Codes.GREEN;
		protected override string ExpectedCircuitCan => CircuitCodeList.Codes.ORANGE;
		protected override ZDateTime ExpectedLimitPaymentDate => ZDateTime.Empty;
		protected override ZDateTime ExpectedATCLimitPaymentDate => AtcLimitPaymentDate;
		protected override string ExpectedAtcPaymentProofNumber => AtcPaymentProofNumber;
		protected override string ExpectedPaymentProofNumber => PaymentProofNumber;
		protected override string ExpectedExportMRN => ExportMRN;
		protected override string ExpectedMovementReferenceNumberReject => string.Empty;
		protected override ZDateTime ExpectedMovementReferenceNumberIssueDateReject => ZDateTime.Empty;
		protected override List<(ZString, ZString)> ExpectedDocumentsAEATInstructionC => new List<(ZString, ZString)> {
																													(MRNCode + "_I_AEAT_CLR_C.pdf", CsvClearance),
																													(MRNCode + "_I_AEAT_CER_C.pdf", CsvImportCertificate),
																													(MRNCode + "_I_AEAT_M031.pdf", MRNCode),
																													(MRNCode + "_I_AEAT_J031.pdf", MRNCode) };
		protected override int ExpcetedNumberDocMessagesAmountAEATInstructionC => 4;
		protected override List<(ZString, ZString)> ExpectedDocumentsATCInstructionC => new List<(ZString, ZString)> {
																													(MRNCode + "_I_AEAT_CLR_C.pdf", CsvClearance),
																													(MRNCode + "_I_AEAT_CER_C.pdf", CsvImportCertificate),
																													(MRNCode + "_I_AEAT_M031.pdf", MRNCode),
																													(MRNCode + "_I_AEAT_J031.pdf", MRNCode),
																													(MRNCode + "_I_AEAT_M032.pdf", MRNCode),
																													(MRNCode + "_I_AEAT_J032.pdf", MRNCode) };
		protected override int ExpcetedNumberDocMessagesAmountATCInstructionC => 6;

		string GetAcceptanceTestFileYellowCircuit() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.Box40AmendmentImportTestFilePath, "AcceptedYellowCircuitMessage.txt");
		string GetAcceptanceTestFileYellowCircuitPDAStatus() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.Box40AmendmentImportTestFilePath, "AcceptedYellowCircuitMessagePDAStatus.txt");

		protected override bool IsImportQuery => false;
		protected override string AcceptanceTestFilePDSResponse => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.Box40AmendmentImportTestFilePath, "AcceptedPDSMessage.txt");
		protected override string AcceptanceTestFileWithClearance => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.Box40AmendmentImportTestFilePath, "AcceptedGreenCircuitMessage.txt");
		protected override string RejectedTestFile => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.Box40AmendmentImportTestFilePath, "RejectedMessage.txt");
		protected override string AcceptanceTestFileWithoutGuarantees => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.Box40AmendmentImportTestFilePath, "AcceptedGreenCircuitMessageNoGuarantees.txt");
		protected override string AcceptanceTestFileWithGuaranteesAEAT => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.Box40AmendmentImportTestFilePath, "AcceptedGreenCircuitMessageGuaranteesAEAT.txt");
		protected override string AcceptanceTestFileWithGuaranteesATC => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.Box40AmendmentImportTestFilePath, "AcceptedGreenCircuitMessageGuaranteesATC.txt");
		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.Box40AmendmentImportTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

		protected override Box40AmendmentImportResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new Box40AmendmentImportResponseMessageProcessor(logger);

		protected override ZString GetExpectedProcessorFriendlyName() => "Import Amendment Box 40 Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.ImportAmendmentBox40 };

		protected override ZString RejectedMessageNum => "20200703102751000000";
		protected override string ExpectedentryStatusCode => EntryStatusCodes.Cleared;
		protected override ZString RejectedMessageInterpretation => "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td><strong>Error</strong></td><td><strong>Location/Description</strong></td></tr>" +
					"<tr><td>2814</td><td>1.2<br>Partida(1).CONCEPTOS TRIBUTARIOS (CAS 47)(2).BASE IMPONIBLE TRIBUTO(CAS 47). Cas.47.  Base Imponible del I.V.A. incorrecta. Número de orden del tributo: 2  Base correcta 91.29.C47TributoBaseImponible.KK</td></tr></table>";
	}
}
