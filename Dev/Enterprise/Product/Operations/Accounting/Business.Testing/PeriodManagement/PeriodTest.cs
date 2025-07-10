using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.PeriodManagement
{
	[TestedType(typeof(Period))]
	public class PeriodTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSettingEndDateSetsStartDateOfNextPeriodCorrectly()
		{
			ZDateTime endDate = FirstPeriod.AM_EndDate;
			FirstPeriod.AM_EndDate = new ZDateTime(endDate.Year, endDate.Month, endDate.Day, 10, 11, 12);
			AssertEquals("FirstPeriod.AM_EndDate", new ZDateTime(endDate.Year, endDate.Month, endDate.Day, 23, 59, 0), FirstPeriod.AM_EndDate);
			AssertEquals("SecondPeriod.AM_StartDate", FirstPeriod.AM_EndDate.AddMinutes(1), SecondPeriod.AM_StartDate);
			endDate = LastPeriod.AM_EndDate;
			LastPeriod.AM_EndDate = new ZDateTime(endDate.Year, endDate.Month, endDate.Day, 13, 14, 15);
			AssertEquals("LastPeriod.AM_EndDate", new ZDateTime(endDate.Year, endDate.Month, endDate.Day, 23, 59, 0), LastPeriod.AM_EndDate);
			AssertEquals("FirstPeriodOfNextYear.AM_StartDate", LastPeriod.AM_EndDate.AddMinutes(1), FirstPeriodOfNextYear.AM_StartDate);
		}

		[TestDate(1999, 7, 2)]
		public void TestReadOnlyEndDateNOTransaction()
		{
			AssertEquals("Should be Editable", false, FirstPeriod.AM_EndDateInfo.ReadOnly);
			AssertEquals("Should be Editable", false, SecondPeriod.AM_EndDateInfo.ReadOnly);
			AssertEquals("Should be Editable", true, LastPeriod.AM_EndDateInfo.ReadOnly);
			SecondPeriod.AM_IsGeneralLedgerClosed = false;
			SecondPeriod.AM_IsSubLedgerClosed = true;
			AssertEquals("Should NOT be Editable", true, SecondPeriod.AM_EndDateInfo.ReadOnly);
			SecondPeriod.AM_IsGeneralLedgerClosed = true;
			SecondPeriod.AM_IsSubLedgerClosed = false;
			AssertEquals("Should NOT be Editable", true, SecondPeriod.AM_EndDateInfo.ReadOnly);
			SecondPeriod.AM_IsGeneralLedgerClosed = true;
			SecondPeriod.AM_IsSubLedgerClosed = true;
			AssertEquals("Should NOT be Editable", true, SecondPeriod.AM_EndDateInfo.ReadOnly);
			ZDateTime endDate = LastPeriod.AM_EndDate;
			LastPeriod.AM_EndDate = ZDateTime.Today.AddDays(-1);
			AssertEquals("Should be Editable", false, LastPeriod.AM_EndDateInfo.ReadOnly);
			LastPeriod.AM_EndDate = endDate;
			AssertEquals("Should be Editable", true, LastPeriod.AM_EndDateInfo.ReadOnly);
			AccGLAggregate gLAggregate = Factory.NewWithValidTestData<AccGLAggregate>();
			gLAggregate.AA_Period = LastPeriod.AM_Period;
			Factory.Save();
			AssertEquals("Should be Editable", false, LastPeriod.AM_EndDateInfo.ReadOnly);
		}

		public void TestEndDateReadOnlyForNewlyCratedLastPeriod()
		{
			AssertEquals("Precondition: Should not be saved.", false, FirstPeriod.IsInDatabase);
			AssertEquals("Precondition: Should not be saved.", false, SecondPeriod.IsInDatabase);
			AssertEquals("Precondition: Should not be saved.", false, LastPeriod.IsInDatabase);
			AssertEquals("Precondition: Should not be saved.", false, FirstPeriodOfNextYear.IsInDatabase);
			AssertEquals("Precondition: Should not be saved.", false, LastPeriodOfNextYear.IsInDatabase);
			AssertEquals("Only the last Period End Date should be readonly.", false, FirstPeriod.AM_EndDateInfo.ReadOnly);
			AssertEquals("Only the last Period End Date should be readonly.", false, SecondPeriod.AM_EndDateInfo.ReadOnly);
			AssertEquals("Only the last Period End Date should be readonly.", false, FirstPeriodOfNextYear.AM_EndDateInfo.ReadOnly);
			AssertEquals("The last Period End Date should be readonly.", true, LastPeriod.AM_EndDateInfo.ReadOnly);
			AssertEquals("The last Period End Date should be readonly.", true, LastPeriodOfNextYear.AM_EndDateInfo.ReadOnly);
			Factory.Save();
			AssertEquals("If Period is already saved End Date should not be readonly.", false, FirstPeriod.AM_EndDateInfo.ReadOnly);
			AssertEquals("If Period is already saved End Date should not be readonly.", false, SecondPeriod.AM_EndDateInfo.ReadOnly);
			AssertEquals("If Period is already saved End Date should not be readonly.", false, FirstPeriodOfNextYear.AM_EndDateInfo.ReadOnly);
			AssertEquals("If Period is already saved End Date should not be readonly.", false, LastPeriod.AM_EndDateInfo.ReadOnly);
			AssertEquals("If Period is already saved End Date should not be readonly.", false, LastPeriodOfNextYear.AM_EndDateInfo.ReadOnly);
		}

		[TestDate(1999, 7, 2)]
		public void TestEndDateHHMMSS()
		{
			FirstPeriod.AM_EndDate = SecondPeriod.AM_EndDate.AddDays(-1);
			AssertEquals("AM_EndDate Hour should be 23:00", "23", FirstPeriod.AM_EndDate.Hour.ToString());
			AssertEquals("AM_EndDate Minute should be 23:59", "59", FirstPeriod.AM_EndDate.Minute.ToString());
			AssertEquals("AM_EndDate Second should be 23:59:00", "0", FirstPeriod.AM_EndDate.Second.ToString());
		}

		[TestDate(1999, 7, 2)]
		public void TestEditEndDateNOTransaction()
		{
			Factory.Save();
			//End Date of second period is 31/8/1999
			FirstPeriod.AM_EndDate = SecondPeriod.AM_EndDate.AddDays(-1);
			AssertEquals("Update the start date of next period", FirstPeriod.AM_EndDate.AddMinutes(1), SecondPeriod.AM_StartDate);
			LastPeriod.AM_EndDate = FirstPeriodOfNextYear.AM_EndDate.AddDays(-1);
			AssertEquals("Update the start date of next period in next year", LastPeriod.AM_EndDate.AddMinutes(1), FirstPeriodOfNextYear.AM_StartDate);
		}

		[TestDate(2000, 6, 15)]
		public void TestEditPeriodEndDateReadOnlyWhenNextPeriodsExist()
		{
			Factory.Save();
			AssertEquals("Should not be Editable", false, LastPeriod.AM_EndDateInfo.ReadOnly);
			AssertEquals("Should be Editable", false, LastPeriodOfNextYear.AM_EndDateInfo.ReadOnly);
		}

		[TestDate(2002, 6, 15)]
		public void TestEditPeriodEndDateReadOnlyWhenDateHasAlreadyPassed()
		{
			Factory.Save();
			AssertEquals("Should be Editable", false, LastPeriodOfNextYear.AM_EndDateInfo.ReadOnly);
		}

		[TestDate(1999, 7, 2)]
		public void TestNextPeriodAndPreviousPeriodObject()
		{
			Factory.Save();
			AssertEquals("Should be the same", SecondPeriod.AM_StartDate, FirstPeriod.NextPeriod.AM_StartDate);
			AssertEquals("Should be the same", FirstPeriod.AM_StartDate, SecondPeriod.PreviousPeriod.AM_StartDate);
		}

		[TestDate(1999, 7, 2)]
		public void TestFirstPeriodOfNextYearObject()
		{
			AssertEquals("Should be the same", FirstPeriodOfNextYear.AM_StartDate.ToShortDateString(), LastPeriod.AM_EndDate.AddDays(1).ToShortDateString());
		}

		[TestDate(1999, 7, 2)]
		public void TestReadOnlyStartDate()
		{
			AssertEquals("Should NOT be Editable", true, FirstPeriod.AM_StartDateInfo.ReadOnly);
			AssertEquals("Should NOT be Editable", true, SecondPeriod.AM_StartDateInfo.ReadOnly);
			AssertEquals("Should NOT be Editable", true, LastPeriod.AM_StartDateInfo.ReadOnly);
		}

		[TestDate(1999, 7, 2)]
		public void TestInvalidDate()
		{
			FirstPeriod.AM_StartDate = ZDateTime.Invalid;
			AssertEquals("Has error", true, FirstPeriod.AM_StartDateInfo.HasErrors());
			FirstPeriod.AM_EndDate = ZDateTime.Invalid;
			AssertEquals("Has error", true, FirstPeriod.AM_EndDateInfo.HasErrors());
		}

		public void TestModifyEndDateChangesStartDateForNextPeriod()
		{
			AssertEquals("Precondition: FirstPeriod.AM_StartDate", new ZDateTime(1999, 7, 1, 0, 0, 0), FirstPeriod.AM_StartDate);
			AssertEquals("Precondition: FirstPeriod.AM_EndDate", new ZDateTime(1999, 7, 31, 23, 59, 0), FirstPeriod.AM_EndDate);
			AssertEquals("Precondition: SecondPeriod.AM_StartDate", new ZDateTime(1999, 8, 1, 0, 0, 0), SecondPeriod.AM_StartDate);
			AssertEquals("Precondition: SecondPeriod.AM_EndDate", new ZDateTime(1999, 8, 31, 23, 59, 0), SecondPeriod.AM_EndDate);
			FirstPeriod.AM_EndDate = new ZDateTime(1999, 7, 25);
			AssertEquals("FirstPeriod.AM_StartDate", new ZDateTime(1999, 7, 1, 0, 0, 0), FirstPeriod.AM_StartDate);
			AssertEquals("FirstPeriod.AM_EndDate", new ZDateTime(1999, 7, 25, 23, 59, 0), FirstPeriod.AM_EndDate);
			AssertEquals("SecondPeriod.AM_StartDate", new ZDateTime(1999, 7, 26, 0, 0, 0), SecondPeriod.AM_StartDate);
			AssertEquals("SecondPeriod.AM_EndDate", new ZDateTime(1999, 8, 31, 23, 59, 0), SecondPeriod.AM_EndDate);
		}

		public void TestSetDefaultValue()
		{
			AssertEquals("Should be the same", GlbCompany.CurrentCompany.PK, LastPeriod.AM_GC_Company.ToGuid());
		}

		public void TestReadOnlyProperties()
		{
			AssertEquals("Should be ReadOnly", true, LastPeriod.AM_StartDateInfo.ReadOnly);
			AssertEquals("Should be ReadOnly", true, LastPeriod.AM_IsGeneralLedgerClosedInfo.ReadOnly);
			AssertEquals("Should be ReadOnly", true, LastPeriod.AM_IsSubLedgerClosedInfo.ReadOnly);
		}

		public void TestReadonlyForYearAndPeriod()
		{
			using (Env.Instance.SetTemporaryUserContext(new UserContext("CWWeb", Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				AssertEquals("Should not be Editable", true, FirstPeriod.AM_YearInfo.ReadOnly);
				AssertEquals("Should not be Editable", true, FirstPeriod.AM_PeriodInfo.ReadOnly);
			}

			using (Env.Instance.SetTemporaryUserContext(new UserContext("CWSupport", Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				AssertEquals("Should be Editable", false, FirstPeriod.AM_YearInfo.ReadOnly);
				AssertEquals("Should be Editable", false, FirstPeriod.AM_PeriodInfo.ReadOnly);
			}
		}

		public void TestUpdateRestPeriodsInSameYear()
		{
			var currentYearPeriodsManager = TestObjectCreator.CreateTestPeriods(new ZDateTime(2020, 07, 1));
			currentYearPeriodsManager.FinancialYear = 2021;
			var previousYearPeriodsManager = TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 07, 1));
			previousYearPeriodsManager.FinancialYear = 2020;
			Factory.Save();

			AssertEquals(12, previousYearPeriodsManager.Periods.Count);
			AssertEquals(12, currentYearPeriodsManager.Periods.Count);

			previousYearPeriodsManager.Periods[10].AM_Year = 2021;
			previousYearPeriodsManager.Periods[10].AM_Period = 202101;
			Factory.Save();

			var query = new ZDBOnlyQuery(typeof(AccPeriodManagement));
			query.AddToFilter(AccPeriodManagementSchema.AM_Year, (short)2021);
			query.OrderBy = AccPeriodManagementSchema.Constants.AM_Period;
			var currentPeriods = Factory.Load<Period>(query);

			AssertEquals(14, currentPeriods.Length);

			var startPeriod = 202101;
			for (int i = 0; i < 14; i++)
			{
				AssertEquals(startPeriod + i, currentPeriods[i].AM_Period);
			}
		}

		PeriodManager TestPeriodManager;
		PeriodManager TestNextYearPeriodManager;
		Period FirstPeriod;
		Period SecondPeriod;
		Period LastPeriod;
		Period FirstPeriodOfNextYear;
		Period LastPeriodOfNextYear;
		TestObjectCreator TestObjectCreator;
		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory);
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			TestPeriodManager = TestObjectCreator.CreateTestPeriods(new ZDateTime(1999, 7, 1));
			FirstPeriod = TestPeriodManager.Periods[0];
			SecondPeriod = TestPeriodManager.Periods[1];
			LastPeriod = TestPeriodManager.Periods[TestPeriodManager.Periods.Count - 1];
			TestNextYearPeriodManager = TestObjectCreator.CreateTestPeriods(new ZDateTime(2000, 7, 1));
			FirstPeriodOfNextYear = TestNextYearPeriodManager.Periods[0];
			LastPeriodOfNextYear = TestNextYearPeriodManager.Periods[TestNextYearPeriodManager.Periods.Count - 1];
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New(typeof(Period));
		}
	}
}
