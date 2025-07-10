using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	class LocalSisterCompanyChargePosterCreator : ILocalSisterCompanyChargePosterCreator
	{
		public IProcessor CreateLocalSisterCompanyChargePoster(IWorkflowProvider provider)
		{
			IProcessor result = null;

			var plugIn = provider as IJobInvoicingPlugIn;
			if (plugIn != null && !(plugIn is Enterprise.Integration.Forwarding.IForwardingConsol))
			{
				result = new LocalSisterCompanyChargePoster(plugIn);
			}

			return result;
		}
	}
}
