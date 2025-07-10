using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsSADLineEmptySecurityBlockWrapper : IETLineSecurityBlock
{
	public ZString UNDangerousGoodsCode => ZString.Empty;

	public ZString TransportChargesMethodOfPayment => ZString.Empty;

	public ZString CommercialReferenceNumber => ZString.Empty;

	public ITrader Consignor => new SADEmptyTraderWrapper();

	public ITrader Consignee => new SADEmptyTraderWrapper();
}
