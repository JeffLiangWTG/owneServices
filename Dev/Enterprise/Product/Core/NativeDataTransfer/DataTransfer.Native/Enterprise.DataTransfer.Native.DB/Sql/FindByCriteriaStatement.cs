using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Schema;
using Microsoft.SqlServer.Types;
using static System.FormattableString;

namespace Enterprise.DataTransfer.Native.DB.Sql
{
	public class FindByCriteriaStatement : SqlStatement
	{
		public FindByCriteriaStatement(Table table, ColumnDef[] columnsDef, IEnumerable<MultipleValueCriteria> criterias, Dictionary<string, SqlParameter> keyToParameters, Dictionary<string, TvpItem> tvpParameters)
			: base(table.Name, columnsDef, keyToParameters)
		{
			this.criterias = criterias;
			this.tvpParameters = tvpParameters;
		}

		readonly IEnumerable<MultipleValueCriteria> criterias;
		readonly Dictionary<string, TvpItem> tvpParameters;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		protected override string CreateFromClause()
		{
			var schema = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema(tableName);
			var schemaName = schema?.SqlSchemaName ?? "dbo";
			return " FROM " + schemaName + "." + tableName;
		}

		protected override string CreateWhereClause()
		{
			var whereStatements = criterias.Where(c => c.TableName == tableName).Select(criteria => CreateWhereSearchCondition(criteria)).ToArray();
			var result = whereStatements.Any() ? " WHERE " + string.Join(" AND ", whereStatements) : string.Empty;
			return result;
		}

		internal string CreateWhereSearchCondition(MultipleValueCriteria criteria)
		{
			if (tvpParameters != null)
			{
				var key = Invariant($"{criteria.TableName}{criteria.ColumnName}"); // SQL Text used for Table valued Parameters
				if (!tvpParameters.TryGetValue(key, out var tvp))
				{
					tvp = new TvpItem(criteria.TableName, criteria.ColumnName, Invariant($"@tvpParam{tvpParameters.Count}"), criteria.Values); // SQL Text used for Table valued Parameters
					tvpParameters.Add(key, tvp);
				}
				return Invariant($"({criteria.ColumnName} in (SELECT value FROM {tvp.ParamName}))"); // SQL Text used for Table valued Parameters
			}
			else
			{
				var values = criteria.Values;

				if (!values.Any())
				{
					return string.Empty;
				}

				var columnDef = Table.Get(criteria.TableName).Columns[criteria.ColumnName];

				var clauseBuilder = new StringBuilder();
				var columnName = columnDef.Name;
				var valueArray = values.Select(i => ValueToString(i, columnDef)).ToArray();
				var valueArrayCount = valueArray.Length;

				for (var i = 0; i < valueArrayCount; ++i)
				{
					var valueString = valueArray[i];

					if (valueString == null)
					{
						clauseBuilder.Append(NullClause(columnName));
					}
					else
					{
						var sqlParameter = AddSqlParameter(valueString, columnDef);
						var sqlParameterName = sqlParameter.Name;

						if (valueString.Contains("%"))
						{
							clauseBuilder.Append(LikeClause(columnName, sqlParameterName));
						}
						else
						{
							clauseBuilder.Append(EqualClause(columnName, sqlParameterName));
						}
					}

					if (i < valueArrayCount - 1)
					{
						clauseBuilder.Append(keywordOr);
					}
				}

				var clause = clauseBuilder.ToString();
				return (string.IsNullOrEmpty(clause)) ? string.Empty : "(" + clause + ")";
			}
		}

		string ValueToString(object value, ColumnDef columnDef)
		{
			var valueType = value.GetType();

			if (valueType == typeof(bool))
			{
				return (bool)value ? "Y" : "N";
			}
			else if (valueType == typeof(DateTime))
			{
				return ((DateTime)value).ToString("s", CultureInfo.InvariantCulture);
			}
			else if (valueType == typeof(DateTimeOffset))
			{
				return ((DateTimeOffset)value).ToString("O", CultureInfo.InvariantCulture);
			}
			else if (valueType == typeof(SqlGeography))
			{
				return ((SqlGeography)value).AsTextZM().ToSqlString().ToString();
			}
			else if (valueType == typeof(DBNull))
			{
				return null;
			}

			var valueString = value.ToString().Trim();
			if (valueType == typeof(String) && columnDef.DoesDataTypeRepresentBoolean())
			{
				return DoesValueRepresentBooleanTrue(valueString) ? "Y" : "N";
			}

			return valueString;
		}

		bool DoesValueRepresentBooleanTrue(string value)
		{
			var lowercaseValue = value.ToLower();
			return lowercaseValue == "true" || lowercaseValue == "y"; // actual ToString value for bool (and Y/N)
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "string for sql")]
		string NullClause(string columnName)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0} IS NULL", columnName);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "string for sql")]
		string LikeClause(string columnName, string value)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0} LIKE {1}", columnName, value);
		}

		string EqualClause(string columnName, string value)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}={1}", columnName, value); // string for sql
		}

		const string keywordOr = " OR ";
	}
}
