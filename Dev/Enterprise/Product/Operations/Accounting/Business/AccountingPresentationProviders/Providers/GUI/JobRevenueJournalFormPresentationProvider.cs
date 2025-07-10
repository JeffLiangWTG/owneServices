using CargoWise.Common;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;

namespace Enterprise.Accounting.Business.AccountingPresentationProviders
{
	public interface IJobRevenueJournalFormPresentationProvider
	{
		PreSaveActionsResult PreSaveActions();
	}

	public class JobRevenueJournalFormPresentationProvider : IJobRevenueJournalFormPresentationProvider
	{
		public JobRevenueJournalFormPresentationProvider(IClosedJobReopener closedJobReopener)
		{
			ClosedJobReopener = Argument.NotNull(closedJobReopener, nameof(closedJobReopener));
		}

		readonly IClosedJobReopener ClosedJobReopener;

		PreSaveActionsResult IJobRevenueJournalFormPresentationProvider.PreSaveActions()
		{
			if (!ClosedJobReopener.ReopenClosedJobs())
			{
				return PreSaveActionsResult.Failure(Res.GetString("cba2e260-7bb2-4cec-a2c8-14a2c261315a", "Posting requires job reopening."));
			}
			return PreSaveActionsResult.Success();
		}
	}
}
