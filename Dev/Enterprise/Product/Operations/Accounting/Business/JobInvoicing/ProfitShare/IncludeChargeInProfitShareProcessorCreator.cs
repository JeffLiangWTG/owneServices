using CargoWise.EntityFramework;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	class IncludeChargeInProfitShareProcessorCreator : IIncludeChargeInProfitShareProcessorCreator
	{
		public IProcessor CreateIncludeChargeInProfitShareProcessor(IWorkflowProvider provider)
		{
			IProcessor result = null;

			var plugIn = provider as IJobInvoicingPlugIn;
			if (plugIn != null && !(plugIn is IJobCostingPlugIn))
			{
				result = new IncludeChargeInProfitShareProcessor(plugIn);
			}

			return result;
		}
	}
}