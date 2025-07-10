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
	[TestedType(typeof(DateField))]
	class DateFieldTest : FitlerFieldTestWithClearValues
	{
		public void TestJsonConverter()
		{
			var testTime = new DateTime(2023, 5, 8);
			Field.DisplayName = "Json Test";
			Field.FieldName = "TestField";
			Field.Value = testTime;

			var result = JsonConverterHelper.Serialize(Field);
			var deserialisedField = JsonConverterHelper.Deserialize(result, Field.GetType()) as DateField;

			AssertEquals("Json Test", deserialisedField.DisplayName);
			AssertEquals("TestField", deserialisedField.FieldName);
			AssertEquals(testTime, deserialisedField.Value);
		}

		public void TestNoExceptionThrownWhenCallingGetToNextDate()
		{
			Field.DisplayName = "ADateField";
			Field.Value = ZDateTime.Empty;
			var macro = "<ADateField.ToNextDate>";
			var toNextDateValueProvider = Field.ValueProviders.First(v => v.IsResponsibleForReplacing(macro, Passes.FirstPass));
			AssertEquals(ZDateTime.Empty, toNextDateValueProvider.GetReplacement(macro, new Report(null, null)));

			Field.Value = ZDateTime.Today;
			AssertEquals(ZDateTime.Today.AddDays(1), toNextDateValueProvider.GetReplacement(macro, new Report(null, null)));
		}

		public virtual void TestWhereClauseWithInput()
		{
			Field.FieldName = "abc";
			Field.Value = new DateTime(2003, 1, 25);
			Match whereClauseMatch = Regex.Match(Field.WhereClause(), @"abc >= (@p[0-9]+) AND abc < (@p[0-9]+)");
			Assert("WhereClause should be of the form 'abc >= @p123 AND abc < @p456' but was: " + Field.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Should have 2 params", 2, Field.SqlParameters().Count);
			AssertEquals("Param 1's name should match the name in the where clause", whereClauseMatch.Groups[1].Value, Field.SqlParameters()[0].ToString());
			AssertEquals("Param 1's value should be set", new DateTime(2003, 1, 25), Field.SqlParameters()[0].Value);
			AssertEquals("Param 2's name should match the name in the where clause", whereClauseMatch.Groups[2].Value, Field.SqlParameters()[1].ToString());
			AssertEquals("Param 2's value should be set", new DateTime(2003, 1, 26), Field.SqlParameters()[1].Value);
		}

		public virtual void TestWhereClauseWithYearAndMonthOnly()
		{
			Field.PickerFormat = DocEngineDatePickerFormats.YearAndMonth;
			Field.FieldName = "abc";
			Field.Value = new DateTime(2003, 1, 25);
			Match whereClauseMatch = Regex.Match(Field.WhereClause(), @"abc >= (@p[0-9]+) AND abc < (@p[0-9]+)");
			Assert("WhereClause should be of the form 'abc >= @p123 AND abc < @p456' but was: " + Field.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Should have 2 params", 2, Field.SqlParameters().Count);
			AssertEquals("Param 1's name should match the name in the where clause", whereClauseMatch.Groups[1].Value, Field.SqlParameters()[0].ToString());
			AssertEquals("Param 1's value should be set", new DateTime(2003, 1, 1), Field.SqlParameters()[0].Value);
			AssertEquals("Param 2's name should match the name in the where clause", whereClauseMatch.Groups[2].Value, Field.SqlParameters()[1].ToString());
			AssertEquals("Param 2's value should be set", new DateTime(2003, 2, 1), Field.SqlParameters()[1].Value);
		}

		public void TestOnlyUsesDatePartInShortFormat()
		{
			Field.Value = new DateTime(2003, 1, 25, 11, 11, 11, 11);
			AssertEquals("Should only use date part", new ZDateTime(new DateTime(2003, 1, 25, 0, 0, 0, 0)).Date, Field.Value);
		}

		public void TestAlsoUsesTimePartInLongFormat()
		{
			Field.PickerFormat = DocEngineDatePickerFormats.Long;
			Field.Value = new DateTime(2003, 1, 25, 11, 11, 11, 11);
			AssertEquals("Should also use time part", new ZDateTime(new DateTime(2003, 1, 25, 11, 11, 11, 11)), Field.Value);
		}

		public void TestValueAsString()
		{
			ZDateTime testDate = new ZDateTime(2003, 12, 30);

			Field.Value = testDate;
			string valueAsString = Field.ValueAsStringForSerialisation;
			AssertEquals("ValueAsString when Value populated", "2003-12-30T00:00:00", Field.ValueAsStringForSerialisation);

			Field.ValueAsStringForSerialisation = "";
			AssertEquals("Value when ValueAsString=''", ZDateTime.Empty, Field.Value);

			Field.ValueAsStringForSerialisation = valueAsString;
			AssertEquals("Value when ValueAsString populated", testDate, Field.Value);
		}

		public void TestValueAsString_UsingDateTimeParseForUDFCompatibility()
		{
			ZDateTime testDate = new ZDateTime(2003, 12, 30);
			Field.ValueAsStringForSerialisation = testDate.ToString();
			AssertEquals("Value", testDate, Field.Value);
		}

		public void TestEndOfDateValueProvider()
		{
			Field.DisplayName = "ADateField";
			Field.Value = new DateTime(2003, 1, 25);
			foreach (ValueProvider provider in Field.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<ADateField.EndOfDate>", Passes.FirstPass))
				{
					AssertEquals("Should be 2003-01-25 23:59:59", new ZDateTime(2003, 1, 25, 23, 59, 59), provider.GetReplacement("<ADateField.EndOfDate>", new Report(null, null)));
				}
			}
		}

		public void TestToNextDateValueProvider()
		{
			Field.DisplayName = "ADateField";
			Field.Value = new DateTime(2003, 1, 25);
			foreach (ValueProvider provider in Field.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<ADateField.ToNextDate>", Passes.FirstPass))
				{
					AssertEquals("Should be 2003-01-26 00:00:00", new ZDateTime(2003, 1, 26, 00, 00, 00), provider.GetReplacement("<ADateField.ToNextDate>", new Report(null, null)));
				}
			}
		}

		public void TestEndOfDateValueProvider_ForEmptyDate()
		{
			Field.DisplayName = "ADateField";
			Field.Value = ZDateTime.Empty;
			foreach (ValueProvider provider in Field.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<ADateField.EndOfDate>", Passes.FirstPass))
				{
					AssertEquals("An empty date should return an empty string", ZDateTime.Empty, provider.GetReplacement("<ADateField.EndOfDate>", new Report(null, null)));
				}
			}
		}

		public void TestSqlFormatValueProvider()
		{
			Field.DisplayName = "ADateField";
			Field.Value = new ZDateTime(2005, 1, 2);
			foreach (ValueProvider provider in Field.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<ADateField.SqlFormat>", Passes.FirstPass))
				{
					AssertEquals("2005-01-02 00:00:00.000", provider.GetReplacement("<ADateField.SqlFormat>", new Report(null, null)));
				}
			}
		}

		public void TestSqlFormatValueProvider_ForEmptyDate()
		{
			Field.DisplayName = "ADateField";
			Field.Value = ZDateTime.Empty;
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
			Field.Value = new ZDateTime(2005, 1, 2);
			foreach (ValueProvider provider in Field.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<ADateField.ValueForSQLParameter>", Passes.FirstPass))
				{
					var replacement = provider.GetReplacement("<ADateField.ValueForSQLParameter>", new Report(null, null));
					AssertType<ReplacementWithSqlDbType>(replacement);
					AssertEquals(SqlDbType.DateTime, ((ReplacementWithSqlDbType)replacement).SqlDbType);
					AssertEquals(new DateTime(2005, 1, 2), ((ReplacementWithSqlDbType)replacement).MacroValue);

					responsibleCount++;
				}
			}

			AssertEquals("Should have one responsible for <ADateField.ValueForSQLParameter>.", 1, responsibleCount);
		}

		public void TestValueForSQLParameter_ForEmptyDate()
		{
			int responsibleCount = 0;
			Field.DisplayName = "ADateField";
			Field.Value = ZDateTime.Empty;
			foreach (ValueProvider provider in Field.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<ADateField.ValueForSQLParameter>", Passes.FirstPass))
				{
					var replacement = provider.GetReplacement("<ADateField.ValueForSQLParameter>", new Report(null, null));
					AssertType<ReplacementWithSqlDbType>(replacement);
					AssertEquals(SqlDbType.DateTime, ((ReplacementWithSqlDbType)replacement).SqlDbType);
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
			Field.Value = new ZDateTime(2000, 1, 1);
			AssertEquals("HasSerialisableValueChanged", true, Field.HasSerialisableValueChanged);
		}

		public void TestHasChanges()
		{
			ZDateTime testDate = new ZDateTime(2000, 1, 1);
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
			AssertEquals("Value", ZDateTime.Empty, Field.Value);

			Field.Schedule.FillWithValidTestData();
			Field.Schedule.DayNumber = 3;
			ZDateTime previousValue = Field.Value;

			ReportScheduleTask scheduleTask2 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask2.CalcNextRunTimeLocal = new ZDateTime(2001, 4, 20);
			Field.SetScheduleTask(scheduleTask2);
			Assert("Value should have changed.", Field.Value != previousValue);
			AssertEquals("Schedule.ScheduleTask", scheduleTask2, Field.Schedule.ScheduleTask);
			AssertEquals("Value", Field.Schedule.GetScheduleDate(), Field.Value);
			AssertEquals("Value.IsEmpty", false, Field.Value.IsEmpty);
		}

		[TestUtcOffset(-2, 0, 0)]
		public void TestValueIsUpdatedWhenScheduleChanges()
		{
			ZDateTime value = new ZDateTime(2006, 3, 2);
			Field.Value = value;

			AssertEquals("Value", value, Field.Value);

			ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.CalcNextRunTimeLocal = ZDate.Today;
			Field.SetScheduleTask(scheduleTask);

			Field.Schedule.FillWithValidTestData();
			Field.Schedule.DayNumber = 6;
			AssertEquals("Value", Field.Schedule.GetScheduleDate(), Field.Value);
			AssertEquals("Value.IsEmpty", false, Field.Value.IsEmpty);
		}

		public void TestJsonConverter_EmptyValue()
		{
			TestJsonConverter(null, ZDateTime.Empty);
		}

		public void TestJsonConverter_EmptyScheduleValue()
		{
			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2006, 10, 1);
			Field.SetScheduleTask(scheduleTask);
			TestJsonConverter(scheduleTask, ZDateTime.Empty);
		}

		public void TestJsonConverter_ValidValue()
		{
			var value = new ZDateTime(2006, 1, 5);
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
			TestJsonConverter(scheduleTask, new ZDateTime(2006, 10, 3));
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
			Field.Value = ZDateTime.Invalid;
			AssertHasError(Field.ValueInfo, "x is an invalid date!");

			Field.Value = ZDateTime.Now;
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
			Field.Value = new ZDateTime(2019, 3, 15);
			var reportData = new SelectedValueReportData();

			Field.FillFilterData(reportData.FilterData);

			CombineAssertions("FillFilterData will produce the expected value.", () =>
			{
				AssertEquals(1, reportData.FilterData.DateFilterCollection.Count);
				AssertEquals(new DateTime(2019, 3, 15), reportData.FilterData.DateFilterCollection[0].Value);
				AssertEquals("Short", reportData.FilterData.DateFilterCollection[0].DateFormat);
			});

			reportData.FilterData.DateFilterCollection.Clear();

			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2024, 4, 2);
			Field.SetScheduleTask(scheduleTask);
			var reportFilterData = new ReportFilterData();
			reportFilterData.DateFilterCollection.Add(new DateFilter { Value = new DateTime(1910, 1, 1, 0, 1, 0), DisplayName = Field.DisplayName });

			Field.SetFilterValue(reportFilterData);

			Field.FillFilterData(reportData.FilterData);

			CombineAssertions("FillFilterData will produce the expected value for schedule.", () =>
			{
				AssertEquals(1, reportData.FilterData.DateFilterCollection.Count);
				AssertEquals(new DateTime(1910, 1, 1, 0, 1, 0), reportData.FilterData.DateFilterCollection[0].Value);
				AssertEquals("Short", reportData.FilterData.DateFilterCollection[0].DateFormat);
			});
		}

		public void TestSetFilterValue()
		{
			var reportFilterData = new ReportFilterData();
			reportFilterData.DateFilterCollection.Add(new DateFilter { Value = new DateTime(2024, 4, 8), DisplayName = "abc" });
			Field.DisplayName = "abc";
			Field.SetFilterValue(reportFilterData);
			AssertEquals("Run value: ", new DateTime(2024, 4, 8), Field.Value);

			reportFilterData.DateFilterCollection.Clear();

			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2024, 4, 2);
			Field.SetScheduleTask(scheduleTask);
			reportFilterData.DateFilterCollection.Add(new DateFilter { Value = new DateTime(1910, 1, 1, 0, 1, 0), DisplayName = "abc" });

			Field.SetFilterValue(reportFilterData);

			AssertEquals("Schedule Storage value:", new DateTime(1910, 1, 1, 0, 1, 0), Field.Schedule.ToStorageValue());
			AssertEquals("Schedule value:", new DateTime(2024, 3, 31), Field.Value);
		}

		public override FilterFieldWithUTSupport[] GetFieldsWithValidClearValueTestCases()
		{
			DateField field = (DateField)GetNewBusinessObject();
			field.Value = new ZDateTime(2006, 01, 01);
			return new FilterFieldWithUTSupport[] { field };
		}

		public override int ExpectedNumberOfClearValueTestCases
		{
			get { return 1; }
		}

		internal DateField Field
		{
			get
			{
				if (field == null)
				{
					field = (DateField)GetNewBusinessObject();
				}
				return field;
			}
		}

		void TestJsonConverter(ReportScheduleTask scheduleTask, ZDateTime expectedValue)
		{
			var scheduled = (scheduleTask != null);

			Field.DisplayName = "Goober";
			Field.FieldName = "Grape";

			var result = JsonConverterHelper.Serialize(Field);
			var deserialisedField = JsonConverterHelper.Deserialize<DateField>(result);

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

		public void TestSerialization_PickerFormat()
		{
			var cases = new DocEngineDatePickerFormats[]
			{
				DocEngineDatePickerFormats.Short,
				DocEngineDatePickerFormats.Long,
				DocEngineDatePickerFormats.YearAndMonth,
			};

			var sourceField = new DateField(Factory);
			sourceField.Value = ZDateTime.BrettsBirthday;

			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDate.Today;
			sourceField.SetScheduleTask(scheduleTask);

			sourceField.Schedule.FillWithValidTestData();
			sourceField.Schedule.PeriodScope = PeriodScopeList.Codes.Previous;
			sourceField.Schedule.PeriodCount = 1;
			sourceField.Schedule.Hour = 2;

			CombineAssertions(() =>
			{
				foreach (var item in cases)
				{
					sourceField.PickerFormat = item;
					var serializedDateField = JsonConverterHelper.Serialize(sourceField);
					AssertDeserializedPickerFormat(serializedDateField, item);
				}
			});
		}

		void AssertDeserializedPickerFormat(string serializedData, DocEngineDatePickerFormats expectedValue)
		{
			var deserialisedField = JsonConverterHelper.Deserialize<DateField>(serializedData);
			AssertEquals("PickerFormat", expectedValue, deserialisedField.PickerFormat);
		}

		public void TestRunAndScheduleJsonConverter()
		{
			var runValue = new ZDateTime(1979, 2, 12);

			var sourceField = new DateField(Factory);
			sourceField.Value = runValue;

			var result = JsonConverterHelper.Serialize(sourceField);
			var deserialisedField = JsonConverterHelper.Deserialize<DateField>(result);

			AssertEquals("Run Value", runValue, deserialisedField.Value);

			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDate.Today;
			deserialisedField.SetScheduleTask(scheduleTask);

			deserialisedField.Schedule.Period = ScheduleRecurrenceType.Daily;
			deserialisedField.Schedule.PeriodScope = PeriodScopeList.Codes.Previous;
			deserialisedField.Schedule.PeriodCount = 3;

			result = JsonConverterHelper.Serialize(deserialisedField);
			var deserialisedField2 = JsonConverterHelper.Deserialize<DateField>(result);

			AssertEquals("Run Value", runValue, deserialisedField2.Value);

			deserialisedField2.SetScheduleTask(scheduleTask);

			AssertEquals("Schedule.Period", deserialisedField.Schedule.Period, deserialisedField2.Schedule.Period);
			AssertEquals("Schedule.PeriodScope", deserialisedField.Schedule.PeriodScope, deserialisedField2.Schedule.PeriodScope);
			AssertEquals("Schedule.PeriodCount", deserialisedField.Schedule.PeriodCount, deserialisedField2.Schedule.PeriodCount);
			AssertEquals("Value", deserialisedField.Value, deserialisedField2.Value);
		}

		public override void TestSafeCopyValuesFrom()
		{
			var runValue = ZDateTime.BrettsBirthday;

			var sourceField = new DateField(Factory);
			sourceField.Value = runValue;

			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDate.Today;
			sourceField.SetScheduleTask(scheduleTask);

			sourceField.Schedule.Period = ScheduleRecurrenceType.Daily;
			sourceField.Schedule.PeriodScope = PeriodScopeList.Codes.Previous;
			sourceField.Schedule.PeriodCount = 3;

			var destinationField = new DateField(Factory);
			((IFilter)destinationField).SafeCopyValuesFrom(sourceField);
			AssertEquals("Run Value", runValue, destinationField.Value);

			destinationField.SetScheduleTask(scheduleTask);
			AssertEquals("Schedule.Period", sourceField.Schedule.Period, destinationField.Schedule.Period);
			AssertEquals("Schedule.PeriodScope", sourceField.Schedule.PeriodScope, destinationField.Schedule.PeriodScope);
			AssertEquals("Schedule.PeriodCount", sourceField.Schedule.PeriodCount, destinationField.Schedule.PeriodCount);
			AssertEquals("Value", ZDate.Today.AddDays(-3), destinationField.Value);
		}

		public override void TestClearValues()
		{
			DateField field = new DateField(Factory);
			field.Value = ZDateTime.BrettsBirthday;
			((IFilter)field).ClearValues();
			AssertEquals(ZDateTime.Empty, field.Value);
		}

		public void TestGetObjectDataWithInvalidDateTime()
		{
			var dateField = new DateField(Factory);
			dateField.Value = ZDateTime.Invalid;

			var serializable = dateField as IJsonSerializable;
			var jsonData = (DateFieldJsonData)serializable.GetJsonData();
			var store = jsonData.Value;

			AssertNull(store.RunValue);

			dateField.Value = ZDateTime.Today;
			jsonData = (DateFieldJsonData)serializable.GetJsonData();
			store = jsonData.Value;

			AssertEquals(ZDateTime.Today, store.RunValue);
		}

		DateField field;
	}
}
