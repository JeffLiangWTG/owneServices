using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Messaging
{
	public static class Constants
	{
		#region SuppressResourceStringsCheckRegion

		public static class DateFormatType
		{
			public const string Date = "yyyyMMdd";
			public const string DateSlash = "yyyy/MM/dd";
			public const string DateTime = "yyyyMMddHHmmss";
			public const string DateTimeNoSecond = "yyyyMMddHHmm";
			public const string DateYY = "yyMMdd";
			public const string DateKorean = "yyyy-MM-dd";
			public const string DateTimeKorean = "yyyy-MM-dd HH:mm:ss";
			public const string DateTimeKoreanNoSecond = "yyyy-MM-dd HH:mm";
			public const string DateYYKorean = "yy-MM-dd";
			public const string MonthYear = "MM/yyyy";
			public const string Year = "yyyy";
			public const string DateWithMicroSeconds = "yyyyMMddHHmmssffffff";
		}

		#endregion

		public static class IdentificationType
		{
			public const string KoreanRegNoForResident = MasterFiles.Business.CountryCompliance.KoreaSouthComplianceInfo.CodeTypes.KoreanRegNoForResident;
			public const string KoreanRegNoForForeigner = MasterFiles.Business.CountryCompliance.KoreaSouthComplianceInfo.CodeTypes.KoreanRegNoForForeigner;
			public const string BusinessRegNo = MasterFiles.Business.OrgCusCode.CodeTypes.GovBusinessCode;
			public const string PassportNo = MasterFiles.Business.OrgCusCode.CodeTypes.PassportID;
			public const string UnipassIDForIndividual = MasterFiles.Business.CountryCompliance.KoreaSouthComplianceInfo.CodeTypes.UnipassIDForIndividual;
			public const string UnipassIDForOrganization = MasterFiles.Business.CountryCompliance.KoreaSouthComplianceInfo.CodeTypes.UnipassIDForOrganization;
			public const string ForeignCompanyID = MasterFiles.Business.CountryCompliance.KoreaSouthComplianceInfo.CodeTypes.ForeignCompanyID;
			public const string OfficeID = MasterFiles.Business.CountryCompliance.KoreaSouthComplianceInfo.CodeTypes.OfficeID;
			public const string CorporationCode = MasterFiles.Business.OrgCusCode.CodeTypes.CorporationCode;
			public const string BuildingNumber = MasterFiles.Business.CountryCompliance.KoreaSouthComplianceInfo.CodeTypes.BuildingNumber;
			public const string RoadNameCode = MasterFiles.Business.CountryCompliance.KoreaSouthComplianceInfo.CodeTypes.RoadNameCode;
			public const string CertificateOfOriginExporterNumber = "10";
			public const string IndustrialParkCode = MasterFiles.Business.CountryCompliance.KoreaSouthComplianceInfo.CodeTypes.IndustrialParkCode;
			public const string AuthorizedImporterRegNo = "AIM";
			public const string CourierCompanyID = MasterFiles.Business.CountryCompliance.KoreaSouthComplianceInfo.CodeTypes.CourierCompanyID;
			public const string ControlledPremisesID = MasterFiles.Business.OrgCusCode.CodeTypes.ControlledPremisesID;
			public const string ECommerceCompanyID = MasterFiles.Business.CountryCompliance.KoreaSouthComplianceInfo.CodeTypes.ECommerceCompanyID;
			public const string OnlineTradeSellerID = "CEC";
		}

		public static class ResponseTypeCode
		{
			public const string Required = "AB";
			public const string NotRequired = "NA";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Column layout")]
		public const string ColumnLayoutContextLocalExport = "Local Export";
		public static class Communication
		{
			public const string TelNo = "TE";
			public const string Email = "EM";
			public const string ExtensionTelNo = "EX";
			public const string Phone = "CE";
			public const string Fax = "FX";
		}

		public static class FunctionCode
		{
			public const string Cancel = "1";
			public const string Original = "9";
			public const string Resend = "35";
			public const string Addition = "2";
		}

		public static class AdditionalInformationStatementCodes_929
		{
			public const string _257 = "257";
			public const string _258 = "258";
			public const string _259 = "259";
		}

		public static class Exportdeclaration
		{
			public const string Correction = "A";
			public const string Drop = "B";
			public const string Excellency = "C";
		}
		public static class TaxType
		{
			public const string TBD = "TBD";
			public const string TAD = "TAD";
			public const string TPA = "TPA";
		}

		public static class CustomsBrokerRoleCode
		{
			public const string Customs = "CB";
			public const string Taxpayer = "PR";
		}

		public const string GOVCBR = "GOVCBR";
		public const string DefaultWeightUnit = "KG";
		public const string NetWeightUnit = "KGM";

		public static class OneOrMultiple
		{
			public const string ONE = "ONE";
			public const string MUL = "MUL";
			public const string OST = "OST";
		}

		public static class RequestOverrideCode
		{
			public const string Before = "1";
			public const string After = "2";
		}

		public static class MessageSubTypeLocalExport
		{
			public const string Amendment = "1";
			public const string Cancellation = "2";
		}

		public static class YesNo
		{
			public const string Yes = "Y";
			public const string No = "N";
		}

		public static class EntryTypeForStatementLine
		{
			public const string OtherImportCost = "OTH";
		}

		public static class ApplicationNumberType
		{
			public const string ABT = "ABT";
			public const string XXC = "XXC";
		}

		public const string MRN = "MRN";
		public const string ExportDeclarationNumberEntryLineSplitor = ":";
		public const string ImportDutyReductionRateRegulationCodeSplitor = ":";
		public const string Hyphen = "-";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not a code smell")]
		public const string ExtendedHoursRequestApplicationNumberText = "-개청-";
		public const int CustomsBrokerCommentLength = 200;

		public static class DutyReductionClassificationCode
		{
			public const string DutyExemption = "A";
			public const string DutyReduction = "B";
			public const string InstallmentPayment = "C";
			public const string SpecificUseCodeOnly = "D";
			public const string SpecificUseCodeAndDutyReductionOrInstallment = "T";
			public const string Invalid = "X";
		}

		public static class AgricultureTaxClassification
		{
			public const string OnDutyExemptionReduction = "A";
			public const string OnSpecialConsumptionTax = "B";
			public const string OnBoth = "C";
		}

		public static class ZZ
		{
			public static class CodeListAttributeNames
			{
				public const string InstallmentMonths = "ISM";
				public const string InstallmentFrequency = "ISF";
				public const string InstallmentToBeAdited = "ISA";
				public const string BusinessNumber = "CustomsOfficeBusinessNumber";
				public const string Address = "CustomsOfficeAddress";
				public const string BankAccountID = "BankAccountID";
				public const string RefundDepartment = "CustomsOfficeRefundDepartment";
				public const string MandatoryDocWhenExemptCode = "MandatoryDocWhenExempt";
				public const string FTATradeGroup = "FTATradeGroup";
				public const string TradePreferenceTradeGroup = "TradePreferenceTradeGroup";
				public const string PostClearanceProcedure = "ISP";
				public const string RequiredExportDocumentType = "RequiredExportDocumentType";
			}

			public static class RateTypes
			{
				public const string DomesticTax = "DMT";
			}

			public static class RateCodes
			{
				public const string DutyAdValorem = "DTA";
				public const string DutySpecific = "DTS";
				public const string DutyReductionRate = "DRR";
				public const string EducationTaxRate = "EDT";
			}

			public static class RateFormulaConstants
			{
				public const string VFD = "VFD";
				public const string Multiply = "*";
				public const string UQPlaceholderOpeningBracket = "[";
			}

			public static class TradeGroups
			{
				public const string All = "ALL";
				public const string China = "CN";
				public const string UnitedStates = "US";
			}

			public static class TariffAttributeNames
			{
				public const string TaxCode = "TaxCode";
				public const string InvolvesInstallationCost = "InvolvesInstallationCost";
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Attribute strings")]
			public static class TariffAttributes
			{
				public const string IsDutyExempt = "IsDutyExempt";
				public const string AgricultureTaxAApplies = "AgricultureTaxAApplies";
				public const string AgricultureTaxBApplies = "AgricultureTaxBApplies";
				public const string TaxClassification1 = "TaxClassification1";
				public const string PostClearanceProcedure = "PostClearanceProcedure";
				public const string SpecialConsumptionTax = "SCT";
				public const string LiquorTax = "LQT";
				public const string TransportationTax = "TRT";
				public const string ValueAddedTax = "VAT";
				public const string InvoiceQuantityInCU1 = "InvoiceQuantity in CU1";
				public const string ReImport = "For Re-Import of Previously Exported Goods";
			}

			public static class TariffTypes
			{
				public const string DutyReductionExemption = "DRE";
				public const string DomesticTaxRate = "DMT";
				public const string DomesticTaxReductionExemption = "DTE";
			}

			public static class NKCodeType
			{
				public const string CustomsOffice = "CUSOF";
				public const string CustomsDepartment = "CUSDP";
				public const string IndustrialParkCode = "INDPK";
				public const string EXFTA = "EXFTA";
				public const string InstallmentCode = "ISC";
				public const string BrandCode = "BRDCD";
				public const string ENGAR = "ENGAR";
				public const string INGAR = "INGAR";
				public const string OtherGovernmentAndAssociatedAgency = "OGAAA";
				public const string ExpressDeliveryServiceIDs = "EDSID";
				public const string DetailedFTACountries = "DFTA";
				public const string KR006 = "KR006";
				public const string TaxOffice = "TAXOF";
				public const string OGARegulationCategory = "OGARC";
				public const string KRFTE = "KRFTE";
			}

			public static class RefCusMap
			{
				public const string CountryKRCCode = "CNTRY";
				public const string KRPreferenceCode = "KRPRG";
			}

			public static class ApplicabilityAdditionalCodes
			{
				public const string Max = "MAX";
				public const string Min = "MIN";
			}
			public static class RefCusConditionClass
			{
				public const string RATE = "RATE";
			}
			public static class RefCusConditionType
			{
				public const string PostClearanceProcedure = "5FN";
				public const string OGA = "OGA";
				public const string SteelProduct = "SPA";
				public const string SimpleDrawback = "SDW";
			}
			public static class RefCusConditionValue
			{
				public const string A = "A";
				public const string C = "C";
				public const string EU = "EU";
			}

			public static class RefCusTaxOrFeeCodes
			{
				public const string AgricultureTaxA = "AGA";
				public const string AgricultureTaxB = "AGB";
				public const string VATRateA = "VTA";
				public const string VATRateB = "VTB";
				public const string VATRateC = "VTC";

				public static class LatePaymentInterests
				{
					public const string AnnualRateWithinSixMonths = "PLTR";
					public const string DailyRateAfterSixMonths = "LPIR";
				}

				public static class PenaltyForLateDeclaration
				{
					public const string FirstLevel = "PL1";
					public const string SecondLevel = "PL2";
					public const string ThirdLevel = "PL3";
					public const string FourthLevel = "PL4";
				}
				public static class DutyPenaltyCauseCodes
				{
					public const string IllegitimateMissedDeclaration = "DPE1";
					public const string GeneralMissedDeclaration = "DPE2";
					public const string FraudulentIllegitimateMissedDeclaration = "DPEA";
					public const string IllegitimateLateDeclaration = "DPE3";
					public const string GeneralLateDeclaration = "DPE4";
					public const string FraudulentIllegitimateLateDeclaration = "DPEB";
				}

				public static class TaxPenaltyCauseCodes
				{
					public const string IllegitimateMissedDeclaration = "TPE1";
					public const string GeneralMissedDeclaration = "TPE2";
					public const string InternationalIllegitimateMissedDeclaration = "TPEA";
					public const string IllegitimateLateDeclaration = "TPE3";
					public const string GeneralLateDeclaration = "TPE4";
					public const string InternationalIllegitimateLateDeclaration = "TPEB";
				}

				public static class DutyPenaltyReductionRates
				{
					public const string WithinTwelveMonths = "DPR1";
					public const string WithinEighteenMonths = "DPR2";
					public const string WithinTwentyFourMonths = "DPR3";

					public static IEnumerable<KeyValuePair<int, string>> MonthsAndRateCodes = new Dictionary<int, string>
					{
						{ TwelveMonthsSinceDueDate, WithinTwelveMonths },
						{ EighteenMonthsSinceDueDate, WithinEighteenMonths },
						{ TwentyFourMonthsSinceDueDate, WithinTwentyFourMonths }
					};
				}

				public static class TaxPenaltyReductionRates
				{
					public const string WithinTwelveMonths = "TPR1";
					public const string WithinEighteenMonths = "TPR2";
					public const string WithinTwentyFourMonths = "TPR3";

					public static IEnumerable<KeyValuePair<int, string>> MonthsAndRateCodes = new Dictionary<int, string>
					{
						{ TwelveMonthsSinceDueDate, WithinTwelveMonths },
						{ EighteenMonthsSinceDueDate, WithinEighteenMonths },
						{ TwentyFourMonthsSinceDueDate, WithinTwentyFourMonths }
					};
				}
			}

			public static class RefCusConditionValueType
			{
				public const string OGADocumentName = "OGADN";
				public const string OGARegulationNumber = "OGARG";
				public const string TradeGroup = "TRG";
				public const string SimpleDrawbackRate = "DWA";
			}

			public static class Preferences
			{
				public const string AlphabetF = "F";
				public const string GeneralInternationalPreference = "F";
				public const string GeneralInternationalPreference1 = "F1";
				public const string IsraelFTA = "FIL";
			}

			public const int NumberOfDaysInYearForPenaltyCalculation = 365;
			public const int SixMonthsSinceDueDate = 6;
			public const int TwelveMonthsSinceDueDate = 12;
			public const int EighteenMonthsSinceDueDate = 18;
			public const int TwentyFourMonthsSinceDueDate = 24;
		}
		public static class TransportModes
		{
			public const string Sea = "10";
			public const string Rail = "20";
			public const string Road = "30";
			public const string Air = "40";
			public const string Mail = "50";
			public const string Complex = "60";
			public const string FixedTransportInstallations = "70";
			public const string InlandWaterwayTransport = "80";
			public const string Other = "90";

			public static string ConvertTransportMode(string transportMode)
			{
				var result = "";
				switch (transportMode)
				{
					case Core.Constants.TransportModes.Sea:
						result = Sea;
						break;
					case Core.Constants.TransportModes.Rail:
						result = Rail;
						break;
					case Core.Constants.TransportModes.Road:
						result = Road;
						break;
					case Core.Constants.TransportModes.Truck:
						result = Road;
						break;
					case Core.Constants.TransportModes.Air:
						result = Air;
						break;
					case Core.Constants.TransportModes.Mail:
						result = Mail;
						break;
					case Core.Constants.TransportModes.SeaAir:
						result = Complex;
						break;
					case Core.Constants.TransportModes.AirSea:
						result = Complex;
						break;
					case Core.Constants.TransportModes.FixedTransportInstallations:
						result = FixedTransportInstallations;
						break;
					case Core.Constants.TransportModes.InlandWaterwayTransport:
						result = InlandWaterwayTransport;
						break;
					default:
						result = Other;
						break;
				}
				return result;
			}
		}
		public const string UnderbondMovement = "UDM";
		public static class VATRateCode
		{
			public const string VATTaxation = "A";
			public const string VATExemption = "B";
			public const string VATCut = "C";
		}
		public static class ProductOrMaterialCode
		{
			public const string Product = "A";
			public const string RawMaterial = "B";
		}

		public static class DeclarantType
		{
			public static class GOVCBRD72
			{
				public const string Importer = "1";
				public const string Broker = "2";
			}

			public static class GOVCBR5FE
			{
				public const string Broker = "01";
				public const string Importer = "05";
			}
		}

		public enum BeforeOrAfterAmendment { Before = 1, After }

		public static class EntryNumberCheckDigit
		{
			public const string U = "U";
			public const string X = "X";
			public const string M = "M";
			public const string R = "R";
			public const string B = "B";
			public const string S = "S";
			public const string F = "F";
			public const string H = "H";
		}

		public static class NumberFormatDigit
		{
			public const string D2 = "D2";
			public const string D3 = "D3";
			public const string D4 = "D4";
			public const string D5 = "D5";
			public const string D6 = "D6";
			public const string D7 = "D7";
			public const string D8 = "D8";
		}

		public static class NumberFountainMaxValues
		{
			public const long _4digit = 9999;
			public const long _5digit = 99999;
			public const long _6digit = 999999;
			public const long _7digit = 9999999;
			public const long _8digit = 99999999;
		}

		public static class SupportingDocAttachedCode
		{
			public const string EnterAll = "N";
			public const string AgreementTaxRateApplicationAllInputs = "B";
		}
		public static class DutyRateTypeCode
		{
			public const string DutyAdValorem = "1";
			public const string DutySpecific = "3";
		}
		public static class JobComInvLineRefsType
		{
			public const string CertificateOfOtiginReferenceNo = "COOR";
		}

		public static class PenaltyCalculationConstants
		{
			public const int TwentyDaysAfterDueDate = 20;
			public const int FiftyDaysAfterDueDate = 50;
			public const int EightyDaysAfterDueDate = 80;
			public const decimal ThresholdAmountExemptForOverduePenalty = 1500000m;
			public const decimal PenaltyRateForOverduePayment = 1.03m;

			public static decimal CalculateLatePaymentInterestAmountAndTruncate(decimal amount)
			{
				return TruncateAmount(amount * PenaltyRateForOverduePayment);
			}

			public static decimal TruncateAmount(decimal amount)
			{
				return Math.Truncate(amount / 10) * 10;
			}

			public static ZDateTime GetDeclarationDueDate(ZDateTime arrivalDate) => arrivalDate.IsEmpty ? ZDateTime.Empty : arrivalDate.AddDays(30);
		}
		public static class DomestictaxClassificationCode
		{
			public const string Exempt = "0";
			public const string Standard = "A";
			public const string ProbationalRate = "B";
			public const string PreferentialRate = "C";
			public const string LQT = "1";
			public const string TRT = "6";
		}
		public static class DomesticTaxBaseQtyOrPriceCode
		{
			public static class UQs
			{
				public const string AlcoholContent = "AC";
				public const string Unit = "U";
				public const string Minutes = "MIN";
			}
			public const string HSCodeInMinutes = "8523292231";
		}
		public const string CusMiscRequestJobNumberPrefix = "MSC";

		public static class CW1PackType
		{
			public const string Barrels = "BBL";
			public const string Can = "CAN";
			public const string PlywoodBox = "PWB";
			public const string Carton = "CTN";
			public const string Each = "EA";
			public const string Pieces = "PCS";
			public const string Tray = "TRY";
		}

		public static class EDIInterchangeType
		{
			public const string DLT = "DLT";
			public const string DOC = "DOC";
			public const string RSP = "RSP";
			public const string ESR = "ESR";
			public const string SSR = "SSR";
			public const string XER = "XER";
		}

		public static class KrCurrency
		{
			public const string RussianRouble = "RUB";
		}

		public static class InterchangeJsonKeys
		{
			public const string CustomStatus = "custom.CustomsErrCode";
			public const string CustomStatusDesc = "custom.CustomsErrDesc";
		}

		[ThreadSafe]
		public readonly static ZString[] KrCurrencyCodeList = {
			Core.Constants.CurrencyCodes.UnitedArabEmirates,
			Core.Constants.CurrencyCodes.Argentina,
			Core.Constants.CurrencyCodes.Australia,
			Core.Constants.CurrencyCodes.Bangladesh,
			Core.Constants.CurrencyCodes.Bahrain,
			Core.Constants.CurrencyCodes.BruneiDarussalam,
			Core.Constants.CurrencyCodes.Brazil,
			Core.Constants.CurrencyCodes.Canada,
			Core.Constants.CurrencyCodes.Liechtenstein,
			Core.Constants.CurrencyCodes.Chile,
			Core.Constants.CurrencyCodes.China,
			Core.Constants.CurrencyCodes.Colombia,
			Core.Constants.CurrencyCodes.CzechRepublic,
			Core.Constants.CurrencyCodes.Denmark,
			Core.Constants.CurrencyCodes.Egypt,
			Core.Constants.CurrencyCodes.Ethiopia,
			Core.Constants.CurrencyCodes.EuropeanUnion,
			Core.Constants.CurrencyCodes.Fiji,
			Core.Constants.CurrencyCodes.SouthGeorgiaAndTheSouthSandwic,
			Core.Constants.CurrencyCodes.HongKong,
			Core.Constants.CurrencyCodes.Hungary,
			Core.Constants.CurrencyCodes.Indonesia,
			Core.Constants.CurrencyCodes.Israel,
			Core.Constants.CurrencyCodes.India,
			Core.Constants.CurrencyCodes.Jordan,
			Core.Constants.CurrencyCodes.Japan,
			Core.Constants.CurrencyCodes.Kenya,
			Core.Constants.CurrencyCodes.Cambodia,
			Core.Constants.CurrencyCodes.KoreaRepublicOf,
			Core.Constants.CurrencyCodes.Kuwait,
			Core.Constants.CurrencyCodes.Kazakhstan,
			Core.Constants.CurrencyCodes.SriLanka,
			Core.Constants.CurrencyCodes.LibyanArabJamahiriya,
			Core.Constants.CurrencyCodes.Myanmar,
			Core.Constants.CurrencyCodes.Mongolia,
			Core.Constants.CurrencyCodes.Macau,
			Core.Constants.CurrencyCodes.Mexico,
			Core.Constants.CurrencyCodes.Malaysia,
			Core.Constants.CurrencyCodes.Norway,
			Core.Constants.CurrencyCodes.Nepal,
			Core.Constants.CurrencyCodes.NewZealand,
			Core.Constants.CurrencyCodes.Oman,
			Core.Constants.CurrencyCodes.Philippines,
			Core.Constants.CurrencyCodes.Pakistan,
			Core.Constants.CurrencyCodes.Poland,
			Core.Constants.CurrencyCodes.Qatar,
			Core.Constants.CurrencyCodes.RomaniaNew,
			KrCurrency.RussianRouble,
			Core.Constants.CurrencyCodes.SaudiArabia,
			Core.Constants.CurrencyCodes.Sweden,
			Core.Constants.CurrencyCodes.Singapore,
			Core.Constants.CurrencyCodes.Thailand,
			Core.Constants.CurrencyCodes.Turkey,
			Core.Constants.CurrencyCodes.Taiwan,
			Core.Constants.CurrencyCodes.UnitedStates,
			Core.Constants.CurrencyCodes.Uzbekistan,
			Core.Constants.CurrencyCodes.VietNam,
			Core.Constants.CurrencyCodes.SouthAfrica
		};

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Codes")]
		public static class ManufacturerDefaultCode
		{
			public const string UnipassID = "제조미상9999000";
			public const string IndustrialParkCode = "999";
			public const string CompanyName = "미상";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Codes")]
		public static class SupplierDefaultCode
		{
			public const string UnipassID = "공급신청자미상9999";
			public const string SupplierID = "ZZZZZZZZ9999A";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Codes")]
		public static class BuyerDefaultCode
		{
			public const string BuyerID = "ZZZZZZZZ9999A";
		}

		public static class ContainerTerminalOperatorDefaultCode
		{
			public static string GetFinalBondedAreaCode(string customsOfficeCode)
			{
				return customsOfficeCode + "99999";
			}
		}

		public const string KRLanguageText = "KO-KR";

		public const int ExchangeRateDigit = 4;

		public static class DecimalPlacesConstants
		{
			public const int Amount = 4;
			public const int AmountInKRW = 0;
			public const int CustomsValue = 0;
			public const int ExchangeRate = 4;
			public const int TaxRate = 2;
			public const int CostRate = 2;
			public const int Freight = 0;
			public const int Insurance = 0;
			public const int CommercialChargeAmount = 0;
			public const int InvoiceQuantity = 4;
			public const int LocalExportInvoiceQuantity = 3;
			public const int Weight = 3;
			public const int NoOfPacks = 0;
			public const int Qty = 0;
			public const int QtyOrWeight = 4;
			public const int QtyOrPrice = 2;
			public const int TotalInvoiceAmount = 2;
			public const int ImportTotalInvoiceAmount = 0;
			public const int UnitPrice = 6;
			public const int DutyTaxFee = 0;
			public const int QuantityToClaimRefund = 4;
			public const int DutyRate = 2;
			public const int SpecificDutyRate = 0;
			public const int UsedQtyPlaces = 4;
			public const int RefundAmount = 0;
			public const int EngineDisplacement = 3;
			public const int TotalRefundAmount = 0;
			public const int CustomsQuantityOrWeight = 3;
			public const int Month = 0;
			public const int CustomsQuantity = 5;
			public const int DrawbackQuantity = 3;
			public const int ProvAdditionalRate = 2;
		}

		public static class IdNumbersFormatConstants
		{
			public const string ExportContainerNo = "D2";
			public const string ExportGAApprovalSequenceNo = "D2";
			public const string ExportVehicleNo = "D3";
			public const string EntryLineNo = "D3";
			public const string InvoiceLineNo = "D2";
			public const string ImportLineNo = "D3";
			public const string ImportEntryLineNo = "D3";
			public const string D2 = "D2";
		}
		public static class ValuationMethod
		{
			public const string A = "A";
			public const string B = "B";
		}

		public static class ExemptionProcessCode
		{
			public const string A = "A";
			public const string B = "B";
		}

		public static class ImportCargoManagementNumber
		{
			public const string No = "NO";
		}

		public static class RefundRequestType
		{
			public const string StandAlone = "STA";
			public const string Simultaneous = "5FE";
		}

		public static ZBool IsExchangeRatePublished(ZString currencyCode)
		{
			return KrCurrencyCodeList.Contains(currencyCode);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Questions")]
		public static class ValuationDeclarationQuestions
		{
			public static ResourceStringData Q5A => Res.GetData("FDDE4069-0FFA-48C1-8EAC-46138918D415", "(a) Does it fall under the special relationship set out in Article 23, Paragraph 1 of the Enforcement Decree of the Customs Act between the\r\nbuyer and the seller? (If not, do not include (b), (c), (d), (e))");
			public static ResourceStringData Q5B => Res.GetData("23650C18-1FE5-42CB-9F06-BACC95A22EC1", "(b) If it falls under a special relationship in (a), which special relationship does it fall under Article 23, Paragraph 1 of the Enforcement\r\nDecree of the Customs Act?");
			public static ResourceStringData Q5ALong => Res.GetData("C5F8768F-CE27-4F86-BA72-B5F1AFB2C80A", "(a) Does it fall under the special relationship set out in Article 23, Paragraph 1 of the Enforcement Decree of the Customs Act between the buyer and the seller?\r\n(If not, do not include (b), (c), (d), (e))");
			public static ResourceStringData Q5BLong => Res.GetData("528D70E5-8DA3-4FB7-93FF-650496129064", "(b) If it falls under a special relationship in (a), which special relationship does it fall under Article 23, Paragraph 1 of the Enforcement Decree of the Customs Act?");
			public static ResourceStringData Q5C => Res.GetData("FCFBFE15-D245-4FBD-8CC4-D478FABF70A8", "(c) Have related relationships influenced the pricing of imported goods?");
			public static ResourceStringData Q5D => Res.GetData("C2CAA36B-33FB-46B9-A672-274475EB7D7C", "(d) Is the transaction price close to the comparative price of Article 5 of the Enforcement Rules of the Customs Act?");
			public static ResourceStringData Q5E => Res.GetData("0A02E574-D59E-4B4C-A4F5-7FCBC155F76C", "(e) How is the price of imported goods determined in transactions between related parties?");

			public static ResourceStringData Q5Eb => Res.GetData("A23EAAD9-E78F-4FBE-A1E3-AE5E4A235EFA", "(e) Other methods of determining the price");
			public static ResourceStringData Q6A => Res.GetData("29454811-3417-4002-9DD7-986E89911B6B", "(a) Are there any restrictions on the buyer’s ability to use or dispose of the imported goods, other than the following?\r\n- Restrictions imposed by Korean laws or public institutions\r\n- Restrictions on areas of sale\r\n- Restrictions that do not substantially affect the price of the imported goods");
			public static ResourceStringData Q6B => Res.GetData("6EC56B87-5654-464A-A3E0-F98EA1961804", "(b) Are there any conditions or circumstances affecting the sale or pricing beyond the product price itself?");

			public static ResourceStringData Q7A => Res.GetData("034E7703-E6EA-4685-ABD6-2285EAC1DFE6", "(a) Are any royalties or license fees paid directly or indirectly by the buyer as part of the transaction involving the imported goods?");
			public static ResourceStringData Q7B => Res.GetData("B0311C23-C745-44FC-AC1F-69636299F9B5", "(b) Is any portion of the revenue from the use or resale of the imported goods directly or indirectly paid to the seller?");
			public static ResourceStringData Q8 => Res.GetData("AE995D20-6C01-433D-B0D2-AF7866EF601C", "8. Are there any of the following amounts that are not included in the transaction price on the invoice?");
			public static ResourceStringData Q8A => Res.GetData("CFAA89F5-B680-4BC3-92B3-D32C9AA31935", "(a) Amount by which the buyer offsets the price of the imported goods against the seller's debt");
			public static ResourceStringData Q8B => Res.GetData("CD3036B4-8A9D-4266-AD2B-5FD2896D4A6D", "(b) Amount repaid by the buyer on behalf of the seller’s debt");
			public static ResourceStringData Q8C => Res.GetData("86B1BE6E-D462-4A26-BDAB-36060F3B1D79", "(c) Unrecognized discounts – restrictions on use or disposal, conditions affecting pricing, conditions that compensate for ex-post attributable profits, price discounts, etc., where special relationships between buyers and sellers influence the transaction price");
			public static ResourceStringData Q8D => Res.GetData("E26BF65F-D7F6-4536-B669-B70A9FA56D24", "(d) Other indirect payments (as described in Article 20(6) of the Enforcement Decree of the Customs Act)");
			public static ResourceStringData Q9 => Res.GetData("3D470677-A293-41F0-B1BF-9F90CDEBB7D6", "9. Are there any of the following costs borne by the buyer that are not included in the transaction price?");
			public static ResourceStringData Q9A => Res.GetData("5AEB7760-F29F-483C-BCB4-71B3B0079325", "(a) Fees and brokerage charges (excluding standard purchase fees)");
			public static ResourceStringData Q9B => Res.GetData("982CB97E-AB98-491F-8C39-6E2E16D6B676", "(b) Container and packaging costs (e.g., special packaging for liquid or gas transport)");
			public static ResourceStringData Q10 => Res.GetData("A7754158-4570-48FB-B974-D7F6A4D02A2E", "10. In connection with the production or export of imported goods, are any of the following goods or services provided by the buyer free of charge\r\nor at reduced prices, but not included in the transaction price?");
			public static ResourceStringData Q10A => Res.GetData("56A5411F-A1EB-4FD6-A16C-3B93456C822E", "(a) Materials, components, parts, and similar items incorporated into imported goods");
			public static ResourceStringData Q10B => Res.GetData("AF4F8ED0-3C77-4716-90A6-1B7365AEEA57", "(b) Tools, molds, dies, and similar items used in the production of imported goods");
			public static ResourceStringData Q10C => Res.GetData("8420F3E4-DCB4-4E55-9FD4-60A9093DD92A", "(c) Goods consumed during the production of imported goods");

			public static ResourceStringData Q10D => Res.GetData("2156B7AF-3B5A-4422-9BF3-0D7C174707CE", "(d) Work carried out in a foreign country, including technology, design, craft, sketches, and other elements necessary for the production of imported goods");

			public static ResourceStringData Q11 => Res.GetData("A20F0E96-84A9-423C-98D7-0C82467E57BC", "11. Apart from transportation costs, are any of the following expenses included in the transaction price?");
			public static ResourceStringData Q11A => Res.GetData("3DDB4CC1-AC69-48D6-B8C8-8B6E74271796", "(a) Technical services for construction, installation, assembly, maintenance, or after-import work");
			public static ResourceStringData Q11B => Res.GetData("BDB1CDE1-5B38-48F5-987A-6D6897244DE1", "(b) Duties and domestic taxes subject to refund or reduction upon export from the exporting country");

			public static ResourceStringData Q11C => Res.GetData("80870857-C614-4BF0-9DF1-B798D3F1A6EB", "(c) Other expenses (inspection, purchase fees, training, annual interest, etc., incurred for the buyer’s own needs regardless of the contract)");

			public static ResourceStringData Q11D => Res.GetData("29990A62-EE7C-456F-88BD-9C4D8DC83533", "(d) Recognized price discounts, such as cash discounts, quantity discounts, etc.");
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not a code smell")]
		public static class Lists
		{
			public const string KrOnlyTest = "kr only list";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Questions")]
		public static class HsExtensionCodes
		{
			public static class Code
			{
				public const string Category = "CAT";
				public const string SubCategory = "SCA";
			}
			public static class Description
			{
				public const string Category = "Category";
				public const string SubCategory = "Sub-Category";
			}
		}
	}
}
