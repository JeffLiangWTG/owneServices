using System.Collections.Generic;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.Business
{
	public class ExportIncoTermAndCustomsChargeFactory : IncoTermAndCustomsChargeFactory
	{
		protected override ICustomsChargeCode[] GetCharges()
		{
			var result = new List<ICustomsChargeCode>(base.GetCharges());
			result.Remove(UCCChargesProvider.TransportCosts);
			result.Remove(UCCChargesProvider.InsuranceCosts);
			result.Remove(UCCChargesProvider.Adjustment);
			result.Remove(UCCChargesProvider.AirTransportCosts);
			result.Remove(UCCChargesProvider.IndirectAndOtherPayments);
			result.Add(ESChargeProvider.GetNewInternationalFreightExp());
			result.Add(ESChargeProvider.GetNewTransportCostsUntilESBorder());
			result.Add(ESChargeProvider.GetNewInsuranceUntilESBorder());
			result.Add(ESChargeProvider.GetNewOtherInternationalPayments());
			result.Add(ESChargeProvider.GetNewOtherNationalPayments());
			result.Add(ESChargeProvider.GetNewTransportCostsExp());
			result.Add(ESChargeProvider.GetNewInsuranceCostsExp());
			result.Add(ESChargeProvider.GetNewAdjustment());
			result.Add(ESChargeProvider.GetNewAirTransportCosts());
			result.Add(ESChargeProvider.GetNewIndirectAndOtherPaymentsExp());
			return result.ToArray();
		}

		protected override void SetupIncotermChargeConfigurations()
		{
			base.SetupIncotermChargeConfigurations();
			SetupDeliveredAtPlaceUnloadedConfiguration();
		}

		protected override void SetupFreeOnBoardConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, ESChargeProvider.GetNewTransportCostsUntilESBorder(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, ESChargeProvider.GetNewInsuranceUntilESBorder(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, ESChargeProvider.GetNewOtherNationalPayments(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, ESChargeProvider.GetNewInternationalFreightExp(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, ESChargeProvider.GetNewTransportCostsExp(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, ESChargeProvider.GetNewInsuranceCostsExp(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, ESChargeProvider.GetNewOtherInternationalPayments(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupFreeCarrierConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, ESChargeProvider.GetNewTransportCostsUntilESBorder(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, ESChargeProvider.GetNewInsuranceUntilESBorder(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, ESChargeProvider.GetNewInternationalFreightExp(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, ESChargeProvider.GetNewTransportCostsExp(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, ESChargeProvider.GetNewInsuranceCostsExp(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, ESChargeProvider.GetNewOtherInternationalPayments(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, ESChargeProvider.GetNewOtherNationalPayments(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupFreeAlongsideShipConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, ESChargeProvider.GetNewTransportCostsUntilESBorder(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, ESChargeProvider.GetNewInsuranceUntilESBorder(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, ESChargeProvider.GetNewInternationalFreightExp(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, ESChargeProvider.GetNewTransportCostsExp(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, ESChargeProvider.GetNewInsuranceCostsExp(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, ESChargeProvider.GetNewOtherInternationalPayments(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, ESChargeProvider.GetNewOtherNationalPayments(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupCostInsuranceAndFreightConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, ESChargeProvider.GetNewTransportCostsUntilESBorder(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, ESChargeProvider.GetNewInsuranceUntilESBorder(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, ESChargeProvider.GetNewOtherNationalPayments(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, ESChargeProvider.GetNewInternationalFreightExp(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, ESChargeProvider.GetNewInsuranceCostsExp(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, ESChargeProvider.GetNewOtherInternationalPayments(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, ESChargeProvider.GetNewTransportCostsExp(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = true, IsRecommended = true });
		}

		protected override void SetupCarriageAndInsurancePaidToConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, ESChargeProvider.GetNewTransportCostsUntilESBorder(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, ESChargeProvider.GetNewInsuranceUntilESBorder(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, ESChargeProvider.GetNewOtherNationalPayments(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, ESChargeProvider.GetNewInternationalFreightExp(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, ESChargeProvider.GetNewInsuranceCostsExp(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, ESChargeProvider.GetNewOtherInternationalPayments(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, ESChargeProvider.GetNewTransportCostsExp(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = true, IsRecommended = true });
		}

		protected override void SetupCostAndFreightConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, ESChargeProvider.GetNewTransportCostsUntilESBorder(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, ESChargeProvider.GetNewInsuranceUntilESBorder(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, ESChargeProvider.GetNewOtherNationalPayments(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, ESChargeProvider.GetNewInternationalFreightExp(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, ESChargeProvider.GetNewInsuranceCostsExp(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, ESChargeProvider.GetNewOtherInternationalPayments(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, ESChargeProvider.GetNewTransportCostsExp(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = true, IsRecommended = true });
		}

		protected override void SetupCarriagePaidToConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, ESChargeProvider.GetNewTransportCostsUntilESBorder(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, ESChargeProvider.GetNewInsuranceUntilESBorder(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, ESChargeProvider.GetNewOtherNationalPayments(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, ESChargeProvider.GetNewInternationalFreightExp(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, ESChargeProvider.GetNewInsuranceCostsExp(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, ESChargeProvider.GetNewOtherInternationalPayments(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, ESChargeProvider.GetNewTransportCostsExp(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = true, IsRecommended = true });
		}

		protected override void SetupDeliveredAtPlaceConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, ESChargeProvider.GetNewTransportCostsUntilESBorder(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, ESChargeProvider.GetNewInsuranceUntilESBorder(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, ESChargeProvider.GetNewOtherNationalPayments(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, ESChargeProvider.GetNewInternationalFreightExp(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, ESChargeProvider.GetNewTransportCostsExp(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, ESChargeProvider.GetNewInsuranceCostsExp(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, ESChargeProvider.GetNewOtherInternationalPayments(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupDeliveredAtTerminalConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, ESChargeProvider.GetNewTransportCostsUntilESBorder(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, ESChargeProvider.GetNewInsuranceUntilESBorder(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, ESChargeProvider.GetNewOtherNationalPayments(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, ESChargeProvider.GetNewInternationalFreightExp(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, ESChargeProvider.GetNewTransportCostsExp(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, ESChargeProvider.GetNewInsuranceCostsExp(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, ESChargeProvider.GetNewOtherInternationalPayments(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
		}

		protected void SetupDeliveredAtPlaceUnloadedConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, ESChargeProvider.GetNewTransportCostsUntilESBorder(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, ESChargeProvider.GetNewInsuranceUntilESBorder(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, ESChargeProvider.GetNewOtherNationalPayments(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, ESChargeProvider.GetNewInternationalFreightExp(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, ESChargeProvider.GetNewTransportCostsExp(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, ESChargeProvider.GetNewInsuranceCostsExp(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, ESChargeProvider.GetNewOtherInternationalPayments(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupDeliveredDutyPaidConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, ESChargeProvider.GetNewTransportCostsUntilESBorder(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, ESChargeProvider.GetNewInsuranceUntilESBorder(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, ESChargeProvider.GetNewOtherNationalPayments(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, ESChargeProvider.GetNewInternationalFreightExp(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, ESChargeProvider.GetNewTransportCostsExp(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, ESChargeProvider.GetNewInsuranceCostsExp(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, ESChargeProvider.GetNewOtherInternationalPayments(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupExWorksConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, ESChargeProvider.GetNewTransportCostsUntilESBorder(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, ESChargeProvider.GetNewInsuranceUntilESBorder(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, ESChargeProvider.GetNewOtherNationalPayments(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, ESChargeProvider.GetNewInternationalFreightExp(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, ESChargeProvider.GetNewTransportCostsExp(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, ESChargeProvider.GetNewInsuranceCostsExp(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, ESChargeProvider.GetNewOtherInternationalPayments(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
		}
	}
}
