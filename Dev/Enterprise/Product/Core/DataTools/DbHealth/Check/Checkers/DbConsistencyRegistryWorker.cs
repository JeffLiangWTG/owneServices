using System;
using System.Globalization;
using CargoWise.Data;
using Enterprise.DbHealth.Shared;

namespace Enterprise.DbHealth.Check
{
	class DbConsistencyRegistryWorker : DbRegistryWorker
	{
		public DbConsistencyRegistryWorker(DbConnection connection, string databaseName)
		{
			this.connection = connection;
			this.databaseName = databaseName;
		}
		readonly DbConnection connection;
		readonly string databaseName;

		internal const string ConsistencyCheckerLastCheckStepPropertyName = "ConsistencyCheckerRegistryWorker.LastCheckStep";
		internal const string ConsistencyCheckerLastRunningDbPropertyName = "ConsistencyCheckerRegistryWorker.LastRunningDb";
		internal const string ConsistencyCheckerLastRunningTablePropertyName = "ConsistencyCheckerRegistryWorker.LastRunningTable";
		internal const string ConsistencyCheckerLastRunningElapsedTimePropertyName = "ConsistencyCheckerRegistryWorker.LastRunningElapsedTime";

		protected override int LastCheckStep
		{
			get
			{
				var propertyValue = DataUtils.LoadDbExtendedProperty(connection, ConsistencyCheckerLastCheckStepPropertyName, databaseName);
				if (!string.IsNullOrWhiteSpace(propertyValue))
				{
					int step;
					if (int.TryParse(propertyValue, out step))
					{
						return step;
					}
				}

				return 0;
			}

			set { DataUtils.SaveDbExtendedProperty(connection, ConsistencyCheckerLastCheckStepPropertyName, value.ToString(CultureInfo.InvariantCulture), databaseName); }
		}

		protected override string LastCheckStepDatabase
		{
			get
			{
				var stepDatabase = DataUtils.LoadDbExtendedProperty(connection, ConsistencyCheckerLastRunningDbPropertyName, databaseName);
				if (!String.IsNullOrWhiteSpace(stepDatabase))
				{
					return stepDatabase;
				}

				return String.Empty;
			}

			set { DataUtils.SaveDbExtendedProperty(connection, ConsistencyCheckerLastRunningDbPropertyName, value, databaseName); }
		}

		protected override string LastCheckStepTableOrView
		{
			get
			{
				var stepTableOrView = DataUtils.LoadDbExtendedProperty(connection, ConsistencyCheckerLastRunningTablePropertyName, databaseName);
				if (!String.IsNullOrWhiteSpace(stepTableOrView))
				{
					return stepTableOrView;
				}

				return String.Empty;
			}

			set { DataUtils.SaveDbExtendedProperty(connection, ConsistencyCheckerLastRunningTablePropertyName, value, databaseName); }
		}

		protected override string LastCheckStepElapsedTime
		{
			get
			{
				var value = DataUtils.LoadDbExtendedProperty(connection, ConsistencyCheckerLastRunningElapsedTimePropertyName, databaseName);
				if (!String.IsNullOrWhiteSpace(value))
				{
					return value;
				}

				return String.Empty;
			}

			set { DataUtils.SaveDbExtendedProperty(connection, ConsistencyCheckerLastRunningElapsedTimePropertyName, value, databaseName); }
		}

		public override string IsStuckMessage
		{
			get
			{
				return string.Format(CultureInfo.InvariantCulture,
					"Database Consistency Check is repeatedly timing out at the same point [Step = {0} Object = {1}.{2}]\r\n. i.e.: No Consistency check is actually being performed.",
					CurrentStep, CurrentDatabase, CurrentTableOrView);
			}
		}

#if DEBUG
		#region Test

		public int LastCheckStepExposed
		{
			get { return LastCheckStep; }
			set { LastCheckStep = value; }
		}

		public string LastCheckStepDatabaseExposed
		{
			get { return LastCheckStepDatabase; }
			set { LastCheckStepDatabase = value; }
		}

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

		#endregion
#endif
	}
}
