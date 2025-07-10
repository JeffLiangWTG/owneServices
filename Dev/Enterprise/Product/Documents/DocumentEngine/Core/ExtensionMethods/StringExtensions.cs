using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.Renderer;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DocumentEngine
{
	static class StringExtensions
	{
		public static bool ContainsNumber(this string source)
		{
			return source != null && source.IndexOfAny(new char[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' }) != -1;
		}

		public static bool ContainsLetters(this string source)
		{
			if (source != null)
			{
				for (int i = 0; i < source.Length; i++)
				{
					if (char.IsLetter(source, i))
					{
						return true;
					}
				}
			}
			return false;
		}

		public static bool ContainsInnerWhitespace(this string source)
		{
			if (source != null)
			{
				source = source.Trim();
				for (int i = 0; i < source.Length; i++)
				{
					if (char.IsWhiteSpace(source, i))
					{
						return true;
					}
				}
			}
			return false;
		}

		public static string HandleBrokenFontsSymbols(this string source, Report report)
		{
			if (!string.IsNullOrEmpty(source) && report != null)
			{
				//Handle non-breaking spaces:
				//	Font files have many encodings inside, and in order to create smaller files, Excel uses Win1232 encoding if available, and uses Unicode otherwise.
				//	Even if a non-breakinge space is a valid ANSI character, it is missing in the ANSI table inside some TTF fonts, e.g Lucida Console
				//	This causes that character to be exported as a disgracious square.
				//	=> This code below will replace it with a space if required.
				if (source.IndexOf(nonBreakingSpace) != -1)
				{
					var workSheet = report.WorkSheetCurrentlyBeingProcessed;
					if (workSheet != null && report.Renderer != null)
					{
						var row = report.Renderer.CurrentRow;
						var column = report.Renderer.CurrentColumn;
						var format = workSheet.GetCellFormat(row, column);
						if (fontWithBrokenNonBreakingSpace.Contains(format.FontName))
						{
							return source.Replace(nonBreakingSpace, space);
						}
					}
				}
			}

			return source;
		}

		#region SuppressResourceStringsCheckRegion

		const char nonBreakingSpace = (char)160;
		const char space = (char)32;
		static readonly List<string> fontWithBrokenNonBreakingSpace = new List<string>()
		{
			"Lucida Console",
		};

		#endregion

		public static string ShrinkToMaxLength(this string source, int maxLength)
		{
			if (maxLength < 7)
			{
				throw new ArgumentException("You cannot have a maxLength less than 7 as the padding used by this function is 5 characters long. Using this function to restrict to lengths less than 25 characters is generally not advised.");
			}

			if (source != null && source.Length > maxLength)
			{
				var prefixLength = (maxLength * 4 / 5) - 5;
				int suffixLength = maxLength - prefixLength - 5;
				return source.Substring(0, prefixLength) + " ... " + source.Substring(source.Length - suffixLength);
			}
			return source;
		}

		public static string EscapeQuotes(this string value)
		{
			return string.IsNullOrWhiteSpace(value) ? value : value.EscapeCharacter('"');
		}

		public static string UnEscapeQuotes(this string value)
		{
			return string.IsNullOrWhiteSpace(value) ? value : value.Replace("\\\"", "\"");
		}

		public static string EscapeAngleBrackets(this string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return value;
			}

			return value.Replace("<", "\\<").Replace(">", "\\>");
		}

		public static string UnEscapeAngleBrackets(this string value)
		{
			return string.IsNullOrWhiteSpace(value) ? value : value.Replace("\\<", "<").Replace("\\>", ">");
		}

		public static string EscapeBackslashes(this string value)
		{
			return string.IsNullOrWhiteSpace(value) ? value : value.EscapeCharacter('\\');
		}

		public static string UnEscapeBackslashes(this string value)
		{
			return string.IsNullOrWhiteSpace(value) ? value : value.Replace("\\\\", "\\");
		}

		public static string EscapeDollarSignForRegexReplace(this string value)
		{
			return string.IsNullOrWhiteSpace(value) ? value : value.Replace(@"$", @"$$");
		}

		public static string UnEscapeHtmlBreakLineAngleBrackets(this string value)
		{
			return string.IsNullOrWhiteSpace(value) ? value : value.Replace("\\<br /\\>", "<br />");
		}

		public static string EscapeForJScript(this string value)
		{
			var escapedValue = new StringBuilder(value.Length);
			foreach (var c in value)
			{
				escapedValue.Append(GetEscapedValue(c));
			}

			return escapedValue.ToString();
		}

		public static string UnEscapeForJScript(this string value)
		{
			var result = value;
			ListToBeEscapedForJScript.ForEach(s => result = result.Replace(s.s, s.c.ToString()));
			return result;
		}

		public static string StripOutMostAngleBrackets(this string value)
		{
			var valueTrimed = value.Trim();
			var first = valueTrimed.First();
			var last = valueTrimed.Last();
			if (valueTrimed.Length > 2 && first == '<' && last == '>')
			{
				return valueTrimed.Substring(1, value.Length - 2);
			}
			throw new ArgumentException(FormattableString.Invariant($"{value} is an invalid macro."));
		}

		// Got the list of what to replace from https://msdn.microsoft.com/en-us/library/2yfce773(v=vs.94).aspx
		[ThreadSafe]
		static readonly Lazy<List<(char c, string s)>> listToBeEscapedForJScript = new Lazy<List<(char c, string s)>>(() =>
			new List<(char c, string s)>
			{
				('\b', @"\b"),
				('\t', @"\t"),
				('\n', @"\n"),
				('\v', @"\v"),
				('\r', @"\r"),
				('\"', @"\"""),
				('\'', @"'"),
				('\\', @"\\"),
				('<', @"\<"),
				('>', @"\>")
			});
		internal static List<(char c, string s)> ListToBeEscapedForJScript => listToBeEscapedForJScript.Value;

		static string GetEscapedValue(char c)
		{
			var result = ListToBeEscapedForJScript.FirstOrDefault(i => i.c == c);
			return result.Equals(default(ValueTuple<char, string>)) ? c.ToString() : result.s;
		}

		static string EscapeCharacter(this string value, char toEscape)
		{
			switch (toEscape)
			{
				case '"':
					return (quoteEscapeRegex ?? (quoteEscapeRegex = new Regex(@"(?<!\\)""", RegexOptions.Compiled))).Replace(value, "\\\"");
				case '<':
					return (lessEscapeRegex ?? (lessEscapeRegex = new Regex(@"(?<!\\)<", RegexOptions.Compiled))).Replace(value, "\\<");
				case '>':
					return (greaterEscapeRegex ?? (greaterEscapeRegex = new Regex(@"(?<!\\)>", RegexOptions.Compiled))).Replace(value, "\\>");
				case '\\':
					// Matches any backslash not already used to escape another character
					return (backslashEscapeRegex ?? (backslashEscapeRegex = new Regex(@"\\(?![<>=""])", RegexOptions.Compiled))).Replace(value, "\\\\");
				default:
					return new Regex(@"(?<!\\)" + toEscape).Replace(value, "\\" + toEscape);
			}
		}

		static Regex quoteEscapeRegex;
		static Regex lessEscapeRegex;
		static Regex greaterEscapeRegex;
		static Regex backslashEscapeRegex;

		public static bool TryGetEnum<T>(this string valueName, out T enumValue)
		{
			bool result = !string.IsNullOrEmpty(valueName) && Enum.IsDefined(typeof(T), valueName);
			enumValue = (T)(result ? Enum.Parse(typeof(T), valueName, true) : Enum.GetValues(typeof(T)).GetValue(0));
			return result;
		}

		#region Remove invalid characters

		public static string RemoveInvalidCharacters(this string source)
		{
			return Regex.Replace(source, InvalidHexPattern.Value, string.Empty);
		}

		public static string RemoveInvalidCharactersAndCarriageReturn(this string source)
		{
			return Regex.Replace(source, InvalidHexPatternWithCarriageReturn.Value, string.Empty);
		}

		// Hex string is more clear than regular string since some characters are invisible.
		static readonly ImmutableArray<string> InvalidHexStrings = ImmutableArray.Create(
			"4F-03-0C-20-4F-03",
			"A0-00-4F-03",
			"A0-00-02-03",
			"A0-00-05-03"
		);

		static readonly Lazy<string> InvalidHexPattern = new Lazy<string>(() =>
		{
			//  remove invalid characters
			return string.Join("|", Array.ConvertAll(InvalidHexStrings.ToArray(), ConvertHexStringToString));
		});

		static readonly Lazy<string> InvalidHexPatternWithCarriageReturn = new Lazy<string>(() =>
		{
			//  remove "\r" and invalid characters
			return "\\r|" + InvalidHexPattern.Value;
		});

		static string ConvertHexStringToString(string hexString)
		{
			hexString = hexString.Replace("-", "");

			var bytes = new byte[hexString.Length / 2];
			for (var i = 0; i < bytes.Length; i++)
			{
				bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
			}

			return Regex.Escape(Encoding.Unicode.GetString(bytes));
		}

		static string ConvertStringToHexString(string str)
		{
			return BitConverter.ToString(Encoding.Unicode.GetBytes(str));
		}

		public static string GetShortestInvalidCharacters(string text)
		{
			var lengthChecker = new LengthChecker(new GraphicsManager().Graphics, new Font(new FontFamily("Arial"), 10), 0);

			// Since the length of ShortestInvalidCharacters is usually within 3, this linear search's time complexity is superior to that of binary search.
			for (var len = 1; len <= text.Length; len++)
			{
				for (var i = 0; i <= text.Length - len; i++)
				{
					var partText = text.Substring(i, len);

					try
					{
						lengthChecker.GetWidthOfCharacterSequence(partText);
					}
					catch
					{
						return ConvertStringToHexString(partText);
					}
				}
			}
			return string.Empty;
		}

		#endregion
	}

	public static class PublicStringExtensions
	{
		public static ReportSQLSource ParseDataSource(this string cellValue)
		{
			ReportSQLSource result = null;
			if (!string.IsNullOrEmpty(cellValue))
			{
				dataSection = dataSection ?? (dataSection = new Regex("^Data:([^=]+)=(.+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline | RegexOptions.Compiled));
				if (dataSection.IsMatch(cellValue))
				{
					var match = dataSection.Match(cellValue);
					result = new ReportSQLSource(match.Groups[1].Value, match.Groups[2].Value);
				}
			}
			return result;
		}

		public static string RemoveBetween(this string source, string start, string end)
		{
			string result = source;
			int startIndex = 0;
			int endIndex = 0;

			while (startIndex != -1 && endIndex != -1)
			{
				startIndex = result.IndexOf(start, startIndex, StringComparison.CurrentCulture);

				if (startIndex != -1)
				{
					endIndex = result.IndexOf(end, startIndex + 1, StringComparison.CurrentCulture);

					if (endIndex != -1)
					{
						result = result.Remove(startIndex, endIndex - startIndex + 1);
					}
				}
			}

			return result;
		}

		[ThreadStatic]
		static Regex dataSection;
	}
}
