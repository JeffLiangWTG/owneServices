using System;
using System.Text;
using System.Text.RegularExpressions;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Formats the LiteralTextADO of a query.
	///		1. Removes WITH (NOLOCK) from the query
	///		2. Places a line break before and after any AND or OR operator not found in Query Value WHERE Clause
	///		3. Places a line break before and after any open or close bracket
	///		4. Indents all query information within a set of brackets
	/// </summary>
	public static class ZQueryFormatter
	{
		public static string GetFormattedText(string unformattedSql)
		{
			StringBuilder builder = new StringBuilder();

			string sql = ParseSqlQuery(unformattedSql);

			int numTabs = 0;
			foreach (string line in sql.Split('\n'))
			{
				string newLine = line;
				for (int i = 0; i < numTabs; i++)
				{
					if (i == numTabs - 1 && line.Contains(")"))
					{
						break;
					}

					newLine = "\t" + newLine;
				}

				if (line.Contains("("))
				{
					numTabs++;
				}

				if (line.Contains(")"))
				{
					numTabs--;
				}

				if (!string.IsNullOrEmpty(newLine.Trim()))
				{
					builder.AppendLine(newLine);
				}
			}

			return builder.ToString();
		}

		static string FormatSegment(Match match)
		{
			return "\n" + match.ToString().TrimEnd().ToUpper() + "\n";
		}

		static string FormatSQL(string unformattedSql)
		{
			string sql = unformattedSql.Replace("WITH (NOLOCK) ", "");

			Regex rex = new Regex(@"(\band |\bor |\(|\))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			sql = rex.Replace(sql, FormatSegment);
			return sql;
		}

		static string ParseSqlQuery(string unformattedSql)
		{
			StringBuilder builder = new StringBuilder();

			string pattern = "(\'.*?\')";
			string[] tokens = Regex.Split(unformattedSql, pattern, RegexOptions.IgnoreCase);
			foreach (string match in tokens)
			{
				builder.Append(!match.StartsWith("'", StringComparison.OrdinalIgnoreCase) ? FormatSQL(match) : match);
			}
			return builder.ToString();
		}
	}
}
