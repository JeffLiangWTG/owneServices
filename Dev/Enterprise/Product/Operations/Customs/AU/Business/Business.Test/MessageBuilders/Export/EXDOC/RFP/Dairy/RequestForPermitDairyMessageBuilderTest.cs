using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class RequestForPermitDairyMessageBuilderTest : RequestForPermitMessageBuilderAbstractTest
	{
		[TestDate(2016, 12, 25)]
		public override void TestRFPMessageBuilder()
		{
			expectedRFPMessage = DairyLodgeMessage;
			base.TestRFPMessageBuilder();
		}

		[TestDate(2017, 04, 01)]
		public void TestRFPMessageBuilderErrata44()
		{
			expectedRFPMessage = DairyLodgeMessage_E44;
			base.TestRFPMessageBuilder();
		}

		protected override ZString ExpectedRFPMessage => expectedRFPMessage;

		protected override ZString ExpectedTransferRFPMessage => DairyTransferMessage;

		protected override ZString ExpectedWithdrawlRFPMessage => DairyWithdrawlMessage;

		protected override ZString ExpectedCopyRFPMessage => DairyCopyMessage;

		protected override ZString ExpectedAcceptedTransferInRFPMessage => DairyAcceptedTransferInMessage;

		protected override ZString ExpectedDeclinedTransferInRFPMessage => DairyDeclinedTransferInMessage;

		protected override RequestForPermitHeaderMessageBuilder GetCopyMessageBuilder => new RequestForPermitDairyHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.CPY);

		protected override RequestForPermitHeaderMessageBuilder GetMessageBuilder => new RequestForPermitDairyHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.LDG);

		protected override RequestForPermitHeaderMessageBuilder GetTransferMessageBuilder => new RequestForPermitDairyHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.TRF);

		protected override RequestForPermitHeaderMessageBuilder GetWithdrawlMessageBuilder => new RequestForPermitDairyHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.CAN);

		protected override RequestForPermitHeaderMessageBuilder GetAcceptTransferInMessageBuilder => new RequestForPermitDairyHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.AcceptTransferIn);

		protected override RequestForPermitHeaderMessageBuilder GetDeclinetTransferInMessageBuilder => new RequestForPermitDairyHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.DeclineTransferIn);

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
			invoiceHeader.QuarantineExDocHeader.QH_ObtainExportCustomsPermit = ZBool.True;
			invoiceHeader.QuarantineExDocHeader.QH_CertificateRequiredLocation = "MEL";
			invoiceHeader.QuarantineExDocHeader.QH_AQISRegion = "MEL";
			invoiceHeader.QuarantineExDocHeader.QH_ExporterDeclaration = "THIS PRODUCT WAS PRODUCED AND PROCESSED IN AUSTRALIA.";
			invoiceHeader.QuarantineExDocHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.Manual;
			invoiceHeader.QuarantineExDocHeader.QH_StorageEstablishment = "88";
			invoiceHeader.QuarantineExDocHeader.QH_AMLCQuota = true;
			invoiceHeader.QuarantineExDocHeader.QH_AMLCQuotaYear = "2017-18";
			invoiceHeader.QuarantineExDocHeader.QH_ConsigneeAgentName = "Consignee Agent Test";
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "TSTSUP";
			var aU = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			supplier.SetCustomsCode(OrgCusCode.AUQuarantineCodeTypes.EXDOCExporterNumber, aU, "99999");
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
			invoiceHeader.QuarantineExDocHeader.QH_AuthorisationEstablishment = "604";
			invoiceHeader.QuarantineExDocHeader.QH_AuthorisingOfficerID = "QADINS";
			invoiceHeader.JZ_InvoiceNumber = "9999ABC";
			invoiceHeader.JZ_InvoiceDate = new ZDateTime(2004, 12, 12);
			invoiceHeader.QuarantineExDocHeader.QH_TransfereeExporterNumber = "55555";
			invoiceHeader.QuarantineExDocHeader.QH_TransfereeEDIUserIdentifier = "4444";
			invoiceHeader.QuarantineExDocHeader.QH_DecOfCompliance = EXDOCYesNoEmpty.Codes.Yes;
			invoiceHeader.QuarantineExDocHeader.QH_ImportedProductFlag = EXDOCYesNoEmpty.Codes.No;
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
			quarantineLine.QL_PercentOfMilkProtein = 5;
			quarantineLine.QL_PercentOfMilkFat = 2;
			quarantineLine.QL_TotalWeightOfMilkProteinInMixtures = 4000;
			quarantineLine.QL_TotalWeightOfMilkFatInMixtures = 1600;
			quarantineLine.QL_PreservationType = EXDOCPreservationTypeCodes.Codes.Frozen;
			quarantineLine.QL_ProductType = "MIL";
			quarantineLine.QL_PackType = EXDOCPackTypeCodes.Codes.Cartons;
			quarantineLine.QL_CutCode = "DC0001";
			quarantineLine.QL_SendHCDesc = true;
			quarantineLine.QL_AqisCustomsWeight = 2000;
			quarantineLine.QL_UseByStart = new ZDateTime(2004, 02, 13);
			quarantineLine.QL_UseByEnd = new ZDateTime(2005, 02, 12);
			quarantineLine.QL_OuterPackCount = 104;
			quarantineLine.QL_OuterPackType = EXDOCPacakgeTypeCodes.Codes.Bags;
			quarantineLine.QL_OuterPackWeight = 20;
			quarantineLine.QL_OuterPackWeightUnit = EXDOCMetricWeightUnitCodes.Codes.Kilogram;
			quarantineLine.QL_IMA1SerialNumber = "ZUBIN HAS A SMALL";
			quarantineLine.QL_IMA1QuotaYear = "SMALL FOR GEOFF";
			quarantineLine.QL_SendHCDesc = true;
			quarantineLine.QL_BatchCode = "BC123456";
			var container = Factory.New<CusContainer>();
			container.CO_ContainerNumber = "FESU1234567";
			container.SealStartNumber = "000111";
			container.SealEndNumber = "000222";
			helper.Declaration.CusContainers.Add(container);
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
			var query = new ZQuery(CusContainerInvoiceLinePivotSchema.C2_JI, invoiceLine.PK);
			var containerPivot = Factory.LoadTop1<CusContainerInvoiceLinePivot>(query);
			containerPivot.C2_GrossWeight = 23.456;
			containerPivot.C2_NetWeight = 34.678;
			var processingProcess = quarantineLine.Processes.AddNew();
			processingProcess.EE_AuthorisationEstablishmentID = "604";
			processingProcess.EE_StartDate = new ZDateTime(2004, 1, 24);
			processingProcess.EE_EndDate = new ZDateTime(2004, 1, 24);
			processingProcess.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Processing;
		}
		JobComInvoiceHeader invoiceHeader;
		string expectedRFPMessage;

		const string DairyLodgeMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+D::AQ:9++13'LOC+9+MEL'LOC+12+USLGB'LOC+8+LONG BEACH'LOC+36+US'LOC+30+AU'LOC+91+MEL'LOC+48+MEL'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'RFF+AAE:AAEETF6AL'FTX+DCL+++THIS PRODUCT WAS PRODUCED AND PROCESSED IN AUSTRALIA.'FTX+AAG+++NEDDY SEAGOON'MOA+63::US'GIS+M::AQ:PHC'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+Y::AQ:QI'GIS+Y::AQ:ACS'GIS+N::AQ:DCT'GIS+Y::AQ:DOC'GIS+::AQ:TAC'GIS+N::AQ:IPF'PNA+EX+99999'PNA+CN+++++10:WALLACE THE IMPORTER'ADR++5:62 WEST WALLABY ST+WASHINGTON+654321+US+:::DC'CTA+AG+:CONSIGNEE AGENT TEST'TDT+12+191+1+++++:::ADMIRALENGRACHT'DTM+136:20040223:102'PRC+IN:PP:AQ'PNA+FO+604'PNA+AV+QADINS'PRC+ST:PP:AQ'PNA+MP+88'LIN+1'MEA+AAA+SQ+KGM:2080.000'MEA+AAI+AAE+KGM:2116'MEA+MP++P1:5.00'MEA+MF++P1:2.00'MEA+MP+AAL+KGM:4000.00'MEA+MF+AAL+KGM:1600.00'MEA+AAF+SQ+KGM:2000'PIA+5+FMILCT  :CC'PIA+5+DC0001:BP'PIA+5+04011000:HS'IMD+++UHC:::MILK, OF A FAT CONTENT, BY WEIGHT, :NOT EXC 1%, NOT CONCENTRATED NOR SW:EETENED'GIN+BX+BC123456'ATT+10++N:FCI:AQ'DTM+194:20040213:102'DTM+206:20050212:102'MOA+63:10855.00'PAC+104+3+BG::AQ'MEA+AAU+AAL:4+KGM:20.000'EQD+CN+FESU1234567'MEA+WT+AAC+:34.678'MEA+WT+AAE+:23.456'GIN+BN+ZUBIN HAS A SMALL'GIN+IL+9999ABC'DTM+3:20041212:102'FTX+AAI+++SMALL FOR GEOFF'SEL++000111+000222'PRC+PC:PP:AQ'DTM+194:20040124:102'DTM+206:20040124:102'PNA+MP+604'UNT+67+<<MSGNO PLACEHOLDER>>'";
		const string DairyLodgeMessage_E44 = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+D::AQ:9++13'LOC+9+MEL'LOC+12+USLGB'LOC+8+LONG BEACH'LOC+36+US'LOC+30+AU'LOC+91+MEL'LOC+48+MEL'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'RFF+AAE:AAEETF6AL'FTX+DCL+++THIS PRODUCT WAS PRODUCED AND PROCESSED IN AUSTRALIA.'FTX+AAG+++NEDDY SEAGOON'FTX+ABW+++2017-18'MOA+63::US'GIS+M::AQ:PHC'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+Y::AQ:QI'GIS+Y::AQ:ACS'GIS+N::AQ:DCT'GIS+Y::AQ:DOC'GIS+::AQ:TAC'GIS+N::AQ:IPF'PNA+EX+99999'PNA+CN+++++10:WALLACE THE IMPORTER'ADR++5:62 WEST WALLABY ST+WASHINGTON+654321+US+:::DC'CTA+AG+:CONSIGNEE AGENT TEST'TDT+12+191+1+++++:::ADMIRALENGRACHT'DTM+136:20040223:102'PRC+IN:PP:AQ'PNA+FO+604'PNA+AV+QADINS'PRC+ST:PP:AQ'PNA+MP+88'LIN+1'MEA+AAA+SQ+KGM:2080.000'MEA+AAI+AAE+KGM:2116'MEA+MP++P1:5.00'MEA+MF++P1:2.00'MEA+MP+AAL+KGM:4000.00'MEA+MF+AAL+KGM:1600.00'MEA+AAF+SQ+KGM:2000'PIA+5+FMILCT  :CC'PIA+5+DC0001:BP'PIA+5+04011000:HS'IMD+++UHC:::MILK, OF A FAT CONTENT, BY WEIGHT, :NOT EXC 1%, NOT CONCENTRATED NOR SW:EETENED'GIN+BX+BC123456'ATT+10++N:FCI:AQ'DTM+194:20040213:102'DTM+206:20050212:102'MOA+63:10855.00'PAC+104+3+BG::AQ'MEA+AAU+AAL:4+KGM:20.000'EQD+CN+FESU1234567'MEA+WT+AAC+:34.678'MEA+WT+AAE+:23.456'GIN+BN+ZUBIN HAS A SMALL'GIN+IL+9999ABC'DTM+3:20041212:102'FTX+AAI+++SMALL FOR GEOFF'SEL++000111+000222'PRC+PC:PP:AQ'DTM+194:20040124:102'DTM+206:20040124:102'PNA+MP+604'UNT+68+<<MSGNO PLACEHOLDER>>'";
		const string DairyTransferMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+D::AQ:9++90'GIS+N::AQ:CT'PNA+EX+99999'PNA+TT+55555'CTA+AG+4444'UNT+7+<<MSGNO PLACEHOLDER>>'";
		const string DairyWithdrawlMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+D::AQ:9++1'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'PNA+EX+99999'UNT+5+<<MSGNO PLACEHOLDER>>'";
		const string DairyCopyMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+D::AQ:9++31'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'PNA+EX+99999'UNT+5+<<MSGNO PLACEHOLDER>>'";
		const string DairyAcceptedTransferInMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+D::AQ:9++92+AP'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'PNA+EX+99999'UNT+5+<<MSGNO PLACEHOLDER>>'";
		const string DairyDeclinedTransferInMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+D::AQ:9++92+RE'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'PNA+EX+99999'UNT+5+<<MSGNO PLACEHOLDER>>'";
	}
}
