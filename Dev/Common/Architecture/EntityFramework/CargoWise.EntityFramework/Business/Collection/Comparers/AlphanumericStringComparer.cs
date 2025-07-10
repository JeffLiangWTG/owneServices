using System;
using System.Collections.Generic;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class AlphanumericStringComparer : IComparer<ZString>
	{
		public virtual int Compare(ZString x, ZString y)
		{
			var xCursor = 0;
			var yCursor = 0;

			while (xCursor < x.Length && yCursor < y.Length)
			{
				var xSubstring = GetNextSubstring(x, ref xCursor);
				var ySubstring = GetNextSubstring(y, ref yCursor);

				var result = CompareSubstrings(xSubstring, ySubstring);

				if (result != 0)
				{
					return result;
				}
			}

			return x.Length - y.Length;
		}

		string GetNextSubstring(string str, ref int cursor)
		{
			var substring = "";
			var substringIsNumeric = char.IsDigit(str[cursor]);

			while (cursor < str.Length)
			{
				var nextXChar = str[cursor];

				if (char.IsDigit(nextXChar) != substringIsNumeric)
				{
					break;
				}

				substring += nextXChar;
				cursor++;
			}

			return substring;
		}

		int CompareSubstrings(string xSubstring, string ySubstring)
		{
			if (char.IsDigit(xSubstring[0]) && char.IsDigit(ySubstring[0]))
			{
				return long.TryParse(xSubstring, out long xNumericChunk) && long.TryParse(ySubstring, out long yNumericChunk) ? xNumericChunk.CompareTo(yNumericChunk) : 0;
			}
			else
			{
				return string.Compare(xSubstring, ySubstring, StringComparison.Ordinal);
			}
		}
	}
}
