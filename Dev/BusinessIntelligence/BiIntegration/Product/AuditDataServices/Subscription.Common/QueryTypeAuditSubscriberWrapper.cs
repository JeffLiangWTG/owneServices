namespace Enterprise.AuditDataServices.Subscription.Common
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.Linq;
	using CargoWise.Data;
	using CargoWise.Schema;
	using Enterprise.Integration;
	using Microsoft.CSharp.RuntimeBinder;

	public abstract class QueryTypeAuditSubscriberWrapper : AuditSubscriberWrapper, IQueryTypeAuditSubscriber
	{
		protected QueryTypeAuditSubscriberWrapper(IQueryTypeAuditSubscriber subscriber, DbConnection auditConnection, ILogger logger) : base(subscriber, auditConnection, logger)
		{
			this.CustomFilter = subscriber.CustomFilter;
		}
		public Action<DataRow> CustomFilter { get; }
		#region Properties and Fields

		public bool NotifyInsert => ((IQueryTypeAuditSubscriber)Subscriber).NotifyInsert;
		public bool NotifyUpdate => ((IQueryTypeAuditSubscriber)Subscriber).NotifyUpdate;
		public bool NotifyDelete => ((IQueryTypeAuditSubscriber)Subscriber).NotifyDelete;
		public IEnumerable<SchemaColumn> SpecificColumns => ((IQueryTypeAuditSubscriber)Subscriber).SpecificColumns;

		public string PKColumnName
		{
			get
			{
				var table = default(ITableSchema);

				if (string.IsNullOrEmpty(pKColumnName))
				{
					try
					{
						table = (ITableSchema)Subscriber.Table;
					}
					catch (RuntimeBinderException)
					{
						pKColumnName = "ChangedValue";
					}
				}

				return pKColumnName = pKColumnName ?? table.PK.Name;
			}
		}

		string pKColumnName;

		#endregion

		#region Filter Query Resultx

		/// <summary>
		/// Gets the rows of the change table that match the desired operation
		/// </summary>
		protected virtual DataTable FilterQueryResult(DataTable queryResult)
		{
			var changeTableFilteredByUpdate = FilterRowsByUpdate(queryResult);
			var changeTableFilteredByInsertDelete = FilterRowsByInsertDelete(changeTableFilteredByUpdate);
			return changeTableFilteredByInsertDelete;
		}

		DataTable FilterRowsByUpdate(DataTable queryResult)
		{
			var changeTable = queryResult.Clone();
			while (queryResult.Rows.Count > 1)
			{
				var beforeRow = queryResult.Rows[0];
				var afterRow = queryResult.Rows[1];

				if (RowsArePartOfSameTable(beforeRow, afterRow) && RowsAreAnUpdatePair(beforeRow, afterRow))
				{
					AddUpdateRowsForProcessing(queryResult, changeTable, beforeRow, afterRow);
				}
				else
				{
					AddInsertOrDeleteRowForProcessing(queryResult, changeTable, beforeRow);
				}
			}
			if (queryResult.Rows.Count == 1)
			{
				AddInsertOrDeleteRowForProcessing(queryResult, changeTable, queryResult.Rows[0]);
			}

			return changeTable;
		}

		DataTable FilterRowsByInsertDelete(DataTable filteredQueryResult)
		{
			var changeTable = filteredQueryResult.Clone();

			if (ThereAreRelevantChanges(filteredQueryResult))
			{
				DataRow beforeUpdateRow = null;
				DataRow deletedRow = null;
				byte[] deletedRowSeqVal = null;
				int operation;

				foreach (DataRow rowResult in filteredQueryResult.Rows)
				{
					DataRow rowChangeTable = ReadNextChange(out operation, changeTable, rowResult);
					ProcessRowsByOperation(ref beforeUpdateRow, ref deletedRow, ref deletedRowSeqVal, operation, changeTable, rowChangeTable);
				}
				ProcessFinalDeletedRow(ref deletedRow, ref deletedRowSeqVal, changeTable);
			}
			return changeTable;
		}

		bool RowsArePartOfSameTable(DataRow beforeRow, DataRow afterRow)
		{
			var result = true; // default is true as if we do not specify a table name we assume it comes from the same table
			if (beforeRow.Table.Columns.Contains("TableName"))
			{
				result = beforeRow["TableName"] == afterRow["TableName"];
			}
			return result;
		}

		internal bool RowsAreAnUpdatePair(DataRow beforeRow, DataRow afterRow)
		{
			return
				IsRegularUpdatePair(beforeRow, afterRow)
				|| (
					IsPotentiallyDeferredUpdate(beforeRow, afterRow)
					&& HaveSameLsn(beforeRow, afterRow)
					&& HaveSamePk(beforeRow, afterRow)
					);
		}

		bool HaveSamePk(DataRow beforeRow, DataRow afterRow)
		{
			var afterRowPk = (Guid)afterRow[PKColumnName];
			var beforeRowPk = (Guid)beforeRow[PKColumnName];
			return afterRowPk.Equals(beforeRowPk);
		}

		bool HaveSameLsn(DataRow beforeRow, DataRow afterRow)
		{
			var afterRowStartLsn = (byte[])afterRow[AuditFieldNames.StartLsnFieldName];
			var beforeRowStartLsn = (byte[])beforeRow[AuditFieldNames.StartLsnFieldName];
			return afterRowStartLsn.SequenceEqual(beforeRowStartLsn);
		}

		static bool IsRegularUpdatePair(DataRow beforeRow, DataRow afterRow)
		{
			var afterRowOperation = Convert.ToInt32(afterRow[AuditFieldNames.OperationFieldName]);
			var beforeRowOperation = Convert.ToInt32(beforeRow[AuditFieldNames.OperationFieldName]);
			return (beforeRowOperation == 3) && (afterRowOperation == 4);
		}

		public static bool IsPotentiallyDeferredUpdate(DataRow beforeRow, DataRow afterRow)
		{
			var afterRowOperation = Convert.ToInt32(afterRow[AuditFieldNames.OperationFieldName]);
			var beforeRowOperation = Convert.ToInt32(beforeRow[AuditFieldNames.OperationFieldName]);
			return (beforeRowOperation == 1) && (afterRowOperation == 2);
		}

		void AddUpdateRowsForProcessing(DataTable queryResult, DataTable changeTable, DataRow beforeRow, DataRow afterRow)
		{
			if (Convert.ToInt32(beforeRow[AuditFieldNames.OperationFieldName]) == 1)
			{
				beforeRow[AuditFieldNames.OperationFieldName] = 3;
				afterRow[AuditFieldNames.OperationFieldName] = 4;
			}

			if (NotifyUpdate)
			{
				if (SpecificColumns == null)
				{
					changeTable.Rows.Add(beforeRow.ItemArray);
					changeTable.Rows.Add(afterRow.ItemArray);
				}
				else
				{
					foreach (var col in SpecificColumns)
					{
						if (!beforeRow[col.Name].Equals(afterRow[col.Name]))
						{
							changeTable.Rows.Add(beforeRow.ItemArray);
							changeTable.Rows.Add(afterRow.ItemArray);
							break;
						}
					}
				}
			}

			beforeRow.Delete();
			afterRow.Delete();
			queryResult.AcceptChanges();
		}

		void AddInsertOrDeleteRowForProcessing(DataTable queryResult, DataTable changeTable, DataRow row)
		{
			changeTable.Rows.Add(row.ItemArray);
			row.Delete();
			queryResult.AcceptChanges();
		}

		#endregion

		#region Process Filtered Result

		public DataRow ReadNextChange(out int operation, DataTable changeTable, DataRow rowResult)
		{
			string[] columnsResult = GetColumnNamesFromDataTable(rowResult.Table);
			string[] columnsChangeTable = GetColumnNamesFromDataTable(changeTable);

			try
			{
				operation = Convert.ToInt32(rowResult[AuditFieldNames.OperationFieldName], CultureInfo.InvariantCulture);
			}
			catch (InvalidCastException)
			{
				var message = $"Failure during execution of {Code}. The change table yielded the following row where it could not parse {AuditFieldNames.StartLsnFieldName} or {AuditFieldNames.SeqValFieldName} or {AuditFieldNames.LsnPeriodFieldName}:";
				foreach (string columnName in columnsResult)
				{
					var value = rowResult[columnName] == DBNull.Value ? "NULL" : rowResult[columnName];
					message = message + "\r\n > " + columnName + " = " + value;
				}
				throw new InvalidCastException(message);
			}

			var rowChangeTable = changeTable.NewRow();
			foreach (var column in columnsChangeTable)
			{
				if (columnsResult.Contains(column))
				{
					rowChangeTable[column] = rowResult[column];
				}
			}
			return rowChangeTable;
		}

		public string[] GetColumnNamesFromDataTable(DataTable queryResult)
		{
			return queryResult.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();
		}

		void ProcessRowsByOperation(ref DataRow beforeUpdateRow, ref DataRow deletedRow, ref byte[] deletedRowSeqVal, int operation, DataTable changeTable, DataRow rowChangeTable)
		{
			if (deletedRow != null && (operation != 2 || !NextSeqValHighWaterMark.SequenceEqual(deletedRowSeqVal)) && NotifyDelete)
			{
				changeTable.Rows.Add(deletedRow);
				deletedRow.AcceptChanges();
				deletedRow.Delete();
				deletedRow = null;
				deletedRowSeqVal = null;
			}

			switch (operation)
			{
				case 1:
					if (NotifyDelete)
					{
						deletedRow = rowChangeTable;
						deletedRowSeqVal = NextSeqValHighWaterMark;
					}
					break;

				case 2:
					if (NotifyInsert)
					{
						changeTable.Rows.Add(rowChangeTable);
					}
					break;

				case 3:
					if (NotifyUpdate)
					{
						beforeUpdateRow = rowChangeTable;
					}
					break;

				case 4:
					if (beforeUpdateRow != null)
					{
						MatchBeforeUpdateRowWithRespectiveAfterUpdateOneIfHasRelevantChanges(changeTable, beforeUpdateRow, rowChangeTable);
						beforeUpdateRow = null;
					}
					break;
			}
		}

		void ProcessFinalDeletedRow(ref DataRow deletedRow, ref byte[] deletedRowSeqVal, DataTable changeTable)
		{
			if (deletedRow != null && NotifyDelete)
			{
				changeTable.Rows.Add(deletedRow);
				deletedRow.AcceptChanges();
				deletedRow.Delete();
				deletedRow = null;
				deletedRowSeqVal = null;
			}
		}

		void MatchBeforeUpdateRowWithRespectiveAfterUpdateOneIfHasRelevantChanges(DataTable changeTable, DataRow beforeUpdateRow, DataRow afterUpdateRow)
		{
			changeTable.Rows.Add(beforeUpdateRow);
			beforeUpdateRow.AcceptChanges();
			beforeUpdateRow.SetModified();

			foreach (DataColumn column in beforeUpdateRow.Table.Columns)
			{
				if (
					!column.ColumnName.StartsWith(CdcColumnPrefix, StringComparison.OrdinalIgnoreCase)
					&& !column.ColumnName.StartsWith(AfterUpdate, StringComparison.OrdinalIgnoreCase)
					&& !column.ColumnName.StartsWith(BeforeUpdate, StringComparison.OrdinalIgnoreCase)
				)
				{
					beforeUpdateRow[column] = afterUpdateRow[column];
				}
			}
		}

		const string CdcColumnPrefix = "__$";
		const string BeforeUpdate = "beforeUpdate";
		const string AfterUpdate = "afterUpdate";

		#endregion

	}
}
