using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobProviderForInvoicing : IJobProviderForInvoicing
	{
		void IJobProviderForInvoicing.ParentOperationalJobRefChanged(ZString originalCostReference, IJobInvoicingPlugIn plugIn, bool isParentInDatabase)
		{
			var job = (Job)new JobHeader.Loader(plugIn).Load();
			if (job != null)
			{
				job.ParentOperationalJobRefChanged(originalCostReference, plugIn.InvoicingSupporter.OperationalJobRef, isParentInDatabase);
			}
		}
	}
}