using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public class TransportMeansImport : ITTransportMeansImport
{
	public TransportMeansImport(JobComInvoiceHeader invoice)
	{
		Argument.NotNull(invoice, JobComInvoiceHeader.Schema.TableName);
		this.invoice = invoice;
		DeliveryTerms = new TDeliveryTerms(invoice);
	}

	readonly JobComInvoiceHeader invoice;

	public ZString BorderMode { get => invoice.JobDeclaration.TransportModeTranslator.TranslateToWCOCode(invoice.JobDeclaration.JE_TransportMode); }
	public ZString BorderNationality { get => invoice.JobDeclaration.ZG_Box18TransportNationality; }
	public ITDeliveryTerms DeliveryTerms { get; set; }
	public ZString DepartureIdentity { get => invoice.JobDeclaration.ZG_Box18TransportID; }
	public ZString DispatchCountry { get => invoice.JobDeclaration.JE_GoodsOrigin; }
	public ZString InlandMode { get => invoice.JobDeclaration.TransportModeTranslator.TranslateToWCOCode(invoice.JobDeclaration.JE_TransportModeInland); }
}
