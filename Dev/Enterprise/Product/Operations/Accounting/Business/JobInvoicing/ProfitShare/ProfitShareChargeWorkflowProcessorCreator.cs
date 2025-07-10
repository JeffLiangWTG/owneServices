using CargoWise.EntityFramework;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare
{
	class ProfitShareChargeWorkflowProcessorCreator : IProfitShareChargeWorkflowProcessorCreator
	{
		public IProcessor CreateProfitShareChargeWorkflowProcessor(IWorkflowProvider provider)
		{
			IProcessor result = null;

			var consol = provider as IJobCostingPlugIn;
			var plugIn = provider as IJobInvoicingPlugIn;

			if (consol != null)
			{
				result = new ProfitShareChargeWorkflowProcessor(consol);
			}
			else if (plugIn != null)
			{
				result = new ProfitShareChargeWorkflowProcessor(plugIn);
			}

			return result;
		}
	}
}
