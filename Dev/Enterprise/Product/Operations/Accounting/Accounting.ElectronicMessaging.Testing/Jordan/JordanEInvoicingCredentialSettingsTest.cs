using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using Enterprise.Integration.Accounting;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Jordan.Testing
{
	[TestedType(typeof(JordanCredentialSettings))]
	class JordanEInvoicingCredentialSettingsTest : GlobalEInvoicingCertificateCredentialSettingsTest
	{
		protected override IEInvoicingCertificateCredentialSettings GetCredentialSettings() => new JordanCredentialSettings();

		protected override bool ExpectedIsCompanyCredentialsRequired => true;
	}
}
