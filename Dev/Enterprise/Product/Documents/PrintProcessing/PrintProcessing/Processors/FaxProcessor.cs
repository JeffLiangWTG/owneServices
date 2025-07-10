using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.PrintProcessing.FaxRouting;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.PrintProcessing
{
	class FaxProcessor : MergedPrintGroupProcessor
	{
		public FaxProcessor(StmPrintJobMergedCollection mergedPrintGroup, PrintJobManager.ProgressDelegate logProgress = null)
			: base(mergedPrintGroup)
		{
			LogProgress = logProgress;
		}

		protected override void ProcessPrintJobs(StmPrintJobMergedCollection mergedPrintJobs)
		{
			while (mergedPrintJobs.Count > 0)
			{
				StmPrintJob printJob = mergedPrintJobs[0];
				ProcessIndividualItem(printJob);

				if (mergedPrintJobs.Contains(printJob))
				{
					mergedPrintJobs.Remove(printJob);
				}
				printJob.DeleteStoredAttachment();
			}
		}

		protected override void ProcessIndividualItemCore(StmPrintJob printJob)
		{
			if (printJob.SP_JobType.ToUpper() == "FAX")
			{
				LogProcessing(printJob);
				if (deliveryDecider.Deliver(printJob))
				{
					printJob.CreateLogOnParent(Events.DocumentSent);
				}
			}
		}

		internal IFaxDeliverer deliveryDecider = new FaxDeliveryDecider();
	}
}
