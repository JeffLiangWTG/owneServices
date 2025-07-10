using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class ImportH1CommonCommodityWrapper : ICommonH1Commodity
{
	public ImportH1CommonCommodityWrapper(JobComInvoiceLine invoiceLine)
	{
		this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
	}
	protected readonly JobComInvoiceLine invoiceLine;

	public ZString GoodsDescription => invoiceLine.JI_Description.SubstringSafe(0, 512).Replace("\r\n", " ").Replace("\n", " ");
}
