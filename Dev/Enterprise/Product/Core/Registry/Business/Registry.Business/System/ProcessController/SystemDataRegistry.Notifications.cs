using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	partial class SystemDataRegistry
	{
		#region SuppressResourceStringsCheckRegion

		public StringRegistryItem ServiceTaskNudgeCommunicationErrorNotificationEmail
		{
			get =>
				GetItem("ServiceTaskNudgeCommunicationErrorNotificationEmail", () =>
					new StringRegistryItem(
						"ServiceTaskNudgeCommunicationErrorNotificationEmail",
						SystemDataRegistry.Categories.System_ProcessController_Notifications,
						ResString.GetMultilingualString("{BA4BC646-02D4-47F8-9EA2-EDC2E2C42715}", "Service Task Nudge Communication Errors Notification email"),
						ResString.GetMultilingualString("{08914CE2-280E-4858-83B1-8361AD16FD24}", "The email address that will be notified about communication errors during nudging Service Tasks. If address is empty no notifications are sent."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
						string.Empty)
				);
		}

		public IntRegistryItem ServiceTaskNudgeCommunicationErrorNotificationFrequencyInMinutes
		{
			get =>
				GetItem("ServiceTaskNudgeCommunicationErrorNotificationFrequencyInMinutes", () =>
					new IntRegistryItem(
						"ServiceTaskNudgeCommunicationErrorNotificationFrequencyInMinutes",
						SystemDataRegistry.Categories.System_ProcessController_Notifications,
						ResString.GetMultilingualString("{B62B66DC-4D57-47D4-9132-EBE967E341DA}", "Service Task Nudge Communication Errors Notification Frequency"),
						ResString.GetMultilingualString("{4023EAF5-F86A-4A96-B8CB-84A9449949C1}", "Minimal frequency of emails for communication errors during nudging Service Tasks (minutes). 0 - notify on any occasion."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
						60,
						0,
						1440)
				);
		}

		public TimeSpan ServiceTaskNudgeCommunicationErrorNotificationFrequency => TimeSpan.FromMinutes(ServiceTaskNudgeCommunicationErrorNotificationFrequencyInMinutes.Value);

		#endregion SuppressResourceStringsCheckRegion
	}
}
