using System.Linq;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.Business
{
	public class UCCIncoTermAndCustomsChargeFactory : EUIncoTermAndCustomsChargeFactory
	{
		public UCCIncoTermAndCustomsChargeFactory() { }

		protected override ICustomsChargeCode[] GetCharges()
		{
			return SetCustomsChargeCode(new ICustomsChargeCode[]
				{
					UCCChargesProvider.CommissionAndBrokerage,
					UCCChargesProvider.CommissionExceptBuyingCommissions,
					UCCChargesProvider.ContainersAndPacking,
					UCCChargesProvider.MaterialsComponentsParts,
					UCCChargesProvider.ToolsMiesMoulds,
					UCCChargesProvider.MaterialsConsumed,
					UCCChargesProvider.EngineeringDevelopmentArtwork,
					UCCChargesProvider.RoyaltiesLicenseFee,
					UCCChargesProvider.ProceedsOfAnySubsequentResale,
					UCCChargesProvider.IndirectAndOtherPayments,
					GetOverseasInsurance(),
					UCCChargesProvider.Additions71,
					UCCChargesProvider.TransportCosts,
					UCCChargesProvider.OtherNotElsewhereDeclared,
					UCCChargesProvider.Adjustment,
					UCCChargesProvider.AirTransportCosts,
					UCCChargesProvider.ConstructionErectionAssembly,
					UCCChargesProvider.ImportDutiesOrOther,
					UCCChargesProvider.Interest,
					UCCChargesProvider.RightToReproduce,
					UCCChargesProvider.BuyingCommissions,
					UCCChargesProvider.Deductions71,
					UCCChargesProvider.DiscountNotElsewhereDeclared,
					UCCChargesProvider.DeductionsNotElsewhereDeclared,
					ChargeCodeProvider.StatisticalValue
				});
		}

		protected override ICustomsChargeCode[] SetCustomsChargeCode(ICustomsChargeCode[] result)
		{
			var dedCharge = result.FirstOrDefault(x => x.Code == CustomsChargeTypeList.Codes.DeductionCharge);
			if (dedCharge is CustomsChargeCode dedCode && !dedCode.IsIncludedInITOTIfDeemed.HasValue)
			{
				dedCode.IsIncludedInITOTIfDeemed = true;
			}
			var addCharge = result.FirstOrDefault(x => x.Code == CustomsChargeTypeList.Codes.AdditionCharge);
			if (addCharge is CustomsChargeCode addCode && !addCode.IsIncludedInITOTIfDeemed.HasValue)
			{
				addCode.IsIncludedInITOTIfDeemed = false;
			}
			return result;
		}

		public override CustomsChargeCode GetOverseasInsurance() => UCCChargesProvider.InsuranceCosts;

		protected override string FreightToEUBorderCodeCore => UCCCustomsChargeTypeList.Codes.TransportCostsCharge;
		protected override string FreightAfterEUBorderCodeCore => UCCCustomsChargeTypeList.Codes.TransportCostsCharge;

		protected override void SetupIncotermChargeConfigurations()
		{
			base.SetupIncotermChargeConfigurations();
			SetupDeliveredAtFrontierConfiguration();
			SetupDeliveredDutyUnpaidConfiguration();
			SetupDeliveredExQuayConfiguration();
			SetupDeliveredExShipConfiguration();
		}

		protected override void SetupCostAndFreightConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, UCCChargesProvider.ContainersAndPacking, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, UCCChargesProvider.InsuranceCosts, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, UCCChargesProvider.TransportCosts, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, UCCChargesProvider.AirTransportCosts, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, UCCChargesProvider.ImportDutiesOrOther, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupCostInsuranceAndFreightConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, UCCChargesProvider.ContainersAndPacking, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, UCCChargesProvider.InsuranceCosts, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, UCCChargesProvider.TransportCosts, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, UCCChargesProvider.AirTransportCosts, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, UCCChargesProvider.ImportDutiesOrOther, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupCarriageAndInsurancePaidToConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, UCCChargesProvider.ContainersAndPacking, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, UCCChargesProvider.InsuranceCosts, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, UCCChargesProvider.TransportCosts, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, UCCChargesProvider.AirTransportCosts, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, UCCChargesProvider.ImportDutiesOrOther, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupCarriagePaidToConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, UCCChargesProvider.ContainersAndPacking, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, UCCChargesProvider.InsuranceCosts, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, UCCChargesProvider.TransportCosts, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, UCCChargesProvider.AirTransportCosts, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, UCCChargesProvider.ImportDutiesOrOther, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
		}

		protected virtual void SetupDeliveredAtFrontierConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtFrontier, UCCChargesProvider.ContainersAndPacking, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtFrontier, UCCChargesProvider.InsuranceCosts, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtFrontier, UCCChargesProvider.TransportCosts, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtFrontier, UCCChargesProvider.AirTransportCosts, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtFrontier, UCCChargesProvider.ImportDutiesOrOther, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupDeliveredAtPlaceConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, UCCChargesProvider.ContainersAndPacking, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, UCCChargesProvider.InsuranceCosts, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, UCCChargesProvider.TransportCosts, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, UCCChargesProvider.AirTransportCosts, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, UCCChargesProvider.ImportDutiesOrOther, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupDeliveredAtTerminalConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, UCCChargesProvider.ContainersAndPacking, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, UCCChargesProvider.InsuranceCosts, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, UCCChargesProvider.TransportCosts, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, UCCChargesProvider.AirTransportCosts, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, UCCChargesProvider.ImportDutiesOrOther, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupDeliveredDutyPaidConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, UCCChargesProvider.ContainersAndPacking, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, UCCChargesProvider.InsuranceCosts, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, UCCChargesProvider.TransportCosts, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, UCCChargesProvider.AirTransportCosts, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, UCCChargesProvider.ImportDutiesOrOther, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
		}

		protected virtual void SetupDeliveredDutyUnpaidConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyUnpaid, UCCChargesProvider.ContainersAndPacking, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyUnpaid, UCCChargesProvider.InsuranceCosts, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyUnpaid, UCCChargesProvider.TransportCosts, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyUnpaid, UCCChargesProvider.AirTransportCosts, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyUnpaid, UCCChargesProvider.ImportDutiesOrOther, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
		}

		protected virtual void SetupDeliveredExQuayConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredExQuay, UCCChargesProvider.ContainersAndPacking, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredExQuay, UCCChargesProvider.InsuranceCosts, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredExQuay, UCCChargesProvider.TransportCosts, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredExQuay, UCCChargesProvider.AirTransportCosts, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredExQuay, UCCChargesProvider.ImportDutiesOrOther, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
		}

		protected virtual void SetupDeliveredExShipConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredExShip, UCCChargesProvider.ContainersAndPacking, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredExShip, UCCChargesProvider.InsuranceCosts, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredExShip, UCCChargesProvider.TransportCosts, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredExShip, UCCChargesProvider.AirTransportCosts, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredExShip, UCCChargesProvider.ImportDutiesOrOther, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupExWorksConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, UCCChargesProvider.ContainersAndPacking, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, UCCChargesProvider.InsuranceCosts, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, UCCChargesProvider.TransportCosts, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, UCCChargesProvider.AirTransportCosts, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, UCCChargesProvider.ImportDutiesOrOther, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupFreeAlongsideShipConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, UCCChargesProvider.ContainersAndPacking, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, UCCChargesProvider.InsuranceCosts, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, UCCChargesProvider.TransportCosts, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, UCCChargesProvider.AirTransportCosts, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, UCCChargesProvider.ImportDutiesOrOther, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupFreeCarrierConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, UCCChargesProvider.ContainersAndPacking, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, UCCChargesProvider.InsuranceCosts, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, UCCChargesProvider.TransportCosts, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, UCCChargesProvider.AirTransportCosts, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, UCCChargesProvider.ImportDutiesOrOther, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupFreeOnBoardConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, UCCChargesProvider.ContainersAndPacking, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, UCCChargesProvider.InsuranceCosts, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, UCCChargesProvider.TransportCosts, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, UCCChargesProvider.AirTransportCosts, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, UCCChargesProvider.ImportDutiesOrOther, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
		}
	}
}

