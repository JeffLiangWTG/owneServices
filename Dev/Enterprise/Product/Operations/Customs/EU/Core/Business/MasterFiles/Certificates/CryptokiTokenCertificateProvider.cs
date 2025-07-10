using CargoWise.Application;
using CargoWise.Cryptoki.Common.ClientServerApi;

namespace Enterprise.Customs.EU.Business
{
	public class CryptokiTokenCertificateProvider : CryptokiCertificateProvider
	{
		protected override CertificateInfo GetCertificate(string chipset, byte[] serialNumber)
		{
			var selectedChipset = CertificateHelper.ParseChipset(chipset);
			return ObjectFactory.Get<ICryptoApi>().GetCertificateFromToken(selectedChipset, serialNumber);
		}

		protected override CertificateInfo[] GetCertificates(string chipset)
		{
			var selectedChipset = CertificateHelper.ParseChipset(chipset);
			return ObjectFactory.Get<ICryptoApi>().GetCertificatesFromToken(selectedChipset);
		}
	}
}
