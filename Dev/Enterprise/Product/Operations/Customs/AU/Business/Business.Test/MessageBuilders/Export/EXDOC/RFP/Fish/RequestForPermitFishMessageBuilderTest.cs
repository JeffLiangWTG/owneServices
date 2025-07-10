using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class RequestForPermitFishMessageBuilderTest : RequestForPermitMessageBuilderAbstractTest
	{
		[TestDate(2016, 12, 25)]
		public override void TestRFPMessageBuilder()
		{
			expectedRFPMessage = FishLodgeMessage;
			base.TestRFPMessageBuilder();
		}

		[TestDate(2017, 04, 01)]
		public void TestRFPMessageBuilderErrata44()
		{
			expectedRFPMessage = FishLodgeMessage_E44;
			base.TestRFPMessageBuilder();
		}

		protected override ZString ExpectedRFPMessage => expectedRFPMessage;

		protected override RequestForPermitHeaderMessageBuilder GetMessageBuilder => new RequestForPermitFishHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.LDG);

		protected override RequestForPermitHeaderMessageBuilder GetTransferMessageBuilder => new RequestForPermitFishHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.TRF);

		protected override RequestForPermitHeaderMessageBuilder GetWithdrawlMessageBuilder => new RequestForPermitFishHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.CAN);

		protected override RequestForPermitHeaderMessageBuilder GetCopyMessageBuilder => new RequestForPermitFishHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.CPY);

		protected override RequestForPermitHeaderMessageBuilder GetAcceptTransferInMessageBuilder => new RequestForPermitFishHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.AcceptTransferIn);

		protected override RequestForPermitHeaderMessageBuilder GetDeclinetTransferInMessageBuilder => new RequestForPermitFishHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.DeclineTransferIn);

		protected override ZString ExpectedTransferRFPMessage => FishTransferMessage;

		protected override ZString ExpectedWithdrawlRFPMessage => FishWithdrawlMessage;

		protected override ZString ExpectedCopyRFPMessage => FishCopyMessage;

		protected override ZString ExpectedAcceptedTransferInRFPMessage => FishAcceptedTransferInMessage;

		protected override ZString ExpectedDeclinedTransferInRFPMessage => FishDeclinedTransferInMessage;

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			invoiceHeader = helper.Header1;
			helper.Declaration.JE_RL_NKPortOfLoading = "AUSYD";
			helper.Declaration.JE_RL_NKPortOfArrival = "USWAS";
			helper.Declaration.JE_RL_NKPortOfFirstArrival = "MXLZC";
			helper.Declaration.JE_RL_NKFinalDestination = "USWAS";
			helper.Declaration.JE_DeclarationReference = "B00001003";
			helper.Declaration.DeclarationNumber = "AAEETF6AL";
			helper.Declaration.JE_MarksAndNumbers = "MARKS1";
			var notifyTextNote = helper.Declaration.Notes.AddNew();
			notifyTextNote.ST_Description = PredefinedNoteTypes.Instance.EXDOCNotifyText.Description;
			notifyTextNote.ST_NoteText = "NEDDY SEAGOON";
			var letterOfCreditInfoNote = helper.Declaration.Notes.AddNew();
			letterOfCreditInfoNote.ST_Description = PredefinedNoteTypes.Instance.EXDOCLetterOfCredit.Description;
			letterOfCreditInfoNote.ST_NoteText = "ATTN MR BANKER, BANK OF NEW ZEALAND";
			var quarantineExDocHeader = invoiceHeader.QuarantineExDocHeader;
			quarantineExDocHeader.QH_MinimumTemperature = -6.5m;
			quarantineExDocHeader.QH_MaximumTemperature = -1.25m;
			quarantineExDocHeader.QH_TemperatureUM = EXDOCTemperatureUnitCodes.Codes.Celsius;
			quarantineExDocHeader.QH_AQISRegion = "CBR";
			quarantineExDocHeader.QH_CertificateRequiredLocation = "CBR";
			quarantineExDocHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.Automatic;
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			quarantineExDocHeader.QH_CancelTransferIndicator = true;
			quarantineExDocHeader.QH_TransfereeExporterNumber = "12345";
			quarantineExDocHeader.QH_TransfereeEDIUserIdentifier = "98765";
			quarantineExDocHeader.QH_DecOfCompliance = EXDOCYesNoEmpty.Codes.Yes;
			quarantineExDocHeader.QH_ConsigneeAgentName = "Consignee Agent Test";
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
			helper.Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			helper.Declaration.JE_OH_Importer = importer.PK;
			helper.Declaration.JE_VoyageFlightNo = "68";
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "BLUEBOTTLE INDUSTRIES";
			vessel.RV_LloydsNumber = "TITANIC";
			helper.Declaration.JE_VesselName = vessel.RV_Code;
			helper.Declaration.JE_ExportDate = new ZDate(2004, 04, 20);
			quarantineExDocHeader.QH_InspectionRequestedDate = new ZDateTime(2004, 04, 10);
			quarantineExDocHeader.QH_AuthorisationEstablishment = "604";
			var letter = quarantineExDocHeader.RecommendationLetters.AddNew();
			letter.ZA_LetterNumber = "ABC123";
			letter.ZA_LetterDate = new ZDate(2017, 12, 25);
			var invoiceLine = helper.Line1;
			invoiceLine.JI_TempImportNum = "ABC123";
			invoiceLine.JI_TempImportDate = new ZDateTime(2004, 03, 15);
			invoiceLine.JI_Tariff = "02011030";
			invoiceLine.JI_Description = "CHILLED COD BRAIN";
			invoiceLine.JI_LinePrice = 1000;
			invoiceLine.AddInfo.ZA_TILV = "1AUD";
			var quarantineLine = invoiceLine.QuarantineExDocLine;
			quarantineLine.QL_NetQuantity = 100;
			quarantineLine.QL_NetQuantityUnit = EXDOCMetricWeightUnitCodes.Codes.MetricTonne;
			quarantineLine.QL_ImperialNetWeight = 100;
			quarantineLine.QL_ImperialNetWeightUnit = EXDOCImperialWeightUnitCodes.Codes.TonUkOrLongtonUs;
			quarantineLine.QL_GrossMetricWeight = 120;
			quarantineLine.QL_GrossMetricWeightUnit = EXDOCMetricWeightUnitCodes.Codes.MetricTonne;
			quarantineLine.QL_PreservationType = EXDOCPreservationTypeCodes.Codes.Chilled;
			quarantineLine.QL_ProductType = "AET";
			quarantineLine.QL_PackType = EXDOCPacakgeTypeCodes.Codes.BulkPack;
			quarantineLine.QL_CutCode = "6130";
			quarantineLine.QL_CatchStartDate = new ZDateTime(2011, 01, 01);
			quarantineLine.QL_CatchEndDate = new ZDateTime(2011, 02, 02);
			quarantineLine.QL_AqisCustomsWeight = 2000;
			quarantineLine.QL_UseByStart = new ZDateTime(2004, 01, 20);
			quarantineLine.QL_UseByEnd = new ZDateTime(2004, 01, 22);
			quarantineLine.QL_OuterPackCount = 1;
			quarantineLine.QL_OuterPackType = EXDOCPacakgeTypeCodes.Codes.Cartons;
			quarantineLine.QL_SendHCDesc = true;
			quarantineLine.QL_BatchCode = "BC123456";
			var harvestProcess = quarantineLine.Processes.AddNew();
			harvestProcess.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Harvest;
			harvestProcess.EE_StartDate = new ZDateTime(2003, 11, 29);
			harvestProcess.EE_EndDate = new ZDateTime(2003, 12, 01);
			harvestProcess.EE_Depuration = new ZDateTime(2003, 12, 02);
			harvestProcess.EE_HarvestArea = "MYALL LAKES";
			harvestProcess.EE_LeaseNumber = "123A";
			harvestProcess.EE_AuthorisationEstablishmentID = "604";
			var processingProcess = quarantineLine.Processes.AddNew();
			processingProcess.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Processing;
			processingProcess.EE_StartDate = new ZDateTime(2003, 12, 22);
			processingProcess.EE_EndDate = new ZDateTime(2003, 12, 22);
			processingProcess.EE_AuthorisationEstablishmentID = "604";
		}
		JobComInvoiceHeader invoiceHeader;
		string expectedRFPMessage;

		const string FishLodgeMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+F::AQ:9++13'LOC+9+SYD'LOC+12+USWAS'LOC+8+WASHINGTON'LOC+36+US'LOC+30+AU'LOC+49+MX'LOC+91+CBR'LOC+48+CBR'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'RFF+AAE:AAEETF6AL'MEA+TE+ADE+CEL::-6.50:-1.25'GIS+A::AQ:PHC'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'GIS+Y::AQ:DOC'GIS+::AQ:TAC'DOC+911+ABC123'DTM+137:20040315:102'PNA+EX+99999'PNA+CN+++++10:CONSIGNEE JOE'ADR++5:63 PENSYLLVANIA AVE+WASHINGTON++US+:::DC'CTA+AG+:CONSIGNEE AGENT TEST'TDT+12+68+1+++++:::BLUEBOTTLE INDUSTRIES'DTM+136:20040420:102'PRC+IN:PP:AQ'DTM+318:20040410:102'PNA+FO+604'LIN+1'MEA+AAA+SQ+TNE:100.000'MEA+AAI+AAL+LTN:100'MEA+AAI+AAE+TNE:120'PIA+5+CAETBP  :CC'PIA+5+6130:BP'PIA+5+02011030:HS'IMD+++UHC:::CHILLED COD BRAIN'ATT+10++N:FCI:AQ'DTM+194:20040120:102'DTM+206:20040122:102'DTM+163:20110101:102'DTM+164:20110202:102'MOA+63:1000.00'PAC+1+3+CT::AQ'PCI++MARKS1'PRC+HA:PP:AQ'DTM+194:20031129:102'DTM+206:20031201:102'DTM+9:20031202:102'LOC+48+MYALL LAKES+123A'PNA+SK+604'PRC+PC:PP:AQ'DTM+194:20031222:102'DTM+206:20031222:102'PNA+MP+604'UNT+58+<<MSGNO PLACEHOLDER>>'";
		const string FishLodgeMessage_E44 = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+F::AQ:9++13'LOC+9+SYD'LOC+12+USWAS'LOC+8+WASHINGTON'LOC+36+US'LOC+30+AU'LOC+49+MX'LOC+91+CBR'LOC+48+CBR'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'RFF+AAE:AAEETF6AL'MEA+TE+ADE+CEL::-6.50:-1.25'GIS+A::AQ:PHC'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'GIS+Y::AQ:DOC'GIS+::AQ:TAC'DOC+911+ABC123'DTM+137:20040315:102'DOC+916+ABC123'DTM+242:20171225:102'PNA+EX+99999'PNA+CN+++++10:CONSIGNEE JOE'ADR++5:63 PENSYLLVANIA AVE+WASHINGTON++US+:::DC'CTA+AG+:CONSIGNEE AGENT TEST'TDT+12+68+1+++++:::BLUEBOTTLE INDUSTRIES'DTM+136:20040420:102'PRC+IN:PP:AQ'DTM+318:20040410:102'PNA+FO+604'LIN+1'MEA+AAA+SQ+TNE:100.000'MEA+AAI+AAL+LTN:100'MEA+AAI+AAE+TNE:120'PIA+5+CAETBP  :CC'PIA+5+6130:BP'PIA+5+02011030:HS'IMD+++UHC:::CHILLED COD BRAIN'ATT+10++N:FCI:AQ'DTM+194:20040120:102'DTM+206:20040122:102'DTM+163:20110101:102'DTM+164:20110202:102'MOA+63:1000.00'PAC+1+3+CT::AQ'PCI++MARKS1'PRC+HA:PP:AQ'DTM+194:20031129:102'DTM+206:20031201:102'DTM+9:20031202:102'LOC+48+MYALL LAKES+123A'PNA+SK+604'PRC+PC:PP:AQ'DTM+194:20031222:102'DTM+206:20031222:102'PNA+MP+604'UNT+60+<<MSGNO PLACEHOLDER>>'";
		const string FishTransferMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+F::AQ:9++90'GIS+Y::AQ:CT'PNA+EX+99999'PNA+TT+12345'CTA+AG+98765'UNT+7+<<MSGNO PLACEHOLDER>>'";
		const string FishWithdrawlMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+F::AQ:9++1'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'PNA+EX+99999'UNT+5+<<MSGNO PLACEHOLDER>>'";
		const string FishCopyMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+F::AQ:9++31'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'PNA+EX+99999'UNT+5+<<MSGNO PLACEHOLDER>>'";
		const string FishAcceptedTransferInMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+F::AQ:9++92+AP'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'PNA+EX+99999'UNT+5+<<MSGNO PLACEHOLDER>>'";
		const string FishDeclinedTransferInMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+F::AQ:9++92+RE'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'PNA+EX+99999'UNT+5+<<MSGNO PLACEHOLDER>>'";
	}
}
