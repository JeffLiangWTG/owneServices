using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Testing
{
	sealed partial class SystemDataRegistryTest
	{
		public void TestServiceTaskNudgeCommunicationErrorNotificationEmail()
		{
			TestRegistryItem(
				ItemSet.ServiceTaskNudgeCommunicationErrorNotificationEmail,
				"ServiceTaskNudgeCommunicationErrorNotificationEmail",
				SystemDataRegistry.Categories.System_ProcessController_Notifications,
				"Service Task Nudge Communication Errors Notification email",
				"The email address that will be notified about communication errors during nudging Service Tasks. If address is empty no notifications are sent.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				TextEditorType.TextBox,
				string.Empty);
		}

		public void TestServiceTaskNudgeCommunicationErrorNotificationFrequencyInMinutes()
		{
			TestRegistryItem(
				ItemSet.ServiceTaskNudgeCommunicationErrorNotificationFrequencyInMinutes,
				"ServiceTaskNudgeCommunicationErrorNotificationFrequencyInMinutes",
				SystemDataRegistry.Categories.System_ProcessController_Notifications,
				"Service Task Nudge Communication Errors Notification Frequency",
				"Minimal frequency of emails for communication errors during nudging Service Tasks (minutes). 0 - notify on any occasion.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				60,
				0,
				1440);
		}

		public void TestServiceTaskNudgeCommunicationErrorNotificationFrequency()
		{
			CombineAssertions(() =>
			{
				Test(1, TimeSpan.FromMinutes(1));
				Test(10, TimeSpan.FromMinutes(10));
				Test(100, TimeSpan.FromMinutes(100));
			});

			void Test(int value, TimeSpan expected)
			{
				using (SystemDataRegistry.Instance.ServiceTaskNudgeCommunicationErrorNotificationFrequencyInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value))
				{
					AssertEquals(expected, SystemDataRegistry.Instance.ServiceTaskNudgeCommunicationErrorNotificationFrequency);
				}
			}
		}
	}
}
