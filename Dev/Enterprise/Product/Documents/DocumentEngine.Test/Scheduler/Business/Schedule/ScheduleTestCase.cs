using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing
{
	abstract class ScheduleTestCase<T> : NonPersistentBusinessObjectTestCase where T : Schedule
	{
		public void TestIsThisPeriodScope()
		{
			Schedule.PeriodScope = "";
			AssertEquals("IsThisPeriodScope", false, Schedule.IsThisPeriodScope);
			Schedule.PeriodScope = PeriodScopeList.Codes.This;
			AssertEquals("IsThisPeriodScope", true, Schedule.IsThisPeriodScope);
		}

		public void TestCloneAndCopyChangesFrom()
		{
			Schedule.PeriodScope = "x";
			Schedule.PeriodCount = 1;

			T clone = (T)Schedule.Clone();
			AssertEquals("clone.PeriodScope", "x", clone.PeriodScope);
			AssertEquals("clone.PeriodNumber", (ZInt)1, clone.PeriodCount);
			AssertNoErrors(clone);
			AssertEquals("clone.HasChanges", false, clone.HasChanges);

			clone.PeriodScope = "y";
			clone.PeriodCount = 2;
			Schedule.CopyChangesFrom(clone);
			AssertEquals("PeriodScope", "y", Schedule.PeriodScope);
			AssertEquals("PeriodNumber", (ZInt)2, Schedule.PeriodCount);
		}

		public void TestPeriodScope()
		{
			Schedule.PeriodCount = 3;
			Schedule.PeriodScope = PeriodScopeList.Codes.Next;
			AssertEquals("PeriodNumber", (ZShort)3, Schedule.PeriodCount);
			Schedule.PeriodScope = PeriodScopeList.Codes.This;
			AssertEquals("PeriodNumber", ZShort.Zero, Schedule.PeriodCount);
		}

		public void TestPeriodNumberInfoReadOnly()
		{
			Schedule.PeriodScope = PeriodScopeList.Codes.This;
			AssertEquals("PeriodNumberInfo.ReadOnly", true, Schedule.PeriodCountInfo.ReadOnly);
			Schedule.PeriodScope = PeriodScopeList.Codes.Next;
			AssertEquals("PeriodNumberInfo.ReadOnly", false, Schedule.PeriodCountInfo.ReadOnly);
		}

		[ExpectNoExceptions]
		public void TestRunPreSaveValidation()
		{
			var mockSchedule = new Mock<T>();
			var mockValidation = new Mock<ScheduleValidation>(mockSchedule.Object);
			mockSchedule.Protected().Setup<ScheduleValidation>("GetNewValidation").Returns(mockValidation.Object);
			mockValidation.Setup(m => m.ValidateAll());
			mockSchedule.Object.RunPreSaveValidation();
			mockValidation.VerifyAll();
		}

		[TestUtcOffset(2, 0, 0)]
		public void TestBaseDateTime()
		{
			AssertEquals("BaseDateTime", ZDateTime.Empty, Schedule.BaseDateTimeForTesting);

			ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			Schedule.ScheduleTask = scheduleTask;
			scheduleTask.CalcStartDateLocal = new ZDateTime(2001, 1, 3);
			AssertEquals("BaseDateTime", new ZDateTime(2001, 1, 3), Schedule.BaseDateTimeForTesting);

			scheduleTask.CalcNextRunTimeLocal = new ZDateTime(2004, 1, 3);
			AssertEquals("BaseDateTime", new ZDateTime(2004, 1, 3), Schedule.BaseDateTimeForTesting);
		}

		[TestUtcOffset(-2, 0, 0)]
		public void TestClearDate()
		{
			AssertEquals("BaseDateTime", ZDateTime.Empty, Schedule.BaseDateTimeForTesting);

			ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			Schedule.ScheduleTask = scheduleTask;

			scheduleTask.CalcNextRunTimeLocal = new ZDateTime(2004, 1, 3);
			AssertEquals("BaseDateTime", new ZDateTime(2004, 1, 3), Schedule.BaseDateTimeForTesting);

			Schedule.ClearDate();
			AssertEquals("BaseDateTime cleared", ZDateTime.Empty, Schedule.BaseDateTimeForTesting);
		}

		public void TestPeriodsToAdd()
		{
			Schedule.PeriodScope = PeriodScopeList.Codes.This;
			AssertEquals("PeriodsToAdd", 0, Schedule.PeriodsToAddForTesting);

			Schedule.PeriodScope = PeriodScopeList.Codes.Previous;
			Schedule.PeriodCount = 2;
			AssertEquals("PeriodsToAdd", -2, Schedule.PeriodsToAddForTesting);

			Schedule.PeriodScope = PeriodScopeList.Codes.Next;
			AssertEquals("PeriodsToAdd", 2, Schedule.PeriodsToAddForTesting);
		}

		public void TestValidRange()
		{
			AssertEquals("ValidRange()", false, Business.Schedule.ValidRangeForTesting(DateTime.MinValue));
			AssertEquals("ValidRange()", false, Business.Schedule.ValidRangeForTesting(DateTime.MaxValue));
			AssertEquals("ValidRange()", true, Business.Schedule.ValidRangeForTesting(new DateTime(1901, 8, 2)));
			AssertEquals("ValidRange()", true, Business.Schedule.ValidRangeForTesting(new DateTime(1901, 9, 2)));
			AssertEquals("ValidRange()", true, Business.Schedule.ValidRangeForTesting(new DateTime(1918, 6, 3)));
			AssertEquals("ValidRange()", true, Business.Schedule.ValidRangeForTesting(new DateTime(1918, 5, 3)));
			AssertEquals("ValidRange()", true, Business.Schedule.ValidRangeForTesting(new DateTime(1918, 5, 3, 12, 2, 44)));
		}

		public void TestPopulatePeriodScopeAndNumber()
		{
			Schedule.PopulatePeriodScopeAndNumberForTesting(new DateTime(1910, 1, 5));
			AssertEquals(PeriodScopeList.Codes.This, Schedule.PeriodScope);
			AssertEquals("PeriodNumber", ZInt.Zero, Schedule.PeriodCount);

			Schedule.PopulatePeriodScopeAndNumberForTesting(new DateTime(1909, 10, 2));
			AssertEquals(PeriodScopeList.Codes.Previous, Schedule.PeriodScope);
			AssertEquals("PeriodNumber", (ZInt)3, Schedule.PeriodCount);

			Schedule.PopulatePeriodScopeAndNumberForTesting(new DateTime(1910, 5, 1));
			AssertEquals(PeriodScopeList.Codes.Next, Schedule.PeriodScope);
			AssertEquals("PeriodNumber", (ZInt)4, Schedule.PeriodCount);

			Schedule.PopulatePeriodScopeAndNumberForTesting(new DateTime(1951, 9, 4));
			AssertEquals(PeriodScopeList.Codes.Next, Schedule.PeriodScope);
			AssertEquals("PeriodNumber", (ZInt)500, Schedule.PeriodCount);

			Schedule.PopulatePeriodScopeAndNumberForTesting(new DateTime(1868, 5, 4));
			AssertEquals(PeriodScopeList.Codes.Previous, Schedule.PeriodScope);
			AssertEquals("PeriodNumber", (ZInt)500, Schedule.PeriodCount);

			Schedule.PopulatePeriodScopeAndNumberForTesting(new DateTime(2743, 4, 4));
			AssertEquals(PeriodScopeList.Codes.Next, Schedule.PeriodScope);
			AssertEquals("PeriodNumber", (ZInt)9999, Schedule.PeriodCount);

			Schedule.PopulatePeriodScopeAndNumberForTesting(new DateTime(1076, 10, 4));
			AssertEquals(PeriodScopeList.Codes.Previous, Schedule.PeriodScope);
			AssertEquals("PeriodNumber", (ZInt)9999, Schedule.PeriodCount);
		}

		public void TestHasChangesAndHasErrorsAreFalseOnCreation()
		{
			AssertEquals("HasChanges", false, Schedule.HasChanges);
			AssertEquals("HasErrors", false, Schedule.HasErrors);
		}

		public virtual void TestIsValid()
		{
			AssertEquals("HasErrors", false, Schedule.HasErrors);
			AssertEquals("IsValid", true, Schedule.IsValid);

			Schedule.PeriodScope = PeriodScopeList.Codes.Next;
			Schedule.PeriodCount = Business.Schedule.MaxPeriodCount + 1;
			AssertEquals("IsValid", false, Schedule.IsValid);

			Schedule.FillWithValidTestData();
			AssertEquals("IsValid", true, Schedule.IsValid);
		}

		public void TestToStorageValueWillReportErrorIfNotIsValid()
		{
			try
			{
				var expectedExceptionMessage = "Some date filters are invalid. Please check their value!";
				var stmMenuItem = Factory.NewWithValidTestData<StmMenuItem>();
				var task = Factory.NewWithValidTestData<ReportScheduleTask>();

				stmMenuItem.SU_BusinessContext = "REPCUSTOMSREPORT";
				stmMenuItem.SU_MenuName = "DECLARATION PROFILE REPORT";
				stmMenuItem.SU_MenuPath = ".";
				stmMenuItem.SU_ContactType = "NCT";
				task.S5_ParentID = stmMenuItem.PK;

				Factory.Save();

				Schedule.ScheduleTask = task;
				Schedule.PeriodScope = "Next";
				Schedule.PeriodCount = Business.Schedule.MaxPeriodCount + 1;
				var expectedReportedMessage = $"ToStorageValue for report details [REPCUSTOMSREPORT:DECLARATION PROFILE REPORT:.:NCT] cannot be called when IsValid is false.The PeriodScope is:Next , and The PeriodCount is:{Schedule.PeriodCount}";

				AssertExceptionThrown<InvalidOperationException>(expectedExceptionMessage, () => Schedule.ToStorageValue());
				AssertEquals(expectedReportedMessage, ErrorReporter.LastMessageReported);
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestCalculatorWithRealScheduleTaskPicksUpScheduleTaskCompany()
		{
			ReportScheduleTask task = Factory.NewWithValidTestData<ReportScheduleTask>();
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			task.S5_GB = branch.PK;
			branch.GB_GC = company.PK;
			Factory.Save();

			Schedule.ScheduleTask = task;
			AssertEquals("Calculator", company.PK, Schedule.CalculatorForTesting.scheduledTaskCompany.PK);
		}

		public void TestCalculatorNotNullEvenIfScheduleTaskIsNull()
		{
			Schedule.ScheduleTask = null;
			AssertNotNull("Calculator", Schedule.CalculatorForTesting);
		}

		public virtual void TestClear()
		{
			Schedule.PeriodScope = "x";
			Schedule.PeriodCount = 3;
			Schedule.Clear();
			AssertEquals("PeriodScope", "", Schedule.PeriodScope);
			AssertEquals("PeriodNumber", ZInt.Zero, Schedule.PeriodCount);
		}

		protected T Schedule
		{
			get
			{
				if (schedule == null)
				{
					schedule = (T)GetNewBusinessObject();
				}
				return schedule;
			}
		}

		public abstract void TestDescription();
		public abstract void TestToStorageValue();
		T schedule;
	}
}
