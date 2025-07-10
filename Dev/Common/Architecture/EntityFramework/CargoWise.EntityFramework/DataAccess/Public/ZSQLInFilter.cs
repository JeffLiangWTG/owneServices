using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public sealed class ZSQLInFilter : AbstractFilterPart, IFilterPart, IDbOnlyFilter
	{
		internal ZSQLInFilter(SchemaColumn column, SQLComparisonOperator comparisonOperator, ICollection values, ComparisonOptions options, bool allowTableValuedParameter = false)
		{
			if (column == null)
			{
				throw new ArgumentNullException(nameof(column));
			}

			if (comparisonOperator == null)
			{
				throw new ArgumentNullException(nameof(comparisonOperator));
			}

			if (values == null)
			{
				throw new ArgumentNullException(nameof(values));
			}

			if (!comparisonOperator.In(SQLComparisonOperator.Equal, SQLComparisonOperator.NotEqual, SQLComparisonOperator.StartsWith, SQLComparisonOperator.EndsWith))
			{
				throw new NotSupportedException("comparisonOperator must be SQLComparisonOperator.Equal, NotEqual, StartsWith, or EndsWith.");
			}

			Column = column;
			ComparisonOperator = comparisonOperator;

			var distinctValues = values.Cast<object>().Distinct();
			// using GetHashCode to work around collections with potentially different data types
			// as long as the list is stable for the session, this is good enough
			// may get different result for x86/x64 and different builds of .Net
			// toString would be slower and still have to deal with null
			try
			{
				Values = distinctValues.OrderBy(x => x).ToArray();
			}
			catch (Exception)
			{
				// fallback for multitype value array
				Values = distinctValues.OrderBy(x => x != null ? x.GetHashCode() : 0).ToArray();
			}
			ComparisonOptions = options;

			if (ComparisonOperator.In(SQLComparisonOperator.StartsWith, SQLComparisonOperator.EndsWith)
				&& Values.Any(value => !(value == null || value is string || value is ZString)))
			{
				throw new NotSupportedException("comparisonOperator SQLComparisonOperator.StartsWith and EndsWith can be used only with string values.");
			}

			this.allowTableValuedParameter = allowTableValuedParameter;
		}

		readonly object[] Values;

		public IEnumerable<object> GetValues()
		{
			return Values;
		}

		public readonly SchemaColumn Column;
		public readonly SQLComparisonOperator ComparisonOperator;
		internal readonly ComparisonOptions ComparisonOptions;

		public ZQuery[] GetOrParts()
		{
			ZQuery[] result = Array.Empty<ZQuery>();
			if (ComparisonOperator != SQLComparisonOperator.NotEqual)
			{
				result = new ZQuery[Values.Length];
				for (int i = 0; i < Values.Length; i++)
				{
					if (Column.IsNullable || Values[i] != null)
					{
						result[i] = new ZQuery(Column, ComparisonOperator, Values[i], ComparisonOptions);
					}
				}
			}
			return result;
		}

		#region Equals / GetHashCode

		public override bool Equals(object obj)
		{
			ZSQLInFilter rhs = obj as ZSQLInFilter;
			bool result = rhs != null;

			result = result && Column == rhs.Column;
			result = result && ComparisonOperator == rhs.ComparisonOperator;
			result = result && ItemsEqual(Values, rhs.Values);
			return result;
		}

		public override int GetHashCode()
		{
			return Values.Length ^ (Values.Length == 0 ? 0 : Values[0].GetHashCode());
		}

		static bool ItemsEqual(object[] lhs, object[] rhs)
		{
			bool result = lhs.Length == rhs.Length;
			if (result)
			{
				for (int i = 0; i < lhs.Length; i++)
				{
					if (!object.Equals(lhs[i], rhs[i]))
					{
						result = false;
						break;
					}
				}
			}
			return result;
		}

		#endregion

		#region SuppressResourceStringsCheckRegion

		#region IFilterPart Members

		IEnumerable<SchemaColumn> IFilterPart.BlobFilters
		{
			get { return Column.IsLargeBinaryOrText ? new[] { Column } : Enumerable.Empty<SchemaColumn>(); }
		}

		IFilterPart[] IFilterPart.GetSimplifiedVersion(JoinCondition lastJoinCondition)
		{
			return FilterIsEmpty ? null : new IFilterPart[] { this };
		}

		IEnumerable<IFilterPart> IFilterPart.FilterParts => Enumerable.Empty<IFilterPart>();

		void IFilterPart.DisableModifications()
		{
			// Object is immutable so no work to do
		}

		public IFilterPart DeepClone()
		{
			return (ZSQLInFilter)MemberwiseClone();   // Immutable
		}

		public override bool HasParameters
		{
			get { return Values.Length > 0; }
		}

		public ZNonPersistentDataQuery ParameterisedSql(ParameterNameFactory factory)
		{
			var builder = new SqlBuilder(factory);
			ParameterisedSql(builder);
			return new ZNonPersistentDataQuery(builder.ToString(), builder.GetParameters());
		}

		public void ParameterisedSql(SqlBuilder sqlBuilder)
		{
			if (UseTableValuedParameter)
			{
				var parameter = ZSqlParameter.New(sqlBuilder.NameFactory, Values, Column, ComparisonOperator, ComparisonOptions, isTableValued: true);

				sqlBuilder.Append("(");
				sqlBuilder.Append(Column.Name)
				.Append(((ComparisonOperator == SQLComparisonOperator.Equal) ? " in" : " not in"))
				.Append(" (SELECT Value FROM ")
				.AppendParameter(parameter)
				.Append(")");
				if (NeedNotEmptyCheck)
				{
					sqlBuilder.Append(" AND " + Column.Name + " <> ''");
				}
				sqlBuilder.Append(")");
			}
			else if (Values.Length <= MAXIMUM_ELEMENTS_FOR_PARAMETERISATION)
			{
				GetSql(sqlBuilder, true, true);
			}
			else
			{
				GetSql(sqlBuilder, true, false);
			}
		}

		bool UseTableValuedParameter =>
				allowTableValuedParameter
			&& ParameterSettingsCache.TVPRule.ShouldUseTVP(Values.Length)
			&& Column.SupportTVP
			&& ComparisonOperator.In(SQLComparisonOperator.Equal, SQLComparisonOperator.NotEqual)
			&& Values.All(value => value != null && value != DBNull.Value);

		bool NeedNotEmptyCheck
		{
			get
			{
				return (Column is SchemaStringColumn stringColumn) && stringColumn.IsNonBlankFilteredIndexParticipant
					&&
					(
						ComparisonOperator == SQLComparisonOperator.Equal && Values.All(value => !string.IsNullOrEmpty(value.ToString()))
						|| ComparisonOperator == SQLComparisonOperator.NotEqual && Values.Any(value => string.IsNullOrEmpty(value.ToString()))
					);
			}
		}

		readonly bool allowTableValuedParameter;
		public int ConstantsTempTableID { get; set; }
		public const int MAXIMUM_ELEMENTS_FOR_PARAMETERISATION = 100;

		public const int MAXIMUM_SUPPORTS_COUNT_FOR_PARAMETER = 2100;

		public string LiteralTextADO
		{
			get
			{
				var builder = new SqlBuilder(SqlBuilder.QueryType.LiteralADO);
				AddLiteralTextADO(builder);
				return builder.ToString();
			}
		}

		public void AddLiteralTextADO(SqlBuilder sqlBuilder)
		{
			GetSql(sqlBuilder, false, true);
		}

		#region GetSql
		#region SuppressResourceStringsCheckRegion

		string GetParametrizedSqlValueText(object value)
		{
			return ZSqlParameter.New("@fakey", value, Column, SQLComparisonOperator.Equal, ComparisonOptions).ParameterValueTextSql;
		}

		void GetSql(SqlBuilder result, bool isForSql, bool parameterise)
		{
			if (ComparisonOperator == SQLComparisonOperator.StartsWith || ComparisonOperator == SQLComparisonOperator.EndsWith)
			{
				if (ComparisonOperator == SQLComparisonOperator.StartsWith && Values.Length * 2 > MAXIMUM_ELEMENTS_FOR_PARAMETERISATION)
				{
					parameterise = false;
				}
				result.Append("(");

				var schemaResolver = ObjectFactory.Get<IApplicationSchemaResolver>();
				var pkColumnName = schemaResolver.GetTableSchema(Column.TableName).PK.Name;
				result.Append(pkColumnName + " IN (SELECT " + pkColumnName + " FROM " + Column.TableName);
				AddConstantsTempTable(result, parameterise);
				result.Append("))");
			}
			else
			{
				BuildSqlPart(result, Column.Name, Values, isForSql, parameterise);
			}
		}

		void AddConstantsTempTable(SqlBuilder builder, bool parameterise)
		{
			var tempTableName = "ConTempTable" + ConstantsTempTableID;

			builder.Append(" JOIN ( ");
			GenerateConstValuesSQL(builder, parameterise);
			builder.Append($" ) {tempTableName} ON ");
			builder.Append(Column.Name + $" LIKE {tempTableName}.escapedValue ESCAPE '" + ComparisonOperator.SqlEscapeCharacter + "' ");

			if (ComparisonOperator == SQLComparisonOperator.StartsWith)
			{
				builder.Append("AND " + Column.Name + $" >= {tempTableName}.value ");
				builder.Append("AND " + Column.Name + $" <= CONCAT(SUBSTRING({tempTableName}.value, 1, LEN({tempTableName}.value) -1), 'þ') ");
			}
		}

		internal void GenerateConstValuesSQL(SqlBuilder builder, bool parameterise)
		{
			if (Values == null || !Values.Any())
			{
				throw new ArgumentException("Input values cannot be null or empty.");
			}

			var values = Values.DistinctBySqlComparisonOperator(ComparisonOperator).ToArray();

			builder.Append("SELECT ");
			if (ComparisonOperator == SQLComparisonOperator.StartsWith)
			{
				builder.Append("value, ");
			}
			builder.Append("escapedValue FROM (VALUES ");
			for (var i = 0; i < values.Length; i++)
			{
				if (i != 0)
				{
					builder.Append(", ");
				}
				builder.Append("(");
				if (ComparisonOperator == SQLComparisonOperator.StartsWith)
				{
					AppendParameter(values[i], builder, parameterise);
					builder.Append(", ");
				}

				AppendParameter(ComparisonOperator.EscapedSqlValue(values[i]), builder, parameterise);
				builder.Append(")");
			}

			builder.Append(") AS Con(");
			if (ComparisonOperator == SQLComparisonOperator.StartsWith)
			{
				builder.Append("value, ");
			}
			builder.Append("escapedValue)");
		}

		void AppendParameter(object value, SqlBuilder sb, bool parameterise)
		{
			if (parameterise)
			{
				sb.AppendParameter(Column, value, SQLComparisonOperator.Equal, ComparisonOptions, isTableValued: false);
			}
			else
			{
				sb.Append(GetParametrizedSqlValueText(value));
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Refactoring is needed in future")]
		void BuildSqlPart(SqlBuilder sb, string columnName, IList values, bool isForSql, bool parameterise)
		{
			if (Column.ColumnType == SchemaColumnType.DateTimeOffset && !isForSql)
			{
				//From https://social.msdn.microsoft.com/Forums/en-US/2f1a8b79-57e4-4c29-94dd-7d3a0dd18fc8/rowfilter-string-for-a-datetimeoffset-value?forum=adodotnetdataset
				//ADO does not support DateTimeOffset. DateTimeOffset->String->DateTime treats it as UTC, then converts it to system time zone.
				//But if we do this to both sides, then we are effectively comparing UTCs.
				columnName = "CONVERT(CONVERT(" + columnName + ", System.String), System.DateTime)";
			}

			if (values.Count == 0)
			{
				if (ComparisonOperator == SQLComparisonOperator.Equal)
				{
					sb.Append("1 = 0");
				}
			}
			else if (values.Count == 1 && (Column.ColumnType != SchemaColumnType.String || !((SchemaStringColumn)Column).IsNonBlankFilteredIndexParticipant))
			{
				sb.Append(columnName);
				sb.Append(ComparisonOperator == SQLComparisonOperator.NotEqual ? " <> " : " = ");
				AppendParameter(values[0], sb, parameterise);
			}
			else if (Column.ColumnType == SchemaColumnType.Bool && values.Count > 1 && values.Contains(true) && values.Contains(false))
			{
				sb.Append(ComparisonOperator == SQLComparisonOperator.NotEqual ? "1 = 0" : "1 = 1");
			}
			else
			{
				bool iHaveEmptyGuidValue = false;
				var iHaveEmptyStringValue = false;
				sb.Append("(");
				sb.Append(columnName);
				if (ComparisonOperator == SQLComparisonOperator.NotEqual)
				{
					sb.Append(" not");
				}
				sb.Append(" in (");
				for (int i = 0; i < values.Count; i++)
				{
					if (i > 0)
					{
						sb.Append(", ");
					}
					AppendParameter(values[i], sb, parameterise);
					iHaveEmptyGuidValue |= Column.ColumnType == SchemaColumnType.Guid && !Column.IsNullable && new ZGuid(values[i]).IsEmpty;
					iHaveEmptyStringValue |= Column.ColumnType == SchemaColumnType.String && string.IsNullOrEmpty(values[i]?.ToString());
				}

				sb.Append(")");
				if (((!iHaveEmptyStringValue && ComparisonOperator == SQLComparisonOperator.Equal) || (iHaveEmptyStringValue && ComparisonOperator == SQLComparisonOperator.NotEqual)) && Column.ColumnType == SchemaColumnType.String && ((SchemaStringColumn)Column).IsNonBlankFilteredIndexParticipant)
				{
					sb.Append(" AND " + Column.Name + " <> ''");
				}
				if (iHaveEmptyGuidValue)
				{
					sb.Append(" or ");
					sb.Append(columnName);
					sb.Append(" is null");
				}
				sb.Append(")");
			}
		}

		#endregion
		#endregion

		public bool NeedsBrackets
		{
			get { return false; }
		}

		public bool FilterIsEmpty
		{
			get { return Values.Length == 0 && ComparisonOperator == SQLComparisonOperator.Equal; }
		}

		bool IFilterPart.ContainsOrOperator
		{
			get { return Values.Length > 1 && ComparisonOperator != SQLComparisonOperator.Equal && ComparisonOperator != SQLComparisonOperator.NotEqual; }
		}

		#endregion

		#region IDbOnlyFiler Members

		bool IDbOnlyFilter.IsDbOnlyFilter
		{
			get { return ComparisonOperator != SQLComparisonOperator.Equal && ComparisonOperator != SQLComparisonOperator.NotEqual; }
		}

		#endregion

		#region ToCSharpCode
#if DEBUG

		public string ToCSharpCode()
		{
			return ToCSharpCode("", new NumberPublisher(), "query");
		}

		public string ToCSharpCode(string joinCondition, NumberPublisher publisher, string queryVariable)
		{
			string objects = "";
			string separator = "";
			foreach (object o in Values)
			{
				string textValue = ZSqlParameter.New("@fakey", o, Column).ParameterValueTextSql.Replace("'", "\"");
				objects += separator + textValue;
				separator = ", ";
			}
			return queryVariable + ".AddToFilter(" + (!string.IsNullOrEmpty(joinCondition) ? joinCondition + ", " : "") + Column.TableName + "Schema." + Column.ObjectName + ", SQLComparisonOperator." + ComparisonOperator.ToString() + ", new object[] { " + objects + " } );";
		}

#endif
		#endregion

		#endregion // SuppressResourceStringsCheckRegion
	}
}
