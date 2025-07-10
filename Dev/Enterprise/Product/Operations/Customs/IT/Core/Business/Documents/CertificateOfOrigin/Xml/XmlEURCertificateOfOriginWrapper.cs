using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;
using IXmlEURCertificateOfOrigin = CargoWise.Customs.IT.MessageDefinitions.CertificateOfOrigin.IEURCertificateOfOrigin;

namespace Enterprise.Customs.IT.Business.Documents.CertificateOfOrigin;

public class XmlEURCertificateOfOriginWrapper : XmlCertificateOfOriginWrapper, IEURCertificateOfOrigin
{
	public XmlEURCertificateOfOriginWrapper(BusinessObjectFactory factory, IXmlEURCertificateOfOrigin certificateOfOrigin) : base(factory, certificateOfOrigin)
	{
		this.certificateOfOrigin = Argument.NotNull(certificateOfOrigin, nameof(certificateOfOrigin));
	}

	readonly IXmlEURCertificateOfOrigin certificateOfOrigin;

	IEURGoodsSummary IEURCertificateOfOrigin.GoodsSummary => goodsSummary ?? (goodsSummary = new XmlEURGoodsSummaryWrapper(certificateOfOrigin.GoodsSummary));
	IEURGoodsSummary goodsSummary;

	ZString IEURCertificateOfOrigin.OriginCountry => originCountry ?? (originCountry = GetCountryDescription(certificateOfOrigin.OriginCountry));
	string originCountry;

	ZString IEURCertificateOfOrigin.OriginGroup => originGroup ?? (originGroup = GetCountriesDescription(certificateOfOrigin.OriginGroup));
	string originGroup;

	ZString IEURCertificateOfOrigin.DestinationGroup => destinationGroup ?? (destinationGroup = GetCountriesDescription(certificateOfOrigin.DestinationGroup));
	string destinationGroup;

	protected override ZString GetCountryDescription(ZString countryCode)
	{
		return countryCode == EuropeanUnionCode
			? EuropeanUnionDescription
			: base.GetCountryDescription(countryCode);
	}

	ZString GetCountriesDescription(IEnumerable<string> countryCodes)
	{
		if (countryCodes == null)
		{
			return ZString.Empty;
		}

		var countryGroupBuilder = new ZStringBuilder();

		foreach (var countryCode in countryCodes)
		{
			countryGroupBuilder.Append(GetCountryDescription(countryCode));
		}

		return countryGroupBuilder.ToStringWithDelimiterBetweenAppends(delimiterWithSpace);
	}

	const string delimiterWithSpace = ", ";

	const string EuropeanUnionCode = "UE";

	ZString EuropeanUnionDescription => Res.GetString("C0CB74FF-2DA6-42EB-9697-74F9BA0C5489", "Europe");
}
