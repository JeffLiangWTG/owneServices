using System;
using System.Collections.Generic;
using System.Globalization;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.EmailNotification
{
	public class CreditLimitExceededEmail : CreditLimitEmail
	{
		public CreditLimitExceededEmail(IEnumerable<InvoicingBase> invoices, decimal creditLimit, decimal balanceOverLimit, bool isLocal = true, RefCurrency globalCurrency = null)
			: base(invoices, creditLimit, isLocal, globalCurrency)
		{
			BalanceOverLimit = balanceOverLimit;
		}

		protected readonly decimal BalanceOverLimit;

		protected override string GetCreditLimitHeaderMessage()
		{
			return String.Format(CultureInfo.InvariantCulture, (NoResString)"{0} / {1} {2}credit limit of {3} {4}{5} has been exceeded by {3} {4}{6}",
				Header.OH_FullName,
				Header.OH_Code,
				IsLocal ? string.Empty : (NoResString)"Global ",
				Currency.RX_Code,
				Currency.RX_Symbol,
				Utilities.FormatNumberWithGroupSeparators(CreditLimit, Currency.Decimals, CultureInfo.InvariantCulture),
				Utilities.FormatNumberWithGroupSeparators(BalanceOverLimit, Currency.Decimals, CultureInfo.InvariantCulture));
		}

		protected override string GetSubjectCore()
		{
			return String.Format(CultureInfo.InvariantCulture, (NoResString)"{0}Credit Limit Exceeded by {1} {2}{3} ({4} / {5})",
				IsLocal ? string.Empty : (NoResString)"Global ",
				Currency.RX_Code,
				Currency.RX_Symbol,
				Utilities.FormatNumberWithGroupSeparators(BalanceOverLimit, Currency.Decimals, CultureInfo.InvariantCulture),
				Header.OH_FullName,
				Header.OH_Code);
		}

		protected override string GetCreditLimitGrantedEmailNotificationNote()
		{
			var notes = AccountingConfigurationRegistry.Instance.ARExceedCreditLimitGrantedEmailNotificationNote.Value;

			return Ledger == LedgerTypes.AccountsReceivable && !notes.IsEmpty
				? (NoResString)"<p>" + notes + (NoResString)"</p>"
				: string.Empty;
		}
	}
}
