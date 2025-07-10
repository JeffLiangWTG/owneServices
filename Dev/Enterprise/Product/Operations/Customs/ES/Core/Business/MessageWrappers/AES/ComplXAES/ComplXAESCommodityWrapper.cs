using CargoWise.Common;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class ComplXAESCommodityWrapper : IComplXAESCommodity
{
	public ComplXAESCommodityWrapper(CusEntryLine entryLine)
	{
		this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
		Argument.GreaterThan(entryLine.InvoiceLines.Count, 0, nameof(entryLine.InvoiceLines));
	}
	readonly CusEntryLine entryLine;

	public ICommonGoodsMeasureWithSpecified GoodsMeasure => goodsMeasure ?? (goodsMeasure = new CommonGoodsMeasureWithSpecifiedWrapper(entryLine));
	CommonGoodsMeasureWithSpecifiedWrapper goodsMeasure;
}
