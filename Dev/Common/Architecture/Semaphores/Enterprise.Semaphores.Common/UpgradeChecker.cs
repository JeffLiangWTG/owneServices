using System;
using System.Diagnostics;
using System.ServiceModel;
using CargoWise.Data;
using CargoWise.Types;

namespace Enterprise.Semaphores.Common
{
	class UpgradeChecker : IUpgradeChecker
	{
		public UpgradeChecker()
		{
			upgradeCheckPulsed = Stopwatch.StartNew();
		}

		readonly Stopwatch upgradeCheckPulsed;

		public DateTime CheckForUpgrade(bool isForegroundThread, TimeSpan upgradeCheckDuration)
		{
			return !IsItTimeToCheckForUpgrade(upgradeCheckDuration)
				? DateTime.MinValue
				: ExecuteUpgradeCheck(isForegroundThread);
		}

		protected virtual DateTime ExecuteUpgradeCheck(bool isForegroundThread)
		{
			upgradeCheckPulsed.Restart();

			var connection = Db.Connection;
			var newConnection = false;

			if (!isForegroundThread)
			{
				connection = connection is AdminConnection ? Db.NewAdminConnection() : Db.NewExtraConnectionToMainDb();
				newConnection = true;
			}

			string forthcomingUpgPtyValue;
			try
			{
				forthcomingUpgPtyValue = DataUtils.LoadDbExtendedPropertyWithNoLock(connection, DataUtils.DateTimeForthcomingUpgradeExtPty);
			}
			catch (CommunicationException ex)
			{
				var errorMessage = $@"ExecuteUpgradeCheck Error:
IsNewConnection:{newConnection},
IsInTransaction:{connection?.IsInTransaction}
appdomain:{AppDomain.CurrentDomain.FriendlyName}
DB {connection?.CurrentDatabase}";
				throw new CommunicationException(errorMessage, ex);
			}
			finally
			{
				if (!isForegroundThread)
				{
					connection.Dispose();
				}
			}

			if (TryParseUpgradeDateTimeUtc(forthcomingUpgPtyValue, out var dateTimeUpgrade, out var dateTimeUpgradeRefreshed))
			{
				var utcNow = ZDateTime.UtcNow.ToDateTime();
				if (dateTimeUpgrade > utcNow && Math.Abs((utcNow - dateTimeUpgradeRefreshed).TotalMinutes) <= 3)
				{
					return DateTime.SpecifyKind(dateTimeUpgrade, DateTimeKind.Utc);
				}
			}

			return DateTime.MinValue;
		}

		protected virtual bool IsItTimeToCheckForUpgrade(TimeSpan upgradeCheckDuration)
		{
			return upgradeCheckPulsed.Elapsed >= GetStopwatchThreshold(upgradeCheckDuration);
		}

		static TimeSpan GetStopwatchThreshold(TimeSpan upgradeCheckDuration)
		{
			return TimeSpan.FromMilliseconds(upgradeCheckDuration.TotalMilliseconds * Heartbeat.DURATION_GUARANTEE_PERCENTAGE * 0.75);
		}

		static bool TryParseUpgradeDateTimeUtc(string upgradeDateTimeUtc, out DateTime dateTimeUpgradeUtc, out DateTime dateTimeUpgradeRefreshedUtc)
		{
			dateTimeUpgradeUtc = dateTimeUpgradeRefreshedUtc = DateTime.MinValue;

			if (string.IsNullOrEmpty(upgradeDateTimeUtc))
			{
				return false;
			}

			var values = upgradeDateTimeUtc.Split('|'); // 'upgradeUtc|refreshedUtc'

			if (values.Length != 2)
			{
				return false;
			}

			if (!string.IsNullOrEmpty(values[0]) && !string.IsNullOrEmpty(values[1]) && SqlFormatInfo.TryParseFromSqlDateTime(values[0], out dateTimeUpgradeUtc) && SqlFormatInfo.TryParseFromSqlDateTime(values[1], out dateTimeUpgradeRefreshedUtc))
			{
				return true;
			}

			dateTimeUpgradeUtc = dateTimeUpgradeRefreshedUtc = DateTime.MinValue;

			return false;
		}
	}
}
