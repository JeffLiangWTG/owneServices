namespace CargoWise.Bi.Maintenance
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using CargoWise.Bi.Common;
	using CargoWise.Data;
	using CargoWise.Types;
	using Enterprise.Environment;
	using Enterprise.Integration;

	public abstract class IndexMaintenance
	{
		#region SuppressResourceStringsCheckRegion

		protected IndexMaintenance(DbConnection biConnection, ILogger logger)
		{
			this.biConnection = biConnection;
			this.logger = logger;
		}
		protected readonly DbConnection biConnection;

		protected readonly ILogger logger;

		protected abstract string BiDatabaseName { get; }

		protected void Log(LogType logType, string message, Exception ex = null)
		{
			if (ex == null)
			{
				logger?.Log(logType, message);
			}
			else
			{
				logger?.Log(logType, message, ex);
			}
		}

		public abstract IEnumerable<string> GetErrorList();

		public abstract bool HasOnlyDeadlockError();

		public abstract ZDateTime GetLastIndexRebuildUtcDate();

		public abstract IEnumerable<string> GetReorganizedTableIndexList();

		public abstract IEnumerable<string> GetRebuiltTableIndexList();

		protected abstract IndexMaintenanceResultCode OrganizeIndex();

		protected virtual void LogCdcHistorySummaryError(IndexMaintenanceResultCode result)
		{ }

		public void RunIndexMaintenance()
		{
			if (ActionRequired())
			{
				Log(LogType.Debug, "Reorganizing indexes");
				var result = OrganizeIndex();

				LogIndexRebuildResult(result);
				LogIndexReorganizeError(result);
				LogCdcHistorySummaryError(result);
			}
			else
			{
				Log(LogType.Debug, "Indexes have already been reorganized. Skipping maintenance.");
			}
		}

		void LogIndexRebuildResult(IndexMaintenanceResultCode result)
		{
			switch (result.IsIndexRebuilt)
			{
				case IndexRebuildResultCode.NoIndexCorruption:
					break;
				case IndexRebuildResultCode.AllCorruptedIndexRebuilt:
				case IndexRebuildResultCode.SomeCorruptedIndexRebuilt:
					Log(LogType.Warning,
						string.Format(CultureInfo.InvariantCulture,
							"The following tables have indexes rebuilt.\r\n{0}",
							string.Join("\r\n", GetRebuiltTableIndexList())));
					break;
				default:
					throw new BiMaintenanceException(string.Format(CultureInfo.InvariantCulture, "Unknown IsIndexRebuilt code ({0}) returned.", result.IsIndexRebuilt));
			}
		}

		void LogIndexReorganizeError(IndexMaintenanceResultCode result)
		{
			switch (result.ErrorCode)
			{
				case IndexErrorCode.NoIndexesReorganized:
					Log(LogType.Debug, "No indexes have been reorganized. Task should be scheduled within the 3 hour window from daily start time.");
					break;
				case IndexErrorCode.AllIndexesReorganized:
					Log(LogType.Information, "All indexes have been reorganized.");
					break;
				case IndexErrorCode.Timeout:
					Log(LogType.Information,
						string.Format(CultureInfo.InvariantCulture,
							"The following tables have indexes reorganized.\r\n{0}",
							string.Join("\r\n", GetReorganizedTableIndexList())));
					break;
				case IndexErrorCode.TableError:
					HandleTableErrors();
					break;
				case IndexErrorCode.SqlError:
					break;
				default:
					throw new BiMaintenanceException(string.Format(CultureInfo.InvariantCulture, "Unknown error code ({0}) returned.", result.ErrorCode));
			}
		}

		void HandleTableErrors()
		{
			if (HasOnlyDeadlockError())
			{
				var lastRebuildDate = GetLastIndexRebuildUtcDate();
				bool shouldThrowException = true;

				if (!lastRebuildDate.IsEmpty)
				{
					var timeDiff = (ZDateTime.UtcNow - lastRebuildDate);
					shouldThrowException = timeDiff.TotalDays > 2;
				}

				if (shouldThrowException)
				{
					throw new BiMaintenanceException(
						string.Format(CultureInfo.InvariantCulture,
							"Index reorganization failed.\r\n{0}",
							string.Join("\r\n", GetErrorList())));
				}
				else
				{
					Log(LogType.Warning, string.Format(CultureInfo.InvariantCulture,
						"Index reorganization failed.\r\n{0}",
						string.Join("\r\n", GetErrorList())));
				}
			}
			else
			{
				throw new BiMaintenanceException(
					string.Format(CultureInfo.InvariantCulture,
						"Index reorganization failed.\r\n{0}",
						string.Join("\r\n", GetErrorList())));
			}
		}

		#region Implementation

		bool ActionRequired()
		{
			var result = true;

			using (((ICurrentDbControl)biConnection).UseDatabase(BiDatabaseName))
			{
				DateTime? lastRunTimeUtc = BiMasterState.GetParameterDate(biConnection, BiConstants.LastIndexRebuildUtcDt);
				if (lastRunTimeUtc.HasValue)
				{
					var currentTime = GetCurrentUtcDateTime();
					var lastRunDifference = currentTime - lastRunTimeUtc.Value;
					result = lastRunDifference > runningInterval;
				}
			}

			return result;
		}

		#endregion

		#region Time Calculations

		protected virtual DateTime GetCurrentUtcDateTime()
		{
			return Env.Time.CurrentUtcDateTime;
		}

		protected int CommandTimeout
		{
			get
			{
				return Convert.ToInt32(TimeSpan.FromHours(2).TotalSeconds);
			}
		}

		TimeSpan runningInterval
		{
			get
			{
				return TimeSpan.FromHours(12);
			}
		}

		#endregion

		#endregion
	}
}
