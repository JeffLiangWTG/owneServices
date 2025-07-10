using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business
{
	public sealed class DeCustomsValuationCalculator : EU.Business.Declaration.EuCustomsValuationCalculator
	{
		public DeCustomsValuationCalculator(IChargeApportionee chargeApportionee) : base(chargeApportionee)
		{
		}

		public ZDecimal GetValueForVat(JobComInvoiceLine line)
			=> GetChargesAndApportionedChargesAmountForVATAdditions(false, false, true, line.LocalCurrency) + GetChargesAndApportionedChargesAmountForVATAdditions(false, true, true, line.LocalCurrency);

		public override ZDecimal GetAmountToAddToITOTForVatableGstable(RefCurrency currency) => AddValues(c => c.J7_IsGSTApplicable, currency);

		public override ZDecimal GetAmountToAddToITOTForStatistical(RefCurrency currency) => AddValues(c => c.J7_IsStatisticalValueApplicable, currency);

		public override ZDecimal GetAmountToAddToITOTForDutiable(RefCurrency currency) => AddValues(c => c.J7_IsDutiable, currency);

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

		ZDecimal AddValues(Func<JobComInvCharge, bool> isApplicable, RefCurrency currency)
		{
			var currencyConverter = ChargeApportionee.CurrencyConverter;
			var charges = ChargeApportionee.Charges.ChargesToAddToITOTWithThisFlag(isApplicable)
				.Concat(ChargeApportionee.ApportionedCharges.ChargesToAddToITOTWithThisFlag(isApplicable));
			var amount = charges.Aggregate(ZDecimal.Zero, (current, charge) =>
			{
				var valueInCurrency = IsIATA(charge.Charge) ? IataConverter.ConvertExact(charge.Charge.Money, currency).Amount : currencyConverter.ConvertExact(charge.Charge.Money, currency).Amount;
				return current + valueInCurrency * (charge.Substract ? -1 : 1);
			});

			return amount;
		}
		CurrencyConverter IataConverter
		{
			get
			{
				if (iataConverter == null)
				{
					iataConverter = new RefCurrencyCurrencyConverter(ChargeApportionee.Factory, ChargeApportionee.CurrencyConverter.DateForRate, ZArchitecture.Core.ExchangeRateType.IATA, true);
				}
				else
				{
					var currencyConverter = ChargeApportionee.CurrencyConverter;
					if (iataConverter.DateForRate != currencyConverter.DateForRate)
					{
						iataConverter.DateForRate = currencyConverter.DateForRate;
					}
				}
				return iataConverter;
			}
		}
		CurrencyConverter iataConverter;

		static ZBool IsIATA(JobComInvCharge charge) => charge.J7_ExchangeRateType == ChargeExchangeRateTypeList.Codes.IATARate;
	}
}
