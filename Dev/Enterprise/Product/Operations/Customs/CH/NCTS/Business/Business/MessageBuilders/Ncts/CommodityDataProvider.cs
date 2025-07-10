using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class CommodityDataProvider : ICommodity, IGoodsMeasure
{
	public static CommodityDataProvider New(NctsCommonCargoDesc goodsItem) => goodsItem == null ? null : new CommodityDataProvider(goodsItem);

	protected CommodityDataProvider(NctsCommonCargoDesc goodsItem)
	{
		this.goodsItem = goodsItem;
	}
	protected readonly NctsCommonCargoDesc goodsItem;

	NctsDepartureMovementHeader DepartureMovementHeader => (NctsDepartureMovementHeader)(goodsItem.Header?.MovementHeader);

	public string DescriptionOfGoods => CachedValueHelper.GetValue(ref descriptionOfGoods, () => GetCargoDescValue(g => g.BY_Description));
	CachedValue<string> descriptionOfGoods;

	public string CUSCode => CachedValueHelper.GetValue(ref cusCode, () => GetCargoDescValue(g => g.BY_CusC4Number));
	CachedValue<string> cusCode;

	public IReadOnlyCollection<IDangerousGoods> DangerousGoods => dangerousGoods ??= GetDangerousGoodsDataProviders()?.ToArray();
	IReadOnlyCollection<IDangerousGoods> dangerousGoods;

	IEnumerable<IDangerousGoods> GetDangerousGoodsDataProviders() => goodsItem is NctsDepartureCargoDesc departureGoodsItem ? DangerousGoodsDataProvider.NewCollection(departureGoodsItem.UNDGs) : null;

	public decimal? GrossMass => CachedValueHelper.GetValue(ref grossMass, () => GetCargoDescValue(g => g.GrossMassInKilograms));
	CachedValue<decimal?> grossMass;

	public decimal? NetMass => CachedValueHelper.GetValue(ref netMass, () => GetCargoDescValue(g => g.NetMassInKilograms)?.ReturnNullIfEmpty());
	CachedValue<decimal?> netMass;

	public decimal? SupplementaryUnits => goodsItem.BY_CustomsSecondQuantity.ReturnNullIfEmpty();

	public IGoodsMeasure GoodsMeasure => GoodsMeasureCore;

	protected virtual IGoodsMeasure GoodsMeasureCore => this;

	public ICommodityCode CommodityCode => CachedValueHelper.GetValue(ref commodityCode, () => CommodityCodeCore);
	CachedValue<ICommodityCode> commodityCode;

	protected virtual ICommodityCode CommodityCodeCore => GetCargoDescValue(g => g.BY_HarmonisedTariff) is var harmonisedTariff && harmonisedTariff.HasValue ? CommodityCodeDataProvider.New(harmonisedTariff.Value) : null;

	public  ICommoditySpecification CommoditySpecification => commoditySpecification ??= DepartureMovementHeader.IsNationalTransitSwitzerland ? NationalTransitCommoditySpecificationDataProvider.New(goodsItem as NctsDepartureCargoDesc) : null;
	ICommoditySpecification commoditySpecification;

	protected virtual T? GetCargoDescValue<T>(Func<NctsCommonCargoDesc, T> getter) where T : struct, IZType => getter(goodsItem);
}
