using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using Enterprise.Integration.Accounting;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Malaysia.Testing
{
	[TestedType(typeof(MalaysiaCredentialSettings))]
	class MalaysiaEInvoicingCredentialSettingsTest : GlobalEInvoicingCertificateCredentialSettingsTest
	{
		protected override IEInvoicingCertificateCredentialSettings GetCredentialSettings() => new MalaysiaCredentialSettings();

		protected override bool ExpectedIsCompanyCredentialsRequired => true;
	}
}
