namespace Enterprise.Integration.Accounting
{
	public static class IEInvoicingCredentialSettingsExtensions
	{
		public static bool IsCertificate(this IEInvoicingCredentialSettings credentialSettings)
			=> credentialSettings is IEInvoicingCertificateCredentialSettings;

		public static bool IsPassword(this IEInvoicingCredentialSettings credentialSettings)
			=> credentialSettings is IEInvoicingPasswordCredentialSettings;

		public static bool IsNoCredential(this IEInvoicingCredentialSettings credentialSettings)
			=> !IsCertificate(credentialSettings)
			&& !IsPassword(credentialSettings);
	}
}
