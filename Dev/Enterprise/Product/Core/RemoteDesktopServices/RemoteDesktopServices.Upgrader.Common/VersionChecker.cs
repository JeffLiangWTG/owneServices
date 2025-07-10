using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Loader.Common;
using Microsoft.Win32;

namespace CargoWise.RemoteDesktopServices.Upgrader.Common
{
	public class VersionChecker : InstallationItem
	{
		public const string UpgradeCodePath = @"SOFTWARE\Classes\Installer\UpgradeCodes\";
		public const string ProductCodePath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\";
		public const string WOW64ProductCodePath = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\";
		readonly string encryptedUpgradeCode;
		readonly string latestVersion;
		readonly TimeSpan maxWaitDurationForVersionChange;
		readonly string pluginProductName;
		readonly string manualFixLink;

		public VersionChecker(Installation installation, string latestVersion, string encryptedUpgradeCode, TimeSpan waitUpdateTime, string pluginProductName, string manualFixLink)
			: base(installation)
		{
			this.latestVersion = latestVersion;
			this.encryptedUpgradeCode = encryptedUpgradeCode;
			this.maxWaitDurationForVersionChange = waitUpdateTime;
			this.pluginProductName = pluginProductName;
			this.manualFixLink = manualFixLink;
		}

		protected override bool NeedsToInstallCore()
		{
			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected override InstallationResult InstallExcludingDependencies()
		{
			try
			{
				var currentlyInstalledVersion = string.Empty;

				using (var targetRegistry = Registry.LocalMachine)
				{
					Stopwatch stopwatch = new Stopwatch();
					stopwatch.Start();

					do
					{
						if (stopwatch.Elapsed > maxWaitDurationForVersionChange)
						{
							return InstallationResult.Error($"The {pluginProductName} has timed out installing. It may be blocked or in a stuck state. For further information please refer to: {manualFixLink}");
						}
						Thread.Sleep(5000);

						var updateNode = targetRegistry.OpenSubKey(UpgradeCodePath + encryptedUpgradeCode);
						var encryptProductCode = updateNode?.GetValueNames();
						var productCode = DecryptProductCode(encryptProductCode?.FirstOrDefault());

						var productNode = targetRegistry.OpenSubKey(ProductCodePath + productCode);
						currentlyInstalledVersion = productNode?.GetValue("DisplayVersion")?.ToString();
						if (currentlyInstalledVersion is null)
						{
							var wOW64ProductNode = targetRegistry.OpenSubKey(WOW64ProductCodePath + productCode);
							currentlyInstalledVersion = wOW64ProductNode?.GetValue("DisplayVersion")?.ToString();
						}
					} while (latestVersion != currentlyInstalledVersion);
				}

				return InstallationResult.OK();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return InstallationResult.Error("Installation failed because of exception: " + ex.ToString());
			}
		}

		public static string DecryptProductCode(string input)
		{
			static string GetReversedValue(string inputValue)
			{
				char[] charArray = inputValue.ToCharArray();
				Array.Reverse(charArray);
				return new string(charArray);
			}

			if (string.IsNullOrEmpty(input))
			{
				return null;
			}

			return new System.Text.StringBuilder()
				.Append("{")
				.Append(GetReversedValue(input.Substring(0, 8)))
				.Append("-")
				.Append(GetReversedValue(input.Substring(8, 4)))
				.Append("-")
				.Append(GetReversedValue(input.Substring(12, 4)))
				.Append("-")
				.Append(GetReversedValue(input.Substring(16, 2)))
				.Append(GetReversedValue(input.Substring(18, 2)))
				.Append("-")
				.Append(GetReversedValue(input.Substring(20, 2)))
				.Append(GetReversedValue(input.Substring(22, 2)))
				.Append(GetReversedValue(input.Substring(24, 2)))
				.Append(GetReversedValue(input.Substring(26, 2)))
				.Append(GetReversedValue(input.Substring(28, 2)))
				.Append(GetReversedValue(input.Substring(30, 2)))
				.Append("}")
				.ToString();
		}
	}
}
