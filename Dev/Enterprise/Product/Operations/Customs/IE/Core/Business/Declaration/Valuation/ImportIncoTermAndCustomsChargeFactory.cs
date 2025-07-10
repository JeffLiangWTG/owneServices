using System.Collections.Generic;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ImportIncoTermAndCustomsChargeFactory : EU.Business.EUIncoTermAndCustomsChargeFactory
	{
		protected override string FreightToEUBorderCodeCore => AISChargeCodeList.Codes.AK;
		protected override MultilingualString FreightToEUBorderDesc => AISChargeCodeList.Descriptions.AK;

		protected override string FreightAfterEUBorderCodeCore => AISChargeCodeList.Codes._1X;
		protected override MultilingualString FreightAfterEUBorderDesc => AISChargeCodeList.Descriptions._1X;

		protected override ICustomsChargeCode[] GetCharges() => ImportChargesProvider.Codes;

		protected override void SetupIncotermChargeConfigurations()
		{
			base.SetupIncotermChargeConfigurations();
			SetupDeliveredAtFrontierConfiguration();
			SetupDeliveredExQuayConfiguration();
			SetupDeliveredExShipConfiguration();
			SetupDeliveredDutyUnpaidConfiguration();
			SetupOtherConfiguration();
		}

		protected override void SetupErrorConfiguration()
		{
		}

		protected virtual IReadOnlyList<ICustomsChargeCode> IncludedInInvoiceCharges => ImportChargesProvider.IncludedInInvoice;

		protected override void SetupExWorksConfiguration()
		{
			SetupChargeConfigs(Core.Constants.IncoTerms.ExWorks,
				includedInInvoiceCharges: IncludedInInvoiceCharges,
				includedInInvoiceDeemedForThisCharge: ImportChargesProvider.IncludedInInvoiceDeemedForThisCharge,
				recommendedCharges: ImportChargesProvider.RecommendedAndMandatoryCodesEXW_FCA_FAS_FOB,
				mandatoryCharges: ImportChargesProvider.RecommendedAndMandatoryCodesEXW_FCA_FAS_FOB
				);
		}

		protected override void SetupFreeCarrierConfiguration()
		{
			SetupChargeConfigs(Core.Constants.IncoTerms.FreeCarrier,
				includedInInvoiceCharges: IncludedInInvoiceCharges,
				includedInInvoiceDeemedForThisCharge: ImportChargesProvider.IncludedInInvoiceDeemedForThisCharge,
				recommendedCharges: ImportChargesProvider.RecommendedAndMandatoryCodesEXW_FCA_FAS_FOB,
				mandatoryCharges: ImportChargesProvider.RecommendedAndMandatoryCodesEXW_FCA_FAS_FOB
				);
		}

		protected override void SetupFreeAlongsideShipConfiguration()
		{
			SetupChargeConfigs(Core.Constants.IncoTerms.FreeAlongsideShip,
				includedInInvoiceCharges: IncludedInInvoiceCharges,
				includedInInvoiceDeemedForThisCharge: ImportChargesProvider.IncludedInInvoiceDeemedForThisCharge,
				recommendedCharges: ImportChargesProvider.RecommendedAndMandatoryCodesEXW_FCA_FAS_FOB,
				mandatoryCharges: ImportChargesProvider.RecommendedAndMandatoryCodesEXW_FCA_FAS_FOB
				);
		}

		protected override void SetupFreeOnBoardConfiguration()
		{
			SetupChargeConfigs(Core.Constants.IncoTerms.FreeOnBoard,
				includedInInvoiceCharges: IncludedInInvoiceCharges,
				includedInInvoiceDeemedForThisCharge: ImportChargesProvider.IncludedInInvoiceDeemedForThisCharge,
				recommendedCharges: ImportChargesProvider.RecommendedAndMandatoryCodesEXW_FCA_FAS_FOB,
				mandatoryCharges: ImportChargesProvider.RecommendedAndMandatoryCodesEXW_FCA_FAS_FOB
				);
		}

		protected override void SetupCostAndFreightConfiguration()
		{
			SetupChargeConfigs(Core.Constants.IncoTerms.CostAndFreight,
				includedInInvoiceCharges: ImportChargesProvider.IncludedInInvoice,
				includedInInvoiceDeemedForThisCharge: ImportChargesProvider.IncludedInInvoiceDeemedForThisCharge,
				recommendedCharges: ImportChargesProvider.RecommendedCodesCFR_CIF_CPT
				);
		}

		protected override void SetupCostInsuranceAndFreightConfiguration()
		{
			SetupChargeConfigs(Core.Constants.IncoTerms.CostInsuranceAndFreight,
				includedInInvoiceCharges: ImportChargesProvider.IncludedInInvoice,
				includedInInvoiceDeemedForThisCharge: ImportChargesProvider.IncludedInInvoiceDeemedForThisCharge,
				recommendedCharges: ImportChargesProvider.RecommendedCodesCFR_CIF_CPT);
		}

		protected override void SetupCarriagePaidToConfiguration()
		{
			SetupChargeConfigs(Core.Constants.IncoTerms.CarriagePaidTo,
				includedInInvoiceCharges: ImportChargesProvider.IncludedInInvoice,
				includedInInvoiceDeemedForThisCharge: ImportChargesProvider.IncludedInInvoiceDeemedForThisCharge,
				recommendedCharges: ImportChargesProvider.RecommendedCodesCFR_CIF_CPT
				);
		}

		protected override void SetupCarriageAndInsurancePaidToConfiguration()
		{
			SetupChargeConfigs(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, includedInInvoiceCharges: ImportChargesProvider.IncludedInInvoice, includedInInvoiceDeemedForThisCharge: ImportChargesProvider.IncludedInInvoiceDeemedForThisCharge);
		}

		protected override void SetupDeliveredAtTerminalConfiguration()
		{
			SetupChargeConfigs(Core.Constants.IncoTerms.DeliveredAtTerminal, includedInInvoiceCharges: ImportChargesProvider.IncludedInInvoice, includedInInvoiceDeemedForThisCharge: ImportChargesProvider.IncludedInInvoiceDeemedForThisCharge);
		}

		protected override void SetupDeliveredAtPlaceConfiguration()
		{
			SetupChargeConfigs(Core.Constants.IncoTerms.DeliveredAtPlace, includedInInvoiceCharges: ImportChargesProvider.IncludedInInvoice, includedInInvoiceDeemedForThisCharge: ImportChargesProvider.IncludedInInvoiceDeemedForThisCharge);
		}

		protected void SetupDeliveredAtFrontierConfiguration()
		{
			SetupChargeConfigs(Core.Constants.IncoTerms.DeliveredAtFrontier, includedInInvoiceCharges: ImportChargesProvider.IncludedInInvoice, includedInInvoiceDeemedForThisCharge: ImportChargesProvider.IncludedInInvoiceDeemedForThisCharge);
		}

		protected void SetupDeliveredExQuayConfiguration()
		{
			SetupChargeConfigs(Core.Constants.IncoTerms.DeliveredExQuay, includedInInvoiceCharges: ImportChargesProvider.IncludedInInvoice, includedInInvoiceDeemedForThisCharge: ImportChargesProvider.IncludedInInvoiceDeemedForThisCharge);
		}
		protected void SetupDeliveredExShipConfiguration()
		{
			SetupChargeConfigs(Core.Constants.IncoTerms.DeliveredExShip, includedInInvoiceCharges: ImportChargesProvider.IncludedInInvoice, includedInInvoiceDeemedForThisCharge: ImportChargesProvider.IncludedInInvoiceDeemedForThisCharge);
		}
		protected void SetupDeliveredDutyUnpaidConfiguration()
		{
			SetupChargeConfigs(Core.Constants.IncoTerms.DeliveredDutyUnpaid, includedInInvoiceCharges: ImportChargesProvider.IncludedInInvoice, includedInInvoiceDeemedForThisCharge: ImportChargesProvider.IncludedInInvoiceDeemedForThisCharge);
		}
		protected override void SetupDeliveredDutyPaidConfiguration()
		{
			SetupChargeConfigs(Core.Constants.IncoTerms.DeliveredDutyPaid,
				includedInInvoiceCharges: ImportChargesProvider.IncludedInInvoice,
				includedInInvoiceDeemedForThisCharge: ImportChargesProvider.IncludedInInvoiceDeemedForThisCharge,
				recommendedCharges: ImportChargesProvider.RecommendedAndMandatoryCodesDDP,
				mandatoryCharges: ImportChargesProvider.RecommendedAndMandatoryCodesDDP
				);
		}

		protected void SetupOtherConfiguration()
		{
			SetupChargeConfigs(Core.Constants.IncoTerms.Other, includedInInvoiceCharges: ImportChargesProvider.IncludedInInvoice, includedInInvoiceDeemedForThisCharge: ImportChargesProvider.IncludedInInvoiceDeemedForThisCharge);
		}

		public override void SetupToEUBorderCharge(JobComInvCharge charge, ZDecimal amount, ZString currency)
		{
			charge.J7_ChargeType = FreightToEUBorderCode;
			charge.J7_Amount = amount;
			charge.J7_RX_NKCurrency = currency;
		}

		public override void SetupAfterEUBorderCharge(JobComInvCharge charge, ZDecimal amount, ZString currency)
		{
			charge.J7_ChargeType = FreightAfterEUBorderCode;
			charge.J7_Amount = amount;
			charge.J7_RX_NKCurrency = currency;
		}

		void SetupChargeConfigs(string incoTerm, IReadOnlyList<ICustomsChargeCode> includedInInvoiceCharges, ICustomsChargeCode[] includedInInvoiceDeemedForThisCharge, ICustomsChargeCode[] recommendedCharges = null, ICustomsChargeCode[] mandatoryCharges = null)
		{
			foreach (var chargeCode in ImportChargesProvider.Codes)
			{
				var chargeConfiguration = new ChargeConfiguration();

				var isIncludedInInvoice = includedInInvoiceCharges != null && chargeCode.In(includedInInvoiceCharges);
				if (isIncludedInInvoice)
				{
					chargeConfiguration.IsIncludedInInvoice = true;
				}

				var isIncludedInInvoiceDeemedForThisCharge = includedInInvoiceDeemedForThisCharge != null && chargeCode.In(includedInInvoiceDeemedForThisCharge);
				if (isIncludedInInvoiceDeemedForThisCharge)
				{
					chargeConfiguration.IsIncludedInInvoiceAmountFixed = true;
				}

				var isRecommended = recommendedCharges != null && chargeCode.In(recommendedCharges);
				if (isRecommended)
				{
					chargeConfiguration.IsRecommended = true;
				}

				var isMandatory = mandatoryCharges != null && chargeCode.In(mandatoryCharges);
				if (isMandatory)
				{
					chargeConfiguration.IsMandatory = true;
				}

				if (isIncludedInInvoice || isIncludedInInvoiceDeemedForThisCharge || isRecommended || isMandatory)
				{
					AddChargeConfiguration(incoTerm, chargeCode, chargeConfiguration);
				}
			}
		}

		public virtual bool CanThisIncoTermHaveThisChargeForValidation(ZString incoTerm, ICustomsChargeCode charge)
		{
			var chargeConfiguration = GetConfiguration(incoTerm, charge?.Code ?? ZString.Empty);
			return chargeConfiguration != null && chargeConfiguration.IsIncludedInInvoice;
		}
	}
}
