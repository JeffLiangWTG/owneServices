using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class IndiaGSTReversalHelperTest : TestCaseWithFactory
	{
		public void TestIsTargetTax()
		{
			var tax = Factory.NewWithValidTestData<AccTaxRate>();
			var targetType = new[] {
				AccTaxRate.Types.IntegratedGST ,
				AccTaxRate.Types.Rated
			};

			CombineAssertions("only IntegratedGST(INT) and Rated(RAT) are GST target", () =>
			{
				foreach (var taxType in tax.Lookups.Types.GetAllCodes())
				{
					tax.AT_Type = taxType;
					AssertEquals(targetType.Contains(taxType), IndiaGSTReversalHelper.CheckIsConstraintTaxID(tax));
				}
			});
		}

		[TestDate(2021, 06, 01)]
		public void TestCheckIsOverIndiaFinacialYearEndDatePeriod()
		{
			AssertNotEquals("PreCondition", Core.Constants.CountryCodes.India, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			AccountingConfigurationRegistry.Instance.IndiaGSTReversalAllowedPeriod.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 2);
			AssertEquals("PreCondition", 2, AccountingConfigurationRegistry.Instance.IndiaGSTReversalAllowedPeriod.Value);

			AssertWithCountry("this invoice financial end date belong to 2021-03-31", new ZDateTime(2021, 03, 20), true);
			AssertWithCountry("this invoice financial end date belong to 2022-03-31", new ZDateTime(2021, 04, 01), false);

			void AssertWithCountry(string comment,ZDateTime dateForFinancialYearEnd, bool expectedResult)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
				{
					Env.Security.AllowCreditingIndiaGSTEightMonthsAfterFinancialYearEnd.IsAllowed = false;
					AssertEquals("PreCondition", Core.Constants.CountryCodes.India, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					AssertEquals("PreCondition", 2, AccountingConfigurationRegistry.Instance.IndiaGSTReversalAllowedPeriod.Value);
					AssertEquals($"India Country:true, {comment}", expectedResult, IndiaGSTReversalHelper.CheckIsExceedIndiaFinancialYearEndDatePeriod(dateForFinancialYearEnd, ZDateTime.Now));
				}

				AssertEquals($"India Country:false, {comment}", false, IndiaGSTReversalHelper.CheckIsExceedIndiaFinancialYearEndDatePeriod(dateForFinancialYearEnd, ZDateTime.Now));
			}
		}

		public void TestCheckIsOverIndiaFinacialYearEndDatePeriod_RegistrySepecialValue()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				Env.Security.AllowCreditingIndiaGSTEightMonthsAfterFinancialYearEnd.IsAllowed = false;
				AccountingConfigurationRegistry.Instance.IndiaGSTReversalAllowedPeriod.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 0);
				AssertEquals($"registry:0 mean will always fail after financial year end", true, IndiaGSTReversalHelper.CheckIsExceedIndiaFinancialYearEndDatePeriod(new ZDateTime(2021, 03, 20), new ZDateTime(2021, 04, 01)));
			}
		}

		public void TestARCreditingIndiaGSTEightMonthsAfterFinancialYearEndMessage_ReverseINV()
		 => AssertEquals(@"You cannot reverse this transaction as period to credit GST amounts has lapsed. You can create an amending credit note without GST.

If you must reverse this transaction, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Receivables -> Receivables Transactions -> Allow Crediting India GST Eight months after Financial Year End", IndiaGSTReversalHelper.ARCreditingIndiaGSTEightMonthsAfterFinancialYearEndMessage_ReverseINV);

		public void TestARCreditingIndiaGSTEightMonthsAfterFinancialYearEndMessage_CreditNotdeGST()
		 => AssertEquals(@"You cannot credit GST for this transaction as the period to credit GST amounts has lapsed. You can still post this credit note with a different Tax ID

If you must post this credit note with the same Tax ID, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Receivables -> Receivables Transactions -> Allow Crediting India GST Eight months after Financial Year End", IndiaGSTReversalHelper.ARCreditingIndiaGSTEightMonthsAfterFinancialYearEndMessage_CreditNoteGST);
	}
}
