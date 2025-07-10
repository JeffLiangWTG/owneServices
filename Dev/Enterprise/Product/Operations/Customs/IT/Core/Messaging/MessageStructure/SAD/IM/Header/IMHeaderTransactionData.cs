using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class IMHeaderTransactionData
{
	public IMHeaderTransactionData(ITransactionData transactionData)
	{
		this.transactionData = Argument.NotNull(transactionData, nameof(transactionData));
	}

	readonly ITransactionData transactionData;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 3, false)]
	[MessageFieldImportRules("R")]
	public ZString CurrencyCode => transactionData.CurrencyCode;

	[MessageLayout(Order = 1)]
	[MessageFieldDecimalRepresentation(17, 2, false, true)]
	[MessageFieldImportRules("R", "TRN0001")]
	public ZDecimal? TotalAmountInvoiced => transactionData.TotalAmountInvoiced;

	[MessageLayout(Order = 2)]
	[MessageFieldDecimalRepresentation(11, 5, false)]
	[MessageFieldImportRules("D", "CN1")]
	public ZDecimal? ExchangeRate => transactionData.ExchangeRate;

	[MessageLayout(Order = 3)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 2, false)]
	[MessageFieldImportRules("R")]
	public ZString NatureOfTransactionCode => transactionData.NatureOfTransactionCode;
}
