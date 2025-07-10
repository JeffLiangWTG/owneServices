using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework
{
	public class SqlBuilder
	{
		public enum QueryType
		{
			Parameterised,
			Literal,
			LiteralADO
		}

		public SqlBuilder(QueryType queryType = QueryType.Parameterised)
		{
			this.queryType = queryType;
			if (queryType == QueryType.Parameterised)
			{
				parameterNameFactory = new ParameterNameFactory();
			}
		}

		public SqlBuilder(ParameterNameFactory parameterNameFactory)
		{
			this.parameterNameFactory = parameterNameFactory;
			queryType = QueryType.Parameterised;
		}

		readonly ParameterNameFactory parameterNameFactory;
		readonly QueryType queryType;
		readonly Stack<Action<object>> prefixes = new Stack<Action<object>>();
		readonly StringBuilder stringBuilder = new StringBuilder();
		readonly Dictionary<string, ZSqlParameter> parameters = new Dictionary<string, ZSqlParameter>();
		readonly HashSet<ZSqlParameter> parametersByValue = new HashSet<ZSqlParameter>(ZSqlParameter.DuplicateComparer.Instance);

		string selectStatement;
		public IEnumerable<ZSqlParameter> Parameters => parameters.Values;
		public ParameterNameFactory NameFactory => parameterNameFactory;

		#region Builder Methods

		/// <summary>
		/// If something is appended in the disposable region, we prefix it with arg
		/// </summary>
		internal IDisposable WithPrefix(string arg) => WithPrefix(_ => stringBuilder.Append(arg));

		/// <summary>
		/// If something is appended in the disposable region, we prefix it with arg
		/// </summary>
		internal IDisposable WithPrefix(Action<object> action)
		{
			prefixes.Push(action);
			return new PrefixPopper(this);
		}

		struct PrefixPopper : IDisposable
		{
			public PrefixPopper(SqlBuilder sqlBuilder)
			{
				this.sqlBuilder = sqlBuilder;
			}
			readonly SqlBuilder sqlBuilder;
			public void Dispose()
			{
				if (sqlBuilder.prefixes.Count > 0)
				{
					sqlBuilder.prefixes.Pop();
				}
			}
		}

		public void FinaliseSelectStatement()
		{
			// We are taking a copy of the initial select statement without clauses (e.g. "select * from Table")
			// this copy is used when UNION's require redeclaring all of the columns.
			// There is an obvious existing bug here: We don't respect "Top N" on union queries.
			// I am not going to fix this now because that is not the purpose of this refactor.
			selectStatement = stringBuilder.ToString();
		}

		/// <summary>
		/// We can insert parameters into the SqlBuilder before writing sql,
		/// This is for when we are expecting to nest this SqlBuilder in some other context that has its own parameters.
		/// </summary>
		public void ReserveParameter(ZSqlParameter parameter)
		{
			parameters[parameter.ParameterName] = parameter;
			parametersByValue.Add(parameter);
		}

		public SqlBuilder AppendSelectStatement()
		{
			stringBuilder.Append(selectStatement);
			return this;
		}

		public SqlBuilder Append(object arg)
		{
			var r = arg.ToString();
			if (!string.IsNullOrEmpty(r))
			{
				while (prefixes.Count > 0)
				{
					var actions = prefixes.ToList();
					actions.Reverse();
					prefixes.Clear(); // The recursion into here is a bit ugly. WithPrefix(Action) isn't used much so this is deletable with some work.
					foreach (var prefix in actions)
					{
						prefix.Invoke(arg);
					}
				}
			}
			stringBuilder.Append(r);
			return this;
		}

		public void Append(string parameterisedQueryText, ZSqlParameter[] newParameters)
		{
			if (queryType == QueryType.Parameterised)
			{
				foreach (var parameter in newParameters)
				{
					var originalParameterName = parameter.ParameterName;
					if (AddParameterAndCheckIfRenamed(parameter, out var newParameter))
					{
						parameterisedQueryText = Regex.Replace(parameterisedQueryText, originalParameterName + "\\b", newParameter.ParameterName);
					}
				}
			}
			else if (queryType == QueryType.LiteralADO)
			{
				foreach (var parameter in newParameters)
				{
					var originalParameterName = parameter.ParameterName;
					var valueText = GetLiteralTextADOValueText(parameter.SchemaColumn, parameter.Value);
					parameterisedQueryText = Regex.Replace(parameterisedQueryText, originalParameterName + "\\b", valueText);
				}
			}
			else if (queryType == QueryType.Literal)
			{
				foreach (var parameter in newParameters)
				{
					var originalParameterName = parameter.ParameterName;
					var valueText = parameter.ParameterValueTextSql;
					parameterisedQueryText = Regex.Replace(parameterisedQueryText, originalParameterName + "\\b", valueText);
				}
			}

			Append(parameterisedQueryText);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1066:DoNotUseGoToCaseOrDefault", Justification = "Baseline")]
		public SqlBuilder AppendParameter(SchemaColumn column, object value, SQLComparisonOperator comparison, ComparisonOptions comparisonOptions, bool isTableValued)
		{
			switch (queryType)
			{
				case QueryType.Parameterised:
					var parameter = ZSqlParameter.New(parameterNameFactory, value, column, comparison, comparisonOptions, isTableValued);
					AddParameterAndCheckIfRenamed(parameter, out var newParameter);
					if (parameter.IsLiteralOnly)
					{
						goto case QueryType.Literal;
					}
					else
					{
						return Append(newParameter.ParameterName);
					}

				case QueryType.LiteralADO:
					return Append(GetLiteralTextADOValueText(column, value));

				case QueryType.Literal:
					return Append(GetLiteralTextValueText(column, value));

				default:
					throw new NotImplementedException(FormattableString.Invariant($"Unknown query type: {queryType}"));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1066:DoNotUseGoToCaseOrDefault", Justification = "Baseline")]
		public SqlBuilder AppendParameter(ZSqlParameter parameter)
		{
			switch (queryType)
			{
				case QueryType.Parameterised:
					AddParameterAndCheckIfRenamed(parameter, out var newParameter);
					if (parameter.IsLiteralOnly)
					{
						goto case QueryType.Literal;
					}
					else
					{
						return Append(newParameter.ParameterName);
					}

				case QueryType.LiteralADO:
					return Append(parameter.ParameterValueTextADO);

				case QueryType.Literal:
					return Append(parameter.ParameterValueTextSql);

				default:
					throw new NotImplementedException(FormattableString.Invariant($"Unknown query type: {queryType}"));
			}
		}

		bool AddParameterAndCheckIfRenamed(ZSqlParameter parameter, out ZSqlParameter newParameter)
		{
			if (parameter.IsAutoGeneratedParameter && parametersByValue.TryGetValue(parameter, out newParameter))
			{
				return !StringComparer.OrdinalIgnoreCase.Equals(parameter.ParameterName, newParameter.ParameterName);
			}

			int collidingParameterCount = 0;
			var initialParameterName = parameter.ParameterName;
			while (parameters.TryGetValue(parameter.ParameterName, out var existingParameter) && !existingParameter.Equals(parameter))
			{
				collidingParameterCount++;
				parameter = parameter.ShallowClone(initialParameterName + collidingParameterCount);
			}

			newParameter = parameter;
			ReserveParameter(parameter);

			return collidingParameterCount > 0;
		}

		static string GetLiteralTextValueText(SchemaColumn column, object value)
		{
			return ZSqlParameter.New((NoResString)"@fakey", value, column).ParameterValueTextSql;
		}

		static string GetLiteralTextADOValueText(SchemaColumn column, object value)
		{
			string result = ZSqlParameter.New((NoResString)"@fakey", value, column).ParameterValueTextADO;
			if ((value is ZGuid || value is Guid) && !result.StartsWith("CONVERT("))
			{
				result = (NoResString)"CONVERT(" + result + (NoResString)", 'System.Guid')";
			}
			return result;
		}

		#endregion

		public int Length => stringBuilder.Length;

		public bool HasPendingPrefix => prefixes.Count > 0;

		public string GetSql() => stringBuilder.ToString();

		public ZSqlParameter[] GetParameters() => parameters.Values.ToArray();

		#region Object overrides

		public override string ToString() => GetSql();

		#endregion
	}
}
