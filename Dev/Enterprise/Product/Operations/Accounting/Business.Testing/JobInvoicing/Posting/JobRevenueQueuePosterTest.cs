using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	class JobRevenueQueuePosterTest : JobRevenuePosterTest
	{
		protected override IProcessor CreateJobPostingProcessor(IJobInvoicingPlugIn plugin)
		{
			return new JobRevenueQueuePoster(plugin, new[] { Charge }, new[] { GetGenericJobChargePostingQueue() });
		}

		IJobChargePostingQueue GetGenericJobChargePostingQueue()
		{
			var queue = JobChargePostingQueue.CreateNew(Factory, JobChargePostingQueueLookups.PostRevenue, Charge, Shipment.PK, "JS");
			var result = new JobChargePostingQueueData(queue);
			return result;
		}

		protected override IPostingJobTransactionsApprovalGUIProvider GUIProvider => null;
	}
}
