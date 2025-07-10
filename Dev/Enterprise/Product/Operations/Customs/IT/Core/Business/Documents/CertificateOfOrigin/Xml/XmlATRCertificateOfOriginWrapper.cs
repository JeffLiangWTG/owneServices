using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;
using IXmlATRCertificateOfOrigin = CargoWise.Customs.IT.MessageDefinitions.CertificateOfOrigin.IATRCertificateOfOrigin;

namespace Enterprise.Customs.IT.Business.Documents.CertificateOfOrigin;

public class XmlATRCertificateOfOriginWrapper : XmlCertificateOfOriginWrapper, IATRCertificateOfOrigin
{
	public XmlATRCertificateOfOriginWrapper(BusinessObjectFactory factory, IXmlATRCertificateOfOrigin certificateOfOrigin) : base(factory, certificateOfOrigin)
	{
		this.certificateOfOrigin = Argument.NotNull(certificateOfOrigin, nameof(certificateOfOrigin));
	}

	readonly IXmlATRCertificateOfOrigin certificateOfOrigin;

	IATRBoxItems IATRCertificateOfOrigin.ATRBoxItemBuilder => new XmlATRBoxItemBuilder(certificateOfOrigin.GoodsSummary);

	IATRCertificateItem IATRCertificateOfOrigin.TotalATRCertificateItem => null;

	IATRCertificateDeclaration IATRCertificateOfOrigin.Declaration => new XmlATRCertificateDeclaration(certificateOfOrigin, Factory);

	ZBool IATRCertificateOfOrigin.ShouldAddTotalCertificateItem => false;

	ZString IATRCertificateOfOrigin.ARTNumberCaption => Res.GetString("EBA8FAB6-DD97-4575-93D4-EABC0D4FD3AE", "A.TR.No");

	ZString IATRCertificateOfOrigin.ARTEuropeanUnionCaption => Res.GetString("DA6CECD6-0964-4CC7-B2B7-784000908B46", "EUROPEAN UNION");

	ZString ICertificateOfOrigin.Url => certificateOfOrigin.Url;
}
