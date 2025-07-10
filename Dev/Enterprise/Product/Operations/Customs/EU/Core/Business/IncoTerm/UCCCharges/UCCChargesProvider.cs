using System;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.Business
{
	public static class UCCChargesProvider
	{
		public static CustomsChargeCode CommissionAndBrokerage => commissionAndBrokerage ?? (commissionAndBrokerage = new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge, UCCCustomsChargeTypeList.Descriptions.CommissionAndBrokerageCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true
		});
		[ThreadStatic]
		static CustomsChargeCode commissionAndBrokerage;

		public static CustomsChargeCode CommissionExceptBuyingCommissions => commissionExceptBuyingCommissions ?? (commissionExceptBuyingCommissions = new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.CommissionExceptBuyingCommissionsCharge, UCCCustomsChargeTypeList.Descriptions.CommissionExceptBuyingCommissionsCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = false,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true
		});
		[ThreadStatic]
		static CustomsChargeCode commissionExceptBuyingCommissions;

		public static CustomsChargeCode ContainersAndPacking => containersAndPacking ?? (containersAndPacking = new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.ContainersAndPackingCharge, UCCCustomsChargeTypeList.Descriptions.ContainersAndPackingCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = false,
			IsPercentageApplicable = false
		});
		[ThreadStatic]
		static CustomsChargeCode containersAndPacking;

		public static CustomsChargeCode MaterialsComponentsParts => materialsComponentsParts ?? (materialsComponentsParts = new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.MaterialsComponentsPartsCharge, UCCCustomsChargeTypeList.Descriptions.MaterialsComponentsPartsCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false
		});
		[ThreadStatic]
		static CustomsChargeCode materialsComponentsParts;

		public static CustomsChargeCode ToolsMiesMoulds => toolsMiesMoulds ?? (toolsMiesMoulds = new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.ToolsMiesMouldsCharge, UCCCustomsChargeTypeList.Descriptions.ToolsMiesMouldsCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true
		});
		[ThreadStatic]
		static CustomsChargeCode toolsMiesMoulds;

		public static CustomsChargeCode MaterialsConsumed => materialsConsumed ?? (materialsConsumed = new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.MaterialsConsumedCharge, UCCCustomsChargeTypeList.Descriptions.MaterialsConsumedCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false
		});
		[ThreadStatic]
		static CustomsChargeCode materialsConsumed;

		public static CustomsChargeCode EngineeringDevelopmentArtwork => engineeringDevelopmentArtwork ?? (engineeringDevelopmentArtwork = new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.EngineeringDevelopmentArtworkCharge, UCCCustomsChargeTypeList.Descriptions.EngineeringDevelopmentArtworkCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true
		});
		[ThreadStatic]
		static CustomsChargeCode engineeringDevelopmentArtwork;

		public static CustomsChargeCode RoyaltiesLicenseFee => royaltiesLicenseFee ?? (royaltiesLicenseFee = new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge, UCCCustomsChargeTypeList.Descriptions.RoyaltiesLicenseFeeCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = false,
			IsVATibleDeemedForThisCharge = false,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true
		});
		[ThreadStatic]
		static CustomsChargeCode royaltiesLicenseFee;

		public static CustomsChargeCode ProceedsOfAnySubsequentResale => proceedsOfAnySubsequentResale ?? (proceedsOfAnySubsequentResale = new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.ProceedsOfAnySubsequentResaleCharge, UCCCustomsChargeTypeList.Descriptions.ProceedsOfAnySubsequentResaleCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false
		});
		[ThreadStatic]
		static CustomsChargeCode proceedsOfAnySubsequentResale;

		public static CustomsChargeCode IndirectAndOtherPayments => indirectAndOtherPayments ?? (indirectAndOtherPayments = new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.IndirectAndOtherPaymentsCharge, UCCCustomsChargeTypeList.Descriptions.IndirectAndOtherPaymentsCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = false,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = false,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.GroupInvoice
		});
		[ThreadStatic]
		static CustomsChargeCode indirectAndOtherPayments;

		public static CustomsChargeCode InsuranceCosts => insuranceCosts ?? (insuranceCosts = new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, UCCCustomsChargeTypeList.Descriptions.InsuranceCostsCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.GroupInvoice
		});
		[ThreadStatic]
		static CustomsChargeCode insuranceCosts;

		public static CustomsChargeCode Additions71 => additions71 ?? (additions71 = new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.Additions71Charge, UCCCustomsChargeTypeList.Descriptions.Additions71Charge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
			ParentTypes = ChargeParentTypes.InvoiceLine
		});
		[ThreadStatic]
		static CustomsChargeCode additions71;

		public static CustomsChargeCode TransportCosts => transportCosts ?? (transportCosts = new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, UCCCustomsChargeTypeList.Descriptions.TransportCostsCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = false,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = false,
			IsIncoTermNeutral = false,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.GroupInvoice
		});
		[ThreadStatic]
		static CustomsChargeCode transportCosts;

		public static CustomsChargeCode OtherNotElsewhereDeclared => other ?? (other = new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge, UCCCustomsChargeTypeList.Descriptions.OtherNotElsewhereDeclaredCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false
		});
		[ThreadStatic]
		static CustomsChargeCode other;

		public static CustomsChargeCode Adjustment => adjustment ?? (adjustment = new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.AdjustmentCharge, UCCCustomsChargeTypeList.Descriptions.AdjustmentCharge)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.GroupInvoice
		});
		[ThreadStatic]
		static CustomsChargeCode adjustment;

		public static CustomsChargeCode AirTransportCosts => airTransportCosts ?? (airTransportCosts = new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge, UCCCustomsChargeTypeList.Descriptions.AirTransportCostsCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = false,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = false,
			IsIncoTermNeutral = false,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.GroupInvoice
		});
		[ThreadStatic]
		static CustomsChargeCode airTransportCosts;

		public static CustomsChargeCode ConstructionErectionAssembly => constructionErectionAssembly ?? (constructionErectionAssembly = new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge, UCCCustomsChargeTypeList.Descriptions.ConstructionErectionAssemblyCharge)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false
		});
		[ThreadStatic]
		static CustomsChargeCode constructionErectionAssembly;

		public static CustomsChargeCode ImportDutiesOrOther => importDutiesOrOther ?? (importDutiesOrOther = new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge, UCCCustomsChargeTypeList.Descriptions.ImportDutiesOrOtherCharge)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = false,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.InvoiceLine
		});
		[ThreadStatic]
		static CustomsChargeCode importDutiesOrOther;

		public static CustomsChargeCode Interest => interest ?? (interest = new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.InterestCharge, UCCCustomsChargeTypeList.Descriptions.InterestCharge)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true
		});
		[ThreadStatic]
		static CustomsChargeCode interest;

		public static CustomsChargeCode RightToReproduce => rightToReproduce ?? (rightToReproduce = new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.RightToReproduceCharge, UCCCustomsChargeTypeList.Descriptions.RightToReproduceCharge)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = false,
			IsVATibleDeemedForThisCharge = false,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = false,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.InvoiceLine
		});
		[ThreadStatic]
		static CustomsChargeCode rightToReproduce;

		public static CustomsChargeCode BuyingCommissions => buyingCommissions ?? (buyingCommissions = new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.BuyingCommissionsCharge, UCCCustomsChargeTypeList.Descriptions.BuyingCommissionsCharge)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = false,
			IsVATibleDeemedForThisCharge = false,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = false,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true
		});
		[ThreadStatic]
		static CustomsChargeCode buyingCommissions;

		public static CustomsChargeCode Deductions71 => deductions71 ?? (deductions71 = new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.Deductions71Charge, UCCCustomsChargeTypeList.Descriptions.Deductions71Charge)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = false,
			IsVATibleDeemedForThisCharge = false,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = false,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
			ParentTypes = ChargeParentTypes.InvoiceLine
		});
		[ThreadStatic]
		static CustomsChargeCode deductions71;

		public static CustomsChargeCode DiscountNotElsewhereDeclared => discountNotElsewhereDeclared ?? (discountNotElsewhereDeclared = new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.DiscountNotElsewhereDeclaredCharge, UCCCustomsChargeTypeList.Descriptions.DiscountNotElsewhereDeclaredCharge)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = false,
			IsVATibleDeemedForThisCharge = false,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = false,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true
		});
		[ThreadStatic]
		static CustomsChargeCode discountNotElsewhereDeclared;

		public static CustomsChargeCode DeductionsNotElsewhereDeclared => deductionsNotElsewhereDeclaredCharge ?? (deductionsNotElsewhereDeclaredCharge = new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.DeductionsNotElsewhereDeclaredCharge, UCCCustomsChargeTypeList.Descriptions.DeductionsNotElsewhereDeclaredCharge)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = false,
			IsVATibleDeemedForThisCharge = false,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = false,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false
		});
		[ThreadStatic]
		static CustomsChargeCode deductionsNotElsewhereDeclaredCharge;
	}
}
