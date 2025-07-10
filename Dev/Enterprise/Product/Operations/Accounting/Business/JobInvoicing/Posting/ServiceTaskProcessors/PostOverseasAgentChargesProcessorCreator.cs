using CargoWise.EntityFramework;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting
{
	public class PostOverseasAgentChargesProcessorCreator : IPostOverseasAgentChargesProcessorCreator
	{
		public IProcessor CreateOverseasAgentChargesPoster(IWorkflowProvider provider)
		{
			IProcessor result = null;

			if (provider is IJobCostingPlugIn)
			{
				var consol = provider as IJobCostingPlugIn;

				result = new OverseasAgentChargesPoster(consol);
			}
			else if (provider is IJobInvoicingPlugIn)
			{
				var plugin = provider as IJobInvoicingPlugIn;

				result = new JobOverseasAgentChargesPoster(plugin);
			}

			return result;
		}
	}
}
