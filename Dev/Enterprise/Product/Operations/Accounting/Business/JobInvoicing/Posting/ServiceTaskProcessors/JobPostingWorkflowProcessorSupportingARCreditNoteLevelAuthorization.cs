using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public abstract class JobPostingWorkflowProcessorSupportingARCreditNoteLevelAuthorization : JobPostingWorkflowProcessor
	{
		protected JobPostingWorkflowProcessorSupportingARCreditNoteLevelAuthorization(IJobInvoicingPlugIn plugIn, IPostingJobTransactionsApprovalGUIProvider guiProviderForARCreditNoteLevelAuthorization = null,
			ZDateTime invoiceDateOverride = new ZDateTime(), ZDateTime postDateOverride = new ZDateTime())
			: base(plugIn)
		{
			GuiProviderForARCreditNoteLevelAuthorization = guiProviderForARCreditNoteLevelAuthorization;
			InvoiceDateOverride = invoiceDateOverride;
			PostDateOverride = postDateOverride;
		}
		ZDateTime InvoiceDateOverride { get; }
		ZDateTime PostDateOverride { get; }

		IPostingJobTransactionsApprovalGUIProvider GuiProviderForARCreditNoteLevelAuthorization { get; }

		protected sealed override bool PerformPost(InvoicingPostManager postManager)
		{
			bool result = false;
			TransactionCreatorHashtable transactions = PerformPostCore(postManager);

			if (!postManager.CancelPosting)
			{
				bool continueProcessing;
				if (GuiProviderForARCreditNoteLevelAuthorization != null)
				{
					continueProcessing = GetOverrideDatesStatus(transactions, postManager);
					continueProcessing = continueProcessing && PerformARCreditNoteLevelAuthorization(postManager);
				}
				else
				{
					continueProcessing = PerformBackDating(postManager, transactions);
				}

				if (continueProcessing)
				{
					result = true;
				}
			}
			return result;
		}

		protected abstract TransactionCreatorHashtable PerformPostCore(InvoicingPostManager postManager);

		protected virtual bool PerformBackDating(InvoicingPostManager postManager, TransactionCreatorHashtable transactions) => true;

		bool GetOverrideDatesStatus(TransactionCreatorHashtable transactions, InvoicingPostManager postManager)
		{
			Argument.NotNull(transactions, "transactions");

			var changeTransactionDates = new ChangeTransactionDatesBusinessObjectBase(postManager.Factory);
			changeTransactionDates.PostDate = PostDateOverride;
			changeTransactionDates.InvoiceDate = InvoiceDateOverride;
			changeTransactionDates.RunPreSaveValidation();

			if (!changeTransactionDates.HasErrors)
			{
				postManager.ChangeTransactionDateOnAllARInvoicesAndCFXLinesAndUpdateChargesAndLinesExchangeRateForBackDating(InvoiceDateOverride, PostDateOverride, postManager.Factory);
				return true;
			}
			else
			{
				var collector = new ZNotificationCollector(changeTransactionDates, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);
				Notifications.AddError(collector.GetErrors().ToUniqueMessageListString());
				return false;
			}
		}

		bool PerformARCreditNoteLevelAuthorization(InvoicingPostManager postManager)
		{
			if (GuiProviderForARCreditNoteLevelAuthorization != null)
			{
				Argument.NotNull(postManager, "postManager");
				return postManager.PerformARCreditNoteLevelAuthorization(GuiProviderForARCreditNoteLevelAuthorization, true);
			}
			return true;
		}
	}
}
