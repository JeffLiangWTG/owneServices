using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Integration
{
	public delegate void PostTransactionsDelegate(JobInvoicingPostingOption postingOption);

	public interface IBulkPostingModuleHelper
	{
		void Initialize(ZString businessObjectName, bool isConsolPosting);
		IMenuItem GetPostMenuItem(PostTransactionsDelegate modulePostTransactionsDelegate, params JobInvoicingPostingOption[] optionsToCreateMenuItemsFor);
		void PostTransactions(JobInvoicingPostingOption postingOption, BusinessObject[] selectedElements);
		void PostTransactions(JobInvoicingPostingOption postingOption, IJobCostingPlugIn[] selectedConsols);
	}

	public interface IBulkPostingModuleInternalsForTesting : IDisposable
	{
#if DEBUG
		IMenuItem PostMenuItem { get; }
		JobInvoicingPostingOption LastUsedPostingOptionForTest { get; }
		bool DontDoActualPosting { get; set; }
#endif

	}
}
