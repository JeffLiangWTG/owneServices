using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Messaging.Business;
#pragma warning disable IDE0001 // Prevent simplification to Enterprise.Registry.Business.Customs.AU.EntryChargeTypeList.Codes
using ChargeTypes = Enterprise.Customs.AU.Declaration.Business.CusEntryChargeTypeList.Codes;
#pragma warning restore IDE0001 // Prevent simplification to Enterprise.Registry.Business.Customs.AU.EntryChargeTypeList.Codes

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class CMRHeaderChargesResponseProcessorTest : BaseImportDeclarationMessageProcessorTest
	{
		public void TestUpdateEntryFeesForOriginalVsAmendment()
		{
			entryHeader.CH_BGMReference = "B00149481/1";
			entryHeader.Declaration.JE_DeclarationReference = "B00149481";

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingFormalLodge.Code;
			entryHeader.Charges[ChargeTypes.AQISProcessingCharge].C1_ChargeAmount = 30.10m;
			entryHeader.Charges[ChargeTypes.DeclarationProcessingCharge].C1_ChargeAmount = 30.10m;

			EDIMessage incomingMessage = entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			incomingMessage.EM_MessageText = responseMessageWithoutDeclarationCharge;//Declaration and AQIS processing fee are not reported

			processor.ProcessMessage(incomingMessage);
			AssertEquals("DeclarationProcessing Charge should be updated when an original response is processed", 30.10m, entryHeader.DeclarationProcessingCharge);
			AssertEquals("AQIS Processing Charge should be updated when an original response is processed", 30.10m, entryHeader.AQISProcessingCharge);
		}

		public void TestSetDutiesAndTaxesToEntryHeader()
		{
			EDIMessage outgoing = entryHeader.Messages.AddNew(typeof(CMRIMDMessage));
			outgoing.EM_MessageText = iMDMessageWithoutPaymentIncludedText;

			entryHeader.Charges[ChargeTypes.AQISProcessingCharge].C1_ChargeAmount = 10.25m;
			entryHeader.Charges[ChargeTypes.DeclarationProcessingCharge].C1_ChargeAmount = 32.25m;
			entryHeader.Charges[ChargeTypes.OtherCharges].C1_ChargeAmount = 42.25m;
			entryHeader.Charges[ChargeTypes.TotalPayableAdmin].C1_ChargeAmount = 52.25m;
			entryHeader.Charges[ChargeTypes.Woodlevy].C1_ChargeAmount = 78.52m;
			entryHeader.Charges[ChargeTypes.AQISContainerCharges].C1_ChargeAmount = 178.52m;

			EDIMessage incomingMessage = entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			incomingMessage.EM_MessageText = responseMessage;
			processor.ProcessMessage(incomingMessage);

			AssertEquals("AQIS Processing Charge", 1.25m, entryHeader.Charges[ChargeTypes.AQISProcessingCharge].C1_ChargeAmount);
			AssertEquals("Declaration Processing Charge", 3.25m, entryHeader.Charges[ChargeTypes.DeclarationProcessingCharge].C1_ChargeAmount);
			AssertEquals("Total Other Charges", 4.25m, entryHeader.Charges[ChargeTypes.OtherCharges].C1_ChargeAmount);
			AssertEquals("Total Payable Admin", 5.25m, entryHeader.Charges[ChargeTypes.TotalPayableAdmin].C1_ChargeAmount);
			AssertEquals("Total Payable Duty", 0m, entryHeader.Charges[ChargeTypes.DutyAmount].C1_ChargeAmount);
			AssertEquals("Total Payable GST", 0m, entryHeader.Charges[ChargeTypes.GSTAmount].C1_ChargeAmount);
			AssertEquals("Total Payable LCT", 0m, entryHeader.Charges[ChargeTypes.LCTAmount].C1_ChargeAmount);
			AssertEquals("Total Payable WET", 0m, entryHeader.Charges[ChargeTypes.WetAmount].C1_ChargeAmount);
			AssertEquals("Total Payable Woodlevy", 10.25m, entryHeader.Charges[ChargeTypes.Woodlevy].C1_ChargeAmount);
			AssertEquals("AQIS Container Charges", 11.25m, entryHeader.Charges[ChargeTypes.AQISContainerCharges].C1_ChargeAmount);
			AssertEquals("TotalCountervailingDuty", 0m, entryHeader.Charges[ChargeTypes.CountervailingDuty].C1_ChargeAmount);
			AssertEquals("TotalDeferredGST", 0m, entryHeader.Charges[ChargeTypes.GSTDeferred].C1_ChargeAmount);
			AssertEquals("TotalDumpingDuty", 0m, entryHeader.Charges[ChargeTypes.DumpingDuty].C1_ChargeAmount);
			AssertEquals("Total Duty Deferred", 197.36m, entryHeader.Charges[ChargeTypes.DutyDeferredAmount].C1_ChargeAmount);

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingAmendment.Code;
			entryHeader.EntryNumber = "AAAA6RHTE";
			incomingMessage = entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			incomingMessage.EM_MessageText = responseMessageForAmendmentAfterPayment;
			processor.ProcessMessage(incomingMessage);

			AssertEquals("AQIS Processing Charge", 1.25m, entryHeader.Charges[ChargeTypes.AQISProcessingCharge].C1_ChargeAmount);
			AssertEquals("Declaration Processing Charge", 3.25m, entryHeader.Charges[ChargeTypes.DeclarationProcessingCharge].C1_ChargeAmount);
			AssertEquals("Total Other Charges", 0m, entryHeader.Charges[ChargeTypes.OtherCharges].C1_ChargeAmount);
			AssertEquals("Total Payable Admin", 0m, entryHeader.Charges[ChargeTypes.TotalPayableAdmin].C1_ChargeAmount);
			AssertEquals("Total Payable Duty", 0m, entryHeader.Charges[ChargeTypes.DutyAmount].C1_ChargeAmount);
			AssertEquals("Total Payable GST", 0m, entryHeader.Charges[ChargeTypes.GSTAmount].C1_ChargeAmount);
			AssertEquals("Total Payable LCT", 0m, entryHeader.Charges[ChargeTypes.LCTAmount].C1_ChargeAmount);
			AssertEquals("Total Payable WET", 0m, entryHeader.Charges[ChargeTypes.WetAmount].C1_ChargeAmount);
			AssertEquals("Total Payable Woodlevy", 10.00m, entryHeader.Charges[ChargeTypes.Woodlevy].C1_ChargeAmount);
			AssertEquals("AQIS Container Charges", 0m, entryHeader.Charges[ChargeTypes.AQISContainerCharges].C1_ChargeAmount);
			AssertEquals("TotalCountervailingDuty", 0m, entryHeader.Charges[ChargeTypes.CountervailingDuty].C1_ChargeAmount);
			AssertEquals("TotalDeferredGST", 0m, entryHeader.Charges[ChargeTypes.GSTDeferred].C1_ChargeAmount);
			AssertEquals("TotalDumpingDuty", 0m, entryHeader.Charges[ChargeTypes.DumpingDuty].C1_ChargeAmount);
		}

		readonly string iMDMessageWithoutPaymentIncludedText = "UNH+1+CUSDEC:D:99B:UN'BGM+929:::IMD+B00122382/1/SYD1:1+9'CST++N10::95'" +
			"LOC+8+AUSYD::6'LOC+12+AUSYD::6'LOC+9+USLAX::6'LOC+79+AUSYD::6'DTM+178:20050902:102'DTM+260:20050901:102'DTM+252:20050902:102'" +
			"GIS+Y:153:95'GIS+LLB:109:95'GIS+TLB:109:95'FII+COQ+323232+:::242200::215'MEA+AAE+G+KG:150.00000'FTX+DEL+++ABC IMPORTS PTY LTD'" +
			"RFF+ABQ:1050'RFF+ADU:B00148379/1'RFF+ANU:B'RFF+APH:FOB'RFF+AMG:0001'TDT+20++A++QF::3'NAD+IM+AAA3334666F::95'NAD+VT+AA33HF::95'" +
			"NAD+DP++ALEXANDRIA++156 MAIN RD++:::NSW+2015+AU'NAD+CB+54321::95'MOA+63:1500.00:HKD'MOA+141:5720.71:HKD'MOA+39:1500.00:HKD'" +
			"MOA+313:600.00:AUD'+MOA+71:120.00:AUD'UNS+D'DMS+1'LIN+1+I'PAC+1+1'PCI+1'RFF+MWB:08125226666'PCI+1'RFF+HWB:22323244'CST+1+I::95+N10::95'" +
			"FTX+AAA+++FILES, RASPS AND SIMILAR TOOLS'LOC+27+HK::5'MEA+AAA++NO:150.00'NAD+SU+AAA3336647M::95'PAC++1'PCI+1'FTX+RAH+++00070:00085:N'" +
			"MOA+38:1500.00:HKD'MOA+68:720.00:AUD'RFF+ABD:82031000'RFF+AED:81'RFF+AFV:TV'UNS+S'UNT+55+1'";

		protected string responseMessageWithoutDeclarationCharge = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::IMDR+30HD 6C01 94F6:1+11'
FTX+AHN+++FINALISED:FINALISED'
GIS+N:117:95'
GIS+TLB:109:95'
GIS+LLB:109:95'
NAD+MR+AAA374M::95'
NAD+VT+AA33HF::95'
NAD+CB+54321::95'
NAD+IM++BOLEROPLUS PTY LTD'
NAD+CB++EAGLE DATAMATION INTERNATIONAL PTY:LIMIT'
RFF+ABO:B00149481/1/CMT1::1'
RFF+ABT:AAAA6RHTE::1'
RFF+ABQ:OREF'
RFF+ADU:B00149481/1'
RFF+AAE:N10'
TAX+3'
MOA+39:0000000009870.00'
TAX+3'
MOA+40:0000000009870.00'
TAX+3'
MOA+55:0000000000493.50'
TAX+3'
MOA+369:0000000001048.55'
TAX+3'
MOA+68:0000000000122.00'
TAX+3'
MOA+128:0000000001542.05'
DOC+1'
CST+1+N10::95'
FTX+AAF+++5%'
TAX+1'
MOA+40:0000000009870.00'
TAX+1'
MOA+55:0000000000493.50'
TAX+1'
MOA+369:0000000001048.55'
TAX+1'
MOA+56:0000000010485.50'
TAX+1'
MOA+68:0000000000122.00'
CNT+5:1'
UNT+43+000001'".Replace("\r\n", "");

		protected virtual ZDecimal ExpectedTotalPayableAfterMessageProcessed => 35.50m;

		protected override void SetUp()
		{
			base.SetUp();
			entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			processor = GetMessageProcessor() as CMRHeaderChargesResponseProcessor;
		}

		protected CMRHeaderChargesResponseProcessor processor;
		protected CusEntryLine entryLine2;

		readonly string responseMessage = @"UNH+000001+CUSRES:D:99B:UN'
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
RFF+ABT:AAAA6RHTE::2'
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
MOA+128:000000000035.50'
TAX+3'
MOA+346:0000000000197.36'
TAX+3'
MOA+26:0000000000001.25'
TAX+3'
MOA+23:000000000003.25'
TAX+3'
MOA+304:000000000004.25'
TAX+3'
MOA+7:000000000005.25'
TAX+3'
MOA+58:000000000010.25'
TAX+3'
MOA+35:000000000011.25'
TAX+3'
MOA+206:000000000012.25'
DOC+1+1'
CST+1+N10::95'
FTX+AAF+++7.5%'
TAX+1'
GIS+LAQ:109:95'
TAX+1'
CST+2+N10::95'
FTX+AAF+++7.5%'
TAX+1'
GIS+LAQ:109:95'
CNT+5:2'
UNT+68+000001'".Replace("\r\n", "");

		readonly string responseMessageForAmendmentAfterPayment = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::IMDR+18JI 3057 B2A6:1+11'
FTX+AHN+++CLEAR:CLEAR'
GIS+N:117:95'
GIS+TLB:109:95'
GIS+LLB:109:95'
NAD+MR+AAA374M::95'
NAD+VT+AA33HF::95'
NAD+CB+54321::95'
NAD+IM++BOLEROPLUS PTY LTD'
NAD+CB++EAGLE DATAMATION INTERNATIONAL PTY:LIMIT'
RFF+ABO:B00122382/1/CMT1::2'
RFF+ABT:AAAA6RHTE::2'
RFF+ABQ:787820 CHANGED'
RFF+ADU:B00122382/1'
RFF+AAE:N10'
ERP+::0'
ERC+ID0545::95'
FTX+AAO+++AIR WAYBILL HAS NOT BEEN REPORTED'
TAX+3'
MOA+39:0000000002000.00'
TAX+3'
MOA+40:0000000002000.00'
TAX+3'
MOA+369:0000000000215.00'
TAX+3'
MOA+68:0000000000150.00'
TAX+3'
MOA+128:0000000000010.00'
TAX+3'
MOA+58:000000000010.00'
DOC+1+1'
CST+1+N10::95'
FTX+AAF+++FREE'
TAX+1'
MOA+40:0000000002000.00'
TAX+1'
MOA+369:0000000000215.00'
TAX+1'
MOA+56:0000000002150.00'
TAX+1'
MOA+68:0000000000150.00'
CNT+5:1'
UNT+44+000001'".Replace("\r\n", "");
	}
}
