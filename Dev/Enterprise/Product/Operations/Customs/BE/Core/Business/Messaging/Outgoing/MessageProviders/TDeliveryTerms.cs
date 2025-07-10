using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public class TDeliveryTerms : ITDeliveryTerms
{
	public TDeliveryTerms(JobComInvoiceHeader invoice)
	{
		Argument.NotNull(invoice, JobComInvoiceHeader.Schema.TableName);
		this.invoice = invoice;
	}

	readonly JobComInvoiceHeader invoice;

	public ZString DeliveryTerms { get => invoice.JZ_IncoTerm; }
	public ZString DeliveryTermsPlace { get => invoice.JZ_IncoTermPlace; }
	public ZString DeliveryTermsPlaceCode { get => invoice.JobDeclaration.ZG_AgreedPlaceCode; }
}
