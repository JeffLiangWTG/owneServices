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
	public class ZUpdateCommandBuilderBase : ZSqlCommandBuilder
	{
		public ZUpdateCommandBuilderBase(DataRow row, string tableName, bool compress, int statementIndex, ITableSchema schema, bool isBulk = false, StringBuilder commandText = null, List<ILargeColumnSaver> largeColumnSavers = null)
			: base(row, tableName, compress, schema, largeColumnSavers)
		{
			this.isBulk = isBulk;
			this.useParameters = UseParameters;
			this.statementIndex = statementIndex;
			this.CommandText = commandText;
		}

		readonly bool isBulk;
		readonly bool useParameters;
		readonly int statementIndex;

		#region SuppressResourceStringsCheckRegion

		protected override void Build()
		{
			CreateNamesAndValues();

			var fromClause = GetFromClause();

			if (useParameters)
			{
				CommandText.Append(
$@"EXEC sys.sp_executesql N'UPDATE {AddSchemaName(TableName)} SET
	{SetClause}
{fromClause}WHERE 1=1
	{WhereClause};
{SetRowCountClause}
'
, N'{ParameterDeclarations}'
, {ParameterValues};
");
			}
			else
			{
				CommandText.Append(
$@"UPDATE {AddSchemaName(TableName)} SET
	{SetClause}
{fromClause}WHERE 1=1
	{WhereClause};
");
			}
		}

		string GetFromClause()
		{
			if (Schema != null)
			{
				if (!string.IsNullOrWhiteSpace(Schema.PkIndexName))
				{
					var pk = Schema.PK.Name;
					if (namesAndValues.Any(p => p.IsWhere && p.ColumnName.Equals(pk, StringComparison.OrdinalIgnoreCase)))
					{
						return $"FROM\r\n\t{AddSchemaName(TableName)} WITH (INDEX([{Schema.PkIndexName}]), UPDLOCK)\r\n";
					}
				}
			}

			return string.Empty;
		}

		internal string GetFromClauseForBulk()
		{
			return GetFromClause();
		}

		#endregion // SuppressResourceStringsCheckRegion

		public override bool IsConcurrencyCheckRequired
		{
			get { return true; }
		}

		protected bool AddDeclarationClauseForGeographyColumns(int paramIndex, SchemaColumn schemaColumn, object value)
		{
			if (schemaColumn.SqlDbTypeDeclaration.Equals("geography", StringComparison.OrdinalIgnoreCase)) // part of sql query
			{
				var valueString = GetQuoteEscapeTruncateAndCompress(value, schemaColumn);
				if (!valueString.Equals(SQL_NULL_KEYWORD, StringComparison.OrdinalIgnoreCase))
				{
					CommandText.AppendLine(string.Format(CultureInfo.InvariantCulture,
						"DECLARE @{0}_{1} geography = {2};" // part of sql query
						, statementIndex // 0
						, paramIndex     // 1
						, valueString    // 2
						));

					return true;
				}
			}

			return false;
		}

		protected bool AddSetClause()
		{
			bool hasOnlySystemChanges = true;

			var columns = Row.Table.Columns;
			for (int i = 0; i < columns.Count; i++)
			{
				DataColumn col = columns[i];
				try
				{
					if (IsRowValueChanged(Row, col))
					{
						var paramColumnName = col.ColumnName;

						var schemaColumn = Schema.GetSchemaColumn(col.ColumnName);
						var paramType = schemaColumn.SqlDbTypeDeclaration;
						var value = Row[col];

						if (schemaColumn.IsLargeBinaryOrText)
						{
							value = GetSourceValueIfSet(Row, value, schemaColumn);
							value = AddDataParameterAndBlobSaverIfLargeBlobOrText(Row, value, schemaColumn);
						}
						var paramValue = GetQuoteEscapeTruncateAndCompress(value, schemaColumn);
						setNamesAndValues.Add(new NameAndValue { ColumnName = paramColumnName, TypeDeclaration = paramType, Value = paramValue, IsSet = true, IsDeclaration = true });

						hasOnlySystemChanges &= IsSystemColumn(col) && !schemaColumn.ShouldCheckConcurrency(Row, col);
					}
				}
				catch (InvalidTableNameException)
				{
					CommandText.Append(ZSaver.TableDoesNotExistIdentifier).Append(" [").Append(TableName).Append("]"); // until we have AutoSchema.GetSchemaColumnSafe()
				}
			}

			return hasOnlySystemChanges;
		}

		protected virtual bool IsRowValueChanged(DataRow row, DataColumn column)
		{
			return DataUtils.IsRowValueChanged(row, column);
		}

		public static bool IsSystemColumn(DataColumn column)
		{
			var columnName = column.ColumnName;
			var prefixLenght = columnName.IndexOf('_');
			if (prefixLenght > 0)
			{
				columnName = columnName.Substring(prefixLenght);
			}

			return columnName.Equals(CargoWise.Schema.Schema.IsValidColumnSuffix, StringComparison.OrdinalIgnoreCase)
				|| columnName.Equals(CargoWise.Schema.Schema.SystemLastEditTimeUtcColumnSuffix, StringComparison.OrdinalIgnoreCase)
				|| columnName.Equals(CargoWise.Schema.Schema.SystemLastEditUserColumnSuffix, StringComparison.OrdinalIgnoreCase);
		}

		protected override void AddConcurrencyCheckWhereClause(bool ignoreAllConcurrencyCheck = false)
		{
			var tableName = Row.Table.TableName;
			var paramIndex = namesAndValues.Count;
			var groupIndex = 0;
			foreach (DataColumn column in Row.Table.Columns)
			{
				var schemaColumn = Schema.GetSchemaColumn(column.ColumnName);

				var isPK = column.ColumnName == Schema.PK.Name;
				if (isPK || !ignoreAllConcurrencyCheck && schemaColumn.ShouldCheckConcurrency(Row, column))
				{
#if DEBUG
					if (!isPK)
					{
						ConcurrencyCheckFields.Add(column);
					}
#endif

					var rowHasCurrentVersion = Row.HasVersion(DataRowVersion.Current);
					object originalData = Row[column, DataRowVersion.Original];
					object currentData = (rowHasCurrentVersion) ? Row[column, DataRowVersion.Current] : null;
					var concurrencyPolicy = (!ignoreAllConcurrencyCheck) ? DataUtils.GetConcurrencyPolicy(Row, column) : null;

					var multiValue = !isPK && concurrencyPolicy.AllowAutomaticMergeIfDatabaseValuesAreEqual(Row, column) && rowHasCurrentVersion && !originalData.Equals(currentData);
					if (multiValue)
					{
						groupIndex++;
						paramIndex += AddParameter(groupIndex, paramIndex, column.ColumnName, schemaColumn, originalData, matchSetParameter: false);
						paramIndex += AddParameter(groupIndex, paramIndex, column.ColumnName, schemaColumn, currentData, matchSetParameter: true);
					}
					else
					{
						paramIndex += AddParameter(null, paramIndex, column.ColumnName, schemaColumn, originalData, matchSetParameter: false);
					}
				}
			}
		}

		int AddParameter(int? groupIndex, int paramIndex, string columnName, SchemaColumn schemaColumn, object value, bool matchSetParameter = false)
		{
			int result = 0;

			string paramName = string.Format("@{0}", paramIndex);
			if (isBulk)
			{
				paramName = string.Format(CultureInfo.InvariantCulture, "@{0}_{1}", statementIndex, paramIndex);
			}
			string paramType;
			string paramValue;

			if (matchSetParameter)
			{
				var param = setNamesAndValues.FirstOrDefault(p => p.ColumnName == columnName);
				if (param != null)
				{
					if (AddDeclarationClauseForGeographyColumns(paramIndex, schemaColumn, value))
					{
						param.Value = string.Format(CultureInfo.InvariantCulture, "@{0}_{1}", statementIndex, paramIndex); // part of sql query
					}

					setNamesAndValues.Remove(param);
					param.GroupIndex = groupIndex;
					param.Name = paramName;
					param.IsWhere = true;
					namesAndValues.Add(param);

					return 1;
				}
			}

			if (value.Equals(DBNull.Value))
			{
				namesAndValues.Add(new NameAndValue { GroupIndex = groupIndex, ColumnName = columnName, Value = SQL_NULL_KEYWORD, IsWhere = true });
			}
			else if (schemaColumn.IsLargeBinaryOrText)
			{
				var pair = CreateLargeBinaryOrTextConcurrencyCheck(value, schemaColumn);
				namesAndValues.Add(new NameAndValue { GroupIndex = groupIndex, ColumnName = pair.Key, Name = paramName, TypeDeclaration = "nvarchar(max)", Value = pair.Value, IsWhere = true, IsDeclaration = true }); // sql data type

				result++;
			}
			else
			{
				paramType = schemaColumn.SqlDbTypeDeclaration;
				paramValue = AddDeclarationClauseForGeographyColumns(paramIndex, schemaColumn, value) ? string.Format(CultureInfo.InvariantCulture, "@{0}_{1}", statementIndex, paramIndex) : GetQuoteEscapeTruncateAndCompress(value, schemaColumn);
				namesAndValues.Add(new NameAndValue { GroupIndex = groupIndex, ColumnName = columnName, Name = paramName, TypeDeclaration = paramType, Value = paramValue, IsWhere = true, IsDeclaration = true });

				result++;
			}

			return result;
		}

		internal override void AddConcurrencyCheck(StringBuilder currentCommandText, string concurrencyErrorText)
		{
			if (IsConcurrencyCheckRequired)
			{
				if (useParameters)
				{
					currentCommandText.AppendLine(string.Format("if (@row_count = 0) RAISERROR('{0}', 16, 1);", concurrencyErrorText)); // part of sql command
				}
				else
				{
					base.AddConcurrencyCheck(currentCommandText, concurrencyErrorText);
				}
			}
		}

		#region Parameterization

		class NameAndValue
		{
			public int? GroupIndex;
			public string ColumnName;
			public string Name;
			public string TypeDeclaration;
			public string Value;
			public bool IsSet;
			public bool IsWhere;
			public bool IsDeclaration;
		}

		List<NameAndValue> setNamesAndValues;
		List<NameAndValue> namesAndValues;

		void CreateNamesAndValues()
		{
			setNamesAndValues = new List<NameAndValue>();
			namesAndValues = new List<NameAndValue>();
			if (IsConcurrencyCheckRequired)
			{
				namesAndValues.Add(new NameAndValue { Name = "@0", TypeDeclaration = "int out", Value = "@row_count out", IsDeclaration = true }); // part of sql procedure declaration
			}

			bool hasOnlySystemChanges = AddSetClause();
			AddConcurrencyCheckWhereClause(hasOnlySystemChanges);
			CheckSetParameters();
		}

		internal void CreateNamesAndValuesForBulk()
		{
			setNamesAndValues = new List<NameAndValue>();
			namesAndValues = new List<NameAndValue>();
			bool hasOnlySystemChanges = AddSetClause();
			AddConcurrencyCheckWhereClause(hasOnlySystemChanges);
			CheckSetParameters();
		}

		internal IDictionary<string, string> GetSetNamesAndValuesForBulk()
		{
			Dictionary<string, string> setNamesAndValuesForBulk = new Dictionary<string, string>();

			foreach (var nameAndValue in namesAndValues.Where(p => p.IsSet))
			{
				if (setNamesAndValuesForBulk.ContainsKey(nameAndValue.ColumnName))
				{
					continue;
				}
				setNamesAndValuesForBulk.Add(nameAndValue.ColumnName, useParameters ? nameAndValue.Name : nameAndValue.Value);
			}

			return setNamesAndValuesForBulk;
		}

		internal string GetPkValueForBulk()
		{
			string pkValue = "";
			if (Schema != null)
			{
				if (useParameters)
				{
					var pkNameAndValue = namesAndValues.FirstOrDefault(p => p.IsWhere && p.ColumnName == Schema.PK.Name);
					if (null != pkNameAndValue)
					{
						pkValue = pkNameAndValue.Name;
					}
				}
				else
				{
					pkValue = GetQuoteEscapeTruncateAndCompress(Row[Schema.PK.Name], Schema.PK);
				}
			}

			return pkValue;
		}

		void CheckSetParameters()
		{
			var paramIndex = namesAndValues.Count;
			foreach (var param in setNamesAndValues)
			{
				var paramName = "";

				if (isBulk)
				{
					paramName = string.Format(CultureInfo.InvariantCulture, "@{0}_{1}", statementIndex, paramIndex++);
				}
				else
				{
					paramName = string.Format(CultureInfo.InvariantCulture, "@{0}", paramIndex++);
				}

				param.Name = paramName;
				namesAndValues.Add(param);
			}
		}

		public string SetClause
		{
			get { return string.Join(",\r\n\t", namesAndValues.Where(p => p.IsSet).Select(p => string.Format(CultureInfo.InvariantCulture, "{0} = {1}", p.ColumnName, useParameters ? p.Name : p.Value))); }
		}

		public string WhereClause
		{
			get
			{
				return string.Join("\r\n\t", namesAndValues
					.Where(p => p.IsWhere)
					.Select(p => p.GroupIndex.HasValue
						? string.Format(CultureInfo.InvariantCulture, "AND ({0})", string.Join(" OR ", namesAndValues.Where(g => g.GroupIndex == p.GroupIndex).Select(GetClauseFromNameValue))) // sql operator
						: string.Format(CultureInfo.InvariantCulture, "AND {0}", GetClauseFromNameValue(p)) // sql operator
						)
					.Distinct()
					);
			}
		}

		string GetClauseFromNameValue(NameAndValue nameValue)
		{
			if (nameValue.Value == SQL_NULL_KEYWORD)
			{
				return FormattableString.Invariant($"{nameValue.ColumnName} is NULL"); // sql value NULL
			}

			if (nameValue.TypeDeclaration.Equals("geography", StringComparison.OrdinalIgnoreCase)) // sql geography type
			{
				return FormattableString.Invariant($"geography::STGeomFromText({nameValue.ColumnName}.STAsText(), 4326).STEquals({(useParameters ? nameValue.Name : nameValue.Value)}) = 1"); // sql geography api
			}

			return FormattableString.Invariant($"{nameValue.ColumnName} = {(useParameters ? nameValue.Name : nameValue.Value)}"); // sql operator
		}

		public string SetRowCountClause
		{
			get { return (IsConcurrencyCheckRequired) ? "SET @0 = @@ROWCOUNT;" : string.Empty; } // sql command part
		}

		public string ParameterDeclarations
		{
			get { return string.Join(", ", namesAndValues.Where(p => p.IsDeclaration).Select(p => string.Format("{0} {1}", p.Name, p.TypeDeclaration))); }
		}

		public string ParameterValues
		{
			get { return string.Join(", ", namesAndValues.Where(p => p.IsDeclaration).Select(p => string.Format("{0} = {1}", p.Name, p.Value))); }
		}

		#endregion // Parameterization
	}
}
