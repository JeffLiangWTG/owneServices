using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.Certificates;

namespace Enterprise.Customs.EU.GUI.Certificates.Testing
{
	public sealed class DummyCertificateSelector : CertificateSelector
	{
		public DummyCertificateSelector(string chipset, IReadOnlyList<CryptokiCertificate> cryptokiCertificates) : base(chipset)
		{
			this.cryptokiCertificates = cryptokiCertificates;
		}
		readonly IReadOnlyList<CryptokiCertificate> cryptokiCertificates;

		protected override string CertificateSource => "Dummy";

		protected override SelectCertificateForm GetSelectCertificateForm(IReadOnlyList<CryptokiCertificate> certificates) => new DummySelectCertificateForm(cryptokiCertificates);
	}
}
