using CargoWise.Data;
using Enterprise.DbHealth.Shared;
using static System.FormattableString;

namespace Enterprise.DbHealth.IndexUpdate
{
	class IndexStatisticsUpdateRegistryWorker : DbRegistryWorker
	{
		protected override int LastCheckStep
		{
			get { return 0; }
			set { }
		}

		internal const string StatisticsLastRunningDbPropertyName = "StatisticsUpdaterRegistryWorker.LastRunningDb";
		internal const string StatisticsLastRunningTablePropertyName = "StatisticsUpdaterRegistryWorker.LastRunningTable";
		internal const string StatisticsLastRunningElapsedTimePropertyName = "StatisticsUpdaterRegistryWorker.LastRunningElapsedTime";

		protected override string LastCheckStepDatabase
		{
			get
			{
				var stepDatabase = DataUtils.LoadDbExtendedProperty(Db.Connection, StatisticsLastRunningDbPropertyName, Db.DatabaseName);
				if (!string.IsNullOrWhiteSpace(stepDatabase))
				{
					return stepDatabase;
				}

				return string.Empty;
			}

			set { DataUtils.SaveDbExtendedProperty(Db.Connection, StatisticsLastRunningDbPropertyName, value, Db.DatabaseName); }
		}

		protected override string LastCheckStepTableOrView
		{
			get
			{
				var stepTableOrView = DataUtils.LoadDbExtendedProperty(Db.Connection, StatisticsLastRunningTablePropertyName, Db.DatabaseName);
				if (!string.IsNullOrWhiteSpace(stepTableOrView))
				{
					return stepTableOrView;
				}

				return string.Empty;
			}

			set { DataUtils.SaveDbExtendedProperty(Db.Connection, StatisticsLastRunningTablePropertyName, value, Db.DatabaseName); }
		}

		protected override string LastCheckStepElapsedTime
		{
			get
			{
				var value = DataUtils.LoadDbExtendedProperty(Db.Connection, StatisticsLastRunningElapsedTimePropertyName, Db.DatabaseName);
				if (!string.IsNullOrWhiteSpace(value))
				{
					return value;
				}

				return string.Empty;
			}

			set { DataUtils.SaveDbExtendedProperty(Db.Connection, StatisticsLastRunningElapsedTimePropertyName, value, Db.DatabaseName); }
		}

		public override string IsStuckMessage
		{
			get
			{
				return Invariant($"Index Statistics Updater is repeatedly timing out at the same point [Object = {CurrentDatabase}.{CurrentTableOrView}]\r\n. i.e.: No stats are actually being updated.");
			}
		}

		public override bool SteppingApplied
		{
			get { return false; }
		}
#if DEBUG
		public string LastCheckStepTableOrViewExposed
		{
			get { return LastCheckStepTableOrView; }
			set { LastCheckStepTableOrView = value; }
		}

		public string LastCheckStepElapsedTimeExposed
		{
			get { return LastCheckStepElapsedTime; }
			set { LastCheckStepElapsedTime = value; }
		}
		
		public string LastCheckStepDatabaseExposed
		{
			get { return LastCheckStepDatabase; }
			set { LastCheckStepDatabase = value; }
		}
#endif
	}
}
