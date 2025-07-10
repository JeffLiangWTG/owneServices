using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Environment;

namespace CargoWise.Bi.Product.Manager.Business
{
	#region SuppressResourceStringsCheckRegion

	public class AuditInformation : BiDatabaseInformation
	{
		public AuditInformation(string serverName, string dbName)
			: base(serverName, dbName)
		{
		}

		public new string ServerName
		{
			get
			{
				return base.ServerName;
			}
		}

		public new string DatabaseName
		{
			get
			{
				return base.DatabaseName;
			}
		}

		#region Audit Data Information

		public string LastTransactionProcessedServerDateTime { get; private set; }
		public string LastPartitioningDate { get; private set; }
		public string LastIndexRebuildDate { get; private set; }

		public AuditTableCollection AuditTables { get; private set; }
		public DataLossCollection DataLoss { get; private set; }
		public SubscriberCollection Subscribers { get; private set; }
		public CdcHistoryCollection CdcHistorySummary { get; private set; }

		public void RefreshInfo()
		{
			using (var biConnection = Db.NewAdminConnection(ServerName, DatabaseName))
			{
				RetrieveMasterStateInfo(biConnection);
				RetrieveAuditTablesInfo(biConnection);
				RetrieveDataLossInfo(biConnection);
				RetrieveSubscriberInfo(biConnection);
				CdcHistorySummary = new CdcHistoryCollection();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void RetrieveMasterStateInfo(DbConnection biConnection)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				SELECT ParamName, ParamValue FROM [{0}].[MasterState] WITH (NOLOCK)
				WHERE ParamName IN
					('{1}',
					 '{2}',
					 '{3}')",
				BiConstants.BiAdminSchemaName,
				BiConstants.LastMaxLsnTimeProcessed,
				BiConstants.LastPartitionPurgeUtcDt,
				BiConstants.LastIndexRebuildUtcDt);

			using (var reader = biConnection.Command(sqlText).ExecuteReader())
			{
				while (reader.Read())
				{
					if (reader[1] != DBNull.Value)
					{
						var paramName = reader[0].ToString();
						var paramValue = reader[1].ToString();

						if (paramName.Equals(BiConstants.LastMaxLsnTimeProcessed, StringComparison.OrdinalIgnoreCase))
						{
							LastTransactionProcessedServerDateTime = new ZDateTime(paramValue).ToBestReadableDateTimeString();
						}
						else if (paramName.Equals(BiConstants.LastPartitionPurgeUtcDt, StringComparison.OrdinalIgnoreCase))
						{
							var utcTime = new ZDateTime(paramValue, DateTimeKind.Utc);
							LastPartitioningDate = new ZDateTime(Env.Time.GetLocalTimeFromUtc(utcTime.ToDateTime())).ToBestReadableDateTimeString();
						}
						else if (paramName.Equals(BiConstants.LastIndexRebuildUtcDt, StringComparison.OrdinalIgnoreCase))
						{
							var utcTime = new ZDateTime(paramValue, DateTimeKind.Utc);
							LastIndexRebuildDate = new ZDateTime(Env.Time.GetLocalTimeFromUtc(utcTime.ToDateTime())).ToBestReadableDateTimeString();
						}
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void RetrieveAuditTablesInfo(DbConnection biConnection)
		{
			AuditTables = new AuditTableCollection();

			using (var cmd = biConnection.Command("biadmin.usp_GetAuditTablesInfo"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@lastPartitioningDate", SqlDbType.NVarChar, 128, string.IsNullOrEmpty(LastPartitioningDate) ? DBNull.Value : LastPartitioningDate);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var schemaName = reader["SchemaName"].ToString();
						var tableName = reader["TableName"].ToString();
						var auditTable = new AuditTable(schemaName, tableName);

						auditTable.RowCount = new ZLong(reader["RowCounts"]);
						auditTable.TotalSize = new ZDecimal(reader["Total_MB"]);
						auditTable.SizeUsed = new ZDecimal(reader["Used_MB"]);
						auditTable.SizeUnused = new ZDecimal(reader["Unused_MB"]);

						auditTable.CurrentState = reader["CurrentState"].ToString();
						auditTable.LoadRecordCount = new ZLong(reader["LoadRecordCount"]);
						auditTable.LoadDuration = new ZLong(reader["LoadDurationMS"]);
						auditTable.SqlErrorMessage = reader["SqlErrorMessage"].ToString();

						var sqlErrorDatetimeUTC = reader["SqlErrorDatetimeUTC"];
						if (sqlErrorDatetimeUTC != DBNull.Value)
						{
							auditTable.SqlErrorDatetimeUTC = new ZDateTime(sqlErrorDatetimeUTC);
						}
						else
						{
							auditTable.SqlErrorDatetimeUTC = ZDateTime.Empty;
						}

						auditTable.IndexReorganizationSqlErrorMessage =
							reader["IndexReorganizationSqlErrorMessage"].ToString();

						var earliestTransactionDate = reader["MinTranEndTimeUtc"];
						if (earliestTransactionDate != DBNull.Value)
						{
							auditTable.EarliestTransactionDate = new ZDateTime(earliestTransactionDate);
						}

						var latestTransactionDate = reader["MaxTranEndTimeUtc"];
						if (latestTransactionDate != DBNull.Value)
						{
							auditTable.LatestTransactionDate = new ZDateTime(latestTransactionDate);
						}

						AuditTables.Add(auditTable);
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void RetrieveDataLossInfo(DbConnection biConnection)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				SELECT SourceSchemaName, SourceTableName, StartLsn, EndLsn, MinTableLsn, LossType FROM [{0}].[DataLossLog] WITH (NOLOCK)",
				BiConstants.BiAdminSchemaName);

			DataLoss = new DataLossCollection();

			using (var reader = biConnection.Command(sqlText).ExecuteReader())
			{
				while (reader.Read())
				{
					var schema = reader["SourceSchemaName"].ToString();
					var name = reader["SourceTableName"].ToString();
					var dataLoss = new DataLoss(schema, name);

					dataLoss.StartLsn = "0x" + BitConverter.ToString((byte[])reader["StartLsn"]).Replace("-", "");
					dataLoss.EndLsn = "0x" + BitConverter.ToString((byte[])reader["EndLsn"]).Replace("-", "");
					dataLoss.MinTableLsn = "0x" + BitConverter.ToString((byte[])reader["MinTableLsn"]).Replace("-", "");
					dataLoss.LossType = reader["LossType"].ToString();

					DataLoss.Add(dataLoss);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void RetrieveSubscriberInfo(DbConnection biConnection)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				SELECT s.SubscriberCode, s.Description, s.LsnHighWaterMark, s.SeqValHighWaterMark, s.PeriodHighWatermark, ltm.TranEndTimeUtc
					FROM [{0}].[SubscriberControl] s  WITH (NOLOCK)
					LEFT JOIN [{0}].[LsnTimeMapping] ltm  WITH (NOLOCK)
						ON s.LsnHighWaterMark = ltm.StartLsn",
				BiConstants.BiAdminSchemaName);

			Subscribers = new SubscriberCollection();

			using (var reader = biConnection.Command(sqlText).ExecuteReader())
			{
				while (reader.Read())
				{
					var code = reader["SubscriberCode"].ToString();
					var subscriber = new SubscriberInfo(code);

					subscriber.Description = reader["Description"].Equals(DBNull.Value) ? string.Empty : reader["Description"].ToString();
					subscriber.LsnHighWaterMark = "0x" + BitConverter.ToString((byte[])reader["LsnHighWaterMark"]).Replace("-", "");
					subscriber.SeqValHighWaterMark = "0x" + BitConverter.ToString((byte[])reader["SeqValHighWaterMark"]).Replace("-", "");
					subscriber.PeriodHighWatermark = ZInt.Parse(reader["PeriodHighWatermark"].Equals(DBNull.Value) ? "0" : reader["PeriodHighWatermark"].ToString());
					subscriber.TransactionDateUtc = new ZDateTime(reader["TranEndTimeUtc"]);
					if (subscriber.TransactionDateUtc != ZDateTime.Empty)
					{
						subscriber.TransactionDateLocal = new ZDateTime(Env.Time.GetLocalTimeFromUtc(subscriber.TransactionDateUtc.ToDateTime()));
					}

					Subscribers.Add(subscriber);
				}
			}
		}

		public int FilterCdcHistorySummary(string tableName, ZDateTime utcFromDate, ZDateTime utcToDate)
		{
			using (var biConnection = Db.NewExtraConnectionWithMainDbCredentials(ServerName, DatabaseName))
			{
				RetrieveMasterStateInfo(biConnection);
				return RetrieveCdcHistorySummary(biConnection, tableName, utcFromDate, utcToDate);
			}
		}

		int RetrieveCdcHistorySummary(DbConnection biConnection, string tableName, ZDateTime utcFromDate, ZDateTime utcToDate)
		{
			int? fromLsnPeriod = null;
			int? toLsnPeriod = null;

			List<string> filters = new List<string>();
			if (!string.IsNullOrEmpty(tableName))
			{
				filters.Add($"s.ChangedTableName = @tableName");
			}
			if (!utcFromDate.IsEmpty)
			{
				fromLsnPeriod = (utcFromDate.Year % 100) * 100 + utcFromDate.Month;
				filters.Add($"m.TranEndTimeUtc >= @fromDate");
				filters.Add($"s.LsnPeriod >= @fromLsnPeriod");
			}
			if (!utcToDate.IsEmpty)
			{
				toLsnPeriod = (utcToDate.Year % 100) * 100 + utcToDate.Month;
				filters.Add($"m.TranEndTimeUtc <= @toDate");
				filters.Add($"s.LsnPeriod <= @toLsnPeriod");
			}

			var totalCount = RetrieveCdcHistorySummaryCount(biConnection, tableName, utcFromDate, utcToDate, fromLsnPeriod, toLsnPeriod, filters);
			RetrieveCdcHistorySummaryDetails(biConnection, tableName, utcFromDate, utcToDate, fromLsnPeriod, toLsnPeriod, filters);

			return totalCount;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		int RetrieveCdcHistorySummaryCount(DbConnection biConnection, string tableName, ZDateTime utcFromDate, ZDateTime utcToDate, int? fromLsnPeriod, int? toLsnPeriod, List<string> filters)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
SELECT COUNT(*)
from [{0}].CdcHistorySummary s
inner join [{0}].LsnTimeMapping m
	ON s.Lsn = m.StartLsn{1}",
				BiConstants.BiAdminSchemaName,
				filters.Any() ? $"\r\nWHERE {string.Join(" AND ", filters)}" : "");

			using (var cmd = biConnection.Command(sqlText))
			{
				if (!string.IsNullOrEmpty(tableName))
				{
					cmd.AddParameter("@tableName", SqlDbType.VarChar, 128, tableName);
				}
				if (!utcFromDate.IsEmpty)
				{
					cmd.AddParameter("@fromDate", SqlDbType.DateTime, utcFromDate.ToDateTime());
				}
				if (fromLsnPeriod.HasValue)
				{
					cmd.AddParameter("@fromLsnPeriod", SqlDbType.SmallInt, fromLsnPeriod);
				}
				if (!utcToDate.IsEmpty)
				{
					cmd.AddParameter("@toDate", SqlDbType.DateTime, utcToDate.AddDays(1).ToDateTime());
				}
				if (toLsnPeriod.HasValue)
				{
					cmd.AddParameter("@toLsnPeriod", SqlDbType.SmallInt, toLsnPeriod);
				}

				return Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void RetrieveCdcHistorySummaryDetails(DbConnection biConnection, string tableName, ZDateTime utcFromDate, ZDateTime utcToDate, int? fromLsnPeriod, int? toLsnPeriod, List<string> filters)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
SELECT TOP 10000 s.SchemaName, s.ChangedTableName, s.NumberOfRows, s.Lsn, m.TranEndTimeUtc
from [{0}].CdcHistorySummary s
inner join [{0}].LsnTimeMapping m
	ON s.Lsn = m.StartLsn{1}
order by lsn, ChangedTableName",
				BiConstants.BiAdminSchemaName,
				filters.Any() ? $"\r\nWHERE {string.Join(" AND ", filters)}" : "");

			CdcHistorySummary = new CdcHistoryCollection();

			using (var cmd = biConnection.Command(sqlText))
			{
				if (!string.IsNullOrEmpty(tableName))
				{
					cmd.AddParameter("@tableName", SqlDbType.VarChar, 128, tableName);
				}
				if (!utcFromDate.IsEmpty)
				{
					cmd.AddParameter("@fromDate", SqlDbType.DateTime, utcFromDate.ToDateTime());
				}
				if (fromLsnPeriod.HasValue)
				{
					cmd.AddParameter("@fromLsnPeriod", SqlDbType.SmallInt, fromLsnPeriod);
				}
				if (!utcToDate.IsEmpty)
				{
					cmd.AddParameter("@toDate", SqlDbType.DateTime, utcToDate.ToDateTime());
				}
				if (toLsnPeriod.HasValue)
				{
					cmd.AddParameter("@toLsnPeriod", SqlDbType.SmallInt, toLsnPeriod);
				}
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var schema = reader["SchemaName"].ToString();
						var name = reader["ChangedTableName"].ToString();
						var cdcHistory = new CdcHistory(schema, name);

						cdcHistory.NumberOfRows = Convert.ToInt32(reader["NumberOfRows"], CultureInfo.InvariantCulture);
						cdcHistory.Lsn = "0x" + BitConverter.ToString((byte[])reader["Lsn"]).Replace("-", "");
						cdcHistory.TransactionDateUtc = new ZDateTime(reader["TranEndTimeUtc"]);
						if (cdcHistory.TransactionDateUtc != ZDateTime.Empty)
						{
							cdcHistory.TransactionDateLocal = new ZDateTime(Env.Time.GetLocalTimeFromUtc(cdcHistory.TransactionDateUtc.ToDateTime()));
						}

						CdcHistorySummary.Add(cdcHistory);
					}
				}
			}
		}

		#endregion
	}

	#endregion
}
