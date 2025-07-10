using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.EmailNotification
{
	public class UnapprovedCreditNoteNotificationEmail : AccountingHtmlEmailDef
	{
		public UnapprovedCreditNoteNotificationEmail(List<UnapprovedRequestDetails> unapprovedRCNRequests, List<UnapprovedRequestDetails> unapprovedRIRRequests,
			ZStringBuilder errorMessageCollector, Guid companyPK) : base()
		{
			this.unapprovedRCNRequests = SortByAuth(unapprovedRCNRequests);
			this.unapprovedRIRRequests = SortByAuth(unapprovedRIRRequests);
			this.errorMessageCollector = errorMessageCollector;
			this.companyPK = companyPK;
			FromDisplayName = (NoResString)"Credit Note Approvals Notification Email";
		}

		readonly List<UnapprovedRequestDetails> unapprovedRCNRequests;
		readonly List<UnapprovedRequestDetails> unapprovedRIRRequests;
		readonly ZStringBuilder errorMessageCollector;
		readonly Guid companyPK;

		#region Overrides

		public override ZString EmailDescription => (NoResString)"Credit Note Approvals Email Notification";

		protected override GuidRegistryItem Recipient => AccountingConfigurationRegistry.Instance.CreditNoteApprovalsNotifyGroup;

		protected override string GetSubject() => (NoResString)"Unactioned Credit Note Approval Requests";

		protected override string GetBody()
		{
			#region SuppressResourceStringsCheckRegion

			var addApproverColumntoRCN = unapprovedRCNRequests.Any(x => x.usesMultiApproverOption);
			var addApproverColumntoRIR = unapprovedRIRRequests.Any(x => x.usesMultiApproverOption);

			var messageBuilder = new ZStringBuilder();

			messageBuilder.Append("<br/>");
			messageBuilder.Append("<p>The following Credit Notes require Authorisation.</p>");
			if (unapprovedRCNRequests.Any())
			{
				messageBuilder.Append("<h3>Credit Notes:</h3>");
				messageBuilder.Append("<table>");
				messageBuilder.Append("<tbody>");
				messageBuilder.Append("<tr>");
				CreateExpectedEmailBodyHeader(messageBuilder, addApproverColumntoRCN);
				messageBuilder.Append("</tr>");
				foreach (var unapprovedRCNRequest in unapprovedRCNRequests)
				{
					messageBuilder.Append("<tr>");
					var link = GetLinkToRequest(unapprovedRCNRequest.requestPK);
					CreateTableRowForTransaction(link, unapprovedRCNRequest, messageBuilder, addApproverColumntoRCN);
					messageBuilder.Append("</tr>");
				}
				messageBuilder.Append("</tbody>");
				messageBuilder.Append("</table>");
				messageBuilder.Append("<br/>");
			}
			if (unapprovedRIRRequests.Any())
			{
				messageBuilder.Append("<h3>Invoice Reversals:</h3>");
				messageBuilder.Append("<table>");
				messageBuilder.Append("<tbody>");
				messageBuilder.Append("<tr>");
				CreateExpectedEmailBodyHeader(messageBuilder, addApproverColumntoRIR);
				messageBuilder.Append("</tr>");
				foreach (var unapprovedRIRRequest in unapprovedRIRRequests)
				{
					messageBuilder.Append("<tr>");
					var link = GetLinkToRequest(unapprovedRIRRequest.requestPK);
					CreateTableRowForTransaction(link, unapprovedRIRRequest, messageBuilder, addApproverColumntoRIR);
					messageBuilder.Append("</tr>");
				}
				messageBuilder.Append("</tbody>");
				messageBuilder.Append("</table>");
				messageBuilder.Append("<br/>");
			}
			messageBuilder.Append(FormattableString.Invariant($"<p>To approve or reject the credit notes, please navigate to <a href=\"{ObjectFactory.Get<IShowModuleUrlHandler>().Create(ModuleIDs.ARCreditNoteApproval)}\">Manage > Receivables > Credit Note Approval</a>.</p>"));
			return messageBuilder.ToStringWithNewLineBetweenAppends();

			#endregion
		}

		protected override string GetCss()
		{
			return String.Empty;
		}

		protected override Guid GetCompanyPK()
		{
			return companyPK;
		}

		protected override Guid GetBranchPK()
		{
			return Guid.Empty;
		}

		protected override Guid GetDepartmentPK()
		{
			return Guid.Empty;
		}

		protected override void DisplayEmailHasNoRecipientsError(string exceptionMessage)
		{
			AddExceptionMessageToErrorMessageCollector(exceptionMessage);
		}

		protected override void DisplayEmailNotCompleteError(string exceptionMessage)
		{
			AddExceptionMessageToErrorMessageCollector(exceptionMessage);
		}

		protected override void DisplayEmailSendFailedError(string exceptionMessage)
		{
			AddExceptionMessageToErrorMessageCollector(exceptionMessage);
		}

		#endregion

		#region Implementation

		void CreateExpectedEmailBodyHeader(ZStringBuilder builder, bool includeApproverColumn)
		{
			#region SuppressResourceStringsCheckRegion

			builder.Append(FormattableString.Invariant($"<th align=\"left\">Approval Request:</th>"));
			builder.Append(FormattableString.Invariant($"<th align=\"left\">Branch:</th>"));
			builder.Append(FormattableString.Invariant($"<th align=\"left\">Department:</th>"));
			builder.Append(FormattableString.Invariant($"<th align=\"left\">Created by:</th>"));
			builder.Append(FormattableString.Invariant($"<th align=\"left\">Created On:</th>"));
			builder.Append(FormattableString.Invariant($"<th align=\"left\">Approvals Required:</th>"));
			builder.Append(FormattableString.Invariant($"<th align=\"left\">Next Approval Required:</th>"));
			if (includeApproverColumn)
			{
				builder.Append(FormattableString.Invariant($"<th align=\"left\">Reviewed by:</th>"));
			}
			builder.Append(FormattableString.Invariant($"<th align=\"left\">Reason Description:</th>"));

			#endregion
		}

		void CreateTableRowForTransaction(string link, UnapprovedRequestDetails unapprovedRCNRequest, ZStringBuilder builder, bool includeApproverColumn)
		{
			var approvalNumber = ZString.Empty;
			if (unapprovedRCNRequest.approvalNumber != ZString.Empty)
			{
				approvalNumber = FormattableString.Invariant($" - {unapprovedRCNRequest.approvalNumber}");
			}

			#region SuppressResourceStringsCheckRegion

			builder.Append(FormattableString.Invariant($"<td align=\"left\"><a href=\"{link}"));
			builder.Append(FormattableString.Invariant($"\">Credit Note Approval{approvalNumber}</a></td>"));
			builder.Append(FormattableString.Invariant($"<td align=\"left\">{unapprovedRCNRequest.branchName}</td>"));
			builder.Append(FormattableString.Invariant($"<td align=\"left\">{unapprovedRCNRequest.departmentCode}</td>"));
			builder.Append(FormattableString.Invariant($"<td align=\"left\">{unapprovedRCNRequest.createUser}</td>"));
			builder.Append(FormattableString.Invariant($"<td align=\"left\">{unapprovedRCNRequest.createDate}</td>"));
			builder.Append(FormattableString.Invariant($"<td align=\"left\">{unapprovedRCNRequest.totalApprovalLevel}</td>"));
			builder.Append(FormattableString.Invariant($"<td align=\"left\">{unapprovedRCNRequest.nextApprovalLevel}</td>"));
			if (includeApproverColumn)
			{
				builder.Append(FormattableString.Invariant($"<td align=\"left\">{unapprovedRCNRequest.approvers}</td>"));
			}
			builder.Append(FormattableString.Invariant($"<td align=\"left\">{unapprovedRCNRequest.reason}</td>"));

			#endregion
		}

		void AddExceptionMessageToErrorMessageCollector(string exceptionMessage)
		{
			errorMessageCollector.Append(exceptionMessage);
		}

		string GetLinkToRequest(ZGuid requestPK)
		{
			var link = ZString.Empty;
			var controllerID = ControllerIDs.ARCreditNoteApproval;
			if (controllerID != null)
			{
				link = ObjectFactory.Get<IShowViewFormUrlCreator>().Create(controllerID, requestPK.ToGuid());
			}
			return link;
		}

		List<UnapprovedRequestDetails> SortByAuth(List<UnapprovedRequestDetails> unapprovedRequestsList)
		{
			unapprovedRequestsList.Sort(new UnapprovedRequestDetailsComparer());
			return unapprovedRequestsList;
		}

		#region Comparer

		class UnapprovedRequestDetailsComparer : IComparer<UnapprovedRequestDetails>
		{
			public int Compare(UnapprovedRequestDetails x, UnapprovedRequestDetails y)
			{
				if (x.totalApprovalLevel != y.totalApprovalLevel)
				{
					return GetSubCodeOrder(x.totalApprovalLevel).CompareTo(GetSubCodeOrder(y.totalApprovalLevel));
				}

				return 0;
			}

			int GetSubCodeOrder(string subCode)
			{
				if (subCode == AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly) { return 1; }
				else if (subCode == AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly) { return 2; }
				else if (subCode == AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired) { return 3; }
				else if (subCode == ZString.Empty) { return 4; }
				else { return 5; }
			}
		}

		#endregion

		#endregion

	}

	#region Data Structure

	public struct UnapprovedRequestDetails
	{
		public UnapprovedRequestDetails(ZString companyCode, ZGuid requestPK, ZString approvalNumber, ZString branchName, ZString departmentCode
			, ZString createUser, ZString createDate, ZString approvers, ZString nextApprovalLevel, ZString totalApprovalLevel, ZString reason, ZBool usesMultiApproverOption)
		{
			this.companyCode = companyCode;
			this.requestPK = requestPK;
			this.approvalNumber = approvalNumber;
			this.branchName = branchName;
			this.departmentCode = departmentCode;
			this.createUser = createUser;
			this.createDate = createDate;
			this.approvers = approvers;
			this.nextApprovalLevel = nextApprovalLevel;
			this.totalApprovalLevel = totalApprovalLevel;
			this.reason = reason;
			this.usesMultiApproverOption = usesMultiApproverOption;
		}

		public readonly ZString companyCode;
		public readonly ZGuid requestPK;
		public readonly ZString approvalNumber;
		public readonly ZString branchName;
		public readonly ZString departmentCode;
		public readonly ZString createUser;
		public readonly ZString createDate;
		public readonly ZString approvers;
		public readonly ZString nextApprovalLevel;
		public readonly ZString totalApprovalLevel;
		public readonly ZString reason;
		public readonly ZBool usesMultiApproverOption;

		#region Overrides

		public override bool Equals(object obj)
		{
			var result = false;
			if (obj is UnapprovedRequestDetails)
			{
				var castedObj = (UnapprovedRequestDetails)obj;
				result = companyCode == castedObj.companyCode && requestPK == castedObj.requestPK && approvalNumber == castedObj.approvalNumber &&
								branchName == castedObj.branchName && departmentCode == castedObj.departmentCode && createUser == castedObj.createUser &&
								createDate == castedObj.createDate && reason == castedObj.reason && totalApprovalLevel == castedObj.totalApprovalLevel;
			}
			return result;
		}

		public override int GetHashCode()
		{
			return companyCode.GetHashCode() ^ requestPK.GetHashCode() ^ approvalNumber.GetHashCode() ^ branchName.GetHashCode() ^ departmentCode.GetHashCode() ^
				createUser.GetHashCode() ^ createDate.GetHashCode() ^ reason.GetHashCode() ^ totalApprovalLevel.GetHashCode();
		}

		#endregion

		#region Comparison Operators

		public static bool operator ==(UnapprovedRequestDetails detail1, UnapprovedRequestDetails detail2)
		{
			return detail1.Equals(detail2);
		}

		public static bool operator !=(UnapprovedRequestDetails detail1, UnapprovedRequestDetails detail2)
		{
			return !detail1.Equals(detail2);
		}

		#endregion
	}

	#endregion

}
