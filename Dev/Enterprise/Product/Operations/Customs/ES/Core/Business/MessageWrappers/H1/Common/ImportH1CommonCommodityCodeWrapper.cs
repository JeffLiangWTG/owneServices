using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class ImportH1CommonCommodityCodeWrapper : CommodityCodeCommonWrapper, ICommonH1CommodityCode
{
	public ImportH1CommonCommodityCodeWrapper(JobComInvoiceLine invoiceLine) : base(invoiceLine)
	{
	}

	const int taricCodeStartIndex = 8;
	const int taricCodeLength = 2;

	public ZString TaricCode => invoiceLine.JI_Tariff.SubstringSafe(taricCodeStartIndex, taricCodeLength);
}
