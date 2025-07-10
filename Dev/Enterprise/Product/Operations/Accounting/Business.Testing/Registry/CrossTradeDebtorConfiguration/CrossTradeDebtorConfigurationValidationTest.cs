using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing
{
	public class CrossTradeDebtorConfigurationValidationTest : TestCaseWithFactory
	{
		public void TestCrossTradeJobTypeValidation()
		{
			var configuration = new CrossTradeDebtorConfiguration();

			configuration.JobType = "";
			AssertHasError(configuration.JobTypeInfo, "Please enter a Job Type.");

			configuration.JobType = "ABC";
			AssertHasError(configuration.JobTypeInfo, "Enter a valid selection.");
		}

		public void TestCrossTradeModeValidation()
		{
			var configuration = new CrossTradeDebtorConfiguration();

			configuration.Mode = "";
			AssertHasError(configuration.ModeInfo, "Please enter a Mode.");

			configuration.Mode = "ABC";
			AssertHasError(configuration.ModeInfo, "Enter a valid selection.");
		}

		public void TestCrossTradeDefaultDebtorValidation()
		{
			var configuration = new CrossTradeDebtorConfiguration();

			configuration.Debtor = "";
			AssertHasError(configuration.DebtorInfo, "Please enter a Default Debtor.");

			configuration.Debtor = "ABC";
			AssertHasError(configuration.DebtorInfo, "Enter a valid selection.");
		}

		public void TestChargePaymentTypeValidation()
		{
			var configuration = new CrossTradeDebtorConfiguration();

			configuration.ChargePaymentType = "";
			AssertHasError(configuration.ChargePaymentTypeInfo, "Please enter a Charges Prepaid/Collect.");

			configuration.ChargePaymentType = "ABC";
			AssertHasError(configuration.ChargePaymentTypeInfo, "Enter a valid selection.");
		}

		public void TestDuplicatedConfiguration()
		{
			var header = new CrossTradeDebtorConfigurationHeader();

			var configuration1 = header.Configurations.AddNew();
			configuration1.JobType = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All;
			configuration1.Mode = JobConfigurationSelectorLookups.ModeAdditionalCodes.All;
			configuration1.ChargePaymentType = PrepaidCollectFreightForwardingList.Codes.CCX;
			configuration1.Debtor = DefaultDebtorList.Codes.CollectBillToParty;

			var configuration2 = header.Configurations.AddNew();
			configuration2.JobType = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All;
			configuration2.Mode = JobConfigurationSelectorLookups.ModeAdditionalCodes.All;
			configuration2.ChargePaymentType = PrepaidCollectFreightForwardingList.Codes.PPD;
			configuration2.Debtor = DefaultDebtorList.Codes.PrepaidBillToParty;

			configuration1.Validation.CheckDuplicatedConfiguration();
			configuration2.Validation.CheckDuplicatedConfiguration();
			AssertNoErrors(configuration1);
			AssertNoErrors(configuration2);

			configuration2.ChargePaymentType = PrepaidCollectFreightForwardingList.Codes.CCX;
			configuration1.Validation.CheckDuplicatedConfiguration();
			configuration2.Validation.CheckDuplicatedConfiguration();
			AssertHasRowError(configuration1, "A record already exists with the same configuration.");
			AssertHasRowError(configuration2, "A record already exists with the same configuration.");

			configuration1.ChargePaymentType = PrepaidCollectFreightForwardingList.Codes.All;
			configuration1.Validation.CheckDuplicatedConfiguration();
			configuration2.Validation.CheckDuplicatedConfiguration();
			var expectedError = "PPD or CCX cannot be used in combination with Charges Prepaid/Collect - ALL for the same configuration.\r\nPlease either create a single configuration using Charges Prepaid/Collect - ALL, or a configuration for each PPD and CCX.";
			AssertHasRowError(configuration1, expectedError);
			AssertHasRowError(configuration2, expectedError);
		}

		public void TestCrossTradePreSaveValidate()
		{
			var configuration = new CrossTradeDebtorConfiguration();

			configuration.JobType = "";
			configuration.Mode = "ALD";
			configuration.ChargePaymentType = "III";
			configuration.Debtor = "FailDebtor";

			configuration.RunPreSaveValidation();

			AssertHasErrors(configuration.JobTypeInfo);
			AssertHasErrors(configuration.ModeInfo);
			AssertHasErrors(configuration.ChargePaymentTypeInfo);
			AssertHasErrors(configuration.DebtorInfo);
		}
	}
}
