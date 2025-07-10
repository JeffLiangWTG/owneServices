using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin
{
	public class ATRCertificateDeclarationWrapper : IATRCertificateDeclaration
	{
		public ATRCertificateDeclarationWrapper(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
		}
		readonly JobDeclaration declaration;

		ZString IATRCertificateDeclaration.GoodsOrigin => GetCountryDescription(declaration.JE_GoodsOrigin);

		ZString IATRCertificateDeclaration.GoodsDestination => GetCountryDescription(declaration.JE_GoodsDestination);

		ZString IATRCertificateDeclaration.FullFormattedExporterAddress => declaration.SupplierDocumentaryAddress?.Address?.AddressFullFormatted ?? ZString.Empty;

		ZString IATRCertificateDeclaration.FullFormattedImporterDocumentaryAddress => declaration.ImporterDocumentaryAddress?.Address?.AddressFullFormatted ?? ZString.Empty;

		ZString GetCountryDescription(ZString countryCode) => new CountryDescriptionRetriever(countryCode, declaration.Factory).RetrieveCountryDescription();
	}
}
