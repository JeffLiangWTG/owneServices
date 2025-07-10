using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework
{
	public class ZNonPersistentDataQuery : AbstractFilterPart, IFilterPart
	{
		public ZNonPersistentDataQuery(string sql)
			: this(sql, ZSqlParameter.EmptyArray, false, cmdTimeout: null)
		{
		}

		public ZNonPersistentDataQuery(string sql, ZSqlParameterCollection parameters, bool simpleConstruct = false, int? cmdTimeout = null)
			: this(sql, parameters.ToArray(), simpleConstruct, cmdTimeout)
		{
		}

		internal ZNonPersistentDataQuery(string storedProcedureName, ZSqlParameter[] parameters, bool isStoredProc, bool simpleConstruct = false)
			: this(storedProcedureName, parameters, simpleConstruct, cmdTimeout: null)
		{
			this.isStoredProc = isStoredProc;
		}

		public ZNonPersistentDataQuery(string sql, ZSqlParameter[] parameters, bool simpleConstruct = false, int? cmdTimeout = null)
			: this((sql, parameters), simpleConstruct, cmdTimeout)
		{
		}

		internal ZNonPersistentDataQuery((string sql, ZSqlParameter[] parameters) sqlWithParameters, bool simpleConstruct = false, int? cmdTimeout = null)
		{
			var sql = sqlWithParameters.sql ?? String.Empty;
			var sqlParameters = sqlWithParameters.parameters ?? ZSqlParameter.EmptyArray;

#if DEBUG

			foreach (var p in sqlParameters.Where(x => x.SchemaColumn is SchemaStringColumn))
			{
				if (((SchemaStringColumn)p.SchemaColumn).IsNonBlankFilteredIndexParticipant)
				{
					if (!p.IsLiteralOnly && sql.IndexOf(p.SchemaColumn.Name + " <> ''", StringComparison.OrdinalIgnoreCase) < 0
						&& (IsComparisonRequiringNotEmptyCheck(sql, p) || IsInFilterRequiringNotEmptyCheck(sql, p, sqlParameters)))
					{
						ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture,
							"It appears following SQL does not contain support for filtered index on {0}:\r\n\t{1}\r\nPerhaps you need to add following check: {0} <> ''",
							p.SchemaColumn.Name, sql));
					}
				}
			}

#endif

			if (!simpleConstruct)
			{
				if (sqlParameters.Any(p => p.IsLiteralOnly))
				{
					var builder = new SqlBuilder();
					ReplaceParametersInQuery(builder, sql, sqlParameters,
						p => (p.IsLiteralOnly)
							? $"{p.ParameterValueTextSql}{LiteralGeneratedSqlComment}"
							: p.ParameterisedSql);
					sql = builder.ToString();
				}
			}

			this.ParameterisedQueryText = sql;
			this.parameters = sqlParameters;
			CmdTimeout = cmdTimeout;
		}

#if DEBUG

		bool IsInFilterRequiringNotEmptyCheck(string sql, ZSqlParameter parameter, ZSqlParameter[] allParameters)
		{
			var columnName = parameter.SchemaColumn.Name;
			if (sql.IndexOf(columnName + " in (", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				if (parameter.Value is ICollection<object> values)
				{
					return values.All(value => !value.Equals(""));
				}
				else
				{
					return allParameters.Where(p => p.SchemaColumn == parameter.SchemaColumn).All(p => !p.Value.Equals(""));
				}
			}
			else if (sql.IndexOf(columnName + " not in (", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				if (parameter.Value is ICollection<object> values)
				{
					return values.Any(value => value.Equals(""));
				}
				else
				{
					return allParameters.Where(p => p.SchemaColumn == parameter.SchemaColumn).Any(p => p.Value.Equals(""));
				}
			}

			return false;
		}

		bool IsComparisonRequiringNotEmptyCheck(string sql, ZSqlParameter parameter)
		{
			var columnName = parameter.SchemaColumn.Name;
			return
				(
					sql.IndexOf(columnName + " = ", StringComparison.OrdinalIgnoreCase) >= 0
					|| sql.IndexOf(columnName + " like ", StringComparison.OrdinalIgnoreCase) >= 0
					|| sql.IndexOf(columnName + " > ", StringComparison.OrdinalIgnoreCase) >= 0
					|| sql.IndexOf(columnName + " >= ", StringComparison.OrdinalIgnoreCase) >= 0
				)
				&& !parameter.Value.Equals("");
		}

#endif

		public bool IsEmpty
		{
			get { return Parameters.Length == 0 && ParameterisedQueryText.Length == 0; }
		}

		public ZSqlParameter[] Parameters
		{
			get { return parameters; }
		}
		readonly ZSqlParameter[] parameters;

		public readonly string ParameterisedQueryText;

		internal readonly bool isStoredProc;

		public int? CmdTimeout { get; private set; }

		public override bool HasParameters
		{
			get { return parameters.Length > 0 || ParameterisedQueryText.IndexOf("where", StringComparison.OrdinalIgnoreCase) >= 0; }
		}

		public override bool HasComparisonOperatorLike
		{
			get
			{
				if (ParameterisedQueryText != null && SqlTextProcessor.HasLike(ParameterisedQueryText))
				{
					return true;
				}

				return parameters != null && parameters.Any(x => x.HasComparisonOperatorLike);
			}
		}

#if DEBUG
		#region ToCSharpCode

		public string ToCSharpCode()
		{
			string varname;
			return ToCSharpCode("", new NumberPublisher(), out varname);
		}

		public string ToCSharpCode(string lastJoinCondition, NumberPublisher publisher, out string varName)
		{
			varName = "nonPersistentDataQuery" + publisher.GetNextVariableNumberSuffix();
			using (StringBuilderPool.Get(out var builder))
			{
				builder.AppendLine("<parameters>");
				builder.Append(varName + " = new ZNonPersistentDataQuery(\"" + ParameterisedQueryText + "\", <parameters>);");
				return builder.ToString();
			}
		}

		#endregion
#endif

		#region Equals / GetHashCode

		public override bool Equals(object obj)
		{
			ZNonPersistentDataQuery rhs = obj as ZNonPersistentDataQuery;
			return
				rhs != null &&
				ParameterisedQueryText == rhs.ParameterisedQueryText &&
				ParametersEqual(Parameters, rhs.Parameters);
		}

		public override int GetHashCode()
		{
			return ParameterisedQueryText.GetHashCode();
		}

		static bool ParametersEqual(ZSqlParameter[] lhs, ZSqlParameter[] rhs)
		{
			bool result = lhs.Length == rhs.Length;
			if (result)
			{
				for (int i = 0; i < lhs.Length; i++)
				{
					result = object.Equals(lhs[i].Value, rhs[i].Value);
				}
			}
			return result;
		}

		#endregion

		#region IFilterPart Members

		void IFilterPart.DisableModifications()
		{
			// Object is immutable so no work to do
		}

		IEnumerable<SchemaColumn> IFilterPart.BlobFilters
		{
			get { return Parameters.SelectMany(p => p.BlobFilters); }
		}

		ZNonPersistentDataQuery IFilterPart.ParameterisedSql(ParameterNameFactory factory)
		{
			return this;
		}

		void IFilterPart.ParameterisedSql(SqlBuilder sqlBuilder)
		{
			sqlBuilder.Append(ParameterisedQueryText, Parameters);
		}

		public string LiteralTextSql
		{
			get
			{
				var builder = new SqlBuilder();
				ReplaceParametersInQuery(builder, ParameterisedQueryText, Parameters, p => p.ParameterValueTextSql);
				return builder.ToString();
			}
		}

		public string LiteralTextADO
		{
			get
			{
				var builder = new SqlBuilder();
				AddLiteralTextADO(builder);
				return builder.GetSql();
			}
		}

		public void AddLiteralTextADO(SqlBuilder sqlBuilder)
		{
			ReplaceParametersInQuery(sqlBuilder, ParameterisedQueryText, Parameters, p => p.ParameterValueTextADO);
		}

		void ReplaceParametersInQuery(SqlBuilder sqlBuilder, string sql, ZSqlParameter[] @params, Func<ZSqlParameter, string> replacementFunc)
		{
			if (@params.Length == 0)
			{
				sqlBuilder.Append(sql);
			}
			else
			{
				var lastPositionWrittenFromSql = 0;
				var orderedParameters = @params.OrderByDescending(p => p.ParameterName.Length).ToArray();
				foreach (var atPosition in sql.AllIndexesOf('@'))
				{
					string atSubstring = "";
					for (int i = 0; i < orderedParameters.Length; i++)
					{
						var param = orderedParameters[i];
						if (atSubstring.Length != param.ParameterName.Length)
						{
							atSubstring = ((ZString)sql).SubstringSafe(atPosition, param.ParameterName.Length);
						}

						if (atSubstring.Equals(param.ParameterName, StringComparison.OrdinalIgnoreCase))
						{
							var atPos = atPosition;
							if (param.IsTableValued)
							{
								var tvpUsingSql = "SELECT Value FROM " + atSubstring;
								var pos = sql.IndexOf(tvpUsingSql, lastPositionWrittenFromSql, StringComparison.OrdinalIgnoreCase);
								if (pos > -1)
								{
									atSubstring = tvpUsingSql;
									atPos = pos;
								}
							}

							int gapLength = atPos - lastPositionWrittenFromSql;
							if (gapLength >= 0)
							{
								sqlBuilder.Append(sql.Substring(lastPositionWrittenFromSql, gapLength));
								sqlBuilder.Append(replacementFunc(param));
								lastPositionWrittenFromSql = atPos + atSubstring.Length;
							}
							else
							{
								ZStringBuilder errorMessageBuilder = new ZStringBuilder();
								errorMessageBuilder.Append((NoResString)"Unable to replace parameters in query because of invalid SQL.");
								errorMessageBuilder.Append("SQL:");
								errorMessageBuilder.Append(sql);
								errorMessageBuilder.Append((NoResString)"SQL Parameters:");
								foreach (ZSqlParameter parameter in @params)
								{
									errorMessageBuilder.Append(String.Format(CultureInfo.InvariantCulture, (NoResString)"Name: {0}, IsTableValued: {1}", parameter.ParameterName, parameter.IsTableValued));
								}
								errorMessageBuilder.Append((NoResString)"Local variables:");
								errorMessageBuilder.Append(String.Format(CultureInfo.InvariantCulture, (NoResString)"param.ParameterName: {0}", param.ParameterName));
								errorMessageBuilder.Append(String.Format(CultureInfo.InvariantCulture, "atPosition: {0}", atPosition));
								errorMessageBuilder.Append(String.Format(CultureInfo.InvariantCulture, "atPos: {0}", atPos));
								errorMessageBuilder.Append(String.Format(CultureInfo.InvariantCulture, "lastPositionWrittenFromSql: {0}", lastPositionWrittenFromSql));
								errorMessageBuilder.Append(String.Format(CultureInfo.InvariantCulture, "atSubstring: {0}", atSubstring));
								errorMessageBuilder.Append(String.Format(CultureInfo.InvariantCulture, "replacementFunc: {0}", replacementFunc(param)));
								ErrorReporter.ReportOnce("ZNonPersistentDataQuery_ReplaceParametersInQuery_InvalidSql", errorMessageBuilder.ToStringWithNewLineBetweenAppends());
							}

							break;
						}
					}
				}
				if (lastPositionWrittenFromSql < sql.Length)
				{
					sqlBuilder.Append(sql.Substring(lastPositionWrittenFromSql));
				}
			}
		}

		public IFilterPart DeepClone()
		{
			return new ZNonPersistentDataQuery(ParameterisedQueryText, parameters);
		}

		bool IFilterPart.NeedsBrackets
		{
			get { return true; }
		}

		bool IFilterPart.FilterIsEmpty
		{
			get { return ParameterisedQueryText.Length == 0; }
		}

		bool IFilterPart.ContainsOrOperator
		{
			get
			{
				if (ParameterisedQueryText.Length == 0)
				{
					return false;
				}
				else
				{
					OrDetector detector = new OrDetector();
					return detector.ContainsOrOperator(ParameterisedQueryText);
				}
			}
		}

		IFilterPart[] IFilterPart.GetSimplifiedVersion(JoinCondition lastJoinCondition)
		{
			return new IFilterPart[] { this };
		}

		IEnumerable<IFilterPart> IFilterPart.FilterParts => Enumerable.Empty<IFilterPart>();

		#endregion

		const string LiteralGeneratedSqlComment = " /* Parameterised value literalised by ZNonPersistentDataQuery */";
	}

	public class ZDataQuery : ZNonPersistentDataQuery
	{
		public ZDataQuery(ZConnectionInfo connectionInfo, string tableName, ZQuery query)
			: base(query.GetAsCompleteSQLStatementWithParameters(connectionInfo.PathToTables + tableName))
		{
			Query = query;
			TableName = tableName;
			ReloadExisitingRows = query.ReLoadExistingRows;
			Timeout = query.Timeout;
		}

		public ZDataQuery(string storedProcedureName, ZSqlParameter[] parameters, string tableName)
			: base(storedProcedureName, parameters, true, false)
		{
			TableName = tableName;
			Query = ZQuery.EmptyQuery;
		}

		public readonly string TableName;
		public readonly bool ReloadExisitingRows;
		public readonly int Timeout;
		internal readonly ZQuery Query;
	}
}
