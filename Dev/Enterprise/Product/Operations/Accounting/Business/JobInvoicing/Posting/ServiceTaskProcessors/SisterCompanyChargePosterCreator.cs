using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	class SisterCompanyChargePosterCreator : ISisterCompanyChargePosterCreator
	{
		public IProcessor CreateSisterCompanyChargePoster(IWorkflowProvider provider)
		{
			IProcessor result = null;

			var plugIn = provider as IJobInvoicingPlugIn;
			if (plugIn != null && !(plugIn is Enterprise.Integration.Forwarding.IForwardingConsol))
			{
				result = new SisterCompanyChargePoster(plugIn);
			}

			return result;
		}
	}
}
