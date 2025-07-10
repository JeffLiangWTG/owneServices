using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	partial class SystemDataRegistry
	{
		public StringRegistryItem ProcessControllerSyslogServerHostname =>
			GetItem("ProcessControllerSyslogServerHostname", () => new StringRegistryItem(
				"ProcessControllerSyslogServerHostname",
				Categories.System_ProcessController_Logging_Syslog,
				ResString.GetMultilingualString("92DD5860-D228-4170-9EF0-209B6A5E7C21", "Syslog server hostname"),
				ResString.GetMultilingualString("40A0CA52-3B98-4349-BCB7-A07BDE9636DE", "Enter syslog server hostname."),
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				string.Empty));

		public IntRegistryItem ProcessControllerSyslogServerPort =>
			GetItem("ProcessControllerSyslogServerPort", () => new IntRegistryItem(
				"ProcessControllerSyslogServerPort",
				Categories.System_ProcessController_Logging_Syslog,
				ResString.GetMultilingualString("B3BBE85E-13CD-44E4-B0AD-30D6D022A93B", "Syslog server port"),
				ResString.GetMultilingualString("B744053C-3FFA-42E3-B73C-E7F99BEDFDBA", "Enter syslog server port."),
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController));

		public BooleanRegistryItem ProcessControllerSyslogSslEnable =>
			GetItem("ProcessControllerSyslogSslEnable", () => new BooleanRegistryItem(
				"ProcessControllerSyslogSslEnable",
				Categories.System_ProcessController_Logging_Syslog,
				ResString.GetMultilingualString("BC41729A-CAEF-4EFB-B04E-C56EAB7B412C", "Enable SSL"),
				ResString.GetMultilingualString("F6BC37CB-71B2-42F9-8430-FD714667A480", "Enable the use of security protocol SSL."),
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				false));

		public CodePairRegistryItem ProcessControllerSyslogProtocolVersion =>
			GetItem("ProcessControllerSyslogProtocolVersion", () => new CodePairRegistryItem(
				"ProcessControllerSyslogProtocolVersion",
				Categories.System_ProcessController_Logging_Syslog,
				ResString.GetMultilingualString("4936CD43-41E5-4BEF-99B3-7629714EB1FF", "Syslog protocol version"),
				ResString.GetMultilingualString("B6DB971B-0C8D-45BB-A705-A39F5E02F305", "This setting allows you to set the syslog protocol version."),
				new CodeDescriptionPairListProvider(() => new CodeDescriptionPairList
				{
					new CodeDescriptionPair("RFC5424", ResString.GetMultilingualString("E714499A-7769-493E-8B46-E03CC1C7D1BD", "Protocol Version RFC 5424")),
					new CodeDescriptionPair("RFC3164", ResString.GetMultilingualString("90F63DD4-4F45-49AE-A8E4-1F7129345B1F", "Protocol Version RFC 3164")),
				}),
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				"RFC5424"));

		public CodePairRegistryItem ProcessControllerSyslogNetworkProtocol =>
			GetItem("ProcessControllerSyslogNetworkProtocol", () => new CodePairRegistryItem(
				"ProcessControllerSyslogNetworkProtocol",
				Categories.System_ProcessController_Logging_Syslog,
				ResString.GetMultilingualString("E8F3CDFB-BBC3-49B7-93DC-346BAAD161D2", "Syslog network protocol"),
				ResString.GetMultilingualString("B9AADFD6-8A52-4A50-860A-D4D34DD3AD86", "This setting allows you to set the syslog network protocol."),
				new CodeDescriptionPairListProvider(() => new CodeDescriptionPairList
				{
					new CodeDescriptionPair("TCP", ResString.GetMultilingualString("676E5E59-69AD-489E-801F-0A7683E0542F", "Network Protocol TCP")),
					new CodeDescriptionPair("UDP", ResString.GetMultilingualString("87F3D3AB-9CAB-4598-93D4-1745B5CF5DC3", "Network Protocol UDP")),
				}),
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				"TCP"));
	}
}
