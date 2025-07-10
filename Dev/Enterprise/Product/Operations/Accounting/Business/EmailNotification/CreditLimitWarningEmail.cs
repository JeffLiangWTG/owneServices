using System;
using System.Collections.Generic;
using System.Globalization;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.EmailNotification
{
	public class CreditLimitWarningEmail : CreditLimitEmail
	{
		public CreditLimitWarningEmail(IEnumerable<InvoicingBase> invoices, decimal creditLimit, int creditThreshold, bool isLocal = true, RefCurrency globalCurrency = null)
			: base(invoices, creditLimit, isLocal, globalCurrency)
		{
			CreditThreshold = creditThreshold;
		}

		protected readonly decimal CreditThreshold;

		protected override string GetCreditLimitHeaderMessage()
		{
			return String.Format(CultureInfo.InvariantCulture, (NoResString)"{0} / {1} is within {2}% of {3}credit limit of {4} {5}{6}",
				Header.OH_FullName,
				Header.OH_Code,
				CreditThreshold,
				IsLocal ? string.Empty : (NoResString)"global ",
				Currency.RX_Code,
				Currency.RX_Symbol,
				Utilities.FormatNumberWithGroupSeparators(CreditLimit, Currency.Decimals, CultureInfo.InvariantCulture));
		}

		protected override string GetSubjectCore()
		{
			return String.Format(CultureInfo.InvariantCulture, (NoResString)"{0}Credit Limit Warning - {1} / {2} within {3}% of {0}Credit Limit",
				IsLocal ? string.Empty : (NoResString)"Global ",
				Header.OH_FullName,
				Header.OH_Code,
				CreditThreshold);
		}
	}
}
