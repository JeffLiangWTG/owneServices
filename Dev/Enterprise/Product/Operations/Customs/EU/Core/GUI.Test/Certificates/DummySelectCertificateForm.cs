using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.Certificates;

namespace Enterprise.Customs.EU.GUI.Certificates.Testing
{
	sealed class DummySelectCertificateForm : SelectCertificateForm
	{
		public DummySelectCertificateForm(IReadOnlyList<CryptokiCertificate> certificates) : base(certificates)
		{
			SelectedCertificate = certificates.FirstOrDefault();
		}
	}
}
