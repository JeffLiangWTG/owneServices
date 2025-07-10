using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.DataTransfer.Native.DB.Sql;
using SqlBulkCopyOptions = CargoWise.Data.Providers.Common.SqlBulkCopyOptions;

namespace Enterprise.DataTransfer.Native.DB
{
	/// <summary>
	/// Wrapper for RowFactory
	/// </summary>
	public class RowRepository : IRowRepository
	{
		public RowRepository(DbConnection connection) : this(connection, new RowFactory(connection, null)) { }
		public RowRepository(RowFactory rowFactory) : this(null, rowFactory) { }

		public RowRepository(DbConnection connection, RowFactory rowFactory)
		{
			this.rowFactory = rowFactory;
			this.connection = connection;
		}
		readonly RowFactory rowFactory;
		readonly DbConnection connection;

		RowFactory TempRowFactory => (tempRowFactoryLazyLoaded ?? (tempRowFactoryLazyLoaded = new RowFactory()));
		RowFactory tempRowFactoryLazyLoaded;

		IApplicationSchemaResolver SchemaResolver => schemaResolverLazyLoaded ?? (schemaResolverLazyLoaded = ObjectFactory.Get<IApplicationSchemaResolver>());
		IApplicationSchemaResolver schemaResolverLazyLoaded;

		#region SuppressResourceStringsCheckRegion

		#region Create

		public DataRow New(Table table)
		{
			var id = Guid.Empty;
			return Create(table, id);
		}

		public DataRow Create(Table table)
		{
			var id = Guid.NewGuid();
			return Create(table, id);
		}

		public DataRow Create(Table table, Guid id)
		{
			var tablePKName = table.Columns.PrimaryKey.Name;
			var row = rowFactory.New(table.Name);
			row[tablePKName] = id;
			row.Table.Rows.Add(row);
			return row;
		}

		#endregion

		#region Merge

		public DataRow Merge(DataRow from, DataRow to)
		{
			if (!from.HasSameSchema(to))
			{
				throw new System.Exception("Could not merge rows with different schema");
			}
			var primaryKey = to.PrimaryKey();

			foreach (var column in from.ToDictionary())
			{
				to[column.Key] = column.Value;
			}

			foreach (var column in primaryKey)
			{
				to[column.Key] = column.Value;
			}

			return to;
		}

		#endregion

		#region Find

		public DataRow FindByPK(Guid pk, string tableName)
		{
			var table = Table.Get(tableName);
			return Show(pk, table);
		}

		public DataRow Show(Guid pk, Table table)
		{
			if (pk == Guid.Empty)
			{
				throw new InvalidOperationException("No PK provided.");
			}
			var row = rowFactory.LoadFromPK(table.Name, pk);

			return row;
		}

		public DataRow Show(Criteria criteria, Table table)
		{
			return Show(new[] { criteria }, table);
		}

		public DataRow Show(IEnumerable<Criteria> criterias, Table table)
		{
			return ShowAll(criterias, table, 1)?.FirstOrDefault();
		}

		public DataRow[] ShowAll(IEnumerable<Criteria> criterias, Table table, int? maxRows = null)
		{
			foreach (var criteria in criterias)
			{
				if (criteria.TableName != table.Name)
				{
					throw new InvalidOperationException("Table name in Criteria is not correct");
				}
			}

			return LoadAll(criterias, table, maxRows);
		}

		DataRow[] LoadAll(IEnumerable<Criteria> criterias, Table table, int? maxRows, bool fetchOnlyFromLocalCache = false)
		{
			var originalCriterias = criterias;
			var filteredCriterias = originalCriterias.Where(c => !c.IsBinary);

			var filterQuery = ZQueryHelper.Generate(table, filteredCriterias, maxRows, fetchOnlyFromLocalCache);
			var dataRowList = Load(table.Name, filterQuery);
			if (dataRowList == null || dataRowList.Length == 0)
			{
				return null;
			}

			var binaryCriterias = originalCriterias.Where(c => c.IsBinary);
			return FilterBinaryDataRow(dataRowList, binaryCriterias).ToArray();
		}

		public DataRow[] Load(string tableName, ZQuery filterQuery)
		{
			return rowFactory.Load(tableName, filterQuery);
		}

		static IEnumerable<DataRow> FilterBinaryDataRow(IEnumerable<DataRow> dataRowList, IEnumerable<Criteria> binaryCriterias)
		{
			foreach (var row in dataRowList)
			{
				var byteRowValue = Array.Empty<byte>();
				var isMatchCriterias = true;

				foreach (var varbinaryCriteria in binaryCriterias)
				{
					if (!(row[varbinaryCriteria.ColumnName] is DBNull))
					{
						byteRowValue = (byte[])row[varbinaryCriteria.ColumnName];
					}

					if (BytesIsEqual(byteRowValue, GetBytes(varbinaryCriteria)))
					{
						continue;
					}

					isMatchCriterias = false;
					break;
				}

				if (isMatchCriterias)
				{
					yield return row;
				}
			}
		}

		static byte[] GetBytes(Criteria varbinaryCriteria)
		{
			switch (varbinaryCriteria.Value)
			{
				case byte[] bytes:
					return bytes;
				case DBNull nil:
					return Array.Empty<byte>();
				default:
					throw new InvalidOperationException(FormattableString.Invariant($"Unknown type: {varbinaryCriteria.Value?.GetType()}"));
			}
		}

		static bool BytesIsEqual(byte[] first, byte[] second)
		{
			if (first == null && second == null)
			{
				return true;
			}

			if (first == null || second == null)
			{
				return false;
			}

			if (first.Length != second.Length)
			{
				return false;
			}

			if (first.Length == 0)
			{
				return true;
			}

			for (int i = 0; i < first.Length; i++)
			{
				if (first[i] != second[i])
				{
					return false;
				}
			}

			return true;
		}

		public IList<DataRow> LoadMany(IList<IEnumerable<Criteria>> criterias, Table table)
		{
			var tableInfo = GetOrCreateTableColumnSets(table);
			var columnNamesExcludingBinary = new HashSet<string>(criterias[0].Where(x => !x.IsBinary).Select(x => x.ColumnName));
			var criteriaIndex = tableInfo.GetOrCreateIndex(columnNamesExcludingBinary);

			var criteriasCount = criterias.Count;
			if (criteriasCount == 1)
			{
				var row = criteriaIndex.FindRowInLocalCache(criterias[0])
					?? Show(criterias[0], table);
				return new List<DataRow>(1) { row };
			}

			(var localRowCount, var localRowsSparse) = FetchFromLocalCache(criterias, criteriaIndex);
			if (localRowCount == criteriasCount)
			{
				return localRowsSparse;
			}

			var criteriasToFetchFromDb = CriteriasExcludingLocal(criterias, localRowsSparse, localRowCount);
			const string rowIndexColumnName = "tmpNativeRowIndex";
			int criteriaCountPerRow = criteriasToFetchFromDb.First().Count();
			var columnNames = new HashSet<string>(criteriaCountPerRow);
			var columnNamesWithAnyNullValue = new HashSet<string>(criteriaCountPerRow);
			var rows = CreateRowsFromCriteria(criteriasToFetchFromDb, table, rowIndexColumnName, columnNames, columnNamesWithAnyNullValue);
			var tempTableName = tableInfo.GetTempTableName(columnNames, criteriasCount, tempTableUniqueifier);
			CreateTempTableIfNeeded(rowIndexColumnName, columnNames, table.Name, tempTableName);
			SaveRows(rows, rowIndexColumnName, columnNames, tempTableName);
			var dbRows = LoadMatchingRows(tableInfo, tempTableName, criteriasToFetchFromDb, rowIndexColumnName, columnNames, columnNamesWithAnyNullValue);

			var tempTable = rows[0].Table;
			tempTable.Rows.Clear();

			return MergeLocalAndDbRows(criteriasCount, localRowCount, localRowsSparse, dbRows);
		}

		/// <summary>
		/// An index on the DataTable in memory, based on a set of column names with non-binary data types from a criteria list.
		/// For large native XML files, the number of rows in memory for a table can be over 100k.
		/// An index lookup is essential when searching.
		/// The CargoWise EntityFramework has built-in automatic indexing for a single column, by creating a DataView (see class RowIndexManager).
		/// This is OK if the index is on the child's parent foreign key and the parent has a small number of children.
		/// However, a RatingHeader can have over 100k children and while most RateEntry, RateLines and RateLineItems have less than 5 children
		/// there are some cases where they have over 100.
		/// A single column index is not enough.
		/// So here we build and cache a DataView for a unique set of criteria names.
		/// </summary>
		class CriteriaIndex
		{
			readonly ITableSchema tableSchema;
			readonly ZDataTable dataTable;
			readonly Dictionary<string, int> columnNameToIndexOrder;
			readonly List<string> orderedColumnNames;

			readonly object[] rowKey;

			public CriteriaIndex(ITableSchema tableSchema, ZDataTable dataTable, HashSet<string> columnNamesExcludingBinary)
			{
				this.tableSchema = tableSchema;
				this.dataTable = dataTable;

				orderedColumnNames = new List<string>(columnNamesExcludingBinary);
				orderedColumnNames.Sort();

				columnNameToIndexOrder = new Dictionary<string, int>(columnNamesExcludingBinary.Count);
				for (int i = 0; i < orderedColumnNames.Count; ++i)
				{
					columnNameToIndexOrder.Add(orderedColumnNames[i], i);
				}

				rowKey = new object[columnNamesExcludingBinary.Count];
			}

			public DataRow FindRowInLocalCache(IEnumerable<Criteria> criterias)
			{
				bool hasBinaryCriteria = false;
				var dataView = GetOrCreateDataView();
				foreach (var criteria in criterias)
				{
					if (!criteria.IsBinary)
					{
						var rowKeyIndex = columnNameToIndexOrder[criteria.ColumnName];
						rowKey[rowKeyIndex] = criteria.Value;
					}
					else
					{
						hasBinaryCriteria = true;
					}
				}

				var viewRows = dataView.FindRows(rowKey);
				DataRow result = null;
				if (viewRows.Length > 0)
				{
					if (!hasBinaryCriteria)
					{
						result = viewRows[0].Row;
					}
					else
					{
						var binaryCriterias = criterias.Where(c => c.IsBinary);
						result = FilterBinaryDataRow(viewRows.Select(x => x.Row), binaryCriterias)
							.FirstOrDefault();
					}
				}

				return result;
			}

			DataView GetOrCreateDataView()
				=> view ?? (view = CreateDataView());
			DataView view;

			DataView CreateDataView()
			{
				string sort = string.Join(", ", orderedColumnNames.Select(x => tableSchema.GetSchemaColumn(x).Name));
				return new DataView(dataTable, string.Empty, sort, DataViewRowState.CurrentRows);
			}
		}

		/// <summary>
		/// Uniqueifier to ensure each instance has unique temp table names.
		/// </summary>
		readonly string tempTableUniqueifier = Guid.NewGuid().ToString("N");

		/// <summary>
		/// All the distinct sets of column names encountered in criterias for a single table.
		/// </summary>
		class TableColumnSets
		{
			readonly Table table;
			public ITableSchema Schema { get; }
			readonly ZDataTable dataTable;

			/// <summary>
			/// Indexed columns are columns with non-binary data types.
			/// </summary>
			readonly Dictionary<HashSet<string>, CriteriaIndex> indexedColumnSets = new Dictionary<HashSet<string>, CriteriaIndex>(HashSet<string>.CreateSetComparer());

			/// <summary>
			/// All distinct sets of column names encountered, including column with binary data types.
			/// Numbered from 0..N
			/// Used for assigning a temp table name.
			/// </summary>
			readonly Dictionary<HashSet<string>, int> allColumnSets = new Dictionary<HashSet<string>, int>(HashSet<string>.CreateSetComparer());

			public TableColumnSets(Table table, ITableSchema tableSchema, ZDataTable dataTable)
			{
				this.table = table;
				Schema = tableSchema;
				this.dataTable = dataTable;
			}

			public string Name => table.Name;

			public CriteriaIndex GetOrCreateIndex(HashSet<string> columnNamesExcludingBinary)
			{
				if (!indexedColumnSets.TryGetValue(columnNamesExcludingBinary, out var columnSet))
				{
					columnSet = new CriteriaIndex(Schema, dataTable, columnNamesExcludingBinary);
					indexedColumnSets.Add(columnNamesExcludingBinary, columnSet);
				}

				return columnSet;
			}

			public string GetTempTableName(HashSet<string> columnNames, long batchSize, string uniqueifier)
			{
				int batchSizeSuffix = batchSize < 10
					? 1
					: (batchSize < 100) ? 2 : 3;

				if (!allColumnSets.TryGetValue(columnNames, out var setId))
				{
					setId = allColumnSets.Count;
					allColumnSets.Add(columnNames, setId);
				}

				var tempTableName = "#" + table.Name + "_" + setId + "_" + batchSizeSuffix + "_" + uniqueifier;
				return tempTableName;
			}
		}

		readonly Dictionary<string, TableColumnSets> tableNameToColumnSets = new Dictionary<string, TableColumnSets>();
		readonly HashSet<string> createdTempTables = new HashSet<string>();

		TableColumnSets GetOrCreateTableColumnSets(Table table)
		{
			if (!tableNameToColumnSets.TryGetValue(table.Name, out var tableColumnSets))
			{
				var tableSchema = SchemaResolver.GetTableSchema(table.Name);
				var dataTable = rowFactory.GetTable(table.Name);
				tableColumnSets = new TableColumnSets(table, tableSchema, dataTable);
				tableNameToColumnSets.Add(table.Name, tableColumnSets);
			}

			return tableColumnSets;
		}

		(int localRowCount, DataRow[] localRowsSparse) FetchFromLocalCache(IList<IEnumerable<Criteria>> criterias, CriteriaIndex index)
		{
			var criteriasCount = criterias.Count;
			var localRowsSparse = new DataRow[criteriasCount];
			int localRowCount = 0;

			for (int i = 0; i < criteriasCount; ++i)
			{
				var row = index.FindRowInLocalCache(criterias[i]);
				if (row != null)
				{
					localRowsSparse[i] = row;
					++localRowCount;
				}
			}
			return (localRowCount, localRowsSparse);
		}

		static IList<IEnumerable<Criteria>> CriteriasExcludingLocal(IList<IEnumerable<Criteria>> criterias, DataRow[] localRowsSparse, int localRowCount)
		{
			var criteriasToFetchFromDb = criterias;
			if (localRowCount != 0)
			{
				var criteriasCount = criterias.Count;
				criteriasToFetchFromDb = new List<IEnumerable<Criteria>>(criteriasCount - localRowCount);
				for (int i = 0; i < criteriasCount; ++i)
				{
					if (localRowsSparse[i] == null)
					{
						criteriasToFetchFromDb.Add(criterias[i]);
					}
				}
			}
			return criteriasToFetchFromDb;
		}

		static DataRow[] MergeLocalAndDbRows(int criteriasCount, int localRowCount, DataRow[] localRowsSparse, DataRow[] dbRows)
		{
			var result = dbRows;
			if (localRowCount != 0)
			{
				int dbRowIndex = 0;
				for (int i = 0; i < criteriasCount; ++i)
				{
					if (localRowsSparse[i] == null)
					{
						localRowsSparse[i] = dbRows[dbRowIndex++];
					}
				}
				result = localRowsSparse;
			}

			return result;
		}

		/// <summary>
		/// Create rows from given criterias.
		/// </summary>
		/// <param name="criterias">list of criterias for each row</param>
		/// <param name="table">table definition</param>
		/// <param name="rowIndexColumnName">column name to contain row index</param>
		/// <param name="columnNames">will be populated with the column names found in the criteria</param>
		/// <param name="columnNamesWithAnyNullValue">will be populated with the column names found in the criteria that have any null values</param>
		/// <returns>array of rows, one for each criterias</returns>
		DataRow[] CreateRowsFromCriteria(
			IList<IEnumerable<Criteria>> criterias,
			Table table,
			string rowIndexColumnName,
			HashSet<string> columnNames,
			HashSet<string> columnNamesWithAnyNullValue)
		{
			columnNames.Clear();
			columnNamesWithAnyNullValue.Clear();
			var criteriasCount = criterias.Count;
			var rows = new DataRow[criteriasCount];
			var rowFactoryTable = TempRowFactory.GetTable(table.Name);
			if ((rowFactoryTable.PrimaryKey?.Length ?? 0) != 1)
			{
				throw new InvalidOperationException("Table does not have a single column primary key: " + table.Name);
			}
			var columns = rowFactoryTable.Columns;
			var rowIndexColumn = columns.Contains(rowIndexColumnName)
				? columns[rowIndexColumnName]
				: columns.Add(rowIndexColumnName, typeof(int));
			foreach (var criteria in criterias[0])
			{
				columnNames.Add(criteria.ColumnName);
			}

			for (int rowIndex = 0; rowIndex < criteriasCount; ++rowIndex)
			{
				var rowCriterias = criterias[rowIndex];
				var row = rowFactoryTable.NewRow();
				row[rowIndexColumn] = rowIndex;
				int criteriaCount = 0;
				foreach (var criteria in rowCriterias)
				{
					++criteriaCount;
					if (criteria.TableName != table.Name)
					{
						throw new InvalidOperationException("Table name in Criteria is not correct");
					}
					else if (rowIndex > 0 && !columnNames.Contains(criteria.ColumnName))
					{
						throw new InvalidOperationException("All criterias must have the same set of column names: " + criteria.ColumnName);
					}
					row[criteria.ColumnName] = criteria.Value;
					if (criteria.Value == null || criteria.Value == DBNull.Value)
					{
						columnNamesWithAnyNullValue.Add(criteria.ColumnName);
					}
				}
				if (criteriaCount != columnNames.Count)
				{
					throw new InvalidOperationException("All criterias must have the same number of columns: " + rowIndex.ToString(CultureInfo.InvariantCulture));
				}
				rows[rowIndex] = row;
			}

			return rows;
		}

		/// <summary>
		/// Load matching rows for the rows in the temporary table.
		/// If more than one match was found for any criteria, then silently take one.
		/// </summary>
		/// <param name="table">table definition</param>
		/// <param name="criterias">criterias for each row in the temporary table</param>
		/// <param name="rowIndexColumnName">row index column name of the temporary table</param>
		/// <param name="columnNames">column names to match</param>
		/// <param name="columnNamesWithAnyNullValue">column names to match that have any null values</param>
		/// <returns>An array with the same length as the temporary table.
		/// Each element containing the matching row found for that row in the temporary table or null if none</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		DataRow[] LoadMatchingRows(
			TableColumnSets table,
			string tempTableName,
			IList<IEnumerable<Criteria>> criterias,
			string rowIndexColumnName,
			HashSet<string> columnNames,
			HashSet<string> columnNamesWithAnyNullValue)
		{
			var rowCount = criterias.Count;
			var schemaResolver = SchemaResolver;
			var tableSchema = table.Schema;

			// Only need to load non-matching columns, since the matching columns will be the same.
			var loadedColumns = tableSchema.All.Cast<SchemaColumn>().Where(x => !columnNames.Contains(x.Name)).ToList();
			var loadedColumnsSql = string.Join(", ", loadedColumns.Select(col => "a." + col.Name));
			string sql = $@"select {rowIndexColumnName}, {loadedColumnsSql}
from {tempTableName} n
join {table.Name} a on {string.Join("\r\n\tand ", columnNames.Select(x => JoinCriteriaSql(tableSchema, "a", "n", x, columnNamesWithAnyNullValue)))}
order by {rowIndexColumnName};
truncate table {tempTableName};";

			var matchingRows = new DataRow[rowCount];

			DataTable loadedTable;
			using (var cmd = connection.Command(sql))
			{
				cmd.CommandTimeout = 0;
				loadedTable = DataUtils.GetDataTableFromCommand(cmd);
			}

			var rowFactoryTable = rowFactory.GetTable(table.Name);
			var targetPrimaryKeyColumn = rowFactoryTable.PrimaryKey[0];
			var sourcePrimaryKeyColumn = loadedTable.Columns[targetPrimaryKeyColumn.ColumnName];
			var rowIndexColumn = loadedTable.Columns[rowIndexColumnName];
			foreach (DataRow loadedRow in loadedTable.Rows)
			{
				object primaryKeyValue = loadedRow[sourcePrimaryKeyColumn];
				var rowWithSamePk = rowFactoryTable.Rows.Find(primaryKeyValue);
				var rowIndex = (int)loadedRow[rowIndexColumn];
				if (rowWithSamePk == null)
				{
					if (matchingRows[rowIndex] == null)
					{
						var newRow = rowFactoryTable.NewRow();
						matchingRows[rowIndex] = newRow;
						newRow.BeginEdit();
						foreach (var col in loadedColumns)
						{
							if (rowFactoryTable.Columns.Contains(col.Name))
							{
								newRow[col.Name] = ZSqlLoader.GetUncompressedSanitisedRowValue(col.Name, loadedRow[col.Name], col) ?? DBNull.Value;
							}
						}
						var rowCriterias = criterias[rowIndex];
						foreach (var criteria in rowCriterias)
						{
							newRow[criteria.ColumnName] = criteria.Value;
						}
						newRow.EndEdit();
						rowFactoryTable.Rows.Add(newRow);
						newRow.AcceptChanges();
					}
					else
					{
						// Multiple matching records found.
						// Native XML just silently takes the first.
					}
				}
				else
				{
					// Existing row matches another item in the criterias list.
					// The criterias must be exact duplicates, so nothing to do other than store the matching row.
					matchingRows[rowIndex] = rowWithSamePk;
				}
			}

			return matchingRows;
		}

		string JoinCriteriaSql(ITableSchema tableSchema, string mainTableName, string tempTableName, string columnName, HashSet<string> columnNamesWithAnyNullValue)
		{
			var columnSchema = tableSchema.GetSchemaColumn(columnName);

			var mainColumnName = mainTableName + "." + columnName;
			var tempColumnName = tempTableName + "." + columnName;
			var nullPart = columnNamesWithAnyNullValue.Contains(columnName)
				? "(" + mainColumnName + " is null and " + tempColumnName + " is null) or "
				: "";

			string result;
			if (!columnSchema.IsLargeBinaryOrText)
			{
				result = "(" + nullPart + mainColumnName + " = " + tempColumnName + ")";
			}
			else
			{
				string uncompressFunction = columnSchema.IsBinary ? "CLRUncompressAsBytes" : "CLRUncompressAsString";
				result = "(" + nullPart + "dbo." + uncompressFunction + "(" + mainColumnName + ") = " + tempColumnName + ")";
			}

			return result;
		}

		void SaveRows(DataRow[] rows, string rowIndexColumnName, IEnumerable<string> columnNames, string tempTableName)
		{
			var internalConnection = (IDbConnectionInternals)connection;

			using (var bulkCopy = connection.GetSqlBulkCopy(SqlBulkCopyOptions.FireTriggers | SqlBulkCopyOptions.CheckConstraints, internalConnection.ADOTransaction))
			{
				bulkCopy.BulkCopyTimeout = 0;
				bulkCopy.BatchSize = rows.Length;
				bulkCopy.DestinationTableName = tempTableName;

				bulkCopy.ColumnMappings.Add(rowIndexColumnName, rowIndexColumnName);
				foreach (var columnName in columnNames)
				{
					bulkCopy.ColumnMappings.Add(columnName, columnName);
				}

				try
				{
					bulkCopy.WriteToServer(rows);
				}
				catch (InvalidOperationException ex)
				{
					var moreInformativeMessage = "BulkCopy failure: " + tempTableName + " (" + string.Join(", ", columnNames) + ")";
					throw new InvalidOperationException(moreInformativeMessage, ex);
				}
			}
		}

		void CreateTempTableIfNeeded(string rowIndexColumnName, HashSet<string> columnNames, string tableName, string tempTableName)
		{
			if (!createdTempTables.Contains(tempTableName))
			{
				var columnNamesSql = string.Join(", ", columnNames);
				string sql =
$@"if (object_id(N'tempdb..{tempTableName}', N'U') is not null) drop table {tempTableName};
select top 0 {rowIndexColumnName} = cast(0 as int), {columnNamesSql}
into {tempTableName}
from {tableName};";
				connection.ExecuteNonQuery(sql);
				createdTempTables.Add(tempTableName);
			}
		}

		#endregion

		public void Delete(DataRow row)
		{
			row.Delete();
		}

		public void Save()
		{
			rowFactory.Save();
			DropTempTables();
		}

		void DropTempTables()
		{
			if (createdTempTables.Count == 0)
			{
				return;
			}

			var stringBuilder = new StringBuilder();
			foreach (var tempTableName in createdTempTables)
			{
				var sql = $@"drop table {tempTableName};";
				stringBuilder.AppendLine(sql);
			}
			connection.ExecuteNonQuery(stringBuilder.ToString());
			createdTempTables.Clear();
		}

		#endregion

#if DEBUG
		public IEnumerable<string> GetCreatedTempTablesForTest()
			=> createdTempTables;
#endif
	}
}
