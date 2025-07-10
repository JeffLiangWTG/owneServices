using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;

namespace Enterprise.Accounting.GUI
{
	public class ReopenClosedJobSecurityOverrideProvider : SecurityOverrideProviderWithJobReopenSupport, IReopenClosedJobSecurityOverrideProvider
	{
		void IReopenClosedJobSecurityOverrideProvider.AddClosedJobForSecurityProvider(IReadOnlyCollection<Job> closedJobs)
		{
			UniqueClosedJobs = closedJobs;
			closedJobsList = new ZStringBuilder(UniqueClosedJobs.Select(x => x.JH_JobNum)).ToStringWithDelimiterBetweenAppends(", ");
		}

		protected override string GetReopenClosedJobSecurityGrantedMessage()
		{
			return Res.GetString("3752B248-9383-4C3D-A5BB-0ADBF4071013", "Closed Job(s): {0}\r\n{1}", closedJobsList, SecurityOverrideProviderWithJobReopenSupport.ReopenClosedJobSecurityGrantedMessage);
		}

		protected override string GetReopenClosedJobSecurityOverrideMessage()
		{
			return Res.GetString("C5CE5875-197C-42EA-A2F3-A3FE473344B8", "Closed Job(s): {0}\r\n{1}", closedJobsList, SecurityOverrideProviderWithJobReopenSupport.ReopenClosedJobSecurityOverrideMessageMoreThanOneJobs);
		}

		protected override string GetReopenRestrictedClosedJobSecurityOverrideMessage()
		{
			return Res.GetString("2CB5D67C-0F1D-4B17-965E-809808594482", "Closed Job(s): {0}\r\n{1}", closedJobsList, SecurityOverrideProviderWithJobReopenSupport.ReopenRestrictedClosedJobSecurityOverrideMessageMoreThanOneJobs);
		}

		IReadOnlyCollection<Job> UniqueClosedJobs;
		string closedJobsList;
	}
}
