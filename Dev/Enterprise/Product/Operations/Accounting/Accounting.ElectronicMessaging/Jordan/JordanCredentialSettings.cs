using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;

namespace Enterprise.Accounting.ElectronicMessaging.Jordan
{
	class JordanCredentialSettings : GlobalEInvoicingCertificateCredentialSettings
	{
		protected override bool IsCompanyCredentialsRequired => true;
	}
}
