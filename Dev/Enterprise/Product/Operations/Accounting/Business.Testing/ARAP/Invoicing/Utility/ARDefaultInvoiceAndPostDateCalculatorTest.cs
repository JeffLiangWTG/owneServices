using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class ARDefaultInvoiceAndPostDateCalculatorTest : TestCaseWithFactory
	{
		[TestDate(2015, 1, 1)]
		public void TestGetDefaultDate()
		{
			//no config
			AssertEquals(ZDateTime.MinSmallDateTimeValue, ARDefaultInvoiceAndPostDateCalculator.GetDefaultDate(ZDateTime.MinSmallDateTimeValue));

			//default registry
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvAndPstDateDefaultingRuleTypes.Default.Code);
			AssertEquals(ZDateTime.MinSmallDateTimeValue, ARDefaultInvoiceAndPostDateCalculator.GetDefaultDate(ZDateTime.MinSmallDateTimeValue));

			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new ZDateTime(2015, 1, 3).ToDateTime());
			AssertEquals(ZDateTime.MinSmallDateTimeValue, ARDefaultInvoiceAndPostDateCalculator.GetDefaultDate(ZDateTime.MinSmallDateTimeValue));

			//MonthEndSuspension
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvAndPstDateDefaultingRuleTypes.MonthEndSuspension.Code);
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty,
				new ZDateTime(2015, 1, 3).ToDateTime());
			AssertEquals(new ZDateTime(2015, 1, 1), ARDefaultInvoiceAndPostDateCalculator.GetDefaultDate(ZDateTime.MinSmallDateTimeValue));

			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty,
				new ZDateTime(2014, 1, 3).ToDateTime());
			AssertEquals(new ZDateTime(2014, 1, 31), ARDefaultInvoiceAndPostDateCalculator.GetDefaultDate(ZDateTime.MinSmallDateTimeValue));

			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty,
				new ZDateTime(2015, 1, 31).ToDateTime());
			AssertEquals(new ZDateTime(2015, 1, 1), ARDefaultInvoiceAndPostDateCalculator.GetDefaultDate(ZDateTime.MinSmallDateTimeValue));

			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty,
				new ZDateTime(2015, 1, 1).ToDateTime());
			AssertEquals(new ZDateTime(2015, 1, 1), ARDefaultInvoiceAndPostDateCalculator.GetDefaultDate(ZDateTime.MinSmallDateTimeValue));

			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty,
				new ZDateTime(2015, 2, 1).ToDateTime());
			AssertEquals(new ZDateTime(2015, 2, 28), ARDefaultInvoiceAndPostDateCalculator.GetDefaultDate(ZDateTime.MinSmallDateTimeValue));

			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty,
				new ZDateTime(2016, 1, 1).ToDateTime());
			AssertEquals(new ZDateTime(2016, 1, 31), ARDefaultInvoiceAndPostDateCalculator.GetDefaultDate(ZDateTime.MinSmallDateTimeValue));
		}

		[TestDate(2015, 3, 1)]
		public void TestCanClosePeriod()
		{
			var firstPeriod = Factory.New<Period>();
			firstPeriod.AM_Period = 201501;
			firstPeriod.AM_StartDate = new ZDateTime(2015, 1, 1);
			firstPeriod.AM_EndDate = new ZDateTime(2015, 1, 31);

			var secondPeriod = Factory.New<Period>();
			secondPeriod.AM_Period = 201502;
			secondPeriod.AM_StartDate = new ZDateTime(2015, 2, 1);
			secondPeriod.AM_EndDate = new ZDateTime(2015, 2, 28);

			//no config
			AssertEquals(ZDateTime.MinSmallDateTimeValue, ARDefaultInvoiceAndPostDateCalculator.GetDefaultDate(ZDateTime.MinSmallDateTimeValue));
			AssertEquals(string.Empty, ARDefaultInvoiceAndPostDateCalculator.CanClosePeriod(firstPeriod));
			AssertEquals(string.Empty, ARDefaultInvoiceAndPostDateCalculator.CanClosePeriod(secondPeriod));

			//default registry
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvAndPstDateDefaultingRuleTypes.Default.Code);
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new ZDateTime(2015, 1, 3).ToDateTime());
			AssertEquals(ZDateTime.MinSmallDateTimeValue, ARDefaultInvoiceAndPostDateCalculator.GetDefaultDate(ZDateTime.MinSmallDateTimeValue));
			AssertEquals(string.Empty, ARDefaultInvoiceAndPostDateCalculator.CanClosePeriod(firstPeriod));
			AssertEquals(string.Empty, ARDefaultInvoiceAndPostDateCalculator.CanClosePeriod(secondPeriod));

			//MTH
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvAndPstDateDefaultingRuleTypes.MonthEndSuspension.Code);
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new ZDateTime(2015, 1, 3).ToDateTime());
			AssertEquals(new ZDateTime(2015, 1, 31), ARDefaultInvoiceAndPostDateCalculator.GetDefaultDate(ZDateTime.MinSmallDateTimeValue));

			AssertEquals(string.Empty, ARDefaultInvoiceAndPostDateCalculator.CanClosePeriod(secondPeriod));
			var errormsg = ARDefaultInvoiceAndPostDateCalculator.CanClosePeriod(firstPeriod);

			AssertEquals(@"You cannot close the period as current invoice date is within the period.
Please review or reinstate the invoice date before closing the period.", errormsg);
		}

		[TestDate(2015, 1, 1)]
		public void TestCheckSuspensionNeedsToBeLifted()
		{
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, DateTime.MinValue);
			AssertEquals(true, ARDefaultInvoiceAndPostDateCalculator.CheckSuspensionNeedsToBeLifted(ZDateTime.Now, GlbCompany.CurrentCompany));

			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new ZDateTime(2015, 1, 3).ToDateTime());
			AssertEquals(false, ARDefaultInvoiceAndPostDateCalculator.CheckSuspensionNeedsToBeLifted(ZDateTime.Now, GlbCompany.CurrentCompany));

			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new ZDateTime(2015, 2, 1).ToDateTime());
			AssertEquals(true, ARDefaultInvoiceAndPostDateCalculator.CheckSuspensionNeedsToBeLifted(ZDateTime.Now, GlbCompany.CurrentCompany));
		}

		public static void ValidateDefaultARInvoiceAndPostDate(InvoicingBase invoicingBase, Func<InvoicingBase> validationAction)
		{
			if (invoicingBase.AH_Ledger == LedgerTypes.AccountsReceivable &&
				(invoicingBase.AH_TransactionType == TransactionTypes.Invoice || invoicingBase.AH_TransactionType == TransactionTypes.CreditNote || invoicingBase.AH_TransactionType == TransactionTypes.AdjustmentNote))
			{
				var today = ZDateTime.Now;

				//registry not configed
				var invoice = validationAction();
				AssertEquals("should be today", today, invoice.AH_InvoiceDate);
				AssertEquals(false, invoice.AH_InvoiceDate_ReadOnly_Exposed);
				AssertEquals("should be today", today, invoice.AH_PostDate);

				//default registry
				AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvAndPstDateDefaultingRuleTypes.Default.Code);
				invoice = validationAction();
				AssertEquals("should be today", today, invoice.AH_InvoiceDate);
				AssertEquals(false, invoice.AH_InvoiceDate_ReadOnly_Exposed);
				AssertEquals("should be today", today, invoice.AH_PostDate);

				var invoiceDateWarningMsg = @"Invoice Date is non-editable as the Registry 'Invoice and Post Dates Defaulting Behavior' is set to 'MTH - Month End Suspension'.";
				var postDateWarningMsg = @"Post Date is non-editable as the Registry 'Invoice and Post Dates Defaulting Behavior' is set to 'MTH - Month End Suspension'.";

				//MonthEndSuspension configed but hidden registry CurrentInvoiceDate not configed
				AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvAndPstDateDefaultingRuleTypes.MonthEndSuspension.Code);
				AssertEquals(AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.Value, DateTime.MinValue);
				invoice = validationAction();
				AssertEquals("should be today", today, invoice.AH_InvoiceDate);
				AssertEquals(true, invoice.AH_InvoiceDate_ReadOnly_Exposed);
				AssertHasWarning(invoice.AH_InvoiceDateInfo, invoiceDateWarningMsg);

				AssertEquals("should be today", today, invoice.AH_PostDate);
				AssertEquals(true, invoice.AH_PostDate_ReadOnly_Exposed);
				AssertHasWarning(invoice.AH_PostDateInfo, postDateWarningMsg);

				//MonthEndSuspension configed, hidden registry CurrentInvoiceDate configed as current year/month
				AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, today.AddDays(2).ToDateTime());
				invoice = validationAction();
				AssertEquals("should be today", today, invoice.AH_InvoiceDate);
				AssertEquals(true, invoice.AH_InvoiceDate_ReadOnly_Exposed);
				AssertHasWarning(invoice.AH_InvoiceDateInfo, invoiceDateWarningMsg);

				AssertEquals("should be today", today, invoice.AH_PostDate);
				AssertEquals(true, invoice.AH_PostDate_ReadOnly_Exposed);
				AssertHasWarning(invoice.AH_PostDateInfo, postDateWarningMsg);

				//MonthEndSuspension configed, hidden registry CurrentInvoiceDate configed as not current year/month
				var configDateTime = today.AddMonths(1).ToDateTime();
				AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, configDateTime);
				invoice = validationAction();
				AssertEquals("should be the last day of the configed year/month",
					new ZDateTime(configDateTime.Year, configDateTime.Month, DateTime.DaysInMonth(configDateTime.Year, configDateTime.Month))
					, invoice.AH_InvoiceDate);
				AssertEquals(true, invoice.AH_InvoiceDate_ReadOnly_Exposed);
				AssertHasWarning(invoice.AH_InvoiceDateInfo, invoiceDateWarningMsg);

				AssertEquals("should be the last day of the configed year/month",
					new ZDateTime(configDateTime.Year, configDateTime.Month, DateTime.DaysInMonth(configDateTime.Year, configDateTime.Month))
					, invoice.AH_PostDate);
				AssertEquals(true, invoice.AH_PostDate_ReadOnly_Exposed);
				AssertHasWarning(invoice.AH_PostDateInfo, postDateWarningMsg);
			}
			else
			{
				Assert(true);
			}
		}
	}
}
