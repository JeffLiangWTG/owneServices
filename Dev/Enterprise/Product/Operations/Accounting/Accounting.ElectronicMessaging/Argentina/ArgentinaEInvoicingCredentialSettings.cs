using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina
{
	public class ArgentinaEInvoicingCredentialSettings : GlobalEInvoicingCertificateCredentialSettings
	{
		protected override bool IsCompanyCredentialsRequired => true;

		protected override string[] HiddenGridColumns => new string[] { "GP_MailBoxID" };
	}
}
