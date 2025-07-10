using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class InvoicingPluginToFreightReopenClosedInternalJobDataProvider : IReOpenClosedJobDataProvider
	{
		public InvoicingPluginToFreightReopenClosedInternalJobDataProvider(Job job)
		{
			this.job = Argument.NotNull(job, nameof(job));
		}

		readonly Job job;

		string IReOpenClosedJobDataProvider.JobReopenLogText() => string.Format(CultureInfo.InvariantCulture, (NoResString)" - {0} Auto JRJ", job.JH_JobNum);

		IReadOnlyCollection<Job> IReOpenClosedJobDataProvider.GetAllJobs()
		{
			return job.Charges
					  .Where(charge => charge.JR_JH_InternalJob.IsValid && (charge.ShouldCreateCostJRJ || charge.ShouldCreateSellJRJ))
					  .Select(charge => charge.InternalJob as Job)
					  .Where(job => job != null)
					  .ToList();
		}

		public BusinessObjectFactory Factory => job.Factory;
	}
}
