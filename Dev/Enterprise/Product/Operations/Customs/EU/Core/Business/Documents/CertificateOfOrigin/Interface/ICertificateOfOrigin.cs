using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin
{
	public interface ICertificateOfOrigin
	{
		ZString DeclarationReference { get; }
		ZString ReferenceDateFormat { get; }
		ZString SupplierAddress { get; }
		ZString ImporterAddress { get; }
		ZString DestinationCountry { get; }
		ITransportDetail TransportDetail { get; }
		ICustomsEndorsement CustomsEndorsement { get; }
		IExporterDeclaration ExporterDeclaration { get; }
		ZString Url { get; }
		ZString Remarks { get; }
	}
}
