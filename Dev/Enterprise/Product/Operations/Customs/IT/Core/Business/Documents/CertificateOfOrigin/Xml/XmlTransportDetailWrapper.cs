using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;
using IXmlCertificateOfOrigin = CargoWise.Customs.IT.MessageDefinitions.CertificateOfOrigin.ICertificateOfOrigin;

namespace Enterprise.Customs.IT.Business.Documents.CertificateOfOrigin;

public class XmlTransportDetailWrapper : ITransportDetail
{
	public XmlTransportDetailWrapper(IXmlCertificateOfOrigin certificateOfOrigin)
	{
		this.certificateOfOrigin = Argument.NotNull(certificateOfOrigin, nameof(certificateOfOrigin));
	}

	readonly IXmlCertificateOfOrigin certificateOfOrigin;

	ZString ITransportDetail.Voyage => voyage ?? (voyage = certificateOfOrigin.TransportDetails);
	string voyage;
}
