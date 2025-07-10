using System;

namespace Enterprise.RemotePrinting.Client
{
	public struct WebClientUpdateConfiguration
	{
		public enum UpdateMode
		{
			Automatic,
			Manual,
			Custom
		}

		[Flags]
		public enum DaysOfWeek
		{
			None = 0,
			Sunday = 1,
			Monday = 2,
			Tuesday = 4,
			Wednesday = 8,
			Thursday = 16,
			Friday = 32,
			Saturday = 64,
			All = Sunday | Monday | Tuesday | Wednesday | Thursday | Friday | Saturday
		}

		public UpdateMode Mode;

		// Manual and Custom update options
		public bool SendDailyNotificationAboutNewVersion;

		// Custom update options
		public bool AutomaticUpdateToMajorVersion;
		public bool AutomaticUpdateToMinorVersion;
		public int ForceAutomaticUpdateAfterNDays;
		public DateTime UpdateAllowedTimeFrom;
		public DateTime UpdateAllowedTimeTo;
		public DaysOfWeek UpdateAllowedDaysOfWeek;
		public bool NotifyBeforeUpdate;
		public bool NotifyAfterUpdate;

		public string VersionBeforeUpdate;
		public DateTime UpdateRunningDate;
		public int PauseAutomaticUpdateHours;
		public DateTime NewUpdateAppearedDate;
		public DateTime NewUpdateLastNotificationDate;
		public DateTime UpdateWithNotMatchingLastNotificationDate;

		public WebClientUpdateConfiguration(
			UpdateMode mode,
			bool sendDailyNotificationAboutNewVersion,
			bool automaticUpdateToMajorVersion,
			bool automaticUpdateToMinorVersion,
			int forceAutomaticUpdateAfterNDays,
			DateTime updateAllowedTimeFrom,
			DateTime updateAllowedTimeTo,
			DaysOfWeek updateAllowedDaysOfWeek,
			bool notifyBeforeUpdate,
			bool notifyAfterUpdate,
			int pauseAutomaticUpdateHours
		)
		{
			Mode = mode;
			SendDailyNotificationAboutNewVersion = sendDailyNotificationAboutNewVersion;
			AutomaticUpdateToMajorVersion = automaticUpdateToMajorVersion;
			AutomaticUpdateToMinorVersion = automaticUpdateToMinorVersion;
			ForceAutomaticUpdateAfterNDays = forceAutomaticUpdateAfterNDays;
			UpdateAllowedTimeFrom = MakeDateSafeIfNeeded(updateAllowedTimeFrom);
			UpdateAllowedTimeTo = MakeDateSafeIfNeeded(updateAllowedTimeTo);
			UpdateAllowedDaysOfWeek = updateAllowedDaysOfWeek;
			NotifyBeforeUpdate = notifyBeforeUpdate;
			NotifyAfterUpdate = notifyAfterUpdate;

			VersionBeforeUpdate = string.Empty;
			UpdateRunningDate = EmptyDate;
			PauseAutomaticUpdateHours = pauseAutomaticUpdateHours;
			NewUpdateAppearedDate = EmptyDate;
			NewUpdateLastNotificationDate = EmptyDate;
			UpdateWithNotMatchingLastNotificationDate = EmptyDate;
		}

		public const int MinNotEmptyYear = 2001;
		public static DateTime EmptyDate { get; } = new DateTime(MinNotEmptyYear - 1, 1, 1);

		public DateTime UpdateAllowedTimeFromSafe
		{
			get => MakeDateSafeIfNeeded(UpdateAllowedTimeFrom);
			set => UpdateAllowedTimeFrom = MakeDateSafeIfNeeded(value);
		}

		public DateTime UpdateAllowedTimeToSafe
		{
			get => MakeDateSafeIfNeeded(UpdateAllowedTimeTo);
			set => UpdateAllowedTimeTo = MakeDateSafeIfNeeded(value);
		}

		/// <remarks>Time only will have value 0001-01-01 HH:mm:ss, but datetime picker control in UI does not allow date less then 17xx year something.</remarks>
		static DateTime MakeDateSafeIfNeeded(DateTime time)
		{
			if (time.Year < MinNotEmptyYear)
			{
				time = DateTime.Today + time.TimeOfDay;
			}
			return time;
		}

		#region Allowed Days Of Week

		bool GetUpdateAllowedOnDayOfWeek(DaysOfWeek dayOfWeek) => (UpdateAllowedDaysOfWeek & dayOfWeek) == dayOfWeek;

		void SetUpdateAllowedOnDayOfWeek(DaysOfWeek dayOfWeek, bool allowed)
		{
			if (allowed)
			{
				UpdateAllowedDaysOfWeek |= dayOfWeek;
			}
			else
			{
				UpdateAllowedDaysOfWeek &= ~dayOfWeek;
			}
		}

		public bool UpdateAllowedOnSunday
		{
			get => GetUpdateAllowedOnDayOfWeek(DaysOfWeek.Sunday);
			set => SetUpdateAllowedOnDayOfWeek(DaysOfWeek.Sunday, value);
		}

		public bool UpdateAllowedOnMonday
		{
			get => GetUpdateAllowedOnDayOfWeek(DaysOfWeek.Monday);
			set => SetUpdateAllowedOnDayOfWeek(DaysOfWeek.Monday, value);
		}

		public bool UpdateAllowedOnTuesday
		{
			get => GetUpdateAllowedOnDayOfWeek(DaysOfWeek.Tuesday);
			set => SetUpdateAllowedOnDayOfWeek(DaysOfWeek.Tuesday, value);
		}

		public bool UpdateAllowedOnWednesday
		{
			get => GetUpdateAllowedOnDayOfWeek(DaysOfWeek.Wednesday);
			set => SetUpdateAllowedOnDayOfWeek(DaysOfWeek.Wednesday, value);
		}

		public bool UpdateAllowedOnThursday
		{
			get => GetUpdateAllowedOnDayOfWeek(DaysOfWeek.Thursday);
			set => SetUpdateAllowedOnDayOfWeek(DaysOfWeek.Thursday, value);
		}

		public bool UpdateAllowedOnFriday
		{
			get => GetUpdateAllowedOnDayOfWeek(DaysOfWeek.Friday);
			set => SetUpdateAllowedOnDayOfWeek(DaysOfWeek.Friday, value);
		}

		public bool UpdateAllowedOnSaturday
		{
			get => GetUpdateAllowedOnDayOfWeek(DaysOfWeek.Saturday);
			set => SetUpdateAllowedOnDayOfWeek(DaysOfWeek.Saturday, value);
		}

		#endregion
	}
}
