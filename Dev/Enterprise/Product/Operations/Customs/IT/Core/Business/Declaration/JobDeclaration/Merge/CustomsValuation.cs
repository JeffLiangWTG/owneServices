using CargoWise.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public class CustomsValuation
{
	public CustomsValuation(CusEntryHeader entryHeader)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		currencyConverter = Argument.NotNull(entryHeader.CurrencyConverter, nameof(entryHeader.CurrencyConverter));
		localCurrency = Argument.NotNull(entryHeader.LocalCurrency, nameof(entryHeader.LocalCurrency));
	}

	readonly CusEntryHeader entryHeader;
	readonly CurrencyConverter currencyConverter;
	readonly RefCurrency localCurrency;

	public void Calculate()
	{
		var freightAdjustments = Money.Empty;
		var invoiceAmount = Money.Empty;

		foreach (CusEntryLine entryLine in entryHeader.MergedLines)
		{
			var entryLineCustomsValuation = new EntryLineCustomsValuationCalculator(entryLine).EvaluateAmount();

			CalculateEntryLineValue(entryLine, entryLineCustomsValuation);
			CalculateEntryLineAdjustmentsAmount(entryLine, entryLineCustomsValuation);

			freightAdjustments = currencyConverter.Add(freightAdjustments, entryLineCustomsValuation.FreightAdjustments);
			invoiceAmount = currencyConverter.Add(invoiceAmount, entryLineCustomsValuation.LineValue);
		}

		entryHeader.CH_FreightAdjustment = currencyConverter.ConvertExact(freightAdjustments, localCurrency).Amount;
		entryHeader.InvoiceAmount = invoiceAmount.Amount;
	}

	void CalculateEntryLineAdjustmentsAmount(CusEntryLine entryLine, EntryLineCustomsValuation entryLineCustomsValuation)
	{
		var entryLineAdjustments = new Money(entryLine.CL_StatisticalValue, localCurrency);
		entryLineAdjustments = currencyConverter.Subtract(entryLineAdjustments, entryLineCustomsValuation.FreightAdjustments);
		entryLineAdjustments = currencyConverter.Subtract(entryLineAdjustments, entryLineCustomsValuation.LineValue);
		entryLine.ZG_AdjustmentAmount = currencyConverter.ConvertExact(entryLineAdjustments, localCurrency).Amount;
	}

	void CalculateEntryLineValue(CusEntryLine entryLine, EntryLineCustomsValuation entryLineCustomsValuation)
	{
		entryLine.ZG_LinesValue = currencyConverter.ConvertExact(entryLineCustomsValuation.LineValue, localCurrency).Amount;
	}
}
