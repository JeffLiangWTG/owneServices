using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class RequestForPermitSkinsAndHidesMessageBuilderTest : RequestForPermitMessageBuilderAbstractTest
	{
		protected override ZString ExpectedRFPMessage => SkinsAndHidesLodgeMessage;

		protected override ZString ExpectedTransferRFPMessage => SkinsAndHidesTransferMessage;

		protected override ZString ExpectedWithdrawlRFPMessage => SkinsAndHidesWithdrawlMessage;

		protected override ZString ExpectedCopyRFPMessage => SkinsAndHidesCopyMessage;

		protected override ZString ExpectedAcceptedTransferInRFPMessage => SkinsAndHidesAcceptedTransferInMessage;

		protected override ZString ExpectedDeclinedTransferInRFPMessage => SkinsAndHidesDelinedTransferInMessage;

		protected override RequestForPermitHeaderMessageBuilder GetMessageBuilder => new RequestForPermitSkinsAndHidesHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.LDG);

		protected override RequestForPermitHeaderMessageBuilder GetTransferMessageBuilder => new RequestForPermitSkinsAndHidesHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.TRF);

		protected override RequestForPermitHeaderMessageBuilder GetWithdrawlMessageBuilder => new RequestForPermitSkinsAndHidesHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.CAN);

		protected override RequestForPermitHeaderMessageBuilder GetCopyMessageBuilder => new RequestForPermitSkinsAndHidesHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.CPY);

		protected override RequestForPermitHeaderMessageBuilder GetAcceptTransferInMessageBuilder => new RequestForPermitSkinsAndHidesHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.AcceptTransferIn);

		protected override RequestForPermitHeaderMessageBuilder GetDeclinetTransferInMessageBuilder => new RequestForPermitSkinsAndHidesHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.DeclineTransferIn);

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			invoiceHeader = helper.Header1;
			helper.Declaration.JE_RL_NKPortOfLoading = "AUMEL";
			helper.Declaration.JE_RL_NKPortOfArrival = "THBKK";
			helper.Declaration.JE_RL_NKFinalDestination = "THBKK";
			var quarantineHeader = invoiceHeader.QuarantineExDocHeader;
			quarantineHeader.QH_CertificateRequiredLocation = "CBR";
			quarantineHeader.QH_AQISRegion = "CBR";
			helper.Declaration.JE_DeclarationReference = "B00001005";
			var notifyTextNote = helper.Declaration.Notes.AddNew();
			notifyTextNote.ST_Description = PredefinedNoteTypes.Instance.EXDOCNotifyText.Description;
			notifyTextNote.ST_NoteText = "NEDDY SEAGOON";
			var letterOfCreditInfoNote = helper.Declaration.Notes.AddNew();
			letterOfCreditInfoNote.ST_Description = PredefinedNoteTypes.Instance.EXDOCLetterOfCredit.Description;
			letterOfCreditInfoNote.ST_NoteText = "ATTN MR BANKER, BANK OF NEW ZEALAND";
			quarantineHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.Automatic;
			quarantineHeader.QH_ExporterDeclaration = "SOLELY";
			quarantineHeader.QH_TransfereeExporterNumber = "H343K";
			quarantineHeader.QH_TransfereeEDIUserIdentifier = "I342";
			quarantineHeader.QH_DecOfCompliance = EXDOCYesNoEmpty.Codes.Yes;
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
			importer.MainAddress.OA_City = "BANGKOK";
			importer.MainAddress.OA_PostCode = "654321";
			importer.OH_RL_NKClosestPort = "THBKK";
			importer.MainAddress.OA_State = "BK";
			helper.Declaration.JE_OH_Importer = importer.PK;
			helper.Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			helper.Declaration.JE_VoyageFlightNo = "191";
			helper.Declaration.JE_VesselName = "ADMIRALENGRACHT";
			helper.Declaration.JE_ExportDate = new ZDate(2006, 09, 09);
			var quarantineLine = helper.Line1.QuarantineExDocLine;
			quarantineLine.QL_NetQuantity = 100;
			quarantineLine.QL_NetQuantityUnit = EXDOCMetricWeightUnitCodes.Codes.Kilogram;
			quarantineLine.QL_GrossMetricWeight = 110;
			quarantineLine.QL_GrossMetricWeightUnit = EXDOCMetricWeightUnitCodes.Codes.Kilogram;
			quarantineLine.QL_PreservationType = EXDOCPreservationTypeCodes.Codes.Unrefrigerated;
			quarantineLine.QL_ProductType = "AB";
			quarantineLine.QL_PackType = EXDOCPackTypeCodes.Codes.Cartons;
			quarantineLine.QL_CutCode = "SH0132";
			quarantineLine.QL_SaltingDate = new ZDateTime(2006, 09, 01);
			quarantineLine.QL_OuterPackCount = 10;
			quarantineLine.QL_OuterPackType = EXDOCPacakgeTypeCodes.Codes.Cartons;
			quarantineLine.QL_SendHCDesc = true;
			quarantineLine.QL_BatchCode = "BC123456";
			quarantineLine.QL_AqisCustomsWeight = 2000;
		}
		JobComInvoiceHeader invoiceHeader;

		const string SkinsAndHidesLodgeMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+S::AQ:9++13'LOC+9+MEL'LOC+12+THBKK'LOC+8+BANGKOK'LOC+36+TH'LOC+30+AU'LOC+91+CBR'LOC+48+CBR'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'FTX+DCL+++SOLELY'FTX+AAG+++NEDDY SEAGOON'FTX+AAW+++ATTN MR BANKER, BANK OF NEW ZEALAND'GIS+A::AQ:PHC'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'PNA+EX+99999'PNA+CN+++++10:WALLACE THE IMPORTER'ADR++5:62 WEST WALLABY ST+BANGKOK+654321+TH+:::BK'CTA+AG+:CONSIGNEE AGENT TEST'TDT+12+191+1+++++:::ADMIRALENGRACHT'DTM+136:20060909:102'LIN+1'MEA+AAA+SQ+KGM:100.000'MEA+AAI+AAE+KGM:110'MEA+AAF+SQ+KGM:2000'PIA+5+UAB CT  :CC'PIA+5+SH0132:BP'ATT+10++N:FCI:AQ'DTM+9:20060901:102'PAC+10+3+CT::AQ'UNT+34+<<MSGNO PLACEHOLDER>>'";
		const string SkinsAndHidesWithdrawlMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+S::AQ:9++1'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'PNA+EX+99999'UNT+5+<<MSGNO PLACEHOLDER>>'";
		const string SkinsAndHidesTransferMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+S::AQ:9++90'GIS+N::AQ:CT'PNA+EX+99999'PNA+TT+H343K'CTA+AG+I342'UNT+7+<<MSGNO PLACEHOLDER>>'";
		const string SkinsAndHidesCopyMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+S::AQ:9++31'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'PNA+EX+99999'UNT+5+<<MSGNO PLACEHOLDER>>'";
		const string SkinsAndHidesAcceptedTransferInMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+S::AQ:9++92+AP'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'PNA+EX+99999'UNT+5+<<MSGNO PLACEHOLDER>>'";
		const string SkinsAndHidesDelinedTransferInMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+S::AQ:9++92+RE'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'PNA+EX+99999'UNT+5+<<MSGNO PLACEHOLDER>>'";
	}
}
