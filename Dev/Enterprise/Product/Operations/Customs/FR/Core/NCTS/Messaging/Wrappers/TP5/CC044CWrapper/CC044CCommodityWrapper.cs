using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5;

public class CC044CCommodityWrapper : CommodityWrapper
{
	CC044CCommodityWrapper(NctsCommonCargoDesc item) : base(item)
	{
	}

	public new static CC044CCommodityWrapper New(NctsCommonCargoDesc item) => item == null ? null : new CC044CCommodityWrapper(item);

	bool StatusIsNewOrDif => item.BY_UnloadedState.In(new ZString[] { NctsUnloadedStateList.Codes.NEW, NctsUnloadedStateList.Codes.DIF });
	bool IsUnloadedGoodsItem => item is NctsUnloadedCargoDesc;

	public override string DescriptionOfGoods => descriptionOfGoods ?? (descriptionOfGoods = StatusIsNewOrDif || IsUnloadedGoodsItem ? item.BY_Description : null);
	string descriptionOfGoods;

	public override ICommodityCode CommodityCode => commodityCode ?? (commodityCode = StatusIsNewOrDif || IsUnloadedGoodsItem ? CommodityCodeWrapper.New(item.BY_HarmonisedTariff) : null);
	ICommodityCode commodityCode;

	public override IGoodsMeasure GoodsMeasure => goodsMeasure ??= CC044CGoodsMeasureWrapper.New(item);
	IGoodsMeasure goodsMeasure;
}
