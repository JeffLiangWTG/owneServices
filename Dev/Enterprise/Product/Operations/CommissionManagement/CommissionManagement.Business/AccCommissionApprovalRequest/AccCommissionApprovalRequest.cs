using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business
{
	[CodeProperty(AccCommissionApprovalRequest.Schema.CRQ_BatchNumber), DescriptionProperty(AccCommissionApprovalRequest.Schema.CRQ_BatchNumber)]
	public class AccCommissionApprovalRequest : AutoAccCommissionApprovalRequest, ICommissionPayable
	{
		#region Schema

		public new class Schema : AutoAccCommissionApprovalRequest.Schema
		{
			public const string IncludeSummaryAsEmailAttachment = "IncludeSummaryAsEmailAttachment";
			public const string ApproveStatus1Description = "ApproveStatus1Description";
			public const string ApproveStatus2Description = "ApproveStatus2Description";
		}

		#endregion

		public AccCommissionApprovalRequest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region ApproveStatus1Description

		public ZString ApproveStatus1Description
		{
			get
			{
				if (CRQ_GS_NKApprovingStaff1.IsEmpty)
				{
					return ZString.Empty;
				}
				else if (CRQ_Staff1HasApproved)
				{
					return string.Format("{0}: {1}", CommissionApprovalRequestApproveStatusList.Descriptions.Approved, Staff1ApprovedDateLocal.ToLongTimeString());
				}
				else
				{
					return CommissionApprovalRequestApproveStatusList.Descriptions.Pending;
				}
			}
		}

		public ZPropertyInfo ApproveStatus1DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ApproveStatus1Description); }
		}

		#endregion

		#region ApproveStatus2Description

		public ZString ApproveStatus2Description
		{
			get
			{
				if (CRQ_GS_NKApprovingStaff2.IsEmpty)
				{
					return ZString.Empty;
				}
				else if (CRQ_Staff2HasApproved)
				{
					return string.Format("{0}: {1}", CommissionApprovalRequestApproveStatusList.Descriptions.Approved, Staff2ApprovedDateLocal.ToLongTimeString());
				}
				else
				{
					return CommissionApprovalRequestApproveStatusList.Descriptions.Pending;
				}
			}
		}

		public ZPropertyInfo ApproveStatus2DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ApproveStatus2Description); }
		}

		#endregion

		#region CRQ_GS_NKApprovingStaff1

		protected bool CRQ_GS_NKApprovingStaff1_ReadOnly
		{
			get { return IsInDatabase; }
		}

		internal SecurityCore ApprovingStaff1Security
		{
			get
			{
				var approvingStaff1 = ApprovingStaff1;
				if (approvingStaff1 == null)
				{
					approvingStaff1Security = null;
				}
				else if (approvingStaff1Security == null || approvingStaff1Security.UserPK != approvingStaff1.PK)
				{
					approvingStaff1Security = new SecurityCore(null, approvingStaff1, Guid.Empty, Guid.Empty, Guid.Empty);
				}

				return approvingStaff1Security;
			}
		}
		SecurityCore approvingStaff1Security;

		#endregion

		#region CRQ_GS_NKApprovingStaff2

		protected bool CRQ_GS_NKApprovingStaff2_ReadOnly
		{
			get { return IsInDatabase; }
		}

		internal SecurityCore ApprovingStaff2Security
		{
			get
			{
				var approvingStaff2 = ApprovingStaff2;
				if (approvingStaff2 == null)
				{
					approvingStaff2Security = null;
				}
				else if (approvingStaff2Security == null || approvingStaff2Security.UserPK != approvingStaff2.PK)
				{
					approvingStaff2Security = new SecurityCore(null, approvingStaff2, Guid.Empty, Guid.Empty, Guid.Empty);
				}

				return approvingStaff2Security;
			}
		}
		SecurityCore approvingStaff2Security;

		#endregion

		#region Status

		public ZString Status
		{
			get
			{
				if (!Items.Any())
				{
					return CommissionApprovalRequestStatusList.Codes.Canceled;
				}
				else if (IsApproved)
				{
					return CommissionApprovalRequestStatusList.Codes.Approved;
				}
				else
				{
					return CommissionApprovalRequestStatusList.Codes.Pending;
				}
			}
		}

		public ZString StatusDescription
		{
			get { return new CommissionApprovalRequestStatusList().GetDescriptionFromCode(Status); }
		}

		public ZBool IsApproved
		{
			get
			{
				return
					(CRQ_GS_NKApprovingStaff1.IsEmpty || CRQ_Staff1HasApproved) &&
					(CRQ_GS_NKApprovingStaff2.IsEmpty || CRQ_Staff2HasApproved);
			}
		}

		#endregion

		#region IncludeSummaryAsEmailAttachment

		public ZBool IncludeSummaryAsEmailAttachment
		{
			get { return includeSummaryAsEmailAttachment; }
			set
			{
				SetNonPersistentPropertyValue(IncludeSummaryAsEmailAttachmentInfo, ref includeSummaryAsEmailAttachment, value);
			}
		}
		ZBool includeSummaryAsEmailAttachment;

		public ZPropertyInfo IncludeSummaryAsEmailAttachmentInfo
		{
			get { return GetZPropertyInfo(Schema.IncludeSummaryAsEmailAttachment); }
		}

		#endregion

		#region HasAtLeastOneStaffWhoHasApproved

		public ZBool HasAtLeastOneStaffWhoHasApproved
		{
			get { return CRQ_Staff1HasApproved || CRQ_Staff2HasApproved; }
		}

		#endregion

		#endregion

		#region Approve

		#region Staff 1

		public ZDateTime Staff1ApprovedDateLocal
		{
			get
			{
				var log = Staff1ApprovedLog;
				return log != null ? log.SL_PostedTimeUtc.ToLocalBranchTime() : ZDateTime.Empty;
			}
		}

		public void ApproveStaff1()
		{
			if (!CRQ_Staff1HasApproved)
			{
				CRQ_GS_NKApprovingStaff1 = GlbStaff.CurrentUser.GS_Code;
				CRQ_Staff1HasApproved = true;
				Logs.AddNew(Events.StatusChange, ApprovedLevel1ReferenceLog);
				CheckApproved();
			}
		}

		#endregion

		#region Staff 2

		public ZDateTime Staff2ApprovedDateLocal
		{
			get
			{
				var log = Staff2ApprovedLog;
				return log != null ? log.SL_PostedTimeUtc.ToLocalBranchTime() : ZDateTime.Empty;
			}
		}

		public void ApproveStaff2()
		{
			if (!CRQ_Staff2HasApproved)
			{
				CRQ_GS_NKApprovingStaff2 = GlbStaff.CurrentUser.GS_Code;
				CRQ_Staff2HasApproved = true;
				Logs.AddNew(Events.StatusChange, ApprovedLevel2ReferenceLog);
				CheckApproved();
			}
		}

		#endregion

		void CheckApproved()
		{
			if (IsApproved)
			{
				var commissionLinesToApprove = SelectedItems.Select(x => x.CommissionLine).ToArray();
				foreach (var line in commissionLinesToApprove)
				{
					line.CL0_ApprovedDateTimeUtc = ZDateTime.UtcNow;
				}

				commissionLinesApprovedPendingSave = commissionLinesToApprove;
			}
		}

		AccCommissionLine[] commissionLinesApprovedPendingSave;

		#region Approve Logs

		StmALog GetStaffApprovedLog(string reference)
		{
			var query = new ZQuery(StmALogSchema.SL_Parent, PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.StatusChangeCode);
			query.AddToFilter(StmALogSchema.SL_Reference, reference);
			query.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + OrderByClause.Descending;

			return Factory.LoadTop1<StmALog>(query);
		}

		StmALog Staff1ApprovedLog
		{
			get { return GetStaffApprovedLog(ApprovedLevel1ReferenceLog); }
		}

		StmALog Staff2ApprovedLog
		{
			get { return GetStaffApprovedLog(ApprovedLevel2ReferenceLog); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log Reference Values should be in English only")]
		const string ApprovedLevel1ReferenceLog = "Approved Level 1";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log Reference Values should be in English only")]
		const string ApprovedLevel2ReferenceLog = "Approved Level 2";

		#endregion

		#endregion

		#region Amendment made to a Transaction after Approval Request sent

		public bool AmendmentMadeToTransactionAfterApprovalRequest()
		{
			var amendingTransactionQuery = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			var commissionHeaderSubquery = new ZDBOnlySubQuery(typeof(AccCommissionHeader), AccCommissionHeaderSchema.CH0_AH_Source, AccTransactionHeaderSchema.AH_TransactionBelongsToGroup);

			var commissionLineQuery = new ZDBOnlySubQuery(typeof(AccCommissionLine), AccCommissionLineSchema.CL0_ParentID);
			var requestItemQuery = new ZDBOnlySubQuery(typeof(AccCommissionApprovalRequestItem), AccCommissionApprovalRequestItemSchema.CRI_CL0);
			requestItemQuery.AddToFilter(AccCommissionApprovalRequestItemSchema.CRI_CRQ, PK);

			commissionLineQuery.AddSubQuery(requestItemQuery, JoinCondition.And);
			var commissionLineGroupQuery = new ZDBOnlySubQuery(typeof(AccCommissionLineGroup), AccCommissionLineGroupSchema.CLG_CH0);
			var commissionLine2Query = new ZDBOnlySubQuery(typeof(AccCommissionLine), AccCommissionLineSchema.CL0_ParentID);
			commissionLine2Query.AddSubQuery(requestItemQuery, JoinCondition.And);
			commissionLineGroupQuery.AddSubQuery(commissionLine2Query, JoinCondition.And);

			commissionLineQuery.AddAsUnionQuery(commissionLineGroupQuery);

			commissionHeaderSubquery.AddSubQuery(commissionLineQuery, JoinCondition.And);
			amendingTransactionQuery.AddSubQuery(commissionHeaderSubquery, JoinCondition.And);

			var amendingTransactions = Factory.Load<AccTransactionHeader>(amendingTransactionQuery);

			var commissionHeadersInRequestQuery = new ZDBOnlyQuery(typeof(AccCommissionHeader));
			commissionHeadersInRequestQuery.AddSubQuery(commissionLineQuery, JoinCondition.And);

			var commissionHeadersInRequest = Factory.Load<AccCommissionHeader>(commissionHeadersInRequestQuery);

			var amendingTransactionsNotFound = (from amending in amendingTransactions
												join header in commissionHeadersInRequest
												on amending.PK equals header.CH0_AH_Source into matches
												from match in matches.DefaultIfEmpty()
												where match == null
												select amending).ToArray();

			var amendingTransactionsNotFoundWithOutsideCommissionQuery = new ZDBOnlyQuery(typeof(AccTransactionHeader));

			var outsideCommissionHeaderSubquery = new ZDBOnlySubQuery(typeof(AccCommissionHeader), AccCommissionHeaderSchema.CH0_AH_Source, AccTransactionHeaderSchema.PK);
			outsideCommissionHeaderSubquery.AddToFilter(AccCommissionHeaderSchema.CH0_AH_Source, amendingTransactionsNotFound.Select(a => a.PK).ToArray());
			var outsideCommissionLineQuery = new ZDBOnlySubQuery(typeof(AccCommissionLine), AccCommissionLineSchema.CL0_ParentID);
			outsideCommissionLineQuery.AddToFilter(AccCommissionLineSchema.CL0_CancelledDateTimeUtc, null);

			var outsideCommissionLineGroupQuery = new ZDBOnlySubQuery(typeof(AccCommissionLineGroup), AccCommissionLineGroupSchema.CLG_CH0);
			var outsideCommissionLine2Query = new ZDBOnlySubQuery(typeof(AccCommissionLine), AccCommissionLineSchema.CL0_ParentID);
			outsideCommissionLine2Query.AddToFilter(AccCommissionLineSchema.CL0_CancelledDateTimeUtc, null);
			outsideCommissionLineGroupQuery.AddSubQuery(outsideCommissionLine2Query, JoinCondition.And);
			outsideCommissionLineQuery.AddAsUnionQuery(outsideCommissionLineGroupQuery);
			outsideCommissionHeaderSubquery.AddSubQuery(outsideCommissionLineQuery, JoinCondition.And);

			amendingTransactionsNotFoundWithOutsideCommissionQuery.AddSubQuery(outsideCommissionHeaderSubquery, JoinCondition.And);

			var amendingTransactionsNotFoundWithOutsideCommissions = Factory.Load<AccTransactionHeader>(amendingTransactionsNotFoundWithOutsideCommissionQuery).ToArray();

			var sourcesWithErrors = (from amending in amendingTransactionsNotFoundWithOutsideCommissions
									 join header in commissionHeadersInRequest
									 on amending.AH_TransactionBelongsToGroup equals header.CH0_AH_Source
									 group header by new { header.CH0_GroupingSourceTableCode, header.GroupingSourceUniqueId }
															into grp
									 select new { grp.Key.CH0_GroupingSourceTableCode, grp.Key.GroupingSourceUniqueId }).ToArray();

			void AddAmendmentTransactionErrors(CommissionApprovalRequestItemGrouping grouping)
			{
				if (grouping.IsLeaf)
				{
					foreach (var sourceWithError in sourcesWithErrors)
					{
						if (sourceWithError.CH0_GroupingSourceTableCode == grouping.SourceTableCode &&
							sourceWithError.GroupingSourceUniqueId == grouping.SourceUniqueId)
						{
							grouping.AddRowError(Res.GetString("b8d4106a-dabc-4a5b-9bc6-166bc124810d", "Amendment done since approval request."));
							break;
						}
					}
				}
				else
				{
					foreach (var subgrouping in grouping.SubGroupings)
					{
						AddAmendmentTransactionErrors(subgrouping);
					}
				}
			}

			foreach (var grouping in CommissionApprovalRequestItemGroupings)
			{
				AddAmendmentTransactionErrors(grouping);
			}

			return amendingTransactionsNotFoundWithOutsideCommissions.Length > 0;
		}

		#endregion

		#region Obsolete

		public bool AddObsoleteErrors()
		{
			var isObsolete = false;
			foreach (var grouping in CommissionApprovalRequestItemGroupings)
			{
				isObsolete |= AddObsoleteErrorsOnLeafLevelGrouping(grouping);
			}

			return isObsolete;
		}

		bool AddObsoleteErrorsOnLeafLevelGrouping(CommissionApprovalRequestItemGrouping grouping)
		{
			var isObsolete = false;
			if (grouping.IsLeaf)
			{
				if (grouping.CommissionStatus == AccCommissionLineCommissionStatusList.Codes.Paid)
				{
					grouping.AddRowError(Res.GetString("7057775a-816c-4655-ad7d-227a52c91aa1", "Already been paid out."));
					isObsolete = true;
				}
				else if (grouping.IsCanceled)
				{
					grouping.AddRowError(Res.GetString("b2c94736-7208-4dda-9052-c037973f2ec8", "Has been canceled."));
					isObsolete = true;
				}
			}
			else
			{
				foreach (var subgrouping in grouping.SubGroupings)
				{
					isObsolete |= AddObsoleteErrorsOnLeafLevelGrouping(subgrouping);
				}
			}

			return isObsolete;
		}

		#endregion

		#region Save

		public override void OnSaving()
		{
			base.OnSaving();

			if (!IsInDatabase)
			{
				PopulateBatchNumber();
				DetachCommissionLinesFromAllOtherApprovalRequests();
				EmailSender.CreateCommissionApprovalRequestEmail();
			}
		}

		void DetachCommissionLinesFromAllOtherApprovalRequests()
		{
			var requestItemsQuery = new ZQuery { AllowTableValuedParameters = true }.AddToFilter(AccCommissionApprovalRequestItemSchema.CRI_CL0, Items.Select(x => x.CRI_CL0));
			requestItemsQuery.AddToFilter(AccCommissionApprovalRequestItemSchema.CRI_CRQ, SQLComparisonOperator.NotEqual, PK);

			foreach (var requestItem in Factory.Load<AccCommissionApprovalRequestItem>(requestItemsQuery))
			{
				requestItem.Delete();
			}
		}

		void PopulateBatchNumber()
		{
			if (!IsInDatabase && CRQ_BatchNumber.IsEmpty)
			{
				CRQ_BatchNumber = Env.NumberFountains.CommissionApprovalRequestBatchNo.GetNextFormatted(Factory);
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			if (saveSucceeded)
			{
				ApproveStatus1DescriptionInfo.RefreshBinding();
				ApproveStatus2DescriptionInfo.RefreshBinding();

				if (commissionLinesApprovedPendingSave != null)
				{
					PublishApprovedDatesToViews(commissionLinesApprovedPendingSave);
					commissionLinesApprovedPendingSave = null;
				}
			}

			base.OnSaved(saveSucceeded);
		}

		void PublishApprovedDatesToViews(IEnumerable<AccCommissionLine> commissionLines)
		{
			foreach (var commissionLine in commissionLines)
			{
				PublishApprovedDatesToViews(commissionLine);
			}
		}

		void PublishApprovedDatesToViews(AccCommissionLine commissionLine)
		{
			var viewsQuery = new ZQuery(ViewCommissionLineSchema.PK, commissionLine.PK);
			viewsQuery.FetchOnlyFromLocalCache = true;
			foreach (var view in Factory.Load<ViewCommissionLine>(viewsQuery))
			{
				view.VCL_ApprovedDateTimeUtc = commissionLine.CL0_ApprovedDateTimeUtc;
			}
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			DeleteRelatedBusinessObjects();
			base.Delete();
		}

		void DeleteRelatedBusinessObjects()
		{
			Items.DeleteAll();
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Human Readable Name

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return IsDeleted ?
					base.HumanReadableName :
					(ZString)Res.GetString("8ae612d6-3cb0-4e93-850d-9b49b249063b", "Commission Approval Request {0}", CRQ_BatchNumber);
			}
		}

		#endregion

		#region Related Business Objects

		#region Items

		public AccCommissionApprovalRequestItemCollection Items
		{
			get { return items ?? (items = new AccCommissionApprovalRequestItemCollection(this)); }
		}
		AccCommissionApprovalRequestItemCollection items;

		public IEnumerable<AccCommissionApprovalRequestItem> SelectedItems
		{
			get { return Items.Where(x => x.CRI_IsSelected); }
		}

		#endregion

		#region CommissionApprovalRequestItemGroupingCollection

		public TopLevelCommissionApprovalRequestItemGroupingCollection CommissionApprovalRequestItemGroupingCollection
		{
			get
			{
				if (commissionApprovalRequestItemGroupingCollection == null)
				{
					commissionApprovalRequestItemGroupingCollection = new TopLevelCommissionApprovalRequestItemGroupingCollection(Items);
					commissionApprovalRequestItemGroupingCollection.Init();
				}

				return commissionApprovalRequestItemGroupingCollection;
			}
		}
		TopLevelCommissionApprovalRequestItemGroupingCollection commissionApprovalRequestItemGroupingCollection;

		IEnumerable<CommissionApprovalRequestItemGrouping> CommissionApprovalRequestItemGroupings
		{
			get { return CommissionApprovalRequestItemGroupingCollection.Cast<CommissionApprovalRequestItemGrouping>(); }
		}

		#endregion

		#region Emails

		public AccCommissionApprovalRequestEmailSender EmailSender
		{
			get { return emailSender ?? (emailSender = new AccCommissionApprovalRequestEmailSender(this)); }
		}
		AccCommissionApprovalRequestEmailSender emailSender;

		#endregion

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get { return new CommissionApprovalRequestDocumentSupporter(this); }
		}

		#endregion

		#region ICommissionPayable Members

		ZString ICommissionPayable.BatchNumber
		{
			get { return CRQ_BatchNumber; }
		}

		IEnumerable<ViewCommissionLine> ICommissionPayable.CommissionLinesForPayment
		{
			get
			{
				return
					from item in SelectedItems
					let line = item.ViewCommissionLine
					where
						line != null
					select line;
			}
		}

		#endregion
	}
}
