using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using CargoWise.Schema;
using SqlBulkCopyOptions = CargoWise.Data.Providers.Common.SqlBulkCopyOptions;

namespace CargoWise.EntityFramework
{
	public class ZSqlSaver : ZSqlSaverBase
	{
		public ZSqlSaver(DataSet data, DbConnection connection, IApplicationSchemaResolver schemaResolver)
			: base(data, connection, schemaResolver)
		{
		}

		public ZSqlSaver(DataTable dataTable, DbConnection connection, IApplicationSchemaResolver schemaResolver)
			: base(dataTable, connection, schemaResolver)
		{
		}

		public ZSqlSaver(DataSet data, ZSqlConnectionInfo connectionInfo, IApplicationSchemaResolver schemaResolver)
			: base(data, connectionInfo, schemaResolver)
		{
		}

		#region command builder getter

		public override ZSqlCommandBuilder GetZBulkUpdateCommandBuilder(IEnumerable<DataRow> rowsToUpdate, string tableName, bool compress, ITableSchema schema)
		{
			return new ZBulkUpdateCommandBuilder(rowsToUpdate, tableName, compress, schema);
		}

		public override ZSqlCommandBuilder GetZUpdateCommandBuilder(DataRow row, string tableName, bool compress, int statementIndex, ITableSchema schema, bool isBulk = false, StringBuilder commandText = null, List<ILargeColumnSaver> largeColumnSavers = null)
		{
			return new ZUpdateCommandBuilder(row, tableName, compress, statementIndex, schema, isBulk, commandText, largeColumnSavers);
		}

		public override ZSqlCommandBuilder GetZDeleteCommandBuilder(DataRow row, string tableName, bool compress, int statementIndex, ITableSchema schema)
		{
			return new ZDeleteCommandBuilder(row, tableName, compress, statementIndex, schema);
		}

		public override ZSqlCommandBuilder GetZInsertCommandBuilder(string tableName, bool compress, int statementIndex, ITableSchema schema, int expectedNumberOfRows)
		{
			return new ZInsertCommandBuilder(tableName, compress, statementIndex, schema, expectedNumberOfRows);
		}

		#endregion

		protected override bool HasDataSourceColumn(DataRow row)
		{
			if (row is ZDataRow zDataRow && row.Table != null)
			{
				var columns = row.Table.Columns?.Cast<DataColumn>();
				return columns.Any(col => zDataRow.HasSource(col.ColumnName));
			}
			return false;
		}

		protected override bool IsRowValueChanged(DataRow row, DataColumn column)
		{
			return ZDataUtils.IsRowValueChanged(row, column);
		}

		protected override bool DoesUpdateRowHavePersistentChangesFromOriginal(IApplicationSchemaResolver resolver, DataRow row)
		{
			return ZDataUtils.DoesUpdateRowHavePersistentChangesFromOriginal(resolver, row);
		}

		protected override int SaveSplitRows(IList<DataRow> rows, int startingIndex, bool canConsolidateInserts = true, bool isBulkUpdate = false)
		{
			var result = ShouldBulkCopy(rows, startingIndex, out var bulkCopyRows)
				? SaveBulkRows(bulkCopyRows, startingIndex)
				: base.SaveSplitRows(rows, startingIndex, canConsolidateInserts, isBulkUpdate);

			return result;
		}

		protected override void SaveRows(IList<DataRow> rows)
		{
			var changedTables = rows.Select(r => r.Table);
#if NETFRAMEWORK
			var distinctOnes = changedTables.DistinctBy(t => t.TableName);
#else
			var distinctOnes = Enumerable.DistinctBy(changedTables, t => t.TableName);
#endif
			var bulkCopySettings = distinctOnes.Where(IsBulkCopying)
				.Select(t => t.ExtendedProperties[typeof(BulkCopySetting)] as BulkCopySetting);
			foreach (var setting in bulkCopySettings)
			{
				setting.BulkCopying();
			}

			base.SaveRows(rows);
		}

		int SaveBulkRows(IList<DataRow> bulkSaveRows, int startingIndex)
		{
			var masterTable = bulkSaveRows[0].Table;
			var bulkCopySetting = masterTable.ExtendedProperties[typeof(BulkCopySetting)] as BulkCopySetting;
			using var bulkCopy = ConnectionInfo.DbConnection.GetSqlBulkCopy((SqlBulkCopyOptions)bulkCopySetting.Options, ((IDbConnectionInternals)ConnectionInfo.DbConnection).InternalDbTransaction);
			var sqlBulkCopy = bulkCopy as SqlServerBulkCopy;
			Argument.NotNull(sqlBulkCopy, nameof(sqlBulkCopy));

			sqlBulkCopy!.BulkCopyTimeout = 0;
			sqlBulkCopy.BatchSize = bulkCopySetting.BatchSize;
			sqlBulkCopy.DestinationTableName = masterTable.TableName;

			foreach (DataColumn column in masterTable.Columns)
			{
				sqlBulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
			}

			try
			{
				var fullPathAndTableName = ConnectionInfo.PathToTables + masterTable.TableName;
				var schema = SchemaCache.GetOrAdd(fullPathAndTableName, () => SchemaResolver.GetTableSchema(fullPathAndTableName.Split('.').Last()));
				using var bulkCopyDataReader = new BulkCopyDataRowsReader(bulkSaveRows, schema);
				sqlBulkCopy.WriteToServer(bulkCopyDataReader);
			}
			catch (SqlException sqlEx) when (DbErrorMatch.GetExceptionType(sqlEx) == DbErrorType.CannotInsertDuplicateUniqueIndexKey)
			{
				throw new ZUniqueIndexViolationException(sqlEx,
					ConnectionInfo.DbConnection,
					masterTable,
					ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(masterTable.TableName));
			}
			catch (Exception standardException) when (!standardException.IsCriticalException())
			{
				var parsedErrorPks = ParseErrorPkFromExceptionMessage(standardException.Message);
				var rowsWithError = bulkSaveRows.Where(row => parsedErrorPks.Contains(DataUtils.GetPk(row))).ToList();

				throw new ZDataException(standardException, rowsWithError, true, ConnectionInfo.DbConnection);
			}

			return startingIndex + bulkSaveRows.Count;
		}

		static bool ShouldBulkCopy(IList<DataRow> rows, int startingIndex, out IList<DataRow> bulkCopyRows)
		{
			var shouldBulkCopy = false;
			bulkCopyRows = null;
			if (ShouldStartCollectingBulkCopyRows(rows, startingIndex))
			{
				bulkCopyRows = new List<DataRow>();
				var masterTable = rows[startingIndex].Table;
				var currentIndex = startingIndex;
				while (currentIndex < rows.Count)
				{
					bulkCopyRows.Add(rows[currentIndex]);
					var nextRow = currentIndex + 1 < rows.Count ? rows[currentIndex + 1] : null;
					if (!IsNextRowSameTableInsert(nextRow, masterTable.TableName))
					{
						break;
					}

					currentIndex++;
				}

				var bulkCopySetting = masterTable.ExtendedProperties[typeof(BulkCopySetting)] as BulkCopySetting;
				if (bulkCopySetting != null && bulkCopyRows.Count >= bulkCopySetting.Threshold)
				{
					shouldBulkCopy = true;
				}
			}

			return shouldBulkCopy;
		}

		static bool IsBulkCopying(DataTable dataTable)
		{
			if (dataTable?.ExtendedProperties[typeof(BulkCopySetting)] is BulkCopySetting bulkCopySetting)
			{
				var tableWithNewRows = dataTable.GetChanges(DataRowState.Added);
				if (tableWithNewRows != null)
				{
					return tableWithNewRows.Rows.Cast<DataRow>().Count(r =>
								ShouldRowBeBulkSaved(bulkCopySetting.CheckRowsShouldBePersistent, r)) >= bulkCopySetting.Threshold;
				}
			}

			return false;
		}

		static bool ShouldRowBeBulkSaved(bool checkRowsShouldBePersistent, DataRow row) => !checkRowsShouldBePersistent || ZDataUtils.ShouldRowBeSaved(row);
	}
}
