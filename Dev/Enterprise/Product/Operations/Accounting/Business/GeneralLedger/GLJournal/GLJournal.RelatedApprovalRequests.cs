using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals
{
	public partial class GLJournal
	{
		[ResourceStringData("GLJournal|ApprovalRequestStatus", Caption = "Approval Request Status", MediumCaption = "Request Status")]
		[List("ApprovalStatusList")]
		public ZString ApprovalRequestStatus
		{
			get
			{
				var request = GetLatestLinkedApprovalRequestInDB();
				return request != null ? request.XP_ApprovalStatus : ZString.Empty;
			}
		}

		public GLJournalApprovalRequestCollection Approvals
		{
			get
			{
				if (approvals == null)
				{
					approvals = new GLJournalApprovalRequestCollection(Factory, new ZQuery(GenApprovalRequestSchema.XP_ParentID, PK));
				}

				return approvals;
			}
		}
		GLJournalApprovalRequestCollection approvals;

		#region Original Request

		public ZString OriginalRequest_RequesterFullName
		{
			get { return this.OriginalRequest == null ? ZString.Empty : this.OriginalRequest.CreatedUser_FullName; }
		}

		GLJournalApprovalRequest OriginalRequest
		{
			get
			{
				if (!isOriginalRequestSet)
				{
					var query = GetNewQueryForRelatedApprovalRequests();
					query.AddToFilter(GenApprovalRequestSchema.XP_ApprovalStatus, SQLComparisonOperator.NotEqual, Enterprise.Core.Constants.GenApprovalRequestApprovalStatus.Cancelled);
					query.OrderBy = GenApprovalRequestSchema.XP_SystemCreateTimeUtc.Name;

					originalRequest = Factory.LoadTop1<GLJournalApprovalRequest>(query);

					isOriginalRequestSet = true;
				}

				return originalRequest;
			}
		}

		bool isOriginalRequestSet;

		GLJournalApprovalRequest originalRequest;

		public ZString OriginalPostedRequest_ApproverFullName
		{
			get { return this.OriginalPostedRequest == null ? ZString.Empty : this.OriginalPostedRequest.ApprovedUser_FullName; }
		}

		GLJournalApprovalRequest OriginalPostedRequest
		{
			get
			{
				if (!isOriginalPostedRequestSet)
				{
					var query = GetNewQueryForRelatedApprovalRequests();
					query.AddToFilter(GenApprovalRequestSchema.XP_ApprovalStatus, SQLComparisonOperator.Equal, Enterprise.Core.Constants.GenApprovalRequestApprovalStatus.Posted);
					query.OrderBy = GenApprovalRequestSchema.XP_SystemCreateTimeUtc.Name;

					originalPostedRequest = Factory.LoadTop1<GLJournalApprovalRequest>(query);

					isOriginalPostedRequestSet = true;
				}

				return originalPostedRequest;
			}
		}

		bool isOriginalPostedRequestSet;

		GLJournalApprovalRequest originalPostedRequest;

		#endregion

		#region Last Request

		public ZDateTime LastPostedRequest_RequestedTime
		{
			get { return this.LastPostedRequest == null ? ZDateTime.Empty : this.LastPostedRequest.CreatedTimeLocal; }
		}

		public ZDateTime LastPostedRequest_ApprovedTime
		{
			get { return this.LastPostedRequest == null ? ZDateTime.Empty : this.LastPostedRequest.XP_ApprovalDate; }
		}

		public ZString LastPostedRequest_RequesterFullName
		{
			get { return this.LastPostedRequest == null ? ZString.Empty : this.LastPostedRequest.CreatedUser_FullName; }
		}

		public ZString LastPostedRequest_ApproverFullName
		{
			get { return this.LastPostedRequest == null ? ZString.Empty : this.LastPostedRequest.ApprovedUser_FullName; }
		}

		GLJournalApprovalRequest LastPostedRequest
		{
			get
			{
				if (!isLastPostedRequestSet)
				{
					var query = GetNewQueryForRelatedApprovalRequests();
					query.AddToFilter(GenApprovalRequestSchema.XP_ApprovalStatus, SQLComparisonOperator.Equal, Enterprise.Core.Constants.GenApprovalRequestApprovalStatus.Posted);
					query.OrderBy = GenApprovalRequestSchema.XP_SystemCreateTimeUtc.Name + " DESC";

					lastPostedRequest = Factory.LoadTop1<GLJournalApprovalRequest>(query);

					isLastPostedRequestSet = true;
				}

				return lastPostedRequest;
			}
		}

		bool isLastPostedRequestSet;

		GLJournalApprovalRequest lastPostedRequest;

		public ZString LastRequest_RequesterFullName
		{
			get { return this.LastRequest == null ? ZString.Empty : this.LastRequest.CreatedUser_FullName; }
		}

		GLJournalApprovalRequest LastRequest
		{
			get
			{
				if (!isLastRequestSet)
				{
					var query = GetNewQueryForRelatedApprovalRequests();
					query.AddToFilter(GenApprovalRequestSchema.XP_ApprovalStatus, SQLComparisonOperator.NotEqual, Enterprise.Core.Constants.GenApprovalRequestApprovalStatus.Cancelled);
					query.OrderBy = GenApprovalRequestSchema.XP_SystemCreateTimeUtc.Name + " DESC";

					lastRequest = Factory.LoadTop1<GLJournalApprovalRequest>(query);

					isLastRequestSet = true;
				}

				return lastRequest;
			}
		}

		bool isLastRequestSet;

		GLJournalApprovalRequest lastRequest;

		#endregion

		ZQuery GetNewQueryForRelatedApprovalRequests()
		{
			return new ZQuery(new GLJournalApprovalRequestCollection(Factory, new ZQuery(GenApprovalRequestSchema.XP_ParentID, PK)).CompleteFilter);
		}

		public CodeDescriptionPairList ApprovalStatusList
		{
			get { return ((GLJournalLookups)Lookups).ApprovalStatusList; }
		}

		public ZPropertyInfo ApprovalRequestStatusInfo
		{
			get { return GetZPropertyInfo(Schema.ApprovalRequestStatus); }
		}

		public void RefreshRequestFields()
		{
			ApprovalRequestStatusInfo.RefreshBinding();
		}

		public bool HasNotPostedApprovalRequest
		{
			get
			{
				var status = ApprovalRequestStatus;
				var liveRequestStatuses = new[] { Core.Constants.GenApprovalRequestApprovalStatus.Requested, Core.Constants.GenApprovalRequestApprovalStatus.Approved };
				return !status.IsEmpty && liveRequestStatuses.Contains(status.ToString());
			}
		}

		public void RelinkEDocsFromRequestToNewJournal(GLJournalApprovalRequest request)
		{
			if (!IsInDatabase) // if new journal, redirect the eDocs from approval request to journal
			{
				var storageMainOnRequest = request.StorageMain;
				if (storageMainOnRequest != null)
				{
					var storageDocsOnRequest = storageMainOnRequest.eDocs;
					if (storageDocsOnRequest != null && storageDocsOnRequest.Count > 0)
					{
						foreach (StorageDocsBase eDoc in storageDocsOnRequest.Cast<StorageDocsBase>().ToArray())
						{
							((DocumentFactory)DocManagerInfo.MasterFactory).Allocate(eDoc, PK);
						}
						Factory.ChildFactories.Add(storageDocsOnRequest.MasterFactory);
					}
				}
			}
		}

		internal GLJournalApprovalRequest GetLatestLinkedApprovalRequestInDB()
		{
			GLJournalApprovalRequest request = null;
			if (!IsInDatabase)
			{
				if (!latestLinkedApprovalRequestPK.IsEmpty)
				{
					request = Factory.Load<GLJournalApprovalRequest>(latestLinkedApprovalRequestPK);
					if (request != null && !request.IsInDatabase)
					{
						request = null;
					}
				}
				if (request == null && !previousLinkedApprovalRequestPK.IsEmpty)
				{
					request = Factory.Load<GLJournalApprovalRequest>(previousLinkedApprovalRequestPK);
				}
				if (request != null && previousLinkedApprovalRequestPK != request.PK)
				{
					previousLinkedApprovalRequestPK = request.PK;
				}
			}
			else
			{
				var query = GetNewQueryForRelatedApprovalRequests();
				query.OrderBy = GenApprovalRequestSchema.XP_SystemCreateTimeUtc.Name + " DESC";

				request = Factory.LoadTop1<GLJournalApprovalRequest>(query);
			}

			return request;
		}

		internal void SetLatestLinkedApprovalRequest(GLJournalApprovalRequest value)
		{
			GetLatestLinkedApprovalRequestInDB(); //to populate previousLinkedApprovalRequestPK

			latestLinkedApprovalRequestPK = value.PK;

			RefreshRequestFields();
		}
		ZGuid latestLinkedApprovalRequestPK;
		ZGuid previousLinkedApprovalRequestPK;

		public ZGuid LatestLinkedApprovalRequestPK
		{
			get
			{
				return latestLinkedApprovalRequestPK;
			}
		}

		public ZGuid PreviousLinkedApprovalRequestPK
		{
			get
			{
				return previousLinkedApprovalRequestPK;
			}
		}

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();

			ResetAllCachedRequests();
		}

		internal void ResetAllCachedRequests()
		{
			isOriginalPostedRequestSet = false;
			isOriginalRequestSet = false;
			isLastPostedRequestSet = false;
			isLastRequestSet = false;
		}
	}
}
