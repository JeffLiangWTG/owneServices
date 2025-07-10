using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business
{
	public static class IChargeApportioneeExtensionMethods
	{
		public static ZDecimal GetTotalAmountInLocalCurrency(this IChargeApportionee chargeApportionee, string chargeCode, CurrencyConverter currencyConverter)
		{
			var totalAmountInLocalCurrency = ZDecimal.Zero;
			foreach (var charge in chargeApportionee.Charges.GetCharge(chargeCode))
			{
				totalAmountInLocalCurrency += currencyConverter.ConvertExact(charge.Money, currencyConverter.LocalCurrency).Amount.Truncate();
			}
			foreach (var apportionedCharge in chargeApportionee.ApportionedCharges.GetCharge(chargeCode))
			{
				totalAmountInLocalCurrency += currencyConverter.ConvertExact(apportionedCharge.Money, currencyConverter.LocalCurrency).Amount.Truncate();
			}
			return totalAmountInLocalCurrency;
		}

		public static ZDecimal GetImportTotalInvoiceAmountInLocalCurrency(this IChargeApportionee chargeApportionee, CurrencyConverter currencyConverter)
		{
			return calculateImportChargesInLocalCurrency(chargeApportionee, currencyConverter, new Func<JobComInvCharge, bool>(charge => charge.J7_Calc_IsIncludedInInvoiceAmount && !charge.J7_IsIncludedInITOT));
		}

		public static ZDecimal GetImportAdditionalAmountInLocalCurrency(this IChargeApportionee chargeApportionee, CurrencyConverter currencyConverter, string valuationCode)
		{
			return calculateImportChargesInLocalCurrency(chargeApportionee, currencyConverter, new Func<JobComInvCharge, bool>(charge => ImportChargeMethodCodeList.GetAdditionalAmountList(valuationCode).Contains((string)charge.J7_ChargeType) && !charge.J7_Calc_IsIncludedInInvoiceAmount && charge.J7_IsDutiable));
		}

		public static ZDecimal GetImportDeductionAmountInLocalCurrency(this IChargeApportionee chargeApportionee, CurrencyConverter currencyConverter, string valuationCode)
		{
			return calculateImportChargesInLocalCurrency(chargeApportionee, currencyConverter, new Func<JobComInvCharge, bool>(charge => ImportChargeMethodCodeList.GetDeductionAmountList(valuationCode).Contains((string)charge.J7_ChargeType) && charge.J7_Calc_IsIncludedInInvoiceAmount && !charge.J7_IsDutiable));
		}

		public static ZDecimal GetImportFreightInLocalCurrency(this IChargeApportionee chargeApportionee, CurrencyConverter currencyConverter, string valuationCode)
		{
			return calculateImportChargesInLocalCurrency(chargeApportionee, currencyConverter, new Func<JobComInvCharge, bool>(charge => ImportChargeMethodCodeList.GetFreightList(valuationCode).Contains((string)charge.J7_ChargeType)));
		}

		public static ZDecimal GetImportInsuranceInLocalCurrency(this IChargeApportionee chargeApportionee, CurrencyConverter currencyConverter, string valuationCode)
		{
			return calculateImportChargesInLocalCurrency(chargeApportionee, currencyConverter, new Func<JobComInvCharge, bool>(charge => ImportChargeMethodCodeList.GetInsuranceList(valuationCode).Contains((string)charge.J7_ChargeType)));
		}

		static ZDecimal calculateImportChargesInLocalCurrency(this IChargeApportionee chargeApportionee, CurrencyConverter currencyConverter, Func<JobComInvCharge, bool> needsCalculation)
		{
			Money result = Money.Empty;
			foreach (var charge in chargeApportionee.Charges)
			{
				if (needsCalculation(charge))
				{
					result = currencyConverter.Add(result, currencyConverter.ConvertExact(charge.Money, currencyConverter.LocalCurrency));
				}
			}
			foreach (var apportionedCharge in chargeApportionee.ApportionedCharges)
			{
				if (needsCalculation(apportionedCharge))
				{
					result = currencyConverter.Add(result, currencyConverter.ConvertExact(apportionedCharge.Money, currencyConverter.LocalCurrency));
				} 
			}
			return result.Amount.Truncate();
		}
	}
}
