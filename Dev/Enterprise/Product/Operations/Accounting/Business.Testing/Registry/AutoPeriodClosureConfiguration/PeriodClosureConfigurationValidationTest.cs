using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Registry.Business;
using static Enterprise.Accounting.Business.AccountingUtils;

namespace Enterprise.Accounting.Business.Testing
{
	public class PeriodClosureConfigurationValidationTest : TestCaseWithFactory
	{
		public void TestValidateInterval()
		{
			var errorMessageForNoneZeroSubLedgerInterval = "The 'interval' value of sub ledger type cannot be 0 if the 'interval' value of general ledger type is greater than 0";
			var errorMessageForNoneZeroGeneralLedgerInterval = "The 'interval' value of general ledger type cannot be 0 if the 'interval' value of adjustment ledger type is greater than 0";

			var errorMessageForGeneralLedgerIntervalSize = "The 'interval' value for general ledger type must be the same or greater than the sub ledger type";
			var errorMessageForAdjustmentLedgerIntervalSize = "The 'interval' value for adjustment ledger type must be the same or greater than the general ledger type";

			var testCases = new (int SubLedgerInterval, int GeneralLedgerInterval, int AdjustmentLedgerInterval, string errorMessage)[]
			{
				(0,1,1, "Error - SubLedgerInterval: " + errorMessageForNoneZeroSubLedgerInterval),
				(0,1,0, "Error - SubLedgerInterval: " + errorMessageForNoneZeroSubLedgerInterval),
				(1,0,1, "Error - GeneralLedgerInterval: " + errorMessageForNoneZeroGeneralLedgerInterval),
				(0,0,1, "Error - GeneralLedgerInterval: " + errorMessageForNoneZeroGeneralLedgerInterval),
				(2,1,0, "Error - GeneralLedgerInterval: " + errorMessageForGeneralLedgerIntervalSize),
				(2,1,1, "Error - GeneralLedgerInterval: " + errorMessageForGeneralLedgerIntervalSize),
				(2,3,1, "Error - AdjustmentLedgerInterval: " + errorMessageForAdjustmentLedgerIntervalSize),

				(1,0,0, string.Empty),
				(1,1,0, string.Empty),
				(0,0,0, string.Empty),
				(1,1,2, string.Empty),
				(2,3,3, string.Empty),
			};

			foreach (var testCase in testCases)
			{
				var config = new PeriodClosureConfiguration(PeriodClosureConfigurationIntervalType.Minutes, testCase.SubLedgerInterval, testCase.GeneralLedgerInterval, testCase.AdjustmentLedgerInterval);
				config.Validation.ValidateInterval();
				if (!string.IsNullOrEmpty(testCase.errorMessage))
				{
					AssertEquals(1, config.NotificationsIncludingChildren.GetErrors().Count());
					AssertEquals(testCase.errorMessage, config.NotificationsIncludingChildren.FirstOrDefault().Message);
				}
				else
				{
					AssertEquals(0, config.NotificationsIncludingChildren.GetErrors().Count());
				}
			}
		}

		public void TestValidateIntervalWillClearOldErrorInfo()
		{
			var config = new PeriodClosureConfiguration(PeriodClosureConfigurationIntervalType.Minutes);
			config.SubLedgerIntervalInfo.AddError("Error1");
			config.GeneralLedgerIntervalInfo.AddError("Error2");
			config.AdjustmentLedgerIntervalInfo.AddError("Error3");

			AssertHasError(config.SubLedgerIntervalInfo, "Error1");
			AssertHasError(config.GeneralLedgerIntervalInfo, "Error2");
			AssertHasError(config.AdjustmentLedgerIntervalInfo, "Error3");

			config.Validation.ValidateInterval();

			AssertNoErrors(config.SubLedgerIntervalInfo);
			AssertNoErrors(config.GeneralLedgerIntervalInfo);
			AssertNoErrors(config.AdjustmentLedgerIntervalInfo);
		}

		public void TestValidateIntervalType()
		{
			var config = new PeriodClosureConfiguration();
			config.Validation.ValidateIntervalType();
			AssertHasError(config.IntervalTypeInfo, "Please enter a value.");

			config.IntervalType = "AAA";
			config.Validation.ValidateIntervalType();
			AssertNoError(config.IntervalTypeInfo, "Please enter a value.");
			AssertHasError(config.IntervalTypeInfo, "Enter a valid selection.");

			config.IntervalType = PeriodClosureConfigurationIntervalType.Minutes;
			config.Validation.ValidateIntervalType();
			AssertNoErrors(config.IntervalTypeInfo);
		}
	}
}
