using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.PeriodManagement.Testing
{
	[TestedType(typeof(ExtendLastFinancialYearSettings))]
	public class ExtendLastFinancialYearSettingsTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ExtendLastFinancialYearSettings();
		}

		protected override void SetUp()
		{
			base.SetUp();
			extendLastFinancialYearSettings = (ExtendLastFinancialYearSettings)GetNewBusinessObject();
		}
		ExtendLastFinancialYearSettings extendLastFinancialYearSettings;

		public void TestValidatePeriodFormat()
		{
			extendLastFinancialYearSettings.PeriodFormat = ZString.Empty;
			AssertHasError(extendLastFinancialYearSettings.PeriodFormatInfo, "Please enter a value.");

			extendLastFinancialYearSettings.PeriodFormat = "AC";
			AssertHasError(extendLastFinancialYearSettings.PeriodFormatInfo, "Enter a valid selection.");

			extendLastFinancialYearSettings.PeriodFormat = ACPeriodFormat.Month;
			AssertNoError(extendLastFinancialYearSettings.PeriodFormatInfo, "Enter a valid selection.");

			extendLastFinancialYearSettings.PeriodFormat = ACPeriodFormat.Weeks;
			AssertNoError(extendLastFinancialYearSettings.PeriodFormatInfo, "Enter a valid selection.");

			extendLastFinancialYearSettings.PeriodFormat = ACPeriodFormat.FourFourFive;
			AssertNoError(extendLastFinancialYearSettings.PeriodFormatInfo, "Enter a valid selection.");

			extendLastFinancialYearSettings.PeriodFormat = ACPeriodFormat.FourWeeks;
			AssertNoError(extendLastFinancialYearSettings.PeriodFormatInfo, "Enter a valid selection.");
		}

		public void TestValidateEndDate()
		{
			extendLastFinancialYearSettings.StartDate = new ZDateTime(2020, 01, 01);
			extendLastFinancialYearSettings.EndDate = ZDateTime.Empty;
			AssertHasError(extendLastFinancialYearSettings.EndDateInfo, "Please enter a Financial Year End Date.");

			extendLastFinancialYearSettings.EndDate = ZDateTime.Invalid;
			AssertHasError(extendLastFinancialYearSettings.EndDateInfo, "Enter a valid selection.");

			extendLastFinancialYearSettings.LastPeriodEndDate = new ZDateTime(2023, 02, 28);
			extendLastFinancialYearSettings.EndDate = new ZDateTime(2023, 01, 31);
			AssertHasError(extendLastFinancialYearSettings.EndDateInfo, "End Date cannot be earlier than Last Day of Last Accounting Period");

			extendLastFinancialYearSettings.PeriodFormat = ACPeriodFormat.Month;
			extendLastFinancialYearSettings.EndDate = new ZDateTime(2023, 03, 30);
			AssertHasError(extendLastFinancialYearSettings.EndDateInfo, "Financial Year End Date must be equal to Last Day of Month");

			extendLastFinancialYearSettings.PeriodFormat = ACPeriodFormat.Weeks;
			extendLastFinancialYearSettings.EndDate = new ZDateTime(2023, 03, 30);
			AssertHasWarning(extendLastFinancialYearSettings.EndDateInfo, "Financial Year normally ends on the last day of a month");

			extendLastFinancialYearSettings.EndDate = new ZDateTime(2023, 02, 28);
			AssertHasError(extendLastFinancialYearSettings.EndDateInfo, "Date Range between Financial Year Start Date and Financial Year End Date is more than 2 years");
			AssertNoWarning(extendLastFinancialYearSettings.EndDateInfo, "Financial Year normally ends on the last day of a month");

			extendLastFinancialYearSettings.EndDate = new ZDateTime(2021, 01, 31);
			AssertNoError(extendLastFinancialYearSettings.EndDateInfo, "Date Range between Financial Year Start Date and Financial Year End Date is more than 2 years");
		}
	}
}
