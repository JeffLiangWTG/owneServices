using System;
using System.Text.RegularExpressions;
using CargoWise.Common;

namespace CargoWise.Data
{
	/// <summary>
	/// Used to remove big binary/text literals from command texts in order to make them more readable
	/// in, for instance, ErrorReports.
	/// </summary>
	public class CommandTextTrimmer
	{
		public CommandTextTrimmer(int maxCommandTextLength)
		{
			if (maxCommandTextLength <= 0)
			{
				throw new ArgumentException("Invalid argument.", nameof(maxCommandTextLength)); // Hard codded exception message
			}

			this.maxCommandTextLength = maxCommandTextLength;
		}

		public CommandTextTrimmer()
		{
			maxCommandTextLength = 10000;
		}

		internal readonly int maxCommandTextLength;

		public string GetTruncatedTextWithoutBlobs(string commandText)
		{
			Argument.NotNull(commandText, nameof(commandText));

			// truncate before removing blobs otherwise memory usage is huge on long CommandText
			return GetTextWithoutBlobs(GetTruncatedText(commandText));
		}

		public string GetTruncatedText(string commandText)
		{
			Argument.NotNull(commandText, nameof(commandText));

			string result = commandText;
			if (result.Length > maxCommandTextLength)
			{
				result = result.Substring(0, maxCommandTextLength);
			}

			return result;
		}

		public string GetTextWithoutBlobs(string commandText)
		{
			Argument.NotNull(commandText, nameof(commandText));

			return Regex.Replace(commandText, "0x[0-9a-fA-F]+", new MatchEvaluator(LengthOfBlob));
		}

		public string GetTextWithoutMultiLineComments(string commandText)
		{
			Argument.NotNull(commandText, nameof(commandText));

			return Regex.Replace(commandText, @"/\*[\w\W]*?(?=\*/)\*/", "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
		}

		public string EnsureCommandThatCausedExceptionIsNotTruncated(Exception e, string commandText)
		{
			if (commandText.Length > maxCommandTextLength)
			{
				var exceptionIndex = IndexOfCommandThatCausedException(e, commandText);
				if (exceptionIndex > 0)
				{
					commandText = commandText.Substring(exceptionIndex);
				}
			}

			return commandText;
		}

		protected string LengthOfBlob(Match blobMatch)
		{
			Argument.NotNull(blobMatch, nameof(blobMatch)); // Suggested By ReviewBot 

			int numOfBytes = (blobMatch.Value.Length - 2) / 2;
			return string.Format("<System.Byte[{0}]>", numOfBytes); // not seen by users
		}

		int IndexOfCommandThatCausedException(Exception e, string commandText)
		{
			string startOfException = GetPkFromSqlException(e);
			if (startOfException != null)
			{
				return commandText.IndexOf("SET @PK = '" + startOfException, StringComparison.Ordinal);
			}
			return 0;
		}

		string GetPkFromSqlException(Exception e)
		{
			var message = e.Message;
			try
			{
				if (message.Length > 36 && message.StartsWith("{", StringComparison.Ordinal) && message.Substring(1, 37).EndsWith(",", StringComparison.Ordinal))
				{
					var updatePk = message.Substring(1, 36);

					if (Guid.TryParse(updatePk, out Guid result) && result != Guid.Empty)
					{
						return updatePk;
					}
				}
			}
			catch (ArgumentNullException)
			{
			}
			catch (FormatException)
			{
			}
			return null;
		}
	}
}
