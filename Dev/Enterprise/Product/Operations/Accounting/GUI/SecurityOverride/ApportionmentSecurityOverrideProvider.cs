using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;

namespace Enterprise.Accounting.GUI
{
	public class ApportionmentSecurityOverrideProvider : SecurityOverrideProviderWithJobReopenSupport
	{
		public ApportionmentSecurityOverrideProvider(IApportionedChargesHeaderList apportionments)
		{
			this.Apportionments = apportionments;
		}

		readonly IApportionedChargesHeaderList Apportionments;

		protected override string GetReopenClosedJobSecurityGrantedMessage()
		{
			return Res.GetString("c1c86539-eb57-4358-98a6-dff8b44ee988", "Closed Job(s) :{0}\r\n{1}", GetClosedJobs(), SecurityOverrideProviderWithJobReopenSupport.ReopenClosedJobSecurityGrantedMessage);
		}

		protected override string GetReopenClosedJobSecurityOverrideMessage()
		{
			return Res.GetString("c1c86539-eb57-4358-98a6-dff8b44ee988", "Closed Job(s) :{0}\r\n{1}", GetClosedJobs(), SecurityOverrideProviderWithJobReopenSupport.ReopenClosedJobSecurityOverrideMessageMoreThanOneJobs);
		}

		protected override string GetReopenRestrictedClosedJobSecurityOverrideMessage()
		{
			return Res.GetString("303E1F62-08AB-4151-835E-5F02AFA1AF0F", "Closed Job(s) :{0}\r\n{1}", GetClosedJobs(), SecurityOverrideProviderWithJobReopenSupport.ReopenRestrictedClosedJobSecurityOverrideMessageMoreThanOneJobs);
		}

		string GetClosedJobs()
		{
			List<ZString> jobNumbers = new List<ZString>();

			if (Apportionments != null)
			{
				foreach (var job in Apportionments.GetJobsToReopen())
				{
					jobNumbers.Add(job.JH_JobNum);
				}
			}
			return ZString.Join(",", jobNumbers.ToArray());
		}
	}
}
