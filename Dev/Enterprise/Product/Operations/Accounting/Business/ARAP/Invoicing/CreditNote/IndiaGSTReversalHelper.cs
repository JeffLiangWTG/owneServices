using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	internal static class IndiaGSTReversalHelper
	{
		public static bool CheckIsConstraintTaxID(AccTaxRate tax)
		{
			return tax.AT_Type == AccTaxRate.Types.IntegratedGST || tax.AT_Type == AccTaxRate.Types.Rated;
		}

		public static bool CheckIsExceedIndiaFinancialYearEndDatePeriod(ZDateTime dateForFinancialYearEnd, ZDateTime postDate)
		{
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.India
				&& !Env.Security.AllowCreditingIndiaGSTEightMonthsAfterFinancialYearEnd.IsAllowed)
			{
				var offsetLimitMonth = AccountingConfigurationRegistry.Instance.IndiaGSTReversalAllowedPeriod.Value;
				var indiaFinancialYearEnd = dateForFinancialYearEnd.Month <= 3
					? new ZDateTime(dateForFinancialYearEnd.Year, 03, 31)
					: new ZDateTime(dateForFinancialYearEnd.Year + 1, 03, 31);

				return postDate.Date > indiaFinancialYearEnd.AddMonths(offsetLimitMonth);
			}

			return false;
		}

		public static ZString ARCreditingIndiaGSTEightMonthsAfterFinancialYearEndMessage_ReverseINV => Res.GetString("75E7E780-9940-4DF3-BAC7-6B01E0703486", @"You cannot reverse this transaction as period to credit GST amounts has lapsed. You can create an amending credit note without GST.

If you must reverse this transaction, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

{0}", Env.Security.AllowCreditingIndiaGSTEightMonthsAfterFinancialYearEnd.DisplayTextPathToSecurityRight);

		public static ZString ARCreditingIndiaGSTEightMonthsAfterFinancialYearEndMessage_CreditNoteGST => Res.GetString("3565467B-DD04-45D8-8122-69988A898F9A", @"You cannot credit GST for this transaction as the period to credit GST amounts has lapsed. You can still post this credit note with a different Tax ID

If you must post this credit note with the same Tax ID, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

{0}", Env.Security.AllowCreditingIndiaGSTEightMonthsAfterFinancialYearEnd.DisplayTextPathToSecurityRight);
	}
}
