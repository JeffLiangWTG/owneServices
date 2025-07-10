using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Accounting.Business
{
	//
	// !!! This class is used in web service, which means that Env.CurrentCompany can be null. Use GetCurrentCompany method instead. !!!
	//
	public static class TaxAmountCalculator
	{
		public static ZDecimal GetLocalTaxAmount(BusinessObjectFactory factory, ZGuid company, ZDecimal localExTaxAmount, AccTaxRate taxRate, ZDecimal? rate, ZDecimal? effectiveExtraRate, bool withoutRounding = false)
		{
			return taxRate != null ? GetLocalTaxAmount(factory, company, localExTaxAmount, taxRate, rate, effectiveExtraRate, ZDecimal.Zero, ZDecimal.Zero, true, withoutRounding) : ZDecimal.Zero;
		}

		public static ZDecimal GetLocalTaxAmount(BusinessObjectFactory factory, ZGuid company, ZDecimal localExTaxAmount, AccTaxRate taxRate, ZDecimal? rate, ZDecimal? effectiveExtraRate, ZDecimal osTaxAmount
			, ZDecimal exchangeRate, bool recalculateFromExTaxAmount = false, bool withoutRounding = false)
		{
			return GetLocalTax(factory, localExTaxAmount, taxRate, rate ?? 0, effectiveExtraRate ?? 0, osTaxAmount, exchangeRate, recalculateFromExTaxAmount, withoutRounding, amount => GetTaxAmountFromExTaxAmount(amount, taxRate, rate ?? 0, effectiveExtraRate ?? 0, GetCurrentCompany(factory, company).LocalCurrency.Decimals), company);
		}

		public static ZDecimal GetLocalTaxAmountFromOSTaxAmount(BusinessObjectFactory factory, ZGuid company, ZDecimal osTaxAmount, ZDecimal exchangeRate, bool withoutRounding = false)
		{
			return GetLocalTax(factory, ZDecimal.Zero, null, ZDecimal.Zero, ZDecimal.Zero, osTaxAmount, exchangeRate, false, withoutRounding, (a) => ZDecimal.Zero, company);
		}

		public static (ZDecimal oSTaxAmount, ZDecimal oSExTaxAmount) SplitOSTotalToTaxAndExTaxAmounts(ZDecimal localTaxAmount, Func<ZDecimal> calculateOSExTaxAmountFromLocalExTaxAmount, ZDecimal overseasTotal)
		{
			var oSExTaxAmount = localTaxAmount == 0m
				? overseasTotal
				: calculateOSExTaxAmountFromLocalExTaxAmount();
			var oSTaxAmount = overseasTotal - oSExTaxAmount;
			return (oSTaxAmount, oSExTaxAmount);
		}

		public static ZDecimal GetOSTaxAmount(BusinessObjectFactory factory, ZDecimal osExTaxAmount, AccTaxRate taxRate, ZDecimal? rate, ZDecimal? effectiveExtraRate, RefCurrency currency, ZGuid company, bool withoutRounding = false)
		{
			return taxRate != null ? GetOSTaxAmount(factory, osExTaxAmount, taxRate, rate, effectiveExtraRate, ZDecimal.Zero, ZDecimal.Zero, currency, company, true, withoutRounding) : ZDecimal.Zero;
		}

		public static ZDecimal GetOSTaxAmount(BusinessObjectFactory factory, ZDecimal osExTaxAmount, AccTaxRate taxRate, ZDecimal? rate, ZDecimal? effectiveExtraRate, ZDecimal localTaxAmount, ZDecimal exchangeRate
			, RefCurrency currency, ZGuid company, bool recalculateFromExTaxAmount = false, bool withoutRounding = false)
		{
			return GetOSTax(factory, osExTaxAmount, taxRate, localTaxAmount, exchangeRate, currency, recalculateFromExTaxAmount, withoutRounding, amount => GetTaxAmountFromExTaxAmount(amount, taxRate, rate ?? 0, effectiveExtraRate ?? 0, currency.Decimals), company);
		}

		public static ZDecimal GetTaxAmountFromExTaxAmount(ZDecimal exTaxAmount, AccTaxRate taxRate, ZDecimal rate, ZDecimal effectiveExtraRate, int decimalPlace)
		{
			ZDecimal result;
			if (taxRate.IsIndiaStateTax || taxRate.IsMexicoNeedExtraType)
			{
				result = Utilities.Round(exTaxAmount * rate / 100, decimalPlace) +
						 Utilities.Round(exTaxAmount * effectiveExtraRate / 100, decimalPlace);
			}
			else
			{
				result = exTaxAmount / 100 * (rate + effectiveExtraRate);
			}

			return result;
		}

		public static ZDecimal GetLocalGSTAmountFromLocalTaxAmount(BusinessObjectFactory factory, ZGuid company, ZDecimal localTaxAmount, AccTaxRate taxRate, ZDecimal? rate, ZDecimal? effectiveExtraRate, ZDecimal osGstAmount
			, ZDecimal exchangeRate, bool recalculateFromTaxAmount = false, bool withoutRounding = false)
		{
			return GetLocalTax(factory, localTaxAmount, taxRate, rate ?? 0, effectiveExtraRate ?? 0, osGstAmount, exchangeRate, recalculateFromTaxAmount, withoutRounding, amount => GetGSTAmountFromTaxAmount(amount, rate ?? 0, effectiveExtraRate ?? 0), company);
		}

		public static ZDecimal GetOSGSTAmountFromOSTaxAmount(BusinessObjectFactory factory, ZDecimal osTaxAmount, AccTaxRate taxRate, ZDecimal? rate, ZDecimal? effectiveExtraRate, RefCurrency currency, ZGuid company, bool withoutRounding = false)
		{
			return taxRate != null ? GetOSGSTAmountFromOSTaxAmount(factory, osTaxAmount, taxRate, rate, effectiveExtraRate, ZDecimal.Zero, ZDecimal.Zero, currency, company, true, withoutRounding) : ZDecimal.Zero;
		}

		public static ZDecimal GetOSGSTAmountFromOSTaxAmount(BusinessObjectFactory factory, ZDecimal osTaxAmount, AccTaxRate taxRate, ZDecimal? rate, ZDecimal? effectiveExtraRate, ZDecimal localGSTAmount, ZDecimal exchangeRate
			, RefCurrency currency, ZGuid company, bool recalculateFromTaxAmount = false, bool withoutRounding = false)
		{
			return GetOSTax(factory, osTaxAmount, taxRate, localGSTAmount, exchangeRate, currency, recalculateFromTaxAmount, withoutRounding, amount => GetGSTAmountFromTaxAmount(amount, rate ?? 0, effectiveExtraRate ?? 0), company);
		}

		public static ZDecimal GetLocalGSTAmountFromLocalExTaxAmount(BusinessObjectFactory factory, ZGuid company, ZDecimal localExTaxAmount, AccTaxRate taxRate, ZDecimal? rate, ZDecimal? effectiveExtraRate, ZDecimal osGstAmount
			, ZDecimal exchangeRate, bool recalculateFromExTaxAmount = false, bool withoutRounding = false)
		{
			return GetLocalTax(factory, localExTaxAmount, taxRate, rate ?? 0, effectiveExtraRate ?? 0, osGstAmount, exchangeRate, recalculateFromExTaxAmount, withoutRounding, amount => GetGSTAmount(amount, rate ?? 0), company);
		}

		public static ZDecimal GetOSGSTAmountFromOSExTaxAmount(BusinessObjectFactory factory, ZDecimal osExTaxAmount, AccTaxRate taxRate, ZDecimal? rate, RefCurrency currency, ZGuid company, bool withoutRounding = false)
		{
			return GetOSGSTAmountFromOSExTaxAmount(factory, osExTaxAmount, taxRate, rate, ZDecimal.Zero, ZDecimal.Zero, currency, company, true, withoutRounding);
		}

		public static ZDecimal GetOSGSTAmountFromOSExTaxAmount(BusinessObjectFactory factory, ZDecimal osExTaxAmount, AccTaxRate taxRate, ZDecimal? rate, ZDecimal localGSTAmount, ZDecimal exchangeRate
			, RefCurrency currency, ZGuid company, bool recalculateFromExTaxAmount = false, bool withoutRounding = false)
		{
			return GetOSTax(factory, osExTaxAmount, taxRate, localGSTAmount, exchangeRate, currency, recalculateFromExTaxAmount, withoutRounding, amount => GetGSTAmount(amount, rate ?? 0), company);
		}

		public static ZDecimal GetLocalExtraTaxAmountFromLocalTaxAmount(BusinessObjectFactory factory, ZGuid company, ZDecimal localTaxAmount, AccTaxRate taxRate, ZDecimal? rate, ZDecimal? effectiveExtraRate, bool withoutRounding = false)
		{
			return taxRate != null ? GetLocalExtraTaxAmountFromTaxAmount(factory, company, localTaxAmount, taxRate, rate, effectiveExtraRate, osExtraTaxAmount: ZDecimal.Zero
				, localExtraTaxAmount: ZDecimal.Zero, exchangeRate: ZDecimal.Zero, recalculateFromTaxAmount: true, withoutRounding: withoutRounding) : ZDecimal.Zero;
		}

		public static ZDecimal GetLocalExtraTaxAmountFromTaxAmount(BusinessObjectFactory factory, ZGuid company, ZDecimal localTaxAmount, AccTaxRate taxRate, ZDecimal? rate, ZDecimal? effectiveExtraRate,
			ZDecimal osExtraTaxAmount, ZDecimal localExtraTaxAmount
			, ZDecimal exchangeRate, bool recalculateFromTaxAmount = false, bool withoutRounding = false)
		{
			return GetLocalExtraTax(factory, localTaxAmount, taxRate, rate ?? 0, effectiveExtraRate ?? 0, osExtraTaxAmount, localExtraTaxAmount, exchangeRate, recalculateFromTaxAmount, withoutRounding, amount => GetExtraTaxAmountFromTaxAmount(amount, rate ?? 0, effectiveExtraRate ?? 0), company);
		}

		public static ZDecimal GetOSExtraTaxAmountFromOSTaxAmount(BusinessObjectFactory factory, ZDecimal osTaxAmount, AccTaxRate taxRate, ZDecimal? rate, ZDecimal? effectiveExtraRate, RefCurrency currency, ZGuid companyPK, bool withoutRounding = false)
		{
			return GetOSExtraTaxAmountFromOSTaxAmount(factory, osTaxAmount, taxRate, rate, effectiveExtraRate, ZDecimal.Zero, ZDecimal.Zero, currency, companyPK, true, withoutRounding);
		}

		public static ZDecimal GetOSExtraTaxAmountFromOSTaxAmount(BusinessObjectFactory factory, ZDecimal osTaxAmount, AccTaxRate taxRate, ZDecimal? rate, ZDecimal? effectiveExtraRate, ZDecimal localExtraTaxAmount, ZDecimal exchangeRate
			, RefCurrency currency, ZGuid companyPK, bool recalculateFromTaxAmount = false, bool withoutRounding = false)
		{
			return GetOSExtraTax(factory, osTaxAmount, taxRate, localExtraTaxAmount, exchangeRate, currency, companyPK, recalculateFromTaxAmount, withoutRounding, amount => GetExtraTaxAmountFromTaxAmount(amount, rate ?? 0, effectiveExtraRate ?? 0));
		}

		public static ZDecimal GetLocalExtraTaxAmountFromLocalExTaxAmount(BusinessObjectFactory factory, ZGuid company, ZDecimal localExTaxAmount, AccTaxRate taxRate, ZDecimal? rate, ZDecimal? effectiveExtraRate
			, ZDecimal osExtraTaxAmount, ZDecimal localExtraTaxAmount, ZDecimal exchangeRate, bool recalculateFromExTaxAmount = false, bool withoutRounding = false)
		{
			return GetLocalExtraTax(factory, localExTaxAmount, taxRate, rate ?? 0, effectiveExtraRate ?? 0, osExtraTaxAmount, localExtraTaxAmount, exchangeRate, recalculateFromExTaxAmount, withoutRounding, amount => GetExtraTaxAmountFromExTaxAmount(amount, effectiveExtraRate ?? 0), company);
		}

		public static ZDecimal GetOSExtraTaxAmountFromOSExTaxAmount(BusinessObjectFactory factory, ZDecimal osExTaxAmount, AccTaxRate taxRate, ZDecimal? effectiveExtraRate, RefCurrency currency, ZGuid companyPK, bool withoutRounding = false)
		{
			return taxRate != null ? GetOSExtraTaxAmountFromOSExTaxAmount(factory, osExTaxAmount, taxRate, effectiveExtraRate, ZDecimal.Zero, ZDecimal.Zero, currency, companyPK, true, withoutRounding) : ZDecimal.Zero;
		}

		public static ZDecimal GetOSExtraTaxAmountFromOSExTaxAmount(BusinessObjectFactory factory, ZDecimal osExTaxAmount, AccTaxRate taxRate, ZDecimal? effectiveExtraRate
			, ZDecimal localExtraTaxAmount, ZDecimal exchangeRate, RefCurrency currency, ZGuid companyPK, bool recalculateFromExTaxAmount = false, bool withoutRounding = false)
		{
			return GetOSExtraTax(factory, osExTaxAmount, taxRate, localExtraTaxAmount, exchangeRate, currency, companyPK, recalculateFromExTaxAmount, withoutRounding, amount => GetExtraTaxAmountFromExTaxAmount(amount, effectiveExtraRate ?? 0));
		}

		public static ZDecimal GetLocalWithholdingTaxAmountFromLocalExTaxAmount(ZDecimal localExTaxAmount, AccWithholding withholdingRate, bool withoutRounding = false)
		{
			return GetWithholdingTaxAmount(localExTaxAmount, withholdingRate, GlbCompany.CurrentCompany.LocalCurrency, withoutRounding);
		}

		public static ZDecimal GetOSWithholdingTaxAmountFromLocalWithHoldingAmount(ZDecimal localWHTAmount, ZDecimal exchangeRate, RefCurrency currency)
		{
			if (currency != null)
			{
				return Env.CurrentCompany.ExchangeRate.LocalToForeign(localWHTAmount, exchangeRate, currency.RX_Code);
			}

			return ZDecimal.Zero;
		}

		public static ZDecimal GetOSWithholdingTaxAmountFromLocalExTaxAmount(ZDecimal localExTaxAmount, AccWithholding withholdingRate, ZDecimal exchangeRate, RefCurrency currency)
		{
			var localWithHoldingTax = GetLocalWithholdingTaxAmountFromLocalExTaxAmount(localExTaxAmount, withholdingRate, true);
			return GetOSWithholdingTaxAmountFromLocalWithHoldingAmount(localWithHoldingTax, exchangeRate, currency);
		}

		public static ZDecimal GetOSWithholdingTaxFromOSExTaxAmount(ZDecimal osExTaxAmount, AccWithholding withholdingRate, RefCurrency currency)
		{
			return GetWithholdingTaxAmount(osExTaxAmount, withholdingRate, currency);
		}

		static ZDecimal GetWithholdingTaxAmount(ZDecimal amount, AccWithholding withholdingRate, RefCurrency currency, bool withoutRounding = false)
		{
			var result = ZDecimal.Zero;
			if (withholdingRate != null)
			{
				result = amount / 100 * withholdingRate.AW_Rate;
			}

			if (withoutRounding)
			{
				return result;
			}
			else
			{
				if (currency != null)
				{
					return (ZDecimal)Utilities.Round(result, currency.Decimals);
				}
			}

			return result;
		}

		internal static ICompany GetCurrentCompany(BusinessObjectFactory factory, ZGuid companyPK)
		{
			var company = Globals.IsWeb ? null : Env.CurrentCompany;
			if (company?.PK != companyPK)
			{
				company = factory.Load<GlbCompany>(companyPK);
			}

			return company;
		}

		static ZDecimal GetLocalTax(BusinessObjectFactory factory, ZDecimal localSourceAmount, AccTaxRate taxRate, ZDecimal rate, ZDecimal effectiveExtraRate, ZDecimal osTaxAmount, ZDecimal exchangeRate, bool recalculateFromExTaxAmount
			, bool withoutRounding, Func<ZDecimal, ZDecimal> calculateTax, ZGuid company)
		{
			var result = ZDecimal.Zero;

			if (localSourceAmount.IsEmpty && osTaxAmount.IsEmpty)
			{
				return result;
			}

			if ((ShouldCalculateTaxFromLocalAmount(factory, taxRate, company, rate, effectiveExtraRate) || recalculateFromExTaxAmount) &&
				(taxRate != null && !taxRate.IsMexicoNeedExtraType))
			{
				result = calculateTax(localSourceAmount);
				result = withoutRounding || result.IsEmpty ? result : (ZDecimal)Utilities.Round(result, GetCurrentCompany(factory, company).LocalCurrency.Decimals);
			}
			else if (!osTaxAmount.IsEmpty)
			{
				result = withoutRounding ?
							GetCurrentCompany(factory, company).ExchangeRate.ForeignToLocalWithoutRounding(osTaxAmount, exchangeRate) :
							GetCurrentCompany(factory, company).ExchangeRate.ForeignToLocal(osTaxAmount, exchangeRate);
			}

			return result;
		}

		static ZDecimal GetOSTax(BusinessObjectFactory factory, ZDecimal osSourceAmount, AccTaxRate taxRate, ZDecimal localTaxAmount, ZDecimal exchangeRate, RefCurrency currency, bool enforceRecalculation, bool withoutRounding
			, Func<ZDecimal, ZDecimal> calcululateTax, ZGuid company)
		{
			var result = ZDecimal.Zero;

			if (currency == null || osSourceAmount.IsEmpty && localTaxAmount.IsEmpty)
			{
				return result;
			}

			if (taxRate != null && (enforceRecalculation || taxRate.IsMexicoNeedExtraType))
			{
				result = calcululateTax(osSourceAmount);
				result = withoutRounding || result.IsEmpty ? result : (ZDecimal)Utilities.Round(result, currency.Decimals);
			}
			else if (!localTaxAmount.IsEmpty)
			{
				result = withoutRounding ?
					GetCurrentCompany(factory, company).ExchangeRate.LocalToForeignWithoutRounding(localTaxAmount, exchangeRate, currency.RX_Code) :
					GetCurrentCompany(factory, company).ExchangeRate.LocalToForeign(localTaxAmount, exchangeRate, currency.RX_Code);
			}

			return result;
		}

		static ZDecimal GetGSTAmountFromTaxAmount(ZDecimal taxAmount, ZDecimal rate, ZDecimal effectiveExtraRate)
		{
			return effectiveExtraRate + rate != 0 ? taxAmount / (effectiveExtraRate + rate) * rate : 0M;
		}

		internal static decimal GetGSTAmount(ZDecimal osExTaxAmount, ZDecimal rate)
		{
			return osExTaxAmount / 100 * rate;
		}

		static ZDecimal GetLocalExtraTax(BusinessObjectFactory factory, ZDecimal localTaxAmount, AccTaxRate taxRate, ZDecimal rate, ZDecimal effectiveExtraRate, ZDecimal osExtraTaxAmount, ZDecimal localExtraTaxAmount, ZDecimal exchangeRate
			, bool recalculateFromTaxAmount, bool withoutRounding, Func<ZDecimal, ZDecimal> calculateExtraTax, ZGuid company)
		{
			var result = ZDecimal.Zero;

			if (localTaxAmount.IsEmpty && osExtraTaxAmount.IsEmpty && localExtraTaxAmount.IsEmpty)
			{
				return result;
			}

			if (taxRate != null && taxRate.IsLocalExtraTaxAmountValuePersistent)
			{
				if (!localExtraTaxAmount.IsEmpty)
				{
					result = withoutRounding ?
						localExtraTaxAmount :
						(ZDecimal)Utilities.Round(localExtraTaxAmount, GetCurrentCompany(factory, company).LocalCurrency.Decimals);

					return result;
				}
			}

			if (ShouldCalculateTaxFromLocalAmount(factory, taxRate, company, rate, effectiveExtraRate) || (taxRate != null && recalculateFromTaxAmount))
			{
				result = calculateExtraTax(localTaxAmount);
				result = withoutRounding || result.IsEmpty ? result : (ZDecimal)Utilities.Round(result, GetCurrentCompany(factory, company).LocalCurrency.Decimals);
			}
			else if (!osExtraTaxAmount.IsEmpty)
			{
				result = withoutRounding ?
							GetCurrentCompany(factory, company).ExchangeRate.ForeignToLocalWithoutRounding(osExtraTaxAmount, exchangeRate) :
							GetCurrentCompany(factory, company).ExchangeRate.ForeignToLocal(osExtraTaxAmount, exchangeRate);
			}

			return result;
		}

		static ZDecimal GetOSExtraTax(BusinessObjectFactory factory, ZDecimal osSourceAmount, AccTaxRate taxRate, ZDecimal localExtraTaxAmount, ZDecimal exchangeRate, RefCurrency currency, ZGuid companyPK, bool enforceRecalculation, bool withoutRounding
			, Func<ZDecimal, ZDecimal> calcululateTax)
		{
			var result = ZDecimal.Zero;

			if (currency == null || osSourceAmount.IsEmpty && localExtraTaxAmount.IsEmpty)
			{
				return result;
			}

			if (taxRate != null && enforceRecalculation)
			{
				result = calcululateTax(osSourceAmount);
				result = withoutRounding || result.IsEmpty ? result : (ZDecimal)Utilities.Round(result, currency.Decimals);
			}
			else if (!localExtraTaxAmount.IsEmpty)
			{
				result = withoutRounding ?
							GetCurrentCompany(factory, companyPK).ExchangeRate.LocalToForeignWithoutRounding(localExtraTaxAmount, exchangeRate, currency.RX_Code) :
							GetCurrentCompany(factory, companyPK).ExchangeRate.LocalToForeign(localExtraTaxAmount, exchangeRate, currency.RX_Code);
			}

			return result;
		}

		static ZDecimal GetExtraTaxAmountFromTaxAmount(ZDecimal taxAmount, ZDecimal rate, ZDecimal effectiveExtraRate)
		{
			return effectiveExtraRate + rate != 0 ? taxAmount / (effectiveExtraRate + rate) * effectiveExtraRate : 0M;
		}

		internal static ZDecimal GetExtraTaxAmountFromExTaxAmount(ZDecimal osExTaxAmount, ZDecimal effectiveExtraRate)
		{
			return osExTaxAmount / 100 * effectiveExtraRate;
		}

		internal static bool UseLocalExTaxAmountToCalculateLocalTax(ZGuid company)
		{
			return AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.GetFallBackValueAtAllLevels(company.ToGuid(), Guid.Empty, Guid.Empty);
		}

		static bool ShouldCalculateTaxFromLocalAmount(BusinessObjectFactory factory, AccTaxRate taxRate, ZGuid company, ZDecimal rate, ZDecimal effectiveExtraRate) =>
			(factory != null && !factory.HasAnyOfContexts(BusinessContext.CASS, BusinessContext.APBulkInvoicePoster, BusinessContext.OSTaxAmountModifiedFromCalculatedAmountForConsolCost))
			&& taxRate != null && effectiveExtraRate + rate != ZDecimal.Zero && (UseLocalExTaxAmountToCalculateLocalTax(company) || taxRate.IsIndiaStateTax);

		[CodeAlive("Used through interface declared in Spring.Net xml")]
		public class CrossAssemblyAccess : ITaxAmountCalculator
		{
			ZDecimal ITaxAmountCalculator.GetExtraTaxAmountFromTaxAmount(ZDecimal taxAmount, ZDecimal rate, ZDecimal effectiveExtraRate) => TaxAmountCalculator.GetExtraTaxAmountFromTaxAmount(taxAmount, rate, effectiveExtraRate);

			(ZDecimal oSTaxAmount, ZDecimal oSExTaxAmount) ITaxAmountCalculator.SplitOSTotalToTaxAndExTaxAmounts(ZDecimal localTaxAmount, Func<ZDecimal> getForeignExAmount, ZDecimal overseasTotal)
				=> TaxAmountCalculator.SplitOSTotalToTaxAndExTaxAmounts(localTaxAmount, getForeignExAmount, overseasTotal);

			ZDecimal ITaxAmountCalculator.GetExtraTaxAmountFromExTaxAmount(ZDecimal exTaxAmount, ZDecimal effectiveExtraRate) => TaxAmountCalculator.GetExtraTaxAmountFromExTaxAmount(exTaxAmount, effectiveExtraRate);
		}
	}
}
