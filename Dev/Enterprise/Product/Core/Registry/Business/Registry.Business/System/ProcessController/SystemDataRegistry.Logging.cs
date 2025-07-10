using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	partial class SystemDataRegistry
	{
		public CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryItem LoggingMethods =>
			GetItem(nameof(LoggingMethods), () => new CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryItem(
					nameof(LoggingMethods),
					Categories.System_ProcessController_Logging,
					ResString.GetMultilingualString("27805F4E-C4FB-4359-A19D-53CEA3909386", "Logging Method"),
				ResString.GetMultilingualString("5D2069E1-004E-4478-8767-76D129A81B96", @"Logging method to use for both reading and writing.

Logs can be written with multiple methods simultaneously, but logs can only be read and displayed inside CargoWise from one method."),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
					new LoggingMethodsDataType(),
					false));

		public VerboseLoggingRegistryItem ProcessControllerVerboseLogging =>
			GetItem("ProcessControllerLimitedVerboseLogging",
				() => new VerboseLoggingRegistryItem(
					"ProcessControllerLimitedVerboseLogging",
					Categories.System_ProcessController_Logging,
					ResString.GetMultilingualString("{E3CF1B9F-3FF1-4EE4-83B6-28B8C9A224F7}", "Enable verbose logging"),
					ResString.GetMultilingualString("{7C1434CA-CFD0-4292-BA46-F1F4B975A207}", "Specifies whether or not verbose logging is enabled."),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController));

		public BooleanRegistryItem ProcessControllerNLogInternalLoggingEnabled =>
			GetItem("ProcessControllerNLogInternalLoggingEnabled", () => new BooleanRegistryItem(
					"ProcessControllerNLogInternalLoggingEnabled",
					Categories.System_ProcessController_Logging,
					ResString.GetMultilingualString("{36CBFE95-C93E-4E87-9BE3-BDDA42F84AE2}", "Enable NLog internal logging"),
				ResString.GetMultilingualString("{4CDC3C0D-00F0-42FF-AF56-6811BFE02797}", @"Activate NLog internal logging.

This will output the internal debug information of the logging framework. It will be written to the same location that the application log was trying to be written to."),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
					false));
	}
}
