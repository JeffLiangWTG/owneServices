using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Accounting.Business.AccountingUtils;

namespace Enterprise.Accounting.ServiceTasks.Testing
{
	[TestedType(typeof(PeriodClosureServiceTask))]
	public class PeriodClosureServiceTaskTest : ServiceTaskTestCase<PeriodClosureServiceTask>
	{
		public void TestBeginAndEndLogs()
		{
			var serviceTask = new PeriodClosureServiceTask();
			var log = InitialiseAndRunTaskSchedule(serviceTask).ToString();

			var exepectedMessage = @"Information|Period Closure starting.
Information|Period Closure completed.";
			AssertEquals(exepectedMessage, log.Trim());
		}

		[TestDate(2021, 02, 01)]
		public void TestNotActiveBranchForCompany()
		{
			var periodTestHelper = new AccountingPeriodTestHelper();

			var admin1 = Factory.NewWithValidTestData<GlbStaff>();
			admin1.GS_EmailAddress = "admin1@cargowise.com";
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(admin1);
			Factory.Save();

			var company1 = TestObjectCreator.CreateNewCompany("TS1");
			company1.GC_OH_OrgProxy = TestObjectCreator.AALSHI.PK;
			var company2 = TestObjectCreator.CreateNewCompany("TS2");
			company2.GC_OH_OrgProxy = TestObjectCreator.AALSHI.PK;
			var branch1 = TestObjectCreator.CreateNewBranch(company1, "TB1");
			branch1.GB_IsActive = false;
			var branch2 = TestObjectCreator.CreateNewBranch(company2, "TB2");
			branch2.GB_IsActive = true;
			Factory.Save();

			periodTestHelper.PostPeriodsForEntireYear(2021, company1.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			periodTestHelper.PostPeriodsForEntireYear(2021, company2.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			Factory.Save();

			var config = new PeriodClosureConfiguration(PeriodClosureConfigurationIntervalType.Minutes, 1, 0, 0);
			using (AccountingConfigurationRegistry.Instance.AutoPeriodClosureConfiguration.SetTemporaryValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, config))
			using (AccountingConfigurationRegistry.Instance.AutoPeriodClosureConfiguration.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, config))
			using (AccountingConfigurationRegistry.Instance.AutoPeriodClosureNotifyGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			{
				var serviceTask = new PeriodClosureServiceTask();
				var log = InitialiseAndRunTaskSchedule(serviceTask).ToString();
				AssertEquals(@"Information|Period Closure starting.
Error|Cannot find an active branch for company TS1
Debug|Start processing company TS2
Debug|Sub Ledger for Period 202101 of company TS2 has been auto closed.
Debug|End processing company TS2
Information|Period Closure completed.
", log);

				branch1.GB_IsActive = true;
				Factory.Save();
				serviceTask = new PeriodClosureServiceTask();
				log = InitialiseAndRunTaskSchedule(serviceTask).ToString();
				AssertEquals(@"Information|Period Closure starting.
Debug|Start processing company TS1
Debug|Sub Ledger for Period 202101 of company TS1 has been auto closed.
Debug|End processing company TS1
Debug|Start processing company TS2
Debug|End processing company TS2
Information|Period Closure completed.
", log);

				using (Env.Instance.TemporaryServiceTaskContext(PeriodClosureServiceTask.Code, canRunInAnyBranch: true))
				{
					serviceTask.RunTask();
				}
			}
		}

		#region Test when Current Time Larger/Equals/Lower according to interval: Minutes/Hours/Days

		[TestDate(2021, 02, 28, 23, 59, 59)]
		public void TestSetDifferentInterTypeForDifferentCompany_WhenTimeNowLower_Minutes()
		{
			SetUpForDifferentCompany();
			var testCases = new (string Company, int Period, bool SubLedgerClosed, bool GeneralLedgerClosed, bool SubledgerClosedForAdjustments)[]
			{
				("TS1", 202101, true, true, true ),
				("TS1", 202102, false, false, false ),

				("TS2", 202101, true, true, true ),
				("TS2", 202102, false, false, false ),

				("TS3", 202101, true, true, true ),
				("TS3", 202102, false, false, false ),
			};

			AssertEquals("Per condition", false, ZDateTime.Now > GetAccountingPeriodCalculator("TS1").GetLastDayForPeriod(202102).AddMinutes(1));
			AssertEquals("Per condition", false, ZDateTime.Now > GetAccountingPeriodCalculator("TS2").GetLastDayForPeriod(202102).AddHours(1));
			AssertEquals("Per condition", false, ZDateTime.Now > GetAccountingPeriodCalculator("TS3").GetLastDayForPeriod(202102).AddDays(1));

			TestSetDifferentInterTypeForDifferentCompany(testCases);
		}

		[TestDate(2021, 03, 01, 00, 00, 00)]
		public void TestSetDifferentInterTypeForDifferentCompany_WhenTimeNowEquals_Minutes()
		{
			SetUpForDifferentCompany();
			var testCases = new (string Company, int Period, bool SubLedgerClosed, bool GeneralLedgerClosed, bool SubledgerClosedForAdjustments)[]
			{
				("TS1", 202101, true, true, true ),
				("TS1", 202102, true, true, true ),
				("TS1", 202103, false, false, false ),

				("TS2", 202101, true, true, true ),
				("TS2", 202102, false, false, false ),

				("TS3", 202101, true, true, true ),
				("TS3", 202102, false, false, false ),
			};

			AssertEquals("Per condition", true, ZDateTime.Now == GetAccountingPeriodCalculator("TS1").GetLastDayForPeriod(202102).AddMinutes(1));
			AssertEquals("Per condition", false, ZDateTime.Now > GetAccountingPeriodCalculator("TS2").GetLastDayForPeriod(202102).AddHours(1));
			AssertEquals("Per condition", false, ZDateTime.Now > GetAccountingPeriodCalculator("TS3").GetLastDayForPeriod(202102).AddDays(1));

			TestSetDifferentInterTypeForDifferentCompany(testCases);
		}

		[TestDate(2021, 03, 01, 00, 00, 01)]
		public void TestSetDifferentInterTypeForDifferentCompany_WhenTimeNowLarger_Minutes()
		{
			SetUpForDifferentCompany();
			var testCases = new (string Company, int Period, bool SubLedgerClosed, bool GeneralLedgerClosed, bool SubledgerClosedForAdjustments)[]
			{
				("TS1", 202101, true, true, true ),
				("TS1", 202102, true, true, true ),
				("TS1", 202103, false, false, false ),

				("TS2", 202101, true, true, true ),
				("TS2", 202102, false, false, false ),

				("TS3", 202101, true, true, true ),
				("TS3", 202102, false, false, false ),
			};

			AssertEquals("Per condition", true, ZDateTime.Now > GetAccountingPeriodCalculator("TS1").GetLastDayForPeriod(202102).AddMinutes(1));
			AssertEquals("Per condition", false, ZDateTime.Now > GetAccountingPeriodCalculator("TS2").GetLastDayForPeriod(202102).AddHours(1));
			AssertEquals("Per condition", false, ZDateTime.Now > GetAccountingPeriodCalculator("TS3").GetLastDayForPeriod(202102).AddDays(1));

			TestSetDifferentInterTypeForDifferentCompany(testCases);
		}

		[TestDate(2021, 03, 01, 00, 58, 59)]
		public void TestSetDifferentInterTypeForDifferentCompany_WhenTimeNowLower_Hours()
		{
			SetUpForDifferentCompany();
			var testCases = new (string Company, int Period, bool SubLedgerClosed, bool GeneralLedgerClosed, bool SubledgerClosedForAdjustments)[]
			{
				("TS1", 202101, true, true, true ),
				("TS1", 202102, true, true, true ),
				("TS1", 202103, false, false, false ),

				("TS2", 202101, true, true, true ),
				("TS2", 202102, false, false, false ),
				("TS2", 202103, false, false, false ),

				("TS3", 202101, true, true, true ),
				("TS3", 202102, false, false, false ),
			};

			AssertEquals("Per condition", true, ZDateTime.Now > GetAccountingPeriodCalculator("TS1").GetLastDayForPeriod(202102).AddMinutes(1));
			AssertEquals("Per condition", false, ZDateTime.Now > GetAccountingPeriodCalculator("TS2").GetLastDayForPeriod(202102).AddHours(1));
			AssertEquals("Per condition", false, ZDateTime.Now > GetAccountingPeriodCalculator("TS3").GetLastDayForPeriod(202102).AddDays(1));

			TestSetDifferentInterTypeForDifferentCompany(testCases);
		}

		[TestDate(2021, 03, 01, 00, 59, 00)]
		public void TestSetDifferentInterTypeForDifferentCompany_WhenTimeNowEquals_Hours()
		{
			SetUpForDifferentCompany();
			var testCases = new (string Company, int Period, bool SubLedgerClosed, bool GeneralLedgerClosed, bool SubledgerClosedForAdjustments)[]
			{
				("TS1", 202101, true, true, true ),
				("TS1", 202102, true, true, true ),
				("TS1", 202103, false, false, false ),

				("TS2", 202101, true, true, true ),
				("TS2", 202102, true, true, true ),
				("TS2", 202103, false, false, false ),

				("TS3", 202101, true, true, true ),
				("TS3", 202102, false, false, false ),
			};

			AssertEquals("Per condition", true, ZDateTime.Now > GetAccountingPeriodCalculator("TS1").GetLastDayForPeriod(202102).AddMinutes(1));
			AssertEquals("Per condition", true, ZDateTime.Now == GetAccountingPeriodCalculator("TS2").GetLastDayForPeriod(202102).AddHours(1));
			AssertEquals("Per condition", false, ZDateTime.Now > GetAccountingPeriodCalculator("TS3").GetLastDayForPeriod(202102).AddDays(1));

			TestSetDifferentInterTypeForDifferentCompany(testCases);
		}

		[TestDate(2021, 03, 01, 00, 59, 01)]
		public void TestSetDifferentInterTypeForDifferentCompany_WhenTimeNowLarger_Hours()
		{
			SetUpForDifferentCompany();
			var testCases = new (string Company, int Period, bool SubLedgerClosed, bool GeneralLedgerClosed, bool SubledgerClosedForAdjustments)[]
			{
				("TS1", 202101, true, true, true ),
				("TS1", 202102, true, true, true ),
				("TS1", 202103, false, false, false ),

				("TS2", 202101, true, true, true ),
				("TS2", 202102, true, true, true ),
				("TS2", 202103, false, false, false ),

				("TS3", 202101, true, true, true ),
				("TS3", 202102, false, false, false ),
			};

			AssertEquals("Per condition", true, ZDateTime.Now > GetAccountingPeriodCalculator("TS1").GetLastDayForPeriod(202102).AddMinutes(1));
			AssertEquals("Per condition", true, ZDateTime.Now > GetAccountingPeriodCalculator("TS2").GetLastDayForPeriod(202102).AddHours(1));
			AssertEquals("Per condition", false, ZDateTime.Now > GetAccountingPeriodCalculator("TS3").GetLastDayForPeriod(202102).AddDays(1));

			TestSetDifferentInterTypeForDifferentCompany(testCases);
		}

		[TestDate(2021, 03, 01, 23, 58, 59)]
		public void TestSetDifferentInterTypeForDifferentCompany_WhenTimeNowLower_Days()
		{
			SetUpForDifferentCompany();
			var testCases = new (string Company, int Period, bool SubLedgerClosed, bool GeneralLedgerClosed, bool SubledgerClosedForAdjustments)[]
			{
				("TS1", 202101, true, true, true ),
				("TS1", 202102, true, true, true ),
				("TS1", 202103, false, false, false ),

				("TS2", 202101, true, true, true ),
				("TS2", 202102, true, true, true ),
				("TS2", 202103, false, false, false ),

				("TS3", 202101, true, true, true ),
				("TS3", 202102, false, false, false ),
				("TS3", 202103, false, false, false ),
			};

			AssertEquals("Per condition", true, ZDateTime.Now > GetAccountingPeriodCalculator("TS1").GetLastDayForPeriod(202102).AddMinutes(1));
			AssertEquals("Per condition", true, ZDateTime.Now > GetAccountingPeriodCalculator("TS2").GetLastDayForPeriod(202102).AddHours(1));
			AssertEquals("Per condition", false, ZDateTime.Now > GetAccountingPeriodCalculator("TS3").GetLastDayForPeriod(202102).AddDays(1));

			TestSetDifferentInterTypeForDifferentCompany(testCases);
		}

		[TestDate(2021, 03, 01, 23, 59, 00)]
		public void TestSetDifferentInterTypeForDifferentCompany_WhenTimeNowEquals_Days()
		{
			SetUpForDifferentCompany();
			var testCases = new (string Company, int Period, bool SubLedgerClosed, bool GeneralLedgerClosed, bool SubledgerClosedForAdjustments)[]
			{
				("TS1", 202101, true, true, true ),
				("TS1", 202102, true, true, true ),
				("TS1", 202103, false, false, false ),

				("TS2", 202101, true, true, true ),
				("TS2", 202102, true, true, true ),
				("TS2", 202103, false, false, false ),

				("TS3", 202101, true, true, true ),
				("TS3", 202102, true, true, true ),
				("TS3", 202103, false, false, false ),
			};

			AssertEquals("Per condition", true, ZDateTime.Now > GetAccountingPeriodCalculator("TS1").GetLastDayForPeriod(202102).AddMinutes(1));
			AssertEquals("Per condition", true, ZDateTime.Now > GetAccountingPeriodCalculator("TS2").GetLastDayForPeriod(202102).AddHours(1));
			AssertEquals("Per condition", true, ZDateTime.Now == GetAccountingPeriodCalculator("TS3").GetLastDayForPeriod(202102).AddDays(1));

			TestSetDifferentInterTypeForDifferentCompany(testCases);
		}

		[TestDate(2021, 03, 01, 23, 59, 01)]
		public void TestSetDifferentInterTypeForDifferentCompany_WhenTimeNowLarger_Days()
		{
			SetUpForDifferentCompany();
			var testCases = new (string Company, int Period, bool SubLedgerClosed, bool GeneralLedgerClosed, bool SubledgerClosedForAdjustments)[]
			{
				("TS1", 202101, true, true, true ),
				("TS1", 202102, true, true, true ),
				("TS1", 202103, false, false, false ),

				("TS2", 202101, true, true, true ),
				("TS2", 202102, true, true, true ),
				("TS2", 202103, false, false, false ),

				("TS3", 202101, true, true, true ),
				("TS3", 202102, true, true, true ),
				("TS3", 202103, false, false, false ),
			};

			AssertEquals("Per condition", true, ZDateTime.Now > GetAccountingPeriodCalculator("TS1").GetLastDayForPeriod(202102).AddMinutes(1));
			AssertEquals("Per condition", true, ZDateTime.Now > GetAccountingPeriodCalculator("TS2").GetLastDayForPeriod(202102).AddHours(1));
			AssertEquals("Per condition", true, ZDateTime.Now > GetAccountingPeriodCalculator("TS3").GetLastDayForPeriod(202102).AddDays(1));

			TestSetDifferentInterTypeForDifferentCompany(testCases);
		}

		#endregion

		#region Test when set different value on Sub Ledger / General Ledger / Adjustment Ledger Interval

		[TestDate(2021, 03, 01, 00, 00, 00)]
		public void TestWhenOnlySetSubLedgerInterval()
		{
			SetUpForCurrentCompany(2021);

			//Will Only Close Sub Ledger period
			var testCases = new (string Company, int Period, bool SubLedgerClosed, bool GeneralLedgerClosed, bool SubledgerClosedForAdjustments)[]
			{
				("EDI", 202101, true, false, false ),
				("EDI", 202102, true, false, false ),
				("EDI", 202103, false, false, false ),
			};

			var serviceTask = new PeriodClosureServiceTask();
			var configForCurrentCompanyEDI = new PeriodClosureConfiguration(PeriodClosureConfigurationIntervalType.Minutes, 1, 0, 0);

			using (AccountingConfigurationRegistry.Instance.AutoPeriodClosureConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configForCurrentCompanyEDI))
			{
				var log = InitialiseAndRunTaskSchedule(serviceTask).ToString();
				AssertPeriod(testCases, log);
			}
		}

		[TestDate(2021, 03, 01, 00, 00, 00)]
		public void TestWhenSetSubLedgerAndGeneralLedgerInterval()
		{
			SetUpForCurrentCompany(2021);

			//Will Close Sub Ledger & General Ledger period
			var testCases = new (string Company, int Period, bool SubLedgerClosed, bool GeneralLedgerClosed, bool SubledgerClosedForAdjustments)[]
			{
				("EDI", 202101, true, true, false ),
				("EDI", 202102, true, true, false ),
				("EDI", 202103, false, false, false ),
			};

			var serviceTask = new PeriodClosureServiceTask();
			var configForCurrentCompanyEDI = new PeriodClosureConfiguration(PeriodClosureConfigurationIntervalType.Minutes, 1, 1, 0);

			using (AccountingConfigurationRegistry.Instance.AutoPeriodClosureConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configForCurrentCompanyEDI))
			{
				var log = InitialiseAndRunTaskSchedule(serviceTask).ToString();
				AssertPeriod(testCases, log);
			}
		}

		[TestDate(2021, 03, 01, 00, 00, 00)]
		public void TestWhenSetAllInterval()
		{
			SetUpForCurrentCompany(2021);

			//Will Close Sub Ledger & General Ledger & Adjustment Ledger period
			var testCases = new (string Company, int Period, bool SubLedgerClosed, bool GeneralLedgerClosed, bool SubledgerClosedForAdjustments)[]
			{
				("EDI", 202101, true, true, true ),
				("EDI", 202102, true, true, true ),
				("EDI", 202103, false, false, false ),
			};

			var serviceTask = new PeriodClosureServiceTask();
			var configForCurrentCompanyEDI = new PeriodClosureConfiguration(PeriodClosureConfigurationIntervalType.Minutes, 1, 1, 1);

			using (AccountingConfigurationRegistry.Instance.AutoPeriodClosureConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configForCurrentCompanyEDI))
			{
				var log = InitialiseAndRunTaskSchedule(serviceTask).ToString();
				AssertPeriod(testCases, log);
				AssertEquals(
@"Information|Period Closure starting.
Debug|Start processing company EDI
Debug|Sub Ledger for Period 202101 of company EDI has been auto closed.
Debug|Sub Ledger for Period 202102 of company EDI has been auto closed.
Debug|General Ledger for Period 202101 of company EDI has been auto closed.
Debug|General Ledger for Period 202102 of company EDI has been auto closed.
Debug|Adjustment Ledger for Period 202101 of company EDI has been auto closed.
Debug|Adjustment Ledger for Period 202102 of company EDI has been auto closed.
Debug|End processing company EDI
Information|Period Closure completed.
", log);
			}
		}

		[TestDate(2021, 03, 01, 00, 00, 00)]
		public void TestWhenSetNoneIntervalValues()
		{
			SetUpForCurrentCompany(2021);

			//Will not Close Sub Ledger & General Ledger & Adjustment Ledger period
			var testCases = new (string Company, int Period, bool SubLedgerClosed, bool GeneralLedgerClosed, bool SubledgerClosedForAdjustments)[]
			{
				("EDI", 202101, false, false, false ),
				("EDI", 202102, false, false, false ),
				("EDI", 202103, false, false, false ),
			};

			var serviceTask = new PeriodClosureServiceTask();
			var configForCurrentCompanyEDI = new PeriodClosureConfiguration(PeriodClosureConfigurationIntervalType.Minutes, 0, 0, 0);

			using (AccountingConfigurationRegistry.Instance.AutoPeriodClosureConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configForCurrentCompanyEDI))
			{
				var log = InitialiseAndRunTaskSchedule(serviceTask).ToString();
				AssertPeriod(testCases, log);
				AssertEquals(
@"Information|Period Closure starting.
Information|Period Closure completed.
", log);
			}
		}

		#endregion

		#region Test when NextUnClosedSubLedgerPeriod / NextUnClosedGLPeriod / NextUnClosedForAdjustmentsSubLedgerPeriod is null

		[TestDate(2021, 03, 01, 00, 00, 00)]
		public void TestWhenNextUnClosedSubLedgerPeriodIsNull()
		{
			SetUpForCurrentCompany(2020);

			while (PeriodManager.NextUnClosedSubLedgerPeriod != null)
			{
				PeriodManager.CloseSubLedgerPeriod();
			}
			Factory.Save();
			var testCases = new (string Company, int Period, bool SubLedgerClosed, bool GeneralLedgerClosed, bool SubledgerClosedForAdjustments)[]
			{
				("EDI", 202001, true, false, false ),
				("EDI", 202002, true, false, false ),
				("EDI", 202003, true, false, false ),
				("EDI", 202004, true, false, false ),
				("EDI", 202005, true, false, false ),
				("EDI", 202006, true, false, false ),
				("EDI", 202007, true, false, false ),
				("EDI", 202008, true, false, false ),
				("EDI", 202009, true, false, false ),
				("EDI", 202010, true, false, false ),
				("EDI", 202011, true, false, false ),
				("EDI", 202012, true, false, false ),
			};
			AssertPeriod(testCases, string.Empty, true);
			AssertNull("Per condition", PeriodManager.NextUnClosedSubLedgerPeriod);

			var serviceTask = new PeriodClosureServiceTask();
			var configForCurrentCompanyEDI = new PeriodClosureConfiguration(PeriodClosureConfigurationIntervalType.Minutes, 1, 1, 1);
			testCases = new (string Company, int Period, bool SubLedgerClosed, bool GeneralLedgerClosed, bool SubledgerClosedForAdjustments)[]
			{
				("EDI", 202001, true, true, true ),
				("EDI", 202002, true, true, true ),
				("EDI", 202003, true, true, true ),
				("EDI", 202004, true, true, true ),
				("EDI", 202005, true, true, true ),
				("EDI", 202006, true, true, true ),
				("EDI", 202007, true, true, true ),
				("EDI", 202008, true, true, true ),
				("EDI", 202009, true, true, true ),
				("EDI", 202010, true, true, true ),
				("EDI", 202011, true, true, true ),
				("EDI", 202012, true, true, true ),
			};
			using (AccountingConfigurationRegistry.Instance.AutoPeriodClosureConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configForCurrentCompanyEDI))
			{
				var log = InitialiseAndRunTaskSchedule(serviceTask).ToString();
				AssertPeriod(testCases, string.Empty, true);
				AssertEquals(
@"Information|Period Closure starting.
Debug|Start processing company EDI
Debug|General Ledger for Period 202001 of company EDI has been auto closed.
Debug|General Ledger for Period 202002 of company EDI has been auto closed.
Debug|General Ledger for Period 202003 of company EDI has been auto closed.
Debug|General Ledger for Period 202004 of company EDI has been auto closed.
Debug|General Ledger for Period 202005 of company EDI has been auto closed.
Debug|General Ledger for Period 202006 of company EDI has been auto closed.
Debug|General Ledger for Period 202007 of company EDI has been auto closed.
Debug|General Ledger for Period 202008 of company EDI has been auto closed.
Debug|General Ledger for Period 202009 of company EDI has been auto closed.
Debug|General Ledger for Period 202010 of company EDI has been auto closed.
Debug|General Ledger for Period 202011 of company EDI has been auto closed.
Debug|General Ledger for Period 202012 of company EDI has been auto closed.
Debug|Adjustment Ledger for Period 202001 of company EDI has been auto closed.
Debug|Adjustment Ledger for Period 202002 of company EDI has been auto closed.
Debug|Adjustment Ledger for Period 202003 of company EDI has been auto closed.
Debug|Adjustment Ledger for Period 202004 of company EDI has been auto closed.
Debug|Adjustment Ledger for Period 202005 of company EDI has been auto closed.
Debug|Adjustment Ledger for Period 202006 of company EDI has been auto closed.
Debug|Adjustment Ledger for Period 202007 of company EDI has been auto closed.
Debug|Adjustment Ledger for Period 202008 of company EDI has been auto closed.
Debug|Adjustment Ledger for Period 202009 of company EDI has been auto closed.
Debug|Adjustment Ledger for Period 202010 of company EDI has been auto closed.
Debug|Adjustment Ledger for Period 202011 of company EDI has been auto closed.
Debug|Adjustment Ledger for Period 202012 of company EDI has been auto closed.
Debug|End processing company EDI
Information|Period Closure completed.
", log);
			}
		}

		[TestDate(2021, 03, 01, 00, 00, 00)]
		public void TestWhenNextUnClosedGLPeriodIsNull()
		{
			SetUpForCurrentCompany(2020);

			while (PeriodManager.NextUnClosedGLPeriod != null)
			{
				PeriodManager.CloseSubLedgerPeriod();
				PeriodManager.CloseGLPeriod();
			}
			Factory.Save();
			var testCases = new (string Company, int Period, bool SubLedgerClosed, bool GeneralLedgerClosed, bool SubledgerClosedForAdjustments)[]
			{
				("EDI", 202001, true, true, false ),
				("EDI", 202002, true, true, false ),
				("EDI", 202003, true, true, false ),
				("EDI", 202004, true, true, false ),
				("EDI", 202005, true, true, false ),
				("EDI", 202006, true, true, false ),
				("EDI", 202007, true, true, false ),
				("EDI", 202008, true, true, false ),
				("EDI", 202009, true, true, false ),
				("EDI", 202010, true, true, false ),
				("EDI", 202011, true, true, false ),
				("EDI", 202012, true, true, false ),
			};
			AssertPeriod(testCases, string.Empty, true);
			AssertNull("Per condition", PeriodManager.NextUnClosedGLPeriod);

			var serviceTask = new PeriodClosureServiceTask();
			var configForCurrentCompanyEDI = new PeriodClosureConfiguration(PeriodClosureConfigurationIntervalType.Minutes, 1, 1, 1);
			testCases = new (string Company, int Period, bool SubLedgerClosed, bool GeneralLedgerClosed, bool SubledgerClosedForAdjustments)[]
			{
				("EDI", 202001, true, true, true ),
				("EDI", 202002, true, true, true ),
				("EDI", 202003, true, true, true ),
				("EDI", 202004, true, true, true ),
				("EDI", 202005, true, true, true ),
				("EDI", 202006, true, true, true ),
				("EDI", 202007, true, true, true ),
				("EDI", 202008, true, true, true ),
				("EDI", 202009, true, true, true ),
				("EDI", 202010, true, true, true ),
				("EDI", 202011, true, true, true ),
				("EDI", 202012, true, true, true ),
			};
			using (AccountingConfigurationRegistry.Instance.AutoPeriodClosureConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configForCurrentCompanyEDI))
			{
				var log = InitialiseAndRunTaskSchedule(serviceTask).ToString();
				AssertPeriod(testCases, string.Empty, true);
				AssertEquals(
@"Information|Period Closure starting.
Debug|Start processing company EDI
Debug|Adjustment Ledger for Period 202001 of company EDI has been auto closed.
Debug|Adjustment Ledger for Period 202002 of company EDI has been auto closed.
Debug|Adjustment Ledger for Period 202003 of company EDI has been auto closed.
Debug|Adjustment Ledger for Period 202004 of company EDI has been auto closed.
Debug|Adjustment Ledger for Period 202005 of company EDI has been auto closed.
Debug|Adjustment Ledger for Period 202006 of company EDI has been auto closed.
Debug|Adjustment Ledger for Period 202007 of company EDI has been auto closed.
Debug|Adjustment Ledger for Period 202008 of company EDI has been auto closed.
Debug|Adjustment Ledger for Period 202009 of company EDI has been auto closed.
Debug|Adjustment Ledger for Period 202010 of company EDI has been auto closed.
Debug|Adjustment Ledger for Period 202011 of company EDI has been auto closed.
Debug|Adjustment Ledger for Period 202012 of company EDI has been auto closed.
Debug|End processing company EDI
Information|Period Closure completed.
", log);
			}
		}

		[TestDate(2021, 03, 01, 00, 00, 00)]
		public void TestWhenNextUnClosedAdjustmentsSubLedgerPeriodIsNull()
		{
			SetUpForCurrentCompany(2020);

			while (PeriodManager.NextUnClosedForAdjustmentsSubLedgerPeriod != null)
			{
				PeriodManager.CloseSubLedgerPeriod();
				PeriodManager.CloseGLPeriod();
				PeriodManager.CloseGLPeriodForAdjustments();
			}
			Factory.Save();
			var testCases = new (string Company, int Period, bool SubLedgerClosed, bool GeneralLedgerClosed, bool SubledgerClosedForAdjustments)[]
			{
				("EDI", 202001, true, true, true ),
				("EDI", 202002, true, true, true ),
				("EDI", 202003, true, true, true ),
				("EDI", 202004, true, true, true ),
				("EDI", 202005, true, true, true ),
				("EDI", 202006, true, true, true ),
				("EDI", 202007, true, true, true ),
				("EDI", 202008, true, true, true ),
				("EDI", 202009, true, true, true ),
				("EDI", 202010, true, true, true ),
				("EDI", 202011, true, true, true ),
				("EDI", 202012, true, true, true ),
			};
			AssertPeriod(testCases, string.Empty, true);
			AssertNull("Per condition", PeriodManager.NextUnClosedForAdjustmentsSubLedgerPeriod);

			var serviceTask = new PeriodClosureServiceTask();
			var configForCurrentCompanyEDI = new PeriodClosureConfiguration(PeriodClosureConfigurationIntervalType.Minutes, 1, 1, 1);
			using (AccountingConfigurationRegistry.Instance.AutoPeriodClosureConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configForCurrentCompanyEDI))
			{
				var log = InitialiseAndRunTaskSchedule(serviceTask).ToString();
				AssertEquals(
@"Information|Period Closure starting.
Debug|Start processing company EDI
Debug|End processing company EDI
Information|Period Closure completed.
", log);
			}
		}

		#endregion

		#region Test Error Happened When Close Period

		public void TestNotSetRegistryAutoPeriodClosureNotifyGroup()
		{
			var serviceTask = new PeriodClosureServiceTask();
			var configForCurrentCompanyEDI = new PeriodClosureConfiguration(PeriodClosureConfigurationIntervalType.Minutes, 1, 1, 1);
			var configForNonCurrentCompanySIN = new PeriodClosureConfiguration(PeriodClosureConfigurationIntervalType.Hours, 1, 1, 1);
			var configForNonCurrentCompanyDEM = new PeriodClosureConfiguration(PeriodClosureConfigurationIntervalType.Days, 1, 1, 1);
			using (AccountingConfigurationRegistry.Instance.AutoPeriodClosureConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configForCurrentCompanyEDI))
			using (AccountingConfigurationRegistry.Instance.AutoPeriodClosureConfiguration.SetTemporaryValue(TestObjectCreator.NonCurrentNonDemoCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configForNonCurrentCompanySIN))
			using (AccountingConfigurationRegistry.Instance.AutoPeriodClosureConfiguration.SetTemporaryValue(TestObjectCreator.NonCurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configForNonCurrentCompanyDEM))
			{
				var log = InitialiseAndRunTaskSchedule(serviceTask).ToString();
				AssertEquals(@"Information|Period Closure starting.
Error|Please add notify group for company EDI in Accounting -> Email Notification -> Auto Period Closure Notify Group
Error|Please add notify group for company DEM in Accounting -> Email Notification -> Auto Period Closure Notify Group
Error|Please add notify group for company SIN in Accounting -> Email Notification -> Auto Period Closure Notify Group
Information|Period Closure completed.
", log);
			}
		}

		public void TestErrorHappenedWhenClosePeriod()
		{
			SetUpForCurrentCompany(2020);

			DirectReceipt receipt = Factory.NewWithValidTestData<DirectReceipt>();
			receipt.AH_PostDate = new ZDateTime(2020, 2, 5);
			Factory.Save();
			var testCases = new (string Company, int Period, bool SubLedgerClosed, bool GeneralLedgerClosed, bool SubledgerClosedForAdjustments)[]
			{
				("EDI", 202001, false, false, false ),
				("EDI", 202002, false, false, false ),
			};
			AssertPeriod(testCases, string.Empty, true);

			var serviceTask = new PeriodClosureServiceTask();
			var configForCurrentCompanyEDI = new PeriodClosureConfiguration(PeriodClosureConfigurationIntervalType.Minutes, 1, 1, 1);
			using (AccountingConfigurationRegistry.Instance.AutoPeriodClosureConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configForCurrentCompanyEDI))
			{
				var log = InitialiseAndRunTaskSchedule(serviceTask).ToString();
				testCases = new (string Company, int Period, bool SubLedgerClosed, bool GeneralLedgerClosed, bool SubledgerClosedForAdjustments)[]
				{
					("EDI", 202001, true, true, true ),
					("EDI", 202002, false, false, false ),
				};

				AssertPeriod(testCases, log);
				AssertEquals(@"Information|Period Closure starting.
Debug|Start processing company EDI
Debug|Sub Ledger for Period 202001 of company EDI has been auto closed.
Error|Sub Ledger for Period 202002 of company EDI cannot be auto closed. You cannot close the sub-ledger for this period as there are receipts posted in this period that are not part of a deposit batch.
Please create deposit batches for these receipts.
Debug|General Ledger for Period 202001 of company EDI has been auto closed.
Debug|Adjustment Ledger for Period 202001 of company EDI has been auto closed.
Debug|End processing company EDI
Information|Period Closure completed.
", log);
				AssertEquals("Should have sent email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				var emailsCreated = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Sub Ledger for Period 202002 of company EDI cannot be auto closed.", emailsCreated.Subject);
				AssertEquals(@"You cannot close the sub-ledger for this period as there are receipts posted in this period that are not part of a deposit batch.
Please create deposit batches for these receipts.", emailsCreated.Body);
				AssertEquals(2, emailsCreated.Recipients.Count);
			}
		}

		#endregion

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		#region Common Method

		string TestSetDifferentInterTypeForDifferentCompany((string Company, int Period, bool SubLedgerClosed, bool GeneralLedgerClosed, bool SubledgerClosedForAdjustments)[] testCases)
		{
			var admin1 = Factory.NewWithValidTestData<GlbStaff>();
			admin1.GS_EmailAddress = "admin1@cargowise.com";
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(admin1);
			Factory.Save();

			var company1 = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "TS1"));
			var company2 = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "TS2"));
			var company3 = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "TS3"));

			var serviceTask = new PeriodClosureServiceTask();
			var configForCurrentCompany1 = new PeriodClosureConfiguration(PeriodClosureConfigurationIntervalType.Minutes, 1, 1, 1);
			var configForNonCurrentCompany2 = new PeriodClosureConfiguration(PeriodClosureConfigurationIntervalType.Hours, 1, 1, 1);
			var configForNonCurrentCompany3 = new PeriodClosureConfiguration(PeriodClosureConfigurationIntervalType.Days, 1, 1, 1);
			using (DisposableEnvironment.ForCompany(company1.GC_Code))
			using (AccountingConfigurationRegistry.Instance.AutoPeriodClosureConfiguration.SetTemporaryValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, configForCurrentCompany1))
			using (AccountingConfigurationRegistry.Instance.AutoPeriodClosureConfiguration.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, configForNonCurrentCompany2))
			using (AccountingConfigurationRegistry.Instance.AutoPeriodClosureConfiguration.SetTemporaryValue(company3.PK.ToGuid(), Guid.Empty, Guid.Empty, configForNonCurrentCompany3))
			using (AccountingConfigurationRegistry.Instance.AutoPeriodClosureNotifyGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			{
				var log = InitialiseAndRunTaskSchedule(serviceTask).ToString();
				AssertPeriod(testCases, log);

				return log;
			}
		}

		void AssertPeriod((string Company, int Period, bool SubLedgerClosed, bool GeneralLedgerClosed, bool SubledgerClosedForAdjustments)[] testCases, string log, bool ignoreAssertLog = false)
		{
			for (int i = 0; i < testCases.Length; i++)
			{
				var messageForSubLedger = $"Sub Ledger for Period {testCases[i].Period} of company {testCases[i].Company} has been auto closed";
				var messageForGeneralLedger = $"General Ledger for Period {testCases[i].Period} of company {testCases[i].Company} has been auto closed";
				var messageForAdjustmentLedger = $"Adjustment Ledger for Period {testCases[i].Period} of company {testCases[i].Company} has been auto closed";

				ReleaseFactory();
				var calculator = GetAccountingPeriodCalculator(testCases[i].Company);
				AssertEquals(testCases[i].SubLedgerClosed, calculator.IsPeriodSubLedgerClosed(testCases[i].Period));
				AssertEquals(testCases[i].GeneralLedgerClosed, calculator.IsPeriodGLClosed(testCases[i].Period));
				AssertEquals(testCases[i].SubledgerClosedForAdjustments, calculator.IsPeriodSubledgerClosedForAdjustments(testCases[i].Period));

				if (!ignoreAssertLog)
				{
					if (testCases[i].SubLedgerClosed)
					{
						AssertContains(messageForSubLedger, log);
					}
					else
					{
						AssertNotContains(messageForSubLedger, log);
					}

					if (testCases[i].GeneralLedgerClosed)
					{
						AssertContains(messageForGeneralLedger, log);
					}
					else
					{
						AssertNotContains(messageForGeneralLedger, log);
					}

					if (testCases[i].SubledgerClosedForAdjustments)
					{
						AssertContains(messageForAdjustmentLedger, log);
					}
					else
					{
						AssertNotContains(messageForAdjustmentLedger, log);
					}
				}
			}
		}

		AccountingPeriodCalculator GetAccountingPeriodCalculator(string companyCode)
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, companyCode));
			return new AccountingPeriodCalculator(Factory, company);
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
		}

		void SetUpForCurrentCompany(int year)
		{
			var periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.PostPeriodsForEntireYear(year, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);

			var admin1 = TestObjectCreator.CreateStaff("XXX");
			admin1.GS_EmailAddress = "admin1@cargowise.com";
			var admin2 = TestObjectCreator.CreateStaff("YYY");
			admin2.GS_EmailAddress = "admin2@cargowise.com";
			var group = TestObjectCreator.CreateStaffGroup("ZZZ");
			group.Staff.Add(admin1);
			group.Staff.Add(admin2);
			Factory.Save();
			AccountingConfigurationRegistry.Instance.AutoPeriodClosureNotifyGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			Factory.Save();
		}

		void SetUpForDifferentCompany()
		{
			var periodTestHelper = new AccountingPeriodTestHelper();

			var company1 = TestObjectCreator.CreateNewCompany("TS1");
			company1.GC_OH_OrgProxy = TestObjectCreator.AALSHI.PK;
			var company2 = TestObjectCreator.CreateNewCompany("TS2");
			company2.GC_OH_OrgProxy = TestObjectCreator.AALSHI.PK;
			var company3 = TestObjectCreator.CreateNewCompany("TS3");
			company3.GC_OH_OrgProxy = TestObjectCreator.AALSHI.PK;
			var branch1 = TestObjectCreator.CreateNewBranch(company1, "TB1");
			var branch2 = TestObjectCreator.CreateNewBranch(company2, "TB2");
			var branch3 = TestObjectCreator.CreateNewBranch(company3, "TB3");
			Factory.Save();

			periodTestHelper.PostPeriodsForEntireYear(2021, company1.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			periodTestHelper.PostPeriodsForEntireYear(2021, company2.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			periodTestHelper.PostPeriodsForEntireYear(2021, company3.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);

			Factory.Save();
		}

		#endregion

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		PeriodManager PeriodManager => periodManager ?? (periodManager = new PeriodManager(Factory));
		PeriodManager periodManager;
	}
}
