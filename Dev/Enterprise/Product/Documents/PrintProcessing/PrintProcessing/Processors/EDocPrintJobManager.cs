using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Scheduler.Business;

namespace Enterprise.PrintProcessing
{
	public class EDocPrintJobManager : PrintJobManager
	{
		internal override MergedPrintGroupProcessor GetProcessor(StmPrintJobMergedCollection mergedPrintGroup, ProgressDelegate logProgress)
		{
			return new EDocProcessor(mergedPrintGroup, logProgress);
		}

		internal override void ProcessPrintJobsCore(IEnumerable<StmPrintJob> printJobs)
		{
			var jobsExcludeEML = new List<StmPrintJob>();
			foreach (var printJob in printJobs)
			{
				var isEMLJobWithEDocsNotProcessed = printJob.IsEmailJobFromReportRun && !printJob.SP_EDocsProcessed;
				if (isEMLJobWithEDocsNotProcessed)
				{
					printJob.SP_EDocsProcessed = true;
					printJob.Factory.Save();
				}
				else
				{
					jobsExcludeEML.Add(printJob);
				}
			}
			base.ProcessPrintJobsCore(jobsExcludeEML);
		}

		protected override void DeliverPrintJobs(StmPrintJobMergedCollection mergedPrintGroup)
		{
			if (mergedPrintGroup.ShouldSaveToEDocs && !mergedPrintGroup.EDocsProcessed)
			{
				base.DeliverPrintJobs(mergedPrintGroup);
			}
		}

		protected override void NotifyPrintJobsDelivered(StmPrintJobMergedCollection mergedPrintJobs)
		{
			mergedPrintJobs.OfType<StmPrintJob>().ForEach(j =>
			{
				ConcurrencyInfo.SetConcurrencyPolicy(j, nameof(StmPrintJob.SP_JobType), ConcurrencyPolicy.Ignore);
				ConcurrencyInfo.SetConcurrencyPolicy(j, nameof(StmPrintJob.SP_RetryAttempts), ConcurrencyPolicy.Ignore);
			});
			mergedPrintJobs.MarkAllEDocsProcessed();
		}
	}
}
