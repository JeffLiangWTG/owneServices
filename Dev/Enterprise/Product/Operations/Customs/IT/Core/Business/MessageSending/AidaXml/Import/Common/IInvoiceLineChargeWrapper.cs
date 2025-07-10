using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

interface IInvoiceLineChargeWrapper
{
	ZString ChargeCode { get; }
	ZDecimal Amount { get; }
	Money MoneyInLocalCurrency { get; }
	RefCurrency Currency { get; }
	ZBool IsDutiable { get; }
	ZBool IsIncludedInLine { get; }
	CurrencyConverter CurrencyConverter { get; }
	ZGuid LineId { get; }
}
