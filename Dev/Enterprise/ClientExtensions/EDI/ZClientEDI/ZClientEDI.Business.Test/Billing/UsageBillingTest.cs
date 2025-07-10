using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(UsageBilling))]
	public class UsageBillingTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2010, 11, 11)]
		public void TestDateToDefaulting()
		{
			UsageBilling monthlyBilling = new UsageBilling(new BusinessObjectFactory());
			AssertEquals(new ZDateTime(2010, 10, 31), monthlyBilling.DateTo);

			TestDateAttribute.Date = new DateTime(2010, 12, 31);
			monthlyBilling = new UsageBilling(new BusinessObjectFactory());
			AssertEquals(new ZDateTime(2010, 11, 30), monthlyBilling.DateTo);
		}

		[TestDate(2010, 11, 11)]
		public void TestMinimumAmountToBill()
		{
			EDIDataRegistry.Instance.MinimumAmountToBill.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 15m);
			UsageBilling monthlyUsageBilling = new UsageBilling(new BusinessObjectFactory());
			AssertEquals(15m, monthlyUsageBilling.MinimumAmountToBill);
		}

		[TestDate(2015, 6, 7)]
		public void TestUpdateIsBackPostAvailable()
		{
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			var branch1 = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var branch3 = Factory.NewWithValidTestData<GlbBranch>();
			var company1 = branch1.Company;
			var company2 = branch2.Company;
			var company3 = branch3.Company;
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			Factory.Save();

			var currentMonth = new ZDateTime(TestDateAttribute.Date.Year, TestDateAttribute.Date.Month, 1);
			var lastMonth = currentMonth.AddMonths(-1);

			var company1CurrentPeriod = periodHelper.SetupSinglePeriod(201506, currentMonth, currentMonth.AddMonths(1).AddDays(-1), company1.PK);
			var company1LastPeriod = periodHelper.SetupSinglePeriod(201505, lastMonth, lastMonth.AddMonths(1).AddDays(-1), company1.PK);

			var company2CurrentPeriod = periodHelper.SetupSinglePeriod(201506, currentMonth, currentMonth.AddMonths(1).AddDays(-1), company2.PK);
			var company2LastPeriod = periodHelper.SetupSinglePeriod(201505, lastMonth, lastMonth.AddMonths(1).AddDays(-1), company2.PK);

			var company3CurrentPeriod = periodHelper.SetupSinglePeriod(201506, currentMonth, currentMonth.AddMonths(1).AddDays(-1), company3.PK);
			var company3LastPeriod = periodHelper.SetupSinglePeriod(201505, lastMonth, lastMonth.AddMonths(1).AddDays(-1), company3.PK);

			company1LastPeriod.AM_IsSubLedgerClosed = true;
			company2LastPeriod.AM_IsSubLedgerClosed = false;
			company3LastPeriod.AM_IsSubLedgerClosed = false;

			var billing = new UsageBilling(Factory);
			billing.UpdateIsBackPostAvailable(new[] { branch1 });
			AssertEquals("IsBackPostAvailable all companies closed", false, billing.IsBackPostAvailable);

			billing = new UsageBilling(Factory);
			billing.UpdateIsBackPostAvailable(new[] { branch1, branch2 });
			AssertEquals("IsBackPostAvailable some companies closed", false, billing.IsBackPostAvailable);

			billing = new UsageBilling(Factory);
			billing.UpdateIsBackPostAvailable(new[] { branch2, branch3 });
			AssertEquals("IsBackPostAvailable all companies open", true, billing.IsBackPostAvailable);

			billing.UpdateIsBackPostAvailable(new[] { branch1, branch2, branch3 });
			AssertEquals("IsBackPostAvailable some companies closed", false, billing.IsBackPostAvailable);

			TestDateAttribute.Date = new DateTime(2015, 6, 30);
			billing = new UsageBilling(Factory);
			billing.DateTo = new DateTime(2015, 6, 30);
			billing.UpdateIsBackPostAvailable(new[] { branch2 });
			AssertEquals("IsBackPostAvailable all companies open, billing month is current month", false, billing.IsBackPostAvailable);
		}
	}
}
