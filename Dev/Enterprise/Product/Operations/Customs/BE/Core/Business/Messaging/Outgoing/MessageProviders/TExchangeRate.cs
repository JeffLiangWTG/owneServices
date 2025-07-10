using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public class TExchangeRate : ITExchangeRate
{
	public TExchangeRate(CusEntryLine entryLine)
	{
		Argument.NotNull(entryLine, CusEntryLine.Schema.TableName);
		this.entryLine = entryLine;
	}

	readonly CusEntryLine entryLine;

	public ZString Currency { get => entryLine.TotalLinePrice.Currency.Code; }
	public ZBool ExchangeRateSpecified { get => ExchangeRate.IsValid && !ExchangeRate.IsEmpty; }
	public ZDecimal ExchangeRate { get => entryLine.RandomLine.InvoiceHeader.JZ_InvoiceCurrExRate; }
}
