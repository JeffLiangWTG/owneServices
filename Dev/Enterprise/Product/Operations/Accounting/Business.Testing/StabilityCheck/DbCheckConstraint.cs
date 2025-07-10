using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Enterprise.Accounting.Business.Testing.StabilityCheck
{
	/// <summary>
	/// This class provides basic parsing capabilities for expressions in CHECK constraints.
	/// It does not support complex expressions, including: functions, multi-column constraints, constants other than strings.
	/// </summary>
	internal class DbCheckConstraint
	{
		const string ColumnNameRegex = @"\[[a-zA-Z_][a-zA-Z0-9_]*\]";
		const string ValueRegex = @"'[^']*'";
		static readonly string ColumnEqualRegex = $@"({ColumnNameRegex})=({ValueRegex})";
		static readonly Regex InSetRegex = new Regex($@"^\({ColumnEqualRegex}(?: OR {ColumnEqualRegex})*\)$");

		public string SchemaName { get; }
		public string TableName { get; }
		public string ConstraintName { get; }
		public string Expression { get; }

		public bool IsInSetConstraint { get; private set; }
		public string ColumnName { get; private set; }
		public List<string> ColumnValues { get; private set; }

		public DbCheckConstraint(string schemaName, string tableName, string constraintName, string expression)
		{
			SchemaName = schemaName;
			TableName = tableName;
			ConstraintName = constraintName;
			Expression = expression;

			ParseExpression();
		}

		void ParseExpression()
		{
			Match match = InSetRegex.Match(Expression);
			if (!match.Success)
			{
				return;
			}
			string columnName = match.Groups[1].Captures[0].Value;
			foreach (Capture capture in match.Groups[3].Captures)
			{
				if (capture.Value != columnName)
				{
					return;
				}
			}
			List<string> values = new List<string>();
			foreach (Capture capture in match.Groups[2].Captures)
			{
				values.Add(UnquoteValue(capture.Value));
			}
			foreach (Capture capture in match.Groups[4].Captures)
			{
				values.Add(UnquoteValue(capture.Value));
			}

			IsInSetConstraint = true;
			ColumnName = UnquoteName(columnName);
			ColumnValues = values;
		}

		string UnquoteName(string name)
		{
			if (name.Length < 2 || !name.StartsWith("[") || !name.EndsWith("]"))
			{
				return name;
			}
			return name.Substring(1, name.Length - 2);
		}

		static string UnquoteValue(string value)
		{
			if (value.Length < 2 || !value.StartsWith("'") || !value.EndsWith("'"))
			{
				return value;
			}
			return value.Substring(1, value.Length - 2);
		}
	}
}