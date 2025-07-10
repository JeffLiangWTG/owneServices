using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.PeriodManagement.Testing
{
	public class UpdateGldPeriodsDateRangeSettingValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckStartDate()
		{
			dateRangeSetting.StartDate = ZDateTime.Empty;
			AssertHasWarning(dateRangeSetting.StartDateInfo, "Start Date is empty. It may take a long time for the system to update the General Ledger Data Records.");

			dateRangeSetting.StartDate = new ZDateTime(2022, 9, 2);
			AssertHasError(dateRangeSetting.StartDateInfo, "Start Date specified must be the 'Start Date' of an accounting period of the login company.");

			dateRangeSetting.StartDate = new ZDateTime(2022, 9, 1);
			AssertNoErrors(dateRangeSetting.StartDateInfo);
		}

		public void TestCheckEndDate()
		{
			dateRangeSetting.EndDate = ZDateTime.Empty;
			AssertHasWarning(dateRangeSetting.EndDateInfo, "End Date is empty. It may take a long time for the system to update the General Ledger Data Records.");

			dateRangeSetting.EndDate = new ZDateTime(2022, 9, 25);
			AssertHasError(dateRangeSetting.EndDateInfo, "End Date specified must be the 'End Date' of an accounting period of the login company.");

			dateRangeSetting.EndDate = new ZDateTime(2022, 9, 30);
			AssertNoErrors(dateRangeSetting.EndDateInfo);
		}

		public void TestCompareStartDateAndEndDate()
		{
			dateRangeSetting.StartDate = new ZDateTime(2022, 10, 1);
			dateRangeSetting.EndDate = new ZDateTime(2022, 9, 30);
			AssertHasError(dateRangeSetting.StartDateInfo, "End Date specified must be greater than the 'Start Date'.");
			AssertHasError(dateRangeSetting.EndDateInfo, "End Date specified must be greater than the 'Start Date'.");

			dateRangeSetting.StartDate = new ZDateTime(2022, 10, 1);
			dateRangeSetting.EndDate = new ZDateTime(2022, 10, 31);
			AssertNoErrors(dateRangeSetting.StartDateInfo);
			AssertNoErrors(dateRangeSetting.EndDateInfo);

			dateRangeSetting.StartDate = new ZDateTime(2022, 09, 01);
			dateRangeSetting.EndDate = new ZDateTime(2023, 09, 30);
			AssertHasWarning(dateRangeSetting.StartDateInfo, "Start Date and End Date specified do not fall in the same accounting year. It may take a long time for the system to update the General Ledger Data Records.");
			AssertHasWarning(dateRangeSetting.EndDateInfo, "Start Date and End Date specified do not fall in the same accounting year. It may take a long time for the system to update the General Ledger Data Records.");

			dateRangeSetting.StartDate = new ZDateTime(2023, 09, 01);
			AssertNoWarnings(dateRangeSetting.StartDateInfo);
			AssertNoWarnings(dateRangeSetting.EndDateInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			dateRangeSetting = new UpdateGldPeriodsDateRangeSetting();

			TestObjectCreator.CreateTestPeriodsForEntireYear(2022);
			TestObjectCreator.CreateTestPeriodsForEntireYear(2023);

			Factory.Save();
		}

		UpdateGldPeriodsDateRangeSetting dateRangeSetting;

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}
		TestObjectCreator fTestObjectCreator;
	}
}
