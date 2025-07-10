using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

#pragma warning disable IDE0001 // Prevent simplification to Enterprise.Registry.Business.Customs.AU.EntryChargeTypeList.Codes
using ChargeTypes = Enterprise.Customs.AU.Declaration.Business.CusEntryChargeTypeList.Codes;
#pragma warning restore IDE0001 // Prevent simplification to Enterprise.Registry.Business.Customs.AU.EntryChargeTypeList.Codes

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class CMRHeaderAndLineChargesResponseProcessorTest : CMRHeaderChargesResponseProcessorTest
	{
		public void TestSetLineDutyRate()
		{
			string response = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::IMDR+4A1G 4H8A 89G5:1+11'
FTX+AHN+++CLEAR:CLEAR'
GIS+N:117:95'
GIS+TLB:109:95'
GIS+LLB:109:95'
NAD+MR+AAA374M::95'
NAD+VT+AA33HF::95'
NAD+CB+54321::95'
NAD+IM++AUSTRALIAN CUSTOMS SERVICE'
NAD+CB++EAGLE DATAMATION INTERNATIONAL PTY:LIMIT'
RFF+ABO:B00148580/1/SYD2::12'
RFF+ABT:AAAA9JARY::3'
RFF+ABQ:373'
RFF+ADU:B00148580/1'
RFF+AAE:N10'
ERP+::0'
ERC+ID0545::95'
FTX+AAO+++AIR WAYBILL HAS NOT BEEN REPORTED'
TAX+3'
MOA+39:0000000001100.00'
TAX+3'
MOA+40:0000000001100.00'
TAX+3'
MOA+55:0000000000000.54'
TAX+3'
MOA+369:0000000000126.55'
TAX+3'
MOA+68:0000000000165.00'
TAX+3'
MOA+128:0000000000000.30'
DOC+1+1'
CST+2+N10::95'
FTX+AAF+++5% ?+ $0.05449/LITRE'
TAX+1'
GIS+LAQ:109:95'
TAX+1'
MOA+40:0000000001100.00'
TAX+1'
MOA+55:0000000000000.54'
TAX+1'
MOA+369:0000000000126.55'
TAX+1'
MOA+56:0000000001265.54'
TAX+1'
MOA+68:0000000000165.00'
ERP+::218'
ERC+2::95'
FTX+ABS+++GOODS (CHEMICALS) MAY BE REGULATED BY NICNAS. RING 1800638528'
CNT+5:1'
UNT+51+000001'".Replace("\r\n", "");

			CMRCodeLists codeForLA = CMRCodeLists.New(Factory);
			codeForLA.CI_Code = "L";
			codeForLA.CI_CodeType = CMRCodeLists.CodeTypes.QUANUNIT;
			codeForLA.CI_Name = "LITRE";

			CusEntryHeader entryHeader = SetUpEntryHeader();
			entryHeader.CH_BGMReference = "B00148580/1";
			entryHeader.Declaration.JE_DeclarationReference = "B00148580";
			CusEntryLine entryLine2 = entryHeader.MergedLines.FindByLineNumber(2);
			AssertEquals("Percent rate is not there yet", false, entryLine2.CL_DutyPercent == 5m);
			AssertEquals("Flat amount is not matching what is in the message", false, entryLine2.CL_FlatAmount == 0.05449m);
			AssertEquals("Flat UQ is not matching what is in the message", false, entryLine2.CL_FlatAmountUQ == "L");

			EDIMessage incomingMessage = (EDIMessage)Factory.New(IncomingMessageType);
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			incomingMessage.EM_MessageText = response;
			CMRHeaderAndLineChargesResponseProcessor processor = (CMRHeaderAndLineChargesResponseProcessor)GetMessageProcessor();
			processor.ProcessMessage(incomingMessage);
			AssertEquals("Percent rate is there now", true, entryLine2.CL_DutyPercent == 5m);
			AssertEquals("Flat amount is matching what is in the message", true, entryLine2.CL_FlatAmount == 0.05449m);
			AssertEquals("Flat UQ is not matching what is in the message", true, entryLine2.CL_FlatAmountUQ == "L");
		}

		public void TestPopulateLineLevelCharge()
		{
			#region Test Messages
			string responseMessage = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::IMDR+20BE E99A 3365:1+11'
FTX+AHN+++CLEAR:CLEAR'
GIS+N:117:95'
GIS+TLB:109:95'
GIS+LLB:109:95'
NAD+MR+AAA447Y::95'
NAD+VT+AA33JL::95'
NAD+CB+54333::95'
NAD+IM++PORTER DATA MANAGEMENT PTY LTD'
NAD+CB++SOFTWARE CRAFT PTY LTD'
RFF+ABO:B00122382/1/SYD4::4'
RFF+ABT:AAAA3JMYK::2'
RFF+ABQ:AJAXWIDGET/50'
RFF+ADU:B00122382'
RFF+AAE:N10'
ERP+::0'
ERC+ID0548::95'
FTX+AAO+++BILL OF LADING NOT REPORTED FOR VESSEL VOYAGE'
TAX+3'
MOA+39:0000000002631.57'
TAX+3'
MOA+40:0000000002631.57'
TAX+3'
MOA+55:0000000000197.36'
TAX+3'
MOA+369:0000000000309.20'
TAX+3'
MOA+68:0000000000263.16'
TAX+3'
MOA+128:0000000000566.31'
TAX+3'
MOA+26:0000000000006.50'
TAX+3'
MOA+35:0000000000003.75'
TAX+3'
MOA+23:0000000000049.50'
DOC+1+1'
CST+1+N10::95'
FTX+AAF+++7.5%'
TAX+1'
GIS+LAQ:109:95'
TAX+1'
MOA+40:0000000001315.79'
TAX+1'
MOA+55:0000000000098.68'
TAX+1'
MOA+369:0000000000154.60'
TAX+1'
MOA+56:0000000001546.05'
TAX+1'
MOA+68:0000000000130.58'
CST+2+N10::95'
FTX+AAF+++7.5%'
TAX+1'
GIS+LAQ:109:95'
TAX+1'
MOA+40:0000000001316.80'
TAX+1'
MOA+55:0000000000099.67'
TAX+1'
MOA+369:0000000000155.61'
TAX+1'
MOA+56:0000000001546.05'
TAX+1'
MOA+68:0000000000132.58'
CNT+5:2'
UNT+68+000001'".Replace("\r\n", "");

			string amendmentMessageForMoreDutyAndGST = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::IMDR+2J98 F5BD 6I65:1+11'
FTX+AHN+++CLEAR:CLEAR'
GIS+N:117:95'
GIS+TLB:109:95'
GIS+LLB:109:95'
NAD+MR+AAA447Y::95'
NAD+VT+AA33JL::95'
NAD+CB+54333::95'
NAD+IM++PORTER DATA MANAGEMENT PTY LTD'
NAD+CB++SOFTWARE CRAFT PTY LTD'
RFF+ABO:B00122382/1/SYD4::4'
RFF+ABT:AAAA3JMYK::4'
RFF+ABQ:AJAXWIDGET/50'
RFF+ADU:B00122382'
RFF+AAE:N10'
ERP+::0'
ERC+ID0548::95'
FTX+AAO+++BILL OF LADING NOT REPORTED FOR VESSEL VOYAGE'
TAX+3'
MOA+39:0000000003289.47'
TAX+3'
MOA+40:0000000003289.47'
TAX+3'
MOA+55:0000000000246.70'
TAX+3'
MOA+369:0000000000393.08'
TAX+3'
MOA+68:0000000000394.74'
TAX+3'
MOA+128:0000000000133.22'
TAX+3'
MOA+35:0000000000003.75'
DOC+1+1'
CST+1+N10::95'
FTX+AAF+++7.5%'
TAX+1'
GIS+LAQ:109:95'
TAX+1'
MOA+40:0000000001973.68'
TAX+1'
MOA+55:0000000000148.02'
TAX+1'
MOA+369:0000000000235.85'
TAX+1'
MOA+56:0000000002358.54'
TAX+1'
MOA+68:0000000000236.84'
CST+2+N10::95'
FTX+AAF+++7.5%'
TAX+1'
GIS+LAQ:109:95'
TAX+1'
MOA+40:0000000001315.80'
TAX+1'
MOA+55:0000000000098.67'
TAX+1'
MOA+369:0000000000154.61'
TAX+1'
MOA+56:0000000001572.37'
TAX+1'
MOA+68:0000000000157.90'
CNT+5:2'
UNT+64+000001'".Replace("\r\n", "");

			string refundMessage = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::IMDR+4459 154A I365:1+11'
FTX+AHN+++CLEAR:CLEAR'
GIS+N:117:95'
GIS+TLB:109:95'
GIS+LLB:109:95'
NAD+MR+AAA447Y::95'
NAD+VT+AA33JL::95'
NAD+CB+54333::95'
NAD+IM++PORTER DATA MANAGEMENT PTY LTD'
NAD+CB++SOFTWARE CRAFT PTY LTD'
RFF+ABO:B00122382/1/SYD4::4'
RFF+ABT:AAAA3JMYK::6'
RFF+ABQ:AJAXWIDGET/50'
RFF+ADU:B00122382'
RFF+AAE:N10'
ERP+::0'
ERC+ID0548::95'
FTX+AAO+++BILL OF LADING NOT REPORTED FOR VESSEL VOYAGE'
TAX+3'
MOA+39:0000000002368.42'
TAX+3'
MOA+40:0000000002368.42'
TAX+3'
MOA+55:0000000000177.62'
TAX+3'
MOA+369:0000000000309.20'
TAX+3'
MOA+68:0000000000394.73'
TAX+3'
MOA+128:-0000000000019.74'
TAX+3'
MOA+35:0000000000003.75'
DOC+1+1'
CST+1+N10::95'
FTX+AAF+++7.5%'
TAX+1'
GIS+LAQ:109:95'
TAX+1'
MOA+40:0000000001052.63'
TAX+1'
MOA+55:0000000000078.94'
TAX+1'
MOA+369:0000000000154.60'
TAX+1'
MOA+56:0000000001307.01'
TAX+1'
MOA+68:0000000000175.44'
CST+2+N10::95'
FTX+AAF+++7.5%'
TAX+1'
GIS+LAQ:109:95'
TAX+1'
MOA+40:0000000001315.80'
TAX+1'
MOA+55:0000000000098.67'
TAX+1'
MOA+369:0000000000154.61'
TAX+1'
MOA+56:0000000001633.76'
TAX+1'
MOA+68:0000000000219.29'
CNT+5:2'
UNT+64+000001'".Replace("\r\n", "");

			#endregion

			var entryHeader = SetUpEntryHeader();
			entryHeader.CH_BGMReference = "B00122382/1";
			entryHeader.Declaration.JE_DeclarationReference = "B00122382";
			entryHeader.Declaration.InvoiceLines[0].AddInfo.ZA_CalcTILV_Hidden = "123.45AUD";

			var entryLine1 = entryHeader.MergedLines.FindByLineNumber(1);
			var entryLine2 = entryHeader.MergedLines.FindByLineNumber(2);

			AssertEquals("T&I for Line1", 123.45m, entryLine1.CustomsCalculatedTransportAndInsuranceInLocalCurrency.Amount);
			AssertEquals("T&I for Line2", 0m, entryLine2.CustomsCalculatedTransportAndInsuranceInLocalCurrency.Amount);

			var outgoing = Factory.New<CMRIMDMessage>();
			entryHeader.Messages.Add(outgoing);
			outgoing.EM_MessageText = "UNH+1+CUSDEC:D:99B:UN'BGM+929:::IMD+B00122382/1:1+9'CST++10::95'LOC+8+AUMEL::6'LOC+9+SGSIN::6'LOC+12+AUMEL::6'LOC+79+AUMEL::6'DTM+7:20041229:102'DTM+252:20041229:102'DTM+260:20041229:102'MEA+AAE+G+KG:123.00'RFF+ABQ'RFF+APH:FOB'PAC+0'PAC++1'TDT+20+123+S(SEA)+++++9044748::11'NAD+AT+31006626946::95'NAD+CB+00657C'MOA+141:150.00:AUD'MOA+40:150.00:AUD'MOA+63:150.00:AUD'MOA+39:150.00:AUD'UNS+D'DMS+1'RFF+BH:HOUSE BILL'RFF+MB:OCEANBILL'CST+1+LINE ACTION CODE::95+N10::95'FTX+AAA+++REFILLABLE, LIQUID INK REFILL'LOC+27+US::5'NAD+SU+2997664Y::95'PCI+1'MOA+40:150.00:AUD'MOA+38:150.00:AUD'MOA+5:0.03:AUD'RFF+ABA'RFF+ABC'RFF+ABD:96081000'RFF+ABL'RFF+AEA:45'RFF+AES'RFF+AFD'UNT+42+1'";
			outgoing.EM_LinkedObject = entryHeader;

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			outgoing.GetLogs().AddNew(Events.AddedARecordToTheSystem, new ZDateTimeOffset(2005, 1, 1));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			#region First Response
			incomingMessage = (EDIMessage)Factory.New(IncomingMessageType);
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			incomingMessage.EM_MessageText = responseMessage;
			processor.ProcessMessage(incomingMessage);

			AssertEquals("CV for Line1", 1315.79m, entryLine1.CL_CustomsValue);
			AssertEquals("Duty for Line1", 98.68m, entryLine1.Fees.GetOrAddFeeByFeeType(ChargeTypes.DutyAmount).CF_ChargeAmount);
			AssertEquals("GST for Line1", 154.60m, entryLine1.Fees.GetOrAddFeeByFeeType(ChargeTypes.GSTAmount).CF_ChargeAmount);
			AssertEquals("T&I for Line1", 130.58m, entryLine1.CustomsCalculatedTransportAndInsuranceInLocalCurrency.Amount);

			AssertEquals("CV for Line2", 1316.80m, entryLine2.CL_CustomsValue);
			AssertEquals("Duty for Line2", 99.67m, entryLine2.Fees.GetOrAddFeeByFeeType(ChargeTypes.DutyAmount).CF_ChargeAmount);
			AssertEquals("GST for Line2", 155.61m, entryLine2.Fees.GetOrAddFeeByFeeType(ChargeTypes.GSTAmount).CF_ChargeAmount);
			AssertEquals("T&I for Line2", 132.58m, entryLine2.CustomsCalculatedTransportAndInsuranceInLocalCurrency.Amount);
			AssertEquals("VOTI for Line2", 1549.05m, entryLine2.VOTI);

			AssertEquals("AQIS Container Charge", 3.75m, entryHeader.Charges[ChargeTypes.AQISContainerCharges].C1_ChargeAmount);
			AssertEquals("AQIS Processing", 6.50m, entryHeader.Charges[ChargeTypes.AQISProcessingCharge].C1_ChargeAmount);
			AssertEquals("Declaration Processing Charge", 49.50m, entryHeader.Charges[ChargeTypes.DeclarationProcessingCharge].C1_ChargeAmount);

			AssertEquals("Total T&I matches the header amount.", 263.16m, entryHeader.TAndI);
			AssertEquals("Total paid", 568.31m, entryHeader.CH_TotalPaid);

			#endregion

			#region Second Response after payment

			var outgoing2 = Factory.New<CMRIMDMessage>();
			entryHeader.Messages.Add(outgoing2);
			outgoing2.EM_MessageText = "UNH+1+CUSDEC:D:99B:UN'BGM+929:::IMD+B00122382/1MESSAGEERRORS/1:2+4'CST++N10::95'LOC+8+AUSYD::6'LOC+9+NZAKL::6'LOC+12+AUSYD::6'LOC+79+AUSYD::6'DTM+178:20050208:102'DTM+252:20050208:102'DTM+260:20050208:102'GIS+EPA:109:95'GIS+Y:153:95'FII+COQ+323232:DEBORAH SPAGARINO TEST ACCOUNT+:::242200::215'MEA+AAE+G+KG:150.00000'RFF+ABQ:CHAGED DEC'RFF+ADU:S00039742'RFF+AMG:00001'RFF+APH:FOB'RFF+ANU:B'TDT+20++A++QF::3'NAD+IM+AAA3334647R::95'NAD+CB+54321::95'NAD+VT+AA33HF::95'MOA+141:1500.00:AUD'MOA+63:1500.00:AUD'MOA+39:1500.00:AUD'UNS+D'DMS+1'LIN+1+I'PAC+150+1'PCI+1'RFF+MWB:08112345675'PCI+1'RFF+HWB:1'CST+1+I::95+N10::95'FTX+AAA+++PENCIL LEADS, BLACK OR COLOURED'LOC+27+IT::5'MEA+AAA++NO:100.00'NAD+SU+66015286036::95'MOA+38:1500.00:AUD'RFF+ABD:96092000'RFF+AED:17'RFF+AFV:TV'UNS+S'UNT+45+1'";
			outgoing2.EM_LinkedObject = entryHeader;
			outgoing2.EM_ReceiveTransmit = "TRX";

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			outgoing2.Logs.AddNew(Events.AddedARecordToTheSystem, new ZDateTimeOffset(2005, 1, 2));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingAmendment.Code;
			incomingMessage = (EDIMessage)Factory.New(IncomingMessageType);
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			incomingMessage.EM_MessageText = amendmentMessageForMoreDutyAndGST;
			processor = (CMRHeaderAndLineChargesResponseProcessor)GetMessageProcessor();
			processor.ProcessMessage(incomingMessage);

			AssertEquals("CV for Line1", 1973.68m, entryLine1.CL_CustomsValue);
			AssertEquals("Duty for Line1", 148.02m, entryLine1.Fees.GetOrAddFeeByFeeType(ChargeTypes.DutyAmount).CF_ChargeAmount);
			AssertEquals("GST for Line1", 235.85m, entryLine1.Fees.GetOrAddFeeByFeeType(ChargeTypes.GSTAmount).CF_ChargeAmount);
			AssertEquals("T&I for Line1", 236.84m, entryLine1.CustomsCalculatedTransportAndInsuranceInLocalCurrency.Amount);

			AssertEquals("CV for Line2", 1315.80m, entryLine2.CL_CustomsValue);
			AssertEquals("Duty for Line2", 98.67m, entryLine2.Fees.GetOrAddFeeByFeeType(ChargeTypes.DutyAmount).CF_ChargeAmount);
			AssertEquals("GST for Line2", 154.61m, entryLine2.Fees.GetOrAddFeeByFeeType(ChargeTypes.GSTAmount).CF_ChargeAmount);
			AssertEquals("T&I for Line2", 157.90m, entryLine2.CustomsCalculatedTransportAndInsuranceInLocalCurrency.Amount);

			AssertEquals("AQIS Container Charge", 3.75m, entryHeader.Charges[ChargeTypes.AQISContainerCharges].C1_ChargeAmount);
			AssertEquals("AQIS Processing", 6.50m, entryHeader.Charges[ChargeTypes.AQISProcessingCharge].C1_ChargeAmount);
			AssertEquals("Declaration Processing Charge", 49.50m, entryHeader.Charges[ChargeTypes.DeclarationProcessingCharge].C1_ChargeAmount);

			AssertEquals("Total T&I", 394.74m, entryHeader.TAndI);
			AssertEquals("Total paid", 696.90m, entryHeader.CH_TotalPaid);

			#endregion

			#region Refund

			incomingMessage = (EDIMessage)Factory.New(IncomingMessageType);
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			incomingMessage.EM_MessageText = refundMessage;
			processor = (CMRHeaderAndLineChargesResponseProcessor)GetMessageProcessor();
			processor.ProcessMessage(incomingMessage);

			AssertEquals("AQIS Processing", 6.50m, entryHeader.Charges[ChargeTypes.AQISProcessingCharge].C1_ChargeAmount);
			AssertEquals("AQIS Container Charge", 3.75m, entryHeader.Charges[ChargeTypes.AQISContainerCharges].C1_ChargeAmount);
			AssertEquals("Declaration Processing Charge", 49.50m, entryHeader.Charges[ChargeTypes.DeclarationProcessingCharge].C1_ChargeAmount);

			AssertEquals("CV for Line1", 1052.63m, entryLine1.CL_CustomsValue);
			AssertEquals("Duty for Line1", 78.94m, entryLine1.Fees.GetOrAddFeeByFeeType(ChargeTypes.DutyAmount).CF_ChargeAmount);
			AssertEquals("GST for Line1", 154.60m, entryLine1.Fees.GetOrAddFeeByFeeType(ChargeTypes.GSTAmount).CF_ChargeAmount);
			AssertEquals("T&I for Line1", 175.44m, entryLine1.CustomsCalculatedTransportAndInsuranceInLocalCurrency.Amount);

			AssertEquals("CV for Line2", 1315.80m, entryLine2.CL_CustomsValue);
			AssertEquals("Duty for Line2", 98.67m, entryLine2.Fees.GetOrAddFeeByFeeType(ChargeTypes.DutyAmount).CF_ChargeAmount);
			AssertEquals("GST for Line2", 154.61m, entryLine2.Fees.GetOrAddFeeByFeeType(ChargeTypes.GSTAmount).CF_ChargeAmount);
			AssertEquals("T&I for Line2", 219.29m, entryLine2.CustomsCalculatedTransportAndInsuranceInLocalCurrency.Amount);

			AssertEquals("Total paid", 546.57m, entryHeader.CH_TotalPaid);
			AssertEquals("Total T&I", 394.73m, entryHeader.TAndI);

			#endregion
		}

		public void TestPopulateHeaderLevelCharge_ConsolidatedDeclaration()
		{
			#region Test Messages

			string responseMessage = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::IMDR+20BE E99A 3365:1+11'
FTX+AHN+++CLEAR:CLEAR'
GIS+N:117:95'
GIS+TLB:109:95'
GIS+LLB:109:95'
NAD+MR+AAA447Y::95'
NAD+VT+AA33JL::95'
NAD+CB+54333::95'
NAD+IM++PORTER DATA MANAGEMENT PTY LTD'
NAD+CB++SOFTWARE CRAFT PTY LTD'
RFF+ABO:CE00122382/SYD4::4'
RFF+ABT:AAAA3JMYK::2'
RFF+ABQ:AJAXWIDGET/50'
RFF+ADU:B00122382'
RFF+AAE:N10'
ERP+::0'
ERC+ID0548::95'
FTX+AAO+++BILL OF LADING NOT REPORTED FOR VESSEL VOYAGE'
TAX+3'
MOA+39:0000000002631.57'
TAX+3'
MOA+40:0000000002631.57'
TAX+3'
MOA+55:0000000000197.36'
TAX+3'
MOA+369:0000000000309.20'
TAX+3'
MOA+68:0000000000263.16'
TAX+3'
MOA+128:0000000000566.31'
TAX+3'
MOA+26:0000000000006.50'
TAX+3'
MOA+35:0000000000003.75'
TAX+3'
MOA+23:0000000000049.50'
DOC+1+1'
CST+1+N10::95'
FTX+AAF+++7.5%'
TAX+1'
GIS+LAQ:109:95'
TAX+1'
MOA+40:0000000001315.79'
TAX+1'
MOA+55:0000000000098.68'
TAX+1'
MOA+369:0000000000154.60'
TAX+1'
MOA+56:0000000001546.05'
TAX+1'
MOA+68:0000000000130.58'
CST+2+N10::95'
FTX+AAF+++7.5%'
TAX+1'
GIS+LAQ:109:95'
TAX+1'
MOA+40:0000000001316.80'
TAX+1'
MOA+55:0000000000099.67'
TAX+1'
MOA+369:0000000000155.61'
TAX+1'
MOA+56:0000000001546.05'
TAX+1'
MOA+68:0000000000132.58'
CNT+5:2'
UNT+68+000001'".Replace("\r\n", "");

			#endregion

			GlbStaff.CurrentUser.GS_Code = User.ServiceUserCode;
			var consolidatedDeclaration = Customs.Business.Testing.ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 2);
			consolidatedDeclaration.CRD_JobReferenceNumber = "CE00122382";
			var leadDec = consolidatedDeclaration.LeadDeclaration as JobDeclaration;
			leadDec.Invoices.AddNew().InvoiceLines.AddNew();
			var leadDeclarationEntryHeader = leadDec.CustomsEntryHeaders[0];
			leadDeclarationEntryHeader.AllEntryLines.AddNew().InvoiceLines.AddRange(leadDec.InvoiceLines);
			leadDeclarationEntryHeader.AllEntryLines[0].CL_LineNumber = (short)1;
			leadDeclarationEntryHeader.AllEntryLines[0].ZA_AggregateEntryLineNumber = 1;
			leadDeclarationEntryHeader.MergedLines.Add(leadDeclarationEntryHeader.AllEntryLines[0]);
			leadDeclarationEntryHeader.Declaration.InvoiceLines[0].AddInfo.ZA_CalcTILV_Hidden = "123.45AUD";

			var declaration2 = (JobDeclaration)consolidatedDeclaration.JobDeclarations[1];
			declaration2.Invoices.AddNew().InvoiceLines.AddNew();
			var declarationEntryHeader2 = declaration2.CustomsEntryHeaders[0];
			declarationEntryHeader2.AllEntryLines.AddNew().InvoiceLines.AddRange(declaration2.InvoiceLines);
			declarationEntryHeader2.AllEntryLines[0].CL_LineNumber = (short)2;
			declarationEntryHeader2.AllEntryLines[0].ZA_AggregateEntryLineNumber = 2;
			declarationEntryHeader2.MergedLines.Add(declarationEntryHeader2.AllEntryLines[0]);

			var entryLine1 = leadDeclarationEntryHeader.MergedLines.FindByLineNumber(1);
			var entryLine2 = declarationEntryHeader2.MergedLines.FindByLineNumber(2);

			AssertEquals("T&I for Line1", 123.45m, entryLine1.CustomsCalculatedTransportAndInsuranceInLocalCurrency.Amount);
			AssertEquals("T&I for Line2", 0m, entryLine2.CustomsCalculatedTransportAndInsuranceInLocalCurrency.Amount);

			var outgoing = Factory.New<CMRIMDMessage>();
			leadDeclarationEntryHeader.Messages.Add(outgoing);
			outgoing.EM_MessageText = "UNH+1+CUSDEC:D:99B:UN'BGM+929:::IMD+CE00122382/1:1+9'CST++10::95'LOC+8+AUMEL::6'LOC+9+SGSIN::6'LOC+12+AUMEL::6'LOC+79+AUMEL::6'DTM+7:20041229:102'DTM+252:20041229:102'DTM+260:20041229:102'MEA+AAE+G+KG:123.00'RFF+ABQ'RFF+APH:FOB'PAC+0'PAC++1'TDT+20+123+S(SEA)+++++9044748::11'NAD+AT+31006626946::95'NAD+CB+00657C'MOA+141:150.00:AUD'MOA+40:150.00:AUD'MOA+63:150.00:AUD'MOA+39:150.00:AUD'UNS+D'DMS+1'RFF+BH:HOUSE BILL'RFF+MB:OCEANBILL'CST+1+LINE ACTION CODE::95+N10::95'FTX+AAA+++REFILLABLE, LIQUID INK REFILL'LOC+27+US::5'NAD+SU+2997664Y::95'PCI+1'MOA+40:150.00:AUD'MOA+38:150.00:AUD'MOA+5:0.03:AUD'RFF+ABA'RFF+ABC'RFF+ABD:96081000'RFF+ABL'RFF+AEA:45'RFF+AES'RFF+AFD'UNT+42+1'";
			outgoing.EM_LinkedObject = leadDeclarationEntryHeader;

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			outgoing.GetLogs().AddNew(Events.AddedARecordToTheSystem, new ZDateTimeOffset(2005, 1, 1));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			incomingMessage = (EDIMessage)Factory.New(IncomingMessageType);
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			incomingMessage.EM_MessageText = responseMessage;
			processor.ProcessMessage(incomingMessage);

			CombineAssertions(() =>
			{
				AssertEquals("CV for Line1", 1315.79m, entryLine1.CL_CustomsValue);
				AssertEquals("Duty for Line1", 98.68m, entryLine1.Fees.GetOrAddFeeByFeeType(ChargeTypes.DutyAmount).CF_ChargeAmount);
				AssertEquals("GST for Line1", 154.60m, entryLine1.Fees.GetOrAddFeeByFeeType(ChargeTypes.GSTAmount).CF_ChargeAmount);
				AssertEquals("T&I for Line1", 130.58m, entryLine1.CustomsCalculatedTransportAndInsuranceInLocalCurrency.Amount);

				AssertEquals("CV for Line2", 1316.80m, entryLine2.CL_CustomsValue);
				AssertEquals("Duty for Line2", 99.67m, entryLine2.Fees.GetOrAddFeeByFeeType(ChargeTypes.DutyAmount).CF_ChargeAmount);
				AssertEquals("GST for Line2", 155.61m, entryLine2.Fees.GetOrAddFeeByFeeType(ChargeTypes.GSTAmount).CF_ChargeAmount);
				AssertEquals("T&I for Line2", 132.58m, entryLine2.CustomsCalculatedTransportAndInsuranceInLocalCurrency.Amount);
				AssertEquals("VOTI for Line2", 1549.05m, entryLine2.VOTI);

				AssertEquals("Lead AQIS Container Charge", 3.75m, leadDeclarationEntryHeader.Charges[ChargeTypes.AQISContainerCharges].C1_ChargeAmount);
				AssertEquals("Lead AQIS Processing", 6.50m, leadDeclarationEntryHeader.Charges[ChargeTypes.AQISProcessingCharge].C1_ChargeAmount);
				AssertEquals("Lead Declaration Processing Charge", 49.50m, leadDeclarationEntryHeader.Charges[ChargeTypes.DeclarationProcessingCharge].C1_ChargeAmount);
				AssertEquals("Lead T&I is zero.", 0.00m, leadDeclarationEntryHeader.AddInfo.TILVInAUD);

				AssertEquals("Other AQIS Container Charge", 0m, declarationEntryHeader2.Charges[ChargeTypes.AQISContainerCharges].C1_ChargeAmount);
				AssertEquals("Other AQIS Processing", 0m, declarationEntryHeader2.Charges[ChargeTypes.AQISProcessingCharge].C1_ChargeAmount);
				AssertEquals("Other Declaration Processing Charge", 0m, declarationEntryHeader2.Charges[ChargeTypes.DeclarationProcessingCharge].C1_ChargeAmount);
				AssertEquals("Other T&I is zero.", 0.00m, declarationEntryHeader2.AddInfo.TILVInAUD);

				AssertEquals("declaration1: Total paid", 313.03m, leadDeclarationEntryHeader.CH_TotalPaid);
				AssertEquals("declaration2: Total paid", 255.28m, declarationEntryHeader2.CH_TotalPaid);
				AssertEquals("Total T&I matches the header amount.", 263.16m, consolidatedDeclaration.TAndI);
			});
		}

		public void TestDutyDeferredAmount()
		{
			string responseMessage = @"UNH+000002+CUSRES:D:99B:UN'
BGM+961:::IMDR+1H84 G0AE 2DB8:1+11'
FTX+AHN+++FINALISED:FINALISED'
GIS+N:117:95'
GIS+TLB:109:95'
GIS+LLB:109:95'
NAD+MR+AAA374M::95'
NAD+VT+AA33HF::95'
NAD+CB+54321::95'
NAD+IM++MICHAEL J BOWEN & ASSOCIATES P'
NAD+CB++WISETECH GLOBAL LIMITED'
RFF+ABO:B00122382/1/CMT3::3'
RFF+ABT:AAAEWRYYN::1'
RFF+ABQ:CLOTHES'
RFF+ADU:B00122382/1 HB123'
RFF+AAE:N10'
ERP+::0'
ERC+ID0545::95'
FTX+AAO+++AIR WAYBILL HAS NOT BEEN REPORTED'
ERP+::1'
ERC+ID0288::95'
FTX+AAO+++UNIT VALUE FOR Quantity=?+00000000010.00 IS NOT IN RANGE FOR STATISTICAL CODE'
TAX+3'
MOA+39:0000000020000.00'
TAX+3'
MOA+40:0000000020000.00'
TAX+3'
MOA+210:0000000002210.20'
TAX+3'
MOA+68:0000000000102.00'
TAX+3'
MOA+128:0000000000185.00'
TAX+3'
MOA+346:0000000002000.00'
TAX+3'
MOA+26:0000000000033.00'
TAX+3'
MOA+23:0000000000152.00'
TAX+3'
MOA+55:0000000002000.00'
DOC+1+1'
CST+1+N10::95'
FTX+AAF+++10%'
TAX+1'
MOA+40:0000000020000.00'
TAX+1'
MOA+210:0000000002210.20'
TAX+1'
MOA+56:0000000022102.00'
TAX+1'
MOA+68:0000000000102.00'
TAX+1'
MOA+55:0000000002000.00'
CNT+5:1'
UNT+55+000002'".Replace("\r\n", "");

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "AUT";
			org.AUIsDutyDeferred = true;
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_DeclarationReference = "B00122382";

			var entryHeader = SetUpEntryHeader();
			entryHeader.CH_BGMReference = "B00122382/1";
			Factory.Save();

			var outgoing = Factory.New<CMRIMDMessage>();
			entryHeader.Messages.Add(outgoing);
			outgoing.EM_MessageText = "UNH+1+CUSDEC:D:99B:UN'BGM+929:::IMD+B00122382/1:1+9'CST++10::95'LOC+8+AUMEL::6'LOC+9+SGSIN::6'LOC+12+AUMEL::6'LOC+79+AUMEL::6'DTM+7:20041229:102'DTM+252:20041229:102'DTM+260:20041229:102'MEA+AAE+G+KG:123.00'RFF+ABQ'RFF+APH:FOB'PAC+0'PAC++1'TDT+20+123+S(SEA)+++++9044748::11'NAD+AT+31006626946::95'NAD+CB+00657C'MOA+141:150.00:AUD'MOA+40:150.00:AUD'MOA+63:150.00:AUD'MOA+39:150.00:AUD'UNS+D'DMS+1'RFF+BH:HOUSE BILL'RFF+MB:OCEANBILL'CST+1+LINE ACTION CODE::95+N10::95'FTX+AAA+++REFILLABLE, LIQUID INK REFILL'LOC+27+US::5'NAD+SU+2997664Y::95'PCI+1'MOA+40:150.00:AUD'MOA+38:150.00:AUD'MOA+5:0.03:AUD'RFF+ABA'RFF+ABC'RFF+ABD:96081000'RFF+ABL'RFF+AEA:45'RFF+AES'RFF+AFD'UNT+42+1'";
			outgoing.EM_LinkedObject = entryHeader;
			outgoing.EM_Status = EDIMessage.Status.Sent;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			outgoing.GetLogs().AddNew(Events.AddedARecordToTheSystem, new ZDateTimeOffset(2005, 1, 1));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			incomingMessage = (EDIMessage)Factory.New(IncomingMessageType);
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			incomingMessage.EM_MessageText = responseMessage;
			processor.ProcessMessage(incomingMessage);

			AssertEquals("Added a DTD charge.", 2000m, entryHeader.Charges[ChargeTypes.DutyDeferredAmount].C1_ChargeAmount);
			AssertEquals("Total Payable Amount", 185m, entryHeader.TotalAmountPayable);
		}

		public void TestDutyIsPartiallyDeferred()
		{
			string responseMessage = @"UNH+000002+CUSRES:D:99B:UN'
BGM+961:::IMDR+1H84 G0AE 2DB8:1+11'
FTX+AHN+++FINALISED:FINALISED'
GIS+N:117:95'
GIS+TLB:109:95'
GIS+LLB:109:95'
NAD+MR+AAA374M::95'
NAD+VT+AA33HF::95'
NAD+CB+54321::95'
NAD+IM++MICHAEL J BOWEN & ASSOCIATES P'
NAD+CB++WISETECH GLOBAL LIMITED'
RFF+ABO:B00122382/1/CMT3::3'
RFF+ABT:AAAEWRYYN::1'
RFF+ABQ:CLOTHES'
RFF+ADU:B00122382/1 HB123'
RFF+AAE:N10'
ERP+::0'
ERC+ID0545::95'
FTX+AAO+++AIR WAYBILL HAS NOT BEEN REPORTED'
ERP+::1'
ERC+ID0288::95'
FTX+AAO+++UNIT VALUE FOR Quantity=?+00000000010.00 IS NOT IN RANGE FOR STATISTICAL CODE'
TAX+3'
MOA+39:0000000020000.00'
TAX+3'
MOA+40:0000000020000.00'
TAX+3'
MOA+210:0000000002210.20'
TAX+3'
MOA+68:0000000000102.00'
TAX+3'
MOA+128:0000000000185.00'
TAX+3'
MOA+346:0000000001800.00'
TAX+3'
MOA+26:0000000000033.00'
TAX+3'
MOA+23:0000000000152.00'
TAX+3'
MOA+55:0000000002000.00'
DOC+1+1'
CST+1+N10::95'
FTX+AAF+++10%'
TAX+1'
MOA+40:0000000020000.00'
TAX+1'
MOA+210:0000000002210.20'
TAX+1'
MOA+56:0000000022102.00'
TAX+1'
MOA+68:0000000000102.00'
TAX+1'
MOA+55:0000000002000.00'
CNT+5:1'
UNT+55+000002'".Replace("\r\n", "");

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "AUT";
			org.AUIsDutyDeferred = true;
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_DeclarationReference = "B00122382";

			var entryHeader = SetUpEntryHeader();
			entryHeader.CH_BGMReference = "B00122382/1";
			Factory.Save();

			var outgoing = Factory.New<CMRIMDMessage>();
			entryHeader.Messages.Add(outgoing);
			outgoing.EM_MessageText = "UNH+1+CUSDEC:D:99B:UN'BGM+929:::IMD+B00122382/1:1+9'CST++10::95'LOC+8+AUMEL::6'LOC+9+SGSIN::6'LOC+12+AUMEL::6'LOC+79+AUMEL::6'DTM+7:20041229:102'DTM+252:20041229:102'DTM+260:20041229:102'MEA+AAE+G+KG:123.00'RFF+ABQ'RFF+APH:FOB'PAC+0'PAC++1'TDT+20+123+S(SEA)+++++9044748::11'NAD+AT+31006626946::95'NAD+CB+00657C'MOA+141:150.00:AUD'MOA+40:150.00:AUD'MOA+63:150.00:AUD'MOA+39:150.00:AUD'UNS+D'DMS+1'RFF+BH:HOUSE BILL'RFF+MB:OCEANBILL'CST+1+LINE ACTION CODE::95+N10::95'FTX+AAA+++REFILLABLE, LIQUID INK REFILL'LOC+27+US::5'NAD+SU+2997664Y::95'PCI+1'MOA+40:150.00:AUD'MOA+38:150.00:AUD'MOA+5:0.03:AUD'RFF+ABA'RFF+ABC'RFF+ABD:96081000'RFF+ABL'RFF+AEA:45'RFF+AES'RFF+AFD'UNT+42+1'";
			outgoing.EM_LinkedObject = entryHeader;
			outgoing.EM_Status = EDIMessage.Status.Sent;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			outgoing.GetLogs().AddNew(Events.AddedARecordToTheSystem, new ZDateTimeOffset(2005, 1, 1));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			incomingMessage = (EDIMessage)Factory.New(IncomingMessageType);
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			incomingMessage.EM_MessageText = responseMessage;
			processor.ProcessMessage(incomingMessage);

			AssertEquals("Added a DTD charge.", 1800m, entryHeader.Charges[ChargeTypes.DutyDeferredAmount].C1_ChargeAmount);
			AssertEquals("Total Payable Amount", 385m, entryHeader.TotalAmountPayable);
		}

		public void TestDutyDeferredAmountRemoved()
		{
			string responseMessage = @"UNH+000002+CUSRES:D:99B:UN'
BGM+961:::IMDR+1H84 G0AE 2DB8:1+11'
FTX+AHN+++FINALISED:FINALISED'
GIS+N:117:95'
GIS+TLB:109:95'
GIS+LLB:109:95'
NAD+MR+AAA374M::95'
NAD+VT+AA33HF::95'
NAD+CB+54321::95'
NAD+IM++MICHAEL J BOWEN & ASSOCIATES P'
NAD+CB++WISETECH GLOBAL LIMITED'
RFF+ABO:B00122382/1/CMT3::3'
RFF+ABT:AAAEWRYYN::1'
RFF+ABQ:CLOTHES'
RFF+ADU:B00122382/1 HB123'
RFF+AAE:N10'
ERP+::0'
ERC+ID0545::95'
FTX+AAO+++AIR WAYBILL HAS NOT BEEN REPORTED'
ERP+::1'
ERC+ID0288::95'
FTX+AAO+++UNIT VALUE FOR Quantity=?+00000000010.00 IS NOT IN RANGE FOR STATISTICAL CODE'
TAX+3'
MOA+39:0000000020000.00'
TAX+3'
MOA+40:0000000020000.00'
TAX+3'
MOA+210:0000000002210.20'
TAX+3'
MOA+68:0000000000102.00'
TAX+3'
MOA+128:0000000000185.00'
TAX+3'
MOA+26:0000000000033.00'
TAX+3'
MOA+23:0000000000152.00'
TAX+3'
MOA+55:0000000002000.00'
DOC+1+1'
CST+1+N10::95'
FTX+AAF+++10%'
TAX+1'
MOA+40:0000000020000.00'
TAX+1'
MOA+210:0000000002210.20'
TAX+1'
MOA+56:0000000022102.00'
TAX+1'
MOA+68:0000000000102.00'
TAX+1'
MOA+55:0000000002000.00'
CNT+5:1'
UNT+55+000002'".Replace("\r\n", "");

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "AUT";
			org.AUIsDutyDeferred = true;
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_DeclarationReference = "B00122382";

			var entryHeader = SetUpEntryHeader();
			entryHeader.CH_BGMReference = "B00122382/1";
			entryHeader.Charges[ChargeTypes.DutyDeferredAmount].C1_ChargeAmount = 2000m;
			Factory.Save();

			var outgoing = Factory.New<CMRIMDMessage>();
			entryHeader.Messages.Add(outgoing);
			outgoing.EM_MessageText = "UNH+1+CUSDEC:D:99B:UN'BGM+929:::IMD+B00122382/1:1+9'CST++10::95'LOC+8+AUMEL::6'LOC+9+SGSIN::6'LOC+12+AUMEL::6'LOC+79+AUMEL::6'DTM+7:20041229:102'DTM+252:20041229:102'DTM+260:20041229:102'MEA+AAE+G+KG:123.00'RFF+ABQ'RFF+APH:FOB'PAC+0'PAC++1'TDT+20+123+S(SEA)+++++9044748::11'NAD+AT+31006626946::95'NAD+CB+00657C'MOA+141:150.00:AUD'MOA+40:150.00:AUD'MOA+63:150.00:AUD'MOA+39:150.00:AUD'UNS+D'DMS+1'RFF+BH:HOUSE BILL'RFF+MB:OCEANBILL'CST+1+LINE ACTION CODE::95+N10::95'FTX+AAA+++REFILLABLE, LIQUID INK REFILL'LOC+27+US::5'NAD+SU+2997664Y::95'PCI+1'MOA+40:150.00:AUD'MOA+38:150.00:AUD'MOA+5:0.03:AUD'RFF+ABA'RFF+ABC'RFF+ABD:96081000'RFF+ABL'RFF+AEA:45'RFF+AES'RFF+AFD'UNT+42+1'";
			outgoing.EM_LinkedObject = entryHeader;

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			outgoing.GetLogs().AddNew(Events.AddedARecordToTheSystem, new ZDateTimeOffset(2005, 1, 1));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			incomingMessage = (EDIMessage)Factory.New(IncomingMessageType);
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			incomingMessage.EM_MessageText = responseMessage;
			processor.ProcessMessage(incomingMessage);

			AssertEquals("Removed a DTD charge.", 0m, entryHeader.Charges[ChargeTypes.DutyDeferredAmount].C1_ChargeAmount);
			AssertEquals("Total Payable Amount", 2185m, entryHeader.TotalAmountPayable);
		}

		public void TestDutyDeferralPlus()
		{
			string responseMessage = @"UNH+000002+CUSRES:D:99B:UN'
BGM+961:::IMDR+1H84 G0AE 2DB8:1+11'
FTX+AHN+++FINALISED:FINALISED'
GIS+N:117:95'
GIS+TLB:109:95'
GIS+LLB:109:95'
NAD+MR+AAA374M::95'
NAD+VT+AA33HF::95'
NAD+CB+54321::95'
NAD+IM++MICHAEL J BOWEN & ASSOCIATES P'
NAD+CB++WISETECH GLOBAL LIMITED'
RFF+ABO:B00122382/1/CMT3::3'
RFF+ABT:AAAEWRYYN::1'
RFF+ABQ:CLOTHES'
RFF+ADU:B00122382/1 HB123'
RFF+AAE:N10'
ERP+::0'
ERC+ID0545::95'
FTX+AAO+++AIR WAYBILL HAS NOT BEEN REPORTED'
ERP+::1'
ERC+ID0288::95'
FTX+AAO+++UNIT VALUE FOR Quantity=?+00000000010.00 IS NOT IN RANGE FOR STATISTICAL CODE'
TAX+3'
MOA+39:0000000020000.00'
TAX+3'
MOA+40:0000000020000.00'
TAX+3'
MOA+210:0000000002210.20'
TAX+3'
MOA+58:0000000000066.00'
TAX+3'
MOA+68:0000000000102.00'
TAX+3'
MOA+346:0000000002621.00'
TAX+3'
MOA+26:0000000000033.00'
TAX+3'
MOA+23:0000000000152.00'
TAX+3'
MOA+55:0000000002000.00'
DOC+1+1'
CST+1+N10::95'
FTX+AAF+++10%'
TAX+1'
MOA+40:0000000020000.00'
TAX+1'
MOA+210:0000000002210.20'
TAX+1'
MOA+56:0000000022102.00'
TAX+1'
MOA+68:0000000000102.00'
TAX+1'
MOA+149:0000000000091.00'
TAX+1'
MOA+371:0000000000092.00'
TAX+1'
MOA+125:0000000000093.00'
TAX+1'
MOA+122:0000000000094.00'
TAX+1'
MOA+55:0000000002000.00'
CNT+5:1'
UNT+55+000002'".Replace("\r\n", "");

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "AUT";
			org.AUIsDutyDeferred = true;
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_DeclarationReference = "B00122382";

			var entryHeader = SetUpEntryHeader();
			entryHeader.CH_BGMReference = "B00122382/1";
			Factory.Save();

			var outgoing = Factory.New<CMRIMDMessage>();
			entryHeader.Messages.Add(outgoing);
			outgoing.EM_MessageText = "UNH+1+CUSDEC:D:99B:UN'BGM+929:::IMD+B00122382/1:1+9'CST++10::95'LOC+8+AUMEL::6'LOC+9+SGSIN::6'LOC+12+AUMEL::6'LOC+79+AUMEL::6'DTM+7:20041229:102'DTM+252:20041229:102'DTM+260:20041229:102'MEA+AAE+G+KG:123.00'RFF+ABQ'RFF+APH:FOB'PAC+0'PAC++1'TDT+20+123+S(SEA)+++++9044748::11'NAD+AT+31006626946::95'NAD+CB+00657C'MOA+141:150.00:AUD'MOA+40:150.00:AUD'MOA+63:150.00:AUD'MOA+39:150.00:AUD'UNS+D'DMS+1'RFF+BH:HOUSE BILL'RFF+MB:OCEANBILL'CST+1+LINE ACTION CODE::95+N10::95'FTX+AAA+++REFILLABLE, LIQUID INK REFILL'LOC+27+US::5'NAD+SU+2997664Y::95'PCI+1'MOA+40:150.00:AUD'MOA+38:150.00:AUD'MOA+5:0.03:AUD'RFF+ABA'RFF+ABC'RFF+ABD:96081000'RFF+ABL'RFF+AEA:45'RFF+AES'RFF+AFD'UNT+42+1'";
			outgoing.EM_LinkedObject = entryHeader;
			outgoing.EM_Status = EDIMessage.Status.Sent;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			outgoing.GetLogs().AddNew(Events.AddedARecordToTheSystem, new ZDateTimeOffset(2005, 1, 1));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			incomingMessage = (EDIMessage)Factory.New(IncomingMessageType);
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			incomingMessage.EM_MessageText = responseMessage;
			processor.ProcessMessage(incomingMessage);

			AssertEquals("Added a DTD charge.", 2621m, entryHeader.Charges[ChargeTypes.DutyDeferredAmount].C1_ChargeAmount);
			AssertEquals("Quarantine Processing Charge (26)", 33.0m, entryHeader.AQISProcessingCharge);
			AssertEquals("Declaration Processing Charge (23)", 152.0m, entryHeader.DeclarationProcessingCharge);
			AssertEquals("Wood Levy (58)", 66.0m, entryHeader.WoodLevy);
			AssertEquals("Wine Equalisation Tax (149)", 91.0m, entryHeader.WETAmount);
			AssertEquals("Wine Equalisation Tax WH (149)", 91.0m, entryHeader.WETAmountIncludingWHEstimate);
			AssertEquals("Luxury Car Tax (371)", 92.0m, entryHeader.LCTAmount);
			AssertEquals("Dumping Duty (125)", 93.0m, entryHeader.DumpingDuty);
			AssertEquals("Countervailing Duty (122)", 94.0m, entryHeader.CountervailingDuty);
			AssertEquals("All Other Duties", 187.0m, entryHeader.AllOtherDuties);

			AssertEquals("Total Amount", 0m, entryHeader.CH_TotalPaid);
			AssertEquals("Total Payable Amount", 0m, entryHeader.TotalAmountPayable);  // all deferrable charges add up to the Duty Deferred Amount.

			AssertEquals("Line Duty - summed (55)", 2000.0m, entryHeader.DutyAmount);
			AssertEquals("Line Duty WH - summed (55)", 2000.0m, entryHeader.DutyAmountIncludingWHEstimate);
			AssertEquals("Total Duty Amount", 2000m, entryHeader.TotalDutyAmount);
			AssertEquals("Total Deferred Duty", 2000m, entryHeader.DeferredDuty);
			AssertEquals("Payable Duty", 0m, entryHeader.PayableDuty);

			Factory.Save();
			AssertEquals("Paid since the value at MOA+128 is zero.", true, entryHeader.IsCustomsChargePaid);
		}

		public void TestDutyDeferralPlusWithExcise()
		{
			string responseMessage = @"UNH+000002+CUSRES:D:99B:UN'
BGM+961:::IMDR+1H84 G0AE 2DB8:1+11'
FTX+AHN+++FINALISED:FINALISED'
GIS+N:117:95'
GIS+TLB:109:95'
GIS+LLB:109:95'
NAD+MR+AAA374M::95'
NAD+VT+AA33HF::95'
NAD+CB+54321::95'
NAD+IM++MICHAEL J BOWEN & ASSOCIATES P'
NAD+CB++WISETECH GLOBAL LIMITED'
RFF+ABO:B00122382/1/CMT3::3'
RFF+ABT:AAAEWRYYN::1'
RFF+ABQ:CLOTHES'
RFF+ADU:B00122382/1 HB123'
RFF+AAE:N10'
ERP+::0'
ERC+ID0545::95'
FTX+AAO+++AIR WAYBILL HAS NOT BEEN REPORTED'
ERP+::1'
ERC+ID0288::95'
FTX+AAO+++UNIT VALUE FOR Quantity=?+00000000010.00 IS NOT IN RANGE FOR STATISTICAL CODE'
TAX+3'
MOA+39:0000000020000.00'
TAX+3'
MOA+40:0000000020000.00'
TAX+3'
MOA+210:0000000002210.20'
TAX+3'
MOA+58:0000000000066.00'
TAX+3'
MOA+68:0000000000102.00'
TAX+3'
MOA+128:0000000000100.00'
TAX+3'
MOA+346:0000000002521.00'
TAX+3'
MOA+26:0000000000033.00'
TAX+3'
MOA+23:0000000000152.00'
TAX+3'
MOA+55:0000000002000.00'
DOC+1+1'
CST+1+N10::95'
FTX+AAF+++10%'
TAX+1'
MOA+40:0000000020000.00'
TAX+1'
MOA+210:0000000002210.20'
TAX+1'
MOA+56:0000000022102.00'
TAX+1'
MOA+68:0000000000102.00'
TAX+1'
MOA+149:0000000000091.00'
TAX+1'
MOA+371:0000000000092.00'
TAX+1'
MOA+125:0000000000093.00'
TAX+1'
MOA+122:0000000000094.00'
TAX+1'
MOA+55:0000000002000.00'
CNT+5:1'
UNT+55+000002'".Replace("\r\n", "");

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "AUT";
			org.AUIsDutyDeferred = true;
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_DeclarationReference = "B00122382";

			var entryHeader = SetUpEntryHeader();
			entryHeader.CH_BGMReference = "B00122382/1";
			Factory.Save();

			var outgoing = Factory.New<CMRIMDMessage>();
			entryHeader.Messages.Add(outgoing);
			outgoing.EM_MessageText = "UNH+1+CUSDEC:D:99B:UN'BGM+929:::IMD+B00122382/1:1+9'CST++10::95'LOC+8+AUMEL::6'LOC+9+SGSIN::6'LOC+12+AUMEL::6'LOC+79+AUMEL::6'DTM+7:20041229:102'DTM+252:20041229:102'DTM+260:20041229:102'MEA+AAE+G+KG:123.00'RFF+ABQ'RFF+APH:FOB'PAC+0'PAC++1'TDT+20+123+S(SEA)+++++9044748::11'NAD+AT+31006626946::95'NAD+CB+00657C'MOA+141:150.00:AUD'MOA+40:150.00:AUD'MOA+63:150.00:AUD'MOA+39:150.00:AUD'UNS+D'DMS+1'RFF+BH:HOUSE BILL'RFF+MB:OCEANBILL'CST+1+LINE ACTION CODE::95+N10::95'FTX+AAA+++REFILLABLE, LIQUID INK REFILL'LOC+27+US::5'NAD+SU+2997664Y::95'PCI+1'MOA+40:150.00:AUD'MOA+38:150.00:AUD'MOA+5:0.03:AUD'RFF+ABA'RFF+ABC'RFF+ABD:96081000'RFF+ABL'RFF+AEA:45'RFF+AES'RFF+AFD'UNT+42+1'";
			outgoing.EM_LinkedObject = entryHeader;
			outgoing.EM_Status = EDIMessage.Status.Sent;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			outgoing.GetLogs().AddNew(Events.AddedARecordToTheSystem, new ZDateTimeOffset(2005, 1, 1));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			incomingMessage = (EDIMessage)Factory.New(IncomingMessageType);
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			incomingMessage.EM_MessageText = responseMessage;
			processor.ProcessMessage(incomingMessage);

			AssertEquals("Added a DTD charge.", 2521m, entryHeader.Charges[ChargeTypes.DutyDeferredAmount].C1_ChargeAmount);
			AssertEquals("Deferred Duty Amount", 2521m, entryHeader.TotalDeferredDutyFromCustoms);

			AssertEquals("Quarantine Processing Charge (26)", 33.0m, entryHeader.AQISProcessingCharge);
			AssertEquals("Declaration Processing Charge (23)", 152.0m, entryHeader.DeclarationProcessingCharge);
			AssertEquals("Wood Levy (58)", 66.0m, entryHeader.WoodLevy);

			AssertEquals("Wine Equalisation Tax (149)", 91.0m, entryHeader.WETAmount);
			AssertEquals("Wine Equalisation Tax WH (149)", 91.0m, entryHeader.WETAmountIncludingWHEstimate);
			AssertEquals("Luxury Car Tax (371)", 92.0m, entryHeader.LCTAmount);
			AssertEquals("Dumping Duty (125)", 93.0m, entryHeader.DumpingDuty);
			AssertEquals("Countervailing Duty (122)", 94.0m, entryHeader.CountervailingDuty);
			AssertEquals("All Other Duties", 187.0m, entryHeader.AllOtherDuties);

			AssertEquals("Line Duty - summed (55)", 2000.0m, entryHeader.DutyAmount);
			AssertEquals("Line Duty WH - summed (55)", 2000.0m, entryHeader.DutyAmountIncludingWHEstimate);

			AssertEquals("Total Amount", 100m, entryHeader.CH_TotalPaid);
			AssertEquals("Total Payable Amount", 100m, entryHeader.TotalAmountPayable);  // all deferrable charges add up to less than the Duty Deferred Amount.

			AssertEquals("Total Duty Amount", 2000m, entryHeader.TotalDutyAmount);
			AssertEquals("Total Deferred Duty", 1900m, entryHeader.DeferredDuty);
			AssertEquals("Payable Duty", 100m, entryHeader.PayableDuty);

			Factory.Save();
			AssertEquals("Deemed as Paid due to duty deferral despite the value at MOA+128 is $100.", true, entryHeader.IsCustomsChargePaid);
		}

		public void TestFindByLineNumber()
		{
			string responseWithoutLine1 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::IMDR+4A1G 4H8A 89G5:1+11'
FTX+AHN+++CLEAR:CLEAR'
GIS+N:117:95'
GIS+TLB:109:95'
GIS+LLB:109:95'
NAD+MR+AAA374M::95'
NAD+VT+AA33HF::95'
NAD+CB+54321::95'
NAD+IM++AUSTRALIAN CUSTOMS SERVICE'
NAD+CB++EAGLE DATAMATION INTERNATIONAL PTY:LIMIT'
RFF+ABO:B00148580/1/SYD2::12'
RFF+ABT:AAAA9JARY::3'
RFF+ABQ:373'
RFF+ADU:B00148580/1'
RFF+AAE:N10'
ERP+::0'
ERC+ID0545::95'
FTX+AAO+++AIR WAYBILL HAS NOT BEEN REPORTED'
TAX+3'
MOA+39:0000000001100.00'
TAX+3'
MOA+40:0000000001100.00'
TAX+3'
MOA+55:0000000000000.54'
TAX+3'
MOA+369:0000000000126.55'
TAX+3'
MOA+68:0000000000165.00'
TAX+3'
MOA+128:0000000000000.30'
DOC+1+1'
CST+2+N10::95'
FTX+AAF+++$0.05449/LITRE'
TAX+1'
GIS+LAQ:109:95'
TAX+1'
MOA+40:0000000001100.00'
TAX+1'
MOA+55:0000000000000.54'
TAX+1'
MOA+369:0000000000126.55'
TAX+1'
MOA+56:0000000001265.54'
TAX+1'
MOA+68:0000000000165.00'
ERP+::218'
ERC+2::95'
FTX+ABS+++GOODS (CHEMICALS) MAY BE REGULATED BY NICNAS. RING 1800638528'
CNT+5:1'
UNT+51+000001'".Replace("\r\n", "");

			SetUpEntryHeader();
			entryHeader.Declaration.JE_DeclarationReference = "B00148580";
			entryHeader.CH_BGMReference = "B00148580/1";
			entryLine2.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DutyAmount, 1m);
			entryLine2.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.GSTAmount, 10m);

			incomingMessage = (EDIMessage)Factory.New(IncomingMessageType);
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			incomingMessage.EM_MessageText = responseWithoutLine1;
			incomingMessage.EM_LinkedObject = entryHeader;
			processor = (CMRHeaderAndLineChargesResponseProcessor)GetMessageProcessor();
			processor.ProcessMessage(incomingMessage);

			AssertEquals("EntryLine2 has duty amount set by Processor", 0.54m, entryLine2.DutyAmount);
			AssertEquals("EntryLine2 has GST amount set by Processor", 126.55m, entryLine2.GSTVATAmount);
			AssertEquals("Entry Line2 has TotalDutyAndGST set by Processor", 127.09m, entryLine2.Fees.GetOrAddFeeByFeeType(CusEntryChargeTypeList.Codes.TotalDutyTaxForLine).CF_ChargeAmount);
		}

		public void TestAllLineLevelCharges()
		{
			string responseMessage = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::IMDR+20BE E99A 3365:1+11'
FTX+AHN+++CLEAR:CLEAR'
GIS+N:117:95'
GIS+TLB:109:95'
GIS+LLB:109:95'
NAD+MR+AAA447Y::95'
NAD+VT+AA33JL::95'
NAD+CB+54333::95'
NAD+IM++PORTER DATA MANAGEMENT PTY LTD'
NAD+CB++SOFTWARE CRAFT PTY LTD'
RFF+ABO:B00122382/1/SYD4::4'
RFF+ABT:AAAA3JMYK::2'
RFF+ABQ:AJAXWIDGET/50'
RFF+ADU:B00122382'
RFF+AAE:N10'
ERP+::0'
ERC+ID0548::95'
FTX+AAO+++BILL OF LADING NOT REPORTED FOR VESSEL VOYAGE'
TAX+3'
MOA+39:0000000002631.57'
TAX+3'
MOA+40:0000000002631.57'
TAX+3'
MOA+55:0000000000197.36'
TAX+3'
MOA+369:0000000000309.20'
TAX+3'
MOA+68:0000000000263.16'
TAX+3'
MOA+128:0000000000566.31'
TAX+3'
MOA+26:0000000000006.50'
TAX+3'
MOA+35:0000000000003.75'
TAX+3'
MOA+23:0000000000049.50'
DOC+1+1'
CST+1+N10::95'
FTX+AAF+++7.5%'
TAX+1'
GIS+LAQ:109:95'
TAX+1'
MOA+40:0000000001315.79'
TAX+1'
MOA+Z01:0000000009876.54'
TAX+1'
MOA+292:0000000009450.45'
TAX+1'
MOA+55:0000000000098.68'
TAX+1'
MOA+56:0000000001546.05'
TAX+1'
MOA+68:0000000000131.58'
TAX+1'
MOA+122:000000000001.79'
TAX+1'
MOA+125:000000000002.79'
TAX+1'
MOA+149:000000000003.79'
TAX+1'
MOA+210:000000000004.79'
TAX+1'
MOA+369:0000000000154.60'
TAX+1'
MOA+371:000000000005.79'
UNT+72+000001'".Replace("\r\n", "");

			CusEntryHeader entryHeader = SetUpEntryHeader();
			entryHeader.CH_BGMReference = "B00122382/1";
			entryHeader.Declaration.JE_DeclarationReference = "B00122382";
			CusEntryLine entryLine = entryHeader.MergedLines.FindByLineNumber(1);

			incomingMessage = (EDIMessage)Factory.New(IncomingMessageType);
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			incomingMessage.EM_MessageText = responseMessage;
			processor.ProcessMessage(incomingMessage);

			AssertEquals("Customs value", 1315.79m, entryLine.CL_CustomsValue);
			AssertEquals("Duty", 98.68m, entryLine.Fees.GetAmount(ChargeTypes.DutyAmount));
			AssertEquals("CountervailingDuty", 1.79m, entryLine.Fees.GetAmount(ChargeTypes.CountervailingDuty));
			AssertEquals("DumpingDuty", 2.79m, entryLine.Fees.GetAmount(ChargeTypes.DumpingDuty));
			AssertEquals("WetAmount", 3.79m, entryLine.Fees.GetAmount(ChargeTypes.WetAmount));
			AssertEquals("GSTDeferred", 4.79m, entryLine.Fees.GetAmount(ChargeTypes.GSTDeferred));
			AssertEquals("GSTAmount", 154.60m, entryLine.Fees.GetAmount(ChargeTypes.GSTAmount));
			AssertEquals("LCTAmount", 5.79m, entryLine.Fees.GetAmount(ChargeTypes.LCTAmount));
			AssertEquals("TotalLinePayable", 267.44m, entryLine.Fees.GetAmount(ChargeTypes.TotalDutyTaxForLine));
			AssertEquals("SecurityConcession", 9450.45m, entryLine.Fees.GetAmount(ChargeTypes.SecurityConcession));
			AssertEquals("SecurityLiability", 9876.54m, entryLine.Fees.GetAmount(ChargeTypes.SecurityLiability));
			AssertEquals("T&I", 131.58m, entryLine.CustomsCalculatedTransportAndInsuranceInLocalCurrency.Amount);
		}

		public void TestWeeklySettlementResponse()
		{
			string responseMessage = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::IMDR+G22F 552G 0BF:1+11'
FTX+AHN+++CLEAR:CLEAR'
GIS+N:117:95'
GIS+TLB:109:95'
GIS+LLB:109:95'
NAD+MR+FGP767W::95'
NAD+VT+AA63CM::95'
NAD+CB+2582::95'
NAD+IM++COCA-COLA AMATIL (AUST) PTY LT'
NAD+CB++SELECT CUSTOMS SERVICES PTY. LTD.'
RFF+ABO:B00024124/1/MEL1::1'
RFF+ABT:ACNAP7M4K::1'
RFF+ABQ:WEEKLY SETTLEMENT'
RFF+ADU:B00024124/1'
RFF+AAE:N30'
ERP+::59'
ERC+ID0288::95'
FTX+AAO+++UNIT VALUE FOR Quantity=?+00000000004.89 IS NOT IN RANGE FOR STATISTICAL CODE'
TAX+3'
MOA+40:0000000970078.97'
TAX+3'
MOA+55:0000002737640.54'
TAX+3'
MOA+210:0000000377001.98'
TAX+3'
MOA+128:0000002737640.54'
DOC+1'
CST+1+N30::95'
FTX+AAF+++$47.47/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000005523.56'
TAX+1'
MOA+55:0000000004849.91'
TAX+1'
MOA+210:0000000001070.94'
TAX+1'
MOA+56:0000000010709.46'
TAX+1'
MOA+68:0000000000335.99'
TAX+1'
MOA+146:000000054.0635'
TAX+1'
MOA+312:000000003.2886'
CST+2+N30::95'
FTX+AAF+++$47.47/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000003847.01'
TAX+1'
MOA+55:0000000005353.71'
TAX+1'
MOA+210:0000000001086.87'
TAX+1'
MOA+56:0000000010868.75'
TAX+1'
MOA+68:0000000001668.03'
TAX+1'
MOA+146:000000034.1104'
TAX+1'
MOA+312:000000014.7899'
CST+3+N30::95'
FTX+AAF+++$47.47/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000001315.52'
TAX+1'
MOA+55:0000000001208.83'
TAX+1'
MOA+210:0000000000258.20'
TAX+1'
MOA+56:0000000002582.03'
TAX+1'
MOA+68:0000000000057.68'
TAX+1'
MOA+146:000000051.6596'
TAX+1'
MOA+312:000000002.2650'
CST+4+N30::95'
FTX+AAF+++$47.47/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000001475.95'
TAX+1'
MOA+55:0000000001089.34'
TAX+1'
MOA+210:0000000000259.14'
TAX+1'
MOA+56:0000000002591.41'
TAX+1'
MOA+68:0000000000026.12'
TAX+1'
MOA+146:000000064.3170'
TAX+1'
MOA+312:000000001.1382'
CST+5+N30::95'
FTX+AAF+++$47.47/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000002207.21'
TAX+1'
MOA+55:0000000001756.82'
TAX+1'
MOA+210:0000000000449.22'
TAX+1'
MOA+56:0000000004492.25'
TAX+1'
MOA+68:0000000000528.22'
TAX+1'
MOA+146:000000059.6395'
TAX+1'
MOA+312:000000014.2726'
CST+6+N30::95'
FTX+AAF+++$33.43/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000003155.01'
TAX+1'
MOA+55:0000000001630.91'
TAX+1'
MOA+210:0000000000506.10'
TAX+1'
MOA+56:0000000005061.07'
TAX+1'
MOA+68:0000000000275.15'
TAX+1'
MOA+146:000000064.6705'
TAX+1'
MOA+312:000000005.6399'
CST+7+N30::95'
FTX+AAF+++5%'
TAX+1'
MOA+40:0000000006408.88'
TAX+1'
MOA+55:0000000000320.44'
TAX+1'
MOA+210:0000000000713.00'
TAX+1'
MOA+56:0000000007130.06'
TAX+1'
MOA+68:0000000000400.74'
TAX+1'
MOA+146:000000057.7178'
TAX+1'
MOA+312:000000003.6090'
CST+8+N30::95'
FTX+AAF+++$80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000257362.28'
TAX+1'
MOA+55:0000000265844.40'
TAX+1'
MOA+210:0000000055088.08'
TAX+1'
MOA+56:0000000550880.83'
TAX+1'
MOA+68:0000000027674.15'
TAX+1'
MOA+146:000000077.8444'
TAX+1'
MOA+312:000000008.3706'
CST+9+N30::95'
FTX+AAF+++5% ?+ $75.10/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000023458.67'
TAX+1'
MOA+55:0000000023126.16'
TAX+1'
MOA+210:0000000004698.14'
TAX+1'
MOA+56:0000000046981.49'
TAX+1'
MOA+68:0000000000396.66'
TAX+1'
MOA+146:000000080.2499'
TAX+1'
MOA+312:000000001.3569'
CST+10+N30::95'
FTX+AAF+++5% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000059058.73'
TAX+1'
MOA+55:0000000319228.38'
TAX+1'
MOA+210:0000000038235.90'
TAX+1'
MOA+56:0000000382359.05'
TAX+1'
MOA+68:0000000004071.94'
TAX+1'
MOA+146:000000015.0151'
TAX+1'
MOA+312:000000001.0352'
CST+11+N30::95'
FTX+AAF+++5% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000011043.81'
TAX+1'
MOA+55:0000000022735.70'
TAX+1'
MOA+210:0000000003432.58'
TAX+1'
MOA+56:0000000034325.80'
TAX+1'
MOA+68:0000000000546.29'
TAX+1'
MOA+146:000000040.0312'
TAX+1'
MOA+312:000000001.9801'
CST+12+N30::95'
FTX+AAF+++5% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000065910.34'
TAX+1'
MOA+55:0000000105495.01'
TAX+1'
MOA+210:0000000017254.33'
TAX+1'
MOA+56:0000000172543.39'
TAX+1'
MOA+68:0000000001138.04'
TAX+1'
MOA+146:000000051.8578'
TAX+1'
MOA+312:000000000.8954'
CST+13+N30::95'
FTX+AAF+++5% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000002112.46'
TAX+1'
MOA+55:0000000000759.11'
TAX+1'
MOA+210:0000000000293.39'
TAX+1'
MOA+56:0000000002933.91'
TAX+1'
MOA+68:0000000000062.34'
TAX+1'
MOA+146:000000259.9310'
TAX+1'
MOA+312:000000007.6707''
CST+14+N30::95
FTX+AAF+++5% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000002284.95'
TAX+1'
MOA+55:0000000002545.84'
TAX+1'
MOA+210:0000000000487.18'
TAX+1'
MOA+56:0000000004871.82'
TAX+1'
MOA+68:0000000000041.03'
TAX+1'
MOA+146:000000075.5605'
TAX+1'
MOA+312:000000001.3568'
CST+15+N30::95'
FTX+AAF+++5% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000000333.55'
TAX+1'
MOA+55:0000000000943.80'
TAX+1'
MOA+210:0000000000129.45'
TAX+1'
MOA+56:0000000001294.53'
TAX+1'
MOA+68:0000000000017.18'
TAX+1'
MOA+146:000000028.9288'
TAX+1'
MOA+312:000000001.4900'
CST+16+N30::95'
FTX+AAF+++$80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000168453.90'
TAX+1'
MOA+55:0000001168768.83'
TAX+1'
MOA+210:0000000134690.58'
TAX+1'
MOA+56:0000001346905.81'
TAX+1'
MOA+68:0000000009683.08'
TAX+1'
MOA+146:000000011.5894'
TAX+1'
MOA+312:000000000.6661'
CST+17+N30::95'
FTX+AAF+++$80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000068969.51'
TAX+1'
MOA+55:0000000177236.50'
TAX+1'
MOA+210:0000000024963.14'
TAX+1'
MOA+56:0000000249631.45'
TAX+1'
MOA+68:0000000003425.44'
TAX+1'
MOA+146:000000031.2906'
TAX+1'
MOA+312:000000001.5540'
CST+18+N30::95'
FTX+AAF+++$80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000001805.46'
TAX+1'
MOA+55:0000000007936.46'
TAX+1'
MOA+210:0000000000981.24'
TAX+1'
MOA+56:0000000009812.48'
TAX+1'
MOA+68:0000000000070.56'
TAX+1'
MOA+146:000000018.2924'
TAX+1'
MOA+312:000000000.7148'
CST+19+N30::95'
FTX+AAF+++$80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000000090.64'
TAX+1'
MOA+55:0000000000193.62'
TAX+1'
MOA+210:0000000000028.75'
TAX+1'
MOA+56:0000000000287.53'
TAX+1'
MOA+68:0000000000003.27'
TAX+1'
MOA+146:000000037.6411'
TAX+1'
MOA+312:000000001.3579'
CST+20+N30::95'
FTX+AAF+++$80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000000979.90'
TAX+1'
MOA+55:0000000001822.25'
TAX+1'
MOA+210:0000000000282.64'
TAX+1'
MOA+56:0000000002826.40'
TAX+1'
MOA+68:0000000000024.25'
TAX+1'
MOA+146:000000043.2397'
TAX+1'
MOA+312:000000001.0700'
CST+21+N30::95'
FTX+AAF+++3% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000000171.77'
TAX+1'
MOA+55:0000000000439.36'
TAX+1'
MOA+210:0000000000062.73'
TAX+1'
MOA+56:0000000000627.38'
TAX+1'
MOA+68:0000000000016.25'
TAX+1'
MOA+146:000000031.8092'
TAX+1'
MOA+312:000000003.0092'
CST+22+N30::95'
FTX+AAF+++5% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000001948.41'
TAX+1'
MOA+55:0000000009418.54'
TAX+1'
MOA+210:0000000001142.70'
TAX+1'
MOA+56:0000000011427.04'
TAX+1'
MOA+68:0000000000060.09'
TAX+1'
MOA+146:000000016.8082'
TAX+1'
MOA+312:000000000.5183'
CST+23+N30::95'
FTX+AAF+++5% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000004938.38'
TAX+1'
MOA+55:0000000022671.65'
TAX+1'
MOA+210:0000000002830.65'
TAX+1'
MOA+56:0000000028306.54'
TAX+1'
MOA+68:0000000000696.51'
TAX+1'
MOA+146:000000017.7079'
TAX+1'
MOA+312:000000002.4975'
CST+24+N30::95'
FTX+AAF+++5% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000000302.80'
TAX+1'
MOA+55:0000000000369.74'
TAX+1'
MOA+210:0000000000068.43'
TAX+1'
MOA+56:0000000000684.31'
TAX+1'
MOA+68:0000000000011.77'
TAX+1'
MOA+146:000000068.6621'
TAX+1'
MOA+312:000000002.6689'
CST+25+N30::95'
FTX+AAF+++5% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000039582.87'
TAX+1'
MOA+55:0000000056516.42'
TAX+1'
MOA+210:0000000009696.22'
TAX+1'
MOA+56:0000000096962.26'
TAX+1'
MOA+68:0000000000862.97'
TAX+1'
MOA+146:000000058.3611'
TAX+1'
MOA+312:000000001.2723'
CST+26+N30::95'
FTX+AAF+++5% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000000916.04'
TAX+1'
MOA+55:0000000001153.04'
TAX+1'
MOA+210:0000000000209.22'
TAX+1'
MOA+56:0000000002092.20'
TAX+1'
MOA+68:0000000000023.12'
TAX+1'
MOA+146:000000066.5243'
TAX+1'
MOA+312:000000001.6790'
CST+27+N30::95'
FTX+AAF+++5% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000000865.33'
TAX+1'
MOA+55:0000000002301.17'
TAX+1'
MOA+210:0000000000318.73'
TAX+1'
MOA+56:0000000003187.37'
TAX+1'
MOA+68:0000000000020.87'
TAX+1'
MOA+146:000000030.8165'
TAX+1'
MOA+312:000000000.7432'
CST+28+N30::95'
FTX+AAF+++5% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000002375.66'
TAX+1'
MOA+55:0000000003360.91'
TAX+1'
MOA+210:0000000000579.54'
TAX+1'
MOA+56:0000000005795.44'
TAX+1'
MOA+68:0000000000058.87'
TAX+1'
MOA+146:000000058.9201'
TAX+1'
MOA+312:000000001.4600'
CST+29+N30::95'
FTX+AAF+++5% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000012167.96'
TAX+1'
MOA+55:0000000017484.84'
TAX+1'
MOA+210:0000000002996.07'
TAX+1'
MOA+56:0000000029960.76'
TAX+1'
MOA+68:0000000000307.96'
TAX+1'
MOA+146:000000057.9757'
TAX+1'
MOA+312:000000001.4673'
CST+30+N30::95'
FTX+AAF+++5% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000008676.93'
TAX+1'
MOA+55:0000000017667.31'
TAX+1'
MOA+210:0000000002655.45'
TAX+1'
MOA+56:0000000026554.52'
TAX+1'
MOA+68:0000000000210.28'
TAX+1'
MOA+146:000000040.4858'
TAX+1'
MOA+312:000000000.9811'
CST+31+N30::95'
FTX+AAF+++5% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000000036.27'
TAX+1'
MOA+55:0000000000103.85'
TAX+1'
MOA+210:0000000000014.10'
TAX+1'
MOA+56:0000000000141.07'
TAX+1'
MOA+68:0000000000000.95'
TAX+1'
MOA+146:000000028.5815'
TAX+1'
MOA+312:000000000.7486'
CST+32+N30::95'
FTX+AAF+++$80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000015306.79'
TAX+1'
MOA+55:0000000047833.57'
TAX+1'
MOA+210:0000000006459.07'
TAX+1'
MOA+56:0000000064590.73'
TAX+1'
MOA+68:0000000001450.37'
TAX+1'
MOA+146:000000025.7312'
TAX+1'
MOA+312:000000002.4381'
CST+33+N30::95'
FTX+AAF+++3% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000001624.61'
TAX+1'
MOA+55:0000000006187.88'
TAX+1'
MOA+210:0000000000794.20'
TAX+1'
MOA+56:0000000007942.06'
TAX+1'
MOA+68:0000000000129.57'
TAX+1'
MOA+146:000000021.2790'
TAX+1'
MOA+312:000000001.6970'
CST+34+N30::95'
FTX+AAF+++3% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000002305.07'
TAX+1'
MOA+55:0000000002984.17'
TAX+1'
MOA+210:0000000000535.34'
TAX+1'
MOA+56:0000000005353.47'
TAX+1'
MOA+68:0000000000064.23'
TAX+1'
MOA+146:000000063.5846'
TAX+1'
MOA+312:000000001.7717'
CST+35+N30::95'
FTX+AAF+++5% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000002227.72'
TAX+1'
MOA+55:0000000007487.23'
TAX+1'
MOA+210:0000000000976.97'
TAX+1'
MOA+56:0000000009769.78'
TAX+1'
MOA+68:0000000000054.83'
TAX+1'
MOA+146:000000024.2861'
TAX+1'
MOA+312:000000000.5977'
CST+36+N30::95'
FTX+AAF+++$80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000043702.46'
TAX+1'
MOA+55:0000000161393.96'
TAX+1'
MOA+210:0000000020760.96'
TAX+1'
MOA+56:0000000207609.66'
TAX+1'
MOA+68:0000000002513.24'
TAX+1'
MOA+146:000000021.7735'
TAX+1'
MOA+312:000000001.2521'
CST+37+N30::95'
FTX+AAF+++5% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000000652.88'
TAX+1'
MOA+55:0000000005098.47'
TAX+1'
MOA+210:0000000000579.41'
TAX+1'
MOA+56:0000000005794.16'
TAX+1'
MOA+68:0000000000042.81'
TAX+1'
MOA+146:000000010.3631'
TAX+1'
MOA+312:000000000.6795'
CST+38+N30::95'
FTX+AAF+++$80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000003050.83'
TAX+1'
MOA+55:0000000012414.90'
TAX+1'
MOA+210:0000000001563.09'
TAX+1'
MOA+56:0000000015630.93'
TAX+1'
MOA+68:0000000000165.20'
TAX+1'
MOA+146:000000019.7599'
TAX+1'
MOA+312:000000001.0699'
CST+39+N30::95'
FTX+AAF+++5% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000000023.95'
TAX+1'
MOA+55:0000000000126.15'
TAX+1'
MOA+210:0000000000015.15'
TAX+1'
MOA+56:0000000000151.58'
TAX+1'
MOA+68:0000000000001.48'
TAX+1'
MOA+146:000000015.4118'
TAX+1'
MOA+312:000000000.9523'
CST+40+N30::95'
FTX+AAF+++$80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000004326.82'
TAX+1'
MOA+55:0000000005639.95'
TAX+1'
MOA+210:0000000001019.04'
TAX+1'
MOA+56:0000000010190.41'
TAX+1'
MOA+68:0000000000223.64'
TAX+1'
MOA+146:000000061.6883'
TAX+1'
MOA+312:000000003.1884'
CST+41+N30::95'
FTX+AAF+++5% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000001538.32'
TAX+1'
MOA+55:0000000000948.23'
TAX+1'
MOA+210:0000000000254.05'
TAX+1'
MOA+56:0000000002540.59'
TAX+1'
MOA+68:0000000000054.04'
TAX+1'
MOA+146:000000141.9638'
TAX+1'
MOA+312:000000004.9870'
CST+42+N30::95'
FTX+AAF+++$80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000000054.41'
TAX+1'
MOA+55:0000000000374.87'
TAX+1'
MOA+210:0000000000043.05'
TAX+1'
MOA+56:0000000000430.59'
TAX+1'
MOA+68:0000000000001.31'
TAX+1'
MOA+146:000000011.6709'
TAX+1'
MOA+312:000000000.2809'
CST+43+N30::95'
FTX+AAF+++$80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000000024.12'
TAX+1'
MOA+55:0000000000195.87'
TAX+1'
MOA+210:0000000000022.07'
TAX+1'
MOA+56:0000000000220.77'
TAX+1'
MOA+68:0000000000000.78'
TAX+1'
MOA+146:000000009.9014'
TAX+1'
MOA+312:000000000.3201'
CST+44+N30::95'
FTX+AAF+++5% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000001524.59'
TAX+1'
MOA+55:0000000000456.97'
TAX+1'
MOA+210:0000000000198.69'
TAX+1'
MOA+56:0000000001986.96'
TAX+1'
MOA+68:0000000000005.40'
TAX+1'
MOA+146:000000321.9831'
TAX+1'
MOA+312:000000001.1404'
CST+45+N30::95'
FTX+AAF+++$80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000000178.40'
TAX+1'
MOA+55:0000000000472.81'
TAX+1'
MOA+210:0000000000066.08'
TAX+1'
MOA+56:0000000000660.85'
TAX+1'
MOA+68:0000000000009.64'
TAX+1'
MOA+146:000000030.3401'
TAX+1'
MOA+312:000000001.6394'
CST+46+N30::95'
FTX+AAF+++$80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000000736.35'
TAX+1'
MOA+55:0000000004052.66'
TAX+1'
MOA+210:0000000000482.73'
TAX+1'
MOA+56:0000000004827.31'
TAX+1'
MOA+68:0000000000038.30'
TAX+1'
MOA+146:000000014.6101'
TAX+1'
MOA+312:000000000.7599'
CST+47+N30::95'
FTX+AAF+++3% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000049039.76'
TAX+1'
MOA+55:0000000061556.76'
TAX+1'
MOA+210:0000000011165.66'
TAX+1'
MOA+56:0000000111656.69'
TAX+1'
MOA+68:0000000001060.17'
TAX+1'
MOA+146:000000065.6278'
TAX+1'
MOA+312:000000001.4187'
CST+48+N30::95'
FTX+AAF+++5% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000000150.47'
TAX+1'
MOA+55:0000000000210.15'
TAX+1'
MOA+210:0000000000036.78'
TAX+1'
MOA+56:0000000000367.89'
TAX+1'
MOA+68:0000000000007.27'
TAX+1'
MOA+146:000000059.7103'
TAX+1'
MOA+312:000000002.8849'
CST+49+N30::95'
FTX+AAF+++5% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000005010.49'
TAX+1'
MOA+55:0000000007666.89'
TAX+1'
MOA+210:0000000001279.59'
TAX+1'
MOA+56:0000000012795.99'
TAX+1'
MOA+68:0000000000118.61'
TAX+1'
MOA+146:000000054.3248'
TAX+1'
MOA+312:000000001.2859'
CST+50+N30::95'
FTX+AAF+++5% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000003958.66'
TAX+1'
MOA+55:0000000008060.10'
TAX+1'
MOA+210:0000000001214.49'
TAX+1'
MOA+56:0000000012144.91'
TAX+1'
MOA+68:0000000000126.15'
TAX+1'
MOA+146:000000040.4870'
TAX+1'
MOA+312:000000001.2901'
CST+51+N30::95'
FTX+AAF+++$80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000029549.58'
TAX+1'
MOA+55:0000000033400.70'
TAX+1'
MOA+210:0000000006322.66'
TAX+1'
MOA+56:0000000063226.67'
TAX+1'
MOA+68:0000000000276.39'
TAX+1'
MOA+146:000000071.1386'
TAX+1'
MOA+312:000000000.6653'
CST+52+N30::95'
FTX+AAF+++5% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000018803.72'
TAX+1'
MOA+55:0000000027682.94'
TAX+1'
MOA+210:0000000004805.72'
TAX+1'
MOA+56:0000000048057.22'
TAX+1'
MOA+68:0000000001570.56'
TAX+1'
MOA+146:000000056.5389'
TAX+1'
MOA+312:000000004.7223'
CST+53+N30::95'
FTX+AAF+++5% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000010079.27'
TAX+1'
MOA+55:0000000007065.41'
TAX+1'
MOA+210:0000000001732.01'
TAX+1'
MOA+56:0000000017320.12'
TAX+1'
MOA+68:0000000000175.44'
TAX+1'
MOA+146:000000123.5204'
TAX+1'
MOA+312:000000002.1500'
CST+54+N30::95'
FTX+AAF+++5% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000000238.37'
TAX+1'
MOA+55:0000000000373.76'
TAX+1'
MOA+210:0000000000061.62'
TAX+1'
MOA+56:0000000000616.27'
TAX+1'
MOA+68:0000000000004.14'
TAX+1'
MOA+146:000000052.9711'
TAX+1'
MOA+312:000000000.9200'
CST+55+N30::95'
FTX+AAF+++5% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000010975.37'
TAX+1'
MOA+55:0000000086532.78'
TAX+1'
MOA+210:0000000009869.93'
TAX+1'
MOA+56:0000000098699.30'
TAX+1'
MOA+68:0000000001191.15'
TAX+1'
MOA+146:000000010.2638'
TAX+1'
MOA+312:000000001.1139'
CST+56+N30::95'
FTX+AAF+++5% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000001039.65'
TAX+1'
MOA+55:0000000003699.38'
TAX+1'
MOA+210:0000000000476.73'
TAX+1'
MOA+56:0000000004767.35'
TAX+1'
MOA+68:0000000000028.32'
TAX+1''
MOA+146:000000022.9199
TAX+1'
MOA+312:000000000.6243'
CST+57+N30::95'
FTX+AAF+++5% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000000029.50'
TAX+1'
MOA+55:0000000000119.67'
TAX+1'
MOA+210:0000000000015.08'
TAX+1'
MOA+56:0000000000150.85'
TAX+1'
MOA+68:0000000000001.68'
TAX+1'
MOA+146:000000020.0680'
TAX+1'
MOA+312:000000001.1428'
CST+58+N30::95'
FTX+AAF+++5% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000000208.95'
TAX+1'
MOA+55:0000000000580.95'
TAX+1'
MOA+210:0000000000080.06'
TAX+1'
MOA+56:0000000000800.68'
TAX+1'
MOA+68:0000000000010.78'
TAX+1'
MOA+146:000000029.4503'
TAX+1'
MOA+312:000000001.5193''
CST+59+N30::95''
FTX+AAF+++5% ?+ $80.41/LITRE ALCOHOL'
TAX+1'
MOA+40:0000000005938.10'
TAX+1'
MOA+55:0000000000690.51'
TAX+1'
MOA+210:0000000000689.04'
TAX+1'
MOA+56:0000000006890.44'
TAX+1'
MOA+68:0000000000261.83'
TAX+1'
MOA+146:000001213.0949'
TAX+1'
MOA+312:000000053.4892'
ERP+::213'
ERC+9::95'
FTX+ABS+++AGE CERTIFICATE REQUIRED UNDER S10 OF SPIRITS ACT 1906.'
ERP+::214'
ERC+9::95'
ERC+10::95'
ERC+11::95'
ERC+12::95'
ERC+13::95'
ERC+14::95'
ERC+15::95'
ERC+16::95'
ERC+17::95'
ERC+18::95'
ERC+19::95'
ERC+20::95'
ERC+21::95'
ERC+36::95'
ERC+38::95'
ERC+39::95'
ERC+40::95'
ERC+41::95'
ERC+42::95'
ERC+43::95'
ERC+44::95'
ERC+45::95'
ERC+46::95'
ERC+51::95'
ERC+58::95'
ERC+59::95'
FTX+ABS+++AGE CERTIFICATE REQUIRED UNDER S105A OF CUSTOMS ACT 1901.'
ERP+::309'
ERC+16::95'
ERC+17::95'
ERC+18::95'
ERC+19::95'
ERC+20::95'
ERC+36::95'
ERC+38::95'
ERC+40::95'
ERC+45::95'
FTX+ABS+++TO CLAIM US PREFERENCE, KNOWLEDGE THAT THE IMPORTED GOODS MEET AUSFTA RULES OF ORIGIN (ROO) IS REQUIRED. A STATEMENT LIKE ""MADE IN THE US"" DOES NOT PROVIDE THIS KNOWLEDGE. FOR INFORMATION ABOUT THE AUSTRALIA-UNITED STATES FREE TRADE AGREEMENT WWW.BORDER.GOV.AU/BUSI/FREE/UNIT FOR INFORMATION ON OBTAINING KNOWLEDGE WWW.BORDER.GOV.AU/CUSTOMSNOTICES/DOCUMENTS/ACN0554.PDF'
ERP+::397'
ERC+46::95'
ERC+51::95'
FTX+ABS+++TO CLAIM JAPANESE PREFERENCE RATES UNDER THE JAPAN -AUSTRALIA ECONOMIC PARTNERSHIP AGREEMENT (JAEPA) A CERTIFICATE OF ORIGIN OR AN ORIGIN CERTIFICATION DOCUMENT, OR A COPY OF ONE, IS NECESSARY FOR EACH SHIPMENT.  DETAILED INFORMATION IS AVAILABLE AT WWW.BORDER.GOV.AU/BUSI/FREE/JAPA'
CNT+5:59'
UNT+1020+000001'".Replace("\r\n", "");

			var entryHeader = SetUpEntryHeader();
			entryHeader.CH_BGMReference = "B00024124/1";
			entryHeader.Declaration.JE_DeclarationReference = "B00024124";
			entryHeader.Declaration.JE_SettlementPeriodType = "SW";
			var entryLine1 = entryHeader.MergedLines.FindByLineNumber(1);
			var entryLine2 = entryHeader.MergedLines.FindByLineNumber(2);

			incomingMessage = (EDIMessage)Factory.New(IncomingMessageType);
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			incomingMessage.EM_MessageText = responseMessage;
			processor.ProcessMessage(incomingMessage);

			AssertEquals("Customs value", 5523.56m, entryLine1.CL_CustomsValue);
			AssertEquals("Duty - line 1", 4849.91m, entryLine1.Fees.GetAmount(ChargeTypes.DutyAmount));
			AssertEquals("CountervailingDuty", 0m, entryLine1.Fees.GetAmount(ChargeTypes.CountervailingDuty));
			AssertEquals("DumpingDuty", 0m, entryLine1.Fees.GetAmount(ChargeTypes.DumpingDuty));
			AssertEquals("WetAmount", 0m, entryLine1.Fees.GetAmount(ChargeTypes.WetAmount));
			AssertEquals("GSTDeferred", 1070.94m, entryLine1.Fees.GetAmount(ChargeTypes.GSTDeferred));
			AssertEquals("GSTAmount", 0m, entryLine1.Fees.GetAmount(ChargeTypes.GSTAmount));
			AssertEquals("LCTAmount", 0m, entryLine1.Fees.GetAmount(ChargeTypes.LCTAmount));
			AssertEquals("Entry Fee", 0m, entryLine1.Fees.GetAmount(ChargeTypes.EntryFee));
			AssertEquals("Entry Fee (DeclarationProcessingCharge)", 0m, entryLine1.Fees.GetAmount(ChargeTypes.DeclarationProcessingCharge));
			AssertEquals("TotalLinePayable", 4849.91m, entryLine1.Fees.GetAmount(ChargeTypes.TotalDutyTaxForLine));

			AssertEquals("Entry Fee", 0m, entryLine2.Fees.GetAmount(ChargeTypes.EntryFee));
			AssertEquals("Entry Fee (DeclarationProcessingCharge)", 0m, entryLine2.Fees.GetAmount(ChargeTypes.DeclarationProcessingCharge));
			AssertEquals("TotalLinePayable", 5353.71m, entryLine2.Fees.GetAmount(ChargeTypes.TotalDutyTaxForLine));

			AssertEquals("CH_TotalPaid should not include any entry fee", 10203.62M, entryHeader.CH_TotalPaid);
		}

		protected CusEntryHeader SetUpEntryHeader()
		{
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_TotalPaid = 0m;
			entryHeader.Charges.RemoveAndDeleteAll();

			CusEntryLine entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = (short)1;

			CusEntryLine entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = (short)2;

			return entryHeader;
		}
	}
}
