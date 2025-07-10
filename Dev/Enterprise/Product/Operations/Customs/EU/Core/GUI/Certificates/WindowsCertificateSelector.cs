using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.GUI.Certificates
{
	public class WindowsCertificateSelector : CertificateSelector
	{
		public static CertificateSelector New(string chipset) => new WindowsCertificateSelector(chipset);

		protected WindowsCertificateSelector(string chipset) : base(chipset) { }

		protected override string CertificateSource => EUCommonConstants.CertificateSource.Windows;
	}
}
