using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using Enterprise.Integration.Accounting;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico.Testing
{
	[TestedType(typeof(MexicoEInvoicingCredentialSettings))]
	class MexicoEInvoicingCredentialSettingsTest : GlobalEInvoicingCertificateCredentialSettingsTest
	{
		protected override IEInvoicingCertificateCredentialSettings GetCredentialSettings() => new MexicoEInvoicingCredentialSettings();

		protected override bool ExpectedIsCompanyCredentialsRequired => true;

		protected override string[] ExpectedHiddenColumns => new string[] { "GP_MailBoxID" };
	}
}
