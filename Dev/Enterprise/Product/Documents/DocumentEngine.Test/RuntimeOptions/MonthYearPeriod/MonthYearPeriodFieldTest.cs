using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(MonthYearPeriodField))]
	internal class MonthYearPeriodFieldTest : FitlerFieldTestWithClearValues
	{
		public override int ExpectedNumberOfClearValueTestCases => 1;

		public override FilterFieldWithUTSupport[] GetFieldsWithValidClearValueTestCases()
		{
			var field = new MonthYearPeriodField(new BusinessObjectFactory());
			field.Month = 12;
			field.Year = 2024;
			var results = new ArrayList { field };
			return (FilterFieldWithUTSupport[])results.ToArray(typeof(FilterFieldWithUTSupport));
		}

		public override void TestClearValues()
		{
			var field = new MonthYearPeriodField(Factory);
			field.Month = 12;
			field.Year = 2024;
			field.ClearValues();
			AssertEquals(ZInt.Zero, field.Month);
			AssertEquals(ZInt.Zero, field.Year);
		}

		public override void TestSafeCopyValuesFrom()
		{
			var source = new MonthYearPeriodField(Factory);
			source.Month = 12;
			source.Year = 2024;
			var destination = new MonthYearPeriodField(Factory);
			destination.SafeCopyValuesFrom(source);
			AssertEquals(source.Month, destination.Month);
			AssertEquals(source.Year, destination.Year);
		}

		public void TestValueAsString()
		{
			var field = new MonthYearPeriodField(Factory);
			field.Month = 1;
			field.Year = 2025;
			AssertEquals("01/2025", field.ValueAsStringForSerialisation);
			AssertEquals("HasChanges should be true", true, field.HasChanges);

			field.HasChanges = false;
			field.ValueAsStringForSerialisation = "12/2024";
			AssertEquals(12, field.Month);
			AssertEquals(2024, field.Year);
			AssertEquals("HasChanges should be true", true, field.HasChanges);
			AssertEquals("HasSerialisableValueChanged should be true", true, field.HasSerialisableValueChanged);
		}

		public void TestIsEmpty()
		{
			Assert("Should be empty by default", new NumberField(new BusinessObjectFactory()).IsEmpty);
		}

		public void TestJsonConverter()
		{
			var field = new MonthYearPeriodField(Factory);
			field.DisplayName = "Json Test";
			field.Month = 12;
			field.Year = 2024;
			var result = JsonConverterHelper.Serialize(field);
			var deserialisedField = JsonConverterHelper.Deserialize<MonthYearPeriodField>(result);
			AssertEquals("Json Test", deserialisedField.DisplayName);
			AssertEquals(12, deserialisedField.Month);
			AssertEquals(2024, deserialisedField.Year);
			AssertEquals(true, deserialisedField.HasSerialisableValueChanged);
		}

		public void TestValidateMonthAndYear()
		{
			var field = new MonthYearPeriodField(Factory);

			field.Month = 99;
			field.Year = 9999;
			AssertHasErrorContaining(field.MonthInfo, "Invalid value for month.");
			AssertHasErrorContaining(field.YearInfo, "Invalid value for year.");

			field.Month = 12;
			field.Year = 2024;
			AssertNoErrors(field.MonthInfo);
			AssertNoErrors(field.YearInfo);

			field.Month = 0;
			field.Year = 2024;
			AssertHasErrorContaining(field.MonthInfo, "Please enter month.");
			AssertNoErrors(field.YearInfo);

			field.Month = 12;
			field.Year = 0;
			AssertNoErrors(field.MonthInfo);
			AssertHasErrorContaining(field.YearInfo, "Please enter year.");

			field.Month = 0;
			field.Year = 0;
			AssertNoErrors(field.MonthInfo);
			AssertNoErrors(field.YearInfo);
		}

		public void TestSpecialisedValueProviders()
		{
			var field = new MonthYearPeriodField(Factory);
			field.DisplayName = "MonthYearPeriod";
			field.Month = 12;
			field.Year = 2024;
			AssertProvider(field, "<MonthYearPeriod.Month>", 12);
			AssertProvider(field, "<MonthYearPeriod.Year>", 2024);
		}

		void AssertProvider(MonthYearPeriodField field, string macro, int expectedValue)
		{
			int responsibleCount = 0;
			foreach (var provider in field.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing(macro, Passes.FirstPass))
				{
					AssertEquals($"{macro} should return {expectedValue}", expectedValue, provider.GetReplacement(macro, new Report(null, null)));
					responsibleCount++;
				}
			}
			AssertEquals($"Should have one responsible for {macro}", 1, responsibleCount);
		}
	}
}
