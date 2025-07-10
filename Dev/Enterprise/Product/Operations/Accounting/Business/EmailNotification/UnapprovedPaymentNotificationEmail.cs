using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.EmailNotification
{
	public class UnapprovedPaymentNotificationEmail : AccountingHtmlEmailDef
	{
		public UnapprovedPaymentNotificationEmail(List<PaymentApprovalDetails> unapprovedCreditorApprovals, List<PaymentApprovalDetails> unapprovedDebtorApprovals,
					ZStringBuilder errorMessageCollector, Guid companyPK) : base()
		{
			UnapprovedCreditorApprovals = unapprovedCreditorApprovals;
			UnapprovedCreditorApprovals.Sort(DetailsComparer);
			UnapprovedDebtorApprovals = unapprovedDebtorApprovals;
			UnapprovedDebtorApprovals.Sort(DetailsComparer);

			ErrorMessageCollector = errorMessageCollector;
			CompanyPK = companyPK;
		}

		readonly List<PaymentApprovalDetails> UnapprovedCreditorApprovals;
		readonly List<PaymentApprovalDetails> UnapprovedDebtorApprovals;
		readonly ZStringBuilder ErrorMessageCollector;
		readonly Guid CompanyPK;

		protected override GuidRegistryItem Recipient => AccountingConfigurationRegistry.Instance.PaymentApprovalsNotifyGroup;

		#region SuppressResourceStringsCheckRegion

		public override ZString EmailDescription => "Payment Approvals Notification Email";

		protected override string GetSubject() => "Unactioned Payment Approvals";

		protected override string GetBody()
		{
			var messageBuilder = new ZStringBuilder();

			messageBuilder.Append("<br/>");
			messageBuilder.Append("<p>The following Payment Approvals require Authorisation.</p>");
			if (UnapprovedDebtorApprovals.Any())
			{
				messageBuilder.Append("<h3>Payments to Debtor Organizations:</h3>");
				AddTableForApprovals(UnapprovedDebtorApprovals, ControllerIDs.ARPaymentProcessing, messageBuilder);
				messageBuilder.Append($"<p>To manage all requests, please navigate to <a href=\"{ObjectFactory.Get<IShowModuleUrlHandler>().Create(ModuleIDs.ARPaymentProcessing)}\">Manage > Receivables > Payment Processing</a>.</p>");
				messageBuilder.Append("<br/>");
			}
			if (UnapprovedCreditorApprovals.Any())
			{
				messageBuilder.Append("<h3>Payments to Creditor Organizations:</h3>");
				AddTableForApprovals(UnapprovedCreditorApprovals, ControllerIDs.APPaymentProcessing, messageBuilder);
				messageBuilder.Append($"<p>To manage all requests, please navigate to <a href=\"{ObjectFactory.Get<IShowModuleUrlHandler>().Create(ModuleIDs.APPaymentProcessing)}\">Manage > Payables > Payment Processing</a>.</p>");
				messageBuilder.Append("<br/>");
			}

			return messageBuilder.ToStringWithNewLineBetweenAppends();
		}

		void AddTableForApprovals(List<PaymentApprovalDetails> approvals, ControllerID controllerID, ZStringBuilder messageBuilder)
		{
			messageBuilder.Append("<table>");
			messageBuilder.Append("<tbody>");
			messageBuilder.Append("<tr>");
			messageBuilder.Append("<th align=\"left\">Approval Request:</th>");
			messageBuilder.Append("<th align=\"left\">Currency:</th>");
			messageBuilder.Append("<th align=\"left\">Amount:</th>");
			messageBuilder.Append("<th align=\"left\">Bank:</th>");
			messageBuilder.Append("<th align=\"left\">Cheque Book Branch:</th>");
			messageBuilder.Append("<th align=\"left\">Payment Batch:</th>");
			messageBuilder.Append("<th align=\"left\">Organization:</th>");
			messageBuilder.Append("<th align=\"left\">Organization Name:</th>");
			messageBuilder.Append("<th align=\"left\">Payment Date:</th>");
			messageBuilder.Append("<th align=\"left\">Created By:</th>");
			messageBuilder.Append("<th align=\"left\">Authorization Level Required:</th>");
			messageBuilder.Append("<th align=\"left\">Previous Actions:</th>");
			messageBuilder.Append("</tr>");
			foreach (var approval in approvals)
			{
				messageBuilder.Append("<tr>");
				var linkForApproval = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(controllerID, approval.ApprovalPK.ToGuid());
				var linkForPaymentBatch = approval.PaymentBatchPK.IsEmpty ? string.Empty : ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.PaymentBatch, approval.PaymentBatchPK.ToGuid());

				messageBuilder.Append(FormattableString.Invariant($"<td align=\"left\"><a href=\"{linkForApproval}\">{approval.ApprovalName}</a></td>"));
				messageBuilder.Append(FormattableString.Invariant($"<td align=\"left\">{approval.CurrencyCode}</td>"));
				messageBuilder.Append(FormattableString.Invariant($"<td align=\"left\">{approval.Amount}</td>"));
				messageBuilder.Append(FormattableString.Invariant($"<td align=\"left\">{approval.BankCode}</td>"));
				messageBuilder.Append(FormattableString.Invariant($"<td align=\"left\">{approval.ChequeBookBranch}</td>"));
				messageBuilder.Append(FormattableString.Invariant($"<td align=\"left\"><a href=\"{linkForPaymentBatch}\">{approval.PaymentBatch}</td>"));
				messageBuilder.Append(FormattableString.Invariant($"<td align=\"left\">{approval.OrgCode}</td>"));
				messageBuilder.Append(FormattableString.Invariant($"<td align=\"left\">{approval.OrgName}</td>"));
				messageBuilder.Append(FormattableString.Invariant($"<td align=\"left\">{approval.PaymentDate.ToShortDateString()}</td>"));
				messageBuilder.Append(FormattableString.Invariant($"<td align=\"left\">{approval.CreateUserCode}</td>"));
				messageBuilder.Append(FormattableString.Invariant($"<td align=\"left\">{approval.AuthLevel}</td>"));
				messageBuilder.Append(FormattableString.Invariant($"<td align=\"left\">{approval.PrevActions}</td>"));
				messageBuilder.Append("</tr>");
			}
			messageBuilder.Append("</tbody>");
			messageBuilder.Append("</table>");
			messageBuilder.Append("<br/>");
		}

		#endregion

		protected override void DisplayEmailHasNoRecipientsError(string exceptionMessage) => AddExceptionMessageToErrorMessageCollector(exceptionMessage);
		protected override void DisplayEmailNotCompleteError(string exceptionMessage) => AddExceptionMessageToErrorMessageCollector(exceptionMessage);
		protected override void DisplayEmailSendFailedError(string exceptionMessage) => AddExceptionMessageToErrorMessageCollector(exceptionMessage);

		void AddExceptionMessageToErrorMessageCollector(string exceptionMessage) => ErrorMessageCollector.Append(exceptionMessage);

		protected override Guid GetCompanyPK() => CompanyPK;
		protected override Guid GetBranchPK() => Guid.Empty;
		protected override Guid GetDepartmentPK() => Guid.Empty;

		protected override string GetCss() => ZString.Empty;

		readonly Comparison<PaymentApprovalDetails> DetailsComparer = (PaymentApprovalDetails details1, PaymentApprovalDetails details2) => details1.PaymentDate.CompareTo(details2.PaymentDate);

		public struct PaymentApprovalDetails
		{
			public PaymentApprovalDetails(ZString approvalName, ZString companyCode, ZGuid approvalPK, ZString currencyCode, ZString amount, ZString bankCode, ZString orgCode, ZString orgName, ZDateTime paymentDate, ZString authLevel, ZString chequeBookBranch, ZGuid paymentBatchPK, ZString paymentBatch, ZString createUserCode, ZString prevActions)
			{
				ApprovalName = approvalName;
				CompanyCode = companyCode;
				ApprovalPK = approvalPK;
				CurrencyCode = currencyCode;
				Amount = amount;
				BankCode = bankCode;
				OrgCode = orgCode;
				OrgName = orgName;
				PaymentDate = paymentDate;
				AuthLevel = authLevel;
				ChequeBookBranch = chequeBookBranch;
				PaymentBatchPK = paymentBatchPK;
				PaymentBatch = paymentBatch;
				CreateUserCode = createUserCode;
				PrevActions = prevActions;
			}

			public readonly ZString ApprovalName;
			public readonly ZString CompanyCode;
			public readonly ZGuid ApprovalPK;
			public readonly ZString CurrencyCode;
			public readonly ZString Amount;
			public readonly ZString BankCode;
			public readonly ZString OrgCode;
			public readonly ZString OrgName;
			public readonly ZDateTime PaymentDate;
			public readonly ZString AuthLevel;
			public readonly ZString ChequeBookBranch;
			public readonly ZGuid PaymentBatchPK;
			public readonly ZString PaymentBatch;
			public readonly ZString CreateUserCode;
			public readonly ZString PrevActions;

			#region Overrides

			public override bool Equals(object obj) => obj is PaymentApprovalDetails details
				&& details.ApprovalName == ApprovalName
				&& details.CompanyCode == CompanyCode
				&& details.ApprovalPK == ApprovalPK
				&& details.CurrencyCode == CurrencyCode
				&& details.Amount == Amount
				&& details.BankCode == BankCode
				&& details.OrgCode == OrgCode
				&& details.OrgName == OrgName
				&& details.PaymentDate == PaymentDate
				&& details.AuthLevel == AuthLevel
				&& details.ChequeBookBranch == ChequeBookBranch
				&& details.PaymentBatchPK == PaymentBatchPK
				&& details.PaymentBatch == PaymentBatch
				&& details.CreateUserCode == CreateUserCode
				&& details.PrevActions == PrevActions;

			public override int GetHashCode() =>
				ApprovalName.GetHashCode() ^
				CompanyCode.GetHashCode() ^
				ApprovalPK.GetHashCode() ^
				CurrencyCode.GetHashCode() ^
				Amount.GetHashCode() ^
				BankCode.GetHashCode() ^
				OrgCode.GetHashCode() ^
				OrgName.GetHashCode() ^
				PaymentDate.GetHashCode() ^
				AuthLevel.GetHashCode() ^
				ChequeBookBranch.GetHashCode() ^
				PaymentBatchPK.GetHashCode() ^
				PaymentBatch.GetHashCode() ^
				CreateUserCode.GetHashCode() ^
				PrevActions.GetHashCode();

			#endregion

			#region Comparison Operators

			public static bool operator ==(PaymentApprovalDetails detail1, PaymentApprovalDetails detail2)
			{
				return detail1.Equals(detail2);
			}

			public static bool operator !=(PaymentApprovalDetails detail1, PaymentApprovalDetails detail2)
			{
				return !detail1.Equals(detail2);
			}

			#endregion
		}
	}
}
