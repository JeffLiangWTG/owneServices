using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class IncompleteImportH1CommodityWrapper : ImportH1CommonCommodityWrapper, IIncompleteImportH1Commodity
{
	public IncompleteImportH1CommodityWrapper(JobComInvoiceLine invoiceLine) : base(invoiceLine)
	{
	}

	public ICommonH1CommodityCode CommodityCode => commodityCode ??= commodityCode = new ImportH1CommonCommodityCodeWrapper(invoiceLine);
	ImportH1CommonCommodityCodeWrapper commodityCode;
}
