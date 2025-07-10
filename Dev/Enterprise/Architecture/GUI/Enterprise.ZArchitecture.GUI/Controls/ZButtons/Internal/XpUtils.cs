using CargoWise.Common;
using CargoWise.Common.Testing;
using Microsoft.Win32;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	public static class XpUtils
	{
		static XpUtils()
		{
			CheckWindowsTheme();
			SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(SystemEvents_UserPreferenceChanged);
		}

		// TODO : Figure out how to implement this.
		public static bool IsAppUsingXpTheme
		{
			get { return true; }
		}

		public static bool IsWindowsXpOrGreater
		{
			get { return new VersionHelper().IsWindowsXPOrGreater(); }
		}

		public static bool IsWindowsUsingXpTheme
		{
			get { return isWindowsUsingXpTheme; }
		}
		[SuppressThreadStaticFieldMessage]
		static bool isWindowsUsingXpTheme;

		static void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
		{
			if (e.Category == UserPreferenceCategory.Window)
			{
				CheckWindowsTheme();
			}
		}

		static void CheckWindowsTheme()
		{
			isWindowsUsingXpTheme = IsWindowsXpOrGreater && (XpThemeAPI.IsThemeActive() == 1);
		}
	}
}