using Enterprise.Security;

namespace Enterprise.Accounting.Business.TransactionApproval
{
	public interface ISecurityOverrideProviderWithApprovalRequest : ISecurityOverrideProvider
	{
		bool ShouldApprovalRequestBeCreated { get; }
	}
}
