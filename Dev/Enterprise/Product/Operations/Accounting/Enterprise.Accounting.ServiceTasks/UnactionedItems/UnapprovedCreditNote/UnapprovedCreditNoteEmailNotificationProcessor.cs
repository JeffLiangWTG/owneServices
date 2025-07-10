using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.Business.AccountingConstants;

namespace Enterprise.Accounting.ServiceTasks
{
	public class UnapprovedCreditNoteEmailNotificationProcessor : UnactionedItemEmailNotificationProcessor<DynamicBusinessObject>
	{
		public UnapprovedCreditNoteEmailNotificationProcessor(ILogger logger) : base(logger) { }

		IEnumerable<ARCreditNoteApprovalRequest> UnactionedRequestsBizObjs;

		protected override IEnumerable<DynamicBusinessObject> GetUnactionedItems()
		{
			var selectQuery = FormattableString.Invariant($@"
	SELECT XP_PK RequestPK, ApprovalNumber, BranchPK, GB_Code BranchCode, GE_Code DepartmentCode, DepartmentPK, GC_Name CompanyName, GC_PK CompanyPK, GC_Code CompanyCode, XP_ReasonDescription Reason
	FROM (
		SELECT 
		XP_PK, 
		JH_JobNum AS ApprovalNumber, 
		ISNULL(XP_GB_JobBranch,JH_GB) AS BranchPK, 
		ISNULL(XP_GE_JobDepartment, JH_GE) AS DepartmentPK, 
		XP_ReasonDescription
		FROM dbo.GenApprovalRequest AS ARCreditNotes
		LEFT JOIN dbo.JobHeader	ON XP_ParentID = JH_PK
		WHERE XP_ApprovalStatus = '{Constants.GenApprovalRequestApprovalStatus.Requested}' 
		AND XP_ParentTableCode = '{JobHeaderSchema.Constants.Prefix}'
		AND XP_ApprovalType = '{Constants.GenApprovalRequestApprovalType.ARCreditNote}'
	
		UNION ALL
	
		SELECT 
		XP_PK, 
		AH_TransactionNum AS ApprovalNumber, 
		ISNULL(XP_GB_JobBranch, AH_GB) AS BranchPK, 
		ISNULL(XP_GE_JobDepartment, AH_GE) AS DepartmentPK, 
		XP_ReasonDescription
		FROM dbo.GenApprovalRequest AS ARCreditNotesForReversal
		LEFT JOIN dbo.AccTransactionHeader ON XP_ParentID = AH_PK 
		WHERE XP_ApprovalStatus = '{Constants.GenApprovalRequestApprovalStatus.Requested}' 
		AND XP_ParentTableCode = '{AccTransactionHeaderSchema.Constants.Prefix}' 
		AND XP_ApprovalType in ('{Constants.GenApprovalRequestApprovalType.ARCreditNoteForReversal}','{Constants.GenApprovalRequestApprovalType.ARCreditNote}')
	
		UNION ALL
	
		SELECT 
		XP_PK, 
		'Not Implemented' AS ApprovalNumber,
		XP_GB_JobBranch AS BranchPK, 
		XP_GE_JobDepartment AS DepartmentPK,
		XP_ReasonDescription
		FROM dbo.GenApprovalRequest AS ARCreditNotesForReversal
		WHERE XP_ApprovalStatus = '{Constants.GenApprovalRequestApprovalStatus.Requested}' 
		AND XP_ParentTableCode = '{GenApprovalRequestSchema.Constants.Prefix}' 
		AND XP_ApprovalType in ('{Constants.GenApprovalRequestApprovalType.ARCreditNoteForReversal}','{Constants.GenApprovalRequestApprovalType.ARCreditNote}')
	) AS UnapprovedCreditNotes
	INNER JOIN dbo.GlbBranch ON BranchPK = GB_PK 
	INNER JOIN dbo.GlbDepartment ON DepartmentPK = GE_PK 
	LEFT JOIN dbo.GlbCompany ON GB_GC = GC_PK");

			var unactionedRequests = new DynamicBusinessObjectCollection(Factory);
			unactionedRequests.Load(selectQuery);
			UnactionedRequestsBizObjs = Factory.Load(typeof(ARCreditNoteApprovalRequest), new ZQuery(GenApprovalRequestSchema.PK, unactionedRequests.Select(GetItemPK))).Cast<ARCreditNoteApprovalRequest>();

#if DEBUG
			if (Globals.IsTest)
			{
				itemPKs_ForTestOnly = unactionedRequests.Select(GetItemPK);
			}
#endif

			return unactionedRequests;
		}

		protected override AccountingHtmlEmailDef GetNewNotificationEmail(IEnumerable<DynamicBusinessObject> itemsGroupedByCompany, ZStringBuilder errorMessages, ZGuid companyPK)
		{
			var companyUnactionedRCNRequestsList = new List<UnapprovedRequestDetails>();
			var companyUnactionedRIRRequestsList = new List<UnapprovedRequestDetails>();
			AddRequestDetailsToCompanyLists(itemsGroupedByCompany, companyUnactionedRCNRequestsList, companyUnactionedRIRRequestsList);
			return new UnapprovedCreditNoteNotificationEmail(companyUnactionedRCNRequestsList, companyUnactionedRIRRequestsList, errorMessages, companyPK.ToGuid());
		}

		protected override ZBool ShouldSendEmailForCompany(ZGuid companyPK)
		{
			return !AccountingConfigurationRegistry.Instance.CreditNoteApprovalsNotifyGroup.GetFallBackValueAtAllLevels(companyPK.ToGuid(), Guid.Empty, Guid.Empty).Equals(Guid.Empty);
		}

		protected override ZGuid GetItemPK(DynamicBusinessObject item) => new ZGuid(item["RequestPK"]);
		protected override ZGuid GetItemCompanyPK(DynamicBusinessObject item) => new ZGuid(item["CompanyPK"]);
		protected override ZString GetItemCompanyName(DynamicBusinessObject item) => new ZString(item["CompanyName"]);
		protected override ZGuid GetItemBranchPK(DynamicBusinessObject item) => new ZGuid(item["BranchPK"]);

		void AddRequestDetailsToCompanyLists(IEnumerable<DynamicBusinessObject> unactionedRequestsGroupedByCompany, List<UnapprovedRequestDetails> companyUnactionedRCNRequestsList, List<UnapprovedRequestDetails> companyUnactionedRIRRequestsList)
		{
			foreach (var unactionedRequestsGroupedByBranch in unactionedRequestsGroupedByCompany.GroupBy(x => x["BranchPK"]))
			{
				foreach (var unactionedRequestGroupedByDepartment in unactionedRequestsGroupedByBranch.GroupBy(x => x["DepartmentPK"]))
				{
					AddRequestDetailsToCompanyListsCore(unactionedRequestGroupedByDepartment, companyUnactionedRCNRequestsList, companyUnactionedRIRRequestsList);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant for future functionality")]
		void AddRequestDetailsToCompanyListsCore(IEnumerable<DynamicBusinessObject> unactionedRequestGroupedByDepartment,
			List<UnapprovedRequestDetails> companyUnactionedRCNRequestsList, List<UnapprovedRequestDetails> companyUnactionedRIRRequestsList)
		{
			foreach (var unactionedRequestDynamicBizO in unactionedRequestGroupedByDepartment)
			{
				var unactionedRequestBizO = UnactionedRequestsBizObjs.First(x => x.PK == GetItemPK(unactionedRequestDynamicBizO));
				var companyCode = unactionedRequestDynamicBizO["CompanyCode"].ToString();
				var requestPK = unactionedRequestBizO.PK;
				var approvalNumberFromBizO = unactionedRequestDynamicBizO["ApprovalNumber"].ToString();
				var approvalNumberForEmail = approvalNumberFromBizO == "Not Implemented" ? string.Empty : approvalNumberFromBizO;
				var branchCode = unactionedRequestDynamicBizO["BranchCode"].ToString();
				var departmentCode = unactionedRequestDynamicBizO["DepartmentCode"].ToString();
				var createUser = unactionedRequestBizO.CreatedUser_FullName;
				var createDate = unactionedRequestBizO.XP_SystemCreateTimeUtc.ToShortDateString();
				var reason = unactionedRequestDynamicBizO["Reason"].ToString();
				var nextAuthRequired = unactionedRequestBizO.NextAuthorisationLevelRequired.ToString();
				var approvalsRequired = unactionedRequestBizO.ApprovingOptionForDisplay;
				var requestUsesMultiApprovers = unactionedRequestBizO.ApprovingOption != ApprovalCredentialOption.SingleLogin;

				var approverString = string.Join(", ", new[] {
					unactionedRequestBizO.ApprovingUser1?.GS_Code,
					unactionedRequestBizO.ApprovingUser2?.GS_Code,
					unactionedRequestBizO.ApprovingUser3?.GS_Code,
					unactionedRequestBizO.ApprovingUser4?.GS_Code,
					unactionedRequestBizO.ApprovingUser5?.GS_Code,
					unactionedRequestBizO.ApprovingUser6?.GS_Code
				}.Where(x => x.HasValue));

				if (unactionedRequestBizO.XP_ApprovalType == "RCN")
				{
					companyUnactionedRCNRequestsList.Add(new UnapprovedRequestDetails(companyCode, requestPK, approvalNumberForEmail, branchCode, departmentCode, createUser, createDate, approverString, nextAuthRequired, approvalsRequired, reason, requestUsesMultiApprovers));
				}
				else
				{
					companyUnactionedRIRRequestsList.Add(new UnapprovedRequestDetails(companyCode, requestPK, approvalNumberForEmail, branchCode, departmentCode, createUser, createDate, approverString, nextAuthRequired, approvalsRequired, reason, requestUsesMultiApprovers));
				}
			}
		}

#if DEBUG
		public IEnumerable<ZGuid> itemPKs_ForTestOnly;
#endif
	}
}
