using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business;

#region SuppressResourceStringsCheckRegion

public static class UniversalReferenceConstants
{
	public static class RefCusCodeList
	{
		public const string EntryStatus = "CSTA";

		public static class PassarTypes
		{
			public const string NCTSDeclarationType = "N0231";
			public const string NCTSBondType = "N0251";
			public const string N0252 = "N0252";
			public const string N1053 = "N1053";
			public const string N1054 = "N1054";
			public const string N1057 = "N1057";
			public const string N1119 = "N1119";
			public const string N1121 = "N1121";
			public const string N1141 = "N1141";
			public const string N1150 = "N1150";
			public const string N0296 = "N0296";
			public const string N2000 = "N2000";
			public const string N3000 = "N3000";
			public const string InputControl = "N1113";
			public const string NextProcedure = "N1123";
			public const string Restrictions = "PRMAP";
			public const string EUCountries = "CL010";
			public const string TBSGA = "TBSGA";
			public const string TBSGB = "TBSGB";
			public const string TBSGC = "TBSGC";
			public const string TBSGD = "TBSGD";
			public const string TBSGE = "TBSGE";
			public const string TBMG = "TBMG";
		}

		public static class EdecTypes
		{
			public const string CITESCommodityType = "CITCT";
			public const string CITESScientificName = "CITSN";
			public const string VehiclesMarks = "VEHMC";
			public const string PermitObligation = "PRMOC";
			public const string NonCustomsLawObligation = "NCLOC";
			public const string StorageType = "STGCD";
			public const string DeclarationReason = "PREAS";
			public const string SeletionResult = "SELRE";
			public const string DirectTransportationCountry = "COUDT";
			public const string CorrectionReason = "CREAS";
			public const string TobaccoMainGroup = "TBMG";
			public const string TobaccoSubGroupCigars = "TBSGA";
			public const string TobaccoSubGroupCigarettes = "TBSGB";
			public const string TobaccoSubGroupTobacco = "TBSGC";
			public const string TobaccoSubGroupAssortment = "TBSGD";
			public const string TobaccoSubGroupECigarettes = "TBSGE";
			public const string TobaccoBrand = "TBBND";
			public const string ExportCodeMineralOil = "EXCMO";
			public const string ClearanceLocation = "CLLOC";
			public const string PermitItemDetailsKey = "PRMKY";
			public const string WarehouseType = "WHSTY";
			public const string SpecificCircumstanceIndicator = "SPECI";
			public const string AdditionalCodes = "ADDCD";
			public const string DocumentType = "EBDTY";
			public const string EComplaintFields = "ECFLD";
			public const string RefundType = "RFNDT";
			public const string Direction = "DIR";
			public const string RefinementType = "REFTY";
			public const string ProcessType = "PROTY";
			public const string BillingType = "BILTY";
			public const string CustomsOffice = "CUSCH";
			public const string TransportDocumentType = "TD44N";
			public const string ECICS = "ECICS";
		}

		public static class Attributes
		{
			public const string IsImports = "IsImports";
			public const string IsExports = "IsExports";
			public const string GSPCertificate = "GSPCertificate";
			public const string OriginDocument = "OriginDocument";
			public const string Reference = "Reference";
			public const string IssuingDate = "IssuingDate";
			public const string IsHeader = "IsHeader";
			public const string RestrictionCode = "RestrictionCode";
			public const string Level = "Level";
			public const string PermitAuthority = "PermitAuthority";
			public const string LinkedCodeType = "LinkedCodeType";
			public const string PermitNumberAllowed = "PermitNumberAllowed";
			public const string PermitExceptionReasonAllowed = "PermitExceptionReasonAllowed";
			public const string AdditionalInformation = "AdditionalInformation";
		}

		public static class AttributeValues
		{
			public const string Yes = "Y";
			public const string No = "N";
			public const string Item = "Item";
			public const string Header = "Header";
		}
	}

	public static class PermitCodes
	{
		public const string ReversTobacco = "4";
		public const string Commitment = "6";
		public const string PeriodicTax = "7";
		public const string ObligationMineralOilTax = "8";
		public const string SingleEPermit = "11";
		public const string GeneralEPermit = "12";
		public const string Other = "999";

		public static bool IsEPermit(string code) => code == SingleEPermit || code == GeneralEPermit;
	}

	public static class PermitObligationCodes
	{
		public const string NoPermit = "0";
		public const string PermitNeeded = "1";
		public const string NoPermitNeeded = "2";
	}

	public static class TransportationTypeCodes
	{
		public const string Truck = "2";
	}

	public static class TariffTypes
	{
		public const string ImportTariff = "IMP";
		public const string ExportTariff = "EXP";
		public const string AdditionalTaxesTariff = "ADT";
	}

	public static class RateTypes
	{
		public const string Duties = "DTY";
		public const string AdditionalTaxes = "ADT";
		public const string AdditionalFees = "FEE";
	}

	public static class FeeRateCodes
	{
		public const string CustomsReliefControlTax = "995";
	}

	public static class TariffAttributes
	{
		public const string TareSupplement = "tareSupplement";
		public const string QuantityCode1 = "quantityCode1";
		public const string NetMassOptional = "netMassOptional";
		public const string StorageType = "storageType";
		public const string HasOptionalPermit = "hasOptionalPermit";
		public const string HasOptionalNCL = "hasOptionalNCL";
		public const string CustomsFavourHintCode = "customsFavourHintCode";
		public const string SensibleGoodsCode = "sensibleGoodsCode";
		public const string AssessmentCode = "assessmentCode";

		public static class Values
		{
			public const string _0 = "0";
			public const string _1 = "1";
			public const string _2 = "2";
			public const string _3 = "3";
			public const string _4 = "4";
			public const string _5 = "5";
			public const string Yes = "Y";
			public const string No = "N";
		}
	}

	public static class CusConditionType
	{
		public const string WeightCheck1 = "WGTC1";
		public const string WeightCheck2 = "WGTC2";
		public const string MeanValueCheck = "MVC";
		public const string FederalOfficeForAgriculture = "PA1";
		public const string PermitAuthorityPrefix = "PA";
		public const string NonCustomsLawPrefix = "N";
		public const string PlantHealth = "N270";
	}

	public static class CusConditionValueType
	{
		public const string Permit = "PRM";
		public const string NonCustomsLaw = "NCL";
		public const string Restriction = "RST";
	}

	public static class FormulaPlaceholder
	{
		public const string StatisticalValue = "VFS";
		public const string DutyCalculation = "DTY";
		public const string FreeRateFormula = "0";
		public const string NetWeightUOMPlaceHolder = "[" + SwissCustomsConstants.MeasurementUnits.NetWeightUOM + "]";
		public const string GrossWeightUOMPlaceHolder = "[" + SwissCustomsConstants.MeasurementUnits.GrossWeightUOM + "]";
	}

	public static class Tariffs
	{
		public const string BeginOfIndustrialTariffs = "25000000";
		public const string NegligibleImportTariff = "99999999000000";
		public const string NegligibleExportTariff = "99999999000";
	}

	public static class TariffChapters
	{
		public const string Tobaccos = "24";
	}

	public static class TariffNumbers
	{
		public const string ForIndustrialManufacture = "24011010";
		public const string ForIndustrialManufacturePartlyStemmed = "24012010";
		public const string ForIndustrialManufactureTobaccoRefuse = "24013010";
		public const string ProductsContainingTobaccoOther = "24041290";
		public const string CigarCherootsCigarillosContainingTobacco = "24021000";
		public const string CigarettesContainingTobaccoMoreThan = "24022010";
		public const string CigarettesContainingTobaccoNotMoreThan = "24022020";
		public const string CigarCherootsCigarillosOthers = "24029000";
		public const string WaterPipeTobaccoSpecifiedInSubheading = "24031100";
		public const string SmokingTobaccoOther = "24031900";
		public const string HomogenisedTobacco = "24039100";
		public const string ChewingTobaccoRollTobaccoAndSnuff = "24039910";
		public const string ExpandedTobacco = "24039940";
		public const string OtherManufacturedTobaccoOtherOther = "24039990";
		public const string SmokingOthersTobacco = "24031900";
		public const string GraphiteInPowderOrFlakes = "25041000";

		public static bool IsCigarsTariffCode(string code)
		{
			switch (code)
			{
				case CigarCherootsCigarillosContainingTobacco:
				case CigarettesContainingTobaccoMoreThan:
				case CigarettesContainingTobaccoNotMoreThan:
				case CigarCherootsCigarillosOthers:
				case WaterPipeTobaccoSpecifiedInSubheading:
				case SmokingTobaccoOther:
				case ChewingTobaccoRollTobaccoAndSnuff:
				case OtherManufacturedTobaccoOtherOther:
					return true;
				default:
					return false;
			}
		}

		public static bool IsIndustrialManufactureTariffCode(string code)
		{
			switch (code)
			{
				case ForIndustrialManufacture:
				case ForIndustrialManufacturePartlyStemmed:
				case ForIndustrialManufactureTobaccoRefuse:
					return true;
				default:
					return false;
			}
		}

		public static bool IsChewingRollingOtherTobacco(string code)
		{
			switch (code)
			{
				case HomogenisedTobacco:
				case ExpandedTobacco:
					return true;
				default:
					return false;
			}
		}

		public static bool IsTariffNumberForNumOfElementsBasedTaxation(string tariffNumber)
		{
			switch (tariffNumber)
			{
				case CigarCherootsCigarillosContainingTobacco:
				case CigarettesContainingTobaccoMoreThan:
				case CigarettesContainingTobaccoNotMoreThan:
				case CigarCherootsCigarillosOthers:
					return true;
				default:
					return false;
			}
		}

		public static bool IsTariffNumberForWeightBasedTaxation(string tariffNumber)
		{
			switch (tariffNumber)
			{
				case WaterPipeTobaccoSpecifiedInSubheading:
				case SmokingTobaccoOther:
				case ChewingTobaccoRollTobaccoAndSnuff:
				case OtherManufacturedTobaccoOtherOther:
					return true;
				default:
					return false;
			}
		}

		public static string[] TobaccoQuantityBasedTaxationTariffNumbers => new string[] { TariffNumbers.CigarCherootsCigarillosContainingTobacco, TariffNumbers.CigarettesContainingTobaccoMoreThan, TariffNumbers.CigarettesContainingTobaccoNotMoreThan, TariffNumbers.CigarCherootsCigarillosOthers, TariffNumbers.WaterPipeTobaccoSpecifiedInSubheading, TariffNumbers.SmokingTobaccoOther, TariffNumbers.ChewingTobaccoRollTobaccoAndSnuff, TariffNumbers.OtherManufacturedTobaccoOtherOther };

		public static bool IsTariffNumberForQuantityBasedTaxation(string tariffNumber) => IsTariffNumberForNumOfElementsBasedTaxation(tariffNumber) || IsTariffNumberForWeightBasedTaxation(tariffNumber);

		public static bool IsTariffNumberForCigarettes(string tariffNumber)
		{
			switch (tariffNumber)
			{
				case CigarettesContainingTobaccoMoreThan:
				case CigarettesContainingTobaccoNotMoreThan:
				case CigarCherootsCigarillosOthers:
					return true;
				default:
					return false;
			}
		}

		public static bool IsTariffNumberForOtherTobaccoProducts(string tariffNumber)
		{
			switch (tariffNumber)
			{
				case WaterPipeTobaccoSpecifiedInSubheading:
				case SmokingTobaccoOther:
				case ChewingTobaccoRollTobaccoAndSnuff:
					return true;
				default:
					return false;
			}
		}
	}

	public static class ImportTariffCodeGroups
	{
		public const string Cigarets = "2402";
		public const string SmokingTobacco = "2403";
		public const string ProductsContainingTobacco = "2404";
	}

	public static class TariffStatisticalCodes
	{
		public const string TobaccoPrivateGoodsNoMoreOf10kgOr1000CHF = "911";
		public const string StatisticalCodeOther = "999";
		public const string SnuffCigars = "11";
		public const string SnuffCigarettes = "12";
		public const string SnuffSmokingTobacco = "13";

		public static bool IsSnuffTobaccoStatisticalCode(string code) => code == SnuffCigars || code == SnuffCigarettes || code == SnuffSmokingTobacco;
	}

	public static class AdditionalTaxesTariffs
	{
		public const string Tariff280200 = "280-200";
		public const string Tariff700002 = "700-002";
		public const string Tariff450001 = "450-001";
		public const string Tariff450201 = "450-201";
		public const string Tariff450002 = "450-002";
		public const string Tariff450202 = "450-202";
		public const string Tariff465001 = "465-001";
		public const string Tariff465201 = "465-201";
		public const string Tariff465002 = "465-002";
		public const string Tariff465202 = "465-202";
		public const string Tariff470001 = "470-001";
		public const string Tariff470002 = "470-002";
		public const string Tariff470201 = "470-201";
		public const string Tariff470202 = "470-202";

		public const string TariffType_QuantityBased = "450";

		public static bool IsTobaccoTariff450002Or450202(string code)
		{
			switch (code)
			{
				case Tariff450002:
				case Tariff450202:
					return true;
				default:
					return false;
			}
		}

		public static bool IsTobaccoTariff450001Or450201(string code)
		{
			switch (code)
			{
				case Tariff450001:
				case Tariff450201:
					return true;
				default:
					return false;
			}
		}

		public static bool IsSOTATariff465001Or465201(string code)
		{
			switch (code)
			{
				case Tariff465001:
				case Tariff465201:
					return true;
				default:
					return false;
			}
		}

		public static bool IsSOTATariff465002Or465202(string code)
		{
			switch (code)
			{
				case Tariff465002:
				case Tariff465202:
					return true;
				default:
					return false;
			}
		}

		public static bool IsTobaccoTariff465001Or465201(string code)
		{
			switch (code)
			{
				case Tariff465001:
				case Tariff465201:
					return true;
				default:
					return false;
			}
		}

		public static bool IsTobaccoTariff465002Or465202(string code)
		{
			switch (code)
			{
				case Tariff465002:
				case Tariff465202:
					return true;
				default:
					return false;
			}
		}

		public static bool IsTobaccoTariff470001Or470201(string code)
		{
			switch (code)
			{
				case Tariff470001:
				case Tariff470201:
					return true;
				default:
					return false;
			}
		}

		public static bool IsTobaccoTariff470002Or470202(string code)
		{
			switch (code)
			{
				case Tariff470002:
				case Tariff470202:
					return true;
				default:
					return false;
			}
		}

		public static bool IsTobaccoTariffCode450(string code) => code.StartsWith("450");
		public static bool IsTobaccoTariffCode470(string code) => code.StartsWith("470");

		public const string TariffKey_011 = "011";
		public const string TariffKey_012 = "012";
		public const string TariffKey_013 = "013";
		public const string TariffKey_014 = "014";
		public const string TariffKey_015 = "015";
		public const string TariffKey_016 = "016";
		public const string TariffKey_017 = "017";

		public static bool IsTariffKeyForGrossMassBasedTaxation(string tariffKey)
		{
			switch (tariffKey)
			{
				case TariffKey_011:
				case TariffKey_012:
				case TariffKey_013:
				case TariffKey_014:
				case TariffKey_015:
				case TariffKey_016:
				case TariffKey_017:
					return true;
				default:
					return false;
			}
		}

		public static bool IsForcedManualRate(string code) => code.StartsWith("450-00") || code.StartsWith("450-20");
	}

	public static class AdditionalTaxesTypes
	{
		public const string Duty = "110";
		public const string VeterinaryInspection = "290";
		public const string CitesFauna = "292";
		public const string CitesFlora = "792";
		public const string Spirits = "280";
		public const string Tobacco = "450";
		public const string Banderole = "460";
		public const string SOTA = "465";
		public const string TobaccoPreventionFund = "470";
		public const string Beer = "480";
		public const string MotorVehicle = "660";
		public const string PrepaidDisposal = "970";
		public const string OtherTaxesOrFees = "150";
		public const string MineralOilGasoline = "600";
		public const string MineralOilFuelsAndOthers = "640";
		public const string CO2CoalAndCoke = "743";
		public const string _710 = "710";
		public const string _720 = "720";
		public const string _730 = "730";
		public const string CO2HeatingOil = "740";

		public static IReadOnlyCollection<string> ExcludedTaxTypes => new string[] { Tobacco, Banderole, SOTA, TobaccoPreventionFund, Beer, PrepaidDisposal };

		public static bool IsAdditionalTaxTypeForMineralOilProducts(string taxType)
		{
			switch (taxType)
			{
				case MineralOilGasoline:
				case MineralOilFuelsAndOthers:
				case CO2CoalAndCoke:
					return true;
				default:
					return false;
			}
		}

		public static bool IsAdditionalTaxTypeForFuels(string taxType)
		{
			switch (taxType)
			{
				case MineralOilGasoline:
				case MineralOilFuelsAndOthers:
				case _710:
				case _720:
				case _730:
				case CO2HeatingOil:
				case CO2CoalAndCoke:
					return true;
				default:
					return false;
			}
		}

		public static bool IsAdditionalTaxTypeForSpirits(string taxType)
		{
			switch (taxType)
			{
				case Spirits:
					return true;
				default:
					return false;
			}
		}
	}

	public static class AdditionTaxesControlOffices
	{
		public const string ControlOffice_Cites01 = "CITES01";
		public const string ControlOffice_Cites02 = "CITES02";
		public const string ControlOffice_Cites03 = "CITES03";
		public const string ControlOffice_Cites04 = "CITES04";
		public const string ControlOffice_Cites05 = "CITES05";
		public const string ControlOffice_Cites07 = "CITES07";

		public static IReadOnlyCollection<string> CitesControlOffices => new string[] { ControlOffice_Cites01, ControlOffice_Cites02, ControlOffice_Cites03, ControlOffice_Cites04, ControlOffice_Cites05, ControlOffice_Cites07 };
	}

	public static class AdditionalInformationTypeCodes
	{
		public const string WarehouseNumber = "26";
		public const string ExportCodeMineralOil = "27";
		public const string FreeZoneTraffic = "28";
		public const string BorderZoneTraffic = "29";
		public const string VolAlcohol = "A1101";
		public const string LitresAlcohol = "A1102";
		public const string PartialShipmentNumber = "V1201";
		public const string ReferenceOfTheFristPartShipment = "V1202";
		public const string VocQuantityInKilograms = "A1301";
		public const string AlcoholOnBeerRefundLiters = "A1102";
		public const string A1401 = "A1401";
		public const string ProductMainGroup = "A1402";
		public const string ProductSubgroup = "A1403";
		public const string A1404 = "A1404";
		public const string A1405 = "A1405";
		public const string A1406 = "A1406";
		public const string VehicleIdentificationNumber = "W1101";
		public const string VehicleMatriculationNumber = "W1102";
		public const string VehicleBrand = "W1103";
	}

	public static class InAndOutwardProcessingDirectionCodes
	{
		public const string Active = "1";
		public const string Passive = "2";
	}

	public static class InAndOutwardProcessingStatusCodes
	{
		public const string RepairFalse = "0";
		public const string RepairTrue = "1";
	}

	public static class InAndOutwardProcessingProcessTypesEdec
	{
		public const string DueProcedure = "1";
		public const string SimplifiedProcedure = "2";
		public const string SpecialProcedure = "3";
	}

	public static class InAndOutwardProcessingRefinementTypesEdec
	{
		public const string CommercialProcessing = "1";
		public const string ContractProcessing = "2";
	}

	public static class InAndOutwardProcessingBillingTypesEdec
	{
		public const string SuspensiveProcedure = "1";
		public const string RefundProcedure = "2";
	}

	public static class InAndOutwardProcessingBillingTypesPassar
	{
		public const string NonCollection = "1";
		public const string Refund = "2";
	}

	public static class InAndOutwardRefinementAccountingTypesPassar
	{
		public const string NonCollection = "non-collection";
		public const string Refund = "refund";
	}

	public static class InAndOutwardRefinementProcessTypesPassar
	{
		public const string Ordinary = "ordinary";
		public const string Simplified = "simplified";
		public const string Special = "special";
	}

	public static class InAndOutwardRefinementTypesPassar
	{
		public const string Own = "own";
		public const string Pay = "pay";
	}

	public static class DeclarationTypeCodes
	{
		public const string Definitive = "01";
		public const string Provisional = "02";
	}

	public static class DeclarationTimeCodes
	{
		public const string PresentationToCustoms = "01";
		public const string AdvanceDeclaration = "02";
		public const string RetrospectiveDeclaration = "03";
	}

	public static class ClearanceLocation
	{
		public const string CustomsOffice = "1";
		public const string Domicile = "2";
	}

	public static class PrimaryPreferenceCodes
	{
		public const string PreferentialTariff = "PR";
		public const string NormalTariff = "NT";
	}
	public static class ProcedureCodesPassar
	{
		public const string ExportFromFreeCirculation = "20";
		public const string ReExportAfterInwardProcessing = "41";
		public const string OutwardProcessing = "50";
	}
	public static class ProcedureCodesEdec
	{
		public const string NormalDuty = "01";
		public const string RefinementTransportation = "02";
		public const string RepairTransportation = "03";
		public const string ReturnedGoodsExport = "04";
		public const string CustomsRelief = "05";
		public const string Tobacco = "06";
		public const string DutyFree = "07";
		public const string ExemptFromDuty = "08";
		public const string ReturnedGoods = "10";
		public const string ReturnedGoodsVAT = "11";

		public static bool IsReturnedGoods(string code) => code == ReturnedGoods || code == ReturnedGoodsVAT;

		public static bool IsWithoutDuty(string code) => code == ExemptFromDuty || IsReturnedGoods(code);

		public static bool IsCustomsRelief(string code) => code == RefinementTransportation || code == RepairTransportation || code == CustomsRelief || code == ReturnedGoods || code == ReturnedGoodsVAT;

		public static bool IsRepairOrRefinement(string code) => code == RefinementTransportation || code == RepairTransportation;
	}

	public static class FreeZoneTradeCode
	{
		public const string Hochsavoyen = "61";
		public const string Samnaun = "66";
	}

	public static class TaxCodes
	{
		public const string StandardRate = "1";
		public const string ReducedRate = "2";
		public const string ExemptVat = "3";
		public const string RelocationProcedure = "90";
		public const string ProcessingTraffic = "91";
		public const string DeferredTaxation = "92";

		public static bool IsAdditionalZeroPercentageVatCode(string code)
		{
			switch (code)
			{
				case RelocationProcedure:
				case ProcessingTraffic:
				case DeferredTaxation:
					return true;
				default:
					return false;
			}
		}
	}

	public static class TradeGroup
	{
		public const string EFTACountries = "100002";
		public const string DevelopingCountries = "100003";
		public const string CountriesOutsideSecurityZone = "300002";
	}

	public static class DeclarationReason
	{
		public const string ProofOfOriginEUCountries = "1";
		public const string ProofOfOriginEFTACountries = "2";
		public const string ProofOfOriginFreeTradeAgreementCountries = "3";
		public const string ProofOfOriginDevelopingCountries = "4";
	}

	public static class CorrectionReason
	{
		public const string Inspect = "3";
		public const string ConvertProvisoryToDefinitiveDeclaration = "7";
	}

	public static class TobaccoMainGroupCodes
	{
		public const string Cigars = "1";
		public const string Cigarettes = "2";
		public const string CutTobacco = "3";
		public const string Assortment = "4";
		public const string ECigarettes = "5";
	}

	public static class TobaccoSubGroupCodes
	{
		public const string _02 = "02";
		public const string _03 = "03";
	}

	public static class ExportCodes
	{
		public const string _11 = "11";
		public const string _12 = "12";
		public const string _13 = "13";
		public const string _14 = "14";
	}

	public static class WarehouseTypeCodes
	{
		public const string BondedWarehouse = "1";
		public const string InterimStorageAbroad = "2";
	}

	public static class CustomsStatusCodes
	{
		public const string ShipmentRelease = "203";
		public const string CustomsDeclarationReceived = "204";
		public const string Activated = "ACT";
	}

	public static class SpecificCircumstanceIndicators
	{
		public const string PostalAndExpressItems = "A";
		public const string ShipAndAircraftSupplies = "B";
	}

	public static class NonCustomsLawObligationCodes
	{
		public const string NotPossible = "0";
		public const string Needed = "1";
		public const string NotNeededAccordingDeclarant = "2";
	}

	public static class StorageCodes
	{
		public const string ImportHomeWithFinalTax = "1";
		public const string ImportHomeWithProvisionalTax = "2";
		public const string TransportToApprovedWarehouse = "3";
		public const string TransportToStockholdingWarehouse = "4";
		public const string ImportWithSpecialDispatchNode = "5";
	}

	public static class PasswordStatusList
	{
		public const string Received = "RCV";
		public const string Suspended = "SUS";
		public const string Queued = "QUE";
	}

	public static class RefundType
	{
		public const string Refund = "1";
		public const string RequestForAlcohol = "2";
		public const string RefundOfAlcoholOnBeer = "3";
		public const string ReturnedGoodsWithRefundRequest = "4";
		public const string TobaccoTaxRefund = "6";
		public const string TobaccoProductsExTaxWarehouse = "7";
		public const string OtherRefunds = "8";
	}

	public static class QuantityUnit
	{
		public const string LiterPureAlcohol = "LPA";
	}

	public static class AssessmentCodeValues
	{
		public const string Per100kgGrossMass = "11";
		public const string PerPiece = "12";
		public const string PerKgNetMass = "13";
		public const string PerHectolitre = "17";
		public const string Per1000Pieces = "22";
		public const string Per1000LitresAt15DegreesCelsius = "23";
		public const string Per1000kgNetMass = "24";
	}

	public static class EntryStatusCodes
	{
		public const string New = "NEW";
	}

	public static class InputControlCodes
	{
		public const string Simplified = "1";
		public const string Ordinary = "2";
	}

	public static class PassarReasonCodes
	{
		public const string Duplication = "20";
		public const string Others = "99";
	}

	public static class RestrictionCode
	{
		public const string FOEN_411 = "411";
		public const string FOEN_412 = "412";
		public const string FOEN_413 = "413";
		public const string FOEN_414 = "414";
	}

	public static class RestrictionAdditionalInformationCode
	{
		public const string AuthorizationItemNumber = "B1001";
	}

	public static class SupportingDocumentTypeCodes
	{
		public const string WVBEUR1 = "9541";
		public const string WVBEUR1CN = "9542";
		public const string WVBEURMED = "9543";
		public const string WVBEUR1TransitionalRules = "9544";

		public static bool IsSupportingDocumentWithPreference(string code)
		{
			switch (code)
			{
				case WVBEUR1:
				case WVBEUR1CN:
				case WVBEURMED:
				case WVBEUR1TransitionalRules:
					return true;
				default:
					return false;
			}
		}
	}

	public static class TransportDocumentTypeCodes
	{
		public const string MAWB = "N741";
	}

	public static class RateOverrideReasonCode
	{
		public const string Overridden = "OVR";
	}

	public static class PermitAuthorityCodes
	{
		public const string FOAG = "1";
		public const string AAT = "2";
		public const string BWIP = "3";
		public const string BWRP = "4";
		public const string FOPH = "5";
		public const string FOEN = "6";
		public const string COE = "7";
		public const string CA = "8";
		public const string FSVO_CITES = "11";
		public const string FSF = "12";
		public const string TOS = "15";
		public const string FOE = "17";
		public const string COW = "18";
		public const string SM = "20";
		public const string STB = "21";
		public const string RS = "22";
		public const string FOC = "23";
		public const string IVI = "24";
		public const string AR = "25";
		public const string FSVO_Other = "26";
		public const string FTA = "80";
		public const string FOCBS_Origin = "95";
		public const string FOCBS_MOT = "96";
		public const string FOCBS_COV = "97";
		public const string FOCBS_Other = "98";
		public const string Other = "99";

		public static bool IsNotApplicableForEPermitImport(ZString issuer)
		{
			switch (issuer)
			{
				case FOAG:
				case AAT:
				case FOPH:
				case FOEN:
				case COE:
				case CA:
				case FSF:
				case TOS:
				case FOE:
				case COW:
				case SM:
				case STB:
				case RS:
				case FOC:
				case IVI:
				case FTA:
				case FOCBS_MOT:
				case FOCBS_COV:
				case FOCBS_Other:
				case Other:
					return true;
				default:
					return false;
			}
		}

		public static bool IsNotApplicableForEPermitExport(ZString issuer)
		{
			switch (issuer)
			{
				case FOAG:
				case AAT:
				case FOPH:
				case FOEN:
				case COE:
				case CA:
				case FSF:
				case TOS:
				case FOE:
				case COW:
				case SM:
				case STB:
				case RS:
				case FOC:
				case IVI:
				case FSVO_Other:
				case FTA:
				case FOCBS_MOT:
				case FOCBS_COV:
				case FOCBS_Other:
				case Other:
					return true;
				default:
					return false;
			}
		}

		public static bool IsApplicableForSingleEPermit(ZString issuer)
		{
			switch (issuer)
			{
				case BWIP:
				case BWRP:
				case FSVO_CITES:
				case FSVO_Other:
					return true;
				default:
					return false;
			}
		}

		public static bool IsNotApplicableForSingleEPermit(ZString issuer) => !issuer.IsEmpty && !IsApplicableForSingleEPermit(issuer);

		public static bool IsNotApplicableForGeneralEPermitImport(ZString issuer) => !issuer.IsEmpty && !IsNotApplicableForNonEPermitImport(issuer);

		public static bool IsNotApplicableForGeneralEPermitExport(ZString issuer) => !issuer.IsEmpty && !IsNotApplicableForNonEPermitExport(issuer);

		public static bool IsNotApplicableForNonEPermitImport(ZString issuer)
		{
			switch (issuer)
			{
				case BWIP:
				case BWRP:
				case FSVO_Other:
					return true;
				default:
					return false;
			}
		}

		public static bool IsNotApplicableForNonEPermitExport(ZString issuer)
		{
			switch (issuer)
			{
				case BWIP:
				case BWRP:
					return true;
				default:
					return false;
			}
		}
	}
}

#endregion
