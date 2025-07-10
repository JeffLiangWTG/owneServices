using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	public class ZInsertCommandBuilderBase : ZSqlCommandBuilder
	{
		public ZInsertCommandBuilderBase(string tableName, bool compress, int statementIndex, ITableSchema schema, int expectedNumberOfRows)
			: base(null, tableName, compress, schema)
		{
			this.rowsToInsert = new List<DataRow>();
			this.useParameters = UseParameters;
			this.statementIndex = statementIndex;
			this.nullParameters = new Dictionary<SqlDbType, string>();
			this.expectedNumberOfRows = expectedNumberOfRows;

			geographyNames = new List<string>();
			parameterNamesList = new List<List<string>>();
			parameterDeclarations = new List<string>();
			parameterValues = new List<string>();
			literalValues = new List<string>();
		}

		public readonly List<DataRow> rowsToInsert;
		readonly bool useParameters;
		readonly int statementIndex;
		readonly int expectedNumberOfRows;
		int lastProcessedRow;

		#region SuppressResourceStringsCheckRegion

		protected override void Build()
		{
			CreateNamesAndValues();

			if (useParameters)
			{
				AppendWithDelimiter(CommandText, GeographyNamesRowDelimiter, GeographyNames, true);
				CommandText.Append("EXEC sys.sp_executesql N'INSERT ");
				CommandText.Append(AddSchemaName(TableName));
				CommandText.Append(" (");
				AppendWithDelimiter(CommandText, ColumnNamesDelimiter, ColumnNames);
				CommandText.Append(") VALUES\r\n\t");
				CommandText.Append(ParameterNames);
				CommandText.Append("\r\n'\r\n, N'");
				AppendWithDelimiter(CommandText, ParameterDeclarationsRowDelimiter, ParameterDeclarations);
				CommandText.Append("'\r\n, ");
				AppendWithDelimiter(CommandText, ParameterValuesRowDelimiter, ParameterValues);
				CommandText.Append(";\r\n");
			}
			else
			{
				AppendWithDelimiter(CommandText, GeographyNamesRowDelimiter, GeographyNames, true);
				CommandText.Append("INSERT ");
				CommandText.Append(AddSchemaName(TableName));
				CommandText.Append(" (");
				AppendWithDelimiter(CommandText, ColumnNamesDelimiter, ColumnNames);
				CommandText.Append(") VALUES\r\n\t");
				AppendWithDelimiter(CommandText, LiteralRowDelimiter, LiteralValues);
				CommandText.Append(";\n");
			}
		}

		#endregion // SuppressResourceStringsCheckRegion

		public override bool IsConcurrencyCheckRequired => false;

		public void AddRow(DataRow row) => rowsToInsert.Add(row);

		#region NamesAndValues

		void CreateNamesAndValues()
		{
			var rowCount = rowsToInsert.Count;
			var table = rowsToInsert[0].Table;
			var tableColumnCount = table.Columns.Count;
			var parameterNameFormatLength = expectedNumberOfRows.ToString().Length;
			var parameterNameFormat = FormattableString.Invariant($@"{{0:d{parameterNameFormatLength}}}");

			if (rowCount > expectedNumberOfRows)
			{
				parameterNameFormat += ParameterNamesDelimiter;
			}

			StringBuilder sbParameterDeclarations = null;
			StringBuilder sbParameterValues = null;
			StringBuilder sbLiteralValues = null;
			if (useParameters)
			{
				sbParameterDeclarations = new StringBuilder();
				sbParameterValues = new StringBuilder();
			}
			else
			{
				sbLiteralValues = new StringBuilder();
			}

			var tuple = GetColumnsUsingCache(table);
			var tableColumns = tuple.Columns;
			columnNames = tuple.ColumnNames.ToList();

			for (var rowIndex = lastProcessedRow; rowIndex < rowCount; rowIndex++)
			{
				var row = rowsToInsert[rowIndex];
				var rowIndexAsString = string.Format(parameterNameFormat, rowIndex + 1);
				var parameterNames = new List<string>();

				if (useParameters)
				{
					if (rowIndex > lastProcessedRow)
					{
						sbParameterDeclarations.Append(ParameterDeclarationsRowDelimiter);
						sbParameterValues.Append(ParameterValuesRowDelimiter);
					}
				}
				else
				{
					if (rowIndex > lastProcessedRow)
					{
						sbLiteralValues.Append(LiteralRowDelimiter);
					}
					sbLiteralValues.Append(LiteralRowStart);
				}

				for (var columnIndex = 0; columnIndex < tableColumnCount; columnIndex++)
				{
					var column = table.Columns[columnIndex];
					var schemaColumn = tableColumns[columnIndex];
					var schemaColumnSqlDbDefault = schemaColumn.SqlDbDefault;
					var schemaColumnSqlDbType = schemaColumn.SqlDbType;
					var schemaColumnSqlDbTypeDeclaration = schemaColumn.SqlDbTypeDeclaration;

					var value = row[column.Ordinal];

					NeedToDetectSP_CustomProperties = column.ColumnName == "SP_CustomProperties"
						&& ((string)row["SP_EmailSubjectLine"]).StartsWith("Yusen Logistics")
						&& ((string)row["SP_DocumentName"]).StartsWith("MAWB (Copy for Shipping Advice)");
					DetectEmptySP_CustomProperties(value as byte[], "GetFromRow");

					var valueIsNull = false;
					if (value != DBNull.Value)
					{
						if (schemaColumn.IsLargeBinaryOrText)
						{
							value = GetSourceValueIfSet(row, value, schemaColumn);

							DetectEmptySP_CustomProperties(value as byte[], "GetSourceValueIfSet");

							value = AddDataParameterAndBlobSaverIfLargeBlobOrText(row, value, schemaColumn);

							DetectEmptySP_CustomProperties(value as byte[], "AddDataParameterAndBlobSaverIfLargeBlobOrText");
						}

						value = GetQuoteEscapeTruncateAndCompress(value, schemaColumn);
					}
					else
					{
						if (schemaColumnSqlDbDefault == null || schemaColumnSqlDbDefault == DBNull.Value)
						{
							valueIsNull = true;
							value = SQL_NULL_KEYWORD;
						}
						else
						{
							value = GetQuoteEscapeTruncateAndCompress(schemaColumnSqlDbDefault, schemaColumn);
						}
					}
					ClearEmptySP_CustomPropertiesDetectingInfo();

					string parameterName;
					bool existingNull = false;

					if (valueIsNull)
					{
						if (!nullParameters.TryGetValue(schemaColumnSqlDbType, out parameterName))
						{
							nullParameters.Add(schemaColumnSqlDbType, parameterName = "@" + rowIndexAsString + (columnIndex + 1).ToString(CultureInfo.InvariantCulture));
						}
						else
						{
							existingNull = true;
						}
					}
					else
					{
						if (schemaColumn is SchemaGeographyColumn)
						{
							geographyNames.Add(string.Format(CultureInfo.InvariantCulture,
								"DECLARE @{0}_{1}{2} geography = {3};" // format string for code
								, statementIndex   // 0
								, rowIndexAsString // 1
								, columnIndex + 1  // 2
								, value            // 3
							));

							value = string.Format(CultureInfo.InvariantCulture,
								"@{0}_{1}{2}" // format string for code
								, statementIndex // 0
								, rowIndexAsString // 1
								, columnIndex + 1 // 2
							);
						}

						parameterName = "@" + rowIndexAsString + (columnIndex + 1).ToString(CultureInfo.InvariantCulture);
					}

					if (useParameters)
					{
						parameterNames.Add(parameterName);

						if (!existingNull)
						{
							if (columnIndex > 0)
							{
								sbParameterDeclarations.Append(ParameterDeclarationsColumnDelimiter);
								sbParameterValues.Append(ParameterValuesColumnDelimiter);
							}

							sbParameterDeclarations.Append(parameterName);
							sbParameterDeclarations.Append(ParameterDeclarationsDelimiter);
							sbParameterDeclarations.Append(schemaColumnSqlDbTypeDeclaration);

							sbParameterValues.Append(parameterName);
							sbParameterValues.Append(ParameterValuesDelimiter);

							sbParameterValues.Append(value);
						}
					}
					else
					{
						if (columnIndex > 0)
						{
							sbLiteralValues.Append(LiteralColumnDelimiter);
						}
						sbLiteralValues.Append(value);
					}
				}

				if (useParameters)
				{
					parameterNamesList.Add(parameterNames);
				}
				else
				{
					sbLiteralValues.Append(LiteralRowEnd);
				}
			}

			if (useParameters)
			{
				parameterDeclarations.Add(sbParameterDeclarations.ToString());
				parameterValues.Add(sbParameterValues.ToString());
			}
			else
			{
				literalValues.Add(sbLiteralValues.ToString());
			}

			lastProcessedRow = rowCount;
		}

		readonly Dictionary<SqlDbType, string> nullParameters;

		public List<string> ColumnNames => columnNames;
		List<string> columnNames;

		public List<string> GeographyNames => geographyNames;
		readonly List<string> geographyNames;

		public string ParameterNames
		{
			get
			{
				var sb = new StringBuilder();
				var needRowDelimiter = false;
				foreach (var list in parameterNamesList)
				{
					if (needRowDelimiter)
					{
						sb.Append(ParameterNamesRowDelimiter);
					}

					sb.Append(ParameterNamesRowStart);
					var needComma = false;
					foreach (var name in list)
					{
						if (needComma)
						{
							sb.Append(ParameterNamesColumnDelimiter);
						}
						sb.Append(name);
						needComma = true;
					}

					needRowDelimiter = true;
					sb.Append(ParameterNamesRowEnd);
				}

				return sb.ToString();
			}
		}
		readonly List<List<string>> parameterNamesList;

		public List<string> ParameterDeclarations => parameterDeclarations;
		readonly List<string> parameterDeclarations;

		public List<string> ParameterValues => parameterValues;
		readonly List<string> parameterValues;

		public List<string> LiteralValues => literalValues;
		readonly List<string> literalValues;

		#endregion // NamesAndValues

		readonly Dictionary<string, (ImmutableArray<SchemaColumn>, ImmutableArray<string>)> schemaColumnCache = new Dictionary<string, (ImmutableArray<SchemaColumn>, ImmutableArray<string>)>();

		(ImmutableArray<SchemaColumn> Columns, ImmutableArray<string> ColumnNames) GetColumnsUsingCache(DataTable table)
		{
			var tableName = table.TableName;
			if (!schemaColumnCache.TryGetValue(tableName, out var tuple))
			{
				var tableColumnCount = table.Columns.Count;
				var schemaColumns = Schema.All;
				var columns = new SchemaColumn[tableColumnCount];
				for (var columnIndex = 0; columnIndex < tableColumnCount; columnIndex++)
				{
					columns[columnIndex] = schemaColumns[table.Columns[columnIndex].ColumnName];
				}

				tuple = (columns.ToImmutableArray(), columns.Select(c => c.Name).ToImmutableArray());
				schemaColumnCache.Add(tableName, tuple);
			}
			return tuple;
		}

		static void AppendWithDelimiter(StringBuilder builder, string delimiter, List<string> strings, bool includeTrailingDelimiter = false)
		{
			var needDelimiter = false;
			foreach (var str in strings)
			{
				if (needDelimiter)
				{
					builder.Append(delimiter);
				}
				builder.Append(str);
				needDelimiter = true;
			}

			if (needDelimiter && includeTrailingDelimiter)
			{
				builder.Append(delimiter);
			}
		}

		const string ColumnNamesDelimiter = ", ";
		const string GeographyNamesRowDelimiter = "\r\n";

		const string ParameterNamesRowStart = "(";
		const string ParameterNamesRowDelimiter = ",\r\n\t";
		const string ParameterNamesColumnDelimiter = ", ";
		const string ParameterNamesRowEnd = ")";
		const string ParameterNamesDelimiter = "_";

		const string ParameterDeclarationsRowDelimiter = "\r\n  , ";
		const string ParameterDeclarationsColumnDelimiter = ", ";
		const string ParameterDeclarationsDelimiter = " ";

		const string ParameterValuesRowDelimiter = "\r\n, ";
		const string ParameterValuesColumnDelimiter = ", ";
		const string ParameterValuesDelimiter = " = ";

		const string LiteralRowStart = "(";
		const string LiteralRowDelimiter = ",\r\n\t";
		const string LiteralColumnDelimiter = ", ";
		const string LiteralRowEnd = ")";
	}
}
