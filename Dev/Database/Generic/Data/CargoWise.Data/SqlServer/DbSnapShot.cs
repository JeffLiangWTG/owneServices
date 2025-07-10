#if DEBUG

using System.Collections.Generic;
using System.Text;
using CargoWise.Common;

namespace CargoWise.Data
{
	/// <summary>
	/// Utility class for counting and comparing database records for each table.
	/// </summary>
	public class DbSnapshot
	{
		public class TableInfo
		{
			public long RowCount { get; set; }
			public long ReservedKB { get; set; }
			public long DataKB { get; set; }
			public long IndexSizeKB { get; set; }
			public long UnusedKB { get; set; }
		}

		public DbSnapshot(string snapshotName, Dictionary<string, TableInfo> tableRecordCount = null)
		{
			Argument.NotNullOrEmpty(snapshotName, nameof(snapshotName));

			this.tableRecordCount = tableRecordCount ?? new Dictionary<string, TableInfo>();
			name = snapshotName;
		}

		public Dictionary<string, TableInfo> TableRecordCount
		{
			get
			{
				return tableRecordCount;
			}
		}

		readonly string name;
		readonly Dictionary<string, TableInfo> tableRecordCount;

		public string Name
		{
			get
			{
				return name;
			}
		}

		public string Diff(DbSnapshot right, List<string> tablesToIgnore)
		{
			Argument.NotNull(tablesToIgnore, nameof(tablesToIgnore));
			Argument.NotNull(right, nameof(right));

			StringBuilder resultBuilder = new StringBuilder(200);
			TableInfo tableInfo;
			foreach (var table in TableRecordCount.Keys)
			{
				if (!string.IsNullOrEmpty(table) && !tablesToIgnore.Contains(table) && TableRecordCount.TryGetValue(table, out tableInfo) && tableInfo != null)
				{
					var leftCount = tableInfo.RowCount;
					var rightCount = 0L;
					TableInfo rightTableInfo;
					if (right.TableRecordCount.TryGetValue(table, out rightTableInfo) && rightTableInfo != null)
					{
						rightCount = rightTableInfo.RowCount;
					}

					var diff = leftCount - rightCount;
					if (diff != 0L)
					{
						resultBuilder.AppendLine(string.Format("Table: {0}, {1}: {2}, {3}: {4}, Diff: {5}", table, Name, leftCount, right.Name, rightCount, diff)); // Utility string not required for other languagues
					}
				}
			}

			foreach (var table in right.TableRecordCount.Keys)
			{
				if (!string.IsNullOrEmpty(table) && !tablesToIgnore.Contains(table) && !TableRecordCount.ContainsKey(table) && right.TableRecordCount.TryGetValue(table, out tableInfo) && tableInfo != null)
				{
					var rightCount = tableInfo.RowCount;
					var leftCount = 0L;
					var diff = leftCount - rightCount;

					if (diff != 0L)
					{
						resultBuilder.AppendLine(string.Format("Table: {0}, {1}: {2}, {3}: {4}, Diff: {5}", table, Name, leftCount, right.Name, rightCount, diff)); // Utility string not required for other languagues
					}
				}
			}

			return resultBuilder.ToString();
		}

		/// <summary>
		/// Minus operator will only work on RowCount.
		/// </summary>
		public static DbSnapshot operator -(DbSnapshot left, DbSnapshot right)
		{
			Argument.NotNull(left, nameof(left));
			Argument.NotNull(right, nameof(right));

			string newName = left.Name + " - " + right.Name;

			DbSnapshot resultSnapshot = new DbSnapshot(newName);
			foreach (var table in left.TableRecordCount.Keys)
			{
				TableInfo tableInfo;
				if (!string.IsNullOrEmpty(table) && left.TableRecordCount.TryGetValue(table, out tableInfo) && tableInfo != null)
				{
					var leftCount = tableInfo.RowCount;
					var rightCount = 0L;
					if (right.TableRecordCount.TryGetValue(table, out tableInfo) && tableInfo != null)
					{
						rightCount = tableInfo.RowCount;
					}
					resultSnapshot.TableRecordCount.Add(table, new TableInfo() { RowCount = leftCount - rightCount });
				}
			}

			return resultSnapshot;
		}

		public static DbSnapshot TakeSnapshot(string name, DbConnection connection = null)
		{
			Argument.NotNullOrEmpty(name, nameof(name));

			Dictionary<string, TableInfo> tableRecordCount;

			if (connection == null)
			{
				using (var newConnection = Db.NewExtraConnectionToMainDb())
				{
					tableRecordCount = GetTableUsageInfo(newConnection);
				}
			}
			else
			{
				tableRecordCount = GetTableUsageInfo(connection);
			}

			return new DbSnapshot(name, tableRecordCount);
		}

#if DEBUG
		internal
#endif
		static Dictionary<string, TableInfo> GetTableUsageInfo(DbConnection connection
#if DEBUG
			, string extraSqlForTest = null
#endif
			)
		{
			Argument.NotNull(connection, nameof(connection));

			var tableRecordCount = new Dictionary<string, TableInfo>();

			var sql = @"
				DECLARE @TableName varchar(128)

				CREATE TABLE #SnapshotResult
				(
					TableName varchar(128),
					Rows      varchar(50),
					Reserved  varchar(50),
					Data      varchar(50),
					IndexSize varchar(50),
					Unused    varchar(50)
				)

				DECLARE TableCursor CURSOR FAST_FORWARD FOR
					SELECT '['+SCHEMA_NAME(schema_id)+'].['+name+']' AS name
					FROM sys.objects
					WHERE [type] = 'U'
					AND   is_ms_shipped = 0

				OPEN TableCursor
				FETCH NEXT FROM TableCursor INTO @TableName
				WHILE (@@FETCH_STATUS = 0)
				BEGIN
					INSERT INTO #SnapshotResult EXEC sp_spaceused @objname = @TableName
					FETCH NEXT FROM TableCursor INTO @TableName
				END

				CLOSE TableCursor
				DEALLOCATE TableCursor

				" +
#if DEBUG
				(extraSqlForTest ?? "") +
#endif
				@"
				SELECT 
					TableName,
					convert(bigint, Rows) Rows,
					convert(bigint, left(Reserved, charindex('KB', Reserved) - 1)) Reserved,
					convert(bigint, left(Data, charindex('KB', Data) - 1)) Data,
					convert(bigint, left(IndexSize, charindex('KB', IndexSize) - 1)) IndexSize,
					convert(bigint, left(Unused, charindex('KB', Unused) - 1)) Unused
					FROM #SnapshotResult

				DROP TABLE #SnapshotResult"; // SQL expression

			using (var command = connection.Command(sql))
			{
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						string tableName = GetTableNameByRegex(reader.GetString(reader.GetOrdinal("TableName")));
						var rowCount = reader.GetInt64(reader.GetOrdinal("Rows")); // May be a part of SQL expression.
						var reservedKB = reader.GetInt64(reader.GetOrdinal("Reserved")); // May be a part of SQL expression.
						var dataKB = reader.GetInt64(reader.GetOrdinal("Data")); // May be a part of SQL expression.
						var indexSizeKB = reader.GetInt64(reader.GetOrdinal("IndexSize"));
						var unusedKB = reader.GetInt64(reader.GetOrdinal("Unused")); // May be a part of SQL expression.

						tableRecordCount.Add(tableName, new TableInfo { RowCount = rowCount, ReservedKB = reservedKB, DataKB = dataKB, IndexSizeKB = indexSizeKB, UnusedKB = unusedKB });
					}
				}
			}

			return tableRecordCount;
		}

		static string GetTableNameByRegex(string fullTableName)
		{
			var match = System.Text.RegularExpressions.Regex.Match(fullTableName, @"\[(\w+)\]\.\[(\w+)\]", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			if (match.Success)
			{
				return match.Groups[2].Value;
			}
			else
			{
				return fullTableName;
			}
		}
	}
}

#endif
