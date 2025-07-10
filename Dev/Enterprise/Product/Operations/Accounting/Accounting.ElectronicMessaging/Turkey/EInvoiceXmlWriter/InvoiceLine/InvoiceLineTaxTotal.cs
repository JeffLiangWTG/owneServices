using System;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.efatura.uyumsoft.com.tr;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey
{
	public class InvoiceLineTaxTotal
	{
		public InvoiceLineTaxTotal(PostingJournal line, EInvoiceHelper helper)
		{
			Line = line;
			Helper = helper;
			if (Line.OSGSTVATAmount != null)
			{
				LineTaxAmount = Helper.ConvertToPositiveValue(Line.OSGSTVATAmount.Value);
			}
			if (Line.OSExtraVATAmount != null)
			{
				TevkifatValue = Helper.ConvertToPositiveValue(Line.OSExtraVATAmount.Value);
				LineTaxAmount = LineTaxAmount + TevkifatValue;
			}
			if (Helper.HasWithholdingTax(Line))
			{
				TevkifatRate = ZDecimal.Parse(Convert.ToInt32(Line.VATTaxID.ExtraTaxRate.Value * Hundred / Line.VATTaxID.TaxRate.Value).ToString());
			}
			Currency = Line.OSCurrency.Code.Value;
		}

		PostingJournal Line { get; }
		ZDecimal TevkifatValue { get; }
		ZDecimal LineTaxAmount { get; }
		ZDecimal TevkifatRate { get; }
		ZString Currency { get; }
		EInvoiceHelper Helper { get; }
		const decimal Hundred = 100m;

		internal TaxTotalType BuildInvoiceLineTaxTotal()
		{
			var taxExemptionReasonCode = LineTaxAmount == 0
				? Line.GetTaxMessageCodeWithFallback(TurkeyComplianceInfo.EInvoiceTaxCategoryConstants.DefaultExemptionReasonCode)
				: string.Empty;
			var taxExemptionReasonDescription = LineTaxAmount == 0
				? Line.GetTaxMessageDescriptionWithFallback()
				: string.Empty;

			return new TaxTotalType()
			{
				TaxAmount = new TaxAmountType()
				{
					Value = Helper.FixDecimalPlacesAndSign(LineTaxAmount),
					currencyID = Currency
				},
				TaxSubtotal = new TaxSubtotalType[]
				{
					Helper.CreateTaxRateSubtotal(
						Line.OSAmount.Value,
						LineTaxAmount,
						Line.VATTaxID.TaxRate.Value,
						"KDV",
						"0015",
						taxExemptionReasonCode,
						taxExemptionReasonDescription)
				}
			};
		}

		internal TaxTotalType[] BuildInvoiceLineWithholdingTaxTotal()
		{
			var withholdingTaxTotal = new TaxTotalType()
			{
				TaxAmount = new TaxAmountType()
				{
					Value = Helper.FixDecimalPlacesAndSign(TevkifatValue),
					currencyID = Currency
				},
				TaxSubtotal = new TaxSubtotalType[]
				{
					Helper.CreateTaxRateSubtotal(
						LineTaxAmount,
						TevkifatValue,
						TevkifatRate,
						"TEVKIFAT",
						Line.GetTaxMessageCodeWithFallback())
				}
			};

			return new TaxTotalType[] { withholdingTaxTotal };
		}
	}
}
