using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using Enterprise.Integration.Accounting;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina.Testing
{
	[TestedType(typeof(ArgentinaEInvoicingCredentialSettings))]
	class ArgentinaEInvoicingCredentialSettingsTest : GlobalEInvoicingCertificateCredentialSettingsTest
	{
		protected override IEInvoicingCertificateCredentialSettings GetCredentialSettings() => new ArgentinaEInvoicingCredentialSettings();

		protected override bool ExpectedIsCompanyCredentialsRequired => true;

		protected override string[] ExpectedHiddenColumns => new string[] { "GP_MailBoxID" };
	}
}
