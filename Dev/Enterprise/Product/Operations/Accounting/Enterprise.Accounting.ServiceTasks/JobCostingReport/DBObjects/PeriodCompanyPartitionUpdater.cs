using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ServiceTasks.JobCostingReport
{
	internal class PeriodCompanyPartitionUpdater
	{
		internal PeriodCompanyPartitionUpdater(DbConnection connection
			, IEnumerable<IDeleteDataWhenRemovePartition> dependentObjectsThatNeedToDeleteDataWhenPartitionIsRemoved
			, ILogger logger)
		{
			this.connection = Argument.NotNull(connection, nameof(connection));
			this.dependentObjectsThatNeedToDeleteDataWhenPartitionIsRemoved = Argument.NotNull(dependentObjectsThatNeedToDeleteDataWhenPartitionIsRemoved, nameof(dependentObjectsThatNeedToDeleteDataWhenPartitionIsRemoved));
			this.logger = Argument.NotNull(logger, nameof(logger));
		}

		readonly DbConnection connection;
		readonly IEnumerable<IDeleteDataWhenRemovePartition> dependentObjectsThatNeedToDeleteDataWhenPartitionIsRemoved;
		readonly ILogger logger;

		internal void SyncPartitionsWithAccountingPeriods()
		{
			if (DropStagingTableIfRequires(connection))
			{
				var hasRunPartition = false;
				var isThereAnyPartitionToCreate = true;
				var (currentStartPeriod, newStartPeriod) = GetStartPeriod(connection);

				while (isThereAnyPartitionToCreate)
				{
					var newPartitionsToCreate = GetNewPartitionInfoThatNeedToBeCreated(newStartPeriod);
					var archivalPartitions = GetPartitionsThatAreOutsideTheSlidingWindow(newStartPeriod);
					var orphanPartitions = GetPartitionKeysThatDoNotHaveCorrespondingAccountingPeriods();

					if (newPartitionsToCreate.Count > 0 || archivalPartitions.Count > 0 || orphanPartitions.Count > 0)
					{
						using (var manager = connection.BeginTransactionWithManager())
						{
							var partitionsToRemove = archivalPartitions.Union(orphanPartitions).OrderBy(p => p.PeriodKey);
							foreach (var partitionInfo in partitionsToRemove)
							{
								RemovePartition(partitionInfo);
							}

							foreach (var partitionInfo in newPartitionsToCreate)
							{
								AddPartition(partitionInfo);
							}

							manager.CommitTransaction();
						}

						if (currentStartPeriod != newStartPeriod)
						{
							AccountingConfigurationRegistry.Instance.JCDReportDataCollectionStartingPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newStartPeriod);
						}
						hasRunPartition = true;
					}
					else
					{
						isThereAnyPartitionToCreate = false;
						AccountingMasterFilesRegistry.Instance.NewCompanyPartitionKeyNeedsToBeCreated.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isThereAnyPartitionToCreate);
					}
				}

				if (hasRunPartition)
				{
					logger.Log(LogType.Information, "Partition keys are in-sync with accounting periods.");
				}
			}
			else
			{
				throw new InvalidOperationException(FormattableString.Invariant($@"Could not drop {JCDTableAndPartitionObject.StagingTableName} table."));
			}
		}

		List<PartitionInfo> GetNewPartitionInfoThatNeedToBeCreated(int startingPeriod)
		{
			var sql = FormattableString.Invariant($@"{GetSQL("LEFT")} WHERE PF.Period IS NULL AND AM.AM_Period >= {startingPeriod}");
			return LoadPartitionInfo(sql);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void AddPartition(PartitionInfo partitionInfo)
		{
			logger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Creating Partition {0}", partitionInfo.AM_Key));

			using (var command = connection.Command("CreatePeriodCompanyPartitionKeys"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@NewPartitionKey", SqlDbType.Char, 42, partitionInfo.AM_Key);
				command.ExecuteNonQuery();
			}
			using (var command = connection.Command("Update dbo.JobCostingDataQueue SET JCQ_HasNoPeriod = 0 WHERE JCQ_PostDate between @startDate and @endDate AND JCQ_HasNoPeriod = 1"))
			{
				command.CommandType = CommandType.Text;
				command.AddParameter("@startDate", SqlDbType.DateTime, partitionInfo.AM_StartDate);
				command.AddParameter("@endDate", SqlDbType.DateTime, partitionInfo.AM_EndDate);
				command.ExecuteNonQuery();
			}
			using (var command = connection.Command("Update dbo.JobCostingDataQueue SET JCQ_HasNoPeriod = 0 WHERE JCQ_ReverseDate between @startDate and @endDate AND JCQ_HasNoPeriod = 1"))
			{
				command.CommandType = CommandType.Text;
				command.AddParameter("@startDate", SqlDbType.DateTime, partitionInfo.AM_StartDate);
				command.AddParameter("@endDate", SqlDbType.DateTime, partitionInfo.AM_EndDate);
				command.ExecuteNonQuery();
			}

			logger.Log(LogType.Debug, "Partition is created successfully");
		}

		List<PartitionInfo> GetPartitionKeysThatDoNotHaveCorrespondingAccountingPeriods()
		{
			var sql = FormattableString.Invariant($@"{GetSQL("RIGHT")} WHERE PF.PeriodKey <> '99999900000000-0000-0000-0000-000000000000' AND AM.AM_Period IS NULL");
			return LoadPartitionInfo(sql);
		}

		List<PartitionInfo> GetPartitionsThatAreOutsideTheSlidingWindow(int startingPeriod)
		{
			var sql = FormattableString.Invariant($@"{GetSQL("INNER")} WHERE AM.AM_Period < {startingPeriod}");
			return LoadPartitionInfo(sql);
		}

		List<PartitionInfo> LoadPartitionInfo(string sql)
		{
			var partitionInfos = new List<PartitionInfo>();
			var dt = DataUtils.GetDataTableFromQuery(connection, sql);
			if (dt?.Rows.Count > 0)
			{
				foreach (DataRow row in dt.Rows)
				{
					var pInfo = CreatePartitionInfo(row);
					if (pInfo != null)
					{
						partitionInfos.Add(pInfo);
					}
				}
			}
			return partitionInfos;
		}

		PartitionInfo CreatePartitionInfo(DataRow row)
		{
			if (row != null)
			{
				var amPeriod = GetFieldValueSafely("AM_Period", 0);
				var amKey = GetFieldValueSafely("AM_Key", string.Empty);
				var startDate = GetFieldValueSafely("AM_StartDate", DateTime.MinValue);
				var endDate = GetFieldValueSafely("AM_EndDate", DateTime.MinValue);
				var companyPk = GetFieldValueSafely("AM_GC_Company", Guid.Empty);
				var periodKey = GetFieldValueSafely("PeriodKey", string.Empty);
				return new PartitionInfo(amPeriod, amKey, startDate, endDate, companyPk, periodKey);
			}

			T GetFieldValueSafely<T>(string columnName, T defaultValue) => row[columnName] != DBNull.Value ? row.Field<T>(columnName) : defaultValue;

			return null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void RemovePartition(PartitionInfo partitionInfo)
		{
			logger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Dropping Partition {0}", partitionInfo.PeriodKey));

			using (var command = connection.Command("DropPeriodCompanyPartitionKeys"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@partitionKeyToRemove", SqlDbType.Char, 42, partitionInfo.PeriodKey);
				logger.Log(LogType.Debug, "  Executing: " + command.CommandText);
				command.ExecuteNonQuery();
			}

			if (partitionInfo.AM_StartDate != DateTime.MinValue && partitionInfo.AM_EndDate != DateTime.MinValue)
			{
				using (var command = connection.Command("DELETE FROM dbo.JobCostingDataQueue WHERE (JCQ_PostDate between @startDate and @endDate) OR (JCQ_ReverseDate between @startDate and @endDate)"))
				{
					command.CommandType = CommandType.Text;
					command.AddParameter("@startDate", SqlDbType.DateTime, partitionInfo.AM_StartDate);
					command.AddParameter("@endDate", SqlDbType.DateTime, partitionInfo.AM_EndDate);
					logger.Log(LogType.Debug, "  Executing: " + command.CommandText);
					command.ExecuteNonQuery();
				}
			}

			if (partitionInfo.AM_GC_Company != Guid.Empty && partitionInfo.AM_Period != 0)
			{
				foreach (var removalObject in dependentObjectsThatNeedToDeleteDataWhenPartitionIsRemoved)
				{
					using (var command = connection.Command("SELECT 1 AS Placeholder"))
					{
						removalObject.SetCommandForRemovePartition(command, partitionInfo);
						logger.Log(LogType.Debug, "  Executing: " + command.CommandText);
						command.ExecuteNonQuery();
					} 
				}
			}

			logger.Log(LogType.Debug, "Partition is removed successfully");
		}

		internal static (int currentStartPeriod, int startingPeriod) GetStartPeriod(DbConnection connection)
		{
			var currentStartPeriod = AccountingConfigurationRegistry.Instance.JCDReportDataCollectionStartingPeriod.Value;
			var calculatedStartPeriod = CalculateStartPeriod(connection);
			var newStartPeriod = calculatedStartPeriod > currentStartPeriod ? calculatedStartPeriod : currentStartPeriod;
			return (currentStartPeriod, newStartPeriod);
		}

		static int CalculateStartPeriod(DbConnection connection)
		{
			var maxPartitionCount = AccountingConfigurationRegistry.Instance.MaximumNumberOfJCDPartitionKeys.Value;
			var sqlText = $@"SELECT	TOP 2 AM_Period, 
									RANK() over (ORDER by AM_Period ASC) Position 
							FROM	(SELECT Top {maxPartitionCount} AM_Period 
									FROM  dbo.AccPeriodManagement 
									ORDER BY AM_Period DESC) t
							GROUP BY AM_Period
							ORDER BY Position";
			var periodTable = DataUtils.GetDataTableFromQuery(connection, sqlText);

			if (periodTable != null && periodTable.Rows.Count > 0)
			{
				var minPeriod = Convert.ToInt32(periodTable.Rows[0]["AM_Period"]);
				var periodCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.AccPeriodManagement WHERE AM_Period >= @minPeriod", (cmd) => cmd.AddParameter("@minPeriod", SqlDbType.Int, minPeriod));
				if (periodCount <= maxPartitionCount)
				{
					return minPeriod;
				}

				if (periodCount > maxPartitionCount && periodTable.Rows.Count == 2)
				{
					var nextMinPeriod = Convert.ToInt32(periodTable.Rows[1]["AM_Period"]);
					return nextMinPeriod;
				}

				if (periodCount > maxPartitionCount && periodTable.Rows.Count == 1)
				{
					throw new InvalidOperationException(FormattableString.Invariant($"Cannot create partitions. Allowed maximum number of partitions is too small to generate report data for at least one accounting period of all companies. Please set a higher value at {AccountingConfigurationRegistry.Instance.MaximumNumberOfJCDPartitionKeys.HumanReadableRegistryPath()}."));
				}
			}
			return 0;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		bool DropStagingTableIfRequires(DbConnection connection)
		{
			var stagingTableExist = DataUtils.ObjectExists(connection, JCDTableAndPartitionObject.StagingTableName);

			if (stagingTableExist)
			{
				using (var manager = connection.BeginTransactionWithManager())
				{
					if (DataUtils.ObjectExists(connection, JCDTableAndPartitionObject.MainTableName))
					{
						using (var cmd = connection.Command(FormattableString.Invariant($@"INSERT INTO {JCDTableAndPartitionObject.MainTableName} SELECT * FROM {JCDTableAndPartitionObject.StagingTableName};")))
						{
							cmd.ExecuteNonQuery();
						}
					}

					using (var cmd = connection.Command(FormattableString.Invariant($@"DROP TABLE {JCDTableAndPartitionObject.StagingTableName};")))
					{
						cmd.ExecuteNonQuery();
					}

					manager.CommitTransaction();

					stagingTableExist = DataUtils.ObjectExists(connection, JCDTableAndPartitionObject.StagingTableName);
				}
			}
			return !stagingTableExist;
		}

		static string GetSQL(string joinName) => FormattableString.Invariant($@"SELECT AM_Period
																						, convert(char(6), AM_Period) + convert(char(36), AM_GC_Company) as AM_Key
																						, AM_StartDate
																						, AM_EndDate
																						, AM_GC_Company
																						, PF.PeriodKey
																				FROM	dbo.AccPeriodManagement AM
																						{joinName} JOIN
																						(	SELECT	CAST(SUBSTRING(CAST(value as char(42)), 1, 6) AS INT) as Period
																									, CAST(SUBSTRING(CAST(value as char(42)), 7, 36) as uniqueidentifier) as GC_PK
																									, [value] as PeriodKey
																							FROM	sys.partition_functions AS pf
																									INNER JOIN sys.partition_range_values AS prv ON prv.function_id = pf.function_id
																							WHERE	pf.name = 'PF_AccountingPeriodCompany'
																						) PF ON AM_GC_Company = PF.GC_PK AND AM.AM_Period = PF.Period");
	}

	public sealed class PartitionInfo : IEquatable<PartitionInfo>
	{
		public PartitionInfo(int amPeriod, string amKey, DateTime startDate, DateTime endDate, Guid companyPk, string periodKey)
		{
			AM_Period = amPeriod;
			AM_Key = amKey;
			AM_StartDate = startDate;
			AM_EndDate = endDate;
			AM_GC_Company = companyPk;
			PeriodKey = periodKey;
		}

		public int AM_Period { get; }
		public string AM_Key { get; }
		public DateTime AM_StartDate { get; }
		public DateTime AM_EndDate { get; }
		public Guid AM_GC_Company { get; }
		public string PeriodKey { get; }

		public override bool Equals(object obj)
			=> obj is PartitionInfo pi
			&& Equals(pi);

		public bool Equals(PartitionInfo other)
			=> this.AM_Period == other.AM_Period
			&& this.AM_Key == other.AM_Key
			&& this.AM_StartDate == other.AM_StartDate
			&& this.AM_EndDate == other.AM_EndDate
			&& this.AM_GC_Company == other.AM_GC_Company
			&& this.PeriodKey == other.PeriodKey;

		public override int GetHashCode()
			=> AM_Period.GetHashCode()
			^ (AM_Key ?? string.Empty).GetHashCode()
			^ AM_StartDate.GetHashCode()
			^ AM_EndDate.GetHashCode()
			^ AM_GC_Company.GetHashCode()
			^ (PeriodKey ?? string.Empty).GetHashCode();
	}
}
