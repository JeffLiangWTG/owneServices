using System.Linq;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.CN.Business
{
	public class ImportIncoTermAndCustomsChargeFactory : IncoTermAndCustomsChargeFactory
	{
		public static CustomsChargeCode Royalty => new CustomsChargeCode(CustomsChargeTypeList.Codes.Royalty, CustomsChargeTypeList.Descriptions.Royalty)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = false,
			IsVATibleDeemedForThisCharge = true,
			IsIncoTermNeutral = true
		};

		public new static CustomsChargeCode OverseasFreight => new CustomsChargeCode(CustomsChargeTypeList.Codes.OverseasFreight, CustomsChargeTypeList.Descriptions.OverseasFreight)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsPercentageApplicable = true,
			IsIncludedInITOTIfDeemed = false
		};

		public new static CustomsChargeCode OverseasInsurance => new CustomsChargeCode(CustomsChargeTypeList.Codes.OverseasInsurance, CustomsChargeTypeList.Descriptions.OverseasInsurance)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsPercentageApplicable = true,
			IsIncludedInITOTIfDeemed = false
		};

		public override CustomsChargeCode GetOverseasFreight() => OverseasFreight;

		public override CustomsChargeCode GetOverseasInsurance() => OverseasInsurance;

		protected override void SetupCostAndInsuranceConfiguration()
		{
			base.SetupCostAndInsuranceConfiguration();
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndInsurance, GetRoyaltyCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupCostInsuranceAndFreightConfiguration()
		{
			base.SetupCostInsuranceAndFreightConfiguration();
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, GetRoyaltyCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupCostAndFreightConfiguration()
		{
			base.SetupCostAndFreightConfiguration();
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, GetRoyaltyCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false }, ignoreExisting: true);
		}

		protected override void SetupFreeOnBoardConfiguration()
		{
			base.SetupFreeOnBoardConfiguration();
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, GetRoyaltyCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false }, ignoreExisting: true);
		}

		protected override void SetupExWorksConfiguration()
		{
			base.SetupExWorksConfiguration();
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, GetRoyaltyCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false }, ignoreExisting: true);
		}

		protected override ICustomsChargeCode[] GetCharges()
		{
			return base.GetCharges().Append(GetRoyaltyCharges()).ToArray();
		}

		public CustomsChargeCode GetRoyaltyCharges() => Royalty;
	}
}
