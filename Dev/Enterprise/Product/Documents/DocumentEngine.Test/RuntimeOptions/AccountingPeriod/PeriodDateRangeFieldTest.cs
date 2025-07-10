using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(PeriodDateRangeField))]
	sealed class PeriodDateRangeFieldTest : DateRangeFieldTest
	{
		public void TestJsonConverter()
		{
			var field = new PeriodDateRangeField(Factory);
			field.DisplayName = "Json Test";

			var result = JsonConverterHelper.Serialize(field);
			var deserialisedField = JsonConverterHelper.Deserialize<PeriodDateRangeField>(result);

			AssertEquals("Json Test", deserialisedField.DisplayName);
		}

		public void TestSubstituteIsFalse()
		{
			AssertEquals("SubstituteMaxDateForNullTo", false, Field.SubstituteMaxDateForNullTo);
			AssertEquals("SubstituteMinDateForNullFrom", false, Field.SubstituteMinDateForNullFrom);
		}

		public override void TestValidateValueLow()
		{
			AccPeriodTestDataCreator.Create(Factory);
			base.TestValidateValueLow();

			Field.ValueHigh = new ZDateTime(2008, 2, 2);
			Field.ValueLow = new ZDateTime(2008, 2, 1);
			AssertHasError(Field.ValueLowInfo, PeriodDateRangeFieldForTesting.InvalidPeriodDateErrorMessage);

			Field.ValueLow = ZDateTime.Empty;
			AssertNoError(Field.ValueLowInfo, PeriodDateRangeFieldForTesting.InvalidPeriodDateErrorMessage);

			Field.ValueHigh = new ZDateTime(2007, 1, 1);
			Field.ValueLow = new ZDateTime(2006, 2, 1);
			AssertHasError(Field.ValueLowInfo, PeriodDateRangeFieldForTesting.PeriodDateRangeDifferentYearErrorMessage);

			Field.SetScheduleTask(Factory.NewWithValidTestData<ReportScheduleTask>());
			Field.ValidateValueLow();
			AssertNoErrors(Field.ValueLowInfo);
		}

		public override void TestValidateValueHigh()
		{
			AccPeriodTestDataCreator.Create(Factory);
			base.TestValidateValueHigh();

			Field.ValueHigh = new ZDateTime(2008, 2, 1);
			AssertHasError(Field.ValueHighInfo, PeriodDateRangeFieldForTesting.InvalidPeriodDateErrorMessage);

			Field.ValueHigh = ZDateTime.Empty;
			AssertNoError(Field.ValueHighInfo, PeriodDateRangeFieldForTesting.InvalidPeriodDateErrorMessage);

			Field.ValueLow = new ZDateTime(2006, 2, 1);
			Field.ValueHigh = new ZDateTime(2007, 1, 1);
			AssertHasError(Field.ValueHighInfo, PeriodDateRangeFieldForTesting.PeriodDateRangeDifferentYearErrorMessage);

			Field.SetScheduleTask(Factory.NewWithValidTestData<ReportScheduleTask>());
			Field.ValidateValueHigh();
			AssertNoErrors(Field.ValueHighInfo);
		}

		public void TestPeriodValidation_NonCurrentCompany()
		{
			var testObjectCreator = new AccountingTestObjectCreator(Factory);
			var nonCurrentCompany = testObjectCreator.NonCurrentCompany;
			var chart = testObjectCreator.CreateAlternateChart("CHL");
			var reportingBook = testObjectCreator.CreateAccReportingBook("RTE", chart.PK, nonCurrentCompany.PK);
			Factory.Save();

			var dateField = (PeriodDateRangeField)Field;
			dateField.ReadOnlyIfFilter = "Reporting Book";
			dateField.DependencyValue = reportingBook.PK.ToString();
			dateField.ValueHigh = new ZDateTime(2022, 11, 02);
			dateField.ValueLow = new ZDateTime(2022, 01, 02);
			AssertHasError(dateField.ValueHighInfo, dateField.InvalidPeriodDateErrorMessageWithCompany);
		}

		public class PeriodDateRangeFieldForTesting : PeriodDateRangeField
		{
			public PeriodDateRangeFieldForTesting(BusinessObjectFactory factory) : base(factory)
			{
			}

			internal new static string InvalidPeriodDateErrorMessage => PeriodDateRangeField.InvalidPeriodDateErrorMessage;

			internal new static string PeriodDateRangeDifferentYearErrorMessage => PeriodDateRangeField.PeriodDateRangeDifferentYearErrorMessage;
		}
	}
}
