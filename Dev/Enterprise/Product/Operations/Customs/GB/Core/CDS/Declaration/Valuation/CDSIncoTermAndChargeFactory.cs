using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSIncoTermAndChargeFactory : UCCIncoTermAndCustomsChargeFactory
	{
		public CDSIncoTermAndChargeFactory() { }

		protected override ICustomsChargeCode[] GetCharges() => base.GetCharges().Where(x => x.Code != ChargeTypeList.Codes.StatisticalValue).ToArray();

		public override bool IsThisChargeRecommendedForThisInvoice(ICommonInvoice invoice, ZString incoterm, ZString chargeCode)
		{
			if (chargeCode == CDSCustomsChargeTypeList.Codes.AirTransportCostsCharge
				&& (incoterm == Core.Constants.IncoTerms.ExWorks
					|| incoterm == Core.Constants.IncoTerms.FreeAlongsideShip
					|| incoterm == Core.Constants.IncoTerms.FreeCarrier
					|| incoterm == Core.Constants.IncoTerms.FreeOnBoard))
			{
				var invoiceHeader = invoice as JobComInvoiceHeader;
				var declaration = invoiceHeader?.JobDeclaration;
				if (declaration != null)
				{
					return declaration.IsAir;
				}
			}

			return true;
		}

		protected override string FreightToEUBorderCodeCore => CDSCustomsChargeTypeList.Codes.AirTransportCostsCharge;
		protected override string FreightAfterEUBorderCodeCore => CDSCustomsChargeTypeList.Codes.AirTransportCostsCharge;

		public override void SetupAfterEUBorderCharge(JobComInvCharge charge, ZDecimal amount, ZString currency)
		{
			base.SetupAfterEUBorderCharge(charge, amount, currency);
			charge.J7_IsStatisticalValueApplicable = false;
		}

		public override bool CanThisChargeBeIncludedOnLineButNotOnInvoice(ZString chargeCode)
		{
			return chargeCode == CDSCustomsChargeTypeList.Codes.AirTransportCostsCharge ||
				chargeCode == CDSCustomsChargeTypeList.Codes.TransportCostsCharge ||
				chargeCode == CDSCustomsChargeTypeList.Codes.InsuranceCostsCharge;
		}

		protected override void SetupIncotermChargeConfigurations()
		{
			base.SetupIncotermChargeConfigurations();
			SetupDeliveredAtPlaceUnloaded();
		}

		protected override void SetupDeliveredAtTerminalConfiguration() { }
		protected override void SetupDeliveredAtFrontierConfiguration() { }
		protected override void SetupDeliveredDutyUnpaidConfiguration()	{ }
		protected override void SetupDeliveredExQuayConfiguration() { }
		protected override void SetupDeliveredExShipConfiguration() { }

		protected void SetupDeliveredAtPlaceUnloaded()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, UCCChargesProvider.ContainersAndPacking, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsIncludedInInvoiceAmountFixed = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, UCCChargesProvider.InsuranceCosts, new ChargeConfiguration
			{
				IsIncludedInInvoice = false,
				IsIncludedInInvoiceAmountFixed = false,
				IsMandatory = false,
				IsRecommended = true
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, UCCChargesProvider.TransportCosts, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsIncludedInInvoiceAmountFixed = false,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, UCCChargesProvider.AirTransportCosts, new ChargeConfiguration
			{
				IsIncludedInInvoice = true,
				IsIncludedInInvoiceAmountFixed = false,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, UCCChargesProvider.ImportDutiesOrOther, new ChargeConfiguration
			{
				IsIncludedInInvoice = false,
				IsIncludedInInvoiceAmountFixed = true,
				IsMandatory = false,
				IsRecommended = false
			});
		}
	}
}
