using System.Collections.Generic;
using Enterprise.Security;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public interface IReopenClosedJobSecurityOverrideProvider : ISecurityOverrideProvider
	{
		void AddClosedJobForSecurityProvider(IReadOnlyCollection<Job> closedJobCollection);
	}
}
