using System.Collections.Generic;
using System.Globalization;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.Accounting.Business.EmailNotification
{
	public class ARCreditNoteApprovalRequestEmail : InvoicingBaseApprovalRequestEmail<ARCreditNoteApprovalRequestDetails>
	{
		public ARCreditNoteApprovalRequestEmail(ARCreditNoteApprovalRequest approvalRequest)
			: base(approvalRequest)
		{
		}

		protected override string GetSubject()
		{
			return Invariant($"AR Credit Note Approval request - {ApprovalRequest.JobNumber} was {ApprovalStatus}");
		}

		protected override string GetBody()
		{
			if (new List<string> { Constants.GenApprovalRequestApprovalStatus.Approved, Constants.GenApprovalRequestApprovalStatus.Rejected }.Contains(ApprovalRequest.XP_ApprovalStatus))
			{
				var builder = new ZStringBuilder(Invariant($@"<p>The following AR Credit Note approval request was {ApprovalStatus.ToLower(CultureInfo.CurrentCulture)} on {ApprovalRequest.XP_ApprovalDate}:</p>
{GetLinkForMoreDetails()}
<p><span style=""font-weight:bold;"">Created on: </span>{ApprovalRequest.CreatedTimeLocal.ToBestReadableDateTimeString()}</p>
<p><span style=""font-weight:bold;"">Reason code: </span>{ApprovalRequest.XP_ReasonCode}</p>
<p><span style=""font-weight:bold;"">Description: </span>{ApprovalRequest.XP_ReasonDescription}</p>
<p><span style=""font-weight:bold;"">{ApprovalStatus} by: </span>{GetApprovingUsers()}</p>"));

				if (ApprovalRequest.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Approved)
				{
					builder.AppendLine();
					builder.Append(AccountingConfigurationRegistry.Instance.PostCreditNoteOnApproval.Value
						? (NoResString)"<p>No further action is required. The credit note will be automatically posted.</p>"
						: (NoResString)"<p>You can now post the credit note.</p>");
				}
				return builder.ToString();
			}
			else
			{
				return base.GetBody();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "email text that can be sent to any language speaking user")]
		ZString GetApprovingUsers()
		{
			var approvedStaff1 = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, ApprovalRequest.XP_GS_NKApprovingUser1);
			var approvedStaff2 = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, ApprovalRequest.XP_GS_NKApprovingUser2);

			ZString result;
			if (ApprovalRequest.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Approved)
			{
				result = approvedStaff1?.GS_FullName ?? "User not found";
				if (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.Value.AuthorizationMode == Constants.AuthorizationMode.Codes.TwoApprovers)
				{
					result += Invariant($", {approvedStaff2?.GS_FullName ?? "User not found"}");
				}
			}
			else
			{
				result = approvedStaff2?.GS_FullName ?? approvedStaff1?.GS_FullName ?? "User not found";
			}
			return result;
		}

		protected override string GetLinkForMoreDetails()
		{
			var innerText = Invariant($@"Credit Note Approval - {ApprovalRequest.JobNumber}");
			var link = ObjectFactory.Get<IShowViewFormUrlCreator>().Create(ControllerIDs.ARCreditNoteApproval, ApprovalRequest.PK.ToGuid());

			var builder = new ZStringBuilder(
				string.IsNullOrWhiteSpace(link)
				? Invariant($"<p>{innerText}</p>")
				: Invariant($@"<p><a href=""{link}"">{innerText}</a></p>"));

			var otherLinks = base.GetLinkForMoreDetails();
			if (!string.IsNullOrWhiteSpace(otherLinks))
			{
				builder.AppendLine();
				builder.Append(otherLinks);
			}

			return builder.ToString();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "email text that can be sent to any language speaking user")]
		protected override string GetApprovalBizoName()
		{
			return "AR Credit Note";
		}
	}
}
