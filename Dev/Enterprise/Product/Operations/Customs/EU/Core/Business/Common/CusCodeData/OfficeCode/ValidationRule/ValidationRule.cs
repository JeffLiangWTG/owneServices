using CargoWise.ComponentModel;

namespace Enterprise.Customs.EU.Business
{
	public abstract class ValidationRule
	{
		public abstract bool IsApplied { get; }

		public abstract ValidationResult Validate(object value);

		public INotificationType NotificationSeverity => NotificationSeverityCore;

		protected virtual INotificationType NotificationSeverityCore => NotificationType.Warning;
	}
}
