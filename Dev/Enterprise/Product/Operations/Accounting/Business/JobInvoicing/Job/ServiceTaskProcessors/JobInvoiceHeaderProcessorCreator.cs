using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	class JobInvoiceHeaderProcessorCreator : IJobInvoiceHeaderProcessorCreator
	{
		public IProcessor CreateJobInvoiceHeaderProcessor(IWorkflowProvider provider)
		{
			IProcessor result = null;

			var plugIn = provider as IJobInvoicingPlugIn;
			if (plugIn != null && !(plugIn is Enterprise.Integration.Forwarding.IForwardingConsol))
			{
				result = new JobInvoiceHeaderProcessor(plugIn);
			}

			return result;
		}
	}
}