using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.TransactionApproval
{
	public class ARCreditNoteApprovalHelper : TransactionApprovalHelper<ARCreditNoteApprovalRequest, ARCreditNoteApprovalRequestDetails>
	{
		public ARCreditNoteApprovalHelper(ARCreditNoteApprovalRequest approvingRequest)
			: base(approvingRequest, (x, y) => new ARCreditNoteApprovalRequestCollection(x, y))
		{
		}

		public void DeleteNewReqeustCreatedForApprovedRequest()
		{
			approvingRequest.Delete();
		}

		public ZGuid ParentId => approvingRequest.XP_ParentID;
	}

	public class APInvoiceChargesApprovalHelper : TransactionApprovalHelper<APInvoiceChargesApprovalRequest, APInvoiceChargesApprovalRequestDetails>
	{
		public APInvoiceChargesApprovalHelper(APInvoiceChargesApprovalRequest approvingRequest)
			: base(approvingRequest, (x, y) => new APInvoiceChargesApprovalRequestCollection(x, y))
		{
		}

		public APInvoiceChargesApprovalRequest[] GetCanceledRequestedAndApprovedRequestsNotInThisPostingAction(APInvoiceChargesApprovalRequest[] requestsForThisPostingAction)
		{
			var foundRequests = new List<APInvoiceChargesApprovalRequest>();

			Action<IEnumerable<APInvoiceChargesApprovalRequest>> findRequestsNotInThisPostingAction = requests =>
			{
				foreach (var request in requests)
				{
					if (!requestsForThisPostingAction.Any(x => IsPostingActionTheSame(x, request)))
					{
						foundRequests.Add(request);
					}
				}
			};

			findRequestsNotInThisPostingAction(ApprovalRequestsForThisParentIDWithRequestedOrErrorStatus);
			findRequestsNotInThisPostingAction(ApprovedRequestsForThisParentID);

			foundRequests.ForEach(x => x.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Cancelled);

			return foundRequests.ToArray();
		}
	}

	public class GLJournalApprovalHelper : TransactionApprovalHelper<GLJournalApprovalRequest, GLJournalApprovalRequestDetails>
	{
		public GLJournalApprovalHelper(GLJournalApprovalRequest approvingRequest, ZString approvingJournalDescription)
			: base(approvingRequest, (x, y) => new GLJournalApprovalRequestCollection(x, y))
		{
			journalDescription = approvingJournalDescription;
		}

		readonly ZString journalDescription;
		protected override ZQuery GetRequestForThisParentIDQuery(params string[] status)
		{
			var approvalFilter = new ZQuery() { IsNoResultQuery = true };
			if (approvingRequest.XP_ParentID.IsEmpty)
			{
				if (!approvingRequest.LatestLinkedApprovalRequestInDBPK.IsEmpty)
				{
					approvalFilter = new ZQuery(GenApprovalRequestSchema.PK, SQLComparisonOperator.Equal, approvingRequest.LatestLinkedApprovalRequestInDBPK);
					approvalFilter.AddToFilter(GenApprovalRequestSchema.XP_ApprovalStatus, status);
				}
			}
			else
			{
				approvalFilter = base.GetRequestForThisParentIDQuery(status);
			}

			return approvalFilter;
		}

		protected override void PrepareToPost(GLJournalApprovalRequest approvalRequestForPosting)
		{
			approvalRequestForPosting.PrepareToPost(approvingRequest.OriginalJournalPK, journalDescription);
		}
	}

	public interface ITransactionApprovalHelper
	{
		bool IsThereApprovedRequestForThisPostingDetails { get; }
		bool IsThereApprovedRequestForThisPostingActionButAnotherPostingDetails { get; }
		string GetComparisonDataWithTheOtherPostingDetails();
		bool CancelApprovedRequestForThisPostingActionButAnotherPostingDetails();
		void ApproveAndPostRequest(ISecurityOverrideProvider securityOverrideProvider);
		GenApprovalRequest PostApprovedRequestForThisPostingDetails();
		bool AreThereAnyApprovalRequestForThisPostingActionWithRequestedStatus { get; }
		bool CancelApprovalRequestForThisPostingActionWithRequestedOrErrorStatus();
	}

	public interface ITransactionApprovalHelper<RequestType, DetailsType> : ITransactionApprovalHelper
		where RequestType : TransactionApprovalRequest<DetailsType>
		where DetailsType : ApprovalRequestDetails
	{
		bool IsRequestForTheSamePostingActionAndDetails(RequestType externalRequest);
	}

	public abstract class TransactionApprovalHelper<RequestType, DetailsType> : ITransactionApprovalHelper<RequestType, DetailsType>
		where RequestType : TransactionApprovalRequest<DetailsType>
		where DetailsType : ApprovalRequestDetails
	{
		protected TransactionApprovalHelper(RequestType approvingRequest, Func<BusinessObjectFactory, ZQuery, TransactionApprovalRequestCollection<RequestType>> createApprovalRequestCollection)
		{
			Argument.NotNull(approvingRequest, nameof(approvingRequest));
			Argument.NotNull(createApprovalRequestCollection, nameof(createApprovalRequestCollection));

			this.approvingRequest = approvingRequest;
			this.createApprovalRequestCollection = createApprovalRequestCollection;
		}

		protected readonly RequestType approvingRequest;
		readonly Func<BusinessObjectFactory, ZQuery, TransactionApprovalRequestCollection<RequestType>> createApprovalRequestCollection;

		#region Implementation

		bool ITransactionApprovalHelper.IsThereApprovedRequestForThisPostingDetails
		{
			get
			{
				return (approvingRequest.IsInDatabase && approvingRequest.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Approved) ||
					(ApprovedRequestForThisPostingActionInDB != null && ApprovedRequestForThisPostingActionInDB.ArePostingDetailsTheSame(approvingRequest));
			}
		}

		bool ITransactionApprovalHelper.IsThereApprovedRequestForThisPostingActionButAnotherPostingDetails
		{
			get
			{
				return ApprovedRequestForThisPostingActionInDB != null && !ApprovedRequestForThisPostingActionInDB.ArePostingDetailsTheSame(approvingRequest);
			}
		}

		public string GetComparisonDataWithTheOtherPostingDetails()
		{
			var builder = new StringBuilder();

			builder.AppendLine($"<!-- Database (Old) -->");
			builder.AppendLine(Encoding.Unicode.GetString(ApprovedRequestForThisPostingActionInDB.XP_ApprovalRequestData).TrimWithUnicodeWhitespace());

			approvingRequest.SerializePostingDetails();

			builder.AppendLine($"<!-- Control (New) -->");
			builder.AppendLine(Encoding.Unicode.GetString(approvingRequest.XP_ApprovalRequestData).TrimWithUnicodeWhitespace());

			return builder.ToString();
		}

		bool ITransactionApprovalHelper.CancelApprovedRequestForThisPostingActionButAnotherPostingDetails()
		{
			bool result = false;
			if (((ITransactionApprovalHelper)this).IsThereApprovedRequestForThisPostingActionButAnotherPostingDetails)
			{
				ApprovedRequestForThisPostingActionInDB.SetContext(BusinessContext.CancelApprovalRequestDueToUpdatingLinkedTransaction);
				ApprovedRequestForThisPostingActionInDB.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Cancelled;
				result = true;
			}

			return result;
		}

		public GenApprovalRequest PostApprovedRequestForThisPostingDetails()
		{
			RequestType approvalRequestForPosting = null;
			if (approvingRequest.IsInDatabase && approvingRequest.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Approved)
			{
				approvalRequestForPosting = approvingRequest;
			}
			else if (ApprovedRequestForThisPostingActionInDB != null)
			{
				approvalRequestForPosting = ApprovedRequestForThisPostingActionInDB;
			}

			if (approvalRequestForPosting != null)
			{
				PostRequest(approvalRequestForPosting);
			}

			return approvalRequestForPosting;
		}

		void ITransactionApprovalHelper.ApproveAndPostRequest(ISecurityOverrideProvider securityOverrideProvider)
		{
			approvingRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			var securityOverride = securityOverrideProvider != null ? securityOverrideProvider.UserSecurityOverride : null;
			approvingRequest.XP_GS_NKApprovingUser1 = securityOverride != null ? approvingRequest.Factory.GetCachedReadOnlyFactory().Load<GlbStaff>(securityOverride.UserPK).GS_Code : GlbStaff.CurrentUser.GS_Code;
			approvingRequest.XP_ApprovalDate = ZDateTime.Now;
			PostRequest(approvingRequest);
		}

		void PostRequest(RequestType approvalRequestForPosting)
		{
			PrepareToPost(approvalRequestForPosting);
			approvalRequestForPosting.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted;
		}

		protected virtual void PrepareToPost(RequestType approvalRequestForPosting)
		{
		}

		bool ITransactionApprovalHelper.AreThereAnyApprovalRequestForThisPostingActionWithRequestedStatus
		{
			get { return ApprovalRequstForThisPostingActionWithRequestedOrErrorStatusInDB != null && ApprovalRequstForThisPostingActionWithRequestedOrErrorStatusInDB.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Requested; }
		}

		bool ITransactionApprovalHelper.CancelApprovalRequestForThisPostingActionWithRequestedOrErrorStatus()
		{
			var result = false;
			if (ApprovalRequstForThisPostingActionWithRequestedOrErrorStatusInDB != null)
			{
				ApprovalRequstForThisPostingActionWithRequestedOrErrorStatusInDB.SetContext(BusinessContext.CancelApprovalRequestDueToUpdatingLinkedTransaction);
				ApprovalRequstForThisPostingActionWithRequestedOrErrorStatusInDB.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Cancelled;
				result = true;
			}

			return result;
		}

		protected internal TransactionApprovalRequestCollection<RequestType> ApprovedRequestsForThisParentID
		{
			get
			{
				if (approvedRequestsForThisParentID == null)
				{
					var approvalFilter = GetRequestForThisParentIDQuery(Constants.GenApprovalRequestApprovalStatus.Approved);
					approvedRequestsForThisParentID = CreateApprovalRequestCollection(approvingRequest.Factory, approvalFilter);
				}

				return approvedRequestsForThisParentID;
			}
		}
		TransactionApprovalRequestCollection<RequestType> approvedRequestsForThisParentID;

		protected TransactionApprovalRequestCollection<RequestType> ApprovalRequestsForThisParentIDWithRequestedOrErrorStatus
		{
			get
			{
				if (approvalRequestsForThisParentIDWithRequestedOrErrorStatus == null)
				{
					var approvalFilter = GetRequestForThisParentIDQuery(Constants.GenApprovalRequestApprovalStatus.Requested, Constants.GenApprovalRequestApprovalStatus.Error);
					approvalRequestsForThisParentIDWithRequestedOrErrorStatus = CreateApprovalRequestCollection(approvingRequest.Factory, approvalFilter);
				}

				return approvalRequestsForThisParentIDWithRequestedOrErrorStatus;
			}
		}
		TransactionApprovalRequestCollection<RequestType> approvalRequestsForThisParentIDWithRequestedOrErrorStatus;

		protected virtual ZQuery GetRequestForThisParentIDQuery(params string[] status)
		{
			var approvalFilter = new ZQuery(GenApprovalRequestSchema.PK, SQLComparisonOperator.NotEqual, approvingRequest.PK);
			approvalFilter.AddToFilter(GenApprovalRequestSchema.XP_ParentID, approvingRequest.XP_ParentID);
			approvalFilter.AddToFilter(GenApprovalRequestSchema.XP_ApprovalStatus, status);
			approvalFilter.IsNoResultQuery = approvingRequest.XP_ParentID.IsEmpty;

			return approvalFilter;
		}

		TransactionApprovalRequestCollection<RequestType> CreateApprovalRequestCollection(BusinessObjectFactory factory, ZQuery query)
		{
			return createApprovalRequestCollection(factory, query);
		}

		RequestType ApprovedRequestForThisPostingActionInDB
		{
			get
			{
				if (approvedRequestForThisPostingActionInDB == null ||
					approvedRequestForThisPostingActionInDB.IsDeleted ||
					!ApprovedRequestsForThisParentID.Contains(approvedRequestForThisPostingActionInDB))
				{
					approvedRequestForThisPostingActionInDB = null;
					foreach (RequestType approval in ApprovedRequestsForThisParentID)
					{
						if (IsPostingActionTheSame(approvingRequest, approval))
						{
							approvedRequestForThisPostingActionInDB = approval;
							break;
						}
					}
				}

				return approvedRequestForThisPostingActionInDB;
			}
		}
		RequestType approvedRequestForThisPostingActionInDB;

		RequestType ApprovalRequstForThisPostingActionWithRequestedOrErrorStatusInDB
		{
			get
			{
				if (approvalRequstForThisPostingActionWithRequestedOrErrorStatusInDB == null ||
					approvalRequstForThisPostingActionWithRequestedOrErrorStatusInDB.IsDeleted ||
					!ApprovalRequestsForThisParentIDWithRequestedOrErrorStatus.Contains(approvalRequstForThisPostingActionWithRequestedOrErrorStatusInDB))
				{
					approvalRequstForThisPostingActionWithRequestedOrErrorStatusInDB = null;
					foreach (RequestType approval in ApprovalRequestsForThisParentIDWithRequestedOrErrorStatus)
					{
						if (IsPostingActionTheSame(approvingRequest, approval))
						{
							approvalRequstForThisPostingActionWithRequestedOrErrorStatusInDB = approval;
							break;
						}
					}
				}

				return approvalRequstForThisPostingActionWithRequestedOrErrorStatusInDB;
			}
		}
		RequestType approvalRequstForThisPostingActionWithRequestedOrErrorStatusInDB;

		protected static bool IsPostingActionTheSame(RequestType approval1, RequestType approval2)
		{
			return approval1.IsPostingActionTheSame(approval2);
		}

		bool ITransactionApprovalHelper<RequestType, DetailsType>.IsRequestForTheSamePostingActionAndDetails(RequestType externalRequest)
		{
			return approvingRequest.XP_ParentID == externalRequest.XP_ParentID
				&& IsPostingActionTheSame(approvingRequest, externalRequest)
				&& approvingRequest.ArePostingDetailsTheSame(externalRequest);
		}

		#endregion
	}
}
