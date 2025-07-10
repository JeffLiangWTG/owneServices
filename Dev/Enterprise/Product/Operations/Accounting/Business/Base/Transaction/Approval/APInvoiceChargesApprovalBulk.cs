using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.Aggregator;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Accounting.Business.TransactionApproval
{
	public class APInvoiceChargesApprovalBulk : TransactionApprovalBulk<InvoicingBase, APInvoiceChargesApprovalRequest, APInvoiceChargesApprovalRequestDetails>
	{
		public APInvoiceChargesApprovalBulk(BusinessObjectFactory factory, ISecurityOverrideProvider interactiveSecurityOverrideProvider, params APInvoiceChargesApprovalRequest[] approvalRequests)
			: base(factory, interactiveSecurityOverrideProvider, approvalRequests)
		{
		}

		public void PostApprovalsAndRemovePosted(IPostingJobAndTransactionApprovalGUIProvider postingGUIProvider)
		{
			PostApprovalsAndRemovePostedCore(postingGUIProvider);
		}

		public override bool IsChargeHidingApplied()
		{
			foreach (APInvoiceChargesApprovalRequest approvalRequest in Approvals)
			{
				if (approvalRequest.PostingDetails.Charges.Count > approvalRequest.PostingDetails.FilteredCharges.Count)
				{
					return true;
				}
			}
			return false;
		}

		protected override bool CheckLevelSecurityRights(InvoicingBase invoice)
		{
			return APInvoiceLevelAuthorizationWithApprovalRequest.CheckLevelSecurityRights(invoice);
		}

		protected override BusinessObject GetRequestParent(APInvoiceChargesApprovalRequest request, out ZString errorMessage)
		{
			BusinessObject parent = null;
			var errorMessages = new ZStringBuilder();
			if (request.IsTransactionRelated)
			{
				(parent, var restoreSavedDataResult) = request.GetLinkedInvoice();
				if (restoreSavedDataResult != null && restoreSavedDataResult.Result != InvoicingBase.RestoreSavedDataResult.ResultType.Success)
				{
					errorMessages.AppendLine(Res.GetString("A5B072D6-57E4-4731-9B95-2E85ADDC63E3", "Request ({0}): {1}.", request.FormatedRequestId, restoreSavedDataResult.Error));
				}
			}
			else if (request.IsJobRelated)
			{
				parent = request.Factory.Load<Job>(request.XP_ParentID);
			}
			else if (request.IsConsolRelated)
			{
				parent = request.Factory.Load<GenericConsol.GenericConsol>(request.XP_ParentID);
			}
			else
			{
				errorMessages.AppendLine(Res.GetString("A6F517F5-DF1C-49E5-AD12-1C4B9725E579", "Request ({0}) - reference type {1} is invalid.", request.FormatedRequestId, request.ReferenceType));
			}

			errorMessage = errorMessages.ToString();

			return parent;
		}

		protected override string PostNonTransactionRelatedRequest(APInvoiceChargesApprovalRequest request, IPostingTransactionApprovalGUIProvider postingGUIProvider, bool isBulkPosting)
		{
			var jobAndTransactionPostingGUIProvider = ((IPostingJobAndTransactionApprovalGUIProvider)postingGUIProvider);
			if (isBulkPosting)
			{
				var result = jobAndTransactionPostingGUIProvider.PostRequestInBulk(request, bulkPostingCache);
				bulkPostingCache = result.BulkPostingCache;

				return result.ErrorMessage;
			}
			else
			{
				return jobAndTransactionPostingGUIProvider.PostRequest(request);
			}
		}

		protected override void PrepareForPosting(APInvoiceChargesApprovalRequest request, InvoicingBase transaction, List<ITransactionParticipant> factoryList)
		{
			transaction.MultiPeriodApportionmentJournals.ForEach(journal => factoryList.Add(new AggregateWrapper(journal, journal)));
		}

		protected override void ValidateTransactionForBulkPosting(InvoicingBase transaction, IPostingTransactionApprovalGUIProvider postingGUIProvider, bool isBulkPosting)
		{
			base.ValidateTransactionForBulkPosting(transaction, postingGUIProvider, isBulkPosting);

			if (transaction.HasErrors())
			{
				return;
			}

			var jobAndTransactionPostingGUIProvider = ((IPostingJobAndTransactionApprovalGUIProvider)postingGUIProvider);
			using (transaction.TemporarySetProvider(isBulkPosting ? null : jobAndTransactionPostingGUIProvider.GetNewSecurityOverrideProviderForPosting(transaction)))
			{
				transaction.SetContext(BusinessContext.ValidateTransactionForApproveAndPost);
				new InvoicingPreSaveHelper().PreSaveActions(transaction, jobAndTransactionPostingGUIProvider, isBulkPosting);
				transaction.RemoveContext(BusinessContext.ValidateTransactionForApproveAndPost);
			}
		}

		protected override void PrepareForAuthorization(APInvoiceChargesApprovalRequest requestInNewFactory, InvoicingBase transaction)
		{
			transaction.MoveFromIncompleteToPayableLedger();
		}

		protected override LevelAuthorizationWithApprovalRequest<InvoicingBase, APInvoiceChargesApprovalRequest, APInvoiceChargesApprovalRequestDetails> GetLevelAuthorizationWithApprovalRequest(IPostingTransactionApprovalGUIProvider postingGUIProvider, InvoicingBase transaction)
			=> new APInvoiceLevelAuthorizationWithApprovalRequest(postingGUIProvider, transaction, false);

		protected override bool PerformLevelAuthorization(LevelAuthorizationWithApprovalRequest<InvoicingBase, APInvoiceChargesApprovalRequest, APInvoiceChargesApprovalRequestDetails> levelAuthorization)
			=> ((APInvoiceLevelAuthorizationWithApprovalRequest)levelAuthorization).PerformLevelAuthorization(true);

		protected override void FinalizePosting(IEnumerable<ZGuid> postedTransactions, IPostingTransactionApprovalGUIProvider postingGUIProvider, bool isBulkPosting)
		{
			if (bulkPostingCache != null)
			{
				bulkPostingCache.FinalizePosting(postedTransactions);
			}
			else if (postedTransactions.Any())
			{
				var jobAndTransactionPostingGUIProvider = ((IPostingJobAndTransactionApprovalGUIProvider)postingGUIProvider);
				if (isBulkPosting)
				{
					jobAndTransactionPostingGUIProvider.BulkPrintAPInvoicesAndCreditNotes(postedTransactions.ToArray());
				}
				else
				{
					jobAndTransactionPostingGUIProvider.PrintAPInvoiceAndCreditNote(postedTransactions.First());
				}
			}
		}

		IBulkPostingCache bulkPostingCache;
	}
}
