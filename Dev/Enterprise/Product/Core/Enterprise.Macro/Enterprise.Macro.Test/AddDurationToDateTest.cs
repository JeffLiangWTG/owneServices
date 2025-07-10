using System;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Macro.Test
{
	sealed class AddDurationToDateTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			PrepareBranchHolidays();
		}

		public void TestAddDurationToDate()
		{
			TestCase("16-FEB-24 08:00", "HOURS", "23", null, null, "17-FEB-24 07:00");
			TestCase("16-FEB-24 08:00", "DAYS", "3", "Y", null, "21-FEB-24 08:00");
			TestCase("16-FEB-24 08:00", "HOURS", "0", null, null, "16-FEB-24 08:00");
			TestCase("16-FEB-24 08:00", "DAYS", "0", "Y", null, "16-FEB-24 08:00");
			TestCase("15-FEB-24 08:00", "DAYS", "2", "Y", "BR1", "21-FEB-24 08:00");
			TestCase("16-FEB-24 08:00", "DAYS", "2", "Y", "BR1", "21-FEB-24 08:00");
			TestCase("17-FEB-24 08:00", "DAYS", "2", "Y", "BR1", "21-FEB-24 08:00");
			TestCase("20-FEB-24 08:00", "DAYS", "2", "Y", "BR2", "22-FEB-24 08:00");
			TestCase("16-FEB-24 22:00", "HOURS", "3", null, null, "17-FEB-24 01:00");
			TestCase("16-FEB-24 08:00", "HOURS", "48", null, null, "18-FEB-24 08:00");
			TestCase("17-FEB-24 08:00", "MINUTES", "48", null, null, "17-FEB-24 08:48");
			TestCase("17-FEB-24 08:00:30", "SECONDS", "48", null, null, "17-FEB-24 08:01:18");
			TestCase("17-FEB-24 08:00:30.100", "MILLISECONDS", "48", null, null, "17-FEB-24 08:00:30.148");
			TestCase("17-FEB-24 07:00", "HOURS", "-23", null, null, "16-FEB-24 08:00");
			TestCase("21-FEB-24 08:00", "DAYS", "-3", "Y", null, "16-FEB-24 08:00");
			TestCase("21-FEB-24 08:00", "DAYS", "-2", "Y", "BR1", "15-FEB-24 08:00");
			TestCase("19-FEB-24 08:00", "DAYS", "-2", "Y", "BR1", "14-FEB-24 08:00");
			TestCase("18-FEB-24 08:00", "DAYS", "-2", "Y", "BR1", "14-FEB-24 08:00");
			TestCase("20-FEB-24 08:00", "DAYS", "-2", "Y", "BR2", "16-FEB-24 08:00");
			TestCase("17-FEB-24 01:00", "HOURS", "-3", null, null, "16-FEB-24 22:00");
			TestCase("18-FEB-24 08:00", "HOURS", "-48", null, null, "16-FEB-24 08:00");
			TestCase("17-FEB-24 08:48", "MINUTES", "-48", null, null, "17-FEB-24 08:00");
			TestCase("17-FEB-24 08:01:18", "SECONDS", "-48", null, null, "17-FEB-24 08:00:30");
			TestCase("17-FEB-24 08:00:30.148", "MILLISECONDS", "-48", null, null, "17-FEB-24 08:00:30.100");
			void TestCase(string date, string span, string value, string workDay, string branch, string expectedDate)
			{
				var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
				dummy.Z0_Date = ParseDateTime(date, span);
				var expected = ParseDateTime(expectedDate, span);
				var expr = $"AddDurationToDate(Z0_Date, \"{span}\", \"{value}\", \"{workDay}\", \"{branch}\")".With(Context).CreateExpression();
				using (var scope = new MacroScope(dummy))
				{
					var result = expr.Evaluate(scope);
					AssertEquals(expected, (ZDateTime)result);
				}
			}
		}

		public void TestAddDurationToDateNoTypeOrBranch()
		{
			TestCase("15-FEB-24 08:00", "DAYS", "2", "Y", "19-FEB-24 08:00", "17-FEB-24 08:00");
			void TestCase(string date, string span, string value, string workDay, string expectedDate1, string expectedDate2)
			{
				var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
				dummy.Z0_Date = ParseDateTime(date, span);
				var expected1 = ParseDateTime(expectedDate1, span);
				var expected2 = ParseDateTime(expectedDate2, span);

				var expr = $"AddDurationToDate(Z0_Date, \"{span}\", \"{value}\", \"{workDay}\")".With(Context).CreateExpression();
				using (var scope = new MacroScope(dummy))
				{
					var result = expr.Evaluate(scope);
					AssertEquals(expected1, (ZDateTime)result);
				}
				var expr2 = $"AddDurationToDate(Z0_Date, \"{span}\", \"{value}\")".With(Context).CreateExpression();
				using (var scope = new MacroScope(dummy))
				{
					var result = expr2.Evaluate(scope);
					AssertEquals(expected2, (ZDateTime)result);
				}
			}
		}

		public void TestAddDurationToHolidayWhenWorkdayOnly()
		{
			TestCase("17-FEB-24 00:00", "DAYS", "1", "Y", null, "19-FEB-24 00:00");
			TestCase("18-FEB-24 08:00", "DAYS", "1", "Y", null, "19-FEB-24 08:00");
			TestCase("16-FEB-24 00:00", "DAYS", "1", "Y", "BR1", "20-FEB-24 00:00");
			TestCase("17-FEB-24 08:00", "DAYS", "1", "Y", "BR1", "20-FEB-24 08:00");
			TestCase("18-FEB-24 08:00", "DAYS", "1", "Y", "BR1", "20-FEB-24 08:00");
			TestCase("19-FEB-24 08:00", "DAYS", "1", "Y", "BR1", "20-FEB-24 08:00");
			TestCase("20-FEB-24 08:00", "DAYS", "1", "Y", "BR2", "21-FEB-24 08:00");
			TestCase("17-FEB-24 00:00", "DAYS", "-1", "Y", null, "16-FEB-24 00:00");
			TestCase("18-FEB-24 08:00", "DAYS", "-1", "Y", null, "16-FEB-24 08:00");
			TestCase("16-FEB-24 00:00", "DAYS", "-1", "Y", "BR1", "15-FEB-24 00:00");
			TestCase("17-FEB-24 08:00", "DAYS", "-1", "Y", "BR1", "15-FEB-24 08:00");
			TestCase("18-FEB-24 08:00", "DAYS", "-1", "Y", "BR1", "15-FEB-24 08:00");
			TestCase("19-FEB-24 08:00", "DAYS", "-1", "Y", "BR1", "15-FEB-24 08:00");
			TestCase("20-FEB-24 08:00", "DAYS", "-1", "Y", "BR2", "19-FEB-24 08:00");
			void TestCase(string date, string span, string value, string workDay, string branch, string expectedDate)
			{
				var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
				dummy.Z0_Date = ParseDateTime(date, span);
				var expected = ParseDateTime(expectedDate, span);
				var expr = $"AddDurationToDate(Z0_Date, \"{span}\", \"{value}\", \"{workDay}\", \"{branch}\")".With(Context).CreateExpression();
				using (var scope = new MacroScope(dummy))
				{
					var result = expr.Evaluate(scope);
					AssertEquals(expected, (ZDateTime)result);
				}
			}
		}

		public void TestAddDurationToDateOnField()
		{
			TestCase("16-FEB-24 08:00", "HOURS", "23", null, null, "17-FEB-24 07:00");
			TestCase("16-FEB-24 08:00", "DAYS", "3", "Y", null, "21-FEB-24 08:00");
			TestCase("16-FEB-24 08:00", "HOURS", "0", null, null, "16-FEB-24 08:00");
			TestCase("16-FEB-24 08:00", "DAYS", "0", "Y", null, "16-FEB-24 08:00");
			TestCase("15-FEB-24 08:00", "DAYS", "2", "Y", "BR1", "21-FEB-24 08:00");
			TestCase("16-FEB-24 08:00", "DAYS", "2", "Y", "BR1", "21-FEB-24 08:00");
			TestCase("17-FEB-24 08:00", "DAYS", "2", "Y", "BR1", "21-FEB-24 08:00");
			TestCase("20-FEB-24 08:00", "DAYS", "2", "Y", "BR2", "22-FEB-24 08:00");
			TestCase("16-FEB-24 22:00", "HOURS", "3", null, null, "17-FEB-24 01:00");
			TestCase("16-FEB-24 08:00", "HOURS", "48", null, null, "18-FEB-24 08:00");
			TestCase("17-FEB-24 08:00", "MINUTES", "48", null, null, "17-FEB-24 08:48");
			TestCase("17-FEB-24 08:00:30", "SECONDS", "48", null, null, "17-FEB-24 08:01:18");
			TestCase("17-FEB-24 08:00:30.100", "MILLISECONDS", "48", null, null, "17-FEB-24 08:00:30.148");
			TestCase("17-FEB-24 07:00", "HOURS", "-23", null, null, "16-FEB-24 08:00");
			TestCase("21-FEB-24 08:00", "DAYS", "-3", "Y", null, "16-FEB-24 08:00");
			TestCase("21-FEB-24 08:00", "DAYS", "-2", "Y", "BR1", "15-FEB-24 08:00");
			TestCase("19-FEB-24 08:00", "DAYS", "-2", "Y", "BR1", "14-FEB-24 08:00");
			TestCase("18-FEB-24 08:00", "DAYS", "-2", "Y", "BR1", "14-FEB-24 08:00");
			TestCase("20-FEB-24 08:00", "DAYS", "-2", "Y", "BR2", "16-FEB-24 08:00");
			TestCase("17-FEB-24 01:00", "HOURS", "-3", null, null, "16-FEB-24 22:00");
			TestCase("18-FEB-24 08:00", "HOURS", "-48", null, null, "16-FEB-24 08:00");
			TestCase("17-FEB-24 08:48", "MINUTES", "-48", null, null, "17-FEB-24 08:00");
			TestCase("17-FEB-24 08:01:18", "SECONDS", "-48", null, null, "17-FEB-24 08:00:30");
			TestCase("17-FEB-24 08:00:30.148", "MILLISECONDS", "-48", null, null, "17-FEB-24 08:00:30.100");
			void TestCase(string date, string span, string value, string workDay, string branch, string expectedDate)
			{
				var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
				dummy.Z0_Date = ParseDateTime(date, span);
				var expected = ParseDateTime(expectedDate, span);

				var expr = $"AddDurationToDate(\"{span}\", \"{value}\", \"{workDay}\", \"{branch}\")".With(Context).CreateExpression();
				using (var scope = new MacroScope(dummy.Z0_Date))
				{
					var result = expr.Evaluate(scope);
					AssertEquals(expected, (ZDateTime)result);
				}
			}
		}

		public void TestAddDurationToDateOnFieldNoTypeOrBranch()
		{
			TestCase("15-FEB-24 08:00", "DAYS", "2", "Y", "19-FEB-24 08:00", "17-FEB-24 08:00");
			void TestCase(string date, string span, string value, string workDay, string expectedDate1, string expectedDate2)
			{
				var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
				dummy.Z0_Date = ParseDateTime(date, span);
				var expected1 = ParseDateTime(expectedDate1, span);
				var expected2 = ParseDateTime(expectedDate2, span);

				var expr = $"AddDurationToDate(\"{span}\", \"{value}\", \"{workDay}\")".With(Context).CreateExpression();
				using (var scope = new MacroScope(dummy.Z0_Date))
				{
					var result = expr.Evaluate(scope);
					AssertEquals(expected1, (ZDateTime)result);
				}
				var expr2 = $"AddDurationToDate(\"{span}\", \"{value}\")".With(Context).CreateExpression();
				using (var scope = new MacroScope(dummy.Z0_Date))
				{
					var result = expr2.Evaluate(scope);
					AssertEquals(expected2, (ZDateTime)result);
				}
			}
		}

		public void TestAddDurationToDateValidation()
		{
			TestCase(null, "HOURS", "23", null, null, "Date is empty.");
			TestCase("16-FEB-24 08:00", "", "23", null, null, "Span is empty.");
			TestCase("16-FEB-24 08:00", "HOURS", "", null, null, "Value is empty.");
			TestCase("16-FEB-24 08:00", "Months", "23", null, null, "'Months' is not a valid time span unit.");
			TestCase("16-FEB-24 08:00", "HOURS", "2x", null, null, "'2x' is not a valid number.");
			TestCase("16-FEB-24 08:00", "DAYS", "2", "XX", null, "The WorkDay must be 'Y' or 'N'.");
			TestCase("16-FEB-24 08:00", "DAYS", "3", "Y", "xxx", "'xxx' is not a valid branch code.");
			TestCase("16-FEB-24 08:00", "HOURS", "2", "Y", "BR1", "The 'WORKDAY' option is only valid when using 'DAYS' as the time span unit.");
			TestCase("15-FEB-24 08:00", "MINUTES", "2", "Y", "BR1", "The 'WORKDAY' option is only valid when using 'DAYS' as the time span unit.");
			TestCase("15-FEB-24 08:01:18", "SECONDS", "2", "Y", "BR1", "The 'WORKDAY' option is only valid when using 'DAYS' as the time span unit.");
			TestCase("15-FEB-24 08:00:30.148", "MILLISECONDS", "2", "Y", "BR1", "The 'WORKDAY' option is only valid when using 'DAYS' as the time span unit.");
			void TestCase(string date, string span, string value, string workDay, string branch, string expectedError)
			{
				var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
				dummy.Z0_Date = !string.IsNullOrEmpty(date) ? ParseDateTime(date, span) : ZDateTime.Empty;

				var expr = $"AddDurationToDate(\"{span}\", \"{value}\", \"{workDay}\", \"{branch}\")".With(Context).CreateExpression();
				using (var scope = new MacroScope(dummy.Z0_Date))
				{
					var result = expr.Evaluate(scope);

					var notifications = expr.Errors
						.Select(err => err.Message)
						.ToArray();
					Assert(notifications.Contains(expectedError));
				}
			}
		}

		public void TestAddDurationToDateWhenEveryDayIsHoliday()
		{
			TestCase("01-JAN-24 08:00", "DAYS", "2", "Y", "BR3", "Holiday setting error: exceeded maximum allowed iterations while checking holidays.");
			void TestCase(string date, string span, string value, string workDay, string branch, string expectedError)
			{
				var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
				dummy.Z0_Date = !string.IsNullOrEmpty(date) ? ParseDateTime(date, span) : ZDateTime.Empty;
				PrepareBranchHolidaysForMaxHolidays(dummy.Z0_Date);

				var expr = $"AddDurationToDate(\"{span}\", \"{value}\", \"{workDay}\", \"{branch}\")".With(Context).CreateExpression();
				using (var scope = new MacroScope(dummy.Z0_Date))
				{
					var result = expr.Evaluate(scope);

					var notifications = expr.Errors
						.Select(err => err.Message)
						.ToArray();
					Assert(notifications.Contains(expectedError));
				}
			}
		}

		void PrepareBranchHolidays()
		{
			var company = Factory.New<GlbCompany>();

			var glbBranch1 = Factory.New<GlbBranch>();
			glbBranch1.GB_Code = "BR1";
			glbBranch1.GB_GC = company.PK;
			var holiday11 = glbBranch1.GlbHolidays.AddNew();
			var holiday12 = glbBranch1.GlbHolidays.AddNew();

			var glbBranch2 = Factory.New<GlbBranch>();
			glbBranch2.GB_Code = "BR2";
			glbBranch2.GB_GC = company.PK;
			var holiday21 = glbBranch2.GlbHolidays.AddNew();

			holiday11.GH_Date = new ZDateTime(2024, 02, 16);

			holiday12.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Third;
			holiday12.GH_RecurrDay = DayOfWeekCodeList.Codes.Monday;
			holiday12.GH_RecurrMonth = "FEB";

			holiday21.GH_Date = new ZDateTime(2024, 02, 20);

			Factory.Save();
		}

		void PrepareBranchHolidaysForMaxHolidays(ZDateTime date)
		{
			var company = Factory.New<GlbCompany>();

			var glbBranch3 = Factory.New<GlbBranch>();
			glbBranch3.GB_Code = "BR3";
			glbBranch3.GB_GC = company.PK;
			var onlyDate = date.ToDateTime().Date;

			for (var i = 0; i <= AddDurationToDateHelper.MaxHolidaysForwardCount; i++)
			{
				var holiday = glbBranch3.GlbHolidays.AddNew();
				holiday.GH_Date = onlyDate.AddDays(i);
			}
			Factory.Save();
		}

		ZDateTime ParseDateTime(string dateStr, string span)
		{
			var format = "dd-MMM-yy HH:mm";

			switch (span)
			{
				case "SECONDS":
					format = "dd-MMM-yy HH:mm:ss";
					break;
				case "MILLISECONDS":
					format = "dd-MMM-yy HH:mm:ss.fff";
					break;
			}

			return DateTime.ParseExact(dateStr, format, CultureInfo.InvariantCulture);
		}

		IMacroEvaluationContext Context
		{
			get
			{
				if (context == null)
				{
					context = new IMacroLibrary[]
					{
						new CargoWiseOneStandardLibrary()
					}
					.CreateContext();
				}

				return context;
			}
		}
		IMacroEvaluationContext context;
	}
}
