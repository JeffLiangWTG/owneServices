using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IT.Business;

public abstract class IncoTermAndCustomsChargeFactory : UCCIncoTermAndCustomsChargeFactory
{
	public override CustomsChargeCode GetOverseasInsurance() => CustomsChargeCodeProvider.GetNewInsuranceCostsCharge();

	protected override void SetupCostAndFreightConfiguration()
	{
		base.SetupCostAndFreightConfiguration();

		SetupAirTransportCostsAsNotMandatoryAndNotRecommended(Core.Constants.IncoTerms.CostAndFreight);
	}

	protected override void SetupCostInsuranceAndFreightConfiguration()
	{
		base.SetupCostInsuranceAndFreightConfiguration();

		SetupAirTransportCostsAsNotMandatoryAndNotRecommended(Core.Constants.IncoTerms.CostInsuranceAndFreight);
	}

	protected override void SetupCarriageAndInsurancePaidToConfiguration()
	{
		base.SetupCarriageAndInsurancePaidToConfiguration();

		SetupAirTransportCostsAsNotMandatoryAndNotRecommended(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo);
	}

	protected override void SetupCarriagePaidToConfiguration()
	{
		base.SetupCarriagePaidToConfiguration();

		SetupAirTransportCostsAsNotMandatoryAndNotRecommended(Core.Constants.IncoTerms.CarriagePaidTo);
	}

	protected override void SetupDeliveredAtPlaceConfiguration()
	{
		base.SetupDeliveredAtPlaceConfiguration();

		SetupAirTransportCostsAsNotMandatoryAndNotRecommended(Core.Constants.IncoTerms.DeliveredAtPlace);
	}

	protected override void SetupDeliveredAtFrontierConfiguration()
	{
		base.SetupDeliveredAtFrontierConfiguration();

		SetupAirTransportCostsAsNotMandatoryAndNotRecommended(Core.Constants.IncoTerms.DeliveredAtFrontier);
	}

	protected override void SetupDeliveredAtTerminalConfiguration()
	{
		base.SetupDeliveredAtTerminalConfiguration();

		SetupAirTransportCostsAsNotMandatoryAndNotRecommended(Core.Constants.IncoTerms.DeliveredAtTerminal);
	}

	protected override void SetupDeliveredDutyPaidConfiguration()
	{
		base.SetupDeliveredDutyPaidConfiguration();

		SetupAirTransportCostsAsNotMandatoryAndNotRecommended(Core.Constants.IncoTerms.DeliveredDutyPaid);
	}

	protected override void SetupDeliveredDutyUnpaidConfiguration()
	{
		base.SetupDeliveredDutyUnpaidConfiguration();

		SetupAirTransportCostsAsNotMandatoryAndNotRecommended(Core.Constants.IncoTerms.DeliveredDutyUnpaid);
	}

	protected override void SetupDeliveredExQuayConfiguration()
	{
		base.SetupDeliveredExQuayConfiguration();

		SetupAirTransportCostsAsNotMandatoryAndNotRecommended(Core.Constants.IncoTerms.DeliveredExQuay);
	}

	protected override void SetupDeliveredExShipConfiguration()
	{
		base.SetupDeliveredExShipConfiguration();

		SetupAirTransportCostsAsNotMandatoryAndNotRecommended(Core.Constants.IncoTerms.DeliveredExShip);
	}

	protected override void SetupExWorksConfiguration()
	{
		base.SetupExWorksConfiguration();

		SetupAirTransportCostsAsNotMandatoryAndNotRecommended(Core.Constants.IncoTerms.ExWorks);
	}

	protected override void SetupFreeAlongsideShipConfiguration()
	{
		base.SetupFreeAlongsideShipConfiguration();

		SetupAirTransportCostsAsNotMandatoryAndNotRecommended(Core.Constants.IncoTerms.FreeAlongsideShip);
	}

	protected override void SetupFreeCarrierConfiguration()
	{
		base.SetupFreeCarrierConfiguration();

		SetupAirTransportCostsAsNotMandatoryAndNotRecommended(Core.Constants.IncoTerms.FreeCarrier);
	}

	protected override void SetupFreeOnBoardConfiguration()
	{
		base.SetupFreeOnBoardConfiguration();

		SetupAirTransportCostsAsNotMandatoryAndNotRecommended(Core.Constants.IncoTerms.FreeOnBoard);
	}

	void SetupAirTransportCostsAsNotMandatoryAndNotRecommended(string incoTerm)
	{
		var baseAftChargeConfiguration = GetConfiguration(incoTerm, UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge);

		var aftChargeConfigurationNotMandatoryAndNotRecommended = new ChargeConfiguration()
		{
			IsIncludedInInvoice = baseAftChargeConfiguration.IsIncludedInInvoice,
			IsIncludedInInvoiceAmountFixed = baseAftChargeConfiguration.IsIncludedInInvoiceAmountFixed,
			IsMandatory = false,
			IsRecommended = false,
		};

		AddChargeConfiguration(incoTerm, UCCChargesProvider.AirTransportCosts, aftChargeConfigurationNotMandatoryAndNotRecommended, ignoreExisting: true);
	}
}
