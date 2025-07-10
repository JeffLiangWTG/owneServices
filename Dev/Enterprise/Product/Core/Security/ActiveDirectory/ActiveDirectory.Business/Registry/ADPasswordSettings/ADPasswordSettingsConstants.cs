using System;

namespace Enterprise.Security.ActiveDirectory
{
	public static class ADPasswordSettingsConstants
	{
		public static class PasswordAge
		{
			public static readonly int Minimun = 1;
			public static readonly int MinimunRecommended = 1;
			public static readonly int Maximum = TimeSpan.MaxValue.Days;
			public static readonly int MaximumRecommended = 42;
		}

		public static class PasswordLength
		{
			public static readonly int Minimum = 1;
			public static readonly int Recommended = 7;
			public static readonly int Maximum = 255;
		}

		public static class PasswordHistoryLength
		{
			public static readonly int Minimum = 1;
			public static readonly int Recommended = 24;
			public static readonly int Maximum = 1_024;
		}

		public static class LockoutThreshold
		{
			public static readonly int Minimum = 1;
			public static readonly int Maximum = UInt16.MaxValue;
		}

		public static class LockoutObservationWindow
		{
			public static readonly int Minimum = 1;
			public static readonly int Recommended = 30;
		}

		public static class LockoutDuration
		{
			public static readonly int Minimum = 1;
			public static readonly int Recommended = 30;
		}
	}
}
