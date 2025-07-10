using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Accounting.Business.TransactionApproval
{
	public class NonInteractiveGLJournalSecurityOverrideProvider : SecurityOverrideProvider, ISecurityOverrideProviderWithApprovalRequest, Enterprise.Integration.Security.INonInteractiveSecurityOverrideProvider
	{
		public NonInteractiveGLJournalSecurityOverrideProvider(bool shouldApprovalRequestBeCreated)
		{
			this.shouldApprovalRequestBeCreated = shouldApprovalRequestBeCreated;
		}

		public bool ShouldApprovalRequestBeCreated => shouldApprovalRequestBeCreated;

		readonly bool shouldApprovalRequestBeCreated;

		protected override SecurityCore RequestLoginCredentials(SecurityCheckpoint checkPoint)
		{
			return shouldApprovalRequestBeCreated ? null : Env.Security;
		}

		protected override SecurityCertificate RequestGrantedConfirmation(SecurityCheckpoint checkPoint)
		{
			return SecurityCertificate.Granted;
		}
	}
}
