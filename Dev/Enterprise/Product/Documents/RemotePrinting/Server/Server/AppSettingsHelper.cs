using System.Configuration;

namespace Enterprise.RemotePrinting.Server
{
	public static class AppSettingsHelper
	{
		public static class Keys
		{
			public const string ClientInstallationFile = "ClientInstallationFile";
			public const string ClientInstallationFolder = "ClientInstallationFolder";
			public const string ClientRequirementOS = "ClientRequirementOS";
			public const string ClientRequirementDotnet = "ClientRequirementDotnet";
			public const string ClientIntermediateInstallationFile = "ClientIntermediateInstallationFile";
		}

		public static string ClientInstallationFile => GetSetting(Keys.ClientInstallationFile);

		public static string ClientInstallationFolder => GetSetting(Keys.ClientInstallationFolder);

		public static string ClientRequirementOS => GetSetting(Keys.ClientRequirementOS);

		public static string ClientRequirementDotnet => GetSetting(Keys.ClientRequirementDotnet);

		public static string ClientIntermediateInstallationFile => GetSetting(Keys.ClientIntermediateInstallationFile);

		public static string GetSetting(string key)
		{
			return ConfigurationManager.AppSettings[key];
		}
	}
}