using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common;
using ChargeTypes = Enterprise.Customs.BR.Business.ImportCustomsChargeTypeList.Codes;

namespace Enterprise.Customs.BR.Business
{
	public static class ImportChargesProvider
	{
		public static IReadOnlyList<ICustomsChargeCode> Codes
		{
			get
			{
				return new[]
				{
					FreightComponents,
					FreightInNationalTerritory,
					OtherAdditionsCustomsValue,
					CommissionsBrokerage,
					PackagingReceptacles,
					PackingCosts,
					MaterialsComponents,
					ToolsMatricesMolds,
					MaterialsConsumedProduction,
					EngineeringProjects,
					RoyaltiesLicenseRights,
					ValueInstallment,
					InternalFreightExportingCountry,
					InternalInsuranceExportingCountry,
					LoadingUnloadingHandlingExportingCountry,
					InternationalLoadingUnloadingHandling,
					LoadingUnloadingHandlingEntranceImportingCountry,
					InternalFreightImportingCountry,
					InternalInsuranceImportingCountry,
					LoadingUnloadingHandlingImportingCountry,
					RightsOtherTaxes,
					FinancingInterest,
					ConstructionInstallationAssembly,
					OtherDeductionsCustomsValue,
					OtherExpensesICMS,
				};
			}
		}

		public static IEnumerable<string> ConfiguredIncoTerms => new BRIncoTermList().GetAllCodes();

		public static ChargeConfiguration GetChargeConfiguration() => new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false };

		public static bool IsDeductions(ZString chargeCode)
		{
			switch (chargeCode)
			{
				case ChargeTypes.InternalFreightImportingCountry:
				case ChargeTypes.InternalInsuranceImportingCountry:
				case ChargeTypes.LoadingUnloadingHandlingImportingCountry:
				case ChargeTypes.RightsOtherTaxes:
				case ChargeTypes.FinancingInterest:
				case ChargeTypes.ConstructionInstallationAssembly:
				case ChargeTypes.OtherDeductionsCustomsValue:
					return true;
				default:
					return false;
			}
		}

		public static bool IsAdditions(ZString chargeCode)
		{
			switch (chargeCode)
			{
				case ChargeTypes.OtherAdditionsCustomsValue:
				case ChargeTypes.CommissionsBrokerage:
				case ChargeTypes.EngineeringProjects:
				case ChargeTypes.InternalFreightExportingCountry:
				case ChargeTypes.InternationalLoadingUnloadingHandling:
				case ChargeTypes.ValueInstallment:
				case ChargeTypes.InternalInsuranceExportingCountry:
				case ChargeTypes.LoadingUnloadingHandlingEntranceImportingCountry:
				case ChargeTypes.LoadingUnloadingHandlingExportingCountry:
				case ChargeTypes.MaterialsComponents:
				case ChargeTypes.MaterialsConsumedProduction:
				case ChargeTypes.PackingCosts:
				case ChargeTypes.PackagingReceptacles:
				case ChargeTypes.RoyaltiesLicenseRights:
				case ChargeTypes.ToolsMatricesMolds:
					return true;
				default:
					return false;
			}
		}

		public static bool IsAdditionsOrDeductions(ZString chargeCode) => IsAdditions(chargeCode) || IsDeductions(chargeCode);

		#region CustomsChargeCodes

		public static CustomsChargeCode CommissionsBrokerage => commissionsBrokerage ?? (commissionsBrokerage = new CustomsChargeCode(ChargeTypes.CommissionsBrokerage, ImportCustomsChargeTypeList.Descriptions.CommissionsBrokerage)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInITOTDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.FOB,
			IsIncoTermNeutral = true,
		});
		[ThreadStatic]
		static CustomsChargeCode commissionsBrokerage;

		public static CustomsChargeCode PackagingReceptacles => packagingReceptacles ?? (packagingReceptacles = new CustomsChargeCode(ChargeTypes.PackagingReceptacles, ImportCustomsChargeTypeList.Descriptions.PackagingReceptacles)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInITOTDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.FOB,
			IsIncoTermNeutral = true,
		});
		[ThreadStatic]
		static CustomsChargeCode packagingReceptacles;

		public static CustomsChargeCode PackingCosts => packingCosts ?? (packingCosts = new CustomsChargeCode(ChargeTypes.PackingCosts, ImportCustomsChargeTypeList.Descriptions.PackingCosts)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInITOTDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.FOB,
			IsIncoTermNeutral = true,
		});
		[ThreadStatic]
		static CustomsChargeCode packingCosts;

		public static CustomsChargeCode MaterialsComponents => materialsComponents ?? (materialsComponents = new CustomsChargeCode(ChargeTypes.MaterialsComponents, ImportCustomsChargeTypeList.Descriptions.MaterialsComponents)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInITOTDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.FOB,
			IsIncoTermNeutral = true,
		});
		[ThreadStatic]
		static CustomsChargeCode materialsComponents;

		public static CustomsChargeCode ToolsMatricesMolds => toolsMatricesMolds ?? (toolsMatricesMolds = new CustomsChargeCode(ChargeTypes.ToolsMatricesMolds, ImportCustomsChargeTypeList.Descriptions.ToolsMatricesMolds)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInITOTDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.FOB,
			IsIncoTermNeutral = true,
		});
		[ThreadStatic]
		static CustomsChargeCode toolsMatricesMolds;

		public static CustomsChargeCode MaterialsConsumedProduction => materialsConsumedProduction ?? (materialsConsumedProduction = new CustomsChargeCode(ChargeTypes.MaterialsConsumedProduction, ImportCustomsChargeTypeList.Descriptions.MaterialsConsumedProduction)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInITOTDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.FOB,
			IsIncoTermNeutral = true,
		});
		[ThreadStatic]
		static CustomsChargeCode materialsConsumedProduction;

		public static CustomsChargeCode EngineeringProjects => engineeringProjects ?? (engineeringProjects = new CustomsChargeCode(ChargeTypes.EngineeringProjects, ImportCustomsChargeTypeList.Descriptions.EngineeringProjects)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInITOTDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.FOB,
			IsIncoTermNeutral = true,
		});
		[ThreadStatic]
		static CustomsChargeCode engineeringProjects;

		public static CustomsChargeCode RoyaltiesLicenseRights => royaltiesLicenseRights ?? (royaltiesLicenseRights = new CustomsChargeCode(ChargeTypes.RoyaltiesLicenseRights, ImportCustomsChargeTypeList.Descriptions.RoyaltiesLicenseRights)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInITOTDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.FOB,
			IsIncoTermNeutral = true,
		});
		[ThreadStatic]
		static CustomsChargeCode royaltiesLicenseRights;

		public static CustomsChargeCode ValueInstallment => valueInstallment ?? (valueInstallment = new CustomsChargeCode(ChargeTypes.ValueInstallment, ImportCustomsChargeTypeList.Descriptions.ValueInstallment)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInITOTDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.FOB,
			IsIncoTermNeutral = true,
		});
		[ThreadStatic]
		static CustomsChargeCode valueInstallment;

		public static CustomsChargeCode InternalFreightExportingCountry => internalFreightExportingCountry ?? (internalFreightExportingCountry = new CustomsChargeCode(ChargeTypes.InternalFreightExportingCountry, ImportCustomsChargeTypeList.Descriptions.InternalFreightExportingCountry)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInITOTDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.FOB,
			IsIncoTermNeutral = true,
		});
		[ThreadStatic]
		static CustomsChargeCode internalFreightExportingCountry;

		public static CustomsChargeCode InternalInsuranceExportingCountry => internalInsuranceExportingCountry ?? (internalInsuranceExportingCountry = new CustomsChargeCode(ChargeTypes.InternalInsuranceExportingCountry, ImportCustomsChargeTypeList.Descriptions.InternalInsuranceExportingCountry)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInITOTDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.FOB,
			IsIncoTermNeutral = true,
		});
		[ThreadStatic]
		static CustomsChargeCode internalInsuranceExportingCountry;

		public static CustomsChargeCode LoadingUnloadingHandlingExportingCountry => loadingUnloadingHandlingExportingCountry ?? (loadingUnloadingHandlingExportingCountry = new CustomsChargeCode(ChargeTypes.LoadingUnloadingHandlingExportingCountry, ImportCustomsChargeTypeList.Descriptions.LoadingUnloadingHandlingExportingCountry)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInITOTDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.FOB,
			IsIncoTermNeutral = true,
		});
		[ThreadStatic]
		static CustomsChargeCode loadingUnloadingHandlingExportingCountry;

		public static CustomsChargeCode InternationalLoadingUnloadingHandling => internationalLoadingUnloadingHandling ?? (internationalLoadingUnloadingHandling = new CustomsChargeCode(ChargeTypes.InternationalLoadingUnloadingHandling, ImportCustomsChargeTypeList.Descriptions.InternationalLoadingUnloadingHandling)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInITOTDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.FOB,
			IsIncoTermNeutral = true,
		});
		[ThreadStatic]
		static CustomsChargeCode internationalLoadingUnloadingHandling;

		public static CustomsChargeCode LoadingUnloadingHandlingEntranceImportingCountry => loadingUnloadingHandlingEntranceImportingCountry ?? (loadingUnloadingHandlingEntranceImportingCountry = new CustomsChargeCode(ChargeTypes.LoadingUnloadingHandlingEntranceImportingCountry, ImportCustomsChargeTypeList.Descriptions.LoadingUnloadingHandlingEntranceImportingCountry)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInITOTDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.FOB,
			IsIncoTermNeutral = true,
		});
		[ThreadStatic]
		static CustomsChargeCode loadingUnloadingHandlingEntranceImportingCountry;

		public static CustomsChargeCode OtherAdditionsCustomsValue => otherAdditionsCustomsValue ?? (otherAdditionsCustomsValue = new CustomsChargeCode(ChargeTypes.OtherAdditionsCustomsValue, ImportCustomsChargeTypeList.Descriptions.OtherAdditionsCustomsValue)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInITOTDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.FOB,
			IsIncoTermNeutral = true,
		});
		[ThreadStatic]
		static CustomsChargeCode otherAdditionsCustomsValue;

		public static CustomsChargeCode InternalFreightImportingCountry => internalFreightImportingCountry ?? (internalFreightImportingCountry = new CustomsChargeCode(ChargeTypes.InternalFreightImportingCountry, ImportCustomsChargeTypeList.Descriptions.InternalFreightImportingCountry)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInITOTDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.FOB,
			IsIncoTermNeutral = true,
		});
		[ThreadStatic]
		static CustomsChargeCode internalFreightImportingCountry;

		public static CustomsChargeCode InternalInsuranceImportingCountry => internalInsuranceImportingCountry ?? (internalInsuranceImportingCountry = new CustomsChargeCode(ChargeTypes.InternalInsuranceImportingCountry, ImportCustomsChargeTypeList.Descriptions.InternalInsuranceImportingCountry)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInITOTDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.FOB,
			IsIncoTermNeutral = true,
		});
		[ThreadStatic]
		static CustomsChargeCode internalInsuranceImportingCountry;

		public static CustomsChargeCode LoadingUnloadingHandlingImportingCountry => loadingUnloadingHandlingImportingCountry ?? (loadingUnloadingHandlingImportingCountry = new CustomsChargeCode(ChargeTypes.LoadingUnloadingHandlingImportingCountry, ImportCustomsChargeTypeList.Descriptions.LoadingUnloadingHandlingImportingCountry)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInITOTDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.FOB,
			IsIncoTermNeutral = true,
		});
		[ThreadStatic]
		static CustomsChargeCode loadingUnloadingHandlingImportingCountry;

		public static CustomsChargeCode RightsOtherTaxes => rightsOtherTaxes ?? (rightsOtherTaxes = new CustomsChargeCode(ChargeTypes.RightsOtherTaxes, ImportCustomsChargeTypeList.Descriptions.RightsOtherTaxes)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInITOTDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.FOB,
			IsIncoTermNeutral = true,
		});
		[ThreadStatic]
		static CustomsChargeCode rightsOtherTaxes;

		public static CustomsChargeCode FinancingInterest => financingInterest ?? (financingInterest = new CustomsChargeCode(ChargeTypes.FinancingInterest, ImportCustomsChargeTypeList.Descriptions.FinancingInterest)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInITOTDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.FOB,
			IsIncoTermNeutral = true,
		});
		[ThreadStatic]
		static CustomsChargeCode financingInterest;

		public static CustomsChargeCode ConstructionInstallationAssembly => constructionInstallationAssembly ?? (constructionInstallationAssembly = new CustomsChargeCode(ChargeTypes.ConstructionInstallationAssembly, ImportCustomsChargeTypeList.Descriptions.ConstructionInstallationAssembly)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInITOTDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.FOB,
			IsIncoTermNeutral = true,
		});
		[ThreadStatic]
		static CustomsChargeCode constructionInstallationAssembly;

		public static CustomsChargeCode OtherDeductionsCustomsValue => otherDeductionsCustomsValue ?? (otherDeductionsCustomsValue = new CustomsChargeCode(ChargeTypes.OtherDeductionsCustomsValue, ImportCustomsChargeTypeList.Descriptions.OtherDeductionsCustomsValue)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInITOTDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.FOB,
			IsIncoTermNeutral = true,
		});
		[ThreadStatic]
		static CustomsChargeCode otherDeductionsCustomsValue;

		public static CustomsChargeCode FreightComponents => freightComponents ?? (freightComponents = new CustomsChargeCode(ChargeTypes.FreightComponents, ImportCustomsChargeTypeList.Descriptions.FreightComponents)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = false,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInITOTDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.NetWeight,
			IsIncoTermNeutral = false,
			IsPercentageApplicable = false,
		});
		[ThreadStatic]
		static CustomsChargeCode freightComponents;

		public static CustomsChargeCode FreightInNationalTerritory => freightInNationalTerritory ?? (freightInNationalTerritory = new CustomsChargeCode(ChargeTypes.FreightInNationalTerritory, ImportCustomsChargeTypeList.Descriptions.FreightInNationalTerritory)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = false,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = false,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInITOTDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.NetWeight,
			IsIncoTermNeutral = false,
		});
		[ThreadStatic]
		static CustomsChargeCode freightInNationalTerritory;

		public static CustomsChargeCode OverseasInsurance => overseasInsurance ?? (overseasInsurance = new CustomsChargeCode(CustomsChargeTypeList.Codes.OverseasInsurance, CustomsChargeTypeList.Descriptions.OverseasInsurance)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInITOTDeemedForThisCharge = false,
			DistributeBy = ChargeDistributeByList.Codes.FOB,
		});
		[ThreadStatic]
		static CustomsChargeCode overseasInsurance;

		public static CustomsChargeCode OtherExpensesICMS => otherExpensesICMS ?? (otherExpensesICMS = new CustomsChargeCode(ImportCustomsChargeTypeList.Codes.OtherExpensesICMS, ImportCustomsChargeTypeList.Descriptions.OtherExpensesICMS)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = false,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInITOTDeemedForThisCharge = true,
			DistributeBy = Common.ChargeDistributeByList.Codes.Value,
			IsIncoTermNeutral = false,
			IsPercentageApplicable = false,
		});
		[ThreadStatic]
		static CustomsChargeCode otherExpensesICMS;

		#endregion
	}
}
