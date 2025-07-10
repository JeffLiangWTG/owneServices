using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin
{
	public class ExporterDeclarationWrapper : IExporterDeclaration
	{
		ZString IExporterDeclaration.Place => place ?? (place = GetPlace());
		string place;

		protected virtual ZString GetPlace() => ZString.Empty;

		ZDate IExporterDeclaration.ReferenceDate => referenceDate ?? (referenceDate = GetReferenceDate()).Value;
		ZDate? referenceDate;

		protected virtual ZDate GetReferenceDate() => ZDate.Empty;

		ZString IExporterDeclaration.ExporterDetails => ZString.Empty;
	}
}
