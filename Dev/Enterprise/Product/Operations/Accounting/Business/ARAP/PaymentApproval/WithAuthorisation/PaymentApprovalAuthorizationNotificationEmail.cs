using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using static System.FormattableString;
using ActionType = Enterprise.Accounting.Business.ARAP.PaymentApproval.PaymentApprovalWithAuthorisation.ActionType;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	#region SuppressResourceStringsCheckRegion

	public class PaymentApprovalAuthorizationNotificationEmail : AccountingHtmlEmailDef
	{
		public PaymentApprovalAuthorizationNotificationEmail(ActionType action, PaymentApprovalWithAuthorisation payment)
		{
			var addLog = payment.Logs.AddedLog;

			this.creatorsGS_Code = payment.Logs.AddedLog?.SL_GS_NKUser ?? string.Empty;
			this.action = action;
			this.paymentDate = payment.AV_PaymentDate.ToShortDateString();
			this.rejectionReasonCode = payment.AV_RejectionReasonCode;
			this.rejectionReasonDetails = payment.AV_RejectionReasonDetails;
			this.paymentPK = payment.PK;
			this.AddRecipientForUserCommunication(addLog?.User?.EmailAddress);
			this.isAP = payment is APPaymentApprovalWithAuthorisation;
			this.createdOn = addLog?.SL_EventTime.ToBestReadableDateTimeString() ?? string.Empty;
			this.AR_AP = isAR ? "AR" : "AP";
			this.org = payment.Header?.OH_Code;
			this.currency = payment.AV_RX_NKPaymentCurrency;
			this.amount = payment.AV_Amount.ToString(payment.RXDecimals);

			switch (action)
			{
				case ActionType.Reject:
					actionText = "Rejected";
					break;

				case ActionType.Authorize:
					actionText = "Approved";
					break;

				case ActionType.Unauthorize:
					actionText = "Unauthorized";
					break;

				case ActionType.Cancel:
					actionText = "Cancelled";
					break;
			}
		}

		readonly ZString creatorsGS_Code;
		readonly string createdOn, amount, currency, actionText, AR_AP, org, paymentDate, rejectionReasonCode, rejectionReasonDetails;
		readonly ActionType action;
		readonly ZGuid paymentPK;
		readonly bool isAP;
		bool isAR => !isAP;

		readonly ZStringBuilder errorMessageCollector = new ZStringBuilder();

		protected override void SendCore()
		{
			if (creatorsGS_Code != GlbStaff.CurrentUser.GS_Code)
			{
				var factory = new BusinessObjectFactory() { NameForDebugging = "PaymentApprovalAuthorizationNotificationEmail" };
				Env.OutgoingMailManager.CreateAndSave(this, factory);
			}
		}

		protected override string GetSubject()
		{
			var description = Invariant($"{org} {currency} {amount}");
			return Invariant($"{AR_AP} Payment Request {actionText} - {description}");
		}

		[SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
		protected override string GetBody()
		{
			string bold(string text) => Invariant($@"<span style=""font-weight:bold;"">{text}</span>");
			string paragragh(string text) => Invariant($"<p>{text}</p>");

			var lines = new List<string>
			{
				Invariant($"The following {AR_AP} Payment Request was {actionText.ToLowerInvariant()}:"),
				GetLinkToPaymentRequest(),
				Invariant($"{bold("Created on:")} {createdOn}"),
				Invariant($"{bold("Payment Amount:")} {amount} {currency}"),
				Invariant($"{bold("Organization:")} {org}")
			};

			if (action == ActionType.Reject)
			{
				lines.Add(Invariant($"{bold("Rejection Reason:")} {rejectionReasonCode}"));
				lines.Add(Invariant($"{bold("Rejection Details:")} {rejectionReasonDetails}"));
			}

			return "<br/>" + new ZStringBuilder(lines.Select(paragragh)).ToStringWithNewLineBetweenAppends();
		}

		protected string GetLinkToPaymentRequest()
		{
			var text = Invariant($@"{org} - {paymentDate} - {AR_AP} PAYMENT");
			var controllerID = isAP ? ControllerIDs.APPaymentProcessing : ControllerIDs.ARPaymentProcessing;
			var link = ObjectFactory.Get<IShowViewFormUrlCreator>().Create(controllerID, paymentPK.ToGuid());

			return string.IsNullOrWhiteSpace(link)
				? text
				: Invariant($@"<a href=""{link}"">{text}</a>");
		}

		protected override string GetCss()
		{
			return string.Empty;
		}

		protected override Guid GetCompanyPK()
		{
			return GlbCompany.CurrentCompany.PK.ToGuid();
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

		void AddExceptionMessageToErrorMessageCollector(string exceptionMessage)
		{
			errorMessageCollector.Append(exceptionMessage);
		}

		protected override GuidRegistryItem Recipient { get; }
		public override ZString EmailDescription => ZString.Empty;
	}

	#endregion
}
