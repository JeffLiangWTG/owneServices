using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin
{
	public interface IExporterDeclaration
	{
		ZString Place { get; }
		ZString ExporterDetails { get; }
		ZDate ReferenceDate { get; }
	}
}
