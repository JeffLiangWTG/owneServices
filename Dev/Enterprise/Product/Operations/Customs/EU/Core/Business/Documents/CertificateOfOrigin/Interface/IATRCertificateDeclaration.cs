using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin
{
	public interface IATRCertificateDeclaration
	{
		ZString GoodsOrigin { get; }
		ZString GoodsDestination { get; }
		ZString FullFormattedExporterAddress { get; }
		ZString FullFormattedImporterDocumentaryAddress { get; }
	}
}
