using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	class JobConsolCostOnlyPosterCreator : IConsolCostOnlyPosterCreator
	{
		public IProcessor CreateConsolCostOnlyPoster(IWorkflowProvider provider)
		{
			IProcessor result = null;

			var plugIn = provider as IJobInvoicingPlugIn;
			if (plugIn != null && plugIn is Enterprise.Integration.Forwarding.IForwardingConsol)
			{
				result = new JobConsolCostOnlyPoster(plugIn);
			}

			return result;
		}
	}
}