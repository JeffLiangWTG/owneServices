using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Security;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class DefaultAccessSecurityProviderForWorkflowPosting : DefaultAccessSecurityProvider, ISecurityOverrideProviderWithApprovalRequest
	{
		bool ISecurityOverrideProviderWithApprovalRequest.ShouldApprovalRequestBeCreated => false;
	}
}
