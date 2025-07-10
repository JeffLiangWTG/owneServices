using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using Enterprise.Integration.Accounting;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Poland.Testing
{
	[TestedType(typeof(PolandEInvoicingObjectFactory.PolandCredentialSettings))]
	class PolandEInvoicingCredentialSettingsTest : GlobalEInvoicingCertificateCredentialSettingsTest
	{
		protected override IEInvoicingCertificateCredentialSettings GetCredentialSettings() => new PolandEInvoicingObjectFactory.PolandCredentialSettings();

		protected override bool ExpectedIsCompanyCredentialsRequired => true;

		protected override string[] ExpectedHiddenColumns => new string[] { "GP_MailBoxID" };
	}
}
