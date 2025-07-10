using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using Enterprise.Integration.Accounting;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Testing
{
	[TestedType(typeof(KoreaSouthEInvoicingCredentialSettings))]
	class KoreaSouthEInvoicingCredentialSettingsTest : GlobalEInvoicingCertificateCredentialSettingsTest
	{
		protected override IEInvoicingCertificateCredentialSettings GetCredentialSettings() => new KoreaSouthEInvoicingCredentialSettings();

		protected override bool ExpectedIsCompanyCredentialsRequired => true;
	}
}
