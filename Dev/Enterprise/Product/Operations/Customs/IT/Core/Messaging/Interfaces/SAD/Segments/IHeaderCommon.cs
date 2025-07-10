using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface IHeaderCommon
{
	ZString AnnualProgressiveNumber { get; }
	ZString AuthorizationNo { get; }
	ZString AuthorizationCIN { get; }
	ZInt TotalItems { get; }
	ZDate AcceptanceDate { get; }
	IDeclaration Declaration { get; }
	ITrader Consignor { get; }
	ITrader Consignee { get; }
	IDeclarantTrader DeclarantTrader { get; }
	ZString CountryOfDispatch { get; }
	ITermOfDeliveryGroup TermsOfDelivery { get; }
	ITransactionData TransactionData { get; }
	IDeferredPayment DeferredPayment { get; }
	IWarehouseIdentification WarehouseIdentification { get; }
	ZString CountryOfDestination { get; }
	ZBool? IsContainerizedTransport { get; }
	ZString TransportModeAtBorder { get; }
	ZString InlandTransportMode { get; }
	ZDate DateLimitOfTemporaryOperation { get; }
}
