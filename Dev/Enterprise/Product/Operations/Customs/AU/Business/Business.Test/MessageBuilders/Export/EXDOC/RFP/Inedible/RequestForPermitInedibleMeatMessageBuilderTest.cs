using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class RequestForPermitInedibleMeatMessageBuilderTest : RequestForPermitMessageBuilderAbstractTest
	{
		protected override ZString ExpectedRFPMessage => InedibleMeatLodgeMessage;

		protected override ZString ExpectedTransferRFPMessage => InedibleMeatTransferMessage;

		protected override ZString ExpectedWithdrawlRFPMessage => InedibleMeatWithdrawlMessage;

		protected override ZString ExpectedCopyRFPMessage => InedibleMeatCopyMessage;

		protected override ZString ExpectedAcceptedTransferInRFPMessage => InedibleMeatAcceptedTransferInMessage;

		protected override ZString ExpectedDeclinedTransferInRFPMessage => InedibleMeatDeclinedTransferInMessage;

		protected override RequestForPermitHeaderMessageBuilder GetCopyMessageBuilder => new RequestForPermitInedibleMeatHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.CPY);

		protected override RequestForPermitHeaderMessageBuilder GetMessageBuilder => new RequestForPermitInedibleMeatHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.LDG);

		protected override RequestForPermitHeaderMessageBuilder GetTransferMessageBuilder => new RequestForPermitInedibleMeatHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.TRF);

		protected override RequestForPermitHeaderMessageBuilder GetWithdrawlMessageBuilder => new RequestForPermitInedibleMeatHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.CAN);

		protected override RequestForPermitHeaderMessageBuilder GetAcceptTransferInMessageBuilder => new RequestForPermitInedibleMeatHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.AcceptTransferIn);

		protected override RequestForPermitHeaderMessageBuilder GetDeclinetTransferInMessageBuilder => new RequestForPermitInedibleMeatHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.DeclineTransferIn);

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			invoiceHeader = helper.Header1;
			helper.Declaration.JE_RL_NKPortOfLoading = "AUMEL";
			helper.Declaration.JE_RL_NKPortOfArrival = "USLGB";
			helper.Declaration.JE_RL_NKFinalDestination = "USLGB";
			helper.Declaration.JE_DeclarationReference = "B00001003";
			helper.Declaration.DeclarationNumber = "AAEETF6AL";
			var notifyTextNote = helper.Declaration.Notes.AddNew();
			notifyTextNote.ST_Description = PredefinedNoteTypes.Instance.EXDOCNotifyText.Description;
			notifyTextNote.ST_NoteText = "NEDDY SEAGOON";
			var letterOfCreditInfoNote = helper.Declaration.Notes.AddNew();
			letterOfCreditInfoNote.ST_Description = PredefinedNoteTypes.Instance.EXDOCLetterOfCredit.Description;
			letterOfCreditInfoNote.ST_NoteText = "ATTN MR BANKER, BANK OF NEW ZEALAND";
			var additionalTextNote = helper.Declaration.Notes.AddNew();
			additionalTextNote.ST_Description = PredefinedNoteTypes.Instance.EXDOCAdditionalInformation.Description;
			additionalTextNote.ST_NoteText = "ADDITIONAL INFO";
			var quarantineHeader = invoiceHeader.QuarantineExDocHeader;
			quarantineHeader.QH_ObtainExportCustomsPermit = ZBool.True;
			quarantineHeader.QH_CertificateRequiredLocation = "MEL";
			quarantineHeader.QH_AQISRegion = "MEL";
			quarantineHeader.QH_ExporterDeclaration = "THIS PRODUCT WAS PRODUCED AND PROCESSED IN AUSTRALIA.";
			quarantineHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.Manual;
			quarantineHeader.QH_ProductUseIndicator = "A";
			quarantineHeader.QH_AbsoluteTemperature = 12;
			quarantineHeader.QH_TemperatureUM = EXDOCTemperatureUnitCodes.Codes.Celsius;
			quarantineHeader.QH_ConsigneeAgentName = "Consignee Agent Test";
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "TSTSUP";
			var au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia);
			supplier.SetCustomsCode(OrgCusCode.AUQuarantineCodeTypes.EXDOCExporterNumber, au, "99999");
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "TSTIMP";
			importer.OH_FullName = "WALLACE THE IMPORTER";
			importer.MainAddress.OA_Address1 = "62 WEST WALLABY ST";
			importer.MainAddress.OA_City = "WASHINGTON";
			importer.MainAddress.OA_PostCode = "654321";
			importer.OH_RL_NKClosestPort = "USLGB";
			importer.MainAddress.OA_State = "DC";
			helper.Declaration.JE_OH_Importer = importer.PK;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			helper.Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			helper.Declaration.JE_VoyageFlightNo = "191";
			helper.Declaration.JE_VesselName = "ADMIRALENGRACHT";
			helper.Declaration.JE_ExportDate = new ZDate(2004, 02, 23);
			quarantineHeader.QH_AuthorisationEstablishment = "604";
			quarantineHeader.QH_AuthorisingOfficerID = "QADINS";
			invoiceHeader.JZ_InvoiceNumber = "9999ABC";
			invoiceHeader.JZ_InvoiceDate = new ZDateTime(2004, 12, 12);
			quarantineHeader.QH_TransfereeExporterNumber = "55555";
			quarantineHeader.QH_TransfereeEDIUserIdentifier = "4444";
			quarantineHeader.QH_DecOfCompliance = EXDOCYesNoEmpty.Codes.Yes;
			quarantineHeader.QH_ImportedProductFlag = EXDOCYesNoEmpty.Codes.No;
			var invoiceLine = helper.Line1;
			invoiceLine.JI_Tariff = "04011000";
			invoiceLine.JI_Description = "MILK, OF A FAT CONTENT, BY WEIGHT, NOT EXC 1%, NOT CONCENTRATED NOR SWEETENED";
			invoiceLine.JI_Drawback = false;
			invoiceLine.JI_LinePrice = 10855;
			var quarantineLine = invoiceLine.QuarantineExDocLine;
			quarantineLine.QL_NetQuantity = 2080;
			quarantineLine.QL_NetQuantityUnit = EXDOCMetricWeightUnitCodes.Codes.Kilogram;
			quarantineLine.QL_GrossMetricWeight = 2116;
			quarantineLine.QL_GrossMetricWeightUnit = EXDOCMetricWeightUnitCodes.Codes.Kilogram;
			quarantineLine.QL_PreservationType = EXDOCPreservationTypeCodes.Codes.Frozen;
			quarantineLine.QL_ProductType = "FPA";
			quarantineLine.QL_PackType = EXDOCPackTypeCodes.Codes.Cartons;
			quarantineLine.QL_CutCode = "IN4036";
			quarantineLine.QL_AqisCustomsWeight = 2000;
			quarantineLine.QL_UseByStart = new ZDateTime(2004, 02, 13);
			quarantineLine.QL_UseByEnd = new ZDateTime(2005, 02, 12);
			quarantineLine.QL_OuterPackCount = 104;
			quarantineLine.QL_OuterPackType = EXDOCPacakgeTypeCodes.Codes.Bags;
			quarantineLine.QL_OuterPackWeight = 20;
			quarantineLine.QL_OuterPackWeightUnit = EXDOCMetricWeightUnitCodes.Codes.Kilogram;
			quarantineLine.QL_SendHCDesc = true;
			quarantineLine.QL_BatchCode = "BC123456";
			var container = Factory.New<CusContainer>();
			container.CO_ContainerNumber = "FESU1234567";
			container.CO_Seal = "000001";
			helper.Declaration.CusContainers.Add(container);
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
			var containerPivot = Factory.LoadTop1<CusContainerInvoiceLinePivot>(new ZQuery(CusContainerInvoiceLinePivotSchema.C2_JI, invoiceLine.PK));
			containerPivot.C2_GrossWeight = 23.456;
			containerPivot.C2_NetWeight = 34.678;
			var processingProcess = quarantineLine.Processes.AddNew();
			processingProcess.EE_AuthorisationEstablishmentID = "604";
			processingProcess.EE_StartDate = new ZDateTime(2004, 1, 24);
			processingProcess.EE_EndDate = new ZDateTime(2004, 1, 24);
			processingProcess.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Processing;
		}
		JobComInvoiceHeader invoiceHeader;

		const string InedibleMeatLodgeMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+I::AQ:9++13'LOC+9+MEL'LOC+12+USLGB'LOC+8+LONG BEACH'LOC+36+US'LOC+30+AU'LOC+91+MEL'LOC+48+MEL'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'RFF+AAE:AAEETF6AL'FTX+DCL+++THIS PRODUCT WAS PRODUCED AND PROCESSED IN AUSTRALIA.'FTX+AAG+++NEDDY SEAGOON'FTX+ACB+++ADDITIONAL INFO'MEA+TE+ADE+CEL:12.00'MOA+63::US'GIS+M::AQ:PHC'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+Y::AQ:ACS'GIS+N::AQ:DCT'GIS+A::AQ:PUI'PNA+EX+99999'PNA+CN+++++10:WALLACE THE IMPORTER'ADR++5:62 WEST WALLABY ST+WASHINGTON+654321+US+:::DC'CTA+AG+:CONSIGNEE AGENT TEST'TDT+12+191+1+++++:::ADMIRALENGRACHT'DTM+136:20040223:102'PRC+IN:PP:AQ'PNA+FO+604'PNA+AV+QADINS'LIN+1'MEA+AAA+SQ+KGM:2080.000'MEA+AAI+AAE+KGM:2116'PIA+5+FFPACT  :CC'PIA+5+IN4036:BP'PIA+5+04011000:HS'IMD+++UHC:::MILK, OF A FAT CONTENT, BY WEIGHT, :NOT EXC 1%, NOT CONCENTRATED NOR SW:EETENED'GIN+BX+BC123456'ATT+10++N:FCI:AQ'DTM+194:20040213:102'DTM+206:20050212:102'MOA+63:10855.00'PAC+104+3+BG::AQ'MEA+AAU+AAL:4+KGM:20.000'EQD+CN+FESU1234567'SEL+000001'PRC+PC:PP:AQ'DTM+194:20040124:102'DTM+206:20040124:102'PNA+MP+604'UNT+53+<<MSGNO PLACEHOLDER>>'";
		const string InedibleMeatTransferMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+I::AQ:9++90'GIS+N::AQ:CT'PNA+EX+99999'PNA+TT+55555'CTA+AG+4444'UNT+7+<<MSGNO PLACEHOLDER>>'";
		const string InedibleMeatWithdrawlMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+I::AQ:9++1'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'PNA+EX+99999'UNT+5+<<MSGNO PLACEHOLDER>>'";
		const string InedibleMeatCopyMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+I::AQ:9++31'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'PNA+EX+99999'UNT+5+<<MSGNO PLACEHOLDER>>'";
		const string InedibleMeatAcceptedTransferInMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+I::AQ:9++92+AP'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'PNA+EX+99999'UNT+5+<<MSGNO PLACEHOLDER>>'";
		const string InedibleMeatDeclinedTransferInMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+I::AQ:9++92+RE'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'PNA+EX+99999'UNT+5+<<MSGNO PLACEHOLDER>>'";
	}
}
