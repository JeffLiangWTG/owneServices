using CargoWise.EntityFramework;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class RevenueRecognizerCreator : IRevenueRecognizerCreator
	{
		public IProcessor CreateRevenueRecognizer(IWorkflowProvider provider)
		{
			IProcessor result = null;

			var consol = provider as IJobCostingPlugIn;
			var plugIn = provider as IJobInvoicingPlugIn;
			if (consol != null)
			{
				result = new RevenueRecognizer(consol);
			}
			else if (plugIn != null)
			{
				result = new RevenueRecognizer(plugIn);
			}

			return result;
		}
	}
}
