using System;
using System.Collections.Generic;
using Moq;

namespace CargoWise.Data.Providers.Common
{
	public static class SqlFailoverSettingsTestHelper
	{
		public static IDisposable SetMockWindowsRegistry(IEnumerable<string> serversThatAreEnabled)
		{
			var windowsRegistry = CreateMockWindowsRegistry(serversThatAreEnabled);
			return SqlFailoverSettings.SetWindowsRegistryForTest(windowsRegistry);
		}

		public static IWindowsRegistry CreateMockWindowsRegistry(IEnumerable<string> serversThatAreEnabled)
		{
			var windowsRegistryMock = new Mock<IWindowsRegistry>();
			windowsRegistryMock.Setup(r => r.GetValue(MultiSubnetFailoverRegistryKey, It.IsAny<string>(), It.IsAny<object>()))
				.Returns((Func<string, string, object, object>)((k, v, defaultValue) => defaultValue));

			foreach (var serverName in serversThatAreEnabled)
			{
				var registryValueName = serverName.Replace('\\', '$');
				windowsRegistryMock.Setup(r => r.GetValue(MultiSubnetFailoverRegistryKey, registryValueName, It.IsAny<object>())).Returns(1);
			}

			return windowsRegistryMock.Object;
		}

		public const string MultiSubnetFailoverRegistryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\WiseTech Global\SqlFailoverSettings";
	}
}
