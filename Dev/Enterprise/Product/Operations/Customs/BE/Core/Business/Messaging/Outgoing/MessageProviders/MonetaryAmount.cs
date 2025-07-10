using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public class MonetaryAmount : IMonetaryAmount
{
	public MonetaryAmount(CusEntryLine entryLine)
	{
		Argument.NotNull(entryLine, CusEntryLine.Schema.TableName);
		this.entryLine = entryLine;
		ExchangeRate = new TExchangeRate(entryLine);
	}

	readonly CusEntryLine entryLine;

	public ITExchangeRate ExchangeRate { get; }
	public ZDecimal Amount { get => entryLine.TotalLinePrice.Amount; }
}
