using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface ISpecialMentionUnloadingDataInfo
{
	ZString CommodityCode { get; }
	ZDecimal? Quantity { get; }
	ZDecimal? SupplementaryUnit { get; }
}
