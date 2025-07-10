using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.Schema;
using Enterprise.Integration;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.AuditDataServices.Subscription.Common
{
	public class TableValuePairSubscriberWrapper : AuditSubscriberWrapper, ITableValuePairSubscriber
	{
		public TableValuePairSubscriberWrapper(TableValuePairSubscriber subscriber, DbConnection auditConnection, ILogger logger) : base(subscriber, auditConnection, logger)
		{
		}

		public Dictionary<IChangedTableSchema, List<string>> ChangedTableColumnValues { get; private set; }

		#region Should Run Subscriber

		public override bool HasChangesToProcess()
		{
			return GetChangeCount() > 0;
		}

		public override void ValidateSubscriber()
		{
			if (auditConnection == null)
			{
				throw new ArgumentException("Requires an Audit Connection");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Check for Updates")]
		int GetChangeCount()
		{
			var changesCount = 0;
			using (var cmd = auditConnection.Command($"[{BiConstants.BiAdminSchemaName}].usp_GetSubscriberHighWaterMark"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@SubscriberCode", SqlDbType.Char, 3, Code);
				cmd.AddParameter("@LatestLsn", SqlDbType.Binary, 10, 0, 0, MaxLsn);

				cmd.AddOutputParameter("@LsnHighWaterMark", SqlDbType.Binary, 10, 0, 0, null);
				cmd.AddOutputParameter("@SeqValHighWaterMark", SqlDbType.Binary, 10, 0, 0, null);
				cmd.AddOutputParameter("@PeriodHighWaterMark", SqlDbType.SmallInt, -1, 0, 0, null);

				cmd.AddOutputParameter("@LsnWithChangesCount", SqlDbType.Int, -1, 0, 0, null);

				cmd.ExecuteNonQuery();

				// Assign previous LSN High Watermarks as low water marks
				NextLsnHighWaterMark = GetByteOutputParameter(cmd, "@LsnHighWaterMark");
				NextSeqValHighWaterMark = GetByteOutputParameter(cmd, "@SeqValHighWaterMark");
				NextPeriodHighWaterMark = GetSmallintOutputParameter(cmd, "@PeriodHighWaterMark");

				Logger.Log(LogType.Debug, $"> Current Subscriber High Water Marks (LSN: {ByteArrayToString(NextLsnHighWaterMark)}, SeqVal: {ByteArrayToString(NextSeqValHighWaterMark)}, Period: {NextPeriodHighWaterMark})");

				changesCount = GetIntOutputParameter(cmd, "@LsnWithChangesCount");
			}
			return changesCount;
		}

		public override bool HasChangesToProcessFromCache(Dictionary<string, Lsn> tableStateLsnHighWaterMarkCache, Dictionary<string, Lsn> subscriberLsnHighWaterMarkCache, Dictionary<string, int> subscriberCommandIdHighWaterMarkCache)
		{
			var tableNames = tableStateLsnHighWaterMarkCache.Keys.ToArray();
			foreach (var tableName in tableNames)
			{
				if (IsTableStateHwmGreaterThanSubscriberControlHwm(tableName, tableStateLsnHighWaterMarkCache, subscriberLsnHighWaterMarkCache, subscriberCommandIdHighWaterMarkCache))
				{
					return true;
				}
			}
			return false;
		}

		#endregion

		#region Process Changes

		public override bool FetchDataAndProcessChanges()
		{
			try
			{
				if (ChangedTableColumnValues != null || CheckForMoreChanges())
				{
					ProcessChanges(Logger, ChangedTableColumnValues);
					UpdateLsnHighWaterMark();
					LookForChangesAndSendListOfTables();
					return ChangedTableColumnValues.Any();
				}
			}
			catch (SqlException ex)
				when (new DbErrorMatch(ex).ExceptionType == DbErrorType.InvalidObjectName && ex.Message.Contains("$partition.PF_LsnPeriodFunction"))
			{
				Logger.Log(LogType.Debug, $@"Audit DB Partitioning required, deleting ""{BiConstants.LastIndexRebuildUtcDt}"" and nudging ASP Service task");
				BiMasterState.DeleteParameter(auditConnection, BiConstants.LastIndexRebuildUtcDt);
				ObjectFactory.Get<IServiceTaskNudger>().NudgeServiceTask("ASP");
				ShouldUpdateHighWaterMark = false;
			}
			return false;
		}

		bool CheckForMoreChanges()
		{
			if (HasChangesToProcess())
			{
				LookForChangesAndSendListOfTables();
				return ChangedTableColumnValues.Any();
			}
			return false;
		}

		void LookForChangesAndSendListOfTables()
		{
			var schemaResolver = ObjectFactory.Get<IApplicationSchemaResolver>();
			ChangedTableColumnValues = new Dictionary<IChangedTableSchema, List<string>>();
			while (!((CurrentLsnHighWaterMark != null && MaxLsn.SequenceEqual(CurrentLsnHighWaterMark)) && CurrentPeriodHighWaterMark == MaxLsnPeriod))
			{
				var tablesWithChangeDataTable = GetTablesWithChangesSinceLastCheck();

				if (NextLsnHighWaterMark != null)
				{
					if (ThereAreRelevantChanges(tablesWithChangeDataTable))
					{
						foreach (DataRow row in tablesWithChangeDataTable.Rows)
						{
							var dataRowVersion = row.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Default;
							var schemaName = row["SchemaName", dataRowVersion].ToString();
							var tableName = row["TableName", dataRowVersion].ToString();
							var pkName = row["PkName", dataRowVersion].ToString();
							var value = row["ChangedValue", dataRowVersion].ToString();
							var tableSchema = new ChangedTableSchema(schemaName, tableName, pkName);
							if (!ChangedTableColumnValues.Keys.Contains(tableSchema))
							{
								ChangedTableColumnValues.Add(tableSchema, new List<string>());
							}
							if (!ChangedTableColumnValues[tableSchema].Contains(value))
							{
								ChangedTableColumnValues[tableSchema].Add(value);
							}
						}
					}
					else
					{
						UpdateLsnHighWaterMark();
					}
				}
			}
		}

		DataTable GetTablesWithChangesSinceLastCheck()
		{
			using (var cmd = auditConnection.Command($"[{BiConstants.BiAdminSchemaName}].[usp_GetChangesBySchemaTableColumn]"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@LsnHighWaterMark", SqlDbType.Binary, 10, 0, 0, NextLsnHighWaterMark);
				cmd.AddParameter("@PeriodHighWaterMark", SqlDbType.SmallInt, -1, 0, 0, NextPeriodHighWaterMark);

				cmd.AddParameter("@LatestLsn", SqlDbType.Binary, 10, 0, 0, MaxLsn);
				cmd.AddParameter("@LatestPeriod", SqlDbType.SmallInt, MaxLsnPeriod);
				cmd.AddOutputParameter("@CurrentLsnHighWaterMark", SqlDbType.Binary, 10, 0, 0, null);
				cmd.AddOutputParameter("@CurrentPeriodHighWaterMark", SqlDbType.SmallInt, -1, 0, 0, null);

				var result = DataUtils.GetDataTableFromCommand(cmd);
				CurrentLsnHighWaterMark = GetByteOutputParameter(cmd, "@CurrentLsnHighWaterMark");
				CurrentPeriodHighWaterMark = GetSmallintOutputParameter(cmd, "@CurrentPeriodHighWaterMark");
				NextLsnHighWaterMark = CurrentLsnHighWaterMark;
				NextPeriodHighWaterMark = CurrentPeriodHighWaterMark;
				NextSeqValHighWaterMark = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

				return result;
			}
		}

		public void ProcessChanges(ILogger logger, Dictionary<IChangedTableSchema, List<string>> changedTableColumnValues)
		{
			logger.Log(LogType.Debug, $"> Processing changes for {changedTableColumnValues.Count} table(s)");
			((TableValuePairSubscriber)Subscriber).ProcessChanges(logger, changedTableColumnValues);
			logger.Log(LogType.Debug, "> Processing completed");
		}

		#endregion
	}
}
