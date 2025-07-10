using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	class JobCostQueuePosterTest : JobCostPosterTest
	{
		protected override IProcessor CreateJobPostingProcessor(IJobInvoicingPlugIn plugin)
		{
			return new JobCostQueuePoster(plugin, new[] { Charge }, new[] { GenericJobChargePostingQueue });
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();

			var queue = JobChargePostingQueue.CreateNew(Factory, JobChargePostingQueueLookups.PostCost, Charge, Shipment.PK, "JS");
			GenericJobChargePostingQueue = new JobChargePostingQueueData(queue);
			Factory.Save();
		}

		IJobChargePostingQueue GenericJobChargePostingQueue;
	}
}
