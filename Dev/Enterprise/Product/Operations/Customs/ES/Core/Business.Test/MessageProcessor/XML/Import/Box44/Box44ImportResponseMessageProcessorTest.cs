using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.ImportDocCas44PendV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Testing;
using NUnit.Framework;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class Box44ImportResponseMessageProcessorTest : ImportGenericResponseMessageProcessorTest<Box44ImportResponseMessageProcessor, ImportDocCas44PendV1Sal>
	{
		public void TestProcessBox44AcceptedWithClearanceEntryInstructionC()
		{
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.C;

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearance, InterchangeID);

			ProcessMessageForTest(message);
			AssertAcceptedEntryInstruction(message, entryHeader, parallel: true);
		}

		public void TestProcessBox44AcceptedWithClearanceEntryInstructionZ()
		{
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.Z;

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearance, InterchangeID);

			ProcessMessageForTest(message);
			AssertAcceptedEntryInstruction(message, entryHeader, parallel: true);
		}

		[TestDate(2020, 10, 05, 09, 36, 0)]
		public void TestProcessBox44AcceptedWithClearanceEntryInstructionB()
		{
			entryHeader.TotalAmount = 1.04;
			SetDocumentDescription();
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearance, InterchangeID);

			ProcessMessageForTest(message);

			var expectedMessageInterpretation =
				"<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>04-11-2020</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#C6C600\">YELLOW</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>TEST444444444444</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>04-11-2020, 00:00:00</td></tr>" +
				"<tr><td>Import cert. (CSV):</td><td>&nbsp;&nbsp;</td><td>CRTF444444444444</td></tr></table>" +
				"<br><H2>Taxes and fees data</H2>" +
				"<table border=\"0\"><tr><td>AEAT Dispatch Status:</td><td>&nbsp;&nbsp;</td><td>L - Despachado con Levante</td></tr>" +
				"<tr><td>ATC Dispatch Status:</td><td>&nbsp;&nbsp;</td><td>D - Despachado sin Levante</td></tr>" +
				"<tr><td>AEAT Accounting Status:</td><td>&nbsp;&nbsp;</td><td>PL - Pendiente de Contraccion</td></tr>" +
				"<tr><td>ATC Accounting Status:</td><td>&nbsp;&nbsp;</td><td>PL - Pendiente de Contraccion</td></tr>" +
				"<tr><td>AEAT Unfinished Pendencies:</td><td>&nbsp;&nbsp;</td><td>N</td></tr>" +
				"<tr><td>ATC Unfinished Pendencies:</td><td>&nbsp;&nbsp;</td><td>N</td></tr></table><br><br>" +
				"<table border=\"0\"><tr><td>Total:</td><td>&nbsp;&nbsp;</td><td>1.04</td></tr>" +
				"<tr><td>Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>0.69</td></tr>" +
				"<tr><td>ATC Total:</td><td>&nbsp;&nbsp;</td><td>0.70</td></tr>" +
				"<tr><td>ATC Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>0.70</td></tr>" +
				"<tr><td>Total Deferred VAT:</td><td>&nbsp;&nbsp;</td><td>35</td></tr>" +
				"<tr><td>Clearance Guarantee VAT Exemption:</td><td>&nbsp;&nbsp;</td><td>0.15</td></tr>" +
				"<tr><td>Real Clearance Guarantee:</td><td>&nbsp;&nbsp;</td><td>0.89</td></tr>" +
				"<tr><td>Pendency Guarantee VAT Exemption:</td><td>&nbsp;&nbsp;</td><td>0.20</td></tr>" +
				"<tr><td>Real Pendency Guarantee:</td><td>&nbsp;&nbsp;</td><td>-0.20</td></tr></table>" +
				"<br><H2>Payment information</H2><br>" +
				"<table border=\"0\"><tr><td>Payment date limit:</td><td>&nbsp;&nbsp;</td><td>07-06-2021</td></tr>" +
				"<tr><td>ATC payment date limit:</td><td>&nbsp;&nbsp;</td><td>07-06-2021</td></tr></table><br>" +
				"<br><H2>Guarantees</H2><br>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
				"<thead><tr class=\"tableheadings\"><th>Customs</th><th>GRN</th><th>Real Debt</th><th>Potential Debt</th><th>Undetermined Real Debt</th></tr></thead>" +
				"<tr><td>AEAT</td><td>16ESAGL9990000096</td><td>1.04</td><td>2</td><td>1</td></tr>" +
				"<tr><td>ATC</td><td>16ESAGL9990000096</td><td>1.04</td><td>2</td><td>1</td></tr>" +
				"</table><br><br><H2>Taxes and fees response (Spanish Customs)</H2><br>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Item</th><th>Type</th><th>MAX/MIN Rate</th><th>Base Amount</th><th>Tax Rate</th><th>Total Amount</th><th>Total Guaranteed Amount</th></tr></thead>" +
				"<tr><td>1</td><td>A00</td><td>&nbsp;</td><td>100.650</td><td>2.700000 %</td><td>2.72</td><td>2.72</td></tr>" +
				"<tr><td>1</td><td>B00</td><td>MA</td><td>104.950</td><td>21.000000 %</td><td>22.04</td><td>22.04</td></tr>" +
				"<tr><td>2</td><td>A00</td><td>&nbsp;</td><td>1219.350</td><td>17.600000 %</td><td>214.61</td><td>214.61</td></tr>" +
				"<tr><td>2</td><td>B00</td><td>&nbsp;</td><td>1481.380</td><td>10.000000 €/KN</td><td>148.14</td><td>148.14</td></tr>" +
				"<tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>Total: </td><td>387.51</td><td>387.51</td></tr>" +
				"</table><br><br><H2>Required Certificates</H2><br><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead>" +
				"<tr class=\"tableheadings\"><th>Item</th><th>Measure</th><th>Agency</th><th>Documents</th></tr></thead>" +
				"<tr><td>2</td><td>SNM</td><td>SIF05 -  Sanidad Exterior - M&#186; Sanidad</td><td>" +
				"N853 - DOC.SANIT.COMUN ENTRADA PRODUCTOS (DSCE-P)[anex II-2, secc B, R/UE 2019/1715], C640</td></tr>" +
				"<tr><td>2</td><td>VIM</td><td>SIF06 -  Sanidad Inetrior - M&#186; Sanidad</td><td>" +
				"C657, C678</td></tr>" +
				"</table>";

			AssertAcceptedEntryInstruction(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation);

			CombineAssertions("Test EntryLines fees", () =>
			{
				AssertEquals("EntryLine1.Fees count", 2, entryLine1.Fees.Count);

				AssertEquals("EntryLine1.Fee[0].CF_ChargeType", "A00", entryLine1.Fees[0].CF_ChargeType);
				AssertEquals("EntryLine1.Fee[0].CF_BaseValue", 100.650M, entryLine1.Fees[0].CF_BaseValue);
				AssertEquals("EntryLine1.Fee[0].CF_Rate", 2.700000M, entryLine1.Fees[0].CF_Rate);
				AssertEquals("EntryLine1.Fee[0].MaxMin", ZString.Empty, entryLine1.Fees[0].MaxMin);
				AssertEquals("EntryLine1.Fee[0].G4_RateDuty", "%", entryLine1.Fees[0].G4_RateDuty);
				AssertEquals("EntryLine1.Fee[0].CF_ChargeAmount", 2.72M, entryLine1.Fees[0].CF_ChargeAmount);

				AssertEquals("EntryLine1.Fee[1].CF_ChargeType", "B00", entryLine1.Fees[1].CF_ChargeType);
				AssertEquals("EntryLine1.Fee[1].CF_BaseValue", 104.950M, entryLine1.Fees[1].CF_BaseValue);
				AssertEquals("EntryLine1.Fee[1].CF_Rate", 21.000000M, entryLine1.Fees[1].CF_Rate);
				AssertEquals("EntryLine1.Fee[1].MaxMin", "MA", entryLine1.Fees[1].MaxMin);
				AssertEquals("EntryLine1.Fee[1].G4_RateDuty", "%", entryLine1.Fees[1].G4_RateDuty);
				AssertEquals("EntryLine1.Fee[1].CF_ChargeAmount", 22.04M, entryLine1.Fees[1].CF_ChargeAmount);

				AssertEquals("EntryLine2.Fees count", 2, entryLine2.Fees.Count);

				AssertEquals("EntryLine2.Fee[0].CF_ChargeType", "A00", entryLine2.Fees[0].CF_ChargeType);
				AssertEquals("EntryLine2.Fee[0].CF_BaseValue", 1219.350M, entryLine2.Fees[0].CF_BaseValue);
				AssertEquals("EntryLine2.Fee[0].CF_Rate", 17.600000M, entryLine2.Fees[0].CF_Rate);
				AssertEquals("EntryLine2.Fee[0].MaxMin", ZString.Empty, entryLine2.Fees[0].MaxMin);
				AssertEquals("EntryLine2.Fee[0].G4_RateDuty", "%", entryLine2.Fees[0].G4_RateDuty);
				AssertEquals("EntryLine2.Fee[0].CF_ChargeAmount", 214.61M, entryLine2.Fees[0].CF_ChargeAmount);

				AssertEquals("EntryLine2.Fee[1].CF_ChargeType", "B00", entryLine2.Fees[1].CF_ChargeType);
				AssertEquals("EntryLine2.Fee[1].CF_BaseValue", 1481.380M, entryLine2.Fees[1].CF_BaseValue);
				AssertEquals("EntryLine2.Fee[1].CF_Rate", 10.000000M, entryLine2.Fees[1].CF_Rate);
				AssertEquals("EntryLine2.Fee[1].MaxMin", ZString.Empty, entryLine2.Fees[1].MaxMin);
				AssertEquals("EntryLine2.Fee[1].G4_RateDuty", "KGM", entryLine2.Fees[1].G4_RateDuty);
				AssertEquals("EntryLine2.Fee[1].CF_ChargeAmount", 148.14M, entryLine2.Fees[1].CF_ChargeAmount);

				AssertEquals("EntryLine3.Fees count (not deleted)", 2, entryLine3.Fees.Count);

				AssertEquals("EntryLine3.FeeVAT.CF_MethodOfPayment is DEF", UniversalReferenceConstants.FeeMethodOfPayment.Deferred, feeVAT.CF_MethodOfPayment);
				AssertEquals("EntryLine3.FeeNotVAT.CF_MethodOfPayment empty", ZString.Empty, feeNotVAT.CF_MethodOfPayment);
			});
		}

		public void TestProcessBox44AcceptedWithClearanceEntryInstructionBeWithExistingMRN()
		{
			entryHeader.MovementReferenceNumberSetter(MRNCode, MovementReferenceNumberIssueDate);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearance, InterchangeID);

			ProcessMessageForTest(message);
			AssertAcceptedEntryInstruction(message, entryHeader, parallel: true);
		}

		public void TestProcessBox44AcceptedWithClearanceEntryInstructionNotBorCorZ()
		{
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.A;
			entryHeader.TotalAmount = 1.04;
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearance, InterchangeID);

			ProcessMessageForTest(message);
			AssertAcceptedEntryInstruction(message, entryHeader, entryStatusCode: EntryStatusCodes.Cleared);
		}

		public void TestProcessBox44AcceptedWithoutClearance()
		{
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.A;
			entryHeader.TotalAmount = 0.04;
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithoutClearance, InterchangeID);

			ProcessMessageForTest(message);
			AssertAcceptedEntryInstructionWithoutClearance(message, entryHeader, entryStatusCode: EntryStatusCodes.ClearedWithPendingDocuments);
		}

		public void TestProcessBox44AcceptedDifferedMessage()
		{
			entryHeader.TotalAmount = ZDecimal.Zero;
			SetDocumentDescription();
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearance, InterchangeID);

			ProcessMessageForTest(message);

			var expectedMessageInterpretation =
				"<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>04-11-2020</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#C6C600\">YELLOW</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>TEST444444444444</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>04-11-2020, 00:00:00</td></tr>" +
				"<tr><td>Import cert. (CSV):</td><td>&nbsp;&nbsp;</td><td>CRTF444444444444</td></tr></table>" +
				"<br><H2>Taxes and fees data</H2>" +
				"<H3>Warning: Taxes and fees data received differ from sent data</H3>" +
				"<table border=\"0\"><tr><td>AEAT Dispatch Status:</td><td>&nbsp;&nbsp;</td><td>L - Despachado con Levante</td></tr>" +
				"<tr><td>ATC Dispatch Status:</td><td>&nbsp;&nbsp;</td><td>D - Despachado sin Levante</td></tr>" +
				"<tr><td>AEAT Accounting Status:</td><td>&nbsp;&nbsp;</td><td>PL - Pendiente de Contraccion</td></tr>" +
				"<tr><td>ATC Accounting Status:</td><td>&nbsp;&nbsp;</td><td>PL - Pendiente de Contraccion</td></tr>" +
				"<tr><td>AEAT Unfinished Pendencies:</td><td>&nbsp;&nbsp;</td><td>N</td></tr>" +
				"<tr><td>ATC Unfinished Pendencies:</td><td>&nbsp;&nbsp;</td><td>N</td></tr></table><br><br>" +
				"<table border=\"0\"><tr><td>Total:</td><td>&nbsp;&nbsp;</td><td>1.04</td></tr>" +
				"<tr><td>Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>0.69</td></tr>" +
				"<tr><td>ATC Total:</td><td>&nbsp;&nbsp;</td><td>0.70</td></tr>" +
				"<tr><td>ATC Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>0.70</td></tr>" +
				"<tr><td>Total Deferred VAT:</td><td>&nbsp;&nbsp;</td><td>35</td></tr>" +
				"<tr><td>Clearance Guarantee VAT Exemption:</td><td>&nbsp;&nbsp;</td><td>0.15</td></tr>" +
				"<tr><td>Real Clearance Guarantee:</td><td>&nbsp;&nbsp;</td><td>0.89</td></tr>" +
				"<tr><td>Pendency Guarantee VAT Exemption:</td><td>&nbsp;&nbsp;</td><td>0.20</td></tr>" +
				"<tr><td>Real Pendency Guarantee:</td><td>&nbsp;&nbsp;</td><td>-0.20</td></tr></table>" +
				"<br><H2>Payment information</H2><br>" +
				"<table border=\"0\"><tr><td>Payment date limit:</td><td>&nbsp;&nbsp;</td><td>07-06-2021</td></tr>" +
				"<tr><td>ATC payment date limit:</td><td>&nbsp;&nbsp;</td><td>07-06-2021</td></tr></table><br>" +
				"<br><H2>Guarantees</H2><br>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
				"<thead><tr class=\"tableheadings\"><th>Customs</th><th>GRN</th><th>Real Debt</th><th>Potential Debt</th><th>Undetermined Real Debt</th></tr></thead>" +
				"<tr><td>AEAT</td><td>16ESAGL9990000096</td><td>1.04</td><td>2</td><td>1</td></tr>" +
				"<tr><td>ATC</td><td>16ESAGL9990000096</td><td>1.04</td><td>2</td><td>1</td></tr>" +
				"</table><br><br><H2>Taxes and fees response (Spanish Customs)</H2><br>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Item</th><th>Type</th><th>MAX/MIN Rate</th><th>Base Amount</th><th>Tax Rate</th><th>Total Amount</th><th>Total Guaranteed Amount</th></tr></thead>" +
				"<tr><td>1</td><td>A00</td><td>&nbsp;</td><td>100.650</td><td>2.700000 %</td><td>2.72</td><td>2.72</td></tr>" +
				"<tr><td>1</td><td>B00</td><td>MA</td><td>104.950</td><td>21.000000 %</td><td>22.04</td><td>22.04</td></tr>" +
				"<tr><td>2</td><td>A00</td><td>&nbsp;</td><td>1219.350</td><td>17.600000 %</td><td>214.61</td><td>214.61</td></tr>" +
				"<tr><td>2</td><td>B00</td><td>&nbsp;</td><td>1481.380</td><td>10.000000 €/KN</td><td>148.14</td><td>148.14</td></tr>" +
				"<tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>Total: </td><td>387.51</td><td>387.51</td></tr>" +
				"</table><br><br><H2>Required Certificates</H2><br><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead>" +
				"<tr class=\"tableheadings\"><th>Item</th><th>Measure</th><th>Agency</th><th>Documents</th></tr></thead>" +
				"<tr><td>2</td><td>SNM</td><td>SIF05 -  Sanidad Exterior - M&#186; Sanidad</td><td>" +
				"N853 - DOC.SANIT.COMUN ENTRADA PRODUCTOS (DSCE-P)[anex II-2, secc B, R/UE 2019/1715], C640</td></tr>" +
				"<tr><td>2</td><td>VIM</td><td>SIF06 -  Sanidad Inetrior - M&#186; Sanidad</td><td>" +
				"C657, C678</td></tr>" +
				"</table>";

			AssertAcceptedEntryInstruction(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, parallel: true);
		}

		public void TestProcessAcceptedMessageCreatesEntryLineSupportingDocuments()
		{
			entryHeader.TotalAmount = 1.04;

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
				entryLine.AddEntryLineDocument<SupportingDocument>("X004", "ES3600000004");
				entryLine.AddEntryLineDocument<SupportingDocument>("X005", "ES3600000005");
			}

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearance, InterchangeID);

			ProcessMessageForTest(message);
			AssertAcceptedEntryInstruction(message, entryHeader);

			CombineAssertions(() =>
			{
				foreach (CusEntryLine entryLine in entryHeader.MergedLines)
				{
					var clSupDocs = GetCLSupportingDocuments(entryLine);
					AssertEquals("There are 5 CL SupportingDocuments after calling SaveSupportingDocumentsForCL for entryLine with CL_LineNumber " + entryLine.CL_LineNumber, 5, clSupDocs.Length);
					AssertContainsExactElementsInAnyOrder("The 5 CL SupportingDocuments after calling SaveSupportingDocumentsForCL have the correct CSI_Codes for entryLine with CL_LineNumber " + entryLine.CL_LineNumber, new ZString[] { "X001", "X002", "X003", "X004", "X005" }, clSupDocs.Select(x => x.CSI_Code).ToArray());
				}
			});
		}

		public void TestProcessAcceptedMessageWithNoClearanceTriggers031DocumentCapture()
		{
			AssertProcessAcceptedMessageWithNoClearanceTriggers031DocumentCapture(
				messageBody: AcceptanceTestFileGreenCircuitWithoutClearance,
				expectedEntryStatus: EntryStatusCodes.ClearedWithPendingComplementaryDeclarations,
				expectedLimitPaymentDate: LimitPaymentDate,
				expectedAtcLimitPaymentDate: LimitPaymentDate);
		}

		void SetDocumentDescription()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "Description");

			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, parent: grouping);
			helper.CreateNewOrGetExistingCusCodeList("ES", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "N853", "DOC.SANIT.COMUN ENTRADA PRODUCTOS (DSCE-P)[anex II-2, secc B, R/UE 2019/1715]", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			entryHeader.CH_EntryStatus = EntryStatusCodes.CustomsDeclarationAccepted;
		}

		protected override string ExpectedEntryStatusForInstructionC => EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;

		protected override List<(ZString, ZString)> ExpectedDocumentsAEATInstructionC =>
		[
			(MRNCode + "_I_AEAT_CLR.pdf", "TEST444444444444"),
			(MRNCode + "_I_AEAT_CER.pdf", "CRTF444444444444"),
			(MRNCode + "_I_AEAT_M031.pdf", MRNCode),
			(MRNCode + "_I_AEAT_J031.pdf", MRNCode)
		];

		protected override int ExpcetedNumberDocMessagesAmountAEATInstructionC => 4;

		protected override List<(ZString, ZString)> ExpectedDocumentsATCInstructionC =>
		[
			(MRNCode + "_I_AEAT_CLR.pdf", "TEST444444444444"),
			(MRNCode + "_I_AEAT_CER.pdf", "CRTF444444444444"),
			(MRNCode + "_I_AEAT_M031.pdf", MRNCode),
			(MRNCode + "_I_AEAT_J031.pdf", MRNCode),
			(MRNCode + "_I_AEAT_M032.pdf", MRNCode),
			(MRNCode + "_I_AEAT_J032.pdf", MRNCode)
		];

		void AssertAcceptedEntryInstruction(TestEdiMessage message, CusEntryHeader entryHeader, string expectedMessageInterpretation = "", string entryStatusCode = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, bool parallel = false)
		{
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageNum: MessageNum, messageSubType: "ACC", entryStatusCode: entryStatusCode, circuit: ExpectedCircuit, acceptanceDate: AcceptanceDate, entryReleaseDate: EntryReleaseDate, limitPaymentDate: ExpectedLimitPaymentDate, csvClearance: CsvClearance, csvImportCertificate: CsvImportCertificate, circuitCan: ExpectedCircuitCan, atcLimitPaymentDate: AtcLimitPaymentDate, parallel: parallel, movementReferenceNumber: MRNCode, atcPaymentProofNumber: ExpectedAtcPaymentProofNumber, paymentProofNumber: ExpectedPaymentProofNumber, exportMRN: ExpectedExportMRN);
		}
		void AssertAcceptedEntryInstructionWithoutClearance(TestEdiMessage message, CusEntryHeader entryHeader, string expectedMessageInterpretation = "", string entryStatusCode = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, bool parallel = false)
		{
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageNum: MessageNum, messageSubType: "ACC", entryStatusCode: entryStatusCode, circuit: ExpectedCircuit, acceptanceDate: AcceptanceDate, parallel: parallel, movementReferenceNumber: MRNCode, atcPaymentProofNumber: ExpectedAtcPaymentProofNumber, paymentProofNumber: ExpectedPaymentProofNumber, exportMRN: ExpectedExportMRN);
		}

		protected override int ExpcetedNumberDocMessagesAmountATCInstructionC => 6;
		string AcceptanceTestFileWithoutClearance => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.Box44TestFilePath, "AcceptanceTestFileWithoutClearance.txt");
		string AcceptanceTestFileGreenCircuitWithoutClearance => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.Box44TestFilePath, "AcceptedGreenCircuitMessageNoClearance.txt");
		protected override string AcceptanceTestFileWithClearance => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.Box44TestFilePath, "AcceptanceTestFileWithClearance.txt");
		protected override string RejectedTestFile => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.Box44TestFilePath, "RejectedMessage.txt");
		protected override string AcceptanceTestFileWithoutGuarantees => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.Box44TestFilePath, "AcceptanceTestFileWithClearanceNoGuarantees.txt");
		protected override string AcceptanceTestFileWithGuaranteesAEAT => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.Box44TestFilePath, "AcceptanceTestFileWithClearanceGuaranteesAEAT.txt");
		protected override string AcceptanceTestFileWithGuaranteesATC => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.Box44TestFilePath, "AcceptanceTestFileWithClearanceGuaranteesATC.txt");
		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.Box44TestFilePath, "AcceptedMessageWithLongSegmentId.txt");

		protected override Box44ImportResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new(logger);
		protected override ZString GetExpectedProcessorFriendlyName() => "Box44 Declaration Message Processor";
		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => [DeclarationMessageTypeList.Codes.Box44Documents];
		protected override bool IsImportQuery => false;
		protected override string ExpectedCircuit => CircuitCodeList.Codes.YELLOW;
		protected override string ExpectedCircuitCan => CircuitCodeList.Codes.GREEN;
		protected override string ExpectedExportMRN => string.Empty;
		protected override ZDateTime ExpectedLimitPaymentDate => LimitPaymentDate;
		protected override ZDateTime ExpectedATCLimitPaymentDate => AtcLimitPaymentDate;
		protected override string ExpectedAtcPaymentProofNumber => string.Empty;
		protected override string ExpectedPaymentProofNumber => string.Empty;
		protected override string ExpectedMovementReferenceNumberReject => MRNCode;
		protected override ZDateTime ExpectedMovementReferenceNumberIssueDateReject => ZDateTime.Empty;
		protected override ZString RejectedMessageNum => "20210324100716885000";
		protected override ZString RejectedEntryStatus => EntryStatusCodes.CustomsDeclarationAccepted;
		protected override string ExpectedentryStatusCode => EntryStatusCodes.Cleared;
		protected override ZString RejectedMessageInterpretation => "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td><strong>Error</strong></td><td><strong>Location/Description</strong></td></tr>" +
					"<tr><td>0301</td><td>0.0<br>9002 C678 NO PROCEDE El certificado no existe..</td></tr></table>";
	}
}
