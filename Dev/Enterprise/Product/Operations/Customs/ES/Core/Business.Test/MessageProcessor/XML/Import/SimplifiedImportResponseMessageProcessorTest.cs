using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.DeclaSimpliImporV1Sal;
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
	public class SimplifiedImportResponseMessageProcessorTest : ImportGenericResponseMessageProcessorTest<SimplifiedImportResponseMessageProcessor, DeclaSimpliImporV1Sal>
	{
		[TestDate(2020, 10, 05, 09, 36, 0)]
		public void TestProcessDSPAcceptedGreenCircuitMessage()
		{
			entryHeader.TotalAmount = 45.13;

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearance, InterchangeID);

			ProcessMessageForTest(message);

			var expectedMessageInterpretation =
				"<H3>Accepted Declaration</H3>" +
				"<table border=\"0\"><tr><td>Description:</td><td>&nbsp;&nbsp;</td><td>(1)Admisión de DSP Complementada</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>04-11-2020, 00:00:00</td></tr>" +
				"<tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES00999930006184</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr>" +
				"<tr><td>ATC Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>TEST444444444444</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>04-11-2020, 00:00:00</td></tr>" +
				"<tr><td>Import cert. (CSV):</td><td>&nbsp;&nbsp;</td><td>CRTF444444444444</td></tr></table>" +
				"<br><H2>Taxes and fees data</H2><br>" +
				"<table border=\"0\"><tr><td>Total:</td><td>&nbsp;&nbsp;</td><td>45.13</td></tr>" +
				"<tr><td>Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>45.13</td></tr>" +
				"<tr><td>ATC Total:</td><td>&nbsp;&nbsp;</td><td>219.11</td></tr>" +
				"<tr><td>ATC Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>219.11</td></tr>" +
				"<tr><td>Total Deferred VAT:</td><td>&nbsp;&nbsp;</td><td>1.15</td></tr></table><br>" +
				"<H2>Payment information</H2><br>" +
				"<table border=\"0\"><tr><td>Payment Proof Number:</td><td>&nbsp;&nbsp;</td><td>1234</td></tr>" +
				"<tr><td>Payment date limit:</td><td>&nbsp;&nbsp;</td><td>07-06-2021</td></tr>" +
				"<tr><td>ATC Proof of Payment Number:</td><td>&nbsp;&nbsp;</td><td>JUSTPAGOATC</td></tr>" +
				"<tr><td>ATC payment date limit:</td><td>&nbsp;&nbsp;</td><td>06-06-2079</td></tr></table><br>" +
				"<br><H2>Guarantees</H2><br>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
				"<thead><tr class=\"tableheadings\"><th>Customs</th><th>GRN</th><th>Real Debt</th><th>Potential Debt</th><th>Undetermined Real Debt</th></tr></thead>" +
				"<tr><td>AEAT</td><td>16ESAGL9990000096</td><td>45.13</td><td>0</td><td>0</td></tr>" +
				"<tr><td>AEAT</td><td>16ESAGL9990000095</td><td>1.15</td><td>1.15</td><td>1.15</td></tr>" +
				"<tr><td>ATC</td><td>18ESCGL9980000060</td><td>1.31</td><td>1.31</td><td>1.31</td></tr></table>" +
				"<br><br><H2>Taxes and fees response (Spanish Customs)</H2><br>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
				"<thead><tr class=\"tableheadings\"><th>Item</th><th>Type</th><th>MAX/MIN Rate</th><th>Base Amount</th><th>Tax Rate</th><th>Total Amount</th><th>Total Guaranteed Amount</th></tr></thead>" +
				"<tr><td>1</td><td>A00</td><td>MA</td><td>159.710</td><td>6.000000 %</td><td>9.58</td><td>9.58</td></tr>" +
				"<tr><td>2</td><td>A00</td><td>&nbsp;</td><td>1219.350</td><td>17.600000 %</td><td>214.61</td><td>214.61</td></tr>" +
				"<tr><td>2</td><td>B00</td><td>&nbsp;</td><td>1481.380</td><td>10.000000 %</td><td>148.14</td><td>148.14</td></tr>" +
				"<tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>Total: </td><td>372.33</td><td>372.33</td></tr>" +
				"<tr><td>1</td><td>3IG</td><td>&nbsp;</td><td>43.710</td><td>3.000000 €/KN</td><td>1.31</td><td>1.31</td></tr>" +
				"<tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>Total: </td><td>1.31</td><td>1.31</td></tr></table><br>" +
				"<br><H2>Required Certificates</H2><br>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
				"<thead><tr class=\"tableheadings\"><th>Item</th><th>Measure</th><th>Agency</th><th>Documents</th></tr></thead>" +
				"<tr><td>1</td><td>SNM</td><td>SIF05 -  Sanidad Exterior - M&#186; Sanidad</td><td>N853, C657, C678, 1405, 1413, C640</td></tr>" +
				"<tr><td>1</td><td>VIM</td><td>SIF02 -  Sanidad Animal, M Agricultura</td><td>N853, C657, C678, C640</td></tr>" +
				"<tr><td>2</td><td>SNM</td><td>SIF06 -  Sanidad Interior - M&#186; Sanidad</td><td>N853, C657, C678, 1405, 1413, C640</td></tr></table>";
			AssertAcceptedEntryInstruction(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation);

			CombineAssertions("Test EntryLines fees", () =>
			{
				AssertEquals("EntryLine1.Fees count", 2, entryLine1.Fees.Count);

				AssertEquals("EntryLine1.Fee[0].CF_ChargeType", "A00", entryLine1.Fees[0].CF_ChargeType);
				AssertEquals("EntryLine1.Fee[0].CF_BaseValue", 159.710M, entryLine1.Fees[0].CF_BaseValue);
				AssertEquals("EntryLine1.Fee[0].CF_Rate", 6.000000M, entryLine1.Fees[0].CF_Rate);
				AssertEquals("EntryLine1.Fee[0].MaxMin", "MA", entryLine1.Fees[0].MaxMin);
				AssertEquals("EntryLine1.Fee[0].G4_RateDuty", "%", entryLine1.Fees[0].G4_RateDuty);
				AssertEquals("EntryLine1.Fee[0].CF_ChargeAmount", 9.58M, entryLine1.Fees[0].CF_ChargeAmount);
				AssertEquals("EntryLine1.Fee[0].CF_RateOverrideReasonCode", "OVR", entryLine1.Fees[0].CF_RateOverrideReasonCode);

				AssertEquals("EntryLine1.Fee[1].CF_ChargeType", "3IG", entryLine1.Fees[1].CF_ChargeType);
				AssertEquals("EntryLine1.Fee[1].CF_BaseValue", 43.710M, entryLine1.Fees[1].CF_BaseValue);
				AssertEquals("EntryLine1.Fee[1].CF_Rate", 3.000000M, entryLine1.Fees[1].CF_Rate);
				AssertEquals("EntryLine1.Fee[1].MaxMin", ZString.Empty, entryLine1.Fees[1].MaxMin);
				AssertEquals("EntryLine1.Fee[1].G4_RateDuty", "KGM", entryLine1.Fees[1].G4_RateDuty);
				AssertEquals("EntryLine1.Fee[1].CF_ChargeAmount", 1.31M, entryLine1.Fees[1].CF_ChargeAmount);
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
				AssertEquals("EntryLine2.Fee[1].G4_RateDuty", "%", entryLine2.Fees[1].G4_RateDuty);
				AssertEquals("EntryLine2.Fee[1].CF_ChargeAmount", 148.14M, entryLine2.Fees[1].CF_ChargeAmount);
				AssertEquals("EntryLine2.Fee[1].CF_RateOverrideReasonCode", "OVR", entryLine2.Fees[1].CF_RateOverrideReasonCode);

				AssertEquals("EntryLine3.Fees count (not deleted)", 2, entryLine3.Fees.Count);

				AssertEquals("EntryLine3.FeeVAT.CF_MethodOfPayment is DEF", UniversalReferenceConstants.FeeMethodOfPayment.Deferred, feeVAT.CF_MethodOfPayment);
				AssertEquals("EntryLine3.FeeNotVAT.CF_MethodOfPayment empty", ZString.Empty, feeNotVAT.CF_MethodOfPayment);
			});
		}

		public void TestProcessDSPAcceptedGreenCircuitMessageWithExistingMRN()
		{
			entryHeader.MovementReferenceNumberSetter(MRNCode);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearance, InterchangeID);

			ProcessMessageForTest(message);
			AssertAcceptedEntryInstruction(message, entryHeader, parallel: true);
		}

		public void TestProcessDSPAcceptedGreenCircuitDifferedMessage()
		{
			entryHeader.TotalAmount = ZDecimal.Zero;

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearance, InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretation = "<H3>Warning: Taxes and fees data received differ from sent data</H3>";
			AssertAcceptedEntryInstruction(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, parallel: true);
		}

		public void TestProcessDSPAcceptedYellowCircuitMessage()
		{
			entryHeader.TotalAmount = 380;
			AddFeesToEntryLine();
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileYellowCircuit(), InterchangeID);

			ProcessMessageForTest(message);
			AssertAcceptedYellowEntryInstruction(message, entryHeader, entryStatusCode: EntryStatusCodes.ClearedWithPendingDocuments, circuit: CircuitCodeList.Codes.YELLOW, circuitCan: CircuitCodeList.Codes.YELLOW, parallel: true);

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

		public void TestProcessDSPAcceptedYellowCircuitMessageWithStatusPDA()
		{
			entryHeader.TotalAmount = 380;
			AddFeesToEntryLine();
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileYellowCircuitPDAStatus(), InterchangeID);

			ProcessMessageForTest(message);
			AssertAcceptedYellowEntryInstruction(message, entryHeader, entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, circuit: ZString.Empty, circuitCan: CircuitCodeList.Codes.YELLOW, parallel: true);

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
			limitPaymentNull = true;
			AssertProcessPDSAcceptedMessageAndTriggerInboxRequestWithPreviousDocument_EHub();
		}

		public void TestProcessPDSAcceptedMessageAndTriggerInboxRequestWhenPreviousDocumentInDeclaration_xT()
		{
			declaration.PreviousDocuments.AddNew();
			limitPaymentNull = true;
			AssertProcessPDSAcceptedMessageAndTriggerInboxRequestWithPreviousDocument_xT();
		}

		public void TestProcessPDSAcceptedMessageAndTriggerInboxRequestWhenPreviousDocumentInInvoiceHeader_EHub()
		{
			invoice.PreviousDocuments.AddNew();
			limitPaymentNull = true;
			AssertProcessPDSAcceptedMessageAndTriggerInboxRequestWithPreviousDocument_EHub();
		}

		public void TestProcessPDSAcceptedMessageAndTriggerInboxRequestWhenPreviousDocumentInInvoiceHeader_xT()
		{
			invoice.PreviousDocuments.AddNew();
			limitPaymentNull = true;
			AssertProcessPDSAcceptedMessageAndTriggerInboxRequestWithPreviousDocument_xT();
		}

		public void TestProcessPDSAcceptedMessageAndTriggerInboxRequestWhenPreviousDocumentInInvoiceLines_EHub()
		{
			invoiceLine1.PreviousDocuments.AddNew();
			invoiceLine2.PreviousDocuments.AddNew();
			invoiceLine3.PreviousDocuments.AddNew();
			limitPaymentNull = true;
			AssertProcessPDSAcceptedMessageAndTriggerInboxRequestWithPreviousDocument_EHub();
		}

		public void TestProcessPDSAcceptedMessageAndTriggerInboxRequestWhenPreviousDocumentInInvoiceLines_xT()
		{
			invoiceLine1.PreviousDocuments.AddNew();
			invoiceLine2.PreviousDocuments.AddNew();
			invoiceLine3.PreviousDocuments.AddNew();
			limitPaymentNull = true;
			AssertProcessPDSAcceptedMessageAndTriggerInboxRequestWithPreviousDocument_xT();
		}

		public void TestProcessPDSAcceptedMessageAndNotTriggerInboxRequestWhenNoPreviousDocuments()
		{
			limitPaymentNull = true;
			AssertProcessPDSAcceptedMessageAndNotTriggerInboxRequestWhenNoPreviousDocument();
		}

		public void TestProcessPDSAcceptedMessageAndNotTriggerInboxRequestWhenNoPreviousDocumentInOneInvoiceLine()
		{
			invoiceLine1.PreviousDocuments.AddNew();
			invoiceLine2.PreviousDocuments.AddNew();
			limitPaymentNull = true;
			AssertProcessPDSAcceptedMessageAndNotTriggerInboxRequestWhenNoPreviousDocument();
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
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
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
				entryInstruction1.CEI_Style = EntrySubStyleList.Codes.T2L;
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
			entryHeader.TotalAmount = 45.13;

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
			AssertAcceptedEntryInstruction(message, entryHeader);

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

		public void TestProcessAcceptedMessageWithNoClearanceTriggers031DocumentCapture()
		{
			AssertProcessAcceptedMessageWithNoClearanceTriggers031DocumentCapture(
				messageBody: AcceptanceTestFileGreenCircuitWithoutClearance,
				expectedPaymentProofNumber: PaymentProofNumber,
				expectedAtcPaymentProofNumber: AtcPaymentProofNumber);
		}

		string GetAcceptanceTestFileYellowCircuit() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.SimplifiedImportTestFilePath, "AcceptedYellowCircuitMessage.txt");
		string GetAcceptanceTestFileYellowCircuitPDAStatus() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.SimplifiedImportTestFilePath, "AcceptedYellowCircuitMessagePDAStatus.txt");
		protected override string AcceptanceTestFilePDSResponse => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.SimplifiedImportTestFilePath, "AcceptedPDSMessage.txt");
		protected override string AcceptanceTestFileWithClearance => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.SimplifiedImportTestFilePath, "AcceptedGreenCircuitMessage.txt");
		protected override string RejectedTestFile => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.SimplifiedImportTestFilePath, "RejectedMessage.txt");
		protected override string AcceptanceTestFileWithoutGuarantees => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.SimplifiedImportTestFilePath, "AcceptedGreenCircuitMessageNoGuarantees.txt");
		protected override string AcceptanceTestFileWithGuaranteesAEAT => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.SimplifiedImportTestFilePath, "AcceptedGreenCircuitMessageGuaranteesAEAT.txt");
		protected override string AcceptanceTestFileWithGuaranteesATC => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.SimplifiedImportTestFilePath, "AcceptedGreenCircuitMessageGuaranteesATC.txt");
		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.SimplifiedImportTestFilePath, "AcceptedMessageWithLongSegmentId.txt");
		string AcceptanceTestFileGreenCircuitWithoutClearance => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.SimplifiedImportTestFilePath, "AcceptedGreenCircuitMessageNoClearance.txt");

		protected override SimplifiedImportResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new SimplifiedImportResponseMessageProcessor(logger);

		protected override ZString GetExpectedProcessorFriendlyName() => "Import Simplified Pre-Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => [DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration];

		protected override ZString RejectedMessageNum => "TARIC20201020141013988000";
		protected override ZString RejectedMessageInterpretation => "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td><strong>Error</strong></td><td><strong>Location/Description</strong></td></tr>" +
					"<tr><td>4303</td><td>3.3<br>Partida(3).CASILLA 44(3).REFERENCIA DOCUMENTO (CAS 44). No se puede declarar el documento 1413 si la posición taric (Cas.33) no tiene la medida SAV y SNM o teniéndola no tiene la remisión 02.363..C44Referencia.KK</td></tr>" +
					"<tr><td>4303</td><td>2.1<br>Partida(3).CASILLA 44(3).REFERENCIA DOCUMENTO (CAS 44). No se puede declarar el documento 1413 si la posición taric (Cas.33) no tiene la medida SAV y SNM o teniéndola no tiene la remisión 02.363..C44Referencia.KK</td></tr></table>";

		void AssertAcceptedEntryInstruction(TestEdiMessage message, CusEntryHeader entryHeader, string expectedMessageInterpretation = "", string entryStatusCode = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, string circuit = CircuitCodeList.Codes.GREEN, string circuitCan = CircuitCodeList.Codes.GREEN, bool parallel = false)
		{
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageNum: MessageNum, messageSubType: "ACC", entryStatusCode: entryStatusCode, circuit: circuit, acceptanceDate: AcceptanceDate, entryReleaseDate: EntryReleaseDate, paymentProofNumber: ExpectedPaymentProofNumber, atcPaymentProofNumber: ExpectedAtcPaymentProofNumber, limitPaymentDate: ExpectedLimitPaymentDate, csvClearance: CsvClearance, csvImportCertificate: CsvImportCertificate, circuitCan: circuitCan, parallel: parallel, movementReferenceNumber: MRNCode);
		}

		void AssertAcceptedYellowEntryInstruction(TestEdiMessage message, CusEntryHeader entryHeader, string expectedMessageInterpretation = "", string entryStatusCode = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, string circuit = CircuitCodeList.Codes.GREEN, string circuitCan = CircuitCodeList.Codes.GREEN, bool parallel = false)
		{
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageNum: MessageNum, messageSubType: "ACC", entryStatusCode: entryStatusCode, circuit: circuit, acceptanceDate: AcceptanceDate, circuitCan: circuitCan, parallel: parallel, movementReferenceNumber: MRNCode);
		}

		bool limitPaymentNull;
		protected override int ExpcetedNumberDocMessagesAmountATCInstructionC => 6;
		protected override string ExpectedEntryStatusForInstructionC => EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
		protected override bool IsImportQuery => false;
		protected override string ExpectedCircuit => CircuitCodeList.Codes.GREEN;
		protected override ZDateTime ExpectedLimitPaymentDate => limitPaymentNull ? ZDateTime.Empty : LimitPaymentDate;
		protected override ZDateTime ExpectedATCLimitPaymentDate => ZDateTime.Empty;
		protected override string ExpectedAtcPaymentProofNumber => AtcPaymentProofNumber;
		protected override string ExpectedPaymentProofNumber => PaymentProofNumber;
		protected override string ExpectedExportMRN => string.Empty;
		protected override string ExpectedCircuitCan => CircuitCodeList.Codes.GREEN;
		protected override string ExpectedMovementReferenceNumberReject => string.Empty;
		protected override ZDateTime ExpectedMovementReferenceNumberIssueDateReject => ZDateTime.Empty;
		protected override string ExpectedentryStatusCode => EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
	}
}
