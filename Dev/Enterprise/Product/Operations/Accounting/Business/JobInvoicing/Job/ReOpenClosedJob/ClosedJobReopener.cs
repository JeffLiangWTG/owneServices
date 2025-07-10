using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using static Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBase;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public interface IClosedJobReopener
	{
		void ValidateClosedJob(ZPropertyInfo jobPropertyInfo, Job job);

		bool ReopenClosedJobs();
	}

	public class ClosedJobReopener : IClosedJobReopener
	{
		public ClosedJobReopener(IReOpenClosedJobDataProvider reOpenClosedJobDataProvider, IReopenClosedJobSecurityOverrideProvider reopenClosedJobSecurityOverrideProvider)
		{
			ReOpenClosedJobDataProvider = Argument.NotNull(reOpenClosedJobDataProvider, nameof(reOpenClosedJobDataProvider));
			ReopenClosedJobSecurityOverrideProvider = Argument.NotNull(reopenClosedJobSecurityOverrideProvider, nameof(reopenClosedJobSecurityOverrideProvider));
		}

		public readonly IReOpenClosedJobDataProvider ReOpenClosedJobDataProvider;
		readonly IReopenClosedJobSecurityOverrideProvider ReopenClosedJobSecurityOverrideProvider;

		void IClosedJobReopener.ValidateClosedJob(ZPropertyInfo jobPropertyInfo, Job job)
		{
			if (!jobPropertyInfo.HasErrors() && job != null && job.JH_Status == JobHeaderStatus.Closed.Code)
			{
				if (!JobReopenSecurityCheckHelper.CanReopenJob_NonInteractiveSecurityCheck(job.Factory, job))
				{
					jobPropertyInfo.AddWarning(ReopenClosedJobSecurityMessage);
				}
				else
				{
					jobPropertyInfo.AddWarning(ReopenClosedJobWarningMessage);
				}
			}
		}

		bool IClosedJobReopener.ReopenClosedJobs()
		{
			var result = true;

			new JobHeaderReloader(ReOpenClosedJobDataProvider.Factory);

			var closedJobs = GetUniqueClosedJobs(ReOpenClosedJobDataProvider.GetAllJobs());

			if (closedJobs != null && closedJobs.Any())
			{
				ReopenClosedJobSecurityOverrideProvider.AddClosedJobForSecurityProvider(closedJobs);
				var provider = new ReOpenJobProvider(ReopenClosedJobSecurityOverrideProvider, ReOpenClosedJobDataProvider.Factory);
				foreach (var job in closedJobs)
				{
					if (JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(provider, job))
					{
						using (job.TemporarySetProvider(ReopenClosedJobSecurityOverrideProvider))
						{
							job.ReOpenByImport(ReOpenClosedJobDataProvider.JobReopenLogText());
						}
					}
					else
					{
						result = false;
						break;
					}
				}
			}

			return result;
		}

		IReadOnlyCollection<Job> GetUniqueClosedJobs(IReadOnlyCollection<Job> allRelatedJobs)
		{
			return (allRelatedJobs != null) ? allRelatedJobs.Where(x => x.IsClosed).Distinct().ToList() : null;
		}

		string ReopenClosedJobSecurityMessage
		{
			get
			{
				return Res.GetString("155AE8D0-1C08-4A5F-94D1-356A9488DCEE", "This job is currently closed. As you do not have the access right to re-open the Job, you will require authorization to proceed upon saving.");
			}
		}

		string ReopenClosedJobWarningMessage
		{
			get
			{
				return Res.GetString("D1751A3E-C549-48C0-BBC4-89FBA4D00DD6", "This job is currently closed. The job will be reopened after saving.");
			}
		}

		class ReOpenJobProvider : IFactoryProvider, ISecurityOverrideProviderSource
		{
			public ReOpenJobProvider(ISecurityOverrideProvider securityOverrideProvider, BusinessObjectFactory businessObjectFactory)
			{
				Provider = securityOverrideProvider;
				Factory = businessObjectFactory;
			}

			public BusinessObjectFactory Factory { get; set; }

			public ISecurityOverrideProvider SecurityOverrideProvider
			{
				get { return ((ISecurityOverrideProviderSource)this).Provider; }
				set { ((ISecurityOverrideProviderSource)this).Provider = value; }
			}

			public ISecurityOverrideProvider Provider
			{
				get
				{
					if (provider == null)
					{
						provider = new DefaultAccessSecurityProvider();
					}
					return provider;
				}
				set
				{
					provider = value;
				}
			}

			ISecurityOverrideProvider provider;
		}
	}
}
