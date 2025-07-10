namespace Enterprise.Integration.Accounting
{
	public interface IEInvoicingCertificateCredentialSettings : IEInvoicingCredentialSettings, IGridControlSettings
	{
		/// <summary>
		/// Days before a validation warning appears for certificate expiry.
		/// </summary>
		int ExpiryWarningDays { get; }
	}
}
