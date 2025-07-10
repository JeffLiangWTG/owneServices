using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Client.EDI.TfsRest
{
	public interface IDevOpsRestContext
	{
		IEnumerable<PullRequestInfo> GetPullRequestsByOwner(string ownerId);
		string GetUserId(ZString userCode);
	}
}
