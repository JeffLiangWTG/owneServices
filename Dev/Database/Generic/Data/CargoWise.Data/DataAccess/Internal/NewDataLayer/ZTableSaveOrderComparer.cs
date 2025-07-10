using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.Common.Testing;
using CargoWise.Data;
using CargoWise.Database.Abstractions;
using CargoWise.Schema;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// An IComparer implementation for save order based on foreign keys.
	/// 
	/// This is not a complete implementation because of implicit dependencies.
	/// For example, the order of table A = B and B = C but A > C. This is not consistent with the contract of IComparer,
	/// but for perforance reasons it's the best that can be achieved.
	/// 
	/// Call the Sort() method to sort an array of tables correctly.
	/// </summary>
	public class ZTableSaveOrderComparer : IComparer<DataTable>, IComparer<ITableSchema>
	{
		public ZTableSaveOrderComparer(DataTable[] tablesThatNeedToBeSaved)
			: this(Array.ConvertAll(tablesThatNeedToBeSaved, delegate(DataTable table)
			{ return ToITable(table); }))
		{
		}

		public ZTableSaveOrderComparer(ITableSchema[] tablesThatNeedToBeSaved)
			: this(Array.ConvertAll(tablesThatNeedToBeSaved, delegate(ITableSchema table)
			{ return ToITable(table); }))
		{
		}

		ZTableSaveOrderComparer(ITable[] tablesThatNeedToBeSaved)
		{
			this.tablesThatNeedToBeSaved = tablesThatNeedToBeSaved;
		}

		public static DataTable[] Sort(DataTable[] tables)
		{
			return Array.ConvertAll(SortByDatabase(Array.ConvertAll(tables, ToITable)), ToDataTable);
		}

		public static ITableSchema[] Sort(ITableSchema[] tables)
		{
			return Array.ConvertAll(SortByDatabase(Array.ConvertAll(tables, ToITable)), ToTableSchema);
		}

		#region Sort

		static ITable[] SortByDatabase(ITable[] tables)
		{
			var databases = new Dictionary<string, List<ITable>>();
			foreach (var table in tables)
			{
				var databaseName = GetDatabaseName(table.TableName);
				List<ITable> list;
				if (!databases.TryGetValue(databaseName, out list))
				{
					list = new List<ITable>();
					databases.Add(databaseName, list);
				}
				list.Add(table);
			}
			var result = new List<ITable>();
			foreach (var pair in databases)
			{
				result.AddRange(Sort(pair.Value.ToArray()));
			}
			return result.ToArray();
		}

		static string GetDatabaseName(string tableName)
		{
			var result = "";
			var match = RefDbRegex.Match(tableName);
			if (match.Success)
			{
				result = match.Groups[1].Value;
			}
			return result;
		}

		static Regex RefDbRegex
		{
			get
			{
				if (refDbRegex == null)
				{
					refDbRegex = new Regex(@"^" + RefDbTableNameResolver.RefDbAffix + @"([a-zA-Z0-9]{5})_\S*$", RegexOptions.Compiled);
				}

				return refDbRegex;
			}
		}

		[ThreadStatic]
		static Regex refDbRegex;

		static ITable[] Sort(ITable[] tables)
		{
			ITable[] result = (ITable[])tables.Clone();
			ZTableSaveOrderComparer comparer = new ZTableSaveOrderComparer(tables);
			result = result.OrderBy(t => t, new ComparePushingTablesWithUnconstraintedForeignKeysToEnd(comparer)).ToArray();

			int iterations = 0;
			while (Sort(result, 0, result.Length, ref iterations))
			{
				if (iterations > 10000)
				{
					ErrorReporter.ReportOnce("TooManyIterationsOfTableSaveOrderSort",
						string.Format("Over 10000 iterations in ZTableSaveOrderComparer.Sort for tables '{0}'\r\n\r\nCurrent state of sorted array: {1}\r\nCache count: {2}\r\nValues: {3}",
						GetTableNames(tables),
						result.Length > iterations ? result[iterations].TableName : result.Last().TableName,
						saveOrderCache.StrongCacheCount,
						GetTablesCompareValues(tables, comparer)));
					break;
				}
			}
			return result;
		}

		static string GetTablesCompareValues(ITable[] tables, ZTableSaveOrderComparer comparer)
		{
			var sb = new StringBuilder();
			for (int i = 0; i < tables.Length - 1; i++)
			{
				for (int j = i + 1; j < tables.Length; j++)
				{
					var table1 = tables[i];
					var table2 = tables[j];
					var tablesComparison = comparer.Compare(table1, table2);

					sb.AppendLine();
					sb.AppendFormat("{0}-{1}: {2}", table1.TableName, table2.TableName, tablesComparison.ToString());
				}
			}
			return sb.ToString();
		}

		class ComparePushingTablesWithUnconstraintedForeignKeysToEnd : IComparer<ITable>
		{
			internal ComparePushingTablesWithUnconstraintedForeignKeysToEnd(ZTableSaveOrderComparer tableSaveOrderComparer)
			{
				this.tableSaveOrderComparer = tableSaveOrderComparer;
			}

			readonly ZTableSaveOrderComparer tableSaveOrderComparer;

			public int Compare(ITable x, ITable y)
			{
				int result = tableSaveOrderComparer.Compare(x, y);
				if (result == 0)
				{
					if (HasParentGuidColumn(x) && !HasParentGuidColumn(y))
					{
						result = 1;
					}
					if (!HasParentGuidColumn(x) && HasParentGuidColumn(y))
					{
						result = -1;
					}
				}
				return result;
			}
		}

		static bool Sort(ITable[] tables, int from, int length, ref int iterations)
		{
			ZTableSaveOrderComparer comparer = new ZTableSaveOrderComparer(tables);
			bool madeChanges = false;

			for (int i = from; i < length; i++)
			{
				for (int j = i + 1; j < length; j++)
				{
					iterations++;
					if (comparer.Compare(tables[i], tables[j]) > 0)
					{
						Insert(tables, i, j);
						Sort(tables, i, j - i, ref iterations);
						madeChanges = true;
						break;
					}
				}
			}
			return madeChanges;
		}

		static void Insert<T>(T[] array, int insertAt, int from)
		{
			T itemToInsert = array[from];
			Array.Copy(array, insertAt, array, insertAt + 1, from - insertAt);
			array[insertAt] = itemToInsert;
		}

		static string GetTableNames(IEnumerable<ITable> tables)
		{
			return string.Join(",", tables.Select(table => table.TableName));
		}

		#endregion

		#region Compare

		public int Compare(DataTable x, DataTable y)
		{
			return Compare(ToITable(x), ToITable(y));
		}

		public int Compare(ITableSchema x, ITableSchema y)
		{
			return Compare(ToITable(x), ToITable(y));
		}

		int Compare(ITable x, ITable y)
		{
			int? result = 0;
			if (x.TableName != y.TableName)
			{
				result = saveOrderCache[new ParentChildTablePair(x, y)];
				if (result == null)
				{
					bool canCacheOutcome;
					result = CompareSaveOrderNoCache(x, y, out canCacheOutcome);
					if (canCacheOutcome)
					{
						saveOrderCache.Add(new ParentChildTablePair(x, y), result);
						saveOrderCache.Add(new ParentChildTablePair(y, x), -result);
					}
				}
			}
			return (int)result;
		}

		int CompareSaveOrderNoCache(ITable x, ITable y, out bool canCacheOutcome)
		{
			canCacheOutcome = true;
			var result = ZTableSaveOrderComparerWhiteList.Compare(x.TableName, y.TableName);

			if (result == 0)
			{
				bool isParent, isChild;
				GetIsParentAndIsChildUsingAllEnterpriseTables(x, y, out isParent, out isChild);
				if (isParent && isChild)
				{
					canCacheOutcome = false;
					GetIsParentAndIsChildUsingOnlyTablesToBeSaved(x, y, out isParent, out isChild);
					if (isParent && isChild)
					{
						throw new InvalidOperationException("Circular dependency between tables " + x.TableName + " and " + y.TableName + " in the DataSet");
					}
				}
				if (isParent)
				{
					result = -1;
				}
				else if (isChild)
				{
					result = 1;
				}
			}

			return result;
		}

		#endregion

		#region HasParentGuidColumn

		[SuppressThreadStaticFieldMessage]
		static readonly LRUCache<string, bool?> hasParentGuidColumnCache = new LRUCache<string, bool?>();

		static bool HasParentGuidColumn(ITable table)
		{
			return hasParentGuidColumnCache[table.TableName] ?? HasParentGuidColumnUncached(table);
		}

		static bool HasParentGuidColumnUncached(ITable table)
		{
			foreach (string columnName in table.ColumnNames)
			{
				if ((columnName.EndsWith(Schema.Schema.ParentIDColumnSuffix) || columnName.EndsWith("_Parent") || columnName.EndsWith("_ParentGUID") || columnName.EndsWith("_ForeignKey")) &&
				!IsConstrainedForeignKey(columnName))
				{
					hasParentGuidColumnCache.Add(table.TableName, true);
					return true;
				}
			}
			hasParentGuidColumnCache.Add(table.TableName, false);
			return false;
		}

		static bool IsConstrainedForeignKey(string columnName)
		{
			var colNameParts = columnName.Split('_');
			return (colNameParts.Length >= 3 && colNameParts[0].Length >= 2 && colNameParts[0].Length <= 3 && colNameParts[1].Length >= 2 && colNameParts[1].Length <= 3);
		}

		#endregion

		#region ParentChildTablePair

		struct ParentChildTablePair
		{
			public ParentChildTablePair(ITable parentTable, ITable childTable)
				: this(parentTable.TableName, childTable.TableName)
			{
			}

			public ParentChildTablePair(string parentTableName, string childTableName)
			{
				this.parentTableName = parentTableName;
				this.childTableName = childTableName;
			}

			public override bool Equals(object obj)
			{
				bool result = false;
				if (obj is ParentChildTablePair)
				{
					ParentChildTablePair rhs = (ParentChildTablePair)obj;
					result =
						parentTableName == rhs.parentTableName &&
						childTableName == rhs.childTableName;
				}
				return result;
			}

			public override int GetHashCode()
			{
				return parentTableName.GetHashCode() ^ childTableName.GetHashCode();
			}

			readonly string parentTableName;
			readonly string childTableName;
		}

		#endregion

		#region ITable / Cast

		interface ITable
		{
			string TableName { get; }
			IEnumerable<string> ColumnNames { get; }
		}

		static DataTable ToDataTable(ITable table)
		{
			return ((DataTableAdapter)table).Table;
		}

		static ITableSchema ToTableSchema(ITable table)
		{
			return ((TableSchemaAdapter)table).Table;
		}

		static ITable ToITable(DataTable table)
		{
			return DataTableAdapter.New(table);
		}

		static ITable ToITable(ITableSchema table)
		{
			return TableSchemaAdapter.New(table);
		}

		[DebuggerDisplay("TableName={TableName}")]
		class TableSchemaAdapter : ITable
		{
			protected TableSchemaAdapter(ITableSchema table)
			{
				this.Table = table;
			}

			public static ITable New(ITableSchema table)
			{
				return table == null ? null : new TableSchemaAdapter(table);
			}

			public string TableName
			{
				get { return Table.TableName; }
			}

			public IEnumerable<string> ColumnNames
			{
				get
				{
					List<string> result = new List<string>();
					foreach (SchemaColumn column in Table.All)
					{
						result.Add(column.Name);
					}
					return result.ToArray();
				}
			}

			public ITableSchema Table { get; set; }
		}

		[DebuggerDisplay("TableName={TableName}")]
		class DataTableAdapter : ITable
		{
			protected DataTableAdapter(DataTable table)
			{
				this.Table = table;
			}

			public static ITable New(DataTable table)
			{
				return table == null ? null : new DataTableAdapter(table);
			}

			public string TableName
			{
				get { return Table.TableName; }
			}

			public IEnumerable<string> ColumnNames
			{
				get
				{
					List<string> result = new List<string>();
					foreach (DataColumn column in Table.Columns)
					{
						result.Add(column.ColumnName);
					}
					return result.ToArray();
				}
			}

			public DataTable Table { get; set; }
		}

		#endregion

		#region Implementation

		#region Just for test
#if DEBUG

		public static void CleanSaveOrderCacheForTest() => saveOrderCache.Clear();
		public static void CleanParentGUIDCacheForTest() => hasParentGuidColumnCache.Clear();

#endif
		#endregion

		readonly ITable[] tablesThatNeedToBeSaved;
		[SuppressThreadStaticFieldMessage]
		static readonly LRUCache<ParentChildTablePair, int?> saveOrderCache = new LRUCache<ParentChildTablePair, int?>(10000);

		void GetIsParentAndIsChildUsingAllEnterpriseTables(ITable x, ITable y, out bool isParent, out bool isChild)
		{
			GetIsParentAndIsChild(x, y, null, out isParent, out isChild);
		}

		void GetIsParentAndIsChildUsingOnlyTablesToBeSaved(ITable x, ITable y, out bool isParent, out bool isChild)
		{
			string[] tableNamesThatNeedToBeSaved = Array.ConvertAll(tablesThatNeedToBeSaved, delegate(ITable table)
			{ return table.TableName; });
			GetIsParentAndIsChild(x, y, tableNamesThatNeedToBeSaved, out isParent, out isChild);
		}

		void GetIsParentAndIsChild(ITable x, ITable y, string[] restrictToTables, out bool isParent, out bool isChild)
		{
			isParent = IsParent(x, y, restrictToTables);
			isChild = IsParent(y, x, restrictToTables);
		}

		bool IsParent(ITable parentTable, ITable childTable, string[] restrictToTables)
		{
			return IsParent(parentTable, childTable, restrictToTables, new Dictionary<string, object>());
		}

		bool IsParent(ITable parentTable, ITable childTable, string[] restrictToTables, Dictionary<string, object> childrenSoFar)
		{
			//StmNote is always considered a child table, so that it always gets deleted first no matter what.
			if (childTable.TableName == "StmNote")
			{ return true; } // programmatic constant
			if (parentTable.TableName == "StmNote")
			{ return false; } // programmatic constant

			var fkTables = GetFKTables(childTable, restrictToTables).ToArray();
			foreach (ITable fkTable in fkTables)
			{
				if (fkTable.TableName == parentTable.TableName)
				{
					return true;
				}
			}
			foreach (ITable fkTable in fkTables)
			{
				if (!childrenSoFar.ContainsKey(fkTable.TableName))
				{
					childrenSoFar.Add(fkTable.TableName, null);
					if (IsParent(parentTable, fkTable, restrictToTables, childrenSoFar))
					{
						return true;
					}
				}
			}
			return false;
		}

		IEnumerable<ITable> GetFKTables(ITable childTable, IList<string> restrictToTables)
		{
			foreach (ITable table in GetFKParentTables(childTable.ColumnNames))
			{
				if (restrictToTables == null || restrictToTables.Contains(table.TableName))
				{
					yield return table;
				}
			}
		}

		IEnumerable<ITable> GetFKParentTables(IEnumerable<string> columnNames)
		{
			foreach (string fkColumnName in columnNames)
			{
				string fkPrefix = ZRowRelationshipManager.GetForeignKeyTablePrefix(fkColumnName);
				if (fkPrefix != null)
				{
					ITable fkTableSchema = GetTableFromColumnNamePrefix(fkPrefix);
					if (fkTableSchema != null)
					{
						yield return fkTableSchema;
					}
				}
			}
		}

		ITable GetTableFromColumnNamePrefix(string columnPrefix)
		{
			foreach (ITable table in tablesThatNeedToBeSaved)
			{
				string columnName = GetFirst(table.ColumnNames);
				if (columnName != null && columnName.StartsWith(columnPrefix) && columnName.Substring(columnPrefix.Length, 1) == "_")
				{
					return table;
				}
			}
			return ToITable(GlobalServiceProvider.Instance.GetRequiredService<IApplicationSchemaResolver>().GetTableSchemaFromColumnNamePrefix(columnPrefix));
		}

		T GetFirst<T>(IEnumerable<T> enumerable)
		{
			IEnumerator<T> enumerator = enumerable.GetEnumerator();
			return enumerator.MoveNext() ? enumerator.Current : default(T);
		}

		#endregion
	}
}
