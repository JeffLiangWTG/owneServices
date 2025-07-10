using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.BR.Business
{
	public class IncoTermAndCustomsChargeFactory : CommonIncoTermAndCustomsChargeFactory
	{
		protected override void SetupIncotermChargeConfigurations()
		{
			base.SetupIncotermChargeConfigurations();
			SetupDeliveredAtPlaceUnloadedConfiguration();
			SetupCNIConfiguration();
			SetupCNFConfiguration();
			SetupOCVConfiguration();
		}

		protected virtual void SetupCNIConfiguration()
		{
			AddChargeConfiguration(BRIncoTermList.Codes.CPLUSI, CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(BRIncoTermList.Codes.CPLUSI, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(BRIncoTermList.Codes.CPLUSI, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(BRIncoTermList.Codes.CPLUSI, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(BRIncoTermList.Codes.CPLUSI, CustomsChargeCodeProvider.PackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(BRIncoTermList.Codes.CPLUSI, CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(BRIncoTermList.Codes.CPLUSI, CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(BRIncoTermList.Codes.CPLUSI, CustomsChargeCodeProvider.Commission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(BRIncoTermList.Codes.CPLUSI, CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(BRIncoTermList.Codes.CPLUSI, CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(BRIncoTermList.Codes.CPLUSI, CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected virtual void SetupCNFConfiguration()
		{
			AddChargeConfiguration(BRIncoTermList.Codes.CPLUSF, CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(BRIncoTermList.Codes.CPLUSF, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(BRIncoTermList.Codes.CPLUSF, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(BRIncoTermList.Codes.CPLUSF, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(BRIncoTermList.Codes.CPLUSF, CustomsChargeCodeProvider.PackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(BRIncoTermList.Codes.CPLUSF, CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(BRIncoTermList.Codes.CPLUSF, CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(BRIncoTermList.Codes.CPLUSF, CustomsChargeCodeProvider.Commission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(BRIncoTermList.Codes.CPLUSF, CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(BRIncoTermList.Codes.CPLUSF, CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(BRIncoTermList.Codes.CPLUSF, CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected virtual void SetupOCVConfiguration()
		{
			AddChargeConfiguration(BRIncoTermList.Codes.OCV, CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(BRIncoTermList.Codes.OCV, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(BRIncoTermList.Codes.OCV, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(BRIncoTermList.Codes.OCV, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(BRIncoTermList.Codes.OCV, CustomsChargeCodeProvider.PackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(BRIncoTermList.Codes.OCV, CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(BRIncoTermList.Codes.OCV, CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(BRIncoTermList.Codes.OCV, CustomsChargeCodeProvider.Commission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(BRIncoTermList.Codes.OCV, CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(BRIncoTermList.Codes.OCV, CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(BRIncoTermList.Codes.OCV, CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected virtual void SetupDeliveredAtPlaceUnloadedConfiguration()
		{
			AddChargeConfiguration(BRIncoTermList.Codes.DPU, CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(BRIncoTermList.Codes.DPU, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(BRIncoTermList.Codes.DPU, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(BRIncoTermList.Codes.DPU, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(BRIncoTermList.Codes.DPU, CustomsChargeCodeProvider.PackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(BRIncoTermList.Codes.DPU, CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(BRIncoTermList.Codes.DPU, CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(BRIncoTermList.Codes.DPU, CustomsChargeCodeProvider.Commission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(BRIncoTermList.Codes.DPU, CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(BRIncoTermList.Codes.DPU, CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(BRIncoTermList.Codes.DPU, CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		public override bool MakeFlagsReadOnlyWhenDeemed => true;

		protected void AddChargeConfigurations(ZString incoTerm, ChargeConfiguration chargeConfiguration, params ICustomsChargeCode[] charges)
		{
			foreach (var charge in charges)
			{
				AddChargeConfiguration(incoTerm, charge, chargeConfiguration);
			}
		}
	}
}
