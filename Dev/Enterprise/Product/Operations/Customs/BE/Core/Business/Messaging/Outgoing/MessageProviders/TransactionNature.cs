using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public class TransactionNature : ITTransactionNature
{
	public TransactionNature(JobComInvoiceHeader invoice)
	{
		this.invoice = invoice;
	}

	readonly JobComInvoiceHeader invoice;

	public ZString TransactionNature1 { get => invoice.JZ_ValuationCode.SubstringSafe(0, 1); }
	public ZString TransactionNature2 { get => invoice.JZ_ValuationCode.SubstringSafe(1, 1); }
}
