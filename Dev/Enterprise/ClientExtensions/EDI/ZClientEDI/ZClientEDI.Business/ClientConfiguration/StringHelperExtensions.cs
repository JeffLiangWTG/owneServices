using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Client.EDI
{
	public static class ZStringHelperExtensions
	{
		public static string CapitaliseFirstLettersOfWords(this ZString value)
		{
			StringBuilder result = new StringBuilder();

			var words = value.Split(' ');
			for (int i = 0; i < words.Length; i++)
			{
				if (i > 0)
				{
					result.Append(" ");
				}
				var word = words[i];
				if (word.Length > 0)
				{
					result.Append(char.ToUpper(word[0], CultureInfo.InvariantCulture));
					ZString stringSegment = word.Substring(1);
					result.Append(stringSegment == stringSegment.ToUpper() ? stringSegment.ToLower() : stringSegment);
				}
			}

			var combinedWords = result.ToString();
			var secondSetOfWords = combinedWords.Split('\'');
			result = new StringBuilder(combinedWords.Length);
			for (int i = 0; i < secondSetOfWords.Length; i++)
			{
				if (i > 0)
				{
					result.Append("'");
				}
				var word = secondSetOfWords[i];
				if (word.Length > 0)
				{
					result.Append(char.ToUpper(word[0], CultureInfo.InvariantCulture));
					result.Append(word.Substring(1));
				}
			}

			return result.ToString();
		}

		public static string CleanUpTextForHTMLWithoutTagEscaping(this ZString text)
		{
			string tempText = NormaliseWhitespaceCharactersForHtml(text);

			var sb = new StringBuilder();
			int lastIndex = 0;
			foreach (Match match in Regex.Matches(tempText, @"<[^<>]+?>|&nbsp;"))
			{
				sb.Append(WebUtility.HtmlEncode(tempText.Substring(lastIndex, match.Index - lastIndex)));
				sb.Append(match.Value);
				lastIndex = match.Index + match.Length;
			}
			sb.Append(WebUtility.HtmlEncode(tempText.Substring(lastIndex, tempText.Length - lastIndex)));

			return sb.ToString();
		}

		public static string CleanUpTextForHTML(this ZString text)
		{
			string result = text.NormaliseNewLine();
			result = WebUtility.HtmlEncode(result);
			result = result.Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;");
			result = result.Replace("\r\n", "<br />");
			return result;
		}

		public static string NormaliseWhitespaceCharactersForHtml(this ZString text)
		{
			string result = text.NormaliseNewLine();
			result = result.Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;");
			result = result.Replace("\r\n", "<br />");
			return result;
		}

		public static string NormaliseNewLine(this ZString originalString)
		{
			return Regex.Replace(originalString, @"\r\n|\n|\r", "\r\n");
		}

		public static string SplitCamelCase(this ZString originalString)
		{
			var result = Regex.Replace(Regex.Replace(originalString, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"), @"(\p{Ll})(\P{Ll})", "$1 $2");
			result = Regex.Replace(result, @"[ ]{2,}", " ");	//Remove extra space
			return result;
		}
	}
}

