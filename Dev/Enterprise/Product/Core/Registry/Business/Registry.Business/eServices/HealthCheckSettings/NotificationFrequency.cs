using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.eServices.HealthCheckSettings
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class NotificationFrequency : AutoNotificationFrequency
	{
		internal const int DEFAULT_TIME_INTERVAL = 60;
		public const int MAX_TIME_INTERVAL_COUNT = 87600;
		internal const int MIN_TIME_INTERVAL_COUNT = 4;

		public NotificationFrequency() { }

		public NotificationFrequency(BusinessObjectFactory factory)
			: base(factory)
		{ }

		[List("Lookups.Settings")]
		public override ZString Settings
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return base.Settings;
			}
			set
			{
				base.Settings = value;
				TimeIntervalInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateTimeInterval();
				}
			}
		}

		public override void ValidateSettings()
		{
			base.ValidateSettings();
			MandatoryValidation.CheckEntered(SettingsInfo);
			ListValidation.ErrorIfInvalidCode(SettingsInfo, Lookups.Settings);
		}

		public override ZInt TimeInterval
		{
			get
			{
				if (TimeInterval_ReadOnly)
				{
					return Settings == HealthCheckConstants.NotificationFrequencyConstants.SPAM ? new ZInt(MIN_TIME_INTERVAL_COUNT + 1) : ZInt.Zero;
				}
				return base.TimeInterval;
			}
			set
			{
				base.TimeInterval = value;
			}
		}

		public override void ValidateTimeInterval()
		{
			base.ValidateTimeInterval();
			if (!TimeInterval_ReadOnly && (TimeInterval <= MIN_TIME_INTERVAL_COUNT || TimeInterval > MAX_TIME_INTERVAL_COUNT))
			{
				TimeIntervalInfo.AddError(Res.GetString("5e9c46cb-58a1-4f0d-9631-203f50e7e279", "Must be a decimal number, less than or equal to {0} and larger than {1}.", MAX_TIME_INTERVAL_COUNT, MIN_TIME_INTERVAL_COUNT));
			}
		}

		protected override bool TimeInterval_ReadOnly
		{
			get { return Settings.ToUpper() != HealthCheckConstants.NotificationFrequencyConstants.Periodically; }
		}

		public NotificationFrequencyLookups Lookups
		{
			get { return lookups ?? (lookups = new NotificationFrequencyLookups(this)); }
		}
		NotificationFrequencyLookups lookups;

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			Settings = HealthCheckConstants.NotificationFrequencyConstants.Disable;
			TimeInterval = DEFAULT_TIME_INTERVAL;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new NotificationFrequency(factory);
		}
	}
}
