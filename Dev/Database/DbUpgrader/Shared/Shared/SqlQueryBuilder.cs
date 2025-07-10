using System.Collections.Generic;
using System.Globalization;

namespace Enterprise.DbUpgrader.Shared
{
	/// <summary>
	/// Helper class to compose large queries in such a way that the final query can be easily inspected in the debugger.
	/// Previously a lot of the DbUpgrader code used ZStringBuilder for this and leaked it everywhere.
	/// This implementation starts with a matching API surface, and will eventually morph into a better, single-purposes class.
	/// </summary>
	public sealed class SqlQueryBuilder
	{
		public SqlQueryBuilder()
		{
			statements = new List<string>();
		}
		public SqlQueryBuilder(string initialStatement)
		{
			statements = new List<string>();
			statements.Add(initialStatement);
		}

		readonly List<string> statements;

		public SqlQueryBuilder Append(string statement)
		{
			statements.Add(statement);
			return this;
		}

		public SqlQueryBuilder AppendLine()
		{
			statements.Add(System.Environment.NewLine);
			return this;
		}

		public SqlQueryBuilder AppendLine(string statement)
		{
			statements.Add(statement + System.Environment.NewLine);
			return this;
		}

		public SqlQueryBuilder AppendFormat(string format, params object[] args)
		{
			statements.Add(string.Format(CultureInfo.InvariantCulture, format, args));
			return this;
		}

		public void Clear() => statements.Clear();

		public void Prepend(string statement) => statements.Insert(0, statement);

		public bool IsEmpty => statements.Count == 0;

		public override string ToString() => ToString(string.Empty);

		public string ToString(string delimiter) => string.Join(delimiter, statements);
	}

	public static class SqlQueryBuilderPolyfillExtensions
	{
		public static string ToStringWithNewLineBetweenAppends(this SqlQueryBuilder builder) => builder.ToString(System.Environment.NewLine);
	}
}
