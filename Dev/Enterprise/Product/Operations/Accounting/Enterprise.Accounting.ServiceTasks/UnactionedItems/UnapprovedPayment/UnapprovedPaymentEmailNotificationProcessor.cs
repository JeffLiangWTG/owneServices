using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.Business.EmailNotification.UnapprovedPaymentNotificationEmail;

namespace Enterprise.Accounting.ServiceTasks
{
	public class UnapprovedPaymentEmailNotificationProcessor : UnactionedItemEmailNotificationProcessor<PaymentApprovalWithAuthorisation>
	{
		public UnapprovedPaymentEmailNotificationProcessor(ILogger logger) : base(logger) { }

		StmALog[] ApprovalLogs;

		protected override ZGuid GetItemPK(PaymentApprovalWithAuthorisation item) => item.PK;
		protected override ZGuid GetItemCompanyPK(PaymentApprovalWithAuthorisation item) => item.Branch?.GB_GC ?? ZGuid.Empty;
		protected override ZString GetItemCompanyName(PaymentApprovalWithAuthorisation item) => item.Branch?.Company?.CompanyName ?? ZString.Empty;
		protected override ZGuid GetItemBranchPK(PaymentApprovalWithAuthorisation item) => item.AV_GB;

		protected override AccountingHtmlEmailDef GetNewNotificationEmail(IEnumerable<PaymentApprovalWithAuthorisation> items, ZStringBuilder errorMessages, ZGuid companyPK)
		{
			var companyName = GetItemCompanyName(items.First());

			var apPaymentDetails = new List<PaymentApprovalDetails>();
			var arPaymentDetails = new List<PaymentApprovalDetails>();
			foreach (var approval in items)
			{
				var logs = ApprovalLogs.Where(x => x.SL_Parent == approval.PK);
				var prevActions = logs.Where(x => x.SL_SE_NKEvent == Events.AuthorisedCode || x.SL_SE_NKEvent == Events.AuthorisationRejectedCode).OrderBy(x => x.SL_EventTime).Select(x => FormattableString.Invariant($"{x.SL_GS_NKUser} ({x.SL_SE_NKEvent})"));

				var details = new PaymentApprovalDetails
				(
					approval.HumanReadableShortcutName,
					companyName,
					approval.PK,
					approval.CurrencyCode,
					approval.OverseasTotalAmount.ToString(approval.CurrencyDecimals),
					approval.BankAccount?.AB_Code ?? ZString.Empty,
					approval.Header?.OH_Code ?? ZString.Empty,
					approval.Header?.OH_FullName ?? ZString.Empty,
					approval.AV_PaymentDate,
					approval.AuthorisationRequired?.AuthorisationRequirementMultilingual ?? ZString.Empty,
					approval.ChequeBook?.Branch?.GB_Code ?? ZString.Empty,
					approval.PaymentBatch?.PK ?? ZGuid.Empty,
					approval.PaymentBatchNumber,
					logs.FirstOrDefault(x => x.SL_SE_NKEvent == Events.AddedARecordToTheSystemCode)?.SL_GS_NKUser ?? ZString.Empty,
					string.Join(", ", prevActions)
				);

				switch (approval.Ledger)
				{
					case LedgerTypes.AccountsPayable:
						apPaymentDetails.Add(details);
						break;
					case LedgerTypes.AccountsReceivable:
						arPaymentDetails.Add(details);
						break;
					default:
						throw new InvalidOperationException(FormattableString.Invariant($@"Payment Approval ""{approval.HumanReadableShortcutName}"" has invalid Ledger Type"));
				}
			}

			return new UnapprovedPaymentNotificationEmail(apPaymentDetails, arPaymentDetails, errorMessages, companyPK.ToGuid());
		}

		protected override IEnumerable<PaymentApprovalWithAuthorisation> GetUnactionedItems()
		{
			var items = Factory.Load(typeof(PaymentApprovalWithAuthorisation), new ZQuery(AccPaymentApprovalSchema.AV_Status, PaymentApprovalStatus.AwaitingApproval)).Cast<PaymentApprovalWithAuthorisation>();
			Factory.AddFetchHint(typeof(AccBankAccount), new ZQuery(AccBankAccountSchema.PK, items.Select(x => x.AV_AB)));
			Factory.AddFetchHint(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.PK, items.Select(x => x.AV_OH)));
			Factory.AddFetchHint(typeof(AccChequeBook), new ZQuery(AccChequeBookSchema.PK, items.Select(x => x.AV_AK)));

			var logsQuery = new ZQuery(StmALogSchema.SL_Parent, items.Select(x => x.PK));
			logsQuery.AddToFilter(new ZQuery(StmALogSchema.SL_SE_NKEvent, new[] { Events.AddedARecordToTheSystemCode, Events.AuthorisedCode, Events.AuthorisationRejectedCode }), JoinCondition.And);
			ApprovalLogs = Factory.Load<StmALog>(logsQuery);

			return items;
		}

		protected override ZBool ShouldSendEmailForCompany(ZGuid companyPK)
		{
			return !AccountingConfigurationRegistry.Instance.PaymentApprovalsNotifyGroup.GetFallBackValueAtAllLevels(companyPK.ToGuid(), Guid.Empty, Guid.Empty).Equals(Guid.Empty);
		}
	}
}
