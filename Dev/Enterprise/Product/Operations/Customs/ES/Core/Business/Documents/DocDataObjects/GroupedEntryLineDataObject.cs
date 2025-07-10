using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Documents.DocDataObjects
{
	public class GroupedEntryLineDataObject : EntryLineDataObject
	{
		public GroupedEntryLineDataObject(CusEntryLine entryLine, IEnumerable<CusEntryLine> entryLines) : base(entryLine)
		{
			EntryLine = entryLine;
			EntryLines = entryLines;
		}

		public new CusEntryLine EntryLine { get; set; }
		public IEnumerable<CusEntryLine> EntryLines { get; set; }

		protected override ZDecimal ExchangeRateCore
		{
			get
			{
				var line = EntryLines.FirstOrDefault();
				return line.RandomLine.InvoiceHeader != null
					? line.RandomLine.InvoiceHeader.JZ_InvoiceCurrExRate
					: 0.0;
			}
		}

		protected override ZDecimal TotalACore => decimal.Round((EntryLines.Select(x => x.Price.Amount).Sum(x => x) + IndirectAndOtherPaymentsCharges), 2, MidpointRounding.AwayFromZero);
		protected override ZDecimal PriceCore => decimal.Round(EntryLines.Select(x => x.Price.Amount).Sum(x => x), 2, MidpointRounding.AwayFromZero);
		protected override ZDecimal OtherNotElsewhereDeclaredChargesCore
		{
			get
			{
				var otherChargesValue = ZDecimal.Zero;
				foreach (CusEntryLine line in EntryLines)
				{
					otherChargesValue += -line.InvoiceLines.Sum(x => ((JobComInvoiceLine)x).JI_NegAdj);
				}

				otherChargesValue -= (CostOfTransportOTEU + ConstructionErectionAssemblyCharges + ImportDutiesOrOtherCharges);

				return otherChargesValue;
			}
		}

		protected override ZDecimal GetEGVChargeAmountForCalculation()
		{
			var egvAmount = ZDecimal.Zero;
			foreach (CusEntryLine line in EntryLines)
			{
				egvAmount += line.InvoiceLines.Sum(x => ((JobComInvoiceLine)x).ValuationCalculator.GetEGVChargeAmount());
			}
			return egvAmount;
		}

		protected override ZDecimal GetESCustomsValue()
		{
			var customsValue = ZDecimal.Zero;
			foreach (CusEntryLine line in EntryLines)
			{
				customsValue += line.InvoiceLines.Sum(x => ((JobComInvoiceLine)x).JI_Calc_ESCustomsValue);
			}
			return customsValue;
		}

		protected override decimal GetAdditionChargesCore(ZString[] chargeCodes)
		{
			var chargesAmount = ZDecimal.Zero;
			foreach (CusEntryLine line in EntryLines)
			{
				var lineChargesInCorrectCurrency = line.InvoiceLines
					.SelectMany(x => ((JobComInvoiceLine)x).Charges.Cast<InvoiceLineCharge>())
					.Where(x => chargeCodes.Contains(x.J7_ChargeType) && (x.J7_IsDutiable && !x.J7_IsIncludedInITOT))
					.Select(x => GetAmountInCorrectCurrency(x.J7_Amount, x.Currency, line));

				var apportionedLineChargesInCorrectCurrency = line.InvoiceLines
					.SelectMany(x => ((JobComInvoiceLine)x).ApportionedCharges.Cast<InvoiceLineApportionCharge>())
					.Where(x => chargeCodes.Contains(x.J7_ChargeType) && (x.J7_IsDutiable && !x.J7_IsIncludedInITOT))
					.Select(x => GetAmountInCorrectCurrency(x.J7_Amount, x.Currency, line));

				var lineTotalChargeAmount = lineChargesInCorrectCurrency.Concat(apportionedLineChargesInCorrectCurrency).Sum(x => x);

				chargesAmount += decimal.Round(lineTotalChargeAmount, 2, MidpointRounding.AwayFromZero);
			}
			return chargesAmount;
		}

		protected override decimal GetDeductionChargesCore(ZString[] chargeCodes)
		{
			var chargesAmount = ZDecimal.Zero;
			foreach (CusEntryLine line in EntryLines)
			{
				var lineChargesInCorrectCurrency = line.InvoiceLines
					.SelectMany(x => ((JobComInvoiceLine)x).Charges.Cast<InvoiceLineCharge>())
					.Where(x => chargeCodes.Contains(x.J7_ChargeType) && (!x.J7_IsDutiable && x.J7_IsIncludedInITOT))
					.Select(x => GetAmountInCorrectCurrency(x.J7_Amount, x.Currency, line));

				var apportionedLineChargesInCorrectCurrency = line.InvoiceLines
					.SelectMany(x => ((JobComInvoiceLine)x).ApportionedCharges.Cast<InvoiceLineApportionCharge>())
					.Where(x => chargeCodes.Contains(x.J7_ChargeType) && (!x.J7_IsDutiable && x.J7_IsIncludedInITOT))
					.Select(x => GetAmountInCorrectCurrency(x.J7_Amount, x.Currency, line));

				var lineTotalChargeAmount = lineChargesInCorrectCurrency.Concat(apportionedLineChargesInCorrectCurrency).Sum(x => x);

				chargesAmount += decimal.Round(lineTotalChargeAmount, 2, MidpointRounding.AwayFromZero);
			}
			return chargesAmount;
		}

		ZDecimal GetAmountInCorrectCurrency(ZDecimal chargeAmount, RefCurrency currency, CusEntryLine line)
		{
			return currency != null ? line.RandomLine.InvoiceHeader.CurrencyConverter.ConvertExact(new Money(chargeAmount, currency), line.Declaration.LocalCurrency).Amount : chargeAmount;
		}

		protected override ZString EntryHeaderReferenceCore => EntryLines.FirstOrDefault().Header.CH_BGMReference;
	}
}
