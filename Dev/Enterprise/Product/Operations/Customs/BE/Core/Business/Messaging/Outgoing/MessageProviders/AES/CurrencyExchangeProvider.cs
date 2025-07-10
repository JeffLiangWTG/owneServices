using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public class CurrencyExchangeProvider : ICurrencyExchange
{
	public CurrencyExchangeProvider(JobComInvoiceHeader invoiceHeader)
	{
		this.invoiceHeader = Argument.NotNull(invoiceHeader, nameof(JobComInvoiceHeader));
	}

	protected readonly JobComInvoiceHeader invoiceHeader;

	public string InternalCurrencyUnit => invoiceHeader.JZ_RX_NKInvoice_Currency;

	public decimal ExchangeRate => invoiceHeader.JZ_InvoiceCurrExRate;
}
