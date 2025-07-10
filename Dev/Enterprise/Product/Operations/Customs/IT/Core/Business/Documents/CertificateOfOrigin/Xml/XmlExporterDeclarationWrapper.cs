using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;
using IXmlExporterDeclaration = CargoWise.Customs.IT.MessageDefinitions.CertificateOfOrigin.IExporterDeclaration;

namespace Enterprise.Customs.IT.Business.Documents.CertificateOfOrigin;

public class XmlExporterDeclarationWrapper : IExporterDeclaration
{
	public XmlExporterDeclarationWrapper(IXmlExporterDeclaration exporterDeclaration)
	{
		this.exporterDeclaration = Argument.NotNull(exporterDeclaration, nameof(exporterDeclaration));
	}

	readonly IXmlExporterDeclaration exporterDeclaration;

	ZString IExporterDeclaration.Place => place ?? (place = exporterDeclaration.ReferencePlace);
	string place;

	ZString IExporterDeclaration.ExporterDetails => ZString.Empty;

	ZDate IExporterDeclaration.ReferenceDate => referenceDate ?? (referenceDate = new ZDate(exporterDeclaration.ReferenceDate)).Value;
	ZDate? referenceDate;
}
