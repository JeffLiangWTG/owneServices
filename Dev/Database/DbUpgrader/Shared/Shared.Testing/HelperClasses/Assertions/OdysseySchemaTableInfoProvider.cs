using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DbUpgrader.Shared.Testing
{
	[Immutable]
	class OdysseySchemaTableInfoProvider : ITableInfoProvider
	{
		OdysseySchemaTableInfoProvider()
		{
		}

		public static ITableInfoProvider Instance { get; }	= new OdysseySchemaTableInfoProvider();

		public ITableInfo GetTableInfo(string tableName)
		{
			var tableSchema = EnterpriseSchema.GetTableSchema(tableName);
			return new TableInfo(tableSchema);
		}

		class TableInfo : ITableInfo
		{
			public TableInfo(ITableSchema tableSchema)
			{
				TableSchema = tableSchema;
			}

			ITableSchema TableSchema { get; }

			public string TableName => TableSchema.TableName;
			public string SchemaName => TableSchema.SqlSchemaName;
			public string ColumnPrefix => TableSchema.PK.ColumnPrefix;

			public IColumnInfo GetColumnInfo(string columnName)
			{
				var schemaColumn = TableSchema.GetSchemaColumn(columnName);
				return schemaColumn != null ? new ColumnInfo(schemaColumn) : null;
			}

			class ColumnInfo : IColumnInfo
			{
				public ColumnInfo(SchemaColumn column)
				{
					Column = column;
				}

				SchemaColumn Column { get; }

				public bool IsSparse => Column.IsSparse;
				public bool IsBinary => Column.IsBinary;
				public bool IsDecimal => Column is ISchemaDecimalColumn;
				public byte Precision => Column is ISchemaDecimalColumn col ? col.Precision : default;
				public byte Scale => Column is ISchemaDecimalColumn col ? col.Scale : default;

				public int MaxLength => Column.MaxLength;
				public bool IsNullable => Column.IsNullable;

				public Type Type => Column.DotNetType;
				public SqlDbType SqlDbType => Column.SqlDbType;
				public object SqlDbDefault => Column.SqlDbDefault;
				public string SqlDbTypeDeclaration => Column.SqlDbTypeDeclaration;
			}
		}
	}
}
