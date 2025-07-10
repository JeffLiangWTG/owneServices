using System;
using System.Globalization;
using System.Threading;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;

namespace Enterprise.DbUpgrader.Startup
{
	class SystemUpgradeWarning
	{
		public SystemUpgradeWarning(DbConnection connection)
		{
			this.connection = connection;
		}

		readonly DbConnection connection;

		public int SetSystemUpgradeWarningPeriodAndPause(IUpgradeTaskWorkflowLogger logger)
		{
			try
			{
				return SetSystemUpgradeWarningPeriodAndPauseUnsafe(logger);
			}
			catch (Exception ex) when (ex is SqlException || ex is ArgumentNullException || ex is FormatException)
			{
				return 0;
			}
		}

		public void ClearSystemUpgradeWarningPeriod()
		{
			DataUtils.DropDbExtendedProperty(connection, DataUtils.DateTimeForthcomingUpgradeExtPty, Db.DatabaseName);
		}

		int SetSystemUpgradeWarningPeriodAndPauseUnsafe(IUpgradeTaskWorkflowLogger logger)
		{
			var datetimeUpgrade = DateTime.MinValue;
			var datetimeRefreshed = DateTime.MinValue;

			var pauseMins = DbRegistry.GetNewSystemUpgradeWarningPeriodItem().LoadValue(connection);

			if (pauseMins < 0 || pauseMins > 1440)
			{
				pauseMins = 5;
			}

			logger.StartTask((pauseMins == 0) ? "." : String.Format(CultureInfo.InvariantCulture, "The system will upgrade in {0} minutes.", pauseMins));

			datetimeUpgrade = GetCurrentUtcFromDbServer().AddMinutes(pauseMins);

			for (var sleepMins = 0; ; sleepMins++)
			{
				datetimeRefreshed = GetCurrentUtcFromDbServer();
				string forthcomingUpgPtyValue = SqlFormatInfo.ToSqlDateTimeString(datetimeUpgrade) + "|" + SqlFormatInfo.ToSqlDateTimeString(datetimeRefreshed);
				DataUtils.SaveDbExtendedProperty(connection, DataUtils.DateTimeForthcomingUpgradeExtPty, forthcomingUpgPtyValue, Db.DatabaseName);

#if DEBUG
				if (NUnit.Framework.TestingState.IsRunningTests)
				{
					break;
				}
#endif

				if (sleepMins >= pauseMins)
				{
					break;
				}

				Thread.Sleep(TimeSpan.FromMinutes(1));
			}

			return pauseMins;
		}

		DateTime GetCurrentUtcFromDbServer()
		{
			const string getUtcFromSql = "SELECT GETUTCDATE()";
			return Convert.ToDateTime(connection.ExecuteScalar(getUtcFromSql), CultureInfo.InvariantCulture);
		}
	}
}
