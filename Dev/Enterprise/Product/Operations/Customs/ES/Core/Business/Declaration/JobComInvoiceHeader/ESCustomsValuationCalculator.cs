using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class ESCustomsValuationCalculator : EuCustomsValuationCalculator
	{
		public ESCustomsValuationCalculator(IChargeApportionee chargeApportionee)
			: base(chargeApportionee)
		{
		}

		public ZDecimal GetAdjustement(bool isPosAdj)
		{
			Money adjustement;
			if (isPosAdj)
			{
				adjustement = ChargeApportionee.CurrencyConverter.Add(ChargeApportionee.Charges.GetCharge(true, false),
					ChargeApportionee.ApportionedCharges.GetCharge(true, false));
			}
			else
			{
				adjustement = ChargeApportionee.CurrencyConverter.Add(ChargeApportionee.Charges.GetCharge(false, true),
					ChargeApportionee.ApportionedCharges.GetCharge(false, true));
			}

			return ChargeApportionee.CurrencyConverter.ConvertExact(adjustement, ChargeApportionee.CurrencyConverter.LocalCurrency).Amount;
		}

		public ZDecimal GetVATAdditions(ICurrency currency)
		{
			decimal result = GetChargesAndApportionedChargesAmountForVATAdditions(false, false, true, currency)
				+ GetVATDeductionValue(currency)
				- GetVATAdditionValue(currency)
				- GetREAChargeAmount((RefCurrency)currency);

			return result;
		}

		public ZDecimal GetVATAdditionValue(ICurrency currency) => GetChargesAndApportionedChargesAmountForVATAdditions(true, false, false, currency);
		public ZDecimal GetVATDeductionValue(ICurrency currency) => GetChargesAndApportionedChargesAmountForVATAdditions(false, true, true, currency);

		public ZDecimal GetEGVChargeAmount()
		{
			var egvChargeCode = ESCustomsChargeTypeList.Codes.ExportedGoodsValueForOutwardProcessing;
			var chargesAmount = ChargeApportionee.Charges.GetCharge(egvChargeCode, ChargeApportionee.CurrencyConverter.LocalCurrency);
			var apportionedChargesAmount = ChargeApportionee.ApportionedCharges.GetCharge(egvChargeCode, ChargeApportionee.CurrencyConverter.LocalCurrency);
			return chargesAmount + apportionedChargesAmount;
		}

		public ZDecimal GetEGPChargeAmount()
		{
			var egpChargeCode = ESCustomsChargeTypeList.Codes.InvoicedExportedGoodsValueForOutwardProcessing;
			var chargesAmount = ChargeApportionee.Charges.GetCharge(egpChargeCode, ChargeApportionee.CurrencyConverter.LocalCurrency);
			var apportionedChargesAmount = ChargeApportionee.ApportionedCharges.GetCharge(egpChargeCode, ChargeApportionee.CurrencyConverter.LocalCurrency);
			return chargesAmount + apportionedChargesAmount;
		}

		public ZDecimal GetREAChargeAmount(RefCurrency refCurrency = null)
		{
			var currency = refCurrency ?? ChargeApportionee.CurrencyConverter.LocalCurrency;

			var reaChargeCode = ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation;
			var chargesAmount = ChargeApportionee.Charges.GetCharge(reaChargeCode, currency);
			var apportionedChargesAmount = ChargeApportionee.ApportionedCharges.GetCharge(reaChargeCode, currency);
			return chargesAmount + apportionedChargesAmount;
		}

		ZDecimal GetChargesAndApportionedChargesAmountForVATAdditions(bool isDutiable, bool isIncludedInLines, bool isGSTApplicable, ICurrency currency)
		{
			var charges = ChargeApportionee.Charges.GetCharge(isDutiable, isIncludedInLines, isGSTApplicable);
			if (charges.Currency != currency)
			{
				charges = ChargeApportionee.CurrencyConverter.ConvertExact(charges, currency);
			}

			var apportionedCharges = ChargeApportionee.ApportionedCharges.GetCharge(isDutiable, isIncludedInLines, isGSTApplicable);
			if (apportionedCharges.Currency != currency)
			{
				apportionedCharges = ChargeApportionee.CurrencyConverter.ConvertExact(apportionedCharges, currency);
			}

			return charges.Amount + apportionedCharges.Amount;
		}

		public override ZDecimal GetAmountToAddToITOTForStatistical(RefCurrency currency)
		{
			var randomCustomCharge = ChargeApportionee.Charges.FirstOrDefault() ?? ChargeApportionee.ApportionedCharges?.Cast<JobComInvCharge>().FirstOrDefault();
			var chargeCode = randomCustomCharge?.ChargeCode as CustomsChargeCode;
			var isExport = chargeCode != null && chargeCode.IsForExport;
			if (isExport)
			{
				return ChargeApportionee.Charges.AmountToAddToITOTForStatisticalChargesES(currency, ChargeApportionee.Charges.CurrencyConverter) +
					ChargeApportionee.ApportionedCharges.AmountToAddToITOTForStatisticalChargesES(currency, ChargeApportionee.ApportionedCharges.CurrencyConverter);
			}
			else
			{
				return base.GetAmountToAddToITOTForStatistical(currency);
			}
		}
	}
}
