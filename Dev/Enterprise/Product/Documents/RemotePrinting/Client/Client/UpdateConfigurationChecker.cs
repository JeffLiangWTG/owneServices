using System;
using System.Globalization;

namespace Enterprise.RemotePrinting.Client
{
	public class UpdateConfigurationChecker
	{
		readonly IUpdateConfigurationProvider updateConfigurationProvider;
		WebClientUpdateConfiguration updateConfiguration;

		public UpdateConfigurationChecker(IUpdateConfigurationProvider updateConfigurationProvider)
		{
			this.updateConfigurationProvider = updateConfigurationProvider;
			this.updateConfiguration = updateConfigurationProvider?.UpdateConfiguration ?? new WebClientUpdateConfiguration { Mode = WebClientUpdateConfiguration.UpdateMode.Automatic };
		}

		public bool CanUpdate(Version installedVersion, Version newVersion)
		{
			if (installedVersion >= newVersion)
			{
				ResetTrackingDays();
				return false;
			}

			if (updateConfiguration.Mode == WebClientUpdateConfiguration.UpdateMode.Automatic)
			{
				return CanUpdateAutomatic(installedVersion, newVersion);
			}

			if (updateConfiguration.Mode == WebClientUpdateConfiguration.UpdateMode.Manual)
			{
				return CanUpdateManual(installedVersion, newVersion);
			}

			if (updateConfiguration.Mode == WebClientUpdateConfiguration.UpdateMode.Custom)
			{
				return CanUpdateCustom(installedVersion, newVersion);
			}

			return true;
		}

		bool CanUpdateManual(Version installedVersion, Version newVersion)
		{
			SendDailyNotificationAboutNewVersion(installedVersion, newVersion);
			return false;
		}

		bool CanUpdateCustom(Version installedVersion, Version newVersion)
		{
			var requiresUpdate = SatisfiesVersionConditions(installedVersion, newVersion) || RequiresUpdateAfterNDays();

			if (!requiresUpdate)
			{
				SendDailyNotificationAboutNewVersion(installedVersion, newVersion);
			}

			var result = requiresUpdate && SatisfiesTimeConditions() && SatisfiesDaysOfWeekConditions();

			return result;
		}

		bool CanUpdateAutomatic(Version installedVersion, Version newVersion)
		{
			if (!string.IsNullOrEmpty(updateConfiguration.VersionBeforeUpdate))
			{
				var previousVersion = new Version(updateConfiguration.VersionBeforeUpdate);
				var hours = (DateTime.Now - updateConfiguration.UpdateRunningDate).TotalHours;
				if (previousVersion == installedVersion && hours < updateConfiguration.PauseAutomaticUpdateHours)
				{
					LogWriter.Append(WebPrintEventLogEntryType.SystemInformation, $"Automatic update not running because previous update failed {Math.Round(hours, 2)} hours ago.");
					SendNotificationAfterAutomaticUpdateFailed(newVersion);
					return false;
				}
			}
			return true;
		}

		bool SatisfiesVersionConditions(Version installedVersion, Version newVersion)
		{
			return (updateConfiguration.AutomaticUpdateToMajorVersion && IsNewMajorVersion(installedVersion, newVersion)) ||
				(updateConfiguration.AutomaticUpdateToMinorVersion && !IsNewMajorVersion(installedVersion, newVersion));
		}

		bool IsNewMajorVersion(Version installedVersion, Version newVersion)
		{
			return newVersion.Major > installedVersion.Major ||
				(newVersion.Major == installedVersion.Major && newVersion.MajorRevision > installedVersion.MajorRevision);
		}

		bool RequiresUpdateAfterNDays()
		{
			if (updateConfiguration.ForceAutomaticUpdateAfterNDays <= 0)
			{
				return false;
			}

			if (updateConfiguration.NewUpdateAppearedDate.Year < WebClientUpdateConfiguration.MinNotEmptyYear)
			{
				updateConfiguration.NewUpdateAppearedDate = DateTime.Now.Date;
				updateConfigurationProvider.SaveUpdateTrackingConfiguration(updateConfiguration);
			}

			return (DateTime.Now.Date - updateConfiguration.NewUpdateAppearedDate).TotalDays >= updateConfiguration.ForceAutomaticUpdateAfterNDays;
		}

		bool SatisfiesTimeConditions()
		{
			var nowSeconds = TimeSeconds(DateTime.Now);
			var fromSeconds = TimeSeconds(updateConfiguration.UpdateAllowedTimeFrom);
			var toSeconds = TimeSeconds(updateConfiguration.UpdateAllowedTimeTo);

			var fromIsEmpty = IsEmptyTime(updateConfiguration.UpdateAllowedTimeFrom);
			var toIsEmpty = IsEmptyTime(updateConfiguration.UpdateAllowedTimeTo);

			var isAboveFrom = fromIsEmpty || (nowSeconds >= fromSeconds);
			var isBelowTo = toIsEmpty || (nowSeconds <= toSeconds);

			return (fromIsEmpty && toIsEmpty)
				|| (fromIsEmpty && isBelowTo)
				|| (toIsEmpty && isAboveFrom)
				|| ((fromSeconds > toSeconds) && (isAboveFrom || isBelowTo)) // overnight
				|| (isAboveFrom && isBelowTo);
		}

		double TimeSeconds(DateTime time)
		{
			return IsEmptyTime(time) ? 0.0 : new TimeSpan(time.Hour, time.Minute, time.Second).TotalSeconds;
		}

		bool IsEmptyTime(DateTime time)
		{
			return time.Hour == 0 && time.Minute == 0 && time.Second == 0;
		}

		bool SatisfiesDaysOfWeekConditions()
		{
			return SatisfiesDaysOfWeekConditions(DateTime.Now.DayOfWeek);
		}

#if DEBUG
		protected
#endif
		bool SatisfiesDaysOfWeekConditions(DayOfWeek dayOfWeek)
		{
			return (updateConfiguration.UpdateAllowedOnMonday && (dayOfWeek == DayOfWeek.Monday))
				|| (updateConfiguration.UpdateAllowedOnTuesday && (dayOfWeek == DayOfWeek.Tuesday))
				|| (updateConfiguration.UpdateAllowedOnWednesday && (dayOfWeek == DayOfWeek.Wednesday))
				|| (updateConfiguration.UpdateAllowedOnThursday && (dayOfWeek == DayOfWeek.Thursday))
				|| (updateConfiguration.UpdateAllowedOnFriday && (dayOfWeek == DayOfWeek.Friday))
				|| (updateConfiguration.UpdateAllowedOnSaturday && (dayOfWeek == DayOfWeek.Saturday))
				|| (updateConfiguration.UpdateAllowedOnSunday && (dayOfWeek == DayOfWeek.Sunday));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Message")]
		void SendDailyNotificationAboutNewVersion(Version installedVersion, Version newVersion)
		{
			if (!updateConfiguration.SendDailyNotificationAboutNewVersion)
			{
				return;
			}

			var today = DateTime.Today;
			if ((today - updateConfiguration.NewUpdateLastNotificationDate.Date).TotalDays >= 1)
			{
				updateConfiguration.NewUpdateLastNotificationDate = today;
				updateConfigurationProvider.SaveUpdateTrackingConfiguration(updateConfiguration);

				var message = string.Format(CultureInfo.CurrentCulture,
					"New Remote Printing Client update version {0} is available for print server {1}.\r\nCurrent installed version is {2}.\r\nAutomatic update is disabled, please do a manual update.",
					newVersion, updateConfigurationProvider.MachineName, installedVersion);

				if ((updateConfiguration.Mode == WebClientUpdateConfiguration.UpdateMode.Custom) &&
					(updateConfiguration.ForceAutomaticUpdateAfterNDays > 0))
				{
					var daysToUpdate = updateConfiguration.ForceAutomaticUpdateAfterNDays - (int)(DateTime.Now.Date - updateConfiguration.NewUpdateAppearedDate).TotalDays;
					if (daysToUpdate <= 0)
					{
						daysToUpdate = 0;
					}

					message += string.Format(CultureInfo.CurrentCulture,
						"\r\n\r\nRemote Printing Client will be forcefully automatically updated in {0} days",
						daysToUpdate);
				}

				updateConfigurationProvider.SendNotification(message);
			}
		}

		void SendNotificationAfterAutomaticUpdateFailed(Version newVersion)
		{
			if (updateConfiguration.NewUpdateLastNotificationDate < updateConfiguration.UpdateRunningDate)
			{
				updateConfiguration.NewUpdateLastNotificationDate = DateTime.Now;
				updateConfigurationProvider.SaveUpdateTrackingConfiguration(updateConfiguration);

				var message = string.Format(CultureInfo.CurrentCulture,
					@"New Remote Printing Client update version {0} is available for print server {1}.
Automatic update will be suspended for {2} hours because previous automatic upgrade failed, please check errors in installation log files.
Please do a manual update.",
					newVersion, updateConfigurationProvider.MachineName, updateConfiguration.PauseAutomaticUpdateHours);
				updateConfigurationProvider.SendNotification(message);
			}
		}

		void ResetTrackingDays()
		{
			if ((updateConfiguration.NewUpdateAppearedDate.Year >= WebClientUpdateConfiguration.MinNotEmptyYear) ||
				(updateConfiguration.NewUpdateLastNotificationDate.Year >= WebClientUpdateConfiguration.MinNotEmptyYear) ||
				!string.IsNullOrEmpty(updateConfiguration.VersionBeforeUpdate))
			{
				updateConfiguration.NewUpdateAppearedDate = WebClientUpdateConfiguration.EmptyDate;
				updateConfiguration.NewUpdateLastNotificationDate = WebClientUpdateConfiguration.EmptyDate;
				updateConfiguration.VersionBeforeUpdate = string.Empty;
				updateConfiguration.UpdateRunningDate = WebClientUpdateConfiguration.EmptyDate;

				updateConfigurationProvider.SaveUpdateTrackingConfiguration(updateConfiguration);
			}
		}
	}
}
