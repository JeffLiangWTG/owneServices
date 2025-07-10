using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.GUI.Certificates
{
	public class TokenCertificateSelector : CertificateSelector
	{
		public static CertificateSelector New(string chipset) => new TokenCertificateSelector(chipset);

		protected TokenCertificateSelector(string chipset) : base(chipset) { }

		protected override string CertificateSource => EUCommonConstants.CertificateSource.Token;
	}
}
