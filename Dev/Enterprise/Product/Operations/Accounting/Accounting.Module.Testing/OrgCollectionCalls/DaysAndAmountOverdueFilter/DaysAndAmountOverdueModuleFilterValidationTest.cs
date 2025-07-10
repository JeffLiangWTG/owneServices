using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Accounting.Module.Testing
{
	public class DaysAndAmountOverdueModuleFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateAmountOverdue()
		{
			var filter = new DaysAndAmountOverdueModuleFilter("Max Days and/or Amount Overdue");
			filter.AmountOverdue = 10;
			AssertNoErrors(filter.AmountOverdueInfo);

			filter.AmountOverdue = decimal.MaxValue;
			AssertHasErrors(filter.AmountOverdueInfo);

			filter.AmountOverdue = -5;
			AssertNoErrors(filter.AmountOverdueInfo);
		}

		public void TestValidateAndOrDecider()
		{
			var filter = new DaysAndAmountOverdueModuleFilter("Max Days and/or Amount Overdue");
			filter.AndOrDecider = "AND";
			AssertNoErrors(filter.AndOrDeciderInfo);

			filter.AndOrDecider = "BAD";
			AssertHasError(filter.AndOrDeciderInfo, "Choose a valid code.");

			filter.AndOrDecider = "OR";
			AssertNoErrors(filter.AndOrDeciderInfo);

			filter.AndOrDecider = ZString.Empty;
			AssertHasError(filter.AndOrDeciderInfo, "Choose a valid code.");
		}

		public void TestValidateDaysOverdue()
		{
			var filter = new DaysAndAmountOverdueModuleFilter("Max Days and/or Amount Overdue");
			filter.DaysOverdue = 7;
			AssertNoErrors(filter.DaysOverdueInfo);

			filter.DaysOverdue = -5;
			AssertHasError(filter.DaysOverdueInfo, "Days Overdue should be greater or equal to 0.");

			filter.DaysOverdue = 0;
			AssertNoErrors(filter.DaysOverdueInfo);
		}
	}
}
