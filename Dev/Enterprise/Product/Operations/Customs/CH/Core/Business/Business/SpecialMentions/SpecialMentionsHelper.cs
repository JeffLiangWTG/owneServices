using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public static class SpecialMentionsHelper
{
	const char CR = '\r';
	const char LF = '\n';
	static char[] CRLF => new[] { CR, LF };
	const int CRLF_Length = 2;
	const string NewLine = "\r\n";

	const int MaxLineLength = 70;
	public const int MaxLines = 99;
	internal const int MaxLength = (MaxLineLength + CRLF_Length) * MaxLines;

	public static PredefinedNoteType NoteType => PredefinedNoteTypes.Instance.CustomsSpecialMentions;

	internal static MultilingualString ExceedMaximumLineNumberMessage => ResString.GetMultilingualString("CCA81561-AA8F-4011-8A84-C7ABE9AF6EB2", "Special mentions may have a maximum of 99 lines.");

	internal static MultilingualString TotalLimitMessage => ResString.GetMultilingualString("6dd6e4e5-c570-488b-b112-2fe68389cf30", "Special Mentions overall may have a maximum of 99 lines.");

	public static ZString FormatText(ZString text)
	{
		if (!HasTooLongLines(text))
		{
			return text;
		}

		return WrapTooLongLines(text);
	}

	static ZString WrapTooLongLines(ZString text)
	{
		var result = new StringBuilder();

		int lineStartPos = 0;
		bool noNewLineAtEnd = false;
		while (lineStartPos < text.Length)
		{
			int lineEndPos = text.IndexOfAny(CRLF, lineStartPos);
			if (lineEndPos == -1)
			{
				lineEndPos = text.Length;
				noNewLineAtEnd = true;
			}

			int partStartPos = lineStartPos;
			while (lineEndPos > partStartPos + MaxLineLength)
			{
				int partEndPos = partStartPos + MaxLineLength;

				int spacePos = -1;
				for (int p = partEndPos - 1; p >= partStartPos; p--)
				{
					if (text[p] == ' ')
					{
						spacePos = p;
						break;
					}
				}
				if (spacePos == -1)
				{
					result.Append(text, partStartPos, partEndPos - partStartPos);
					partStartPos = partEndPos;
				}
				else
				{
					partEndPos = spacePos;
					result.Append(text, partStartPos, partEndPos - partStartPos);
					partStartPos = partEndPos + 1;
				}
				result.Append(CRLF);
			}
			result.Append(text, partStartPos, lineEndPos - partStartPos).Append(CRLF);

			lineStartPos = lineEndPos + (lineEndPos < text.Length && IsCRLF(text, lineEndPos) ? 2 : 1);
		}

		if (noNewLineAtEnd)
		{
			RemoveNewLineAtEnd(result);
		}

		return result.ToString();
	}

	static void RemoveNewLineAtEnd(StringBuilder result)
	{
		if (result.Length > 1 && result[result.Length - 1] == LF)
		{
			result.Length--;
			if (result.Length > 1 && result[result.Length - 1] == CR)
			{
				result.Length--;
			}
		}
	}

	static bool HasTooLongLines(ZString text)
	{
		for (int lineStartPos = 0; lineStartPos < text.Length;)
		{
			int lineEndPos = text.IndexOfAny(CRLF, lineStartPos);
			if (lineEndPos == -1)
			{
				lineEndPos = text.Length;
			}
			if (lineEndPos - lineStartPos > MaxLineLength)
			{
				return true;
			}
			lineStartPos = lineEndPos + (lineEndPos < text.Length && IsCRLF(text, lineEndPos) ? 2 : 1);
		}
		return false;
	}

	static bool IsCRLF(ZString text, int pos)
	{
		return pos + 1 < text.Length && text[pos] == CR && text[pos + 1] == LF;
	}

	public static int CountLines(ZString text)
	{
		int count = 0;
		if (text.Length > 0)
		{
			for (int pos = 0; pos < text.Length; pos++)
			{
				if (text[pos] == CR || text[pos] == LF)
				{
					count++;
					if (pos + 1 < text.Length && text[pos] == CR && text[pos + 1] == LF)
					{
						pos++;
					}
				}
			}

			var lastPos = text.Length - 1;
			if (text[lastPos] != CR && text[lastPos] != LF)
			{
				count++;
			}
		}
		return count;
	}

	internal static void Validate(ZPropertyInfo specialMentionsInfo, int countOfLines, Func<IEnumerable<int[]>> overallCountOfLinesGetter = null)
	{
		if (countOfLines > MaxLines)
		{
			specialMentionsInfo.AddMessageError(ExceedMaximumLineNumberMessage);
		}
		else if (overallCountOfLinesGetter != null && overallCountOfLinesGetter().Where(x => x.Max() <= MaxLines).Any(x => x.Sum() > MaxLines))
		{
			specialMentionsInfo.AddMessageError(TotalLimitMessage);
		}
	}

	public static ZString[] SplitIntoLines(ZString text)
	{
		return text.IsEmpty ? Array.Empty<ZString>() : text.Trim().Split(NewLine);
	}
}
