using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico
{
	public class MexicoEInvoicingCredentialSettings : GlobalEInvoicingCertificateCredentialSettings
	{
		protected override bool IsCompanyCredentialsRequired => true;

		protected override string[] HiddenGridColumns => new string[] { "GP_MailBoxID" };
	}
}
