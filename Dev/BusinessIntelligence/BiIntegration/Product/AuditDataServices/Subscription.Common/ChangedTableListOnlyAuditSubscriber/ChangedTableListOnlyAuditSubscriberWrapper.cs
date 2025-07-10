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
	[WTG.StaticAnalysis.Annotation.CodeAlive("Types implementing IAuditSubscriber are loaded by reflection.")]
	public class ChangedTableListOnlyAuditSubscriberWrapper : AuditSubscriberWrapper, IChangedTableListOnlyAuditSubscriber
	{
		public ChangedTableListOnlyAuditSubscriberWrapper(ChangedTableListOnlyAuditSubscriber subscriber, DbConnection auditConnection, ILogger logger) : base(subscriber, auditConnection, logger)
		{
		}

		#region Has Changes To Process

		public override bool HasChangesToProcess()
		{
			bool changesFound = TablesWithChangesSinceLastCheck != null && TablesWithChangesSinceLastCheck.Rows.Count > 0;
			return changesFound;
		}

		/// <summary>
		/// Get earliest new transaction changes (after current high water mark).
		/// LsnTimeMapping used to calculate the LSN period. So it is safe to use NOLOCK to avoid blocking the Audit ETL.
		/// </summary>
		DataTable GetTablesWithChangesSinceLastCheck()
		{
			using (var cmd = auditConnection.Command($"[{BiConstants.BiAdminSchemaName}].usp_GetChangesBySchemaAndTable"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@SubscriberCode", SqlDbType.Char, 3, Code);
				cmd.AddParameter("@LatestLsn", SqlDbType.Binary, 10, 0, 0, MaxLsn);
				cmd.AddParameter("@LatestPeriod", SqlDbType.SmallInt, MaxLsnPeriod);
				var columnMappingTable = GetSchemaAndTableAsTVP();
				cmd.AddTableValuedParameter("@SchemaTableNames", "TVP_SchemaTableColumnMapping", columnMappingTable);

				cmd.AddOutputParameter("@LatestSeqVal", SqlDbType.Binary, 10, 0, 0, DBNull.Value);

				var result = DataUtils.GetDataTableFromCommand(cmd);

				NextLsnHighWaterMark = MaxLsn;
				NextSeqValHighWaterMark = GetByteOutputParameter(cmd, "@LatestSeqVal");

				Logger.Log(LogType.Debug, $"> Current Subscriber High Water Marks (LSN: {ByteArrayToString(NextLsnHighWaterMark)}, SeqVal: {ByteArrayToString(NextSeqValHighWaterMark)}, Period: {NextPeriodHighWaterMark})");

				return result;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Test Cases")]
		byte[] GetBinaryOutputParameter(object objectBinaryOutput)
		{
			return (objectBinaryOutput == DBNull.Value) ? null : (byte[])objectBinaryOutput;
		}

		public IEnumerable<ITableSchema> SubscribedTables => ((ChangedTableListOnlyAuditSubscriber)Subscriber).SubscribedTables;

		public override bool HasChangesToProcessFromCache(Dictionary<string, Lsn> tableStateLsnHighWaterMarkCache, Dictionary<string, Lsn> subscriberLsnHighWaterMarkCache, Dictionary<string, int> subscriberCommandIdHighWaterMarkCache)
		{
			var tableNames = SubscribedTables.Select(t => $"[{t.SqlSchemaName}].[{t.TableName}]").ToArray();
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

		public override void ValidateSubscriber()
		{
			if (SubscribedTables == null)
			{
				throw new ArgumentException("Subscriber tables property cannot be null.");
			}
			else if (!SubscribedTables.Any())
			{
				throw new ArgumentException("Subscriber tables property cannot be empty.");
			}
			else if (SubscribedTables.Any(t => t == null))
			{
				throw new ArgumentException("Subscriber tables property cannot contain any null elements.");
			}
		}

		public override bool FetchDataAndProcessChanges()
		{
			if (HasChangesToProcess())
			{
				var changedTables =
					from ct in TablesWithChangesSinceLastCheck.AsEnumerable()
					join st in SubscribedTables
						on new { schemaName = ct["SchemaName"].ToString(), tableName = ct["TableName"].ToString() }
						equals new { schemaName = st.SqlSchemaName, tableName = st.TableName }
					select st;
				ProcessChanges(Logger, changedTables);
				TablesWithChangesSinceLastCheck = null;
				UpdateLsnHighWaterMark();
				return true;
			}
			return false;
		}

		public void ProcessChanges(ILogger logger, IEnumerable<ITableSchema> changedTables)
		{
			((ChangedTableListOnlyAuditSubscriber)Subscriber).ProcessChanges(logger, changedTables);
		}

		DataTable tablesWithChangesSinceLastCheck;
		DataTable TablesWithChangesSinceLastCheck
		{
			get
			{
				if (tablesWithChangesSinceLastCheck == null)
				{
					try
					{
						tablesWithChangesSinceLastCheck = GetTablesWithChangesSinceLastCheck();
					}
					catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.InvalidObjectName)
					{
						Logger.Log(LogType.Debug, "Audit DB Partitioning required, nudging ASP Service task");
						ObjectFactory.Get<IServiceTaskNudger>().NudgeServiceTask("ASP");
						ShouldUpdateHighWaterMark = false;
					}
				}
				return tablesWithChangesSinceLastCheck;
			}
			set
			{
				tablesWithChangesSinceLastCheck = value;
			}
		}

		DataTable GetSchemaAndTableAsTVP()
		{
			var dataTable = new DataTable();
			dataTable.Columns.Add("SchemaName", typeof(string));
			dataTable.Columns.Add("TableName", typeof(string));
			dataTable.Columns.Add("ColumnName", typeof(string));

			foreach (var elem in SubscribedTables)
			{
				var row = dataTable.NewRow();
				row["SchemaName"] = elem.SqlSchemaName;
				row["TableName"] = elem.TableName;
				row["ColumnName"] = string.Empty;
				dataTable.Rows.Add(row);
			}

			return dataTable;
		}
	}
}
