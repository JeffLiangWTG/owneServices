using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Accounting.Business
{
	public interface ISecurityOverrideProviderSupportTwoApproverLogin
	{
		bool RequiresTwoApprovers { get; }
		bool RequiresSequentialApprovals { get; }
		List<ZGuid> UserPKsForTwoCredentialLogin { get; set; }
	}
}