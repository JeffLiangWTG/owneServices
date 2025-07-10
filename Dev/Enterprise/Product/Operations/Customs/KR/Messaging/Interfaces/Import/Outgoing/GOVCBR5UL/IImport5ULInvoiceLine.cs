using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport5ULInvoiceLine
	{
		ZInt InvoiceLineNo { get; }
		ZString HSDescription { get; }
		ZString ItemDescription { get; }
		ZDecimal RefundQuantity { get; }
		ZDecimal InvoiceQuantity { get; }
		ZDecimal UnitPrice { get; }
	}
}
