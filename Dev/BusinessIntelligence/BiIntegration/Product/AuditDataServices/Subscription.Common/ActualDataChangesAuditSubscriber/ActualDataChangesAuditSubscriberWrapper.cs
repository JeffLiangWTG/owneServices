using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.Schema;
using Enterprise.Integration;

namespace Enterprise.AuditDataServices.Subscription.Common
{
	public class ActualDataChangesAuditSubscriberWrapper : QueryTypeAuditSubscriberWrapper, IActualDataChangesAuditSubscriber
	{
		public ActualDataChangesAuditSubscriberWrapper(ActualDataChangesAuditSubscriber subscriber, DbConnection auditConnection, ILogger logger) : base(subscriber, auditConnection, logger)
		{
		}

		#region Properties and Fields

		string AuditTableQualifiedName => $"[{Table.SqlSchemaName}].[{Table.TableName}]";

		public ITableSchema Table => ((ActualDataChangesAuditSubscriber)Subscriber).Table;

		public DataTable ChangeTable { get; private set; }

		IEnumerable<byte> tableMaxLsn;
		public IEnumerable<byte> TableMaxLsn
		{
			get
			{
				return tableMaxLsn;
			}
			set
			{
				tableMaxLsn = value;
			}
		}

		#endregion

		#region Should Run Subscriber

		public override bool HasChangesToProcess()
		{
			var hasChangesToProcess = false;
			using (var cmd = auditConnection.Command($"[{BiConstants.BiAdminSchemaName}].[usp_SubscriberPendingChanges]"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@subscriberCode", SqlDbType.Char, 3, Code);
				cmd.AddParameter("@subscriberTableName", SqlDbType.VarChar, 128, Table.TableName);
				cmd.AddParameter("@subscriberSchemaName", SqlDbType.VarChar, 128, Table.SqlSchemaName);
				cmd.AddParameter("@latestPeriod", SqlDbType.SmallInt, MaxLsnPeriod);

				cmd.AddOutputParameter("@lsnHighWaterMark", SqlDbType.Binary, 10, 0, 0, DBNull.Value);
				cmd.AddOutputParameter("@seqValHighWaterMark", SqlDbType.Binary, 10, 0, 0, DBNull.Value);
				cmd.AddOutputParameter("@commandIdHighWaterMark", SqlDbType.Int, 0, 0, 0, DBNull.Value);
				cmd.AddOutputParameter("@operationHighWaterMark", SqlDbType.Int, 0, 0, 0, DBNull.Value);
				cmd.AddOutputParameter("@nextPeriodToProcess", SqlDbType.SmallInt, 0, 0, 0, DBNull.Value);

				cmd.ExecuteNonQuery();

				NextLsnHighWaterMark = GetByteOutputParameter(cmd, "@lsnHighWaterMark");
				NextSeqValHighWaterMark = GetByteOutputParameter(cmd, "@seqValHighWaterMark");
				NextCommandIdHighWaterMark = GetIntOutputParameter(cmd, "@commandIdHighWaterMark");
				NextOperationHighWaterMark = GetIntOutputParameter(cmd, "@operationHighWaterMark");
				NextPeriodHighWaterMark = MaxLsnPeriod;

				var nextPeriodToProcessObj = cmd.GetParameterValue("@nextPeriodToProcess");
				if (nextPeriodToProcessObj != DBNull.Value)
				{
					NextPeriodHighWaterMark = Convert.ToInt16(nextPeriodToProcessObj);
					hasChangesToProcess = true;
				}

				Logger.Log(LogType.Debug, $"> Current Subscriber High Water Marks (LSN: {ByteArrayToString(NextLsnHighWaterMark)}, SeqVal: {ByteArrayToString(NextSeqValHighWaterMark)}, Period: {NextPeriodHighWaterMark})");
				return hasChangesToProcess;
			}
		}

		public override void ValidateSubscriber()
		{
			if (Table == null)
			{
				throw new ArgumentException("Subscriber table cannot be null.");
			}
		}

		public override bool HasChangesToProcessFromCache(Dictionary<string, Lsn> tableStateLsnHighWaterMarkCache, Dictionary<string, Lsn> subscriberLsnHighWaterMarkCache, Dictionary<string, int> subscriberCommandIdHighWaterMarkCache)
		{
			var tableName = AuditTableQualifiedName;
			var result = IsTableStateHwmGreaterThanSubscriberControlHwm(tableName, tableStateLsnHighWaterMarkCache, subscriberLsnHighWaterMarkCache, subscriberCommandIdHighWaterMarkCache);
			if (result)
			{
				TableMaxLsn = tableStateLsnHighWaterMarkCache[tableName].Value;
			}
			return result;
		}

		#endregion

		#region Process Changes

		public override bool FetchDataAndProcessChanges()
		{
			var changesProcessed = false;
			var queryResult = FetchChanges();
			ChangeTable = new DataTable();

			if (ThereAreRelevantChanges(queryResult))
			{
				ChangeTable = FilterQueryResult(queryResult);

				if (ThereAreRelevantChanges(ChangeTable))
				{
					ProcessChanges(Logger, ChangeTable);
					UpdateLsnHighWaterMark();
					changesProcessed = true;
				}
				else
				{
					ProcessEmptyBatch();
				}
			}
			else
			{
				ProcessEmptyBatch();
			}

			return changesProcessed;
		}

		void ProcessEmptyBatch()
		{
			Logger.Log(LogType.Debug, "> No relevant changes found");
			if (NextLsnHighWaterMark != null)
			{
				UpdateLsnHighWaterMark();
			}
		}

		public void ProcessChanges(ILogger logger, DataTable changeTable)
		{
			logger.Log(LogType.Debug, $"> Processing {changeTable.Rows.Count} change(s)");
			((ActualDataChangesAuditSubscriber)Subscriber).ProcessChanges(logger, changeTable);
			logger.Log(LogType.Debug, "> Processing completed");
		}

		readonly byte[] maxSeqValue = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
		protected virtual DataTable FetchChanges()
		{
			var queryResult = ExecuteFetchChangesQuery();

			if (CustomFilter != null)
			{
				for (var i = 0; i < queryResult.Rows.Count; i++)
				{
					RunCustomFilter(queryResult.Rows[i]);
				}
				queryResult.AcceptChanges();
			}
			
			if (ThereAreRelevantChanges(queryResult))
			{
				var index = queryResult.Rows.Count - 1;
				var lastRow = queryResult.Rows[index];
				var lastRowOperation = Convert.ToInt32(lastRow[AuditFieldNames.OperationFieldName]);
				while (lastRowOperation == 3 && lastRow != null)
				{
					queryResult.Rows.RemoveAt(index--);
					if (index >= 0)
					{
						lastRow = queryResult.Rows[index];
						lastRowOperation = Convert.ToInt32(lastRow[AuditFieldNames.OperationFieldName]);
					}
					else
					{
						lastRow = null;
					}
				}

				if (lastRow != null)
				{
					NextOperationHighWaterMark = lastRowOperation;
					NextLsnHighWaterMark = (byte[])lastRow[AuditFieldNames.StartLsnFieldName];
					NextSeqValHighWaterMark = (byte[])lastRow[AuditFieldNames.SeqValFieldName];
					NextPeriodHighWaterMark = Convert.ToInt16(lastRow[AuditFieldNames.LsnPeriodFieldName]);
					NextCommandIdHighWaterMark = Convert.ToInt32(lastRow[AuditFieldNames.CommandIdFieldName]);
				}
			}

			if (!ThereAreRelevantChanges(queryResult))
			{
				if (NextPeriodHighWaterMark != MaxLsnPeriod)
				{
					NextPeriodHighWaterMark = GetNextPeriod(NextPeriodHighWaterMark);
					queryResult = FetchChanges();
				}
				else
				{
					NextLsnHighWaterMark = MaxLsn;
					NextSeqValHighWaterMark = maxSeqValue;
					NextPeriodHighWaterMark = MaxLsnPeriod;
					NextCommandIdHighWaterMark = int.MaxValue;
					NextOperationHighWaterMark = int.MaxValue;
				}
			}

			return queryResult;
		}

		void RunCustomFilter(DataRow row)
		{
			if (CustomFilter != null)
			{
				CustomFilter(row);
			}
		}

		int GetNextPeriod(int currentPeriod)
		{
			var year = (currentPeriod / 100) + 2000;
			var month = currentPeriod % 100;
			var newPeriod = (new DateTime(year, month, 1)).AddMonths(1);
			return ((newPeriod.Year - 2000) * 100) + newPeriod.Month;
		}

		protected virtual DataTable ExecuteFetchChangesQuery()
		{
			try
			{
				DataTable queryResult;

#pragma warning disable CW1107 // Do Not Use Db.Connection Methods
				using (var cmd = auditConnection.Command($"[{BiConstants.BiAdminSchemaName}].usp_FetchChanges"))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.AddParameter("@SubscriberTableName", SqlDbType.VarChar, 128, Table.TableName);
					cmd.AddParameter("@AuditTableQualifiedName", SqlDbType.VarChar, 261, AuditTableQualifiedName);

					cmd.AddParameter("@BatchSize", SqlDbType.BigInt, BatchSize);
					cmd.AddParameter("@LatestLsn", SqlDbType.Binary, 10, 0, 0, MaxLsn);
					cmd.AddParameter("@LsnPeriod", SqlDbType.SmallInt, NextPeriodHighWaterMark);

					cmd.AddParameter("@LsnHighWaterMark", SqlDbType.Binary, 10, 0, 0, NextLsnHighWaterMark);
					cmd.AddParameter("@SeqValHighWaterMark", SqlDbType.Binary, 10, 0, 0, NextSeqValHighWaterMark);

					cmd.AddParameter("@CommandId", SqlDbType.Int, NextCommandIdHighWaterMark);
					cmd.AddParameter("@Operation", SqlDbType.Int, NextOperationHighWaterMark);

					cmd.AddOutputParameter("@ToLsn", SqlDbType.Binary, 10, 0, 0, null);
					cmd.AddOutputParameter("@ToSeqVal", SqlDbType.Binary, 10, 0, 0, null);
					cmd.AddOutputParameter("@ToCommandID", SqlDbType.Int, 0, 0, 0, null);
					cmd.AddOutputParameter("@ToOperation", SqlDbType.Int, 0, 0, 0, null);

					queryResult = DataUtils.GetDataTableFromCommand(cmd);
				}
#pragma warning restore CW1107 // Do Not Use Db.Connection Methods
				return queryResult;
			}
			catch (SqlException ex) when (ex.Message.Contains($"Invalid object name '{Table.SqlSchemaName}.{Table.TableName}'"))
			{
				var message = $"Table {AuditTableQualifiedName} is not audited.";
				Logger.Error(message, ex);
				throw new InvalidOperationException(message);
			}
		}

		#endregion
	}
}
