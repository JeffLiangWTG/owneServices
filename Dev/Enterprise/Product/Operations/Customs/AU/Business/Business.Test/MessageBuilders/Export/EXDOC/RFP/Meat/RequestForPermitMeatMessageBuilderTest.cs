using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class RequestForPermitMeatMessageBuilderTest : RequestForPermitMessageBuilderAbstractTest
	{
		[TestDate(2016, 12, 25)]
		public override void TestRFPMessageBuilder()
		{
			expectedRFPMessage = MeatLodgeMessage;
			base.TestRFPMessageBuilder();
		}

		[TestDate(2017, 04, 01)]
		public void TestRFPMessageBuilderErrata44()
		{
			expectedRFPMessage = MeatLodgeMessage_E44;
			base.TestRFPMessageBuilder();
		}

		protected override ZString ExpectedRFPMessage => expectedRFPMessage;

		protected override RequestForPermitHeaderMessageBuilder GetMessageBuilder => new RequestForPermitMeatHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.LDG);

		protected override RequestForPermitHeaderMessageBuilder GetTransferMessageBuilder => new RequestForPermitMeatHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.TRF);

		protected override RequestForPermitHeaderMessageBuilder GetWithdrawlMessageBuilder => new RequestForPermitMeatHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.CAN);

		protected override RequestForPermitHeaderMessageBuilder GetCopyMessageBuilder => new RequestForPermitMeatHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.CPY);

		protected override RequestForPermitHeaderMessageBuilder GetAcceptTransferInMessageBuilder => new RequestForPermitMeatHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.AcceptTransferIn);

		protected override RequestForPermitHeaderMessageBuilder GetDeclinetTransferInMessageBuilder => new RequestForPermitMeatHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.DeclineTransferIn);

		protected override ZString ExpectedWithdrawlRFPMessage => MeatWithdrawlMessage;

		protected override ZString ExpectedTransferRFPMessage => MeatTransferMessage;

		protected override ZString ExpectedCopyRFPMessage => MeatCopyMessage;

		protected override ZString ExpectedAcceptedTransferInRFPMessage => MeatAcceptedTransferInMessage;

		protected override ZString ExpectedDeclinedTransferInRFPMessage => MeatDeclinedTransferInMessage;

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			invoiceHeader = helper.Header1;
			helper.Declaration.JE_RL_NKPortOfLoading = "AUBNE";
			helper.Declaration.JE_RL_NKPortOfArrival = "TWTPE";
			var quarantineHeader = invoiceHeader.QuarantineExDocHeader;
			quarantineHeader.QH_CertificateRequiredLocation = "SYD";
			helper.Declaration.JE_DeclarationReference = "B00000123";
			helper.Declaration.DeclarationNumber = "AAEETF6AL";
			var notifyTextNote = helper.Declaration.Notes.AddNew();
			notifyTextNote.ST_Description = PredefinedNoteTypes.Instance.EXDOCNotifyText.Description;
			notifyTextNote.ST_NoteText = "NEDDY SEAGOON";
			var letterOfCreditInfoNote = helper.Declaration.Notes.AddNew();
			letterOfCreditInfoNote.ST_Description = PredefinedNoteTypes.Instance.EXDOCLetterOfCredit.Description;
			letterOfCreditInfoNote.ST_NoteText = "ATTN MR BANKER, BANK OF NEW ZEALAND";
			quarantineHeader.QH_AbsoluteTemperature = -2.75m;
			quarantineHeader.QH_TemperatureUM = EXDOCTemperatureUnitCodes.Codes.Celsius;
			quarantineHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.Automatic;
			quarantineHeader.QH_SplitHealthCertByContainer = ZBool.True;
			quarantineHeader.QH_ObtainExportCustomsPermit = ZBool.True;
			quarantineHeader.QH_DecOfCompliance = EXDOCYesNoEmpty.Codes.Yes;
			quarantineHeader.QH_TrueAndCompleteIndicator = EXDOCYesNoEmpty.Codes.Yes;
			quarantineHeader.QH_ApprovedCertifier = "H9999";
			quarantineHeader.QH_AvAnimalAge = "less than 1 year";
			quarantineHeader.QH_ConsigneeAgentName = "Consignee Agent Test";
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "TSTSUP";
			var au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia);
			supplier.SetCustomsCode(OrgCusCode.AUQuarantineCodeTypes.EXDOCExporterNumber, au, "1000");
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "TSTIMP";
			importer.OH_FullName = "KWING KWONG";
			importer.MainAddress.OA_Address1 = "456 HIGH ST";
			importer.MainAddress.OA_City = "TAIPEI";
			importer.MainAddress.OA_PostCode = "654321";
			importer.OH_RL_NKClosestPort = "TWTPE";
			importer.MainAddress.OA_State = "TWSTATE";
			helper.Declaration.JE_OH_Importer = importer.PK;
			helper.Declaration.JE_RL_NKFinalDestination = "TWTPE";
			helper.Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			helper.Declaration.JE_VoyageFlightNo = "V123";
			helper.Declaration.JE_VesselName = "ADMIRALENGRACHT";
			helper.Declaration.JE_ExportDate = new ZDate(2004, 01, 30);
			helper.Declaration.JE_MarksAndNumbers = "NM/A1234/ENDV";
			quarantineHeader.QH_AuthorisationEstablishment = "10004";
			quarantineHeader.QH_AuthorisingOfficerID = "GRAHB";
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "AUD";
			invoiceHeader.JZ_InvoiceAmount = 425;
			quarantineHeader.QH_TransfereeExporterNumber = "987J8";
			quarantineHeader.QH_TransfereeEDIUserIdentifier = "FJ9834";
			quarantineHeader.QH_AMLCQuota = true;
			quarantineHeader.QH_AMLCQuotaYear = "2017-18";
			var invoiceLine = helper.Line1;
			invoiceLine.JI_Tariff = "99999998";
			invoiceLine.JI_Description = "BEEF";
			invoiceLine.JI_LinePrice = 425;
			invoiceLine.AddInfo.ZA_TILV = "1AUD";
			invoiceLine.JI_Drawback = false;
			var quarantineLine = invoiceLine.QuarantineExDocLine;
			quarantineLine.QL_NetQuantity = 1456.00m;
			quarantineLine.QL_NetQuantityUnit = EXDOCMetricWeightUnitCodes.Codes.Kilogram;
			quarantineLine.QL_ProductType = "CA";
			quarantineLine.QL_PackType = EXDOCPacakgeTypeCodes.Codes.BulkPack;
			quarantineLine.QL_SupplimentaryCode = ZString.Empty;
			quarantineLine.QL_CutCode = "1000";
			quarantineLine.QL_LabelApprovalIndicator = true;
			quarantineLine.QL_UngradedProductIndicator = true;
			quarantineLine.QL_LabelApprovalNumber = "12345678";
			quarantineLine.QL_NatureOfCommodity = "CT";
			quarantineLine.QL_TreatmentType = "BO";
			quarantineLine.QL_SendHCDesc = true;
			quarantineLine.QL_AqisCustomsWeight = 2000;
			quarantineLine.QL_OuterPackCount = 50;
			quarantineLine.QL_OuterPackType = EXDOCPacakgeTypeCodes.Codes.Cartons;
			quarantineLine.QL_BatchCode = "BC123456";
			var container = Factory.New<CusContainer>();
			container.CO_ContainerNumber = "MARU2103333";
			container.CO_Seal = "654321";
			helper.Declaration.CusContainers.Add(container);
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
			var slaughterProcess = quarantineLine.Processes.AddNew();
			slaughterProcess.EE_AuthorisationEstablishmentID = "780";
			slaughterProcess.EE_StartDate = new ZDateTime(2004, 1, 1);
			slaughterProcess.EE_EndDate = new ZDateTime(2004, 1, 5);
			slaughterProcess.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Slaughter;
			var packingProcess = quarantineLine.Processes.AddNew();
			packingProcess.EE_AuthorisationEstablishmentID = "1004";
			packingProcess.EE_StartDate = new ZDateTime(2004, 1, 8);
			packingProcess.EE_EndDate = new ZDateTime(2004, 1, 8);
			packingProcess.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Packing;
		}
		JobComInvoiceHeader invoiceHeader;
		ZString expectedRFPMessage;

		const string MeatLodgeMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+M::AQ:9++13'LOC+9+BNE'LOC+12+TWTPE'LOC+8+TAIPEI'LOC+36+TW'LOC+30+AU'LOC+91+SYD'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'RFF+AAE:AAEETF6AL'FTX+AAG+++NEDDY SEAGOON'FTX+ACF+++LESS THAN 1 YEAR'MEA+TE+ADE+CEL:-2.75'MOA+63::AUD'GIS+A::AQ:PHC'GIS+Y::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:SST'GIS+Y::AQ:QI'GIS+Y::AQ:ACS'GIS+N::AQ:DCT'GIS+Y::AQ:DOC'GIS+Y::AQ:TAC'PNA+EX+1000'PNA+CN+++++10:KWING KWONG'ADR++5:456 HIGH ST+TAIPEI+654321+TW+:::TWSTATE'PNA+CX+++++10:CONSIGNEE AGENT TEST'TDT+12+V123+1+++++:::ADMIRALENGRACHT'DTM+136:20040130:102'PRC+IN:PP:AQ'PNA+FO+10004'PNA+AV+GRAHB'PNA+PQ+H9999'LIN+1'MEA+AAA+SQ+KGM:1456.000'PIA+5+XCA BP  :CC'PIA+5+1000:BP'PIA+5+99999998:HS'IMD+++UHC:::BEEF'IMD+++NC:::CT'IMD+++TT:::BO'GIN+BX+BC123456'RFF+AFF:12345678'ATT+10++Y:LAI:AQ'ATT+10++Y:UPI:AQ'ATT+10++N:HPI:AQ'ATT+10++N:FCI:AQ'MOA+63:425.00'PAC+50+3+CT::AQ'PCI++NM/A1234/ENDV'EQD+CN+MARU2103333'SEL+654321'PRC+SL:PP:AQ'DTM+194:20040101:102'DTM+206:20040105:102'PNA+MP+780'PRC+PK:PP:AQ'DTM+194:20040108:102'DTM+206:20040108:102'PNA+MP+1004'UNT+62+<<MSGNO PLACEHOLDER>>'";
		const string MeatLodgeMessage_E44 = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+M::AQ:9++13'LOC+9+BNE'LOC+12+TWTPE'LOC+8+TAIPEI'LOC+36+TW'LOC+30+AU'LOC+91+SYD'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'RFF+AAE:AAEETF6AL'FTX+AAG+++NEDDY SEAGOON'FTX+ACF+++LESS THAN 1 YEAR'FTX+ABW+++2017-18'MEA+TE+ADE+CEL:-2.75'MOA+63::AUD'GIS+A::AQ:PHC'GIS+Y::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:SST'GIS+Y::AQ:QI'GIS+Y::AQ:ACS'GIS+N::AQ:DCT'GIS+Y::AQ:DOC'GIS+Y::AQ:TAC'PNA+EX+1000'PNA+CN+++++10:KWING KWONG'ADR++5:456 HIGH ST+TAIPEI+654321+TW+:::TWSTATE'PNA+CX+++++10:CONSIGNEE AGENT TEST'TDT+12+V123+1+++++:::ADMIRALENGRACHT'DTM+136:20040130:102'PRC+IN:PP:AQ'PNA+FO+10004'PNA+AV+GRAHB'PNA+PQ+H9999'LIN+1'MEA+AAA+SQ+KGM:1456.000'PIA+5+XCA BP  :CC'PIA+5+1000:BP'PIA+5+99999998:HS'IMD+++UHC:::BEEF'IMD+++NC:::CT'IMD+++TT:::BO'GIN+BX+BC123456'RFF+AFF:12345678'ATT+10++Y:LAI:AQ'ATT+10++Y:UPI:AQ'ATT+10++N:HPI:AQ'ATT+10++N:FCI:AQ'MOA+63:425.00'PAC+50+3+CT::AQ'PCI++NM/A1234/ENDV'EQD+CN+MARU2103333'SEL+654321'PRC+SL:PP:AQ'DTM+194:20040101:102'DTM+206:20040105:102'PNA+MP+780'PRC+PK:PP:AQ'DTM+194:20040108:102'DTM+206:20040108:102'PNA+MP+1004'UNT+63+<<MSGNO PLACEHOLDER>>'";
		const string MeatWithdrawlMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+M::AQ:9++1'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'PNA+EX+1000'UNT+5+<<MSGNO PLACEHOLDER>>'";
		const string MeatTransferMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+M::AQ:9++90'GIS+N::AQ:CT'PNA+EX+1000'PNA+TT+987J8'CTA+AG+FJ9834'UNT+7+<<MSGNO PLACEHOLDER>>'";
		const string MeatCopyMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+M::AQ:9++31'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'PNA+EX+1000'UNT+5+<<MSGNO PLACEHOLDER>>'";
		const string MeatAcceptedTransferInMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+M::AQ:9++92+AP'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'PNA+EX+1000'UNT+5+<<MSGNO PLACEHOLDER>>'";
		const string MeatDeclinedTransferInMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+M::AQ:9++92+RE'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'PNA+EX+1000'UNT+5+<<MSGNO PLACEHOLDER>>'";
	}
}
