using Enterprise.Customs.Common;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ExportIncoTermAndCustomsChargeFactory : EU.Business.EUIncoTermAndCustomsChargeFactory
	{
		protected override void SetupIncotermChargeConfigurations()
		{
			base.SetupIncotermChargeConfigurations();
			SetupFreeCarrierSellerConfiguration();
			SetupFreeCarrierBuyerConfiguration();
			SetupDeliveredAtPlaceUnloadedConfiguration();
		}

		protected override ICustomsChargeCode[] GetCharges() => new ICustomsChargeCode[]
		{
			ExportChargesProvider.AdditionCharge,
			ExportChargesProvider.EUBorderFreight,
			ExportChargesProvider.DeductionCharge,
			ExportChargesProvider.EUBorderInsurance,
			ExportChargesProvider.OverseasFreight,
			ExportChargesProvider.OverseasInsurance
		};

		protected override void SetupErrorConfiguration()
		{
		}

		protected override void SetupExWorksConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, ExportChargesProvider.AdditionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, ExportChargesProvider.EUBorderFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = true,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, ExportChargesProvider.DeductionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, ExportChargesProvider.EUBorderInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = true,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, ExportChargesProvider.OverseasFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, ExportChargesProvider.OverseasInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = false
			});
		}

		protected override void SetupFreeCarrierConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, ExportChargesProvider.AdditionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, ExportChargesProvider.EUBorderFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = true,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, ExportChargesProvider.DeductionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, ExportChargesProvider.EUBorderInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = true,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, ExportChargesProvider.OverseasFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, ExportChargesProvider.OverseasInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = false
			});
		}

		protected void SetupFreeCarrierSellerConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrierSeller, ExportChargesProvider.AdditionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrierSeller, ExportChargesProvider.EUBorderFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = true,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrierSeller, ExportChargesProvider.DeductionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrierSeller, ExportChargesProvider.EUBorderInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = true,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrierSeller, ExportChargesProvider.OverseasFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrierSeller, ExportChargesProvider.OverseasInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = false
			});
		}

		protected void SetupFreeCarrierBuyerConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrierBuyer, ExportChargesProvider.AdditionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrierBuyer, ExportChargesProvider.EUBorderFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = true,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrierBuyer, ExportChargesProvider.DeductionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrierBuyer, ExportChargesProvider.EUBorderInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = true,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrierBuyer, ExportChargesProvider.OverseasFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrierBuyer, ExportChargesProvider.OverseasInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = false
			});
		}

		protected override void SetupFreeAlongsideShipConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, ExportChargesProvider.AdditionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, ExportChargesProvider.EUBorderFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = true,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, ExportChargesProvider.DeductionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, ExportChargesProvider.EUBorderInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = true,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, ExportChargesProvider.OverseasFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, ExportChargesProvider.OverseasInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = false
			});
		}

		protected override void SetupFreeOnBoardConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, ExportChargesProvider.AdditionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, ExportChargesProvider.EUBorderFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = true,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, ExportChargesProvider.DeductionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, ExportChargesProvider.EUBorderInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = true,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, ExportChargesProvider.OverseasFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, ExportChargesProvider.OverseasInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = false
			});
		}

		protected override void SetupCostAndFreightConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, ExportChargesProvider.AdditionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, ExportChargesProvider.EUBorderFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, ExportChargesProvider.DeductionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, ExportChargesProvider.EUBorderInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = true,
				IsRecommended = false
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, ExportChargesProvider.OverseasFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = true,
				IsMandatory = true,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, ExportChargesProvider.OverseasInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = false
			});
		}

		protected override void SetupCostInsuranceAndFreightConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, ExportChargesProvider.AdditionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, ExportChargesProvider.EUBorderFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, ExportChargesProvider.DeductionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, ExportChargesProvider.EUBorderInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, ExportChargesProvider.OverseasFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = true,
				IsMandatory = true,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, ExportChargesProvider.OverseasInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = true,
				IsMandatory = true,
				IsRecommended = true
			});
		}

		protected override void SetupCarriagePaidToConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, ExportChargesProvider.AdditionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, ExportChargesProvider.EUBorderFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, ExportChargesProvider.DeductionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, ExportChargesProvider.EUBorderInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, ExportChargesProvider.OverseasFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = true,
				IsMandatory = true,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, ExportChargesProvider.OverseasInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = true,
				IsMandatory = true,
				IsRecommended = false
			});
		}

		protected override void SetupCarriageAndInsurancePaidToConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, ExportChargesProvider.AdditionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, ExportChargesProvider.EUBorderFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, ExportChargesProvider.DeductionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, ExportChargesProvider.EUBorderInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, ExportChargesProvider.OverseasFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = true,
				IsMandatory = true,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, ExportChargesProvider.OverseasInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = true,
				IsMandatory = true,
				IsRecommended = true
			});
		}

		protected override void SetupDeliveredAtPlaceConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, ExportChargesProvider.AdditionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, ExportChargesProvider.EUBorderFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, ExportChargesProvider.DeductionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, ExportChargesProvider.EUBorderInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, ExportChargesProvider.OverseasFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = true,
				IsMandatory = true,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, ExportChargesProvider.OverseasInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = true,
				IsMandatory = true,
				IsRecommended = true
			});
		}

		protected override void SetupDeliveredAtTerminalConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, ExportChargesProvider.AdditionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, ExportChargesProvider.EUBorderFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, ExportChargesProvider.DeductionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, ExportChargesProvider.EUBorderInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, ExportChargesProvider.OverseasFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = true,
				IsMandatory = true,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, ExportChargesProvider.OverseasInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = true,
				IsMandatory = true,
				IsRecommended = true
			});
		}

		protected void SetupDeliveredAtPlaceUnloadedConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, ExportChargesProvider.AdditionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, ExportChargesProvider.EUBorderFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, ExportChargesProvider.DeductionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, ExportChargesProvider.EUBorderInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, ExportChargesProvider.OverseasFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = true,
				IsMandatory = true,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, ExportChargesProvider.OverseasInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = true,
				IsMandatory = true,
				IsRecommended = true
			});
		}

		protected override void SetupDeliveredDutyPaidConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, ExportChargesProvider.AdditionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, ExportChargesProvider.EUBorderFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, ExportChargesProvider.DeductionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, ExportChargesProvider.EUBorderInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, ExportChargesProvider.OverseasFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = true,
				IsMandatory = true,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, ExportChargesProvider.OverseasInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = true,
				IsMandatory = true,
				IsRecommended = true
			});
		}
	}
}
