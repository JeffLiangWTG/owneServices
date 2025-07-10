#pragma warning restore IDE0001 // Prevent simplification to Enterprise.Registry.Business.Customs.AU.EntryChargeTypeList.Codes
using System;
using System.Collections;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Business.WarehouseExtensions.Testing;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
#pragma warning disable IDE0001 // Prevent simplification to Enterprise.Registry.Business.Customs.AU.EntryChargeTypeList.Codes
using ChargeTypes = Enterprise.Customs.AU.Declaration.Business.CusEntryChargeTypeList.Codes;
using CusEntryPayInfo = Enterprise.Customs.Business.CusEntryPayInfo;
using CusEntryPayInfoStatusList = Enterprise.Customs.Business.CusEntryPayInfoStatusList;
using WarehouseTransactionStatusList = Enterprise.Customs.Business.WarehouseTransactionStatusList;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class IMDRMessageProcessorTest : CMRHeaderAndLineChargesResponseProcessorTest
	{
		public void TestUpdateConsolidatedDeclarationCharges()
		{
			GlbStaff.CurrentUser.GS_Code = User.ServiceUserCode;

			var consolidatedDeclaration = Customs.Business.Testing.ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory);
			var leadDec = consolidatedDeclaration.LeadDeclaration as JobDeclaration;
			var leadEntryHeader = leadDec.CustomsEntryHeaders[0];
			leadEntryHeader.CH_Status = CustomsEntryStatus.AwaitingFormalLodge.Code;
			leadDec.JE_MasterBill = "MB000";
			leadDec.JE_HouseBill = "HB111";
			leadDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			leadDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			leadDec.JE_DateOfFirstArrival = new ZDateTime(2024, 1, 1);
			leadDec.GrossWeight = new ZArchitecture.ZWeight(100m, "KG");
			leadDec.Volume = new ZArchitecture.ZVolume(200m, "");
			var line1 = leadDec.Invoices.AddNew().InvoiceLines.AddNew();
			leadEntryHeader.AllEntryLines.AddNew().InvoiceLines.AddRange(leadDec.InvoiceLines);
			leadEntryHeader.MergedLines.Add(leadEntryHeader.AllEntryLines[0]);
			leadEntryHeader.CH_TotalPaid = 35m;
			var entryLine1 = leadEntryHeader.AllEntryLines[0];
			entryLine1.ZA_AggregateEntryLineNumber = 1;

			var otherDec = consolidatedDeclaration.JobDeclarations[1] as JobDeclaration;
			otherDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			otherDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			otherDec.EntryHeader.CH_Status = CustomsEntryStatus.AwaitingFormalLodge.Code;
			var otherDecEntryHeader = otherDec.EntryHeader;
			var line2 = otherDec.Invoices.AddNew().InvoiceLines.AddNew();
			otherDecEntryHeader.AllEntryLines.AddNew().InvoiceLines.AddRange(otherDec.InvoiceLines);
			otherDecEntryHeader.MergedLines.Add(otherDecEntryHeader.AllEntryLines[0]);
			var entryLine2 = otherDecEntryHeader.AllEntryLines[0];
			entryLine2.ZA_AggregateEntryLineNumber = 2;
			Factory.Save();

			AssertEquals("Precondition: declaration message status", "", leadDec.JE_MessageStatus);
			AssertEquals("Precondition: declaration message status", "", otherDec.JE_MessageStatus);
			AssertEquals("Precondition: entry message status", "WFL", leadDec.EntryHeader.CH_Status);
			AssertEquals("Precondition: entry message status", "WFL", otherDec.EntryHeader.CH_Status);

			var refNum = consolidatedDeclaration.CRD_JobReferenceNumber;
			leadEntryHeader.CH_BGMReference = refNum;

			string imd = $"UNH+1+CUSDEC:D:99B:UN'BGM+929:::IMD+{refNum}/SYD1:1+9'CST++N20::95'LOC+8+AUSYD::6'LOC+12+AUSYD::6'LOC+9+USLAX::6'LOC+18+9078E::95'LOC+79+AUSYD::6'DTM+260:20051007:102'DTM+252:20051007:102'GIS+EPA:109:95'GIS+Y:153:95'GIS+LLB:109:95'GIS+TLB:109:95'FII+COQ+323232+:::242200::215'MEA+AAE+G+KG:150.00000'FTX+DEL+++ABC IMPORTS PTY LTD'RFF+ABQ:1050'RFF+ADU:{refNum}'RFF+ANU:B'RFF+APH:FOB'RFF+AMG:0001'TDT+20++A++QF::3'NAD+AT+66015286036::95'NAD+VT+AA33HF::95'NAD+DP++ALEXANDRIA++156 MAIN RD++:::NSW+2015+AU'NAD+CB+54321::95'MOA+63:35000.00:AUD'MOA+141:35720.00:AUD'MOA+39:35000.00:AUD'MOA+313:600.00:AUD'MOA+71:120.00:AUD'UNS+D'DMS+1'LIN+1+I'PAC+1+1'PCI+1'RFF+MWB:66666666666'PCI+1'RFF+HWB:TESTING BOND ID'CST+1+I::95+N20::95'FTX+AAA+++COW CARCASSES'LOC+27+NZ::5'MEA+AAA+::WAR+NO:1.00'NAD+SU+AAA3336647M::95'PAC++1'PCI+1'FTX+RAH+++00318:00325:N'PCI+1'FTX+RAH+++00269:00230:N'PCI+1'FTX+RAH+++00007:00022:N'MOA+38:15000.00:AUD'MOA+68:308.57:AUD'RFF+ABD:87032220'RFF+AED:07'RFF+AGW:GEN'RFF+AKG:V1'RFF+AFV:TV'CST+2+I::95+N20::95'FTX+AAA+++COW CARCASSES'LOC+27+NZ::5'MEA+AAA+::WAR+NO:1.00'NAD+SU+AAA3336647M::95'PAC++1'PCI+1'FTX+RAH+++00318:00325:N'PCI+1'FTX+RAH+++00269:00230:N'PCI+1'FTX+RAH+++00007:00022:N'MOA+38:20000.00:AUD'MOA+68:411.43:AUD'RFF+ABD:87032220'RFF+AED:07'RFF+AGW:GEN'RFF+AKG:V2'RFF+AFV:TV'UNS+S'UNT+80+1'";
			string imdr = $@"UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+310F HD38 HJB5:1+11
FTX+AHN+++FINALISED:FINALISED'GIS+N:117:95'GIS+TLB:109:95'GIS+LLB:109:95
NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95'NAD+IM++AUSTRALIAN CUSTOMS SERVICE'NAD+CB++EAGLE DATAMATION INTERNATIONAL PTY:LIMIT
RFF+ABO:{refNum}/SYD1::1'RFF+ABT:AAAA6RHTE::1'RFF+ABQ:1050'RFF+ADU:{refNum}'RFF+AAE:N20
ERP+::0'ERC+ID0545::95'FTX+AAO+++AIR WAYBILL HAS NOT BEEN REPORTED
ERP+::1'ERC+ID0302::95'FTX+AAO+++COMMUNITY PROTECTION PERMIT IS APPLICABLE BUT NOT REQUIRED
ERP+::2'ERC+ID0302::95'FTX+AAO+++COMMUNITY PROTECTION PERMIT IS APPLICABLE BUT NOT REQUIRED
TAX+3'MOA+39:0000000035000.00
TAX+3'MOA+40:0000000035000.00
TAX+3'MOA+68:0000000000720.00
TAX+3'MOA+128:0000000000036.60
TAX+3'MOA+26:0000000000006.50
TAX+3'MOA+23:0000000000030.10
TAX+3'MOA+7:0000000000010.00
TAX+3'MOA+58:0000000000020.00
TAX+3'MOA+304:0000000000030.00
DOC+1+1
CST+1+N20::95'FTX+AAF+++5%
TAX+1'GIS+LAQ:109:95
TAX+1'MOA+40:0000000015000.00
TAX+1'MOA+68:0000000000308.57
TAX+1'MOA+146:000015000.0000
TAX+1'MOA+312:000000308.5700
CST+2+N20::95'FTX+AAF+++5%
TAX+1'GIS+LAQ:109:95
TAX+1'MOA+40:0000000020000.00
TAX+1'MOA+68:0000000000411.43
TAX+1'MOA+146:000020000.0000
TAX+1'MOA+312:000000411.4300
ERP+::1'ERC+1::95'ERC+2::95'FTX+ABS+++QUARANTINE ACT 1908. QUARANTINE PERMISSION TO DELIVER TO THE IMPORTER REQUIRED
ERP+::26'ERC+1::95'ERC+2::95'FTX+ABS+++QUARANTINE CLEARANCE REQUIRED FOR USED OR SECONDHAND OR FIELD TESTED GOODS
ERP+::221'ERC+1::95'ERC+2::95'FTX+ABS+++IMPORTERS OF SUBSTANCES OR PRE-CHARGED EQUIPMENT CONTROLLED UNDER REGULATION 5K SCHEDULE 10 OF THE CUSTOMS (PROHIBITED IMPORTS) REGULATIONS REQUIRE A LICENCE FROM DEH
ERP+::274'ERC+1::95'ERC+2::95'FTX+ABS+++THE IMPORT OF MILITARY OR EX-MILITARY GOODS IS RESTRICTED UNDER THE CUSTOMS (PROHIBITED IMPORTS) REGULATIONS - AN IMPORT PERMIT MAY BE REQUIRED.
NT+5:2'UNT+80+000001'";

			var outMessage = Factory.New<CMRIMDMessage>();
			outMessage.EM_MessageText = imd;
			outMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			outMessage.EM_Status = EDIMessage.Status.Sent;
			consolidatedDeclaration.Messages.Add(outMessage);

			EDIMessage incomingMessage = Factory.New<CMRIMDRMessage>();
			incomingMessage.EM_MessageText = imdr.Replace("\r\n", "'");
			Factory.Save();

			var imdrProcessor = new IMDRMessageProcessor(logger);
			imdrProcessor.ProcessMessage(incomingMessage);
			CombineAssertions(() =>
			{
				AssertEquals($"Post message processing: Entry Status on lead declarations", "FIN", leadDec.JE_EntryStatus);
				AssertEquals($"Post message processing: Entry Status should sync from lead declaration to consolidated declaration", "FIN", consolidatedDeclaration.CRD_CustomsStatus);
				AssertEquals($"Post message processing: Entry Status should sync from lead declaration to job declarations", "FIN", otherDec.JE_EntryStatus);

				AssertEquals("Lead Declaration CH_TotalPaid", 96.60m, leadEntryHeader.CH_TotalPaid);
				AssertEquals("Declaration Processing Charge (23) (DPC) should be stored only against the Lead Declaration", 30.10m, leadEntryHeader.DeclarationProcessingCharge);
				AssertEquals("AQIS Processing Charge (26) (APC) should be stored only against the Lead Declaration", 6.50m, leadEntryHeader.AQISProcessingCharge);
				AssertEquals("Total Payable Admin (7) (TPA) should be stored only against the Lead Declaration", 10.00m, leadEntryHeader.TotalPayableAdmin);
				AssertEquals("Total Wood Levy (58) (WDL) should be stored only against the Lead Declaration", 20.00m, leadEntryHeader.WoodLevy);
				AssertEquals("Total Other Charges (304) (OTH) should be stored only against the Lead Declaration", 30.00m, leadEntryHeader.OtherEntryCharge);
				AssertEquals("Total Duty Deferred 346 (TDT) should be zero on all Declarations", 0.00m, leadEntryHeader.TotalDeferredDutyFromCustoms); // TODO: Actually test this with a message containing this charge
				AssertEquals("Transport & Insurance (68) for lines on the Lead Declaration", 308.57m, leadEntryHeader.TAndI);
				AssertEquals("Entry header has charges attached", 7, leadEntryHeader.Charges.Count);

				AssertEquals("Other Declaration CH_TotalPaid", 0m, otherDecEntryHeader.CH_TotalPaid);
				AssertEquals("AQIS Processing Charge will not save in other declaration", 0m, otherDecEntryHeader.AQISProcessingCharge);
				AssertEquals("Declaration Processing Charge will not save in other declaration", 0m, otherDecEntryHeader.DeclarationProcessingCharge);
				AssertEquals("Total Other Charges will not save in other declaration", 0m, otherDecEntryHeader.OtherEntryCharge);
				AssertEquals("Total Payable Admin will not save in other declaration", 0m, otherDecEntryHeader.TotalPayableAdmin);
				AssertEquals("Total Wood Levy will not save in other declaration", 0m, otherDecEntryHeader.WoodLevy);
				AssertEquals("Total Duty Deferred 346 (TDT) should be zero on all Declarations", 0.00m, otherDecEntryHeader.TotalDeferredDutyFromCustoms);
				AssertEquals("Transport & Insurance (68) for lines on the Other Declaration", 411.43m, otherDecEntryHeader.TAndI);
				AssertEquals("Other Entry header has no charges attached", 0, otherDecEntryHeader.Charges.Count);

				AssertEquals("Total Transport & Insurance (68) should be stored on the Consolidated Declaration", "720.00AUD", consolidatedDeclaration.AddInfo.ZA_TILV);

				AssertEquals("ErrorEmailCount", 0, imdrProcessor.ErrorEmailSendCount);
				AssertEquals("AcknowledgementEmailCount", 1, imdrProcessor.AcknowledgementEmailSendCount);

				AssertEquals("Lead declaration message status", "CFL", leadDec.JE_MessageStatus);
				AssertEquals("Other declaration message status", "CFL", otherDec.JE_MessageStatus);
				AssertEquals("Lead declaration entry status", "FIN", leadDec.JE_EntryStatus);
				AssertEquals("Other declaration entry status", "FIN", otherDec.JE_EntryStatus);
				AssertEquals("Lead entry message status", "CFL", leadEntryHeader.CH_Status);
				AssertEquals("Other entry message status", "CFL", otherDec.EntryHeader.CH_Status);
				AssertEquals("Lead entry entry status", "FIN", leadEntryHeader.CH_EntryStatus);
				AssertEquals("Other entry entry status", "FIN", otherDec.EntryHeader.CH_EntryStatus);
				AssertEquals("Lead entry payment status", "PAP", leadEntryHeader.AddInfo.ZA_PaymentStatus_Hidden);
				AssertEquals("Other entry payment status", "PAP", otherDec.EntryHeader.AddInfo.ZA_PaymentStatus_Hidden);
			});

			var expectedLog = @$"{refNum}
{leadDec.JE_DeclarationReference}
Total Apportioned: $0

{otherDec.JE_DeclarationReference}
Total Apportioned: $0";

			AssertEquals(expectedLog, string.Join("\r\n", logger.Logs).Trim());
		}

		public void TestUpdateConsolidatedDeclarationDutyDeferred()
		{
			GlbStaff.CurrentUser.GS_Code = User.ServiceUserCode;

			var tariffClassificationCharacteristic = CMRTariffClassificationCharacteristic.New(Factory);
			tariffClassificationCharacteristic.TC_TariffClassificationSnapshotTariffClassificationNumber = "24029222";
			tariffClassificationCharacteristic.TC_CharacteristicCode = 29;

			var consolidatedDeclaration = Customs.Business.Testing.ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory);
			var leadDec = consolidatedDeclaration.LeadDeclaration as JobDeclaration;
			var leadEntryHeader = leadDec.CustomsEntryHeaders[0];
			leadEntryHeader.CH_Status = CustomsEntryStatus.AwaitingFormalLodge.Code;
			leadDec.JE_MasterBill = "MB000";
			leadDec.JE_HouseBill = "HB111";
			leadDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			leadDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			leadDec.JE_DateOfFirstArrival = new ZDateTime(2024, 1, 1);
			leadDec.GrossWeight = new ZArchitecture.ZWeight(100m, "KG");
			leadDec.Volume = new ZArchitecture.ZVolume(200m, "");
			var line1 = leadDec.Invoices.AddNew().InvoiceLines.AddNew();
			line1.JI_Tariff = "2402.91.11 01";
			leadEntryHeader.AllEntryLines.AddNew().InvoiceLines.AddRange(leadDec.InvoiceLines);
			leadEntryHeader.MergedLines.Add(leadEntryHeader.AllEntryLines[0]);
			leadEntryHeader.CH_TotalPaid = 35m;
			var entryLine1 = leadEntryHeader.AllEntryLines[0];
			entryLine1.ZA_AggregateEntryLineNumber = 1;

			var otherDec = consolidatedDeclaration.JobDeclarations[1] as JobDeclaration;
			otherDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			otherDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			otherDec.EntryHeader.CH_Status = CustomsEntryStatus.AwaitingFormalLodge.Code;
			var otherDecEntryHeader = otherDec.EntryHeader;
			var line2 = otherDec.Invoices.AddNew().InvoiceLines.AddNew();
			line2.JI_Tariff = "2402.92.22 02";
			otherDecEntryHeader.AllEntryLines.AddNew().InvoiceLines.AddRange(otherDec.InvoiceLines);
			otherDecEntryHeader.MergedLines.Add(otherDecEntryHeader.AllEntryLines[0]);
			var entryLine2 = otherDecEntryHeader.AllEntryLines[0];
			entryLine2.ZA_AggregateEntryLineNumber = 2;
			Factory.Save();

			var refNum = consolidatedDeclaration.CRD_JobReferenceNumber;
			leadEntryHeader.CH_BGMReference = refNum;

			string imd = $"UNH+1+CUSDEC:D:99B:UN'BGM+929:::IMD+{refNum}/SYD1:1+9'CST++N20::95'LOC+8+AUSYD::6'LOC+12+AUSYD::6'LOC+9+USLAX::6'LOC+18+9078E::95'LOC+79+AUSYD::6'DTM+260:20051007:102'DTM+252:20051007:102'GIS+EPA:109:95'GIS+Y:153:95'GIS+LLB:109:95'GIS+TLB:109:95'FII+COQ+323232+:::242200::215'MEA+AAE+G+KG:150.00000'FTX+DEL+++ABC IMPORTS PTY LTD'RFF+ABQ:1050'RFF+ADU:{refNum}'RFF+ANU:B'RFF+APH:FOB'RFF+AMG:0001'TDT+20++A++QF::3'NAD+AT+66015286036::95'NAD+VT+AA33HF::95'NAD+DP++ALEXANDRIA++156 MAIN RD++:::NSW+2015+AU'NAD+CB+54321::95'MOA+63:35000.00:AUD'MOA+141:35720.00:AUD'MOA+39:35000.00:AUD'MOA+313:600.00:AUD'MOA+71:120.00:AUD'UNS+D'DMS+1'LIN+1+I'PAC+1+1'PCI+1'RFF+MWB:66666666666'PCI+1'RFF+HWB:TESTING BOND ID'CST+1+I::95+N20::95'FTX+AAA+++COW CARCASSES'LOC+27+NZ::5'MEA+AAA+::WAR+NO:1.00'NAD+SU+AAA3336647M::95'PAC++1'PCI+1'FTX+RAH+++00318:00325:N'PCI+1'FTX+RAH+++00269:00230:N'PCI+1'FTX+RAH+++00007:00022:N'MOA+38:15000.00:AUD'MOA+68:308.57:AUD'RFF+ABD:87032220'RFF+AED:07'RFF+AGW:GEN'RFF+AKG:V1'RFF+AFV:TV'CST+2+I::95+N20::95'FTX+AAA+++COW CARCASSES'LOC+27+NZ::5'MEA+AAA+::WAR+NO:1.00'NAD+SU+AAA3336647M::95'PAC++1'PCI+1'FTX+RAH+++00318:00325:N'PCI+1'FTX+RAH+++00269:00230:N'PCI+1'FTX+RAH+++00007:00022:N'MOA+38:20000.00:AUD'MOA+68:411.43:AUD'RFF+ABD:87032220'RFF+AED:07'RFF+AGW:GEN'RFF+AKG:V2'RFF+AFV:TV'UNS+S'UNT+80+1'";

			// Total deferred (346) = Declaration Fee (23) + AQIS Fee (26) + Wood Levy (58) + Total WET (149) + Line 1 Duty (55)
			string imdr = $@"UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+310F HD38 HJB5:1+11
FTX+AHN+++FINALISED:FINALISED'GIS+N:117:95'GIS+TLB:109:95'GIS+LLB:109:95
NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95'NAD+IM++AUSTRALIAN CUSTOMS SERVICE'NAD+CB++EAGLE DATAMATION INTERNATIONAL PTY:LIMIT
RFF+ABO:{refNum}/SYD1::1'RFF+ABT:AAAA6RHTE::1'RFF+ABQ:1050'RFF+ADU:{refNum}'RFF+AAE:N20
ERP+::0'ERC+ID0545::95'FTX+AAO+++AIR WAYBILL HAS NOT BEEN REPORTED
ERP+::1'ERC+ID0302::95'FTX+AAO+++COMMUNITY PROTECTION PERMIT IS APPLICABLE BUT NOT REQUIRED
ERP+::2'ERC+ID0302::95'FTX+AAO+++COMMUNITY PROTECTION PERMIT IS APPLICABLE BUT NOT REQUIRED
TAX+3'MOA+39:0000000035000.00
TAX+3'MOA+40:0000000035000.00
TAX+3'MOA+68:0000000000720.00
TAX+3'MOA+128:0000000000036.60
TAX+3'MOA+26:0000000000006.50
TAX+3'MOA+23:0000000000030.10
TAX+3'MOA+7:0000000000010.00
TAX+3'MOA+58:0000000000020.00
TAX+3'MOA+304:0000000000030.00
TAX+3'MOA+346:0000000001456.60
TAX+3'MOA+55:0000000000750.00
DOC+1+1
CST+1+N20::95'FTX+AAF+++5%
TAX+1'GIS+LAQ:109:95
TAX+1'MOA+40:0000000015000.00
TAX+1'MOA+68:0000000000308.57
TAX+1'MOA+146:0000000015000.00
TAX+1'MOA+312:0000000000308.57
TAX+1'MOA+149:0000000000750.00
TAX+1'MOA+55:0000000000500.00
CST+2+N20::95'FTX+AAF+++5%
TAX+1'GIS+LAQ:109:95
TAX+1'MOA+40:0000000020000.00
TAX+1'MOA+68:0000000000411.43
TAX+1'MOA+146:0000000020000.00
TAX+1'MOA+312:0000000000411.43
TAX+1'MOA+149:0000000000150.00
TAX+1'MOA+55:0000000000250.00
ERP+::1'ERC+1::95'ERC+2::95'FTX+ABS+++QUARANTINE ACT 1908. QUARANTINE PERMISSION TO DELIVER TO THE IMPORTER REQUIRED
ERP+::26'ERC+1::95'ERC+2::95'FTX+ABS+++QUARANTINE CLEARANCE REQUIRED FOR USED OR SECONDHAND OR FIELD TESTED GOODS
ERP+::221'ERC+1::95'ERC+2::95'FTX+ABS+++IMPORTERS OF SUBSTANCES OR PRE-CHARGED EQUIPMENT CONTROLLED UNDER REGULATION 5K SCHEDULE 10 OF THE CUSTOMS (PROHIBITED IMPORTS) REGULATIONS REQUIRE A LICENCE FROM DEH
ERP+::274'ERC+1::95'ERC+2::95'FTX+ABS+++THE IMPORT OF MILITARY OR EX-MILITARY GOODS IS RESTRICTED UNDER THE CUSTOMS (PROHIBITED IMPORTS) REGULATIONS - AN IMPORT PERMIT MAY BE REQUIRED.
NT+5:2'UNT+80+000001'";

			var outMessage = Factory.New<CMRIMDMessage>();
			outMessage.EM_MessageText = imd;
			outMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			outMessage.EM_Status = EDIMessage.Status.Sent;
			consolidatedDeclaration.Messages.Add(outMessage);

			EDIMessage incomingMessage = Factory.New<CMRIMDRMessage>();
			incomingMessage.EM_MessageText = imdr.Replace("\r\n", "'");
			Factory.Save();

			var imdrProcessor = new IMDRMessageProcessor(logger);
			imdrProcessor.ProcessMessage(incomingMessage);
			CombineAssertions(() =>
			{
				AssertEquals("Declaration Processing Charge (23) (DPC) should be stored only against the Lead Declaration", 30.10m, leadEntryHeader.DeclarationProcessingCharge);
				AssertEquals("AQIS Processing Charge (26) (APC) should be stored only against the Lead Declaration", 6.50m, leadEntryHeader.AQISProcessingCharge);
				AssertEquals("Total Payable Admin (7) (TPA) should be stored only against the Lead Declaration", 10.00m, leadEntryHeader.TotalPayableAdmin);
				AssertEquals("Total Wood Levy (58) (WDL) should be stored only against the Lead Declaration", 20.00m, leadEntryHeader.WoodLevy);
				AssertEquals("Total Other Charges (304) (OTH) should be stored only against the Lead Declaration", 30.00m, leadEntryHeader.OtherEntryCharge);
				AssertEquals("Wine Equalisation in Lead declaration", 750m, leadEntryHeader.WETAmount);
				AssertEquals("Transport & Insurance (68) for lines on the Lead Declaration", 308.57m, leadEntryHeader.TAndI);
				AssertEquals("Duty Deferred 346 (TDT) should be apportioned by line", 1306.60m, leadEntryHeader.TotalDeferredDutyFromCustoms);
				AssertEquals("Lead Declaration Total Duty", 500m, leadEntryHeader.TotalDutyAmount);
				AssertEquals("Lead Declaration CH_TotalPaid", 40.00m, leadEntryHeader.CH_TotalPaid);
				AssertEquals("Lead Entry header charges attached", 7, leadEntryHeader.Charges.Count);

				AssertEquals("Declaration Processing Charge will not save in other declaration", 0m, otherDecEntryHeader.DeclarationProcessingCharge);
				AssertEquals("AQIS Processing Charge will not save in other declaration", 0m, otherDecEntryHeader.AQISProcessingCharge);
				AssertEquals("Total Payable Admin will not save in other declaration", 0m, otherDecEntryHeader.TotalPayableAdmin);
				AssertEquals("Total Wood Levy will not save in other declaration", 0m, otherDecEntryHeader.WoodLevy);
				AssertEquals("Total Other Charges will not save in other declaration", 0m, otherDecEntryHeader.OtherEntryCharge);
				AssertEquals("Wine Equalisation in other declaration", 150m, otherDecEntryHeader.WETAmount);
				AssertEquals("Transport & Insurance (68) for lines on the Other Declaration", 411.43m, otherDecEntryHeader.TAndI);
				AssertEquals("Duty Deferred 346 (TDT) should be apportioned by line", 150.00m, otherDecEntryHeader.TotalDeferredDutyFromCustoms);
				AssertEquals("Other Declaration Total Duty", 250m, otherDecEntryHeader.TotalDutyAmount);
				AssertEquals("Other Declaration CH_TotalPaid", 250m, otherDecEntryHeader.CH_TotalPaid);
				AssertEquals("Other Entry header charges attached", 1, otherDecEntryHeader.Charges.Count);

				var expectedLog = @$"{refNum}
{leadDec.JE_DeclarationReference}
Is Lead. Deferred Header Fees $56.60
Line 1. Deferred $1250.00
Total Apportioned: $1306.60

{otherDec.JE_DeclarationReference}
Line 2. Deferred $150.00 (is EEG)
Total Apportioned: $150.00";

				AssertEquals(expectedLog, string.Join("\r\n", logger.Logs).Trim());
			});

			// Simulate incorrect calculation to test logging, causes reprocessing attempt.
			logger.ClearLogs();
			incomingMessage.EM_MessageText = incomingMessage.EM_MessageText.Replace("TAX+3'MOA+346:0000000001456.60", "TAX+3'MOA+346:0000000001455.55");
			var imdrProcessor2 = new IMDRMessageProcessor(logger);
			imdrProcessor2.ProcessMessage(incomingMessage);

			var expectedLog2 = @$"Error - DutyDeferred Apportionment Error. Expected 1455.55  Was 1456.60

{refNum}
{leadDec.JE_DeclarationReference}
Is Lead. Deferred Header Fees $56.60
Line 1. Deferred $1250.00
Total Apportioned: $1306.60

{otherDec.JE_DeclarationReference}
Line 2. Deferred $150.00 (is EEG)
Total Apportioned: $150.00


Error - DutyDeferred Apportionment Error. Expected 1455.55  Was 1456.60

CE00000001
B00001000
Is Lead. Deferred Header Fees $56.60
Line 1. Deferred $1250.00
Total Apportioned: $1306.60

B00001001
Line 2. Deferred $150.00 (is EEG)
Total Apportioned: $150.00";

			AssertEquals(expectedLog2, string.Join("\r\n", logger.Logs).Trim());
		}

		public void TestManualAmendmentToCIReply()
		{
			IMDRMessageProcessor messageProcessor = new IMDRMessageProcessor(logger);
			EDIMessage message1 = entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			message1.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+2475 C0I3 DHAF:1+11'NAD+MR+AAA374M::95'RFF+ABO:B00122382/1/SYD1::2'ERP+::0'" +
			"ERC+ID0152::95'FTX+AAO+++LOADING PORT CODE=CNXNG DOES NOT EXIST LOADING PORT CODE=CNXNG'CNT+55:1'UNT+9+000001'";
			messageProcessor.ProcessMessage(message1);
			LogsForNominatedEvent outstandingManualAmendments = new LogsForNominatedEvent(entryHeader.Declaration.Logs, Events.ManualMatchDone);
			AssertEquals("Error IMDR, event should not have been posted", 0, outstandingManualAmendments.Count);

			messageProcessor = new IMDRMessageProcessor(logger);
			EDIMessage message2 = entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			message2.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+167H 1J49 CJB5:1+11'FTX+AHN+++CLEAR:CLEAR'GIS+N:117:95'GIS+TLB:109:95'GIS+LLB:109:95'NAD+MR+FGE373F::95'NAD+VT+AA67ET::95'NAD+CB+488::95'NAD+IM++DEGUSSA AUSTRALIA PTY LTD'" +
			"NAD+CB++ALL PORTS INTERNATIONAL LOGISTICS P:TY. L'RFF+ABO:B00122382/1/MEL2::2'RFF+ABT:AAAA6RHTE::1'RFF+ABQ:6135/2'RFF+ADU:B00122382/1'RFF+AAE:N10'ERP+::0'ERC+ID0548::95'FTX+AAO+++BILL OF LADING NOT REPORTED FOR VESSEL VOYAGE'TAX+3'MOA+39:0000000046468.64'TAX+3'" +
			"MOA+40:0000000044240.26'TAX+3'MOA+210:0000000004646.86'TAX+3'MOA+68:0000000002228.38'TAX+3'MOA+128:0000000000071.00'TAX+3'MOA+26:0000000000006.50'TAX+3'MOA+35:0000000000015.00'TAX+3'MOA+23:0000000000049.50'DOC+1+1'CST+1+N10::95'FTX+AAF+++FREE'TAX+1'GIS+LAQ:109:95'" +
			"TAX+1'MOA+40:0000000044240.26'TAX+1'MOA+210:0000000004646.86'TAX+1'MOA+56:0000000046468.64'TAX+1'MOA+68:0000000002228.38'ERP+::218'ERC+1::95'FTX+ABS+++GOODS (CHEMICALS) MAY BE REGULATED BY NICNAS. RING 1800638528'CNT+5:1'UNT+53+000001'";
			messageProcessor.ProcessMessage(message2);
			outstandingManualAmendments = new LogsForNominatedEvent(entryHeader.Declaration.Logs, Events.ManualMatchDone);
			AssertEquals("Normal IMDR, event should not have been posted", 0, outstandingManualAmendments.Count);

			messageProcessor = new IMDRMessageProcessor(logger);
			EDIMessage message3 = entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			message3.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+167H 1J49 CJB5:1+11'FTX+AHN+++CLEAR:CLEAR'GIS+N:117:95'NAD+MR+FGE373F::95'NAD+VT+AA67ET::95'NAD+CB+488::95'NAD+IM++DEGUSSA AUSTRALIA PTY LTD'" +
			"NAD+CB++ALL PORTS INTERNATIONAL LOGISTICS P:TY. L'RFF+ABO:B00122382/1/MEL2::2'RFF+ABT:AAAA6RHTE::1'RFF+ABQ:6135/2'RFF+ADU:B00122382/1'RFF+AAE:N10'ERP+::0'ERC+ID0548::95'FTX+AAO+++BILL OF LADING NOT REPORTED FOR VESSEL VOYAGE'TAX+3'MOA+39:0000000046468.64'TAX+3'" +
			"MOA+40:0000000044240.26'TAX+3'MOA+210:0000000004646.86'TAX+3'MOA+68:0000000002228.38'TAX+3'MOA+128:0000000000071.00'TAX+3'MOA+26:0000000000006.50'TAX+3'MOA+35:0000000000015.00'TAX+3'MOA+23:0000000000049.50'DOC+1+1'CST+1+N10::95'FTX+AAF+++FREE'TAX+1'GIS+LAQ:109:95'" +
			"TAX+1'MOA+40:0000000044240.26'TAX+1'MOA+210:0000000004646.86'TAX+1'MOA+56:0000000046468.64'TAX+1'MOA+68:0000000002228.38'ERP+::218'ERC+1::95'FTX+ABS+++GOODS (CHEMICALS) MAY BE REGULATED BY NICNAS. RING 1800638528'CNT+5:1'UNT+53+000001'";
			messageProcessor.ProcessMessage(message3);
			outstandingManualAmendments = new LogsForNominatedEvent(entryHeader.Declaration.Logs, Events.ManualMatchDone);
			AssertEquals("Reply to CI Amendment, event should have been posted", 1, outstandingManualAmendments.Count);

			messageProcessor = new IMDRMessageProcessor(logger);
			EDIMessage message4 = entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			message4.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+167H 1J49 CJB5:1+11'FTX+AHN+++CLEAR:CLEAR'GIS+N:117:95'GIS+TLB:109:95'GIS+LLB:109:95'NAD+MR+FGE373F::95'NAD+VT+AA67ET::95'NAD+CB+488::95'NAD+IM++DEGUSSA AUSTRALIA PTY LTD'" +
			"NAD+CB++ALL PORTS INTERNATIONAL LOGISTICS P:TY. L'RFF+ABO:B00122382/1/MEL2::2'RFF+ABT:AAAA6RHTE::1'RFF+ABQ:6135/2'RFF+ADU:B00122382/1'RFF+AAE:N10'ERP+::0'ERC+ID0548::95'FTX+AAO+++BILL OF LADING NOT REPORTED FOR VESSEL VOYAGE'TAX+3'MOA+39:0000000046468.64'TAX+3'" +
			"MOA+40:0000000044240.26'TAX+3'MOA+210:0000000004646.86'TAX+3'MOA+68:0000000002228.38'TAX+3'MOA+128:0000000000071.00'TAX+3'MOA+26:0000000000006.50'TAX+3'MOA+35:0000000000015.00'TAX+3'MOA+23:0000000000049.50'DOC+1+1'CST+1+N10::95'FTX+AAF+++FREE'TAX+1'GIS+LAQ:109:95'" +
			"TAX+1'MOA+40:0000000044240.26'TAX+1'MOA+210:0000000004646.86'TAX+1'MOA+56:0000000046468.64'TAX+1'MOA+68:0000000002228.38'ERP+::218'ERC+1::95'FTX+ABS+++GOODS (CHEMICALS) MAY BE REGULATED BY NICNAS. RING 1800638528'CNT+5:1'UNT+53+000001'";
			messageProcessor.ProcessMessage(message4);
			outstandingManualAmendments = new LogsForNominatedEvent(entryHeader.Declaration.Logs, Events.ManualMatchDone);
			AssertEquals("Normal IMDR, still only 1 event should have been posted", 1, outstandingManualAmendments.Count);
		}

		public void TestPublishUniversalShipmentToBondedWarehouseInwardOnFinalised()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddYears(-1).ToDateTime());
				string iMDRData = "UNH+000002+CUSRES:D:99B:UN'BGM+961:::IMDR+34B9 8CFF 9D56:1+11'FTX+AHN+++FINALISED:FINALISED'GIS+N:117:95'GIS+TLB:109:95'GIS+LLB:109:95'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95'NAD+IM++BOLEROPLUS PTY LTD'NAD+CB++EAGLE DATAMATION INTERNATIONAL PTY:LIMIT'RFF+ABO:B00149171/1/CMT3::3'RFF+ABT:ENT0123456::1'RFF+ABQ:NMNM'RFF+ADU:B00149171/1'RFF+AAE:N10/N20'ERP+::0'ERC+ID0547::95'FTX+AAO+++VESSEL VOYAGE NOT FOUND VESSEL ID=9065182,VOYAGE NO=999,LINKING VOYAGE NO=999'TAX+3'MOA+39:0000000002500.00'TAX+3'MOA+40:0000000002500.00'TAX+3'MOA+369:0000000000203.36'TAX+3'MOA+68:0000000000042.00'TAX+3'MOA+128:0000000000263.61'TAX+3'MOA+26:0000000000007.00'TAX+3'MOA+35:0000000000003.75'TAX+3'MOA+23:0000000000049.50'DOC+1+1'CST+1+N20::95'FTX+AAF+++FREE'TAX+1'MOA+40:0000000000500.00'TAX+1'MOA+68:0000000000008.40'TAX+1'MOA+146:000000012.5000'TAX+1'MOA+312:000000000.2100'CST+2+N10::95'FTX+AAF+++FREE'TAX+1'MOA+40:0000000002000.00'TAX+1'MOA+369:0000000000203.36'TAX+1'MOA+56:0000000002033.60'TAX+1'MOA+68:0000000000033.60'CNT+5:2'UNT+58+000002'";
				JobComInvoiceLine invoiceLine;
				var iMDRMessage = CreateDataForUniversalTesting(JobMessageTypeList.Codes.Import, iMDRData, "B00149171", "ENT0123456", 110m, out invoiceLine);
				Factory.Save();
				new IMDRMessageProcessor(new LoggingInformation()).ProcessMessage(iMDRMessage);
				invoiceLine.Declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreatedPending;
				Factory.Save();
				AssertEquals(true, Env.OutgoingCustomsMailManager.EmailsCreated.Exists(x => x.Body.Contains("<p>Stock Levels have been updated. (WHS Receipt: <a href=")));
				var declaration = invoiceLine.Declaration;
				var exportLog = declaration.Logs.MostRecentLogByEventTime(Events.DataExport);
				var dataContex = exportLog.RelatedEDIMessage.DataContext;
				AssertNotNull(dataContex.DataSourceCollection.FirstOrDefault(x => x.Type.GetValueOrDefault() == nameof(DataContextType.CustomsDeclaration)));
				AssertNotNull(dataContex.RecipientRoleCollection.FirstOrDefault(x => x.Code.Value == RecipientRoleType.BWI));
			}
		}

		public void TestPublishUniversalShipmentHeldToBondedWarehouseInwardOnClear()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddYears(-1).ToDateTime());
			string iMDRData = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+IJCD 1357 J0D:1+11'FTX+AHN+++CLEAR:CLEAR'GIS+N:117:95'GIS+TLB:109:95'GIS+LLB:109:95'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95'NAD+IM++SECURECERTS PTY LTD'NAD+CB++WISETECH GLOBAL PTY LTD'RFF+ABO:B00160215/1/CMT1::1'RFF+ABT:ENT0123456::1'RFF+ABQ:OWN1611'RFF+ADU:B00160215/1'RFF+AAE:N20'ERP+::0'ERC+ID0547::95'FTX+AAO+++VESSEL VOYAGE NOT FOUND VESSEL ID=9044748,VOYAGE NO=1611,LINKING VOYAGE NO=1611'TAX+3'MOA+39:0000000005000.00'TAX+3'MOA+40:0000000005000.00'TAX+3'MOA+68:0000000000600.00'TAX+3'MOA+128:0000000000070.00'TAX+3'MOA+26:0000000000014.00'TAX+3'MOA+35:0000000000006.00'TAX+3'MOA+23:0000000000050.00'DOC+1+1'CST+1+N20::95'FTX+AAF+++10%'TAX+1'MOA+40:0000000005000.00'TAX+1'MOA+68:0000000000600.00'TAX+1'MOA+146:000005000.0000'TAX+1'MOA+312:000000600.0000'CNT+5:1'UNT+46+000001'";
			JobComInvoiceLine invoiceLine;
			var iMDRMessage = CreateDataForUniversalTesting(JobMessageTypeList.Codes.Import, iMDRData, "B00160215", "ENT0123456", 110m, out invoiceLine);
			Factory.Save();
			new IMDRMessageProcessor(new LoggingInformation()).ProcessMessage(iMDRMessage);
			invoiceLine.Declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreatedPending;
			Factory.Save();
			AssertEquals(true, Env.OutgoingCustomsMailManager.EmailsCreated.Exists(x => x.Body.Contains("<p>Stock Levels have been held.")));
			var declaration = invoiceLine.Declaration;
			AssertNotNull("Has DEX event", declaration.Logs.MostRecentLogByEventTime(Events.DataExport));
			var shipment = declaration.GetLastHoldUniversalShipmentFromNote();
			var dataContex = shipment.DataContext;
			AssertNotNull(dataContex.DataSourceCollection.FirstOrDefault(x => x.Type.GetValueOrDefault() == nameof(DataContextType.CustomsDeclaration)));
			AssertNotNull(dataContex.RecipientRoleCollection.FirstOrDefault(x => x.Code.Value == RecipientRoleType.BWI));
			WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT0123456-1", 0m);
		}

		public void TestPublishUniversalShipmentHeldToBondedWarehouseInwardOnHeld()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddYears(-1).ToDateTime());
			string iMDRData = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+4C5H G13H EABG:1+11'FTX+AHN+++HELD:HELD'GIS+N:117:95'GIS+TLB:109:95'GIS+LLB:109:95'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95'NAD+IM++MS AMY TEST'NAD+CB++WISETECH GLOBAL LIMITED'RFF+ABO:B00160215/1/CMT1::1'RFF+ABT:ENT0123456::1'RFF+ABQ:WI00120243T1A'RFF+ADU:B00160215/1'RFF+AAE:N20'ERP+::0'ERC+ID0547::95'FTX+AAO+++VESSEL VOYAGE NOT FOUND VESSEL ID=9044748,VOYAGE NO=87R,LINKING VOYAGE NO=87'ERP+::3'ERC+ID0288::95'FTX+AAO+++UNIT VALUE FOR Quantity=?+00000000000.34 IS NOT IN RANGE FOR STATISTICAL CODE'TAX+3'MOA+39:0000000003000.00'TAX+3'MOA+40:0000000003000.00'TAX+3'MOA+68:0000000000025.00'TAX+3'MOA+128:0000000000092.00'TAX+3'MOA+26:0000000000042.00'TAX+3'MOA+23:0000000000050.00'DOC+1+1'CST+1+N20::95'FTX+AAF+++5%'TAX+1'GIS+LAQ:109:95'TAX+1'MOA+40:0000000001000.00'TAX+1'MOA+68:0000000000008.33'TAX+1'MOA+146:000000001.0000'TAX+1'MOA+312:000000000.0083'CST+2+N20::95'FTX+AAF+++5%'TAX+1'GIS+LAQ:109:95'TAX+1'MOA+40:0000000001000.00'TAX+1'MOA+68:0000000000008.33'TAX+1'MOA+146:000000001.0000'TAX+1'MOA+312:000000000.0083'CST+3+N20::95'FTX+AAF+++$78.44/LITRE ALCOHOL'TAX+1'GIS+LAQ:109:95'TAX+1'MOA+40:0000000001000.00'TAX+1'MOA+68:0000000000008.33'TAX+1'MOA+146:000002941.1764'TAX+1'MOA+312:000000024.5000'ERP+::1'ERC+2::95'FTX+ABS+++QUARANTINE ACT 1908. QUARANTINE PERMISSION TO DELIVER TO THE IMPORTER REQUIRED'ERP+::218'ERC+2::95'FTX+ABS+++NICNAS REGISTRATION REQUIREMENTS APPLY TO ALL IMPORTERS OF INDUSTRIAL CHEMICALS.  CONTACT 1800 638 528 FOR FURTHER INFORMATION.'ERP+::283'ERC+2::95'FTX+ABS+++QUARANTINE CLEARANCE REQUIRED FOR SOIL OR THE TOP 2 METRES OF EARTHS SURFACE'ERP+::284'ERC+2::95'FTX+ABS+++QUARANTINE CLEARANCE REQUIRED FOR GOODS TO BE USED IN SOIL CONDITIONER OR POTTING MIX'CNT+5:3'UNT+85+000001'";
			JobComInvoiceLine invoiceLine;
			var iMDRMessage = CreateDataForUniversalTesting(JobMessageTypeList.Codes.Import, iMDRData, "B00160215", "ENT0123456", 110m, out invoiceLine);
			Factory.Save();
			new IMDRMessageProcessor(new LoggingInformation()).ProcessMessage(iMDRMessage);
			invoiceLine.Declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreatedPending;
			Factory.Save();
			AssertEquals(true, Env.OutgoingCustomsMailManager.EmailsCreated.Exists(x => x.Body.Contains("<p>Stock Levels have been held.")));
			var declaration = invoiceLine.Declaration;
			AssertNotNull("Has DEX event", declaration.Logs.MostRecentLogByEventTime(Events.DataExport));
			var shipment = declaration.GetLastHoldUniversalShipmentFromNote();
			var dataContex = shipment.DataContext;
			AssertNotNull(dataContex.DataSourceCollection.FirstOrDefault(x => x.Type.GetValueOrDefault() == nameof(DataContextType.CustomsDeclaration)));
			AssertNotNull(dataContex.RecipientRoleCollection.FirstOrDefault(x => x.Code.Value == RecipientRoleType.BWI));
			WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT0123456-1", 0m);
		}

		public void TestProcessingANature10ClearResponseWithNotSendToBondedWarehouse()
		{
			// Setup a nature 10 job and lodge without pay.
			// Process a clear message
			// Check that no warehouse status has been set.
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddYears(-1).ToDateTime());
			string iMDRData = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+IJCD 1357 J0D:1+11'FTX+AHN+++CLEAR:CLEAR'GIS+N:117:95'GIS+TLB:109:95'GIS+LLB:109:95'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95'NAD+IM++SECURECERTS PTY LTD'NAD+CB++WISETECH GLOBAL PTY LTD'RFF+ABO:B00160215/1/CMT1::1'RFF+ABT:ENT0123456::1'RFF+ABQ:OWN1611'RFF+ADU:B00160215/1'RFF+AAE:N20'ERP+::0'ERC+ID0547::95'FTX+AAO+++VESSEL VOYAGE NOT FOUND VESSEL ID=9044748,VOYAGE NO=1611,LINKING VOYAGE NO=1611'TAX+3'MOA+39:0000000005000.00'TAX+3'MOA+40:0000000005000.00'TAX+3'MOA+68:0000000000600.00'TAX+3'MOA+128:0000000000070.00'TAX+3'MOA+26:0000000000014.00'TAX+3'MOA+35:0000000000006.00'TAX+3'MOA+23:0000000000050.00'DOC+1+1'CST+1+N20::95'FTX+AAF+++10%'TAX+1'MOA+40:0000000005000.00'TAX+1'MOA+68:0000000000600.00'TAX+1'MOA+146:000005000.0000'TAX+1'MOA+312:000000600.0000'CNT+5:1'UNT+46+000001'";
			JobComInvoiceLine invoiceLine;
			var iMDRMessage = CreateDataForUniversalTesting(JobMessageTypeList.Codes.Import, iMDRData, "B00160215", "ENT0123456", 110m, out invoiceLine);
			invoiceLine.JI_IsPackToBondForLine = false;
			var declaration = invoiceLine.Declaration;
			declaration.DoMerge();
			Factory.Save();
			AssertEquals("WarehouseTransactionStatus", ZString.Empty, declaration.WarehouseTransactionStatus);
			new IMDRMessageProcessor(new LoggingInformation()).ProcessMessage(iMDRMessage);
			Factory.Save();
			AssertEquals("WarehouseTransactionStatus", ZString.Empty, declaration.WarehouseTransactionStatus);
			AssertNull("No DEX event", declaration.Logs.MostRecentLogByEventTime(Events.DataExport));
			WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT0123456-1", 0m);
		}

		public void TestHandlingInwardStatusOnWithdrawalErrorForBondedWarehouseDisabled()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				Env.Registry.AUCustoms.EdificeSendErrorsToGroup = PostMasterGroup.PK.ToGuid();
				CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());
				string iMDRData = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+16I8 BEHH FIA7:1+11'NAD+MR+AAA374M::95'RFF+ABO:B00160272/5/CMT1::2'ERP+::0'ERC+ID0763::95'FTX+AAO+++LODGEMENT DECLARATION QUESTION NUMBER=000000000000010 IS NOT ALLOWED FOR THIS TRANSACTION'CNT+55:1'UNT+9+000001'";
				JobComInvoiceLine invoiceLine;
				var iMDRMessage = CreateDataForUniversalTesting(JobMessageTypeList.Codes.Import, iMDRData, "B00160272", "ENT0123456", 110m, out invoiceLine);
				var entry = (CusEntryHeader)iMDRMessage.EM_LinkedObject;
				entry.CH_Status = CustomsEntryStatus.AwaitingWithdrawal.Code;
				var declaration = entry.Declaration;
				Factory.Save();
				declaration.PublishShipmentForWHSInward(false);
				declaration.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
				Factory.Save();
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT0123456-1", 110m);

				Env.ClearAllEmailsCreated();
				new IMDRMessageProcessor(new LoggingInformation()).ProcessMessage(iMDRMessage);
				entry.CH_Status = CustomsEntryStatus.ClearWithdrawal.Code;
				Factory.Save();
				var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject == "Import Declaration Response(IMDR) Message for Declaration Reference: B00160272 - TRANSACTION REJECTED");
				AssertNotContains("(WHS Receipt: ", email.Body);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT0123456-1", 110m);
				AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.AutomationIsDisabled, declaration.WarehouseTransactionStatus);
			}
		}

		public void TestHandlingInwardStatusOnAmendmentClearButWarehouseFailure()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());
				string iMDRData = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+IJCD 1357 J0D:1+11'FTX+AHN+++FINALISED:FINALISED'GIS+N:117:95'GIS+TLB:109:95'GIS+LLB:109:95'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95'NAD+IM++SECURECERTS PTY LTD'NAD+CB++WISETECH GLOBAL PTY LTD'RFF+ABO:B00160215/1/CMT1::1'RFF+ABT:ENT0123456::1'RFF+ABQ:OWN1611'RFF+ADU:B00160215/1'RFF+AAE:N20'ERP+::0'ERC+ID0547::95'FTX+AAO+++VESSEL VOYAGE NOT FOUND VESSEL ID=9044748,VOYAGE NO=1611,LINKING VOYAGE NO=1611'TAX+3'MOA+39:0000000005000.00'TAX+3'MOA+40:0000000005000.00'TAX+3'MOA+68:0000000000600.00'TAX+3'MOA+128:0000000000070.00'TAX+3'MOA+26:0000000000014.00'TAX+3'MOA+35:0000000000006.00'TAX+3'MOA+23:0000000000050.00'DOC+1+1'CST+1+N20::95'FTX+AAF+++10%'TAX+1'MOA+40:0000000005000.00'TAX+1'MOA+68:0000000000600.00'TAX+1'MOA+146:000005000.0000'TAX+1'MOA+312:000000600.0000'CNT+5:1'UNT+46+000001'";
				JobComInvoiceLine invoiceLine;
				var iMDRMessage = CreateDataForUniversalTesting(JobMessageTypeList.Codes.Import, iMDRData, "B00160215", "ENT0123456", 110m, out invoiceLine);
				var declaration = invoiceLine.Declaration;
				declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardUpdatedPending;
				invoiceLine.JI_PartNo = ZString.Empty;
				Factory.Save();
				new IMDRMessageProcessor(new LoggingInformation()).ProcessMessage(iMDRMessage);
				Factory.Save();
				AssertEquals(true, Env.OutgoingCustomsMailManager.EmailsCreated.Exists(x => x.Body.Contains(@"<p><font color=""red"">Cannot create Stock Levels Update due to the following errors.")));
				var exportLog = declaration.Logs.MostRecentLogByEventTime(Events.DataExport);
				var dataContex = exportLog.RelatedEDIMessage.DataContext;
				AssertNotNull(dataContex.DataSourceCollection.FirstOrDefault(x => x.Type.GetValueOrDefault() == nameof(DataContextType.CustomsDeclaration)));
				AssertNotNull(dataContex.RecipientRoleCollection.FirstOrDefault(x => x.Code.Value == RecipientRoleType.BWI));
				AssertEquals(WarehouseTransactionStatusList.Codes.InwardUpdatedPending, declaration.WarehouseTransactionStatus);
			}
		}

		public void TestHandleErrorWhenPublishUniversalShipmentToBondedWarehouseInwardOnClear()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddYears(-1).ToDateTime());
				string iMDRData = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+IJCD 1357 J0D:1+11'FTX+AHN+++FINALISED:FINALISED'GIS+N:117:95'GIS+TLB:109:95'GIS+LLB:109:95'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95'NAD+IM++SECURECERTS PTY LTD'NAD+CB++WISETECH GLOBAL PTY LTD'RFF+ABO:B00160215/1/CMT1::1'RFF+ABT:ENT0123456::1'RFF+ABQ:OWN1611'RFF+ADU:B00160215/1'RFF+AAE:N20'ERP+::0'ERC+ID0547::95'FTX+AAO+++VESSEL VOYAGE NOT FOUND VESSEL ID=9044748,VOYAGE NO=1611,LINKING VOYAGE NO=1611'TAX+3'MOA+39:0000000005000.00'TAX+3'MOA+40:0000000005000.00'TAX+3'MOA+68:0000000000600.00'TAX+3'MOA+128:0000000000070.00'TAX+3'MOA+26:0000000000014.00'TAX+3'MOA+35:0000000000006.00'TAX+3'MOA+23:0000000000050.00'DOC+1+1'CST+1+N20::95'FTX+AAF+++10%'TAX+1'MOA+40:0000000005000.00'TAX+1'MOA+68:0000000000600.00'TAX+1'MOA+146:000005000.0000'TAX+1'MOA+312:000000600.0000'CNT+5:1'UNT+46+000001'";
				JobComInvoiceLine invoiceLine;
				var iMDRMessage = CreateDataForUniversalTesting(JobMessageTypeList.Codes.Import, iMDRData, "B00160215", "ENT0123456", 110m, out invoiceLine);
				var inwardDeclaration = invoiceLine.Declaration;
				Factory.Save();
				inwardDeclaration.PublishShipmentForWHSInward(false);
				var exportLog1 = inwardDeclaration.Logs.MostRecentLogByEventTime(Events.DataExport);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT0123456-1", 110m);

				var outwardEntry = GetNewEntryHeader(JobMessageTypeList.Codes.ExWarehouse, "B00160266", "EOUT0123457", 100m);
				var outwardDeclaration = outwardEntry.Declaration;
				var outwardInvoiceLine = outwardEntry.MergedLines[0].RandomLine;
				outwardInvoiceLine.AddInfo.ZA_WRN = "ENT0123456";
				outwardInvoiceLine.AddInfo.ZA_WRL = 1;
				Factory.Save();
				outwardDeclaration.PublishShipmentForWHSOutward(true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT0123456-1", 10m);

				invoiceLine.JI_InvoiceQuantity = 90m;
				inwardDeclaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreatedPending;
				Factory.Save();
				new IMDRMessageProcessor(new LoggingInformation()).ProcessMessage(iMDRMessage);
				Factory.Save();
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT0123456-1", 10m);
				AssertEquals(true, Env.OutgoingCustomsMailManager.EmailsCreated.Exists(x => x.Body.Contains(@"<p><font color=""red"">Cannot create Stock Levels Update due to the following errors.")));
				var exportLog = inwardDeclaration.Logs.MostRecentLogByEventTime(Events.DataExport);
				AssertNotEquals(exportLog, exportLog1);
				var dataContex = exportLog.RelatedEDIMessage.DataContext;
				AssertNotNull(dataContex.DataSourceCollection.FirstOrDefault(x => x.Type.GetValueOrDefault() == nameof(DataContextType.CustomsDeclaration)));
				AssertNotNull(dataContex.RecipientRoleCollection.FirstOrDefault(x => x.Code.Value == RecipientRoleType.BWI));
			}
		}

		public void TestPublishUniversalCancelEventToBondedWarehouseInwardOnWithdrawn()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddYears(-1).ToDateTime());
				string iMDRData = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+2JIC 3F2C 3BFD:1+11'FTX+AHN+++WITHDRAWN:WITHDRAWN'GIS+N:117:95'GIS+TLB:109:95'GIS+LLB:109:95'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95'NAD+IM++SECURECERTS PTY LTD'NAD+CB++WISETECH GLOBAL PTY LTD'RFF+ABO:B00160272/1/CMT3::5'RFF+ABT:ENT0123456::1'RFF+ABQ:OWN1302'RFF+ADU:B00160272/1'RFF+AAE:N20'DOC+1+1'CST+1+N20::95'FTX+AAF+++10%'CST+2+N20::95'FTX+AAF+++FREE'CNT+5:2'UNT+23+000001'";
				JobComInvoiceLine invoiceLine;
				var iMDRMessage = CreateDataForUniversalTesting(JobMessageTypeList.Codes.Import, iMDRData, "B00160272", "ENT0123456", 110m, out invoiceLine);
				var entry = (CusEntryHeader)iMDRMessage.EM_LinkedObject;
				entry.CH_Status = CustomsEntryStatus.AwaitingWithdrawal.Code;
				var declaration = entry.Declaration;
				Factory.Save();
				declaration.PublishShipmentForWHSInward(false);
				declaration.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT0123456-1", 110m);
				AssertEquals(WarehouseTransactionStatusList.Codes.InwardCreated, declaration.WarehouseTransactionStatus);

				var result = declaration.PublishCancelEventForWHSInwardAndSaveIfNeeded(true, Enterprise.Customs.Business.WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal);
				AssertEquals(result.ErrorMessage, WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal, declaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT0123456-1", 0m);

				new IMDRMessageProcessor(new LoggingInformation()).ProcessMessage(iMDRMessage);
				entry.CH_Status = CustomsEntryStatus.ClearWithdrawal.Code;
				Factory.Save();
				AssertEquals(true, Env.OutgoingCustomsMailManager.EmailsCreated.Exists(x => x.Body.Contains("<p>Stock Levels Update has been canceled.")));
				var exportLog = declaration.Logs.MostRecentLogByEventTime(Events.DataExport);
				var dataContex = exportLog.RelatedEDIMessage.DataContext;
				AssertNotNull(dataContex.DataSourceCollection.FirstOrDefault(x => x.Type.GetValueOrDefault() == nameof(DataContextType.CustomsDeclaration)));
				AssertNotNull(dataContex.RecipientRoleCollection.FirstOrDefault(x => x.Code.Value == RecipientRoleType.BWI));
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT0123456-1", 0m);
			}
		}

		public void TestPublishUniversalAcceptEventToBondedWarehouseOutwardOnFinalised()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddYears(-1).ToDateTime());
				var inwardEntry = GetNewEntryHeader(JobMessageTypeList.Codes.Import, "B00160265", "EIN0123456", 110m);
				var inwardDeclaration = inwardEntry.Declaration;

				string iMDRData = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+401J AB2B 895D:1+11'FTX+AHN+++CLEAR:CLEAR'GIS+N:117:95'GIS+TLB:109:95'GIS+LLB:109:95'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95'NAD+IM++SECURECERTS PTY LTD'NAD+CB++WISETECH GLOBAL PTY LTD'RFF+ABO:B00160266/1/CMT2::3'RFF+ABT:EOUT0123457::1'RFF+ABQ:OWN1831'RFF+ADU:B00160266/1'RFF+AAE:N30'TAX+3'MOA+40:0000000010000.00'TAX+3'MOA+55:0000000001000.00'TAX+3'MOA+369:0000000001220.00'TAX+3'MOA+128:0000000001110.00'DOC+1'CST+1+N30::95'FTX+AAF+++10%'TAX+1'MOA+40:0000000005000.00'TAX+1'MOA+55:0000000000500.00'TAX+1'MOA+369:0000000000610.00'TAX+1'MOA+56:0000000006100.00'TAX+1'MOA+68:0000000000600.00'TAX+1'MOA+146:000005000.0000'TAX+1'MOA+312:000000600.0000'CST+2+N30::95'FTX+AAF+++10%'TAX+1'MOA+40:0000000005000.00'TAX+1'MOA+55:0000000000500.00'TAX+1'MOA+369:0000000000610.00'TAX+1'MOA+56:0000000006100.00'TAX+1'MOA+68:0000000000600.00'TAX+1'MOA+146:000005000.0000'TAX+1'MOA+312:000000600.0000'CNT+5:2'UNT+59+000001'";
				JobComInvoiceLine outwardInvoiceLine;
				var iMDRMessage = CreateDataForUniversalTesting(JobMessageTypeList.Codes.ExWarehouse, iMDRData, "B00160266", "EOUT0123457", 50m, out outwardInvoiceLine);
				outwardInvoiceLine.AddInfo.ZA_WRN = "EIN0123456";
				outwardInvoiceLine.AddInfo.ZA_WRL = 1;
				var outwardDeclaration = outwardInvoiceLine.Declaration;
				Factory.Save();
				inwardDeclaration.PublishShipmentForWHSInward(false);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("EIN0123456-1", 110m);
				outwardDeclaration.PublishShipmentForWHSOutward(true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("EIN0123456-1", 60m);
				new IMDRMessageProcessor(new LoggingInformation()).ProcessMessage(iMDRMessage);
				Factory.Save();
				AssertEquals(true, Env.OutgoingCustomsMailManager.EmailsCreated.Exists(x => x.Body.Contains("<p>Stock Release can be finalized. (WHS Order: <a href=")));
				var declaration = outwardInvoiceLine.Declaration;
				var exportLog = declaration.Logs.MostRecentLogByEventTime(Events.DataExport);
				var dataContex = exportLog.RelatedEDIMessage.DataContext;
				AssertNotNull(dataContex.DataSourceCollection.FirstOrDefault(x => x.Type.GetValueOrDefault() == nameof(DataContextType.CustomsDeclaration)));
				AssertNotNull(dataContex.RecipientRoleCollection.FirstOrDefault(x => x.Code.Value == RecipientRoleType.BWR));
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("EIN0123456-1", 60m);
			}
		}

		public void TestPublishUniversalAcceptEventToBondedWarehouseOutwardOnClear()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddYears(-1).ToDateTime());
				var inwardEntry = GetNewEntryHeader(JobMessageTypeList.Codes.Import, "B00160265", "EIN0123456", 110m);
				var inwardDeclaration = inwardEntry.Declaration;

				string iMDRData = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+12A6 08CG 895D:1+11'FTX+AHN+++FINALISED:FINALISED'GIS+N:117:95'GIS+TLB:109:95'GIS+LLB:109:95'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95'NAD+IM++SECURECERTS PTY LTD'NAD+CB++WISETECH GLOBAL PTY LTD'RFF+ABO:B00160266/1/CMT2::2'RFF+ABT:EOUT0123457::1'RFF+ABQ:OWN1831'RFF+ADU:B00160266/1'RFF+AAE:N30'TAX+3'MOA+40:0000000005000.00'TAX+3'MOA+55:0000000000500.00'TAX+3'MOA+369:0000000000610.00'TAX+3'MOA+128:0000000001133.20'TAX+3'MOA+23:0000000000023.20'DOC+1'CST+1+N30::95'FTX+AAF+++10%'TAX+1'MOA+40:0000000005000.00'TAX+1'MOA+55:0000000000500.00'TAX+1'MOA+369:0000000000610.00'TAX+1'MOA+56:0000000006100.00'TAX+1'MOA+68:0000000000600.00'TAX+1'MOA+146:000005000.0000'TAX+1'MOA+312:000000600.0000'CNT+5:1'UNT+45+000001'";
				JobComInvoiceLine outwardInvoiceLine;
				var iMDRMessage = CreateDataForUniversalTesting(JobMessageTypeList.Codes.ExWarehouse, iMDRData, "B00160266", "EOUT0123457", 50m, out outwardInvoiceLine);
				outwardInvoiceLine.AddInfo.ZA_WRN = "EIN0123456";
				outwardInvoiceLine.AddInfo.ZA_WRL = 1;
				var outwardDeclaration = outwardInvoiceLine.Declaration;
				Factory.Save();
				inwardDeclaration.PublishShipmentForWHSInward(false);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("EIN0123456-1", 110m);
				outwardDeclaration.PublishShipmentForWHSOutward(true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("EIN0123456-1", 60m);
				new IMDRMessageProcessor(new LoggingInformation()).ProcessMessage(iMDRMessage);
				Factory.Save();
				AssertEquals(true, Env.OutgoingCustomsMailManager.EmailsCreated.Exists(x => x.Body.Contains("<p>Stock Release can be finalized. (WHS Order: <a href=")));
				var exportLog = outwardDeclaration.Logs.MostRecentLogByEventTime(Events.DataExport);
				var dataContex = exportLog.RelatedEDIMessage.DataContext;
				AssertNotNull(dataContex.DataSourceCollection.FirstOrDefault(x => x.Type.GetValueOrDefault() == nameof(DataContextType.CustomsDeclaration)));
				AssertNotNull(dataContex.RecipientRoleCollection.FirstOrDefault(x => x.Code.Value == RecipientRoleType.BWR));
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("EIN0123456-1", 60m);
			}
		}

		public void TestPublishUniversalCancelEventToBondedWarehouseOutwardOnWithdrawn()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddYears(-1).ToDateTime());
				var inwardEntry = GetNewEntryHeader(JobMessageTypeList.Codes.Import, "B00160265", "EIN0123456", 110m);
				var inwardDeclaration = inwardEntry.Declaration;
				string iMDRData = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+2D1A GF1B 6EAD:1+11'FTX+AHN+++WITHDRAWN:WITHDRAWN'GIS+N:117:95'GIS+TLB:109:95'GIS+LLB:109:95'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95'NAD+IM++SECURECERTS PTY LTD'NAD+CB++WISETECH GLOBAL PTY LTD'RFF+ABO:B00160273/1/CMT4::5'RFF+ABT:EOUT0123457::1'RFF+ABQ:OWN0955'RFF+ADU:B00160273/1'RFF+AAE:N30'DOC+1'CST+1+N30::95'FTX+AAF+++10%'CNT+5:1'UNT+21+000001'";
				JobComInvoiceLine outwardInvoiceLine;
				var iMDRMessage = CreateDataForUniversalTesting(JobMessageTypeList.Codes.ExWarehouse, iMDRData, "B00160273", "EOUT0123457", 50m, out outwardInvoiceLine);
				outwardInvoiceLine.AddInfo.ZA_WRN = "EIN0123456";
				outwardInvoiceLine.AddInfo.ZA_WRL = 1;
				var entryHeader = (CusEntryHeader)iMDRMessage.EM_LinkedObject;
				var outwardDeclaration = entryHeader.Declaration;
				inwardDeclaration.WarehouseAddress.SetWarehouseType(false);
				Factory.Save();
				inwardDeclaration.WarehouseAddress.SetWarehouseType(true);
				Factory.Save();
				inwardDeclaration.PublishShipmentForWHSInward(false);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("EIN0123456-1", 110m);
				Warehouse.MainAddress.SetWarehouseType(ZBool.False);
				Factory.Save();
				outwardDeclaration.PublishShipmentForWHSOutward(true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("EIN0123456-1", 60m);
				new IMDRMessageProcessor(new LoggingInformation()).ProcessMessage(iMDRMessage);
				entryHeader.CH_Status = CustomsEntryStatus.ClearWithdrawal.Code;
				Factory.Save();
				AssertEquals(true, Env.OutgoingCustomsMailManager.EmailsCreated.Exists(x => x.Body.Contains("<p>Stock Release has been canceled. (WHS Order: <a href=")));
				var exportLog = outwardDeclaration.Logs.MostRecentLogByEventTime(Events.DataExport);
				var dataContex = exportLog.RelatedEDIMessage.DataContext;
				AssertNotNull(dataContex.DataSourceCollection.FirstOrDefault(x => x.Type.GetValueOrDefault() == nameof(DataContextType.CustomsDeclaration)));
				AssertNotNull(dataContex.RecipientRoleCollection.FirstOrDefault(x => x.Code.Value == RecipientRoleType.BWR));
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("EIN0123456-1", 110m);
			}
		}

		public void TestHandleErrorWhenPublishUniversalCancelEventToBondedWarehouseOutwardOnWithdrawn()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddYears(-1).ToDateTime());
				var inwardEntry = GetNewEntryHeader(JobMessageTypeList.Codes.Import, "B00160265", "ENT0123457", 200m);
				var inwardDeclaration = inwardEntry.Declaration;
				inwardDeclaration.WarehouseAddress.SetWarehouseType(false);
				Factory.Save();
				inwardDeclaration.WarehouseAddress.SetWarehouseType(true);
				Factory.Save();
				inwardDeclaration.PublishShipmentForWHSInward(false);
				inwardDeclaration.WarehouseAddress.SetWarehouseType(false);

				string iMDRData = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+2D1A GF1B 6EAD:1+11'FTX+AHN+++WITHDRAWN:WITHDRAWN'GIS+N:117:95'GIS+TLB:109:95'GIS+LLB:109:95'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95'NAD+IM++SECURECERTS PTY LTD'NAD+CB++WISETECH GLOBAL PTY LTD'RFF+ABO:B00160273/1/CMT4::5'RFF+ABT:ENT0123456::1'RFF+ABQ:OWN0955'RFF+ADU:B00160273/1'RFF+AAE:N30'DOC+1'CST+1+N30::95'FTX+AAF+++10%'CNT+5:1'UNT+21+000001'";
				JobComInvoiceLine invoiceLine;
				var iMDRMessage = CreateDataForUniversalTesting(JobMessageTypeList.Codes.ExWarehouse, iMDRData, "B00160273", "ENT0123456", 110m, out invoiceLine);
				var entry = (CusEntryHeader)iMDRMessage.EM_LinkedObject;
				invoiceLine.AddInfo.ZA_WRN = "ENT0123457";
				invoiceLine.AddInfo.ZA_WRL = 1;
				var outwwardDeclaration = invoiceLine.Declaration;
				Factory.Save();
				var result = outwwardDeclaration.PublishShipmentForWHSOutward(true);
				var warehouseJob = (IWhsOrder)result.FindJobIfExists();
				warehouseJob.WD_DocketStatus = "PIC";
				warehouseJob.WD_FinalisedDate = new ZDateTimeOffset(2014, 1, 1);
				warehouseJob.Factory.Save();
				var exportLog1 = outwwardDeclaration.Logs.MostRecentLogByEventTime(Events.DataExport);

				new IMDRMessageProcessor(new LoggingInformation()).ProcessMessage(iMDRMessage);
				entry.CH_Status = CustomsEntryStatus.ClearWithdrawal.Code;
				Factory.Save();
				AssertEquals(true, Env.OutgoingCustomsMailManager.EmailsCreated.Exists(x => x.Body.Contains(@"<p><font color=""red"">Cannot cancel Stock Release due to the following errors.")));
				var declaration = invoiceLine.Declaration;
				var exportLog = declaration.Logs.MostRecentLogByEventTime(Events.DataExport);
				AssertNotEquals(exportLog1, exportLog);
				var dataContex = exportLog.RelatedEDIMessage.DataContext;
				AssertNotNull(dataContex.DataSourceCollection.FirstOrDefault(x => x.Type.GetValueOrDefault() == nameof(DataContextType.CustomsDeclaration)));
				AssertNotNull(dataContex.RecipientRoleCollection.FirstOrDefault(x => x.Code.Value == RecipientRoleType.BWR));
			}
		}

		public void TestCusEntryPayInfoWhenREFACCComesFirst()
		{
			entryHeader.EntryPayInfos.RemoveAndDeleteAll();
			AssertEquals("No payment information", 0, entryHeader.EntryPayInfos.Count);

			entryHeader.Declaration.JE_PaymentMethod = "IMP";
			entryHeader.CH_BGMReference = "B00122382/1";
			CMRREFACCMessage refundMessage = Factory.New<CMRREFACCMessage>();
			refundMessage.EM_MessageText = CMRImportDeclarationTestData.REFACC;
			refundMessage.EM_MessageNum = "0001007";
			new REFACCMessageProcessor(new LoggingInformation()).ProcessMessage(refundMessage);
			AssertEquals("There should be one PayInfo", 1, entryHeader.EntryPayInfos.Count);
			CusEntryPayInfo payInfo = entryHeader.EntryPayInfos[0];

			AssertEquals(true, payInfo.C9_RemAdvReceived);
			AssertEquals(false, payInfo.C9_CusResReceived);
			AssertEquals(PaymentTransactionTypeList.Codes.Refund, payInfo.C9_TransactionType);
			AssertEquals(CusEntryPayInfoStatusList.Codes.Pending, payInfo.C9_PaymentStatus);
			AssertEquals("", payInfo.C9_IncomingPayResponseNo);
			AssertEquals(0m, payInfo.C9_PaymentAmount);
			AssertEquals("IMP", payInfo.C9_PaymentParty);

			incomingMessage.EM_MessageType = "IMD";
			incomingMessage.EM_MessageText = CMRImportDeclarationTestData.IMDRClearWithNegative;
			incomingMessage.EM_MessageNum = "0001008";
			GetMessageProcessor().ProcessMessage(incomingMessage);

			AssertEquals("There should be only one cusEntryPayInfo", 1, entryHeader.EntryPayInfos.Count);
			AssertEquals(true, payInfo.C9_RemAdvReceived);
			AssertEquals(true, payInfo.C9_CusResReceived);
			AssertEquals(PaymentTransactionTypeList.Codes.Refund, payInfo.C9_TransactionType);
			AssertEquals(CusEntryPayInfoStatusList.Codes.Clear, payInfo.C9_PaymentStatus);
			AssertEquals("0001008", payInfo.C9_IncomingPayResponseNo);
			AssertEquals(-71m, payInfo.C9_PaymentAmount);
			AssertEquals("IMP", payInfo.C9_PaymentParty);
		}

		public void TestSetWarehouseUnitValue()
		{
			string iMDWithWarehouseUnitValue = "UNH+000002+CUSRES:D:99B:UN'BGM+961:::IMDR+34B9 8CFF 9D56:1+11'FTX+AHN+++FINALISED:FINALISED'GIS+N:117:95'GIS+TLB:109:95'GIS+LLB:109:95'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95'NAD+IM++BOLEROPLUS PTY LTD'NAD+CB++EAGLE DATAMATION INTERNATIONAL PTY:LIMIT'RFF+ABO:B00149171/1/CMT3::3'RFF+ABT:AAAA6RHTE::1'RFF+ABQ:NMNM'RFF+ADU:B00149171/1'RFF+AAE:N10/N20'ERP+::0'ERC+ID0547::95'FTX+AAO+++VESSEL VOYAGE NOT FOUND VESSEL ID=9065182,VOYAGE NO=999,LINKING VOYAGE NO=999'TAX+3'MOA+39:0000000002500.00'TAX+3'MOA+40:0000000002500.00'TAX+3'MOA+369:0000000000203.36'TAX+3'MOA+68:0000000000042.00'TAX+3'MOA+128:0000000000263.61'TAX+3'MOA+26:0000000000007.00'TAX+3'MOA+35:0000000000003.75'TAX+3'MOA+23:0000000000049.50'DOC+1+1'CST+1+N20::95'FTX+AAF+++FREE'TAX+1'MOA+40:0000000000500.00'TAX+1'MOA+68:0000000000008.40'TAX+1'MOA+146:000000012.5000'TAX+1'MOA+312:000000000.2100'CST+2+N10::95'FTX+AAF+++FREE'TAX+1'MOA+40:0000000002000.00'TAX+1'MOA+369:0000000000203.36'TAX+1'MOA+56:0000000002033.60'TAX+1'MOA+68:0000000000033.60'CNT+5:2'UNT+58+000002'";

			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = header.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line2 = header.JobComInvoiceLines.AddNew();
			line2.AddInfo.ZA_WUV = 10m;

			declaration.JE_DeclarationReference = "B00149171";

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "B00149171/1";
			CusEntryLine entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;

			CusEntryLine entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			entryLine2.CL_WarehouseUnitValue = 10m;

			line1.JI_CL = entryLine1.PK;
			line2.JI_CL = entryLine2.PK;

			EDIMessage iMDRMessage = declaration.CustomsEntryHeaders[0].Messages.AddNew(typeof(CMRIMDRMessage));
			iMDRMessage.EM_MessageText = iMDWithWarehouseUnitValue;
			iMDRMessage.EM_LinkedObject = entryHeader;

			new IMDRMessageProcessor(new LoggingInformation()).ProcessMessage(iMDRMessage);

			AssertEquals("WUV for EntryLine1 should be 12.5 as advised in the response message", 12.5m, entryLine1.CL_WarehouseUnitValue);
			AssertEquals("WUV for InvoiceLine1 should be 12.5 as advised in the response message", 12.5m, line1.AddInfo.ZA_WUV);

			AssertEquals("WUV for EntryLine2 should be 0 as advised in the response message", 0m, entryLine2.CL_WarehouseUnitValue);
			AssertEquals("WUV for InvoiceLine2 should be 0 as advised in the response message", 0m, line2.AddInfo.ZA_WUV);
		}

		public void TestSetHeaderLevelAmountsIfTheyAreReported()
		{
			string firstIMD = "UNH+1+CUSDEC:D:99B:UN'BGM+929:::IMD+B00148469/6/SYD1:1+9'CST++N20::95'LOC+8+AUSYD::6'LOC+12+AUSYD::6'LOC+9+USLAX::6'LOC+18+9078E::95'LOC+79+AUSYD::6'DTM+260:20051007:102'DTM+252:20051007:102'GIS+EPA:109:95'GIS+Y:153:95'GIS+LLB:109:95'GIS+TLB:109:95'FII+COQ+323232+:::242200::215'MEA+AAE+G+KG:150.00000'FTX+DEL+++ABC IMPORTS PTY LTD'RFF+ABQ:1050'RFF+ADU:B00148469/6'RFF+ANU:B'RFF+APH:FOB'RFF+AMG:0001'TDT+20++A++QF::3'NAD+AT+66015286036::95'NAD+VT+AA33HF::95'NAD+DP++ALEXANDRIA++156 MAIN RD++:::NSW+2015+AU'NAD+CB+54321::95'MOA+63:35000.00:AUD'MOA+141:35720.00:AUD'MOA+39:35000.00:AUD'MOA+313:600.00:AUD'MOA+71:120.00:AUD'UNS+D'DMS+1'LIN+1+I'PAC+1+1'PCI+1'RFF+MWB:66666666666'PCI+1'RFF+HWB:TESTING BOND ID'CST+1+I::95+N20::95'FTX+AAA+++COW CARCASSES'LOC+27+NZ::5'MEA+AAA+::WAR+NO:1.00'NAD+SU+AAA3336647M::95'PAC++1'PCI+1'FTX+RAH+++00318:00325:N'PCI+1'FTX+RAH+++00269:00230:N'PCI+1'FTX+RAH+++00007:00022:N'MOA+38:15000.00:AUD'MOA+68:308.57:AUD'RFF+ABD:87032220'RFF+AED:07'RFF+AGW:GEN'RFF+AKG:V1'RFF+AFV:TV'CST+2+I::95+N20::95'FTX+AAA+++COW CARCASSES'LOC+27+NZ::5'MEA+AAA+::WAR+NO:1.00'NAD+SU+AAA3336647M::95'PAC++1'PCI+1'FTX+RAH+++00318:00325:N'PCI+1'FTX+RAH+++00269:00230:N'PCI+1'FTX+RAH+++00007:00022:N'MOA+38:20000.00:AUD'MOA+68:411.43:AUD'RFF+ABD:87032220'RFF+AED:07'RFF+AGW:GEN'RFF+AKG:V2'RFF+AFV:TV'UNS+S'UNT+80+1'";
			string firstIMDR = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+310F HD38 HJB5:1+11'FTX+AHN+++FINALISED:FINALISED'GIS+N:117:95'GIS+TLB:109:95'GIS+LLB:109:95'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95'NAD+IM++AUSTRALIAN CUSTOMS SERVICE'NAD+CB++EAGLE DATAMATION INTERNATIONAL PTY:LIMIT'RFF+ABO:B00148469/6/SYD1::1'RFF+ABT:AAAA6RHTE::1'RFF+ABQ:1050'RFF+ADU:B00148469/6'RFF+AAE:N20'ERP+::0'ERC+ID0545::95'FTX+AAO+++AIR WAYBILL HAS NOT BEEN REPORTED'ERP+::1'ERC+ID0302::95'FTX+AAO+++COMMUNITY PROTECTION PERMIT IS APPLICABLE BUT NOT REQUIRED'ERP+::2'ERC+ID0302::95'FTX+AAO+++COMMUNITY PROTECTION PERMIT IS APPLICABLE BUT NOT REQUIRED'TAX+3'MOA+39:0000000035000.00'TAX+3'MOA+40:0000000035000.00'TAX+3'MOA+68:0000000000720.00'TAX+3'MOA+128:0000000000036.60'TAX+3'MOA+26:0000000000006.50'TAX+3'MOA+23:0000000000030.10'DOC+1+1'CST+1+N20::95'FTX+AAF+++5%'TAX+1'GIS+LAQ:109:95'TAX+1'MOA+40:0000000015000.00'TAX+1'MOA+68:0000000000308.57'TAX+1'MOA+146:000015000.0000'TAX+1'MOA+312:000000308.5700'CST+2+N20::95'FTX+AAF+++5%'TAX+1'GIS+LAQ:109:95'TAX+1'MOA+40:0000000020000.00'TAX+1'MOA+68:0000000000411.43'TAX+1'MOA+146:000020000.0000'TAX+1'MOA+312:000000411.4300'ERP+::1'ERC+1::95'ERC+2::95'FTX+ABS+++QUARANTINE ACT 1908. QUARANTINE PERMISSION TO DELIVER TO THE IMPORTER REQUIRED'ERP+::26'ERC+1::95'ERC+2::95'FTX+ABS+++QUARANTINE CLEARANCE REQUIRED FOR USED OR SECONDHAND OR FIELD TESTED GOODS'ERP+::221'ERC+1::95'ERC+2::95'FTX+ABS+++IMPORTERS OF SUBSTANCES OR PRE-CHARGED EQUIPMENT CONTROLLED UNDER REGULATION 5K SCHEDULE 10 OF THE CUSTOMS (PROHIBITED IMPORTS) REGULATIONS REQUIRE A LICENCE FROM DEH'ERP+::274'ERC+1::95'ERC+2::95'FTX+ABS+++THE IMPORT OF MILITARY OR EX-MILITARY GOODS IS RESTRICTED UNDER THE CUSTOMS (PROHIBITED IMPORTS) REGULATIONS - AN IMPORT PERMIT MAY BE REQUIRED.'CNT+5:2'UNT+80+000001'";
			string rejectIMDR = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+5FF6 BC3H JB5:1+11'NAD+MR+AAA374M::95'RFF+ABO:B00148469/6/SYD1::1'ERP+::0'ERC+CG0302::95'FTX+AAO+++MESSAGE ALREADY EXISTS Message ID=2005-10-10-12.02.52.718293,Sender Ref No=B00148469/6/SYD1,Sender Ref Ver No=001'CNT+55:1'UNT+9+000001'";

			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_DeclarationReference = "B00148469";

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "B00148469/6";

			CMRIMDMessage iMD = Factory.New<CMRIMDMessage>();
			iMD.EM_MessageText = firstIMD;
			iMD.EM_ReceiveTransmit = "TRX";
			entryHeader.Messages.Add(iMD);

			incomingMessage = entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			incomingMessage.EM_MessageText = firstIMDR;
			incomingMessage.EM_LinkedObject = entryHeader;
			processor.ProcessMessage(incomingMessage);
			AssertEquals("Entry header has charges attached", true, entryHeader.Charges.Count > 0);
			AssertEquals("Declaration Processing Charge > 0", true, entryHeader.DeclarationProcessingCharge > 0);
			AssertEquals("ErrorEmailCount", 0, processor.ErrorEmailSendCount);
			AssertEquals("AcknowledgementEmailCount", 1, processor.AcknowledgementEmailSendCount);

			incomingMessage = entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			incomingMessage.EM_MessageText = rejectIMDR;
			incomingMessage.EM_LinkedObject = entryHeader;
			processor.ProcessMessage(incomingMessage);
			AssertEquals("Rejection message without any charges reported didnt wipe out charges", true, entryHeader.Charges.Count > 0);
			AssertEquals("Declaration Processing Charge > 0", true, entryHeader.DeclarationProcessingCharge > 0);
			AssertEquals("ErrorEmailCount", 1, processor.ErrorEmailSendCount);
			AssertEquals("AcknowledgementEmailCount", 1, processor.AcknowledgementEmailSendCount);
		}

		public void TestHeaderChargesAreRemovedWhenCustomsReportsNoChargesAreApplicable()
		{
			var messageThatShouldResultInNoCharges =
@"UNH+000001+CUSRES:D:99B:UN
BGM+961:::IMDR+2JB1 6CIH 1E50:1+11
FTX+AHN+++FINALISED:FINALISED
GIS+N:117:95
GIS+TLB:109:95
GIS+LLB:109:95
NAD+MR+AAA374M::95
NAD+VT+AA33HF::95
NAD+CB+54321::95
NAD+IM++DEAKIN KM CLIENT1
NAD+CB++WISETECH GLOBAL LIMITED
RFF+ABO:B00122382/1/CMT2::2
RFF+ABT:AAAFGRWKA::1
RFF+ABQ:1
RFF+ADU:B00178578/1
RFF+AAE:N10
ERP+::0
ERC+ID0545::95
FTX+AAO+++AIR WAYBILL HAS NOT BEEN REPORTED
TAX+3
MOA+39:0000000002500.00
TAX+3
MOA+40:0000000002500.00
TAX+3
MOA+68:0000000000185.00
DOC+1+1
CST+1+N10::95
FTX+AAF+++FREE
TAX+1
MOA+40:0000000002500.00
TAX+1
MOA+56:0000000002685.00
TAX+1
MOA+68:0000000000185.00
CNT+5:1
UNT+36+000001";

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee(CMRDeclarationChargeCalculator.Constants.Q1A, 10.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee(CMRDeclarationChargeCalculator.Constants.Q2A, 15.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee(CMRDeclarationChargeCalculator.Constants.Q1S, 50.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee(CMRDeclarationChargeCalculator.Constants.Q2S, 55.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			var taxL = helper.CreateTaxOrFee(CMRDeclarationChargeCalculator.Constants.DAN, 50.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FBT", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			taxL.ZZF_Threshold = 10000m;
			var taxH = helper.CreateTaxOrFee("DAH", 100.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FBT", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			Factory.Save();

			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
			declaration.JE_DeclarationReference = "B00122383";
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 120000;
			invoice.JZ_RX_NKInvoice_Currency = "AUD";
			invoice.AddInfo.ZA_PST = "GEN";
			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 2500.0m;
			line.JI_Tariff = "9999.31.03 03";
			line.JI_Description = "AAAAA";
			line.JI_CustomsUnitQty = "CU";
			line.JI_CustomsQuantity = 100;

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "TSTIMP";
			declaration.JE_OH_Importer = importer.PK;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();

			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals(10.00m, entry.Charges[ChargeTypes.AQISProcessingCharge].C1_ChargeAmount);
			AssertEquals(50.00m, entry.Charges[ChargeTypes.DeclarationProcessingCharge].C1_ChargeAmount);
			var entryLine = entry.MergedLines[0];
			AssertEquals("line Duty is FREE due to the Tariff", 0.0m, entryLine.DutyAmount);

			entry.CH_BGMReference = "B00122382/1";
			entry.CH_Status = CustomsEntryStatus.AwaitingFormalLodge.Code;

			var outMessage = Factory.New<CMRIMDMessage>();
			entry.Messages.Add(outMessage);
			outMessage.EM_MessageText = "UNH+1+CUSDEC:D:99B:UN'BGM+929:::IMD+B00122382/1:1+9'CST++10::95'LOC+8+AUMEL::6'LOC+9+SGSIN::6'LOC+12+AUMEL::6'LOC+79+AUMEL::6'DTM+7:20041229:102'DTM+252:20041229:102'DTM+260:20041229:102'MEA+AAE+G+KG:123.00'RFF+ABQ'RFF+APH:FOB'PAC+0'PAC++1'TDT+20+123+S(SEA)+++++9044748::11'NAD+AT+31006626946::95'NAD+CB+00657C'MOA+141:150.00:AUD'MOA+40:150.00:AUD'MOA+63:150.00:AUD'MOA+39:150.00:AUD'UNS+D'DMS+1'RFF+BH:HOUSE BILL'RFF+MB:OCEANBILL'CST+1+LINE ACTION CODE::95+N10::95'FTX+AAA+++REFILLABLE, LIQUID INK REFILL'LOC+27+US::5'NAD+SU+2997664Y::95'PCI+1'MOA+40:150.00:AUD'MOA+38:150.00:AUD'MOA+5:0.03:AUD'RFF+ABA'RFF+ABC'RFF+ABD:96081000'RFF+ABL'RFF+AEA:45'RFF+AES'RFF+AFD'UNT+42+1'";
			outMessage.EM_LinkedObject = entry;
			outMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			outMessage.EM_Status = EDIMessage.Status.Sent;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			outMessage.GetLogs().AddNew(Events.AddedARecordToTheSystem, new ZDateTimeOffset(2005, 1, 1));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			var inMessage = entry.Messages.AddNew(typeof(CMRIMDRMessage));
			inMessage.EM_MessageText = messageThatShouldResultInNoCharges.Replace("\r\n", "'");

			new IMDRMessageProcessor(new LoggingInformation()).ProcessMessage(inMessage);
			Factory.Save();

			entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("DeclarationProcessing Charge should be updated when an original response is processed", 0.0m, entry.DeclarationProcessingCharge);
			AssertEquals("AQIS Processing Charge should be updated when an original response is processed", 0.0m, entry.AQISProcessingCharge);

			AssertEquals(CustomsEntryStatus.ClearFormalLodge.Code, entry.CH_Status);
			AssertEquals("FIN", declaration.JE_EntryStatus);
		}

		public void TestSetLineLevelCharges()
		{
			entryLine1.CL_CustomsValue = 100m;//should be updated
			entryLine1.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DutyAmount, 10m);//Should be removed
			entryLine1.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.GSTAmount, 20m);//should be removed

			incomingMessage = entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			incomingMessage.EM_MessageText = CMRImportDeclarationTestData.IMDRClear;
			processor.ProcessMessage(incomingMessage);

			AssertEquals("Entry Line Customs value updated", 44240.26m, entryLine1.CL_CustomsValue);
			AssertEquals("Duty amount updated", 0m, entryLine1.DutyAmount);
			AssertEquals("GST Amount updated", 0m, entryLine1.GSTVATAmount);
			AssertEquals("GST amount deferred", 4646.86m, entryLine1.GSTVATDeferred);
		}

		public void TestReportEmail()
		{
			CMRIMDRMessage message = Factory.New<CMRIMDRMessage>();
			message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'" +
				 "BGM+961:::IMDR+AJ3D AI8G 755:1+11'FTX+AHN+++HELDXX:HELDYY'GIS+N:117:95'GIS+TLB:109:95'GIS+LLB:109:95'NAD+MR+AAA447Y::95'" +
				 "NAD+VT+AA33JL::95'NAD+CB+54333::95'NAD+IM++PORTER DATA MANAGEMENT PTY LTD'NAD+CB++SOFTWARE CRAFT PTY LTD'RFF+ABO:B00122382/1/1::1'" +
				 "RFF+ABT:AAAAT33FA::1'RFF+ABQ:433407'RFF+ADU:B00122382'RFF+AAE:N10'ERP+::0'ERC+ID0060::95'FTX+AAO+++INVOICE TOTAL AMOUNT IS LESS THAN FREE ON BOARD AMOUNT'" +
				 "TAX+3'MOA+39:0000000002534.21'TAX+3'MOA+40:0000000002534.21'TAX+3'MOA+369:0000000000266.09'TAX+3'MOA+68:0000000000126.71'TAX+3'" +
				 "MOA+292:0000000009450.00'TAX+3'MOA+Z01:0000000009876.54'TAX+3'" +
				 "MOA+128:0000000000297.84'TAX+3'MOA+26:0000000000002.50'TAX+3'MOA+23:0000000000029.25'DOC+1+1'FTX+AHN+++HELD:HELD'CST+1+N10::95'" +
				 "FTX+AAF+++FREE'TAX+1'GIS+LAQ:109:95'TAX+1'MOA+40:0000000002534.21'TAX+1'MOA+369:0000000000266.09'TAX+1'MOA+68:0000000000126.71'" +
				 "ERP+::2'ERC+1::95'FTX+ABS+++IFIS (IMPORTED FOOD INSPECTION SCHEME) CLEARANCE REQUIRED'ERP+::3'ERC+1::95'FTX+ABS+++IMPORTED FOOD " +
				 "CONTROL ACT 1992. IFIS (IMPORTED FOOD INSPECTION SCHEME) PERMISSION TO DELIVER TO THE IMPORTER REQUIRED'CNT+5:1'UNT+55+000001'" +
				 "UNZ+1+00000000003899'";
			message.EM_LinkedObject = entryHeader;
			TestHelperIMDRMessageProcessor processor = new TestHelperIMDRMessageProcessor(logger);
			EmailDef reportEmail = processor.GetReportfForTest(message);
			AssertContains("Email Report Text", string.Format(@"<strong>
Reference Number: B00122382/1<br>Declaration Reference: {0}<br>Housebill: 31528<br>Masterbill: MasterBill<br>Origin: GBLON<br>Destination: AUSYD<br><br>Status: HELDXX<br>Status Description: HELDYY<br><br>Total Security Concession Amount: 9450.00<br><br>Total Security Uncollected Amount: 9876.54<br>
<br />
</strong>An Import Declaration Response(IMDR) message has been received from the ACS.<br />
<br />
Errors:<br><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Location</th><th>Code</th><th>Text</th></tr></thead><tr><td>&nbsp;</td><td>ID0060</td><td>INVOICE TOTAL AMOUNT IS LESS THAN FREE ON BOARD AMOUNT</td></tr></table>
<br />
Transport(Pack) Line status:<br><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Line</th><th>Status</th><th>Description</th></tr></thead><tr><td>LINE 1</td><td>HELD</td><td>HELD</td></tr></table>
<br />
Message Advices<br><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Location</th><th>Code</th><th>text</th></tr></thead><tr><td>LINE 1</td><td>2</td><td>IFIS (IMPORTED FOOD INSPECTION SCHEME) CLEARANCE REQUIRED</td></tr><tr><td>LINE 1</td><td>3</td><td>IMPORTED FOOD CONTROL ACT 1992. IFIS (IMPORTED FOOD INSPECTION SCHEME) PERMISSION TO DELIVER TO THE IMPORTER REQUIRED</td></tr></table>
<hr />
<br />
<strong><large>
</strong></large>
Regards,<br />", EmailDefBuilder.GetJobLink(declaration, declaration.JE_DeclarationReference)), reportEmail.Body);
		}

		new public void TestUpdateEntryFeesForOriginalVsAmendment()
		{
			entryHeader.CH_BGMReference = "B00149481/1";
			entryHeader.Declaration.JE_DeclarationReference = "B00149481";

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingFormalLodge.Code;
			entryHeader.Charges[ChargeTypes.AQISProcessingCharge].C1_ChargeAmount = 30.10m;
			entryHeader.Charges[ChargeTypes.DeclarationProcessingCharge].C1_ChargeAmount = 30.10m;

			var outMessage = Factory.New<CMRIMDMessage>();
			entryHeader.Messages.Add(outMessage);
			outMessage.EM_MessageText = "UNH+1+CUSDEC:D:99B:UN'BGM+929:::IMD+B00122382/1:1+9'CST++10::95'LOC+8+AUMEL::6'LOC+9+SGSIN::6'LOC+12+AUMEL::6'LOC+79+AUMEL::6'DTM+7:20041229:102'DTM+252:20041229:102'DTM+260:20041229:102'MEA+AAE+G+KG:123.00'RFF+ABQ'RFF+APH:FOB'PAC+0'PAC++1'TDT+20+123+S(SEA)+++++9044748::11'NAD+AT+31006626946::95'NAD+CB+00657C'MOA+141:150.00:AUD'MOA+40:150.00:AUD'MOA+63:150.00:AUD'MOA+39:150.00:AUD'UNS+D'DMS+1'RFF+BH:HOUSE BILL'RFF+MB:OCEANBILL'CST+1+LINE ACTION CODE::95+N10::95'FTX+AAA+++REFILLABLE, LIQUID INK REFILL'LOC+27+US::5'NAD+SU+2997664Y::95'PCI+1'MOA+40:150.00:AUD'MOA+38:150.00:AUD'MOA+5:0.03:AUD'RFF+ABA'RFF+ABC'RFF+ABD:96081000'RFF+ABL'RFF+AEA:45'RFF+AES'RFF+AFD'UNT+42+1'";
			outMessage.EM_LinkedObject = entryHeader;
			outMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			outMessage.EM_Status = EDIMessage.Status.Sent;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			outMessage.GetLogs().AddNew(Events.AddedARecordToTheSystem, new ZDateTimeOffset(2005, 1, 1));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			EDIMessage incomingMessage = entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			incomingMessage.EM_MessageText = responseMessageWithoutDeclarationCharge;//Declaration and AQIS processing fee are not reported

			processor.ProcessMessage(incomingMessage);
			AssertEquals("DeclarationProcessing Charge should be updated when an original response is processed", 0.0m, entryHeader.DeclarationProcessingCharge);
			AssertEquals("AQIS Processing Charge should be updated when an original response is processed", 0.0m, entryHeader.AQISProcessingCharge);
		}

		protected override Type IncomingMessageType => typeof(CMRIMDRMessage);

		protected override ZString GetExpectedMessageCode() => CMRMessage.CMRMessageTypes.IMD;

		protected override ZString GetExpectedMessageName() => "Import Declaration Response(IMDR)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => new IMDRMessageProcessor(logger);

		CMRIMDRMessage CreateDataForUniversalTesting(ZString messageType, ZString messageText, ZString declarationReference, ZString entryNumber, ZDecimal quantity, out JobComInvoiceLine invoiceLine)
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddYears(-1).ToDateTime());
			var entryHeader = GetNewEntryHeader(messageType, declarationReference, entryNumber, quantity);
			var iMDRMessage = (CMRIMDRMessage)entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			iMDRMessage.EM_MessageText = messageText;
			iMDRMessage.EM_LinkedObject = entryHeader;
			var entryLine = entryHeader.MergedLines[0];
			invoiceLine = entryLine.RandomLine;
			return iMDRMessage;
		}

		CusEntryHeader GetNewEntryHeader(ZString messageType, ZString declarationReference, ZString entryNumber, ZDecimal quantity)
		{
			AssertNotNull(PostMasterGroup);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_DeclarationReference = declarationReference;
			declaration.WarehouseDocAddress.E2_OA_Address = Warehouse.MainAddress.PK;

			var warehouse = Factory.LoadTop1<IWhsWarehouse>(new ZQuery(WhsWarehouseSchema.WW_WarehouseCode, "WHS"));
			if (warehouse == null)
			{
				var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
				warehouse = (IWhsWarehouse)helper.CreateWarehouse("WHS", "BOND");
				warehouse.WW_OA_WarehouseAddress = Warehouse.MainAddress.PK;
				warehouse.WW_IsBondedWarehouse = true;
				warehouse.WW_IsVirtualWarehouse = true;
				((IWhsArea)warehouse.Areas[0]).WA_AreaType = "BON";
				warehouse.WW_GB_RelatedCompanyBranch = base.entryHeader.RegistryBranchPK;
				warehouse.WW_AutoPrintPackingSlip = false;
			}

			declaration.Invoices.DeleteAll();
			var invoice = declaration.Invoices.AddNew();
			invoice.AddInfo.ZA_ORG = Core.Constants.CountryCodes.France;
			invoice.JZ_InvoiceAmount = quantity * 100m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			invoice.Charges.AddNew("OFT", quantity * 10m, Core.Constants.CurrencyCodes.Australia);

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = Part.OP_PartNum;
			invoiceLine.JI_IsPackToBondForLine = true;
			invoiceLine.JI_InvoiceQuantity = quantity;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = quantity * 10m;
			invoiceLine.JI_LinePrice = quantity * 100m;
			invoiceLine.AddInfo.ZA_WRQ = quantity;
			invoiceLine.AddInfo.ZA_WRU = "NO";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.EntryNumber = entryNumber;
			return entryHeader;
		}

		GlbGroup PostMasterGroup
		{
			get
			{
				if (postMasterGroup == null)
				{
					postMasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
					postMasterGroup.Staff[0].GS_EmailAddress = "test@cargowise.com";
					var groupNotification = new AutoBillingGroupNotification();
					groupNotification.SendGroupPK = postMasterGroup.PK;
					CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);
				}
				return postMasterGroup;
			}
		}
		GlbGroup postMasterGroup;

		OrgHeader Importer
		{
			get
			{
				if (importer == null)
				{
					importer = Factory.New<OrgHeader>();
					importer.OH_Code = "IMP";
					importer.MiscServ.OM_IMPartAttrib1Name = "VIN1";
					importer.MiscServ.OM_IMPartAttrib1Type = "NON";
					importer.CompanyData.OB_IMUsedBondedWhs = true;
					importer.OH_IsWarehouseClient = true;
				}
				return importer;
			}
		}
		OrgHeader importer;

		OrgHeader Warehouse
		{
			get
			{
				if (warehouse == null)
				{
					warehouse = Factory.New<OrgHeader>();
					warehouse.OH_Code = "W1";
					warehouse.MainAddress.LocalControlledPremisesID = "23423";
				}
				return warehouse;
			}
		}
		OrgHeader warehouse;

		AUOrgSupplierPart Part
		{
			get
			{
				if (part == null)
				{
					part = Factory.New<AUOrgSupplierPart>();
					part.OP_PartNum = "~~1";
					part.OP_StockKeepingUnit = "NO";
					part.RelatedOrganisations.AddOrganisationIfNotExist(Importer.PK, OrgPartRelation.RelationshipTypes.Owner);
					part.AddNewImportPivotWithClassification(Classification.PK);
				}
				return part;
			}
		}
		AUOrgSupplierPart part;

		Classification Classification
		{
			get
			{
				if (classification == null)
				{
					classification = Factory.New<Classification>();
					classification.CC_ClassificationType = Classification.ClassificationType.IMP;
					classification.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					classification.CC_LookupCode = "~~1L";
					classification.CC_TariffNum = StatTariff.SC_TariffClassificationNumber + StatTariff.SC_StatisticalClassificationCode;
				}
				return classification;
			}
		}
		Classification classification;

		CMRStatisticalClassificationPeriodSnapshot StatTariff
		{
			get
			{
				if (statTariff == null)
				{
					statTariff = Factory.New<CMRStatisticalClassificationPeriodSnapshot>();
					statTariff.SC_TariffClassificationNumber = "00000000";
					statTariff.SC_StatisticalClassificationCode = "00";
					statTariff.SC_QuantityUnit = "KG";
					statTariff.SC_StartDate = new ZDateTime(2005, 1, 1);
				}
				return statTariff;
			}
		}
		CMRStatisticalClassificationPeriodSnapshot statTariff;

		sealed class TestHelperIMDRMessageProcessor : IMDRMessageProcessor
		{
			public TestHelperIMDRMessageProcessor(LoggingInformation logger) : base(logger) { }

			public EmailDef GetReportfForTest(CMRCUSRESMessage message)
			{
				incomingMessage = message;
				return GetReport();
			}

			protected override void SendReport(EmailDef email)
			{
				base.SendReport(email);
				SentReportEmails.Add(email);
			}
			public ArrayList SentReportEmails = new ArrayList();
		}
	}
}
