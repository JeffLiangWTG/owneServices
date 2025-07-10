using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.EmailNotification
{
	public class PeriodReopenedEmailTest : MasterFiles.Business.Testing.AccountingEmailDefTest
	{
		protected override Type EmailDefType
		{
			get
			{
				return typeof(PeriodReopenedEmail);
			}
		}

		[TestDate(2017, 09, 28)]
		public void TestSubject()
		{
			Period period = Factory.NewWithValidTestData<Period>();
			period.AM_Period = 200909;
			PeriodReopenedEmail actualMail = new PeriodReopenedEmail(period);
			var expectedSubject = $"Accounting Period 200909 of {GlbCompany.CurrentCompany.GC_Name} ({GlbCompany.CurrentCompany.GC_Code}) has been reopened.";
			AssertEquals("Email Subject", expectedSubject, GetSubject(actualMail));
		}

		[TestDate(2018, 07, 14)]
		public void TestBody()
		{
			Period period = Factory.NewWithValidTestData<Period>();
			period.AM_Period = 200909;
			period.AM_StartDate = new ZDateTime(2009, 09, 01);
			period.AM_EndDate = new ZDateTime(2009, 09, 28);
			period.AM_IsGeneralLedgerClosed = true;
			period.AM_IsSubLedgerClosed = true;
			period.AM_IsSubledgerClosedForAdjustments = true;
			Factory.Save();
			period.AM_IsGeneralLedgerClosed = false;
			period.AM_IsSubLedgerClosed = false;
			period.AM_IsSubledgerClosedForAdjustments = false;
			PeriodReopenedEmail actualMail = new PeriodReopenedEmail(period);
			var expectedSubject = $@"Accounting Period 200909 of {GlbCompany.CurrentCompany.GC_Name} ({GlbCompany.CurrentCompany.GC_Code}) has been reopened by CargoWise Support (E).
- The Sub Ledger has been reopened
- The General Ledger has been reopened
- This period has been reopened for Adjustments

Current Settings:
Period: 200909
Start Date: 01-Sep-09
End Date: 28-Sep-09
Sub Ledger is Closed: No
General Ledger is closed: No
Closed for Adjustments: No";
			AssertMultilineASCIIEquals("Email Body", expectedSubject, GetBody(actualMail));
			period.AM_IsGeneralLedgerClosed = true;
			period.AM_IsSubLedgerClosed = true;
			period.AM_IsSubledgerClosedForAdjustments = true;
			Factory.Save();
			period.AM_IsSubLedgerClosed = false;
			expectedSubject = $@"Accounting Period 200909 of {GlbCompany.CurrentCompany.GC_Name} ({GlbCompany.CurrentCompany.GC_Code}) has been reopened by CargoWise Support (E).
- The Sub Ledger has been reopened

Current Settings:
Period: 200909
Start Date: 01-Sep-09
End Date: 28-Sep-09
Sub Ledger is Closed: No
General Ledger is closed: Yes
Closed for Adjustments: Yes";
			actualMail = new PeriodReopenedEmail(period);
			AssertMultilineASCIIEquals("Email Body", expectedSubject, GetBody(actualMail));
		}
	}
}
