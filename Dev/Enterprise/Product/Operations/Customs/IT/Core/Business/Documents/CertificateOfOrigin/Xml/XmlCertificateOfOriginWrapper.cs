using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;
using IXmlCertificateOfOrigin = CargoWise.Customs.IT.MessageDefinitions.CertificateOfOrigin.ICertificateOfOrigin;
using IXmlTrader = CargoWise.Customs.IT.MessageDefinitions.CertificateOfOrigin.ITrader;

namespace Enterprise.Customs.IT.Business.Documents.CertificateOfOrigin;

public abstract class XmlCertificateOfOriginWrapper : ICertificateOfOrigin
{
	protected XmlCertificateOfOriginWrapper(BusinessObjectFactory factory, IXmlCertificateOfOrigin certificateOfOrigin)
	{
		Factory = Argument.NotNull(factory, nameof(factory));
		this.certificateOfOrigin = Argument.NotNull(certificateOfOrigin, nameof(certificateOfOrigin));
	}

	protected BusinessObjectFactory Factory { get; }
	readonly IXmlCertificateOfOrigin certificateOfOrigin;

	ZString ICertificateOfOrigin.DeclarationReference => ZString.Empty;

	ZString ICertificateOfOrigin.ReferenceDateFormat => ReferenceDateFormat;

	ZString ICertificateOfOrigin.SupplierAddress => supplierAddress ?? (supplierAddress = GetNewTrader(certificateOfOrigin.Consignor));
	string supplierAddress;

	ZString ICertificateOfOrigin.ImporterAddress => importerAddress ?? (importerAddress = GetNewTrader(certificateOfOrigin.Consignee));
	string importerAddress;

	ZString ICertificateOfOrigin.DestinationCountry => destinationCountry ?? (destinationCountry = GetCountryDescription(certificateOfOrigin.DestinationCountry));
	string destinationCountry;

	ITransportDetail ICertificateOfOrigin.TransportDetail => transportDetail ?? (transportDetail = new XmlTransportDetailWrapper(certificateOfOrigin));
	ITransportDetail transportDetail;

	ICustomsEndorsement ICertificateOfOrigin.CustomsEndorsement => customsEndorsement ?? (customsEndorsement = GetNewCustomsEndorsement());
	ICustomsEndorsement customsEndorsement;

	IExporterDeclaration ICertificateOfOrigin.ExporterDeclaration => exporterDeclaration ?? (exporterDeclaration = GetNewExporterDeclaration());
	IExporterDeclaration exporterDeclaration;

	ZString ICertificateOfOrigin.Url => url ?? (url = certificateOfOrigin.Url);
	string url;

	ZString ICertificateOfOrigin.Remarks => remarks ?? (remarks = certificateOfOrigin.Remarks);
	string remarks;

	protected virtual ZString GetCountryDescription(ZString countryCode) => new CountryDescriptionRetriever(countryCode, Factory).RetrieveCountryDescription();

	#region Implementation

	const string ReferenceDateFormat = "dd/MM/yyyy";

	ZString GetNewTrader(IXmlTrader trader)
	{
		if (trader == null)
		{
			return ZString.Empty;
		}

		return new ZStringBuilder()
			.AppendIfNotEmpty(trader.Name)
			.AppendIfNotEmpty(trader.Address)
			.AppendIfNotEmpty(trader.City)
			.AppendIfNotEmpty(trader.ZipCode)
			.AppendIfNotEmpty(trader.CountryCode)
			.ToStringWithNewLineBetweenAppends();
	}

	ICustomsEndorsement GetNewCustomsEndorsement()
	{
		var customsEndorsement = certificateOfOrigin.CustomsEndorsement;
		return customsEndorsement != null
			? new XmlCustomsEndorsementWrapper(customsEndorsement)
			: new EmptyCustomsEndorsementWrapper();
	}

	IExporterDeclaration GetNewExporterDeclaration()
	{
		var exporterDeclaration = certificateOfOrigin.ExporterDeclaration;
		return exporterDeclaration != null
			? new XmlExporterDeclarationWrapper(exporterDeclaration)
			: new EmptyExporterDeclarationWrapper();
	}

	#endregion
}
