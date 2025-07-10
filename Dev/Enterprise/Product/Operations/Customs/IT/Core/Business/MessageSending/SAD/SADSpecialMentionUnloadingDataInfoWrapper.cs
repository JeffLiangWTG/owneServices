using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class SADSpecialMentionUnloadingDataInfoWrapper : ISpecialMentionUnloadingDataInfo
{
	public SADSpecialMentionUnloadingDataInfoWrapper(ZString commodityCode, ZDecimal quantity, ZDecimal supplementaryUnit)
	{
		CommodityCode = commodityCode;
		this.quantity = quantity;
		this.supplementaryUnit = supplementaryUnit;
	}

	readonly ZDecimal quantity;
	readonly ZDecimal supplementaryUnit;

	public static SADSpecialMentionUnloadingDataInfoWrapper Empty() => new SADSpecialMentionUnloadingDataInfoWrapper(ZString.Empty, ZDecimal.Zero, ZDecimal.Zero);

	public ZString CommodityCode { get; }

	public ZDecimal? Quantity => quantity.GetValueOrNullIfZero();

	public ZDecimal? SupplementaryUnit => supplementaryUnit.GetValueOrNullIfZero();
}
