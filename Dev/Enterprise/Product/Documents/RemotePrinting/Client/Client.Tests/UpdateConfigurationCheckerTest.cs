using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	public class UpdateConfigurationCheckerTest : TestCase
	{
		public void TestCanUpdate_ShouldPauseUpgardeTemporarilyIfPreviousUpgradeFailed()
		{
			var directoryPathForTest = new DirectoryInfo(Path.Combine(Constants.WebPrintClientDataPath, "LogsForTest"));
			const string updateLogFileName = "LogFileForTestUpdate.txt";
			try
			{
				var fileTarget = LogWriter.RegisterFileTarget(directoryPathForTest.ToString(), "LogForTest", WebPrintEventLogEntryType.Information, WebPrintEventLogEntryType.Error, "*", 500);
				fileTarget.FileName = new FileInfo(Path.Combine(directoryPathForTest.ToString(), updateLogFileName)).FullName;
				fileTarget.MaxArchiveFiles = 2;

				var provider = new UpdateConfigurationProviderForTest
				{
					UpdateConfiguration = new WebClientUpdateConfiguration
					{
						Mode = WebClientUpdateConfiguration.UpdateMode.Automatic,
						VersionBeforeUpdate = "1.0",
						PauseAutomaticUpdateHours = 24,
						UpdateRunningDate = DateTime.Now.AddHours(-1)
					}
				};
				var checker = new UpdateConfigurationChecker(provider);

				var expectedMessage = @"New Remote Printing Client update version 2.0 is available for print server M1.
Automatic update will be suspended for 24 hours because previous automatic upgrade failed, please check errors in installation log files.
Please do a manual update.";
				var result = checker.CanUpdate(new Version(1, 0), new Version(2, 0));
				CombineAssertions("The first time to require update", () =>
				{
					AssertEquals("Should pause upgrade", false, result);
					AssertEquals("Should send notification", 1, provider.Notifications.Count);
					AssertStartsWith("Should send correct message", expectedMessage, provider.Notifications[0]);
				});

				CombineAssertions("The second time to require update", () =>
				{
					AssertEquals("Should still pause upgrade", false, checker.CanUpdate(new Version(1, 0), new Version(2, 0)));
					AssertEquals("Should send notification one time", 1, provider.Notifications.Count);
					AssertStartsWith("Should send correct message", expectedMessage, provider.Notifications[0]);
				});

				provider = new UpdateConfigurationProviderForTest
				{
					UpdateConfiguration = new WebClientUpdateConfiguration
					{
						Mode = WebClientUpdateConfiguration.UpdateMode.Automatic,
						VersionBeforeUpdate = "1.0",
						PauseAutomaticUpdateHours = 24,
						UpdateRunningDate = DateTime.Now.AddHours(-25)
					}
				};
				checker = new UpdateConfigurationChecker(provider);
				CombineAssertions("After 24 hours", () =>
				{
					AssertEquals("Should upgrade", true, checker.CanUpdate(new Version(1, 0), new Version(2, 0)));
					AssertEquals("Should no notification ", 0, provider.Notifications.Count);
				});

				var fileName = directoryPathForTest.GetFiles(updateLogFileName).First().FullName;
				using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (var stream = new StreamReader(fileStream))
				{
					var log = stream.ReadToEnd();
					AssertContains("Should log anto update cannot run message", "Automatic update not running because previous update failed", log);
				}
			}
			finally
			{
				LogWriter.UnregisterAllLogTargets();
				if (directoryPathForTest.Exists)
				{
					foreach (var tempfile in directoryPathForTest.EnumerateFiles())
					{
						File.Delete(tempfile.FullName);
					}
					directoryPathForTest.Delete();
				}
			}
		}

		public void TestCanUpdate_ResetTrackingDays()
		{
			var provider = new UpdateConfigurationProviderForTest
			{
				UpdateConfiguration = new WebClientUpdateConfiguration
				{
					Mode = WebClientUpdateConfiguration.UpdateMode.Automatic,
					VersionBeforeUpdate = "1.0",
					PauseAutomaticUpdateHours = 24,
					UpdateRunningDate = DateTime.Now.AddHours(-1)
				}
			};
			var checker = new UpdateConfigurationChecker(provider);
			CombineAssertions("Cannot update to older version", () =>
			{
				AssertEquals("Cannot update", false, checker.CanUpdate(new Version(2, 0), new Version(1, 0)));
				AssertEquals("Should clean VersionBeforeUpdate", string.Empty, provider.SavedUpdateConfiguration.VersionBeforeUpdate);
				AssertEquals("Should reset NewUpdateAppearedDate", WebClientUpdateConfiguration.EmptyDate, provider.SavedUpdateConfiguration.NewUpdateAppearedDate);
				AssertEquals("Should reset NewUpdateLastNotificationDate", WebClientUpdateConfiguration.EmptyDate, provider.SavedUpdateConfiguration.NewUpdateLastNotificationDate);
				AssertEquals("Should reset UpdateRunningDate", WebClientUpdateConfiguration.EmptyDate, provider.SavedUpdateConfiguration.UpdateRunningDate);
			});
		}

		public void TestCanUpdate_OlderVersion()
		{
			var checker = new UpdateConfigurationChecker(new UpdateConfigurationProviderForTest());
			Assert("Cannot update to older version", !checker.CanUpdate(new Version(2, 0), new Version(1, 0)));
		}

		public void TestCanUpdate_ManualMode()
		{
			var config = new WebClientUpdateConfiguration
			{
				Mode = WebClientUpdateConfiguration.UpdateMode.Manual,
				SendDailyNotificationAboutNewVersion = false,
				NewUpdateLastNotificationDate = DateTime.Now
			};
			var configProvider = new UpdateConfigurationProviderForTest { UpdateConfiguration = config };
			var checker = new UpdateConfigurationChecker(configProvider);

			Assert("Cannot update when in manual mode", !checker.CanUpdate(new Version(1, 0), new Version(2, 0)));
			AssertEquals("No notifications should be added", 0, configProvider.Notifications.Count);

			config.SendDailyNotificationAboutNewVersion = true;
			configProvider.UpdateConfiguration = config;
			checker = new UpdateConfigurationChecker(configProvider);

			Assert("Cannot update when in manual mode", !checker.CanUpdate(new Version(1, 0), new Version(2, 0)));
			AssertEquals("No notifications should be added", 0, configProvider.Notifications.Count);

			config.NewUpdateLastNotificationDate = DateTime.Now.AddHours(-25);
			configProvider.UpdateConfiguration = config;
			checker = new UpdateConfigurationChecker(configProvider);

			Assert("Cannot update when in manual mode", !checker.CanUpdate(new Version(1, 0), new Version(2, 0)));
			AssertEquals("Should add daily notification", 1, configProvider.Notifications.Count);
			AssertStartsWith("Should add correct message", "New Remote Printing Client update version 2.0 is available for print server M1.", configProvider.Notifications[0]);
			Assert("Should save updated configuration", configProvider.SaveWasInvoked);
		}

		public void TestCanUpdate_AutoMode()
		{
			var config = new WebClientUpdateConfiguration
			{
				Mode = WebClientUpdateConfiguration.UpdateMode.Automatic
			};
			var configProvider = new UpdateConfigurationProviderForTest { UpdateConfiguration = config };
			var checker = new UpdateConfigurationChecker(configProvider);

			Assert("Should be able to update in Auto mode", checker.CanUpdate(new Version(1, 0), new Version(2, 0)));
			AssertEquals("No notifications should be added", 0, configProvider.Notifications.Count);
		}

		public void TestSatisfiesDaysOfWeekConditions()
		{
			AssertSatisfiesDaysOfWeekConditions(WebClientUpdateConfiguration.DaysOfWeek.All, true, true, true, true, true, true, true);
			AssertSatisfiesDaysOfWeekConditions(WebClientUpdateConfiguration.DaysOfWeek.Monday, true, false, false, false, false, false, false);
			AssertSatisfiesDaysOfWeekConditions(WebClientUpdateConfiguration.DaysOfWeek.Tuesday, false, true, false, false, false, false, false);
			AssertSatisfiesDaysOfWeekConditions(WebClientUpdateConfiguration.DaysOfWeek.Wednesday, false, false, true, false, false, false, false);
			AssertSatisfiesDaysOfWeekConditions(WebClientUpdateConfiguration.DaysOfWeek.Thursday, false, false, false, true, false, false, false);
			AssertSatisfiesDaysOfWeekConditions(WebClientUpdateConfiguration.DaysOfWeek.Friday, false, false, false, false, true, false, false);
			AssertSatisfiesDaysOfWeekConditions(WebClientUpdateConfiguration.DaysOfWeek.Saturday, false, false, false, false, false, true, false);
			AssertSatisfiesDaysOfWeekConditions(WebClientUpdateConfiguration.DaysOfWeek.Sunday, false, false, false, false, false, false, true);
			AssertSatisfiesDaysOfWeekConditions(WebClientUpdateConfiguration.DaysOfWeek.None, false, false, false, false, false, false, false);
			AssertSatisfiesDaysOfWeekConditions(WebClientUpdateConfiguration.DaysOfWeek.Tuesday | WebClientUpdateConfiguration.DaysOfWeek.Thursday, false, true, false, true, false, false, false);
			AssertSatisfiesDaysOfWeekConditions(WebClientUpdateConfiguration.DaysOfWeek.Saturday | WebClientUpdateConfiguration.DaysOfWeek.Sunday, false, false, false, false, false, true, true);
		}

		public void AssertSatisfiesDaysOfWeekConditions(WebClientUpdateConfiguration.DaysOfWeek configDaysOfWeek,
			bool allowMonday, bool allowTuesday, bool allowWednesday, bool allowThursday, bool allowFriday, bool allowSaturday, bool allowSunday)
		{
			var config = new WebClientUpdateConfiguration
			{
				Mode = WebClientUpdateConfiguration.UpdateMode.Custom,
				SendDailyNotificationAboutNewVersion = false,
				AutomaticUpdateToMajorVersion = true,
				AutomaticUpdateToMinorVersion = true,
				ForceAutomaticUpdateAfterNDays = 0,
				UpdateAllowedTimeFrom = new DateTime(2020, 1, 1, 0, 0, 0),
				UpdateAllowedTimeTo = new DateTime(2020, 1, 1, 0, 0, 0),
				UpdateAllowedDaysOfWeek = configDaysOfWeek,
				NotifyBeforeUpdate = false,
				NotifyAfterUpdate = false
			};
			var configProvider = new UpdateConfigurationProviderForTest { UpdateConfiguration = config };
			var checker = new UpdateConfigurationCheckerForTest(configProvider);

			AssertEquals("SatisfiesDaysOfWeekConditions - Monday", allowMonday, checker.SatisfiesDaysOfWeekConditions_Exposed(DayOfWeek.Monday));
			AssertEquals("SatisfiesDaysOfWeekConditions - Tuesday", allowTuesday, checker.SatisfiesDaysOfWeekConditions_Exposed(DayOfWeek.Tuesday));
			AssertEquals("SatisfiesDaysOfWeekConditions - Wednesday", allowWednesday, checker.SatisfiesDaysOfWeekConditions_Exposed(DayOfWeek.Wednesday));
			AssertEquals("SatisfiesDaysOfWeekConditions - Thursday", allowThursday, checker.SatisfiesDaysOfWeekConditions_Exposed(DayOfWeek.Thursday));
			AssertEquals("SatisfiesDaysOfWeekConditions - Friday", allowFriday, checker.SatisfiesDaysOfWeekConditions_Exposed(DayOfWeek.Friday));
			AssertEquals("SatisfiesDaysOfWeekConditions - Saturday", allowSaturday, checker.SatisfiesDaysOfWeekConditions_Exposed(DayOfWeek.Saturday));
			AssertEquals("SatisfiesDaysOfWeekConditions - Sunday", allowSunday, checker.SatisfiesDaysOfWeekConditions_Exposed(DayOfWeek.Sunday));
		}

		public void TestCanUpdate_MajorUpdate()
		{
			var config = new WebClientUpdateConfiguration
			{
				Mode = WebClientUpdateConfiguration.UpdateMode.Custom,
				SendDailyNotificationAboutNewVersion = false,
				AutomaticUpdateToMajorVersion = true,
				AutomaticUpdateToMinorVersion = false,
				ForceAutomaticUpdateAfterNDays = 0,
				UpdateAllowedTimeFrom = new DateTime(2020, 1, 1, 0, 0, 0),
				UpdateAllowedTimeTo = new DateTime(2020, 1, 1, 0, 0, 0),
				UpdateAllowedDaysOfWeek = WebClientUpdateConfiguration.DaysOfWeek.All,
				NotifyBeforeUpdate = false,
				NotifyAfterUpdate = false
			};
			var configProvider = new UpdateConfigurationProviderForTest { UpdateConfiguration = config };
			var checker = new UpdateConfigurationChecker(configProvider);

			Assert("Cannot update to minor version", !checker.CanUpdate(new Version(1, 0), new Version(1, 1)));
			Assert("Should be able to update to major version", checker.CanUpdate(new Version(1, 0), new Version(2, 0)));
		}

		public void TestCanUpdate_MinorUpdate()
		{
			var config = new WebClientUpdateConfiguration
			{
				Mode = WebClientUpdateConfiguration.UpdateMode.Custom,
				SendDailyNotificationAboutNewVersion = false,
				AutomaticUpdateToMajorVersion = false,
				AutomaticUpdateToMinorVersion = true,
				ForceAutomaticUpdateAfterNDays = 0,
				UpdateAllowedTimeFrom = new DateTime(2020, 1, 1, 0, 0, 0),
				UpdateAllowedTimeTo = new DateTime(2020, 1, 1, 0, 0, 0),
				UpdateAllowedDaysOfWeek = WebClientUpdateConfiguration.DaysOfWeek.All,
				NotifyBeforeUpdate = false,
				NotifyAfterUpdate = false
			};
			var configProvider = new UpdateConfigurationProviderForTest { UpdateConfiguration = config };
			var checker = new UpdateConfigurationChecker(configProvider);

			Assert("Should be able to update to minor version", checker.CanUpdate(new Version(1, 0), new Version(1, 1)));
			Assert("Cannot update to major version", !checker.CanUpdate(new Version(1, 0), new Version(2, 0)));
		}

		public void TestCanUpdate_AfterNDays()
		{
			var config = new WebClientUpdateConfiguration
			{
				Mode = WebClientUpdateConfiguration.UpdateMode.Custom,
				SendDailyNotificationAboutNewVersion = false,
				AutomaticUpdateToMajorVersion = false,
				AutomaticUpdateToMinorVersion = false,
				ForceAutomaticUpdateAfterNDays = 3,
				UpdateAllowedTimeFrom = new DateTime(2020, 1, 1, 0, 0, 0),
				UpdateAllowedTimeTo = new DateTime(2020, 1, 1, 0, 0, 0),
				UpdateAllowedDaysOfWeek = WebClientUpdateConfiguration.DaysOfWeek.All,
				NotifyBeforeUpdate = false,
				NotifyAfterUpdate = false,

				NewUpdateAppearedDate = DateTime.Today
			};
			var configProvider = new UpdateConfigurationProviderForTest { UpdateConfiguration = config };
			var checker = new UpdateConfigurationChecker(configProvider);

			Assert("Cannot update to new version", !checker.CanUpdate(new Version(1, 0), new Version(2, 0)));

			config.NewUpdateAppearedDate = DateTime.Today.AddDays(-3);
			configProvider.UpdateConfiguration = config;
			checker = new UpdateConfigurationChecker(configProvider);

			Assert("Should be able to update after N days", checker.CanUpdate(new Version(1, 0), new Version(2, 0)));
		}

		public void TestCanUpdate_DayOfWeek()
		{
			var config = new WebClientUpdateConfiguration
			{
				Mode = WebClientUpdateConfiguration.UpdateMode.Custom,
				SendDailyNotificationAboutNewVersion = false,
				AutomaticUpdateToMajorVersion = true,
				AutomaticUpdateToMinorVersion = true,
				ForceAutomaticUpdateAfterNDays = 0,
				UpdateAllowedTimeFrom = new DateTime(2020, 1, 1, 0, 0, 0),
				UpdateAllowedTimeTo = new DateTime(2020, 1, 1, 0, 0, 0),
				UpdateAllowedDaysOfWeek = DateTime.Today.DayOfWeek == DayOfWeek.Friday ? WebClientUpdateConfiguration.DaysOfWeek.Tuesday : WebClientUpdateConfiguration.DaysOfWeek.Friday,
				NotifyBeforeUpdate = false,
				NotifyAfterUpdate = false
			};
			var configProvider = new UpdateConfigurationProviderForTest { UpdateConfiguration = config };
			var checker = new UpdateConfigurationChecker(configProvider);

			Assert("Cannot update to new version", !checker.CanUpdate(new Version(1, 0), new Version(2, 0)));

			config.UpdateAllowedDaysOfWeek = WebClientUpdateConfiguration.DaysOfWeek.All;
			configProvider.UpdateConfiguration = config;
			checker = new UpdateConfigurationChecker(configProvider);

			Assert("Should be able to update on allowed day of week", checker.CanUpdate(new Version(1, 0), new Version(2, 0)));
		}

		public void TestCanUpdate_TimeOfDay()
		{
			var currentTime = DateTime.Now;

			var config = new WebClientUpdateConfiguration
			{
				Mode = WebClientUpdateConfiguration.UpdateMode.Custom,
				SendDailyNotificationAboutNewVersion = false,
				AutomaticUpdateToMajorVersion = true,
				AutomaticUpdateToMinorVersion = true,
				ForceAutomaticUpdateAfterNDays = 0,
				UpdateAllowedTimeFrom = currentTime.Hour > 12 ? new DateTime(2020, 1, 1, 5, 0, 0) : new DateTime(2020, 1, 1, 15, 0, 0),
				UpdateAllowedTimeTo = currentTime.Hour > 12 ? new DateTime(2020, 1, 1, 6, 0, 0) : new DateTime(2020, 1, 1, 16, 0, 0),
				UpdateAllowedDaysOfWeek = WebClientUpdateConfiguration.DaysOfWeek.All,
				NotifyBeforeUpdate = false,
				NotifyAfterUpdate = false
			};
			var configProvider = new UpdateConfigurationProviderForTest { UpdateConfiguration = config };
			var checker = new UpdateConfigurationChecker(configProvider);

			Assert("Cannot update to new version", !checker.CanUpdate(new Version(1, 0), new Version(2, 0)));

			config.UpdateAllowedTimeFrom = currentTime.AddHours(-1);
			config.UpdateAllowedTimeTo = currentTime.AddHours(1);
			configProvider.UpdateConfiguration = config;
			checker = new UpdateConfigurationChecker(configProvider);

			Assert("Should be able to update within allowed time of day", checker.CanUpdate(new Version(1, 0), new Version(2, 0)));
		}

		class UpdateConfigurationProviderForTest : IUpdateConfigurationProvider
		{
			public string MachineName => "M1";

			public WebClientConfiguration SystemConfiguration { get; set; }

			public WebClientUpdateConfiguration UpdateConfiguration { get; set; }

			public void SaveUpdateTrackingConfiguration(WebClientUpdateConfiguration updateConfiguration)
			{
				SaveWasInvoked = true;
				SavedUpdateConfiguration = updateConfiguration;
			}

			public bool SaveWasInvoked { get; set; }

			public WebClientUpdateConfiguration SavedUpdateConfiguration { get; set; }

			public void SendNotification(string message)
			{
				Notifications.Add(message);
			}

			public readonly List<string> Notifications = new List<string>();
		}

		class UpdateConfigurationCheckerForTest : UpdateConfigurationChecker
		{
			public UpdateConfigurationCheckerForTest(IUpdateConfigurationProvider updateConfigurationProvider)
				: base(updateConfigurationProvider)
			{
			}

			public bool SatisfiesDaysOfWeekConditions_Exposed(DayOfWeek dayOfWeek) => SatisfiesDaysOfWeekConditions(dayOfWeek);
		}
	}
}
