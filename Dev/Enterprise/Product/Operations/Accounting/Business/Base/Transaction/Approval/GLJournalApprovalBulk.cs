using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.Aggregator;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Accounting.Business.TransactionApproval
{
	public class GLJournalApprovalBulk : TransactionApprovalBulk<GLJournal, GLJournalApprovalRequest, GLJournalApprovalRequestDetails>
	{
		public GLJournalApprovalBulk(BusinessObjectFactory factory, ISecurityOverrideProvider interactiveSecurityOverrideProvider, params GLJournalApprovalRequest[] approvalRequests)
			: base(factory, interactiveSecurityOverrideProvider, approvalRequests)
		{
		}

		public void PostApprovalsAndRemovePosted(IPostingTransactionApprovalGUIProvider postingGUIProvider)
		{
			PostApprovalsAndRemovePostedCore(postingGUIProvider);
		}

		protected override bool AllowToApproveOwnRequest
		{
			get { return AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.Value; }
		}

		protected override bool CheckLevelSecurityRights(GLJournal journal)
		{
			return GLJournalLevelAuthorizationWithApprovalRequest.CheckLevelSecurityRights(journal);
		}

		protected override GLJournal GetTransactionWithHighestApprovalLevel()
		{
			GLJournal highestApporvalLevelJournal = null;
			int maxSecurityWeight = -1;
			int requestCount = 0;
			foreach (var approvalRequest in Approvals)
			{
				GLJournal journal;
				if (requestCount++ == 0)
				{
					//This is to avoid unnecessary journal deserialization. 
					//First request PostingDetails.Journal will be loaded on form binding anyway, so there is no sense to load it separately through GetLinkedJournal.
					journal = approvalRequest.PostingDetails.Journal;
				}
				else
				{
					//Loading of other request Journals will depend on user activity in a form grid. If user will not select next grid rows, they will not be loaded in memory then.
					//This is compromise between speed and memory allocation.
					//So next journals in this loop are loaded using GetLinkedJournal to be able to be released from memory after this maximum approval level calculation.
					journal = approvalRequest.GetLinkedJournal(new BusinessObjectFactory { RefreshEnabled = false }).journal;
				}
				var security = GLJournalApprovalAuthorizationHelper.RequiredSecurityCheckPoint(journal);
				var securityWeight = GetSecurityCheckpointWeight(security);
				if (securityWeight > maxSecurityWeight)
				{
					maxSecurityWeight = securityWeight;
					highestApporvalLevelJournal = journal;
				}
				if (maxSecurityWeight == 3)
				{
					break;
				}
			}

			return highestApporvalLevelJournal ?? base.GetTransactionWithHighestApprovalLevel();
		}

		static int GetSecurityCheckpointWeight(SecurityCheckpoint checkpoint)
		{
			if (checkpoint == Env.Security.GeneralLedgerJournal_FirstApproval)
			{
				return 1;
			}

			if (checkpoint == Env.Security.GeneralLedgerJournal_SecondApproval)
			{
				return 2;
			}

			if (checkpoint == Env.Security.GeneralLedgerJournal_ThirdApproval)
			{
				return 3;
			}

			return 0;
		}

		protected override BusinessObject GetRequestParent(GLJournalApprovalRequest request, out ZString errorMessage)
		{
			var result = request.GetLinkedJournal(loadJournalWithMutex: true);
			errorMessage = result.errorMessage;

			return result.journal;
		}

		protected override void PrepareForPosting(GLJournalApprovalRequest request, GLJournal transaction, List<ITransactionParticipant> factoryList)
		{
			transaction.IsTopLevel = true;//Journal will be added logs and set last edit user and time when IsTopLevel true, this simulate saving journal with an individual form

			var originalFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var originalJournal = originalFactory.Load(typeof(GLJournal), transaction.PK) as GLJournal;
			if (originalJournal != null)
			{
				originalJournal.GLJournalLines.Load();
			}

			var aggregator = new AggregateWrapper(transaction, originalJournal ?? transaction);
			factoryList.Add(aggregator);

			transaction.RelinkEDocsFromRequestToNewJournal(request);
		}

		protected override LevelAuthorizationWithApprovalRequest<GLJournal, GLJournalApprovalRequest, GLJournalApprovalRequestDetails> GetLevelAuthorizationWithApprovalRequest(IPostingTransactionApprovalGUIProvider postingGUIProvider, GLJournal transaction)
			=> new GLJournalLevelAuthorizationWithApprovalRequest(postingGUIProvider, transaction, false);

		protected override bool PerformLevelAuthorization(LevelAuthorizationWithApprovalRequest<GLJournal, GLJournalApprovalRequest, GLJournalApprovalRequestDetails> levelAuthorization)
			=> ((GLJournalLevelAuthorizationWithApprovalRequest)levelAuthorization).PerformLevelAuthorization(true);
	}
}
