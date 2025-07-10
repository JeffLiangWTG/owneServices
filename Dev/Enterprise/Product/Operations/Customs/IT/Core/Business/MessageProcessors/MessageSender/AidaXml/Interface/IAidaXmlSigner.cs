using System;

namespace Enterprise.Customs.IT.Business;

public interface IAidaXmlSigner
{
	byte[] Sign(byte[] xmlBytes, ICryptokiGlbExternalPassword cryptokiCertificatePassword, DateTime signatureTime);
}
