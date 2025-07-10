using System.Collections.Immutable;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public static class UniversalReferenceConstants
{
	public static class Prefixes
	{
		public const string PortTaxReference = "--";
	}

	public static class AdditionalInfoSpecialCodes
	{
		public const string DG0 = "DG0";
		public const string DG1 = "DG1";
	}

	public static class RefCusProcedureCodes
	{
		public const string IntoTemporaryImport = "5";
		public const string IntoTemporaryExport = "2";
		public const string FinalExport10 = "10";
		public const string CompensatingProductsExport11 = "11";
		public const string TemporaryGoodsExportToBeReintegratedAsTheyAre23 = "23";
		public const string GoodsReExport31 = "31";
		public const string Transit = "80";
		public const string ReimportWarehouseDutiesExempt6171F01 = "6171F01";
		public const string ReleaseForFreeCirculationOfGoodsForConsumption42 = "42";
		public const string ReimportationWithSimultaneousReleaseForConsumption63 = "63";
		public const string PlacingOfGoodsIntoCustomsWarehousingOrIntoFreeZone76 = "76";
		public const string StoringAndPlacingOfGoodsUnderCustomsControlWithPreFinancing77 = "77";
		public const string TemporaryExportUnderOutwardProcessingRegime21 = "21";
		public const string TemporaryExportUnderOutwardProcessingOnTextileProducts22 = "22";

		public static class SpecialUse
		{
			public const string NoPreviousProcedure4400 = "4400";
			public const string InwardProcedure4451 = "4451";
			public const string TemporaryImport4453 = "4453";
			public const string Storage4471 = "4471";
		}
	}

	public static class RefCusPreferences
	{
		public const string NonImpositionOfCustomsDuties = "400";
	}

	public static class RefCusProcedureAttributeNames
	{
		public const string DutyPaymentMethod = "DTYPaymentMethod";
		public const string VatPaymentMethod = "VATPaymentMethod";
		public const string OtherFeePaymentMethod = "OTHPaymentMethod";
	}

	public static class SupportingDocumentTypes
	{
		public const string FeeCalculationThirdUomCertificate = "10YY";
		public const string CustomsDecisionAuthorization = "60YY";
		public const string Y022 = "Y022";
		public const string Y023 = "Y023";
		public const string Y024 = "Y024";
		public const string Y040 = "Y040";
		public const string Y041 = "Y041";
		public const string Y042 = "Y042";
		public const string C019 = "C019";
		public const string C100 = "C100";
		public const string U164 = "U164";
		public const string U165 = "U165";
		public const string U166 = "U166";
		public const string C517 = "C517";
		public const string C518 = "C518";
		public const string C519 = "C519";
		public const string C601 = "C601";
		public const string DeclarationOfIntent = "01DI";
		public const string TIRCarnet = "N952";
		public const string N990 = "N990";
		public const string ExportDeclarationMRNtoUnload32YY = "32YY";
		public const string C651 = "C651";
		public const string C658 = "C658";
		public const string PortTax = "39YY";
		public const string ATRCertificateN018 = "N018";

		public static readonly ImmutableArray<ZString> RecommendedSupportingDocumentCodesForPreference200 = new ZString[] { C100, U164, U165, U166 }.ToImmutableArray();

		public static readonly ImmutableDictionary<string, string> AuthorizationTypeSupportingDocumentTypeCorrelation = new (string AuthorizationType, string DocumentType)[]
		{
			(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, C517),
			(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, C518),
			(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, C519),
		}.ToImmutableDictionary(x => x.AuthorizationType, x => x.DocumentType);
	}

	public static class RefCusRateCodes
	{
		public const string DutyChargeTypeStartingCode = "A";
		public const string DutyForSanMarinoChargeTypeStartingCode = "27";
		public const string VatChargeTypeStartingCodeB = "B";
		public const string VatChargeTypeStartingCode4 = "4";

		public const string ItalianCustomsVatCode = "405";
		public const string ItalianCustomsVatExemptionCode406 = "406";
		public const string ItalianCustomsVatExemptionCode407 = "407";
		public const string CarTax423 = "423";
		public const string MiscellaneousContingentRevenueConcerningTax430 = "430";
		public const string RecoveryOfCourtCostsTax445 = "445";

		public const string TemporaryAntiDumpingDuty = "A35";
		public const string TemportaryCountervailingDuty = "A45";
	}

	public static class RefHarbourRateTypeCode
	{
		public const string Taxes = "TAX";
	}

	public static class DutyMethodOfPayment
	{
		public const string ImmediatePaymentInCashA = "A";
		public const string DeferredPaymentE = "E";
		public const string DeferredPaymentCustomsProcedureF = "F";
		public const string DeferredPaymentVatProcedureG = "G";
		public const string SecurityDepositDeferredPaymentR = "R";
		public const string AgentGeneralGuaranteeAccountT = "T";
		public const string OthersD = "D";
		public const string GuaranteeAtTheInterventionBodyO = "O";
		public const string IndividualGuaranteeS = "S";
		public const string GuaranteeAccountInterestedPartyPermanentAuthorizationU = "U";
		public const string GuaranteeAccountInterestedPartyIndividualAuthorizationV = "V";
	}

	public static class AgreedPlaceCodes
	{
		public const string AgreedPlaceThisMemberState = "1";
		public const string AgreedPlaceAnotherMemberState = "2";
		public const string AgreedPlaceOutsideUnion = "3";
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant Country, Quantity, Year, Complement")]
	public static class RefCusCodeListAttributeName
	{
		public const string Country = "Country";
		public const string Quantity = "Quantity";
		public const string ReferenceNumber = "ReferenceNumber";
		public const string UnitOfQuantity = "UnitOfQuantity";
		public const string Year = "Year";
		public const string Complement = "Complement";
		public const string ItemNumber = "ItemNumber";
	}

	public static class RefCusCodeListAttributeValues
	{
		public const string Yes = "Y";
	}

	public static class RefCusCodeListTypes
	{
		public const string HarbourCommodityCode = "HCOMM";
		public const string SpecificCircumstanceIndicatorCode = "C296E";
		public const string TemporaryStorageDeclaration = "N337";
		public const string DeclarationOrNotificationMrn = "NMRN";
	}

	public static class RefCusRateTypes
	{
		public const string MiscellaneousImportExport = "MIE";
		public const string MiscellaneousNotVatableImportExport = "MNB";
		public const string MiscellaneousNotVatable = "MNV";
	}

	public const decimal CustomsValueInEuroTresholdForOriginDeclaration = 6000;

	public static class CommonResStrings
	{
		public static string OfficeOfPresentationForCentralizedClearance => Res.GetString("E9972D3A-76D5-4C9A-AFD5-D96E33D1CCCA", "Office of Presentation for Centralized Clearance");
		public static string SupervisingCustomsOffice => Res.GetString("7FBB66A5-5C14-4813-BD9A-E2DFF3B2F0C9", "Supervising Customs Office");
	}

	public static class AllowedAdditionalCodes
	{
		public const string _2 = "2";
		public const string _3 = "3";
		public const string _4 = "4";
		public const string _6 = "6";
		public const string _8 = "8";
		public const string A = "A";
		public const string B = "B";
		public const string C = "C";
		public const string D = "D";
		public const string P = "P";
	}

	public static class AllowedNationalAdditionalCodes
	{
		public const string Q = "Q";
		public const string R = "R";
		public const string S = "S";
		public const string T = "T";
		public const string U = "U";
		public const string Z = "Z";
	}

	public static class RefSysConfigTypes
	{
		public const string ItalyAutomaticRemoteDigitalSignatureDelegate01 = "ITARDSDL01";
		public const string ItalyAutomaticRemoteDigitalSignatureDelegate02 = "ITARDSDL02";
	}
}
