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
	[TestedType(typeof(DateRangeField))]
	class DateRangeFieldTest : FitlerFieldTestWithClearValues
	{
		[TestUtcOffset(-8, 0, 0)]
		[TestDate(2008, 8, 8)]
		public void TestConvertToUtc()
		{
			Field.FieldName = "Z0_DateTime";
			Field.ValueLow = new ZDateTime(2008, 6, 2);
			Field.ValueHigh = new ZDateTime(2010, 8, 28);
			Field.ConvertToUtc = false;
			{
				var parameters = Field.SqlParameters();
				var fromDate = (DateTime)parameters[0].Value;
				var toDate = (DateTime)parameters[1].Value;

				AssertEquals(new ZDateTime(2008, 6, 2), fromDate);
				AssertEquals(new ZDateTime(2010, 8, 29), toDate);
			}

			Field.ConvertToUtc = true;
			{
				var parameters = Field.SqlParameters();
				var fromDate = (DateTime)parameters[0].Value;
				var toDate = (DateTime)parameters[1].Value;

				AssertEquals(new ZDateTime(2008, 6, 2, 8, 0, 0), fromDate);
				AssertEquals(new ZDateTime(2010, 8, 29, 8, 0, 0), toDate);
			}

			Field.ConvertToUtc = false;
			{
				var parameters = Field.SqlParameters();
				var fromDate = (DateTime)parameters[0].Value;
				var toDate = (DateTime)parameters[1].Value;

				AssertEquals(new ZDateTime(2008, 6, 2), fromDate);
				AssertEquals(new ZDateTime(2010, 8, 29), toDate);
			}
		}

		public void TestCalculateToDateAccordingDateFormat()
		{
			Field.PickerFormat = DocEngineDatePickerFormats.Long;
			Field.ValueLow = new ZDateTime(2008, 6, 2, 5, 6, 0);
			Field.ValueHigh = new ZDateTime(2010, 8, 28, 5, 6, 0);
			{
				var parameters = Field.SqlParameters();
				var fromDate = (DateTime)parameters[0].Value;
				var toDate = (DateTime)parameters[1].Value;

				AssertEquals(new ZDateTime(2008, 6, 2, 5, 6, 0), fromDate);
				AssertEquals(new ZDateTime(2010, 8, 28, 5, 7, 0), toDate);
			}

			Field.PickerFormat = DocEngineDatePickerFormats.Short;
			Field.ValueLow = new ZDateTime(2008, 6, 2);
			Field.ValueHigh = new ZDateTime(2010, 8, 28);
			{
				var parameters = Field.SqlParameters();
				var fromDate = (DateTime)parameters[0].Value;
				var toDate = (DateTime)parameters[1].Value;

				AssertEquals(new ZDateTime(2008, 6, 2), fromDate);
				AssertEquals(new ZDateTime(2010, 8, 29), toDate);
			}

			Field.PickerFormat = DocEngineDatePickerFormats.YearAndMonth;
			Field.ValueLow = new ZDateTime(2008, 6, 2);
			Field.ValueHigh = new ZDateTime(2010, 8, 2);
			{
				var parameters = Field.SqlParameters();
				var fromDate = (DateTime)parameters[0].Value;
				var toDate = (DateTime)parameters[1].Value;

				AssertEquals(new ZDateTime(2008, 6, 1), fromDate);
				AssertEquals(new ZDateTime(2010, 9, 1), toDate);
			}

			var reportScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			reportScheduleTask.Recurrence.WeeklyRange = ZBool.True;
			Field.SetScheduleTask(reportScheduleTask);
			Field.PickerFormat = DocEngineDatePickerFormats.Long;
			Field.ValueLow = new ZDateTime(2008, 6, 2, 0, 0, 0); //Scheduled reports only support short date selection and doesnt support to date time level
			Field.ValueHigh = new ZDateTime(2010, 8, 28, 0, 0, 0); //Scheduled reports only support short date selection and doesnt support to date time level
			{
				var parameters = Field.SqlParameters();
				var fromDate = (DateTime)parameters[0].Value;
				var toDate = (DateTime)parameters[1].Value;

				AssertEquals(new ZDateTime(2008, 6, 2, 0, 0, 0), fromDate);
				AssertEquals(new ZDateTime(2010, 8, 29, 0, 0, 0), toDate);
			}
		}

		public void TestSubstituteMinDateMaxDateJsonConverter()
		{
			Field.FieldName = "MyDate";
			Field.DisplayName = "My Date";
			Field.ValueLow = new ZDateTime(2008, 8, 8);
			Field.SubstituteMinDateForNullFrom = true;
			Field.SubstituteMaxDateForNullTo = true;

			var result = JsonConverterHelper.Serialize(Field);
			var deserialisedField = JsonConverterHelper.Deserialize<DateRangeField>(result);

			AssertEquals("deserialisedField.FieldName", "MyDate", deserialisedField.FieldName);
			AssertEquals("deserialisedField.SubstituteMinDateForNullFrom", Field.SubstituteMinDateForNullFrom, deserialisedField.SubstituteMinDateForNullFrom);
			AssertEquals("deserialisedField.SubstituteMaxDateForNullTo", Field.SubstituteMaxDateForNullTo, deserialisedField.SubstituteMaxDateForNullTo);

			Field.SubstituteMinDateForNullFrom = false;
			Field.SubstituteMaxDateForNullTo = false;

			result = JsonConverterHelper.Serialize(Field);
			deserialisedField = JsonConverterHelper.Deserialize<DateRangeField>(result);

			AssertEquals("deserialisedField.FieldName", "MyDate", deserialisedField.FieldName);
			AssertEquals("deserialisedField.SubstituteMinDateForNullFrom", Field.SubstituteMinDateForNullFrom, deserialisedField.SubstituteMinDateForNullFrom);
			AssertEquals("deserialisedField.SubstituteMaxDateForNullTo", Field.SubstituteMaxDateForNullTo, deserialisedField.SubstituteMaxDateForNullTo);
		}

		[TestUtcOffset(-5, 0, 0)]
		public void TestFromDateParam()
		{
			Field.FieldName = "MyDate";
			Field.DisplayName = "My Date";
			Field.ValueLow = new ZDateTime(2008, 8, 8);

			using (var report = new Report(null, null))
			{
				Field.ConvertToUtc = false;

				AssertEquals("<My Date.FromDate>", new ZDateTime(2008, 8, 8),
					Field.ValueProviders["<My Date.FromDate>"].GetReplacement("<My Date.FromDate>", report));

				AssertEquals("<My Date.FromDateSqlParam> ConvertToUtc = false", new ZDateTime(2008, 8, 8),
					Field.ValueProviders["<My Date.FromDateSqlParam>"].GetReplacement("<My Date.FromDateSqlParam>", report));

				Field.ConvertToUtc = true;

				AssertEquals("<My Date.FromDate>", new ZDateTime(2008, 8, 8),
					Field.ValueProviders["<My Date.FromDate>"].GetReplacement("<My Date.FromDate>", report));

				AssertEquals("<My Date.FromDateParam> ConvertToUtc = true", new ZDateTime(2008, 8, 8, 5, 0, 0),
					Field.ValueProviders["<My Date.FromDateSqlParam>"].GetReplacement("<My Date.FromDateSqlParam>", report));
			}
		}

		[TestUtcOffset(-5, 0, 0)]
		public void TestFromDateUtc()
		{
			Field.FieldName = "MyDate";
			Field.DisplayName = "My Date";
			Field.ValueLow = new ZDateTime(2008, 8, 8);

			using (var report = new Report(null, null))
			{
				AssertEquals("<My Date.FromDate>", new ZDateTime(2008, 8, 8),
					Field.ValueProviders["<My Date.FromDate>"].GetReplacement("<My Date.FromDate>", report));

				AssertEquals("<My Date.FromDateUtc>", new ZDateTime(2008, 8, 8, 5, 0, 0),
					Field.ValueProviders["<My Date.FromDateUtc>"].GetReplacement("<My Date.FromDateUtc>", report));
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
				Field.ValueHigh = new ZDateTime(2008, 8, 8);

				Field.ConvertToUtc = false;

				AssertEquals("<My Date.ToDate>", new ZDateTime(2008, 8, 8),
					Field.ValueProviders["<My Date.ToDate>"].GetReplacement("<My Date.ToDate>", report));

				AssertEquals("<My Date.ToDateSqlParam> ConvertToUtc = false", new ZDateTime(2008, 8, 9),
					Field.ValueProviders["<My Date.ToDateSqlParam>"].GetReplacement("<My Date.ToDateSqlParam>", report));

				Field.ConvertToUtc = true;

				AssertEquals("<My Date.ToDateSqlParam> ConvertToUtc = true", new ZDateTime(2008, 8, 9, 6, 0, 0),
					Field.ValueProviders["<My Date.ToDateSqlParam>"].GetReplacement("<My Date.ToDateSqlParam>", report));

				Field.PickerFormat = DocEngineDatePickerFormats.Long;
				Field.ValueHigh = new ZDateTime(2008, 8, 8, 5, 15, 0);

				Field.ConvertToUtc = false;

				AssertEquals("<My Date.ToDate>", new ZDateTime(2008, 8, 8, 5, 15, 0),
					Field.ValueProviders["<My Date.ToDate>"].GetReplacement("<My Date.ToDate>", report));

				AssertEquals("<My Date.ToDateSqlParam> ConvertToUtc = false", new ZDateTime(2008, 8, 8, 5, 16, 0),
					Field.ValueProviders["<My Date.ToDateSqlParam>"].GetReplacement("<My Date.ToDateSqlParam>", report));

				Field.ConvertToUtc = true;

				AssertEquals("<My Date.ToDateSqlParam> ConvertToUtc = true", new ZDateTime(2008, 8, 8, 11, 16, 0),
					Field.ValueProviders["<My Date.ToDateSqlParam>"].GetReplacement("<My Date.ToDateSqlParam>", report));
			}
		}

		[TestUtcOffset(-6, 0, 0)]
		public void TestToDateUtc()
		{
			Field.FieldName = "MyDate";
			Field.DisplayName = "My Date";
			Field.ValueHigh = new ZDateTime(2008, 8, 8);

			using (var report = new Report(null, null))
			{
				AssertEquals("<My Date.ToDate>", new ZDateTime(2008, 8, 8),
					Field.ValueProviders["<My Date.ToDate>"].GetReplacement("<My Date.ToDate>", report));

				AssertEquals("<My Date.ToDateUtc>", new ZDateTime(2008, 8, 8, 6, 0, 0),
					Field.ValueProviders["<My Date.ToDateUtc>"].GetReplacement("<My Date.ToDateUtc>", report));
			}
		}

		[TestUtcOffset(-7, 0, 0)]
		public void TestToNextDateUtc()
		{
			Field.FieldName = "MyDate";
			Field.DisplayName = "My Date";
			Field.ValueHigh = new ZDateTime(2008, 8, 8);

			using (var report = new Report(null, null))
			{
				AssertEquals("<My Date.ToNextDate>", new ZDateTime(2008, 8, 9),
					Field.ValueProviders["<My Date.ToNextDate>"].GetReplacement("<My Date.ToNextDate>", report));

				AssertEquals("<My Date.ToNextDateUtc>", new ZDateTime(2008, 8, 9, 7, 0, 0),
					Field.ValueProviders["<My Date.ToNextDateUtc>"].GetReplacement("<My Date.ToNextDateUtc>", report));
			}
		}

		[TestUtcOffset(-8, 0, 0)]
		public void TestToEndDateUtc()
		{
			Field.FieldName = "MyDate";
			Field.DisplayName = "My Date";
			Field.ValueHigh = new ZDateTime(2008, 8, 8);

			using (var report = new Report(null, null))
			{
				AssertEquals("<My Date.ToEndDate>", new ZDateTime(2008, 8, 8, 23, 59, 59),
					Field.ValueProviders["<My Date.ToEndDate>"].GetReplacement("<My Date.ToEndDate>", report));

				AssertEquals("<My Date.ToEndDateUtc>", new ZDateTime(2008, 8, 9, 7, 59, 59),
					Field.ValueProviders["<My Date.ToEndDateUtc>"].GetReplacement("<My Date.ToEndDateUtc>", report));
			}
		}

		public void TestFromAndToDateForSQLParameter()
		{
			Field.FieldName = "MyDate";
			Field.DisplayName = "My Date";
			Field.ValueLow = new ZDateTime(2005, 1, 2);
			Field.ValueHigh = new ZDateTime(2005, 1, 4);

			using (var report = new Report(null, null))
			{
				var replacementFrom = Field.ValueProviders["<My Date.FromDateForSQLParameter>"].GetReplacement("<My Date.FromDateForSQLParameter>", report);
				var replacementTo = Field.ValueProviders["<My Date.ToDateForSQLParameter>"].GetReplacement("<My Date.ToDateForSQLParameter>", report);
				AssertType<ReplacementWithSqlDbType>(replacementFrom);
				AssertType<ReplacementWithSqlDbType>(replacementTo);
				AssertEquals(SqlDbType.DateTime, ((ReplacementWithSqlDbType)replacementFrom).SqlDbType);
				AssertEquals(SqlDbType.DateTime, ((ReplacementWithSqlDbType)replacementTo).SqlDbType);
				AssertEquals(new DateTime(2005, 1, 2), ((ReplacementWithSqlDbType)replacementFrom).MacroValue);
				AssertEquals(new DateTime(2005, 1, 4), ((ReplacementWithSqlDbType)replacementTo).MacroValue);
			}
		}

		public void TestFromAndToDateForSQLParameter_ForEmptyDate()
		{
			Field.FieldName = "MyDate";
			Field.DisplayName = "My Date";
			Field.ValueLow = ZDateTime.Empty;
			Field.ValueHigh = ZDateTime.Empty;

			using (var report = new Report(null, null))
			{
				var replacementFrom = Field.ValueProviders["<My Date.FromDateForSQLParameter>"].GetReplacement("<My Date.FromDateForSQLParameter>", report);
				var replacementTo = Field.ValueProviders["<My Date.ToDateForSQLParameter>"].GetReplacement("<My Date.ToDateForSQLParameter>", report);
				AssertType<ReplacementWithSqlDbType>(replacementFrom);
				AssertType<ReplacementWithSqlDbType>(replacementTo);
				AssertEquals(SqlDbType.DateTime, ((ReplacementWithSqlDbType)replacementFrom).SqlDbType);
				AssertEquals(SqlDbType.DateTime, ((ReplacementWithSqlDbType)replacementTo).SqlDbType);
				AssertEquals(DBNull.Value, ((ReplacementWithSqlDbType)replacementFrom).MacroValue);
				AssertEquals(DBNull.Value, ((ReplacementWithSqlDbType)replacementTo).MacroValue);
			}
		}

		public void TestToNextDateForSQLParameter()
		{
			Field.FieldName = "MyDate";
			Field.DisplayName = "My Date";
			Field.ValueLow = ZDateTime.Empty;
			Field.ValueHigh = ZDateTime.Empty;

			using (var report = new Report(null, null))
			{
				var replacementToNext = Field.ValueProviders["<My Date.ToNextDateForSQLParameter>"].GetReplacement("<My Date.ToNextDateForSQLParameter>", report);
				AssertType<ReplacementWithSqlDbType>(replacementToNext);
				AssertEquals(SqlDbType.DateTime, ((ReplacementWithSqlDbType)replacementToNext).SqlDbType);
				AssertEquals(DBNull.Value, ((ReplacementWithSqlDbType)replacementToNext).MacroValue);
			}

			Field.ValueHigh = new ZDateTime(2005, 1, 2);

			using (var report = new Report(null, null))
			{
				var replacementToNext = Field.ValueProviders["<My Date.ToNextDateForSQLParameter>"].GetReplacement("<My Date.ToNextDateForSQLParameter>", report);
				AssertType<ReplacementWithSqlDbType>(replacementToNext);
				AssertEquals(new DateTime(2005, 1, 3), ((ReplacementWithSqlDbType)replacementToNext).MacroValue);
			}
		}

		[TestUtcOffset(-6, 0, 0)]
		public void TestFromDateUtcForSQLParameter()
		{
			Field.FieldName = "MyDate";
			Field.DisplayName = "My Date";
			Field.ValueLow = ZDateTime.Empty;

			using (var report = new Report(null, null))
			{
				var replacementFromDateUtc = Field.ValueProviders["<My Date.FromDateUtcForSQLParameter>"].GetReplacement("<My Date.FromDateUtcForSQLParameter>", report);
				AssertType<ReplacementWithSqlDbType>(replacementFromDateUtc);
				AssertEquals(SqlDbType.DateTime, ((ReplacementWithSqlDbType)replacementFromDateUtc).SqlDbType);
				AssertEquals(DBNull.Value, ((ReplacementWithSqlDbType)replacementFromDateUtc).MacroValue);
			}

			Field.ValueLow = new ZDateTime(2005, 1, 2);

			using (var report = new Report(null, null))
			{
				var replacementFromDateUtc = Field.ValueProviders["<My Date.FromDateUtcForSQLParameter>"].GetReplacement("<My Date.FromDateUtcForSQLParameter>", report);
				AssertType<ReplacementWithSqlDbType>(replacementFromDateUtc);
				AssertEquals(new DateTime(2005, 1, 2, 6, 0, 0), ((ReplacementWithSqlDbType)replacementFromDateUtc).MacroValue);
			}
		}

		[TestUtcOffset(-6, 0, 0)]
		public void TestToNextDateUtcForSQLParameter()
		{
			Field.FieldName = "MyDate";
			Field.DisplayName = "My Date";
			Field.ValueHigh = ZDateTime.Empty;

			using (var report = new Report(null, null))
			{
				var replacementToNextDateUtc = Field.ValueProviders["<My Date.ToNextDateUtcForSQLParameter>"].GetReplacement("<My Date.ToNextDateUtcForSQLParameter>", report);
				AssertType<ReplacementWithSqlDbType>(replacementToNextDateUtc);
				AssertEquals(SqlDbType.DateTime, ((ReplacementWithSqlDbType)replacementToNextDateUtc).SqlDbType);
				AssertEquals(DBNull.Value, ((ReplacementWithSqlDbType)replacementToNextDateUtc).MacroValue);
			}

			Field.ValueHigh = new ZDateTime(2005, 1, 2);

			using (var report = new Report(null, null))
			{
				var replacementToNextDateUtc = Field.ValueProviders["<My Date.ToNextDateUtcForSQLParameter>"].GetReplacement("<My Date.ToNextDateUtcForSQLParameter>", report);
				AssertType<ReplacementWithSqlDbType>(replacementToNextDateUtc);
				AssertEquals(new DateTime(2005, 1, 3, 6, 0, 0), ((ReplacementWithSqlDbType)replacementToNextDateUtc).MacroValue);
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

		public void TestSerializationAndJsonConverter_KeepsTimeOnDatesEntered()
		{
			DocEngineDatePickerFormats pickerFormat = DocEngineDatePickerFormats.Long;
			ZDateTime valueLow = new ZDateTime(2006, 1, 5, 1, 2, 3);
			ZDateTime valueHigh = new ZDateTime(2007, 1, 5, 23, 22, 21);
			Field.PickerFormat = pickerFormat;
			Field.ValueLow = valueLow;
			Field.ValueHigh = valueHigh;
			TestJsonConverter(null, valueLow, valueHigh, pickerFormat);
		}

		public void TestSerialisationAndJsonConverter_EmptyScheduleValues()
		{
			ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2006, 10, 1);
			Field.SetScheduleTask(scheduleTask);
			TestJsonConverter(scheduleTask, ZDateTime.Empty, ZDateTime.Empty, DocEngineDatePickerFormats.Short);
		}

		public void TestSerialisationAndJsonConverter_ValidValues()
		{
			ZDateTime valueLow = new ZDateTime(2006, 1, 5);
			ZDateTime valueHigh = new ZDateTime(2007, 1, 5);
			DocEngineDatePickerFormats pickerFormat = DocEngineDatePickerFormats.Long;
			Field.ValueLow = valueLow;
			Field.ValueHigh = valueHigh;
			Field.PickerFormat = pickerFormat;
			TestJsonConverter(null, valueLow, valueHigh, pickerFormat);
		}

		public void TestJsonConverterSerialize_TimeOffset()
		{
			var valueLow = new DateTime(2024, 6, 1);
			var valueHigh = new DateTime(2024, 6, 20);
			var dateTimeOffsetFormat = "yyyy-MM-ddTHH:mm:sszzz";

			var expectedValueLow = (new DateTimeOffset(valueLow)).ToString(dateTimeOffsetFormat);
			var expectedValueHigh = (new DateTimeOffset(valueHigh)).ToString(dateTimeOffsetFormat);

			var pickerFormat = DocEngineDatePickerFormats.Short;
			Field.ValueLow = valueLow;
			Field.ValueHigh = valueHigh;
			Field.PickerFormat = pickerFormat;

			var serialisedField = JsonConverterHelper.Serialize(Field);

			AssertContains("serialisedField.ValueLow", expectedValueLow, serialisedField);
			AssertContains("serialisedField.ValueHigh", expectedValueHigh, serialisedField);
		}

		public void TestJsonConverterDeserialize_TimeOffset()
		{
			var json = "{\r\n          \"$type\": \"DateRangeFieldJsonData\",\r\n          \"ValueLow\": {\r\n            \"RunValue\": \"2024-06-01T00:00:00+14:00\",\r\n            \"Schedule\": null\r\n          },\r\n          \"ValueHigh\": {\r\n            \"RunValue\": \"2024-06-20T00:00:00+14:00\",\r\n            \"Schedule\": null\r\n          },\r\n          \"DateFormat\": \"Short\",\r\n          \"ConvertToUtc\": false,\r\n          \"RequireBothFromAndToDates\": false,\r\n          \"SubstituteMinDateForNullFrom\": false,\r\n          \"SubstituteMaxDateForNullTo\": false,\r\n          \"DisplayName\": \"Post Date\",\r\n          \"FieldName\": \"AH_PostDate\"\r\n        }";
			var deserialisedField = JsonConverterHelper.Deserialize<DateRangeField>(json);

			AssertEquals("deserialisedField.ValueLow", new ZDateTime(2024, 6, 1), deserialisedField.ValueLow);
			AssertEquals("deserialisedField.ValueHigh", new ZDateTime(2024, 6, 20), deserialisedField.ValueHigh);
		}

		public void TestSerialisationAndJsonConverter_ValidScheduleValues()
		{
			ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2006, 10, 1);
			Field.SetScheduleTask(scheduleTask);

			Field.LowSchedule.FillWithValidTestData();
			Field.LowSchedule.ByMonth = true;
			Field.LowSchedule.DayNumber = 3;

			Field.HighSchedule.FillWithValidTestData();
			Field.HighSchedule.ByMonth = true;
			Field.HighSchedule.DayNumber = 5;

			TestJsonConverter(scheduleTask, new ZDateTime(2006, 10, 3), new ZDateTime(2006, 10, 5), DocEngineDatePickerFormats.Short);
		}

		public void TestWhereClauseWithInput()
		{
			Field.FieldName = "abc";
			Field.ValueLow = new DateTime(2003, 1, 25);
			Field.ValueHigh = new DateTime(2003, 3, 21);
			Match whereClauseMatch = Regex.Match(Field.WhereClause(), @"abc >= (@p[0-9]+) AND abc < (@p[0-9]+)");
			Assert("WhereClause should be of the form 'abc >= @p123 AND abc < @p456' but was: " + Field.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Should have 2 params", 2, Field.SqlParameters().Count);
			AssertEquals("Param 1's name should match the name in the where clause", whereClauseMatch.Groups[1].Value, Field.SqlParameters()[0].ToString());
			AssertEquals("Param 1's value should be set", new DateTime(2003, 1, 25), Field.SqlParameters()[0].Value);
			AssertEquals("Param 2's name should match the name in the where clause", whereClauseMatch.Groups[2].Value, Field.SqlParameters()[1].ToString());
			AssertEquals("Param 2's value should be set", new DateTime(2003, 3, 22), Field.SqlParameters()[1].Value);
		}

		public void TestWhereClauseWithOnlyYearAndMonth()
		{
			Field.FieldName = "abc";
			Field.PickerFormat = DocEngineDatePickerFormats.YearAndMonth;
			Field.ValueLow = new DateTime(2003, 1, 25);
			Field.ValueHigh = new DateTime(2003, 3, 21);
			Match whereClauseMatch = Regex.Match(Field.WhereClause(), @"abc >= (@p[0-9]+) AND abc < (@p[0-9]+)");
			Assert("WhereClause should be of the form 'abc >= @p123 AND abc < @p456' but was: " + Field.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Should have 2 params", 2, Field.SqlParameters().Count);
			AssertEquals("Param 1's name should match the name in the where clause", whereClauseMatch.Groups[1].Value, Field.SqlParameters()[0].ToString());
			AssertEquals("Param 1's value should be set", new DateTime(2003, 1, 1), Field.SqlParameters()[0].Value);
			AssertEquals("Param 2's name should match the name in the where clause", whereClauseMatch.Groups[2].Value, Field.SqlParameters()[1].ToString());
			AssertEquals("Param 2's value should be set", new DateTime(2003, 4, 1), Field.SqlParameters()[1].Value);
		}

		public void TestOnlyUsesDatePart()
		{
			Field.ValueLow = new DateTime(2003, 1, 25, 11, 11, 11, 11);
			AssertEquals("Should only use date part", new ZDateTime(new DateTime(2003, 1, 25, 0, 0, 0, 0)), Field.ValueLow);
			Field.ValueHigh = new DateTime(2003, 1, 25, 11, 11, 11, 11);
			AssertEquals("Should only use date part", new ZDateTime(new DateTime(2003, 1, 25, 0, 0, 0, 0)), Field.ValueHigh);
		}

		public void TestAlsoUsesTimePartInLongFormat()
		{
			Field.PickerFormat = DocEngineDatePickerFormats.Long;
			Field.ValueLow = new DateTime(2003, 1, 25, 11, 11, 11, 11);
			AssertEquals("Should also use time part", new ZDateTime(new DateTime(2003, 1, 25, 11, 11, 11, 11)), Field.ValueLow);
			Field.ValueHigh = new DateTime(2003, 1, 25, 11, 11, 11, 11);
			AssertEquals("Should also use time part", new ZDateTime(new DateTime(2003, 1, 25, 11, 11, 11, 11)), Field.ValueHigh);
		}

		public void TestWhereClauseWithLowerInput()
		{
			Field.FieldName = "abc";
			Field.ValueLow = new DateTime(2003, 1, 25);
			Match whereClauseMatch = Regex.Match(Field.WhereClause(), @"abc >= (@p[0-9]+)");
			Assert("WhereClause should be of the form 'abc >= @p123' but was: " + Field.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Should have 2 params", 2, Field.SqlParameters().Count);
			AssertEquals("Param 1's name should match the name in the where clause", whereClauseMatch.Groups[1].Value, Field.SqlParameters()[0].ToString());
			AssertEquals("Param 1's value should be set", new DateTime(2003, 1, 25), Field.SqlParameters()[0].Value);
		}

		public void TestWhereClauseWithUpperInput()
		{
			Field.FieldName = "abc";
			Field.ValueHigh = new DateTime(2003, 3, 21);
			Match whereClauseMatch = Regex.Match(Field.WhereClause(), @"abc < (@p[0-9]+)");
			Assert("WhereClause should be of the form 'abc < @p456' but was: " + Field.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Should have 2 params", 2, Field.SqlParameters().Count);
			AssertEquals("Param 2's name should match the name in the where clause", whereClauseMatch.Groups[1].Value, Field.SqlParameters()[1].ToString());
			AssertEquals("Param 2's value should be set", new DateTime(2003, 3, 22), Field.SqlParameters()[1].Value);
		}

		public void TestIsResponsibleOldWay()
		{
			DateRangeField df = CreateTestDateRange();
			int responsibleCount = 0;
			foreach (ValueProvider provider in df.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestDateField>", Passes.FirstPass))
				{
					responsibleCount++;
				}
			}
			AssertEquals("Should have one responsible!", 1, responsibleCount);
		}

		public void TestGetReplacementOldWay()
		{
			var df = CreateTestDateRange();
			foreach (ValueProvider provider in df.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestDateField>", Passes.FirstPass))
				{
					AssertEquals("From: 01-Dec-04 To: 10-Dec-04", provider.GetReplacement("<TestDateField>", new Report(null, null)));
				}
			}

			var reportScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			reportScheduleTask.Recurrence.WeeklyRange = ZBool.True;
			reportScheduleTask.CalcNextRunTimeLocal = new DateTime(2004, 12, 2);
			df.PickerFormat = DocEngineDatePickerFormats.Long;
			df.SetScheduleTask(reportScheduleTask);
			df.LowSchedule.ByWeek = true;
			df.LowSchedule.DayName = "WED";
			df.LowSchedule.PeriodScope = PeriodScopeList.Codes.This;
			df.HighSchedule.ByWeek = true;
			df.HighSchedule.DayName = "FRI";
			df.HighSchedule.PeriodScope = PeriodScopeList.Codes.Next;
			df.HighSchedule.PeriodCount = 1;

			foreach (ValueProvider provider in df.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestDateField>", Passes.FirstPass))
				{
					AssertEquals("From: 01-Dec-04 00:00 To: 10-Dec-04 23:59", provider.GetReplacement("<TestDateField>", new Report(null, null)));
				}
			}
		}

		[ExpectNoExceptions]
		public void TestIsResponsibleOldWayWithEmptyHighValue()
		{
			var df = CreateTestDateRange();
			df.ValueHigh = ZDateTime.Empty;
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
			DateRangeField df = CreateTestDateRange();

			foreach (ValueProvider provider in df.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestDateField.FromDate>", Passes.FirstPass))
				{
					AssertEquals(new ZDateTime(2004, 12, 1), provider.GetReplacement("<TestDateField.FromDate>", new Report(null, null)));
				}
			}
		}

		public void TestIsResponsibleNewWayToDate()
		{
			DateRangeField df = CreateTestDateRange();

			foreach (ValueProvider provider in df.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestDateField.ToDate>", Passes.FirstPass))
				{
					AssertEquals(new ZDateTime(2004, 12, 10), provider.GetReplacement("<TestDateField.ToDate>", new Report(null, null)));
				}
			}
		}

		public void TestIsResponsibleNewWayToNextDate()
		{
			DateRangeField df = CreateTestDateRange();

			foreach (ValueProvider provider in df.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestDateField.ToNextDate>", Passes.FirstPass))
				{
					AssertEquals(new ZDateTime(2004, 12, 11), provider.GetReplacement("<TestDateField.ToNextDate>", new Report(null, null)));
				}
			}
		}

		public void TestGetReplacementForToNextDateHandlesEmptyDate()
		{
			DateRangeField df = CreateTestDateRange();
			df.ValueHigh = ZDateTime.Empty;

			foreach (ValueProvider provider in df.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestDateField.ToNextDate>", Passes.FirstPass))
				{
					AssertEquals(ZDateTime.Empty, provider.GetReplacement("<TestDateField.ToNextDate>", new Report(null, null)));
				}
			}
		}

		public void TestGetReplacementForToEndDate()
		{
			DateRangeField df = CreateTestDateRange();
			df.ValueHigh = ZDateTime.BrettsBirthday;

			foreach (ValueProvider provider in df.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestDateField.ToEndDate>", Passes.FirstPass))
				{
					AssertEquals(ZDateTime.BrettsBirthday.AddDays(1).AddSeconds(-1), provider.GetReplacement("<TestDateField.ToEndDate>", new Report(null, null)));
				}
			}
		}

		public void TestGetReplacementForToNextDateHandlesInvalidDate()
		{
			DateRangeField df = CreateTestDateRange();
			df.ValueHigh = ZDateTime.Invalid;

			foreach (ValueProvider provider in df.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestDateField.ToNextDate>", Passes.FirstPass))
				{
					AssertEquals(ZDateTime.Empty, provider.GetReplacement("<TestDateField.ToNextDate>", new Report(null, null)));
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
							AssertEquals("从: 01-Dec-04 到: 10-Dec-04", provider.GetReplacement("<TestDateField>", new Report(null, null)));
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
			Field.ValueHigh = new ZDateTime(2012, 12, 12, 12, 0, 0, 0);
			Field.ValueLow = new ZDateTime(2011, 11, 11, 11, 0, 0, 0);

			AssertEquals("From: 11-Nov-11 11:00 To: 12-Dec-12 12:00", Field.ValueAsObject);
		}

		public void TestGetReplacementNewWay()
		{
			DateRangeField df = CreateTestDateRange();

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
			Field.ValueHigh = new DateTime(2004, 12, 10);
			Field.SubstituteMaxDateForNullTo = true;

			foreach (ValueProvider provider in Field.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestDateField.ToDate>", Passes.FirstPass))
				{
					AssertEquals("ToDate will not substitute when date is not null", new DateTime(2004, 12, 10), provider.GetReplacement("<TestDateField.ToDate>", new Report(null, null)));
					providerExists = true;
				}
			}
			AssertEquals("There should have been a toDate provider", true, providerExists);

			providerExists = false;
			Field.ValueHigh = ZDateTime.Invalid;
			foreach (ValueProvider provider in Field.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestDateField.ToDate>", Passes.FirstPass))
				{
					AssertEquals("ToDate will substitute with max date when date is null", ZDateTime.MaxSmallDateTimeValue, provider.GetReplacement("<TestDateField.ToDate>", new Report(null, null)));
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
			Field.ValueHigh = new DateTime(2004, 12, 10);
			Field.SubstituteMaxDateForNullTo = true;

			foreach (ValueProvider provider in Field.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestDateField.ToNextDate>", Passes.FirstPass))
				{
					AssertEquals("ToNextDate will not substitute when date is not null", new DateTime(2004, 12, 11), provider.GetReplacement("<TestDateField.ToNextDate>", new Report(null, null)));
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
					AssertEquals("ToNextDate will substitute with max date when date is null", ZDateTime.MaxSmallDateTimeValue, provider.GetReplacement("<TestDateField.ToNextDate>", new Report(null, null)));
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
			Field.ValueLow = new DateTime(2004, 12, 10);
			Field.SubstituteMinDateForNullFrom = true;

			foreach (ValueProvider provider in Field.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestDateField.FromDate>", Passes.FirstPass))
				{
					AssertEquals("FromDate will not substitute when date is not null", new DateTime(2004, 12, 10), provider.GetReplacement("<TestDateField.FromDate>", new Report(null, null)));
					providerExists = true;
				}
			}
			AssertEquals("There should have been a FromDate provider", true, providerExists);

			providerExists = false;
			Field.ValueLow = ZDateTime.Invalid;
			foreach (ValueProvider provider in Field.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestDateField.FromDate>", Passes.FirstPass))
				{
					AssertEquals("FromDate will substitute with min date when date is null", ZDateTime.MinSmallDateTimeValue, provider.GetReplacement("<TestDateField.FromDate>", new Report(null, null)));
					providerExists = true;
				}
			}
			AssertEquals("There should have been a toDate provider", true, providerExists);
		}

		public void TestFromIsNotAfterTo()
		{
			DateRangeField df = CreateTestDateRange();
			df.ValueLow = new DateTime(2003, 1, 25);
			df.ValueHigh = new DateTime(2003, 1, 24);
			df.RunPreSaveValidation();
			AssertHasErrors("There is notification error in ValueInfo of low", df.ValueLowInfo);
			AssertHasErrors("There is notification error in ValueInfo of high", df.ValueHighInfo);
		}

		[TestUtcOffset(2, 0, 0)]
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
			ZDateTime previousValueLow = Field.ValueLow;
			ZDateTime previousValueHigh = Field.ValueHigh;

			ReportScheduleTask scheduleTask2 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask2.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2002, 10, 2);
			Field.SetScheduleTask(scheduleTask2);

			Assert("ValueLow should have changed.", Field.ValueLow != previousValueLow);
			Assert("ValueHigh should have changed.", Field.ValueHigh != previousValueHigh);
			AssertEquals("LowSchedule.ScheduleTask", scheduleTask2, Field.LowSchedule.ScheduleTask);
			AssertEquals("HighSchedule.ScheduleTask", scheduleTask2, Field.HighSchedule.ScheduleTask);
			AssertEquals("ValueLow", Field.LowSchedule.GetScheduleDate().Date, Field.ValueLow);
			AssertEquals("ValueHigh", Field.HighSchedule.GetScheduleDate().Date, Field.ValueHigh);
			AssertEquals("ValueLow.IsEmpty", false, Field.ValueLow.IsEmpty);
			AssertEquals("ValueHigh.IsEmpty", false, Field.ValueHigh.IsEmpty);
		}

		[TestUtcOffset(-2, 0, 0)]
		public void TestValueIsUpdatedWhenScheduleChanges()
		{
			Field.ValueLow = new ZDateTime(2003, 3, 2);
			Field.ValueHigh = new ZDateTime(2003, 3, 10);

			ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDate.Today;
			Field.SetScheduleTask(scheduleTask);

			Field.LowSchedule.FillWithValidTestData();
			Field.LowSchedule.DayNumber = 3;
			Field.HighSchedule.FillWithValidTestData();
			Field.HighSchedule.DayNumber = 6;

			AssertEquals("ValueLow", Field.LowSchedule.GetScheduleDate().Date, Field.ValueLow);
			AssertEquals("ValueHigh", Field.HighSchedule.GetScheduleDate().Date, Field.ValueHigh);
			AssertEquals("ValueLow.IsEmpty", false, Field.ValueLow.IsEmpty);
			AssertEquals("ValueHigh.IsEmpty", false, Field.ValueHigh.IsEmpty);
		}

		[TestUtcOffset(-5, 0, 0)]
		public void TestGetFromDateTimeOffset()
		{
			Field.FieldName = "MyDate";
			Field.DisplayName = "My Date";
			Field.ValueLow = new ZDateTime(2005, 1, 2);

			using (var report = new Report(null, null))
			{
				AssertEquals("<My Date.FromDate.ToDateTimeOffset> should have time offset", new ZDateTimeOffset(2005, 1, 2, 0, 0, 0, new TimeSpan(-5, 0, 0)),
					Field.ValueProviders["<My Date.FromDate.ToDateTimeOffset>"].GetReplacement("<My Date.FromDate.ToDateTimeOffset>", report));
			}
		}

		[TestUtcOffset(-6, 0, 0)]
		public void TestGetToDateTimeOffset()
		{
			Field.FieldName = "MyDate";
			Field.DisplayName = "My Date";
			Field.ValueHigh = new ZDateTime(2005, 1, 4);

			using (var report = new Report(null, null))
			{
				AssertEquals("<My Date.ToDate.ToDateTimeOffset> should have time offset", new ZDateTimeOffset(2005, 1, 4, 0, 0, 0, new TimeSpan(-6, 0, 0)),
					Field.ValueProviders["<My Date.ToDate.ToDateTimeOffset>"].GetReplacement("<My Date.ToDate.ToDateTimeOffset>", report));
			}
		}

		[TestUtcOffset(-10, 0, 0)]
		[ExpectNoExceptions]
		public void TestEmptyFromDateTimeOffset()
		{
			Field.FieldName = "MyDate";
			Field.DisplayName = "My Date";

			using (var report = new Report(null, null))
			{
				AssertEquals("<My Date.FromDate.ToDateTimeOffset> should be empty", ZDateTime.Empty,
					Field.ValueProviders["<My Date.FromDate.ToDateTimeOffset>"].GetReplacement("<My Date.FromDate.ToDateTimeOffset>", report));
			}
		}

		[TestUtcOffset(-12, 0, 0)]
		[ExpectNoExceptions]
		public void TestEmptyToDateTimeOffset()
		{
			Field.FieldName = "MyDate";
			Field.DisplayName = "My Date";

			using (var report = new Report(null, null))
			{
				AssertEquals("<My Date.ToDate.ToDateTimeOffset> should be empty", ZDateTime.Empty,
					Field.ValueProviders["<My Date.ToDate.ToDateTimeOffset>"].GetReplacement("<My Date.ToDate.ToDateTimeOffset>", report));
			}
		}

		[TestUtcOffset(-10, 0, 0)]
		[ExpectNoExceptions]
		public void TestEmptyFromDate_SubstituteMinDate()
		{
			Field.FieldName = "MyDate";
			Field.DisplayName = "My Date";
			Field.SubstituteMinDateForNullFrom = true;

			using (var report = new Report(null, null))
			{
				AssertEquals("<My Date.FromDate.ToDateTimeOffset> should have time offset", new ZDateTimeOffset(ZDateTime.MinSmallDateTimeValue, new TimeSpan(-10, 0, 0)),
					Field.ValueProviders["<My Date.FromDate.ToDateTimeOffset>"].GetReplacement("<My Date.FromDate.ToDateTimeOffset>", report));
			}
		}

		[TestUtcOffset(-12, 0, 0)]
		[ExpectNoExceptions]
		public void TestEmptyToDate_SubstituteMaxDate()
		{
			Field.FieldName = "MyDate";
			Field.DisplayName = "My Date";
			Field.SubstituteMaxDateForNullTo = true;

			using (var report = new Report(null, null))
			{
				AssertEquals("<My Date.ToDate.ToDateTimeOffset> should have time offset", new ZDateTimeOffset(ZDateTime.MaxSmallDateTimeValue, new TimeSpan(-12, 0, 0)),
					Field.ValueProviders["<My Date.ToDate.ToDateTimeOffset>"].GetReplacement("<My Date.ToDate.ToDateTimeOffset>", report));
			}
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
			var field = (DateRangeField)GetNewBusinessObject();
			ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			field.SetScheduleTask(scheduleTask);

			AssertEquals("Low Schedule, Period Scope is Empty", string.Empty, field.LowSchedule.PeriodScope);
			AssertEquals("High Schedule, Period Scope is Empty", string.Empty, field.HighSchedule.PeriodScope);
		}

		public void TestRunAndScheduleJsonConverter()
		{
			var runValueLow = new ZDateTime(1979, 2, 12);
			var runValueHigh = new ZDateTime(2007, 10, 16);

			var sourceField = new DateRangeField(Factory);
			sourceField.ValueLow = runValueLow;
			sourceField.ValueHigh = runValueHigh;

			var result = JsonConverterHelper.Serialize(sourceField);
			var deserialisedField = JsonConverterHelper.Deserialize<DateRangeField>(result);

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
			var deserialisedField2 = JsonConverterHelper.Deserialize<DateRangeField>(result);

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

			Field.ValueLow = ZDateTime.Empty;
			AssertHasError(Field.ValueLowInfo, MockFilterCollectionValidator.ErrorMessage);

			validator.SetIsValid(true);
			Field.ValueHigh = new ZDateTime(2006, 5, 5);
			Field.ValueLow = new ZDateTime(2006, 12, 31);
			AssertHasError(Field.ValueLowInfo, "The 'From date' must be before 'To date'");

			Field.ValueLow = new ZDateTime(2006, 1, 1);
			AssertNoErrors(Field.ValueLowInfo);
		}

		public virtual void TestValidateValueHigh()
		{
			MockFilterCollectionValidator validator = new MockFilterCollectionValidator();
			Field.Validators.Add(validator);
			validator.SetIsValid(false);

			Field.ValueHigh = ZDateTime.Empty;
			AssertHasError(Field.ValueHighInfo, MockFilterCollectionValidator.ErrorMessage);

			validator.SetIsValid(true);
			Field.ValueLow = new ZDateTime(2006, 5, 5);
			Field.ValueHigh = new ZDateTime(2006, 1, 1);
			AssertHasError(Field.ValueHighInfo, "The 'To date' must be after 'From date'");

			Field.ValueHigh = new ZDateTime(2006, 12, 31);
			AssertNoErrors(Field.ValueHighInfo);
		}

		public override void TestSafeCopyValuesFrom()
		{
			var runValueLow = new ZDateTime(1979, 2, 12);
			var runValueHigh = new ZDateTime(2007, 10, 16);

			var sourceField = new DateRangeField(Factory);
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

			var destinationField = new DateRangeField(Factory);
			((IFilter)destinationField).SafeCopyValuesFrom(sourceField);
			AssertEquals("Run ValueLow", runValueLow, destinationField.ValueLow);
			AssertEquals("Run ValueHigh", runValueHigh, destinationField.ValueHigh);

			destinationField.SetScheduleTask(scheduleTask);
			AssertEquals("LowSchedule.Period", sourceField.LowSchedule.Period, destinationField.LowSchedule.Period);
			AssertEquals("LowSchedule.PeriodScope", sourceField.LowSchedule.PeriodScope, destinationField.LowSchedule.PeriodScope);
			AssertEquals("LowSchedule.PeriodCount", sourceField.LowSchedule.PeriodCount, destinationField.LowSchedule.PeriodCount);
			AssertEquals("ValueLow", ZDate.Today.AddDays(-3), destinationField.ValueLow);

			AssertEquals("HighSchedule.Period", sourceField.HighSchedule.Period, destinationField.HighSchedule.Period);
			AssertEquals("HighSchedule.PeriodScope", sourceField.HighSchedule.PeriodScope, destinationField.HighSchedule.PeriodScope);
			AssertEquals("HighSchedule.PeriodCount", sourceField.HighSchedule.PeriodCount, destinationField.HighSchedule.PeriodCount);
			AssertEquals("ValueHigh", ZDate.Today.AddDays(5), destinationField.ValueHigh);
		}

		public void TestCopyADateRangeFieldFromAnotherOneWithHourAndMinuteDateSchedule()
		{
			var sourceField = new DateRangeField(Factory);
			var scheduleTask1 = Factory.NewWithValidTestData<ReportScheduleTask>();
			sourceField.SetScheduleTask(scheduleTask1);

			sourceField.LowSchedule.Period = ScheduleRecurrenceType.HourAndMinute;
			sourceField.HighSchedule.Period = ScheduleRecurrenceType.HourAndMinute;

			var copiedField = new DateRangeField(Factory);
			var scheduleTask2 = Factory.NewWithValidTestData<ReportScheduleTask>();
			copiedField.SetScheduleTask(scheduleTask2);

			AssertNoExceptionThrown("No exception should be thrown when ScheduleRecurrenceType is HourAndMinute if a dateSchedule.ScheduleTask is null", () => copiedField.SafeCopyValuesFrom(sourceField));
			AssertNull("Precondition: No ScheduleTask assigned to the copied DateRange.LowSchedule", copiedField.LowSchedule.ScheduleTask);
			AssertNull("Precondition: No ScheduleTask assigned to the copied DateRange.HighSchedule", copiedField.HighSchedule.ScheduleTask);
			AssertEquals("Should return ZDateTime.Empty when ScheduleRecurrenceType is HourAndMinute if a dateSchedule.ScheduleTask is null ", ZDateTime.Empty, copiedField.LowSchedule.BaseDateTimeForTesting);
		}

		public override void TestClearValues()
		{
			DateRangeField field = new DateRangeField(Factory);
			field.ValueLow = new ZDateTime(1979, 2, 12, 11, 22, 33);
			field.ValueLow = new ZDateTime(2007, 10, 16, 10, 21, 00);
			((IFilter)field).ClearValues();
			AssertEquals(field.ValueLow, ZDateTime.Empty);
			AssertEquals(field.ValueHigh, ZDateTime.Empty);
		}

		public void TestFillFilterData()
		{
			Field.ValueLow = new ZDateTime(2019, 3, 15);
			Field.ValueHigh = new ZDateTime(2019, 3, 16);
			Field.PickerFormat = DocEngineDatePickerFormats.Long;
			var reportData = new SelectedValueReportData();

			Field.FillFilterData(reportData.FilterData);

			CombineAssertions("FillFilterData will produce the expected value.", () =>
			{
				AssertEquals(1, reportData.FilterData.DateRangeFilterCollection.Count);
				AssertEquals(new DateTime(2019, 3, 15), reportData.FilterData.DateRangeFilterCollection[0].ValueLow);
				AssertEquals(new DateTime(2019, 3, 16), reportData.FilterData.DateRangeFilterCollection[0].ValueHigh);
				AssertEquals("Long", reportData.FilterData.DateRangeFilterCollection[0].DateFormat);
			});

			reportData.FilterData.DateRangeFilterCollection.Clear();

			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2024, 4, 2);
			Field.SetScheduleTask(scheduleTask);

			var reportFilterData = new ReportFilterData();
			reportFilterData.DateRangeFilterCollection.Add(new DateRangeFilter { ValueLow = new DateTime(1909, 11, 1, 0, 7, 0), ValueHigh = new DateTime(1910, 1, 1, 0, 1, 0), DisplayName = Field.DisplayName });
			Field.SetFilterValue(reportFilterData);

			Field.FillFilterData(reportData.FilterData);

			CombineAssertions("FillFilterData will produce the expected value.", () =>
			{
				AssertEquals(1, reportData.FilterData.DateRangeFilterCollection.Count);
				AssertEquals(new DateTime(1909, 11, 1, 0, 7, 0), reportData.FilterData.DateRangeFilterCollection[0].ValueLow);
				AssertEquals(new DateTime(1910, 1, 1, 0, 1, 0), reportData.FilterData.DateRangeFilterCollection[0].ValueHigh);
				AssertEquals("Long", reportData.FilterData.DateRangeFilterCollection[0].DateFormat);
			});
		}

		public void TestSetFilterValue()
		{
			var reportFilterData = new ReportFilterData();
			reportFilterData.DateRangeFilterCollection.Add(new DateRangeFilter { ValueLow = new DateTime(2024, 4, 5), ValueHigh = new DateTime(2024, 4, 8), DisplayName = "abc" });
			Field.DisplayName = "abc";
			Field.SetFilterValue(reportFilterData);
			AssertEquals("Run value - Low: ", new DateTime(2024, 4, 5), Field.ValueLow);
			AssertEquals("Run value - High: ", new DateTime(2024, 4, 8), Field.ValueHigh);

			reportFilterData.DateRangeFilterCollection.Clear();

			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2024, 4, 2);
			Field.SetScheduleTask(scheduleTask);
			reportFilterData.DateRangeFilterCollection.Add(new DateRangeFilter { ValueLow = new DateTime(1909, 11, 1, 0, 7, 0), ValueHigh = new DateTime(1910, 1, 1, 0, 1, 0), DisplayName = "abc" });

			Field.SetFilterValue(reportFilterData);

			AssertEquals("Schedule Storage value - Low:", new DateTime(1909, 11, 1, 0, 7, 0), Field.LowSchedule.ToStorageValue());
			AssertEquals("Schedule value - Low:", new DateTime(2024, 3, 31), Field.ValueHigh);
			AssertEquals("Schedule Storage value - High:", new DateTime(1910, 1, 1, 0, 1, 0), Field.HighSchedule.ToStorageValue());
			AssertEquals("Schedule value - Low:", new DateTime(2024, 3, 23), Field.ValueLow);

			reportFilterData.DateRangeFilterCollection.Clear();
			Field.ClearValues();
			reportFilterData.DateRangeFilterCollection.Add(new DateRangeFilter { DisplayName = "abc" });
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

		internal DateRangeField Field
		{
			get
			{
				if (field == null)
				{
					field = (DateRangeField)GetNewBusinessObject();
				}

				return field;
			}
		}

		DateRangeField CreateTestDateRange()
		{
			DateRangeField result = (DateRangeField)GetNewBusinessObject();
			result.FieldName = "abc";
			result.DisplayName = "TestDateField";
			result.ValueLow = new DateTime(2004, 12, 1);
			result.ValueHigh = new DateTime(2004, 12, 10);
			return result;
		}

		void TestJsonConverter(ReportScheduleTask scheduleTask, ZDateTime expectedValueLow, ZDateTime expectedValueHigh, DocEngineDatePickerFormats expectedPickerFormat)
		{
			var scheduled = (scheduleTask != null);

			Field.FieldName = "abc";

			var result = JsonConverterHelper.Serialize(Field);
			var deserialisedField = JsonConverterHelper.Deserialize<DateRangeField>(result);

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

		DateRangeField field;

		public virtual void TestRequiredValidationErrorOnlyDisplaysWhenBothFieldsAreEmpty()
		{
			Field.DisplayName = "Date Range";
			RequiredFilterValidator requiredFilterValidator = new RequiredFilterValidator();
			Field.Validators.Add(requiredFilterValidator);
			Field.ValueLow = ZDateTime.Empty;
			Field.ValueHigh = ZDateTime.Empty;

			AssertHasError(Field.ValueLowInfo, "'Date Range' should have data.");
			AssertHasError(Field.ValueHighInfo, "'Date Range' should have data.");

			Field.ValueLow = new DateTime(2006, 1, 1);
			AssertNoError(Field.ValueLowInfo, "'Date Range' should have data.");
			AssertNoError(Field.ValueHighInfo, "'Date Range' should have data.");

			Field.ValueLow = ZDateTime.Empty;
			Field.ValueHigh = new ZDateTime(2007, 1, 1);
			AssertNoError(Field.ValueLowInfo, "'Date Range' should have data.");
			AssertNoError(Field.ValueHighInfo, "'Date Range' should have data.");
		}
	}
}
