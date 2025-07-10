using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using static Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBase;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public static class IApportionedChargesHeaderListExtension
	{
		public static bool ReopenClosedJobs(this IApportionedChargesHeaderList list)
		{
			var hasClosedJobs = list.HasClosedJob();
			var result = !hasClosedJobs;

			if (hasClosedJobs && list.AllowedReopenClosedJob())
			{
				var provider = SecurityOverrideProviderSource.Get(list).Provider;

				foreach (var job in list.GetJobsToReopen())
				{
					SecurityOverrideProviderSource.Get(job).Provider = provider;
					job.JH_Status = JobHeaderStatus.Working.Code;
				}

				result = true;
			}

			return result;
		}

		public static bool AllowedReopenClosedJob(this IApportionedChargesHeaderList list)
		{
			return JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(list, list.GetJobsToReopen());
		}

		public static bool HasClosedJob(this IApportionedChargesHeaderList list)
		{
			return list.GetJobsToReopen().Any();
		}

		public static Job[] GetJobsToReopen(this IApportionedChargesHeaderList list)
		{
			var result = new HashSet<Job>();

			if (list.Factory.ServiceContainer.GetService<JobHeaderReloader>() == null)
			{
				list.Factory.ServiceContainer.AddService(new JobHeaderReloader(list.Factory));
			}

			foreach (var header in list.Headers)
			{
				foreach (var job in header.Charges.Where(x => x.InvoicingJob != null && x.InvoicingJob.JH_Status == JobHeaderStatus.Closed.Code && header.IsChargeReadyToPost(x)).Select(y => y.InvoicingJob))
				{
					result.Add(job);
				}
			}

			return result.ToArray();
		}
	}
}

#region Test
// Used by ApportionmentListing and ConsolRevenueMaster
// Unit tested for ConsolRevenueMaster by ConsolRevenueApportionFormTest.TestSaveButtonAsksToReOpenClosedJobBeforeSaving
#endregion
