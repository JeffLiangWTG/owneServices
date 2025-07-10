using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.EmailNotification
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Email should always be in English, not Multilingual")]
	public abstract class CreditLimitEmail : AccountingEmailDef
	{
		protected CreditLimitEmail(IEnumerable<InvoicingBase> invoices, decimal creditLimit, bool isLocal, RefCurrency globalCurrency)
		{
			var firstInvoice = invoices.First();
			if (invoices != null && invoices.Any() && firstInvoice.Header != null)
			{
				IsLocal = isLocal;
				ContentType = EmailContentTypes.HTML;
				CreditLimit = creditLimit;
				Header = firstInvoice.Header;
				Ledger = firstInvoice.AH_Ledger;
				Invoices = invoices;
				Currency = IsLocal ? GlbCompany.CurrentCompany.LocalCurrency : globalCurrency;
			}
		}

		#region Implementation

		protected readonly bool IsLocal;
		protected readonly decimal CreditLimit;
		protected readonly string Ledger;
		protected readonly OrgHeader Header;
		protected readonly RefCurrency Currency;
		readonly IEnumerable<InvoicingBase> Invoices;

		string GetBodyCore()
		{
			OrgHeader creditApprovalOrganisation;
			var groupInfoMessage = GetGroupInfoMessage(out creditApprovalOrganisation);

			return string.Format(CultureInfo.InvariantCulture, @"<p>{0}</p>
<p>{1}</p>
<p>{2}</p>
{3}
<p>{4}</p>",
				GetCreditLimitMessage(),
				groupInfoMessage,
				GetCreditApprovedStatusMessage(creditApprovalOrganisation),
				GetCreditLimitGrantedEmailNotificationNote(),
				GetFooterMessage());
		}

		string GetCreditLimitMessage()
		{
			var message = GetCreditLimitHeaderMessage();

			if (Invoices.Count() == 1)
			{
				var firstInvoice = Invoices.First();
				message += string.Format(CultureInfo.InvariantCulture, " with the creation of {0} {1} {2} {3}dated {4} for {5} {6}{7} posted {8} by {9}.",
					firstInvoice.AH_Ledger,
					firstInvoice.AH_TransactionType,
					firstInvoice.AH_TransactionNum,
					firstInvoice.JobNumber.IsEmpty ? string.Empty : string.Format(CultureInfo.InvariantCulture, "(Job {0}) ", firstInvoice.JobNumber),
					firstInvoice.AH_InvoiceDate.ToShortDateString(),
					firstInvoice.AH_RX_NKTransactionCurrency,
					firstInvoice.TransactionCurrency.RX_Symbol,
					Utilities.FormatNumberWithGroupSeparators(Math.Abs(firstInvoice.AH_OSTotal), firstInvoice.TransactionCurrency.Decimals, CultureInfo.InvariantCulture),
					((ZDateTime)Env.Time.CurrentLocalDate).ToShortDateString(),
					Env.CurrentUser.FullName);
			}
			else
			{
				message += string.Format(CultureInfo.InvariantCulture, @" with the creation of {0} Invoices posted on {1} by {2}. The invoices are:
{3}",
					Invoices.Count(),
					((ZDateTime)Env.Time.CurrentLocalDate).ToShortDateString(),
					Env.CurrentUser.FullName,
					CreateInvoiceList());
			}

			return message;
		}

		string GetGroupInfoMessage(out OrgHeader creditApprovalOrganisation)
		{
			return IsLocal ? GetLocalGroupInfoMessage(out creditApprovalOrganisation) : GetGlobalGroupInfoMessage(out creditApprovalOrganisation);
		}

		string GetLocalGroupInfoMessage(out OrgHeader creditApprovalOrganisation)
		{
			string message = string.Empty;

			if (Header.IsSettlementGroup(Ledger))
			{
				message += string.Format(CultureInfo.InvariantCulture, "{0} is a settlement group.", Header.OH_Code.Trim());
				creditApprovalOrganisation = Header;
			}
			else if ((Ledger == LedgerTypes.AccountsPayable && Header.APSettlementGroupPK.IsValid)
				|| (Ledger == LedgerTypes.AccountsReceivable && Header.ARSettlementGroupPK.IsValid && Header.CompanyData.OB_ARUseSettlementGroupCreditLimit))
			{
				var settlementGroup = Ledger == LedgerTypes.AccountsReceivable ? Header.ARSettlementGroup : Header.APSettlementGroup;
				var settlementGroupCode = settlementGroup != null ? settlementGroup.OH_Code.Trim() : ZString.Empty;
				message += string.Format(CultureInfo.InvariantCulture, "{0} is configured to use the credit limit of settlement group {1}.", Header.OH_Code.Trim(), settlementGroupCode);
				creditApprovalOrganisation = settlementGroup;
			}
			else
			{
				creditApprovalOrganisation = Header;
			}

			return message;
		}

		string GetGlobalGroupInfoMessage(out OrgHeader creditApprovalOrganisation)
		{
			string message = string.Empty;

			if (OrgHeaderExtensions.IsGlobalCreditGroupChild(Header))
			{
				creditApprovalOrganisation = Header.MiscServ.ARGlobalCreditGroup;
				message += string.Format(CultureInfo.InvariantCulture, "{0} is configured to use the credit limit of Global Credit Group {1}.", Header.OH_Code.Trim(), creditApprovalOrganisation.OH_Code.Trim());
			}
			else if (OrgHeaderExtensions.IsGlobalCreditGroupParent(Header))
			{
				creditApprovalOrganisation = Header;
				message += string.Format(CultureInfo.InvariantCulture, "{0} is a Global Group.", Header.OH_Code.Trim());
			}
			else
			{
				creditApprovalOrganisation = Header;
			}

			return message;
		}

		string GetCreditApprovedStatusMessage(OrgHeader creditApprovalOrganisation)
		{
			return creditApprovalOrganisation != null && creditApprovalOrganisation.CompanyData.OB_ARCreditApproved ?
				string.Format(CultureInfo.InvariantCulture, "Credit approved.") : string.Format(CultureInfo.InvariantCulture, "Credit pending approval.");
		}

		string GetFooterMessage()
		{
			return string.Format(CultureInfo.InvariantCulture, @"See <a href=""{0}"">{1} / {2}</a> for more details.",
				ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.OrgCollectionCalls, Header.CompanyData.PK.ToGuid()),
				Header.OH_FullName,
				Header.OH_Code);
		}

		string CreateInvoiceList()
		{
			string message = "<ul>";
			foreach (InvoicingBase invoice in Invoices)
			{
				message += "\n" + string.Format(CultureInfo.InvariantCulture, "<li>{0} {1} {2} created {3}</li>", Ledger, invoice.AH_TransactionType, invoice.AH_TransactionNum, invoice.AH_InvoiceDate.ToShortDateString());
			}
			message += "\n</ul>";
			return message;
		}

		GuidRegistryItem LocalRecipient => Ledger == LedgerTypes.AccountsReceivable
			? AccountingConfigurationRegistry.Instance.DebtorCreditLimitNotifyGroup
			: AccountingConfigurationRegistry.Instance.CreditorCreditLimitNotifyGroup;

		GuidRegistryItem GlobalRecipient => AccountingMasterFilesRegistry.Instance.DebtorGlobalCreditLimitNotifyGroup;

		#endregion

		#region Overrides

		protected override GuidRegistryItem Recipient => LocalRecipient;

		protected override string GetBody()
		{
			return GetBodyCore();
		}

		protected override string GetSubject()
		{
			return GetSubjectCore();
		}

		protected override void SendCore()
		{
			if (!IsLocal)
			{
				AddGroupOfRecipientsOrAllUsersIfGroupIsEmpty(GlobalRecipient.Value, GlobalRecipient);
			}
			base.SendCore();
		}

		#endregion

		#region Abstract methods

		protected abstract string GetCreditLimitHeaderMessage();

		protected abstract string GetSubjectCore();

		#endregion

		protected virtual string GetCreditLimitGrantedEmailNotificationNote()
		{
			return string.Empty;
		}
	}
}
