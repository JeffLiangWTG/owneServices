using CargoWise.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public class EntryLineCustomsValuationCalculator
{
	public EntryLineCustomsValuationCalculator(CusEntryLine entryLine)
	{
		this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
	}

	readonly CusEntryLine entryLine;

	public EntryLineCustomsValuation EvaluateAmount()
	{
		var currencyConverter = entryLine.CurrencyConverter;

		var freightAdjustments = Money.Empty;
		var lineValue = Money.Empty;

		foreach (JobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
		{
			var invoiceLineFreightAdjustments = new InvoiceLineFreightAdjustmentsCalculator(invoiceLine).EvaluateAmount();
			freightAdjustments = currencyConverter.Add(freightAdjustments, invoiceLineFreightAdjustments);
			lineValue = currencyConverter.Add(lineValue, invoiceLine.JI_LinePriceMoney);
		}
		return new EntryLineCustomsValuation(freightAdjustments, lineValue);
	}
}
