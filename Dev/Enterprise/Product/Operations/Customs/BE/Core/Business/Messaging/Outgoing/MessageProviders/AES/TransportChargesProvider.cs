using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public class TransportChargesProvider : ITransportCharges
{
	public TransportChargesProvider(JobComInvoiceHeader invoiceHeader)
	{
		this.invoiceHeader = Argument.NotNull(invoiceHeader, nameof(invoiceHeader));
	}

	readonly JobComInvoiceHeader invoiceHeader;

	public string MethodOfPayment => invoiceHeader.ZG_TransportChargesMethodOfPayment;
}
