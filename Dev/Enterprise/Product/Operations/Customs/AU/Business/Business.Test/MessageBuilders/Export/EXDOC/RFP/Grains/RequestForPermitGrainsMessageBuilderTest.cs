using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class RequestForPermitGrainsMessageBuilderTest : RequestForPermitMessageBuilderAbstractTest
	{
		protected override ZString ExpectedRFPMessage => GrainsAndPlantsLodgeMessage;

		protected override ZString ExpectedAmendmentRFPMessage => GrainsAndPlantsAmendmentLodgeMessage;

		protected override ZString ExpectedTransferRFPMessage => GrainsAndPlantsTransferMessage;

		protected override ZString ExpectedWithdrawlRFPMessage => GrainsAndPlantsWithdrawlMessage;

		protected override ZString ExpectedCopyRFPMessage => GrainsAndPlantsCopyMessage;

		protected override ZString ExpectedAcceptedTransferInRFPMessage => GrainsAndPlantsAcceptedTransferInMessage;

		protected override ZString ExpectedDeclinedTransferInRFPMessage => GrainsAndPlantsDeclinedTransferInMessage;

		protected override RequestForPermitHeaderMessageBuilder GetMessageBuilder => new RequestForPermitGrainsAndPlantsHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.LDG);

		protected override RequestForPermitHeaderMessageBuilder GetAmendmentMessageBuilder => new RequestForPermitGrainsAndPlantsHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.RPL);

		protected override RequestForPermitHeaderMessageBuilder GetTransferMessageBuilder => new RequestForPermitGrainsAndPlantsHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.TRF);

		protected override RequestForPermitHeaderMessageBuilder GetWithdrawlMessageBuilder => new RequestForPermitGrainsAndPlantsHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.CAN);

		protected override RequestForPermitHeaderMessageBuilder GetCopyMessageBuilder => new RequestForPermitGrainsAndPlantsHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.CPY);

		protected override RequestForPermitHeaderMessageBuilder GetAcceptTransferInMessageBuilder => new RequestForPermitGrainsAndPlantsHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.AcceptTransferIn);

		protected override RequestForPermitHeaderMessageBuilder GetDeclinetTransferInMessageBuilder => new RequestForPermitGrainsAndPlantsHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.DeclineTransferIn);

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			invoiceHeader = helper.Header1;
			helper.Declaration.JE_RL_NKPortOfLoading = "AUSYD";
			helper.Declaration.JE_MessageSubType = JobDeclaration.MessageSubType.Confirming;
			helper.Declaration.JE_MarksAndNumbers = "MARKS1";
			var quarantineHeader = invoiceHeader.QuarantineExDocHeader;
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			quarantineHeader.QH_AQISRegion = "BNE";
			quarantineHeader.QH_CertificateRequiredLocation = "BNE";
			helper.Declaration.JE_DeclarationReference = "B00001005";
			var notifyTextNote = helper.Declaration.Notes.AddNew();
			notifyTextNote.ST_Description = PredefinedNoteTypes.Instance.EXDOCNotifyText.Description;
			notifyTextNote.ST_NoteText = "NEDDY SEAGOON";
			var letterOfCreditInfoNote = helper.Declaration.Notes.AddNew();
			letterOfCreditInfoNote.ST_Description = PredefinedNoteTypes.Instance.EXDOCLetterOfCredit.Description;
			letterOfCreditInfoNote.ST_NoteText = "ATTN MR BANKER, BANK OF NEW ZEALAND";
			var amendmentTextNote = helper.Declaration.Notes.AddNew();
			amendmentTextNote.ST_Description = PredefinedNoteTypes.Instance.EXDOCAmendmentReason.Description;
			amendmentTextNote.ST_NoteText = "AMENDMENT TEXT";
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "AUD";
			quarantineHeader.QH_ObtainExportCustomsPermit = ZBool.True;
			quarantineHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.Automatic;
			quarantineHeader.QH_DecOfCompliance = EXDOCYesNoEmpty.Codes.Yes;
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "TSTSUP";
			var aU = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia);
			supplier.SetCustomsCode(OrgCusCode.AUQuarantineCodeTypes.EXDOCExporterNumber, aU, "99999");
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "CONSJOE";
			importer.OH_FullName = "CONSIGNEE JOE";
			importer.MainAddress.OA_Address1 = "63 PENSYLLVANIA AVE";
			importer.MainAddress.OA_City = "WASHINGTON";
			importer.OH_RL_NKClosestPort = "USWAS";
			helper.Declaration.JE_OH_Importer = importer.PK;
			helper.Declaration.JE_RL_NKPortOfArrival = "NZWLG";
			helper.Declaration.JE_RL_NKPortOfFirstArrival = "USWAS";
			helper.Declaration.JE_RL_NKFinalDestination = "NZWLG";
			helper.Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			helper.Declaration.JE_VoyageFlightNo = "68";
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "BLUEBOTTLE INDUSTRIES";
			vessel.RV_LloydsNumber = "ECCLES";
			helper.Declaration.JE_VesselName = vessel.RV_Code;
			helper.Declaration.JE_ExportDate = new ZDate(2004, 04, 12);
			quarantineHeader.QH_InspectionRequestedDate = new ZDateTime(2004, 03, 28, 10, 30, 00);
			quarantineHeader.QH_AuthorisationEstablishment = "604";
			quarantineHeader.QH_TransfereeExporterNumber = "45678";
			quarantineHeader.QH_TransfereeEDIUserIdentifier = "123890";
			quarantineHeader.QH_ConsigneeAgentName = "Consignee Agent Test";
			var invoiceLine = helper.Line1;
			invoiceLine.JI_Tariff = "08029019";
			invoiceLine.JI_Description = "PECAN NUTS, SHELLED";
			invoiceLine.JI_Drawback = ZBool.False;
			invoiceLine.JI_LinePrice = 1000;
			invoiceLine.AddInfo.ZA_TILV = "1AUD";
			var quarantineLine = invoiceLine.QuarantineExDocLine;
			quarantineLine.QL_NetQuantity = 100;
			quarantineLine.QL_NetQuantityUnit = EXDOCMetricWeightUnitCodes.Codes.MetricTonne;
			quarantineLine.QL_GrossMetricWeight = 120;
			quarantineLine.QL_GrossMetricWeightUnit = EXDOCMetricWeightUnitCodes.Codes.MetricTonne;
			quarantineLine.QL_ProductType = "WHT";
			quarantineLine.QL_PackType = EXDOCPacakgeTypeCodes.Codes.Bulk;
			quarantineLine.QL_SupplimentaryCode = "GC";
			quarantineLine.QL_AqisCustomsWeight = 2000;
			quarantineLine.QL_AddtionalProductDescription = "MALTING BARLEY";
			quarantineLine.QL_CommercialProductDescription = "AUSTRALIAN TWO SHILLING MALTING BARLEY";
			quarantineLine.QL_UseByStart = new ZDateTime(1999, 05, 19);
			quarantineLine.QL_UseByEnd = new ZDateTime(2006, 05, 19);
			quarantineLine.QL_AddtionalDeclarationComments = "THE GRAIN IS FROM A CROP THAT HAS BEEN INSPECTED DURING THE GROWING SEASON ACCORDING TO APPROPRIATE PROCEDURES AND  NO TILLETIA CONTRAVERSA WAS DETECTED.";
			quarantineLine.QL_OuterPackType = EXDOCPacakgeTypeCodes.Codes.Bulk;
			quarantineLine.QL_SendHCDesc = true;
			quarantineLine.QL_BatchCode = "BC123456";
			var treatmentProcess = quarantineLine.Processes.AddNew();
			treatmentProcess.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Treatment;
			treatmentProcess.EE_TreatmentCode = EXDOCTreatmentCodes.Codes.MoistHeat;
			treatmentProcess.EE_TreatmentInfo = "90-95 DEGREES CELSIUS AT 40% RELATIVE HUMIDITY FOR AT LEAST 15 CONTINUOUS HOURS";
			treatmentProcess.EE_StartDate = new ZDateTime(2004, 03, 15);
		}
		JobComInvoiceHeader invoiceHeader;

		const string GrainsAndPlantsLodgeMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+G::AQ:9++13'LOC+9+SYD'LOC+12+NZWLG'LOC+8+WELLINGTON'LOC+36+NZ'LOC+30+AU'LOC+49+US'LOC+91+BNE'LOC+48+BNE'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'MOA+63::AUD'GIS+A::AQ:PHC'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+Y::AQ:ACS'GIS+C::AQ:DCT'GIS+Y::AQ:DOC'PNA+EX+99999'PNA+CN+++++10:CONSIGNEE JOE'ADR++5:63 PENSYLLVANIA AVE+WASHINGTON++US+:::DC'CTA+AG+:CONSIGNEE AGENT TEST'TDT+12+68+1+++++:::BLUEBOTTLE INDUSTRIES'DTM+136:20040412:102'PRC+IN:PP:AQ'DTM+318:20040328103000:204'PNA+FO+604'LIN+1'MEA+AAA+SQ+TNE:100.000'MEA+AAI+AAE+TNE:120'MEA+AAF+SQ+KGM:2000'PIA+5+XWHTVRGC:CC'PIA+5+08029019:HS'IMD+++UHC:::PECAN NUTS, SHELLED'IMD+++AD:::MALTING BARLEY'IMD+++CD:::AUSTRALIAN TWO SHILLING MALTING BAR:LEY'ATT+10++N:FCI:AQ'DTM+194:19990519:102'DTM+206:20060519:102'FTX+AAZ+++THE GRAIN IS FROM A CROP THAT HAS BEEN INSPECTED DURING THE GROWING SE:ASON ACCORDING TO APPROPRIATE PROCEDURES AND  NO TILLETIA CONTRAVERSA :WAS DETECTED.'MOA+63:1000.00'PAC+0+3+VR::AQ'PCI++MARKS1'PRC+TR:PP:AQ'IMD++25+M HEAT:::90-95 DEGREES CELSIUS AT 40% RELATI:VE HUMIDITY FOR AT LEAST 15 CONTINU:OUS HOURS'DTM+194:20040315:102'UNT+48+<<MSGNO PLACEHOLDER>>'";
		const string GrainsAndPlantsAmendmentLodgeMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+G::AQ:9++4'LOC+9+SYD'LOC+12+NZWLG'LOC+8+WELLINGTON'LOC+36+NZ'LOC+30+AU'LOC+49+US'LOC+91+BNE'LOC+48+BNE'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'FTX+CHG+++AMENDMENT TEXT'MOA+63::AUD'GIS+A::AQ:PHC'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+Y::AQ:ACS'GIS+C::AQ:DCT'GIS+Y::AQ:DOC'PNA+EX+99999'PNA+CN+++++10:CONSIGNEE JOE'ADR++5:63 PENSYLLVANIA AVE+WASHINGTON++US+:::DC'CTA+AG+:CONSIGNEE AGENT TEST'TDT+12+68+1+++++:::BLUEBOTTLE INDUSTRIES'DTM+136:20040412:102'PRC+IN:PP:AQ'DTM+318:20040328103000:204'PNA+FO+604'LIN+1'MEA+AAA+SQ+TNE:100.000'MEA+AAI+AAE+TNE:120'MEA+AAF+SQ+KGM:2000'PIA+5+XWHTVRGC:CC'PIA+5+08029019:HS'IMD+++UHC:::PECAN NUTS, SHELLED'IMD+++AD:::MALTING BARLEY'IMD+++CD:::AUSTRALIAN TWO SHILLING MALTING BAR:LEY'ATT+10++N:FCI:AQ'DTM+194:19990519:102'DTM+206:20060519:102'FTX+AAZ+++THE GRAIN IS FROM A CROP THAT HAS BEEN INSPECTED DURING THE GROWING SE:ASON ACCORDING TO APPROPRIATE PROCEDURES AND  NO TILLETIA CONTRAVERSA :WAS DETECTED.'MOA+63:1000.00'PAC+0+3+VR::AQ'PCI++MARKS1'PRC+TR:PP:AQ'IMD++25+M HEAT:::90-95 DEGREES CELSIUS AT 40% RELATI:VE HUMIDITY FOR AT LEAST 15 CONTINU:OUS HOURS'DTM+194:20040315:102'UNT+49+<<MSGNO PLACEHOLDER>>'";
		const string GrainsAndPlantsWithdrawlMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+G::AQ:9++1'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'PNA+EX+99999'UNT+5+<<MSGNO PLACEHOLDER>>'";
		const string GrainsAndPlantsTransferMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+G::AQ:9++90'GIS+N::AQ:CT'PNA+EX+99999'PNA+TT+45678'CTA+AG+123890'UNT+7+<<MSGNO PLACEHOLDER>>'";
		const string GrainsAndPlantsCopyMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+G::AQ:9++31'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'PNA+EX+99999'UNT+5+<<MSGNO PLACEHOLDER>>'";
		const string GrainsAndPlantsAcceptedTransferInMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+G::AQ:9++92+AP'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'PNA+EX+99999'UNT+5+<<MSGNO PLACEHOLDER>>'";
		const string GrainsAndPlantsDeclinedTransferInMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+G::AQ:9++92+RE'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'PNA+EX+99999'UNT+5+<<MSGNO PLACEHOLDER>>'";
	}
}
