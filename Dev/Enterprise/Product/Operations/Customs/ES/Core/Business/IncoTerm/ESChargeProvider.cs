using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.Business
{
	public static class ESChargeProvider
	{
		public static CustomsChargeCode GetNewInternationalFreight() => new CustomsChargeCode(ESCustomsChargeTypeList.Codes.InternationalFreight, ESCustomsChargeTypeList.Descriptions.InternationalFreight)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false
		};

		public static CustomsChargeCode GetNewTransportCostsAfterEUEntry() => new CustomsChargeCode(ESCustomsChargeTypeList.Codes.TransportCostsAfterEUEntry, ESCustomsChargeTypeList.Descriptions.TransportCostsAfterEUEntry)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = false,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = false,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false
		};

		public static CustomsChargeCode GetNewUnloadingOfGoods() => new CustomsChargeCode(ESCustomsChargeTypeList.Codes.UnloadingOfGoods, ESCustomsChargeTypeList.Descriptions.UnloadingOfGoods)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = false,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false
		};

		public static CustomsChargeCode GetNewPortTransitFee() => new CustomsChargeCode(ESCustomsChargeTypeList.Codes.PortTransitFee, ESCustomsChargeTypeList.Descriptions.PortTransitFee)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = false,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false
		};

		public static CustomsChargeCode GetNewTerminalHandling() => new CustomsChargeCode(ESCustomsChargeTypeList.Codes.TerminalHandlingCharge, ESCustomsChargeTypeList.Descriptions.TerminalHandlingCharge)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = false,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false
		};

		public static CustomsChargeCode GetNewIndirectAndOtherPayments() => new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.IndirectAndOtherPaymentsCharge, UCCCustomsChargeTypeList.Descriptions.IndirectAndOtherPaymentsCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = false,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = false,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false
		};

		public static CustomsChargeCode GetNewCommissionAndBrokerage() => new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge, UCCCustomsChargeTypeList.Descriptions.CommissionAndBrokerageCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = false,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true
		};

		public static CustomsChargeCode GetNewContainersAndPacking() => new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.ContainersAndPackingCharge, UCCCustomsChargeTypeList.Descriptions.ContainersAndPackingCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = false,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = false,
			IsPercentageApplicable = false
		};

		public static CustomsChargeCode GetNewMaterialsComponentsParts() => new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.MaterialsComponentsPartsCharge, UCCCustomsChargeTypeList.Descriptions.MaterialsComponentsPartsCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = false,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false
		};

		public static CustomsChargeCode GetNewToolsMiesMoulds() => new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.ToolsMiesMouldsCharge, UCCCustomsChargeTypeList.Descriptions.ToolsMiesMouldsCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = false,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true
		};

		public static CustomsChargeCode GetNewMaterialsConsumed() => new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.MaterialsConsumedCharge, UCCCustomsChargeTypeList.Descriptions.MaterialsConsumedCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = false,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false
		};

		public static CustomsChargeCode GetNewEngineeringDevelopmentArtwork() => new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.EngineeringDevelopmentArtworkCharge, UCCCustomsChargeTypeList.Descriptions.EngineeringDevelopmentArtworkCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = false,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true
		};

		public static CustomsChargeCode GetNewRoyaltiesLicenseFee() => new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge, UCCCustomsChargeTypeList.Descriptions.RoyaltiesLicenseFeeCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = false,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true
		};

		public static CustomsChargeCode GetNewProceedsOfAnySubsequentResale() => new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.ProceedsOfAnySubsequentResaleCharge, UCCCustomsChargeTypeList.Descriptions.ProceedsOfAnySubsequentResaleCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = false,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false
		};

		public static CustomsChargeCode GetNewTransportCosts() => new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, UCCCustomsChargeTypeList.Descriptions.TransportCostsCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = false,
			IsIncoTermNeutral = false,
			IsPercentageApplicable = false
		};

		public static CustomsChargeCode GetNewInsuranceCosts() => new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, UCCCustomsChargeTypeList.Descriptions.InsuranceCostsCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true
		};

		public static CustomsChargeCode GetNewConstructionErectionAssembly() => new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge, UCCCustomsChargeTypeList.Descriptions.ConstructionErectionAssemblyCharge)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = false,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false
		};

		public static CustomsChargeCode GetNewImportDutiesOrOther() => new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge, UCCCustomsChargeTypeList.Descriptions.ImportDutiesOrOtherCharge)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = false,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = false,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.InvoiceLine
		};

		public static CustomsChargeCode GetNewInternationalFreightExp() => new CustomsChargeCode(ESCustomsChargeTypeList.Codes.InternationalFreightExp, ESCustomsChargeTypeList.Descriptions.InternationalFreightExp)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
			ChargeOperationType = ChargeCodeOperationType.Deducted,
			IsForExport = true,
			IsIncludedInITOTDeemedForThisCharge = true
		};

		public static CustomsChargeCode GetNewTransportCostsUntilESBorder() => new CustomsChargeCode(ESCustomsChargeTypeList.Codes.TransportCostsUntilESBorder, ESCustomsChargeTypeList.Descriptions.TransportCostsUntilESBorder)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = false,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
			ChargeOperationType = ChargeCodeOperationType.Added,
			IsForExport = true,
			IsIncludedInITOTDeemedForThisCharge = true
		};

		public static CustomsChargeCode GetNewInsuranceUntilESBorder() => new CustomsChargeCode(ESCustomsChargeTypeList.Codes.InsuranceUntilESBorder, ESCustomsChargeTypeList.Descriptions.InsuranceUntilESBorder)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = false,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
			ChargeOperationType = ChargeCodeOperationType.Added,
			IsForExport = true,
			IsIncludedInITOTDeemedForThisCharge = true
		};

		public static CustomsChargeCode GetNewOtherInternationalPayments() => new CustomsChargeCode(ESCustomsChargeTypeList.Codes.OtherInternationalPayments, ESCustomsChargeTypeList.Descriptions.OtherInternationalPayments)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
			ChargeOperationType = ChargeCodeOperationType.Deducted,
			IsForExport = true,
			IsIncludedInITOTDeemedForThisCharge = true
		};

		public static CustomsChargeCode GetNewOtherNationalPayments() => new CustomsChargeCode(ESCustomsChargeTypeList.Codes.OtherNationalPayments, ESCustomsChargeTypeList.Descriptions.OtherNationalPayments)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = false,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
			ChargeOperationType = ChargeCodeOperationType.Added,
			IsForExport = true,
			IsIncludedInITOTDeemedForThisCharge = true
		};

		public static CustomsChargeCode GetNewAdjustment() => new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.AdjustmentCharge, UCCCustomsChargeTypeList.Descriptions.AdjustmentCharge)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false
		};

		public static CustomsChargeCode GetNewAirTransportCosts() => new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge, UCCCustomsChargeTypeList.Descriptions.AirTransportCostsCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = false,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = false,
			IsIncoTermNeutral = false,
			IsPercentageApplicable = false
		};

		public static CustomsChargeCode GetNewTransportCostsExp() => new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, UCCCustomsChargeTypeList.Descriptions.TransportCostsCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
			ChargeOperationType = ChargeCodeOperationType.Deducted,
			IsForExport = true,
			IsIncludedInITOTDeemedForThisCharge = true
		};

		public static CustomsChargeCode GetNewInsuranceCostsExp() => new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, UCCCustomsChargeTypeList.Descriptions.InsuranceCostsCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
			ChargeOperationType = ChargeCodeOperationType.Deducted,
			IsForExport = true,
			IsIncludedInITOTDeemedForThisCharge = true
		};

		public static CustomsChargeCode GetNewIndirectAndOtherPaymentsExp() => new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.IndirectAndOtherPaymentsCharge, UCCCustomsChargeTypeList.Descriptions.IndirectAndOtherPaymentsCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = false,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = false,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
			IsForExport = true
		};

		public static CustomsChargeCode GetNewExportedGoodsValueForOutwardProcessing() => new CustomsChargeCode(ESCustomsChargeTypeList.Codes.ExportedGoodsValueForOutwardProcessing, ESCustomsChargeTypeList.Descriptions.ExportedGoodsValueForOutwardProcessing)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = false,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = false,
			IsPercentageApplicable = false,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInITOTDeemedForThisCharge = true,
			ParentTypes = ChargeParentTypes.InvoiceLine
		};

		public static CustomsChargeCode GetNewInvoicedExportedGoodsValueForOutwardProcessing() => new CustomsChargeCode(ESCustomsChargeTypeList.Codes.InvoicedExportedGoodsValueForOutwardProcessing, ESCustomsChargeTypeList.Descriptions.InvoicedExportedGoodsValueForOutwardProcessing)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = false,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = false,
			IsPercentageApplicable = false,
			IsIncludedInITOTIfDeemed = true,
			IsIncludedInITOTDeemedForThisCharge = true,
			ParentTypes = ChargeParentTypes.InvoiceLine
		};

		public static CustomsChargeCode GetNewReaAidAmountIgicBaseCalculation() => new CustomsChargeCode(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, ESCustomsChargeTypeList.Descriptions.ReaAidAmountIgicBaseCalculation)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = false,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = false,
			IsPercentageApplicable = false,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInITOTDeemedForThisCharge = true,
			ParentTypes = ChargeParentTypes.InvoiceLine
		};
	}
}
