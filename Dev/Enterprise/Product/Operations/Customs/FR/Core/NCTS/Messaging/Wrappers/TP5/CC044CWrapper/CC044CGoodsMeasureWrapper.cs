using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5;

public class CC044CGoodsMeasureWrapper : GoodsMeasureWrapper
{
	CC044CGoodsMeasureWrapper(NctsCommonCargoDesc item) : base(item)
	{
	}

	public new static CC044CGoodsMeasureWrapper New(NctsCommonCargoDesc item) => item == null ? null : new CC044CGoodsMeasureWrapper(item);

	public override decimal? GrossMass => grossMass ?? (grossMass = StatusIsNewOrDif || IsUnloadedGoodsItem ?  item.GrossMassInKilograms : decimal.Zero);
	decimal? grossMass;

	public override decimal? NetMass => netMass ?? (netMass = StatusIsNewOrDif || IsUnloadedGoodsItem ? item.NetMassInKilograms : decimal.Zero);
	decimal? netMass;

	bool StatusIsNewOrDif => item.BY_UnloadedState.In(new ZString[] { NctsUnloadedStateList.Codes.NEW, NctsUnloadedStateList.Codes.DIF });
	bool IsUnloadedGoodsItem => item is NctsUnloadedCargoDesc;
}
