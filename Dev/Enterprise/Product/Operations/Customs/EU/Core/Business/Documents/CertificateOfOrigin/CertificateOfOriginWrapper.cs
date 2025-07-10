using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin
{
	public abstract class CertificateOfOriginWrapper : ICertificateOfOrigin
	{
		protected CertificateOfOriginWrapper(JobDeclaration declaration)
		{
			Declaration = Argument.NotNull(declaration, nameof(declaration));
		}

		protected CertificateOfOriginWrapper(CusEntryHeader entryHeader) : this(entryHeader?.Declaration)
		{
			EntryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		}

		ZString ICertificateOfOrigin.Remarks => ZString.Empty;

		ZString ICertificateOfOrigin.DeclarationReference => declarationReference ?? (declarationReference = Declaration.JE_DeclarationReference);
		string declarationReference;

		ZString ICertificateOfOrigin.ReferenceDateFormat => referenceDateFormat ?? (referenceDateFormat = GetReferenceDateFormat());
		string referenceDateFormat;

		ZString ICertificateOfOrigin.SupplierAddress => supplierAddress ?? (supplierAddress = GetFullAddressFormattedOrEmpty(Declaration.SupplierDocumentaryAddress));
		string supplierAddress;

		ZString ICertificateOfOrigin.ImporterAddress => importerAddress ?? (importerAddress = GetImporterAddress());
		string importerAddress;

		ZString ICertificateOfOrigin.DestinationCountry => destinationCountry ?? (destinationCountry = GetCountryDescription(Declaration.FinalDestination));
		string destinationCountry;

		ITransportDetail ICertificateOfOrigin.TransportDetail => (transportDetail ?? (transportDetail = GetNewTransportDetail()));
		ITransportDetail transportDetail;

		ICustomsEndorsement ICertificateOfOrigin.CustomsEndorsement => customsEndorsement ?? (customsEndorsement = GetNewCustomsEndorsement());
		ICustomsEndorsement customsEndorsement;

		IExporterDeclaration ICertificateOfOrigin.ExporterDeclaration => exporterDeclaration ?? (exporterDeclaration = GetNewExporterDeclaration());
		IExporterDeclaration exporterDeclaration;

		protected JobDeclaration Declaration { get; }
		protected CusEntryHeader EntryHeader { get; }

		protected virtual IExporterDeclaration GetNewExporterDeclaration() => new ExporterDeclarationWrapper();

		protected virtual ITransportDetail GetNewTransportDetail() => new TransportDetailWrapper(Declaration);

		protected virtual ICustomsEndorsement GetNewCustomsEndorsement() => new CustomsEndorsementWrapper(Declaration);

		protected virtual ZString GetCountryDescription(RefUNLOCO unloco) => unloco?.Country?.Description ?? ZString.Empty;

		protected virtual ZString GetImporterAddress()
		{
			return GetFullAddressFormattedOrEmpty(Declaration.ImporterDocumentaryAddress);
		}

		protected virtual ZString GetReferenceDateFormat() => "dd/MM/yyyy";

		ZString GetFullAddressFormattedOrEmpty(JobDocAddress docAddress) => docAddress?.Address?.AddressFullFormatted ?? ZString.Empty;

		ZString ICertificateOfOrigin.Url => ZString.Empty;
	}
}
