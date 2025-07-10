using CargoWise.Types;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;

namespace Enterprise.Customs.IT.Business.Documents.CertificateOfOrigin;

public class EmptyExporterDeclarationWrapper : IExporterDeclaration
{
	ZString IExporterDeclaration.Place => ZString.Empty;

	ZString IExporterDeclaration.ExporterDetails => ZString.Empty;

	ZDate IExporterDeclaration.ReferenceDate => ZDate.Empty;
}
