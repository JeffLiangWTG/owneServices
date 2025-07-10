using System;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageDefinitions.CertificateOfOrigin;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;
using IXmlATRCertificateOfOrigin = CargoWise.Customs.IT.MessageDefinitions.CertificateOfOrigin.IATRCertificateOfOrigin;

namespace Enterprise.Customs.IT.Business.Documents.CertificateOfOrigin;

public class XmlATRCertificateDeclaration : IATRCertificateDeclaration
{
	public XmlATRCertificateDeclaration(IXmlATRCertificateOfOrigin certificateOfOrigin, BusinessObjectFactory factory)
	{
		this.certificateOfOrigin = Argument.NotNull(certificateOfOrigin, nameof(certificateOfOrigin));
		this.factory = Argument.NotNull(factory, nameof(factory));
	}
	readonly IXmlATRCertificateOfOrigin certificateOfOrigin;
	readonly BusinessObjectFactory factory;

	ZString IATRCertificateDeclaration.GoodsOrigin => certificateOfOrigin.ExportCountry;

	ZString IATRCertificateDeclaration.GoodsDestination => certificateOfOrigin.DestinationCountry;

	ZString IATRCertificateDeclaration.FullFormattedExporterAddress => GetFullFormattedTraderAddress(certificateOfOrigin.Consignor);

	ZString IATRCertificateDeclaration.FullFormattedImporterDocumentaryAddress => GetFullFormattedTraderAddress(certificateOfOrigin.Consignee);

	ZString GetFullFormattedTraderAddress(ITrader trader)
	{
		if (trader == null)
		{
			return ZString.Empty;
		}

		var builder = new ZStringBuilder();
		builder.Append(trader.Name);
		builder.Append(trader.Address);
		builder.Append(FormattableString.Invariant($"{trader.ZipCode} {trader.City}"));
		builder.Append(new CountryDescriptionRetriever(trader.CountryCode, factory).RetrieveCountryDescription().ToUpper());

		return builder.ToStringWithNewLineBetweenAppends();
	}
}
