using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Integration
{
	public interface IJobInvoicingPlugInAdditionalJobs
	{
		IJobInvoicingPlugIn[] AdditionalJobsToShowChargesFor { get; }
	}

	public interface IJobProviderForInvoicing
	{
		void ParentOperationalJobRefChanged(ZString originalCostReference, IJobInvoicingPlugIn plugIn, bool isParenInDatabase);
	}

	public interface IAdditionalFetchHintsForJobParent
	{
		void LoadAdditionalFetchHints();
	}
}
