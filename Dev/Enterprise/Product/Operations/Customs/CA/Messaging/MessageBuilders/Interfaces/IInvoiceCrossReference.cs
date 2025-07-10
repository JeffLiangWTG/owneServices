using CargoWise.Types;

namespace Enterprise.Customs.CA.Messaging
{
	public interface IInvoiceCrossReference
	{
		ZInt InvoiceLineNumber { get; }
		ZInt InvoicePageNumber { get; }
		ZDecimal InvoiceValue { get; }
	}
}
