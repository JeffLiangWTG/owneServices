using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	public partial class ZDeleteCommandBuilderBase : ZSqlCommandBuilder
	{
		public ZDeleteCommandBuilderBase(DataRow row, string tableName, bool compress, int statementIndex, ITableSchema schema)
			: base(row, tableName, compress, schema)
		{
			this.statementIndex = statementIndex;
		}

		readonly int statementIndex;

		protected override void Build()
		{
			CreateParameters();

			CommandText.Append(
$@"EXEC sys.sp_executesql N'DELETE FROM {AddSchemaName(TableName)}
WHERE 1=1
	{WhereClause}
;
{SetRowCountClause}
'
, N'{ParameterDeclarations}'
, {ParameterValues};
");
		}

		void CreateParameters()
		{
			parameters = new List<Parameter>();
			if (IsConcurrencyCheckRequired)
			{
				parameters.Add(new Parameter { Name = "@0", TypeDeclaration = "int out", Value = "@row_count out", IsDeclaration = true }); // part of sql procedure declaration
			}

			AddConcurrencyCheckWhereClause(!IsConcurrencyCheckRequired);
		}

		protected override void AddConcurrencyCheckWhereClause(bool ignoreAllConcurrencyCheck = false)
		{
			for (int i = 0; i < Row.Table.Columns.Count; i++)
			{
				var col = Row.Table.Columns[i];
				var schemaColumn = Schema?.GetSchemaColumn(col.ColumnName);
				if (schemaColumn == null)
				{
					continue;
				}

				object originalValue = Row[col, DataRowVersion.Original];

				if (col.ColumnName == Schema.PK.Name)
				{
					AddParameter(schemaColumn, parameterIndex: i + 1, value: originalValue, isConcurrency: false);
				}
				else if (!ignoreAllConcurrencyCheck && schemaColumn.ShouldCheckConcurrency(Row, col))
				{
					AddConcurrencyCheckField_ForTest(col);

					AddParameter(schemaColumn, parameterIndex: i + 1, value: originalValue, isConcurrency: true);
				}
			}
		}

		internal override void AddConcurrencyCheck(StringBuilder currentCommandText, string concurrencyErrorText)
		{
			if (IsConcurrencyCheckRequired)
			{
				currentCommandText.AppendLine(string.Format(CultureInfo.InvariantCulture, "if (@row_count = 0) RAISERROR('{0}', 16, 1);", concurrencyErrorText)); // part of sql command
			}
		}

		bool? isConcurrencyCheckRequired;
		public override bool IsConcurrencyCheckRequired
		{
			get
			{
				if (!isConcurrencyCheckRequired.HasValue)
				{
					string tableName = DataUtils.GetTableNameFromDbAndTableName(TableName);
					isConcurrencyCheckRequired = !tableName.Equals("OrgPatternMatch", StringComparison.OrdinalIgnoreCase);
				}

				return isConcurrencyCheckRequired.Value;
			}
		}

		partial void AddConcurrencyCheckField_ForTest(DataColumn column);

		string WhereClause
		{
			get
			{
				return string.Join("\r\n\t", parameters
					.Where(p => p.IsWhere)
					.Select(p => (p.IsConcurrency)
						? string.Format(CultureInfo.InvariantCulture, "AND ISNULL(NULLIF({0}, {1}), NULLIF({1}, {0})) is NULL", p.ColumnName, p.Name) // sql operators
						: string.Format(CultureInfo.InvariantCulture, "AND {0} = {1}", p.ColumnName, p.Name) // sql operators
						)
					);
			}
		}

		string SetRowCountClause
		{
			get { return (IsConcurrencyCheckRequired) ? "SET @0 = @@ROWCOUNT;" : string.Empty; } // sql command part
		}

		public string ParameterDeclarations
		{
			get { return string.Join(", ", parameters.Where(p => p.IsDeclaration).Select(p => string.Format(CultureInfo.InvariantCulture, "{0} {1}", p.Name, p.TypeDeclaration))); }
		}

		public string ParameterValues
		{
			get { return string.Join(", ", parameters.Where(p => p.IsDeclaration).Select(p => string.Format(CultureInfo.InvariantCulture, "{0} = {1}", p.Name, p.Value))); }
		}

		#region Helper classes

		List<Parameter> parameters;

		class Parameter
		{
			public string ColumnName { get; set; }
			public string Name { get; set; }
			public string TypeDeclaration { get; set; }
			public string Value { get; set; }
			public bool IsWhere { get; set; }
			public bool IsConcurrency { get; set; }
			public bool IsDeclaration { get; set; }
		}

		void AddParameter(SchemaColumn schemaColumn, int parameterIndex, object value, bool isConcurrency)
		{
			var paramName = string.Format(CultureInfo.InvariantCulture, "@{0}", parameterIndex);
			var paramType = schemaColumn.SqlDbTypeDeclaration;

			if (value.Equals(DBNull.Value))
			{
				if (schemaColumn.ColumnType == SchemaColumnType.Geography)
				{
					parameters.Add(new Parameter { ColumnName = schemaColumn.Name + ".STAsText()", Name = paramName + ".STAsText()", IsWhere = true, IsConcurrency = isConcurrency });
					parameters.Add(new Parameter { Name = paramName, TypeDeclaration = paramType, Value = SQL_NULL_KEYWORD, IsDeclaration = true, IsConcurrency = isConcurrency });
				}
				else
				{
					parameters.Add(new Parameter
					{
						ColumnName = schemaColumn.Name,
						Name = paramName,
						TypeDeclaration = paramType,
						Value = SQL_NULL_KEYWORD,
						IsWhere = true,
						IsDeclaration = true,
						IsConcurrency = isConcurrency
					});
				}
			}
			else if (schemaColumn.IsLargeBinaryOrText)
			{
				if (schemaColumn.SqlDbType != SqlDbType.Xml && schemaColumn.DotNetType == typeof(string) && string.IsNullOrEmpty(value.ToString()))
				{
					parameters.Add(new Parameter
					{
						ColumnName = schemaColumn.Name,
						Name = paramName,
						TypeDeclaration = paramType,
						Value = GetQuotedAndEscapedString(string.Empty, schemaColumn.IsUnicode),
						IsWhere = true,
						IsDeclaration = true,
						IsConcurrency = isConcurrency
					});
				}
				else
				{
					var pair = CreateLargeBinaryOrTextConcurrencyCheck(value, schemaColumn);
					parameters.Add(new Parameter { ColumnName = pair.Key, Name = paramName, IsWhere = true, IsConcurrency = isConcurrency });
					parameters.Add(new Parameter { Name = paramName, TypeDeclaration = "nvarchar(max)", Value = pair.Value, IsDeclaration = true, IsConcurrency = isConcurrency });
				}
			}
			else if (schemaColumn.ColumnType == SchemaColumnType.Geography)
			{
				var paramValue = GetQuoteEscapeTruncateAndCompress(value, schemaColumn);
				var outerParamName = string.Format(CultureInfo.InvariantCulture, "@{0}_{1}", statementIndex, parameterIndex);

				CommandText.AppendFormat("DECLARE {0} {1} = {2};", outerParamName, paramType, paramValue).AppendLine(); // sql command part
				parameters.Add(new Parameter { ColumnName = schemaColumn.Name + ".STAsText()", Name = paramName + ".STAsText()", IsWhere = true, IsConcurrency = isConcurrency });
				parameters.Add(new Parameter { Name = paramName, TypeDeclaration = paramType, Value = outerParamName, IsDeclaration = true, IsConcurrency = isConcurrency });
			}
			else
			{
				var paramValue = GetQuoteEscapeTruncateAndCompress(value, schemaColumn);
				parameters.Add(new Parameter
				{
					ColumnName = schemaColumn.Name,
					Name = paramName,
					TypeDeclaration = paramType,
					Value = paramValue,
					IsWhere = true,
					IsDeclaration = true,
					IsConcurrency = isConcurrency
				});
			}
		}

		#endregion // Helper classes
	}
}

#region Test
#if DEBUG

namespace CargoWise.EntityFramework
{
	public partial class ZDeleteCommandBuilderBase
	{
		partial void AddConcurrencyCheckField_ForTest(DataColumn column)
		{
			ConcurrencyCheckFields.Add(column);
		}
	}
}

#endif
#endregion
