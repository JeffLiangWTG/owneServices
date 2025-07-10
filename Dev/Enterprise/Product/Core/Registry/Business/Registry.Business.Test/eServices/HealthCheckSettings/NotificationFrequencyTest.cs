using CargoWise.EntityFramework;
using Enterprise.Registry.Business.eServices.HealthCheckSettings;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(NotificationFrequency))]
	sealed class NotificationFrequencyTest : RegistryBusinessObjectTemplateTestCase<NotificationFrequency>
	{
		public void TestTimeIntervalReadOnlyness()
		{
			NotificationFrequency.Settings = HealthCheckConstants.NotificationFrequencyConstants.Disable;
			AssertEquals(true, NotificationFrequency.TimeIntervalInfo.ReadOnly);
			AssertEquals(0, NotificationFrequency.TimeInterval);

			NotificationFrequency.Settings = HealthCheckConstants.NotificationFrequencyConstants.SPAM;
			AssertEquals(true, NotificationFrequency.TimeIntervalInfo.ReadOnly);
			AssertEquals(5, NotificationFrequency.TimeInterval);

			NotificationFrequency.Settings = HealthCheckConstants.NotificationFrequencyConstants.Periodically;
			AssertEquals(false, NotificationFrequency.TimeIntervalInfo.ReadOnly);
			AssertEquals(NotificationFrequency.DEFAULT_TIME_INTERVAL, NotificationFrequency.TimeInterval);
		}

		public void TestSettingsValidation()
		{
			NotificationFrequency.Settings = "";
			AssertHasError(NotificationFrequency.SettingsInfo, "Please enter a value.");

			NotificationFrequency.Settings = "Invalid";
			AssertHasError(NotificationFrequency.SettingsInfo, "Enter a valid selection.");
		}

		public void TestTimeIntervalValidation()
		{
			NotificationFrequency.Settings = HealthCheckConstants.NotificationFrequencyConstants.Periodically;

			NotificationFrequency.TimeInterval = NotificationFrequency.MIN_TIME_INTERVAL_COUNT;
			AssertHasError(NotificationFrequency.TimeIntervalInfo, "Must be a decimal number, less than or equal to 87600 and larger than 4.");

			NotificationFrequency.TimeInterval = NotificationFrequency.MAX_TIME_INTERVAL_COUNT + 1;
			AssertHasError(NotificationFrequency.TimeIntervalInfo, "Must be a decimal number, less than or equal to 87600 and larger than 4.");
		}

		NotificationFrequency NotificationFrequency
		{
			get { return notificationFrequency ?? (notificationFrequency = new NotificationFrequency(Factory)); }
		}
		NotificationFrequency notificationFrequency;

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override NotificationFrequency GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override NotificationFrequency GetBusinessObjectToSerialise()
		{
			return new NotificationFrequency(Factory)
			{
				Settings = HealthCheckConstants.NotificationFrequencyConstants.Periodically
			};
		}
	}
}
