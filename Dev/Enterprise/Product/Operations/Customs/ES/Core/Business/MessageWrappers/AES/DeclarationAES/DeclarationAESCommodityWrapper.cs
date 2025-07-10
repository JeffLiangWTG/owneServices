using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class DeclarationAESCommodityWrapper : IDeclarationAESCommodity
{
	public DeclarationAESCommodityWrapper(CusEntryLine entryLine)
	{
		this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
		Argument.GreaterThan(entryLine.InvoiceLines.Count, 0, nameof(entryLine.InvoiceLines));
		randomLine = entryLine.RandomLine;
	}
	readonly CusEntryLine entryLine;
	readonly JobComInvoiceLine randomLine;

	public ZString GoodsDescription => randomLine.JI_Description.SubstringSafe(0, 512).Replace("\r\n", " ").Replace("\n", " ");

	public ZString CusCode => randomLine.ZG_CusNumber;

	public IDeclarationAESCommodityCode CommodityCode => commodityCode ?? (commodityCode = new DeclarationAESCommodityCodeWrapper(randomLine));
	DeclarationAESCommodityCodeWrapper commodityCode;

	public IReadOnlyCollection<ICommonDangerousGoods> DangerousGoods
	{
		get
		{
			if (dangerousGoods == null)
			{
				var dangerousGoodsList = new List<CommonDangerousGoodsWrapper>();

				var dangerousCode = randomLine.UNDGs.FirstItemForBinding[0].SubstanceCode;
				if (!dangerousCode.IsEmpty)
				{
					dangerousGoodsList.Add(new CommonDangerousGoodsWrapper(1, dangerousCode));
				}

				dangerousGoods = dangerousGoodsList.AsReadOnly();
			}
			return dangerousGoods;
		}
	}
	IReadOnlyCollection<CommonDangerousGoodsWrapper> dangerousGoods;

	public ICommonGoodsMeasureWithSupUnitsAndSpecified GoodsMeasure => goodsMeasure ?? (goodsMeasure = new CommonGoodsMeasureWithSupUnitsAndSpecifiedWrapper(entryLine));
	CommonGoodsMeasureWithSupUnitsAndSpecifiedWrapper goodsMeasure;
}
