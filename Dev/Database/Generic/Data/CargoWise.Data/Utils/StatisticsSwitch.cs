using System.Globalization;
using CargoWise.Common;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Data
{
	/// <summary>
	/// Sets database AUTO_CREATE_STATISTICS and AUTO_UPDATE_STATISTICS options.
	/// Used to temporarily turn off auto create/update of statistics during an upgrade.
	/// </summary>
	public interface IStatisticsSwitch
	{
		void CheckAndTurnStatistics(string dbName, bool isON);
	}

	public partial class StatisticsSwitch : IStatisticsSwitch
	{
		#region Singleton

		// Private constructor
		StatisticsSwitch() { }

		[ThreadSafe]
		static StatisticsSwitch instance;
		public static StatisticsSwitch Instance
		{
			get { return instance ?? (instance = new StatisticsSwitch()); }
		}

		#endregion // Singleton

		public void CheckAndTurnStatistics(string dbName, bool isON)
		{
			using (var connection = Db.NewAdminConnection())
			{
				connection.IsUpgradeCheckDisabled = true;
				((ICurrentDbControl)connection).UseDatabase(Db.SqlMasterDb);

				var stats = GetStatisticsSettings(connection, dbName);

				if (isON) // restore db settings
				{
					if (stats.Stored != null)
					{
						SetDbStatisticsSettings(connection, dbName, stats.Stored.AutoCreate, stats.Stored.AutoUpdate, autoUpdateAsyncON: true);
						DropStoredStatisticsSettings(connection, dbName);
					}
				}
				else
				{
					if (stats.Stored == null && stats.Current != null)
					{
						StoreStatisticsSettings(connection, dbName, stats.Current.ToString());
					}

					SetDbStatisticsSettings(connection, dbName, autoCreateON: false, autoUpdateON: false, autoUpdateAsyncON: false);
				}
			}
		}

		public DbStatisticsSettings GetStatisticsSettings(DbConnection connection, string dbName)
		{
			Argument.NotNull(connection, nameof(connection));

			var sql = string.Format(CultureInfo.InvariantCulture, @"-- StatisticsSwitch.GetStatisticsSettings
SELECT TOP (1)
	CurrentSettings = CASE d.is_auto_create_stats_on       WHEN 1 THEN 'ON' ELSE 'OFF' END + '-' +
										 CASE d.is_auto_update_stats_on       WHEN 1 THEN 'ON' ELSE 'OFF' END + '-' +
										 CASE d.is_auto_update_stats_async_on WHEN 1 THEN 'ON' ELSE 'OFF' END,
	StoredSettings  = ISNULL(CONVERT(varchar(11), p.value), '')
FROM
	sys.databases AS d
	OUTER APPLY {0}.sys.fn_listextendedproperty('PreUpgradeStatisticsStatus', default, default, default, default, default, default) AS p
WHERE
	d.name = N'{1}'
OPTION (RECOMPILE);
"
				, dbName.QuoteName()            // 0
				, dbName.QuoteEscapedName('\'') // 1
				);

			var stats = new DbStatisticsSettings();
			connection.ExecuteReader(sql, (reader) =>
			{
				stats.Current = DbAutoStatsSettings.New((string)reader["CurrentSettings"]);
				stats.Stored = DbAutoStatsSettings.New((string)reader["StoredSettings"]);
			});

			return stats;
		}

		void StoreStatisticsSettings(AdminConnection connection, string dbName, string value)
		{
			Argument.NotNull(connection, nameof(connection));

			var sql = string.Format(CultureInfo.InvariantCulture, @"-- StatisticsSwitch.StoreStatisticsSettings
EXEC {0}.sys.sp_addextendedproperty
	@name  = N'PreUpgradeStatisticsStatus',
	@value = '{1}';
" // Direct SQL query
				, dbName.QuoteName() // 0
				, value              // 1
				);

			connection.ExecuteNonQuery(sql);
		}

		void DropStoredStatisticsSettings(AdminConnection connection, string dbName)
		{
			Argument.NotNull(connection, nameof(connection));

			var sql = string.Format(CultureInfo.InvariantCulture, @"-- StatisticsSwitch.DropStoredStatisticsSettings
EXEC {0}.sys.sp_dropextendedproperty
	@name = N'PreUpgradeStatisticsStatus';
" // Direct SQL query
				, dbName.QuoteName() // 0
				);

			connection.ExecuteNonQuery(sql);
		}

		public void SetDbStatisticsSettings(AdminConnection connection, string dbName, bool autoCreateON, bool autoUpdateON, bool autoUpdateAsyncON)
		{
			Argument.NotNull(connection, nameof(connection));

			var sql = string.Format(CultureInfo.InvariantCulture, @"-- StatisticsSwitch.SetDbStatisticsSettings
ALTER DATABASE {0} SET
	AUTO_CREATE_STATISTICS       {1},
	AUTO_UPDATE_STATISTICS       {2},
	AUTO_UPDATE_STATISTICS_ASYNC {3}
	WITH NO_WAIT;
" // Direct SQL query
				, dbName.QuoteName()                 // 0
				, (autoCreateON) ? "ON" : "OFF"      // 1
				, (autoUpdateON) ? "ON" : "OFF"      // 2
				, (autoUpdateAsyncON) ? "ON" : "OFF" // 3
				);

			connection.ExecuteNonQuery(sql);
		}
	}

	#region Helper classes

	public class DbStatisticsSettings
	{
		public DbAutoStatsSettings Current { get; set; }
		public DbAutoStatsSettings Stored { get; set; }
	}

	public class DbAutoStatsSettings
	{
		public DbAutoStatsSettings(bool autoCreate, bool autoUpdate, bool autoUpdateAsync)
		{
			AutoCreate = autoCreate;
			AutoUpdate = autoUpdate;
			AutoUpdateAsync = autoUpdateAsync;
		}

		public bool AutoCreate { get; }
		public bool AutoUpdate { get; }
		public bool AutoUpdateAsync { get; }

		internal static DbAutoStatsSettings New(string statsSettings)
		{
			if (!string.IsNullOrWhiteSpace(statsSettings))
			{
				var settings = statsSettings.Split('-');
				if (settings.Length == 3)
				{
					return new DbAutoStatsSettings(
						autoCreate: (settings[0] == "ON"),
						autoUpdate: (settings[1] == "ON"),
						autoUpdateAsync: (settings[2] == "ON")
						);
				}
			}

			return null;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}"
				, AutoCreate ? "ON" : "OFF"
				, AutoUpdate ? "ON" : "OFF"
				, AutoUpdateAsync ? "ON" : "OFF"
				);
		}

		public override bool Equals(object obj)
		{
			var objTo = obj as DbAutoStatsSettings;
			if (objTo != null)
			{
				return
					AutoCreate == objTo.AutoCreate
					&& AutoUpdate == objTo.AutoUpdate
					&& AutoUpdateAsync == objTo.AutoUpdateAsync;
			}

			return false;
		}

		public override int GetHashCode()
		{
			return
				(AutoCreate ? 1 << 2 : 0)
				| (AutoUpdate ? 1 << 1 : 0)
				| (AutoUpdateAsync ? 1 : 0);
		}
	}

	#endregion // Helper classes
}

#region Test
#if DEBUG

namespace CargoWise.Data
{
	partial class StatisticsSwitch
	{
		public DbStatisticsSettings GetStatisticsSettings(string dbName)
		{
			return GetStatisticsSettings(Db.Connection, dbName);
		}

		public void SetDbStatisticsSettings(string dbName, bool isON)
		{
			SetDbStatisticsSettings(dbName, autoCreateON: isON, autoUpdateON: isON, autoUpdateAsyncON: isON);
		}

		public void SetDbStatisticsSettings(string dbName, bool autoCreateON, bool autoUpdateON, bool autoUpdateAsyncON)
		{
			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				SetDbStatisticsSettings(connection, dbName, autoCreateON, autoUpdateON, autoUpdateAsyncON);
			}
		}
	}
}

#endif
#endregion // Test
