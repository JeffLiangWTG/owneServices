using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class ETHeaderTransactionData
{
	readonly ITransactionData iTransactionData;

	public ETHeaderTransactionData(ITransactionData iTransactionData)
	{
		this.iTransactionData = Argument.NotNull(iTransactionData, "iTransactionData");
	}

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 3, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("O")]
	public ZString CurrencyCode => iTransactionData.CurrencyCode;

	[MessageLayout(Order = 1)]
	[MessageFieldDecimalRepresentation(17, 2, false, true)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("O")]
	public ZDecimal? TotalAmountInvoiced => iTransactionData.TotalAmountInvoiced;

	[MessageLayout(Order = 2)]
	[MessageFieldDecimalRepresentation(11, 5, false)]
	[MessageFieldExportRules("D", "C556", "CN1")]
	[MessageFieldExportWithTransitRules("D", "C556", "CN1")]
	[MessageFieldTransitRules("D", "C556", "CN1")]
	public ZDecimal? ExchangeRate => iTransactionData.ExchangeRate;

	[MessageLayout(Order = 3)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 2, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("O")]
	public ZString NatureOfTransactionCode => iTransactionData.NatureOfTransactionCode;
}
