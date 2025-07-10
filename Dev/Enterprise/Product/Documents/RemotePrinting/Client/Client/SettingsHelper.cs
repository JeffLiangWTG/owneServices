using Enterprise.RemotePrinting.Client.Properties;

namespace Enterprise.RemotePrinting.Client
{
	public static class SettingsHelper
	{
		public static bool AutoStart => Settings.Default.AutoStart;

		public static void SaveAutoStart(bool value)
		{
			Settings.Default.AutoStart = value;
			Settings.Default.Save();
		}
	}
}
