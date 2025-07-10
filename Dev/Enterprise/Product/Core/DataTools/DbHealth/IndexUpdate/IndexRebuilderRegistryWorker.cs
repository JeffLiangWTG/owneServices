using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbHealth.Shared;
using static System.FormattableString;

namespace Enterprise.DbHealth.IndexUpdate
{
	class IndexRebuilderRegistryWorker : DbRegistryWorker
	{
		protected override int LastCheckStep
		{
			get { return 0; }
			set { }
		}

		internal const string LastRunningDbPropertyName = "IndexRebuilderRegistryWorker.LastRunningDb";
		internal const string LastRunningIndexPropertyName = "IndexRebuilderRegistryWorker.LastRunningIndex";
		internal const string LastRunningElapsedTimePropertyName = "IndexRebuilderRegistryWorker.LastRunningElapsedTime";

		protected override string LastCheckStepTableOrView
		{
			get
			{
				var value = ExtProperty.Database.Select(Db.Connection, LastRunningIndexPropertyName);
				if (!string.IsNullOrWhiteSpace(value))
				{
					return value;
				}

				return string.Empty;
			}

			set { ExtProperty.Database.Update(Db.Connection, LastRunningIndexPropertyName, value); }
		}

		protected override string LastCheckStepDatabase
		{
			get
			{
				var value = ExtProperty.Database.Select(Db.Connection, LastRunningDbPropertyName);

				return Db.Connection.DbNameBySuffix(value);
			}

			set
			{
				var newValue = Db.Connection.DbSuffixByName(value);

				ExtProperty.Database.Update(Db.Connection, LastRunningDbPropertyName, newValue);
			}
		}

		protected override string LastCheckStepElapsedTime
		{
			get
			{
				var value = ExtProperty.Database.Select(Db.Connection, LastRunningElapsedTimePropertyName);
				if (!string.IsNullOrWhiteSpace(value))
				{
					return value;
				}

				return string.Empty;
			}

			set { ExtProperty.Database.Update(Db.Connection, LastRunningElapsedTimePropertyName, value); }
		}

		public override string IsStuckMessage
		{
			get
			{
				return Invariant($"Index Rebuilder is repeatedly timing out at the same point [Index = {CurrentDatabase}.{CurrentTableOrView}]\r\n. i.e.: No indexes are actually being rebuilt.");
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
