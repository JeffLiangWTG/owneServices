using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.BR.Business
{
	public class ImportIncoTermAndCustomsChargeFactory : IncoTermAndCustomsChargeFactory
	{
		protected override ICustomsChargeCode[] GetCharges()
		{
			var result = new List<ICustomsChargeCode>();
			result.Add(GetOverseasFreight());
			result.Add(GetOverseasInsurance());
			result.Add(ImportCommonChargesProvider.OverseasFreightPrepaid);
			result.AddRange(ImportChargesProvider.Codes);
			result.AddRange(ImportCommonChargesProvider.CommonChargesList);
			return result.ToArray();
		}

		protected override IComparer<string> GetChargeCodeComparer()
		{
			return new ImportChargeCodeComparer();
		}

		protected override void SetupIncotermChargeConfigurations()
		{
			base.SetupIncotermChargeConfigurations();

			foreach (var code in ImportChargesProvider.Codes)
			{
				foreach (var incoTerm in ImportChargesProvider.ConfiguredIncoTerms)
				{
					AddChargeConfiguration(incoTerm, code, ImportChargesProvider.GetChargeConfiguration());
				}
			}
		}

		public override CustomsChargeCode GetOverseasFreight() => ImportCommonChargesProvider.OverseasFreightCollect;

		public override CustomsChargeCode GetOverseasInsurance() => ImportChargesProvider.OverseasInsurance;

		public override bool IsThisChargeDiscount(ZString chargeCode)
		{
			return ImportChargesProvider.IsDeductions(chargeCode);
		}

		protected override void SetupDeliveredAtPlaceConfiguration()
		{
			AddChargeConfiguration(BRIncoTermList.Codes.DAP, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfigurations(BRIncoTermList.Codes.DAP, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true }, ImportCommonChargesProvider.OverseasFreightCharges);
		}

		protected override void SetupDeliveredAtTerminalConfiguration()
		{
			AddChargeConfiguration(BRIncoTermList.Codes.DAT, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfigurations(BRIncoTermList.Codes.DAT, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true }, ImportCommonChargesProvider.OverseasFreightCharges);
		}

		protected override void SetupExWorksConfiguration()
		{
			AddChargeConfiguration(BRIncoTermList.Codes.EXW, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfigurations(BRIncoTermList.Codes.EXW, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true }, ImportCommonChargesProvider.OverseasFreightCharges);
		}

		protected override void SetupDeliveredDutyPaidConfiguration()
		{
			AddChargeConfiguration(BRIncoTermList.Codes.DDP, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfigurations(BRIncoTermList.Codes.DDP, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true }, ImportCommonChargesProvider.OverseasFreightCharges);
		}

		protected override void SetupCarriageAndInsurancePaidToConfiguration()
		{
			AddChargeConfiguration(BRIncoTermList.Codes.CIP, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfigurations(BRIncoTermList.Codes.CIP, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true }, ImportCommonChargesProvider.OverseasFreightCharges);
		}

		protected override void SetupCarriagePaidToConfiguration()
		{
			AddChargeConfiguration(BRIncoTermList.Codes.CPT, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfigurations(BRIncoTermList.Codes.CPT, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true }, ImportCommonChargesProvider.OverseasFreightCharges);
		}

		protected override void SetupCostInsuranceAndFreightConfiguration()
		{
			AddChargeConfiguration(BRIncoTermList.Codes.CIF, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfigurations(BRIncoTermList.Codes.CIF, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true }, ImportCommonChargesProvider.OverseasFreightCharges);
		}

		protected override void SetupCostAndFreightConfiguration()
		{
			AddChargeConfiguration(BRIncoTermList.Codes.CFR, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfigurations(BRIncoTermList.Codes.CFR, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true }, ImportCommonChargesProvider.OverseasFreightCharges);
		}

		protected override void SetupFreeAlongsideShipConfiguration()
		{
			AddChargeConfiguration(BRIncoTermList.Codes.FAS, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfigurations(BRIncoTermList.Codes.FAS, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true }, ImportCommonChargesProvider.OverseasFreightCharges);
		}

		protected override void SetupFreeOnBoardConfiguration()
		{
			AddChargeConfiguration(BRIncoTermList.Codes.FOB, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfigurations(BRIncoTermList.Codes.FOB, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true }, ImportCommonChargesProvider.OverseasFreightCharges);
		}

		protected override void SetupFreeCarrierConfiguration()
		{
			AddChargeConfiguration(BRIncoTermList.Codes.FCA, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfigurations(BRIncoTermList.Codes.FCA, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true }, ImportCommonChargesProvider.OverseasFreightCharges);
		}

		protected override void SetupCNIConfiguration()
		{
			AddChargeConfiguration(BRIncoTermList.Codes.CPLUSI, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfigurations(BRIncoTermList.Codes.CPLUSI, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true }, ImportCommonChargesProvider.OverseasFreightCharges);
		}

		protected override void SetupCNFConfiguration()
		{
			AddChargeConfiguration(BRIncoTermList.Codes.CPLUSF, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfigurations(BRIncoTermList.Codes.CPLUSF, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true }, ImportCommonChargesProvider.OverseasFreightCharges);
		}

		protected override void SetupOCVConfiguration()
		{
			AddChargeConfiguration(BRIncoTermList.Codes.OCV, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfigurations(BRIncoTermList.Codes.OCV, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true }, ImportCommonChargesProvider.OverseasFreightCharges);
		}

		protected override void SetupDeliveredAtPlaceUnloadedConfiguration()
		{
			AddChargeConfiguration(BRIncoTermList.Codes.DPU, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfigurations(BRIncoTermList.Codes.DPU, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true }, ImportCommonChargesProvider.OverseasFreightCharges);
		}
	}
}
