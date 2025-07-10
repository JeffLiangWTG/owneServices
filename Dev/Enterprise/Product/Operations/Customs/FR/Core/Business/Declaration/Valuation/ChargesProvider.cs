using System;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public static class ChargesProvider
	{
		public static CustomsChargeCode Adjustment => adjustment ?? (adjustment = new FlagManagedCharge(UCCCustomsChargeTypeList.Codes.AdjustmentCharge, UCCCustomsChargeTypeList.Descriptions.AdjustmentCharge)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine,
			IsIncludedInITOTDeemedForThisCharge = false,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInInvoice = false
		});
		[ThreadStatic]
		static CustomsChargeCode adjustment;

		public static CustomsChargeCode BuyingCommissions => buyingCommissions ?? (buyingCommissions = new FlagManagedCharge(UCCCustomsChargeTypeList.Codes.BuyingCommissionsCharge, UCCCustomsChargeTypeList.Descriptions.BuyingCommissionsCharge)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice,
			IsIncludedInITOTDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = true,
			IsIncludedInInvoice = true,
			IsIncludedInInvoiceDeemedForThisCharge = true
		});
		[ThreadStatic]
		static CustomsChargeCode buyingCommissions;

		public static CustomsChargeCode CommissionExceptBuyingCommissions => commissionExceptBuyingCommissions ?? (commissionExceptBuyingCommissions = new FlagManagedCharge(UCCCustomsChargeTypeList.Codes.CommissionExceptBuyingCommissionsCharge, UCCCustomsChargeTypeList.Descriptions.CommissionExceptBuyingCommissionsCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine,
			IsIncludedInITOTDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInInvoice = false
		});
		[ThreadStatic]
		static CustomsChargeCode commissionExceptBuyingCommissions;

		public static CustomsChargeCode ConstructionErectionAssembly => constructionErectionAssembly ?? (constructionErectionAssembly = new FlagManagedCharge(UCCCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge, UCCCustomsChargeTypeList.Descriptions.ConstructionErectionAssemblyCharge)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine,
			IsIncludedInITOTDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = true,
			IsIncludedInInvoice = true
		});
		[ThreadStatic]
		static CustomsChargeCode constructionErectionAssembly;

		public static CustomsChargeCode ContainersAndPacking => containersAndPacking ?? (containersAndPacking = new FlagManagedCharge(UCCCustomsChargeTypeList.Codes.ContainersAndPackingCharge, UCCCustomsChargeTypeList.Descriptions.ContainersAndPackingCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
			ParentTypes = ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine,
			IsIncludedInITOTDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInInvoice = false
		});
		[ThreadStatic]
		static CustomsChargeCode containersAndPacking;

		public static CustomsChargeCode EngineeringDevelopmentArtwork => engineeringDevelopmentArtwork ?? (engineeringDevelopmentArtwork = new FlagManagedCharge(UCCCustomsChargeTypeList.Codes.EngineeringDevelopmentArtworkCharge, UCCCustomsChargeTypeList.Descriptions.EngineeringDevelopmentArtworkCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice,
			IsIncludedInITOTDeemedForThisCharge = false,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInInvoice = false
		});
		[ThreadStatic]
		static CustomsChargeCode engineeringDevelopmentArtwork;

		public static CustomsChargeCode ImportDutiesOrOther => importDutiesOrOther ?? (importDutiesOrOther = new FlagManagedCharge(UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge, UCCCustomsChargeTypeList.Descriptions.ImportDutiesOrOtherCharge)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = false,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine,
			IsIncludedInITOTDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = true,
			IsIncludedInInvoice = true
		});
		[ThreadStatic]
		static CustomsChargeCode importDutiesOrOther;

		public static CustomsChargeCode Interest => interest ?? (interest = new FlagManagedCharge(UCCCustomsChargeTypeList.Codes.InterestCharge, UCCCustomsChargeTypeList.Descriptions.InterestCharge)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice,
			IsIncludedInITOTDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = true,
			IsIncludedInInvoice = true
		});
		[ThreadStatic]
		static CustomsChargeCode interest;

		public static CustomsChargeCode MaterialsConsumed => materialsConsumed ?? (materialsConsumed = new FlagManagedCharge(UCCCustomsChargeTypeList.Codes.MaterialsConsumedCharge, UCCCustomsChargeTypeList.Descriptions.MaterialsConsumedCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice,
			IsIncludedInITOTDeemedForThisCharge = false,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInInvoice = false
		});
		[ThreadStatic]
		static CustomsChargeCode materialsConsumed;

		public static CustomsChargeCode MaterialsComponentsParts => materialsComponentsParts ?? (materialsComponentsParts = new FlagManagedCharge(UCCCustomsChargeTypeList.Codes.MaterialsComponentsPartsCharge, UCCCustomsChargeTypeList.Descriptions.MaterialsComponentsPartsCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice,
			IsIncludedInITOTDeemedForThisCharge = false,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInInvoice = false
		});
		[ThreadStatic]
		static CustomsChargeCode materialsComponentsParts;

		public static CustomsChargeCode ProceedsOfAnySubsequentResale => proceedsOfAnySubsequentResale ?? (proceedsOfAnySubsequentResale = new FlagManagedCharge(UCCCustomsChargeTypeList.Codes.ProceedsOfAnySubsequentResaleCharge, UCCCustomsChargeTypeList.Descriptions.ProceedsOfAnySubsequentResaleCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine,
			IsIncludedInITOTDeemedForThisCharge = false,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInInvoice = false
		});
		[ThreadStatic]
		static CustomsChargeCode proceedsOfAnySubsequentResale;

		public static CustomsChargeCode RoyaltiesLicenseFee => royaltiesLicenseFee ?? (royaltiesLicenseFee = new FlagManagedCharge(UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge, UCCCustomsChargeTypeList.Descriptions.RoyaltiesLicenseFeeCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
			ParentTypes = ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine,
			IsIncludedInITOTDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInInvoice = false
		});
		[ThreadStatic]
		static CustomsChargeCode royaltiesLicenseFee;

		public static CustomsChargeCode AirInsuranceCosts => airInsuranceCosts ?? (airInsuranceCosts = new CustomsChargeCode(FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge, FRCustomsChargeTypeList.Descriptions.AirInsuranceCostsCharge)
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
		static CustomsChargeCode airInsuranceCosts;

		public static CustomsChargeCode InsuranceCosts => insuranceCosts ?? (insuranceCosts = new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, UCCCustomsChargeTypeList.Descriptions.InsuranceCostsCharge)
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
		static CustomsChargeCode insuranceCosts;

		public static CustomsChargeCode ExclusiveFreightInsideEU => exclusiveFreightInsideEU ?? (exclusiveFreightInsideEU = new CustomsChargeCode(FRCustomsChargeTypeList.Codes.ExclusiveFreightInsideEU, FRCustomsChargeTypeList.Descriptions.ExclusiveFreightInsideEU)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = false,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = false,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice,
		});
		[ThreadStatic]
		static CustomsChargeCode exclusiveFreightInsideEU;

		public static CustomsChargeCode ExclusiveInsuranceInsideEU => exclusiveInsuranceInsideEU ?? (exclusiveInsuranceInsideEU = new CustomsChargeCode(FRCustomsChargeTypeList.Codes.ExclusiveInsuranceInsideEU, FRCustomsChargeTypeList.Descriptions.ExclusiveInsuranceInsideEU)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = false,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = false,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice
		});
		[ThreadStatic]
		static CustomsChargeCode exclusiveInsuranceInsideEU;

		public static CustomsChargeCode InclusiveFreightInsideEU => inclusiveFreightInsideEU ?? (inclusiveFreightInsideEU = new CustomsChargeCode(FRCustomsChargeTypeList.Codes.InclusiveFreightInsideEU, FRCustomsChargeTypeList.Descriptions.InclusiveFreightInsideEU)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = false,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = false,
			IsIncoTermNeutral = false,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice
		});
		[ThreadStatic]
		static CustomsChargeCode inclusiveFreightInsideEU;

		public static CustomsChargeCode InclusiveInsuranceInsideEU => inclusiveInsuranceInsideEU ?? (inclusiveInsuranceInsideEU = new CustomsChargeCode(FRCustomsChargeTypeList.Codes.InclusiveInsuranceInsideEU, FRCustomsChargeTypeList.Descriptions.InclusiveInsuranceInsideEU)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = false,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = false,
			IsIncoTermNeutral = false,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice
		});
		[ThreadStatic]
		static CustomsChargeCode inclusiveInsuranceInsideEU;

		public static CustomsChargeCode InclusiveFreightFromFrenchBorder => inclusiveFreightFromFrenchBorder ?? (inclusiveFreightFromFrenchBorder = new CustomsChargeCode(FRCustomsChargeTypeList.Codes.InclusiveFreightFromFrenchBorder, FRCustomsChargeTypeList.Descriptions.InclusiveFreightFromFrenchBorder)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = false,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = false,
			IsIncoTermNeutral = false,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice
		});
		[ThreadStatic]
		static CustomsChargeCode inclusiveFreightFromFrenchBorder;

		public static CustomsChargeCode InclusiveInsuranceFromFrenchBorder => inclusiveInsuranceFromFrenchBorder ?? (inclusiveInsuranceFromFrenchBorder = new CustomsChargeCode(FRCustomsChargeTypeList.Codes.InclusiveInsuranceFromFrenchBorder, FRCustomsChargeTypeList.Descriptions.InclusiveInsuranceFromFrenchBorder)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = false,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = false,
			IsIncoTermNeutral = false,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice
		});
		[ThreadStatic]
		static CustomsChargeCode inclusiveInsuranceFromFrenchBorder;

		public static CustomsChargeCode ExclusiveFreightToFrenchDestination => exclusiveFreightToFrenchDestination ?? (exclusiveFreightToFrenchDestination = new CustomsChargeCode(FRCustomsChargeTypeList.Codes.ExclusiveFreightToFrenchDestination, FRCustomsChargeTypeList.Descriptions.ExclusiveFreightToFrenchDestination)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = false,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = false,
			IsIncoTermNeutral = false,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice
		});
		[ThreadStatic]
		static CustomsChargeCode exclusiveFreightToFrenchDestination;

		public static CustomsChargeCode ExclusiveInsuranceToFrenchDestination => exclusiveInsuranceToFrenchDestination ?? (exclusiveInsuranceToFrenchDestination = new CustomsChargeCode(FRCustomsChargeTypeList.Codes.ExclusiveInsuranceToFrenchDestination, FRCustomsChargeTypeList.Descriptions.ExclusiveInsuranceToFrenchDestination)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = false,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = false,
			IsIncoTermNeutral = false,
			IsPercentageApplicable = false,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice
		});
		[ThreadStatic]
		static CustomsChargeCode exclusiveInsuranceToFrenchDestination;

		public static CustomsChargeCode Cut => cut ?? (cut = new FlagManagedCharge(FRCustomsChargeTypeList.Codes.Cut, FRCustomsChargeTypeList.Descriptions.Cut)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = false,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = true,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true,
			IsIncludedInITOTDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = true,
			IsIncludedInInvoice = false,
			ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice
		});
		[ThreadStatic]
		static CustomsChargeCode cut;
	}
}
