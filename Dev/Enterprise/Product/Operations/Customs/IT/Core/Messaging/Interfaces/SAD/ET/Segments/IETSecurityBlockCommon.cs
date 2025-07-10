using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface IETSecurityBlockCommon
{
	ZString TransportChargesMethodOfPayment { get; }
	ZString CommercialReferenceNumber { get; }
	ITrader Consignor { get; }
	ITrader Consignee { get; }
}
