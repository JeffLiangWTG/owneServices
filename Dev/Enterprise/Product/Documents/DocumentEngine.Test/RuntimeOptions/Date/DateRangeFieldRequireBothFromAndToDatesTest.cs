using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class DateRangeFieldRequireBothFromAndToDatesTest : TestCaseWithFactory
	{
		public void TestRequireBothFromAndToDates()
		{
			var field = new DateRangeField(Factory)
			{
				RequireBothFromAndToDates = true
			};

			field.RunPreSaveValidation();
			AssertEquals(false, field.HasErrors);

			field.ValueLow = ZDateTime.Now;
			field.RunPreSaveValidation();
			AssertEquals(true, field.HasErrors);
			AssertMultilineASCIIEquals("Checking error message.",
@"Error - ValueLow: Both from and to date must be entered.
Error - ValueHigh: Both from and to date must be entered.",
				field.GetErrors().ToMessageListString());

			field.ValueHigh = ZDateTime.Now;
			field.RunPreSaveValidation();
			AssertEquals(false, field.HasErrors);

			field.ValueLow = ZDateTime.Empty;
			field.RunPreSaveValidation();
			AssertEquals(true, field.HasErrors);
			AssertMultilineASCIIEquals("Checking error message.",
@"Error - ValueLow: Both from and to date must be entered.
Error - ValueHigh: Both from and to date must be entered.",
				field.GetErrors().ToMessageListString());
		}

		public void TestDateRangeMaxYears()
		{
			var field = new DateRangeField(Factory)
			{
				RequireBothFromAndToDates = true,
				DateRangeMaxYears = 1
			};

			field.RunPreSaveValidation();
			AssertEquals(false, field.HasErrors);

			field.ValueLow = new ZDateTime(2020, 1, 1);
			field.ValueHigh = new ZDateTime(2021, 1, 2);
			field.RunPreSaveValidation();
			AssertEquals(true, field.HasErrors);
			AssertMultilineASCIIEquals("Checking error message.",
@"Error - ValueLow: Date Range must be no more than 1 year(s).
Error - ValueHigh: Date Range must be no more than 1 year(s).",
				field.GetErrors().ToMessageListString());

			field.ValueLow = new ZDateTime(2020, 1, 1);
			field.ValueHigh = new ZDateTime(2021, 1, 1);
			field.RunPreSaveValidation();
			AssertEquals("no more than one year", false, field.HasErrors);
		}

		public void TestDateRangeMaxYears_NoExceptionForLargeValue()
		{
			var field = new DateRangeField(Factory)
			{
				RequireBothFromAndToDates = true,
				DateRangeMaxYears = 100000
			};

			AssertNoExceptionThrown(() =>
			{
				field.ValueLow = new ZDateTime(2020, 1, 1);
				field.ValueHigh = ZDateTime.MaxSmallDateTime;
				field.RunPreSaveValidation();

				AssertEquals("no more than a large number years", false, field.HasErrors);
			});
		}

		public void TestDateRangeMaxMonths()
		{
			var field = new DateRangeField(Factory)
			{
				RequireBothFromAndToDates = true,
				DateRangeMaxMonths = 12
			};

			field.RunPreSaveValidation();
			AssertEquals(false, field.HasErrors);

			field.ValueLow = new ZDateTime(2021, 1, 1);
			field.ValueHigh = new ZDateTime(2022, 1, 2);
			field.RunPreSaveValidation();
			AssertEquals(true, field.HasErrors);
			AssertMultilineASCIIEquals("Checking error message.",
@"Error - ValueLow: Date Range must be no more than 12 month(s).
Error - ValueHigh: Date Range must be no more than 12 month(s).",
				field.GetErrors().ToMessageListString());

			field.ValueLow = new ZDateTime(2021, 1, 1);
			field.ValueHigh = new ZDateTime(2022, 1, 1);
			field.RunPreSaveValidation();
			AssertEquals("no more than twelve months", false, field.HasErrors);
		}

		public void TestDateRangeMaxMonths_NoExceptionForLargeValue()
		{
			var field = new DateRangeField(Factory)
			{
				RequireBothFromAndToDates = true,
				DateRangeMaxMonths = 100000
			};

			AssertNoExceptionThrown(() =>
			{
				field.ValueLow = new ZDateTime(2020, 1, 1);
				field.ValueHigh = ZDateTime.MaxSmallDateTime;
				field.RunPreSaveValidation();

				AssertEquals("no more than a large number months", false, field.HasErrors);
			});
		}

		public void TestRequireBothFromAndToDatesSerialization()
		{
			var field = new DateRangeField(Factory)
			{
				RequireBothFromAndToDates = true
			};

			var result = JsonConverterHelper.Serialize(field);
			var deserializedField = JsonConverterHelper.Deserialize<DateRangeField>(result);

			AssertEquals(true, deserializedField.RequireBothFromAndToDates);
		}
	}
}
