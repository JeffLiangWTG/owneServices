using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Certificates.Testing
{
	[TestedType(typeof(CertificateInformationForm))]
	sealed class CertificateInformationFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new CertificateInformationForm(new CryptokiCertificate());
	}
}
