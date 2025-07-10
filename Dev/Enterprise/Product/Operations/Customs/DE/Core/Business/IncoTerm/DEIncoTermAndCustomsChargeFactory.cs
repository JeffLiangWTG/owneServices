using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public class DEIncoTermAndCustomsChargeFactory : EUIncoTermAndCustomsChargeFactory
	{
		protected override ICustomsChargeCode[] GetCharges() => new ICustomsChargeCode[]
		{
			ChargesProvider.AdditionCharge,
			ChargesProvider.EUBorderFreight,
			ChargesProvider.DeductionCharge,
			ChargesProvider.EUBorderInsurance,
			ChargesProvider.OverseasFreight,
			ChargesProvider.OverseasInsurance
		};

		protected override void SetupIncotermChargeConfigurations()
		{
			base.SetupIncotermChargeConfigurations();
			SetupFreeCarrierSellerConfiguration();
			SetupFreeCarrierBuyerConfiguration();
			SetupDeliveredAtPlaceUnloadedConfiguration();
		}

		public override bool GetDefaultIsIncludedInInvoice(ZString incoTerm, ICustomsChargeCode charge)
		{
			var chargeConfiguration = GetConfiguration(incoTerm, charge?.Code ?? ZString.Empty);
			return chargeConfiguration != null && chargeConfiguration.IsIncludedInInvoiceAmount;
		}

		protected override void SetupErrorConfiguration()
		{
		}

		protected override void SetupExWorksConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, ChargesProvider.AdditionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, ChargesProvider.EUBorderFreight, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, ChargesProvider.EUBorderInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true
			});
		}

		protected override void SetupFreeCarrierConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, ChargesProvider.AdditionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, ChargesProvider.EUBorderFreight, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, ChargesProvider.EUBorderInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true
			});
		}

		protected void SetupFreeCarrierSellerConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrierSeller, ChargesProvider.AdditionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrierSeller, ChargesProvider.EUBorderFreight, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrierSeller, ChargesProvider.EUBorderInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true
			});
		}

		protected void SetupFreeCarrierBuyerConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrierBuyer, ChargesProvider.AdditionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrierBuyer, ChargesProvider.EUBorderFreight, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrierBuyer, ChargesProvider.EUBorderInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true
			});
		}

		protected override void SetupFreeAlongsideShipConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, ChargesProvider.AdditionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, ChargesProvider.EUBorderFreight, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, ChargesProvider.EUBorderInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true
			});
		}

		protected override void SetupFreeOnBoardConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, ChargesProvider.AdditionCharge, new ChargeConfiguration
			{
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, ChargesProvider.EUBorderFreight, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, ChargesProvider.EUBorderInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true
			});
		}

		protected override void SetupCostAndFreightConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, ChargesProvider.AdditionCharge, new ChargeConfiguration
			{
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, ChargesProvider.EUBorderFreight, new ChargeConfiguration
			{
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, ChargesProvider.EUBorderInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, ChargesProvider.OverseasFreight, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true,
				IsIncludedInInvoiceAmount = true
			});
		}

		protected override void SetupCostInsuranceAndFreightConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, ChargesProvider.AdditionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmount = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, ChargesProvider.EUBorderFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmount = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, ChargesProvider.EUBorderInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmount = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, ChargesProvider.OverseasFreight, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true,
				IsIncludedInInvoiceAmount = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, ChargesProvider.OverseasInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true,
				IsIncludedInInvoiceAmount = true
			});
		}

		protected override void SetupCarriagePaidToConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, ChargesProvider.AdditionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmount = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, ChargesProvider.EUBorderFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmount = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, ChargesProvider.EUBorderInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmount = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, ChargesProvider.OverseasFreight, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true,
				IsIncludedInInvoiceAmount = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, ChargesProvider.OverseasInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true,
				IsIncludedInInvoiceAmount = true
			});
		}

		protected override void SetupCarriageAndInsurancePaidToConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, ChargesProvider.AdditionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmount = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, ChargesProvider.EUBorderFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmount = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, ChargesProvider.EUBorderInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmount = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, ChargesProvider.OverseasFreight, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true,
				IsIncludedInInvoiceAmount = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, ChargesProvider.OverseasInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true,
				IsIncludedInInvoiceAmount = true
			});
		}

		protected override void SetupDeliveredAtPlaceConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, ChargesProvider.AdditionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmount = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, ChargesProvider.EUBorderFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmount = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, ChargesProvider.DeductionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true,
				IsIncludedInInvoiceAmount = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, ChargesProvider.EUBorderInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmount = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, ChargesProvider.OverseasFreight, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true,
				IsIncludedInInvoiceAmount = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, ChargesProvider.OverseasInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true,
				IsIncludedInInvoiceAmount = true
			});
		}

		protected override void SetupDeliveredAtTerminalConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, ChargesProvider.AdditionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmount = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, ChargesProvider.EUBorderFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmount = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, ChargesProvider.DeductionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true,
				IsIncludedInInvoiceAmount = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, ChargesProvider.EUBorderInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmount = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, ChargesProvider.OverseasFreight, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true,
				IsIncludedInInvoiceAmount = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, ChargesProvider.OverseasInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true,
				IsIncludedInInvoiceAmount = true
			});
		}

		protected void SetupDeliveredAtPlaceUnloadedConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, ChargesProvider.AdditionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmount = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, ChargesProvider.EUBorderFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmount = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, ChargesProvider.DeductionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true,
				IsIncludedInInvoiceAmount = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, ChargesProvider.EUBorderInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmount = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, ChargesProvider.OverseasFreight, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true,
				IsIncludedInInvoiceAmount = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, ChargesProvider.OverseasInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true,
				IsIncludedInInvoiceAmount = true
			});
		}

		protected override void SetupDeliveredDutyPaidConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, ChargesProvider.AdditionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmount = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, ChargesProvider.EUBorderFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmount = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, ChargesProvider.DeductionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true,
				IsIncludedInInvoiceAmount = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, ChargesProvider.EUBorderInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmount = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, ChargesProvider.OverseasFreight, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true,
				IsIncludedInInvoiceAmount = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, ChargesProvider.OverseasInsurance, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsRecommended = true,
				IsIncludedInInvoiceAmount = true
			});
		}

		protected override string FreightToEUBorderCodeCore => ChargesProvider.EUBorderFreight.Code;

		protected override MultilingualString FreightToEUBorderDesc => ChargesProvider.EUBorderFreight.Description;

		protected override string FreightAfterEUBorderCodeCore => ChargesProvider.OverseasFreight.Code;

		protected override MultilingualString FreightAfterEUBorderDesc => ChargesProvider.OverseasFreight.Description;
	}
}
