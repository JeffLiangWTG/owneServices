using System.Collections.Generic;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IN.Business;

public interface ICryptokiCertificateProvider
{
	IReadOnlyList<CryptokiCertificate> GetCertificateList(string libraryName);
}
