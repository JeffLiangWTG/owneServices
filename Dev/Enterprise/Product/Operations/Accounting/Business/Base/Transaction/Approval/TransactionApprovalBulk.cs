using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Core;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Accounting.Business.AccountingConstants;

namespace Enterprise.Accounting.Business.TransactionApproval
{
	public abstract class TransactionApprovalBulk<TransactionType, RequestType, DetailsType> : NonPersistentBusinessObject, IObsoleteValidation
		where TransactionType : TransactionHeader
		where RequestType : TransactionApprovalRequest<DetailsType>
		where DetailsType : ApprovalRequestDetails
	{
		public TransactionApprovalBulk(BusinessObjectFactory factory, ISecurityOverrideProvider interactiveSecurityOverrideProvider, params RequestType[] approvalRequests)
			: base(factory)
		{
			this.approvalRequests = approvalRequests;
			this.InteractiveSecurityOverrideProvider = interactiveSecurityOverrideProvider;
		}

		readonly RequestType[] approvalRequests;

		protected void PostApprovalsAndRemovePostedCore(IPostingTransactionApprovalGUIProvider postingGUIProvider)
		{
			var postedTransactions = new List<ZGuid>(Approvals.Count);
			var isBulkPosting = Approvals.Count > 1;
			foreach (var originalRequest in Approvals)
			{
				var errorMessage = string.Empty;
				var requestInNewFactory = new BusinessObjectFactory().Load<RequestType>(originalRequest.PK);
				if (requestInNewFactory.XP_ApprovalStatus != Constants.GenApprovalRequestApprovalStatus.Approved)
				{
					errorMessage = Res.GetString("8C2F4889-4D30-4533-B565-9C53B28B0FCA", "This request can’t be posted. Request status is ‘{0}’ now. Only approved request can be posted.", requestInNewFactory.XP_ApprovalStatus);
				}
				else
				{
					ZString linkedTransactionErrorMessage;
					var requestParent = GetRequestParent(requestInNewFactory, out linkedTransactionErrorMessage);
					try
					{
						if (requestParent == null)
						{
							if (!linkedTransactionErrorMessage.IsEmpty)
							{
								var approvedMessage = Res.GetString("4978e04b-32c4-4e90-9da2-54610d7699e4", "This request is approved.");
								errorMessage = string.Join(" ", approvedMessage, linkedTransactionErrorMessage.ToString());
							}
							else
							{
								errorMessage = Res.GetString("3C1AA049-7D51-422B-B69E-6CA8270ACBB0", "This request is approved, but related {0} cannot be found.", requestInNewFactory.ReferenceType.ToLower());
							}
						}
						else
						{
							var transaction = requestParent as TransactionType;
							if (transaction == null)
							{
								errorMessage = PostNonTransactionRelatedRequest(requestInNewFactory, postingGUIProvider, isBulkPosting);
							}
							else
							{
								ValidateTransactionForBulkPosting(transaction, postingGUIProvider, isBulkPosting);
								if (transaction.HasErrors)
								{
									errorMessage = Res.GetString("796187AE-A2FE-467A-928B-E3B9D835E172", "This request is approved, but related {0} has validation errors.\r\n{1}", requestInNewFactory.ReferenceType.ToLower(),
										string.Join("\r\n", new ZNotificationCollector(transaction, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetErrors().GetUniqueMessageList()));
								}
								else
								{
									var newPostingGUIProvider = new PostingGUIProvider(postingGUIProvider);
									var levelAuthorization = GetLevelAuthorizationWithApprovalRequest(newPostingGUIProvider, transaction);
#if DEBUG
									levelAuthorization.GetNewHelper_ForTestOnly = GetNewHelperForLevelAuthorization_ForTestOnly;
#endif

									PrepareForAuthorization(requestInNewFactory, transaction);

									var postAuthorizationResult = PerformLevelAuthorization(levelAuthorization);

									if (postAuthorizationResult)
									{
										try
										{
											var factoryList = new List<ITransactionParticipant>(new[] { newPostingGUIProvider.FactoryForApprovalRequests, transaction.Factory }); //request should be saved before transaction, so new transaction on saving can set its number to the request

											PrepareForPosting(requestInNewFactory, transaction, factoryList);

											BusinessObjectFactory.SaveTogether(factoryList.ToArray());

											postedTransactions.Add(transaction.PK);
										}
										catch (ZSaveException ex)
										{
											errorMessage = ex.Message;
										}
									}
									else
									{
										errorMessage = string.Join("\r\n", newPostingGUIProvider.ErrorMessages);
									}
								}
							}
						}
					}
					catch (ExternalStorageException)
					{
						errorMessage = AllocateErrorMessage.LastExternalStorageExceptionMessage;
					}
					finally
					{
						var disposable = requestParent as IDisposable;
						if (disposable != null)
						{
							disposable.Dispose();
						}
					}
				}

				if (!string.IsNullOrEmpty(errorMessage))
				{
					originalRequest.AddRowError(errorMessage);
				}
				else
				{
					Approvals.RemoveFromRelationship(originalRequest);
				}
			}

			FinalizePosting(postedTransactions, postingGUIProvider, isBulkPosting);
		}

#if DEBUG
		[SuppressMessage("Microsoft.Usage", "CA2211: Non-constant fields should not be visible", Justification = "For test only")]
		public Func<RequestType, ITransactionApprovalHelper> GetNewHelperForLevelAuthorization_ForTestOnly;
#endif

		protected virtual void FinalizePosting(IEnumerable<ZGuid> postedTransactions, IPostingTransactionApprovalGUIProvider postingGUIProvider, bool isBulkPosting)
		{
		}

		protected virtual void ValidateTransactionForBulkPosting(TransactionType transaction, IPostingTransactionApprovalGUIProvider postingGUIProvider, bool isBulkPosting)
		{
			transaction.RunPreSaveValidation();
		}

		protected virtual string PostNonTransactionRelatedRequest(RequestType request, IPostingTransactionApprovalGUIProvider postingGUIProvider, bool isBulkPosting)
		{
			throw new NotImplementedException();
		}

		protected virtual LevelAuthorizationWithApprovalRequest<TransactionType, RequestType, DetailsType> GetLevelAuthorizationWithApprovalRequest(IPostingTransactionApprovalGUIProvider postingGUIProvider, TransactionType transaction)
		{
			throw new NotImplementedException();
		}

		protected virtual void PrepareForAuthorization(RequestType requestInNewFactory, TransactionType transaction)
		{
		}

		protected virtual void PrepareForPosting(RequestType request, TransactionType transaction, List<ITransactionParticipant> factoryList)
		{
		}

		protected virtual bool PerformLevelAuthorization(LevelAuthorizationWithApprovalRequest<TransactionType, RequestType, DetailsType> levelAuthorization)
		{
			throw new NotImplementedException();
		}

		[SuppressMessage("Microsoft.Design", "CA1021: Avoid out parameters")]
		protected virtual BusinessObject GetRequestParent(RequestType request, out ZString errorMessage)
		{
			throw new NotImplementedException();
		}

		protected virtual bool SetApprovingUserAndStatus(bool isActionAllowed, bool isCurrentUserRequestSelected, string status, TransactionType transactionToApprove, bool needToUpdateApproverOneForSequentialMode = false)
		{
			var securityCheckResult = CheckSecurityRightsAndGetApprovingUser(isActionAllowed, isCurrentUserRequestSelected, transactionToApprove);

			if (securityCheckResult.isActionAllowed)
			{
				foreach (var approvalRequest in Approvals)
				{
					var applicableStatus = GetApplicableApprovalStatus(approvalRequest, status);
					approvalRequest.XP_ApprovalStatus = applicableStatus;
					if (applicableStatus == Constants.GenApprovalRequestApprovalStatus.Approved ||
						applicableStatus == Constants.GenApprovalRequestApprovalStatus.ApprovalRequested ||
						applicableStatus == Constants.GenApprovalRequestApprovalStatus.Rejected ||
						applicableStatus == Constants.GenApprovalRequestApprovalStatus.RejectionRequested)
					{
						approvalRequest.UpdateApprovalUserAndStatus(securityCheckResult.approvingUser, applicableStatus);
					}
					if (applicableStatus == Constants.GenApprovalRequestApprovalStatus.Cancelled ||
						applicableStatus == Constants.GenApprovalRequestApprovalStatus.Rejected)
					{
						approvalRequest.FinalizeCancelling();
					}
				}
			}

			return securityCheckResult.isActionAllowed;
		}

		protected virtual string GetApplicableApprovalStatus(RequestType approvalRequest, string status) => status;

		protected (bool isActionAllowed, string approvingUser) CheckSecurityRightsAndGetApprovingUser(bool isActionAllowed, bool isCurrentUserRequestSelected, TransactionType transactionToApprove)
		{
			string approvingUser = null;

			if (!isActionAllowed &&
					(!isCurrentUserRequestSelected || AllowToApproveOwnRequest))
			{
				SecurityOverrideProviderSource.Get(transactionToApprove).Provider = InteractiveSecurityOverrideProvider;
				if (CheckLevelSecurityRights(transactionToApprove))
				{
					approvingUser = GlbStaff.CurrentUser.GS_Code;
					if (SecurityOverrideProviderSource.Get(transactionToApprove).Provider.UserSecurityOverride != null)
					{
						approvingUser = Factory.GetCachedReadOnlyFactory().Load<GlbStaff>(SecurityOverrideProviderSource.Get(transactionToApprove).Provider.UserSecurityOverride.UserPK).GS_Code;
					}
					isActionAllowed = true;
				}
				SecurityOverrideProviderSource.Get(transactionToApprove).Provider = null;
			}

			return (isActionAllowed, approvingUser);
		}

		protected readonly ISecurityOverrideProvider InteractiveSecurityOverrideProvider;
		protected abstract bool CheckLevelSecurityRights(TransactionType invoice);

		public ActiveBusinessObjectCollection<RequestType> Approvals
		{
			get
			{
				if (approvals == null)
				{
					approvals = new ActiveBusinessObjectCollection<RequestType>(Factory, new AdhocCollectionRelationship(typeof(RequestType)));
					approvals.AddRange(approvalRequests);
					RegisterEditableChildObject(approvals);
				}

				return approvals;
			}
		}
		ActiveBusinessObjectCollection<RequestType> approvals;

		public virtual bool IsChargeHidingApplied() => false;

		public bool Approve(bool needToUpdateApproverOneForSequentialMode = false) =>
			ChangeStatus(Constants.GenApprovalRequestApprovalStatus.Approved, needToUpdateApproverOneForSequentialMode);

		public bool Reject() =>
			ChangeStatus(Constants.GenApprovalRequestApprovalStatus.Rejected);

		public bool RequestRejection() =>
			ChangeStatus(Constants.GenApprovalRequestApprovalStatus.RejectionRequested);

		public bool Cancel()
		{
			var result = ChangeStatus(Constants.GenApprovalRequestApprovalStatus.Cancelled);
			if (result)
			{
				Approvals.ToList().ForEach(x => x.SetContext(BusinessContext.CancelApprovalRequestByUser));
			}

			return result;
		}

		bool ChangeStatus(string status, bool needToUpdateApproverOneForSequentialMode = false)
		{
			bool isActionAllowed = false;

			if (Approvals.Count > 0)
			{
				string systemCreateUser = Approvals[0].XP_SystemCreateUser;
				bool isCurrentUserRequestSelected = false;
				foreach (RequestType approvalRequest in Approvals)
				{
					if (systemCreateUser != approvalRequest.XP_SystemCreateUser)
					{
						systemCreateUser = null;
					}
					if (approvalRequest.XP_SystemCreateUser == GlbStaff.CurrentUser.GS_Code)
					{
						isCurrentUserRequestSelected = true;
					}
					if (systemCreateUser == null && isCurrentUserRequestSelected)
					{
						break;
					}
				}

				var transactionToApprove = GetTransactionWithHighestApprovalLevel();
				transactionToApprove.IsCreatedFromApprovalRequest = true;
				if (status == Constants.GenApprovalRequestApprovalStatus.Cancelled)
				{
					isActionAllowed = GlbStaff.CurrentUser.GS_Code == systemCreateUser;
				}
				isActionAllowed = SetApprovingUserAndStatus(isActionAllowed, isCurrentUserRequestSelected, status, transactionToApprove, needToUpdateApproverOneForSequentialMode);
			}

			return isActionAllowed;
		}

		protected virtual bool AllowToApproveOwnRequest
		{
			get { return true; }
		}

		public string NotAllowedToApproveMessage
		{
			get
			{
				var message = string.Empty;

				if (Approvals.Any(x => !x.IsAllowedToApproveRequest))
				{
					if (Approvals.Count == 1)
					{
						message = Res.GetString("6664702d-2d53-4b62-bcd7-3f6d4b908032", @"This request cannot be approved until all related requests are in Approved status.
Please approve the related requests first.");
					}
					else if (Approvals.Count > 1)
					{
						message = Res.GetString("c974c346-7e9d-44de-86e5-1301192b5546", @"One of the selected requests is a parent request and it cannot be approved until all related requests are in Approved status.
Please approve the related requests first.");
					}
				}

				return message;
			}
		}

		public string NotAllowedToCancelMessage
		{
			get
			{
				var message = string.Empty;

				if (Approvals.Any(x => !x.IsAllowedToCancelRequest))
				{
					if (Approvals.Count == 1)
					{
						message = Res.GetString("70473a77-522d-4f99-883a-31aa5d63482b", @"This request cannot be canceled as it relates to another request. You can either reject or approve this request.
To cancel all requests in this group, cancel the parent request.");
					}
					else if (Approvals.Count > 1)
					{
						message = Res.GetString("7d8374cc-3923-445d-a23a-b95c6d632342", @"One of the selected requests is related to another request and it cannot be canceled directly. You can either reject or approve this request.
To cancel all requests in this group, cancel the parent request.");
					}
				}

				return message;
			}
		}

		protected virtual TransactionType GetTransactionWithHighestApprovalLevel()
		{
			ZDecimal maxAmountToApprove = 0M;
			foreach (var approvalRequest in Approvals)
			{
				if (approvalRequest.PostingDetails.MaxAmountToApprove > maxAmountToApprove)
				{
					maxAmountToApprove = approvalRequest.PostingDetails.MaxAmountToApprove;
				}
			}

			var transactionToApprove = (TransactionType)Factory.GetCachedReadOnlyFactory().New(Approvals[0].ApprovingTransactionType);
			transactionToApprove.AH_LocalExTaxAmount = maxAmountToApprove;
			transactionToApprove.AH_LocalTaxAmount = 0M;

			return transactionToApprove;
		}

		class PostingGUIProvider : IPostingTransactionApprovalGUIProvider
		{
			public PostingGUIProvider(IPostingTransactionApprovalGUIProvider baseProvider)
			{
				this.baseProvider = baseProvider;

				ErrorMessages = new List<string>();
			}

			readonly IPostingTransactionApprovalGUIProvider baseProvider;

			public readonly List<string> ErrorMessages;

			public bool IsBulkPosting
			{
				get { throw new NotImplementedException(); }
			}

			public ZDialogResult ShowMessage(string messageText, string messageCaption, ZMessageBoxButtons messageBoxButtons, ZMessageBoxIcon messageBoxIcon, ZDialogResult dialogResult)
			{
				ErrorMessages.Add(messageText);
				return dialogResult;
			}

			public ZDialogResult ShowApprovalFormToSetDescription(GenApprovalRequest approvingRequest)
			{
				throw new NotImplementedException();
			}

			public void NotifyBulkPostingIsNotAuthorized(string message)
			{
				throw new NotImplementedException();
			}

			public BusinessObjectFactory FactoryForApprovalRequests
			{
				get { return baseProvider.FactoryForApprovalRequests; }
			}

			public void ResetFactoryForApprovalRequests()
			{
				baseProvider.ResetFactoryForApprovalRequests();
			}

			public void RollbackPosting()
			{
				throw new NotImplementedException();
			}

			public ISecurityOverrideProviderWithApprovalRequest GetNewSecurityOverrideProvider(bool showApprovalRequestButton, bool supportMultipleApprover = false)
			{
				return baseProvider.GetNewSecurityOverrideProvider(showApprovalRequestButton, supportMultipleApprover);
			}

			public ISecurityOverrideProviderWithApprovalRequest GetNewSecurityOverrideProviderForARCreditNote(bool showApprovalRequestButton, bool supportMultipleApprover = false, ARCreditNoteApprovalRequest[] approvalRequests = null)
			{
				throw new NotImplementedException();
			}

			public Tuple<ZGuid, ZString> GetParentIdAndTableCodeForJobPostingAction()
			{
				throw new NotImplementedException();
			}

			public Tuple<ZString, ZString> ShowCreditNoteReversalReasonForm(string existingReasonCode)
			{
				throw new NotImplementedException();
			}

			public JobInvoicingPostingOption PostingOption
			{
				get { throw new NotImplementedException(); }
			}

#if DEBUG
			public bool ShowLoginFormForTest
			{
				get { return baseProvider.ShowLoginFormForTest; }
			}

			public string SecurityItemForTest
			{
				get { return baseProvider.SecurityItemForTest; }
			}
#endif
		}
	}
}
