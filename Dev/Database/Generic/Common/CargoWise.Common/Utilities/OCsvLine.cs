using System;
using System.Collections.Generic;
using System.Text;

namespace CargoWise.Common
{
	public class OCsvLine
	{
		public OCsvLine(string line)
		{
			Delimiter = ',';
			SplitCsvLineValues(line, out FieldValues, out IncludeQuotes);
		}

		public OCsvLine(string line, char delimiter)
		{
			if (delimiter == '"')
			{
				throw new ArgumentException("Delimiter cannot be a '\"' character");
			}
			this.Delimiter = delimiter;
			SplitCsvLineValues(line, out FieldValues, out IncludeQuotes);
		}

		public OCsvLine(string[] fieldValues)
		{
			Delimiter = ',';
			this.FieldValues = fieldValues;
			IncludeQuotes = new bool[fieldValues.Length];
			for (int i = 0; i < IncludeQuotes.Length; i++)
			{
				IncludeQuotes[i] = true;
			}
		}

		public OCsvLine(string[] fieldValues, bool[] includeQuotes)
		{
			Delimiter = ',';
			if (fieldValues.Length != includeQuotes.Length)
			{
				throw new ArgumentException("FieldValues.Length != IncludeQuotes.Length");
			}
			this.FieldValues = fieldValues;
			this.IncludeQuotes = includeQuotes;
		}

		public OCsvLine(string[] fieldValues, bool[] includeQuotes, char delimiter)
		{
			Delimiter = delimiter;
			if (fieldValues.Length != includeQuotes.Length)
			{
				throw new ArgumentException("FieldValues.Length != IncludeQuotes.Length");
			}
			this.FieldValues = fieldValues;
			this.IncludeQuotes = includeQuotes;
		}

		/// <summary>
		/// Get the values of each field.
		/// </summary>
		public readonly string[] FieldValues;

		/// <summary>
		/// Get whether or not to include quotes around each field value.
		/// </summary>
		public readonly bool[] IncludeQuotes;

		/// <summary>
		/// Auto-detect which delimiter seems to be used for the given comma, pipe or ~ delimited line.
		/// </summary>
		public static char AutoDetectDelimiter(string rawRow)
		{
			char result = ',';
			if (HasAtLeastNumberOfOccurrences(rawRow, '|', 4))
			{
				result = '|';
			}
			else if (HasAtLeastNumberOfOccurrences(rawRow, '~', 4))
			{
				result = '~';
			}
			return result;
		}

		static bool HasAtLeastNumberOfOccurrences(string value, char charToCount, int numberOfOccurences)
		{
			var counter = 0;

			for (var i = 0; i < value.Length; i++)
			{
				if (value[i] == charToCount)
				{
					counter++;

					if (counter >= numberOfOccurences)
					{
						return true;
					}
				}
			}

			return false;
		}

		public override string ToString()
		{
			StringBuilder result = new StringBuilder();
			for (int i = 0; i < FieldValues.Length; i++)
			{
				string fieldValue = FieldValues[i];
				if (result.Length != 0)
				{
					result.Append(Delimiter);
				}
				if (IncludeQuotes[i])
				{
					result.Append("\"" + fieldValue.Replace("\"", "\"\"") + "\"");
				}
				else
				{
					result.Append(fieldValue);
				}
			}
			return result.ToString();
		}

		public string ToStringWithNewLine()
		{
			return ToString() + '\n';
		}

		#region Implementation

		protected void SplitCsvLineValues(string line, out string[] values, out bool[] includeQuotes)
		{
			var resultValues = new List<string>();
			var resultIncludeQuotes = new List<bool>();
			if (line != null)
			{
				int index = 0;
				bool doneOne = false;
				while (index < line.Length)
				{
					if (doneOne)
					{
						while (index < line.Length && line[index] != Delimiter)
						{
							index++;
						}
						index++;
					}
					string nextValue = ParseQuotedString(line, index, out index, out var hadQuotes);
					resultValues.Add(nextValue);
					resultIncludeQuotes.Add(hadQuotes);

					doneOne = true;
				}
			}
			values = resultValues.ToArray();
			includeQuotes = resultIncludeQuotes.ToArray();
		}

		/// <summary>
		/// Parses the given string up to a closing quote, or the next comma.
		/// </summary>
		protected string ParseQuotedString(string line, int start, out int end, out bool hadQuotes)
		{
			string result = "";
			int index = start;
			hadQuotes = true;

			while (index < line.Length && char.IsWhiteSpace(line[index]))
			{
				index++;
			}
			if (index < line.Length)
			{
				if (line[index] == '\"')
				{
					int closingQuoteIndex = FindIndexOfNextQuoteNotIncludingDoubleQuotes(line, index + 1);
					if (closingQuoteIndex == -1)
					{
						// invalid file format
						line += "\"";
						closingQuoteIndex = line.Length - 1;
					}

					result = line.Substring(index + 1, closingQuoteIndex - index - 1);

					int nextDelimiter = line.IndexOf(Delimiter, closingQuoteIndex);
					index = nextDelimiter != -1 ? nextDelimiter : line.Length;
				}
				else
				{
					hadQuotes = false;
					index = start;
					int nextDelimiter = line.IndexOf(Delimiter, index);
					if (nextDelimiter == -1)
					{
						nextDelimiter = line.Length;
					}

					result = line.Substring(index, nextDelimiter - index);
					result = result.Trim(); // leading and trailing space-characters adjacent to comma field separators are ignored.
					index = nextDelimiter;
				}
			}

			if (hadQuotes)
			{
				result = result.Replace("\"\"", "\"");
			}

			end = index;
			return result;
		}

		protected int FindIndexOfNextQuoteNotIncludingDoubleQuotes(string line, int index)
		{
			int quoteIndex = index - 1;
			while (true)
			{
				quoteIndex = line.IndexOf('\"', quoteIndex + 1);
				if (quoteIndex == -1 ||
					quoteIndex == line.Length - 1 ||
					line[quoteIndex + 1] != '\"')
				{
					break;
				}
				quoteIndex++;
			}
			return quoteIndex;
		}

		protected char Delimiter;

		#endregion
	}
}
