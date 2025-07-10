using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class RequestForPermitWoolMessageBuilderTest : RequestForPermitMessageBuilderAbstractTest
	{
		protected override RequestForPermitHeaderMessageBuilder GetMessageBuilder => new RequestForPermitWoolHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.LDG);

		protected override RequestForPermitHeaderMessageBuilder GetTransferMessageBuilder => new RequestForPermitWoolHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.TRF);

		protected override RequestForPermitHeaderMessageBuilder GetWithdrawlMessageBuilder => new RequestForPermitWoolHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.CAN);

		protected override RequestForPermitHeaderMessageBuilder GetCopyMessageBuilder => new RequestForPermitWoolHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.CPY);

		protected override RequestForPermitHeaderMessageBuilder GetAcceptTransferInMessageBuilder => new RequestForPermitWoolHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.AcceptTransferIn);

		protected override RequestForPermitHeaderMessageBuilder GetDeclinetTransferInMessageBuilder => new RequestForPermitWoolHeaderMessageBuilder(invoiceHeader, EXDOCMessageTypeCodes.Codes.DeclineTransferIn);

		protected override ZString ExpectedRFPMessage => WoolLodgeMessage;

		protected override ZString ExpectedTransferRFPMessage => WoolTransferMessage;

		protected override ZString ExpectedWithdrawlRFPMessage => WoolWithdrawlMessage;

		protected override ZString ExpectedCopyRFPMessage => WoolCopyMessage;

		protected override ZString ExpectedAcceptedTransferInRFPMessage => WoolAcceptedTransferInMessage;

		protected override ZString ExpectedDeclinedTransferInRFPMessage => WoolDeclinedTransferInMessage;

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			invoiceHeader = helper.Header1;
			helper.Declaration.JE_RL_NKPortOfLoading = "AUADL";
			helper.Declaration.JE_RL_NKPortOfArrival = "NZAKA";
			helper.Declaration.JE_RL_NKFinalDestination = "NZAKA";
			helper.Declaration.JE_MarksAndNumbers = "MARKS1";
			var notifyTextNote = helper.Declaration.Notes.AddNew();
			notifyTextNote.ST_Description = PredefinedNoteTypes.Instance.EXDOCNotifyText.Description;
			notifyTextNote.ST_NoteText = "NEDDY SEAGOON";
			var letterOfCreditInfoNote = helper.Declaration.Notes.AddNew();
			letterOfCreditInfoNote.ST_Description = PredefinedNoteTypes.Instance.EXDOCLetterOfCredit.Description;
			letterOfCreditInfoNote.ST_NoteText = "ATTN MR BANKER, BANK OF NEW ZEALAND";
			var quarantineHeader = invoiceHeader.QuarantineExDocHeader;
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Wool;
			quarantineHeader.QH_AQISRegion = "MEL";
			quarantineHeader.QH_CertificateRequiredLocation = "MEL";
			helper.Declaration.JE_DeclarationReference = "B00001004";
			quarantineHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.Manual;
			quarantineHeader.QH_TransfereeExporterNumber = "3432N";
			quarantineHeader.QH_TransfereeEDIUserIdentifier = "324FC";
			quarantineHeader.QH_DecOfCompliance = EXDOCYesNoEmpty.Codes.Yes;
			quarantineHeader.QH_ConsigneeAgentName = "Consignee Agent Test";
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "TSTSUP";
			var au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia);
			supplier.SetCustomsCode(OrgCusCode.AUQuarantineCodeTypes.EXDOCExporterNumber, au, "22112");
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "WOOLIMP";
			importer.OH_FullName = "WOOL IMPORTS";
			importer.MainAddress.OA_Address1 = "ROOM 123 RAFFLES HOTEL";
			importer.MainAddress.OA_City = "AUCKLAND";
			importer.MainAddress.OA_PostCode = "555";
			importer.OH_RL_NKClosestPort = "NZAKA";
			helper.Declaration.JE_OH_Importer = importer.PK;
			var forwarder = Factory.New<OrgHeader>();
			forwarder.OH_Code = "TSTFORD";
			forwarder.OH_FullName = "AKIRA KURUSAWA";
			helper.Declaration.JE_OH_Forwarder = forwarder.PK;
			helper.Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			helper.Declaration.JE_VoyageFlightNo = "123";
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "GOLDEN FLEECE SHIPPING";
			vessel.RV_LloydsNumber = "MV ARGO";
			helper.Declaration.JE_VesselName = vessel.RV_Code;
			helper.Declaration.JE_ExportDate = new ZDate(2004, 04, 06);
			var invoiceLine = helper.Line1;
			invoiceLine.JI_Tariff = "51011110";
			invoiceLine.JI_Description = "GREASY SHORN WOOL (INCL. FLEECE-WASHED WOOL), NOT CARDED OR COMBED, 19 UM AND FINER";
			var quarantineLine = invoiceLine.QuarantineExDocLine;
			quarantineLine.QL_NetQuantity = 1;
			quarantineLine.QL_NetQuantityUnit = EXDOCMetricWeightUnitCodes.Codes.Kilogram;
			quarantineLine.QL_GrossMetricWeight = 1;
			quarantineLine.QL_GrossMetricWeightUnit = EXDOCMetricWeightUnitCodes.Codes.Kilogram;
			quarantineLine.QL_PreservationType = EXDOCPreservationTypeCodes.Codes.Unrefrigerated;
			quarantineLine.QL_ProductType = "OW";
			quarantineLine.QL_PackType = EXDOCPacakgeTypeCodes.Codes.Bags;
			quarantineLine.QL_CutCode = "W00400";
			quarantineLine.QL_AqisCustomsWeight = 2000;
			quarantineLine.QL_OuterPackCount = 1;
			quarantineLine.QL_OuterPackType = EXDOCPacakgeTypeCodes.Codes.Cartons;
			quarantineLine.QL_OuterPackWeight = 1;
			quarantineLine.QL_OuterPackWeightUnit = EXDOCMetricWeightUnitCodes.Codes.Kilogram;
			quarantineLine.QL_OuterPackAccuracy = EXDOCPackAccuracyCodes.Codes.EqualTo;
			quarantineLine.QL_SendHCDesc = true;
			quarantineLine.QL_BatchCode = "BC123456";
			var container = Factory.New<CusContainer>();
			container.CO_ContainerNumber = "ABCD1234567";
			container.CO_Seal = "JH12345";
			helper.Declaration.CusContainers.Add(container);
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
		}
		JobComInvoiceHeader invoiceHeader;

		const string WoolLodgeMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+W::AQ:9++13'LOC+9+ADL'LOC+12+NZAKA'LOC+8+AKAROA'LOC+36+NZ'LOC+30+AU'LOC+91+MEL'LOC+48+MEL'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'FTX+AAG+++NEDDY SEAGOON'FTX+AAW+++ATTN MR BANKER, BANK OF NEW ZEALAND'GIS+M::AQ:PHC'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'PNA+EX+22112'PNA+CN+++++10:WOOL IMPORTS'ADR++5:ROOM 123 RAFFLES HOTEL+AUCKLAND+555+NZ+:::CAN'CTA+AG+:CONSIGNEE AGENT TEST'TDT+12+123+1+++++:::GOLDEN FLEECE SHIPPING'DTM+136:20040406:102'LIN+1'MEA+AAA+SQ+KGM:1.000'MEA+AAI+AAE+KGM:1'PIA+5+UOW BG  :CC'PIA+5+W00400:BP'PIA+5+51011110:HS'IMD+++UHC:::GREASY SHORN WOOL (INCL. FLEECE-WAS:HED WOOL), NOT CARDED OR COMBED, 19: UM AND FINER'ATT+10++N:FCI:AQ'PAC+1+3+CT::AQ'PCI++MARKS1'MEA+AAU+AAL:4+KGM:1.000'EQD+CN+ABCD1234567'SEL+JH12345'UNT+37+<<MSGNO PLACEHOLDER>>'";
		const string WoolWithdrawlMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+W::AQ:9++1'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'PNA+EX+22112'UNT+5+<<MSGNO PLACEHOLDER>>'";
		const string WoolTransferMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+W::AQ:9++90'GIS+N::AQ:CT'PNA+EX+22112'PNA+TT+3432N'CTA+AG+324FC'UNT+7+<<MSGNO PLACEHOLDER>>'";
		const string WoolCopyMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+W::AQ:9++31'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'PNA+EX+22112'UNT+5+<<MSGNO PLACEHOLDER>>'";
		const string WoolAcceptedTransferInMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+W::AQ:9++92+AP'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'PNA+EX+22112'UNT+5+<<MSGNO PLACEHOLDER>>'";
		const string WoolDeclinedTransferInMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+W::AQ:9++92+RE'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'PNA+EX+22112'UNT+5+<<MSGNO PLACEHOLDER>>'";
	}
}
