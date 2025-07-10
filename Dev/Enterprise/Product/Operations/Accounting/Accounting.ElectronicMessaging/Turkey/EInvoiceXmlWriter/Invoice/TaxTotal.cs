using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.efatura.uyumsoft.com.tr;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey
{
	internal class TaxTotal
	{
		const string DefaultTaxSchemeName = TurkeyComplianceInfo.EInvoiceTaxCategoryConstants.DefaultTaxSchemeName;
		const string DefaultTaxSchemeTaxTypeCode = TurkeyComplianceInfo.EInvoiceTaxCategoryConstants.DefaultTaxSchemeTaxTypeCode;

		public TaxTotal(EInvoiceHelper helper)
		{
			Helper = helper;
			Currency = Helper.UInvoice.OSCurrency.Code.Value;
			ZeroTaxPostingJournals = Helper.NonCommentInvoiceLines.Where(x => x.VATTaxID != null && x.VATTaxID?.TaxRate == 0m).ToList();
			TaxedPostingJournals = Helper.NonCommentInvoiceLines.Where(x => x.VATTaxID != null && x.VATTaxID?.TaxRate != 0m).ToList();
			WithholdingPostingJournals = Helper.NonCommentInvoiceLines.Where(x => Helper.HasWithholdingTax(x)).ToList();
		}

		EInvoiceHelper Helper { get; }
		ZString Currency { get; }
		List<PostingJournal> TaxedPostingJournals { get; }
		List<PostingJournal> ZeroTaxPostingJournals { get; }
		List<PostingJournal> WithholdingPostingJournals { get; }
		internal ZBool HasWithholdingTax => WithholdingPostingJournals.Any();
		internal ZBool HasTaxExemption => ZeroTaxPostingJournals.Any();

		internal TaxTotalType[] BuildTaxTotals()
		{
			var taxTotal = new TaxTotalType() { TaxAmount = new TaxAmountType() { Value = Helper.FixDecimalPlacesAndSign(0), currencyID = Currency } };
			var taxSubtotal = new List<TaxSubtotalType>();

			taxSubtotal.AddRange(BuildTaxExemptionSubTotals());
			taxSubtotal.AddRange(BuildTaxSubTotals(taxTotal));

			taxTotal.TaxSubtotal = taxSubtotal.ToArray();

			return new TaxTotalType[] { taxTotal };
		}

		public IEnumerable<TaxSubtotalType> BuildTaxExemptionSubTotals()
		{
			var result = new List<TaxSubtotalType>();
			var taxTotalsRespectToExemptions = ZeroTaxPostingJournals
				.GroupBy(x => new { TaxMessageCode = x.GetTaxMessageCodeWithFallback(TurkeyComplianceInfo.EInvoiceTaxCategoryConstants.DefaultExemptionReasonCode) })
				.Select(n => new
				{
					n.Key.TaxMessageCode,
					TaxMessageDescription = n.Select(a => a.GetTaxMessageDescriptionWithFallback()).FirstOrDefault(),
					OSAmountSum = n.Sum(a => a.OSAmount.Value)
				});

			foreach (var line in taxTotalsRespectToExemptions)
			{
				var subTotal = Helper.CreateTaxRateSubtotal(line.OSAmountSum, 0m, 0m, DefaultTaxSchemeName, DefaultTaxSchemeTaxTypeCode, line.TaxMessageCode, line.TaxMessageDescription);
				result.Add(subTotal);
			}

			return result;
		}

		IEnumerable<TaxSubtotalType> BuildTaxSubTotals(TaxTotalType taxTotal)
		{
			var result = new List<TaxSubtotalType>();
			var taxTotalsByTaxRate = TaxedPostingJournals
				.GroupBy(x => new
				{
					x.VATTaxID?.TaxRate,
				})
				.Select(x => new
				{
					x.Key.TaxRate,
					TaxableAmount = x.Sum(acs => acs.OSAmount),
					TaxAmount = x.Sum(acs => acs.OSGSTVATAmount + Helper.ConvertToPositiveValue(acs.OSExtraVATAmount))
				})
				.OrderBy(x => x.TaxRate).ToList();

			if (taxTotalsByTaxRate.Any())
			{
				taxTotal.TaxAmount = new TaxAmountType() { Value = Helper.FixDecimalPlacesAndSign(taxTotalsByTaxRate.Sum(x => x.TaxAmount).Value), currencyID = Currency };
			}

			taxTotalsByTaxRate.ForEach(x => result.Add(Helper.CreateTaxRateSubtotal(x.TaxableAmount.Value, x.TaxAmount.Value, x.TaxRate.Value, DefaultTaxSchemeName, DefaultTaxSchemeTaxTypeCode)));

			return result;
		}

		internal TaxTotalType[] BuildWithholdingTaxTotals()
		{
			var withholdingTaxTotal = new TaxTotalType();
			withholdingTaxTotal.TaxSubtotal = BuildWithholdingTaxSubTotals(withholdingTaxTotal).ToArray();

			return new TaxTotalType[] { withholdingTaxTotal };
		}

		IEnumerable<TaxSubtotalType> BuildWithholdingTaxSubTotals(TaxTotalType withholdingTaxTotal)
		{
			var result = new List<TaxSubtotalType>();
			var withholdingTaxTotals = WithholdingPostingJournals
				.GroupBy(pc => new
				{
					pc.VATTaxID?.TaxRate,
					pc.VATTaxID?.ExtraTaxRate,
					TaxMessageCode = pc.GetTaxMessageCodeWithFallback(),
					Description = pc.GetTaxMessageDescriptionWithFallback(),
				})
				.Select(ac => new
				{
					ac.Key.TaxRate,
					ac.Key.ExtraTaxRate,
					ac.Key.TaxMessageCode,
					TaxableAmount = ac.Sum(acs => acs.OSGSTVATAmount + Helper.ConvertToPositiveValue(acs.OSExtraVATAmount)),
					TaxAmount = ac.Sum(acs => Helper.ConvertToPositiveValue(acs.OSExtraVATAmount))
				})
				.OrderBy(x => x.TaxRate).ToList();

			withholdingTaxTotal.TaxAmount = new TaxAmountType() { Value = Helper.FixDecimalPlacesAndSign(withholdingTaxTotals.Sum(x => x.TaxAmount)), currencyID = Currency }; // gut_fatura.tevkifat_tutari dövizli

			withholdingTaxTotals.ForEach(x => result.Add(Helper.CreateTaxRateSubtotal(x.TaxableAmount.Value, x.TaxAmount, decimal.Parse(Convert.ToInt32(x.ExtraTaxRate.Value * Hundred / x.TaxRate.Value).ToString()), TEVKIFAT, x.TaxMessageCode)));

			return result;
		}

		const string TEVKIFAT = "TEVKIFAT";
		const decimal Hundred = 100m;
	}
}
