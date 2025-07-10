using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth
{
	public class KoreaSouthEInvoicingCredentialSettings : GlobalEInvoicingCertificateCredentialSettings
	{
		protected override bool IsCompanyCredentialsRequired => true;
	}
}
