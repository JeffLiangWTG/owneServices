using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.NCTS.Business;

public class TIRHeaderEmptyTransactionDataWrapper : ITransactionData
{
	public ZString CurrencyCode => ZString.Empty;

	public ZDecimal? TotalAmountInvoiced => null;

	public ZDecimal? ExchangeRate => null;

	public ZString NatureOfTransactionCode => ZString.Empty;
}
