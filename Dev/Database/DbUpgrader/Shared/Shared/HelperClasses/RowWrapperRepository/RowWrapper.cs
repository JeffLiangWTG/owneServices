using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Schema;
using Microsoft.SqlServer.Types;

namespace Enterprise.DbUpgrader.Shared
{
	[DebuggerDisplay("{Schema.TableName}|{PK}")]
	public class RowWrapper
	{
		internal RowWrapper(ITableSchema schema)
			: this(schema, Guid.NewGuid())
		{
		}

		internal RowWrapper(ITableSchema schema, Guid pk)
		{
			Argument.NotNull(schema, "schema");
			Schema = schema;

			Values = new Dictionary<string, object>
			{
				{ Schema.PK.Name, pk }
			};
		}

		internal ITableSchema Schema { get; }

		internal Dictionary<string, object> Values { get; }

		internal bool HasChanges { get; set; }

		public Guid PK => (Guid)this[Schema.PK];

		internal IEnumerable<SchemaColumn> SchemaColumns
		{
			get { return Values.Keys.Select(key => Schema.GetSchemaColumn(key)); }
		}

		[SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers", Justification = "I want indexer by SchemaColumn")]
		public object this[SchemaColumn column]
		{
			get { return Get(column); }
			set { Set(column, value); }
		}

		object Get(SchemaColumn column)
		{
			Argument.NotNull(column, "column");

			if (Values.ContainsKey(column.Name))
			{
				return Values[column.Name];
			}

			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "[{0}.{1}] cannot get value that has not been set", Schema.TableName, column.Name));
		}

		void Set(SchemaColumn column, object value)
		{
			Argument.NotNull(column, "column");

			if (column.TableName != Schema.TableName)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "column [{0}] does not belong to [{1}]", column.Name, Schema.TableName));
			}

			if (column.IsPKColumn)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "PK column [{0}] should be initialized via constructor", column.Name));
			}

			if (column.ColumnType == SchemaColumnType.Geography && value is SqlGeography sqlGeoValue)
			{
				if (sqlGeoValue.IsNull)
				{
					value = SqlGeography.STPointFromText(new SqlChars("POINT EMPTY"), 4326); // Hard-coded constant
				}
			}
			else if (value == null && column.DotNetType.IsValueType || value != null && !column.DotNetType.IsInstanceOfType(value))
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "value [{0}] set for [{1}] is not a valid {2}", value ?? "null", column.Name, column.DotNetType.Name));
			}

			if (value is SqlGeography geo)
			{
				value = geo.AsTextZM().ToSqlString().ToString();
			}

			if (Values.ContainsKey(column.Name))
			{
				Values[column.Name] = value;
			}
			else
			{
				Values.Add(column.Name, value);
			}

			HasChanges = true;
		}
	}
}
