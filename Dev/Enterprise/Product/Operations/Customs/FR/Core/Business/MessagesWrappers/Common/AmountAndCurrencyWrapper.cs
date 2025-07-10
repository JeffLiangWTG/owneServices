using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class AmountAndCurrencyWrapper : IAmountAndCurrency
	{
		#region Constants
		public const string ShouldBeDutiable = "D1";
		public const string ShouldNotBeDutiable = "D0";
		public const string ShouldBeStatable = "S1";
		public const string ShouldNotBeStatable = "S0";
		public const string ShouldBeIncludedInInvoice = "I1";
		public const string ShouldNotBeIncludedInInvoice = "I0";
		public const string ShouldBeVatable = "V1";
		public const string ShouldNotBeVatable = "V0";
		public const string ShouldBeIncludedInInvoiceLine = "L1";
		public const string ShouldNotBeIncludedInInvoiceLine = "L0";
		public const string ShouldBeThirdCountryOrEUOrDomestic = "T1";
		public const string ShouldBeThirdCountryOrEU = "T2";
		public const string ShouldBeEUOrDomestic = "T3";
		public const string ShouldBeThirdCountry = "T4";
		public const string ShouldBeEU = "T5";
		public const string ShouldBeDomestic = "T6";
		#endregion

		public static AmountAndCurrencyWrapper New(CusEntryHeader entryHeader, IEnumerable<ZString> chargeCodes, ZString flags = new ZString())
		{
			Argument.NotNull(entryHeader, "CusEntryHeader cannot be null");

			var charges = new List<JobComInvCharge>();

			charges = entryHeader.InvoiceLines.Select(x => x.Charges.Cast<JobComInvCharge>().Concat(x.ApportionedCharges)).SelectMany(a => a).ToList();
			return new AmountAndCurrencyWrapper(charges, entryHeader.CurrencyConverter, chargeCodes, flags);
		}

		public static AmountAndCurrencyWrapper New(CusEntryLine entryLine, IEnumerable<ZString> chargeCodes, bool useApportionedCharges = false, ZString flags = new ZString())
		{
			Argument.NotNull(entryLine, "CusEntryLine cannot be null");

			var charges = new List<JobComInvCharge>();

			if (useApportionedCharges)
			{
				charges = entryLine.InvoiceLines.Select(x => x.Charges.Cast<JobComInvCharge>().Concat(x.ApportionedCharges)).SelectMany(a => a).ToList();
			}
			else
			{
				charges = entryLine.InvoiceLines.Select(x => x.Charges).SelectMany(a => a).Cast<JobComInvCharge>().ToList();
			}

			return new AmountAndCurrencyWrapper(charges, entryLine.CurrencyConverter, chargeCodes, flags);
		}

		protected AmountAndCurrencyWrapper(IEnumerable<JobComInvCharge> charges, CurrencyConverter currencyConverter, IEnumerable<ZString> chargeCodes, ZString flags)
		{
			var newChargeCodes = chargeCodes.Select(x => new ZString(x + "|" + flags));
			Charges = charges;
			CurrencyConverter = currencyConverter;
			ChargeCodes = newChargeCodes;
		}

		public ZDecimal Amount => GetTotal().Amount;

		public ZString Currency => GetTotal().Currency?.Code ?? Core.Constants.CurrencyCodes.EuropeanUnion;

		Money GetTotal()
		{
			if (totalMoney == null)
			{
				var filteredCharges = GetFlagsFilteredChargeList();
				var total = Money.Empty;
				filteredCharges.ForEach(x => total = CurrencyConverter.Add(total, x.Money));
				totalMoney = total;
			}
			return totalMoney;
		}
		Money totalMoney;

		List<JobComInvCharge> GetFlagsFilteredChargeList()
		{
			List<JobComInvCharge> filteredChargeList = new List<JobComInvCharge>();

			if (Charges != null)
			{
				foreach (var charge in Charges)
				{
					foreach (var itemChargeCode in ChargeCodes)
					{
						if (itemChargeCode.Contains(charge.J7_ChargeType, StringComparison.Ordinal))
						{
							if (CheckChargeFlags(charge, itemChargeCode))
							{
								filteredChargeList.Add(charge);
							}
						}
					}
				}
			}

			return filteredChargeList;
		}

		bool CheckChargeFlags(JobComInvCharge charge, ZString itemChargeCode)
		{
			var result = true;

			if ((itemChargeCode.Contains(ShouldBeDutiable, StringComparison.Ordinal) && !charge.J7_IsDutiable)
				|| (itemChargeCode.Contains(ShouldNotBeDutiable, StringComparison.Ordinal) && charge.J7_IsDutiable)
				|| (itemChargeCode.Contains(ShouldBeStatable, StringComparison.Ordinal) && !charge.J7_IsStatisticalValueApplicable)
				|| (itemChargeCode.Contains(ShouldNotBeStatable, StringComparison.Ordinal) && charge.J7_IsStatisticalValueApplicable)
				|| (itemChargeCode.Contains(ShouldBeVatable, StringComparison.Ordinal) && !charge.J7_IsGSTApplicable)
				|| (itemChargeCode.Contains(ShouldNotBeVatable, StringComparison.Ordinal) && charge.J7_IsGSTApplicable)
				|| (itemChargeCode.Contains(ShouldBeIncludedInInvoice, StringComparison.Ordinal) && charge.J7_IsNotIncludedInInvoice)
				|| (itemChargeCode.Contains(ShouldNotBeIncludedInInvoice, StringComparison.Ordinal) && !charge.J7_IsNotIncludedInInvoice)
				|| (itemChargeCode.Contains(ShouldBeIncludedInInvoiceLine, StringComparison.Ordinal) && !charge.J7_IsIncludedInITOT)
				|| (itemChargeCode.Contains(ShouldNotBeIncludedInInvoiceLine, StringComparison.Ordinal) && charge.J7_IsIncludedInITOT)
				|| (itemChargeCode.Contains(ShouldBeThirdCountryOrEUOrDomestic) && !IsThirdCountryOrEUOrDomestic(charge))
				|| (itemChargeCode.Contains(ShouldBeThirdCountryOrEU) && !IsThirdCountryOrEU(charge))
				|| (itemChargeCode.Contains(ShouldBeEUOrDomestic) && !IsEUOrDomestic(charge))
				|| (itemChargeCode.Contains(ShouldBeThirdCountry) && !IsThirdCountry(charge))
				|| (itemChargeCode.Contains(ShouldBeEU) && !IsEU(charge))
				|| (itemChargeCode.Contains(ShouldBeDomestic) && !IsDomestic(charge)))
			{
				result = false;
			}

			return result;
		}

		bool IsThirdCountry(JobComInvCharge charge)
		{
			return charge.J7_IsDutiable && charge.J7_IsStatisticalValueApplicable && charge.J7_IsGSTApplicable;
		}

		bool IsEU(JobComInvCharge charge)
		{
			return !charge.J7_IsDutiable && charge.J7_IsStatisticalValueApplicable && charge.J7_IsGSTApplicable;
		}

		bool IsDomestic(JobComInvCharge charge)
		{
			return !charge.J7_IsDutiable && !charge.J7_IsStatisticalValueApplicable && charge.J7_IsGSTApplicable;
		}

		bool IsThirdCountryOrEU(JobComInvCharge charge)
		{
			return IsThirdCountry(charge) || IsEU(charge);
		}

		bool IsEUOrDomestic(JobComInvCharge charge)
		{
			return IsEU(charge) || IsDomestic(charge);
		}

		bool IsThirdCountryOrEUOrDomestic(JobComInvCharge charge)
		{
			return IsThirdCountry(charge) || IsEU(charge) || IsDomestic(charge);
		}

		protected readonly IEnumerable<JobComInvCharge> Charges;
		protected readonly CurrencyConverter CurrencyConverter;
		protected readonly IEnumerable<ZString> ChargeCodes;
	}
}
