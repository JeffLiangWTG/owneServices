namespace Enterprise.Security
{
	public interface ISecurityOverrideProvider
	{
		ISecurityCertificateProvider SecurityCertificates { get; }
		bool ShouldPromptForGrantedConfirmation { get; }
		bool IsUserInitiatorAndNotAllowedToApprove { get; }
		SecurityCore UserSecurityOverride { get; }
		SecurityCertificate PromptForAccessGrantConfirmation(SecurityCheckpoint checkpoint);
		SecurityCertificate PromptForTemporaryAccess(SecurityCheckpoint checkpoint);
	}
}
