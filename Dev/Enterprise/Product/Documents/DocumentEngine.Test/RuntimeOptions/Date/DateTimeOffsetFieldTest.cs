using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.Scheduler.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(DateTimeOffsetField))]
	class DateTimeOffsetFieldTest : FitlerFieldTestWithClearValues
	{
		public void TestJsonConverter()
		{
			var testTime = new DateTimeOffset(2023, 5, 8, 0, 0, 0, TimeSpan.FromHours(8));
			Field.DisplayName = "Json Test";
			Field.FieldName = "TestField";
			Field.Value = testTime;

			var result = JsonConverterHelper.Serialize(Field);
			var deserialisedField = JsonConverterHelper.Deserialize(result, Field.GetType()) as DateTimeOffsetField;

			AssertEquals("Json Test", deserialisedField.DisplayName);
			AssertEquals("TestField", deserialisedField.FieldName);
			AssertEquals(testTime, deserialisedField.Value);
		}

		public void TestNoExceptionThrownWhenCallingGetToNextDate()
		{
			Field.DisplayName = "ADateField";
			Field.Value = ZDateTimeOffset.Empty;
			var macro = "<ADateField.ToNextDate>";
			var toNextDateValueProvider = Field.ValueProviders.First(v => v.IsResponsibleForReplacing(macro, Passes.FirstPass));
			AssertEquals(ZDateTimeOffset.Empty, toNextDateValueProvider.GetReplacement(macro, new Report(null, null)));

			Field.Value = ZDateTimeOffset.Today;
			AssertEquals(ZDateTimeOffset.Today.AddDays(1), toNextDateValueProvider.GetReplacement(macro, new Report(null, null)));
		}

		public virtual void TestWhereClauseWithInput()
		{
			Field.FieldName = "abc";
			Field.Value = new DateTimeOffset(2003, 1, 25, 0, 0, 0, TimeSpan.FromHours(8));
			Match whereClauseMatch = Regex.Match(Field.WhereClause(), @"abc >= (@p[0-9]+) AND abc < (@p[0-9]+)");
			Assert("WhereClause should be of the form 'abc >= @p123 AND abc < @p456' but was: " + Field.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Should have 2 params", 2, Field.SqlParameters().Count);
			AssertEquals("Param 1's name should match the name in the where clause", whereClauseMatch.Groups[1].Value, Field.SqlParameters()[0].ToString());
			AssertEquals("Param 1's value should be set", new DateTimeOffset(2003, 1, 25, 0, 0, 0, TimeSpan.FromHours(8)), Field.SqlParameters()[0].Value);
			AssertEquals("Param 2's name should match the name in the where clause", whereClauseMatch.Groups[2].Value, Field.SqlParameters()[1].ToString());
			AssertEquals("Param 2's value should be set", new DateTimeOffset(2003, 1, 26, 0, 0, 0, TimeSpan.FromHours(8)), Field.SqlParameters()[1].Value);
		}

		public void TestAlsoUsesTimePartInLongFormat()
		{
			Field.PickerFormat = DocEngineDatePickerFormats.Long;
			Field.Value = new DateTimeOffset(2003, 1, 25, 11, 11, 11, 11, TimeSpan.FromHours(8));
			AssertEquals("Should also use time part", new ZDateTimeOffset(new DateTimeOffset(2003, 1, 25, 11, 11, 11, 11, TimeSpan.FromHours(8))), Field.Value);
		}

		public void TestValueAsString()
		{
			var testDate = new ZDateTimeOffset(2003, 12, 30, 0, 0, 0, TimeSpan.FromHours(8));

			Field.Value = testDate;
			string valueAsString = Field.ValueAsStringForSerialisation;
			AssertEquals("ValueAsString when Value populated", "2003-12-30T00:00:00.0000000+08:00", Field.ValueAsStringForSerialisation);

			Field.ValueAsStringForSerialisation = "";
			AssertEquals("Value when ValueAsString=''", ZDateTimeOffset.Empty, Field.Value);

			Field.ValueAsStringForSerialisation = valueAsString;
			AssertEquals("Value when ValueAsString populated", testDate, Field.Value);
		}

		public void TestValueAsString_UsingDateTimeParseForUDFCompatibility()
		{
			var testDate = new ZDateTimeOffset(2003, 12, 30, 0, 0, 0, TimeSpan.FromHours(8));
			Field.ValueAsStringForSerialisation = testDate.ToString();
			AssertEquals("Value", testDate, Field.Value);
		}

		[TestUtcOffset(8, 0, 0)]
		public void TestEndOfDateValueProvider()
		{
			Field.DisplayName = "ADateField";
			Field.Value = new DateTimeOffset(2003, 1, 25, 0, 0, 0, TimeSpan.FromHours(8));
			foreach (ValueProvider provider in Field.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<ADateField.EndOfDate>", Passes.FirstPass))
				{
					AssertEquals("Should be 2003-01-25 23:59:59", new ZDateTimeOffset(2003, 1, 25, 23, 59, 59, TimeSpan.FromHours(8)), provider.GetReplacement("<ADateField.EndOfDate>", new Report(null, null)));
				}
			}
		}

		public void TestToNextDateValueProvider()
		{
			Field.DisplayName = "ADateField";
			Field.Value = new DateTimeOffset(2003, 1, 25, 0, 0, 0, TimeSpan.FromHours(8));
			foreach (ValueProvider provider in Field.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<ADateField.ToNextDate>", Passes.FirstPass))
				{
					AssertEquals("Should be 2003-01-26 00:00:00", new ZDateTimeOffset(2003, 1, 26, 00, 00, 00, TimeSpan.FromHours(8)), provider.GetReplacement("<ADateField.ToNextDate>", new Report(null, null)));
				}
			}
		}

		public void TestEndOfDateValueProvider_ForEmptyDate()
		{
			Field.DisplayName = "ADateField";
			Field.Value = ZDateTimeOffset.Empty;
			foreach (ValueProvider provider in Field.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<ADateField.EndOfDate>", Passes.FirstPass))
				{
					AssertEquals("An empty date should return an empty string", ZDateTimeOffset.Empty, provider.GetReplacement("<ADateField.EndOfDate>", new Report(null, null)));
				}
			}
		}

		public void TestSqlFormatValueProvider()
		{
			Field.DisplayName = "ADateField";
			Field.Value = new ZDateTimeOffset(2005, 1, 2, 0, 0, 0, TimeSpan.FromHours(8));
			foreach (ValueProvider provider in Field.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<ADateField.SqlFormat>", Passes.FirstPass))
				{
					AssertEquals("2005-01-02 00:00:00.0000000 +08:00", provider.GetReplacement("<ADateField.SqlFormat>", new Report(null, null)));
				}
			}
		}

		public void TestSqlFormatValueProvider_ForEmptyDate()
		{
			Field.DisplayName = "ADateField";
			Field.Value = ZDateTimeOffset.Empty;
			foreach (ValueProvider provider in Field.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<ADateField.SqlFormat>", Passes.FirstPass))
				{
					AssertEquals("An empty date should return an empty string", "", provider.GetReplacement("<ADateField.SqlFormat>", new Report(null, null)));
				}
			}
		}

		public void TestValueForSQLParameter()
		{
			int responsibleCount = 0;
			Field.DisplayName = "ADateField";
			Field.Value = new ZDateTimeOffset(2005, 1, 2, 0, 0, 0, TimeSpan.FromHours(8));
			foreach (ValueProvider provider in Field.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<ADateField.ValueForSQLParameter>", Passes.FirstPass))
				{
					var replacement = provider.GetReplacement("<ADateField.ValueForSQLParameter>", new Report(null, null));
					AssertType<ReplacementWithSqlDbType>(replacement);
					AssertEquals(SqlDbType.DateTimeOffset, ((ReplacementWithSqlDbType)replacement).SqlDbType);
					AssertEquals(new DateTimeOffset(2005, 1, 2, 0, 0, 0, TimeSpan.FromHours(8)), ((ReplacementWithSqlDbType)replacement).MacroValue);

					responsibleCount++;
				}
			}

			AssertEquals("Should have one responsible for <ADateField.ValueForSQLParameter>.", 1, responsibleCount);
		}

		public void TestValueForSQLParameter_ForEmptyDate()
		{
			int responsibleCount = 0;
			Field.DisplayName = "ADateField";
			Field.Value = ZDateTimeOffset.Empty;
			foreach (ValueProvider provider in Field.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<ADateField.ValueForSQLParameter>", Passes.FirstPass))
				{
					var replacement = provider.GetReplacement("<ADateField.ValueForSQLParameter>", new Report(null, null));
					AssertType<ReplacementWithSqlDbType>(replacement);
					AssertEquals(SqlDbType.DateTimeOffset, ((ReplacementWithSqlDbType)replacement).SqlDbType);
					AssertEquals(DBNull.Value, ((ReplacementWithSqlDbType)replacement).MacroValue);

					responsibleCount++;
				}
			}

			AssertEquals("Should have one responsible for <ADateField.ValueForSQLParameter>.", 1, responsibleCount);
		}

		public void TestHasSerialisableValueChangedWhenChangingValue()
		{
			AssertEquals("HasSerialisableValueChanged", false, Field.HasSerialisableValueChanged);
			Field.Value = Field.Value;
			AssertEquals("HasSerialisableValueChanged", false, Field.HasSerialisableValueChanged);
			Field.Value = new ZDateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.FromHours(8));
			AssertEquals("HasSerialisableValueChanged", true, Field.HasSerialisableValueChanged);
		}

		public void TestHasChanges()
		{
			var testDate = new ZDateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.FromHours(8));
			Field.Value = testDate;
			AssertEquals("HasChanges", true, Field.HasChanges);

			Field.HasChanges = false;
			Field.Value = testDate;
			AssertEquals("HasChanges", false, Field.HasChanges);
		}

		[TestUtcOffset(2, 0, 0)]
		public void TestSetScheduleTask()
		{
			AssertNull("Schedule", Field.Schedule);

			ReportScheduleTask scheduleTask1 = Factory.NewWithValidTestData<ReportScheduleTask>();
			Field.SetScheduleTask(scheduleTask1);
			AssertEquals("Schedule.ScheduleTask", scheduleTask1, Field.Schedule.ScheduleTask);
			AssertEquals("Value", ZDateTimeOffset.Empty, Field.Value);

			Field.Schedule.FillWithValidTestData();
			Field.Schedule.DayNumber = 3;
			var previousValue = Field.Value;

			ReportScheduleTask scheduleTask2 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask2.CalcNextRunTimeLocal = new ZDateTime(2001, 4, 20);
			Field.SetScheduleTask(scheduleTask2);
			Assert("Value should have changed.", Field.Value != previousValue);
			AssertEquals("Schedule.ScheduleTask", scheduleTask2, Field.Schedule.ScheduleTask);
			AssertEquals("Value", Field.Schedule.GetScheduleDate().ToOffset(), Field.Value);
			AssertEquals("Value.IsEmpty", false, Field.Value.IsEmpty);
		}

		[TestUtcOffset(-2, 0, 0)]
		public void TestValueIsUpdatedWhenScheduleChanges()
		{
			var value = new ZDateTimeOffset(2006, 3, 2, 0, 0, 0, TimeSpan.FromHours(8));
			Field.Value = value;

			AssertEquals("Value", value, Field.Value);

			ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.CalcNextRunTimeLocal = ZDate.Today;
			Field.SetScheduleTask(scheduleTask);

			Field.Schedule.FillWithValidTestData();
			Field.Schedule.DayNumber = 6;
			AssertEquals("Value", Field.Schedule.GetScheduleDate().ToOffset(), Field.Value);
			AssertEquals("Value.IsEmpty", false, Field.Value.IsEmpty);
		}

		public void TestJsonConverter_EmptyValue()
		{
			TestJsonConverter(null, ZDateTimeOffset.Empty);
		}

		public void TestJsonConverter_EmptyScheduleValue()
		{
			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2006, 10, 1);
			Field.SetScheduleTask(scheduleTask);
			TestJsonConverter(scheduleTask, ZDateTimeOffset.Empty);
		}

		public void TestJsonConverter_ValidValue()
		{
			var value = new ZDateTimeOffset(2006, 1, 5, 0, 0, 0, TimeSpan.FromHours(8));
			Field.Value = value;
			TestJsonConverter(null, value);
		}

		public void TestJsonConverter_ValidScheduleValue()
		{
			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2006, 10, 1);
			Field.SetScheduleTask(scheduleTask);
			Field.Schedule.FillWithValidTestData();
			Field.Schedule.Period = ScheduleRecurrenceType.Monthly;
			Field.Schedule.DayNumber = 3;
			TestJsonConverter(scheduleTask, new ZDateTimeOffset(2006, 10, 3, 0, 0, 0, ZDateTimeOffset.Now.Offset));
		}

		public void TestValueInfoReadOnly()
		{
			AssertEquals("ValueInfo.ReadOnly", false, Field.ValueInfo.ReadOnly);
			Field.SetScheduleTask(Factory.NewWithValidTestData<ReportScheduleTask>());
			AssertEquals("ValueInfo.ReadOnly", true, Field.ValueInfo.ReadOnly);
		}

		public void TestValidateValue()
		{
			Field.DisplayName = "x";
			Field.Value = ZDateTimeOffset.Invalid;
			AssertHasError(Field.ValueInfo, "x is an invalid date!");

			Field.Value = ZDateTimeOffset.Now;
			AssertNoErrors(Field.ValueInfo);

			MockFilterCollectionValidator validator = new MockFilterCollectionValidator();
			Field.Validators.Add(validator);
			validator.SetIsValid(false);
			Field.ValidateValue();
			AssertHasError(Field.ValueInfo, MockFilterCollectionValidator.ErrorMessage);
		}

		public void TestRunPreSaveValidation()
		{
			Field.ValueInfo.AddError("x");
			Field.RunPreSaveValidation();
			AssertNoNotifications(Field);
		}

		public void TestFillFilterData()
		{
			// Arrange
			Field.Value = new ZDateTimeOffset(2019, 3, 15, 0, 0, 0, TimeSpan.FromHours(8));
			var reportData = new SelectedValueReportData();

			// Act
			Field.FillFilterData(reportData.FilterData);

			// Assert
			CombineAssertions("FillFilterData will produce the expected value.", () =>
			{
				AssertEquals(1, reportData.FilterData.DateTimeOffsetFilterCollection.Count);
				AssertEquals(new DateTimeOffset(2019, 3, 15, 0, 0, 0, TimeSpan.FromHours(8)), reportData.FilterData.DateTimeOffsetFilterCollection[0].Value);
				AssertEquals("Short", reportData.FilterData.DateTimeOffsetFilterCollection[0].DateFormat);
			});
		}

		[TestUtcOffset(8, 0, 0)]
		public void TestSetFilterValue()
		{
			var reportFilterData = new ReportFilterData();
			reportFilterData.DateTimeOffsetFilterCollection.Add(new DateTimeOffsetFilter { Value = new DateTimeOffset(2024, 4, 8, 0, 0, 0, TimeSpan.FromHours(8)), DisplayName = "abc" });
			Field.DisplayName = "abc";
			Field.SetFilterValue(reportFilterData);
			AssertEquals("Run value: ", new DateTimeOffset(2024, 4, 8, 0, 0, 0, TimeSpan.FromHours(8)), Field.Value);

			reportFilterData.DateTimeOffsetFilterCollection.Clear();

			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2024, 4, 2);
			Field.SetScheduleTask(scheduleTask);
			reportFilterData.DateTimeOffsetFilterCollection.Add(new DateTimeOffsetFilter { Value = new DateTimeOffset(1910, 1, 1, 0, 1, 0, TimeSpan.FromHours(8)), DisplayName = "abc" });

			Field.SetFilterValue(reportFilterData);

			AssertEquals("Schedule Storage value:", new DateTime(1910, 1, 1, 0, 1, 0), Field.Schedule.ToStorageValue());
			AssertEquals("Schedule value:", new ZDateTimeOffset(2024, 3, 31, 8, 0, 0, TimeSpan.FromHours(8)), Field.Value);
		}

		public override FilterFieldWithUTSupport[] GetFieldsWithValidClearValueTestCases()
		{
			var field = (DateTimeOffsetField)GetNewBusinessObject();
			field.Value = new ZDateTimeOffset(2006, 01, 01, 0, 0, 0, TimeSpan.FromHours(8));
			return new FilterFieldWithUTSupport[] { field };
		}

		public override int ExpectedNumberOfClearValueTestCases
		{
			get { return 1; }
		}

		internal DateTimeOffsetField Field
		{
			get
			{
				if (field == null)
				{
					field = (DateTimeOffsetField)GetNewBusinessObject();
				}
				return field;
			}
		}

		void TestJsonConverter(ReportScheduleTask scheduleTask, ZDateTimeOffset expectedValue)
		{
			var scheduled = (scheduleTask != null);

			Field.DisplayName = "Goober";
			Field.FieldName = "Grape";

			var result = JsonConverterHelper.Serialize(Field);
			var deserialisedField = JsonConverterHelper.Deserialize<DateTimeOffsetField>(result);

			if (scheduled)
			{
				deserialisedField.SetScheduleTask(scheduleTask);
			}

			AssertEquals("deserialisedField.DisplayName", "Goober", deserialisedField.DisplayName);
			AssertEquals("deserialisedField.FieldName", "Grape", deserialisedField.FieldName);
			AssertEquals("deserialisedField.Value", expectedValue, deserialisedField.Value);

			if (scheduled)
			{
				AssertNotNull("Schedule", deserialisedField.Schedule);
				deserialisedField.Schedule.FillWithValidTestData();
				deserialisedField.Schedule.DayNumber = 4;
				Assert("Schedule.ValueChanged should be hooked.", deserialisedField.Value != expectedValue);
			}
			else
			{
				AssertNull("Schedule", deserialisedField.Schedule);
			}
		}

		public void TestRunAndScheduleJsonConverter()
		{
			var runValue = new ZDateTimeOffset(1979, 2, 12, 0, 0, 0, TimeSpan.FromHours(8));

			var sourceField = new DateTimeOffsetField(Factory);
			sourceField.Value = runValue;

			var result = JsonConverterHelper.Serialize(sourceField);
			var deserialisedField = JsonConverterHelper.Deserialize<DateTimeOffsetField>(result);

			AssertEquals("Run Value", runValue, deserialisedField.Value);

			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDate.Today;
			deserialisedField.SetScheduleTask(scheduleTask);

			deserialisedField.Schedule.Period = ScheduleRecurrenceType.Daily;
			deserialisedField.Schedule.PeriodScope = PeriodScopeList.Codes.Previous;
			deserialisedField.Schedule.PeriodCount = 3;

			result = JsonConverterHelper.Serialize(deserialisedField);
			var deserialisedField2 = JsonConverterHelper.Deserialize<DateTimeOffsetField>(result);

			AssertEquals("Run Value", runValue, deserialisedField2.Value);

			deserialisedField2.SetScheduleTask(scheduleTask);

			AssertEquals("Schedule.Period", deserialisedField.Schedule.Period, deserialisedField2.Schedule.Period);
			AssertEquals("Schedule.PeriodScope", deserialisedField.Schedule.PeriodScope, deserialisedField2.Schedule.PeriodScope);
			AssertEquals("Schedule.PeriodCount", deserialisedField.Schedule.PeriodCount, deserialisedField2.Schedule.PeriodCount);
			AssertEquals("Value", deserialisedField.Value, deserialisedField2.Value);
		}

		public override void TestSafeCopyValuesFrom()
		{
			var runValue = new ZDateTimeOffset(2024, 1, 29, 0, 0, 0, TimeSpan.FromHours(8));

			var sourceField = new DateTimeOffsetField(Factory);
			sourceField.Value = runValue;

			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDate.Today;
			sourceField.SetScheduleTask(scheduleTask);

			sourceField.Schedule.Period = ScheduleRecurrenceType.Daily;
			sourceField.Schedule.PeriodScope = PeriodScopeList.Codes.Previous;
			sourceField.Schedule.PeriodCount = 3;

			var destinationField = new DateTimeOffsetField(Factory);
			((IFilter)destinationField).SafeCopyValuesFrom(sourceField);
			AssertEquals("Run Value", runValue, destinationField.Value);

			destinationField.SetScheduleTask(scheduleTask);
			AssertEquals("Schedule.Period", sourceField.Schedule.Period, destinationField.Schedule.Period);
			AssertEquals("Schedule.PeriodScope", sourceField.Schedule.PeriodScope, destinationField.Schedule.PeriodScope);
			AssertEquals("Schedule.PeriodCount", sourceField.Schedule.PeriodCount, destinationField.Schedule.PeriodCount);
			AssertEquals("Value", ZDateTimeOffset.Today.AddDays(-3), destinationField.Value);
		}

		public override void TestClearValues()
		{
			var field = new DateTimeOffsetField(Factory);
			field.Value = new ZDateTimeOffset(2024, 1, 29, 0, 0, 0, TimeSpan.FromHours(8));
			((IFilter)field).ClearValues();
			AssertEquals(ZDateTimeOffset.Empty, field.Value);
		}

		public void TestGetObjectDataWithInvalidDateTime()
		{
			var dateField = new DateTimeOffsetField(Factory);
			dateField.Value = ZDateTimeOffset.Invalid;

			var serializable = dateField as IJsonSerializable;
			var jsonData = (DateTimeOffsetFieldJsonData)serializable.GetJsonData();
			var store = jsonData.Value;

			AssertNull(store.RunValue);

			dateField.Value = ZDateTimeOffset.Today;
			jsonData = (DateTimeOffsetFieldJsonData)serializable.GetJsonData();
			store = jsonData.Value;

			AssertEquals(ZDateTimeOffset.Today, store.RunValue);
		}

		DateTimeOffsetField field;
	}
}
