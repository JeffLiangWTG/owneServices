using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsSADLineSecurityBlockWrapper : IETLineSecurityBlock
{
	public NctsSADLineSecurityBlockWrapper(NctsDepartureCargoDesc goodsItem)
	{
		this.goodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
	}

	readonly NctsDepartureCargoDesc goodsItem;

	public ZString UNDangerousGoodsCode => goodsItem.UNDGs.FirstItemForBinding[0].Substance?.DG_UNNO ?? ZString.Empty;

	public ZString TransportChargesMethodOfPayment => goodsItem.BY_TransportChargesMethodOfPayment;

	public ZString CommercialReferenceNumber => goodsItem.BY_CommercialReferenceNumber;

	public ITrader Consignor => new SADTraderWrapper(goodsItem.SecurityConsignor);

	public ITrader Consignee => new SADTraderWrapper(goodsItem.SecurityConsignee);
}
