using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CargoWiseOne.ResourceStrings;

namespace CargoWise.Common
{
	public static class StringExtensions
	{
		/// <summary>
		/// Reports the zero-based indexes of all occurrences of the specified Unicode character in this string.
		/// </summary>
		/// <param name="str">The string to search for a character.</param>
		/// <param name="value">The Unicode character to seek.</param>
		/// <returns>All indexes where the character was found or empty sequence.</returns>
		public static IEnumerable<int> AllIndexesOf(this string str, char value)
		{
			var index = str.IndexOf(value, 0);
			while (index != -1)
			{
				yield return index;
				index = str.IndexOf(value, index + 1);
			}
		}

		/// <summary>
		/// Reports the zero-based indexes of all occurrences of the specified string in this string.
		/// </summary>
		/// <param name="str">The string to search for a substring.</param>
		/// <param name="value">The substring to seek.</param>
		/// <returns>All indexes where the substring was found or empty sequence.</returns>
		public static IEnumerable<int> AllIndexesOf(this string str, string value)
		{
			var index = str.IndexOf(value, 0);
			while (index != -1)
			{
				yield return index;
				index = str.IndexOf(value, index + 1);
			}
		}

		/// <summary>
		/// Splits a string. Yield return avoids the out of memory exceptions that may occur when processing large files with String.Split()
		/// </summary>
		/// <param name="input">The string to split.</param>
		/// <returns>An enumerable of strings split by line breaks.</returns>
		public static IEnumerable<string> SplitByLine(this string input)
		{
			using (var stringReader = new StringReader(input))
			{
				string line;
				while ((line = stringReader.ReadLine()) != null)
				{
					yield return line;
				}
			}
		}

		public static string QuoteName(this string str, char quoteCharacter = '[')
		{
			if (str != null)
			{
				switch (quoteCharacter)
				{
					case '[':
					case ']':
						return "[" + str.Replace("]", "]]") + "]";
					case '\'':
						return "'" + str.Replace("'", "''") + "'";
					case '(':
					case ')':
						return "(" + str + ")";
					case '{':
					case '}':
						return "{" + str + "}";
					default:
						return quoteCharacter + str + quoteCharacter;
				}
			}

			return str;
		}

		public static string QuoteEscapedName(this string str, char quoteCharacter = '[')
		{
			if (str != null)
			{
				switch (quoteCharacter)
				{
					case '[':
					case ']':
						return str.Replace("]", "]]");
					case '\'':
						return str.Replace("'", "''");
					default:
						return str;
				}
			}

			return str;
		}

		public static string ReFormatForRightToLeftLanguages(this string str)
		{
			if (!string.IsNullOrEmpty(str) && Res.IsRightToLeft(Res.CurrentLanguage))
			{
				var leftToRightMark = (char)0x200E;
				var result = string.Empty;

				var wasRightToLeft = false;
				foreach (var @char in str)
				{
					var isRightToLeft = IsRightToLeft.IsMatch(@char.ToString());
					if (!isRightToLeft && wasRightToLeft)
					{
						result += leftToRightMark;
					}
					result += @char;
					wasRightToLeft = isRightToLeft;
				}

				return result;
			}

			return str;
		}

		static readonly Regex IsRightToLeft = new Regex(@"\p{IsArabic}|\p{IsHebrew}|\p{IsSyriac}|\p{IsThaana}");
	}
}
