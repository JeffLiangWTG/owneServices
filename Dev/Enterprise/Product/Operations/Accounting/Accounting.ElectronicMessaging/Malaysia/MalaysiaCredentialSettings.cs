using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;

namespace Enterprise.Accounting.ElectronicMessaging.Malaysia
{
	public class MalaysiaCredentialSettings : GlobalEInvoicingCertificateCredentialSettings
	{
		protected override bool IsCompanyCredentialsRequired => true;
	}
}
