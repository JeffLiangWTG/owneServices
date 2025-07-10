using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface ITransactionData
{
	ZString CurrencyCode { get; }
	ZDecimal? TotalAmountInvoiced { get; }
	ZDecimal? ExchangeRate { get; }
	ZString NatureOfTransactionCode { get; }
}
