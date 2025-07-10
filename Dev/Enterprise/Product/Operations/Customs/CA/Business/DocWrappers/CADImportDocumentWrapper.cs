using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Messaging;
using Enterprise.DocumentEngineIntegration;

namespace Enterprise.Customs.CA.Business
{
	class CADImportDocumentWrapper : NonPersistentBusinessObject, ISourceIdentifierProvider
	{
		public CADImportDocumentWrapper(CADMessage message)
			: this(new CADAsLodgedDocumentWrapper(message), (message.EM_LinkedObject as CusEntryHeader)?.Declaration, FactoryExtensions.GetFactoryFromBusinessObject(message))
		{
		}

		public CADImportDocumentWrapper(CusEntryHeader entryHeader)
			: this(new CADCurrentDataDocumentWrapper(entryHeader), entryHeader.Declaration, FactoryExtensions.GetFactoryFromBusinessObject(entryHeader))
		{
		}

		internal CADImportDocumentWrapper(ICADHeader cadHeader, JobDeclaration declaration, BusinessObjectFactory factory)
			: base(factory)
		{
			this.cadHeader = cadHeader;
			this.declaration = declaration;
		}
		readonly ICADHeader cadHeader;
		readonly JobDeclaration declaration;

		public ZString TypeCode_Box1 => cadHeader.TypeCode_Box1;
		public ZString WsSType_Box2 => cadHeader.WsSType_Box2;
		public ZDateTime AccountingDate_Box3 => cadHeader.AccountingDate_Box3;
		public ZString TransactionNoFormated_Box4 => cadHeader.AccountSecurityCode_Box4 + cadHeader.CADTransactionNo_Box4;
		public ZString AccountSecurityCode_Box4 => cadHeader.AccountSecurityCode_Box4;
		public ZString CADTransactionNo_Box4 => cadHeader.CADTransactionNo_Box4;
		public ZString OfficeNo_Box5 => cadHeader.OfficeNo_Box5;
		public ZString ModeOfTransport_Box6 => cadHeader.ModeOfTransport_Box6;
		public ZDateTime ReleaseDate_Box7 => cadHeader.ReleaseDate_Box7;
		public ZDecimal GrossWeightKg_Box8 => cadHeader.GrossWeightKg_Box8;
		public ZString CarrierCodeAtImportation_Box9 => cadHeader.CarrierCodeAtImportation_Box9;
		public ZString Pre_CARM_Box10 => cadHeader.Pre_CARM_Box10;
		public ZString Z1_RPP => cadHeader.Z1_RPP;
		public ZString ImporterBN_Box11 => cadHeader.ImporterBN_Box11;
		public ZString ImporterDetails_Box12 => cadHeader.ImporterDetails_Box12;
		public ZString BrokerOrAgentBN_Box13 => cadHeader.BrokerOrAgentBN_Box13;
		public ZString BrokerOrAgentDetails_Box14 => cadHeader.BrokerOrAgentDetails_Box14;
		public ZString CargoControlNo_Box15 => cadHeader.CargoControlNo_Box15;
		public ZString RecordOfIntentNo_Box16 => cadHeader.RecordOfIntentNo_Box16;
		public ZString PreviousTransactionNo_Box17 => cadHeader.PreviousTransactionNo_Box17;
		public ZDateTime AcceptedDate_Box18 => cadHeader.AcceptedDate_Box18;
		public ZString OriginalTransactionNo_Box19 => cadHeader.OriginalTransactionNo_Box19;
		public ZString PrevTransNoWarehouse_Box20 => cadHeader.PrevTransNoWarehouse_Box20;
		public ZString PortOfUnlading_Box21 => cadHeader.PortOfUnlading_Box21;
		public ZString Notes_Box35 => cadHeader.Notes_Box35;
		public ZDecimal TotalValueForDuty_Box113 => cadHeader.TotalValueForDuty_Box113;
		public ZDecimal TotalPSTAndHST_Box114 => cadHeader.TotalPSTAndHST_Box114;
		public ZDecimal TotalPSTCannabisAmount_Box115 => cadHeader.TotalPSTCannabisAmount_Box115;
		public ZDecimal TotalProvAlcoholTaxAmount_Box116 => cadHeader.TotalProvAlcoholTaxAmount_Box116;
		public ZDecimal TotalProvTobaccoAmount_Box117 => cadHeader.TotalProvTobaccoAmount_Box117;
		public ZDecimal TotalDeclarationRelieved_Box118 => cadHeader.TotalDeclarationRelieved_Box118;
		public ZDecimal TotalAmount_Box119 => cadHeader.TotalAmount_Box119;
		public ZDecimal TotalCustomsDuties_Box120 => cadHeader.TotalCustomsDuties_Box120;
		public ZDecimal TotalExciseDuties_Box121 => cadHeader.TotalExciseDuties_Box121;
		public ZDecimal TotalExciseTaxes_Box122 => cadHeader.TotalExciseTaxes_Box122;
		public ZDecimal TotalGST_Box123 => cadHeader.TotalGST_Box123;
		public ZDecimal TotalAnti_Dumping_Box124 => cadHeader.TotalAnti_Dumping_Box124;
		public ZDecimal TotalCountervailing_Box125 => cadHeader.TotalCountervailing_Box125;
		public ZDecimal TotalSurtaxes_Box126 => cadHeader.TotalSurtaxes_Box126;
		public ZDecimal TotalSafeguards_Box127 => cadHeader.TotalSafeguards_Box127;
		public ZDecimal TotalInterest_Box128 => cadHeader.TotalInterest_Box128;
		public ZDecimal TotalDutiesAndTaxesWithInterest_Box129 => cadHeader.TotalDutiesAndTaxesWithInterest_Box129;
		public ZDecimal TotalDutiesAndTaxes_Box130 => cadHeader.TotalDutiesAndTaxes_Box130;
		public ZString BrokerNameAndPhone => Helper.GetCusAgentNameAndPhone(declaration) ?? Helper.GetCurrentUserNameAndPhone();
		public ZDateTime CurrentDate => ZDateTime.Now;
		public Image BrokerSignatureImage => declaration != null ? Helper.GetBroker(declaration)?.SignatureImage : null;

		public BusinessObjectCollectionWrapper<CADSubHeaderDocumentWrapper> CADSubHeaders => new BusinessObjectCollectionWrapper<CADSubHeaderDocumentWrapper>(cadHeader.CADSubHeaders.Select(x => new CADSubHeaderDocumentWrapper(x, Factory)));

		B3AndCADDocumentHelper Helper => helper ?? (helper = new B3AndCADDocumentHelper());
		B3AndCADDocumentHelper helper;

		public class CADSubHeaderDocumentWrapper : NonPersistentBusinessObject
		{
			public CADSubHeaderDocumentWrapper(ICADSubHeader cadSubHeader, BusinessObjectFactory factory)
				: base(factory)
			{
				this.cadSubHeader = cadSubHeader;
			}
			readonly ICADSubHeader cadSubHeader;

			public ZString VendorDetails_Box36 => cadSubHeader.VendorDetails_Box36;
			public ZString PurchaserDetails_Box37 => cadSubHeader.PurchaserDetails_Box37;
			public ZString InvoiceNo_Box38 => cadSubHeader.InvoiceNo_Box38;
			public ZDecimal InvoiceValue_Box39 => cadSubHeader.InvoiceValue_Box39;
			public ZString InvoiceCurrencyCode_Box40 => cadSubHeader.InvoiceCurrencyCode_Box40;
			public ZString PurchaseOrderNo_Box41 => cadSubHeader.PurchaseOrderNo_Box41;
			public ZDecimal FreightCharges_Box42 => cadSubHeader.FreightCharges_Box42;
			public ZString USPortOfExit_Box43 => cadSubHeader.USPortOfExit_Box43;
			public BusinessObjectCollectionWrapper<CADLineDocumentWrapper> CADLines => new BusinessObjectCollectionWrapper<CADLineDocumentWrapper>(cadSubHeader.CADLines.Select(x => new CADLineDocumentWrapper(x, Factory)));
		}

		public class CADLineDocumentWrapper : NonPersistentBusinessObject
		{
			public CADLineDocumentWrapper(ICADLine cadLine, BusinessObjectFactory factory)
				: base(factory)
			{
				this.cadLine = cadLine;
			}
			readonly ICADLine cadLine;

			public ZShort CADLineNo_Box56 => cadLine.CADLineNo_Box56;
			public ZString PreviousLineNoWarehouse_Box57 => cadLine.PreviousLineNoWarehouse_Box57;
			public ZString ClassificationNo_Box58 => TariffFormatter.DisplayFormat(cadLine.ClassificationNo_Box58);
			public ZString ClassificationDescription_Box59 => cadLine.ClassificationDescription_Box59;
			public ZString NarrativeDescription_Box60 => cadLine.NarrativeDescription_Box60;
			public ZDecimal Quantity_Box61 => cadLine.Quantity_Box61;
			public ZString UnitOfMeasure_Box62 => cadLine.UnitOfMeasure_Box62;
			public ZString TimeLimitType_Box63 => cadLine.TimeLimitType_Box63;
			public ZDateTime ExtensionDate_Box64 => cadLine.ExtensionDate_Box64;
			public ZString CountryOfOrigin_Box65 => cadLine.CountryOfOrigin_Box65;
			public ZString USState_Box66 => cadLine.USState_Box66;
			public ZString PlaceOfExport_Box67 => cadLine.PlaceOfExport_Box67;
			public ZString PlaceOfExportCodeState_Box68 => cadLine.PlaceOfExportCodeState_Box68;
			public ZDateTime DirectShipmentDate_Box69 => cadLine.DirectShipmentDate_Box69;
			public ZString TariffTreatment_Box70 => cadLine.TariffTreatment_Box70;
			public ZString TariffCode_Box71 => cadLine.TariffCode_Box71;
			public ZString TimeLimitFrom_Box72 => cadLine.TimeLimitFrom_Box72;
			public ZString TimeLimitTo_Box73 => cadLine.TimeLimitTo_Box73;
			public ZString DestinationProvince_Box74 => cadLine.DestinationProvince_Box74;
			public ZDecimal ValueForCurrencyConversion_Box75 => cadLine.ValueForCurrencyConversion_Box75;
			public ZString Currency_Box76 => cadLine.Currency_Box76;
			public ZDecimal ExchangeRate_Box77 => cadLine.ExchangeRate_Box77;
			public ZDecimal ValueForDuty_Box78 => cadLine.ValueForDuty_Box78;
			public ZString DRPLicense_Box79 => cadLine.DRPLicense_Box79;
			public ZString SpecialAuthOIC_Box80 => cadLine.SpecialAuthOIC_Box80;
			public ZString SpecialAuthorityPermit_Box81 => cadLine.SpecialAuthorityPermit_Box81;
			public ZDecimal CustomsDuty_Box82 => cadLine.CustomsDuty_Box82;
			public ZDecimal ExciseTax_Box83 => cadLine.ExciseTax_Box83;
			public ZDecimal ExciseDuty_Box84 => cadLine.ExciseDuty_Box84;
			public ZDecimal Surtax_Box85 => cadLine.Surtax_Box85;
			public ZDecimal Anti_Dumping_Box86 => cadLine.Anti_Dumping_Box86;
			public ZDecimal Safeguard_Box87 => cadLine.Safeguard_Box87;
			public ZDecimal Countervailing_Box88 => cadLine.Countervailing_Box88;
			public ZDecimal ValueForTax_Box89 => cadLine.ValueForTax_Box89;
			public ZDecimal GST_Box90 => cadLine.GST_Box90;
			public ZDecimal PSTAndHSTAmount_Box91 => cadLine.PSTAndHSTAmount_Box91;
			public ZDecimal ProvincialAlcoholTax_Box92 => cadLine.ProvincialAlcoholTax_Box92;
			public ZDecimal ProvincialTobaccoAmount_Box93 => cadLine.ProvincialTobaccoAmount_Box93;
			public ZDecimal AlcohosPercent_Box94 => cadLine.AlcohosPercent_Box94;
			public ZDecimal ProvincialCannabisExciseDuty_Box95 => cadLine.ProvincialCannabisExciseDuty_Box95;
			public ZString CBSACaseNo_Box96 => cadLine.CBSACaseNo_Box96;
			public ZString RulingNo_Box97 => cadLine.RulingNo_Box97;
			public ZString AppealsCaseNo_Box98 => cadLine.AppealsCaseNo_Box98;
			public ZString ComplianceCaseNo_Box99 => cadLine.ComplianceCaseNo_Box99;
			public ZDecimal LineTotalDutiesAndTaxes_Box100 => cadLine.LineTotalDutiesAndTaxes_Box100;
			public ZString CommodityReason1_Box101 => cadLine.CommodityReason1_Box101;
			public ZString Authority1_Box102 => cadLine.Authority1_Box102;
			public ZString CommodityRemark1_Box103 => cadLine.CommodityRemark1_Box103;
			public ZString CommodityReason2_Box105 => cadLine.CommodityReason2_Box105;
			public ZString Authority2_Box106 => cadLine.Authority2_Box106;
			public ZString CommodityRemark2_Box107 => cadLine.CommodityRemark2_Box107;
			public ZString CommodityReason3_Box109 => cadLine.CommodityReason3_Box109;
			public ZString Authority3_Box110 => cadLine.Authority3_Box110;
			public ZString CommodityRemark3_Box111 => cadLine.CommodityRemark3_Box111;

			TariffFormatter TariffFormatter => new TariffFormatter();
		}

		ZGuid ISourceIdentifierProvider.SourceIdentifier => declaration.PK;
	}
}
