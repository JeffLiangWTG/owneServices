using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class SupernumeraryGoodsLookups : CusSupportingInfoLookups
{
	public SupernumeraryGoodsLookups(SupernumeraryGoods parent) : base(parent)
	{
	}

	public TariffViewCollection TariffList => TariffViewCollection.GetCachedCollection(Factory, SupernumeraryGoods.TariffDataGrouping, SupernumeraryGoods.TariffType, ValuationDate);

	public override CodeDescriptionPairList PackTypeList => NctsPackageLookups.GetPackageUnitTypeList(Factory);

	new SupernumeraryGoods Parent => (SupernumeraryGoods)base.Parent;

	ZDateTime ValuationDate => Parent.Parent?.ValuationDate ?? ZDateTime.Now;
}
