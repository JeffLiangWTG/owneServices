using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class SADTransactionDataWrapper : ITransactionData
{
	public SADTransactionDataWrapper(CusEntryHeader entryHeader)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		invoiceHeader = Argument.NotNull(entryHeader.RandomHeader, nameof(entryHeader.RandomHeader));
	}
	readonly CusEntryHeader entryHeader;
	readonly JobComInvoiceHeader invoiceHeader;

	public ZString CurrencyCode => entryHeader.InvoiceAmountCurrency;

	public ZDecimal? TotalAmountInvoiced => entryHeader.InvoiceAmount;

	public ZDecimal? ExchangeRate => CurrenciesHelper.IsCurrencyRecognizedByCustoms(CurrencyCode) ? null : invoiceHeader.JZ_InvoiceCurrExRate;

	public ZString NatureOfTransactionCode => invoiceHeader.JZ_ValuationCode;
}
