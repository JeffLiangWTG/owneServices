using CargoWise.Types;

namespace Enterprise.Customs.CA.Messaging
{
	public interface ICADLine
	{
		ZShort CADLineNo_Box56 { get; }
		ZString PreviousLineNoWarehouse_Box57 { get; }
		ZString ClassificationNo_Box58 { get; }
		ZString ClassificationDescription_Box59 { get; }
		ZString NarrativeDescription_Box60 { get; }
		ZDecimal Quantity_Box61 { get; }
		ZString UnitOfMeasure_Box62 { get; }
		ZString TimeLimitType_Box63 { get; }
		ZDateTime ExtensionDate_Box64 { get; }
		ZString CountryOfOrigin_Box65 { get; }
		ZString USState_Box66 { get; }
		ZString PlaceOfExport_Box67 { get; }
		ZString PlaceOfExportCodeState_Box68 { get; }
		ZDateTime DirectShipmentDate_Box69 { get; }
		ZString TariffTreatment_Box70 { get; }
		ZString TariffCode_Box71 { get; }
		ZString TimeLimitFrom_Box72 { get; }
		ZString TimeLimitTo_Box73 { get; }
		ZString DestinationProvince_Box74 { get; }
		ZDecimal ValueForCurrencyConversion_Box75 { get; }
		ZString Currency_Box76 { get; }
		ZDecimal ExchangeRate_Box77 { get; }
		ZDecimal ValueForDuty_Box78 { get; }
		ZString DRPLicense_Box79 { get; }
		ZString SpecialAuthOIC_Box80 { get; }
		ZString SpecialAuthorityPermit_Box81 { get; }
		ZDecimal CustomsDuty_Box82 { get; }
		ZDecimal ExciseTax_Box83 { get; }
		ZDecimal ExciseDuty_Box84 { get; }
		ZDecimal Surtax_Box85 { get; }
		ZDecimal Anti_Dumping_Box86 { get; }
		ZDecimal Safeguard_Box87 { get; }
		ZDecimal Countervailing_Box88 { get; }
		ZDecimal ValueForTax_Box89 { get; }
		ZDecimal GST_Box90 { get; }
		ZDecimal PSTAndHSTAmount_Box91 { get; }
		ZDecimal ProvincialAlcoholTax_Box92 { get; }
		ZDecimal ProvincialTobaccoAmount_Box93 { get; }
		ZDecimal AlcohosPercent_Box94 { get; }
		ZDecimal ProvincialCannabisExciseDuty_Box95 { get; }
		ZString CBSACaseNo_Box96 { get; }
		ZString RulingNo_Box97 { get; }
		ZString AppealsCaseNo_Box98 { get; }
		ZString ComplianceCaseNo_Box99 { get; }
		ZDecimal LineTotalDutiesAndTaxes_Box100 { get; }
		ZString CommodityReason1_Box101 { get; }
		ZString Authority1_Box102 { get; }
		ZString CommodityRemark1_Box103 { get; }
		ZString CommodityReason2_Box105 { get; }
		ZString Authority2_Box106 { get; }
		ZString CommodityRemark2_Box107 { get; }
		ZString CommodityReason3_Box109 { get; }
		ZString Authority3_Box110 { get; }
		ZString CommodityRemark3_Box111 { get; }
	}
}
