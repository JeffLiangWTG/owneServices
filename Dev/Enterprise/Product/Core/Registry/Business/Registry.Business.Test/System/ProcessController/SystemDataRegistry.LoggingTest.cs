using System.Linq;
using Enterprise.Integration;

namespace Enterprise.Registry.Business.Testing
{
	sealed partial class SystemDataRegistryTest
	{
		public void TestLoggingMethods()
		{
			TestRegistryItem(
				ItemSet.LoggingMethods,
				"LoggingMethods",
				SystemDataRegistry.Categories.System_ProcessController_Logging,
				"Logging Method",
				"Logging method to use for both reading and writing.\r\n\r\nLogs can be written with multiple methods simultaneously, but logs can only be read and displayed inside CargoWise from one method.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				new LoggingMethodsDataType(),
				false);
		}

		public void TestProcessControllerVerboseLogging()
		{
			TestGenericRegistryItem(
				ItemSet.ProcessControllerVerboseLogging,
				"ProcessControllerLimitedVerboseLogging",
				SystemDataRegistry.Categories.System_ProcessController_Logging,
				"Enable verbose logging",
				"Specifies whether or not verbose logging is enabled.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController);

			var expected = new VerboseLoggingRegistryDataType()
				.DefaultValue
				.Cast<VerboseLoggingBusinessObject>()
				.Select(time => (time.Code, time.Description.GetUnresolvedString(), time.Value));
			var collection = ItemSet.ProcessControllerVerboseLogging
				.DefaultValue
				.Cast<VerboseLoggingBusinessObject>()
				.Select(time => (time.Code, time.Description.GetUnresolvedString(), time.Value));
			AssertContainsExactElementsInAnyOrder("Default collection", expected, collection);
		}

		public void TestProcessControllerNLogInternalLoggingEnabled()
		{
			TestRegistryItem(ItemSet.ProcessControllerNLogInternalLoggingEnabled,
				"ProcessControllerNLogInternalLoggingEnabled",
				SystemDataRegistry.Categories.System_ProcessController_Logging,
				"Enable NLog internal logging",
				"Activate NLog internal logging.\r\n\r\nThis will output the internal debug information of the logging framework. It will be written to the same location that the application log was trying to be written to.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				false);
		}
	}
}
