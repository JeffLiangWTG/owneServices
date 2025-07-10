using CargoWise.EntityFramework;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class CostPosterCreator : ICostPosterCreator
	{
		public IProcessor CreateCostPoster(IWorkflowProvider provider)
		{
			IProcessor result = null;

			var plugin = provider as IJobInvoicingPlugIn;

			if (plugin != null)
			{
				if (provider is IJobCostingPlugIn)
				{
					result = new ConsolCostPoster(plugin);
				}
				else
				{
					result = new JobCostPoster(plugin);
				}
			}

			return result;
		}
	}
}
