using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Schema;

namespace Enterprise.DbUpgrader.Shared
{
	public class RowWrapperRepository
	{
		#region RowWrapperInfo

		enum RowWrapperStates
		{
			New,
			Existing
		}

		sealed class RowWrapperInfo
		{
			public RowWrapperInfo(RowWrapper wrapper, RowWrapperStates state)
			{
				Wrapper = wrapper;
				State = state;
			}

			public RowWrapperStates State { get; set; }
			public RowWrapper Wrapper { get; }
		}

		#endregion

		#region Cache

		readonly Dictionary<Guid, RowWrapperInfo> rowWrappersCache = new Dictionary<Guid, RowWrapperInfo>();

		void AddToCache(RowWrapper wrapper, RowWrapperStates state)
		{
			rowWrappersCache[wrapper.PK] = new RowWrapperInfo(wrapper, state);
		}

		RowWrapper GetFromCache(ITableSchema schema, Guid pk)
		{
			RowWrapperInfo wrapperInfo;
			var wrapper = rowWrappersCache.TryGetValue(pk, out wrapperInfo)
				? wrapperInfo.Wrapper
				: new RowWrapper(schema, pk);

			return wrapper;
		}

		#endregion

		#region New

		public RowWrapper New(ITableSchema schema) => New(schema, Guid.NewGuid());
		public RowWrapper New(ITableSchema schema, Guid pk)
		{
			Argument.NotNull(schema, "schema");

			var wrapper = new RowWrapper(schema, pk);
			AddToCache(wrapper, RowWrapperStates.New);
			return wrapper;
		}

		#endregion

		#region Load

		public const int MaxParameters = 2090;

		/// <summary>
		/// Load in batches to ensure parameters passed in do no violate hard coded constraint of SQL max parameters
		/// </summary>
		/// <param name="schema">Row Table</param>
		/// <param name="pks">PKs of rows to be selected</param>
		public List<RowWrapper> Load(ITableSchema schema, IEnumerable<Guid> pks)
		{
			Argument.NotNull(schema, "schema");

			var result = new List<RowWrapper>();

			int i = 0;
			var pksInBatches = pks.GroupBy(x => i++ / MaxParameters).ToArray();
			foreach (var batchedPks in pksInBatches)
			{
				var formattedPks = string.Join("', '", batchedPks);
				var rows = Load(schema, string.Format(CultureInfo.InvariantCulture, "SELECT * FROM {0} WHERE {1} in ('{2}')", schema.TableName, schema.PK.Name, formattedPks));
				result.AddRange(rows);
			}

			return result;
		}

		public RowWrapper Load(ITableSchema schema, Guid pk)
		{
			Argument.NotNull(schema, "schema");

			var sql = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM {0} WHERE {1} = '{2}'", schema.TableName, schema.PK.Name, pk);

			return LoadCore(schema, sql).FirstOrDefault();
		}

		public List<RowWrapper> Load(ITableSchema schema, string sql = null)
		{
			Argument.NotNull(schema, "schema");

			if (string.IsNullOrWhiteSpace(sql))
			{
				sql = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM {0}", schema.TableName);
			}

			return LoadCore(schema, sql);
		}

		List<RowWrapper> LoadCore(ITableSchema schema, string sql = null)
		{
			var result = new List<RowWrapper>();

			using (var command = Db.Connection.Command(sql))
			using (var adapter = command.NewDataAdapter())
			using (var table = new DataTable { Locale = CultureInfo.InvariantCulture })
			{
				adapter.Fill(table);

				foreach (DataRow dataRow in table.Rows)
				{
					var wrapper = LoadRowWrapper(schema, dataRow);
					result.Add(wrapper);
					AddToCache(wrapper, RowWrapperStates.Existing);
				}
			}

			return result;
		}

		RowWrapper LoadRowWrapper(ITableSchema schema, DataRow row)
		{
			var pk = (Guid)row[schema.PK.Name];

			var wrapper = GetFromCache(schema, pk) ?? new RowWrapper(schema, pk);

			foreach (DataColumn column in row.Table.Columns)
			{
				var schemaColumn = wrapper.Schema.GetSchemaColumn(column.ColumnName);

				if (schemaColumn == null || schemaColumn.IsPKColumn || schemaColumn.IsComputed)
				{
					continue;
				}

				wrapper[schemaColumn] = ConvertDatabaseValue(schemaColumn, row[column]);
			}

			wrapper.HasChanges = false;

			return wrapper;
		}

		public RowWrapper Reload(RowWrapper wrapper, params SchemaColumn[] additionalColumns)
		{
			Argument.NotNull(wrapper, "wrapper");

			var schemaColumns = wrapper.SchemaColumns
				.Concat(additionalColumns ?? Enumerable.Empty<SchemaColumn>());

			var columnNames = schemaColumns.
				Select(col => col.Name)
				.Distinct();

			var builder = new StringBuilder();

			builder.Append(string.Format(CultureInfo.InvariantCulture, "SELECT {0} FROM {1}",
				string.Join(",", columnNames),
				wrapper.Schema.TableName));

			builder.Append(string.Format(CultureInfo.InvariantCulture, " WHERE {0} = '{1}'",
				wrapper.Schema.PK.Name,
				wrapper.PK));

			return LoadCore(wrapper.Schema, builder.ToString())
				.FirstOrDefault();
		}

		#endregion

		#region Saving

		public int Save()
		{
			var rowsAffectted = 0;

			using (var transactionManager = Db.Connection.BeginTransactionWithManager())
			{
				rowsAffectted += rowWrappersCache.Values.Sum(Save);
				transactionManager.CommitTransaction();
			}

			return rowsAffectted;
		}

		int Save(RowWrapperInfo wrapperInfo)
		{
			var rowsAffected = 0;

			if (wrapperInfo.State == RowWrapperStates.New)
			{
				var prefix = wrapperInfo.Wrapper.Schema.PK.ColumnPrefix;
				var auditColumnSuffixes = new[] { $"{prefix}_SystemCreateTimeUtc", $"{prefix}_SystemCreateUser", $"{prefix}_SystemLastEditTimeUtc", $"{prefix}_SystemLastEditUser" };
				var auditColumns = wrapperInfo.Wrapper.Schema.All.Where(c => auditColumnSuffixes.Contains(c.Name) && !wrapperInfo.Wrapper.Values.ContainsKey(c.Name));
				foreach (var column in auditColumns)
				{
					wrapperInfo.Wrapper[column] = column.DotNetType == typeof(DateTime) ? DateTime.UtcNow : "A";
				}

				var insertSql = string.Format(
					CultureInfo.InvariantCulture,
					"INSERT INTO {0} ({1}) VALUES ({2})",
					wrapperInfo.Wrapper.Schema.TableName,
					string.Join(",", wrapperInfo.Wrapper.Values.Keys),
					string.Join(",", wrapperInfo.Wrapper.Values.Keys.Select(col => "@" + col)));

				rowsAffected = SaveToDatabase(wrapperInfo.Wrapper, insertSql);
				wrapperInfo.State = RowWrapperStates.Existing;
			}
			else if (wrapperInfo.State == RowWrapperStates.Existing && wrapperInfo.Wrapper.HasChanges)
			{
				var builder = new StringBuilder();
				builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "UPDATE {0} SET", wrapperInfo.Wrapper.Schema.TableName));

				var columnNames = wrapperInfo.Wrapper.Values.Keys.ToArray();

				for (var i = 0; i < columnNames.Length; i++)
				{
					var columnName = columnNames[i];

					if (columnName == wrapperInfo.Wrapper.Schema.PK.Name)
					{
						continue;
					}

					var isLastColumn = i == columnNames.Length - 1;

					builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0} = @{0}{1}", columnName, isLastColumn ? "" : ","));
				}

				builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "WHERE {0} = @{0}", wrapperInfo.Wrapper.Schema.PK.Name));

				rowsAffected = SaveToDatabase(wrapperInfo.Wrapper, builder.ToString());
			}

			wrapperInfo.Wrapper.HasChanges = false;
			return rowsAffected;
		}

		int SaveToDatabase(RowWrapper wrapper, string sql)
		{
			using (var command = Db.Connection.Command(sql))
			{
				foreach (var value in wrapper.Values)
				{
					var schemaColumn = wrapper.Schema.GetSchemaColumn(value.Key);

					command.AddParameterBasedOnDbColumn("@" + schemaColumn.Name, ConvertParameterValue(value.Value, schemaColumn.DotNetType, schemaColumn.SqlDbType), schemaColumn);
				}

				return command.ExecuteNonQuery();
			}
		}

		object ConvertDatabaseValue(SchemaColumn schemaColumn, object databaseValue)
		{
			if (databaseValue == DBNull.Value)
			{
				return schemaColumn.DotNetType.IsValueType ? Activator.CreateInstance(schemaColumn.DotNetType) : null;
			}

			if (schemaColumn.DotNetType == typeof(bool))
			{
				var valueAsString = Convert.ToString(databaseValue, CultureInfo.InvariantCulture);
				return valueAsString == "Y" || valueAsString == "True";
			}

			return databaseValue;
		}

		object ConvertParameterValue(object value, Type valueType, SqlDbType sqlDbType)
		{
			if (valueType == typeof(bool) && sqlDbType != SqlDbType.Bit)
			{
				return (bool)value ? "Y" : "N";
			}

			if (valueType == typeof(DateTime) && sqlDbType == SqlDbType.SmallDateTime && (DateTime)value < MinSqlSmallDateTime)
			{
				return MinSqlSmallDateTime;
			}

			if (valueType == typeof(DateTime) && sqlDbType == SqlDbType.SmallDateTime && (DateTime)value > MaxSqlSmallDateTime)
			{
				return MaxSqlSmallDateTime;
			}

			if (valueType == typeof(DateTime) && (DateTime)value < SqlDateTime.MinValue.Value)
			{
				return SqlDateTime.MinValue.Value;
			}

			if (valueType == typeof(DateTime) && (DateTime)value > SqlDateTime.MaxValue.Value)
			{
				return SqlDateTime.MaxValue.Value;
			}

			return value ?? DBNull.Value;
		}

		static readonly DateTime MinSqlSmallDateTime = new DateTime(1900, 1, 1);
		static readonly DateTime MaxSqlSmallDateTime = new DateTime(2079, 6, 6);

		#endregion
	}
}
