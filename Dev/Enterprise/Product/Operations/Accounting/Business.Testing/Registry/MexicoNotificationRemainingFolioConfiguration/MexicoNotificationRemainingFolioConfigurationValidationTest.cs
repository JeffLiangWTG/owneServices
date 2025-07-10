using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Registry.Business;

namespace Enterprise.Accounting.Business.Testing
{
	public class MexicoNotificationRemainingFolioConfigurationValidationTest : TestCaseWithFactory
	{
		public void TestValidateFolioQuantity_Less_Than_Zero()
		{
			var config = new MexicoNotificationRemainingFolioConfiguration();

			config.FoliosQuantity = 0;
			AssertNoErrors("Zero", config.FoliosQuantityInfo);

			config.FoliosQuantity = 1;
			AssertNoErrors("Positive value", config.FoliosQuantityInfo);

			config.FoliosQuantity = -1;
			AssertHasError("Negative value", config.FoliosQuantityInfo, "Must be a positive number.");
		}

		public void TestValidateFolioQuantity_Less_Than_Zero_With_SuspendedValidation()
		{
			var config = new MexicoNotificationRemainingFolioConfiguration();

			using (config.GetValidationSuspender())
			{
				config.FoliosQuantity = -1;
				AssertNoErrors("Precondition", config.FoliosQuantityInfo);
			}

			config.FoliosQuantity = -1;
			AssertHasErrors("Postcondition", config.FoliosQuantityInfo);
		}

		public void TestValidateFolioQuantity_Less_Than_Zero_With_RunPreSaveValidation()
		{
			var config = new MexicoNotificationRemainingFolioConfiguration();

			using (config.GetValidationSuspender())
			{
				config.FoliosQuantity = -1;
				AssertNoErrors("Precondition", config.FoliosQuantityInfo);
			}

			config.RunPreSaveValidation();
			AssertHasErrors("Postcondition", config.FoliosQuantityInfo);
		}

		public void TestValidateInterval_Less_Than_Zero()
		{
			var config = new MexicoNotificationRemainingFolioConfiguration();

			config.Interval = 0;
			AssertNoErrors("Zero", config.IntervalInfo);

			config.Interval = 1;
			config.FoliosQuantity = 1;
			AssertNoErrors("Positive value", config.IntervalInfo);

			config.Interval = -1;
			AssertHasError("Negative value", config.IntervalInfo, "Must be a positive number.");
		}

		public void TestValidateInterval_Less_Than_Zero_With_SuspendedValidation()
		{
			var config = new MexicoNotificationRemainingFolioConfiguration();

			using (config.GetValidationSuspender())
			{
				config.Interval = -1;
				AssertNoErrors("Precondition", config.IntervalInfo);
			}

			config.Interval = -1;
			AssertHasErrors("Postcondition", config.IntervalInfo);
		}

		public void TestValidateInterval_Less_Than_Zero_With_RunPreSaveValidation()
		{
			var config = new MexicoNotificationRemainingFolioConfiguration();

			using (config.GetValidationSuspender())
			{
				config.Interval = -1;
				AssertNoErrors("Precondition", config.IntervalInfo);
			}

			config.RunPreSaveValidation();
			AssertHasErrors("Postcondition", config.IntervalInfo);
		}

		public void TestValidateInterval_Interval_Higest_Than_FolioQuantity()
		{
			var config = new MexicoNotificationRemainingFolioConfiguration();

			config.Interval = 0;
			config.FoliosQuantity = 1;

			AssertNoErrors("Interval less than FolioQuantity", config.IntervalInfo);

			config.Interval = 1;
			config.FoliosQuantity = 1;

			AssertNoErrors("Interval equal to FolioQuantity", config.IntervalInfo);

			config.Interval = 2;
			config.FoliosQuantity = 1;

			AssertHasError(config.IntervalInfo, "Interval value must be smaller than or equal to the value of Quantity of remaining Folios/Timbres field.");
		}

		public void TestValidateInterval_Interval_Higest_Than_FolioQuantity_With_SuspendedValidation()
		{
			var config = new MexicoNotificationRemainingFolioConfiguration();

			using (config.GetValidationSuspender())
			{
				config.Interval = 2;
				config.FoliosQuantity = 1;
				AssertNoErrors("Precondition", config.IntervalInfo);
			}

			config.Interval = 2;
			config.FoliosQuantity = 1;
			AssertHasErrors("Postcondition", config.IntervalInfo);
		}

		public void TestValidateInterval_Interval_Higest_Than_FolioQuantity_With_RunPreSaveValidation()
		{
			var config = new MexicoNotificationRemainingFolioConfiguration();

			using (config.GetValidationSuspender())
			{
				config.Interval = 2;
				config.FoliosQuantity = 1;
				AssertNoErrors("Precondition", config.IntervalInfo);
			}

			config.RunPreSaveValidation();
			AssertHasErrors("Postcondition", config.IntervalInfo);
		}
	}
}
