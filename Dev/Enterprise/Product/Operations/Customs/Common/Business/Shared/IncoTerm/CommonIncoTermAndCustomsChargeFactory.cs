using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common.ITOTIncoTerm;

namespace Enterprise.Customs.Common
{
	public partial class CommonIncoTermAndCustomsChargeFactory : IncoTermAndCustomsChargeFactory
	{
		#region Implementation

		protected override bool IsIncludedInITOTReadOnlyForGroupChargeCore(ZString chargeCode)
		{
			return chargeCode != CustomsChargeTypeList.Codes.PackingCost && base.IsIncludedInITOTReadOnlyForGroupChargeCore(chargeCode);
		}

		protected override void SetupIncotermChargeConfigurations()
		{
			SetupCarriageAndInsurancePaidToConfiguration();
			SetupCarriagePaidToConfiguration();
			SetupCostAndFreightConfiguration();
			SetupCostInsuranceAndFreightConfiguration();
			SetupDeliveredAtPlaceConfiguration();
			SetupDeliveredAtTerminalConfiguration();
			SetupDeliveredDutyPaidConfiguration();
			SetupExWorksConfiguration();
			SetupFreeCarrierConfiguration();
			SetupFreeAlongsideShipConfiguration();
			SetupFreeOnBoardConfiguration();
		}

		protected override void SetupErrorConfiguration()
		{
			AddChargeConfiguration(ErrorIncoTermCode, CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(ErrorIncoTermCode, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(ErrorIncoTermCode, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(ErrorIncoTermCode, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(ErrorIncoTermCode, CustomsChargeCodeProvider.PackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(ErrorIncoTermCode, CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(ErrorIncoTermCode, CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(ErrorIncoTermCode, CustomsChargeCodeProvider.Commission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(ErrorIncoTermCode, CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(ErrorIncoTermCode, CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(ErrorIncoTermCode, CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected virtual void SetupDeliveredAtPlaceConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, CustomsChargeCodeProvider.PackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, CustomsChargeCodeProvider.Commission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected virtual void SetupDeliveredAtTerminalConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, CustomsChargeCodeProvider.PackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, CustomsChargeCodeProvider.Commission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected virtual void SetupExWorksConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, CustomsChargeCodeProvider.PackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, CustomsChargeCodeProvider.Commission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected virtual void SetupDeliveredDutyPaidConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, CustomsChargeCodeProvider.PackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, CustomsChargeCodeProvider.Commission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected virtual void SetupCarriageAndInsurancePaidToConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, CustomsChargeCodeProvider.PackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, CustomsChargeCodeProvider.Commission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected virtual void SetupCarriagePaidToConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, CustomsChargeCodeProvider.PackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, CustomsChargeCodeProvider.Commission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected virtual void SetupCostInsuranceAndFreightConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, CustomsChargeCodeProvider.PackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, CustomsChargeCodeProvider.Commission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected virtual void SetupCostAndFreightConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, CustomsChargeCodeProvider.PackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, CustomsChargeCodeProvider.Commission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected virtual void SetupFreeOnBoardConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, CustomsChargeCodeProvider.PackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, CustomsChargeCodeProvider.Commission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected virtual void SetupFreeCarrierConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, CustomsChargeCodeProvider.PackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, CustomsChargeCodeProvider.Commission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected virtual void SetupFreeAlongsideShipConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, CustomsChargeCodeProvider.PackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, CustomsChargeCodeProvider.Commission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override IEnumerable<IITOTIncoTermCalculator> GetIITOTIncoTermCalculators()
		{
			yield return new CFRITOTIncoTermCalculator();
			yield return new CIFITOTIncoTermCalculator();
			yield return new CIPITOTIncoTermCalculator();
			yield return new CPTITOTIncoTermCalculator();
			yield return new DAPITOTIncoTermCalculator();
			yield return new DATITOTIncoTermCalculator();
			yield return new DDPITOTIncoTermCalculator();
			yield return new FASITOTIncoTermCalculator();
			yield return new FCAITOTIncoTermCalculator();
			yield return new FOBITOTIncoTermCalculator();
		}

		#endregion
	}
}
