using System;
using System.Data;
using System.Text.RegularExpressions;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.Scheduler.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(DateTimeOffsetRangeField))]
	class DateTimeOffsetRangeFieldTest : FitlerFieldTestWithClearValues
	{
		[TestUtcOffset(-8, 0, 0)]
		[TestDate(2024, 7, 4)]
		public void TestConvertToUtc()
		{
			Field.FieldName = "Z0_DateTime";
			Field.ValueLow = new ZDateTimeOffset(2008, 6, 2, 0, 0, 0, TimeSpan.FromHours(-8));
			Field.ValueHigh = new ZDateTimeOffset(2010, 8, 28, 0, 0, 0, TimeSpan.FromHours(-8));
			Field.ConvertToUtc = false;
			{
				var parameters = Field.SqlParameters();
				var fromDate = new ZDateTimeOffset(parameters[0].Value);
				var toDate = new ZDateTimeOffset(parameters[1].Value);

				AssertEquals(new ZDateTimeOffset(2008, 6, 2, 0, 0, 0, TimeSpan.FromHours(-8)), fromDate);
				AssertEquals(TimeSpan.FromHours(-8), fromDate.Offset);
				AssertEquals(new ZDateTimeOffset(2010, 8, 29, 0, 0, 0, TimeSpan.FromHours(-8)), toDate);
				AssertEquals(TimeSpan.FromHours(-8), toDate.Offset);
			}

			Field.ConvertToUtc = true;
			{
				var parameters = Field.SqlParameters();
				var fromDate = new ZDateTimeOffset(parameters[0].Value);
				var toDate = new ZDateTimeOffset(parameters[1].Value);

				AssertEquals(new ZDateTimeOffset(2008, 6, 2, 8, 0, 0, TimeSpan.FromHours(0)), fromDate);
				AssertEquals(TimeSpan.FromHours(0), fromDate.Offset);
				AssertEquals(new ZDateTimeOffset(2010, 8, 29, 8, 0, 0, TimeSpan.FromHours(0)), toDate);
				AssertEquals(TimeSpan.FromHours(0), toDate.Offset);
			}

			Field.ConvertToUtc = false;
			{
				var parameters = Field.SqlParameters();
				var fromDate = new ZDateTimeOffset(parameters[0].Value);
				var toDate = new ZDateTimeOffset(parameters[1].Value);

				AssertEquals(new ZDateTimeOffset(2008, 6, 2, 0, 0, 0, TimeSpan.FromHours(-8)), fromDate);
				AssertEquals(TimeSpan.FromHours(-8), fromDate.Offset);
				AssertEquals(new ZDateTimeOffset(2010, 8, 29, 0, 0, 0, TimeSpan.FromHours(-8)), toDate);
				AssertEquals(TimeSpan.FromHours(-8), toDate.Offset);
			}
		}

		public void TestCalculateToDateAccordingDateFormat()
		{
			Field.PickerFormat = DocEngineDatePickerFormats.Long;
			Field.ValueLow = new ZDateTimeOffset(2008, 6, 2, 5, 6, 0, new TimeSpan(8, 0, 0));
			Field.ValueHigh = new ZDateTimeOffset(2010, 8, 28, 5, 6, 0, new TimeSpan(8, 0, 0));
			{
				var parameters = Field.SqlParameters();
				var fromDate = (DateTimeOffset)parameters[0].Value;
				var toDate = (DateTimeOffset)parameters[1].Value;

				AssertEquals(new DateTimeOffset(2008, 6, 2, 5, 6, 0, new TimeSpan(8, 0, 0)), fromDate);
				AssertEquals(new DateTimeOffset(2010, 8, 28, 5, 7, 0, new TimeSpan(8, 0, 0)), toDate);
			}

			Field.PickerFormat = DocEngineDatePickerFormats.Short;
			Field.ValueLow = new ZDateTimeOffset(2008, 6, 2, 0, 0, 0, new TimeSpan(8, 0, 0));
			Field.ValueHigh = new ZDateTimeOffset(2010, 8, 28, 0, 0, 0, new TimeSpan(8, 0, 0));
			{
				var parameters = Field.SqlParameters();
				var fromDate = (DateTimeOffset)parameters[0].Value;
				var toDate = (DateTimeOffset)parameters[1].Value;

				AssertEquals(new DateTimeOffset(2008, 6, 2, 0, 0, 0, new TimeSpan(8, 0, 0)), fromDate);
				AssertEquals(new DateTimeOffset(2010, 8, 29, 0, 0, 0, new TimeSpan(8, 0, 0)), toDate);
			}

			var reportScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			reportScheduleTask.Recurrence.WeeklyRange = ZBool.True;
			Field.SetScheduleTask(reportScheduleTask);
			Field.PickerFormat = DocEngineDatePickerFormats.Long;
			Field.ValueLow = new ZDateTimeOffset(2008, 6, 2, 0, 0, 0, new TimeSpan(8, 0, 0)); //Scheduled reports only support short date selection and doesnt support to date time level
			Field.ValueHigh = new ZDateTimeOffset(2010, 8, 28, 0, 0, 0, new TimeSpan(8, 0, 0)); //Scheduled reports only support short date selection and doesnt support to date time level
			{
				var parameters = Field.SqlParameters();
				var fromDate = (DateTimeOffset)parameters[0].Value;
				var toDate = (DateTimeOffset)parameters[1].Value;

				AssertEquals(new ZDateTimeOffset(2008, 6, 2, 0, 0, 0, new TimeSpan(8, 0, 0)), fromDate);
				AssertEquals(new ZDateTimeOffset(2010, 8, 29, 0, 0, 0, new TimeSpan(8, 0, 0)), toDate);
			}
		}

		public void TestSubstituteMinDateMaxDateJsonConverter()
		{
			Field.FieldName = "MyDate";
			Field.DisplayName = "My Date";
			Field.ValueLow = new ZDateTimeOffset(2008, 8, 8, 0, 0, 0, new TimeSpan(8, 0, 0));
			Field.SubstituteMinDateForNullFrom = true;
			Field.SubstituteMaxDateForNullTo = true;

			var result = JsonConverterHelper.Serialize(Field);
			var deserialisedField = JsonConverterHelper.Deserialize<DateTimeOffsetRangeField>(result);

			AssertEquals("deserialisedField.FieldName", "MyDate", deserialisedField.FieldName);
			AssertEquals("deserialisedField.SubstituteMinDateForNullFrom", Field.SubstituteMinDateForNullFrom, deserialisedField.SubstituteMinDateForNullFrom);
			AssertEquals("deserialisedField.SubstituteMaxDateForNullTo", Field.SubstituteMaxDateForNullTo, deserialisedField.SubstituteMaxDateForNullTo);

			Field.SubstituteMinDateForNullFrom = false;
			Field.SubstituteMaxDateForNullTo = false;

			result = JsonConverterHelper.Serialize(Field);
			deserialisedField = JsonConverterHelper.Deserialize<DateTimeOffsetRangeField>(result);

			AssertEquals("deserialisedField.FieldName", "MyDate", deserialisedField.FieldName);
			AssertEquals("deserialisedField.SubstituteMinDateForNullFrom", Field.SubstituteMinDateForNullFrom, deserialisedField.SubstituteMinDateForNullFrom);
			AssertEquals("deserialisedField.SubstituteMaxDateForNullTo", Field.SubstituteMaxDateForNullTo, deserialisedField.SubstituteMaxDateForNullTo);
		}

		[TestUtcOffset(-5, 0, 0)]
		public void TestFromDateParam()
		{
			Field.FieldName = "MyDate";
			Field.DisplayName = "My Date";
			Field.ValueLow = new ZDateTimeOffset(2008, 8, 8, 0, 0, 0, new TimeSpan(-5, 0, 0));

			using (var report = new Report(null, null))
			{
				Field.ConvertToUtc = false;

				AssertEquals("<My Date.FromDate>", new ZDateTimeOffset(2008, 8, 8, 0, 0, 0, new TimeSpan(-5, 0, 0)),
					Field.ValueProviders["<My Date.FromDate>"].GetReplacement("<My Date.FromDate>", report));

				AssertEquals("<My Date.FromDateSqlParam> ConvertToUtc = false", new ZDateTimeOffset(2008, 8, 8, 0, 0, 0, new TimeSpan(-5, 0, 0)),
					Field.ValueProviders["<My Date.FromDateSqlParam>"].GetReplacement("<My Date.FromDateSqlParam>", report));

				Field.ConvertToUtc = true;

				AssertEquals("<My Date.FromDate>", new ZDateTimeOffset(2008, 8, 8, 0, 0, 0, new TimeSpan(-5, 0, 0)),
					Field.ValueProviders["<My Date.FromDate>"].GetReplacement("<My Date.FromDate>", report));

				AssertEquals("<My Date.FromDateParam> ConvertToUtc = true", new ZDateTimeOffset(2008, 8, 8, 5, 0, 0, new TimeSpan(0, 0, 0)),
					Field.ValueProviders["<My Date.FromDateSqlParam>"].GetReplacement("<My Date.FromDateSqlParam>", report));

				AssertEquals("<My Date.FromDateParam> ConvertToUtc = true", TimeSpan.Zero,
					(Field.ValueProviders["<My Date.FromDateSqlParam>"].GetReplacement("<My Date.FromDateSqlParam>", report) as ZDateTimeOffset?).Value.Offset);
			}
		}

		[TestUtcOffset(-5, 0, 0)]
		public void TestFromDateUtc()
		{
			Field.FieldName = "MyDate";
			Field.DisplayName = "My Date";
			Field.ValueLow = new ZDateTimeOffset(2008, 8, 8, 0, 0, 0, new TimeSpan(-5, 0, 0));

			using (var report = new Report(null, null))
			{
				AssertEquals("<My Date.FromDate>", new ZDateTimeOffset(2008, 8, 8, 0, 0, 0, new TimeSpan(-5, 0, 0)),
					Field.ValueProviders["<My Date.FromDate>"].GetReplacement("<My Date.FromDate>", report));

				AssertEquals("<My Date.FromDateUtc>", new ZDateTimeOffset(2008, 8, 8, 5, 0, 0, new TimeSpan(0, 0, 0)),
					Field.ValueProviders["<My Date.FromDateUtc>"].GetReplacement("<My Date.FromDateUtc>", report));
				AssertEquals("<My Date.FromDateUtc>", TimeSpan.Zero,
					(Field.ValueProviders["<My Date.FromDateUtc>"].GetReplacement("<My Date.FromDateUtc>", report) as ZDateTimeOffset?).Value.Offset);
			}
		}

		[TestUtcOffset(-6, 0, 0)]
		public void TestToDateSqlParam()
		{
			Field.FieldName = "MyDate";
			Field.DisplayName = "My Date";

			using (var report = new Report(null, null))
			{
				Field.PickerFormat = DocEngineDatePickerFormats.Short;
				Field.ValueHigh = new ZDateTimeOffset(2008, 8, 8, 0, 0, 0, TimeSpan.FromHours(-6));

				Field.ConvertToUtc = false;

				AssertEquals("<My Date.ToDate>", new ZDateTimeOffset(2008, 8, 8, 0, 0, 0, TimeSpan.FromHours(-6)),
					Field.ValueProviders["<My Date.ToDate>"].GetReplacement("<My Date.ToDate>", report));

				AssertEquals("<My Date.ToDateSqlParam> ConvertToUtc = false", new ZDateTimeOffset(2008, 8, 9, 0, 0, 0, TimeSpan.FromHours(-6)),
					Field.ValueProviders["<My Date.ToDateSqlParam>"].GetReplacement("<My Date.ToDateSqlParam>", report));

				Field.ConvertToUtc = true;

				AssertEquals("<My Date.ToDateSqlParam> ConvertToUtc = true", new ZDateTimeOffset(2008, 8, 9, 6, 0, 0, TimeSpan.Zero),
					Field.ValueProviders["<My Date.ToDateSqlParam>"].GetReplacement("<My Date.ToDateSqlParam>", report));
				AssertEquals("<My Date.ToDateSqlParam> ConvertToUtc = true", TimeSpan.Zero,
					(Field.ValueProviders["<My Date.ToDateSqlParam>"].GetReplacement("<My Date.ToDateSqlParam>", report) as ZDateTimeOffset?).Value.Offset);

				Field.PickerFormat = DocEngineDatePickerFormats.Long;
				Field.ValueHigh = new ZDateTimeOffset(2008, 8, 8, 5, 15, 0);

				Field.ConvertToUtc = false;

				AssertEquals("<My Date.ToDate>", new ZDateTimeOffset(2008, 8, 8, 5, 15, 0, TimeSpan.FromHours(-6)),
					Field.ValueProviders["<My Date.ToDate>"].GetReplacement("<My Date.ToDate>", report));

				AssertEquals("<My Date.ToDateSqlParam> ConvertToUtc = false", new ZDateTimeOffset(2008, 8, 8, 5, 16, 0, TimeSpan.FromHours(-6)),
					Field.ValueProviders["<My Date.ToDateSqlParam>"].GetReplacement("<My Date.ToDateSqlParam>", report));

				Field.ConvertToUtc = true;

				AssertEquals("<My Date.ToDateSqlParam> ConvertToUtc = true", new ZDateTimeOffset(2008, 8, 8, 11, 16, 0, TimeSpan.FromHours(0)),
					Field.ValueProviders["<My Date.ToDateSqlParam>"].GetReplacement("<My Date.ToDateSqlParam>", report));
				AssertEquals("<My Date.ToDateSqlParam> ConvertToUtc = true", TimeSpan.Zero,
					(Field.ValueProviders["<My Date.ToDateSqlParam>"].GetReplacement("<My Date.ToDateSqlParam>", report) as ZDateTimeOffset?).Value.Offset);
			}
		}

		[TestUtcOffset(-6, 0, 0)]
		public void TestToDateUtc()
		{
			Field.FieldName = "MyDate";
			Field.DisplayName = "My Date";
			Field.ValueHigh = new ZDateTimeOffset(2008, 8, 8, 0, 0, 0, TimeSpan.FromHours(-6));

			using (var report = new Report(null, null))
			{
				var result = Field.ValueProviders["<My Date.ToDate>"].GetReplacement("<My Date.ToDate>", report) as ZDateTimeOffset?;
				AssertEquals("<My Date.ToDate>", new ZDateTimeOffset(2008, 8, 8, 0, 0, 0, TimeSpan.FromHours(-6)),
					result.Value);
				AssertEquals("<My Date.ToDate>", TimeSpan.FromHours(-6),
									result.Value.Offset);

				result = Field.ValueProviders["<My Date.ToDateUtc>"].GetReplacement("<My Date.ToDateUtc>", report) as ZDateTimeOffset?;
				AssertEquals("<My Date.ToDateUtc>", new ZDateTimeOffset(2008, 8, 8, 6, 0, 0, TimeSpan.Zero),
					result.Value);
				AssertEquals("<My Date.ToDateUtc>", TimeSpan.Zero,
									result.Value.Offset);
			}
		}

		[TestUtcOffset(-7, 0, 0)]
		public void TestToNextDateUtc()
		{
			Field.FieldName = "MyDate";
			Field.DisplayName = "My Date";
			Field.ValueHigh = new ZDateTimeOffset(2008, 8, 8, 0, 0, 0, TimeSpan.FromHours(-7));

			using (var report = new Report(null, null))
			{
				var result = Field.ValueProviders["<My Date.ToNextDate>"].GetReplacement("<My Date.ToNextDate>", report) as ZDateTimeOffset?;
				AssertEquals("<My Date.ToNextDate>", new ZDateTimeOffset(2008, 8, 9, 0, 0, 0, TimeSpan.FromHours(-7)),
					result.Value);
				AssertEquals("<My Date.ToNextDate>", TimeSpan.FromHours(-7),
					result.Value.Offset);

				result = Field.ValueProviders["<My Date.ToNextDateUtc>"].GetReplacement("<My Date.ToNextDateUtc>", report) as ZDateTimeOffset?;
				AssertEquals("<My Date.ToNextDateUtc>", new ZDateTimeOffset(2008, 8, 9, 7, 0, 0, TimeSpan.FromHours(0)),
					result);
				AssertEquals("<My Date.ToNextDateUtc>", TimeSpan.Zero,
					result.Value.Offset);
			}
		}

		[TestUtcOffset(-8, 0, 0)]
		public void TestToEndDateUtc()
		{
			Field.FieldName = "MyDate";
			Field.DisplayName = "My Date";
			Field.ValueHigh = new ZDateTimeOffset(2008, 8, 8, 0, 0, 0, TimeSpan.FromHours(-8));

			using (var report = new Report(null, null))
			{
				var result = Field.ValueProviders["<My Date.ToEndDate>"].GetReplacement("<My Date.ToEndDate>", report) as ZDateTimeOffset?;
				AssertEquals("<My Date.ToEndDate>", new ZDateTimeOffset(2008, 8, 8, 23, 59, 59, TimeSpan.FromHours(-8)),
					result.Value);
				AssertEquals("<My Date.ToEndDate>", TimeSpan.FromHours(-8),
					result.Value.Offset);

				result = Field.ValueProviders["<My Date.ToEndDateUtc>"].GetReplacement("<My Date.ToEndDateUtc>", report) as ZDateTimeOffset?;
				AssertEquals("<My Date.ToEndDateUtc>", new ZDateTimeOffset(2008, 8, 9, 7, 59, 59, TimeSpan.FromHours(0)),
					result.Value);
				AssertEquals("<My Date.ToEndDateUtc>", TimeSpan.Zero,
					result.Value.Offset);
			}
		}

		public void TestFromAndToDateForSQLParameter()
		{
			Field.FieldName = "MyDate";
			Field.DisplayName = "My Date";
			Field.ValueLow = new ZDateTimeOffset(2024, 1, 19, 0, 0, 0, new TimeSpan(8, 0, 0));
			Field.ValueHigh = new ZDateTimeOffset(2024, 1, 21, 0, 0, 0, new TimeSpan(8, 0, 0));

			using (var report = new Report(null, null))
			{
				var replacementFrom = Field.ValueProviders["<My Date.FromDateForSQLParameter>"].GetReplacement("<My Date.FromDateForSQLParameter>", report);
				var replacementTo = Field.ValueProviders["<My Date.ToDateForSQLParameter>"].GetReplacement("<My Date.ToDateForSQLParameter>", report);
				AssertType<ReplacementWithSqlDbType>(replacementFrom);
				AssertType<ReplacementWithSqlDbType>(replacementTo);
				AssertEquals(SqlDbType.DateTimeOffset, ((ReplacementWithSqlDbType)replacementFrom).SqlDbType);
				AssertEquals(SqlDbType.DateTimeOffset, ((ReplacementWithSqlDbType)replacementTo).SqlDbType);
				AssertEquals(new DateTimeOffset(2024, 1, 19, 0, 0, 0, new TimeSpan(8, 0, 0)), ((ReplacementWithSqlDbType)replacementFrom).MacroValue);
				AssertEquals(new DateTimeOffset(2024, 1, 21, 0, 0, 0, new TimeSpan(8, 0, 0)), ((ReplacementWithSqlDbType)replacementTo).MacroValue);
			}
		}

		public void TestFromAndToDateForSQLParameter_ForEmptyDate()
		{
			Field.FieldName = "MyDate";
			Field.DisplayName = "My Date";
			Field.ValueLow = ZDateTimeOffset.Empty;
			Field.ValueHigh = ZDateTimeOffset.Empty;

			using (var report = new Report(null, null))
			{
				var replacementFrom = Field.ValueProviders["<My Date.FromDateForSQLParameter>"].GetReplacement("<My Date.FromDateForSQLParameter>", report);
				var replacementTo = Field.ValueProviders["<My Date.ToDateForSQLParameter>"].GetReplacement("<My Date.ToDateForSQLParameter>", report);
				AssertType<ReplacementWithSqlDbType>(replacementFrom);
				AssertType<ReplacementWithSqlDbType>(replacementTo);
				AssertEquals(SqlDbType.DateTimeOffset, ((ReplacementWithSqlDbType)replacementFrom).SqlDbType);
				AssertEquals(SqlDbType.DateTimeOffset, ((ReplacementWithSqlDbType)replacementTo).SqlDbType);
				AssertEquals(DBNull.Value, ((ReplacementWithSqlDbType)replacementFrom).MacroValue);
				AssertEquals(DBNull.Value, ((ReplacementWithSqlDbType)replacementTo).MacroValue);
			}
		}

		public void TestLowSchedulePeriodDefaultsFromScheduleTaskSetToDaily()
		{
			var reportScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			reportScheduleTask.Recurrence.DailyRange = ZBool.True;
			Field.SetScheduleTask(reportScheduleTask);
			AssertEquals("Field.LowSchedule.Period", ScheduleRecurrenceType.Daily, Field.LowSchedule.Period);
			AssertEquals("Field.HighSchedule.Period", ScheduleRecurrenceType.Daily, Field.HighSchedule.Period);
		}

		public void TestLowSchedulePeriodDefaultsFromScheduleTaskSetToWeekly()
		{
			var reportScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			reportScheduleTask.Recurrence.WeeklyRange = ZBool.True;
			Field.SetScheduleTask(reportScheduleTask);
			AssertEquals("Field.LowSchedule.Period", ScheduleRecurrenceType.Weekly, Field.LowSchedule.Period);
			AssertEquals("Field.HighSchedule.Period", ScheduleRecurrenceType.Weekly, Field.HighSchedule.Period);
		}

		public void TestLowSchedulePeriodDefaultsFromScheduleTaskSetToMonthly()
		{
			var reportScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			reportScheduleTask.Recurrence.MonthlyRange = ZBool.True;
			Field.SetScheduleTask(reportScheduleTask);
			AssertEquals("Field.LowSchedule.Period", ScheduleRecurrenceType.Monthly, Field.LowSchedule.Period);
			AssertEquals("Field.HighSchedule.Period", ScheduleRecurrenceType.Monthly, Field.HighSchedule.Period);
		}

		public void TestLowSchedulePeriodDefaultsFromScheduleTaskSetToYearly()
		{
			var reportScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			reportScheduleTask.Recurrence.YearlyRange = ZBool.True;
			Field.SetScheduleTask(reportScheduleTask);
			AssertEquals("Field.LowSchedule.Period", ScheduleRecurrenceType.Yearly, Field.LowSchedule.Period);
			AssertEquals("Field.HighSchedule.Period", ScheduleRecurrenceType.Yearly, Field.HighSchedule.Period);
		}

		public void TestLowSchedulePeriodDefaultsFromScheduleTaskSetToAccounting()
		{
			var reportScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			reportScheduleTask.Recurrence.AccountingRange = ZBool.True;
			Field.SetScheduleTask(reportScheduleTask);
			AssertEquals("Field.LowSchedule.Period", ScheduleRecurrenceType.Monthly, Field.LowSchedule.Period);
			AssertEquals("Field.HighSchedule.Period", ScheduleRecurrenceType.Monthly, Field.HighSchedule.Period);
		}

		public void TestSerializationAndJsonConverter()
		{
			var pickerFormat = DocEngineDatePickerFormats.Long;
			ZDateTimeOffset valueLow = new DateTimeOffset(2024, 1, 19, 0, 0, 0, new TimeSpan(8, 0, 0));
			ZDateTimeOffset valueHigh = new DateTimeOffset(2024, 1, 21, 0, 0, 0, new TimeSpan(8, 0, 0));
			Field.PickerFormat = pickerFormat;
			Field.ValueLow = valueLow;
			Field.ValueHigh = valueHigh;
			TestJsonConverter(null, valueLow, valueHigh, pickerFormat);
		}

		public void TestSerialisationAndJsonConverter_EmptyValues()
		{
			TestJsonConverter(null, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, DocEngineDatePickerFormats.Short);
		}

		public void TestSerialisationAndJsonConverter_EmptyScheduleValues()
		{
			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2006, 10, 1);
			Field.SetScheduleTask(scheduleTask);
			TestJsonConverter(scheduleTask, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, DocEngineDatePickerFormats.Short);
		}

		public void TestSerialisationAndJsonConverter_ValidScheduleValues()
		{
			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2006, 10, 1);
			Field.SetScheduleTask(scheduleTask);

			Field.LowSchedule.FillWithValidTestData();
			Field.LowSchedule.ByMonth = true;
			Field.LowSchedule.DayNumber = 3;

			Field.HighSchedule.FillWithValidTestData();
			Field.HighSchedule.ByMonth = true;
			Field.HighSchedule.DayNumber = 5;

			TestJsonConverter(scheduleTask, new ZDateTimeOffset(2006, 10, 3, 0, 0, 0, ZDateTimeOffset.Now.Offset), new ZDateTimeOffset(2006, 10, 5, 0, 0, 0, ZDateTimeOffset.Now.Offset), DocEngineDatePickerFormats.Short);
		}

		public void TestWhereClauseWithInput()
		{
			Field.FieldName = "abc";
			Field.ValueLow = new DateTimeOffset(2024, 1, 19, 0, 0, 0, new TimeSpan(8, 0, 0));
			Field.ValueHigh = new DateTimeOffset(2024, 1, 20, 0, 0, 0, new TimeSpan(8, 0, 0));
			var whereClauseMatch = Regex.Match(Field.WhereClause(), @"abc >= (@p[0-9]+) AND abc < (@p[0-9]+)");
			Assert("WhereClause should be of the form 'abc >= @p123 AND abc < @p456' but was: " + Field.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Should have 2 params", 2, Field.SqlParameters().Count);
			AssertEquals("Param 1's name should match the name in the where clause", whereClauseMatch.Groups[1].Value, (Field.SqlParameters()[0]).ToString());
			AssertEquals("Param 1's value should be set", new DateTimeOffset(2024, 1, 19, 0, 0, 0, new TimeSpan(8, 0, 0)), (Field.SqlParameters()[0]).Value);
			AssertEquals("Param 2's name should match the name in the where clause", whereClauseMatch.Groups[2].Value, (Field.SqlParameters()[1]).ToString());
			AssertEquals("Param 2's value should be set", new DateTimeOffset(2024, 1, 21, 0, 0, 0, new TimeSpan(8, 0, 0)), (Field.SqlParameters()[1]).Value);
		}

		public void TestAlsoUsesTimePartInLongFormat()
		{
			Field.PickerFormat = DocEngineDatePickerFormats.Long;
			Field.ValueLow = new DateTimeOffset(2003, 1, 25, 11, 11, 11, 11, new TimeSpan(8, 0, 0));
			AssertEquals("Should also use time part", new ZDateTimeOffset(new DateTimeOffset(2003, 1, 25, 11, 11, 11, 11, new TimeSpan(8, 0, 0))), Field.ValueLow);
			Field.ValueHigh = new DateTimeOffset(2003, 1, 25, 11, 11, 11, 11, new TimeSpan(8, 0, 0));
			AssertEquals("Should also use time part", new ZDateTimeOffset(new DateTimeOffset(2003, 1, 25, 11, 11, 11, 11, new TimeSpan(8, 0, 0))), Field.ValueHigh);
		}

		public void TestWhereClauseWithLowerInput()
		{
			Field.FieldName = "abc";
			Field.ValueLow = new DateTimeOffset(2003, 1, 25, 0, 0, 0, new TimeSpan(8, 0, 0));
			var whereClauseMatch = Regex.Match(Field.WhereClause(), @"abc >= (@p[0-9]+)");
			Assert("WhereClause should be of the form 'abc >= @p123' but was: " + Field.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Should have 2 params", 2, Field.SqlParameters().Count);
			AssertEquals("Param 1's name should match the name in the where clause", whereClauseMatch.Groups[1].Value, Field.SqlParameters()[0].ToString());
			AssertEquals("Param 1's value should be set", new DateTimeOffset(2003, 1, 25, 0, 0, 0, new TimeSpan(8, 0, 0)), Field.SqlParameters()[0].Value);
		}

		public void TestWhereClauseWithUpperInput()
		{
			Field.FieldName = "abc";
			Field.ValueHigh = new DateTimeOffset(2003, 3, 21, 0, 0, 0, new TimeSpan(8, 0, 0));
			Match whereClauseMatch = Regex.Match(Field.WhereClause(), @"abc < (@p[0-9]+)");
			Assert("WhereClause should be of the form 'abc < @p456' but was: " + Field.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Should have 2 params", 2, Field.SqlParameters().Count);
			AssertEquals("Param 2's name should match the name in the where clause", whereClauseMatch.Groups[1].Value, Field.SqlParameters()[1].ToString());
			AssertEquals("Param 2's value should be set", new DateTimeOffset(2003, 3, 22, 0, 0, 0, new TimeSpan(8, 0, 0)), Field.SqlParameters()[1].Value);
		}

		public void TestIsResponsibleOldWay()
		{
			var df = CreateTestDateRange();
			var responsibleCount = 0;
			foreach (var provider in df.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestDateField>", Passes.FirstPass))
				{
					responsibleCount++;
				}
			}
			AssertEquals("Should have one responsible!", 1, responsibleCount);
		}

		[TestUtcOffset(8, 0, 0)]
		public void TestGetReplacementOldWay()
		{
			var df = CreateTestDateRange();
			foreach (ValueProvider provider in df.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestDateField>", Passes.FirstPass))
				{
					AssertEquals("From: 01-Dec-04 00:00 +08:00 To: 10-Dec-04 23:59 +08:00", provider.GetReplacement("<TestDateField>", new Report(null, null)));
				}
			}
		}

		[ExpectNoExceptions]
		public void TestIsResponsibleOldWayWithEmptyHighValue()
		{
			var df = CreateTestDateRange();
			df.ValueHigh = ZDateTimeOffset.Empty;
			foreach (ValueProvider provider in df.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestDateField>", Passes.FirstPass))
				{
					provider.GetReplacement("<TestDateField>", new Report(null, null));
				}
			}
		}

		public void TestIsResponsibleNewWayFromDate()
		{
			var df = CreateTestDateRange();

			foreach (ValueProvider provider in df.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestDateField.FromDate>", Passes.FirstPass))
				{
					AssertEquals(new ZDateTimeOffset(2004, 12, 1, 0, 0, 0, new TimeSpan(8, 0, 0)), provider.GetReplacement("<TestDateField.FromDate>", new Report(null, null)));
				}
			}
		}

		public void TestIsResponsibleNewWayToDate()
		{
			var df = CreateTestDateRange();

			foreach (ValueProvider provider in df.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestDateField.ToDate>", Passes.FirstPass))
				{
					AssertEquals(new ZDateTimeOffset(2004, 12, 10, 0, 0, 0, new TimeSpan(8, 0, 0)), provider.GetReplacement("<TestDateField.ToDate>", new Report(null, null)));
				}
			}
		}

		public void TestIsResponsibleNewWayToNextDate()
		{
			var df = CreateTestDateRange();

			foreach (ValueProvider provider in df.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestDateField.ToNextDate>", Passes.FirstPass))
				{
					AssertEquals(new ZDateTimeOffset(2004, 12, 11, 0, 0, 0, new TimeSpan(8, 0, 0)), provider.GetReplacement("<TestDateField.ToNextDate>", new Report(null, null)));
				}
			}
		}

		public void TestGetReplacementForToNextDateHandlesEmptyDate()
		{
			var df = CreateTestDateRange();
			df.ValueHigh = ZDateTimeOffset.Empty;

			foreach (ValueProvider provider in df.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestDateField.ToNextDate>", Passes.FirstPass))
				{
					AssertEquals(ZDateTimeOffset.Empty, provider.GetReplacement("<TestDateField.ToNextDate>", new Report(null, null)));
				}
			}
		}

		public void TestGetReplacementForToEndDate()
		{
			var df = CreateTestDateRange();
			df.ValueHigh = new ZDateTimeOffset(2004, 12, 11, 0, 0, 0, new TimeSpan(8, 0, 0));

			foreach (ValueProvider provider in df.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestDateField.ToEndDate>", Passes.FirstPass))
				{
					AssertEquals(new ZDateTimeOffset(2004, 12, 11, 0, 0, 0, new TimeSpan(8, 0, 0)).AddDays(1).AddSeconds(-1), provider.GetReplacement("<TestDateField.ToEndDate>", new Report(null, null)));
				}
			}
		}

		public void TestGetReplacementForToNextDateHandlesInvalidDate()
		{
			var df = CreateTestDateRange();
			df.ValueHigh = ZDateTimeOffset.Invalid;

			foreach (ValueProvider provider in df.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestDateField.ToNextDate>", Passes.FirstPass))
				{
					AssertEquals(ZDateTimeOffset.Empty, provider.GetReplacement("<TestDateField.ToNextDate>", new Report(null, null)));
				}
			}
		}

		public void TestValueAsObjectTranslatable()
		{
			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				mockChs.Put("3eb816a9-3c15-4f52-beef-370851cf81d3", new ResourceStringData("3eb816a9-3c15-4f52-beef-370851cf81d3", "从: {0} 到: {1}"));
				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
				{
					var df = CreateTestDateRange();
					foreach (ValueProvider provider in df.ValueProviders)
					{
						if (provider.IsResponsibleForReplacing("<TestDateField>", Passes.FirstPass))
						{
							AssertEquals("从: 01-Dec-04 00:00 +08:00 到: 10-Dec-04 23:59 +08:00", provider.GetReplacement("<TestDateField>", new Report(null, null)));
						}
					}
				}
			}
		}

		public void TestHourAndMinuteToFilterIsNotTamperedWith()
		{
			var scheduleTask1 = Factory.NewWithValidTestData<ReportScheduleTask>();
			Field.PickerFormat = DocEngineDatePickerFormats.Long;
			Field.SetScheduleTask(scheduleTask1);
			Field.LowSchedule.FillWithValidTestData();
			Field.LowSchedule.ByHourAndMinute = true;
			Field.HighSchedule.FillWithValidTestData();
			Field.HighSchedule.ByHourAndMinute = true;
			Field.ValueHigh = new ZDateTimeOffset(2012, 12, 12, 12, 0, 0, 0, new TimeSpan(8, 0, 0));
			Field.ValueLow = new ZDateTimeOffset(2011, 11, 11, 11, 0, 0, 0, new TimeSpan(8, 0, 0));

			AssertEquals("From: 11-Nov-11 11:00 +08:00 To: 12-Dec-12 12:00 +08:00", Field.ValueAsObject);
		}

		public void TestGetReplacementNewWay()
		{
			var df = CreateTestDateRange();

			int responsibleCount = 0;
			foreach (ValueProvider provider1 in df.ValueProviders)
			{
				if (provider1.IsResponsibleForReplacing("<TestDateField.FromDate>", Passes.FirstPass))
				{
					responsibleCount++;
				}
			}
			AssertEquals("Should have one responsible for <TestDateField.FromDate>!", 1, responsibleCount);

			responsibleCount = 0;
			foreach (ValueProvider provider in df.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestDateField.ToDate>", Passes.FirstPass))
				{
					responsibleCount++;
				}
			}
			AssertEquals("Should have one responsible for <TestDateField.ToDate>!", 1, responsibleCount);

			responsibleCount = 0;
			foreach (ValueProvider provider in df.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestDateField.ToNextDate>", Passes.FirstPass))
				{
					responsibleCount++;
				}
			}
			AssertEquals("Should have one responsible for <TestDateField.ToNexDate>!", 1, responsibleCount);
		}

		public void TestSubstitueMaxDateForNullTo()
		{
			bool providerExists = false;
			Field.FieldName = "abc";
			Field.DisplayName = "TestDateField";
			Field.ValueHigh = new DateTimeOffset(2004, 12, 10, 0, 0, 0, new TimeSpan(8, 0, 0));
			Field.SubstituteMaxDateForNullTo = true;

			foreach (ValueProvider provider in Field.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestDateField.ToDate>", Passes.FirstPass))
				{
					AssertEquals("ToDate will not substitute when date is not null", new DateTimeOffset(2004, 12, 10, 0, 0, 0, new TimeSpan(8, 0, 0)), provider.GetReplacement("<TestDateField.ToDate>", new Report(null, null)));
					providerExists = true;
				}
			}
			AssertEquals("There should have been a toDate provider", true, providerExists);

			providerExists = false;
			Field.ValueHigh = ZDateTimeOffset.Invalid;
			foreach (ValueProvider provider in Field.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestDateField.ToDate>", Passes.FirstPass))
				{
					AssertEquals("ToDate will substitute with max date when date is null", DateTimeOffset.MaxValue, provider.GetReplacement("<TestDateField.ToDate>", new Report(null, null)));
					providerExists = true;
				}
			}
			AssertEquals("There should have been a toDate provider", true, providerExists);
		}

		public void TestSubstitueMaxDateForNullToNext()
		{
			bool providerExists = false;
			Field.FieldName = "abc";
			Field.DisplayName = "TestDateField";
			Field.ValueHigh = new DateTimeOffset(2004, 12, 10, 0, 0, 0, new TimeSpan(8, 0, 0));
			Field.SubstituteMaxDateForNullTo = true;

			foreach (ValueProvider provider in Field.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestDateField.ToNextDate>", Passes.FirstPass))
				{
					AssertEquals("ToNextDate will not substitute when date is not null", new DateTimeOffset(2004, 12, 11, 0, 0, 0, TimeSpan.FromHours(8)), provider.GetReplacement("<TestDateField.ToNextDate>", new Report(null, null)));
					providerExists = true;
				}
			}
			AssertEquals("There should have been a toDate provider", true, providerExists);

			providerExists = false;
			Field.ClearValueForUnitTest();
			foreach (ValueProvider provider in Field.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestDateField.ToNextDate>", Passes.FirstPass))
				{
					AssertEquals("ToNextDate will substitute with max date when date is null", DateTimeOffset.MaxValue, provider.GetReplacement("<TestDateField.ToNextDate>", new Report(null, null)));
					providerExists = true;
				}
			}
			AssertEquals("There should have been a toDate provider", true, providerExists);
		}

		public void TestSubstitueMinDateForNullFrom()
		{
			bool providerExists = false;
			Field.FieldName = "abc";
			Field.DisplayName = "TestDateField";
			Field.ValueLow = new DateTimeOffset(2004, 12, 10, 0, 0, 0, new TimeSpan(8, 0, 0));
			Field.SubstituteMinDateForNullFrom = true;

			foreach (ValueProvider provider in Field.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestDateField.FromDate>", Passes.FirstPass))
				{
					AssertEquals("FromDate will not substitute when date is not null", new DateTimeOffset(2004, 12, 10, 0, 0, 0, new TimeSpan(8, 0, 0)), provider.GetReplacement("<TestDateField.FromDate>", new Report(null, null)));
					providerExists = true;
				}
			}
			AssertEquals("There should have been a FromDate provider", true, providerExists);

			providerExists = false;
			Field.ValueLow = ZDateTimeOffset.Invalid;
			foreach (ValueProvider provider in Field.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestDateField.FromDate>", Passes.FirstPass))
				{
					AssertEquals("FromDate will substitute with min date when date is null", DateTimeOffset.MinValue, provider.GetReplacement("<TestDateField.FromDate>", new Report(null, null)));
					providerExists = true;
				}
			}
			AssertEquals("There should have been a toDate provider", true, providerExists);
		}

		public void TestFromIsNotAfterTo()
		{
			var df = CreateTestDateRange();
			df.ValueLow = new DateTimeOffset(2003, 1, 25, 0, 0, 0, new TimeSpan(8, 0, 0));
			df.ValueHigh = new DateTimeOffset(2003, 1, 24, 0, 0, 0, new TimeSpan(8, 0, 0));
			df.RunPreSaveValidation();
			AssertHasErrors("There is notification error in ValueInfo of low", df.ValueLowInfo);
			AssertHasErrors("There is notification error in ValueInfo of high", df.ValueHighInfo);
		}

		public void TestSetScheduleTask()
		{
			AssertNull("LowSchedule", Field.LowSchedule);
			AssertNull("HighSchedule", Field.HighSchedule);

			ReportScheduleTask scheduleTask1 = Factory.NewWithValidTestData<ReportScheduleTask>();
			Field.SetScheduleTask(scheduleTask1);
			AssertEquals("LowSchedule.ScheduleTask", scheduleTask1, Field.LowSchedule.ScheduleTask);
			AssertEquals("HighSchedule.ScheduleTask", scheduleTask1, Field.HighSchedule.ScheduleTask);

			Field.LowSchedule.FillWithValidTestData();
			Field.LowSchedule.DayNumber = 6;
			Field.HighSchedule.FillWithValidTestData();
			Field.HighSchedule.DayNumber = 1;
			ZDateTimeOffset previousValueLow = Field.ValueLow;
			ZDateTimeOffset previousValueHigh = Field.ValueHigh;

			ReportScheduleTask scheduleTask2 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask2.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2002, 10, 2);
			Field.SetScheduleTask(scheduleTask2);

			Assert("ValueLow should have changed.", Field.ValueLow != previousValueLow);
			Assert("ValueHigh should have changed.", Field.ValueHigh != previousValueHigh);
			AssertEquals("LowSchedule.ScheduleTask", scheduleTask2, Field.LowSchedule.ScheduleTask);
			AssertEquals("HighSchedule.ScheduleTask", scheduleTask2, Field.HighSchedule.ScheduleTask);
			AssertEquals("ValueLow", new ZDateTime(Field.LowSchedule.GetScheduleDate()).ToOffset(), Field.ValueLow);
			AssertEquals("ValueHigh", new ZDateTime(Field.HighSchedule.GetScheduleDate()).ToOffset(), Field.ValueHigh);
			AssertEquals("ValueLow.IsEmpty", false, Field.ValueLow.IsEmpty);
			AssertEquals("ValueHigh.IsEmpty", false, Field.ValueHigh.IsEmpty);
		}

		public void TestValueInfoReadOnly()
		{
			AssertEquals("ValueLowInfo.ReadOnly", false, Field.ValueLowInfo.ReadOnly);
			AssertEquals("ValueHighInfo.ReadOnly", false, Field.ValueHighInfo.ReadOnly);
			Field.SetScheduleTask(Factory.NewWithValidTestData<ReportScheduleTask>());
			AssertEquals("ValueLowInfo.ReadOnly", true, Field.ValueLowInfo.ReadOnly);
			AssertEquals("ValueHighInfo.ReadOnly", true, Field.ValueHighInfo.ReadOnly);
		}

		public void TestFirstAccessToSchedules()
		{
			var field = (DateTimeOffsetRangeField)GetNewBusinessObject();
			ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			field.SetScheduleTask(scheduleTask);

			AssertEquals("Low Schedule, Period Scope is Empty", string.Empty, field.LowSchedule.PeriodScope);
			AssertEquals("High Schedule, Period Scope is Empty", string.Empty, field.HighSchedule.PeriodScope);
		}

		public void TestRunAndScheduleJsonConverter()
		{
			var runValueLow = new ZDateTimeOffset(1979, 2, 12, 0, 0, 0, new TimeSpan(8, 0, 0));
			var runValueHigh = new ZDateTimeOffset(2007, 10, 16, 0, 0, 0, new TimeSpan(8, 0, 0));

			var sourceField = new DateTimeOffsetRangeField(Factory);
			sourceField.ValueLow = runValueLow;
			sourceField.ValueHigh = runValueHigh;

			var result = JsonConverterHelper.Serialize(sourceField);
			var deserialisedField = JsonConverterHelper.Deserialize<DateTimeOffsetRangeField>(result);

			AssertEquals("Run ValueLow", runValueLow, deserialisedField.ValueLow);
			AssertEquals("Run ValueHigh", runValueHigh, deserialisedField.ValueHigh);

			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDate.Today;
			deserialisedField.SetScheduleTask(scheduleTask);

			deserialisedField.LowSchedule.Period = ScheduleRecurrenceType.Daily;
			deserialisedField.LowSchedule.PeriodScope = PeriodScopeList.Codes.Previous;
			deserialisedField.LowSchedule.PeriodCount = 3;

			deserialisedField.HighSchedule.Period = ScheduleRecurrenceType.Daily;
			deserialisedField.HighSchedule.PeriodScope = PeriodScopeList.Codes.Next;
			deserialisedField.HighSchedule.PeriodCount = 5;

			result = JsonConverterHelper.Serialize(deserialisedField);
			var deserialisedField2 = JsonConverterHelper.Deserialize<DateTimeOffsetRangeField>(result);

			AssertEquals("Run ValueLow", runValueLow, deserialisedField2.ValueLow);
			AssertEquals("Run ValueHigh", runValueHigh, deserialisedField2.ValueHigh);

			deserialisedField2.SetScheduleTask(scheduleTask);

			AssertEquals("LowSchedule.Period", deserialisedField.LowSchedule.Period, deserialisedField2.LowSchedule.Period);
			AssertEquals("LowSchedule.PeriodScope", deserialisedField.LowSchedule.PeriodScope, deserialisedField2.LowSchedule.PeriodScope);
			AssertEquals("LowSchedule.PeriodCount", deserialisedField.LowSchedule.PeriodCount, deserialisedField2.LowSchedule.PeriodCount);
			AssertEquals("ValueLow", deserialisedField.ValueLow, deserialisedField2.ValueLow);

			AssertEquals("HighSchedule.Period", deserialisedField.HighSchedule.Period, deserialisedField2.HighSchedule.Period);
			AssertEquals("HighSchedule.PeriodScope", deserialisedField.HighSchedule.PeriodScope, deserialisedField2.HighSchedule.PeriodScope);
			AssertEquals("HighSchedule.PeriodCount", deserialisedField.HighSchedule.PeriodCount, deserialisedField2.HighSchedule.PeriodCount);
			AssertEquals("ValueHigh", deserialisedField.ValueHigh, deserialisedField2.ValueHigh);
		}

		public virtual void TestValidateValueLow()
		{
			MockFilterCollectionValidator validator = new MockFilterCollectionValidator();
			Field.Validators.Add(validator);
			validator.SetIsValid(false);

			Field.ValueLow = ZDateTimeOffset.Empty;
			AssertHasError(Field.ValueLowInfo, MockFilterCollectionValidator.ErrorMessage);

			validator.SetIsValid(true);
			Field.ValueHigh = new ZDateTimeOffset(2006, 5, 5, 0, 0, 0, new TimeSpan(8, 0, 0));
			Field.ValueLow = new ZDateTimeOffset(2006, 12, 31, 0, 0, 0, new TimeSpan(8, 0, 0));
			AssertHasError(Field.ValueLowInfo, "The 'From date' must be before 'To date'");

			Field.ValueLow = new ZDateTimeOffset(2006, 1, 1, 0, 0, 0, new TimeSpan(8, 0, 0));
			AssertNoErrors(Field.ValueLowInfo);
		}

		public virtual void TestValidateValueHigh()
		{
			MockFilterCollectionValidator validator = new MockFilterCollectionValidator();
			Field.Validators.Add(validator);
			validator.SetIsValid(false);

			Field.ValueHigh = ZDateTimeOffset.Empty;
			AssertHasError(Field.ValueHighInfo, MockFilterCollectionValidator.ErrorMessage);

			validator.SetIsValid(true);
			Field.ValueLow = new ZDateTimeOffset(2006, 5, 5, 0, 0, 0, new TimeSpan(8, 0, 0));
			Field.ValueHigh = new ZDateTimeOffset(2006, 1, 1, 0, 0, 0, new TimeSpan(8, 0, 0));
			AssertHasError(Field.ValueHighInfo, "The 'To date' must be after 'From date'");

			Field.ValueHigh = new ZDateTimeOffset(2006, 12, 31, 0, 0, 0, new TimeSpan(8, 0, 0));
			AssertNoErrors(Field.ValueHighInfo);
		}

		public override void TestSafeCopyValuesFrom()
		{
			var runValueLow = new ZDateTimeOffset(1979, 2, 12, 0, 0, 0, ZDateTimeOffset.Now.Offset);
			var runValueHigh = new ZDateTimeOffset(2007, 10, 16, 0, 0, 0, ZDateTimeOffset.Now.Offset);

			var sourceField = new DateTimeOffsetRangeField(Factory);
			sourceField.ValueLow = runValueLow;
			sourceField.ValueHigh = runValueHigh;

			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDate.Today;
			sourceField.SetScheduleTask(scheduleTask);

			sourceField.LowSchedule.Period = ScheduleRecurrenceType.Daily;
			sourceField.LowSchedule.PeriodScope = PeriodScopeList.Codes.Previous;
			sourceField.LowSchedule.PeriodCount = 3;

			sourceField.HighSchedule.Period = ScheduleRecurrenceType.Daily;
			sourceField.HighSchedule.PeriodScope = PeriodScopeList.Codes.Next;
			sourceField.HighSchedule.PeriodCount = 5;

			var destinationField = new DateTimeOffsetRangeField(Factory);
			((IFilter)destinationField).SafeCopyValuesFrom(sourceField);
			AssertEquals("Run ValueLow", runValueLow, destinationField.ValueLow);
			AssertEquals("Run ValueHigh", runValueHigh, destinationField.ValueHigh);

			destinationField.SetScheduleTask(scheduleTask);
			AssertEquals("LowSchedule.Period", sourceField.LowSchedule.Period, destinationField.LowSchedule.Period);
			AssertEquals("LowSchedule.PeriodScope", sourceField.LowSchedule.PeriodScope, destinationField.LowSchedule.PeriodScope);
			AssertEquals("LowSchedule.PeriodCount", sourceField.LowSchedule.PeriodCount, destinationField.LowSchedule.PeriodCount);
			AssertEquals("ValueLow", ZDateTimeOffset.Today.AddDays(-3), destinationField.ValueLow);

			AssertEquals("HighSchedule.Period", sourceField.HighSchedule.Period, destinationField.HighSchedule.Period);
			AssertEquals("HighSchedule.PeriodScope", sourceField.HighSchedule.PeriodScope, destinationField.HighSchedule.PeriodScope);
			AssertEquals("HighSchedule.PeriodCount", sourceField.HighSchedule.PeriodCount, destinationField.HighSchedule.PeriodCount);
			AssertEquals("ValueHigh", ZDateTimeOffset.Today.AddDays(5), destinationField.ValueHigh);
		}

		//
		public void TestCopyADateRangeFieldFromAnotherOneWithHourAndMinuteDateSchedule()
		{
			var sourceField = new DateTimeOffsetRangeField(Factory);
			var scheduleTask1 = Factory.NewWithValidTestData<ReportScheduleTask>();
			sourceField.SetScheduleTask(scheduleTask1);

			sourceField.LowSchedule.Period = ScheduleRecurrenceType.HourAndMinute;
			sourceField.HighSchedule.Period = ScheduleRecurrenceType.HourAndMinute;

			var copiedField = new DateTimeOffsetRangeField(Factory);
			var scheduleTask2 = Factory.NewWithValidTestData<ReportScheduleTask>();
			copiedField.SetScheduleTask(scheduleTask2);

			AssertNoExceptionThrown("No exception should be thrown when ScheduleRecurrenceType is HourAndMinute if a dateSchedule.ScheduleTask is null", () => copiedField.SafeCopyValuesFrom(sourceField));
			AssertNull("Precondition: No ScheduleTask assigned to the copied DateRange.LowSchedule", copiedField.LowSchedule.ScheduleTask);
			AssertNull("Precondition: No ScheduleTask assigned to the copied DateRange.HighSchedule", copiedField.HighSchedule.ScheduleTask);
			AssertEquals("Should return ZDateTime.Empty when ScheduleRecurrenceType is HourAndMinute if a dateSchedule.ScheduleTask is null ", ZDateTime.Empty, copiedField.LowSchedule.BaseDateTimeForTesting);
		}

		public override void TestClearValues()
		{
			var field = new DateTimeOffsetRangeField(Factory);
			field.ValueLow = new ZDateTimeOffset(1979, 2, 12, 11, 22, 33, new TimeSpan(8, 0, 0));
			field.ValueLow = new ZDateTimeOffset(2007, 10, 16, 10, 21, 00, new TimeSpan(8, 0, 0));
			((IFilter)field).ClearValues();
			AssertEquals(field.ValueLow, ZDateTimeOffset.Empty);
			AssertEquals(field.ValueHigh, ZDateTimeOffset.Empty);
		}

		public void TestFillFilterData()
		{
			// Arrange
			Field.ValueLow = new ZDateTimeOffset(2019, 3, 15, 0, 0, 0, new TimeSpan(8, 0, 0));
			Field.ValueHigh = new ZDateTimeOffset(2019, 3, 16, 0, 0, 0, new TimeSpan(8, 0, 0));
			Field.PickerFormat = DocEngineDatePickerFormats.Long;
			var reportData = new SelectedValueReportData();

			// Act
			Field.FillFilterData(reportData.FilterData);

			// Assert
			CombineAssertions("FillFilterData will produce the expected value.", () =>
			{
				AssertEquals(1, reportData.FilterData.DateTimeOffsetRangeFilterCollection.Count);
				AssertEquals(new DateTimeOffset(2019, 3, 15, 0, 0, 0, new TimeSpan(8, 0, 0)), reportData.FilterData.DateTimeOffsetRangeFilterCollection[0].ValueLow);
				AssertEquals(new DateTimeOffset(2019, 3, 16, 0, 0, 0, new TimeSpan(8, 0, 0)), reportData.FilterData.DateTimeOffsetRangeFilterCollection[0].ValueHigh);
				AssertEquals("Long", reportData.FilterData.DateTimeOffsetRangeFilterCollection[0].DateFormat);
			});
		}

		[TestUtcOffset(8, 0, 0)]
		public void TestSetFilterValue()
		{
			var reportFilterData = new ReportFilterData();
			reportFilterData.DateTimeOffsetRangeFilterCollection.Add(new DateTimeOffsetRangeFilter { ValueLow = new DateTimeOffset(2024, 4, 5, 0, 0, 0, TimeSpan.FromHours(8)), ValueHigh = new DateTimeOffset(2024, 4, 8, 0, 0, 0, TimeSpan.FromHours(8)), DisplayName = "abc" });
			Field.DisplayName = "abc";
			Field.SetFilterValue(reportFilterData);
			AssertEquals("Run value - Low: ", new DateTimeOffset(2024, 4, 5, 0, 0, 0, TimeSpan.FromHours(8)), Field.ValueLow);
			AssertEquals("Run value - High: ", new DateTimeOffset(2024, 4, 8, 0, 0, 0, TimeSpan.FromHours(8)), Field.ValueHigh);

			reportFilterData.DateTimeOffsetRangeFilterCollection.Clear();

			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2024, 4, 2);
			Field.SetScheduleTask(scheduleTask);
			reportFilterData.DateTimeOffsetRangeFilterCollection.Add(new DateTimeOffsetRangeFilter { ValueLow = new DateTimeOffset(1909, 11, 1, 0, 7, 0,TimeSpan.FromHours(8)), ValueHigh = new DateTimeOffset(1910, 1, 1, 0, 1, 0, TimeSpan.FromHours(8)), DisplayName = "abc" });

			Field.SetFilterValue(reportFilterData);

			AssertEquals("Schedule Storage value - Low:", new DateTimeOffset(1909, 11, 1, 0, 7, 0, TimeSpan.FromHours(8)), new ZDateTimeOffset(Field.LowSchedule.ToStorageValue()));
			AssertEquals("Schedule value - Low:", new DateTimeOffset(2024, 3, 31, 8, 0, 0, TimeSpan.FromHours(8)), Field.ValueHigh);
			AssertEquals("Schedule Storage value - High:", new DateTimeOffset(1910, 1, 1, 0, 1, 0, TimeSpan.FromHours(8)), new ZDateTimeOffset(Field.HighSchedule.ToStorageValue()));
			AssertEquals("Schedule value - Low:", new DateTimeOffset(2024, 3, 23, 8, 0, 0, TimeSpan.FromHours(8)), Field.ValueLow);

			reportFilterData.DateTimeOffsetRangeFilterCollection.Clear();
			Field.ClearValues();
			reportFilterData.DateTimeOffsetRangeFilterCollection.Add(new DateTimeOffsetRangeFilter { DisplayName = "abc" });
			Field.SetFilterValue(reportFilterData);
			Assert(!Field.LowSchedule.IsValid);
			Assert(!Field.HighSchedule.IsValid);
		}

		public override FilterFieldWithUTSupport[] GetFieldsWithValidClearValueTestCases()
		{
			return new FilterFieldWithUTSupport[] { CreateTestDateRange() };
		}

		public override int ExpectedNumberOfClearValueTestCases
		{
			get { return 1; }
		}

		DateTimeOffsetRangeField field;
		internal DateTimeOffsetRangeField Field
		{
			get
			{
				if (field == null)
				{
					field = (DateTimeOffsetRangeField)GetNewBusinessObject();
				}

				return field;
			}
		}

		DateTimeOffsetRangeField CreateTestDateRange()
		{
			var result = (DateTimeOffsetRangeField)GetNewBusinessObject();
			result.FieldName = "abc";
			result.DisplayName = "TestDateField";
			result.ValueLow = new DateTimeOffset(2004, 12, 1, 0, 0, 0, new TimeSpan(8, 0, 0));
			result.ValueHigh = new DateTimeOffset(2004, 12, 10, 0, 0, 0, new TimeSpan(8, 0, 0));
			return result;
		}

		void TestJsonConverter(ReportScheduleTask scheduleTask, ZDateTimeOffset expectedValueLow, ZDateTimeOffset expectedValueHigh, DocEngineDatePickerFormats expectedPickerFormat)
		{
			var scheduled = (scheduleTask != null);

			Field.FieldName = "abc";

			var result = JsonConverterHelper.Serialize(Field);
			var deserialisedField = JsonConverterHelper.Deserialize<DateTimeOffsetRangeField>(result);

			if (scheduled)
			{
				deserialisedField.SetScheduleTask(scheduleTask);
			}

			AssertEquals("deserialisedField.FieldName", "abc", deserialisedField.FieldName);
			AssertEquals("deserialisedField.ValueHigh", expectedValueHigh, deserialisedField.ValueHigh);
			AssertEquals("deserialisedField.ValueLow", expectedValueLow, deserialisedField.ValueLow);
			AssertEquals("deserialisedField.PickerFormat", expectedPickerFormat, deserialisedField.PickerFormat);

			if (scheduled)
			{
				AssertNotNull("HighSchedule", deserialisedField.HighSchedule);
				AssertNotNull("LowSchedule", deserialisedField.LowSchedule);

				deserialisedField.HighSchedule.FillWithValidTestData();
				deserialisedField.HighSchedule.DayNumber = 6;

				deserialisedField.LowSchedule.FillWithValidTestData();
				deserialisedField.LowSchedule.DayNumber = 4;

				Assert("HighSchedule.ValueChanged should be hooked.", deserialisedField.ValueHigh != expectedValueHigh);
				Assert("LowSchedule.ValueChanged should be hooked.", deserialisedField.ValueLow != expectedValueLow);
			}
			else
			{
				AssertNull("LowSchedule", deserialisedField.LowSchedule);
				AssertNull("HighSchedule", deserialisedField.HighSchedule);
			}
		}

		public virtual void TestRequiredValidationErrorOnlyDisplaysWhenBothFieldsAreEmpty()
		{
			Field.DisplayName = "Date Range";
			RequiredFilterValidator requiredFilterValidator = new RequiredFilterValidator();
			Field.Validators.Add(requiredFilterValidator);
			Field.ValueLow = ZDateTimeOffset.Empty;
			Field.ValueHigh = ZDateTimeOffset.Empty;

			AssertHasError(Field.ValueLowInfo, "'Date Range' should have data.");
			AssertHasError(Field.ValueHighInfo, "'Date Range' should have data.");

			Field.ValueLow = new ZDateTimeOffset(2006, 1, 1, 0, 0, 0, new TimeSpan(8, 0, 0));
			AssertNoError(Field.ValueLowInfo, "'Date Range' should have data.");
			AssertNoError(Field.ValueHighInfo, "'Date Range' should have data.");

			Field.ValueLow = ZDateTimeOffset.Empty;
			Field.ValueHigh = new ZDateTimeOffset(2007, 1, 1, 0, 0, 0, new TimeSpan(8, 0, 0));
			AssertNoError(Field.ValueLowInfo, "'Date Range' should have data.");
			AssertNoError(Field.ValueHighInfo, "'Date Range' should have data.");
		}
	}
}
