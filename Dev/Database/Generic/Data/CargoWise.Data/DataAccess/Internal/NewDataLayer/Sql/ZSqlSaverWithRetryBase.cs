using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	public class ZSqlSaverWithRetryBase : ZSqlSaverBase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public ZSqlSaverWithRetryBase(DataSet data, DbConnection connection, IApplicationSchemaResolver schemaResolver)
			: base(data, connection, schemaResolver) { }

		public ZSqlSaverWithRetryBase(DataTable dataTable, DbConnection connection, IApplicationSchemaResolver schemaResolver)
			: base(dataTable, connection, schemaResolver) { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public ZSqlSaverWithRetryBase(DataSet data, ZSqlConnectionInfo connectionInfo, IApplicationSchemaResolver schemaResolver)
			: base(data, connectionInfo, schemaResolver) { }

		protected override void SaveRows(IList<DataRow> rows)
		{
			disabledUniqueIndexes = new List<UniqueIndexInfo>();
			try
			{
				SaveRowsWithRetry(rows);
			}
			finally
			{
				RebuildDisabledUniqueIndexes();
				disabledUniqueIndexes = null;
			}
		}

		void SaveRowsWithRetry(IList<DataRow> rows)
		{
			try
			{
				base.SaveRows(rows);
			}
			catch (ZDataException dataException)
			{
				string uniqueIndexName;

				SqlException innerSqlException = dataException.InnerException as SqlException;
				DbErrorMatch dbErrorMatch = innerSqlException != null ? new DbErrorMatch(innerSqlException) : null;

				if (dbErrorMatch != null &&
					dbErrorMatch.ExceptionType == DbErrorType.CannotInsertDuplicateUniqueIndexKey &&
					dataException.Row != null &&
					!string.IsNullOrEmpty(uniqueIndexName = dbErrorMatch.GetIndexNameIfUniqueIndexViolation()) &&
					!UniqueIndexWasDisabled(dataException.Row.Table.TableName, uniqueIndexName) &&
					(rows = GetUnprocessedRows(rows, dataException.Row)) != null && rows.Count > 0)
				{
					DisableUniqueIndex(new UniqueIndexInfo(dataException.Row.Table.TableName, uniqueIndexName));
					SaveRowsWithRetry(rows);
				}
				else
				{
					throw;
				}
			}
		}

		IList<DataRow> GetUnprocessedRows(IList<DataRow> rows, DataRow failedRow)
		{
			int failedIndex = -1; // Failed row is also first unprocessed row
			for (int i = 0; i < rows.Count; i++)
			{
				if (rows[i] == failedRow)
				{
					failedIndex = i;
					break;
				}
			}

			if (failedIndex >= 0 && failedIndex < (rows.Count - 1))
			{
				List<DataRow> unprocessedRows = new List<DataRow>(rows.Count - failedIndex);
				for (int i = failedIndex; i < rows.Count; i++)
				{
					unprocessedRows.Add(rows[i]);
				}
				return unprocessedRows;
			}

			return null;
		}

		#region Unique Indexes

		void DisableUniqueIndex(UniqueIndexInfo indexInfo)
		{
			disabledUniqueIndexes.Add(indexInfo);
			ConnectionInfo.DbConnection.ExecuteNonQuery(string.Format("ALTER INDEX {1} ON {0} DISABLE;", indexInfo.TableName, indexInfo.IndexName)); // this is a sql statement

#if DEBUG
			lastDisableIndexNameForTest = indexInfo.IndexName;
#endif
		}

		void RebuildDisabledUniqueIndexes()
		{
			if (disabledUniqueIndexes != null)
			{
				foreach (UniqueIndexInfo indexInfo in disabledUniqueIndexes)
				{
					ConnectionInfo.DbConnection.ExecuteNonQuery(string.Format("ALTER INDEX {1} ON {0} REBUILD;", indexInfo.TableName, indexInfo.IndexName)); // this is a sql statement
				}
				disabledUniqueIndexes.Clear();
			}
		}

		List<UniqueIndexInfo> disabledUniqueIndexes;

		struct UniqueIndexInfo
		{
			public UniqueIndexInfo(string tableName, string indexName)
			{
				TableName = tableName;
				IndexName = indexName;
			}

			public readonly string TableName;
			public readonly string IndexName;
		}

		bool UniqueIndexWasDisabled(string tableName, string indexName)
		{
			return disabledUniqueIndexes != null && disabledUniqueIndexes.Any(indexInfo => indexInfo.TableName == tableName && indexInfo.IndexName == indexName);
		}

		#endregion

		#region Testing stuff
#if DEBUG
		public string lastDisableIndexNameForTest;
#endif
		#endregion
	}
}
