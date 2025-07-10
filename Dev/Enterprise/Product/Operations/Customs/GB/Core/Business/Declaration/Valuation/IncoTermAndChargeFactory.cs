using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class IncoTermAndChargeFactory : UCCIncoTermAndCustomsChargeFactory
	{
		public IncoTermAndChargeFactory() { }

		protected override ICustomsChargeCode[] GetCharges()
		{
			return new CustomsChargeCode[]
				{
					ChargesProvider.AirFreight,
					ChargesProvider.InternationalFreight,
					ChargesProvider.InternationalInsurance,
					ChargesProvider.AdditionCharge,
					ChargesProvider.DeductionCharge,
					ChargesProvider.VATAdjustment,
					ChargesProvider.Discount,
				};
		}

		protected override void SetupExWorksConfiguration()
		{
			SetUpForPreCIFIncoterm(Core.Constants.IncoTerms.ExWorks);
		}

		protected override void SetupFreeAlongsideShipConfiguration()
		{
			SetUpForPreCIFIncoterm(Core.Constants.IncoTerms.FreeAlongsideShip);
		}

		protected override void SetupFreeCarrierConfiguration()
		{
			SetUpForPreCIFIncoterm(Core.Constants.IncoTerms.FreeCarrier);
		}

		protected override void SetupFreeOnBoardConfiguration()
		{
			SetUpForPreCIFIncoterm(Core.Constants.IncoTerms.FreeOnBoard);
		}

		void SetUpForPreCIFIncoterm(string incoTerm)
		{
			AddChargeConfiguration(incoTerm, ChargesProvider.AirFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, ChargesProvider.InternationalFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(incoTerm, ChargesProvider.InternationalInsurance, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			SetUpForIncoTermNeutralCharges(incoTerm);
		}

		protected override void SetupDeliveredAtPlaceConfiguration()
		{
			SetUpForPostCIFIncoterm(Core.Constants.IncoTerms.DeliveredAtPlace, false);
		}

		protected override void SetupDeliveredAtTerminalConfiguration()
		{
			SetUpForPostCIFIncoterm(Core.Constants.IncoTerms.DeliveredAtTerminal, false);
		}

		protected override void SetupDeliveredExShipConfiguration()
		{
			SetUpForPostCIFIncoterm(Core.Constants.IncoTerms.DeliveredExShip);
		}

		protected override void SetupDeliveredExQuayConfiguration()
		{
			SetUpForPostCIFIncoterm(Core.Constants.IncoTerms.DeliveredExQuay);
		}

		protected override void SetupDeliveredAtFrontierConfiguration()
		{
			SetUpForPostCIFIncoterm(Core.Constants.IncoTerms.DeliveredAtFrontier);
		}

		protected override void SetupDeliveredDutyPaidConfiguration()
		{
			SetUpForPostCIFIncoterm(Core.Constants.IncoTerms.DeliveredDutyPaid, false);
		}

		protected override void SetupDeliveredDutyUnpaidConfiguration()
		{
			SetUpForPostCIFIncoterm(Core.Constants.IncoTerms.DeliveredDutyUnpaid);
		}

		protected override void SetupCarriageAndInsurancePaidToConfiguration()
		{
			SetUpForPostCIFIncoterm(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo);
		}

		protected override void SetupCostInsuranceAndFreightConfiguration()
		{
			SetUpForPostCIFIncoterm(Core.Constants.IncoTerms.CostInsuranceAndFreight);
		}

		void SetUpForPostCIFIncoterm(string incoTerm, bool isInternationalInsuranceInclude = true)
		{
			AddChargeConfiguration(incoTerm, ChargesProvider.AirFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, ChargesProvider.InternationalFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, ChargesProvider.InternationalInsurance, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = isInternationalInsuranceInclude, IsIncludedInInvoice = isInternationalInsuranceInclude, IsMandatory = false, IsRecommended = false });
			SetUpForIncoTermNeutralCharges(incoTerm);
		}

		protected override void SetupCarriagePaidToConfiguration()
		{
			SetUpForCostAndFreightLikeIncoterm(Core.Constants.IncoTerms.CarriagePaidTo);
		}

		protected override void SetupCostAndFreightConfiguration()
		{
			SetUpForCostAndFreightLikeIncoterm(Core.Constants.IncoTerms.CostAndFreight);
		}

		void SetUpForCostAndFreightLikeIncoterm(string incoTerm)
		{
			AddChargeConfiguration(incoTerm, ChargesProvider.AirFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, ChargesProvider.InternationalFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, ChargesProvider.InternationalInsurance, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			SetUpForIncoTermNeutralCharges(incoTerm);
		}

		void SetUpForIncoTermNeutralCharges(string incoTerm)
		{
			AddChargeConfiguration(incoTerm, ChargesProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, ChargesProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, ChargesProvider.VATAdjustment, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(incoTerm, ChargesProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		public override bool IsThisChargeRecommendedForThisInvoice(ICommonInvoice invoice, ZString incoterm, ZString chargeCode)
		{
			var chargeRecommended = true;

			if (chargeCode == ChargesProvider.AirFreightCode)
			{
				var invoiceHeader = invoice as JobComInvoiceHeader;
				chargeRecommended = invoiceHeader?.JobDeclaration?.IsAir ?? false;
			}

			return chargeRecommended;
		}

		protected override string FreightToEUBorderCodeCore => ChargesProvider.AirFreightCode;

		protected override string FreightAfterEUBorderCodeCore => ChargesProvider.AirFreightCode;

		public override void SetupAfterEUBorderCharge(JobComInvCharge charge, ZDecimal amount, ZString currency)
		{
			base.SetupAfterEUBorderCharge(charge, amount, currency);
			charge.J7_IsStatisticalValueApplicable = false;
		}
	}
}
