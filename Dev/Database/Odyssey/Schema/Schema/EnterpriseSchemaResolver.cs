using System;
using CargoWise.Schema;

namespace Enterprise.ZArchitecture.Schema
{
	public class EnterpriseSchemaResolver : IApplicationSchemaResolver
	{
		public ITableSchema GetTableSchema(String tableName)
		{
			return EnterpriseSchema.GetTableSchema(tableName);
		}

		public ITableSchema GetTableSchemaFromColumnNamePrefix(string columnNamePrefix)
		{
			return EnterpriseSchema.GetTableSchemaFromColumnNamePrefix(columnNamePrefix);
		}

		public SchemaPKColumn GetPkColumn(string tableName)
		{
			SchemaPKColumn result = GetPkColumnSafe(tableName)
				?? throw new InvalidSchemaColumnException(String.Concat(tableName.Substring(0, 2), "PK"), tableName);
			return result;
		}

		public SchemaPKColumn GetPkColumnSafe(string tableName)
		{
			ITableSchema schema = EnterpriseSchema.GetTableSchema(tableName);
			return (schema != null) ? schema.PK : null;
		}

		public bool SchemaColumnExists(string columnName, string tableName)
		{
			return GetSchemaColumnSafe(columnName, tableName) != null;
		}

		public SchemaColumn GetSchemaColumn(string columnName, string tableName)
		{
			SchemaColumn result = GetSchemaColumnSafe(columnName, tableName);
			if (result == null)
			{
				throw new InvalidSchemaColumnException(columnName, tableName);
			}
			else
			{
				return result;
			}
		}

#if DEBUG
		public virtual SchemaColumn GetSchemaColumnSafe(string columnName, string tableName)
#else
		public SchemaColumn GetSchemaColumnSafe(string columnName, string tableName)
#endif
		{
			SchemaColumn result = null;

			ITableSchema tableSchema = EnterpriseSchema.GetTableSchema(tableName);
			if (tableSchema != null)
			{
				result = tableSchema.GetSchemaColumn(columnName);
			}
			return result;
		}

		public SchemaColumnCollection GetSchemaColumns(string tableName)
		{
			ITableSchema tableSchema = EnterpriseSchema.GetTableSchema(tableName);
			if (tableSchema == null)
			{
				throw new InvalidTableNameException(tableName);
			}
			else
			{
				return tableSchema.All;
			}
		}

		public string GetColumnNamePrefix(string tableName)
		{
			var pKColumn = GetPkColumnSafe(tableName);
			return (pKColumn == null) ? null : CargoWise.Schema.Schema.GetPrefixFromColumnName(pKColumn.Name);
		}
	}
}
