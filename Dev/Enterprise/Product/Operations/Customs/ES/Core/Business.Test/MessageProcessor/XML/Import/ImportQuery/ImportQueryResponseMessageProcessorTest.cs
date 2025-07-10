using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.ConsultaImportacionV2Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using NUnit.Framework;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;
using CusTempStorageRegHeader = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader;
using CusTempStorageRegLine = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine;

namespace Enterprise.Customs.ES.Business.Testing;

public class ImportQueryResponseMessageProcessorTest : ImportGenericResponseMessageProcessorTest<ImportQueryResponseMessageProcessor, ConsultaImportacionV2Sal>
{
	[TestDate(2020, 10, 05, 09, 36, 0)]
	public void TestProcessDUAAcceptedGreenCircuitMessageWithProcedureTypeB()
	{
		entryHeader.TotalAmount = 380;

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitProcedureB(), InterchangeID);

		ProcessMessageForTest(message);
		var expectedMessageInterpretation =
			"<H3>Accepted Declaration</H3>" +
			"<br><table border=\"0\"></table>" +
			"<table border=\"0\"><tr><td>Export Doc. (MRN):</td><td>&nbsp;&nbsp;</td><td>20EXP0999930006184</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>TEST444444444444</td></tr>" +
			"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>04-11-2020, 00:00:00</td></tr>" +
			"<tr><td>Import cert. (CSV):</td><td>&nbsp;&nbsp;</td><td>CRTF444444444444</td></tr></table><br>" +
			"<H2>Management data</H2><br>" +
			"<table border=\"0\"><tr><td>Administration:</td><td>&nbsp;&nbsp;</td><td>AEAT</td></tr>" +
			"<tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>DUA - Declaración Completa de Importación</td></tr>" +
			"<tr><td>Customs Clearance Status:</td><td>&nbsp;&nbsp;</td><td>L - Accepted with Customs Clearance</td></tr>" +
			"<tr><td>Unfinished Pendencies:</td><td>&nbsp;&nbsp;</td><td>N - No tiene</td></tr></table><br>" +
			"<br><H2>Taxes and fees data</H2><br>" +
			"<table border=\"0\"><tr><td>Total:</td><td>&nbsp;&nbsp;</td><td>380</td></tr>" +
			"<tr><td>Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>310</td></tr>" +
			"<tr><td>Total Deferred VAT:</td><td>&nbsp;&nbsp;</td><td>35</td></tr>" +
			"<tr><td>Clearance Guarantee VAT Exemption:</td><td>&nbsp;&nbsp;</td><td>80</td></tr>" +
			"<tr><td>Real Clearance Guarantee:</td><td>&nbsp;&nbsp;</td><td>300</td></tr>" +
			"<tr><td>Pendency Guarantee VAT Exemption:</td><td>&nbsp;&nbsp;</td><td>10</td></tr>" +
			"<tr><td>Real Pendency Guarantee:</td><td>&nbsp;&nbsp;</td><td>10</td></tr></table><br>" +
			"<H2>Payment information</H2><br>" +
			"<table border=\"0\"><tr><td>Payment Proof Number:</td><td>&nbsp;&nbsp;</td><td>1234</td></tr>" +
			"<tr><td>Payment date limit:</td><td>&nbsp;&nbsp;</td><td>07-06-2021</td></tr></table><br>" +
			"<br><H2>Guarantees</H2><br>" +
			"<table border=\"0\"><tr><td>Accounting Status:</td><td>&nbsp;&nbsp;</td><td>CO - Contractado</td></tr>" +
			"<tr><td>Guarantee Status:</td><td>&nbsp;&nbsp;</td><td>Not Written Off (status not IT)</td></tr></table><br>" +
			"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
			"<thead><tr class=\"tableheadings\"><th>Customs</th><th>GRN</th><th>Real Debt</th><th>Potential Debt</th><th>Undetermined Real Debt</th></tr></thead>" +
			"<tr><td>AEAT</td><td>16ESAGL9990000096</td><td>387.51</td><td>5</td><td>30</td></tr>" +
			"<tr><td>AEAT</td><td>16ESAGL9990000097</td><td>350</td><td>5</td><td>25</td></tr></table><br>" +
			"<br><H2>Required Certificates</H2><br><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead>" +
			"<tr class=\"tableheadings\"><th>Item</th><th>Measure</th><th>Agency</th><th>Documents</th></tr></thead>" +
			"<tr><td>2</td><td>SNM</td><td>SIF05 -  Sanidad Exterior - M&#186; Sanidad</td><td>N851, C085</td></tr>" +
			"<tr><td>2</td><td>SNM</td><td>SIF06 -  Sanidad Inetrior - M&#186; Sanidad</td><td>C657, C678</td></tr>" +
			"</table>";
		AssertAcceptedAEAT(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation);
	}

	public void TestProcessDUAAcceptedGreenCircuitDifferedMessageWithProcedureTypeB()
	{
		entryHeader.TotalAmount = ZDecimal.Zero;

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitProcedureB(), InterchangeID);

		ProcessMessageForTest(message);

		var expectedMessageInterpretation =
				"<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"></table>" +
				"<table border=\"0\"><tr><td>Export Doc. (MRN):</td><td>&nbsp;&nbsp;</td><td>20EXP0999930006184</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>TEST444444444444</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>04-11-2020, 00:00:00</td></tr>" +
				"<tr><td>Import cert. (CSV):</td><td>&nbsp;&nbsp;</td><td>CRTF444444444444</td></tr></table><br>" +
				"<H2>Management data</H2><br>" +
				"<table border=\"0\"><tr><td>Administration:</td><td>&nbsp;&nbsp;</td><td>AEAT</td></tr>" +
				"<tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>DUA - Declaración Completa de Importación</td></tr>" +
				"<tr><td>Customs Clearance Status:</td><td>&nbsp;&nbsp;</td><td>L - Accepted with Customs Clearance</td></tr>" +
				"<tr><td>Unfinished Pendencies:</td><td>&nbsp;&nbsp;</td><td>N - No tiene</td></tr></table><br>" +
				"<br><H2>Taxes and fees data</H2>" +
				"<H3>Warning: Taxes and fees data received differ from sent data</H3><br>" +
				"<table border=\"0\"><tr><td>Total:</td><td>&nbsp;&nbsp;</td><td>380</td></tr>" +
				"<tr><td>Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>310</td></tr>" +
				"<tr><td>Total Deferred VAT:</td><td>&nbsp;&nbsp;</td><td>35</td></tr>" +
				"<tr><td>Clearance Guarantee VAT Exemption:</td><td>&nbsp;&nbsp;</td><td>80</td></tr>" +
				"<tr><td>Real Clearance Guarantee:</td><td>&nbsp;&nbsp;</td><td>300</td></tr>" +
				"<tr><td>Pendency Guarantee VAT Exemption:</td><td>&nbsp;&nbsp;</td><td>10</td></tr>" +
				"<tr><td>Real Pendency Guarantee:</td><td>&nbsp;&nbsp;</td><td>10</td></tr></table><br>" +
				"<H2>Payment information</H2><br>" +
				"<table border=\"0\"><tr><td>Payment Proof Number:</td><td>&nbsp;&nbsp;</td><td>1234</td></tr>" +
				"<tr><td>Payment date limit:</td><td>&nbsp;&nbsp;</td><td>07-06-2021</td></tr></table><br>" +
				"<br><H2>Guarantees</H2><br>" +
				"<table border=\"0\"><tr><td>Accounting Status:</td><td>&nbsp;&nbsp;</td><td>CO - Contractado</td></tr>" +
				"<tr><td>Guarantee Status:</td><td>&nbsp;&nbsp;</td><td>Not Written Off (status not IT)</td></tr></table><br>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
				"<thead><tr class=\"tableheadings\"><th>Customs</th><th>GRN</th><th>Real Debt</th><th>Potential Debt</th><th>Undetermined Real Debt</th></tr></thead>" +
				"<tr><td>AEAT</td><td>16ESAGL9990000096</td><td>387.51</td><td>5</td><td>30</td></tr>" +
				"<tr><td>AEAT</td><td>16ESAGL9990000097</td><td>350</td><td>5</td><td>25</td></tr></table><br>" +
				"<br><H2>Required Certificates</H2><br><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead>" +
				"<tr class=\"tableheadings\"><th>Item</th><th>Measure</th><th>Agency</th><th>Documents</th></tr></thead>" +
				"<tr><td>2</td><td>SNM</td><td>SIF05 -  Sanidad Exterior - M&#186; Sanidad</td><td>N851, C085</td></tr>" +
				"<tr><td>2</td><td>SNM</td><td>SIF06 -  Sanidad Inetrior - M&#186; Sanidad</td><td>C657, C678</td></tr>" +
				"</table>";
		AssertAcceptedAEAT(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, parallel: true);
	}

	public void TestProcessDUAAcceptedGreenCircuitWithProcedureTypeZWithC44DocumentoJustificativo()
	{
		entryHeader.TotalAmount = 380;

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitProcedureZWithDoc(), InterchangeID);

		ProcessMessageForTest(message);
		AssertAcceptedAEAT(message, entryHeader, entryStatusCode: EntryStatusCodes.Cleared);
	}

	public void TestProcessDUAAcceptedGreenCircuitWithProcedureTypeZWithoutC44DocumentoJustificativo()
	{
		entryHeader.TotalAmount = 380;

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitProcedureZWithoutDoc(), InterchangeID);

		ProcessMessageForTest(message);
		AssertAcceptedAEAT(message, entryHeader);
	}

	public void TestProcessDUAAcceptedGreenCircuitWithProcedureTypeA_AEAT()
	{
		entryHeader.TotalAmount = 380;

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitProcedureA_AEAT(), InterchangeID);

		ProcessMessageForTest(message);

		var expectedMessageInterpretation =
				"<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"></table>" +
				"<table border=\"0\"><tr><td>Export Doc. (MRN):</td><td>&nbsp;&nbsp;</td><td>20EXP0999930006184</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>TEST444444444444</td></tr><tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>04-11-2020, 00:00:00</td></tr><tr><td>Import cert. (CSV):</td><td>&nbsp;&nbsp;</td><td>CRTF444444444444</td></tr></table><br>" +
				"<H2>Management data</H2><br>" +
				"<table border=\"0\"><tr><td>Administration:</td><td>&nbsp;&nbsp;</td><td>AEAT</td></tr>" +
				"<tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>DUA - Declaración Completa de Importación</td></tr>" +
				"<tr><td>Customs Clearance Status:</td><td>&nbsp;&nbsp;</td><td>L - Accepted with Customs Clearance</td></tr>" +
				"<tr><td>Unfinished Pendencies:</td><td>&nbsp;&nbsp;</td><td>N - No tiene</td></tr></table><br>" +
				"<br><H2>Taxes and fees data</H2><br>" +
				"<table border=\"0\"><tr><td>Total:</td><td>&nbsp;&nbsp;</td><td>380</td></tr>" +
				"<tr><td>Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>310</td></tr>" +
				"<tr><td>Total Deferred VAT:</td><td>&nbsp;&nbsp;</td><td>35</td></tr>" +
				"<tr><td>Clearance Guarantee VAT Exemption:</td><td>&nbsp;&nbsp;</td><td>80</td></tr>" +
				"<tr><td>Real Clearance Guarantee:</td><td>&nbsp;&nbsp;</td><td>300</td></tr>" +
				"<tr><td>Pendency Guarantee VAT Exemption:</td><td>&nbsp;&nbsp;</td><td>10</td></tr>" +
				"<tr><td>Real Pendency Guarantee:</td><td>&nbsp;&nbsp;</td><td>10</td></tr></table><br>" +
				"<H2>Payment information</H2><br>" +
				"<table border=\"0\"><tr><td>Payment Proof Number:</td><td>&nbsp;&nbsp;</td><td>1234</td></tr>" +
				"<tr><td>Payment date limit:</td><td>&nbsp;&nbsp;</td><td>07-06-2021</td></tr></table><br>" +
				"<br><H2>Guarantees</H2><br>" +
				"<table border=\"0\"><tr><td>Accounting Status:</td><td>&nbsp;&nbsp;</td><td>CO - Contractado</td></tr>" +
				"<tr><td>Guarantee Status:</td><td>&nbsp;&nbsp;</td><td>Not Written Off (status not IT)</td></tr></table><br>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
				"<thead><tr class=\"tableheadings\"><th>Customs</th><th>GRN</th><th>Real Debt</th><th>Potential Debt</th><th>Undetermined Real Debt</th></tr></thead>" +
				"<tr><td>AEAT</td><td>16ESAGL9990000096</td><td>387.51</td><td>5</td><td>30</td></tr>" +
				"<tr><td>AEAT</td><td>16ESAGL9990000097</td><td>350</td><td>5</td><td>25</td></tr>" +
				"</table><br><br><H2>Taxes and fees response (Spanish Customs)</H2><br>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Item</th><th>Type</th><th>MAX/MIN Rate</th><th>Base Amount</th><th>Tax Rate</th><th>Total Amount</th><th>Total Guaranteed Amount</th></tr></thead>" +
				"<tr><td>1</td><td>A00</td><td>&nbsp;</td><td>100.650</td><td>2.700000 %</td><td>2.72</td><td>2.72</td></tr>" +
				"<tr><td>1</td><td>C00</td><td>MA</td><td>104.950</td><td>21.000000 %</td><td>22.04</td><td>22.04</td></tr>" +
				"<tr><td>2</td><td>C00</td><td>&nbsp;</td><td>1219.350</td><td>17.600000 %</td><td>214.61</td><td>214.61</td></tr>" +
				"<tr><td>2</td><td>B00</td><td>&nbsp;</td><td>1481.380</td><td>10.000000 €/KN</td><td>148.14</td><td>148.14</td></tr>" +
				"<tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>Total: </td><td>387.51</td><td>387.51</td></tr></table><br>";
		AssertAcceptedAEAT(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, entryStatusCode: EntryStatusCodes.Cleared);
	}

	public void TestProcesDUAAcceptedGreenCircuitWithProcedureTypeA_ATC()
	{
		entryHeader.TotalAmount = 380;

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitProcedureA_ATC(), InterchangeID);

		ProcessMessageForTest(message);

		var expectedMessageInterpretation =
				"<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"></table>" +
				"<table border=\"0\"><tr><td>Export Doc. (MRN):</td><td>&nbsp;&nbsp;</td><td>20EXP0999930006184</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>TEST444444444444</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>04-11-2020, 00:00:00</td></tr>" +
				"<tr><td>Import cert. (CSV):</td><td>&nbsp;&nbsp;</td><td>CRTF444444444444</td></tr></table><br>" +
				"<H2>Management data</H2><br>" +
				"<table border=\"0\"><tr><td>Administration:</td><td>&nbsp;&nbsp;</td><td>ATC</td></tr>" +
				"<tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>DUA - Declaración Completa de Importación</td></tr>" +
				"<tr><td>Customs Clearance Status:</td><td>&nbsp;&nbsp;</td><td>L - Accepted with Customs Clearance</td></tr>" +
				"<tr><td>Unfinished Pendencies:</td><td>&nbsp;&nbsp;</td><td>N - No tiene</td></tr></table><br>" +
				"<br><H2>Taxes and fees data</H2><br>" +
				"<table border=\"0\"><tr><td>Total:</td><td>&nbsp;&nbsp;</td><td>380</td></tr>" +
				"<tr><td>Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>310</td></tr>" +
				"<tr><td>Total Deferred VAT:</td><td>&nbsp;&nbsp;</td><td>35</td></tr>" +
				"<tr><td>Clearance Guarantee VAT Exemption:</td><td>&nbsp;&nbsp;</td><td>80</td></tr>" +
				"<tr><td>Real Clearance Guarantee:</td><td>&nbsp;&nbsp;</td><td>300</td></tr>" +
				"<tr><td>Pendency Guarantee VAT Exemption:</td><td>&nbsp;&nbsp;</td><td>10</td></tr>" +
				"<tr><td>Real Pendency Guarantee:</td><td>&nbsp;&nbsp;</td><td>10</td></tr></table><br>" +
				"<H2>Payment information</H2><br>" +
				"<table border=\"0\"><tr><td>Payment Proof Number:</td><td>&nbsp;&nbsp;</td><td>JUSTPAGOATC</td></tr>" +
				"<tr><td>Payment date limit:</td><td>&nbsp;&nbsp;</td><td>07-06-2021</td></tr></table><br>" +
				"<br><H2>Guarantees</H2><br>" +
				"<table border=\"0\"><tr><td>Accounting Status:</td><td>&nbsp;&nbsp;</td><td>CO - Contractado</td></tr>" +
				"<tr><td>Guarantee Status:</td><td>&nbsp;&nbsp;</td><td>Not Written Off (status not IT)</td></tr></table><br>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
				"<thead><tr class=\"tableheadings\"><th>Customs</th><th>GRN</th><th>Real Debt</th><th>Potential Debt</th><th>Undetermined Real Debt</th></tr></thead>" +
				"<tr><td>ATC</td><td>16ESAGL9990000096</td><td>387.51</td><td>5</td><td>30</td></tr>" +
				"<tr><td>ATC</td><td>16ESAGL9990000097</td><td>350</td><td>5</td><td>25</td></tr>" +
				"</table><br><br><H2>Taxes and fees response (Spanish Customs)</H2><br>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Item</th><th>Type</th><th>MAX/MIN Rate</th><th>Base Amount</th><th>Tax Rate</th><th>Total Amount</th><th>Total Guaranteed Amount</th></tr></thead>" +
				"<tr><td>1</td><td>3IG</td><td>&nbsp;</td><td>100.650</td><td>2.700000 %</td><td>2.72</td><td>2.72</td></tr>" +
				"<tr><td>1</td><td>400</td><td>MA</td><td>104.950</td><td>21.000000 %</td><td>22.04</td><td>22.04</td></tr>" +
				"<tr><td>2</td><td>300</td><td>&nbsp;</td><td>1219.350</td><td>17.600000 %</td><td>214.61</td><td>214.61</td></tr>" +
				"<tr><td>2</td><td>4IG</td><td>&nbsp;</td><td>1481.380</td><td>10.000000 €/KN</td><td>148.14</td><td>148.14</td></tr>" +
				"<tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>Total: </td><td>387.51</td><td>387.51</td></tr></table><br>";
		AssertAcceptedATC(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, entryStatusCode: EntryStatusCodes.Cleared, atcLimitPaymentDate: AtcLimitPaymentDate);
	}

	public void TestProcessDUAAcceptedYellowCircuitMessage()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileYellowCircuit(), InterchangeID);

		ProcessMessageForTest(message);

		AssertAccepted(message, entryHeader, parallel: true, entryStatusCode: EntryStatusCodes.Cleared, circuit: CircuitCodeList.Codes.YELLOW, acceptanceDate: AcceptanceDate);
	}

	public void TestProcessDUAAcceptedOrangeCircuitMessage()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileOrangeCircuit(), InterchangeID);

		ProcessMessageForTest(message);

		AssertAccepted(message, entryHeader, parallel: true, entryStatusCode: EntryStatusCodes.Cleared, circuit: CircuitCodeList.Codes.ORANGE, acceptanceDate: AcceptanceDate);
	}

	public void TestProcessPDSAcceptedMessage()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFilePDSResponse, InterchangeID);

		ProcessMessageForTest(message);

		AssertAccepted(message, entryHeader, entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, circuit: CircuitCodeList.Codes.YELLOW, acceptanceDate: AcceptanceDate);
	}

	public void TestProcessPDSAcceptedMessageWithFeesAEAT()
	{
		entryHeader.TotalAmount = 380;
		AddFeesToEntryLine();
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFilePDSResponseWithFeesAEAT(), InterchangeID);

		ProcessMessageForTest(message);

		AssertAccepted(message, entryHeader, entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, circuit: CircuitCodeList.Codes.YELLOW, acceptanceDate: AcceptanceDate, parallel: true);

		CombineAssertions("Test EntryLines fees", () =>
		{
			AssertEquals("EntryLine1.Fees count (not deleted and nothing added)", 2, entryLine1.Fees.Count);

			AssertEquals("EntryLine1.Fee[0].CF_ChargeType", "A00", entryLine1.Fees[0].CF_ChargeType);
			AssertEquals("EntryLine1.Fee[0].CF_BaseValue", 200.650M, entryLine1.Fees[0].CF_BaseValue);
			AssertEquals("EntryLine1.Fee[0].CF_Rate", 3.700000M, entryLine1.Fees[0].CF_Rate);
			AssertEquals("EntryLine1.Fee[0].MaxMin", "MA", entryLine1.Fees[0].MaxMin);
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

	public void TestProcessPDSAcceptedMessageWithFeesATC()
	{
		entryHeader.TotalAmount = 380;
		AddFeesToEntryLine();
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFilePDSResponseWithFeesATC(), InterchangeID);

		ProcessMessageForTest(message);

		AssertAcceptedATC(message, entryHeader, entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, circuitCan: CircuitCodeList.Codes.YELLOW, exportMRN: ZString.Empty, atcPaymentProofNumber: ZString.Empty, parallel: true);

		CombineAssertions("Test EntryLines fees", () =>
		{
			AssertEquals("EntryLine1.Fees count (not deleted and nothing added)", 2, entryLine1.Fees.Count);

			AssertEquals("EntryLine1.Fee[0].CF_ChargeType", "A00", entryLine1.Fees[0].CF_ChargeType);
			AssertEquals("EntryLine1.Fee[0].CF_BaseValue", 200.650M, entryLine1.Fees[0].CF_BaseValue);
			AssertEquals("EntryLine1.Fee[0].CF_Rate", 3.700000M, entryLine1.Fees[0].CF_Rate);
			AssertEquals("EntryLine1.Fee[0].MaxMin", "MA", entryLine1.Fees[0].MaxMin);
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

	public void TestProcessPDIAcceptedMessage()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFilePDIResponse(), InterchangeID);

		ProcessMessageForTest(message);

		AssertAccepted(message, entryHeader, entryStatusCode: EntryStatusCodes.IncompletePreDeclaration, circuit: CircuitCodeList.Codes.YELLOW, acceptanceDate: AcceptanceDate);
	}

	public void TestProcessPDIAcceptedMessageWithFeesAEAT()
	{
		entryHeader.TotalAmount = 380;
		AddFeesToEntryLine();
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFilePDIResponseWithFeesAEAT(), InterchangeID);

		ProcessMessageForTest(message);

		AssertAccepted(message, entryHeader, entryStatusCode: EntryStatusCodes.IncompletePreDeclaration, circuit: CircuitCodeList.Codes.YELLOW, acceptanceDate: AcceptanceDate, parallel: true);

		CombineAssertions("Test EntryLines fees", () =>
		{
			AssertEquals("EntryLine1.Fees count", 3, entryLine1.Fees.Count);

			AssertEquals("EntryLine1.Fee[0].CF_ChargeType", "300", entryLine1.Fees[0].CF_ChargeType);
			AssertEquals("EntryLine1.Fee[0].CF_BaseValue", 200.650M, entryLine1.Fees[0].CF_BaseValue);
			AssertEquals("EntryLine1.Fee[0].CF_Rate", 3.700000M, entryLine1.Fees[0].CF_Rate);
			AssertEquals("EntryLine1.Fee[0].MaxMin", "MA", entryLine1.Fees[0].MaxMin);
			AssertEquals("EntryLine1.Fee[0].G4_RateDuty", ZString.Empty, entryLine1.Fees[0].G4_RateDuty);
			AssertEquals("EntryLine1.Fee[0].CF_ChargeAmount", 3.72M, entryLine1.Fees[0].CF_ChargeAmount);
			AssertEquals("EntryLine1.Fee[0].CF_RateOverrideReasonCode", "ADD", entryLine1.Fees[0].CF_RateOverrideReasonCode);

			AssertEquals("EntryLine1.Fee[1].CF_ChargeType", "A00", entryLine1.Fees[1].CF_ChargeType);
			AssertEquals("EntryLine1.Fee[1].CF_BaseValue", 100.650M, entryLine1.Fees[1].CF_BaseValue);
			AssertEquals("EntryLine1.Fee[1].CF_Rate", 2.700000M, entryLine1.Fees[1].CF_Rate);
			AssertEquals("EntryLine1.Fee[1].MaxMin", ZString.Empty, entryLine1.Fees[1].MaxMin);
			AssertEquals("EntryLine1.Fee[1].G4_RateDuty", "%", entryLine1.Fees[1].G4_RateDuty);
			AssertEquals("EntryLine1.Fee[1].CF_ChargeAmount", 2.72M, entryLine1.Fees[1].CF_ChargeAmount);
			AssertEquals("EntryLine1.Fee[1].CF_RateOverrideReasonCode", "OVR", entryLine1.Fees[1].CF_RateOverrideReasonCode);

			AssertEquals("EntryLine1.Fee[2].CF_ChargeType", "C00", entryLine1.Fees[2].CF_ChargeType);
			AssertEquals("EntryLine1.Fee[2].CF_BaseValue", 104.950M, entryLine1.Fees[2].CF_BaseValue);
			AssertEquals("EntryLine1.Fee[2].CF_Rate", 21.000000M, entryLine1.Fees[2].CF_Rate);
			AssertEquals("EntryLine1.Fee[2].MaxMin", "MA", entryLine1.Fees[2].MaxMin);
			AssertEquals("EntryLine1.Fee[2].G4_RateDuty", "%", entryLine1.Fees[2].G4_RateDuty);
			AssertEquals("EntryLine1.Fee[2].CF_ChargeAmount", 22.04M, entryLine1.Fees[2].CF_ChargeAmount);
			AssertEquals("EntryLine1.Fee[2].CF_RateOverrideReasonCode", "OVR", entryLine1.Fees[2].CF_RateOverrideReasonCode);

			AssertEquals("EntryLine2.Fees count", 2, entryLine2.Fees.Count);

			AssertEquals("EntryLine2.Fee[0].CF_ChargeType", "C00", entryLine2.Fees[0].CF_ChargeType);
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

	public void TestProcessPDIAcceptedMessageWithFeesATC()
	{
		entryHeader.TotalAmount = 380;
		AddFeesToEntryLine();
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFilePDIResponseWithFeesATC(), InterchangeID);

		ProcessMessageForTest(message);

		AssertAcceptedATC(message, entryHeader, entryStatusCode: EntryStatusCodes.IncompletePreDeclaration, circuitCan: CircuitCodeList.Codes.YELLOW, exportMRN: ZString.Empty, atcPaymentProofNumber: ZString.Empty, parallel: true);

		CombineAssertions("Test EntryLines fees", () =>
		{
			AssertEquals("EntryLine1.Fees count", 3, entryLine1.Fees.Count);

			AssertEquals("EntryLine1.Fee[0].CF_ChargeType", "A00", entryLine1.Fees[0].CF_ChargeType);
			AssertEquals("EntryLine1.Fee[0].CF_BaseValue", 200.650M, entryLine1.Fees[0].CF_BaseValue);
			AssertEquals("EntryLine1.Fee[0].CF_Rate", 3.700000M, entryLine1.Fees[0].CF_Rate);
			AssertEquals("EntryLine1.Fee[0].MaxMin", "MA", entryLine1.Fees[0].MaxMin);
			AssertEquals("EntryLine1.Fee[0].G4_RateDuty", ZString.Empty, entryLine1.Fees[0].G4_RateDuty);
			AssertEquals("EntryLine1.Fee[0].CF_ChargeAmount", 3.72M, entryLine1.Fees[0].CF_ChargeAmount);
			AssertEquals("EntryLine1.Fee[0].CF_RateOverrideReasonCode", "ADD", entryLine1.Fees[0].CF_RateOverrideReasonCode);

			AssertEquals("EntryLine1.Fee[1].CF_ChargeType", "3IG", entryLine1.Fees[1].CF_ChargeType);
			AssertEquals("EntryLine1.Fee[1].CF_BaseValue", 100.650M, entryLine1.Fees[1].CF_BaseValue);
			AssertEquals("EntryLine1.Fee[1].CF_Rate", 2.700000M, entryLine1.Fees[1].CF_Rate);
			AssertEquals("EntryLine1.Fee[1].MaxMin", ZString.Empty, entryLine1.Fees[1].MaxMin);
			AssertEquals("EntryLine1.Fee[1].G4_RateDuty", "%", entryLine1.Fees[1].G4_RateDuty);
			AssertEquals("EntryLine1.Fee[1].CF_ChargeAmount", 2.72M, entryLine1.Fees[1].CF_ChargeAmount);
			AssertEquals("EntryLine1.Fee[1].CF_RateOverrideReasonCode", "OVR", entryLine1.Fees[1].CF_RateOverrideReasonCode);

			AssertEquals("EntryLine1.Fee[2].CF_ChargeType", "400", entryLine1.Fees[2].CF_ChargeType);
			AssertEquals("EntryLine1.Fee[2].CF_BaseValue", 104.950M, entryLine1.Fees[2].CF_BaseValue);
			AssertEquals("EntryLine1.Fee[2].CF_Rate", 21.000000M, entryLine1.Fees[2].CF_Rate);
			AssertEquals("EntryLine1.Fee[2].MaxMin", "MA", entryLine1.Fees[2].MaxMin);
			AssertEquals("EntryLine1.Fee[2].G4_RateDuty", "%", entryLine1.Fees[2].G4_RateDuty);
			AssertEquals("EntryLine1.Fee[2].CF_ChargeAmount", 22.04M, entryLine1.Fees[2].CF_ChargeAmount);
			AssertEquals("EntryLine1.Fee[2].CF_RateOverrideReasonCode", "OVR", entryLine1.Fees[2].CF_RateOverrideReasonCode);

			AssertEquals("EntryLine2.Fees count", 2, entryLine2.Fees.Count);

			AssertEquals("EntryLine2.Fee[0].CF_ChargeType", "300", entryLine2.Fees[0].CF_ChargeType);
			AssertEquals("EntryLine2.Fee[0].CF_BaseValue", 1219.350M, entryLine2.Fees[0].CF_BaseValue);
			AssertEquals("EntryLine2.Fee[0].CF_Rate", 17.600000M, entryLine2.Fees[0].CF_Rate);
			AssertEquals("EntryLine2.Fee[0].MaxMin", ZString.Empty, entryLine2.Fees[0].MaxMin);
			AssertEquals("EntryLine2.Fee[0].G4_RateDuty", "%", entryLine2.Fees[0].G4_RateDuty);
			AssertEquals("EntryLine2.Fee[0].CF_ChargeAmount", 214.61M, entryLine2.Fees[0].CF_ChargeAmount);
			AssertEquals("EntryLine2.Fee[0].CF_RateOverrideReasonCode", "OVR", entryLine2.Fees[0].CF_RateOverrideReasonCode);

			AssertEquals("EntryLine2.Fee[1].CF_ChargeType", "4IG", entryLine2.Fees[1].CF_ChargeType);
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

	public void TestProcessCancelledMessage()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileCancelledResponse(), InterchangeID);

		ProcessMessageForTest(message);

		AssertAccepted(message, entryHeader, entryStatusCode: EntryStatusCodes.Cancelled, circuit: CircuitCodeList.Codes.YELLOW, acceptanceDate: AcceptanceDate);
	}

	public void TestProcessWithoutCircuitMessage()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileWithoutCircuitResponse(), InterchangeID);

		ProcessMessageForTest(message);

		AssertAccepted(message, entryHeader, entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, acceptanceDate: AcceptanceDate);
	}

	public void TestProcessWithoutCircuitMessageWithFeesAEAT()
	{
		entryHeader.TotalAmount = 380;
		AddFeesToEntryLine();
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileWithoutCircuitResponseWithFeesAEAT(), InterchangeID);

		ProcessMessageForTest(message);

		AssertAccepted(message, entryHeader, entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, acceptanceDate: AcceptanceDate, parallel: true);

		CombineAssertions("Test EntryLines fees", () =>
		{
			AssertEquals("EntryLine1.Fees count (not deleted and nothing added)", 2, entryLine1.Fees.Count);

			AssertEquals("EntryLine1.Fee[0].CF_ChargeType", "A00", entryLine1.Fees[0].CF_ChargeType);
			AssertEquals("EntryLine1.Fee[0].CF_BaseValue", 200.650M, entryLine1.Fees[0].CF_BaseValue);
			AssertEquals("EntryLine1.Fee[0].CF_Rate", 3.700000M, entryLine1.Fees[0].CF_Rate);
			AssertEquals("EntryLine1.Fee[0].MaxMin", "MA", entryLine1.Fees[0].MaxMin);
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

	public void TestProcessWithoutCircuitMessageWithFeesATC()
	{
		entryHeader.TotalAmount = 380;
		AddFeesToEntryLine();
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileWithoutCircuitResponseWithFeesATC(), InterchangeID);

		ProcessMessageForTest(message);

		AssertAcceptedATC(message, entryHeader, entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, circuitCan: ZString.Empty, exportMRN: ZString.Empty, atcPaymentProofNumber: ZString.Empty, parallel: true);

		CombineAssertions("Test EntryLines fees", () =>
		{
			AssertEquals("EntryLine1.Fees count (not deleted and nothing added)", 2, entryLine1.Fees.Count);

			AssertEquals("EntryLine1.Fee[0].CF_ChargeType", "A00", entryLine1.Fees[0].CF_ChargeType);
			AssertEquals("EntryLine1.Fee[0].CF_BaseValue", 200.650M, entryLine1.Fees[0].CF_BaseValue);
			AssertEquals("EntryLine1.Fee[0].CF_Rate", 3.700000M, entryLine1.Fees[0].CF_Rate);
			AssertEquals("EntryLine1.Fee[0].MaxMin", "MA", entryLine1.Fees[0].MaxMin);
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

	public void TestProcessAcceptedC40DocumentoCargoPrecedenteAndC40SumariaDatada()
	{
		entryLine1.AddEntryLineDocument<PreviousDocument>("X001", "ES3600000001");
		entryLine1.AddEntryLineDocument<PreviousDocument>("X002", "ES3600000002");
		entryLine2.AddEntryLineDocument<PreviousDocument>("X003", "ES3600000003");
		entryLine2.AddEntryLineDocument<PreviousDocument>("X004", "ES3600000004");

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitProcedureB(), InterchangeID);

		ProcessMessageForTest(message);
		AssertAcceptedAEAT(message, entryHeader, entryStatusCode: EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, parallel: true);

		CombineAssertions(() =>
		{
			var clPrevDocsEntryLine1 = GetCLPreviousDocuments(entryLine1);
			AssertEquals("There are 1 CL PreviousDocuments after processing for entryLine 1 ", 2, clPrevDocsEntryLine1.Length);
			AssertContainsExactElementsInAnyOrder("The 1 CL PreviousDocuments after processing have the correct CSI_SubType for entryLine 1", new ZString[] { "Z", "X" }, clPrevDocsEntryLine1.Select(x => x.CSI_SubType).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CL PreviousDocuments after processing have the correct CSI_Code for entryLine 1", new ZString[] { "IRR", "SUM" }, clPrevDocsEntryLine1.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CL PreviousDocuments after processing have the correct CSI_ReferenceNumber for entryLine 1", new ZString[] { "20190218", "9999999" }, clPrevDocsEntryLine1.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CL PreviousDocuments after processing have the correct CSI_Status for entryLine 1", new ZString[] { "ACC", "ACC" }, clPrevDocsEntryLine1.Select(x => x.CSI_Status).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CL PreviousDocuments after processing have the correct CSI_LineNo for entryLine 1", new ZInt[] { 0, 1 }, clPrevDocsEntryLine1.Select(x => x.CSI_LineNo).ToArray());

			var clPrevDocsEntryLine2 = GetCLPreviousDocuments(entryLine2);
			AssertEquals("There are 1 CL PreviousDocuments after processing for entryLine 2", 2, clPrevDocsEntryLine2.Length);
			AssertContainsExactElementsInAnyOrder("The 1 CL PreviousDocuments after processing have the correct CSI_SubType for entryLine 2", new ZString[] { "X", "X" }, clPrevDocsEntryLine2.Select(x => x.CSI_SubType).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CL PreviousDocuments after processing have the correct CSI_Code for entryLine 2", new ZString[] { "IX1", "SUM" }, clPrevDocsEntryLine2.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CL PreviousDocuments after processing have the correct CSI_ReferenceNumber for entryLine 2", new ZString[] { "9999999", "123456789" }, clPrevDocsEntryLine2.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CL PreviousDocuments after processing have the correct CSI_Status for entryLine 2", new ZString[] { "ACC", "ACC" }, clPrevDocsEntryLine2.Select(x => x.CSI_Status).ToArray());
			AssertContainsExactElementsInAnyOrder("The 1 CL PreviousDocuments after processing have the correct CSI_LineNo for entryLine 2", new ZInt[] { 0, 12345 }, clPrevDocsEntryLine2.Select(x => x.CSI_LineNo).ToArray());
		});
	}

	public void TestProcessAcceptedC44DocumentosYCertificados()
	{
		entryLine1.AddEntryLineDocument<SupportingDocument>("X001", "ES3600000001");
		entryLine1.AddEntryLineDocument<SupportingDocument>("X002", "ES3600000002");
		entryLine2.AddEntryLineDocument<SupportingDocument>("X003", "ES3600000003");
		entryLine2.AddEntryLineDocument<SupportingDocument>("X004", "ES3600000004");
		entryLine3.AddEntryLineDocument<SupportingDocument>("X005", "ES3600000005");
		entryLine3.AddEntryLineDocument<SupportingDocument>("X006", "ES3600000006");

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileDocumentosYCertificados(), InterchangeID);

		ProcessMessageForTest(message);
		AssertAcceptedAEAT(message, entryHeader, entryStatusCode: EntryStatusCodes.Cleared);

		CombineAssertions(() =>
		{
			var clSupDocsEntryLine1 = GetCLSupportingDocuments(entryLine1);
			AssertEquals("There are 3 CL SupportingDocuments after processing for entryLine 1 " + entryLine1.CL_LineNumber, 3, clSupDocsEntryLine1.Length);
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_Codes for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "N380", "N705", "C055" }, clSupDocsEntryLine1.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_ReferenceNumber for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "14521", "454545", ZString.Empty }, clSupDocsEntryLine1.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_UnitOfQuantity for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "KGM", "DT", ZString.Empty }, clSupDocsEntryLine1.Select(x => x.CSI_UnitOfQuantity).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_Quantity for entryLine 1" + entryLine1.CL_LineNumber, new ZDecimal[] { 27.444, 20.222, ZDecimal.Zero }, clSupDocsEntryLine1.Select(x => x.CSI_Quantity).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_DateOfIssue for entryLine 1" + entryLine1.CL_LineNumber, new ZDateTime[] { new ZDateTime(2019, 01, 28, 00, 00, 00), new ZDateTime(2020, 01, 28, 00, 00, 00), ZDateTime.Empty }, clSupDocsEntryLine1.Select(x => x.CSI_DateOfIssue).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_DateOfExpiry for entryLine 1" + entryLine1.CL_LineNumber, new ZDateTime[] { new ZDateTime(2022, 04, 15, 00, 00, 00), ZDateTime.Empty, ZDateTime.Empty }, clSupDocsEntryLine1.Select(x => x.CSI_DateOfExpiry).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_Status for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "ACC", "ACC", "ACC" }, clSupDocsEntryLine1.Select(x => x.CSI_Status).ToArray());

			var clSupDocsEntryLine2 = GetCLSupportingDocuments(entryLine2);
			AssertEquals("There are 2 CL SupportingDocuments after processing for entryLine 2" + entryLine2.CL_LineNumber, 2, clSupDocsEntryLine2.Length);
			AssertContainsExactElementsInAnyOrder("The 2 CL SupportingDocuments after processing have the correct CSI_Codes for entryLine 2" + entryLine2.CL_LineNumber, new ZString[] { "N381", "N706" }, clSupDocsEntryLine2.Select(x => x.CSI_Code).ToArray());

			var clSupDocsEntryLine3 = GetCLSupportingDocuments(entryLine3);
			AssertEquals("There are 2 CL SupportingDocuments (the original ones) after processing for entryLine 3" + entryLine3.CL_LineNumber, 2, clSupDocsEntryLine3.Length);
			AssertContainsExactElementsInAnyOrder("The 2 CL SupportingDocuments after processing have the correct CSI_Codes for entryLine 3" + entryLine3.CL_LineNumber, new ZString[] { "X005", "X006" }, clSupDocsEntryLine3.Select(x => x.CSI_Code).ToArray());
		});
	}

	public void TestProcessAcceptedC44CertificadoAportadoOrganismo()
	{
		entryLine1.AddEntryLineDocument<SupportingDocument>("X001", "ES3600000001");
		entryLine1.AddEntryLineDocument<SupportingDocument>("X002", "ES3600000002");
		entryLine2.AddEntryLineDocument<SupportingDocument>("X003", "ES3600000003");
		entryLine2.AddEntryLineDocument<SupportingDocument>("X004", "ES3600000004");
		entryLine3.AddEntryLineDocument<SupportingDocument>("X005", "ES3600000005");
		entryLine3.AddEntryLineDocument<SupportingDocument>("X006", "ES3600000006");

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileCertificadoAportadoOrganismo(), InterchangeID);

		ProcessMessageForTest(message);
		AssertAcceptedAEAT(message, entryHeader, entryStatusCode: EntryStatusCodes.Cleared);

		CombineAssertions(() =>
		{
			var clSupDocsEntryLine1 = GetCLSupportingDocuments(entryLine1);
			AssertEquals("There are 3 CL SupportingDocuments after processing for entryLine 1 " + entryLine1.CL_LineNumber, 3, clSupDocsEntryLine1.Length);
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_Codes for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "N380", "N705", "C055" }, clSupDocsEntryLine1.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_ReferenceNumber for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "14521", "454545", "ES0039283" }, clSupDocsEntryLine1.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_UnitOfQuantity for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "KGM", "DT", ZString.Empty }, clSupDocsEntryLine1.Select(x => x.CSI_UnitOfQuantity).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_Quantity for entryLine 1" + entryLine1.CL_LineNumber, new ZDecimal[] { 27.444, 20.222, ZDecimal.Zero }, clSupDocsEntryLine1.Select(x => x.CSI_Quantity).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_DateOfIssue for entryLine 1" + entryLine1.CL_LineNumber, new ZDateTime[] { new ZDateTime(2019, 01, 28, 00, 00, 00), new ZDateTime(2020, 01, 28, 00, 00, 00), ZDateTime.Empty }, clSupDocsEntryLine1.Select(x => x.CSI_DateOfIssue).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_DateOfExpiry for entryLine 1" + entryLine1.CL_LineNumber, new ZDateTime[] { new ZDateTime(2022, 04, 15, 00, 00, 00), ZDateTime.Empty, ZDateTime.Empty }, clSupDocsEntryLine1.Select(x => x.CSI_DateOfExpiry).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_Status for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "ACC", "ACC", "ACC" }, clSupDocsEntryLine1.Select(x => x.CSI_Status).ToArray());

			var clSupDocsEntryLine2 = GetCLSupportingDocuments(entryLine2);
			AssertEquals("There are 2 CL SupportingDocuments after processing for entryLine 2" + entryLine2.CL_LineNumber, 2, clSupDocsEntryLine2.Length);
			AssertContainsExactElementsInAnyOrder("The 2 CL SupportingDocuments after processing have the correct CSI_Codes for entryLine 2" + entryLine2.CL_LineNumber, new ZString[] { "N381", "N706" }, clSupDocsEntryLine2.Select(x => x.CSI_Code).ToArray());

			var clSupDocsEntryLine3 = GetCLSupportingDocuments(entryLine3);
			AssertEquals("There are 2 CL SupportingDocuments (the original ones) after processing for entryLine 3" + entryLine3.CL_LineNumber, 2, clSupDocsEntryLine3.Length);
			AssertContainsExactElementsInAnyOrder("The 2 CL SupportingDocuments after processing have the correct CSI_Codes for entryLine 3" + entryLine3.CL_LineNumber, new ZString[] { "X005", "X006" }, clSupDocsEntryLine3.Select(x => x.CSI_Code).ToArray());
		});
	}

	public void TestProcessAcceptedC44CertificadoAportadoOperador()
	{
		entryLine1.AddEntryLineDocument<SupportingDocument>("X001", "ES3600000001");
		entryLine1.AddEntryLineDocument<SupportingDocument>("X002", "ES3600000002");
		entryLine2.AddEntryLineDocument<SupportingDocument>("X003", "ES3600000003");
		entryLine2.AddEntryLineDocument<SupportingDocument>("X004", "ES3600000004");
		entryLine3.AddEntryLineDocument<SupportingDocument>("X005", "ES3600000005");
		entryLine3.AddEntryLineDocument<SupportingDocument>("X006", "ES3600000006");

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileCertificadoAportadoOperador(), InterchangeID);

		ProcessMessageForTest(message);
		AssertAcceptedAEAT(message, entryHeader, entryStatusCode: EntryStatusCodes.Cleared);

		CombineAssertions(() =>
		{
			var clSupDocsEntryLine1 = GetCLSupportingDocuments(entryLine1);
			AssertEquals("There are 3 CL SupportingDocuments after processing for entryLine 1 " + entryLine1.CL_LineNumber, 3, clSupDocsEntryLine1.Length);
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_Codes for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "N380", "N705", "C055" }, clSupDocsEntryLine1.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_ReferenceNumber for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "14521", "454545", "ES0039283" }, clSupDocsEntryLine1.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_UnitOfQuantity for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "KGM", "DT", ZString.Empty }, clSupDocsEntryLine1.Select(x => x.CSI_UnitOfQuantity).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_Quantity for entryLine 1" + entryLine1.CL_LineNumber, new ZDecimal[] { 27.444, 20.222, ZDecimal.Zero }, clSupDocsEntryLine1.Select(x => x.CSI_Quantity).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_DateOfIssue for entryLine 1" + entryLine1.CL_LineNumber, new ZDateTime[] { new ZDateTime(2019, 01, 28, 00, 00, 00), new ZDateTime(2020, 01, 28, 00, 00, 00), ZDateTime.Empty }, clSupDocsEntryLine1.Select(x => x.CSI_DateOfIssue).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_DateOfExpiry for entryLine 1" + entryLine1.CL_LineNumber, new ZDateTime[] { new ZDateTime(2022, 04, 15, 00, 00, 00), ZDateTime.Empty, ZDateTime.Empty }, clSupDocsEntryLine1.Select(x => x.CSI_DateOfExpiry).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_Status for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "ACC", "ACC", "ACC" }, clSupDocsEntryLine1.Select(x => x.CSI_Status).ToArray());

			var clSupDocsEntryLine2 = GetCLSupportingDocuments(entryLine2);
			AssertEquals("There are 2 CL SupportingDocuments after processing for entryLine 2" + entryLine2.CL_LineNumber, 2, clSupDocsEntryLine2.Length);
			AssertContainsExactElementsInAnyOrder("The 2 CL SupportingDocuments after processing have the correct CSI_Codes for entryLine 2" + entryLine2.CL_LineNumber, new ZString[] { "N381", "N706" }, clSupDocsEntryLine2.Select(x => x.CSI_Code).ToArray());

			var clSupDocsEntryLine3 = GetCLSupportingDocuments(entryLine3);
			AssertEquals("There are 2 CL SupportingDocuments (the original ones) after processing for entryLine 3" + entryLine3.CL_LineNumber, 2, clSupDocsEntryLine3.Length);
			AssertContainsExactElementsInAnyOrder("The 2 CL SupportingDocuments after processing have the correct CSI_Codes for entryLine 3" + entryLine3.CL_LineNumber, new ZString[] { "X005", "X006" }, clSupDocsEntryLine3.Select(x => x.CSI_Code).ToArray());
		});
	}

	public void TestProcessAcceptedC44DocumentoJustificativo()
	{
		entryLine1.AddEntryLineDocument<SupportingDocument>("X001", "ES3600000001");
		entryLine1.AddEntryLineDocument<SupportingDocument>("X002", "ES3600000002");
		entryLine2.AddEntryLineDocument<SupportingDocument>("X003", "ES3600000003");
		entryLine2.AddEntryLineDocument<SupportingDocument>("X004", "ES3600000004");
		entryLine3.AddEntryLineDocument<SupportingDocument>("X005", "ES3600000005");
		entryLine3.AddEntryLineDocument<SupportingDocument>("X006", "ES3600000006");

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileDocumentoJustificativo(), InterchangeID);

		ProcessMessageForTest(message);
		AssertAcceptedAEAT(message, entryHeader, entryStatusCode: EntryStatusCodes.Cleared);

		CombineAssertions(() =>
		{
			var clSupDocsEntryLine1 = GetCLSupportingDocuments(entryLine1);
			AssertEquals("There are 3 CL SupportingDocuments after processing for entryLine 1 " + entryLine1.CL_LineNumber, 3, clSupDocsEntryLine1.Length);
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_Codes for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "N380", "N705", "C055" }, clSupDocsEntryLine1.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_ReferenceNumber for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "14521", "454545", "ES0039283" }, clSupDocsEntryLine1.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_UnitOfQuantity for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "KGM", "DT", ZString.Empty }, clSupDocsEntryLine1.Select(x => x.CSI_UnitOfQuantity).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_Quantity for entryLine 1" + entryLine1.CL_LineNumber, new ZDecimal[] { 27.444, 20.222, ZDecimal.Zero }, clSupDocsEntryLine1.Select(x => x.CSI_Quantity).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_DateOfIssue for entryLine 1" + entryLine1.CL_LineNumber, new ZDateTime[] { new ZDateTime(2019, 01, 28, 00, 00, 00), new ZDateTime(2020, 01, 28, 00, 00, 00), ZDateTime.Empty }, clSupDocsEntryLine1.Select(x => x.CSI_DateOfIssue).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_DateOfExpiry for entryLine 1" + entryLine1.CL_LineNumber, new ZDateTime[] { new ZDateTime(2022, 04, 15, 00, 00, 00), ZDateTime.Empty, ZDateTime.Empty }, clSupDocsEntryLine1.Select(x => x.CSI_DateOfExpiry).ToArray());
			AssertContainsExactElementsInAnyOrder("The 3 CL SupportingDocuments after processing have the correct CSI_Status for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "ACC", "ACC", "ACC" }, clSupDocsEntryLine1.Select(x => x.CSI_Status).ToArray());

			var clSupDocsEntryLine2 = GetCLSupportingDocuments(entryLine2);
			AssertEquals("There are 2 CL SupportingDocuments after processing for entryLine 2" + entryLine2.CL_LineNumber, 2, clSupDocsEntryLine2.Length);
			AssertContainsExactElementsInAnyOrder("The 2 CL SupportingDocuments after processing have the correct CSI_Codes for entryLine 2" + entryLine2.CL_LineNumber, new ZString[] { "N381", "N706" }, clSupDocsEntryLine2.Select(x => x.CSI_Code).ToArray());
			AssertContainsExactElementsInAnyOrder("The 2 CL SupportingDocuments after processing have the correct CSI_Status for entryLine 2" + entryLine2.CL_LineNumber, new ZString[] { "CAN", "CAN" }, clSupDocsEntryLine2.Select(x => x.CSI_Status).ToArray());

			var clSupDocsEntryLine3 = GetCLSupportingDocuments(entryLine3);
			AssertEquals("There are 2 CL SupportingDocuments (the original ones) after processing for entryLine 3" + entryLine3.CL_LineNumber, 2, clSupDocsEntryLine3.Length);
			AssertContainsExactElementsInAnyOrder("The 2 CL SupportingDocuments after processing have the correct CSI_Codes for entryLine 3" + entryLine3.CL_LineNumber, new ZString[] { "X005", "X006" }, clSupDocsEntryLine3.Select(x => x.CSI_Code).ToArray());
		});
	}

	public void TestProcessAcceptedAllC44Documents()
	{
		entryLine1.AddEntryLineDocument<SupportingDocument>("X001", "ES3600000001");
		entryLine1.AddEntryLineDocument<SupportingDocument>("X002", "ES3600000002");
		entryLine2.AddEntryLineDocument<SupportingDocument>("X003", "ES3600000003");
		entryLine2.AddEntryLineDocument<SupportingDocument>("X004", "ES3600000004");
		entryLine3.AddEntryLineDocument<SupportingDocument>("X005", "ES3600000005");
		entryLine3.AddEntryLineDocument<SupportingDocument>("X006", "ES3600000006");

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileDocuments(), InterchangeID);

		ProcessMessageForTest(message);
		AssertAcceptedAEAT(message, entryHeader, entryStatusCode: EntryStatusCodes.Cleared);

		CombineAssertions(() =>
		{
			var clSupDocsEntryLine1 = GetCLSupportingDocuments(entryLine1);
			AssertEquals("There are 4 CL SupportingDocuments after processing for entryLine 1 " + entryLine1.CL_LineNumber, 4, clSupDocsEntryLine1.Length);
			AssertContainsExactElementsInAnyOrder("The 4 CL SupportingDocuments after processing have the correct CSI_Codes for entryLine 1" + entryLine1.CL_LineNumber, new ZString[] { "N380", "N705", "C055", "N706" }, clSupDocsEntryLine1.Select(x => x.CSI_Code).ToArray());

			var clSupDocsEntryLine2 = GetCLSupportingDocuments(entryLine2);
			AssertEquals("There are 2 CL SupportingDocuments (the original ones) after processing for entryLine 2" + entryLine2.CL_LineNumber, 2, clSupDocsEntryLine2.Length);
			AssertContainsExactElementsInAnyOrder("The 2 CL SupportingDocuments after processing have the correct CSI_Codes for entryLine 2" + entryLine2.CL_LineNumber, new ZString[] { "X003", "X004" }, clSupDocsEntryLine2.Select(x => x.CSI_Code).ToArray());

			var clSupDocsEntryLine3 = GetCLSupportingDocuments(entryLine3);
			AssertEquals("There are 2 CL SupportingDocuments (the original ones) after processing for entryLine 3" + entryLine3.CL_LineNumber, 2, clSupDocsEntryLine3.Length);
			AssertContainsExactElementsInAnyOrder("The 2 CL SupportingDocuments after processing have the correct CSI_Codes for entryLine 3" + entryLine3.CL_LineNumber, new ZString[] { "X005", "X006" }, clSupDocsEntryLine3.Select(x => x.CSI_Code).ToArray());
		});
	}

	public void TestProcessAcceptedMessageChangesFeesAEAT()
	{
		entryHeader.TotalAmount = 380;
		AddFeesToEntryLine();

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitProcedureA_AEAT(), InterchangeID);

		ProcessMessageForTest(message);

		var expectedMessageInterpretation =
			"<H3>Accepted Declaration</H3>" +
			"<br><table border=\"0\"></table>" +
			"<table border=\"0\"><tr><td>Export Doc. (MRN):</td><td>&nbsp;&nbsp;</td><td>20EXP0999930006184</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>TEST444444444444</td></tr>" +
			"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>04-11-2020, 00:00:00</td></tr>" +
			"<tr><td>Import cert. (CSV):</td><td>&nbsp;&nbsp;</td><td>CRTF444444444444</td></tr></table><br>" +
			"<H2>Management data</H2><br>" +
			"<table border=\"0\"><tr><td>Administration:</td><td>&nbsp;&nbsp;</td><td>AEAT</td></tr>" +
			"<tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>DUA - Declaración Completa de Importación</td></tr>" +
			"<tr><td>Customs Clearance Status:</td><td>&nbsp;&nbsp;</td><td>L - Accepted with Customs Clearance</td></tr>" +
			"<tr><td>Unfinished Pendencies:</td><td>&nbsp;&nbsp;</td><td>N - No tiene</td></tr></table><br>" +
			"<br><H2>Taxes and fees data</H2><br>" +
			"<table border=\"0\"><tr><td>Total:</td><td>&nbsp;&nbsp;</td><td>380</td></tr>" +
			"<tr><td>Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>310</td></tr>" +
			"<tr><td>Total Deferred VAT:</td><td>&nbsp;&nbsp;</td><td>35</td></tr>" +
			"<tr><td>Clearance Guarantee VAT Exemption:</td><td>&nbsp;&nbsp;</td><td>80</td></tr>" +
			"<tr><td>Real Clearance Guarantee:</td><td>&nbsp;&nbsp;</td><td>300</td></tr>" +
			"<tr><td>Pendency Guarantee VAT Exemption:</td><td>&nbsp;&nbsp;</td><td>10</td></tr>" +
			"<tr><td>Real Pendency Guarantee:</td><td>&nbsp;&nbsp;</td><td>10</td></tr></table><br>" +
			"<H2>Payment information</H2><br>" +
			"<table border=\"0\"><tr><td>Payment Proof Number:</td><td>&nbsp;&nbsp;</td><td>1234</td></tr>" +
			"<tr><td>Payment date limit:</td><td>&nbsp;&nbsp;</td><td>07-06-2021</td></tr></table><br>" +
			"<br><H2>Guarantees</H2><br>" +
			"<table border=\"0\"><tr><td>Accounting Status:</td><td>&nbsp;&nbsp;</td><td>CO - Contractado</td></tr>" +
			"<tr><td>Guarantee Status:</td><td>&nbsp;&nbsp;</td><td>Not Written Off (status not IT)</td></tr></table><br>" +
			"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
			"<thead><tr class=\"tableheadings\"><th>Customs</th><th>GRN</th><th>Real Debt</th><th>Potential Debt</th><th>Undetermined Real Debt</th></tr></thead>" +
			"<tr><td>AEAT</td><td>16ESAGL9990000096</td><td>387.51</td><td>5</td><td>30</td></tr>" +
			"<tr><td>AEAT</td><td>16ESAGL9990000097</td><td>350</td><td>5</td><td>25</td></tr>" +
			"</table><br><br><H2>Taxes and fees response (Spanish Customs)</H2><br>" +
			"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Item</th><th>Type</th><th>MAX/MIN Rate</th><th>Base Amount</th><th>Tax Rate</th><th>Total Amount</th><th>Total Guaranteed Amount</th></tr></thead>" +
			"<tr><td>1</td><td>A00</td><td>&nbsp;</td><td>100.650</td><td>2.700000 %</td><td>2.72</td><td>2.72</td></tr>" +
			"<tr><td>1</td><td>C00</td><td>MA</td><td>104.950</td><td>21.000000 %</td><td>22.04</td><td>22.04</td></tr>" +
			"<tr><td>2</td><td>C00</td><td>&nbsp;</td><td>1219.350</td><td>17.600000 %</td><td>214.61</td><td>214.61</td></tr>" +
			"<tr><td>2</td><td>B00</td><td>&nbsp;</td><td>1481.380</td><td>10.000000 €/KN</td><td>148.14</td><td>148.14</td></tr>" +
			"<tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>Total: </td><td>387.51</td><td>387.51</td></tr></table><br>";
		AssertAcceptedAEAT(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, entryStatusCode: EntryStatusCodes.Cleared);

		CombineAssertions("Test EntryLines fees", () =>
		{
			AssertEquals("EntryLine1.Fees count", 3, entryLine1.Fees.Count);

			AssertEquals("EntryLine1.Fee[0].CF_ChargeType", "300", entryLine1.Fees[0].CF_ChargeType);
			AssertEquals("EntryLine1.Fee[0].CF_BaseValue", 200.650M, entryLine1.Fees[0].CF_BaseValue);
			AssertEquals("EntryLine1.Fee[0].CF_Rate", 3.700000M, entryLine1.Fees[0].CF_Rate);
			AssertEquals("EntryLine1.Fee[0].MaxMin", "MA", entryLine1.Fees[0].MaxMin);
			AssertEquals("EntryLine1.Fee[0].G4_RateDuty", ZString.Empty, entryLine1.Fees[0].G4_RateDuty);
			AssertEquals("EntryLine1.Fee[0].CF_ChargeAmount", 3.72M, entryLine1.Fees[0].CF_ChargeAmount);
			AssertEquals("EntryLine1.Fee[0].CF_RateOverrideReasonCode", "ADD", entryLine1.Fees[0].CF_RateOverrideReasonCode);

			AssertEquals("EntryLine1.Fee[1].CF_ChargeType", "A00", entryLine1.Fees[1].CF_ChargeType);
			AssertEquals("EntryLine1.Fee[1].CF_BaseValue", 100.650M, entryLine1.Fees[1].CF_BaseValue);
			AssertEquals("EntryLine1.Fee[1].CF_Rate", 2.700000M, entryLine1.Fees[1].CF_Rate);
			AssertEquals("EntryLine1.Fee[1].MaxMin", ZString.Empty, entryLine1.Fees[1].MaxMin);
			AssertEquals("EntryLine1.Fee[1].G4_RateDuty", "%", entryLine1.Fees[1].G4_RateDuty);
			AssertEquals("EntryLine1.Fee[1].CF_ChargeAmount", 2.72M, entryLine1.Fees[1].CF_ChargeAmount);
			AssertEquals("EntryLine1.Fee[1].CF_RateOverrideReasonCode", "OVR", entryLine1.Fees[1].CF_RateOverrideReasonCode);

			AssertEquals("EntryLine1.Fee[2].CF_ChargeType", "C00", entryLine1.Fees[2].CF_ChargeType);
			AssertEquals("EntryLine1.Fee[2].CF_BaseValue", 104.950M, entryLine1.Fees[2].CF_BaseValue);
			AssertEquals("EntryLine1.Fee[2].CF_Rate", 21.000000M, entryLine1.Fees[2].CF_Rate);
			AssertEquals("EntryLine1.Fee[2].MaxMin", "MA", entryLine1.Fees[2].MaxMin);
			AssertEquals("EntryLine1.Fee[2].G4_RateDuty", "%", entryLine1.Fees[2].G4_RateDuty);
			AssertEquals("EntryLine1.Fee[2].CF_ChargeAmount", 22.04M, entryLine1.Fees[2].CF_ChargeAmount);
			AssertEquals("EntryLine1.Fee[2].CF_RateOverrideReasonCode", "OVR", entryLine1.Fees[2].CF_RateOverrideReasonCode);

			AssertEquals("EntryLine2.Fees count", 2, entryLine2.Fees.Count);

			AssertEquals("EntryLine2.Fee[0].CF_ChargeType", "C00", entryLine2.Fees[0].CF_ChargeType);
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

	public void TestProcessAcceptedMessageChangesFeesATC()
	{
		entryHeader.TotalAmount = 380;
		AddFeesToEntryLine();

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitProcedureA_ATC(), InterchangeID);

		ProcessMessageForTest(message);

		var expectedMessageInterpretation =
				"<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"></table>" +
				"<table border=\"0\"><tr><td>Export Doc. (MRN):</td><td>&nbsp;&nbsp;</td><td>20EXP0999930006184</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>TEST444444444444</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>04-11-2020, 00:00:00</td></tr>" +
				"<tr><td>Import cert. (CSV):</td><td>&nbsp;&nbsp;</td><td>CRTF444444444444</td></tr></table><br>" +
				"<H2>Management data</H2><br>" +
				"<table border=\"0\"><tr><td>Administration:</td><td>&nbsp;&nbsp;</td><td>ATC</td></tr>" +
				"<tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>DUA - Declaración Completa de Importación</td></tr>" +
				"<tr><td>Customs Clearance Status:</td><td>&nbsp;&nbsp;</td><td>L - Accepted with Customs Clearance</td></tr>" +
				"<tr><td>Unfinished Pendencies:</td><td>&nbsp;&nbsp;</td><td>N - No tiene</td></tr></table><br>" +
				"<br><H2>Taxes and fees data</H2><br>" +
				"<table border=\"0\"><tr><td>Total:</td><td>&nbsp;&nbsp;</td><td>380</td></tr>" +
				"<tr><td>Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>310</td></tr>" +
				"<tr><td>Total Deferred VAT:</td><td>&nbsp;&nbsp;</td><td>35</td></tr>" +
				"<tr><td>Clearance Guarantee VAT Exemption:</td><td>&nbsp;&nbsp;</td><td>80</td></tr>" +
				"<tr><td>Real Clearance Guarantee:</td><td>&nbsp;&nbsp;</td><td>300</td></tr>" +
				"<tr><td>Pendency Guarantee VAT Exemption:</td><td>&nbsp;&nbsp;</td><td>10</td></tr>" +
				"<tr><td>Real Pendency Guarantee:</td><td>&nbsp;&nbsp;</td><td>10</td></tr></table><br>" +
				"<H2>Payment information</H2><br>" +
				"<table border=\"0\"><tr><td>Payment Proof Number:</td><td>&nbsp;&nbsp;</td><td>JUSTPAGOATC</td></tr>" +
				"<tr><td>Payment date limit:</td><td>&nbsp;&nbsp;</td><td>07-06-2021</td></tr></table><br>" +
				"<br><H2>Guarantees</H2><br>" +
				"<table border=\"0\"><tr><td>Accounting Status:</td><td>&nbsp;&nbsp;</td><td>CO - Contractado</td></tr>" +
				"<tr><td>Guarantee Status:</td><td>&nbsp;&nbsp;</td><td>Not Written Off (status not IT)</td></tr></table><br>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
				"<thead><tr class=\"tableheadings\"><th>Customs</th><th>GRN</th><th>Real Debt</th><th>Potential Debt</th><th>Undetermined Real Debt</th></tr></thead>" +
				"<tr><td>ATC</td><td>16ESAGL9990000096</td><td>387.51</td><td>5</td><td>30</td></tr>" +
				"<tr><td>ATC</td><td>16ESAGL9990000097</td><td>350</td><td>5</td><td>25</td></tr>" +
				"</table><br><br><H2>Taxes and fees response (Spanish Customs)</H2><br>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Item</th><th>Type</th><th>MAX/MIN Rate</th><th>Base Amount</th><th>Tax Rate</th><th>Total Amount</th><th>Total Guaranteed Amount</th></tr></thead>" +
				"<tr><td>1</td><td>3IG</td><td>&nbsp;</td><td>100.650</td><td>2.700000 %</td><td>2.72</td><td>2.72</td></tr>" +
				"<tr><td>1</td><td>400</td><td>MA</td><td>104.950</td><td>21.000000 %</td><td>22.04</td><td>22.04</td></tr>" +
				"<tr><td>2</td><td>300</td><td>&nbsp;</td><td>1219.350</td><td>17.600000 %</td><td>214.61</td><td>214.61</td></tr>" +
				"<tr><td>2</td><td>4IG</td><td>&nbsp;</td><td>1481.380</td><td>10.000000 €/KN</td><td>148.14</td><td>148.14</td></tr>" +
				"<tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>Total: </td><td>387.51</td><td>387.51</td></tr></table><br>";
		AssertAcceptedATC(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, entryStatusCode: EntryStatusCodes.Cleared, atcLimitPaymentDate: AtcLimitPaymentDate);

		CombineAssertions("Test EntryLines fees", () =>
		{
			AssertEquals("EntryLine1.Fees count", 3, entryLine1.Fees.Count);

			AssertEquals("EntryLine1.Fee[0].CF_ChargeType", "A00", entryLine1.Fees[0].CF_ChargeType);
			AssertEquals("EntryLine1.Fee[0].CF_BaseValue", 200.650M, entryLine1.Fees[0].CF_BaseValue);
			AssertEquals("EntryLine1.Fee[0].CF_Rate", 3.700000M, entryLine1.Fees[0].CF_Rate);
			AssertEquals("EntryLine1.Fee[0].MaxMin", "MA", entryLine1.Fees[0].MaxMin);
			AssertEquals("EntryLine1.Fee[0].G4_RateDuty", ZString.Empty, entryLine1.Fees[0].G4_RateDuty);
			AssertEquals("EntryLine1.Fee[0].CF_ChargeAmount", 3.72M, entryLine1.Fees[0].CF_ChargeAmount);
			AssertEquals("EntryLine1.Fee[0].CF_RateOverrideReasonCode", "ADD", entryLine1.Fees[0].CF_RateOverrideReasonCode);

			AssertEquals("EntryLine1.Fee[1].CF_ChargeType", "3IG", entryLine1.Fees[1].CF_ChargeType);
			AssertEquals("EntryLine1.Fee[1].CF_BaseValue", 100.650M, entryLine1.Fees[1].CF_BaseValue);
			AssertEquals("EntryLine1.Fee[1].CF_Rate", 2.700000M, entryLine1.Fees[1].CF_Rate);
			AssertEquals("EntryLine1.Fee[1].MaxMin", ZString.Empty, entryLine1.Fees[1].MaxMin);
			AssertEquals("EntryLine1.Fee[1].G4_RateDuty", "%", entryLine1.Fees[1].G4_RateDuty);
			AssertEquals("EntryLine1.Fee[1].CF_ChargeAmount", 2.72M, entryLine1.Fees[1].CF_ChargeAmount);
			AssertEquals("EntryLine1.Fee[1].CF_RateOverrideReasonCode", "OVR", entryLine1.Fees[1].CF_RateOverrideReasonCode);

			AssertEquals("EntryLine1.Fee[2].CF_ChargeType", "400", entryLine1.Fees[2].CF_ChargeType);
			AssertEquals("EntryLine1.Fee[2].CF_BaseValue", 104.950M, entryLine1.Fees[2].CF_BaseValue);
			AssertEquals("EntryLine1.Fee[2].CF_Rate", 21.000000M, entryLine1.Fees[2].CF_Rate);
			AssertEquals("EntryLine1.Fee[2].MaxMin", "MA", entryLine1.Fees[2].MaxMin);
			AssertEquals("EntryLine1.Fee[2].G4_RateDuty", "%", entryLine1.Fees[2].G4_RateDuty);
			AssertEquals("EntryLine1.Fee[2].CF_ChargeAmount", 22.04M, entryLine1.Fees[2].CF_ChargeAmount);
			AssertEquals("EntryLine1.Fee[2].CF_RateOverrideReasonCode", "OVR", entryLine1.Fees[2].CF_RateOverrideReasonCode);

			AssertEquals("EntryLine2.Fees count", 2, entryLine2.Fees.Count);

			AssertEquals("EntryLine2.Fee[0].CF_ChargeType", "300", entryLine2.Fees[0].CF_ChargeType);
			AssertEquals("EntryLine2.Fee[0].CF_BaseValue", 1219.350M, entryLine2.Fees[0].CF_BaseValue);
			AssertEquals("EntryLine2.Fee[0].CF_Rate", 17.600000M, entryLine2.Fees[0].CF_Rate);
			AssertEquals("EntryLine2.Fee[0].MaxMin", ZString.Empty, entryLine2.Fees[0].MaxMin);
			AssertEquals("EntryLine2.Fee[0].G4_RateDuty", "%", entryLine2.Fees[0].G4_RateDuty);
			AssertEquals("EntryLine2.Fee[0].CF_ChargeAmount", 214.61M, entryLine2.Fees[0].CF_ChargeAmount);
			AssertEquals("EntryLine2.Fee[0].CF_RateOverrideReasonCode", "OVR", entryLine2.Fees[0].CF_RateOverrideReasonCode);

			AssertEquals("EntryLine2.Fee[1].CF_ChargeType", "4IG", entryLine2.Fees[1].CF_ChargeType);
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

	public void TestProcessDUAAcceptedGreenCircuitMessageWithoutProcedureWithEntryInstructionB()
	{
		entryHeader.TotalAmount = 380;

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitNoProcedure(), InterchangeID);

		ProcessMessageForTest(message);

		var expectedMessageInterpretation =
				"<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"></table>" +
				"<table border=\"0\"><tr><td>Export Doc. (MRN):</td><td>&nbsp;&nbsp;</td><td>20EXP0999930006184</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>TEST444444444444</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>04-11-2020, 00:00:00</td></tr>" +
				"<tr><td>Import cert. (CSV):</td><td>&nbsp;&nbsp;</td><td>CRTF444444444444</td></tr></table><br>" +
				"<H2>Management data</H2><br>" +
				"<table border=\"0\"><tr><td>Administration:</td><td>&nbsp;&nbsp;</td><td>AEAT</td></tr>" +
				"<tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>DUA - Declaración Completa de Importación</td></tr>" +
				"<tr><td>Customs Clearance Status:</td><td>&nbsp;&nbsp;</td><td>L - Accepted with Customs Clearance</td></tr>" +
				"<tr><td>Unfinished Pendencies:</td><td>&nbsp;&nbsp;</td><td>N - No tiene</td></tr></table><br>" +
				"<br><H2>Taxes and fees data</H2><br>" +
				"<table border=\"0\"><tr><td>Total:</td><td>&nbsp;&nbsp;</td><td>380</td></tr>" +
				"<tr><td>Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>310</td></tr>" +
				"<tr><td>Total Deferred VAT:</td><td>&nbsp;&nbsp;</td><td>35</td></tr>" +
				"<tr><td>Clearance Guarantee VAT Exemption:</td><td>&nbsp;&nbsp;</td><td>80</td></tr>" +
				"<tr><td>Real Clearance Guarantee:</td><td>&nbsp;&nbsp;</td><td>300</td></tr>" +
				"<tr><td>Pendency Guarantee VAT Exemption:</td><td>&nbsp;&nbsp;</td><td>10</td></tr>" +
				"<tr><td>Real Pendency Guarantee:</td><td>&nbsp;&nbsp;</td><td>10</td></tr></table><br>" +
				"<H2>Payment information</H2><br>" +
				"<table border=\"0\"><tr><td>Payment Proof Number:</td><td>&nbsp;&nbsp;</td><td>1234</td></tr>" +
				"<tr><td>Payment date limit:</td><td>&nbsp;&nbsp;</td><td>07-06-2021</td></tr></table><br>" +
				"<br><H2>Guarantees</H2><br>" +
				"<table border=\"0\"><tr><td>Accounting Status:</td><td>&nbsp;&nbsp;</td><td>CO - Contractado</td></tr>" +
				"<tr><td>Guarantee Status:</td><td>&nbsp;&nbsp;</td><td>Not Written Off (status not IT)</td></tr></table><br>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
				"<thead><tr class=\"tableheadings\"><th>Customs</th><th>GRN</th><th>Real Debt</th><th>Potential Debt</th><th>Undetermined Real Debt</th></tr></thead>" +
				"<tr><td>AEAT</td><td>16ESAGL9990000096</td><td>387.51</td><td>5</td><td>30</td></tr>" +
				"<tr><td>AEAT</td><td>16ESAGL9990000097</td><td>350</td><td>5</td><td>25</td></tr></table><br>" +
				"<br><H2>Required Certificates</H2><br><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead>" +
				"<tr class=\"tableheadings\"><th>Item</th><th>Measure</th><th>Agency</th><th>Documents</th></tr></thead>" +
				"<tr><td>2</td><td>SNM</td><td>SIF05 -  Sanidad Exterior - M&#186; Sanidad</td><td>N851, C085</td></tr>" +
				"<tr><td>2</td><td>SNM</td><td>SIF06 -  Sanidad Inetrior - M&#186; Sanidad</td><td>C657, C678</td></tr>" +
				"</table>";
		AssertAcceptedAEAT(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation);
	}

	public void TestProcessMessageWithoutProcedureWithWrongEntryInstruction()
	{
		entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.T2L;

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitNoProcedure(), InterchangeID);

		ProcessMessageForTest(message);
		var expectedMessageInterpretation =
			string.Format("<H3>Processor Failure</H3><br>" +
			"<H4>Failure: Unable to read message text from message (Number:{0}, Type:{1}, Sub:{2}, Ref:{3}); message status set to Failed.</H4>" +
			"<H4>Exception: Unexpected response C012ProcedimientoSolicitado/Entry Instruction value T2L.</H4>", message.EM_MessageNum, MessageType, message.EM_MessageSubType, entryHeader.CH_BGMReference);
		AssertFailure(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation);

		CombineAssertions(() =>
		{
			var concatenatedUserLogStrings = GetAllConcatenatedUserLogStrings();
			AssertContains("logger", "Unable to read message text from message ", concatenatedUserLogStrings);
			AssertContains("logger exception", "Unexpected response C012ProcedimientoSolicitado/Entry Instruction value T2L.", concatenatedUserLogStrings);
		});
	}

	public void TestProcessMessageWithWrongProcedure()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitWrongProcedure(), InterchangeID);

		ProcessMessageForTest(message);
		var expectedMessageInterpretation =
			string.Format("<H3>Processor Failure</H3><br>" +
			"<H4>Failure: Unable to read message text from message (Number:{0}, Type:{1}, Sub:{2}, Ref:{3}); message status set to Failed.</H4>" +
			"<H4>Exception: Unexpected response C012ProcedimientoSolicitado/Entry Instruction value T.</H4>", message.EM_MessageNum, MessageType, message.EM_MessageSubType, entryHeader.CH_BGMReference);
		AssertFailure(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation);

		CombineAssertions(() =>
		{
			var concatenatedUserLogStrings = GetAllConcatenatedUserLogStrings();
			AssertContains("logger", "Unable to read message text from message ", concatenatedUserLogStrings);
			AssertContains("logger exception", "Unexpected response C012ProcedimientoSolicitado/Entry Instruction value T.", concatenatedUserLogStrings);
		});
	}

	public void TestProcessPDSAcceptedMessage_WithWrongApplicationReference()
	{
		var message = CreateNewEDIMessage("AA", AcceptanceTestFilePDSResponse, InterchangeID);

		ProcessMessageForTest(message);

		CombineAssertions(() =>
		{
			AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("EM_MessageSubType", "ACC", message.EM_MessageSubType);
			AssertEquals("CH_Status", "RCV", entryHeader.CH_Status);
			AssertEquals("EM_MessageNum", "TARIC20201020141554350002", message.EM_MessageNum);

			AssertEquals("CH_EntryStatus", EntryStatusCodes.PreDeclarationAccepted, entryHeader.CH_EntryStatus);
			AssertEquals("EntryHeaderStatusDescription", entryStatusList.GetDescriptionFromCode(EntryStatusCodes.PreDeclarationAccepted), entryHeader.EntryHeaderStatusDescription);
		});
	}

	[TestDate(2021, 10, 05, 09, 36, 0)]
	public void TestAddGuaranteesTransactions()
	{
		var wrongGuaranteeReference = "16ESAGP9990000096";
		var wrongOtherGuaranteeReference = "16ESAGP9990000097";
		var extraGuaranteeReference = "16ESAGL9990000098";

		AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: true, addOBLTransaction: true);
		AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: true, addOBLTransaction: true, guaranteeReference: wrongGuaranteeReference, otherGuaranteeReference: wrongOtherGuaranteeReference);

		SetUpGuarantee(extraGuaranteeReference, EUGuaranteeTypeList.Codes.TRA, MRNCode, entryHeader.CH_BGMReference, 0m, 2000, 1300, true, true);
		GuaranteesTestHelper.CreateGuaranteeForEntryInstruction(declaration, (entryInstruction.PK, extraGuaranteeReference));
		declaration.Guarantees[6].PW_BondAmount = 1200m;
		AssertEquals(7, declaration.Guarantees.Count);
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileITAccountingStatus(), InterchangeID);
		ProcessMessageForTest(message);
		var expectedMessageInterpretation =
		"<H3>Accepted Declaration</H3>" +
		"<br><table border=\"0\"></table>" +
		"<table border=\"0\"><tr><td>Export Doc. (MRN):</td><td>&nbsp;&nbsp;</td><td>20EXP0999930006184</td></tr></table>" +
		"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>TEST444444444444</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>04-11-2020, 00:00:00</td></tr>" +
				"<tr><td>Import cert. (CSV):</td><td>&nbsp;&nbsp;</td><td>CRTF444444444444</td></tr></table><br>" +
				"<H2>Management data</H2><br>" +
				"<table border=\"0\"><tr><td>Administration:</td><td>&nbsp;&nbsp;</td><td>AEAT</td></tr>" +
				"<tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>DUA - Declaración Completa de Importación</td></tr>" +
				"<tr><td>Customs Clearance Status:</td><td>&nbsp;&nbsp;</td><td>L - Accepted with Customs Clearance</td></tr>" +
				"<tr><td>Unfinished Pendencies:</td><td>&nbsp;&nbsp;</td><td>N - No tiene</td></tr></table><br>" +
				"<br><H2>Taxes and fees data</H2>" +
				"<H3>Warning: Taxes and fees data received differ from sent data</H3><br>" +
				"<table border=\"0\"><tr><td>Total:</td><td>&nbsp;&nbsp;</td><td>380</td></tr>" +
				"<tr><td>Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>310</td></tr>" +
				"<tr><td>Total Deferred VAT:</td><td>&nbsp;&nbsp;</td><td>35</td></tr>" +
				"<tr><td>Clearance Guarantee VAT Exemption:</td><td>&nbsp;&nbsp;</td><td>80</td></tr>" +
				"<tr><td>Real Clearance Guarantee:</td><td>&nbsp;&nbsp;</td><td>300</td></tr>" +
				"<tr><td>Pendency Guarantee VAT Exemption:</td><td>&nbsp;&nbsp;</td><td>10</td></tr>" +
				"<tr><td>Real Pendency Guarantee:</td><td>&nbsp;&nbsp;</td><td>10</td></tr></table><br>" +
				"<H2>Payment information</H2><br>" +
				"<table border=\"0\"><tr><td>Payment Proof Number:</td><td>&nbsp;&nbsp;</td><td>1234</td></tr>" +
				"<tr><td>Payment date limit:</td><td>&nbsp;&nbsp;</td><td>07-06-2021</td></tr></table><br>" +
				"<br><H2>Guarantees</H2><br>" +
				"<table border=\"0\"><tr><td>Accounting Status:</td><td>&nbsp;&nbsp;</td><td>IT - Ingresada Total</td></tr>" +
				"<tr><td>Guarantee Status:</td><td>&nbsp;&nbsp;</td><td>Written Off</td></tr></table><br>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
				"<thead><tr class=\"tableheadings\"><th>Customs</th><th>GRN</th><th>Real Debt</th><th>Potential Debt</th><th>Undetermined Real Debt</th></tr></thead>" +
				"<tr><td>AEAT</td><td>16ESAGL9990000096</td><td>387.51</td><td>5</td><td>30</td></tr>" +
				"<tr><td>AEAT</td><td>17ESAGL9990000097</td><td>350</td><td>5</td><td>25</td></tr></table><br>" +
				"<br><H2>Required Certificates</H2><br><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead>" +
				"<tr class=\"tableheadings\"><th>Item</th><th>Measure</th><th>Agency</th><th>Documents</th></tr></thead>" +
				"<tr><td>2</td><td>SNM</td><td>SIF05 -  Sanidad Exterior - M&#186; Sanidad</td><td>N851, C085</td></tr>" +
				"<tr><td>2</td><td>SNM</td><td>SIF06 -  Sanidad Inetrior - M&#186; Sanidad</td><td>C657, C678</td></tr>" +
				"</table>";
		AssertAcceptedAEAT(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, parallel: true);

		CombineAssertions(() =>
		{
			var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

			var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
			AssertEquals("Original transactions + 2 new transaction for first guarantee (balance + write off)", 15, guarantee1Transactions.Count());
			AssertNewTransaction("First guarantee's transaction", guarantee1Transactions.First(x => x.CPL_Comment.StartsWith(WriteOffTransactionCommentPrefix)), 422.51m, new ZDateTime(2021, 10, 05, 09, 36, 0));

			var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == OtherGuaranteeReference).GetTransactions();
			AssertEquals("Original transactions + 2 new transaction for second guarantee (balance + write off)", 15, guarantee2Transactions.Count());
			AssertNewTransaction("Second guarantee's transaction", guarantee2Transactions.First(x => x.CPL_Comment.StartsWith(WriteOffTransactionCommentPrefix)), 380m, new ZDateTime(2021, 10, 05, 09, 36, 0));

			var guarantee3Transactions = guaranteeHeaderList.First(x => x.CPH_Number == wrongGuaranteeReference).GetTransactions();
			AssertEquals("Original transactions + 1 new transaction for third guarantee (balance)", 14, guarantee3Transactions.Count());
			AssertEquals("No new TRA Write off transaction for third guarantee", false, guarantee3Transactions.Any(x => x.CPL_Comment.StartsWith(WriteOffTransactionCommentPrefix)));

			var guarantee4Transactions = guaranteeHeaderList.First(x => x.CPH_Number == wrongOtherGuaranteeReference).GetTransactions();
			AssertEquals("Original transactions + 1 new transaction for fourth guarantee (balance)", 14, guarantee4Transactions.Count());
			AssertEquals("No new TRA Write off transaction for fourth guarantee", false, guarantee4Transactions.Any(x => x.CPL_Comment.StartsWith(WriteOffTransactionCommentPrefix)));

			var guarantee5Transactions = guaranteeHeaderList.First(x => x.CPH_Number == extraGuaranteeReference).GetTransactions();
			AssertEquals("Original transactions, no transactions added for fifth guarantee", 13, guarantee5Transactions.Count());
			AssertEquals("No new TRA Write off transaction for fifth guarantee", false, guarantee5Transactions.Any(x => x.CPL_Comment.StartsWith(WriteOffTransactionCommentPrefix)));
		});
	}

	public void TestAddGuaranteesTransactions_PositiveAmount()
	{
		SetUpGuarantee(GuaranteeReference, EUGuaranteeTypeList.Codes.IMP, MRNCode, entryHeader.CH_BGMReference, 500m, 1000, 1300, true, true);
		GuaranteesTestHelper.CreateGuaranteeForEntryInstruction(declaration, (entryInstruction.PK, GuaranteeReference));
		declaration.Guarantees[0].PW_BondAmount = 1000m;
		AssertEquals(1, declaration.Guarantees.Count);

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileITAccountingStatus(), InterchangeID);

		ProcessMessageForTest(message);

		var expectedMessageInterpretation =
				"<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"></table>" +
				"<table border=\"0\"><tr><td>Export Doc. (MRN):</td><td>&nbsp;&nbsp;</td><td>20EXP0999930006184</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>TEST444444444444</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>04-11-2020, 00:00:00</td></tr>" +
				"<tr><td>Import cert. (CSV):</td><td>&nbsp;&nbsp;</td><td>CRTF444444444444</td></tr></table><br>" +
				"<H2>Management data</H2><br>" +
				"<table border=\"0\"><tr><td>Administration:</td><td>&nbsp;&nbsp;</td><td>AEAT</td></tr>" +
				"<tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>DUA - Declaración Completa de Importación</td></tr>" +
				"<tr><td>Customs Clearance Status:</td><td>&nbsp;&nbsp;</td><td>L - Accepted with Customs Clearance</td></tr>" +
				"<tr><td>Unfinished Pendencies:</td><td>&nbsp;&nbsp;</td><td>N - No tiene</td></tr></table><br>" +
				"<br><H2>Taxes and fees data</H2>" +
				"<H3>Warning: Taxes and fees data received differ from sent data</H3><br>" +
				"<table border=\"0\"><tr><td>Total:</td><td>&nbsp;&nbsp;</td><td>380</td></tr>" +
				"<tr><td>Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>310</td></tr>" +
				"<tr><td>Total Deferred VAT:</td><td>&nbsp;&nbsp;</td><td>35</td></tr>" +
				"<tr><td>Clearance Guarantee VAT Exemption:</td><td>&nbsp;&nbsp;</td><td>80</td></tr>" +
				"<tr><td>Real Clearance Guarantee:</td><td>&nbsp;&nbsp;</td><td>300</td></tr>" +
				"<tr><td>Pendency Guarantee VAT Exemption:</td><td>&nbsp;&nbsp;</td><td>10</td></tr>" +
				"<tr><td>Real Pendency Guarantee:</td><td>&nbsp;&nbsp;</td><td>10</td></tr></table><br>" +
				"<H2>Payment information</H2><br>" +
				"<table border=\"0\"><tr><td>Payment Proof Number:</td><td>&nbsp;&nbsp;</td><td>1234</td></tr>" +
				"<tr><td>Payment date limit:</td><td>&nbsp;&nbsp;</td><td>07-06-2021</td></tr></table><br>" +
				"<br><H2>Guarantees</H2><br>" +
				"<table border=\"0\"><tr><td>Accounting Status:</td><td>&nbsp;&nbsp;</td><td>IT - Ingresada Total</td></tr>" +
				"<tr><td>Guarantee Status:</td><td>&nbsp;&nbsp;</td><td>Not Written Off (at least one positive balance)</td></tr></table><br>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
				"<thead><tr class=\"tableheadings\"><th>Customs</th><th>GRN</th><th>Real Debt</th><th>Potential Debt</th><th>Undetermined Real Debt</th></tr></thead>" +
				"<tr><td>AEAT</td><td>16ESAGL9990000096</td><td>387.51</td><td>5</td><td>30</td></tr>" +
				"<tr><td>AEAT</td><td>17ESAGL9990000097</td><td>350</td><td>5</td><td>25</td></tr></table><br>" +
				"<br><H2>Required Certificates</H2><br><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead>" +
				"<tr class=\"tableheadings\"><th>Item</th><th>Measure</th><th>Agency</th><th>Documents</th></tr></thead>" +
				"<tr><td>2</td><td>SNM</td><td>SIF05 -  Sanidad Exterior - M&#186; Sanidad</td><td>N851, C085</td></tr>" +
				"<tr><td>2</td><td>SNM</td><td>SIF06 -  Sanidad Inetrior - M&#186; Sanidad</td><td>C657, C678</td></tr>" +
				"</table>";
		AssertAcceptedAEAT(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, parallel: true);

		var expectedError = "Reference 16ESAGL9990000096 has a positive balance of 440 EUR. Please check the existing transactions for this reference and create a manual adjustment if needed.";
		var concatenatedUserLogStrings = GetAllConcatenatedUserLogStrings();
		AssertContains("logger", expectedError, concatenatedUserLogStrings);
	}

	public void TestAddGuaranteesTransactions_StatusNotIT()
	{
		AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: true, addOBLTransaction: true);

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitProcedureB(), InterchangeID);

		ProcessMessageForTest(message);

		var expectedMessageInterpretation =
				"<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"></table>" +
				"<table border=\"0\"><tr><td>Export Doc. (MRN):</td><td>&nbsp;&nbsp;</td><td>20EXP0999930006184</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>TEST444444444444</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>04-11-2020, 00:00:00</td></tr>" +
				"<tr><td>Import cert. (CSV):</td><td>&nbsp;&nbsp;</td><td>CRTF444444444444</td></tr></table><br>" +
				"<H2>Management data</H2><br>" +
				"<table border=\"0\"><tr><td>Administration:</td><td>&nbsp;&nbsp;</td><td>AEAT</td></tr>" +
				"<tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>DUA - Declaración Completa de Importación</td></tr>" +
				"<tr><td>Customs Clearance Status:</td><td>&nbsp;&nbsp;</td><td>L - Accepted with Customs Clearance</td></tr>" +
				"<tr><td>Unfinished Pendencies:</td><td>&nbsp;&nbsp;</td><td>N - No tiene</td></tr></table><br>" +
				"<br><H2>Taxes and fees data</H2>" +
				"<H3>Warning: Taxes and fees data received differ from sent data</H3><br>" +
				"<table border=\"0\"><tr><td>Total:</td><td>&nbsp;&nbsp;</td><td>380</td></tr>" +
				"<tr><td>Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>310</td></tr>" +
				"<tr><td>Total Deferred VAT:</td><td>&nbsp;&nbsp;</td><td>35</td></tr>" +
				"<tr><td>Clearance Guarantee VAT Exemption:</td><td>&nbsp;&nbsp;</td><td>80</td></tr>" +
				"<tr><td>Real Clearance Guarantee:</td><td>&nbsp;&nbsp;</td><td>300</td></tr>" +
				"<tr><td>Pendency Guarantee VAT Exemption:</td><td>&nbsp;&nbsp;</td><td>10</td></tr>" +
				"<tr><td>Real Pendency Guarantee:</td><td>&nbsp;&nbsp;</td><td>10</td></tr></table><br>" +
				"<H2>Payment information</H2><br>" +
				"<table border=\"0\"><tr><td>Payment Proof Number:</td><td>&nbsp;&nbsp;</td><td>1234</td></tr>" +
				"<tr><td>Payment date limit:</td><td>&nbsp;&nbsp;</td><td>07-06-2021</td></tr></table><br>" +
				"<br><H2>Guarantees</H2><br>" +
				"<table border=\"0\"><tr><td>Accounting Status:</td><td>&nbsp;&nbsp;</td><td>CO - Contractado</td></tr>" +
				"<tr><td>Guarantee Status:</td><td>&nbsp;&nbsp;</td><td>Not Written Off (status not IT)</td></tr></table><br>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
				"<thead><tr class=\"tableheadings\"><th>Customs</th><th>GRN</th><th>Real Debt</th><th>Potential Debt</th><th>Undetermined Real Debt</th></tr></thead>" +
				"<tr><td>AEAT</td><td>16ESAGL9990000096</td><td>387.51</td><td>5</td><td>30</td></tr>" +
				"<tr><td>AEAT</td><td>16ESAGL9990000097</td><td>350</td><td>5</td><td>25</td></tr></table><br>" +
				"<br><H2>Required Certificates</H2><br><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead>" +
				"<tr class=\"tableheadings\"><th>Item</th><th>Measure</th><th>Agency</th><th>Documents</th></tr></thead>" +
				"<tr><td>2</td><td>SNM</td><td>SIF05 -  Sanidad Exterior - M&#186; Sanidad</td><td>N851, C085</td></tr>" +
				"<tr><td>2</td><td>SNM</td><td>SIF06 -  Sanidad Inetrior - M&#186; Sanidad</td><td>C657, C678</td></tr>" +
				"</table>";
		AssertAcceptedAEAT(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, parallel: true);

		CombineAssertions(() =>
		{
			var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

			var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
			AssertEquals("Original transactions + 1 new transaction for first guarantee (balance)", 14, guarantee1Transactions.Count());
			AssertEquals("No new TRA Write off transaction for first guarantee", false, guarantee1Transactions.Any(x => x.CPL_Comment.StartsWith(WriteOffTransactionCommentPrefix)));

			var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == OtherGuaranteeReference).GetTransactions();
			AssertEquals("Original transactions + 1 new transaction for second guarantee (balance)", 14, guarantee2Transactions.Count());
			AssertEquals("No new TRA Write off transaction for second guarantee", false, guarantee2Transactions.Any(x => x.CPL_Comment.StartsWith(WriteOffTransactionCommentPrefix)));
		});
	}

	public void TestAddGuaranteesTransactions_NoPendingAmount()
	{
		SetUpGuarantee(GuaranteeReference, EUGuaranteeTypeList.Codes.IMP, MRNCode, entryHeader.CH_BGMReference, 0, 880, 0, false, false);
		GuaranteesTestHelper.CreateGuaranteeForEntryInstruction(declaration, (entryInstruction.PK, GuaranteeReference));
		declaration.Guarantees[0].PW_BondAmount = 0m;
		AssertEquals(1, declaration.Guarantees.Count);

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileITAccountingStatus(), InterchangeID);

		ProcessMessageForTest(message);

		var expectedMessageInterpretation =
				"<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"></table>" +
				"<table border=\"0\"><tr><td>Export Doc. (MRN):</td><td>&nbsp;&nbsp;</td><td>20EXP0999930006184</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>TEST444444444444</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>04-11-2020, 00:00:00</td></tr>" +
				"<tr><td>Import cert. (CSV):</td><td>&nbsp;&nbsp;</td><td>CRTF444444444444</td></tr></table><br>" +
				"<H2>Management data</H2><br>" +
				"<table border=\"0\"><tr><td>Administration:</td><td>&nbsp;&nbsp;</td><td>AEAT</td></tr>" +
				"<tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>DUA - Declaración Completa de Importación</td></tr>" +
				"<tr><td>Customs Clearance Status:</td><td>&nbsp;&nbsp;</td><td>L - Accepted with Customs Clearance</td></tr>" +
				"<tr><td>Unfinished Pendencies:</td><td>&nbsp;&nbsp;</td><td>N - No tiene</td></tr></table><br>" +
				"<br><H2>Taxes and fees data</H2>" +
				"<H3>Warning: Taxes and fees data received differ from sent data</H3><br>" +
				"<table border=\"0\"><tr><td>Total:</td><td>&nbsp;&nbsp;</td><td>380</td></tr>" +
				"<tr><td>Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>310</td></tr>" +
				"<tr><td>Total Deferred VAT:</td><td>&nbsp;&nbsp;</td><td>35</td></tr>" +
				"<tr><td>Clearance Guarantee VAT Exemption:</td><td>&nbsp;&nbsp;</td><td>80</td></tr>" +
				"<tr><td>Real Clearance Guarantee:</td><td>&nbsp;&nbsp;</td><td>300</td></tr>" +
				"<tr><td>Pendency Guarantee VAT Exemption:</td><td>&nbsp;&nbsp;</td><td>10</td></tr>" +
				"<tr><td>Real Pendency Guarantee:</td><td>&nbsp;&nbsp;</td><td>10</td></tr></table><br>" +
				"<H2>Payment information</H2><br>" +
				"<table border=\"0\"><tr><td>Payment Proof Number:</td><td>&nbsp;&nbsp;</td><td>1234</td></tr>" +
				"<tr><td>Payment date limit:</td><td>&nbsp;&nbsp;</td><td>07-06-2021</td></tr></table><br>" +
				"<br><H2>Guarantees</H2><br>" +
				"<table border=\"0\"><tr><td>Accounting Status:</td><td>&nbsp;&nbsp;</td><td>IT - Ingresada Total</td></tr>" +
				"<tr><td>Guarantee Status:</td><td>&nbsp;&nbsp;</td><td>Excluded (no pending debt)</td></tr></table><br>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
				"<thead><tr class=\"tableheadings\"><th>Customs</th><th>GRN</th><th>Real Debt</th><th>Potential Debt</th><th>Undetermined Real Debt</th></tr></thead>" +
				"<tr><td>AEAT</td><td>16ESAGL9990000096</td><td>387.51</td><td>5</td><td>30</td></tr>" +
				"<tr><td>AEAT</td><td>17ESAGL9990000097</td><td>350</td><td>5</td><td>25</td></tr></table><br>" +
				"<br><H2>Required Certificates</H2><br><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead>" +
				"<tr class=\"tableheadings\"><th>Item</th><th>Measure</th><th>Agency</th><th>Documents</th></tr></thead>" +
				"<tr><td>2</td><td>SNM</td><td>SIF05 -  Sanidad Exterior - M&#186; Sanidad</td><td>N851, C085</td></tr>" +
				"<tr><td>2</td><td>SNM</td><td>SIF06 -  Sanidad Inetrior - M&#186; Sanidad</td><td>C657, C678</td></tr>" +
				"</table>";
		AssertAcceptedAEAT(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, parallel: true);

		var guaranteeHeaderList = LoadCusGuaranteeHeaderList();
		var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
		AssertEquals("Guarantee has no transactions", false, guarantee1Transactions.Any());
	}

	public void TestTemporaryStorage_WithCONTransactionAndTSNotEnabled()
	{
		using (SetTemporaryStorageEnabled(false))
		{
			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, entryHeader.CH_BGMReference);

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileCancelledResponse(), InterchangeID);
			ProcessMessageForTest(message);

			var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			var guarantee = regHeader.Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			TransactionsTestHelper.AssertProcessCancelationResponse_PreRequisites(numTransactions, regHeader.SRH_Status, regLine.SRL_CustomsStatus, numGuaranteeTransactions);
		}
	}

	public void TestProcessCancelationResponse_WithCONTransactionAndLocationNotManagedInPremises()
	{
		using (SetTemporaryStorageEnabled(true))
		{
			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, entryHeader.CH_BGMReference);
			entryHeader.Declaration.CustomsEntryInstructions[0].GoodsLocation.Address.AuthorisationNumber = "9999000000";

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileCancelledResponse(), InterchangeID);
			ProcessMessageForTest(message);

			var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			var guarantee = regHeader.Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			TransactionsTestHelper.AssertProcessCancelationResponse_PreRequisites(numTransactions, regHeader.SRH_Status, regLine.SRL_CustomsStatus, numGuaranteeTransactions);
		}
	}

	public void TestProcessCancelationResponse_WithCONTransactionAndPremiseTypeLAMNotManagedInPremises()
	{
		using (SetTemporaryStorageEnabled(true))
		{
			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, entryHeader.CH_BGMReference);
			regHeader.Premises.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility;

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileCancelledResponse(), InterchangeID);
			ProcessMessageForTest(message);

			var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			var guarantee = regHeader.Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			TransactionsTestHelper.AssertProcessCancelationResponse_PreRequisites(numTransactions, regHeader.SRH_Status, regLine.SRL_CustomsStatus, numGuaranteeTransactions);
		}
	}

	public void TestProcessCancelationResponse_WithCONTransactionAndMessageNotCAN()
	{
		using (SetTemporaryStorageEnabled(true))
		{
			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, entryHeader.CH_BGMReference);

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearanceAEAT, InterchangeID);
			ProcessMessageForTest(message);

			var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			var guarantee = regHeader.Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			TransactionsTestHelper.AssertProcessCancelationResponse_PreRequisites(numTransactions, regHeader.SRH_Status, regLine.SRL_CustomsStatus, numGuaranteeTransactions);
		}
	}

	public void TestProcessCancelationResponse_WithCONTransactionAndTransactionNotDUA()
	{
		using (SetTemporaryStorageEnabled(true))
		{
			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, entryHeader.CH_BGMReference, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements);

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileCancelledResponse(), InterchangeID);
			ProcessMessageForTest(message);

			var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			var guarantee = regHeader.Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			TransactionsTestHelper.AssertProcessCancelationResponse_PreRequisites(numTransactions, regHeader.SRH_Status, regLine.SRL_CustomsStatus, numGuaranteeTransactions);
		}
	}

	public void TestProcessCancelationResponse_WithCONTransactionAndTransactionReferenceNotEntryHeader()
	{
		using (SetTemporaryStorageEnabled(true))
		{
			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, "TestReference");

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileCancelledResponse(), InterchangeID);
			ProcessMessageForTest(message);

			var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			var guarantee = regHeader.Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			TransactionsTestHelper.AssertProcessCancelationResponse_PreRequisites(numTransactions, regHeader.SRH_Status, regLine.SRL_CustomsStatus, numGuaranteeTransactions);
		}
	}

	public void TestProcessCancelationResponse_WithCONAndPNDTransaction()
	{
		using (SetTemporaryStorageEnabled(true))
		{
			var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, entryHeader.CH_BGMReference);
			regLineTransaction1.SRT_PackageQty = 2;
			regLineTransaction1.SRT_GrossWeight = -1.0m;
			regLineTransaction1.SRT_BondAmount = -1.0m;
			var regLineTransaction2 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, entryHeader.CH_BGMReference);
			var regLineTransaction3 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, entryHeader.CH_BGMReference);
			regLineTransaction3.SRT_PackageQty = 1;
			regLineTransaction3.SRT_GrossWeight = -2.0m;
			regLineTransaction3.SRT_BondAmount = -1.0m;

			var regLine2 = Factory.New<CusTempStorageRegLine>();
			regLine2.SRL_LineNumber = 2;
			regLine2.SRL_SRH = regHeader.PK;
			var regLineTransaction4 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, entryHeader.CH_BGMReference);
			regLineTransaction4.SRT_PackageQty = 3;
			regLineTransaction4.SRT_GrossWeight = -3.0m;
			regLineTransaction4.SRT_BondAmount = ZDecimal.Zero;
			var regLineTransaction5 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Pending, entryHeader.CH_BGMReference);
			var guarantee = regHeader.Guarantee.CusGuarantee;

			CombineAssertions("[PreReq]", () =>
			{
				AssertEquals("Number of transactions first RegLine before process cancelation response", 3, regLine.CusTempStorageRegLineTransactions.Count);
				AssertEquals("Number of transactions second RegLine before process cancelation response", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("SRH_Status before process cancelation response is CLS", "CLS", regHeader.SRH_Status);
				AssertEquals("SRL_CustomsStatus before process cancelation response is CLS", "CLS", regLine.SRL_CustomsStatus);
				AssertEquals("Number of guarantee transactions before process cancelation response", 1, guarantee.CusGuaranteeLineTransactions.Count);
				AssertEquals("Transaction1 before process cancelation response status", "CON", regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("Transaction2 before process cancelation response status", "PND", regLineTransaction2.SRT_TransactionStatus);
				AssertEquals("Transaction3 before process cancelation response status", "CON", regLineTransaction3.SRT_TransactionStatus);
				AssertEquals("Transaction4 before process cancelation response status", "CON", regLineTransaction4.SRT_TransactionStatus);
				AssertEquals("Transaction5 before process cancelation response status", "PND", regLineTransaction5.SRT_TransactionStatus);
			});

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileCancelledResponse(), InterchangeID);
			ProcessMessageForTest(message);

			CombineAssertions(() =>
			{
				AssertEquals("Number of transactions first RegLine after process cancelation response has a new transaction", 4, regLine.CusTempStorageRegLineTransactions.Count);
				AssertEquals("Number of transactions second RegLine after process cancelation response has a new transaction", 3, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("SRH_Status after process cancelation response is open", "OPN", regHeader.SRH_Status);
				AssertEquals("SRL_CustomsStatus after process cancelation response is open", "OPN", regLine.SRL_CustomsStatus);
				AssertEquals("Number of guarantee transactions after process cancelation response has a two new transaction", 2, guarantee.CusGuaranteeLineTransactions.Count);
				AssertEquals("Transaction1 after process cancelation response status does not change", "CON", regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("Transaction2 after process cancelation response status changes to DEL", "DEL", regLineTransaction2.SRT_TransactionStatus);
				AssertEquals("Transaction3 after process cancelation response status does not change", "CON", regLineTransaction3.SRT_TransactionStatus);
				AssertEquals("Transaction4 after process cancelation response status does not change", "CON", regLineTransaction4.SRT_TransactionStatus);
				AssertEquals("Transaction5 after process cancelation response status changes to DEL", "DEL", regLineTransaction5.SRT_TransactionStatus);
				AssertTransaction(regLine, bondAmound: 2.0m, packageQty: 3, grossWeight: 3.00m);
				AssertTransaction(regLine2, bondAmound: 0.0m, packageQty: 3, grossWeight: 3.00m);
				AssertGuarantee((CusGuaranteeHeader)guarantee, tranValue: -2.0m);

				message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileCancelledResponse(), InterchangeID);
				ProcessMessageForTest(message);

				AssertEquals("Number of transactions first RegLine after process again cancelation response does not change", 4, regLine.CusTempStorageRegLineTransactions.Count);
				AssertEquals("Number of transactions second RegLine after process again cancelation response does not change", 3, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("Number of guarantee transactions after process again cancelation response does not change", 2, guarantee.CusGuaranteeLineTransactions.Count);
			});
		}
	}

	public void TestSetEntryIssueDate()
	{
		entryHeader.MovementReferenceNumberIssueDate = MovementReferenceNumberIssueDate;
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileWithLongSegmentId(), InterchangeID);
		ProcessMessageForTest(message);
		AssertEquals("MovementReferenceNumberIssueDate", AcceptanceDate, entryHeader.MovementReferenceNumberIssueDate);

		entryHeader.MovementReferenceNumberIssueDate = AcceptanceDate.AddHours(2);
		ProcessMessageForTest(message);
		AssertEquals("MovementReferenceNumberIssueDate", AcceptanceDate.AddHours(2), entryHeader.MovementReferenceNumberIssueDate);
	}

	public void TestProcessAcceptedMessageWithNoClearanceTriggers031DocumentCapture()
	{
		AssertProcessAcceptedMessageWithNoClearanceTriggers031DocumentCapture(
			messageBody: GetAcceptanceTestFileGreenCircuitWithoutClearanceProcedureA_AEAT(),
			messageBodyATC: GetAcceptanceTestFileGreenCircuitWithoutClearanceProcedureA_ATC(),
			expectedEntryStatus: EntryStatusCodes.CustomsDeclarationAccepted,
			expectedAcceptanceDate: AcceptanceDate,
			expectedEntryReleaseDate: EntryReleaseDate,
			expectedLimitPaymentDate: LimitPaymentDate,
			expectedAtcLimitPaymentDate: LimitPaymentDate,
			expectedPaymentProofNumber: PaymentProofNumber,
			expectedAtcPaymentProofNumber: AtcPaymentProofNumber);
	}

	public void TestProcessMessageEntryInstructionSubStyle()
	{
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		CombineAssertions("When IQU message is received, if <C012ProcedimientoSolicitado> value is X or Y, update the entry instruction sub style value to the received value", () =>
		{
			AssertEntryInstruction(GetAcceptanceTestFileGreenCircuitProcedureX(), "X", "C", "X");
			AssertEntryInstruction(GetAcceptanceTestFileGreenCircuitProcedureY(), "Y", "C", "Y");
			AssertEntryInstruction(GetAcceptanceTestFileGreenCircuitProcedureZWithDoc(), "Z", "A", "A");
		});

		void AssertEntryInstruction(string messageText, ZString procedimientoSolicitado, ZString initialSubStyle, ZString expectedSubStyle)
		{
			entryInstruction.CEI_SubStyle = initialSubStyle;
			entryInstruction.Factory.Save();

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, messageText, InterchangeID);

			ProcessMessageForTest(message);

			AssertEquals($"Entry Instruction SubStyle {initialSubStyle}; C012ProcedimientoSolicitado:{procedimientoSolicitado}", expectedSubStyle, entryInstruction.CEI_SubStyle);
		}
	}

	const string WriteOffTransactionCommentPrefix = "Write-off";

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

	protected override int ExpcetedNumberDocMessagesAmountATCInstructionC => 6;

	protected override void SetUp()
	{
		base.SetUp();

		entryHeader.MovementReferenceNumberSetter(MRNCode);

		var (orgHeader, orgAddress) = SetUpOrganization();

		regHeader = Factory.New<CusTempStorageRegHeader>();
		regHeader.SRH_AppCode = "AAA";
		regHeader.SRH_Reference = "reference";
		regHeader.SRH_Status = "CLS";

		SetUpPremises(entryHeader, orgAddress, regHeader, CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse);

		SetUpGuaranteeForRegHeader(orgHeader, regHeader);

		regLine = Factory.New<CusTempStorageRegLine>();
		regLine.SRL_LineNumber = 1;
		regLine.SRL_SRH = regHeader.PK;
		regLine.SRL_CustomsStatus = "CLS";
	}

	CusTempStorageRegHeader regHeader;
	CusTempStorageRegLine regLine;

	string GetAcceptanceTestFileGreenCircuitProcedureB() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedGreenCircuitMessageProcedureB.txt");
	string GetAcceptanceTestFileGreenCircuitProcedureX() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedGreenCircuitMessageProcedureX.txt");
	string GetAcceptanceTestFileGreenCircuitProcedureY() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedGreenCircuitMessageProcedureY.txt");
	string GetAcceptanceTestFileGreenCircuitProcedureZWithDoc() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedGreenCircuitMessageProcedureZWithDoc.txt");
	string GetAcceptanceTestFileGreenCircuitProcedureZWithoutDoc() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedGreenCircuitMessageProcedureZWithoutDoc.txt");
	string GetAcceptanceTestFileGreenCircuitProcedureA_AEAT() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedGreenCircuitMessageProcedureA_AEAT.txt");
	string GetAcceptanceTestFileGreenCircuitProcedureA_ATC() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedGreenCircuitMessageProcedureA_ATC.txt");
	string GetAcceptanceTestFileGreenCircuitWithoutClearanceProcedureA_AEAT() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedGreenCircuitMessageNoClearanceProcedureA_AEAT.txt");
	string GetAcceptanceTestFileGreenCircuitWithoutClearanceProcedureA_ATC() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedGreenCircuitMessageNoClearanceProcedureA_ATC.txt");
	string GetAcceptanceTestFileYellowCircuit() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedYellowCircuitMessage.txt");
	string GetAcceptanceTestFileOrangeCircuit() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedOrangeCircuitMessage.txt");
	string GetAcceptanceTestFilePDSResponseWithFeesAEAT() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedPDSMessage_FeesAEAT.txt");
	string GetAcceptanceTestFilePDSResponseWithFeesATC() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedPDSMessage_FeesATC.txt");
	string GetAcceptanceTestFilePDIResponse() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedPDIMessage.txt");
	string GetAcceptanceTestFilePDIResponseWithFeesAEAT() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedPDIMessage_FeesAEAT.txt");
	string GetAcceptanceTestFilePDIResponseWithFeesATC() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedPDIMessage_FeesATC.txt");
	string GetAcceptanceTestFileCancelledResponse() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "CancelledMessage.txt");
	string GetAcceptanceTestFileWithoutCircuitResponse() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedWithoutCircuit.txt");
	string GetAcceptanceTestFileWithoutCircuitResponseWithFeesAEAT() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedWithoutCircuit_FeesAEAT.txt");
	string GetAcceptanceTestFileWithoutCircuitResponseWithFeesATC() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedWithoutCircuit_FeesATC.txt");
	string GetAcceptanceTestFileDocumentosYCertificados() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedMessageDocumentosYCertificados.txt");
	string GetAcceptanceTestFileCertificadoAportadoOrganismo() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedMessageCertificadoAportadoOrganismo.txt");
	string GetAcceptanceTestFileCertificadoAportadoOperador() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedMessageCertificadoAportadoOperador.txt");
	string GetAcceptanceTestFileDocumentoJustificativo() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedMessageDocumentoJustificativo.txt");
	string GetAcceptanceTestFileDocuments() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedMessageDocuments.txt");
	string GetAcceptanceTestFileGreenCircuitNoProcedure() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedGreenCircuitMessageNoProcedure_AEAT.txt");
	string GetAcceptanceTestFileGreenCircuitWrongProcedure() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedGreenCircuitMessageWrongProcedure.txt");
	string GetAcceptanceTestFileITAccountingStatus() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedITAccountingStatusMessageProcedureB.txt");

	protected override string AcceptanceTestFileWithClearance => string.Empty;
	protected override string AcceptanceTestFileWithClearanceAEAT => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedGreenCircuitMessageNoProcedure_AEAT.txt");
	protected override string AcceptanceTestFileWithClearanceATC => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedGreenCircuitMessageNoProcedure_ATC.txt");
	protected override string AcceptanceTestFilePDSResponse => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedPDSMessage.txt");
	protected override string RejectedTestFile => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "RejectedMessage.txt");
	protected override string AcceptanceTestFileWithoutGuarantees => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedGreenCircuitMessageProcedureBNoGuarantees.txt");
	protected override string AcceptanceTestFileWithGuaranteesAEAT => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedGreenCircuitMessageProcedureBGuaranteesAEAT.txt");
	protected override string AcceptanceTestFileWithGuaranteesATC => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedGreenCircuitMessageProcedureBGuaranteesATC.txt");
	protected override string GetAcceptanceTestFileWithLongSegmentId() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

	protected override ImportQueryResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new ImportQueryResponseMessageProcessor(logger);

	protected override ZString GetExpectedProcessorFriendlyName() => "Import Query Declaration Message Processor";

	protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.ImportQuery };

	protected override ZString SentMessageSubType => DeclarationMessageSubTypeList.Codes.Guarantee;
	protected override ZString RejectedMessageNum => "TARIC20201020141554350002";
	protected override ZString CHStatusWhenWrongXMLOrMessageTextEmpty => "FAL";

	protected override ZString RejectedMessageInterpretation => "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td><strong>Error</strong></td><td><strong>Location/Description</strong></td></tr>" +
				"<tr><td>2814</td><td>1.2<br>Partida(1).CONCEPTOS TRIBUTARIOS (CAS 47)(2).BASE IMPONIBLE TRIBUTO(CAS 47). Cas.47.  Base Imponible del I.V.A. incorrecta. Número de orden del tributo: 2  Base correcta 91.29.C47TributoBaseImponible.KK</td></tr></table>";

	protected override TestEdiMessage SetDataForCorrectPreProcessing(ZString interchangeTransportType)
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		entryHeader.MovementReferenceNumberSetter(MRNCode, ZDateTime.Today);

		var interchangeID = ZGuid.NewZGuid();

		SetSentInterchange(entryHeader, interchangeID);

		var responseInterchange = Factory.New<EDIInterchange>();
		responseInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
		responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		responseInterchange.EI_SessionGUID = interchangeID;
		responseInterchange.EI_TransportType = interchangeTransportType;

		var message = Factory.New<TestEdiMessage>();
		message.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
		message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		message.EM_MessageType = MessageType;
		message.EM_Status = EDIMessage.Status.Queued;
		message.EM_MessageSubType = "AAA";
		message.EM_ApplicationReference = "TEST";
		message.EM_MessageText = GetAcceptanceTestFileGreenCircuitProcedureB();

		responseInterchange.ContainedMessages.Add(message);

		return message;
	}

	protected override TestEdiMessage SetDataForIncorrectApplicationReferencePreProcessing(ZString interchangeTransportType)
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();

		var interchangeID = ZGuid.NewZGuid();

		SetSentInterchange(entryHeader, interchangeID);

		var responseInterchange = Factory.New<EDIInterchange>();
		responseInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
		responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		responseInterchange.EI_SessionGUID = interchangeID;
		responseInterchange.EI_TransportType = interchangeTransportType;

		var message = Factory.New<TestEdiMessage>();
		message.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
		message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		message.EM_MessageType = MessageType;
		message.EM_Status = EDIMessage.Status.Queued;
		message.EM_MessageSubType = "AAA";
		message.EM_ApplicationReference = "TEST";
		message.EM_MessageText = GetAcceptanceTestFileGreenCircuitProcedureB();

		responseInterchange.ContainedMessages.Add(message);
		return message;
	}

	void AssertAccepted(TestEdiMessage message, CusEntryHeader entryHeader, string expectedMessageInterpretation = "", string entryStatusCode = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, string circuit = "", bool parallel = false, string exportMRN = "", ZDateTime? acceptanceDate = null)
	{
		GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageNum: MessageNum, messageSubType: "ACC", entryStatusCode: entryStatusCode, circuit: circuit, acceptanceDate: acceptanceDate, parallel: parallel, movementReferenceNumber: MRNCode, exportMRN: exportMRN, csvClearance: CsvClearance, csvImportCertificate: CsvImportCertificate, entryReleaseDate: EntryReleaseDate);
	}
	void AssertAcceptedAEAT(TestEdiMessage message, CusEntryHeader entryHeader, string expectedMessageInterpretation = "", string entryStatusCode = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, string circuit = CircuitCodeList.Codes.GREEN, bool parallel = false)
	{
		GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageNum: MessageNum, messageSubType: "ACC", entryStatusCode: entryStatusCode, circuit: circuit, acceptanceDate: AcceptanceDate, parallel: parallel, movementReferenceNumber: MRNCode, exportMRN: ExportMRN, csvClearance: CsvClearance, csvImportCertificate: CsvImportCertificate, entryReleaseDate: EntryReleaseDate, paymentProofNumber: PaymentProofNumber, limitPaymentDate: LimitPaymentDate);
	}
	void AssertAcceptedATC(TestEdiMessage message, CusEntryHeader entryHeader, string expectedMessageInterpretation = "", string entryStatusCode = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, string circuit = "", string circuitCan = CircuitCodeList.Codes.GREEN, string exportMRN = ExportMRN, ZDateTime? atcLimitPaymentDate = null, string atcPaymentProofNumber = AtcPaymentProofNumber, bool parallel = false)
	{
		GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageNum: MessageNum, messageSubType: "ACC", entryStatusCode: entryStatusCode, circuit: circuit, acceptanceDate: AcceptanceDate, circuitCan: circuitCan, parallel: parallel, movementReferenceNumber: MRNCode, exportMRN: exportMRN, atcLimitPaymentDate: atcLimitPaymentDate, atcPaymentProofNumber: atcPaymentProofNumber, csvClearance: CsvClearance, csvImportCertificate: CsvImportCertificate, entryReleaseDate: EntryReleaseDate);
	}
	void AssertFailure(TestEdiMessage message, CusEntryHeader entryHeader, string expectedMessageInterpretation = "", string circuit = "", bool parallel = false)
	{
		GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageNum: MessageNum, messageSubType: "AAA", entryStatusCode: "INI", emStatus: EDIMessage.Status.Failed, chStatus: "FAL", circuit: circuit, parallel: parallel, movementReferenceNumber: MRNCode);
	}

	void AssertNewTransaction(string message, SharedCusPermitLineTransaction transaction, ZDecimal amount, ZDateTime acceptanceDate)
	{
		AssertEquals(message + ".CPL_Reference", MRNCode, transaction.CPL_Reference);
		AssertEquals(message + ".CPL_Comment", "Write-off " + entryHeader.CH_BGMReference, transaction.CPL_Comment);
		AssertEquals(message + ".CPL_TranValue", amount, transaction.CPL_TranValue);
		AssertEquals(message + ".CPL_TransactionDate", acceptanceDate, transaction.CPL_TransactionDate);
		AssertEquals(message + ".CPL_TransactionStatus", PermitTransactionStatusList.Codes.Confirmed, transaction.CPL_TransactionStatus);
	}

	void AssertTransaction(CusTempStorageRegLine regLine, ZDecimal bondAmound, int packageQty, decimal grossWeight)
	{
		var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_Comments.Contains("DUA: "));

		AssertEquals("SRT_TransactionType", CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
		AssertEquals("SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
		AssertEquals("SRT_InternalReferenceType", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, transaction.SRT_InternalReferenceType);
		AssertEquals("SRT_InternalReferenceNumber", entryHeader.CH_BGMReference, transaction.SRT_InternalReferenceNumber);
		AssertEquals("SRT_Comments", "DUA: " + entryHeader.CH_BGMReference + TransactionCommentSuffix, transaction.SRT_Comments);
		AssertEquals("SRT_TransactionDate", entryHeader.MovementReferenceNumberIssueDate.ToDateTimeOffset(null), transaction.SRT_TransactionDate);
		AssertEquals("SRT_PackageQty", packageQty, transaction.SRT_PackageQty);
		AssertEquals("SRT_GrossWeight", grossWeight, transaction.SRT_GrossWeight);
		AssertEquals("SRT_BondAmount", bondAmound, transaction.SRT_BondAmount);
		AssertEquals("SRT_Reference", MRNCode, transaction.SRT_Reference);
	}

	void AssertGuarantee(CusGuaranteeHeader cusGuarantee, ZDecimal tranValue)
	{
		var guarantee = cusGuarantee.CusGuaranteeLineTransactions.First(x => x.CPL_Comment.Contains("DUA: ") && x.CPL_TranValue == tranValue);

		AssertEquals("CPL_TransactionType", "TRA", guarantee.CPL_TransactionType);
		AssertEquals("CPL_TransactionDate", entryHeader.MovementReferenceNumberIssueDate, guarantee.CPL_TransactionDate);
		AssertEquals("CPL_Reference", regHeader.SRH_Reference, guarantee.CPL_Reference);
		AssertEquals("CPL_TranValue", tranValue, guarantee.CPL_TranValue);
		AssertEquals("CPL_Comment", "DUA: " + entryHeader.CH_BGMReference + ". MRN: " + entryHeader.MovementReferenceNumber + TransactionCommentSuffix, guarantee.CPL_Comment);
		AssertEquals("CPL_TransactionStatus", "CON", guarantee.CPL_TransactionStatus);
	}

	CusTempStorageRegLineTransaction SetUpTransaction(CusTempStorageRegLine regLine, ZString status, ZString referenceNum, string referenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration)
	{
		var regLineTransaction1 = regLine.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction1.SRT_TransactionStatus = status;
		regLineTransaction1.SRT_InternalReferenceNumber = referenceNum;
		regLineTransaction1.SRT_InternalReferenceType = referenceType;
		regLineTransaction1.SRT_PhysicalInOutDate = EntryReleaseDate.ToDateTimeOffset(null);
		regLineTransaction1.SRT_Reference = MRNCode;

		return regLineTransaction1;
	}

	protected override bool IsImportQuery => true;
	protected override string ExpectedCircuit => CircuitCodeList.Codes.GREEN;
	protected override ZDateTime ExpectedLimitPaymentDate => LimitPaymentDate;
	protected override ZDateTime ExpectedATCLimitPaymentDate => AtcLimitPaymentDate;
	protected override string ExpectedAtcPaymentProofNumber => AtcPaymentProofNumber;
	protected override string ExpectedPaymentProofNumber => PaymentProofNumber;
	protected override string ExpectedExportMRN => ExportMRN;
	protected override string ExpectedCircuitCan => CircuitCodeList.Codes.GREEN;
	protected override string ExpectedMovementReferenceNumberReject => MRNCode;
	protected override ZDateTime ExpectedMovementReferenceNumberIssueDateReject => ZDateTime.Empty;
	protected override string ExpectedentryStatusCode => EntryStatusCodes.Cleared;

	const string TransactionCommentSuffix = " (Canceled)";
}
