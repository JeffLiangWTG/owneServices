using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class RequestForPermitHorticultureMessageBuilderTest : RequestForPermitMessageBuilderAbstractTest
	{
		protected override RequestForPermitHeaderMessageBuilder GetAmendmentMessageBuilder
		{
			get
			{
				invoiceHeader.QuarantineExDocHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.HcrdHealthCertificateReady;
				return new RequestForPermitHorticultureHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.RPL);
			}
		}

		protected override RequestForPermitHeaderMessageBuilder GetMessageBuilder => new RequestForPermitHorticultureHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.LDG);

		protected override RequestForPermitHeaderMessageBuilder GetTransferMessageBuilder => new RequestForPermitHorticultureHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.TRF);

		protected override RequestForPermitHeaderMessageBuilder GetWithdrawlMessageBuilder => new RequestForPermitHorticultureHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.CAN);

		protected override RequestForPermitHeaderMessageBuilder GetCopyMessageBuilder => new RequestForPermitHorticultureHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.CPY);

		protected override RequestForPermitHeaderMessageBuilder GetAcceptTransferInMessageBuilder => new RequestForPermitHorticultureHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.AcceptTransferIn);

		protected override RequestForPermitHeaderMessageBuilder GetDeclinetTransferInMessageBuilder => new RequestForPermitHorticultureHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.DeclineTransferIn);

		protected override ZString ExpectedRFPMessage => HorticultureLodgeMessage;

		protected override ZString ExpectedAmendmentRFPMessage => HorticultureAmendedLodgeMessage;

		protected override ZString ExpectedTransferRFPMessage => HorticultureTransferMessage;

		protected override ZString ExpectedWithdrawlRFPMessage => HorticultureWithdrawlMessage;

		protected override ZString ExpectedCopyRFPMessage => HorticultureCopyMessage;

		protected override ZString ExpectedAcceptedTransferInRFPMessage => HorticultureAcceptedTransferInMessage;

		protected override ZString ExpectedDeclinedTransferInRFPMessage => HorticultureDeclinedTransferInMessage;

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			invoiceHeader = helper.Header1;
			helper.Declaration.JE_RL_NKPortOfLoading = "AUSYD";
			helper.Declaration.JE_DeclarationReference = "B00001002";
			invoiceHeader.JZ_RX_NKInvoice_Currency = "AUD";
			var quarantineHeader = invoiceHeader.QuarantineExDocHeader;
			quarantineHeader.QH_AQISRegion = "BNE";
			quarantineHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.NotRequired;
			quarantineHeader.QH_ObtainExportCustomsPermit = ZBool.True;
			quarantineHeader.QH_DecOfCompliance = EXDOCYesNoEmpty.Codes.Yes;
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "TSTSUP";
			var au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia);
			supplier.SetCustomsCode(OrgCusCode.AUQuarantineCodeTypes.EXDOCExporterNumber, au, "99999");
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "BABERTD";
			importer.OH_FullName = "VACUUM SWITCHING AUSTRALIA PTY LTD";
			importer.MainAddress.OA_Address1 = "PO BOX 299";
			importer.MainAddress.OA_City = "BRISBANE";
			importer.MainAddress.OA_PostCode = "4170";
			importer.OH_RL_NKClosestPort = "AUBNE";
			helper.Declaration.JE_OH_Importer = importer.PK;
			helper.Declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine; //Setting the importer changes it back to IMP (Import)
			helper.Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			helper.Declaration.JE_VoyageFlightNo = "12";
			helper.Declaration.JE_ExportDate = new ZDate(2006, 08, 05);
			helper.Declaration.JE_MarksAndNumbers = "KEEP COOL";
			quarantineHeader.QH_InspectionRequestedDate = new ZDateTime(2006, 07, 26, 11, 16, 00);
			quarantineHeader.QH_AuthorisationEstablishment = "77";
			quarantineHeader.QH_TransfereeExporterNumber = "A2132";
			quarantineHeader.QH_TransfereeEDIUserIdentifier = "N898374";
			quarantineHeader.QH_ConsigneeAgentName = "Consignee Agent Test";
			helper.Declaration.JE_RL_NKPortOfArrival = "AEDXB";
			helper.Declaration.JE_RL_NKPortOfFirstArrival = "JPTYO";
			helper.Declaration.JE_RL_NKFinalDestination = "AEDXB";
			var notifyTextNote = helper.Declaration.Notes.AddNew();
			notifyTextNote.ST_Description = PredefinedNoteTypes.Instance.EXDOCNotifyText.Description;
			notifyTextNote.ST_NoteText = "NEDDY SEAGOON";
			var letterOfCreditInfoNote = helper.Declaration.Notes.AddNew();
			letterOfCreditInfoNote.ST_Description = PredefinedNoteTypes.Instance.EXDOCLetterOfCredit.Description;
			letterOfCreditInfoNote.ST_NoteText = "ATTN MR BANKER, BANK OF NEW ZEALAND";
			var amendmentTextNote = helper.Declaration.Notes.AddNew();
			amendmentTextNote.ST_Description = PredefinedNoteTypes.Instance.EXDOCAmendmentReason.Description;
			amendmentTextNote.ST_NoteText = "AMENDMENT TEXT";
			var invoiceLine = helper.Line1;
			invoiceLine.JI_TempImportNum = "123456";
			invoiceLine.JI_TempImportDate = new ZDateTime(2006, 12, 12);
			invoiceLine.JI_Tariff = "08071900";
			invoiceLine.JI_AUState = "QLD";
			invoiceLine.JI_LinePrice = 1000;
			invoiceLine.AddInfo.ZA_TILV = "1AUD";
			var quarantineLine = invoiceLine.QuarantineExDocLine;
			quarantineLine.QL_NetQuantity = 1960.00m;
			quarantineLine.QL_NetQuantityUnit = EXDOCMetricWeightUnitCodes.Codes.Kilogram;
			quarantineLine.QL_GrossMetricWeight = 2100;
			quarantineLine.QL_GrossMetricWeightUnit = EXDOCMetricWeightUnitCodes.Codes.Kilogram;
			quarantineLine.QL_ProductType = "RCM";
			quarantineLine.QL_PackType = EXDOCPacakgeTypeCodes.Codes.Cartons;
			quarantineLine.QL_AqisCustomsWeight = 2000;
			quarantineLine.QL_OuterPackCount = 140;
			quarantineLine.QL_OuterPackType = EXDOCPacakgeTypeCodes.Codes.Cartons;
			quarantineLine.QL_OuterPackWeight = 14;
			quarantineLine.QL_OuterPackWeightUnit = EXDOCMetricWeightUnitCodes.Codes.Kilogram;
			quarantineLine.QL_OuterPackAccuracy = EXDOCPackAccuracyCodes.Codes.Approximate;
			quarantineLine.QL_StatementNumber1 = 1345;
			quarantineLine.QL_StatementText = "LEAH WAS HOT";
			quarantineLine.QL_SendHCDesc = true;
			quarantineLine.QL_BatchCode = "BC123456";
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_TempImportNum = "123489";
			invoiceLine2.JI_TempImportDate = new ZDateTime(2006, 12, 12);
			invoiceLine2.JI_Tariff = "08071900";
			invoiceLine2.JI_AUState = "QLD";
			invoiceLine2.JI_LinePrice = 234;
			invoiceLine2.AddInfo.ZA_TILV = "1AUD";
			invoiceLine2.JI_Drawback = ZBool.True;
			var quarantineLine2 = invoiceLine2.QuarantineExDocLine;
			quarantineLine2.QL_NetQuantity = 1792.00m;
			quarantineLine2.QL_NetQuantityUnit = EXDOCMetricWeightUnitCodes.Codes.Kilogram;
			quarantineLine2.QL_GrossMetricWeight = 1920;
			quarantineLine2.QL_GrossMetricWeightUnit = EXDOCMetricWeightUnitCodes.Codes.Kilogram;
			quarantineLine2.QL_ProductType = "MHD";
			quarantineLine2.QL_PackType = EXDOCPacakgeTypeCodes.Codes.Cartons;
			quarantineLine2.QL_OuterPackCount = 128;
			quarantineLine2.QL_OuterPackType = EXDOCPacakgeTypeCodes.Codes.Cartons;
			quarantineLine2.QL_OuterPackWeight = 14;
			quarantineLine2.QL_OuterPackWeightUnit = EXDOCMetricWeightUnitCodes.Codes.Kilogram;
			quarantineLine2.QL_OuterPackAccuracy = EXDOCPackAccuracyCodes.Codes.Approximate;
			quarantineLine2.QL_SendHCDesc = true;
		}
		JobComInvoiceHeader invoiceHeader;

		const string HorticultureLodgeMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+H::AQ:0++13'LOC+9+SYD'LOC+12+AEDXB'LOC+8+DUBAI'LOC+36+AE'LOC+30+AU'LOC+49+JP'LOC+48+BNE'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'MOA+63::AUD'GIS+N::AQ:PHC'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+Y::AQ:ACS'GIS+N::AQ:DCT'GIS+Y::AQ:DOC'DOC+911+123456'DTM+137:20061212:102'DOC+911+123489'DTM+137:20061212:102'PNA+EX+99999'PNA+CN+++++10:VACUUM SWITCHING AUSTRALIA PTY LTD'ADR++5:PO BOX 299+BRISBANE+4170+AU+:::QLD'CTA+AG+:CONSIGNEE AGENT TEST'TDT+12+12+4'DTM+136:20060805:102'PRC+IN:PP:AQ'DTM+318:20060726111600:204'PNA+FO+77'LIN+1'MEA+AAA+SQ+KGM:1960.000'MEA+AAI+AAE+KGM:2100'MEA+AAF+SQ+KGM:2000'PIA+5+XRCMCT  :CC'PIA+5+08071900:HS'ATT+10++N:FCI:AQ'LOC+ZZZ+QLD'FTX+ZZZ+++1345'FTX+AAY+++LEAH WAS HOT'MOA+63:1000.00'PAC+140+3+CT::AQ'PCI++KEEP COOL'MEA+AAU+AAL:3+KGM:14.000'LIN+2'MEA+AAA+SQ+KGM:1792.000'MEA+AAI+AAE+KGM:1920'PIA+5+XMHDCT  :CC'PIA+5+08071900:HS'RFF+AGW:DRB'ATT+10++N:FCI:AQ'LOC+ZZZ+QLD'MOA+63:234.00'PAC+128+3+CT::AQ'PCI++KEEP COOL'MEA+AAU+AAL:3+KGM:14.000'UNT+58+<<MSGNO PLACEHOLDER>>'";
		const string HorticultureAmendedLodgeMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+H::AQ:0++4'LOC+9+SYD'LOC+12+AEDXB'LOC+8+DUBAI'LOC+30+AU'LOC+49+JP'LOC+48+BNE'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'FTX+CHG+++AMENDMENT TEXT'MOA+63::AUD'GIS+N::AQ:PHC'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+Y::AQ:ACS'GIS+N::AQ:DCT'GIS+Y::AQ:DOC'DOC+911+123456'DTM+137:20061212:102'DOC+911+123489'DTM+137:20061212:102'PNA+EX+99999'PNA+CN+++++10:VACUUM SWITCHING AUSTRALIA PTY LTD'ADR++5:PO BOX 299+BRISBANE+4170+AU+:::QLD'CTA+AG+:CONSIGNEE AGENT TEST'TDT+12+12+4'DTM+136:20060805:102'PRC+IN:PP:AQ'LIN+1'MEA+AAA+SQ+KGM:1960.000'MEA+AAI+AAE+KGM:2100'MEA+AAF+SQ+KGM:2000'PIA+5+08071900:HS'ATT+10++N:FCI:AQ'LOC+ZZZ+QLD'FTX+ZZZ+++1345'FTX+AAY+++LEAH WAS HOT'MOA+63:1000.00'PAC+140+3+CT::AQ'PCI++KEEP COOL'MEA+AAU+AAL:3+KGM:14.000'LIN+2'MEA+AAA+SQ+KGM:1792.000'MEA+AAI+AAE+KGM:1920'PIA+5+08071900:HS'RFF+AGW:DRB'ATT+10++N:FCI:AQ'LOC+ZZZ+QLD'MOA+63:234.00'PAC+128+3+CT::AQ'PCI++KEEP COOL'MEA+AAU+AAL:3+KGM:14.000'UNT+54+<<MSGNO PLACEHOLDER>>'";
		const string HorticultureWithdrawlMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+H::AQ:0++1'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'PNA+EX+99999'UNT+5+<<MSGNO PLACEHOLDER>>'";
		const string HorticultureTransferMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+H::AQ:9++90'GIS+N::AQ:CT'PNA+EX+99999'PNA+TT+A2132'CTA+AG+N898374'UNT+7+<<MSGNO PLACEHOLDER>>'";
		const string HorticultureCopyMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+H::AQ:0++31'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'PNA+EX+99999'UNT+5+<<MSGNO PLACEHOLDER>>'";
		const string HorticultureAcceptedTransferInMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+H::AQ:0++92+AP'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'PNA+EX+99999'UNT+5+<<MSGNO PLACEHOLDER>>'";
		const string HorticultureDeclinedTransferInMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+H::AQ:0++92+RE'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'PNA+EX+99999'UNT+5+<<MSGNO PLACEHOLDER>>'";
	}
}
