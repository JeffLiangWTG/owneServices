using Enterprise.Customs.Common;

namespace Enterprise.Customs.CN.Business
{
	public class IncoTermAndCustomsChargeFactory : CommonIncoTermAndCustomsChargeFactory
	{
		public static CustomsChargeCode OverseasFreight => new CustomsChargeCode(CustomsChargeTypeList.Codes.OverseasFreight, CustomsChargeTypeList.Descriptions.OverseasFreight)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsPercentageApplicable = true,
			IsIncludedInITOTIfDeemed = true
		};

		public static CustomsChargeCode OverseasInsurance => new CustomsChargeCode(CustomsChargeTypeList.Codes.OverseasInsurance, CustomsChargeTypeList.Descriptions.OverseasInsurance)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsPercentageApplicable = true,
			IsIncludedInITOTIfDeemed = true
		};

		protected override void SetupIncotermChargeConfigurations()
		{
			SetupCostAndInsuranceConfiguration();
			SetupCostAndFreightConfiguration();
			SetupCostInsuranceAndFreightConfiguration();
			SetupFreeOnBoardConfiguration();
			SetupExWorksConfiguration();
		}

		protected virtual void SetupCostAndInsuranceConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndInsurance, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndInsurance, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });

			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndInsurance, GetExWorks(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndInsurance, GetForeignInlandFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndInsurance, GetPackingCost(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndInsurance, GetLandingCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndInsurance, GetAdditionCharge(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndInsurance, GetCommission(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndInsurance, GetDeductionCharge(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndInsurance, GetDiscount(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndInsurance, GetOtherCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupCostInsuranceAndFreightConfiguration()
		{
			base.SetupCostInsuranceAndFreightConfiguration();
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, GetOtherCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false }, ignoreExisting: true);
		}

		protected override void SetupCostAndFreightConfiguration()
		{
			base.SetupCostAndFreightConfiguration();
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, GetOtherCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false }, ignoreExisting: true);
		}

		protected override void SetupFreeOnBoardConfiguration()
		{
			base.SetupFreeOnBoardConfiguration();
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, GetOtherCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false }, ignoreExisting: true);
		}

		protected override void SetupExWorksConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });

			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, GetExWorks(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, GetForeignInlandFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, GetPackingCost(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, GetLandingCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, GetAdditionCharge(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, GetCommission(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, GetDeductionCharge(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, GetDiscount(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, GetOtherCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false }, ignoreExisting: true);
		}

		public override bool MakeFlagsReadOnlyWhenDeemed => true;

		public virtual CustomsChargeCode GetAdditionCharge() => CustomsChargeCodeProvider.AdditionCharge;

		public virtual CustomsChargeCode GetCommission() => CustomsChargeCodeProvider.Commission;

		public virtual CustomsChargeCode GetDeductionCharge() => CustomsChargeCodeProvider.DeductionCharge;

		public virtual CustomsChargeCode GetDiscount() => CustomsChargeCodeProvider.Discount;

		public virtual CustomsChargeCode GetExWorks() => CustomsChargeCodeProvider.ExWorks;

		public virtual CustomsChargeCode GetForeignInlandFreight() => CustomsChargeCodeProvider.ForeignInlandFreight;

		public virtual CustomsChargeCode GetLandingCharges() => CustomsChargeCodeProvider.LandingCharges;

		public virtual CustomsChargeCode GetOtherCharges() => CustomsChargeCodeProvider.OtherCharges;

		public override CustomsChargeCode GetOverseasFreight() => OverseasFreight;

		public override CustomsChargeCode GetOverseasInsurance() => OverseasInsurance;

		public virtual CustomsChargeCode GetPackingCost() => CustomsChargeCodeProvider.PackingCost;

		protected override ICustomsChargeCode[] GetCharges() => new ICustomsChargeCode[]
		{
			GetPackingCost(),
			GetOverseasFreight(),
			GetOverseasInsurance(),
			GetExWorks(),
			GetCommission(),
			GetForeignInlandFreight(),
			GetLandingCharges(),
			GetOtherCharges(),
			GetAdditionCharge(),
			GetDeductionCharge(),
			GetDiscount()
		};
	}
}
